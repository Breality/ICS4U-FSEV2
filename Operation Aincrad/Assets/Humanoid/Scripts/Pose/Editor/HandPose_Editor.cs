/*
using UnityEngine;
using UnityEditor;
using Passer;

namespace Humanoid {
    public class HumanoidHandPose_Editor : Editor {
        //public static void OnDisable(HumanoidPoseMixer poseMixer, HumanoidControl humanoid) {
        //    poseMixer.Cleanup();
        //    UpdateHandPose(humanoid, poseMixer);
        //}

        //public static void HandPoseListInspector(HumanoidPoseMixer poseMixer, HumanoidControl humanoid) {
        //    for (int i = 0; i < poseMixer.mixedPoses.Length; i++) {
        //        MixedHumanoidPose pose = poseMixer.mixedPoses[i];
        //        HumanoidPose oldPose = pose.pose;
        //        HumanoidPose_Editor.HumanoidPoseInspector(poseMixer, i);
        //        if (pose.pose != oldPose) {
        //            if (pose.pose == null) {
        //                // We deleted a pose, let's undo it's effect
        //                pose.value = 0;
        //                pose.pose = oldPose;
        //                poseMixer.ShowPose(humanoid);
        //                pose.pose = null;
        //            }
        //        }
        //    }

        //    EditorGUILayout.BeginHorizontal();
        //    HumanoidPose addPose = (HumanoidPose)EditorGUILayout.ObjectField(null, typeof(HumanoidPose), false, GUILayout.Width(200));
        //    if (addPose != null) {
        //        MixedHumanoidPose newPose = poseMixer.Add();
        //        newPose.pose = addPose;
        //    }
        //    EditorGUILayout.EndHorizontal();
        //}


        //public static void UpdateHandPose(HumanoidControl humanoid, HumanoidPoseMixer poseMixer) {
        //    if (Application.isPlaying)
        //        return;

        //    poseMixer.ShowPose(humanoid);
        //}

        #region Scene
        static int boneIndex = -1;
        public static void UpdateScene(HumanoidControl humanoid, ITarget target, HumanoidPoseMixer poseMixer, ref HumanoidPose.Bone selectedBone) {
            MixedHumanoidPose currentPose = poseMixer.GetEditedPose();
            if (currentPose == null || !currentPose.isEdited) {
                Tools.hidden = false;
                return;
            }

            Tools.hidden = true;

            HumanoidTarget.TargetedBone[] bones = target.GetBones();
            int[] controlIds = new int[bones.Length];
            Tracking.Bone[] boneIds = new Tracking.Bone[bones.Length];

            for (int i = 0; i < bones.Length; i++) {
                if (bones[i] == null || bones[i].bone == null || bones[i].bone.transform == null)
                    continue;

                Handles.FreeMoveHandle(bones[i].bone.transform.position, bones[i].bone.transform.rotation, 0.002F, Vector3.zero, DotHandleCapSaveID);
                controlIds[i] = lastControlID;
                boneIds[i] = bones[i].boneId;
            }

            FindSelectedHandle(controlIds, boneIds, ref boneIndex);
            if (boneIndex == -1)
                return;

            HumanoidTarget.TargetedBone targetedBone = FindTargetedBone(bones, boneIds[boneIndex]);
            if (targetedBone == null)
                return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            Handles.Label(targetedBone.bone.transform.position + Vector3.up * 0.01F, targetedBone.name, style);
            Handles.color = Color.white;

            switch (Tools.current) {
                case Tool.Move: // we do not do move for normal bones (yet)
                case Tool.Rotate:
                    selectedBone = currentPose.pose.CheckBone(boneIds[boneIndex]);
                    Quaternion handleRotation = Handles.RotationHandle(targetedBone.target.transform.rotation, targetedBone.bone.transform.position);
                    targetedBone.target.transform.rotation = handleRotation;
                    selectedBone.setRotation = true;
                    selectedBone.SetReferenceLocal(humanoid);
                    selectedBone.UpdateRotation(humanoid);
                    break;
                case Tool.Scale:
                    //Handles.ScaleHandle(selectedBone.transform.localScale, selectedBone.transform.position, selectedBone.transform.rotation, HandleUtility.GetHandleSize(selectedBone.transform.position));
                    // need to all morphScale first...
                    break;
            }

            Handles.BeginGUI();
            ResetBoneButton(selectedBone, humanoid);
            Handles.EndGUI();

        }

        private static void ResetBoneButton(HumanoidPose.Bone selectedBone, HumanoidControl humanoid) {
            if (GUILayout.Button("Reset Bone", GUILayout.Width(100))) {
                Debug.Log("Got it to work.");
                selectedBone.setRotation = false;
                selectedBone.localRotation = Quaternion.identity;
                HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(selectedBone.boneRef.boneId);
                targetedBone.target.transform.localRotation = selectedBone.localRotation;
            }
        }

        private static HumanoidTarget.TargetedBone FindTargetedBone(HumanoidTarget.TargetedBone[] bones, Tracking.Bone boneId) {
            foreach (HumanoidTarget.TargetedBone bone in bones) {
                if (bone.boneId == boneId)
                    return bone;
            }
            return null;
        }

        static int lastControlID;
        public static void DotHandleCapSaveID(int controlID, Vector3 position, Quaternion rotation, float size, EventType et) {
            lastControlID = controlID;
            Handles.DotHandleCap(controlID, position, rotation, size, et);
        }

        private static void FindSelectedHandle(int[] controlIds, Tracking.Bone[] boneIds, ref int boneIndex) {
            for (int i = 0; i < controlIds.Length; i++) {
                if (controlIds[i] == GUIUtility.hotControl) {
                    boneIndex = i;
                    return;
                }
            }
            return;
        }
        #endregion
    }

}
*/