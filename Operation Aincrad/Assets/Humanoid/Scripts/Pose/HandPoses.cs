using System.Collections.Generic;
using UnityEngine;
using Passer;

namespace Humanoid {

    //using PoseId = System.Int32;

    //[System.Serializable]
    //public class HandPoses : Poses {

        //public enum PoseId {
        //    Unknown,
        //    // Built-in poses
        //    Open,
        //    Pointing,
        //    Closed,
        //    Grabbing,
        //    ThumbsUp,
        //    // The following poses can be recognized, but not configured
        //    //One,
        //    //Two,
        //    //Three,
        //    //Four,
        //    //Five,
        //    //Gun,
        //    //Lasso,
        //    //MiddleFinger,

        //    //Diablo,
        //    // // Possible future poses:
        //    // ThumbsDown,
        //    // Peace,
        //    // Pinch,
        //};
        //private const int nBuiltinPoses = 6;// (int)PoseId.ThumbsUp + 1;

        //public class HandPoseConfiguration {
        //    public/* PoseId*/int pose;
        //    public float thumbCurl;
        //    public float thumbSwing;
        //    public float indexCurl;
        //    public float middleCurl;
        //    public float ringCurl;
        //    public float littleCurl;
        //    public bool withOrientation;
        //    public Vector3 orientation;

        //    public HandPoseConfiguration(PoseId _pose, float _thumbCurl, float _indexCurl, float _middleCurl, float _ringCurl, float _littleCurl) {
        //        pose = (int)_pose;
        //        thumbCurl = _thumbCurl;
        //        thumbSwing = 0;
        //        indexCurl = _indexCurl;
        //        middleCurl = _middleCurl;
        //        ringCurl = _ringCurl;
        //        littleCurl = _littleCurl;
        //        withOrientation = false;
        //    }

        //    public static HandPoseConfiguration[] configurations = {
        //        new HandPoseConfiguration(PoseId.Open, 0F, 0, 0, 0, 0),
        //        new HandPoseConfiguration(PoseId.Pointing, 0F, 0.1F, 1F, 1F, 1F),
        //        new HandPoseConfiguration(PoseId.Closed, -0.2F, 1, 1, 1, 1),
        //        new HandPoseConfiguration(PoseId.Grabbing, -0.3F, -0.1F, -0.1F, -0.1F, -0.1F),
        //        new HandPoseConfiguration(PoseId.ThumbsUp, -0.3F, 1F, 1F, 1F, 1F),
        //        //new HandPoseConfiguration(PoseId.Gun, -0.5F, 0, 1F, 1F, 1F),
        //        //new HandPoseConfiguration(PoseId.Lasso, 0.4F, 0F, 0F, 1, 1),
        //        //new HandPoseConfiguration(PoseId.MiddleFinger, 0.2F, 1, 0, 1, 1),
        //        //new HandPoseConfiguration(PoseId.Diablo, 0F, 0F, 1, 1, 0F),
        //        //new HandPoseConfiguration(PoseId.One, 0, 0, 1, 1, 1),
        //        //new HandPoseConfiguration(PoseId.Two, 0, 0, 0, 0.8F, 0.8F),
        //        //new HandPoseConfiguration(PoseId.Three, 0, 0, 0, 0, 0.6F),
        //        //new HandPoseConfiguration(PoseId.Four, 0, 0, 0, 0, 0),
        //        //new HandPoseConfiguration(PoseId.Five, -0.5F, 0, 0, 0, 0),
        //    };

        //    public static HandPoseConfiguration GetConfiguration(PoseId _pose) {
        //        for (int i = 0; i < configurations.Length; i++) {
        //            if (configurations[i].pose == (int)_pose)
        //                return configurations[i];
        //        }
        //        return null;
        //    }
        //}

        #region Pose Recognition
        //public static int DetermineHandPose(FingersTarget fingers, out float poseConfidence) {
        //    //poseConfidence = 0;
        //    //return 0;
        //    int bestHandPose = -1;
        //    float bestHandPoseConfidence = 0;
        //    for (int i = 0; i < HandPoseConfiguration.configurations.Length; i++) {
        //        float confidence = GetHandPoseConfidence(fingers, HandPoseConfiguration.configurations[i]);
        //        if (confidence > bestHandPoseConfidence) {
        //            bestHandPose = i;
        //            bestHandPoseConfidence = confidence;
        //        }
        //    }

        //    if (bestHandPoseConfidence > 0.4F) {
        //        poseConfidence = bestHandPoseConfidence;
        //        return HandPoseConfiguration.configurations[bestHandPose].pose;
        //    } else {
        //        poseConfidence = 0;
        //        return (int)PoseId.Unknown;
        //    }
        //}

        //private static float GetHandPoseConfidence(FingersTarget fingers, PoseId handPose) {
        //    HandPoseConfiguration configuration = HandPoseConfiguration.GetConfiguration(handPose);
        //    return GetHandPoseConfidence(fingers, configuration);
        //}

        //private static float GetHandPoseConfidence(FingersTarget fingers, HandPoseConfiguration configuration) {
        //    float thumbCurlScore = GetFingerScore(fingers.thumb.curl, configuration.thumbCurl);
        //    float indexCurlScore = GetFingerScore(fingers.index.GetCurl(fingers), configuration.indexCurl);
        //    float middleCurlScore = GetFingerScore(fingers.middle.curl, configuration.middleCurl);
        //    float ringCurlScore = GetFingerScore(fingers.ring.curl, configuration.ringCurl);
        //    float littleCurlScore = GetFingerScore(fingers.little.curl, configuration.littleCurl);

        //    return 1 - (thumbCurlScore + indexCurlScore + middleCurlScore + ringCurlScore + littleCurlScore);
        //}

        //private static float GetFingerScore(float curl, float targetCurl) {
        //    float score = Mathf.Abs(curl - targetCurl);
        //    score = score * score;
        //    return score;
        //}
        #endregion

        //public HandPose[] poses;
        //public HandPose rest = new HandPose(null, "_rest", null, false);
        //public HandPose open = new HandPose("Open", null, false);
        //public HandPose pointing = new HandPose("Pointing", DefaultPointing, false);
        //public HandPose closed = new HandPose("Closed", DefaultClosed, false);
        //public HandPose grabbing = new HandPose("Grabbing", DefaultGrabbing, false);
        //public HandPose thumbsUp = new HandPose("Thumbs Up", DefaultThumbsUp, false);
        //public List<HandPose> customPoses = new List<HandPose>();
        //public HumanoidPoseMixer poseMixer = new HumanoidPoseMixer();

        //public FingersTarget fingers;

        //public void Update() {
        //    rest.ShowPose(1);
        //    foreach (HandPose pose in poses)
        //        pose.ShowPose();
        //}

        //public string[] GetNames() {
        //    string[] handPoseNames = new string[poses.Length];
        //    for (int i = 0; i < poses.Length; i++)
        //        handPoseNames[i] = poses[i].name;
        //    return handPoseNames;
        //}

        //public int GetId(string handPoseName) {
        //    for (int i = 0; i < poses.Length; i++)
        //        if (poses[i].name == handPoseName)
        //            return i;
        //    return -1;
        //}

        //public void Set(int handPoseId, float weight) {
        //    SetPoseValue(poses, (int)handPoseId, weight);
        //}

        //public float Get(PoseId handPoseId) {
        //    return poses[(int)handPoseId].value;
        //}

        //#region Init
        //public void InitPoses(FingersTarget fingers) {
        //    bool wasInitialized = poses != null;

        //    if (poses == null || poses.Length != nBuiltinPoses + customPoses.Count)
        //        poses = new HandPose[nBuiltinPoses + customPoses.Count];

        //    poses[0/*(int)PoseId.Unknown*/] = new HandPose(null, "_unknown", null, false);
        //    poses[1/*(int)PoseId.Open*/] = open;
        //    poses[2/*(int)PoseId.Pointing*/] = pointing;
        //    poses[3/*(int)PoseId.Closed*/] = closed;
        //    poses[4/*(int)PoseId.Grabbing*/] = grabbing;
        //    poses[5/*(int)PoseId.ThumbsUp*/] = thumbsUp;
        //    for (int i = 0; i < customPoses.Count; i++)
        //        poses[nBuiltinPoses + i] = customPoses[i];

        //    foreach (HandPose pose in poses) {
        //        pose.rest = rest;
        //        if (!wasInitialized) {
        //            pose.value = 0;
        //            pose.Reset(fingers);
        //        }
        //    }

        //    if (!isAnyPoseSet(poses)) {
        //        open.value = 1;
        //        open.isCurrent = true;
        //    }

        //    if (/*!Application.isPlaying &&*/ open.value == 1)
        //        CheckRestPose(rest, fingers);
        //}

        //private static void DefaultPointing(FingersTarget fingers, Pose pose) {
        //    pose.bones = new List<Pose.Bone>();

        //    float sign = fingers.handTarget.isLeft ? -1 : 1;

        //    AddBoneToPose(pose, fingers.thumb.proximal, Quaternion.Euler(0, sign * 15, sign * 5));
        //    AddBoneToPose(pose, fingers.thumb.intermediate, Quaternion.Euler(0, sign * 5, 0));

        //    AddBoneToPose(pose, fingers.middle.proximal, Quaternion.Euler(0, 0, sign * -70));
        //    AddBoneToPose(pose, fingers.middle.intermediate, Quaternion.Euler(0, 0, sign * -110));
        //    AddBoneToPose(pose, fingers.middle.distal, Quaternion.Euler(0, 0, sign * -75));

        //    AddBoneToPose(pose, fingers.ring.proximal, Quaternion.Euler(0, 0, sign * -75));
        //    AddBoneToPose(pose, fingers.ring.intermediate, Quaternion.Euler(0, 0, sign * -120));
        //    AddBoneToPose(pose, fingers.ring.distal, Quaternion.Euler(0, 0, sign * -65));

        //    AddBoneToPose(pose, fingers.little.proximal, Quaternion.Euler(0, 0, sign * -80));
        //    AddBoneToPose(pose, fingers.little.intermediate, Quaternion.Euler(0, 0, sign * -130));
        //    AddBoneToPose(pose, fingers.little.distal, Quaternion.Euler(0, 0, sign * -60));
        //}

        //private static void DefaultClosed(FingersTarget fingers, Pose pose) {
        //    pose.bones = new List<Pose.Bone>();

        //    float sign = fingers.handTarget.isLeft ? -1 : 1;

        //    AddBoneToPose(pose, fingers.thumb.proximal, Quaternion.Euler(0, 0, sign * 10));

        //    AddBoneToPose(pose, fingers.index.proximal, Quaternion.Euler(0, 0, sign * -70));
        //    AddBoneToPose(pose, fingers.index.intermediate, Quaternion.Euler(0, 0, sign * -110));
        //    AddBoneToPose(pose, fingers.index.distal, Quaternion.Euler(0, 0, sign * -75));

        //    AddBoneToPose(pose, fingers.middle.proximal, Quaternion.Euler(0, 0, sign * -70));
        //    AddBoneToPose(pose, fingers.middle.intermediate, Quaternion.Euler(0, 0, sign * -110));
        //    AddBoneToPose(pose, fingers.middle.distal, Quaternion.Euler(0, 0, sign * -75));

        //    AddBoneToPose(pose, fingers.ring.proximal, Quaternion.Euler(0, 0, sign * -75));
        //    AddBoneToPose(pose, fingers.ring.intermediate, Quaternion.Euler(0, 0, sign * -120));
        //    AddBoneToPose(pose, fingers.ring.distal, Quaternion.Euler(0, 0, sign * -65));

        //    AddBoneToPose(pose, fingers.little.proximal, Quaternion.Euler(0, 0, sign * -80));
        //    AddBoneToPose(pose, fingers.little.intermediate, Quaternion.Euler(0, 0, sign * -130));
        //    AddBoneToPose(pose, fingers.little.distal, Quaternion.Euler(0, 0, sign * -60));
        //}

        //private static void DefaultGrabbing(FingersTarget fingers, Pose pose) {
        //    pose.bones = new List<Pose.Bone>();

        //    float sign = fingers.handTarget.isLeft ? -1 : 1;

        //    AddBoneToPose(pose, fingers.thumb.proximal, Quaternion.Euler(0, sign * -10, 0));
        //    AddBoneToPose(pose, fingers.thumb.intermediate, Quaternion.Euler(0, sign * -30, 0));
        //    AddBoneToPose(pose, fingers.thumb.distal, Quaternion.Euler(0, sign * -40, 0));

        //    AddBoneToPose(pose, fingers.index.proximal, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.index.intermediate, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.index.distal, Quaternion.Euler(0, 0, sign * 10));

        //    AddBoneToPose(pose, fingers.middle.proximal, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.middle.intermediate, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.middle.distal, Quaternion.Euler(0, 0, sign * 10));

        //    AddBoneToPose(pose, fingers.ring.proximal, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.ring.intermediate, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.ring.distal, Quaternion.Euler(0, 0, sign * 10));

        //    AddBoneToPose(pose, fingers.little.proximal, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.little.intermediate, Quaternion.Euler(0, 0, sign * 10));
        //    AddBoneToPose(pose, fingers.little.distal, Quaternion.Euler(0, 0, sign * 10));
        //}

        //private static void DefaultThumbsUp(FingersTarget fingers, Pose pose) {
        //    pose.bones = new List<Pose.Bone>();

        //    float sign = fingers.handTarget.isLeft ? -1 : 1;

        //    AddBoneToPose(pose, fingers.thumb.proximal, Quaternion.Euler(0, sign * -10, 0));
        //    AddBoneToPose(pose, fingers.thumb.intermediate, Quaternion.Euler(0, sign * -30, 0));
        //    AddBoneToPose(pose, fingers.thumb.distal, Quaternion.Euler(0, sign * -40, 0));

        //    AddBoneToPose(pose, fingers.index.proximal, Quaternion.Euler(0, 0, sign * -70));
        //    AddBoneToPose(pose, fingers.index.intermediate, Quaternion.Euler(0, 0, sign * -110));
        //    AddBoneToPose(pose, fingers.index.distal, Quaternion.Euler(0, 0, sign * -75));

        //    AddBoneToPose(pose, fingers.middle.proximal, Quaternion.Euler(0, 0, sign * -70));
        //    AddBoneToPose(pose, fingers.middle.intermediate, Quaternion.Euler(0, 0, sign * -110));
        //    AddBoneToPose(pose, fingers.middle.distal, Quaternion.Euler(0, 0, sign * -75));

        //    AddBoneToPose(pose, fingers.ring.proximal, Quaternion.Euler(0, 0, sign * -75));
        //    AddBoneToPose(pose, fingers.ring.intermediate, Quaternion.Euler(0, 0, sign * -120));
        //    AddBoneToPose(pose, fingers.ring.distal, Quaternion.Euler(0, 0, sign * -65));

        //    AddBoneToPose(pose, fingers.little.proximal, Quaternion.Euler(0, 0, sign * -80));
        //    AddBoneToPose(pose, fingers.little.intermediate, Quaternion.Euler(0, 0, sign * -130));
        //    AddBoneToPose(pose, fingers.little.distal, Quaternion.Euler(0, 0, sign * -60));
        //}

        //private static void AddBoneToPose(Pose pose, HumanoidTarget.TargetedBone bone, Quaternion targetRotation) {
        //    if (bone == null)
        //        return;
        //    //Pose.Bone poseBone = pose.AddBone(bone.target.transform, bone.bone.transform, bone.target.toBoneRotation);
        //    Pose.Bone poseBone = pose.AddBone(bone);
        //    poseBone.targetRotation = targetRotation;
        //}
        //#endregion
    }

    //[System.Serializable]
    //public class HandPose : Pose {
    //    public HandPose(string _name, HandPoseReset _reset = null, bool __isCustom = true) : base(_name, null, __isCustom) {
    //        if (_reset == null)
    //            reset = DefaultReset;
    //        else
    //            reset = _reset;
    //    }

    //    public HandPose(Pose _rest, string _name, HandPoseReset _reset = null, bool __isCustom = true) : base(_rest, _name, null, __isCustom) {
    //        if (_reset == null)
    //            reset = DefaultReset;
    //        else
    //            reset = _reset;
    //    }

    //    public new void Reset(FingersTarget fingers) {
    //        reset(fingers, this);
    //    }
    //    public delegate void HandPoseReset(FingersTarget fingers, Pose pose);
    //    private HandPoseReset reset;
    //    private static void DefaultReset(FingersTarget fingers, Pose pose) { }
    //}
//}