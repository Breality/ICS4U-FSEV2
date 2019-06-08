/* InstantVR Animator
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.4
 * date: April 15, 2016
 * 
 * - added namespace
 */
using UnityEngine;

namespace Passer {

    [System.Serializable]
    public class HeadAnimator : UnityHeadSensor {
        public bool headAnimation = true;
        public bool faceAnimation = true;

        #region Update
        public override void Update() {
            if (!headTarget.humanoid.animatorEnabled || !enabled || headTarget.humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            if (headAnimation)
                UpdateNeck();
        }

        private void UpdateNeck() {
            if (headTarget.neck.target.confidence.rotation > 0.25F)
                return;

            Vector3 headPosition = headTarget.head.target.transform.position;
            Quaternion headRotation = headTarget.head.target.transform.rotation;

            headTarget.neck.target.transform.rotation = headTarget.head.target.transform.rotation;

            headTarget.neck.target.transform.position = headPosition - headTarget.neck.target.transform.rotation * Vector3.up * headTarget.neck.bone.length;

            headTarget.head.target.transform.position = headPosition;
            headTarget.head.target.transform.rotation = headRotation;
        }
        #endregion

        //float lastFocus;

        //        private Vector3 FocusOnHumanoid(HumanoidControl humanoid) {
        //#if hFACE
        //            if (focusTransformID == 0 && humanoid.headTarget.face.leftEye.target.transform != null) {
        //                return humanoid.headTarget.face.leftEye.target.transform.position;
        //            } else if (focusTransformID == 1 && humanoid.headTarget.face.rightEye.target.transform != null) {
        //                return humanoid.headTarget.face.rightEye.target.transform.position;
        //            } else if (focusTransformID == 2 && humanoid.headTarget.face.jaw.target.transform != null) {
        //                return humanoid.headTarget.face.jaw.target.transform.position;
        //            } else {
        //#else
        //            { 
        //#endif
        //            return humanoid.headTarget.transform.position;
        //            }
        //        }

        //        public int focusTransformID;

        //#region Humanoid Interaction
        //        // returns the most interesting humanoid in sight
        //        private HumanoidControl HumanoidInSightline() {
        //            HumanoidControl mostInterestingHumanoid = null;
        //            float mostInterest = 0; // minimum interest = 0, humanoids are only interesting when the interest > 0

        //            for (int i = 0; i < HumanoidControl.allHumanoids.Length; i++) {
        //                HumanoidControl otherHumanoid = HumanoidControl.allHumanoids[i];
        //                if (otherHumanoid == headTarget.humanoid
        //#if hSTEAMVR || hOCULUS
        //                    || !otherHumanoid.IsVisible(headTarget.humanoid.headTarget.unityVRHead.camera)
        //#endif
        //                    )
        //                    continue;

        //                float interest = CalculateInterest(otherHumanoid);
        //                if (interest > mostInterest) {
        //                    mostInterestingHumanoid = HumanoidControl.allHumanoids[i];
        //                    mostInterest = interest;
        //                }
        //            }
        //            return mostInterestingHumanoid;
        //        }

        //        private void DrawAllInterests() {
        //            for (int i = 0; i < HumanoidControl.allHumanoids.Length; i++) {
        //                HumanoidControl otherHumanoid = HumanoidControl.allHumanoids[i];
        //                float interest = CalculateInterest(otherHumanoid);
        //                Debug.DrawRay(otherHumanoid.headTarget.neck.bone.transform.position + new Vector3(0, 0.3F, 0), Vector3.up * interest, Color.black);
        //            }
        //        }

        //        private float CalculateInterest(HumanoidControl otherHumanoid) {
        //            Vector3 sightlineStart = headTarget.head.target.transform.position + headTarget.head.target.transform.forward * 0.2F;
        //            Vector3 sightlineDirection = headTarget.head.target.transform.forward;

        //            Vector3 headPosition = otherHumanoid.headTarget.transform.position;
        //            Vector3 directionToOtherHumanoid = headPosition - sightlineStart;

        //            float distanceToHead = (headPosition - sightlineStart).magnitude;
        //            float angleToSightline = Vector3.Angle(sightlineDirection, directionToOtherHumanoid);

        //            float interest;

        //            // Distance interest
        //            if (HumanoidIsLookingAtMe(otherHumanoid, headTarget.humanoid))
        //                interest = Mathf.Clamp((60 - angleToSightline) / 100 + (3 - distanceToHead) / 3, -1, 1);
        //            else
        //                interest = Mathf.Clamp((60 - angleToSightline) / 100 + (2 - distanceToHead), -1, 1);

        //            // Movement interest
        //            float limbSpeed = CalculateLimbSpeed(otherHumanoid);
        //            interest += limbSpeed / 10;

        //            // Audio interest
        //            float threshold = 0.35F;
        //            float filteredAudioEnergy = Mathf.Max(0, (otherHumanoid.headTarget.audioEnergy - threshold)); // * 3);

        //            interest = Mathf.Clamp(interest + filteredAudioEnergy /*+ introvertExtrovert * 0.25F*/, -1, 1);

        //            return interest;
        //        }

        //        private bool HumanoidIsLookingAtMe(HumanoidControl otherHumanoid, HumanoidControl me) {
        //            if (otherHumanoid != null) {
        //                Camera camera = otherHumanoid.GetComponentInChildren<Camera>();
        //                if (camera != null) {
        //                    Vector3 otherHeadPosition = camera.transform.position;
        //                    Vector3 otherLookDirection = camera.transform.forward;

        //                    float distanceToRayHit = DistanceToLine(new Ray(otherHeadPosition, otherLookDirection), me.headTarget.transform.position);
        //                    return (distanceToRayHit < 0.3F);
        //                } else {
        //                    Vector3 otherHeadPosition = otherHumanoid.headTarget.transform.position;
        //                    Vector3 otherLookDirection = otherHumanoid.headTarget.transform.forward;

        //                    float distanceToRayHit = DistanceToLine(new Ray(otherHeadPosition, otherLookDirection), me.headTarget.transform.position);
        //                    return (distanceToRayHit < 0.3F);
        //                }
        //            }
        //            return false;
        //        }

        //        private float CalculateLimbSpeed(HumanoidControl otherHumanoid) {
        //            /*
        //             * This needs a store with knowledge about other Humanoids
        //             * which is not implemented yet
        //                float leftHandSpeed = (otherHumanoid.leftHandTarget.transform.position - otherHumanoid.lastLeftHandPosition).magnitude / Time.deltaTime;
        //                float rightHandSpeed = (otherHumanoid.rightHandTarget.transform.position - otherHumanoid.lastRightHandPosition).magnitude / Time.deltaTime;

        //                otherHumanoid.lastLeftHandPosition = otherHumanoid.leftHandTarget.transform.position;
        //                otherHumanoid.lastRightHandPosition = otherHumanoid.rightHandTarget.transform.position;

        //                return Mathf.Max(leftHandSpeed, rightHandSpeed);
        //            */
        //            return 0;
        //        }

        //        private static float DistanceToLine(Ray ray, Vector3 point) {
        //            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        //        }
        //#endregion
    }
}