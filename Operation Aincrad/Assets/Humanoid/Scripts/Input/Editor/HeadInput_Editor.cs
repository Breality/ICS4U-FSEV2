using UnityEngine;
using UnityEditor;

namespace Passer {
    [CustomEditor(typeof(HeadInput))]
    public class HeadInput_Editor : Input_Editor {
        private HeadInput headInput;

        #region Enable
        public void OnEnable() {
            if (serializedObject != null)
                serializedObject.Update();

            headInput = (HeadInput)target;

            headTargetProp = serializedObject.FindProperty("headTarget");
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DetermineHeadTarget();
            if (headInput.headTarget != null)
                Init(headInput.headTarget.humanoid);

            SetEventInput();
        }

        private SerializedProperty headTargetProp;
        private void DetermineHeadTarget() {
            if (headInput.headTarget != null)
                return;

            headInput.headTarget = headInput.GetComponent<HeadTarget>();
            if (headInput.headTarget == null) {
                headTargetProp.objectReferenceValue = (HeadTarget)EditorGUILayout.ObjectField("Head Target", headTargetProp.objectReferenceValue, typeof(HeadTarget), true);
                if (headTargetProp.objectReferenceValue == null && headInput.transform.parent != null)
                    headTargetProp.objectReferenceValue = headInput.transform.parent.GetComponent<HeadTarget>();
                headInput.headTarget = (HeadTarget)headTargetProp.objectReferenceValue;
            }
        }

        #region Events
        private bool showEvents = true;
        private void SetEventInput() {
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            EditorGUI.indentLevel++;
            if (showEvents) {
                SetAudioInput(serializedObject.FindProperty("audioInput"), headInput.audioInput, headInput.headTarget);
#if hFACE
                SetBlinkInput(serializedObject.FindProperty("blinkInput"), headInput.blinkInput, headInput.headTarget);
#endif
            }
            EditorGUI.indentLevel--;
        }

        private void SetAudioInput(SerializedProperty inputProp, InputEvent audioInput, HeadTarget headTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Audio", GUILayout.Width(80));
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref audioInput, headTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.Slider("Audio Trigger Level", headInput.audioLevel, 0, 1);
            EditorGUI.indentLevel++;
        }

#if hFACE
        private void SetBlinkInput(SerializedProperty inputProp, InputEvent blinkInput, HeadTarget headTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Blink", GUILayout.Width(80));
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref blinkInput, headTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }
#endif

        private new static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType) {
            string[] values = System.Enum.GetNames(typeof(HeadInput.InputType));
            return InputTypePopup(inputType, values);
        }
        #endregion
        #endregion
    }
}