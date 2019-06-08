using Passer.Humanoid.Tracking;
using UnityEngine;

namespace Passer {
    public static class TransformExtension {
        public static Transform FindDeepChild(this Transform parent, string name) {
            if (!parent.gameObject.activeInHierarchy)
                return null;

            Transform result = parent.Find(name);
            if (result != null)
                return result;

            foreach (Transform child in parent) {
                result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

//    public interface IControl {
//        //HeadTarget headTarget { get; set; }
//        //HandTarget leftHandTarget { get; set; }
//        //HandTarget rightHandTarget { get; set; }

//        bool isHumanoid { get; }
//        Transform transform { get; }

//#if hOCULUS
//        OculusTracker oculus { get; }
//#endif

//    }

    public interface ITarget {
        HumanoidTarget.TargetedBone[] GetBones();
        SkinnedMeshRenderer blendshapeRenderer {
            get;
        }
        string[] GetBlendshapeNames();
        int FindBlendshape(string namepart);
        void SetBlendshapeWeight(string name, float weight);
        float GetBlendshapeWeight(string name);
    }

    public abstract class Target : MonoBehaviour {
        /// <summary>
        /// Main targeted bone, matches the humanoid target
        /// </summary>

        public bool _showRealObjects = true;
        public virtual bool showRealObjects {
            get { return _showRealObjects; }
            set { _showRealObjects = value; }
        }

        public virtual void InitComponent() { }

        public static bool IsNotInitialized(Quaternion q) {
            return (q.x == 0 && q.y == 0 && q.z == 0 && q.w == 0);
        }

        public static Vector ToVector(Vector3 vector3) {
            return new Vector(vector3.x, vector3.y, vector3.z);
        }

        public static Vector3 ToVector3(Vector position) {
            return new Vector3(position.x, position.y, position.z);
        }

        public static Rotation ToRotation(Quaternion quaternion) {
            return new Rotation(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static Quaternion ToQuaternion(Rotation orientation) {
            return new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w);
        }

        public static void SetRotation(Transform transform, Rotation orientation) {
            transform.rotation = ToQuaternion(orientation);
        }

        public abstract Transform GetDefaultTarget(HumanoidControl humanoid);


        public abstract void StartTarget();
        public abstract void InitSensors();
        public abstract void StartSensors();
        protected abstract void UpdateSensors();
        public virtual void StopSensors() { }
        public abstract void UpdateTarget();
        public abstract void UpdateMovements(HumanoidControl humanoid);
    }

    [System.Serializable]
    public abstract class HumanoidTarget : Target {
        public HumanoidControl humanoid;
        //public IControl iControl;

        public abstract TargetedBone main {
            get;
        }

        public abstract void InitAvatar();

        public virtual void NewComponent(HumanoidControl _humanoid) {
            humanoid = _humanoid;
        }

        #region Gizmos
        public void OnDrawGizmos() {
            if (humanoid == null)
                return;
            if (humanoid.showTargetRig)
                DrawTargetRig(humanoid);
            if (humanoid.showAvatarRig)
                DrawAvatarRig(humanoid);
        }
        public void OnDrawGizmosSelected() {
            if (humanoid == null)
                return;

            DrawTensions();
        }

        public virtual void DrawTargetRig(HumanoidControl humanoid) { }
        public virtual void DrawAvatarRig(HumanoidControl humanoid) { }

        public static void DrawTarget(Confidence confidence, Transform target, Vector3 direction, float length) {
            if (target == null)
                return;
            if (confidence.rotation > 0.8F)
                Debug.DrawRay(target.position, target.rotation * direction * length, Color.green);
            else if (confidence.rotation > 0.6F)
                Debug.DrawRay(target.position, target.rotation * direction * length, Color.yellow);
            else if (confidence.rotation > 0F)
                Debug.DrawRay(target.position, target.rotation * direction * length, Color.red);
            else
                Debug.DrawRay(target.position, target.rotation * direction * length, Color.black);
        }

        public static void DrawTargetBone(TargetedBone bone, Vector3 direction) {
            DrawTargetBone(bone.target, direction);
        }
        public static void DrawTargetBone(TargetTransform target, Vector3 direction) {
            if (target.transform == null)
                return;

            if (target.confidence.rotation > 0.8F)
                Debug.DrawRay(target.transform.position, target.transform.rotation * direction * target.length, Color.green);
            else if (target.confidence.rotation > 0.6F)
                Debug.DrawRay(target.transform.position, target.transform.rotation * direction * target.length, Color.yellow);
            else if (target.confidence.rotation > 0F)
                Debug.DrawRay(target.transform.position, target.transform.rotation * direction * target.length, Color.red);
            else
                Debug.DrawRay(target.transform.position, target.transform.rotation * direction * target.length, Color.black);
        }

        public static void DrawAvatarBone(TargetedBone bone, Vector3 direction) {
            DrawAvatarBone(bone.bone, direction);
        }
        public static void DrawAvatarBone(BoneTransform bone, Vector3 direction) {
            if (bone.transform == null)
                return;

            Debug.DrawRay(bone.transform.position, bone.targetRotation * direction * bone.length, Color.cyan);
        }

        public virtual void DrawTensions() { }
        //private void DrawTensions() {
        //    if (bones == null || humanoid == null || !humanoid.showMuscleTension)
        //        return;

        //    foreach (TargetedBone bone in bones)
        //        DrawTensionGizmo(bone);
        //}

        protected virtual void DrawTensionGizmo(TargetedBone targetedBone) {
            if (targetedBone.bone.transform == null)
                return;

            float tension = targetedBone.GetTension();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetedBone.bone.transform.position, tension * 0.05F);
        }
        #endregion

        // boneNames convention:
        // 0 = UMA
        // 1 = MCS / Morph3D
        // 2 = AutoDesk
        public static void GetDefaultBone(Animator rig, ref Transform boneTransform, Bone boneId, params string[] boneNames) {
            GetDefaultBone(rig, ref boneTransform, BoneReference.HumanBodyBone(boneId), boneNames);
        }
        public static void GetDefaultBone(Animator rig, ref Transform boneTransform, HumanBodyBones boneID, params string[] boneNames) {
            if (boneTransform != null || rig == null)
                return;

            boneTransform = rig.GetBoneTransform(boneID);
            if (boneTransform != null)
                return;

            for (int i = 0; i < boneNames.Length; i++) {
                if (boneTransform != null)
                    return;

                boneTransform = rig.transform.FindDeepChild(boneNames[i]);
            }
        }

        public static void GetDefaultTargetBone(Animator rig, ref Transform boneTransform, Bone boneID, params string[] boneNames) {
            GetDefaultBone(rig, ref boneTransform, boneID, boneNames);
            if (boneTransform == null) {
                boneTransform = TargetedBone.NewTargetTransform(boneID.ToString());
            }
        }

        // For bones not in the Mecanim rig
        // boneNames convention:
        // 0 = UMA
        // 1 = MCS / Morph3D
        // 2 = AutoDesk
        public static void GetDefaultBone(Animator rig, ref Transform boneTransform, params string[] boneNames) {
            if (boneTransform != null)
                return;

            for (int i = 0; i < boneNames.Length; i++) {
                if (boneNames[i] == null)
                    continue;

                boneTransform = rig.transform.FindDeepChild(boneNames[i]);
                if (boneTransform != null)
                    return;
            }

        }

        //public virtual void MatchTargetsToAvatar() {
        //    for (int i = 0; i < bones.Length; i++)
        //        bones[i].CopyBoneToTarget();
        //    if (main.bone.transform != null && transform != null) {
        //        transform.position = main.target.transform.position;
        //        transform.rotation = main.target.transform.rotation;
        //    }
        //}
        //public virtual void CopyTargetToRig() { }
        //public virtual void CopyRigToTarget() { }

        public abstract void MatchTargetsToAvatar();
        public abstract void CopyTargetToRig();
        public abstract void CopyRigToTarget();

        // Needed for Networking
        public abstract void EnableAnimator(bool enabled);

        [System.Serializable]
        public class TargetTransform : TargetData {
            public Transform transform = null;

            public Quaternion baseRotation = Quaternion.identity;
            public Vector3 basePosition = Vector3.zero;

            public Quaternion toBoneRotation = Quaternion.identity;

            [System.NonSerialized]
            private float lastTime;
            [System.NonSerialized]
            public Vector3 lastPosition;
            [System.NonSerialized]
            public Vector3 velocity;

            [System.NonSerialized]
            private Quaternion lastRotation;
            [System.NonSerialized]
            public Quaternion rotationVelocity;

            public void CalculateVelocity() {
                if (transform == null)
                    return;

                float deltaTime = Time.time - lastTime;

                velocity = (transform.position - lastPosition) / deltaTime;
                rotationVelocity = Quaternion.SlerpUnclamped(lastRotation, transform.rotation, 1 / deltaTime);

                lastTime = Time.time;
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }

            //public void Update(Transform trackerTransform) {
            //    if (targetTransform == null)
            //        return;

            //    confidence.position = targetTransform.positionConfidence;
            //    transform.position = trackerTransform.TransformPoint(targetTransform.position);

            //    confidence.rotation = targetTransform.rotationConfidence;
            //    transform.rotation = trackerTransform.rotation * targetTransform.rotation;
            //}

            //public float positionConfidence {
            //    get {
            //        if (targetTransform != null)
            //            return targetTransform.positionConfidence;
            //        else
            //            return confidence.position;
            //    }
            //}
            //public float rotationConfidence {
            //    get {
            //        if (targetTransform != null)
            //            return targetTransform.rotationConfidence;
            //        else
            //            return confidence.rotation;
            //    }
            //}
        }

        [System.Serializable]
        public class BoneTransform {
            public Transform transform;
            public float length;
            public bool jointLimitations = false;
            public float maxAngle;
            public Vector3 minAngles;
            public Vector3 maxAngles;

            // The local rotation in de avatar rig
            public Quaternion baseRotation = Quaternion.identity;
            public Vector3 basePosition = Vector3.zero;

            public Quaternion toTargetRotation;
            public Quaternion targetRotation { get { return transform.rotation * toTargetRotation; } }

            private float lastTime;
            private Quaternion lastRotation;
            public Quaternion rotationVelocity;

            //private float lastPositionTime;
            private Vector3 lastPosition;
            public Vector3 velocity;

            public Quaternion CalculateAngularVelocity() {
                if (transform == null)
                    return Quaternion.identity;

                float deltaTime = Time.time - lastTime;

                Quaternion boneOrientation = transform.rotation * toTargetRotation;
                Quaternion boneRotation = Quaternion.Inverse(lastRotation) * boneOrientation;
                rotationVelocity = Quaternion.SlerpUnclamped(Quaternion.identity, boneRotation, 1 / deltaTime);

                lastTime = Time.time;
                lastRotation = boneOrientation;

                return rotationVelocity;
            }
            public Vector3 CalculateVelocity() {
                float deltaTime = Time.time - lastTime;

                velocity = (lastPosition - transform.position) / deltaTime;

                //lastPositionTime = Time.time;
                lastPosition = transform.position;

                return velocity;
            }
        }

        [System.Serializable]
        public class TargetedBone {
            public string name;
            public Bone boneId = Bone.None;
            public TargetTransform target = new TargetTransform();
            public BoneTransform bone = new BoneTransform();

            [System.NonSerialized]
            public TargetedBone parent;
            [System.NonSerialized]
            public TargetedBone nextBone;

            public TargetedBone() { }

            public TargetedBone(TargetedBone _nextBone) {
                nextBone = _nextBone;
            }

            public virtual void Init() { }

            public bool isPresent {
                get { return target.confidence.position > 0; }
                // 0 says that we could not find a bone and that it is not possible to set a position.
            }

            public static Transform NewTargetTransform(string name) {
                GameObject obj = new GameObject(name);
                obj.transform.rotation = Quaternion.identity;
                return obj.transform;
            }

            public void RetrieveBones(HumanoidControl humanoid) {
                if (humanoid.targetsRig != null)
                    GetDefaultTargetBone(humanoid.targetsRig, ref target.transform, boneId);
                if (humanoid.avatarRig != null)
                    GetDefaultBone(humanoid.avatarRig, ref bone.transform, boneId);
            }

            public void RetrieveBone(HumanoidControl humanoid, HumanBodyBones boneID) {
                if ((bone.transform == null || bone.transform == null) && humanoid.avatarRig != null) {
                    bone.transform = humanoid.avatarRig.GetBoneTransform(boneID);
                }
            }

            public virtual Quaternion DetermineRotation() {
                if (target.transform != null)
                    return target.transform.rotation;
                else
                    return Quaternion.identity;
            }

            public virtual void MatchTargetToAvatar() {
                if (bone.transform == null || target.transform == null)
                    return;

                target.transform.position = bone.transform.position;
                target.transform.rotation = bone.targetRotation;

                DetermineBasePosition();
                DetermineBaseRotation();
            }

            protected virtual void DetermineBasePosition() {
                if (target.basePosition.sqrMagnitude != 0)
                    // Base Position is already determined
                    return;

                if (parent != null) {
                    target.basePosition = parent.target.transform.InverseTransformPoint(target.transform.position);
                }
                else {
                    target.basePosition = target.transform.parent.InverseTransformPoint(target.transform.position);
                }
            }
            protected virtual void DetermineBaseRotation() {
                if (target.basePosition.sqrMagnitude != 0)
                    // Base Rotation is already determined
                    return;

                if (parent != null) {
                    target.baseRotation = Quaternion.Inverse(parent.target.transform.rotation) * target.transform.rotation;
                }
                else {
                    target.baseRotation = Quaternion.Inverse(target.transform.parent.rotation) * target.transform.rotation;
                }
            }

            public void CopyBonePositionToTarget() {
                if (bone.transform != null && target.transform != null) {
                    target.transform.position = bone.transform.position;
                }
            }
            public virtual Vector3 TargetBasePosition() {
                Transform basePositionReference = target.transform.parent;
                return basePositionReference.TransformPoint(target.basePosition);
            }
            public virtual Quaternion TargetBaseRotation() {
                if (parent != null)
                    return parent.target.transform.rotation * target.baseRotation;
                else
                    return target.transform.parent.rotation * target.baseRotation;
            }

            public void SetTargetPositionToAvatar() {
                if (bone.transform != null && target.transform != null)
                    target.transform.position = bone.transform.position;
            }


            public void DoMeasurements() {
                CalculateTarget2BoneRotation();
                CalculateBoneLengths();
            }

            private void CalculateTarget2BoneRotation() {
                if (bone.transform != null) {
                    Quaternion targetRotation = DetermineRotation();
                    target.toBoneRotation = Quaternion.Inverse(targetRotation) * bone.transform.rotation;
                    bone.toTargetRotation = Quaternion.Inverse(bone.transform.rotation) * targetRotation;
                }
            }

            private void CalculateBoneLengths() {
                if (target.transform != null && bone.transform != null && nextBone != null && nextBone.bone.transform != null && nextBone.target.transform != null) {
                    bone.length = Vector3.Distance(bone.transform.position, nextBone.bone.transform.position);
                    if (bone.length <= 0)
                        Debug.Log("zero bone");
                    if (target != null)
                        target.length = Vector3.Distance(target.transform.position, nextBone.target.transform.position);
                }
                else {
                    bone.length = 0.02F; //default value
                }
            }

            public virtual float GetTension() {
                return 0;
            }

            protected static float GetTension(Quaternion restRotation, TargetedBone targetedBone) {
                float angle = Quaternion.Angle(restRotation, targetedBone.bone.targetRotation);
                float wristTension = GetTensionFromAngle(angle, targetedBone.bone.maxAngle);
                return wristTension;
            }

            private static float GetTensionFromAngle(float angle, float max) {
                angle = Mathf.Abs(angle);
                if (max == 0)
                    return 0;
                return angle / max;
            }

            public void SetBonePosition(Vector3 targetPosition) {
                if (bone.transform == null)
                    return;

                bone.transform.position = targetPosition;
            }

            public void SetBoneRotation(Quaternion targetRotation) {
                if (bone.transform == null)
                    return;

                bone.transform.rotation = targetRotation * target.toBoneRotation;
            }
        }

        public static void SetColliderToTrigger(GameObject obj, bool b) {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            for (int j = 0; j < colliders.Length; j++)
                colliders[j].isTrigger = b;
        }
    }
}