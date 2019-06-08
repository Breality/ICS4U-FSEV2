using System;
using UnityEngine;

namespace UnityEditor.EventSystems {
    [CustomEditor(typeof(RigidbodyLimitations), true)]
    public class RigidbodyLimitations_Editor : Editor {
        RigidbodyLimitations rbLimitations;

        SerializedProperty limitXProp;
        SerializedProperty limitYProp;
        SerializedProperty limitZProp;

        SerializedProperty basePositionProp;
        SerializedProperty minLocalPositionProp;
        SerializedProperty maxLocalPositionProp;

        SerializedProperty limitAngleProp;
        SerializedProperty maxLocalAngleProp;
        SerializedProperty limitAxisProp;

        SerializedProperty m_DelegatesProperty;

        GUIContent m_IconToolbarMinus;
        GUIContent m_EventIDName;
        GUIContent[] m_EventTypes;
        GUIContent m_AddButonContent;

        #region Enable
        protected virtual void OnEnable() {
            rbLimitations = (RigidbodyLimitations) target;

            limitXProp = serializedObject.FindProperty("limitX");
            limitYProp = serializedObject.FindProperty("limitY");
            limitZProp = serializedObject.FindProperty("limitZ");

            basePositionProp = serializedObject.FindProperty("basePosition");
            minLocalPositionProp = serializedObject.FindProperty("minLocalPosition");
            maxLocalPositionProp = serializedObject.FindProperty("maxLocalPosition");
            m_DelegatesProperty = serializedObject.FindProperty("m_Delegates");

            limitAngleProp = serializedObject.FindProperty("limitAngle");
            maxLocalAngleProp = serializedObject.FindProperty("maxLocalAngle");
            limitAxisProp = serializedObject.FindProperty("limitAngleAxis");

            m_AddButonContent = new GUIContent("Add New Event Type");
            m_EventIDName = new GUIContent("");

            // Have to create a copy since otherwise the tooltip will be overwritten.
            m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus")) {
                tooltip = "Remove all events in this list."
            };

            string[] eventNames = Enum.GetNames(typeof(RigidbodyLimitations.EventTriggerType));
            m_EventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i) {
                m_EventTypes[i] = new GUIContent(eventNames[i]);
            }
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            Rigidbody rb = rbLimitations.GetComponent<Rigidbody>();
            if (rb == null || !rb.isKinematic)
                EditorGUILayout.HelpBox("Rigidbody Limitations should be used with a Kinematic Rigidbody", MessageType.Warning);
            
            serializedObject.Update();

            Vector3 minLimits = minLocalPositionProp.vector3Value;
            Vector3 maxLimits = maxLocalPositionProp.vector3Value;

            EditorGUILayout.BeginHorizontal();
            limitXProp.boolValue = EditorGUILayout.ToggleLeft("Limit Position X", limitXProp.boolValue, GUILayout.MinWidth(110));
            EditorGUI.BeginDisabledGroup(!limitXProp.boolValue);
            EditorGUILayout.LabelField("Min", GUILayout.Width(30));
            float minX = EditorGUILayout.FloatField(minLimits.x);
            EditorGUILayout.LabelField("Max", GUILayout.Width(30));
            float maxX = EditorGUILayout.FloatField(maxLimits.x);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            limitYProp.boolValue = EditorGUILayout.ToggleLeft("Limit Position Y", limitYProp.boolValue, GUILayout.MinWidth(110));
            EditorGUI.BeginDisabledGroup(!limitYProp.boolValue);
            EditorGUILayout.LabelField("Min", GUILayout.Width(30));
            float minY = EditorGUILayout.FloatField(minLimits.y);
            EditorGUILayout.LabelField("Max", GUILayout.Width(30));
            float maxY = EditorGUILayout.FloatField(maxLimits.y);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            limitZProp.boolValue = EditorGUILayout.ToggleLeft("Limit Position Z", limitZProp.boolValue, GUILayout.MinWidth(110));
            EditorGUI.BeginDisabledGroup(!limitZProp.boolValue);
            EditorGUILayout.LabelField("Min", GUILayout.Width(30));
            float minZ = EditorGUILayout.FloatField(minLimits.z);
            EditorGUILayout.LabelField("Max", GUILayout.Width(30));
            float maxZ = EditorGUILayout.FloatField(maxLimits.z);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            minLocalPositionProp.vector3Value = new Vector3(minX, minY, minZ);
            maxLocalPositionProp.vector3Value = new Vector3(maxX, maxY, maxZ);

            EditorGUILayout.BeginHorizontal();
            limitAngleProp.boolValue = EditorGUILayout.ToggleLeft("Limit Angle", limitAngleProp.boolValue, GUILayout.MinWidth(110));
            EditorGUI.BeginDisabledGroup(!limitAngleProp.boolValue);
            EditorGUILayout.LabelField("Angle", GUILayout.Width(40));
            maxLocalAngleProp.floatValue = EditorGUILayout.FloatField(maxLocalAngleProp.floatValue, GUILayout.Width(40));
            EditorGUILayout.LabelField("Axis", GUILayout.Width(40));
            limitAxisProp.vector3Value = EditorGUILayout.Vector3Field("", limitAxisProp.vector3Value, GUILayout.MinWidth(110));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();


            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

            for (int i = 0; i < m_DelegatesProperty.arraySize; ++i) {
                SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);
                SerializedProperty eventProperty = delegateProperty.FindPropertyRelative("eventID");
                SerializedProperty callbacksProperty = delegateProperty.FindPropertyRelative("callback");
                m_EventIDName.text = eventProperty.enumDisplayNames[eventProperty.enumValueIndex];

                EditorGUILayout.PropertyField(callbacksProperty, m_EventIDName);
                Rect callbackRect = GUILayoutUtility.GetLastRect();

                Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none)) {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1) {
                RemoveEntry(toBeRemovedEntry);
            }

            Rect btPosition = GUILayoutUtility.GetRect(m_AddButonContent, GUI.skin.button);
            const float addButonWidth = 200f;
            btPosition.x = btPosition.x + (btPosition.width - addButonWidth) / 2;
            btPosition.width = addButonWidth;
            if (GUI.Button(btPosition, m_AddButonContent)) {
                ShowAddTriggermenu();
            }

            if (!Application.isPlaying) {
                RigidbodyLimitations rbLim = (RigidbodyLimitations)target;
                basePositionProp.vector3Value = rbLim.transform.localPosition;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry) {
            m_DelegatesProperty.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        void ShowAddTriggermenu() {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < m_EventTypes.Length; ++i) {
                bool active = true;

                // Check if we already have a Entry for the current eventType, if so, disable it
                for (int p = 0; p < m_DelegatesProperty.arraySize; ++p) {
                    SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(p);
                    SerializedProperty eventProperty = delegateEntry.FindPropertyRelative("eventID");
                    if (eventProperty.enumValueIndex == i) {
                        active = false;
                    }
                }
                if (active)
                    menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
                else
                    menu.AddDisabledItem(m_EventTypes[i]);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void OnAddNewSelected(object index) {
            int selected = (int)index;

            m_DelegatesProperty.arraySize += 1;
            SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(m_DelegatesProperty.arraySize - 1);
            SerializedProperty eventProperty = delegateEntry.FindPropertyRelative("eventID");
            eventProperty.enumValueIndex = selected;
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Scene
        public void OnSceneGUI() {
            if (rbLimitations == null)
                return;

            if (rbLimitations.limitAngle)
                DrawArc(rbLimitations.transform, rbLimitations.limitAngleAxis, -rbLimitations.maxLocalAngle, rbLimitations.maxLocalAngle);
        }

        private void DrawArc(Transform transform, Vector3 axis, float minAngle, float maxAngle) {
            Vector3 worldAxis = transform.rotation * axis;

            /* Any direction orthogonal to the axis is ok for the zeroDirection,
             * but we choose to have Z when axis is Y or X.
             * All other zeroDirections are derived from that
             */
            Vector3 orthoDirection = Vector3.up;
            float angle = Vector3.Angle(axis, orthoDirection);
            if (angle == 0 || angle == 180)
                orthoDirection = -Vector3.right;
            Vector3 zeroDirection = Vector3.Cross(axis, orthoDirection);
            if (transform.parent != null)
                zeroDirection = transform.parent.rotation * zeroDirection;

            //Vector3 worldZero = transform.rotation * zeroDirection;
            float size = HandleUtility.GetHandleSize(transform.position) * 2;
            Handles.color = Color.yellow;
            Handles.DrawLine(transform.position, transform.position + worldAxis * size);
            Handles.DrawWireArc(transform.position, worldAxis, zeroDirection, minAngle, size);
            Handles.DrawWireArc(transform.position, worldAxis, zeroDirection, maxAngle, size);
            Handles.color = new Color(1, 0.92F, 0.016F, 0.1F); // transparant yellow
            Handles.DrawSolidArc(transform.position, worldAxis, zeroDirection, minAngle, size);
            Handles.DrawSolidArc(transform.position, worldAxis, zeroDirection, maxAngle, size);
        }
        #endregion
    }
}