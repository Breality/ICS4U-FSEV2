using UnityEngine;
using Passer.Humanoid.Tracking;

namespace Passer {

    [System.Serializable]
    public class FingersTarget : ITarget {
        public HandTarget handTarget;

        #region SubTargets
        public TargetedFinger thumb = null;
        public TargetedFinger index = null;
        public TargetedFinger middle = null;
        public TargetedFinger ring = null;
        public TargetedFinger little = null;
        public TargetedFinger[] allFingers;

        public FingersTarget(HandTarget handTarget) {
            this.handTarget = handTarget;
            thumb = new TargetedFinger(this);
            index = new TargetedFinger(this);
            middle = new TargetedFinger(this);
            ring = new TargetedFinger(this);
            little = new TargetedFinger(this);

            allFingers = new TargetedFinger[] {
                thumb,
                index,
                middle,
                ring,
                little
            };
        }

        #region Finger
        [System.Serializable]
        public class TargetedFinger {
            [System.NonSerialized]
            public FingersTarget fingers;

            //public TargetedPhalanges metaCarpal;
            public TargetedPhalanges proximal;
            public TargetedPhalanges intermediate;
            public TargetedPhalanges distal;

            public TargetedFinger(FingersTarget fingers) {
                this.fingers = fingers;

                //metaCarpal = new TargetedPhalanges(this, proximal);
                proximal = new TargetedPhalanges(this, intermediate);
                intermediate = new TargetedPhalanges(this, distal);
                distal = new TargetedPhalanges(this, null);

                //proximal.parent = metaCarpal;
                proximal.parent = this.fingers.handTarget.hand;
                intermediate.parent = proximal;
                distal.parent = intermediate;
            }

            public void Init(FingersTarget _fingers) {
                fingers = _fingers;

                proximal.parent = fingers.handTarget.hand;

                //metaCarpal.nextBone = proximal;
                proximal.nextBone = intermediate;
                intermediate.nextBone = distal;
            }

            public void RetrieveBones(HumanoidControl humanoid, Bone firstBoneId) {
                proximal.boneId = firstBoneId;
                intermediate.boneId = firstBoneId + 1;
                distal.boneId = firstBoneId + 2;

                //metaCarpal.RetrieveBones(humanoid);
                proximal.RetrieveBones(humanoid);
                intermediate.RetrieveBones(humanoid);
                distal.RetrieveBones(humanoid);
            }

            public void MatchTargetToAvatar() {
                //metaCarpal.MatchTargetToAvatar();
                proximal.MatchTargetToAvatar();
                intermediate.MatchTargetToAvatar();
                distal.MatchTargetToAvatar();
            }

            public Vector3 right {
                get {
                    Quaternion intermediateRotation = Quaternion.FromToRotation(proximal.forward, intermediate.forward);
                    float angle;
                    Vector3 rotationAxis;
                    intermediateRotation.ToAngleAxis(out angle, out rotationAxis);
                    return rotationAxis;
                }
            }

            public void DoMeasurements() {
                //metaCarpal.DoMeasurements();
                proximal.DoMeasurements();
                intermediate.DoMeasurements();
                distal.DoMeasurements();
            }

            #region Curl
            private float _curl;
            public float curl {
                get { return _curl; }
                set { SetCurl(fingers, value); }
            }

            public float GetCurl(HandTarget handTarget) {
                if (proximal.target.transform == null)
                    _curl = 0;
                else if (proximal.boneId == Bone.LeftThumbProximal || proximal.boneId == Bone.RightThumbProximal) {                     
                    //float angle = handTarget.isLeft ? -intermediate.target.transform.localEulerAngles.y : intermediate.target.transform.localEulerAngles.y;
                    Quaternion rotation = Quaternion.Inverse(proximal.target.transform.rotation) * distal.target.transform.rotation;
                    float thumbAngle = handTarget.isLeft ? -rotation.eulerAngles.y : rotation.eulerAngles.z;
                    thumbAngle = UnityAngles.Normalize(thumbAngle) / 90;
                    _curl = Mathf.Clamp(thumbAngle, -0.5F, 1);
                }
                else {
                    float fingerAngle = handTarget.isLeft ? intermediate.target.transform.localEulerAngles.z : -intermediate.target.transform.localEulerAngles.z;
                    //Quaternion fingerRotation = Quaternion.Inverse(handTarget.hand.target.transform.rotation) * distal.target.transform.rotation;
                    //float fingerAngle = handTarget.isLeft ? fingerRotation.eulerAngles.z : -fingerRotation.eulerAngles.z;
                    fingerAngle = UnityAngles.Normalize(fingerAngle) / 90;
                    _curl = Mathf.Clamp(fingerAngle, -0.1F, 1);
                }
                return _curl;
            }

            private void SetCurl(FingersTarget fingers, float curlValue) {
                _curl = Mathf.Clamp(curlValue, -0.1F, 1);
                _curl = (_curl + 0.1F) / 1.1F;

                if (proximal.target.transform == null)
                    return;

                float angle = fingers.handTarget.isLeft ? _curl * 90 : _curl * -90;

                if (this == fingers.thumb) {
                    _curl = Mathf.Clamp(curlValue, -0.1F, 1);
                    _curl = (_curl - 0.05F) / 1.5F;
                    angle = fingers.handTarget.isLeft ? _curl * 90 : _curl * -90;

                    proximal.target.transform.localRotation = proximal.target.baseRotation * Quaternion.AngleAxis(-angle / 2, Vector3.up);
                    Quaternion curl = Quaternion.AngleAxis(-angle, Vector3.up);
                    intermediate.target.transform.localRotation = intermediate.target.baseRotation * curl;
                    distal.target.transform.localRotation = distal.target.baseRotation * curl;
                }
                else {
                    Quaternion curl = Quaternion.AngleAxis(angle, Vector3.forward);
                    proximal.target.transform.localRotation = proximal.target.baseRotation * curl;
                    intermediate.target.transform.localRotation = intermediate.target.baseRotation * curl;
                    distal.target.transform.localRotation = distal.target.baseRotation * curl;
                }
            }
            #endregion

            #region
            private float _swing = 0;
            public float swing {
                get { return _swing; }
                set { SetSwing(value); }
            }

            public float GetSwing() {
                if (fingers == null || proximal.target.transform == null)
                    _swing = 0;
                else if (this == fingers.thumb) {
                    float angle = proximal.target.transform.localEulerAngles.z;
                    _swing = UnityAngles.Normalize(angle) / 90;
                }
                else {
                    float angle = proximal.target.transform.localEulerAngles.y;
                    _swing = UnityAngles.Normalize(angle) / 90;
                }
                return _swing;
            }

            public void SetSwing(float swingValue) {
                _swing = swingValue;

                if (this == fingers.thumb) {
                    float angle = _swing * 90;
                    Vector3 proximalAngles = proximal.target.transform.localEulerAngles;
                    proximal.target.transform.localRotation = Quaternion.Euler(proximalAngles.x, proximalAngles.y, angle);
                }
                else {
                    float angle = _swing * 90;

                    Vector3 proximalAngles = proximal.target.transform.localEulerAngles;
                    proximal.target.transform.localRotation = Quaternion.Euler(proximalAngles.x, angle, proximalAngles.z);
                }
            }
            #endregion
        }

        [System.Serializable]
        public class TargetedPhalanges : HumanoidTarget.TargetedBone {
            [System.NonSerialized]
            TargetedFinger finger;
            public Quaternion baseRotation;
            public Quaternion localBaseRotation;

            public TargetedPhalanges(TargetedFinger finger, HumanoidTarget.TargetedBone nextBone)
                : base(nextBone) {

                this.finger = finger;
            }

            public Vector3 forward {
                get {
                    if (bone.transform == null || (nextBone != null && nextBone.bone.transform == null))
                        return Vector3.forward;

                    if (nextBone == null) { // this is true for distal
                        if (bone.transform.childCount == 1) {
                            Transform childBone = bone.transform.GetChild(0);
                            return (childBone.position - bone.transform.position).normalized;
                        }
                        else if (parent != null) {
                            return parent.DetermineRotation() * finger.fingers.handTarget.outward;
                        }
                        else
                            return finger.fingers.handTarget.isLeft ? Vector3.left : Vector3.right;
                        // This is not correct when the distal and intermediate are not aligned.
                    }
                    else
                        return nextBone.bone.transform.position - bone.transform.position;
                }
            }

            public override Quaternion DetermineRotation() {
                Vector3 up;

                if (finger.fingers == null)
                    return Quaternion.identity;

                Quaternion parentBaseRotation = (parent == null || parent.target.transform == null) ? Quaternion.identity : parent.DetermineRotation();

                Vector3 bendAxis = finger.right;
                float angle = finger.GetCurl(finger.fingers.handTarget);
                if (angle < 10) { // finger is hardly bent
                    if (parent == null || parent.target.transform == null)
                        up = finger.fingers.handTarget.hand.bone.targetRotation * Vector3.up;
                    else
                        up = parent.DetermineRotation() * Vector3.up;
                    bendAxis = Vector3.Cross(up, forward);
                }
                else {
                    up = Vector3.Cross(forward, bendAxis);
                }

                Quaternion lookRotation = Quaternion.LookRotation(forward, up);
                baseRotation = lookRotation * Quaternion.AngleAxis(finger.fingers.handTarget.isLeft ? 90 : -90, Vector3.up);
                //if (parent != null)
                //    target.baseRotation = Quaternion.Inverse(parent.target.transform.rotation) * baseRotation;
                //else
                //    target.baseRotation = Quaternion.Inverse(target.transform.parent.rotation) * baseRotation;



                localBaseRotation = Quaternion.Inverse(parentBaseRotation) * baseRotation;

                return baseRotation;
            }
        }
        #endregion
        #endregion

        #region Limitations
        // Default limitations
        public static readonly Vector3 minProximalAngles = new Vector3(-10, 0, 0);
        public static readonly Vector3 maxProximalAngles = new Vector3(45, 0, 0);

        public static readonly Vector3 minIntermediateAngles = new Vector3(0, 0, 0);
        public static readonly Vector3 maxIntermediateAngles = new Vector3(90, 0, 0);

        public static readonly Vector3 minDistalAngles = new Vector3(0, 0, 0);
        public static readonly Vector3 maxDistalAngles = new Vector3(90, 0, 0);
        #endregion

        #region Configuration
        public void RetrieveBones(HandTarget handTarget) {
            if (handTarget == null)
                return;

            thumb.RetrieveBones(handTarget.humanoid, handTarget.isLeft ? Bone.LeftThumbProximal : Bone.RightThumbProximal);
            index.RetrieveBones(handTarget.humanoid, handTarget.isLeft ? Bone.LeftIndexProximal : Bone.RightIndexProximal);
            middle.RetrieveBones(handTarget.humanoid, handTarget.isLeft ? Bone.LeftMiddleProximal : Bone.RightMiddleProximal);
            ring.RetrieveBones(handTarget.humanoid, handTarget.isLeft ? Bone.LeftRingProximal : Bone.RightRingProximal);
            little.RetrieveBones(handTarget.humanoid, handTarget.isLeft ? Bone.LeftLittleProximal : Bone.RightLittleProximal);
        }

        private static readonly string[] boneNames = {
            "Thumb Proximal",
            "Thumb Intermediate",
            "Thumb Distal",

            "Index Finger Proximal",
            "Index Finger Intermediate",
            "Index Finger Distal",

            "Middle Finger Proximal",
            "Middle Finger Intermediate",
            "Middle Finger Distal",

            "Ring Finger Proximal",
            "Ring Finger Intermediate",
            "Ring Finger Distal",

            "Little Finger Proximal",
            "Little Finger Intermediate",
            "Little Finger Distal",
        };

        HumanoidTarget.TargetedBone[] ITarget.GetBones() {
            HumanoidTarget.TargetedBone[] bones = new HumanoidTarget.TargetedBone[] {
                thumb.proximal,
                thumb.intermediate,
                thumb.distal,

                index.proximal,
                index.intermediate,
                index.distal,

                middle.proximal,
                middle.intermediate,
                middle.distal,

                ring.proximal,
                ring.intermediate,
                ring.distal,

                little.proximal,
                little.intermediate,
                little.distal,
            };

            for (int i = 0; i < bones.Length; i++)
                bones[i].name = boneNames[i];

            return bones;
        }
        public SkinnedMeshRenderer blendshapeRenderer {
            get { return null; }
        }
        string[] ITarget.GetBlendshapeNames() {
            return null;
        }
        int ITarget.FindBlendshape(string namepart) {
            return -1;
        }

        public void SetBlendshapeWeight(string blendshape, float weight) { }
        public float GetBlendshapeWeight(string blendshape) { return 0; }
        #endregion

        #region Init
        public void InitAvatar() {
            thumb.Init(this);
            index.Init(this);
            middle.Init(this);
            ring.Init(this);
            little.Init(this);

            thumb.DoMeasurements();
            index.DoMeasurements();
            middle.DoMeasurements();
            ring.DoMeasurements();
            little.DoMeasurements();
        }

        public void NewComponent(HandTarget _handTarget) {
            handTarget = _handTarget;
            index.fingers = handTarget.fingers;
        }

        public void MatchTargetsToAvatar() {
            thumb.MatchTargetToAvatar();
            index.MatchTargetToAvatar();
            middle.MatchTargetToAvatar();
            ring.MatchTargetToAvatar();
            little.MatchTargetToAvatar();
        }
        #endregion

        #region Update
        public static void CopyFingerTargetsToRig(HandTarget handTarget) {
            if (handTarget.grabbedObject != null && handTarget.grabbedHandle != null) {
                handTarget.grabbedHandle.SetHandPose(handTarget);
            }
        }

        public static void CopyRigToFingerTargets(HandTarget handTarget) {
            handTarget.fingers.thumb.GetCurl(handTarget);
            handTarget.fingers.thumb.GetSwing();
            handTarget.fingers.index.GetCurl(handTarget);
            handTarget.fingers.index.GetSwing();
            handTarget.fingers.middle.GetCurl(handTarget);
            handTarget.fingers.middle.GetSwing();
            handTarget.fingers.ring.GetCurl(handTarget);
            handTarget.fingers.ring.GetSwing();
            handTarget.fingers.little.GetCurl(handTarget);
            handTarget.fingers.little.GetSwing();
        }
        #endregion

        public float GetFingerCurl(Finger fingerID) {
            return allFingers[(int)fingerID].GetCurl(this.handTarget);//curl;
        }

        public void AddFingerCurl(Finger fingerID, float curlValue) {
            allFingers[(int)fingerID].curl += curlValue;
        }

        public void SetFingerCurl(Finger fingerID, float curlValue) {
            allFingers[(int)fingerID].curl = curlValue;
        }

        public enum FingerGroup {
            AllButIndex,
            FingersButIndex,
            AllFingers
        }
        public void SetFingerGroupCurl(FingerGroup fingerGroupID, float curlValue) {
            switch (fingerGroupID) {
                case FingerGroup.AllButIndex:
                    SetFingerCurl(Finger.Thumb, curlValue);
                    SetFingerCurl(Finger.Middle, curlValue);
                    SetFingerCurl(Finger.Ring, curlValue);
                    SetFingerCurl(Finger.Little, curlValue);
                    break;
                case FingerGroup.FingersButIndex:
                    SetFingerCurl(Finger.Middle, curlValue);
                    SetFingerCurl(Finger.Ring, curlValue);
                    SetFingerCurl(Finger.Little, curlValue);
                    break;
                case FingerGroup.AllFingers:
                    SetFingerCurl(Finger.Index, curlValue);
                    SetFingerCurl(Finger.Middle, curlValue);
                    SetFingerCurl(Finger.Ring, curlValue);
                    SetFingerCurl(Finger.Little, curlValue);
                    break;
            }
        }

        public void DetermineFingerCurl(Finger fingerID) {
            allFingers[(int)fingerID].GetCurl(this.handTarget);
        }

        public static Transform GetFingerBone(HandTarget handTarget, Finger finger, FingerBones bone) {
            return GetFingerBone(handTarget.humanoid.avatarRig, handTarget.isLeft, finger, bone);
        }

        public static Transform GetFingerBone(Animator rig, bool isLeft, Finger finger, FingerBones bone) {
            if (rig == null)
                return null;

            int offset = 0;
            if (isLeft)
                offset = 24;
            else
                offset = 39;

            offset += (int)finger * 3 + (int)bone;

            return rig.GetBoneTransform((HumanBodyBones)offset);
        }

        #region DrawRigs
        public void DrawTargetRig(HandTarget handTarget) {
            DrawFingerTargetRig(handTarget, thumb);
            DrawFingerTargetRig(handTarget, index);
            DrawFingerTargetRig(handTarget, middle);
            DrawFingerTargetRig(handTarget, ring);
            DrawFingerTargetRig(handTarget, little);
        }

        private void DrawFingerTargetRig(HandTarget handTarget, TargetedFinger finger) {
            if (handTarget == null)
                return;

            if (finger.proximal.target != null)
                HumanoidTarget.DrawTarget(finger.proximal.target.confidence, finger.proximal.target.transform, handTarget.outward, 0.03F);
            if (finger.intermediate.target != null)
                HumanoidTarget.DrawTarget(finger.intermediate.target.confidence, finger.intermediate.target.transform, handTarget.outward, 0.02F);
            if (finger.distal.target != null)
                HumanoidTarget.DrawTarget(finger.distal.target.confidence, finger.distal.target.transform, handTarget.outward, 0.02F);
        }

        public void DrawAvatarRig(HandTarget handTarget) {
            DrawFingerAvatarRig(handTarget, thumb);
            DrawFingerAvatarRig(handTarget, index);
            DrawFingerAvatarRig(handTarget, middle);
            DrawFingerAvatarRig(handTarget, ring);
            DrawFingerAvatarRig(handTarget, little);
        }

        private void DrawFingerAvatarRig(HandTarget handTarget, TargetedFinger finger) {
            if (finger.proximal.bone.transform != null)
                Debug.DrawRay(finger.proximal.bone.transform.position, finger.proximal.bone.targetRotation * handTarget.outward * finger.proximal.bone.length, Color.cyan);
            if (finger.intermediate.bone.transform != null)
                Debug.DrawRay(finger.intermediate.bone.transform.position, finger.intermediate.bone.targetRotation * handTarget.outward * finger.intermediate.bone.length, Color.cyan);
            if (finger.distal.bone.transform != null)
                Debug.DrawRay(finger.distal.bone.transform.position, finger.distal.bone.targetRotation * handTarget.outward * finger.distal.bone.length, Color.cyan);
        }
        #endregion
    }
}