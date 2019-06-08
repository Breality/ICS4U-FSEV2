using UnityEditor;
using UnityEngine;

namespace Passer {

    [CustomEditor(typeof(Teleporter))]
    public class Teleporter_Editor : InteractionPointer_Editor {
        protected Teleporter teleporter;

        protected SerializedProperty transportTypeProp;

        #region Enable
        public override void OnEnable() {
            base.OnEnable();
            teleporter = (Teleporter)target;

            teleporter.transformToTeleport = FindDeepParentComponent(teleporter.transform, typeof(HumanoidControl));
            teleporter.clickInput.SetMethod(teleporter.TeleportHumanoid, InputEvent.EventType.Start);

            transportTypeProp = serializedObject.FindProperty("transportType");
        }

        private Transform FindDeepParentComponent(Transform t, System.Type type) {
            Component component = t.GetComponent(type.Name);
            if (component == null) {
                if (t.parent != null)
                    return FindDeepParentComponent(t.parent, type);
                else
                    return null;
            } else
                return t;
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            serializedObject.Update();

            pointer.active = EditorGUILayout.Toggle("Active", pointer.active);
            pointer.timedClick = EditorGUILayout.FloatField("Timed teleport", pointer.timedClick);
            pointer.focusPointObj = (GameObject)EditorGUILayout.ObjectField("Target Point Object", pointer.focusPointObj, typeof(GameObject), true);

            pointerModeProp.intValue = (int)(InteractionPointer.RayType)EditorGUILayout.EnumPopup("Mode", (InteractionPointer.RayType)pointerModeProp.intValue);
            transportTypeProp.intValue = (int)(Teleporter.TransportType)EditorGUILayout.EnumPopup("Transport Type", (Teleporter.TransportType)transportTypeProp.intValue);

            if (pointer.rayType == InteractionPointer.RayType.Bezier) {
                pointer.maxDistance = EditorGUILayout.FloatField("Maximum Distance", pointer.maxDistance);
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
