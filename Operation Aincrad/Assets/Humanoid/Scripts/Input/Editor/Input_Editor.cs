using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Passer {
    public class Input_Editor : Editor {
        public static AnimatorControllerParameter[] animatorParameters;
        public static string[] animatorParameterNames;

#if PLAYMAKER
        public static HutongGames.PlayMaker.FsmEvent[] playmakerEvents;
        public static string[] playmakerEventNames;
#endif

        public static void Init(HumanoidControl humanoid) {
            GetAnimatorParameters(humanoid);
#if PLAYMAKER
            GetPlaymakerEvents(humanoid);
#endif
        }

        public static void GetAnimatorParameters(HumanoidControl humanoid) {
            if (humanoid == null || humanoid.targetsRig.runtimeAnimatorController == null)
                return;

            animatorParameters = humanoid.targetsRig.parameters;
            animatorParameterNames = new string[animatorParameters.Length + 1];
            animatorParameterNames[0] = " ";
            for (int i = 0; i < animatorParameters.Length; i++)
                animatorParameterNames[i + 1] = animatorParameters[i].name;
        }

#if PLAYMAKER
        private static void GetPlaymakerEvents(HumanoidControl humanoid) {
            PlayMakerFSM fsm = humanoid.GetComponent<PlayMakerFSM>();
            if (fsm == null) {
                playmakerEvents = null;
                return;
            }

            playmakerEvents = fsm.FsmEvents;

            playmakerEventNames = new string[playmakerEvents.Length + 1];
            playmakerEventNames[0] = " ";
            for (int i = 0; i < playmakerEvents.Length; i++)
                playmakerEventNames[i + 1] = playmakerEvents[i].Name;
        }
#endif

        public delegate GameObject GetTargetGameObjectF(InputEvent.InputType inputType, GameObject gameObject);
        public delegate InputEvent.InputType SetInputTypeF(InputEvent.InputType inputType);


        public static void SetInput(SerializedProperty inputProp, GetTargetGameObjectF GetGameObject, SetInputTypeF SetInputType, ref InputEvent input, GameObject gameObject) {
            SetInputEventType(ref input);

            if (input.eventType == InputEvent.EventType.None)
                return;

            SerializedProperty targetGameObjectProp = inputProp.FindPropertyRelative("targetGameObject");
            SerializedProperty typeProp = inputProp.FindPropertyRelative("type");

            if (input.targetGameObject == null)
                input.type = 0;

            InputEvent.InputType inputType = (InputEvent.InputType)input.type;
            if (!(inputType == InputEvent.InputType.GameObject && input.targetGameObject != null)) {
                InputEvent.InputType newType = SetInputType(inputType);
                if (newType != inputType) {
                    input.methodIndex = 0;
                    input.methodName = "";
                    input.animatorParameterIndex = 0;
                }

                switch (newType) {
                    case InputEvent.InputType.None:
                        break;
#if PLAYMAKER
                    case InputEvent.InputType.Playmaker:
#endif
                    case InputEvent.InputType.Animator:
                    case InputEvent.InputType.GameObject:
                        input.targetGameObject = gameObject;
                        break;
                    default:
                        input.targetGameObject = GetGameObject(newType, gameObject);
                        newType = InputEvent.InputType.GameObject;
                        break;
                }
                input.type = (int)newType;
            }

            SetInput(inputProp, ref input);

            typeProp.intValue = input.type;
            targetGameObjectProp.objectReferenceValue = input.targetGameObject;
        }

        private static void SetInputEventType(ref InputEvent input) {
            string[] values = System.Enum.GetNames(typeof(InputEvent.EventType));
            values[0] = " ";

            if (input.eventType == 0)
                input.eventType = (InputEvent.EventType)EditorGUILayout.Popup((int)input.eventType, values);
            else
                input.eventType = (InputEvent.EventType)EditorGUILayout.Popup((int)input.eventType, values, GUILayout.Width(60));
        }

        public static void SetInputFunction(SerializedProperty inputProp, ref InputEvent input, GameObject gameObject, GetTargetGameObjectF GetGameObject = null, SetInputTypeF SetInputType = null) {
            if (GetGameObject == null)
                GetGameObject = GetTargetGameObject;
            if (SetInputType == null)
                SetInputType = InputTypePopup;

            SerializedProperty targetGameObjectProp = inputProp.FindPropertyRelative("targetGameObject");
            SerializedProperty typeProp = inputProp.FindPropertyRelative("type");

            if (input.targetGameObject == null)
                input.type = 0;

            InputEvent.InputType inputType = (InputEvent.InputType)input.type;
            if (!(inputType == InputEvent.InputType.GameObject && input.targetGameObject != null)) {
                InputEvent.InputType newType = SetInputType(inputType);
                if (newType != inputType) {
                    input.methodIndex = 0;
                    input.methodName = "";
                    input.animatorParameterIndex = 0;
                }

                switch (newType) {
                    case InputEvent.InputType.None:
                        break;
#if PLAYMAKER
                    case InputEvent.InputType.Playmaker:
#endif
                    case InputEvent.InputType.Animator:
                    case InputEvent.InputType.GameObject:
                        input.targetGameObject = gameObject;
                        break;
                    default:
                        input.targetGameObject = GetGameObject(newType, gameObject);
                        newType = InputEvent.InputType.GameObject;
                        break;
                }
                input.type = (int)newType;
            }

            SetInput(inputProp, ref input);

            typeProp.intValue = input.type;
            targetGameObjectProp.objectReferenceValue = input.targetGameObject;
        }

        public static GameObject GetTargetGameObject(InputEvent.InputType inputType, GameObject gameObject) {
            return gameObject;
        }

        public static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType) {
            string[] values = System.Enum.GetNames(inputType.GetType());
            return InputTypePopup(inputType, values);
        }

        public static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType, string[] values) {
            values[0] = " ";

            InputEvent.InputType newType;
            if (inputType == 0)
                newType = (InputEvent.InputType)EditorGUILayout.Popup((int)inputType, values);
            else
                newType = (InputEvent.InputType)EditorGUILayout.Popup((int)inputType, values, GUILayout.Width(150));
            return newType;
        }

        public static void SetInput(SerializedProperty inputProp, ref InputEvent input) {
            switch (input.type) {
                case (int)InputEvent.InputType.Animator:
                    SetAnimatorInput(ref input);
                    break;
#if PLAYMAKER
                case (int)InputEvent.InputType.Playmaker:
                    SetPlayMakerInput(ref input);
                    break;
#endif
                case (int)InputEvent.InputType.GameObject:
                    SetGameObjectInput(inputProp, ref input);
                    break;
                default:
                    break;
            }
        }
        public static void SetAnimatorInput(ref InputEvent input) {
            if (animatorParameterNames != null && animatorParameterNames.Length > 0) {

                input.animatorParameterIndex = EditorGUILayout.Popup(input.animatorParameterIndex, animatorParameterNames, GUILayout.MinWidth(80));

                if (animatorParameters != null && input.animatorParameterIndex > 0 && input.animatorParameterIndex <= animatorParameters.Length) {
                    input.animatorParameterName = animatorParameters[input.animatorParameterIndex - 1].name;
                    input.animatorParameterType = animatorParameters[input.animatorParameterIndex - 1].type;
                } else {
                    input.animatorParameterName = null;
                }
            }
        }

#if PLAYMAKER
        public static void SetPlayMakerInput(ref InputEvent input) {
            if (playmakerEventNames != null && playmakerEventNames.Length > 0) {
                input.playMakerEventIndex = EditorGUILayout.Popup(input.playMakerEventIndex, playmakerEventNames, GUILayout.MinWidth(80));

                if (playmakerEvents != null && input.playMakerEventIndex > 0 && input.playMakerEventIndex <= playmakerEvents.Length) {
                    input.playmakerEventName = playmakerEvents[input.playMakerEventIndex - 1].Name;
                } else {
                    input.playmakerEventName = null;
                }
            }
        }
#endif

        public static void SetGameObjectInput(SerializedProperty inputProp, ref InputEvent input) {
            if (input.targetGameObject == null)
                input.targetGameObject = (GameObject)EditorGUILayout.ObjectField(input.targetGameObject, typeof(GameObject), true, GUILayout.MinWidth(150));
            else
                input.targetGameObject = (GameObject)EditorGUILayout.ObjectField(input.targetGameObject, typeof(GameObject), true, GUILayout.Width(150));

            SetComponentInput(inputProp, ref input);
        }

        public static void SetComponentInput(SerializedProperty inputProp, ref InputEvent input) {
            input.DetermineMethods();

            if (input.objMethods == null || input.objMethods.Length == 0)
                return;

            SerializedProperty methodIndexProp = inputProp.FindPropertyRelative( "methodIndex");
            SerializedProperty methodNameProp = inputProp.FindPropertyRelative( "methodName");
            SerializedProperty targetComponentProp = inputProp.FindPropertyRelative( "targetComponent");
            SerializedProperty enumValProp = inputProp.FindPropertyRelative( "enumVal");

            input.methodIndex = EditorGUILayout.Popup(input.methodIndex, input.objMethodNames, GUILayout.MinWidth(80));

            input.methodParameterCount = 0;
            if (input.methodIndex <= 0 || input.methodIndex > input.objMethods.Length)
                input.targetMethod = null;
            else {
                input.targetMethod = input.objMethods[input.methodIndex - 1];
                input.SetMethod(input.targetComponent, input.targetMethod);
                input.methodName = input.targetMethod.Name;
                input.DetermineMethodProperties();
                input.targetComponent = input.objMethodComponents[input.methodIndex - 1];
                if (input.enumParameter) {
                    string[] enumValues = input.GetEnumMethodValues();
                    input.enumVal = EditorGUILayout.Popup(input.enumVal, enumValues);
                }
            }

            methodIndexProp.intValue = input.methodIndex;
            methodNameProp.stringValue = input.methodName;
            targetComponentProp.objectReferenceValue = input.targetComponent;
            enumValProp.intValue = input.enumVal;
        }

        //#region Input Action

        //public static void SetInputAction(Component component, InputEvent inputEvent, SerializedProperty inputActionProp) {
        //    MemberInfo[] members;
        //    string[] inputActionNames = GetInputActionNames(component.GetType(), out members);
        //    int inputActionIndex = inputEvent.inputAction != null ? FindInputAction(inputActionNames, inputEvent.inputAction.name) : 0;
        //    int newInputActionIndex = EditorGUILayout.Popup(inputActionIndex, inputActionNames, GUILayout.MinWidth(80));
        //    if (newInputActionIndex != inputActionIndex) {
        //        MemberInfo member = members[newInputActionIndex];
        //        if (member.MemberType == MemberTypes.Property) {
        //            PropertyInfo property = (PropertyInfo)member;
        //            inputEvent.inputAction = InputEvent.InputAction.Create(property.PropertyType);
        //        }
        //        //else if (member.MemberType == MemberTypes.Method) {
        //        //    MethodInfo method = (MethodInfo)member;
        //        //}
        //    }
        //    if (inputActionProp != null)
        //        inputActionProp.FindPropertyRelative("name").stringValue = inputActionNames[inputActionIndex];

        //    if (inputEvent.inputAction != null)
        //        inputEvent.inputAction.SetComponent();
        //}

        //private static string[] GetInputActionNames(System.Type componentType, out MemberInfo[] members) {
        //    PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //    MethodInfo[] methods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        //    int total = properties.Length + methods.Length;

        //    string[] inputActionNames = new string[total];
        //    members = new MemberInfo[total];
        //    for (int i = 0; i < total; i++) {
        //        if (i < properties.Length) {
        //            inputActionNames[i] = properties[i].Name;
        //            members[i] = properties[i];
        //        }
        //        else {
        //            inputActionNames[i] = methods[i - properties.Length].Name;
        //            members[i] = methods[i - properties.Length];
        //        }
        //    }
        //    return inputActionNames;
        //}

        //private static int FindInputAction(string[] inputActionNames, string inputActionName) {
        //    for (int i = 0; i < inputActionNames.Length; i++)
        //        if (inputActionNames[i] == inputActionName)
        //            return i;
        //    return -1;
        //}


        //public class InputActionInt : InputEvent.InputAction {
        //    public int value;

        //    public override void SetComponent() {
        //        value = EditorGUILayout.IntField(value);
        //    }
        //}

        //public class InputActionFloat : InputEvent.InputAction {
        //    public float value;

        //    public override void SetComponent() {
        //        value = EditorGUILayout.FloatField(value);
        //    }
        //}

        //public class InputActionEnum : InputEvent.InputAction {
        //    public int value;
        //    public System.Type enumType;
        //    private string[] enumNames;

        //    public override void SetComponent() {
        //        if (enumNames == null)
        //            enumNames = System.Enum.GetNames(enumType);
        //        value = EditorGUILayout.Popup(value, enumNames);
        //    }
        //}

        //public class InputActionMethod : InputEvent.InputAction {
        //    public override void SetComponent() {
        //    }
        //}
        //#endregion  

        #region Utilities
        public static void AddComponentName(string componentName, ref string[] methodNames) {
            string addText = componentName + ".";
            for (int i = 0; i < methodNames.Length; i++) {
                if (methodNames[i] != " ")
                    methodNames[i] = addText + methodNames[i];
            }
        }

        public static string[] Extend(string[] a1, string[] a2) {
            if (a1.Length > 0) {
                string[] r = new string[a1.Length + a2.Length - 1];
                // prevent copying elm 0 from a2 (this is the empty name)
                a2.CopyTo(r, a1.Length - 1);
                a1.CopyTo(r, 0);
                return r;
            } else {
                return a2;
            }
        }

        public static MethodInfo[] Extend(MethodInfo[] a1, MethodInfo[] a2) {
            MethodInfo[] r = new MethodInfo[a1.Length + a2.Length];
            a1.CopyTo(r, 0);
            a2.CopyTo(r, a1.Length);
            return r;
        }
        #endregion
    }
}
