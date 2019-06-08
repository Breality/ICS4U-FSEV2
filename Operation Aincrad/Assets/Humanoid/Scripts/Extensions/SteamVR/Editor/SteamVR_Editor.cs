#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEditor;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Passer {

    public class SteamVR_Editor : Tracker_Editor {

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

#if hVIVETRACKER
            private ViveTracker_Editor.TrackerProps viveTrackerProps;
#endif

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, SteamVRTracker _steam)
                : base(serializedObject, targetObjs, _steam, "steam") {
                tracker = _steam;

                headSensorProp = targetObjs.headTargetObj.FindProperty("steamVR");
                leftHandSensorProp = targetObjs.leftHandTargetObj.FindProperty("steamVR");
                rightHandSensorProp = targetObjs.rightHandTargetObj.FindProperty("steamVR");

#if hVIVETRACKER
                viveTrackerProps = new ViveTracker_Editor.TrackerProps(serializedObject, targetObjs, _steam);
#endif
            }

            public override void Inspector(HumanoidControl humanoid) {
                bool steamVrSupported = SteamVRSupported();
                if (steamVrSupported) {
                    if (humanoid.headTarget.unityVRHead.enabled)
                        humanoid.steam.enabled = true;

                    EditorGUI.BeginDisabledGroup(humanoid.headTarget.unityVRHead.enabled);
                    Inspector(humanoid, "TrackerModels/Lighthouse");
                    EditorGUI.EndDisabledGroup();

#if hVIVETRACKER
                    viveTrackerProps.Inspector(humanoid);
#endif
                }
                else
                    enabledProp.boolValue = false;
            }

            public override void InitControllers() {
                base.InitControllers();
#if hVIVETRACKER
                viveTrackerProps.InitControllers();
#endif
            }

            public override void RemoveControllers() {
                base.RemoveControllers();
#if hVIVETRACKER
                viveTrackerProps.RemoveControllers();
#endif
            }

            public override void SetSensors2Target() {
                base.SetSensors2Target();
#if hVIVETRACKER
                viveTrackerProps.SetSensors2Target();
#endif
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.steamVR, headTarget, "steamVR") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.steam.enabled || !SteamVRSupported())
                    return;

                CheckHmdComponent(headTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(headTarget.steamVR, headTarget);
                headTarget.steamVR.enabled = enabledProp.boolValue;
                headTarget.steamVR.CheckSensorTransform();
                if (!Application.isPlaying) {
                    headTarget.steamVR.SetSensor2Target();
                    headTarget.steamVR.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", headTarget.steamVR.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckHmdComponent(HeadTarget headTarget) {
                if (headTarget.steamVR.sensorTransform == null)
                    return;

                SteamVrHmdComponent sensorComponent = headTarget.steamVR.sensorTransform.GetComponent<SteamVrHmdComponent>();
                if (sensorComponent == null)
                    headTarget.steamVR.sensorTransform.gameObject.AddComponent<SteamVrHmdComponent>();
            }
        }

        #region HMD Component
        [CustomEditor(typeof(SteamVrHmdComponent))]
        public class SteamVrHmdComponent_Editor : Editor {
            SteamVrHmdComponent sensorComponent;

            private void OnEnable() {
                sensorComponent = (SteamVrHmdComponent)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", sensorComponent.status);
                EditorGUILayout.FloatField("Position Confidence", sensorComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", sensorComponent.rotationConfidence);
                EditorGUILayout.Space();
                EditorGUILayout.IntField("Tracker Id", sensorComponent.trackerId);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.steamVR, handTarget, "steamVR") {

            }

            public override void Inspector() {
                if (!handTarget.humanoid.steam.enabled || !SteamVRSupported())
                    return;

                CheckControllerComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.steamVR, handTarget);
                handTarget.steamVR.enabled = enabledProp.boolValue;
                handTarget.steamVR.CheckSensorTransform();
                if (!Application.isPlaying) {
                    handTarget.steamVR.SetSensor2Target();
                    handTarget.steamVR.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.steamVR.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckControllerComponent(HandTarget handTarget) {
                if (handTarget.steamVR.sensorTransform == null)
                    return;

                SteamVrControllerComponent sensorComponent = handTarget.steamVR.sensorTransform.GetComponent<SteamVrControllerComponent>();
                if (sensorComponent == null)
                    sensorComponent = handTarget.steamVR.sensorTransform.gameObject.AddComponent<SteamVrControllerComponent>();
                sensorComponent.isLeft = handTarget.isLeft;
            }
        }

        #region Controller Component
        [CustomEditor(typeof(SteamVrControllerComponent))]
        public class SteamVrControllerComponent_Editor : Editor {
            SteamVrControllerComponent controllerComponent;

            //SerializedProperty controllerTypeProp;

            private void OnEnable() {
                controllerComponent = (SteamVrControllerComponent)target;

                //controllerTypeProp = serializedObject.FindProperty("controllerType");
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", controllerComponent.status);
                EditorGUILayout.FloatField("Position Confidence", controllerComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", controllerComponent.rotationConfidence);
                EditorGUILayout.IntField("Tracker Id", controllerComponent.trackerId);
                EditorGUILayout.Space();
                EditorGUILayout.Toggle("Is Left", controllerComponent.isLeft);
                EditorGUILayout.Vector3Field("Touchpad", controllerComponent.touchPad);
                EditorGUILayout.Slider("Trigger", controllerComponent.trigger, -1, 1);
                EditorGUILayout.Slider("Grip", controllerComponent.gripButton, -1, 1);
                EditorGUILayout.Slider("Menu", controllerComponent.menuButton, -1, 1);
                EditorGUILayout.Slider("Button A", controllerComponent.aButton, -1, 1);
                EditorGUI.EndDisabledGroup();
                //controllerTypeProp.intValue = (int)(SteamVrControllerComponent.SteamVRControllerType)EditorGUILayout.EnumPopup("View Controller Type", controllerComponent.controllerType);

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        #region Object
        /*
        private static SerializedProperty objectEnabledProp;
        private static SerializedProperty objectSensorTransformProp;
        private static SerializedProperty objectSensor2TargetPositionProp;
        private static SerializedProperty objectSensor2TargetRotationProp;

        public static void InitObject(ObjectTarget objectTarget, SerializedObject serializedObject) {
            objectEnabledProp = serializedObject.FindProperty("steamVrController.enabled");
            objectSensorTransformProp = serializedObject.FindProperty("steamVrController.sensorTransform");
            objectSensor2TargetPositionProp = serializedObject.FindProperty("steamVrController.sensor2TargetPosition");
            objectSensor2TargetRotationProp = serializedObject.FindProperty("steamVrController.sensor2TargetRotation");

            objectTarget.steamVR.Init(objectTarget);
        }

        private enum LeftRight {
            Left,
            Right
        }

        public static void ObjectInspector(SteamVrHandController controller) {
            objectEnabledProp.boolValue = Target_Editor.ControllerInspector(controller);
            controller.CheckSensorTransform();

            if (objectEnabledProp.boolValue) {
                EditorGUI.indentLevel++;
                LeftRight leftRight = controller.isLeft ? LeftRight.Left : LeftRight.Right;
                leftRight = (LeftRight)EditorGUILayout.EnumPopup("Tracker Id", leftRight);
                controller.isLeft = leftRight == LeftRight.Left;
                objectSensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", controller.sensorTransform, typeof(Transform), true);
                EditorGUI.indentLevel--;
            }
        }

        public static void SetSensor2Target(SteamVrHandController controller) {
            controller.SetSensor2Target();
            objectSensor2TargetRotationProp.quaternionValue = controller.sensor2TargetRotation;
            objectSensor2TargetPositionProp.vector3Value = controller.sensor2TargetPosition;
        }
        */
        #endregion

        private static bool SteamVRSupported() {
#if UNITY_2017_2_OR_NEWER
                string[] supportedDevices = XRSettings.supportedDevices;
#else
            string[] supportedDevices = VRSettings.supportedDevices;
#endif
            foreach (string supportedDevice in supportedDevices) {
                if (supportedDevice == "OpenVR")
                    return true;
            }
            return false;
        }
    }
}
#endif