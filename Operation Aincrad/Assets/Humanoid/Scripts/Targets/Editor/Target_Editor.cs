using UnityEditor;
using UnityEngine;

namespace Passer {

    public static class Target_Editor {
        //public static void Init(Target target) {
        //    Transform defaultTargetTransform = target.GetDefaultTarget();
        //    Transform targetTransform = (target.transform != null) ? target.transform : defaultTargetTransform;
        //}

        public static Target Inspector(Target target, string name) {
            if (target == null)
                return target;

            EditorGUILayout.BeginHorizontal();
            Transform defaultTargetTransform = null; // target.GetDefaultTarget(target.humanoid);
            Transform targetTransform = target.transform ?? defaultTargetTransform;
            targetTransform = (Transform)EditorGUILayout.ObjectField(name, targetTransform, typeof(Transform), true);

            if (!Application.isPlaying) {
                if (targetTransform == defaultTargetTransform && GUILayout.Button("Show", GUILayout.MaxWidth(60))) {
                    // Call static method CreateTarget on target
                    target = (HumanoidTarget)target.GetType().GetMethod("CreateTarget").Invoke(null, new object[] { target });
                    //} else if (targetTransform != target.transform) {
                    //    target = (HumanoidTarget)target.GetType().GetMethod("SetTarget").Invoke(null, new object[] { target.humanoid, targetTransform });
                }
            }
            EditorGUILayout.EndHorizontal();
            return target;
        }

        public static HumanoidTarget Inspector(HumanoidTarget target, string name) {
            if (target == null)
                return target;

            EditorGUILayout.BeginHorizontal();
            Transform defaultTargetTransform = target.GetDefaultTarget(target.humanoid);
            Transform targetTransform = target.transform ?? defaultTargetTransform;

            GUIContent text = new GUIContent(
                name,
                "The transform controlling the " + name
                );
            targetTransform = (Transform)EditorGUILayout.ObjectField(text, targetTransform, typeof(Transform), true);

            if (!Application.isPlaying) {
                if (targetTransform == defaultTargetTransform && GUILayout.Button("Show", GUILayout.MaxWidth(60))) {
                    // Call static method CreateTarget on target
                    target = (HumanoidTarget)target.GetType().GetMethod("CreateTarget").Invoke(null, new object[] { target });
                } else if (targetTransform != target.transform) {
                    target = (HumanoidTarget)target.GetType().GetMethod("SetTarget").Invoke(null, new object[] { target.humanoid, targetTransform });
                }
            }
            EditorGUILayout.EndHorizontal();
            return target;
        }

        public static bool ControllerInspector(UnitySensor controller) {
            EditorGUILayout.BeginHorizontal();

            controller.enabled = EditorGUILayout.ToggleLeft(controller.name, controller.enabled, GUILayout.MinWidth(80));
            if (controller.enabled && Application.isPlaying)
                EditorGUILayout.EnumPopup(controller.status);

            EditorGUILayout.EndHorizontal();
            return controller.enabled;
        }

        public static bool ControllerInspector(UnitySensor controller, Target target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.ToggleLeft(controller.name, controller.enabled, GUILayout.MinWidth(80));
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, enabled ? "Enabled " : "Disabled " + controller.name);
                controller.enabled = enabled;
            }
            if (Application.isPlaying && enabled)
                EditorGUILayout.EnumPopup(controller.status);
            EditorGUILayout.EndHorizontal();
            return enabled;
        }

        public static void BoneAngleInspector(SerializedProperty minProperty, SerializedProperty maxProperty, float defaultMin, float defaultMax, string boneAxisName, string label) {
            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label, GUILayout.MinWidth(70));

            min = EditorGUILayout.FloatField(min, GUILayout.Width(65));
            GUI.SetNextControlName(boneAxisName + "1");
            EditorGUILayout.MinMaxSlider(ref min, ref max, -180, 180);
            GUI.SetNextControlName(boneAxisName + "2");
            max = EditorGUILayout.FloatField(max, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                min = defaultMin;
                max = defaultMax;
                GUI.FocusControl(boneAxisName + "1");
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) {
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
            }

            SceneView.RepaintAll();
        }

        public static void BoneXAngleInspector(HumanoidTarget.TargetedBone bone, float defaultMin, float defaultMax) {
            float oldMin = bone.bone.minAngles.x;
            float oldMax = bone.bone.maxAngles.x;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("X Limits", GUILayout.MinWidth(70));

            GUI.SetNextControlName(bone.bone.transform.name + "X1");
            bone.bone.minAngles.x = EditorGUILayout.FloatField(bone.bone.minAngles.x, GUILayout.Width(65));

            EditorGUILayout.MinMaxSlider(ref bone.bone.minAngles.x, ref bone.bone.maxAngles.x, -180, 180);

            GUI.SetNextControlName(bone.bone.transform.name + "X2");
            bone.bone.maxAngles.x = EditorGUILayout.FloatField(bone.bone.maxAngles.x, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                bone.bone.minAngles.x = defaultMin;
                bone.bone.maxAngles.x = defaultMax;
                GUI.FocusControl(bone.bone.transform.name + "X1");
            }

            EditorGUILayout.EndHorizontal();

            if (bone.bone.maxAngles.x != oldMax || bone.bone.minAngles.x != oldMin)
                SceneView.RepaintAll();
        }

        public static void BoneYAngleInspector(HumanoidTarget.TargetedBone bone, float defaultMin, float defaultMax) {
            float oldMin = bone.bone.minAngles.y;
            float oldMax = bone.bone.maxAngles.y;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Y Limits", GUILayout.MinWidth(70));

            GUI.SetNextControlName(bone.bone.transform.name + "Y1");
            bone.bone.minAngles.y = EditorGUILayout.FloatField(bone.bone.minAngles.y, GUILayout.Width(65));

            EditorGUILayout.MinMaxSlider(ref bone.bone.minAngles.y, ref bone.bone.maxAngles.y, -180, 180);

            GUI.SetNextControlName(bone.bone.transform.name + "Y2");
            bone.bone.maxAngles.y = EditorGUILayout.FloatField(bone.bone.maxAngles.y, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                bone.bone.minAngles.y = defaultMin;
                bone.bone.maxAngles.y = defaultMax;
                GUI.FocusControl(bone.bone.transform.name + "Y1");

            }

            EditorGUILayout.EndHorizontal();

            if (bone.bone.maxAngles.y != oldMax || bone.bone.minAngles.y != oldMin)
                SceneView.RepaintAll();
        }

        public static void BoneYAngleInspector(HumanoidTarget.TargetedBone bone, SerializedProperty minProperty, SerializedProperty maxProperty, float defaultMin, float defaultMax) {
            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Limits Y", GUILayout.MinWidth(70));

            min = EditorGUILayout.FloatField(min, GUILayout.Width(65));
            GUI.SetNextControlName(bone.bone.transform.name + "Y1");
            EditorGUILayout.MinMaxSlider(ref min, ref max, -180, 180);
            GUI.SetNextControlName(bone.bone.transform.name + "Y2");
            max = EditorGUILayout.FloatField(max, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                min = defaultMin;
                max = defaultMax;
                GUI.FocusControl(bone.bone.transform.name + "Y1");
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) {
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
                SceneView.RepaintAll();
            }
        }

        public static void BoneZAngleInspector(HumanoidTarget.TargetedBone bone, float defaultMin, float defaultMax) {
            float oldMin = bone.bone.minAngles.z;
            float oldMax = bone.bone.maxAngles.z;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Z Limits", GUILayout.MinWidth(70));

            GUI.SetNextControlName(bone.bone.transform.name + "Z1");
            bone.bone.minAngles.z = EditorGUILayout.FloatField(bone.bone.minAngles.z, GUILayout.Width(65));

            EditorGUILayout.MinMaxSlider(ref bone.bone.minAngles.z, ref bone.bone.maxAngles.z, -180, 180);

            GUI.SetNextControlName(bone.bone.transform.name + "Z2");
            bone.bone.maxAngles.z = EditorGUILayout.FloatField(bone.bone.maxAngles.z, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                bone.bone.minAngles.z = defaultMin;
                bone.bone.maxAngles.z = defaultMax;
                GUI.FocusControl(bone.bone.transform.name + "Z1");

            }

            EditorGUILayout.EndHorizontal();

            if (bone.bone.maxAngles.y != oldMax || bone.bone.minAngles.y != oldMin)
                SceneView.RepaintAll();
        }

        public static void BoneZAngleInspector(HumanoidTarget.TargetedBone bone, SerializedProperty minProperty, SerializedProperty maxProperty, float defaultMin, float defaultMax) {
            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Limits Z", GUILayout.MinWidth(70));

            min = EditorGUILayout.FloatField(min, GUILayout.Width(65));
            GUI.SetNextControlName(bone.bone.transform.name + "Z1");
            EditorGUILayout.MinMaxSlider(ref min, ref max, -180, 180);
            GUI.SetNextControlName(bone.bone.transform.name + "Z2");
            max = EditorGUILayout.FloatField(max, GUILayout.Width(65));

            if (GUILayout.Button("R")) {
                min = defaultMin;
                max = defaultMax;
                GUI.FocusControl(bone.bone.transform.name + "Z1");
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) {
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
                SceneView.RepaintAll();
            }
        }

        //public static void DrawTargetBone(HumanoidTarget.TargetedBone bone) {
        //    if (bone.target.transform != null) {
        //        Handles.color = Color.white;
        //        Handles.DrawLine(bone.target.transform.position, bone.target.transform.position + bone.target.transform.rotation * bone.target.normalDirection * bone.bone.length);
        //    }
        //}

        #region xArcs
        public static void DrawXArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle) {
            DrawXArcs(position, rotation, minAngle, maxAngle, rotation * Vector3.down);
        }
        public static void DrawXArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 zeroDirection) {
            Vector3 planeNormal = rotation * Vector3.right;
            DrawXArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection);
        }
        public static void DrawXArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 planeNormal, Vector3 zeroDirection) {
            DrawArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection, Color.red);
        }
        #endregion
        #region yArcs
        public static void DrawYArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle) {
            DrawYArcs(position, rotation, minAngle, maxAngle, rotation * Vector3.forward);
        }

        public static void DrawYArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 zeroDirection) {
            Vector3 planeNormal = rotation * Vector3.up;
            DrawYArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection);
        }

        public static void DrawYArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 planeNormal, Vector3 zeroDirection) {
            DrawArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection, Color.green);
        }
        #endregion
        #region zArcs
        public static void DrawZArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle) {
            DrawZArcs(position, rotation, minAngle, maxAngle, rotation * Vector3.down);
        }

        public static void DrawZArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 zeroDirection) {
            Vector3 planeNormal = rotation * Vector3.forward;
            DrawZArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection);
        }

        public static void DrawZArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 planeNormal, Vector3 zeroDirection) {
            DrawArcs(position, rotation, minAngle, maxAngle, planeNormal, zeroDirection, Color.blue);
        }
        #endregion

        public static void DrawArcs(Vector3 position, Quaternion rotation, float minAngle, float maxAngle, Vector3 planeNormal, Vector3 zeroDirection, Color color) {
            color.a = 0.1F;
            Handles.color = color;
            Handles.DrawSolidArc(position, planeNormal, zeroDirection, minAngle, HandleUtility.GetHandleSize(position));
            Handles.DrawSolidArc(position, planeNormal, zeroDirection, maxAngle, HandleUtility.GetHandleSize(position));

            color.a = 1;
            Handles.color = color;
            Handles.DrawWireArc(position, planeNormal, zeroDirection, minAngle, HandleUtility.GetHandleSize(position));
            Handles.DrawWireArc(position, planeNormal, zeroDirection, maxAngle, HandleUtility.GetHandleSize(position));

            Handles.DrawLine(position, position + zeroDirection * HandleUtility.GetHandleSize(position));
        }

        #region MinMaxSlider

    }

    public class MinMaxSliderAttribute : PropertyAttribute {
        public readonly string name;
        public readonly float defaultMin;
        public readonly float defaultMax;
        public readonly float minLimit;
        public readonly float maxLimit;

        public MinMaxSliderAttribute(string _name, float _defaultMin, float _defaultMax, float _minLimit, float _maxLimit) {
            name = _name;
            defaultMin = _defaultMin;
            defaultMax = _defaultMax;
            minLimit = _minLimit;
            maxLimit = _maxLimit;
        }
    }

    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    class MinMaxSliderDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType == SerializedPropertyType.Vector2) {
                Vector2 range = property.vector2Value;
                float min = range.x;
                float max = range.y;
                MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(label, GUILayout.MinWidth(70));

                min = EditorGUILayout.FloatField(min, GUILayout.Width(65));
                GUI.SetNextControlName(attr.name + "1");
                EditorGUI.MinMaxSlider(position, label, ref min, ref max, attr.minLimit, attr.maxLimit);
                GUI.SetNextControlName(attr.name + "2");
                max = EditorGUILayout.FloatField(max, GUILayout.Width(65));

                if (GUILayout.Button("R")) {
                    min = attr.defaultMin;
                    max = attr.defaultMax;
                    GUI.FocusControl(attr.name + "Y1");
                }

                if (EditorGUI.EndChangeCheck()) {
                    range.x = min;
                    range.y = max;
                    property.vector2Value = range;
                }

                EditorGUILayout.EndHorizontal();
            } else {
                EditorGUI.LabelField(position, label, "Use only with Vector2");
            }
        }
    }
    #endregion
}
