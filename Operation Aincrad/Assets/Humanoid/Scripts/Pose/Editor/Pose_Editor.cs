using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Passer;
using Passer.Humanoid.Tracking;

namespace Passer.Humanoid {

    [CustomEditor(typeof(Pose))]
    public class Pose_Editor : Editor {
        private Pose pose;

        #region Enable
        public void OnEnable() {
            pose = (Pose)target;
            if (pose.bonePoses == null)
                pose.bonePoses = new List<BonePose>();
        }
        #endregion

        #region Disable
        public void OnDisable() {
            pose.Cleanup();
        }
        #endregion

        private enum Reference {
            HumanoidLocal,
            BoneLocal
        }

        #region Inspector
        public override void OnInspectorGUI() {
            for (int i = 0; i < pose.bonePoses.Count; i++) {
                BonePose bonePoses = pose.bonePoses[i];
                if (bonePoses == null)
                    continue;

                if (bonePoses.boneRef.type == BoneType.SideBones && bonePoses.boneRef.side == Side.AnySide) {
                    EditorGUILayout.HelpBox("Configure AnySide like Left Side", MessageType.Info);
                }
                EditorGUILayout.BeginHorizontal();
                bonePoses.boneRef.type = (BoneType)EditorGUILayout.EnumPopup(bonePoses.boneRef.type, GUILayout.Width(159));
                Bone oldBoneId = bonePoses.boneRef.boneId;
                SideBone oldSideBoneId = bonePoses.boneRef.sideBoneId;
                BoneSelector(ref bonePoses.boneRef);
                EditorGUILayout.EndHorizontal();

                if (bonePoses.boneRef.boneId != oldBoneId || bonePoses.boneRef.sideBoneId != oldSideBoneId)
                    PresetReferenceBone(bonePoses.boneRef, ref bonePoses.referenceBoneRef);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reference", GUILayout.MaxWidth(65));
                bonePoses.referenceBoneRef.type = (BoneType)EditorGUILayout.EnumPopup(bonePoses.referenceBoneRef.type, GUILayout.Width(90));

                BoneSelector(ref bonePoses.referenceBoneRef); EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                bonePoses.setTranslation = EditorGUILayout.ToggleLeft("Translation", bonePoses.setTranslation, GUILayout.MaxWidth(131));
                if (bonePoses.setTranslation)
                    bonePoses.translation = EditorGUILayout.Vector3Field("", bonePoses.translation);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                bonePoses.setRotation = EditorGUILayout.ToggleLeft("Rotation", bonePoses.setRotation, GUILayout.MaxWidth(131));
                if (bonePoses.setRotation) {
                    Vector3 eulerAngles = EditorGUILayout.Vector3Field("", bonePoses.rotation.eulerAngles);
                    if (eulerAngles != bonePoses.rotation.eulerAngles)
                        bonePoses.rotation.eulerAngles = eulerAngles;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                bonePoses.setScale = EditorGUILayout.ToggleLeft("Scale", bonePoses.setScale, GUILayout.MaxWidth(131));
                if (bonePoses.setScale)
                    bonePoses.scale = EditorGUILayout.Vector3Field("", bonePoses.scale);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            Buttons();
            EditorUtility.SetDirty(pose);
        }

        private void PresetReferenceBone(BoneReference bone, ref BoneReference referenceBone) {
            if (referenceBone.boneId != Bone.None)
                return;

            if (bone.isFacialBone)
                referenceBone.centerBoneId = CenterBone.Head;
            if (bone.isHandBone) {
                referenceBone.sideBoneId = SideBone.Hand;
                referenceBone.type = BoneType.SideBones;
            }
        }

        private void BoneSelector(ref BoneReference bone) {
            switch (bone.type) {
                case BoneType.AllBones:
                    bone.boneId = (Bone)EditorGUILayout.EnumPopup(bone.boneId);
                    return;
                case BoneType.CenterBones:
                    CenterBone centerBone = (CenterBone)EditorGUILayout.EnumPopup(bone.centerBoneId);
                    if (centerBone != CenterBone.Unknown)
                        bone.centerBoneId = centerBone;
                    return;
                case BoneType.SideBones:
                    if (bone.boneId == Bone.None) {
                        SideBone sideBoneId = bone.sideBoneId;
                        bone.sideBoneId = (SideBone)EditorGUILayout.EnumPopup(sideBoneId);
                    }
                    else {
                        bone.sideBoneId = (SideBone)EditorGUILayout.EnumPopup(bone.sideBoneId);
                    }
                    bone.side = (Side)EditorGUILayout.EnumPopup(bone.side, GUILayout.Width(80));
                    return;
                case BoneType.FaceBones:
                    bone.faceBoneId = (FacialBone)EditorGUILayout.EnumPopup(bone.faceBoneId);
                    return;
                default:
                    return;
            }
        }

        #region Buttons
        private void Buttons() {
            EditorGUILayout.BeginHorizontal();
            AddBoneButton();
            ClearAllButton();
            EditorGUILayout.EndHorizontal();
        }
        private void AddBoneButton() {
            if (GUILayout.Button("Add Bone")) {
                //PoseBone newBone = new PoseBone();
                //PoseBone newBone = ScriptableObject.CreateInstance<PoseBone>();
                //pose.bones.Add(newBone);
                pose.AddBone(Bone.None);
            }
        }
        private void ClearAllButton() {
            if (GUILayout.Button("Clear All")) {
                pose.bonePoses = new List<BonePose>();
                //pose.boneArray = new BonePose[0];
            }

        }
        #endregion
        #endregion

        public static void PoseMixerInspector(PoseMixer poseMixer, HumanoidControl humanoid) {
            foreach (MixedPose pose in poseMixer.mixedPoses) {
                Pose oldPose = pose.pose;
                HumanoidPoseInspector(poseMixer, pose, humanoid);
                if (pose.pose != oldPose && pose.pose == null) {
                    // We deleted a pose, let's undo it's effect
                    pose.value = 0;
                    pose.pose = oldPose;
                    poseMixer.ShowPose(humanoid);
                    pose.pose = null;
                }
            }

            EditorGUILayout.BeginHorizontal();
            Pose addPose = (Pose)EditorGUILayout.ObjectField(null, typeof(Pose), false, GUILayout.Width(200));
            if (addPose != null) {
                MixedPose newPose = poseMixer.Add();
                newPose.pose = addPose;
            }
            EditorGUILayout.EndHorizontal();
        }

        public static MixedPose HumanoidPoseInspector(PoseMixer poseMixer, MixedPose mixedPose, HumanoidControl humanoid) {
            EditorGUILayout.BeginHorizontal();

            //int thisControlID = GUIUtility.GetControlID(FocusType.Passive) + 1;
            mixedPose.pose = (Pose)EditorGUILayout.ObjectField(mixedPose.pose, typeof(Pose), false, GUILayout.Width(200));
            if (mixedPose.pose != null) {
                if (mixedPose.isEdited) {
                    EditorGUILayout.Slider(mixedPose.value, 0, 1);
                    poseMixer.SetPoseValue(mixedPose, 1);
                }
                else {
                    float value = EditorGUILayout.Slider(mixedPose.value, 0, 1);
                    if (value != mixedPose.value) {
                        poseMixer.SetPoseValue(mixedPose, value);
                    }
                }
                if (!Application.isPlaying) {
                    bool isEdited = EditorGUILayout.Toggle(mixedPose.isEdited, "button", GUILayout.Width(19));
                    if (mixedPose.isEdited != isEdited)
                        SceneView.RepaintAll();
                    mixedPose.isEdited = isEdited;
                }
                else {
                    EditorGUILayout.FloatField(mixedPose.pose.GetScore(humanoid, Side.Left));
                }
            }

            EditorGUILayout.EndHorizontal();
            return mixedPose;
        }

        #region Scene
        static int boneIndex = -1;
        public static void UpdateScene(HumanoidControl humanoid, ITarget target, PoseMixer poseMixer, ref BonePose selectedBone) {
            //if (!Application.isPlaying)
            //    poseMixer.ShowPose(humanoid);

            MixedPose currentPose = poseMixer.GetEditedPose();
            if (currentPose == null || !currentPose.isEdited) {
                Tools.hidden = false;
                return;
            }

            Tools.hidden = true;

            HumanoidTarget.TargetedBone[] bones = target.GetBones();
            int[] controlIds = new int[bones.Length];
            Bone[] boneIds = new Bone[bones.Length];

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

        private static void ResetBoneButton(BonePose selectedBone, HumanoidControl humanoid) {
            if (GUILayout.Button("Reset Bone", GUILayout.Width(100))) {
                Debug.Log("Got it to work.");
                selectedBone.setRotation = false;
                selectedBone.rotation = Quaternion.identity;
                HumanoidTarget.TargetedBone targetedBone = humanoid.GetBone(selectedBone.boneRef.boneId);
                targetedBone.target.transform.localRotation = selectedBone.rotation;
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

        //[MenuItem("Assets/Create/Humanoid/Pose", false, 101)]
        //public static void CreateAsset() {
        //    CustomAssetUtility.CreateAsset<Pose>();
        //}
    }
}
