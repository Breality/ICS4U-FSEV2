using UnityEditor;
using UnityEngine;

namespace Passer {

    [CustomEditor(typeof(InteractionPointer))]
    public class InteractionPointer_Editor : Input_Editor {
        protected InteractionPointer pointer;

        protected SerializedProperty timedClickProp;
        protected SerializedProperty pointerModeProp;
        protected SerializedProperty maxDistanceProp;
        protected SerializedProperty resolutionProp;
        protected SerializedProperty speedProp;
        protected SerializedProperty radiusProp;

        #region Enable
        public virtual void OnEnable() {
            pointer = (InteractionPointer)target;

            timedClickProp = serializedObject.FindProperty("timedClick");
            pointerModeProp = serializedObject.FindProperty("rayType");
            maxDistanceProp = serializedObject.FindProperty("maxDistance");
            resolutionProp = serializedObject.FindProperty("resolution");
            speedProp = serializedObject.FindProperty("speed");
            radiusProp = serializedObject.FindProperty("radius");
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            serializedObject.Update();

            pointer.active = EditorGUILayout.Toggle("Active", pointer.active);
            timedClickProp.floatValue = EditorGUILayout.FloatField("Timed Click", timedClickProp.floatValue);
            pointer.focusPointObj = (GameObject) EditorGUILayout.ObjectField("Focus Point Object", pointer.focusPointObj, typeof(GameObject), true);

            EditorGUILayout.ObjectField("Object in Focus", pointer.objectInFocus, typeof(GameObject), true);

            pointerModeProp.intValue = (int)(InteractionPointer.RayType)EditorGUILayout.EnumPopup("Mode", (InteractionPointer.RayType) pointerModeProp.intValue);

            if (pointer.rayType == InteractionPointer.RayType.Bezier || pointer.rayType == InteractionPointer.RayType.Gravity) {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                maxDistanceProp.floatValue = EditorGUILayout.FloatField("Maximum Distance", maxDistanceProp.floatValue);
                resolutionProp.floatValue = EditorGUILayout.FloatField("Resolution", resolutionProp.floatValue);
                EditorGUI.EndDisabledGroup();
            }
            if (pointer.rayType == InteractionPointer.RayType.Gravity) {
                speedProp.floatValue = EditorGUILayout.FloatField("Speed", speedProp.floatValue);
            } else if (pointer.rayType == InteractionPointer.RayType.SphereCast) {
                maxDistanceProp.floatValue = EditorGUILayout.FloatField("Maximum Distance", maxDistanceProp.floatValue);
                radiusProp.floatValue = EditorGUILayout.FloatField("Radius", radiusProp.floatValue);
            }

            InputEventInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private bool showEvents = true;
        private void InputEventInspector() {
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            EditorGUI.indentLevel++;
            if (showEvents) {
                InputInspector(serializedObject.FindProperty("activeInput"), "Active", pointer.activeInput);
                InputInspector(serializedObject.FindProperty("clickInput"), "Click", pointer.clickInput);
                InputInspector(serializedObject.FindProperty("focusInput"), "Focus", pointer.focusInput);
            }
            EditorGUI.indentLevel--;
        }

        private void InputInspector(SerializedProperty inputProp, string label, InputEvent input) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            EditorGUI.indentLevel--;
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref input, pointer.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}