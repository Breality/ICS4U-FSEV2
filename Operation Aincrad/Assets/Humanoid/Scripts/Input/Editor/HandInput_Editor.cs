using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Passer {
    [CustomEditor(typeof(HandInput))]
    public class HandInput_Editor : Input_Editor {

        HandInput handInput;

        #region Enable
        private SerializedProperty handTargetProp;
        private SerializedProperty handPoseListProp;

        public void OnEnable() {
            handInput = (HandInput)target;


            if (serializedObject != null)
                serializedObject.Update();
            handTargetProp = serializedObject.FindProperty("handTarget");
            handPoseListProp = serializedObject.FindProperty("handPoseInput");
            CopyFromControllerInputList();

            if (!Application.isPlaying)
                handInput.handTarget = GetHandTarget(handInput);
            if (handInput.handTarget != null)
                Init(handInput.handTarget.humanoid);
            InitHandPoseInputList(handInput);
        }

        private HandTarget GetHandTarget(HandInput handInput) {
            if (handInput == null)
                return null;

            HandTarget handTarget = handInput.GetComponent<HandTarget>();
            if (handTarget == null) {
                handTarget = handInput.transform.parent.GetComponent<HandTarget>();
            }
            return handTarget;
        }
        #endregion

        #region Disable
        public void OnDisable() {
            if (serializedObject != null)
                serializedObject.Update();
            handTargetProp = serializedObject.FindProperty("handTarget");
            handPoseListProp = serializedObject.FindProperty("handPoseInput");
            CopyToControllerInputList();
        }

        private void CopyToControllerInputList() {
            HandInput handInput = (HandInput)target;
            HandTarget handTarget = handInput.handTarget;

            if (handTarget == null)
                return;

            ControllerInput controllerInput = handTarget.humanoid.GetComponent<ControllerInput>();
            if (controllerInput == null)
                return;

            if (handTarget.isLeft) {
                CopyToControllerInput(handInput.controllerInput[0], controllerInput.leftVerticalInput);
                CopyToControllerInput(handInput.controllerInput[1], controllerInput.leftHorizontalInput);
                CopyToControllerInput(handInput.controllerInput[2], controllerInput.leftStickButtonInput);
                CopyToControllerInput(handInput.controllerInput[3], controllerInput.leftStickTouchInput);
                CopyToControllerInput(handInput.controllerInput[4], controllerInput.leftButtonOneInput);
                CopyToControllerInput(handInput.controllerInput[5], controllerInput.leftButtonTwoInput);
                CopyToControllerInput(handInput.controllerInput[6], controllerInput.leftButtonThreeInput);
                CopyToControllerInput(handInput.controllerInput[7], controllerInput.leftButtonFourInput);
                CopyToControllerInput(handInput.controllerInput[8], controllerInput.leftTrigger1Input);
                CopyToControllerInput(handInput.controllerInput[9], controllerInput.leftTrigger2Input);
                CopyToControllerInput(handInput.controllerInput[10], controllerInput.leftOptionInput);
            } else {
                CopyToControllerInput(handInput.controllerInput[0], controllerInput.rightVerticalInput);
                CopyToControllerInput(handInput.controllerInput[1], controllerInput.rightHorizontalInput);
                CopyToControllerInput(handInput.controllerInput[2], controllerInput.rightStickButtonInput);
                CopyToControllerInput(handInput.controllerInput[3], controllerInput.rightStickTouchInput);
                CopyToControllerInput(handInput.controllerInput[4], controllerInput.rightButtonOneInput);
                CopyToControllerInput(handInput.controllerInput[5], controllerInput.rightButtonTwoInput);
                CopyToControllerInput(handInput.controllerInput[6], controllerInput.rightButtonThreeInput);
                CopyToControllerInput(handInput.controllerInput[7], controllerInput.rightButtonFourInput);
                CopyToControllerInput(handInput.controllerInput[8], controllerInput.rightTrigger1Input);
                CopyToControllerInput(handInput.controllerInput[9], controllerInput.rightTrigger2Input);
                CopyToControllerInput(handInput.controllerInput[10], controllerInput.rightOptionInput);
            }
        }

        private void CopyToControllerInput(InputEvent inputSetting, InputEvent controllerSetting) {
            if ((ControllerInput.InputType)inputSetting.type == ControllerInput.InputType.None)
                return;

            controllerSetting.type = inputSetting.type;
            controllerSetting.targetGameObject = inputSetting.targetGameObject;
            controllerSetting.targetComponent = inputSetting.targetComponent;
            controllerSetting.methodName = inputSetting.methodName;
            controllerSetting.enumVal = inputSetting.enumVal;
        }


        private void CopyFromControllerInputList() {
            HandInput settings = (HandInput)target;

            HandTarget handTarget = (HandTarget)handTargetProp.objectReferenceValue;

            if (handTarget == null)
                return;

            HumanoidControl humanoid = handTarget.humanoid;
            ControllerInput controllerInput = humanoid.GetComponent<ControllerInput>();
            if (controllerInput == null)
                return;

            if (handTarget.isLeft) {
                CopyFromControllerInput(controllerInput.leftVerticalInput, settings.controllerInput[0]);
                CopyFromControllerInput(controllerInput.leftHorizontalInput, settings.controllerInput[1]);
                CopyFromControllerInput(controllerInput.leftStickButtonInput, settings.controllerInput[2]);
                CopyFromControllerInput(controllerInput.leftStickTouchInput, settings.controllerInput[3]);
                CopyFromControllerInput(controllerInput.leftButtonOneInput, settings.controllerInput[4]);
                CopyFromControllerInput(controllerInput.leftButtonTwoInput, settings.controllerInput[5]);
                CopyFromControllerInput(controllerInput.leftButtonThreeInput, settings.controllerInput[6]);
                CopyFromControllerInput(controllerInput.leftButtonFourInput, settings.controllerInput[7]);
                CopyFromControllerInput(controllerInput.leftTrigger1Input, settings.controllerInput[8]);
                CopyFromControllerInput(controllerInput.leftTrigger2Input, settings.controllerInput[9]);
                CopyFromControllerInput(controllerInput.leftOptionInput, settings.controllerInput[10]);
            } else {
                CopyFromControllerInput(controllerInput.rightVerticalInput, settings.controllerInput[0]);
                CopyFromControllerInput(controllerInput.rightHorizontalInput, settings.controllerInput[1]);
                CopyFromControllerInput(controllerInput.rightStickButtonInput, settings.controllerInput[2]);
                CopyFromControllerInput(controllerInput.rightStickTouchInput, settings.controllerInput[3]);
                CopyFromControllerInput(controllerInput.rightButtonOneInput, settings.controllerInput[4]);
                CopyFromControllerInput(controllerInput.rightButtonTwoInput, settings.controllerInput[5]);
                CopyFromControllerInput(controllerInput.rightButtonThreeInput, settings.controllerInput[6]);
                CopyFromControllerInput(controllerInput.rightButtonFourInput, settings.controllerInput[7]);
                CopyFromControllerInput(controllerInput.rightTrigger1Input, settings.controllerInput[8]);
                CopyFromControllerInput(controllerInput.rightTrigger2Input, settings.controllerInput[9]);
                CopyFromControllerInput(controllerInput.rightOptionInput, settings.controllerInput[10]);
            }
        }

        private void CopyFromControllerInput(InputEvent controllerSetting, InputEvent inputSetting) {
            inputSetting.type = controllerSetting.type;
            inputSetting.targetGameObject = controllerSetting.targetGameObject;
            inputSetting.targetComponent = controllerSetting.targetComponent;
            inputSetting.methodName = controllerSetting.methodName;
            inputSetting.enumVal = controllerSetting.enumVal;
        }
        #endregion

        //private ControllerInput GetControllerInput(out bool isleft) {
        //    isleft = false;
        //    HandInput settings = (HandInput)target;

        //    HandTarget handTarget = settings.GetComponent<HandTarget>();
        //    if (handTarget == null)
        //        return null;

        //    isleft = handTarget.isLeft;
        //    HumanoidControl humanoid = handTarget.humanoid;
        //    return humanoid.GetComponent<ControllerInput>();

        //}

        #region Inspector
        public override void OnInspectorGUI() {
            serializedObject.Update();

            HandTargetInspector(handInput);

            SetEventInput(handInput);
            //SetHandPoseInputList(handInput);

            if (handInput.handTarget != null) {
                HumanoidControl humanoid = handInput.handTarget.humanoid;
                ControllerInput controllerSettings = humanoid.GetComponent<ControllerInput>();
                SerializedObject sInput = new SerializedObject(controllerSettings);
                if (controllerSettings != null)
                    SetControllerInput(sInput, handInput.gameObject, controllerSettings, handInput.handTarget.isLeft);
                sInput.ApplyModifiedProperties();
            }

            CopyFromControllerInputList();

            serializedObject.ApplyModifiedProperties();
        }

        private void HandTargetInspector(HandInput handInput) {
            if (handInput.handTarget == null) {
                handInput.handTarget = handInput.GetComponent<HandTarget>();
                if (handInput.handTarget == null) {
                    handTargetProp.objectReferenceValue = (HandTarget)EditorGUILayout.ObjectField("Hand Target", handTargetProp.objectReferenceValue, typeof(HandTarget), true);
                    if (handTargetProp.objectReferenceValue == null && handInput.transform.parent != null)
                        handTargetProp.objectReferenceValue = handInput.transform.parent.GetComponent<HandTarget>();
                    handInput.handTarget = (HandTarget)handTargetProp.objectReferenceValue;
                }
            }
        }

        public MethodInfo[] GetSupportedMethods(System.Type type, out string[] methodNames, out Component[] methodComponents) {
            MethodInfo[] methods = new MethodInfo[0];
            methodNames = new string[0];
            methodComponents = new Component[0];


            string[] componentMethodNames;
            MethodInfo[] componentMethods = InputEvent.GetSupportedMethods(type, out componentMethodNames);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
                if (components[i].GetType() != typeof(HumanoidControl) && !components[i].GetType().GetTypeInfo().IsSubclassOf(typeof(Target)))
#else
            if (type != typeof(HumanoidControl) && !type.IsSubclassOf(typeof(HumanoidTarget)))
#endif
                AddComponentName(type.Name, ref componentMethodNames);

            methodNames = Extend(methodNames, componentMethodNames);
            methods = Extend(methods, componentMethods);
            return methods;
        }

        #region Events
        private bool showEvents = true;
        private void SetEventInput(HandInput handInput) {
            if (handInput.handTarget == null)
                return;

            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            EditorGUI.indentLevel++;
            if (showEvents) {
                SetTouchInput(serializedObject.FindProperty("touchInput"), handInput.touchInput, handInput.handTarget);
                SetGrabInput(serializedObject.FindProperty("grabInput"), handInput.grabInput, handInput.handTarget);
            }
            EditorGUI.indentLevel--;
        }

        private void SetTouchInput(SerializedProperty inputProp, InputEvent touchInput, HandTarget handTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Touch", GUILayout.Width(80));
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref touchInput, handTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }

        private void SetGrabInput(SerializedProperty inputProp, InputEvent grabInput, HandTarget handTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Grab", GUILayout.Width(80));
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref grabInput, handTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }

        private new static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType) {
            string[] values = System.Enum.GetNames(typeof(HandInput.InputType));
            return InputTypePopup(inputType, values);
        }

        public new static GameObject GetTargetGameObject(InputEvent.InputType inputType, GameObject gameObject) {
            HandTarget handTarget = gameObject.GetComponent<HandTarget>();
            if (handTarget == null)
                return gameObject;

            switch ((HandInput.InputType)inputType) {
                case HandInput.InputType.None:
                    return null;
                case HandInput.InputType.Hand:
                    return handTarget.gameObject;
                case HandInput.InputType.Humanoid:
                    return handTarget.humanoid.gameObject;
                default:
                    return gameObject;
            }
        }
        #endregion

        #region HandPose
        private string[] handPoseNames;
        private void InitHandPoseInputList(HandInput handInput) {
            HandTarget handTarget = handInput.handTarget;
            handPoseNames = new string[handTarget.poseMixer.mixedPoses.Count];
            for (int i = 0; i < handPoseNames.Length; i++)
                handPoseNames[i] = handTarget.poseMixer.mixedPoses[i].pose.name;
        }
        private bool showHandPoses = true;
        private void SetHandPoseInputList(HandInput handInput) {
            showHandPoses = EditorGUILayout.Foldout(showHandPoses, "Hand Poses", true);
            if (showHandPoses) {
                EditorGUI.indentLevel++;

                if (handPoseListProp.FindPropertyRelative("Array.size").intValue <= 0)
                    handPoseListProp.arraySize = 1;

                SerializedProperty posesProp = serializedObject.FindProperty("handPoseInput");
                bool cleanupNeeded = false;
                for (int i = 0; i < handInput.handPoseInput.Length - 1; i++) {
                    SerializedProperty poseProp = posesProp.GetArrayElementAtIndex(i);
                    SetHandPoseInput(poseProp, handInput.handPoseInput[i], i, handInput.handTarget);
                    if (handInput.handPoseInput[i].poseId == 0)
                        cleanupNeeded = true;
                }
                EditorGUI.indentLevel--;

                if (cleanupNeeded)
                    CleanupKeyboardInput(ref handInput.handPoseInput);

                int last = handInput.handPoseInput.Length - 1;
                if (handInput.handPoseInput[last] != null && handInput.handPoseInput[last].poseId == 0/*HandPoses.PoseId.Unknown*/) {
                    EditorGUILayout.BeginHorizontal();
                    //handInput.handPoseInput[last].poseId = (HandPoses.PoseId)EditorGUILayout.EnumPopup(handInput.handPoseInput[last].poseId, GUILayout.Width(80));
                    handInput.handPoseInput[last].poseId = EditorGUILayout.Popup(handInput.handPoseInput[last].poseId, handPoseNames, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                } else
                    ExtendInputList(ref handInput.handPoseInput);

            }
        }

        private void SetHandPoseInput(SerializedProperty inputProp, HandInput.PoseInput poseInput, int poseIndex, HandTarget handTarget) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel--;
            //poseInput.poseId = (HandPoses.PoseId)EditorGUILayout.EnumPopup(poseInput.poseId, GUILayout.Width(80));
            poseInput.poseId = EditorGUILayout.Popup(poseInput.poseId, handPoseNames, GUILayout.Width(80));
            SetInput(inputProp, poseInput, handTarget.gameObject);
            EditorGUI.indentLevel++;
            EditorGUILayout.EndHorizontal();
        }


        private void SetInput(SerializedProperty inputProp, InputEvent poseInput, GameObject gameObject) {
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref poseInput, gameObject);
        }

        public static void ExtendInputList(ref HandInput.PoseInput[] inputList) {
            HandInput.PoseInput[] newInputList = new HandInput.PoseInput[inputList.Length + 1];
            for (int i = 0; i < inputList.Length; i++)
                newInputList[i] = inputList[i];
            inputList = newInputList;
        }

        public static void CleanupKeyboardInput(ref HandInput.PoseInput[] inputList) {
            // assumes exactly 1 None keycode in the list (except the last)
            HandInput.PoseInput[] newInputList = new HandInput.PoseInput[inputList.Length - 1];
            int j = 0;
            for (int i = 0; i < inputList.Length - 1; i++) {
                if (inputList[i].poseId != 0)
                    newInputList[j++] = inputList[i];
            }
            newInputList[j] = new HandInput.PoseInput();
            inputList = newInputList;
        }
        #endregion

        #region Controller Input
        private bool showControllerInput = false;
        private void SetControllerInput(SerializedObject sInput, GameObject gameObject, ControllerInput controllerSettings, bool isLeft) {
            showControllerInput = EditorGUILayout.Foldout(showControllerInput, "Controller Input", true);
            if (showControllerInput) {
                controllerSettings.controllerType = (ControllerType)EditorGUILayout.EnumPopup("View Controller Type", controllerSettings.controllerType);
                ControllerInput_Editor.SetControllerInputSide(sInput, gameObject, controllerSettings, isLeft, (int)controllerSettings.controllerType);
            }
        }
        #endregion
        #endregion
    }
}
