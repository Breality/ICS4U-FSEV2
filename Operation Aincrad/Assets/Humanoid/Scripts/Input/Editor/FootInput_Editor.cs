using UnityEngine;
using UnityEditor;

namespace Passer {
    [CustomEditor(typeof(FootInput))]
    public class FootInput_Editor : Input_Editor {
        private FootInput footInput;
        private SerializedProperty footTargetProp;

        public void OnEnable() {
            if (serializedObject != null)
                serializedObject.Update();

            footInput = (FootInput)target;

            footTargetProp = serializedObject.FindProperty("footTarget");
        }

        public void OnDisable() {
            if (serializedObject != null)
                serializedObject.Update();
            footTargetProp = serializedObject.FindProperty("footTarget");
        }

        private FootTarget GetFootTarget(HandInput settings) {
            if (settings == null)
                return null;
            FootTarget handTarget = settings.GetComponent<FootTarget>();
            if (handTarget == null) {
                handTarget = settings.transform.parent.GetComponent<FootTarget>();
            }
            return handTarget;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DetermineFootTarget();
            if (footInput.footTarget != null)
                Init(footInput.footTarget.humanoid);

            SetEventInput();

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty headTargetProp;
        private void DetermineFootTarget() {
            footInput.footTarget = footInput.GetComponent<FootTarget>();
            if (footInput.footTarget == null) {
                footTargetProp.objectReferenceValue = (FootTarget)EditorGUILayout.ObjectField("Foot Target", footTargetProp.objectReferenceValue, typeof(FootTarget), true);
                if (footTargetProp.objectReferenceValue == null)
                    footTargetProp.objectReferenceValue = footInput.transform.parent.GetComponent<FootTarget>();
                footInput.footTarget = (FootTarget)footTargetProp.objectReferenceValue;
            }
        }

        #region Events
        private bool showEvents = true;
        private void SetEventInput() {
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            if (showEvents) {
                EditorGUI.indentLevel++;
                SetEventInput(serializedObject.FindProperty("hitGround"), footInput.hitGround, "Hit Ground", footInput.footTarget);
                EditorGUI.indentLevel--;
            }
        }

        private void SetEventInput(SerializedProperty inputProp, InputEvent input, string label, FootTarget footTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref input, footTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }

        private new static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType) {
            string[] values = System.Enum.GetNames(typeof(FootInput.InputType));
            return InputTypePopup(inputType, values);
        }

        #endregion
    }
}
