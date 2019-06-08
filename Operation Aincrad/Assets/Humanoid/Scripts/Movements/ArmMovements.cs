using UnityEngine;

namespace Passer {

    [System.Serializable]
    public class ArmMovements : Movements {

        #region Update
        public static void Update(HandTarget handTarget) {
            if (handTarget == null || handTarget.hand.bone.transform == null)
                return;


            if (handTarget.hand.target.confidence.position >= handTarget.forearm.target.confidence.rotation &&
                handTarget.hand.target.confidence.position >= handTarget.upperArm.target.confidence.rotation)

                handTarget.armMovements.FullInverseKinematics(handTarget, handTarget.rotationSpeedLimitation);

            else if (handTarget.forearm.target.confidence.position > handTarget.upperArm.target.confidence.rotation)
                handTarget.armMovements.ForearmForwardKinematics(handTarget);

            else
                handTarget.armMovements.FullForwardKinematics(handTarget);
        }

        public void FullInverseKinematics(HandTarget handTarget, bool speedLimitation = true) {

            Vector3 handPosition = NaturalHandPosition(handTarget);
            Quaternion handOrientation = handTarget.hand.bone.targetRotation;

            Quaternion shoulderRotation = NaturalShoulderOrientation(handTarget, ref lastLocalShoulderRotation);
            handTarget.shoulder.SetBoneRotation(shoulderRotation);

            CalculateStretchlessTarget(handTarget);
            handPosition = NaturalHandPosition(handTarget);

            Quaternion upperArmRotation = handTarget.armMovements.NaturalUpperArmOrientation(handTarget, handPosition, handOrientation, speedLimitation);
            handTarget.upperArm.SetBoneRotation(upperArmRotation);

            Quaternion forearmRotation = NaturalForearmOrientation(handTarget, upperArmRotation, handPosition, speedLimitation);
            handTarget.forearm.SetBoneRotation(forearmRotation);

            if (!Application.isPlaying || !handTarget.humanoid.physics || (handTarget.handRigidbody != null && handTarget.handRigidbody.isKinematic)) {
                if (handTarget.forearm.bone.transform != null) {
                    handOrientation = handTarget.armMovements.NaturalHandOrientation(handTarget, forearmRotation);
                    handTarget.hand.SetBoneRotation(handOrientation);
                    handTarget.hand.bone.transform.rotation = HandRotationLimitations(handTarget, handTarget.hand.bone.transform.rotation);

                    if (handTarget.stretchlessTarget != null) {
                        // We need to set the hand position because it is detached
                        PlaceHandOnForearm(handTarget, forearmRotation);
                        handTarget.hand.bone.transform.position = handPosition;
                    }
                    else {
                        handTarget.hand.bone.transform.position = handPosition;
                    }
                }
                else {
                    handTarget.hand.bone.transform.rotation = handTarget.hand.target.transform.rotation * handTarget.hand.target.toBoneRotation;
                    handTarget.hand.bone.transform.position = handTarget.hand.target.transform.position;
                }
            }
        }

        // Forearm Forward Kinematics, rest Inverse Kinematics
        private void ForearmForwardKinematics(HandTarget handTarget) {
            Vector3 upperArmUp = CalculateUpperArmUp(handTarget.forearm.target.transform.rotation);
            Quaternion upperArmRotation = CalculateUpperArmRotation(handTarget.upperArm.bone.transform.position, upperArmUp, handTarget.forearm.target.transform.position, handTarget.isLeft);
            handTarget.upperArm.bone.transform.rotation = upperArmRotation * handTarget.upperArm.target.toBoneRotation;
            handTarget.forearm.bone.transform.rotation = handTarget.forearm.target.transform.rotation * handTarget.forearm.target.toBoneRotation;

            CalculateStretchlessTarget(handTarget);
            if (!handTarget.humanoid.physics || handTarget.handRigidbody.isKinematic) {
                //Quaternion handOrientation = NaturalHandOrientation(forearmRotation);
                // Not working good enough yet
                Quaternion handOrientation = handTarget.hand.target.transform.rotation;
                handTarget.hand.bone.transform.rotation = handOrientation * handTarget.hand.target.toBoneRotation;
                // We need to set the hand position because it is detached
                handTarget.hand.bone.transform.position = handTarget.forearm.bone.transform.position + handTarget.forearm.target.transform.rotation * handTarget.outward * handTarget.forearm.bone.length;
            }
        }

        // Forward Kinematics
        private void FullForwardKinematics(HandTarget handTarget) {
            if (handTarget.humanoid.physics && (handTarget.handRigidbody == null || !handTarget.handRigidbody.isKinematic)) {
                // Hand is colliding, so we need to use Inverse Kinematics
                FullInverseKinematics(handTarget);
            }

            // Still needs rotation speed limits
            handTarget.shoulder.SetBoneRotation(handTarget.shoulder.target.transform.rotation);

            // Still needs rotation speed limits
            handTarget.upperArm.SetBoneRotation(handTarget.upperArm.target.transform.rotation);

            //Quaternion forearmRotation = NaturalForearmOrientation(upperArmRotation);
            // Not working yet
            handTarget.forearm.SetBoneRotation(handTarget.forearm.target.transform.rotation);

            CalculateStretchlessTarget(handTarget);
            if (!handTarget.humanoid.physics || handTarget.handRigidbody.isKinematic) {
                //Quaternion handOrientation = NaturalHandOrientation(forearmRotation);
                // Not working good enough yet
                Quaternion handOrientation = handTarget.hand.target.transform.rotation;
                handTarget.hand.bone.transform.rotation = handOrientation * handTarget.hand.target.toBoneRotation;

                // We need to set the hand position because it is detached
                PlaceHandOnForearm(handTarget, handTarget.forearm.target.transform.rotation);
            }
        }

        private static void CalculateStretchlessTarget(HandTarget handTarget) {
            if (handTarget.stretchlessTarget == null)
                return;

            if (handTarget.upperArm.bone.transform != null && handTarget.forearm.bone.transform != null) {
                float armLength = handTarget.upperArm.bone.length + handTarget.forearm.bone.length;
                //handTarget.transform.position = handTarget.hand.targetTransform.position;
                // Don't do this, because it moves the custom targets...
                Vector3 armDirection = handTarget.hand.target.transform.position - handTarget.upperArm.bone.transform.position;
                float distance = armDirection.magnitude;
                if (distance > armLength + 0.0F) {
                    handTarget.stretchlessTarget.position = handTarget.upperArm.bone.transform.position + armDirection.normalized * armLength;
                    return;
                }
            }
            handTarget.stretchlessTarget.localPosition = Vector3.zero;
        }
        #endregion

        #region Shoulder
        private Quaternion lastLocalShoulderRotation = Quaternion.identity;

        private static Quaternion NaturalShoulderOrientation(HandTarget handTarget, ref Quaternion lastLocalShoulderRotation) {
            if (handTarget.shoulder.target.confidence.rotation > 0.5F)
                return handTarget.shoulder.target.transform.rotation;

            HipsTarget hipsTarget = handTarget.humanoid.hipsTarget;

            Quaternion torsoRotation;
            if (hipsTarget.chest.bone.transform != null)
                torsoRotation = hipsTarget.chest.bone.targetRotation;
            else
                torsoRotation = Quaternion.LookRotation(hipsTarget.hips.bone.targetRotation * Vector3.forward, handTarget.humanoid.up);

            Vector3 upperArmBasePosition = handTarget.shoulder.bone.transform.position + torsoRotation * handTarget.shoulder.target.toBoneRotation * handTarget.upperArm.bone.transform.localPosition;

            float upperArm2HandDistance = Vector3.Distance(upperArmBasePosition, handTarget.hand.target.transform.position);
            float armLength = handTarget.upperArm.bone.length + handTarget.forearm.bone.length;

            float distanceToTarget = upperArm2HandDistance - armLength - 0.03F;

            Quaternion shoulderRotation = handTarget.shoulder.bone.targetRotation;
            if (distanceToTarget > 0) {
                // we need to use the shoulder now to get closer to the target
                Vector3 targetDirection = handTarget.hand.target.transform.position - handTarget.shoulder.bone.transform.position;
                Quaternion toTargetRotation = Quaternion.LookRotation(targetDirection) * Quaternion.AngleAxis(handTarget.isLeft ? 90 : -90, Vector3.up);

                shoulderRotation = Quaternion.Slerp(torsoRotation * handTarget.shoulder.bone.baseRotation, toTargetRotation, distanceToTarget * 4);
            }
            else if (distanceToTarget < 0) {
                shoulderRotation = Quaternion.Slerp(shoulderRotation, torsoRotation * handTarget.shoulder.bone.baseRotation, -distanceToTarget * 10);
            }

            if (handTarget.shoulder.bone.jointLimitations)
                shoulderRotation = LimitAngle(handTarget.shoulder, ref lastLocalShoulderRotation, shoulderRotation);

            return shoulderRotation;
        }
        #endregion

        #region UpperArm
        private Quaternion NaturalUpperArmOrientation(HandTarget handTarget, Vector3 handPosition, Quaternion handOrientation, bool speedLimitation) {
            Quaternion oldUpperArmRotation = handTarget.upperArm.bone.targetRotation;
            Quaternion upperArmRotation = CalculateUpperArmRotation(handTarget, handPosition, handOrientation);
            if (speedLimitation)
                upperArmRotation = LimitRotationSpeed(oldUpperArmRotation, upperArmRotation);
            return upperArmRotation;
        }

        private static Quaternion CalculateUpperArmRotation(Vector3 upperArmPosition, Vector3 upperArmUp, Vector3 forearmPosition, bool isLeft) {
            Vector3 upperArmForward = forearmPosition - upperArmPosition;
            Quaternion upperArmRotation = Quaternion.LookRotation(upperArmForward, upperArmUp);

            if (isLeft)
                upperArmRotation *= Quaternion.Euler(0, 90, 0);
            else
                upperArmRotation *= Quaternion.Euler(0, -90, 0);

            return upperArmRotation;

        }

        private Quaternion CalculateUpperArmRotation(HandTarget handTarget, Vector3 handPosition, Quaternion handRotation) {
            Vector3 upperArmUp = GetElbowAxis(handTarget, handPosition, handRotation);
            return CalculateUpperArmRotation2(handTarget, upperArmUp, handPosition);
        }

        public static Quaternion UpperArmRotationIK(Vector3 upperArmPosition, Vector3 handPosition, Vector3 elbowAxis, float upperArmLength, float forearmLength, bool isLeft) {
            Vector3 upperArmDirection = handPosition - upperArmPosition;
            float upperArm2HandDistance = upperArmDirection.magnitude;
            float upperArmAngle = ConsineRule(upperArm2HandDistance, upperArmLength, forearmLength);
            if (isLeft)
                upperArmAngle = -upperArmAngle;

            Quaternion upperArmRotation = Quaternion.LookRotation(upperArmDirection, elbowAxis);

            upperArmRotation = Quaternion.AngleAxis(upperArmAngle, upperArmRotation * Vector3.up) * upperArmRotation;
            upperArmRotation *= Quaternion.Euler(0, isLeft ? 90 : -90, 0);

            return upperArmRotation;
        }

        // get upper arm up from target positions
        private Vector3 GetElbowAxis(HandTarget handTarget, Vector3 handPosition, Quaternion handRotation) {
            // Something is not right here when using Neuron only
            Vector3 upperArmUp;
            if (handTarget.forearm.target.confidence.rotation < 0.5F) {
                upperArmUp = CalculateElbowAxis(handTarget, handPosition, handRotation);
            }
            else {
                upperArmUp = handTarget.forearm.target.transform.up;
            }
            return upperArmUp;
        }

        public Vector3 CalculateElbowAxis(HandTarget handTarget, Vector3 handPosition, Quaternion handRotation) {
            HipsTarget hipsTarget = handTarget.humanoid.hipsTarget;
            Quaternion hipsOrientation = Quaternion.LookRotation(hipsTarget.hips.target.transform.forward, handTarget.humanoid.up);

            Vector3 elbowAxis = Quaternion.Inverse(hipsOrientation) * handRotation * Vector3.up;
            if (handTarget.isLeft)
                elbowAxis = (elbowAxis + (Vector3.left + Vector3.up).normalized) / 2;
            else
                elbowAxis = (elbowAxis + (Vector3.right + Vector3.up).normalized) / 2;

            float elbowAxisY = elbowAxis.y;
            float elbowAxisZ = elbowAxis.z;
            Vector3 dHandUpper = Quaternion.Inverse(hipsOrientation) * (handPosition - handTarget.upperArm.bone.transform.position);
            if (!handTarget.isLeft) {
                if (dHandUpper.x < 0) elbowAxisZ -= dHandUpper.x * 10;
            }
            else {
                if (dHandUpper.x > 0) elbowAxisZ += dHandUpper.x * 10;
            }
            if (dHandUpper.y > 0) elbowAxisY += dHandUpper.y * 10;

            elbowAxisY = Mathf.Clamp(elbowAxisY, 0.01F, 1);
            elbowAxisZ = Mathf.Clamp(elbowAxisZ, -0.1F, 0.1F);
            elbowAxis = hipsOrientation * new Vector3(elbowAxis.x, elbowAxisY, elbowAxisZ);

            return elbowAxis;
        }

        public static Vector3 CalculateUpperArmUp(Quaternion forearmRotation) {
            return forearmRotation * Vector3.up;
        }

        private Quaternion lastLocalUpperArmRotation = Quaternion.identity;

        public Quaternion CalculateUpperArmRotation2(HandTarget handTarget, Vector3 upperArmUp, Vector3 handPosition) {
            float upperArm2HandDistance = Vector3.Distance(handTarget.upperArm.bone.transform.position, handPosition);

            float upperArmAngle = ConsineRule(upperArm2HandDistance, handTarget.upperArm.bone.length, handTarget.forearm.bone.length);
            if (handTarget.isLeft)
                upperArmAngle = -upperArmAngle;

            Vector3 upperArmForward = handPosition - handTarget.upperArm.bone.transform.position;
            Quaternion upperArmRotation = Quaternion.LookRotation(upperArmForward, upperArmUp);

            upperArmRotation = Quaternion.AngleAxis(upperArmAngle, upperArmRotation * Vector3.up) * upperArmRotation;

            if (handTarget.isLeft)
                upperArmRotation *= Quaternion.Euler(0, 90, 0);
            else
                upperArmRotation *= Quaternion.Euler(0, -90, 0);

            if (handTarget.upperArm.bone.jointLimitations)
                upperArmRotation = LimitAngle(handTarget.upperArm, ref lastLocalUpperArmRotation, upperArmRotation);

            return upperArmRotation;
        }

        #endregion

        #region Forearm
        private static Quaternion NaturalForearmOrientation(HandTarget handTarget, Quaternion upperArmRotation, Vector3 handPosition, bool speedLimitation) {
            Quaternion oldForearmRotation = handTarget.forearm.bone.transform.rotation * handTarget.forearm.bone.toTargetRotation;
            float forearmAngle = CalculateForearmAngle(handTarget, handTarget.upperArm.bone.transform.position, handPosition, handTarget.upperArm.bone.length, handTarget.forearm.bone.length);

            Quaternion localForearmOrientation = Quaternion.AngleAxis(forearmAngle, Vector3.up);

            Quaternion forearmOrientation = upperArmRotation * localForearmOrientation;

            if (speedLimitation)
                forearmOrientation = LimitRotationSpeed(oldForearmRotation, forearmOrientation);
            return forearmOrientation;
        }

        public static Quaternion ForearmRotationIK(Vector3 forearmPosition, Vector3 handPosition, Quaternion handRotation, Quaternion upperArmRotation, bool isLeft) {
            Vector3 elbowAxis = (handRotation * Vector3.up/* + upperArmRotation * Vector3.up) / 2;*/);
            return ForearmRotationIK(forearmPosition, handPosition, elbowAxis, isLeft);
        }

        public static Quaternion ForearmRotationIK(Vector3 forearmPosition, Vector3 handPosition, Vector3 elbowAxis, bool isLeft) {
            Quaternion forearmRotation = ArmBoneRotationIK(forearmPosition, handPosition, elbowAxis, isLeft);
            //Vector3 forearmDirection = handPosition - forearmPosition;
            //Quaternion forearmRotation = Quaternion.LookRotation(forearmDirection, elbowAxis);

            //if (isLeft)
            //    forearmRotation *= Quaternion.Euler(0, 90, 0);
            //else
            //    forearmRotation *= Quaternion.Euler(0, -90, 0);

            return forearmRotation;
        }

        public static float CalculateForearmAngle(HandTarget handTarget, Vector3 upperArmPosition, Vector3 handPosition, float upperArmLength, float forearmLength) {
            float upperArmLength2 = upperArmLength * upperArmLength;
            float forearmLength2 = forearmLength * forearmLength;
            float upperArm2HandDistance = Vector3.Distance(upperArmPosition, handPosition);
            float upperArm2HandDistance2 = upperArm2HandDistance * upperArm2HandDistance;

            float forearmAngle = Mathf.Acos((upperArmLength2 + forearmLength2 - upperArm2HandDistance2) / (2 * upperArmLength * forearmLength)) * Mathf.Rad2Deg;
            if (float.IsNaN(forearmAngle))
                forearmAngle = 180;

            if (handTarget.isLeft)
                forearmAngle = 180 - forearmAngle;
            else
                forearmAngle = forearmAngle - 180;

            if (handTarget.forearm.bone.jointLimitations) {
                if (handTarget.isLeft)
                    forearmAngle = Mathf.Clamp(forearmAngle, 0, handTarget.forearm.bone.maxAngle);
                else
                    forearmAngle = Mathf.Clamp(forearmAngle, -handTarget.forearm.bone.maxAngle, 0);
            }

            return forearmAngle;
        }

        //public float CalculateForearmAngle(HandTarget handTarget, Quaternion upperArmOrientation) {
        //    Quaternion localForearmOrienation = Quaternion.Inverse(upperArmOrientation) * handTarget.forearm.target.transform.rotation;
        //    Vector3 forearmAngles = localForearmOrienation.eulerAngles;
        //    //if (handTarget.jointLimitations)
        //    //    forearmAngles = LimitAngles(handTarget.forearm, forearmAngles);

        //    return forearmAngles.y;
        //}
        #endregion

        #region Hand
        private Quaternion lastLocalHandRotation = Quaternion.identity;

        private Quaternion NaturalHandOrientation(HandTarget handTarget, Quaternion forearmOrientation) {
            Quaternion handOrientation;

            if (handTarget.hand.target.confidence.position > 0.5F && handTarget.hand.target.confidence.rotation < 0.5F)
                handOrientation = CalculateHandOrientation(handTarget);
            else
                handOrientation = handTarget.hand.target.transform.rotation;

            if (handTarget.hand.bone.jointLimitations)
                handOrientation = LimitAngle(handTarget.hand, ref lastLocalHandRotation, handOrientation);
            //handOrientation = HandRotationLimitations(handTarget, handOrientation);
            return handOrientation;
        }

        private Quaternion CalculateHandOrientation(HandTarget handTarget) {
            Vector3 forwarmForward = handTarget.hand.target.transform.position - handTarget.forearm.target.transform.position;
            Vector3 forearmUp = handTarget.forearm.bone.targetRotation * Vector3.up;
            Quaternion handOrientation = Quaternion.LookRotation(forwarmForward, forearmUp);

            handOrientation *= Quaternion.AngleAxis(handTarget.isLeft ? 90 : -90, Vector3.up);
            return handOrientation;

        }

        private static Vector3 NaturalHandPosition(HandTarget handTarget) {
            Vector3 handPosition = GetHandPosition(handTarget);
            handPosition = HandLimitations(handTarget, handPosition);
            return handPosition;
        }
        private static Vector3 GetHandPosition(HandTarget handTarget) {
            if (Application.isPlaying && (handTarget.humanoid.physics && (handTarget.handRigidbody == null || !handTarget.handRigidbody.isKinematic) || handTarget.stretchlessTarget == null)) {
                return handTarget.hand.bone.transform.position;
            }
            else if (handTarget.stretchlessTarget != null) {
                return handTarget.stretchlessTarget.position;
            }
            else {
                return handTarget.hand.target.transform.position;
            }
        }

        private static void PlaceHandOnForearm(HandTarget handTarget, Quaternion forearmRotation) {
            handTarget.hand.bone.transform.position = handTarget.forearm.bone.transform.position + forearmRotation * handTarget.outward * handTarget.forearm.bone.length;
        }
        #endregion

        #region Limitations
        private static Vector3 HandLimitations(HandTarget handTarget, Vector3 position) {
            handTarget.hand.bone.transform.position = position;
            if (handTarget.grabbedObject != null) {
                RigidbodyLimitations rbLimitations = handTarget.grabbedObject.GetComponent<RigidbodyLimitations>();
                if (rbLimitations != null) {
                    Vector3 rblCorrection = rbLimitations.GetCorrectionVector();
                    position += rblCorrection;
                }
            }
            return position;
        }

        private static Quaternion HandRotationLimitations(HandTarget handTarget, Quaternion rotation) {
            handTarget.hand.bone.transform.rotation = rotation;
            if (handTarget.grabbedObject != null) {
                RigidbodyLimitations rbLimitations = handTarget.grabbedObject.GetComponent<RigidbodyLimitations>();
                if (rbLimitations != null) {
                    //Quaternion rblRotation = rbLimitations.GetCorrectionRotation();
                    Quaternion rblRotation = rbLimitations.GetCorrectionAxisRotation(rbLimitations.limitAngleAxis);
                    rotation = rblRotation * handTarget.hand.bone.transform.rotation;
                }
            }
            return rotation;
        }
        #endregion

        private static Quaternion IK(Vector3 bone1position, float bone1length, Vector3 bone1up, float bone2length, Vector3 targetPosition, bool isLeft) {
            Vector3 bone1forward = targetPosition - bone1position;
            float distance2target = bone1forward.magnitude;

            float bone1angle = ConsineRule(distance2target, bone1length, bone2length);
            if (isLeft)
                bone1angle = -bone1angle;

            Quaternion bone1rotation = Quaternion.LookRotation(bone1forward, bone1up);
            bone1rotation = Quaternion.AngleAxis(bone1angle, bone1up) * bone1rotation;

            if (isLeft)
                bone1rotation *= Quaternion.Euler(0, 90, 0);
            else
                bone1rotation *= Quaternion.Euler(0, -90, 0);

            return bone1rotation;
        }

        private static float ConsineRule(float a, float b, float c) {
            float a2 = a * a;
            float b2 = b * b;
            float c2 = c * c;

            float angle = Mathf.Acos((a2 + b2 - c2) / (2 * a * b)) * Mathf.Rad2Deg;
            if (float.IsNaN(angle))
                angle = 0;
            return angle;
        }

        public static Quaternion CalculateBoneRotation(Vector3 bonePosition, Vector3 parentBonePosition) {
            Vector3 direction = bonePosition - parentBonePosition;
            if (direction.magnitude > 0) {
                return Quaternion.LookRotation(direction);
            }
            else
                return Quaternion.identity;
        }

        public static Quaternion CalculateBoneRotation(Vector3 bonePosition, Vector3 parentBonePosition, Vector3 upDirection) {
            Vector3 direction = bonePosition - parentBonePosition;
            if (direction.magnitude > 0) {
                return Quaternion.LookRotation(direction, upDirection);
            }
            else
                return Quaternion.identity;
        }

        public static Quaternion ArmBoneRotationIK(Vector3 bonePosition, Vector3 targetPosition, Vector3 upAxis, bool isLeft) {
            Vector3 boneDirection = targetPosition - bonePosition;
            Quaternion boneRotation = Quaternion.LookRotation(boneDirection, upAxis);

            boneRotation *= Quaternion.Euler(0, isLeft ? 90 : -90, 0);

            return boneRotation;
        }
    }
}