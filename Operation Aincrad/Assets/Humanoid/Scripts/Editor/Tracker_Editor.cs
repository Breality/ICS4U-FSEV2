using UnityEngine;
using UnityEditor;

namespace Passer {

    public class Tracker_Editor : Editor {
        public static void Inspector(HumanoidControl humanoid, Tracker tracker, SerializedProperty enabledProp, string resourceName) {
            EditorGUILayout.BeginHorizontal();
            //EditorGUI.BeginChangeCheck();
            bool wasEnabled = enabledProp.boolValue;
            enabledProp.boolValue = EditorGUILayout.ToggleLeft(tracker.name, enabledProp.boolValue, GUILayout.MinWidth(80));
            if (Application.isPlaying && enabledProp.boolValue)
                EditorGUILayout.EnumPopup(tracker.status);
            EditorGUILayout.EndHorizontal();

#if UNITY_2018_3_OR_NEWER
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(humanoid);
            if (prefabType != PrefabAssetType.NotAPrefab)
                return;
#else
            PrefabType prefabType = PrefabUtility.GetPrefabType(humanoid);
            if (prefabType == PrefabType.Prefab)
                return;
#endif

            if (tracker.humanoid == null)
                tracker.humanoid = humanoid;

            if (enabledProp.boolValue && resourceName != null)
                tracker.AddTracker(humanoid, resourceName);
            else if (wasEnabled) {
                RemoveTracker(tracker);
            }

            tracker.ShowTracker(humanoid.showRealObjects && enabledProp.boolValue);
        }


        public static void Inspector(Tracker tracker, SerializedProperty enabledProp) {
            EditorGUILayout.BeginHorizontal();
            //EditorGUI.BeginChangeCheck();
            enabledProp.boolValue = EditorGUILayout.ToggleLeft(tracker.name, enabledProp.boolValue, GUILayout.MinWidth(80));
            if (Application.isPlaying && enabledProp.boolValue)
                EditorGUILayout.EnumPopup(tracker.status);
            EditorGUILayout.EndHorizontal();
        }

        protected static void RemoveTracker(Tracker tracker) {
            if (tracker.trackerTransform == null)
                return;
            DestroyImmediate(tracker.trackerTransform.gameObject, true);
        }

        public static Transform RemoveTransform(Transform trackerTransform) {
            if (trackerTransform != null) {
                DestroyImmediate(trackerTransform.gameObject, true);
            }
            return null;
        }
    }
}