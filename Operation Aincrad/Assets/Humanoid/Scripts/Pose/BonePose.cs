using Passer.Humanoid.Tracking;
using Passer;
using UnityEngine;

namespace Passer.Humanoid {

    [System.Serializable]
    public class BonePose {

        public BoneType boneType = BoneType.AllBones;

        public BoneReference boneRef = new BoneReference();
        public BoneReference referenceBoneRef = new BoneReference();

        public bool setTranslation;
        public bool setRotation;
        public bool setScale;

        public Vector3 translation;
        public Quaternion rotation;
        public Vector3 scale;

        public void Reset(HumanoidControl humanoid) {
            rotation = Quaternion.identity;
            translation = Vector3.zero;

            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(boneRef.boneId);
            targetedBone.target.transform.localRotation = rotation;
            targetedBone.target.transform.localPosition = translation;
        }

        public void ShowPose(HumanoidControl humanoid, float value) {
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(boneRef.boneId);
            if (targetedBone == null || targetedBone.target.transform == null)
                return;

            Vector3 referencePosition = Vector3.zero;
            Quaternion referenceRotation = Quaternion.identity;
            Vector3 referenceScale = Vector3.zero;

            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(referenceBoneRef.boneId);
            if (referenceBoneRef.boneId != Bone.None && referenceBone.target.transform != null) {
                referencePosition = referenceBone.target.transform.position;
                referenceRotation = referenceBone.bone.targetRotation;
                referenceScale = referenceBone.target.transform.lossyScale;
            }
            else {
                referencePosition = humanoid.transform.position;
                referenceRotation = humanoid.transform.rotation;
                referenceScale = humanoid.transform.lossyScale;
            }

            if (setTranslation)
                targetedBone.target.transform.position = targetedBone.TargetBasePosition() + Vector3.Lerp(Vector3.zero, referenceRotation * translation, value);
            if (setRotation)
                targetedBone.target.transform.rotation = Quaternion.Slerp(targetedBone.TargetBaseRotation(), referenceRotation * rotation, value);
            if (setScale)
                targetedBone.target.transform.localScale = Vector3.Scale(referenceScale, Vector3.Lerp(Vector3.one, referenceRotation * scale, value));
        }

        public void ShowPose(HumanoidControl humanoid, Side showSide, float value) {
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(showSide, boneRef.sideBoneId);
            if (targetedBone == null || targetedBone.target.transform == null)
                return;

            Vector3 referencePosition = Vector3.zero;
            Quaternion referenceRotation = Quaternion.identity;
            Vector3 referenceScale = Vector3.zero;

            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(showSide, referenceBoneRef.sideBoneId);

            if (referenceBone != null && referenceBone.bone.transform != null) {
                referencePosition = referenceBone.target.transform.position;
                referenceRotation = referenceBone.bone.targetRotation;
                referenceScale = referenceBone.target.transform.lossyScale;
            }
            else {
                referencePosition = humanoid.transform.position;
                referenceRotation = humanoid.transform.rotation;
                referenceScale = humanoid.transform.lossyScale;
            }

            if (boneRef.side == Side.AnySide && showSide == Side.Right) {
                // We need to convert the left-orientated anyside to a right side
                // For this, we mirror the translation/rotation along the YZ-plane (not scale!)
                if (setTranslation) {
                    Vector3 mirroredTranslation = new Vector3(-translation.x, translation.y, translation.z);
                    targetedBone.target.transform.position = targetedBone.TargetBasePosition() + Vector3.Lerp(Vector3.zero, referenceRotation * mirroredTranslation, value);
                }
                if (setRotation) {
                    Quaternion mirroredRotation = new Quaternion(rotation.x, -rotation.y, -rotation.z, rotation.w);
                    targetedBone.target.transform.rotation = Quaternion.Slerp(targetedBone.TargetBaseRotation(), referenceRotation * mirroredRotation, value);
                }
                if (setScale)
                    targetedBone.target.transform.localScale = Vector3.Scale(referenceScale, Vector3.Lerp(Vector3.one, referenceRotation * scale, value));
            }
            else {
                if (setTranslation)
                    targetedBone.target.transform.position = targetedBone.TargetBasePosition() + Vector3.Lerp(Vector3.zero, referenceRotation * translation, value);
                if (setRotation)
                    targetedBone.target.transform.rotation = Quaternion.Slerp(targetedBone.TargetBaseRotation(), referenceRotation * rotation, value);
                if (setScale)
                    targetedBone.target.transform.localScale = Vector3.Scale(referenceScale, Vector3.Lerp(Vector3.one, referenceRotation * scale, value));
            }
        }

        public void ShowPoseAdditive(HumanoidControl humanoid, Side showSide, float value) {
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(showSide, boneRef.sideBoneId);
            if (targetedBone == null || targetedBone.target.transform == null)
                return;

            Vector3 referencePosition = Vector3.zero;
            Quaternion referenceRotation = Quaternion.identity;
            Vector3 referenceScale = Vector3.zero;

            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(showSide, referenceBoneRef.sideBoneId);

            if (referenceBone != null && referenceBone.bone.transform != null) {
                referencePosition = referenceBone.target.transform.position;
                referenceRotation = referenceBone.bone.targetRotation;
                referenceScale = referenceBone.target.transform.lossyScale;
            }
            else {
                referencePosition = humanoid.transform.position;
                referenceRotation = humanoid.transform.rotation;
                referenceScale = humanoid.transform.lossyScale;
            }

            if (boneRef.side == Side.AnySide && showSide == Side.Right) {
                // We need to convert the left-orientated anyside to a right side
                // For this, we mirror the translation/rotation along the YZ-plane (not scale!)
                if (setTranslation) {
                    Vector3 mirroredTranslation = new Vector3(-translation.x, translation.y, translation.z);
                    targetedBone.target.transform.position += Vector3.Lerp(Vector3.zero, referenceRotation * mirroredTranslation, value);
                }
                if (setRotation) {
                    Quaternion mirroredRotation = new Quaternion(rotation.x, -rotation.y, -rotation.z, rotation.w);
                    targetedBone.target.transform.rotation = Quaternion.Slerp(Quaternion.identity, referenceRotation * mirroredRotation, value);
                }
                if (setScale)
                    targetedBone.target.transform.localScale = Vector3.Scale(targetedBone.target.transform.localScale, Vector3.Lerp(Vector3.one, referenceRotation * scale, value));
            }
            else {
                if (boneRef.sideBoneId == SideBone.ThumbProximal) {
                    Debug.Log(targetedBone.target.transform.rotation.eulerAngles + " " + referenceRotation.eulerAngles + " " + rotation.eulerAngles + " " + value);
                }
                if (setTranslation)
                    targetedBone.target.transform.position += Vector3.Lerp(Vector3.zero, referenceRotation * translation, value);
                if (setRotation)
                    targetedBone.target.transform.rotation = Quaternion.Slerp(targetedBone.target.transform.rotation, referenceRotation * rotation, value);
                if (setScale)
                    targetedBone.target.transform.localScale = Vector3.Scale(targetedBone.target.transform.localScale, Vector3.Lerp(Vector3.one, referenceRotation * scale, value));
                if (boneRef.sideBoneId == SideBone.ThumbProximal) {
                    Debug.Log(targetedBone.target.transform.rotation.eulerAngles);
                }
            }
        }
        public void UpdateTranslation(HumanoidControl humanoid) {
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(boneRef.boneId);
            if (targetedBone.target.transform == null)
                return;

            setTranslation = true;
            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(referenceBoneRef.boneId);
            Vector3 targetBasePosition = targetedBone.TargetBasePosition();
            if (referenceBoneRef.boneId != Bone.None && referenceBone.target.transform != null) {
                translation = Quaternion.Inverse(referenceBone.bone.targetRotation) * (targetedBone.target.transform.position - targetBasePosition);
            } else {
                translation = Quaternion.Inverse(humanoid.transform.rotation) * (targetedBone.target.transform.position - targetBasePosition);
            }
            return;
        }

        public void UpdateRotation(HumanoidControl humanoid) {
            if (boneRef.boneId < 0 || boneRef.boneId > Bone.Count)
                return;

            setRotation = true;
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(boneRef.boneId);
            if (targetedBone.target.transform == null)
                return;

            if (referenceBoneRef.boneId == Bone.None) {
                rotation = Quaternion.Inverse(humanoid.transform.rotation) * targetedBone.target.transform.rotation;
                return;
            }

            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(referenceBoneRef.boneId);
            if (referenceBone.target.transform == null)
                rotation = Quaternion.Inverse(humanoid.transform.rotation) * targetedBone.target.transform.rotation;
            else
                rotation = Quaternion.Inverse(referenceBone.target.transform.rotation) * targetedBone.target.transform.rotation;
        }

        public void SetReferenceRoot() {
            //old_referenceBoneId = HumanBodyBones.LastBone;
            referenceBoneRef.boneId = Humanoid.Tracking.Bone.None;
        }

        public void SetReferenceLocal(HumanoidControl humanoid) {
            //if (old_boneId < 0 || (int)old_boneId > humanoid.targetedBones.Length)
            //    return;
            if (boneRef.boneId < 0 || boneRef.boneId > Tracking.Bone.Count)
                return;

            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(boneRef.boneId);
            if (targetedBone.parent == null)
                SetReferenceRoot();
            else
                //old_referenceBoneId = targetedBone.parent.boneId;
                referenceBoneRef.boneId = targetedBone.parent.boneId;
        }

        public void SetReference(Tracking.Bone referenceBoneId) {
            referenceBoneRef.boneId = referenceBoneId;
        }

        public float GetScore(HumanoidControl humanoid, Side side) {
            float score = 0;
            HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(side, boneRef.sideBoneId);

            Vector3 referencePosition = Vector3.zero;
            Quaternion referenceRotation = Quaternion.identity;
            Vector3 referenceScale = Vector3.zero;

            HumanoidTarget.TargetedBone referenceBone = humanoid.GetBone(side, referenceBoneRef.sideBoneId);//referenceBoneRef.boneId);
            if (referenceBone != null && referenceBone.bone.transform != null) {
                referencePosition = referenceBone.target.transform.position;
                referenceRotation = referenceBone.bone.targetRotation;
                referenceScale = referenceBone.target.transform.lossyScale;
            }
            else {
                referencePosition = humanoid.transform.position;
                referenceRotation = humanoid.transform.rotation;
                referenceScale = humanoid.transform.lossyScale;
            }

            if (setRotation) {
                float angle = Quaternion.Angle(targetedBone.bone.targetRotation, referenceRotation * rotation);
                score = Mathf.Clamp01((90 - angle) / 90);
            }
            return score;
        }
    }
}