#if hOCULUS
using UnityEditor;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Passer {

    public class Oculus_Editor : Tracker_Editor {

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, OculusTracker _oculus)
                : base(serializedObject, targetObjs, _oculus, "oculus") {
                tracker = _oculus;

                headSensorProp = targetObjs.headTargetObj.FindProperty("oculus");
                leftHandSensorProp = targetObjs.leftHandTargetObj.FindProperty("oculus");
                rightHandSensorProp = targetObjs.rightHandTargetObj.FindProperty("oculus");
            }

            public override void Inspector(HumanoidControl humanoid) {
                bool oculusSupported = OculusSupported();
                if (oculusSupported) {
                    if (humanoid.headTarget.unityVRHead.enabled)
                        humanoid.oculus.enabled = true;

                    EditorGUI.BeginDisabledGroup(humanoid.headTarget.unityVRHead.enabled);
                    Inspector(humanoid, "Oculus");
                    EditorGUI.EndDisabledGroup();
                }
                else
                    enabledProp.boolValue = false;
            }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            SerializedProperty overrideOptitrackPositionProp;

            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.oculus, headTarget, "oculus") {

                overrideOptitrackPositionProp = serializedObject.FindProperty("oculus.overrideOptitrackPosition");
            }

            public override void Inspector() {
                if (!headTarget.humanoid.oculus.enabled || !OculusSupported())
                    return;

                CheckHmdComponent(headTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(headTarget.oculus, headTarget);
                headTarget.oculus.enabled = enabledProp.boolValue;
                headTarget.oculus.CheckSensorTransform();
                if (!Application.isPlaying) {
                    headTarget.oculus.SetSensor2Target();
                    headTarget.oculus.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", headTarget.oculus.sensorTransform, typeof(Transform), true);
#if hOPTITRACK
                    if (headTarget.optitrack.enabled)
                        overrideOptitrackPositionProp.boolValue = EditorGUILayout.Toggle("Override OptiTrack Position", overrideOptitrackPositionProp.boolValue);
                    else
#endif
                        overrideOptitrackPositionProp.boolValue = true;

                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckHmdComponent(HeadTarget headTarget) {
                if (headTarget.oculus.sensorTransform == null)
                    return;

                OculusHmdComponent sensorComponent = headTarget.oculus.sensorTransform.GetComponent<OculusHmdComponent>();
                if (sensorComponent == null)
                    headTarget.oculus.sensorTransform.gameObject.AddComponent<OculusHmdComponent>();
            }
        }
        #region HMD Component
        [CustomEditor(typeof(OculusHmdComponent))]
        public class OculusHmdComponent_Editor : Editor {
            OculusHmdComponent sensorComponent;

            private void OnEnable() {
                sensorComponent = (OculusHmdComponent)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", sensorComponent.status);
                EditorGUILayout.FloatField("Position Confidence", sensorComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", sensorComponent.rotationConfidence);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
#if UNITY_ANDROID
            SerializedProperty controllerTypeProp;
#endif
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.oculus, handTarget, "oculus") {

#if UNITY_ANDROID
                controllerTypeProp = serializedObject.FindProperty("oculus.controllerType");
#endif
            }

            public override void Inspector() {
                if (!handTarget.humanoid.oculus.enabled || !OculusSupported())
                    return;

                CheckControllerComponent(handTarget);

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.oculus, handTarget);
                handTarget.oculus.enabled = enabledProp.boolValue;
                handTarget.oculus.CheckSensorTransform();
                if (!Application.isPlaying) {
                    handTarget.oculus.SetSensor2Target();
                    handTarget.oculus.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.oculus.sensorTransform, typeof(Transform), true);
#if UNITY_ANDROID
                    controllerTypeProp.intValue = (int)(OculusHand.ControllerType)EditorGUILayout.EnumPopup("Controller Type", (OculusHand.ControllerType)controllerTypeProp.intValue);
#endif
                    EditorGUI.indentLevel--;
                }
            }

            protected static void CheckControllerComponent(HandTarget handTarget) {
                if (handTarget.oculus.sensorTransform == null)
                    return;

                OculusControllerComponent sensorComponent = handTarget.oculus.sensorTransform.GetComponent<OculusControllerComponent>();
                if (sensorComponent == null)
                    sensorComponent = handTarget.oculus.sensorTransform.gameObject.AddComponent<OculusControllerComponent>();
                sensorComponent.isLeft = handTarget.isLeft;
            }
        }
        #region Controller Component
        [CustomEditor(typeof(OculusControllerComponent))]
        public class OculusControllerComponent_Editor : Editor {
            OculusControllerComponent controllerComponent;

            private void OnEnable() {
                controllerComponent = (OculusControllerComponent)target;
            }

            public override void OnInspectorGUI() {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Status", controllerComponent.status);
                EditorGUILayout.FloatField("Position Confidence", controllerComponent.positionConfidence);
                EditorGUILayout.FloatField("Rotation Confidence", controllerComponent.rotationConfidence);
                EditorGUILayout.Space();
                EditorGUILayout.Toggle("Is Left", controllerComponent.isLeft);
                EditorGUILayout.Vector3Field("Joystick", controllerComponent.joystick);
                EditorGUILayout.Slider("Index Trigger", controllerComponent.indexTrigger, -1, 1);
                EditorGUILayout.Slider("Hand Trigger", controllerComponent.handTrigger, -1, 1);
                if (controllerComponent.isLeft) {
                    EditorGUILayout.Slider("Button X", controllerComponent.buttonAX, -1, 1);
                    EditorGUILayout.Slider("Button Y", controllerComponent.buttonBY, -1, 1);
                }
                else {
                    EditorGUILayout.Slider("Button A", controllerComponent.buttonAX, -1, 1);
                    EditorGUILayout.Slider("Button B", controllerComponent.buttonBY, -1, 1);
                }
                EditorGUILayout.Slider("Thumbrest", controllerComponent.thumbrest, -1, 1);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion
        #endregion

        #region Object Target
        /*
                private static SerializedProperty objectEnabledProp;
                private static SerializedProperty objectSensorTransformProp;
                private static SerializedProperty objectSensor2TargetPositionProp;
                private static SerializedProperty objectSensor2TargetRotationProp;

                public static void InitObject(SerializedObject serializedObject, ObjectTarget objectTarget) {
                    objectEnabledProp = serializedObject.FindProperty("oculusController.enabled");
                    objectSensorTransformProp = serializedObject.FindProperty("oculusController.sensorTransform");
                    objectSensor2TargetPositionProp = serializedObject.FindProperty("oculusController.sensor2TargetPosition");
                    objectSensor2TargetRotationProp = serializedObject.FindProperty("oculusController.sensor2TargetRotation");

                    objectTarget.oculus.Init(objectTarget);
                }

                private enum LeftRight {
                    Left,
                    Right
                }

                public static void ObjectInspector(OculusController controller) {
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

                public static void SetSensor2Target(OculusController controller) {
                    controller.SetSensor2Target();
                    objectSensor2TargetRotationProp.quaternionValue = controller.sensor2TargetRotation;
                    objectSensor2TargetPositionProp.vector3Value = controller.sensor2TargetPosition;
                }
                */
        #endregion

        private static bool OculusSupported() {
#if UNITY_2017_2_OR_NEWER
                string[] supportedDevices = XRSettings.supportedDevices;
#else
            string[] supportedDevices = VRSettings.supportedDevices;
#endif
            foreach (string supportedDevice in supportedDevices) {
                if (supportedDevice == "Oculus")
                    return true;
            }
            return false;
        }
    }
}
#endif