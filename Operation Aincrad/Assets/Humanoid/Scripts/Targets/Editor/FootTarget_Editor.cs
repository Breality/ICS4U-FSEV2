using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(FootTarget))]
    public class FootTarget_Editor : Editor {
        private FootTarget footTarget;
        private HumanoidControl humanoid;

        private TargetProps[] allProps;

        #region Enable
        public void OnEnable() {
            footTarget = (FootTarget)target;

            humanoid = GetHumanoid(footTarget);
            if (humanoid == null)
                return;

            InitEditors();

            footTarget.InitSensors();
            InitConfiguration(footTarget);
            InitSettings();
        }

        private void InitEditors() {
            allProps = new TargetProps[] {
#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                new ViveTracker_Editor.FootTargetProps(serializedObject, footTarget),
#endif
#if hKINECT1
                new Kinect1_Editor.FootTargetProps(serializedObject, footTarget),
#endif
#if hKINECT2
                new Kinect2_Editor.FootTargetProps(serializedObject, footTarget),
#endif
#if hORBBEC
                new Astra_Editor.FootTargetProps(serializedObject, footTarget),
#endif
#if hNEURON
                new Neuron_Editor.FootTargetProps(serializedObject, footTarget),
#endif
#if hOPTITRACK
                new Optitrack_Editor.FootTargetProps(serializedObject, footTarget),
#endif
            };
        }
        #endregion

        #region Disable
        public void OnDisable() {
            if (humanoid == null) {
                // This target is not connected to a humanoid, so we delete it
                DestroyImmediate(footTarget, true);
                return;
            }

            if (!Application.isPlaying) {
                SetSensor2Target();
            }
        }

        private void SetSensor2Target() {
            foreach (TargetProps props in allProps)
                props.SetSensor2Target();
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            if (footTarget == null || humanoid == null)
                return;

            serializedObject.Update();

            ControllerInspectors(footTarget);
            ConfigurationInspector(footTarget);
            SettingsInspector(footTarget);

            serializedObject.ApplyModifiedProperties();
            UpdateBones(footTarget);
        }

        public static FootTarget Inspector(FootTarget footTarget, string name) {
            if (footTarget == null)
                return footTarget;

            EditorGUILayout.BeginHorizontal();
            Transform defaultTargetTransform = footTarget.GetDefaultTarget(footTarget.humanoid);
            Transform targetTransform = footTarget.transform ?? defaultTargetTransform;

            GUIContent text = new GUIContent(
                name,
                "The transform controlling the " + name
                );
            targetTransform = (Transform)EditorGUILayout.ObjectField(text, targetTransform, typeof(Transform), true);

            if (!Application.isPlaying) {
                if (targetTransform == defaultTargetTransform && GUILayout.Button("Show", GUILayout.MaxWidth(60))) {
                    // Call static method CreateTarget on target
                    footTarget = (FootTarget)footTarget.GetType().GetMethod("CreateTarget").Invoke(null, new object[] { footTarget });
                } else if (targetTransform != footTarget.transform) {
                    footTarget = (FootTarget)footTarget.GetType().GetMethod("SetTarget").Invoke(null, new object[] { footTarget.humanoid, targetTransform, footTarget.isLeft });
                }
            }
            EditorGUILayout.EndHorizontal();
            return footTarget;
        }

        public static HumanoidControl GetHumanoid(HumanoidTarget target) {
            HumanoidControl[] humanoids = GameObject.FindObjectsOfType<HumanoidControl>();
            HumanoidControl foundHumanoid = null;

            for (int i = 0; i < humanoids.Length; i++)
                if (humanoids[i].leftFootTarget.transform == target.transform ||
                    humanoids[i].rightFootTarget.transform == target.transform)
                    foundHumanoid = humanoids[i];

            return foundHumanoid;
        }

        #region Sensors
        private static bool showControllers = true;
        private void ControllerInspectors(FootTarget footTarget) {
            showControllers = EditorGUILayout.Foldout(showControllers, "Controllers");
            if (showControllers) {
                EditorGUI.indentLevel++;

                foreach (TargetProps props in allProps)
                    props.Inspector();

                if (humanoid.animatorEnabled)
                    footTarget.animator.enabled = EditorGUILayout.ToggleLeft("Procedural Animation", footTarget.animator.enabled, GUILayout.MinWidth(80));
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Configuration
        private void InitConfiguration(FootTarget footTarget) {
            if (footTarget.humanoid.avatarRig == null)
                return;

            InitUpperLegConfiguration(footTarget.upperLeg);
            InitLowerLegConfiguration(footTarget.lowerLeg);
            InitFootConfiguration(footTarget.foot);
            InitToesConfiguration(footTarget.toes);
        }

        private static bool showConfiguration;
        private void ConfigurationInspector(FootTarget footTarget) {
            //if (!target.jointLimitations)
            //    return;

            footTarget.RetrieveBones();

            showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration", true);
            if (showConfiguration) {
                EditorGUI.indentLevel++;

                UpperLegConfigurationInspector(ref footTarget.upperLeg, footTarget.isLeft);
                LowerLegConfigurationInspector(ref footTarget.lowerLeg, footTarget.isLeft);
                FootConfigurationInspector(ref footTarget.foot, footTarget.isLeft);
                ToesConfigurationInspector(ref footTarget.toes, footTarget.isLeft);

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateBones(FootTarget target) {
            if (target.humanoid.avatarRig == null)
                return;

            UpdateUpperLegBones(target.upperLeg);
            UpdateLowerLegBones(target.lowerLeg);
            UpdateFootBones(target.foot);
            UpdateToesBones(target.toes);
        }

        #region UpperLeg
        //private string upperLegXname;
        //private SerializedProperty upperLegMinX;
        //private SerializedProperty upperLegMaxX;

        //private string upperLegYname;
        //private SerializedProperty upperLegMinY;
        //private SerializedProperty upperLegMaxY;

        //private string upperLegZname;
        //private SerializedProperty upperLegMinZ;
        //private SerializedProperty upperLegMaxZ;

        private void InitUpperLegConfiguration(FootTarget.TargetedUpperLegBone upperLeg) {
            //if (upperLeg.bone.transform == null)
            //    return;

            //upperLegXname = upperLeg.bone.transform.name + "X";
            //upperLegMinX = serializedObject.FindProperty("upperLeg.bone.minAngles.x");
            //upperLegMaxX = serializedObject.FindProperty("upperLeg.bone.maxAngles.x");

            //upperLegYname = upperLeg.bone.transform.name + "Y";
            //upperLegMinY = serializedObject.FindProperty("upperLeg.bone.minAngles.y");
            //upperLegMaxY = serializedObject.FindProperty("upperLeg.bone.maxAngles.y");

            //upperLegZname = upperLeg.bone.transform.name + "Z";
            //upperLegMinZ = serializedObject.FindProperty("upperLeg.bone.minAngles.z");
            //upperLegMaxZ = serializedObject.FindProperty("upperLeg.bone.maxAngles.z");
        }

        private void UpperLegConfigurationInspector(ref FootTarget.TargetedUpperLegBone upperLeg, bool isLeft) {
            if (upperLeg.bone.transform != null)
                GUI.SetNextControlName(upperLeg.bone.transform.name + "00");
            upperLeg.bone.transform = (Transform)EditorGUILayout.ObjectField("Upper Leg Bone", upperLeg.bone.transform, typeof(Transform), true);
            if (upperLeg.bone.transform != null) {
                EditorGUI.indentLevel++;

                upperLeg.bone.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", upperLeg.bone.jointLimitations);
                if (upperLeg.bone.jointLimitations) {
                    upperLeg.bone.maxAngle = EditorGUILayout.Slider("Max Angle", upperLeg.bone.maxAngle, 0, 180);
                }

                //EditorGUILayout.BeginHorizontal();
                //upperLeg.bone.maxAngle = EditorGUILayout.Slider("Max Angle", upperLeg.bone.maxAngle, 0, 180);
                //if (GUILayout.Button("R", GUILayout.Width(20))) {
                //    upperLeg.bone.maxAngle = FootTarget.maxUpperLegAngle;
                //}
                //EditorGUILayout.EndHorizontal();

                //if (isLeft) {
                //    Target_Editor.BoneAngleInspector(upperLegMinX, upperLegMaxX, FootTarget.minLeftUpperLegAngles.x, FootTarget.maxLeftUpperLegAngles.x, upperLegXname, "X Limits");
                //    Target_Editor.BoneAngleInspector(upperLegMinY, upperLegMaxY, FootTarget.minLeftUpperLegAngles.y, FootTarget.maxLeftUpperLegAngles.y, upperLegYname, "Y Limits");
                //    Target_Editor.BoneAngleInspector(upperLegMinZ, upperLegMaxZ, FootTarget.minLeftUpperLegAngles.z, FootTarget.maxLeftUpperLegAngles.z, upperLegZname, "Z Limits");
                //} else {
                //    Target_Editor.BoneAngleInspector(upperLegMinX, upperLegMaxX, FootTarget.minRightUpperLegAngles.x, FootTarget.maxRightUpperLegAngles.x, upperLegXname, "X Limits");
                //    Target_Editor.BoneAngleInspector(upperLegMinY, upperLegMaxY, FootTarget.minRightUpperLegAngles.y, FootTarget.maxRightUpperLegAngles.y, upperLegYname, "Y Limits");
                //    Target_Editor.BoneAngleInspector(upperLegMinZ, upperLegMaxZ, FootTarget.minRightUpperLegAngles.z, FootTarget.maxRightUpperLegAngles.z, upperLegZname, "Z Limits");
                //}
                EditorGUI.indentLevel--;
            }
        }

        private void UpdateUpperLegBones(FootTarget.TargetedUpperLegBone upperLeg) {
            //if (upperLeg.bone.transform == null)
            //    return;

            //upperLeg.bone.minAngles.x = upperLegMinX.floatValue;
            //upperLeg.bone.maxAngles.x = upperLegMaxX.floatValue;

            //upperLeg.bone.minAngles.y = upperLegMinY.floatValue;
            //upperLeg.bone.maxAngles.y = upperLegMaxY.floatValue;

            //upperLeg.bone.minAngles.z = upperLegMinZ.floatValue;
            //upperLeg.bone.maxAngles.z = upperLegMaxZ.floatValue;
        }

        #endregion

        #region LowerLeg
        //private string lowerLegYname;
        //private SerializedProperty lowerLegMinX;
        //private SerializedProperty lowerLegMaxX;

        private void InitLowerLegConfiguration(FootTarget.TargetedLowerLegBone lowerLeg) {
            //if (lowerLeg.bone.transform == null)
            //    return;

            //lowerLegYname = lowerLeg.bone.transform.name + "X";
            //lowerLegMinX = serializedObject.FindProperty("lowerLeg.bone.minAngles.x");
            //lowerLegMaxX = serializedObject.FindProperty("lowerLeg.bone.maxAngles.x");
        }

        private void LowerLegConfigurationInspector(ref FootTarget.TargetedLowerLegBone lowerLeg, bool isLeft) {
            if (lowerLeg.bone.transform != null)
                GUI.SetNextControlName(lowerLeg.bone.transform.name + "00");
            lowerLeg.bone.transform = (Transform)EditorGUILayout.ObjectField("Lower Leg Bone", lowerLeg.bone.transform, typeof(Transform), true);
            if (lowerLeg.bone.transform != null) {
                EditorGUI.indentLevel++;

                lowerLeg.bone.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", lowerLeg.bone.jointLimitations);
                if (lowerLeg.bone.jointLimitations) {
                    lowerLeg.bone.maxAngle = EditorGUILayout.Slider("Max Angle", lowerLeg.bone.maxAngle, 0, 180);
                }

                //EditorGUILayout.BeginHorizontal();
                //lowerLeg.bone.maxAngle = EditorGUILayout.Slider("Max Angle", lowerLeg.bone.maxAngle, 0, 180);
                //if (GUILayout.Button("R", GUILayout.Width(20))) {
                //    lowerLeg.bone.maxAngle = FootTarget.maxLowerLegAngle;
                //}
                //EditorGUILayout.EndHorizontal();

                //if (isLeft)
                //    Target_Editor.BoneAngleInspector(lowerLegMinX, lowerLegMaxX, FootTarget.minLeftLowerLegAngles.x, FootTarget.maxLeftLowerLegAngles.x, lowerLegYname, "X Limits");
                //else
                //    Target_Editor.BoneAngleInspector(lowerLegMinX, lowerLegMaxX, FootTarget.minRightLowerLegAngles.x, FootTarget.maxRightLowerLegAngles.x, lowerLegYname, "X Limits");
                EditorGUI.indentLevel--;
            }
        }

        private void UpdateLowerLegBones(FootTarget.TargetedLowerLegBone lowerLeg) {
            //if (lowerLeg.bone.transform == null)
            //    return;

            //lowerLeg.bone.minAngles.x = lowerLegMinX.floatValue;
            //lowerLeg.bone.maxAngles.x = lowerLegMaxX.floatValue;
        }
        #endregion

        #region Foot
        //private string footXname;
        //private SerializedProperty footMinX;
        //private SerializedProperty footMaxX;

        //private string footZname;
        //private SerializedProperty footMinZ;
        //private SerializedProperty footMaxZ;

        private void InitFootConfiguration(FootTarget.TargetedFootBone foot) {
            //if (foot.bone.transform == null)
            //    return;

            //footXname = foot.bone.transform.name + "X";
            //footMinX = serializedObject.FindProperty("foot.bone.minAngles.x");
            //footMaxX = serializedObject.FindProperty("foot.bone.maxAngles.x");

            //footZname = foot.bone.transform.name + "Z";
            //footMinZ = serializedObject.FindProperty("foot.bone.minAngles.z");
            //footMaxZ = serializedObject.FindProperty("foot.bone.maxAngles.z");
        }

        private void FootConfigurationInspector(ref FootTarget.TargetedFootBone foot, bool isLeft) {
            if (foot.bone.transform != null)
                GUI.SetNextControlName(foot.bone.transform.name + "00");
            foot.bone.transform = (Transform)EditorGUILayout.ObjectField("Foot Bone", foot.bone.transform, typeof(Transform), true);
            if (foot.bone.transform != null) {
                EditorGUI.indentLevel++;

                foot.bone.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", foot.bone.jointLimitations);
                if (foot.bone.jointLimitations) {
                    foot.bone.maxAngle = EditorGUILayout.Slider("Max Angle", foot.bone.maxAngle, 0, 180);
                }

                //EditorGUILayout.BeginHorizontal();
                //foot.bone.maxAngle = EditorGUILayout.Slider("Max Angle", foot.bone.maxAngle, 0, 180);
                //if (GUILayout.Button("R", GUILayout.Width(20))) {
                //    foot.bone.maxAngle = FootTarget.maxFootAngle;
                //}
                //EditorGUILayout.EndHorizontal();

                //if (isLeft) {
                //    Target_Editor.BoneAngleInspector(footMinX, footMaxX, FootTarget.minLeftFootAngles.x, FootTarget.maxLeftFootAngles.x, footXname, "X Limits");
                //    Target_Editor.BoneAngleInspector(footMinZ, footMaxZ, FootTarget.minLeftFootAngles.z, FootTarget.maxLeftFootAngles.z, footZname, "Z Limits");
                //} else {
                //    Target_Editor.BoneAngleInspector(footMinX, footMaxX, FootTarget.minRightFootAngles.x, FootTarget.maxRightFootAngles.x, footXname, "X Limits");
                //    Target_Editor.BoneAngleInspector(footMinZ, footMaxZ, FootTarget.minRightFootAngles.z, FootTarget.maxRightFootAngles.z, footZname, "Z Limtis");
                //}
                EditorGUI.indentLevel--;
            }
        }

        private void UpdateFootBones(FootTarget.TargetedFootBone foot) {
            //if (foot.bone.transform == null)
            //    return;

            //foot.bone.minAngles.x = footMinX.floatValue;
            //foot.bone.maxAngles.x = footMaxX.floatValue;

            //foot.bone.minAngles.z = footMinZ.floatValue;
            //foot.bone.maxAngles.z = footMaxZ.floatValue;
        }
        #endregion

        #region Toes
        //private string toesXname;
        //private SerializedProperty toesMinX;
        //private SerializedProperty toesMaxX;

        private void InitToesConfiguration(FootTarget.TargetedToesBone toes) {
            //if (toes.bone.transform == null)
            //    return;

            //toesXname = toes.bone.transform.name + "X";
            //toesMinX = serializedObject.FindProperty("toes.bone.minAngles.x");
            //toesMaxX = serializedObject.FindProperty("toes.bone.maxAngles.x");
        }

        private void ToesConfigurationInspector(ref FootTarget.TargetedToesBone toes, bool isLeft) {
            if (toes.bone.transform != null)
                GUI.SetNextControlName(toes.bone.transform.name + "00");
            toes.bone.transform = (Transform)EditorGUILayout.ObjectField("Toes Bone", toes.bone.transform, typeof(Transform), true);
            if (toes.bone.transform != null) {
                EditorGUI.indentLevel++;

                toes.bone.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", toes.bone.jointLimitations);
                if (toes.bone.jointLimitations) {
                    toes.bone.maxAngle = EditorGUILayout.Slider("Max Angle", toes.bone.maxAngle, 0, 180);
                }

                //EditorGUILayout.BeginHorizontal();
                //toes.bone.maxAngle = EditorGUILayout.Slider("Max Angle", toes.bone.maxAngle, 0, 180);
                //if (GUILayout.Button("R", GUILayout.Width(20))) {
                //    toes.bone.maxAngle = FootTarget.maxToesAngle;
                //}
                //EditorGUILayout.EndHorizontal();

                //if (isLeft)
                //    Target_Editor.BoneAngleInspector(toesMinX, toesMaxX, FootTarget.minLeftToesAngles.x, FootTarget.maxLeftToesAngles.x, toesXname, "X Limits");
                //else
                //    Target_Editor.BoneAngleInspector(toesMinX, toesMaxX, FootTarget.minRightToesAngles.x, FootTarget.maxRightToesAngles.x, toesXname, "X Limits");
                EditorGUI.indentLevel--;
            }
        }

        private void UpdateToesBones(FootTarget.TargetedToesBone toes) {
            //if (toes.bone.transform == null)
            //    return;

            //toes.bone.minAngles.x = toesMinX.floatValue;
            //toes.bone.maxAngles.x = toesMaxX.floatValue;
        }

        #endregion
        #endregion

        #region Settings
        private SerializedProperty rotationSpeedLimitationProp;
        private SerializedProperty slidePreventionProp;

        private void InitSettings() {
            rotationSpeedLimitationProp = serializedObject.FindProperty("rotationSpeedLimitation");
            slidePreventionProp = serializedObject.FindProperty("slidePrevention");
        }

        private static bool showSettings;
        private void SettingsInspector(FootTarget footTarget) {
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);
            if (showSettings) {
                EditorGUI.indentLevel++;
                //footTarget.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", footTarget.jointLimitations);
                bool showRealObjects = EditorGUILayout.Toggle("Show Real Objects", footTarget.showRealObjects);
                if (showRealObjects != footTarget.showRealObjects) {
                    footTarget.ShowControllers(showRealObjects);
                    footTarget.showRealObjects = showRealObjects;
                }
                rotationSpeedLimitationProp.boolValue = EditorGUILayout.Toggle("Rotation Speed Limitation", rotationSpeedLimitationProp.boolValue);
                slidePreventionProp.boolValue = EditorGUILayout.Toggle("Slide Prevention", slidePreventionProp.boolValue);

                EditorGUI.indentLevel--;
            }
        }
        #endregion
        #endregion

        #region Scene
        public void OnSceneGUI() {
            if (footTarget == null || humanoid == null)
                return;
            if (Application.isPlaying)
                return;

            if (humanoid.pose != null) {
                if (humanoid.editPose)
                    humanoid.pose.UpdatePose(humanoid);
                else {
                    humanoid.pose.Show(humanoid);
                    footTarget.CopyRigToTarget();
                }
            }

            // update the target rig from the current foot target
            footTarget.CopyTargetToRig();
            // update the avatar bones from the target rig
            humanoid.UpdateMovements();
            // match the target rig with the new avatar pose
            humanoid.MatchTargetsToAvatar();
            // and update all target to match the target rig
            humanoid.CopyRigToTargets();

            // Update the sensors to match the updated targets
            humanoid.UpdateSensorsFromTargets();
        }
        #endregion

        public abstract class TargetProps {
            public SerializedProperty enabledProp;
            public SerializedProperty sensorTransformProp;
            public SerializedProperty sensor2TargetPositionProp;
            public SerializedProperty sensor2TargetRotationProp;

            public FootTarget footTarget;
            public UnityLegSensor sensor;

            public TargetProps(SerializedObject serializedObject, UnityLegSensor _sensor, FootTarget _footTarget, string unitySensorName) {
                enabledProp = serializedObject.FindProperty(unitySensorName + ".enabled");
                sensorTransformProp = serializedObject.FindProperty(unitySensorName + ".sensorTransform");
                sensor2TargetPositionProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetPosition");
                sensor2TargetRotationProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetRotation");

                footTarget = _footTarget;
                sensor = _sensor;

                sensor.Init(footTarget);
            }

            public virtual void SetSensor2Target() {
                if (sensor.sensorTransform == null)
                    return;

                sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(sensor.sensorTransform.rotation) * footTarget.foot.target.transform.rotation;
                sensor2TargetPositionProp.vector3Value = -footTarget.foot.target.transform.InverseTransformPoint(sensor.sensorTransform.position);
            }

            public abstract void Inspector();
        }
    }
}
