using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Passer {

    [CustomEditor(typeof(NetworkingSpawner))]
    public class NetworkingSpawner_Editor : NetworkingStarter_Editor {
        private SerializedProperty humanoidPrefabProp;
        private SerializedProperty spawnPointsProp;
        private SerializedProperty spawnMethodProp;

        public override void OnEnable() {
            base.OnEnable();

            humanoidPrefabProp = serializedObject.FindProperty("humanoidPrefab");
            spawnPointsProp = serializedObject.FindProperty("spawnPoints");
            spawnMethodProp = serializedObject.FindProperty("spawnMethod");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUIContent label;

            label = new GUIContent(
                "Humanoid Prefab",
                "The Humanoid Prefab that will be spawned when networking has started"
                );
            EditorGUILayout.PropertyField(humanoidPrefabProp, label);

            label = new GUIContent(
                "Spawn Points",
                "The possible spawning points for the Humanoid"
                );
            EditorGUILayout.PropertyField(spawnPointsProp, label, true);

            label = new GUIContent(
                "Spawn Mode",
                "The order in which the spawn points are chosen"
                );
            EditorGUILayout.PropertyField(spawnMethodProp, label);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
