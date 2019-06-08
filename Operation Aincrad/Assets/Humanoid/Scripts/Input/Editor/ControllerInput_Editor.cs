using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Passer {

    [CustomEditor(typeof(ControllerInput))]
    public class ControllerInput_Editor : Input_Editor {

        private ControllerInput controllerInput;

        private static string[] humanoidMethodNames;

        private static string[] headTargetMethodNames;

        private static string[] leftHandTargetMethodNames;
        private static string[] rightHandTargetMethodNames;

        #region Enable
        public void OnEnable() {
            controllerInput = (ControllerInput)target;
            HumanoidControl humanoid = controllerInput.GetComponent<HumanoidControl>();

            controllerInput.humanoid = humanoid;

            InitInput();
            InitMouseInput(controllerInput);
            InitGameControllerInput();
        }
        #endregion

        #region Init
        private SerializedProperty viewControllerTypeProp;
        private void InitInput() {
            viewControllerTypeProp = serializedObject.FindProperty("controllerType");
        }
        #endregion

        #region Inspector
        private bool showLeft = true;
        private bool showRight = true;
        public override void OnInspectorGUI() {
            serializedObject.Update();

            viewControllerTypeProp.intValue = (int)(ControllerType)EditorGUILayout.EnumPopup("View Controller Type", (ControllerType)viewControllerTypeProp.intValue);

            Init(controllerInput.humanoid);

            if (controllerInput.controllerType == ControllerType.Keyboard)
                SetKeyboardInputList(controllerInput);
            else if (controllerInput.controllerType == ControllerType.Mouse)
                SetMouseInputList(controllerInput);
            else
                SetGameControllerInput(controllerInput);

            serializedObject.ApplyModifiedProperties();
        }

        private new static InputEvent.InputType InputTypePopup(InputEvent.InputType inputType) {
            string[] values = System.Enum.GetNames(typeof(ControllerInput.InputType));
            return InputTypePopup(inputType, values);
        }

        public new static GameObject GetTargetGameObject(InputEvent.InputType inputType, GameObject gameObject) {
            HumanoidControl humanoid = gameObject.GetComponent<HumanoidControl>();
            if (humanoid == null)
                return gameObject;

            switch ((ControllerInput.InputType)inputType) {
                case ControllerInput.InputType.None:
                    return null;
                case ControllerInput.InputType.Head:
                    return humanoid.headTarget.gameObject;
                case ControllerInput.InputType.LeftHand:
                    return humanoid.leftHandTarget.gameObject;
                case ControllerInput.InputType.RightHand:
                    return humanoid.rightHandTarget.gameObject;
                case ControllerInput.InputType.Humanoid:
                    return humanoid.gameObject;
                default:
                    return gameObject;
            }
        }

        #region GameControllerInput
        private SerializedProperty fingerMovementsProp;
        private void InitGameControllerInput() {
            fingerMovementsProp = serializedObject.FindProperty("fingerMovements");
        }

        private void SetGameControllerInput(ControllerInput controllerInput) {
            GameObject gameObject = controllerInput.humanoid.gameObject;

            GUIContent text = new GUIContent(
                "Finger Movements",
                "Implements finger movements using controller input"
                );
            fingerMovementsProp.boolValue = EditorGUILayout.Toggle(text, fingerMovementsProp.boolValue);

            showLeft = EditorGUILayout.Foldout(showLeft, "Left");
            if (showLeft) {
                SetControllerInputSide(serializedObject, gameObject, controllerInput, true, (int)controllerInput.controllerType);
            }
            showRight = EditorGUILayout.Foldout(showRight, "Right");
            if (showRight) {
                SetControllerInputSide(serializedObject, gameObject, controllerInput, false, (int)controllerInput.controllerType);
            }
        }

        public static void SetControllerInputSide(SerializedObject sInput, GameObject gameObject, ControllerInput settings, bool isLeft, int controllerType) {
            if (isLeft) {
                EditorGUI.indentLevel++;
                SetInput(sInput.FindProperty("leftVerticalInput"), settings.leftVerticalInput, controllerLabels[controllerType].leftVertical, gameObject);
                SetInput(sInput.FindProperty("leftHorizontalInput"), settings.leftHorizontalInput, controllerLabels[controllerType].leftHorizontal, gameObject);
                SetInput(sInput.FindProperty("leftStickButtonInput"), settings.leftStickButtonInput, controllerLabels[controllerType].leftStickPress, gameObject);
                SetInput(sInput.FindProperty("leftStickTouchInput"), settings.leftStickTouchInput, controllerLabels[controllerType].leftStickTouch, gameObject);
                SetInput(sInput.FindProperty("leftButtonOneInput"), settings.leftButtonOneInput, controllerLabels[controllerType].leftButtonOne, gameObject);
                SetInput(sInput.FindProperty("leftButtonTwoInput"), settings.leftButtonTwoInput, controllerLabels[controllerType].leftButtonTwo, gameObject);
                SetInput(sInput.FindProperty("leftButtonThreeInput"), settings.leftButtonThreeInput, controllerLabels[controllerType].leftButtonThree, gameObject);
                SetInput(sInput.FindProperty("leftButtonFourInput"), settings.leftButtonFourInput, controllerLabels[controllerType].leftButtonFour, gameObject);
                SetInput(sInput.FindProperty("leftTrigger1Input"), settings.leftTrigger1Input, controllerLabels[controllerType].leftTrigger1, gameObject);
                SetInput(sInput.FindProperty("leftTrigger2Input"), settings.leftTrigger2Input, controllerLabels[controllerType].leftTrigger2, gameObject);
                SetInput(sInput.FindProperty("leftOptionInput"), settings.leftOptionInput, controllerLabels[controllerType].leftOption, gameObject);
                EditorGUI.indentLevel--;
            } else {
                EditorGUI.indentLevel++;
                SetInput(sInput.FindProperty("rightVerticalInput"), settings.rightVerticalInput, controllerLabels[controllerType].rightVertical, gameObject);
                SetInput(sInput.FindProperty("rightHorizontalInput"), settings.rightHorizontalInput, controllerLabels[controllerType].rightHorizontal, gameObject);
                SetInput(sInput.FindProperty("rightStickButtonInput"), settings.rightStickButtonInput, controllerLabels[controllerType].rightStickPress, gameObject);
                SetInput(sInput.FindProperty("rightStickTouchInput"), settings.rightStickTouchInput, controllerLabels[controllerType].rightStickTouch, gameObject);
                SetInput(sInput.FindProperty("rightButtonOneInput"), settings.rightButtonOneInput, controllerLabels[controllerType].rightButtonOne, gameObject);
                SetInput(sInput.FindProperty("rightButtonTwoInput"), settings.rightButtonTwoInput, controllerLabels[controllerType].rightButtonTwo, gameObject);
                SetInput(sInput.FindProperty("rightButtonThreeInput"), settings.rightButtonThreeInput, controllerLabels[controllerType].rightButtonThree, gameObject);
                SetInput(sInput.FindProperty("rightButtonFourInput"), settings.rightButtonFourInput, controllerLabels[controllerType].rightButtonFour, gameObject);
                SetInput(sInput.FindProperty("rightTrigger1Input"), settings.rightTrigger1Input, controllerLabels[controllerType].rightTrigger1, gameObject);
                SetInput(sInput.FindProperty("rightTrigger2Input"), settings.rightTrigger2Input, controllerLabels[controllerType].rightTrigger2, gameObject);
                SetInput(sInput.FindProperty("rightOptionInput"), settings.rightOptionInput, controllerLabels[controllerType].rightOption, gameObject);
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region KeyboardInput

        private void SetKeyboardInputList(ControllerInput controllerInput) {
            SerializedProperty keysProp = serializedObject.FindProperty("keys");
            int i = 0;
            foreach (ControllerInput.Keyboard keyInput in controllerInput.keys) {
                SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(i);
                SetKeyboardInput(keyProp, keyInput, controllerInput.humanoid);
                i++;
            }
            CleanKeyInput(controllerInput.keys);

            EditorGUILayout.BeginHorizontal();
            KeyCode keyCode = (KeyCode)EditorGUILayout.EnumPopup(KeyCode.None, GUILayout.Width(80));
            if (keyCode != KeyCode.None)
                AddKeyboardInput(controllerInput, keyCode);
            EditorGUILayout.EndHorizontal();
        }

        private void SetKeyboardInput(SerializedProperty inputProp, ControllerInput.Keyboard input, HumanoidControl humanoid) {
            EditorGUILayout.BeginHorizontal();
            input.keyCode = (KeyCode)EditorGUILayout.EnumPopup(input.keyCode, GUILayout.Width(80));
            SetInput(inputProp, input, humanoid.gameObject);
            EditorGUILayout.EndHorizontal();
        }

        private void SetInput(SerializedProperty inputProp, InputEvent keyboardInput, GameObject gameObject) {
            SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref keyboardInput, gameObject);
        }

        private void AddKeyboardInput(ControllerInput controllerInput, KeyCode keyCode) {
            ControllerInput.Keyboard keyboardInput = new ControllerInput.Keyboard();
            keyboardInput.keyCode = keyCode;
            controllerInput.keys.Add(keyboardInput);
        }

        private void ExtendKeyboardInput(ref ControllerInput.Keyboard[] keyboardInput) {
            ControllerInput.Keyboard[] newKeyboardInput = new ControllerInput.Keyboard[keyboardInput.Length + 1];
            for (int i = 0; i < keyboardInput.Length; i++)
                newKeyboardInput[i] = keyboardInput[i];
            keyboardInput = newKeyboardInput;
        }

        private void CleanKeyInput(List<ControllerInput.Keyboard> keyInputs) {
            keyInputs.RemoveAll(keyInput => keyInput.keyCode == KeyCode.None);
        }
        #endregion

        #region MouseInput
        private void InitMouseInput(ControllerInput settings) {
        }

        private void SetMouseInputList(ControllerInput settings) {
            GameObject gameObject = settings.humanoid.gameObject;
            EditorGUI.indentLevel++;
            SetInput(serializedObject.FindProperty("mouseVerticalInput"), settings.mouseVerticalInput, "Vertical", gameObject);
            SetInput(serializedObject.FindProperty("mouseHorizontalInput"), settings.mouseHorizontalInput, "Horizontal", gameObject);
            SetInput(serializedObject.FindProperty("mouseScrollInput"), settings.mouseScrollInput, "Scroll Wheel", gameObject);
            SetInput(serializedObject.FindProperty("mouseButtonLeftInput"), settings.mouseButtonLeftInput, "Left Button", gameObject);
            SetInput(serializedObject.FindProperty("mouseButtonMiddleInput"), settings.mouseButtonMiddleInput, "Middle Button", gameObject);
            SetInput(serializedObject.FindProperty("mouseButtonRightInput"), settings.mouseButtonRightInput, "Right Button", gameObject);
            EditorGUI.indentLevel--;
        }
        #endregion

        #region Game Controller Configurations
        public struct ControllerConfiguration {
            public string leftVertical;
            public string leftHorizontal;
            public string leftStickPress;
            public string leftStickTouch;
            public string leftButtonOne;
            public string leftButtonTwo;
            public string leftButtonThree;
            public string leftButtonFour;
            public string leftTrigger1;
            public string leftTrigger2;
            public string leftOption;

            public string rightVertical;
            public string rightHorizontal;
            public string rightStickPress;
            public string rightStickTouch;
            public string rightButtonOne;
            public string rightButtonTwo;
            public string rightButtonThree;
            public string rightButtonFour;
            public string rightTrigger1;
            public string rightTrigger2;
            public string rightOption;
        }
        public static ControllerConfiguration[] controllerLabels = CreateControllerConfiguration();
        private static ControllerConfiguration[] CreateControllerConfiguration() {
            return new ControllerConfiguration[] {
                GenericControllerLabels(),
                XboxControllerLabels(),
                Ps4ControllerLabels(),
                SteelseriesControllerLabels(),
                SteamVrControllerLabels(),
                OculusTouchControllerLabels(),
                GearVrControllerLabels(),
                WindowsMRControllerLabels(),
                RazerHydraControllerLabels()
            };
        }

        private static ControllerConfiguration GenericControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftStickTouch = "Stick Touch",
                leftButtonOne = "Button One",
                leftButtonTwo = "Button Two",
                leftButtonThree = "Button Three",
                leftButtonFour = "Button Four",
                leftTrigger1 = "Trigger 1",
                leftTrigger2 = "Trigger 2",
                leftOption = "Option",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightStickTouch = "Stick Touch",
                rightButtonOne = "Button One",
                rightButtonTwo = "Button Two",
                rightButtonThree = "Button Three",
                rightButtonFour = "Button Four",
                rightTrigger1 = "Trigger 1",
                rightTrigger2 = "Trigger 2",
                rightOption = "Option",
            };
        }
        private static ControllerConfiguration XboxControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftTrigger1 = "Bumper",
                leftTrigger2 = "Trigger",
                leftOption = "Back",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightButtonOne = "A",
                rightButtonTwo = "B",
                rightButtonThree = "X",
                rightButtonFour = "Y",
                rightTrigger1 = "Bumper",
                rightTrigger2 = "Trigger",
                rightOption = "Start",
            };
        }
        private static ControllerConfiguration Ps4ControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftTrigger1 = "L1",
                leftTrigger2 = "L2",
                leftOption = "Back",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightButtonOne = "Cross",
                rightButtonTwo = "Circle",
                rightButtonThree = "Square",
                rightButtonFour = "Triangle",
                rightTrigger1 = "R1",
                rightTrigger2 = "R2",
                rightOption = "Start",
            };
        }
        private static ControllerConfiguration SteelseriesControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftTrigger1 = "L1",
                leftTrigger2 = "L2",
                leftOption = "Back",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightButtonOne = "A",
                rightButtonTwo = "B",
                rightButtonThree = "X",
                rightButtonFour = "Y",
                rightTrigger1 = "R1",
                rightTrigger2 = "R2",
                rightOption = "Start",
            };
        }
        private static ControllerConfiguration SteamVrControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Touchpad Press",
                leftStickTouch = "Touchpad Touch",
                leftTrigger1 = "Trigger",
                leftTrigger2 = "Grip",
                leftOption = "Menu",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Touchpad Press",
                rightStickTouch = "Touchpad Touch",
                rightTrigger1 = "Trigger",
                rightTrigger2 = "Grip",
                rightOption = "Menu",
            };
        }
        private static ControllerConfiguration OculusTouchControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftStickTouch = "Stick Touch",
                leftButtonOne = "X",
                leftButtonTwo = "Y",
                leftTrigger1 = "Trigger 1",
                leftTrigger2 = "Trigger 2",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightStickTouch = "Stick Touch",
                rightButtonOne = "A",
                rightButtonTwo = "B",
                rightTrigger1 = "Trigger 1",
                rightTrigger2 = "Trigger 2",
            };
        }
        private static ControllerConfiguration GearVrControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Touchpad Press",
                leftStickTouch = "Touchpad Touch",
                leftTrigger1 = "Trigger",
                leftOption = "Back",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Touchpad Press",
                rightStickTouch = "Touchpad Touch",
                rightTrigger1 = "Trigger",
                rightOption = "Back"
            };
        }
        private static ControllerConfiguration WindowsMRControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Press",
                leftStickTouch = "Touch Pad",
                leftTrigger1 = "Select",
                leftTrigger2 = "Grab",
                leftOption = "Menu",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Press",
                rightStickTouch = "Touch Pad",
                rightTrigger1 = "Select",
                rightTrigger2 = "Grab",
                rightOption = "Menu"
            };
        }
        private static ControllerConfiguration RazerHydraControllerLabels() {
            return new ControllerConfiguration {
                leftVertical = "Vertical",
                leftHorizontal = "Horizontal",
                leftStickPress = "Stick Press",
                leftButtonOne = "1",
                leftButtonTwo = "2",
                leftButtonThree = "3",
                leftButtonFour = "4",
                leftTrigger1 = "Bumper",
                leftTrigger2 = "Trigger",
                leftOption = "Option",

                rightVertical = "Vertical",
                rightHorizontal = "Horizontal",
                rightStickPress = "Stick Press",
                rightButtonOne = "1",
                rightButtonTwo = "2",
                rightButtonThree = "3",
                rightButtonFour = "4",
                rightTrigger1 = "Bumper",
                rightTrigger2 = "Trigger",
                rightOption = "Option",
            };
        }

        #endregion

        public static void SetInput(SerializedProperty inputProp, InputEvent input, string label, GameObject gameObject) {
            if (label == null)
                EditorGUILayout.LabelField(" ");
            else {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(80));
                SetInput(inputProp, GetTargetGameObject, InputTypePopup, ref input, gameObject);
                EditorGUI.indentLevel++;
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }

    [InitializeOnLoad]
    class InputManager {
        static InputManager() {
            EnforceInputManagerBindings();
        }

        private static void EnforceInputManagerBindings() {
            try {
                BindAxis(new Axis() { name = "Axis 3", axis = 2, });
                BindAxis(new Axis() { name = "Axis 4", axis = 3, });
                BindAxis(new Axis() { name = "Axis 5", axis = 4, });
                BindAxis(new Axis() { name = "Axis 6", axis = 5, });
                BindAxis(new Axis() { name = "Axis 7", axis = 6, });
                BindAxis(new Axis() { name = "Axis 8", axis = 7, });
                BindAxis(new Axis() { name = "Axis 9", axis = 8, });
                BindAxis(new Axis() { name = "Axis 10", axis = 9, });
                BindAxis(new Axis() { name = "Axis 11", axis = 10, });
                BindAxis(new Axis() { name = "Axis 12", axis = 11, });
                BindAxis(new Axis() { name = "Axis 13", axis = 12, });
            }
            catch {
                Debug.LogError("Failed to apply Humanoid input manager bindings.");
            }
        }

        private class Axis {
            public string name = System.String.Empty;
            public string descriptiveName = System.String.Empty;
            public string descriptiveNegativeName = System.String.Empty;
            public string negativeButton = System.String.Empty;
            public string positiveButton = System.String.Empty;
            public string altNegativeButton = System.String.Empty;
            public string altPositiveButton = System.String.Empty;
            public float gravity = 0.0f;
            public float dead = 0.001f;
            public float sensitivity = 1.0f;
            public bool snap = false;
            public bool invert = false;
            public int type = 2;
            public int axis = 0;
            public int joyNum = 0;
        }

        private static void BindAxis(Axis axis) {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            SerializedProperty axisIter = axesProperty.Copy();
            axisIter.Next(true);
            axisIter.Next(true);
            while (axisIter.Next(false)) {
                if (axisIter.FindPropertyRelative("m_Name").stringValue == axis.name) {
                    // Axis already exists. Don't create binding.
                    return;
                }
            }

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            axisProperty.FindPropertyRelative("m_Name").stringValue = axis.name;
            axisProperty.FindPropertyRelative("descriptiveName").stringValue = axis.descriptiveName;
            axisProperty.FindPropertyRelative("descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            axisProperty.FindPropertyRelative("negativeButton").stringValue = axis.negativeButton;
            axisProperty.FindPropertyRelative("positiveButton").stringValue = axis.positiveButton;
            axisProperty.FindPropertyRelative("altNegativeButton").stringValue = axis.altNegativeButton;
            axisProperty.FindPropertyRelative("altPositiveButton").stringValue = axis.altPositiveButton;
            axisProperty.FindPropertyRelative("gravity").floatValue = axis.gravity;
            axisProperty.FindPropertyRelative("dead").floatValue = axis.dead;
            axisProperty.FindPropertyRelative("sensitivity").floatValue = axis.sensitivity;
            axisProperty.FindPropertyRelative("snap").boolValue = axis.snap;
            axisProperty.FindPropertyRelative("invert").boolValue = axis.invert;
            axisProperty.FindPropertyRelative("type").intValue = axis.type;
            axisProperty.FindPropertyRelative("axis").intValue = axis.axis;
            axisProperty.FindPropertyRelative("joyNum").intValue = axis.joyNum;
            serializedObject.ApplyModifiedProperties();
        }
    }
}