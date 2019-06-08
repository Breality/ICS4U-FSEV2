using Passer;
using Passer.Humanoid.Tracking;
using System.Collections.Generic;
using UnityEngine;

namespace Passer.Humanoid {

    [System.Serializable]
    public class PoseMixer {
        public List<MixedPose> mixedPoses = new List<MixedPose>();
        public int currentPoseIx;
        public MixedPose currentPose;

        public void SetPoseValue(MixedPose newCurrentPose, float value = 1) {
            if (currentPose != newCurrentPose) {
                currentPose = newCurrentPose;

                foreach (MixedPose mixedPose in mixedPoses) {
                    if (mixedPose.value > 0 && mixedPose != currentPose)
                        mixedPose.previousValue = mixedPose.value;
                    else
                        mixedPose.previousValue = 0;
                }
            }

            if (currentPose != null) {
                currentPose.value = value;

                float invValue = 1 - value;
                foreach (MixedPose mixedPose in mixedPoses) {
                    if (mixedPose.previousValue != 0)
                        mixedPose.value = invValue * mixedPose.previousValue;
                }
            }
        }
        public void SetPoseValue(Pose newCurrentPose, float value = 1) {
            if (currentPose.pose != newCurrentPose) {
                foreach (MixedPose mixedPose in mixedPoses) {
                    if (mixedPose.pose == newCurrentPose)
                        currentPose = mixedPose;

                    if (mixedPose.value > 0 && mixedPose != currentPose)
                        mixedPose.previousValue = mixedPose.value;
                    else
                        mixedPose.previousValue = 0;
                }
            }
            if (currentPose.pose != newCurrentPose) {
                // pose was not found, so it was not in the mixedPoses yet,
                // so we need to add it
                currentPose = Add(newCurrentPose);
            }

            if (currentPose != null) {
                currentPose.value = value;

                float invValue = 1 - value;
                foreach (MixedPose mixedPose in mixedPoses) {
                    if (mixedPose.previousValue != 0)
                        mixedPose.value = invValue * mixedPose.previousValue;
                }
            }
        }

        public MixedPose Add() {
            MixedPose newMixedPose = new MixedPose() {
                pose = ScriptableObject.CreateInstance<Pose>()
            };
            mixedPoses.Add(newMixedPose);
            return newMixedPose;
        }

        public MixedPose Add(Pose _pose) {
            MixedPose foundMixedPose = mixedPoses.Find(mixedPose => mixedPose.pose == _pose);
            if (foundMixedPose != null)
                return foundMixedPose;
            MixedPose newMixedPose = new MixedPose() {
                pose = _pose
            };
            mixedPoses.Add(newMixedPose);
            return newMixedPose;
        }

        public MixedPose GetEditedPose() {
            foreach (MixedPose mixedPose in mixedPoses) {
                if (mixedPose.pose != null && mixedPose.isEdited)
                    return mixedPose;
            }
            return null;
        }

        public void ShowPose(HumanoidControl humanoid) {
            foreach (MixedPose mixedPose in mixedPoses) {
                if (mixedPose != null && mixedPose.pose != null)
                    mixedPose.pose.Show(humanoid, mixedPose.value);
            }
        }
        public void ShowPose(HumanoidControl humanoid, Side showSide) {
        //    foreach (MixedPose mixedPose in mixedPoses) {
        //        if (mixedPose != null && mixedPose.pose != null)
        //            mixedPose.pose.Show(humanoid, showSide, mixedPose.value);
        //    }
        //}
        //public void ShowMixedPose(HumanoidControl humanoid, Side showSide) {
            ResetAffectedBones(humanoid, showSide);
            foreach (MixedPose mixedPose in mixedPoses) {
                if (mixedPose != null && mixedPose.pose != null)
                    mixedPose.pose.ShowAdditive(humanoid, showSide, mixedPose.value);
            }
        }

        public void ResetAffectedBones(HumanoidControl humanoid, Side showSide) {
            foreach (MixedPose mixedPose in mixedPoses) {
                if (mixedPose != null && mixedPose.pose != null) {

                    foreach (BonePose bonePose in mixedPose.pose.bonePoses) {
                        HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(showSide, bonePose.boneRef.sideBoneId);
                        if (targetedBone == null || targetedBone.target.transform == null)
                            continue;

                        if (bonePose.setTranslation)
                            targetedBone.target.transform.position = targetedBone.TargetBasePosition();
                        if (bonePose.setRotation)
                            targetedBone.target.transform.rotation = targetedBone.TargetBaseRotation();
                        if (bonePose.setRotation)
                            targetedBone.target.transform.localScale = Vector3.one;
                    }

                }
            }
        }

        public void Cleanup() {
            mixedPoses.RemoveAll(mixedPose => mixedPose.pose == null);
        }

        public string[] GetPoseNames() {
            string[] poseNames = new string[mixedPoses.Count];//.Length];
            for (int i = 0; i < poseNames.Length; i++) {
                if (mixedPoses[i].pose == null)
                    poseNames[i] = "";
                else
                    poseNames[i] = mixedPoses[i].pose.name;
            }
            return poseNames;
        }
    }

    [System.Serializable]
    public class MixedPose {
        /// <summary>
        /// The pose itself
        /// </summary>
        public Pose pose;
        /// <summary>
        /// The current value of the pose
        /// </summary>
        public float value;
        /// <summary>
        /// The value of the pose when it became the previous pose
        /// </summary>
        public float previousValue;
        /// <summary>
        /// Pose is in editing mode
        /// </summary>
        public bool isEdited;
    }
}

