#if hVRTK
using UnityEditor;
using UnityEngine;
using VRTK;

namespace Passer {

    public class Vrtk_Editor : Tracker_Editor {

        #region Tracker
        public class TrackerProps : HumanoidControl_Editor.HumanoidTrackerProps {

            public TrackerProps(SerializedObject serializedObject, HumanoidControl_Editor.HumanoidTargetObjs targetObjs, VrtkTracker vrtk)
                : base(serializedObject, targetObjs, vrtk, "vrtk") {
                tracker = vrtk;

                leftHandSensorProp = targetObjs.leftHandTargetObj.FindProperty("vrtk");
                rightHandSensorProp = targetObjs.rightHandTargetObj.FindProperty("vrtk");

                VRTK_SDKManager sdkManager = FindObjectOfType<VRTK_SDKManager>();
                if (sdkManager == null)
                    return;

                vrtk.sdkManager = sdkManager;
            }

            public override void Inspector(HumanoidControl humanoid) {
                Inspector(humanoid, "VRTK");

                if (humanoid.vrtk.enabled) {
                    if (humanoid.vrtk.sdkManager != null) {
                        humanoid.vrtk.sdkManager.scriptAliasLeftController = humanoid.leftHandTarget.gameObject;
                        humanoid.vrtk.sdkManager.scriptAliasRightController = humanoid.rightHandTarget.gameObject;
                    }
                    else
                        EditorGUILayout.HelpBox("Could not find a VRTK SDK Manager component", MessageType.Warning);
                }

            }

            protected override void RemoveTracker() { }
        }
        #endregion

        #region Head
        public class HeadTargetProps : HeadTarget_Editor.TargetProps {
            public HeadTargetProps(SerializedObject serializedObject, HeadTarget headTarget)
                : base(serializedObject, headTarget.vrtk, headTarget, "vrtk") {
            }

            public override void Inspector() {
                if (!headTarget.humanoid.vrtk.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(headTarget.vrtk, headTarget);
                headTarget.vrtk.enabled = enabledProp.boolValue;
                headTarget.vrtk.CheckSensorTransform();
                if (!Application.isPlaying)
                    headTarget.vrtk.ShowSensor(headTarget.humanoid.showRealObjects && headTarget.showRealObjects);

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", headTarget.vrtk.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Hand
        public class HandTargetProps : HandTarget_Editor.TargetProps {
            public HandTargetProps(SerializedObject serializedObject, HandTarget handTarget)
                : base(serializedObject, handTarget.vrtk, handTarget, "vrtk") {

            }

            public override void Inspector() {
                if (!handTarget.humanoid.vrtk.enabled)
                    return;

                enabledProp.boolValue = Target_Editor.ControllerInspector(handTarget.vrtk, handTarget);
                handTarget.vrtk.enabled = enabledProp.boolValue;
                handTarget.vrtk.CheckSensorTransform();
                if (!Application.isPlaying)
                    handTarget.vrtk.ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);

                if (enabledProp.boolValue) {
                    EditorGUI.indentLevel++;
                    sensorTransformProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Tracker Transform", handTarget.vrtk.sensorTransform, typeof(Transform), true);
                    EditorGUI.indentLevel--;
                }
                CheckControllerEvents(handTarget, enabledProp.boolValue);
            }

            private void CheckControllerEvents(HandTarget handTarget, bool enabled) {
                VRTK_ControllerEvents controllerEvents = handTarget.GetComponent<VRTK_ControllerEvents>();
                if (enabled && controllerEvents == null)
                    handTarget.gameObject.AddComponent<VRTK_ControllerEvents>();
                else if (!enabled && controllerEvents != null)
                    DestroyImmediate(controllerEvents, true);
            }
        }
        #endregion

    }
}
#endif