using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(HipsTarget))]
    public class HipsTarget_Editor : Editor {
        private HipsTarget hipsTarget;
        private HumanoidControl humanoid;

        private TargetProps[] allProps;

        #region Enable
        public void OnEnable() {
            hipsTarget = (HipsTarget)target;

            humanoid = GetHumanoid(hipsTarget);
            if (humanoid == null)
                return;

            InitEditors();

            hipsTarget.InitSensors();
            InitConfiguration(hipsTarget);
        }

        private void InitEditors() {
            allProps = new TargetProps[] {
#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                new ViveTracker_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
#if hKINECT1
                new Kinect1_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
#if hKINECT2
                new Kinect2_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
#if hORBBEC
                new Astra_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
#if hNEURON
                new Neuron_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
#if hOPTITRACK
                new Optitrack_Editor.HipsTargetProps(serializedObject, hipsTarget),
#endif
            };
        }
        #endregion 

        #region Disable
        public void OnDisable() {
            if (humanoid == null) {
                // This target is not connected to a humanoid, so we delete it
                DestroyImmediate(hipsTarget, true);
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
            if (hipsTarget == null || humanoid == null)
                return;

            serializedObject.Update();

            SensorInspectors(hipsTarget);
            ConfigurationInspector(hipsTarget);

            serializedObject.ApplyModifiedProperties();
        }

        public static HumanoidControl GetHumanoid(HumanoidTarget target) {
            HumanoidControl[] humanoids = GameObject.FindObjectsOfType<HumanoidControl>();
            HumanoidControl foundHumanoid = null;

            for (int i = 0; i < humanoids.Length; i++)
                if (humanoids[i].hipsTarget.transform == target.transform)
                    foundHumanoid = humanoids[i];

            return foundHumanoid;
        }

        #region Sensors
        public bool showSensors = true;
        private void SensorInspectors(HipsTarget hipsTarget) {
            showSensors = EditorGUILayout.Foldout(showSensors, "Controllers", true);
            if (showSensors) {
                EditorGUI.indentLevel++;

                foreach (TargetProps props in allProps)
                    props.Inspector();

                if (humanoid.animatorEnabled)
                    hipsTarget.animator.enabled = EditorGUILayout.ToggleLeft("Procedural Animation", hipsTarget.animator.enabled, GUILayout.MinWidth(80));
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Configuration
        private void InitConfiguration(HipsTarget target) {
            if (target.humanoid.avatarRig == null)
                return;

            InitChestConfiguration(target.chest);
            InitSpineConfiguration(target.spine);
            InitHipsConfiguration(target.hips);
        }

        private bool showConfiguration;
        private void ConfigurationInspector(HipsTarget hipsTarget) {
            hipsTarget.RetrieveBones();

            showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration", true);
            if (showConfiguration) {
                EditorGUI.indentLevel++;

                ChestConfigurationInspector(ref hipsTarget.chest);
                SpineConfigurationInspector(ref hipsTarget.spine);
                HipsConfigurationInspector(ref hipsTarget.hips);

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateBones(HipsTarget target) {
            if (target.humanoid.avatarRig == null)
                return;

            UpdateHipsBones(target.hips);
            UpdateSpineBones(target.spine);
            UpdateChestBones(target.chest);
        }

        #region Chest
        private void InitChestConfiguration(HipsTarget.TargetedChestBone upperLeg) {
        }

        private void ChestConfigurationInspector(ref HipsTarget.TargetedChestBone chest) {
            chest.bone.transform = (Transform)EditorGUILayout.ObjectField("Chest", chest.bone.transform, typeof(Transform), true);
            if (chest.bone.transform != null) {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                chest.bone.maxAngle = EditorGUILayout.Slider("Max Angle", chest.bone.maxAngle, 0, 180);
                if (GUILayout.Button("R", GUILayout.Width(20))) {
                    chest.bone.maxAngle = HipsTarget.maxChestAngle;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateChestBones(HipsTarget.TargetedChestBone chest) {
        }

        #endregion

        #region Spine
        private void InitSpineConfiguration(HipsTarget.TargetedSpineBone spine) {
        }

        private void SpineConfigurationInspector(ref HipsTarget.TargetedSpineBone spine) {
            spine.bone.transform = (Transform)EditorGUILayout.ObjectField("Spine", spine.bone.transform, typeof(Transform), true);
            if (spine.bone.transform != null) {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                spine.bone.maxAngle = EditorGUILayout.Slider("Max Angle", spine.bone.maxAngle, 0, 180);
                if (GUILayout.Button("R", GUILayout.Width(20))) {
                    spine.bone.maxAngle = HipsTarget.maxSpineAngle;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateSpineBones(HipsTarget.TargetedSpineBone spine) {
        }
        #endregion

        #region Hips
        private void InitHipsConfiguration(HipsTarget.TargetedHipsBone hips) {
        }

        private void HipsConfigurationInspector(ref HipsTarget.TargetedHipsBone hips) {
            hips.bone.transform = (Transform)EditorGUILayout.ObjectField("Hips", hips.bone.transform, typeof(Transform), true);
        }

        private void UpdateHipsBones(HipsTarget.TargetedHipsBone hips) {
        }
        #endregion

        #endregion
        #endregion

        #region Scene
        public void OnSceneGUI() {
            if (Application.isPlaying)
                return;
            if (hipsTarget == null || humanoid == null)
                return;

            if (humanoid.pose != null) {
                if (humanoid.editPose) 
                    humanoid.pose.UpdatePose(humanoid);                
                else {
                    humanoid.pose.Show(humanoid);
                    hipsTarget.CopyRigToTarget();
                }
            }

            // update the target rig from the current hips target
            hipsTarget.CopyTargetToRig();
            // update the avatar bones to match the target rig
            humanoid.UpdateMovements();
            // match the target rig with the new avatar pose
            humanoid.MatchTargetsToAvatar();
            // and update all targets to match the target rig
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

            public HipsTarget hipsTarget;
            public UnityTorsoSensor sensor;

            public TargetProps(SerializedObject serializedObject, UnityTorsoSensor _sensor, HipsTarget _hipsTarget, string unitySensorName) {
                enabledProp = serializedObject.FindProperty(unitySensorName + ".enabled");
                sensorTransformProp = serializedObject.FindProperty(unitySensorName + ".sensorTransform");
                sensor2TargetPositionProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetPosition");
                sensor2TargetRotationProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetRotation");

                hipsTarget = _hipsTarget;
                sensor = _sensor;

                sensor.Init(hipsTarget);
            }

            public virtual void SetSensor2Target() {
                if (sensor.sensorTransform == null)
                    return;

                sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(sensor.sensorTransform.rotation) * hipsTarget.hips.target.transform.rotation;
                sensor2TargetPositionProp.vector3Value = -hipsTarget.hips.target.transform.InverseTransformPoint(sensor.sensorTransform.position);
            }

            public abstract void Inspector();
        }
    }
}
