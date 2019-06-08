using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class TorsoAnimator : UnityTorsoSensor {

        private float torsoLength;

        private Quaternion torsoUprightOrientation;

        #region Start
        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);

            if (humanoid.avatarRig == null || hipsTarget.hips.bone.transform == null)
                return;

            Vector3 torsoDirection = humanoid.headTarget.neck.bone.transform.position - hipsTarget.hips.bone.transform.position;
            torsoLength = torsoDirection.magnitude;

            Vector3 humanoidUp = humanoid.useGravity ? Vector3.up : humanoid.transform.up;
            torsoUprightOrientation = Quaternion.FromToRotation(humanoidUp, torsoDirection);
        }
        #endregion

        #region Update
        public override void Update() {
            if (!hipsTarget.humanoid.animatorEnabled || !enabled || hipsTarget.humanoid.targetsRig.runtimeAnimatorController != null)
                return;
            if (hipsTarget.humanoid.avatarRig == null || hipsTarget.hips.bone.transform == null)
                return;

            status = Status.Tracking;

            StoreTargets();
            UpdateHipsPosition();
            UpdateHipsRotation();
            RestoreTargets();
        }

        private Quaternion neckRotation;
        private Vector3 neckPosition;
        private Quaternion leftHandRotation;
        private Vector3 leftHandPosition;
        private Quaternion rightHandRotation;
        private Vector3 rightHandPosition;
        private Quaternion leftFootRotation;
        private Vector3 leftFootPosition;
        private Quaternion rightFootRotation;
        private Vector3 rightFootPosition;
        private void StoreTargets() {
            neckRotation = hipsTarget.humanoid.headTarget.neck.target.transform.rotation;
            neckPosition = hipsTarget.humanoid.headTarget.neck.target.transform.position;
            leftHandRotation = hipsTarget.humanoid.leftHandTarget.hand.target.transform.rotation;
            leftHandPosition = hipsTarget.humanoid.leftHandTarget.hand.target.transform.position;
            rightHandRotation = hipsTarget.humanoid.rightHandTarget.hand.target.transform.rotation;
            rightHandPosition = hipsTarget.humanoid.rightHandTarget.hand.target.transform.position;
            leftFootRotation = hipsTarget.humanoid.leftFootTarget.foot.target.transform.rotation;
            leftFootPosition = hipsTarget.humanoid.leftFootTarget.foot.target.transform.localPosition;
            rightFootRotation = hipsTarget.humanoid.rightFootTarget.foot.target.transform.rotation;
            rightFootPosition = hipsTarget.humanoid.rightFootTarget.foot.target.transform.localPosition;
        }

        private void RestoreTargets() {
            hipsTarget.humanoid.headTarget.neck.target.transform.rotation = neckRotation;
            hipsTarget.humanoid.headTarget.neck.target.transform.position = neckPosition;
            hipsTarget.humanoid.leftHandTarget.hand.target.transform.rotation = leftHandRotation;
            hipsTarget.humanoid.leftHandTarget.hand.target.transform.position = leftHandPosition;
            hipsTarget.humanoid.rightHandTarget.hand.target.transform.rotation = rightHandRotation;
            hipsTarget.humanoid.rightHandTarget.hand.target.transform.position = rightHandPosition;
            hipsTarget.humanoid.leftFootTarget.foot.target.transform.rotation = leftFootRotation;
            hipsTarget.humanoid.leftFootTarget.foot.target.transform.localPosition = leftFootPosition;
            hipsTarget.humanoid.rightFootTarget.foot.target.transform.rotation = rightFootRotation;
            hipsTarget.humanoid.rightFootTarget.foot.target.transform.localPosition = rightFootPosition;
        }

        private void UpdateHipsRotation() {
            if (hipsTarget.hips.target.confidence.rotation < 0.5F) {
                Quaternion hipsTargetRotation = hipsTarget.hips.target.transform.rotation;
                Quaternion headTargetRotation = hipsTarget.humanoid.headTarget.head.target.transform.rotation;

                Vector3 neckTargetPosition = hipsTarget.humanoid.headTarget.neck.target.transform.position;

                // still need to add foot based rotation
                if (hipsTarget.humanoid.leftHandTarget.hand.target.confidence.rotation > 0.5F && hipsTarget.humanoid.rightHandTarget.hand.target.confidence.rotation > 0.5F) {
                    Quaternion newHipsRotation = TorsoMovements.CalculateHipsRotation(hipsTarget.hips.target.transform.position, hipsTargetRotation, hipsTarget.humanoid.leftHandTarget.transform.rotation, hipsTarget.humanoid.rightHandTarget.transform.rotation, hipsTarget.humanoid.leftFootTarget.transform, hipsTarget.humanoid.rightFootTarget.transform, headTargetRotation, neckTargetPosition);

                    hipsTarget.hips.target.transform.rotation = newHipsRotation;
                }
                else {
                    Vector3 hipsUpDirection = hipsTarget.humanoid.up;
                    Vector3 hipsForwardDirection = headTargetRotation * Vector3.back;
                    hipsTarget.hips.target.transform.rotation = Quaternion.LookRotation(hipsUpDirection, hipsForwardDirection) * Quaternion.Euler(90, 0, 0);
                }
            }
        }

        private void UpdateHipsPosition() {
            if (hipsTarget.hips.target.confidence.rotation > 0.25F)
                return;

            Vector3 humanoidUp = hipsTarget.humanoid.up;

            Vector3 oldHipPosition = hipsTarget.hips.target.transform.position;
            Vector3 neckPosition = hipsTarget.humanoid.headTarget.neck.target.transform.position;

            Vector3 spineDirection = hipsTarget.humanoid.transform.InverseTransformDirection(neckPosition - hipsTarget.transform.position);
            float hipsAngle = Vector3.Angle(humanoidUp, spineDirection);

            // This is necessary here because the avatar can be changed or scaled.
            torsoLength = Vector3.Distance(hipsTarget.humanoid.headTarget.neck.bone.transform.position, hipsTarget.humanoid.hipsTarget.hips.bone.transform.position);

            Vector3 spineAngles = (Quaternion.FromToRotation(Vector3.up, spineDirection)).eulerAngles;

            Vector3 backVector = Vector3.down * torsoLength;
            Vector3 spine = hipsTarget.humanoid.transform.rotation * Quaternion.Euler(spineAngles) * backVector;

            Vector3 hipsPosition = neckPosition + spine;

            Vector3 headPosition = neckPosition - hipsTarget.humanoid.transform.position;
            float verticalBodyStretch = headPosition.y - hipsTarget.humanoid.avatarNeckHeight;

            //Vector3 hipsUp = hipsTarget.hips.target.transform.rotation * Vector3.up;
            //Vector3 hips2neck = neckPosition - hipsPosition;
            //float angleGap = Vector3.Angle(hipsUp, hips2neck);

            if (verticalBodyStretch >= -0.01F || hipsAngle > 80 || Mathf.Abs(spineDirection.normalized.x) > 0.2F || spineDirection.z < -0.15F) { // || angleGap > 1) {
                // standing upright

                Vector3 uprightSpine = hipsTarget.humanoid.transform.rotation * torsoUprightOrientation * backVector;
                Vector3 targetHipPosition = neckPosition + uprightSpine;
                Vector3 toTargetHipPosition = targetHipPosition - hipsPosition;

                if (hipsAngle < 30) {
                    hipsPosition = hipsPosition + Vector3.ClampMagnitude(toTargetHipPosition, 0.02F);
                }
                else {
                    hipsPosition = targetHipPosition;
                }
            }
            hipsTarget.hips.target.transform.position = hipsPosition;

            Vector3 controllerPosition = Quaternion.Inverse(hipsTarget.humanoid.transform.rotation) * (hipsPosition - hipsTarget.humanoid.transform.position);
            Vector3 movementDirection = new Vector3(controllerPosition.x - oldHipPosition.x, 0, controllerPosition.z - oldHipPosition.z).normalized;
            float angle = Vector3.Angle(movementDirection, hipsTarget.humanoid.hitNormal);

            if (hipsTarget.humanoid.physics && (hipsTarget.humanoid.collided && angle >= 90)) {
                hipsTarget.hips.target.transform.position = oldHipPosition;
            }

            // Make sure the neck position has not changed at all
            hipsTarget.humanoid.headTarget.neck.target.transform.position = neckPosition;
        }

        public static Quaternion CalculateChestRotation(Quaternion chestRotation, Quaternion hipRotation, Quaternion headRotation) {
            Vector3 chestAngles = chestRotation.eulerAngles;
            Vector3 headAnglesCharacterSpace = (Quaternion.Inverse(hipRotation) * headRotation).eulerAngles;
            float chestYRotation = UnityAngles.Normalize(headAnglesCharacterSpace.y) * 0.3F;
            Quaternion newChestRotation = hipRotation * Quaternion.Euler(chestAngles.x, chestYRotation, chestAngles.z);

            return newChestRotation;
        }
        #endregion
    }
}