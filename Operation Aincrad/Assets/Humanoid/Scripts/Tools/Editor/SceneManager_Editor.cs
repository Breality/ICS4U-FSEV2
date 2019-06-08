using UnityEditor;

namespace Passer {

    [CustomEditor(typeof(SceneManager))]
    public class SceneManager_Editor : Editor {
        protected SceneManager sceneManager;

        private SerializedProperty dontDestroyProp;
        private SerializedProperty currentSceneProp;

        public void OnEnable() {
            sceneManager = (SceneManager)target;
            if (sceneManager.sceneNames == null || EditorBuildSettings.scenes.Length != sceneManager.sceneNames.Length)
                sceneManager.sceneNames = new string[EditorBuildSettings.scenes.Length];

            for (int i = 0; i < sceneManager.sceneNames.Length; i++) {
                string scenePath = EditorBuildSettings.scenes[i].path;
                int lastSlash = scenePath.LastIndexOf('/');
                string sceneName = scenePath.Substring(lastSlash + 1);
                sceneName = sceneName.Substring(0, sceneName.Length - 6);
                sceneManager.sceneNames[i] = sceneName;
            }

            currentSceneProp = serializedObject.FindProperty("currentScene");

            dontDestroyProp = serializedObject.FindProperty("dontDestroyOnLoad");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (sceneManager.sceneNames != null) {
                EditorGUILayout.LabelField("Scenes");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.indentLevel++;
                for (int i = 0; i < sceneManager.sceneNames.Length; i++) {
                    EditorGUILayout.TextField("Scene " + i, sceneManager.sceneNames[i]);
                }
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();

            }

            EditorGUILayout.HelpBox("Change the scene list in the File Menu->Build Settings", MessageType.None);

            currentSceneProp.intValue = EditorGUILayout.IntField("Current Scene", currentSceneProp.intValue);

            dontDestroyProp.boolValue = EditorGUILayout.Toggle("Don't Destroy on Load", dontDestroyProp.boolValue);
            serializedObject.ApplyModifiedProperties();
        }
    }
}