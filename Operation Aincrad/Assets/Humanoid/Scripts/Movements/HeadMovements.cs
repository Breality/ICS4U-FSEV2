using System.Collections;
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class HeadMovements : Movements {
        protected HeadTarget headTarget;

        #region Start
        public override void Start(HumanoidControl _humanoid, HumanoidTarget _target) {
            base.Start(_humanoid, _target);
            headTarget = (HeadTarget)_target;

            if (headTarget.neck.bone.transform != null)
                headTarget.neck.bone.transform.rotation = headTarget.neck.target.transform.rotation * headTarget.neck.target.toBoneRotation;
            if (headTarget.head.bone.transform != null)
                headTarget.head.bone.transform.rotation = headTarget.head.target.transform.rotation * headTarget.head.target.toBoneRotation;
        }
        #endregion

        #region Update
        public static void Update(HeadTarget headTarget) {
            if (headTarget.head.bone.transform == null || !headTarget.humanoid.calculateBodyPose)
                return;

#if hFACE
            if (headTarget.neck.target.confidence.rotation < 0.2F && headTarget.head.target.confidence.rotation < 0.2F &&
                headTarget.face.leftEye.target.confidence.rotation > 0.2F) {

                UpdateHeadBonesFromGazeDirection(headTarget);
            }
            else {
#endif
                UpdateHead(headTarget);
                UpdateNeck(headTarget);
#if hFACE
            }
            headTarget.face.UpdateMovements();
#endif
        }

        private static void UpdateNeck(HeadTarget headTarget) {
            Vector3 headPosition = headTarget.head.bone.transform.position;
            Quaternion headRotation = headTarget.head.bone.transform.rotation;

            if (headTarget.neck.target.confidence.rotation > headTarget.head.target.confidence.rotation) {
                headTarget.neck.SetBoneRotation(headTarget.neck.target.transform.rotation);
            }
            else {
                Quaternion neckRotation = Quaternion.Slerp(headTarget.head.bone.targetRotation, headTarget.humanoid.hipsTarget.hips.bone.targetRotation, 0.3F);
                headTarget.neck.SetBoneRotation(neckRotation);

                Vector3 neckPosition = headPosition - neckRotation * Vector3.up * headTarget.neck.bone.length;
                headTarget.neck.SetBonePosition(neckPosition);

            }

            headTarget.head.bone.transform.position = headPosition;
            headTarget.head.bone.transform.rotation = headRotation;
        }

        private static Quaternion oldNeckSpeed;
        private static void UpdateHead(HeadTarget headTarget) {
#if hFACE
            if (headTarget.neck.target.confidence.rotation < 0.2F && headTarget.head.target.confidence.rotation < 0.2F &&
                headTarget.face.leftEye.target.confidence.rotation > 0.2F
                ) {
                //UpdateHeadBonesFromGazeDirection();
                //return;
            }
#endif

            headTarget.head.SetBoneRotation(headTarget.head.target.transform.rotation);
            headTarget.head.SetBonePosition(headTarget.head.target.transform.position);

            //Quaternion oldNeckRotation = headTarget.neck.bone.transform.rotation * headTarget.neck.bone.toTargetRotation;
            //Quaternion oldHeadRotation = headTarget.head.bone.transform.rotation * headTarget.head.bone.toTargetRotation;
            ////Debug.Log(oldHeadRotation.eulerAngles);

            //Quaternion neckRotation;
            //Quaternion headRotation;

            //neckRotation = headTarget.neck.target.transform.rotation;
            //headRotation = headTarget.head.target.transform.rotation;

            //if (Application.isPlaying) {
            //    Quaternion neckSpeed = Quaternion.Inverse(oldNeckRotation) * neckRotation;
            //    float neckSpeedAngle = Quaternion.Angle(neckSpeed, oldNeckSpeed) / Time.deltaTime;
            //    //if (neckSpeedAngle > 0.01F)
            //    //    Debug.Log(neckSpeedAngle);
            //    float maxSpeed = Mathf.Min(360, neckSpeedAngle);
            //    oldNeckSpeed = neckSpeed;

            //    neckRotation = LimitRotationSpeed(oldNeckRotation, neckRotation, maxSpeed);
            //    headRotation = LimitRotationSpeed(oldHeadRotation, headRotation, maxSpeed);
            //}

            //headTarget.neck.bone.transform.rotation = neckRotation * headTarget.neck.target.toBoneRotation;
            //headTarget.head.bone.transform.rotation = headRotation * headTarget.head.target.toBoneRotation;

            ////if (!Application.isPlaying) {
            ////    Vector3 head2neck = headTarget.GetHeadNeckDelta();
            ////    Debug.DrawRay(headTarget.head.target.transform.position, headTarget.head.target.transform.rotation * head2neck);
            ////    headTarget.neck.target.transform.position = headTarget.head.target.transform.position + headTarget.head.target.transform.rotation * head2neck;
            ////}

            //Vector3 head2neck = headTarget.GetHeadNeckDelta();
            //Vector3 headPosition = headTarget.head.target.transform.position;
            //headTarget.neck.target.transform.position = headTarget.head.target.transform.position + headTarget.head.target.transform.rotation * head2neck;
            //headTarget.head.target.transform.position = headPosition;

        }

        public static Quaternion CalculateNeckRotation(Quaternion hipRotation, Quaternion headRotation) {
            Vector3 headAnglesCharacterSpace = (Quaternion.Inverse(hipRotation) * headRotation).eulerAngles;
            float neckYRotation = UnityAngles.Normalize(headAnglesCharacterSpace.y) * 0.6F;
            Quaternion neckRotation = hipRotation * Quaternion.Euler(headAnglesCharacterSpace.x, neckYRotation, headAnglesCharacterSpace.z);

            return neckRotation;
        }

        public static Vector3 CalculateNeckPosition(Vector3 eyePosition, Quaternion eyeRotation, Vector3 eye2neck) {
            Vector3 neckPosition = eyePosition + eyeRotation * eye2neck;
            return neckPosition;
        }

        static float lastTime;
        private static void UpdateHeadBonesFromGazeDirection(HeadTarget headTarget) {
#if hFACE
            Quaternion neckParentRotation = headTarget.humanoid.hipsTarget.hips.bone.transform.rotation * headTarget.humanoid.hipsTarget.hips.bone.toTargetRotation;

            Quaternion oldNeckRotation = headTarget.neck.bone.transform.rotation * headTarget.neck.bone.toTargetRotation;
            Vector3 localNeckAngles = (Quaternion.Inverse(neckParentRotation) * oldNeckRotation).eulerAngles;

            Quaternion predictedRotation = Quaternion.Slerp(Quaternion.identity, headTarget.neck.bone.rotationVelocity, Time.deltaTime);

            Quaternion eyeRotation = headTarget.face.leftEye.target.transform.rotation;
            Vector3 localEyeAngles = (Quaternion.Inverse(oldNeckRotation) * eyeRotation).eulerAngles;
            if (Quaternion.Angle(oldNeckRotation, eyeRotation) < 2) {
                lastTime = Time.time;
                return;
            }

            float headTensionY = CalculateTension(localNeckAngles, headTarget.neck.bone.maxAngles);
            float eyeTensionY = CalculateTension(localEyeAngles, headTarget.face.leftEye.bone.maxAngles);
            float f = Mathf.Clamp01(1 - (headTensionY / eyeTensionY));

            Quaternion targetRotation = Quaternion.LookRotation(headTarget.face.gazeDirection, Vector3.up); // this will overshoot,  because the lookdirection itself is affected...

            Quaternion neckRotation = Quaternion.RotateTowards(oldNeckRotation, targetRotation, f * 3);
            Quaternion desiredRotation = Quaternion.Inverse(oldNeckRotation) * neckRotation;

            // This limits the speed changes
            float deltaTime = Time.time - lastTime;
            Quaternion resultRotation = Quaternion.RotateTowards(predictedRotation, desiredRotation, deltaTime * 50);
            neckRotation = oldNeckRotation * resultRotation;

            headTarget.neck.bone.transform.rotation = neckRotation * headTarget.neck.target.toBoneRotation;
            lastTime = Time.time;
#endif
        }

        private void UpdateHeadBonesFromLookDirection() {
            Quaternion neckParentRotation = headTarget.humanoid.hipsTarget.hips.bone.transform.rotation * headTarget.humanoid.hipsTarget.hips.bone.toTargetRotation;

            Quaternion oldNeckRotation = headTarget.neck.bone.transform.rotation * headTarget.neck.bone.toTargetRotation;
            Vector3 localNeckAngles = (Quaternion.Inverse(neckParentRotation) * oldNeckRotation).eulerAngles;

            Quaternion predictedRotation = Quaternion.Slerp(Quaternion.identity, headTarget.neck.bone.rotationVelocity, Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(headTarget.lookDirection, Vector3.up); // this will overshoot,  because the lookdirection itself is affected...

            Quaternion neckRotation = targetRotation;
            Quaternion desiredRotation = Quaternion.Inverse(oldNeckRotation) * neckRotation;

            // This limits the speed changes
            Quaternion resultRotation = Quaternion.RotateTowards(predictedRotation, desiredRotation, Time.deltaTime * 6);
            neckRotation = oldNeckRotation * resultRotation;

            headTarget.neck.bone.transform.rotation = neckRotation * headTarget.neck.target.toBoneRotation;
        }
        #endregion

        private static float CalculateTension(Vector3 angle, Vector3 maxAngle) {
            float dX = CalculateTension(angle.x, maxAngle.x);
            float dY = CalculateTension(angle.y, maxAngle.y);
            float dZ = CalculateTension(angle.z, maxAngle.z);
            return (dX + dY + dZ);
        }

        private static float CalculateTension(float angle, float maxAngle) {
            return (maxAngle != 0) ? Mathf.Abs(Angle.Normalize(angle) / maxAngle) : 0;
        }
    }

    public class HeadCollisionHandler : MonoBehaviour {

        static public SphereCollider AddHeadCollider(GameObject headObject) {
            HeadTarget headTarget = headObject.GetComponent<HeadTarget>();
            if (headTarget.headRigidbody == null) {
                headTarget.headRigidbody = headObject.AddComponent<Rigidbody>();
                if (headTarget.headRigidbody != null) {
                    headTarget.headRigidbody.mass = 1;
                    headTarget.headRigidbody.useGravity = false;
                    headTarget.headRigidbody.isKinematic = true;
                }
            }

            SphereCollider collider = headObject.AddComponent<SphereCollider>();
            if (collider != null) {
                collider.isTrigger = true;
                collider.radius = 0.1F;
                collider.center = new Vector3(0, 0, 0.05F);
            }

            return collider;
        }

        private HumanoidControl humanoid;
        private Material fadeMaterial;

        public void Initialize(HumanoidControl _humanoid) {
            humanoid = _humanoid;
            if (humanoid.headTarget.collisionFader)
                FindFadeMaterial();
        }

        private void FindFadeMaterial() {
            if (humanoid.headTarget.unityVRHead == null || humanoid.headTarget.unityVRHead.cameraTransform == null)
                return;

            Transform plane = humanoid.headTarget.unityVRHead.cameraTransform.Find("Fader");
            if (plane != null) {
                Renderer renderer = plane.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.enabled = true;
                    fadeMaterial = renderer.sharedMaterial;
                    Color color = Color.black;
                    color.a = 0.0F;
                    fadeMaterial.color = color;
                }
            }
        }

        void OnTriggerEnter(Collider otherCollider) {
            if (fadeMaterial == null || otherCollider.isTrigger)
                return;

            if (otherCollider.gameObject.isStatic) {
                DoFadeOut();
            }
        }

        void OnTriggerExit(Collider otherCollider) {
            if (fadeMaterial == null || otherCollider.isTrigger)
                return;

            if (otherCollider.gameObject.isStatic) {
                DoFadeIn();
            }
        }

        private void DoFadeOut() {
            UnityEngine.Debug.Log("Fade Out");
            StartCoroutine(FadeOut(fadeMaterial));
        }
        private void DoFadeIn() {
            UnityEngine.Debug.Log("Fade In");
            StartCoroutine(FadeIn(fadeMaterial));
        }

        private bool faded = false;
        public float fadeTime = 0.3F;

        public IEnumerator FadeOut(Material fadeMaterial) {
            if (!faded) {
                float elapsedTime = 0.0f;
                Color color = Color.black;
                color.a = 0.0f;
                fadeMaterial.color = color;
                while (elapsedTime < fadeTime) {
                    yield return new WaitForEndOfFrame();
                    elapsedTime += Time.deltaTime;
                    color.a = Mathf.Clamp01(elapsedTime / fadeTime);
                    fadeMaterial.color = color;
                }
            }
            faded = true;
        }

        public IEnumerator FadeIn(Material fadeMaterial) {
            if (faded) {
                float elapsedTime = 0.0f;
                Color color = fadeMaterial.color = Color.black;
                while (elapsedTime < fadeTime) {
                    yield return new WaitForEndOfFrame();
                    elapsedTime += Time.deltaTime;
                    color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
                    fadeMaterial.color = color;
                }
            }
            faded = false;
        }
    }
}