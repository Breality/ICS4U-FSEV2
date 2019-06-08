using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(HeadTarget))]
    public class HeadTarget_Editor : Editor {
        private HeadTarget headTarget;
        private HumanoidControl humanoid;

        private TargetProps[] allProps;

        #region Enable
        public void OnEnable() {
            headTarget = (HeadTarget)target;
            humanoid = GetHumanoid(headTarget);

            InitEditors();

            headTarget.InitSensors();
            InitSensors();
#if hFACE
            FaceTarget_Editor.OnEnable(serializedObject, headTarget);
#endif
        }
        
        private void InitEditors() {
            allProps = new TargetProps[] {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                new SteamVR_Editor.HeadTargetProps(serializedObject, headTarget),
#if hVIVETRACKER
                new ViveTracker_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#endif
#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                new Oculus_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
                new WindowsMR_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hVRTK
                new Vrtk_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hREALSENSE
                new Realsense_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hKINECT1
                new Kinect1_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hKINECT2
                new Kinect2_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hORBBEC
                new Astra_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hOPTITRACK
                new Optitrack_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hNEURON
                new Neuron_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hTOBII
                new Tobii_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hDLIB
                new Tracking.Dlib_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
#if hPUPIL
                new Tracking.Pupil.Pupil_Editor.HeadTargetProps(serializedObject, headTarget),
#endif
            };
        }
        #endregion

        #region Disable
        public void OnDisable() {
            if (humanoid == null) {
                // This target is not connected to a humanoid, so we delete it
                DestroyImmediate(headTarget, true);
                return;
            }

            if (!Application.isPlaying) {
                SetSensor2Target();
            }

#if hFACE
            FaceTarget_Editor.OnDisable(serializedObject, headTarget);
#endif
        }

        private void SetSensor2Target() {
            if (allProps != null)
                foreach (TargetProps props in allProps)
                    props.SetSensor2Target();
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            if (headTarget == null || humanoid == null)
                return;

            serializedObject.Update();

            SensorInspectors(headTarget);
            if (headTarget.humanoid != null) {
#if hFACE
                FaceTarget_Editor.OnInspectorGUI(headTarget);
#endif
                ConfigurationInspector(headTarget);

#if hFACE
                FaceTarget_Editor.ExpressionsInspector(headTarget.face);
#endif
            }

            PoseInspector();
            SettingsInspector(headTarget);
#if hFACE
            FaceTarget_Editor.FocusObjectInspector(headTarget.face);
#endif

            GazeInteractionButton(headTarget);

            //serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private static HumanoidControl GetHumanoid(HumanoidTarget target) {
            HumanoidControl[] humanoids = GameObject.FindObjectsOfType<HumanoidControl>();
            HumanoidControl foundHumanoid = null;

            for (int i = 0; i < humanoids.Length; i++)
                if (humanoids[i].headTarget.transform == target.transform)
                    foundHumanoid = humanoids[i];

            return foundHumanoid;
        }

        #region Sensors
#if hFACE
        private SerializedProperty microphoneEnabledProp;
#endif
        private void InitSensors() {
            // only face properties for now
#if hFACE
            microphoneEnabledProp = serializedObject.FindProperty("microphone.enabled");
#endif
        }

        private bool showControllers = true;
        private void SensorInspectors(HeadTarget headTarget) {
            showControllers = EditorGUILayout.Foldout(showControllers, "Controllers", true);
            if (showControllers) {
                EditorGUI.indentLevel++;
                FirstPersonCameraInspector(headTarget);
                ScreenInspector(headTarget);

                foreach (TargetProps props in allProps)
                    props.Inspector();

                AnimatorInspector(headTarget);
                EditorGUI.indentLevel--;
            }
        }

        private void FirstPersonCameraInspector(HeadTarget headTarget) {
            if (headTarget.unityVRHead == null)
                return;

#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            EditorGUI.BeginDisabledGroup(headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled);
#endif
            bool wasEnabled = headTarget.unityVRHead.enabled;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            if (headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled)
                headTarget.unityVRHead.enabled = false;
#endif
            bool enabled = EditorGUILayout.ToggleLeft(headTarget.unityVRHead.name, headTarget.unityVRHead.enabled, GUILayout.MinWidth(80));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(headTarget, enabled ? "Enabled " : "Disabled " + headTarget.unityVRHead.name);
                headTarget.unityVRHead.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();

#if hFACE
            if (enabled) { // && microphoneEnabledProp != null) {
                EditorGUI.indentLevel++;
                microphoneEnabledProp.boolValue = EditorGUILayout.ToggleLeft("Microphone", microphoneEnabledProp.boolValue);
                EditorGUI.indentLevel--;
            }
#endif

            if (!Application.isPlaying) {
                UnityVRHead.CheckCamera(headTarget);
                if (!wasEnabled && headTarget.unityVRHead.enabled) {
                    UnityVRHead.AddCamera(headTarget);
                }
                else if (wasEnabled && !headTarget.unityVRHead.enabled) {
                    UnityVRHead.RemoveCamera(headTarget);
                }
            }
#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            EditorGUI.EndDisabledGroup();
#endif

        }

        private void ScreenInspector(HeadTarget headTarget) {
            if (headTarget.virtual3d && headTarget.humanoid.showRealObjects) {
                if (headTarget.screenTransform == null)
                    CreateScreen(headTarget);
                headTarget.screenTransform.gameObject.SetActive(true);
            }
            else if (headTarget.screenTransform != null)
                headTarget.screenTransform.gameObject.SetActive(false);
        }

        private void CreateScreen(HeadTarget headTarget) {
            GameObject realWorld = HumanoidControl.GetRealWorld(headTarget.humanoid.transform);

            headTarget.screenTransform = realWorld.transform.Find("Screen");
            if (headTarget.screenTransform == null) {
                GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube); //new GameObject("Screen");
                screenObj.name = "Screen";
                headTarget.screenTransform = screenObj.transform;
                headTarget.screenTransform.parent = realWorld.transform;
                headTarget.screenTransform.localPosition = headTarget.transform.position + headTarget.transform.forward;
                headTarget.screenTransform.rotation = headTarget.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                headTarget.screenTransform.localScale = new Vector3(0.476F, 0.2677F, 0.02F); // 21.5 inch 16:9 screen size
            }
        }

        private void AnimatorInspector(HeadTarget headTarget) {
            if (headTarget.humanoid == null)
                return;

            if (headTarget.humanoid.animatorEnabled) {
                headTarget.headAnimator.enabled = EditorGUILayout.ToggleLeft("Procedural Animation", headTarget.headAnimator.enabled, GUILayout.MinWidth(80));
                if (headTarget.headAnimator.enabled) {
                    EditorGUI.indentLevel++;
                    headTarget.headAnimator.headAnimation = EditorGUILayout.ToggleLeft("Head Animation", headTarget.headAnimator.headAnimation);
                    headTarget.headAnimator.faceAnimation = EditorGUILayout.ToggleLeft("Face Animation", headTarget.headAnimator.faceAnimation);
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Configuration
        private bool showConfiguration;
        private bool showLeftEye;
        private bool showRightEye;
        private void ConfigurationInspector(HeadTarget headTarget) {
            if (headTarget.humanoid == null)
                return;

            showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration", true);
            if (showConfiguration) {
                EditorGUI.indentLevel++;
#if hFACE
                FaceTarget_Editor.ConfigurationInspector(headTarget.face);
#endif
                HeadConfigurationInspector(ref headTarget.head);
                NeckConfigurationInspector(ref headTarget.neck);

                EditorGUI.indentLevel--;
            }
        }

        private void HeadConfigurationInspector(ref HeadTarget.TargetedHeadBone head) {
            head.bone.transform = (Transform)EditorGUILayout.ObjectField("Head", head.bone.transform, typeof(Transform), true);
            if (head.bone.transform != null) {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                head.bone.maxAngle = EditorGUILayout.Slider("Max Angle", head.bone.maxAngle, 0, 180);
                if (GUILayout.Button("R", GUILayout.Width(20))) {
                    head.bone.maxAngle = HeadTarget.maxHeadAngle;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        private void NeckConfigurationInspector(ref HeadTarget.TargetedNeckBone neck) {
            neck.bone.transform = (Transform)EditorGUILayout.ObjectField("Neck", neck.bone.transform, typeof(Transform), true);
            if (neck.bone.transform != null) {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                neck.bone.maxAngle = EditorGUILayout.Slider("Max Angle", neck.bone.maxAngle, 0, 180);
                if (GUILayout.Button("R", GUILayout.Width(20))) {
                    neck.bone.maxAngle = HeadTarget.maxNeckAngle;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        #endregion

        #region Pose
        private void PoseInspector() {
            if (!Application.isPlaying && humanoid.pose != null && humanoid.editPose)
                humanoid.pose.UpdatePose(humanoid);
        }
        #endregion

        #region Settings
        private bool showSettings;
        private void SettingsInspector(HeadTarget headTarget) {
            //showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);
            //if (showSettings) {
            //    EditorGUI.indentLevel++;
            //    //headTarget.jointLimitations = EditorGUILayout.Toggle("Joint Limitations", headTarget.jointLimitations);
            //    EditorGUI.indentLevel--;
            //}
        }
        #endregion

        #region Buttons
        private void GazeInteractionButton(HeadTarget headTarget) {
            InteractionPointer interactionPointer = headTarget.transform.GetComponentInChildren<InteractionPointer>();
            if (interactionPointer != null)
                return;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Interaction Pointer"))
                AddInteractionPointer();
            if (GUILayout.Button("Add Teleporter"))
                AddTeleporter();
            GUILayout.EndHorizontal();
        }

        private void AddInteractionPointer() {
            InteractionPointer pointer = HumanoidInteractionPointer.Add(headTarget.transform, InteractionPointer.PointerType.FocusPoint);
            Camera fpCamera = UnityVRHead.GetCamera(headTarget);
            if (fpCamera != null) {
                pointer.transform.position = fpCamera.transform.position;
                pointer.transform.rotation = fpCamera.transform.rotation;
            }
            pointer.focusPointObj.transform.localPosition = new Vector3(0, 0, 2);

            HeadInput.Add(headTarget.transform);

            ControllerInput controllerInput = headTarget.humanoid.GetComponent<ControllerInput>();
            if (controllerInput != null) {
                controllerInput.leftButtonOneInput.SetMethod(pointer.Click, InputEvent.EventType.Start);
            }
        }

        private void AddTeleporter() {
            Teleporter teleporter = Teleporter.Add(headTarget.transform, InteractionPointer.PointerType.FocusPoint);
            if (headTarget.unityVRHead.cameraTransform != null) {
                teleporter.transform.position = headTarget.unityVRHead.cameraTransform.position;
                teleporter.transform.rotation = headTarget.unityVRHead.cameraTransform.rotation;
            }
            teleporter.focusPointObj.transform.localPosition = new Vector3(0, 0, 2);

            HeadInput.Add(headTarget.transform);

            ControllerInput controllerInput = headTarget.humanoid.GetComponent<ControllerInput>();
            if (controllerInput != null) {
                controllerInput.leftButtonOneInput.SetMethod(teleporter.Click, InputEvent.EventType.Start);
            }
        }
        #endregion
        #endregion

        #region Scene
        public void OnSceneGUI() {
            if (Application.isPlaying)
                return;
            if (headTarget == null || headTarget.humanoid == null)
                return;

#if hFACE
            FaceTarget_Editor.UpdateScene(headTarget.face);
#endif
            if (humanoid.pose != null) {
                if (humanoid.editPose)
                    humanoid.pose.UpdatePose(humanoid);
                else {
                    humanoid.pose.Show(humanoid);
                    headTarget.CopyRigToTarget();
                }
            }                

            // update the target rig from the current head target
            headTarget.CopyTargetToRig();
            // update the avatar bones from the target rig
            humanoid.UpdateMovements();
            // match the target rig with the new avatar pose
            humanoid.MatchTargetsToAvatar();
            // and update all targets to match the target rig
            humanoid.CopyRigToTargets();

            // Update the sensors to match the updated targets
            humanoid.UpdateSensorsFromTargets();

            //if (headTarget.virtual3dTracking && headTarget.humanoid.showRealObjects)
            //    DrawScreen(headTarget);
        }

        //private void DrawScreen(HeadTarget headTarget) {
        //    Handles.DrawWireCube(headTarget.screenTransform.position, headTarget.screenTransform.lossyScale);
        //}
        #endregion

        public abstract class TargetProps {
            public SerializedProperty enabledProp;
            public SerializedProperty sensorTransformProp;
            public SerializedProperty sensor2TargetPositionProp;
            public SerializedProperty sensor2TargetRotationProp;

            public HeadTarget headTarget;
            public UnityHeadSensor sensor;

            public TargetProps(SerializedObject serializedObject, UnityHeadSensor _sensor, HeadTarget _headTarget, string unitySensorName) {
                enabledProp = serializedObject.FindProperty(unitySensorName + ".enabled");
                sensorTransformProp = serializedObject.FindProperty(unitySensorName + ".sensorTransform");
                sensor2TargetPositionProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetPosition");
                sensor2TargetRotationProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetRotation");

                headTarget = _headTarget;
                sensor = _sensor;

                sensor.Init(headTarget);
            }

            public virtual void SetSensor2Target() {
                if (sensor.sensorTransform == null)
                    return;

                sensor2TargetRotationProp.quaternionValue = Quaternion.Inverse(sensor.sensorTransform.rotation) * headTarget.head.target.transform.rotation;
                sensor2TargetPositionProp.vector3Value = -headTarget.head.target.transform.InverseTransformPoint(sensor.sensorTransform.position);
            }

            public abstract void Inspector();
        }
    }
}