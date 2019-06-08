#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace Passer {

    public class WindowsMR_Editor : Tracker_Editor {

#region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {
            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, WindowsMRTracker _mrTracker)
                : base(serializedObject, targetObjs, _mrTracker, "mixedReality") {
                tracker = _mrTracker;

                headTargetProp = targetObjs.headTargetObj.FindProperty("mixedReality.target");
                leftHandTargetProp = targetObjs.leftHandTargetObj.FindProperty("mixedReality.target");
                rightHandTargetProp = targetObjs.rightHandTargetObj.FindProperty("mixedReality.target");

                headSensorTransformProp = targetObjs.headTargetObj.FindProperty("mixedReality.sensorTransform");
                leftHandSensorTransformProp = targetObjs.leftHandTargetObj.FindProperty("mixedReality.sensorTransform");
                rightHandSensorTransformProp = targetObjs.rightHandTargetObj.FindProperty("mixedReality.sensorTransform");
            }

            public override void Inspector(HumanoidControl humanoid) {
                bool mixedRealitySupported = MixedRealitySupported();
                if (mixedRealitySupported) {
                    if (humanoid.headTarget.unityVRHead.enabled)
                        humanoid.mixedReality.enabled = true;

                    EditorGUI.BeginDisabledGroup(humanoid.headTarget.unityVRHead.enabled);
                    Inspector(humanoid, null);
                    EditorGUI.EndDisabledGroup();
                }
                else {
                    enabledProp.boolValue = false;
                }
            }

            protected override void RemoveTracker() { }
        }
#endregion

#region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base (serializedObject, headTarget.mixedReality, headTarget, "mixedReality") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.mixedReality.enabled || !MixedRealitySupported())
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(headTarget.mixedReality, headTarget);
                headTarget.mixedReality.enabled = enabledProp.boolValue;
                headTarget.mixedReality.CheckSensorTransform();
                if (!Application.isPlaying) {
                    headTarget.mixedReality.SetSensor2Target();
                    headTarget.mixedReality.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);
                }

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", headTarget.mixedReality.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
#endregion

#region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.mixedReality, handTarget, "mixedReality") {
            }

            public override void Inspector() {
                if (!PlayerSettings.virtualRealitySupported || !handTarget.humanoid.mixedReality.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.mixedReality, handTarget);
                handTarget.mixedReality.enabled = enabledProp.boolValue;
                handTarget.mixedReality.CheckSensorTransform();
                if (!Application.isPlaying)
                    handTarget.mixedReality.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.mixedReality.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
#endregion

        private static bool MixedRealitySupported() {
            string[] supportedDevices = XRSettings.supportedDevices;
            foreach (string supportedDevice in supportedDevices) {
                if (supportedDevice == "WindowsMR")
                    return true;
            }
            return false;
        }

    }
}
#endif
