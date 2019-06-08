using UnityEngine;

namespace Passer {

    public class FingerMovements {

        public static void Update(HandTarget handTarget) {
            if (handTarget == null || handTarget.hand.bone.transform == null)
                return;
            Quaternion handRotation = handTarget.hand.bone.targetRotation;

            UpdateFinger(handRotation, handTarget.fingers.thumb);
            UpdateFinger(handRotation, handTarget.fingers.index);
            UpdateFinger(handRotation, handTarget.fingers.middle);
            UpdateFinger(handRotation, handTarget.fingers.ring);
            UpdateFinger(handRotation, handTarget.fingers.little);
            //foreach (FingersTarget.TargetedFinger finger in handTarget.fingers.allFingers)
            //    UpdateFinger(handTarget.hand.bone.targetRotation, finger);
            //UpdateFinger(handTarget, finger);
        }

        //private static void UpdateFinger(HandTarget handTarget, FingersTarget.TargetedFinger finger) {
        //    UpdatePhalanx(finger.proximal, handTarget.hand.bone);
        //    UpdatePhalanx(finger.intermediate, finger.proximal.bone);
        //    UpdatePhalanx(finger.distal, finger.intermediate.bone);
        //}

        //private static void UpdatePhalanx(FingersTarget.TargetedPhalanges phalanx, HumanoidTarget.BoneTransform parentBone) {
        //    if (phalanx.bone.transform == null || phalanx.target.transform == null || parentBone.transform == null)
        //        return;

        //    Quaternion phalanxRotationOnParent = parentBone.targetRotation * phalanx.target.transform.localRotation;
        //    phalanx.bone.transform.rotation = phalanxRotationOnParent * phalanx.target.toBoneRotation;
        //}

        private static void UpdateFinger(Quaternion handRotation, FingersTarget.TargetedFinger finger) {
            Quaternion proximalRotation = CalculatePhalanxRotation(finger.proximal, handRotation);
            finger.proximal.SetBoneRotation(proximalRotation);

            Quaternion intermediateRotation = CalculatePhalanxRotation(finger.intermediate, proximalRotation);
            finger.intermediate.SetBoneRotation(intermediateRotation);

            Quaternion distalRotation = CalculatePhalanxRotation(finger.distal, intermediateRotation);
            finger.distal.SetBoneRotation(distalRotation);
        }

        private static Quaternion CalculatePhalanxRotation(FingersTarget.TargetedPhalanges phalanx, Quaternion parentRotation) {
            if (phalanx.target.transform == null)
                return Quaternion.identity;

            Quaternion phalanxRotationOnParent = parentRotation * phalanx.target.transform.localRotation;
            return phalanxRotationOnParent;
        }
    }
}