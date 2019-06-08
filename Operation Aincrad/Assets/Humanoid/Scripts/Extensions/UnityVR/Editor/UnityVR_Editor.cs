/* UnityVR extension editor
 * copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: September 4, 2016
 * 
 */

using UnityEditor;
using UnityEngine;

namespace Passer {

    public class UnityVR_Editor : Editor {

        public static void AddTracker(HumanoidControl humanoid) {
            // you cannot find a tracker in a disabled gameObject
            if (!humanoid.gameObject.activeInHierarchy)
                return;

            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);

            UnityVRDevice.trackerObject = GameObject.Find(UnityVRDevice.trackerName);
            if (UnityVRDevice.trackerObject == null) {
                UnityVRDevice.trackerObject = new GameObject {
                    name = UnityVRDevice.trackerName
                };
                UnityVRDevice.trackerObject.transform.parent = realWorld.transform;
                UnityVRDevice.trackerObject.transform.localPosition = Vector3.zero;
            }
        }

        private static void RemoveTracker() {
            DestroyImmediate(UnityVRDevice.trackerObject, true);
        }

        public static void ShowTracker(bool show) {
            if (UnityVRDevice.trackerObject == null)
                return;

            if (show && !UnityVRDevice.trackerObject.activeSelf && UnityVRDevice.present)
                HumanoidControl_Editor.ShowTracker(UnityVRDevice.trackerObject, true);

            else if (!show && UnityVRDevice.trackerObject.activeSelf)
                HumanoidControl_Editor.ShowTracker(UnityVRDevice.trackerObject, false);
        }

        public static void Inspector(HumanoidControl humanoid) {
            if (humanoid.headTarget == null)
                return;

            FirstPersonCameraInspector(humanoid.headTarget);
#if (UNITY_STANDALONE_WIN || UNITY_ANDROID)
            if (PlayerSettings.virtualRealitySupported)
                AddTracker(humanoid);
            else
                RemoveTracker();

            ShowTracker(humanoid.showRealObjects);
#endif
        }

        private static void FirstPersonCameraInspector(HeadTarget headTarget) {
            if (headTarget.unityVRHead == null || headTarget.humanoid == null)
                return;

#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            EditorGUI.BeginDisabledGroup(headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled);
#endif
            bool wasEnabled = headTarget.unityVRHead.enabled;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            if (headTarget.humanoid.steam.enabled && headTarget.viveTracker.enabled)
                headTarget.unityVRHead.enabled = false;
#endif
            GUIContent text = new GUIContent(
                "First Person Camera",
                "Enables a first person camera. Disabling and enabling again reset the camera position"
                );
            bool enabled = EditorGUILayout.ToggleLeft(text, headTarget.unityVRHead.enabled, GUILayout.Width(200));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(headTarget, enabled ? "Enabled " : "Disabled " + headTarget.unityVRHead.name);
                headTarget.unityVRHead.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();

            if (!Application.isPlaying) {
                UnityVRHead.CheckCamera(headTarget);
                if (!wasEnabled && headTarget.unityVRHead.enabled) {
                    UnityVRHead.AddCamera(headTarget);
                } else if (wasEnabled && !headTarget.unityVRHead.enabled) {
                    UnityVRHead.RemoveCamera(headTarget);
                }
            }
#if hSTEAMVR && hVIVETRACKER && UNITY_STANDALONE_WIN
            EditorGUI.EndDisabledGroup();
#endif

        }
    }
}