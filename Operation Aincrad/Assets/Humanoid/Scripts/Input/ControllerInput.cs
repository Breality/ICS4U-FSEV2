using System.Collections.Generic;
using UnityEngine;

namespace Passer {

    public enum ControllerType {
        GenericController = 0,
        Xbox = 1,
        //PS4 = 2,
        //Steelseries = 3,
#if hSTEAMVR
        SteamVR = 4,
#endif
#if hOCULUS
#if UNITY_STANDALONE_WIN
        OculusTouch = 5,
#elif UNITY_ANDROID
        GearVRController = 6,
#endif
#endif
#if hWINDOWSMR
        WindowsMR = 7,
#endif
#if hHYDRA
        RazerHydra = 8,
#endif
        Keyboard = 9,
        Mouse = 10
    }

    [System.Serializable]
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/controller-input/")]
    public class ControllerInput : MonoBehaviour {

        public ControllerType controllerType;

        public enum InputType {
            None,
            Animator,
#if PLAYMAKER
            Playmaker,
#endif
            GameObject,
            Humanoid,
            Head,
            LeftHand,
            RightHand
        }

        public bool fingerMovements = true;

        public InputEvent leftVerticalInput;
        public InputEvent leftHorizontalInput;
        public InputEvent leftStickButtonInput;
        public InputEvent leftStickTouchInput;
        public InputEvent leftButtonOneInput;
        public InputEvent leftButtonTwoInput;
        public InputEvent leftButtonThreeInput;
        public InputEvent leftButtonFourInput;
        public InputEvent leftTrigger1Input;
        public InputEvent leftTrigger2Input;
        public InputEvent leftOptionInput;

        public InputEvent rightVerticalInput;
        public InputEvent rightHorizontalInput;
        public InputEvent rightStickButtonInput;
        public InputEvent rightStickTouchInput;
        public InputEvent rightButtonOneInput;
        public InputEvent rightButtonTwoInput;
        public InputEvent rightButtonThreeInput;
        public InputEvent rightButtonFourInput;
        public Controller rightTrigger1Input;
        public Controller rightTrigger2Input;
        public InputEvent rightOptionInput;

        public List<Keyboard> keys = new List<Keyboard>();

        public InputEvent mouseVerticalInput;
        public InputEvent mouseHorizontalInput;
        public InputEvent mouseScrollInput;
        public InputEvent mouseButtonLeftInput;
        public InputEvent mouseButtonMiddleInput;
        public InputEvent mouseButtonRightInput;

        public HumanoidControl humanoid;
        private HandTarget leftHandTarget;
        private HandTarget rightHandTarget;

        public void Awake() {
            humanoid = GetComponent<HumanoidControl>();
            leftHandTarget = humanoid.leftHandTarget;
            rightHandTarget = humanoid.rightHandTarget;
        }

        public void Update() {
            if (humanoid.controller == null)
                return;

            if (fingerMovements)
                UpdateFingerMovements();
            UpdateLeft();
            UpdateRight();
            UpdateKeyboard();
            UpdateMouse();
        }

        private void UpdateFingerMovements() {
            UpdateFingerMovementsSide(leftHandTarget.fingers, humanoid.controller.left);
            UpdateFingerMovementsSide(rightHandTarget.fingers, humanoid.controller.right);
        }

        private void UpdateFingerMovementsSide(FingersTarget fingers, ControllerSide controllerSide) {
            fingers.thumb.curl = Mathf.Max(controllerSide.trigger2, controllerSide.trigger1);
            fingers.thumb.curl = controllerSide.stickTouch ? fingers.thumb.curl + 0.3F : fingers.thumb.curl;
            fingers.index.curl = controllerSide.trigger1;
            fingers.middle.curl = Mathf.Max(controllerSide.trigger2, controllerSide.trigger1);
            fingers.ring.curl = Mathf.Max(controllerSide.trigger2, controllerSide.trigger1);
            fingers.little.curl = Mathf.Max(controllerSide.trigger2, controllerSide.trigger1);
        }

        private void UpdateLeft() {
            leftVerticalInput.floatValue = humanoid.controller.left.stickVertical;
            leftHorizontalInput.floatValue = humanoid.controller.left.stickHorizontal;
            leftStickButtonInput.boolValue = humanoid.controller.left.stickButton;
            leftStickTouchInput.boolValue = humanoid.controller.left.stickTouch;
            leftButtonOneInput.boolValue = humanoid.controller.left.buttons[0];
            leftButtonTwoInput.boolValue = humanoid.controller.left.buttons[1];
            leftButtonThreeInput.boolValue = humanoid.controller.left.buttons[2];
            leftButtonFourInput.boolValue = humanoid.controller.left.buttons[3];
            leftTrigger1Input.floatValue = humanoid.controller.left.trigger1;
            leftTrigger2Input.floatValue = humanoid.controller.left.trigger2;
            leftOptionInput.boolValue = humanoid.controller.left.option;
        }

        private void UpdateRight() {
            rightVerticalInput.floatValue = humanoid.controller.right.stickVertical;
            rightHorizontalInput.floatValue = humanoid.controller.right.stickHorizontal;
            rightStickButtonInput.boolValue = humanoid.controller.right.stickButton;
            rightStickTouchInput.boolValue = humanoid.controller.right.stickTouch;
            rightButtonOneInput.boolValue = humanoid.controller.right.buttons[0];
            rightButtonTwoInput.boolValue = humanoid.controller.right.buttons[1];
            rightButtonThreeInput.boolValue = humanoid.controller.right.buttons[2];
            rightButtonFourInput.boolValue = humanoid.controller.right.buttons[3];
            rightTrigger1Input.floatValue = humanoid.controller.right.trigger1;
            rightTrigger2Input.floatValue = humanoid.controller.right.trigger2;
            rightOptionInput.boolValue = humanoid.controller.right.option;
        }

        private void UpdateKeyboard() {
            foreach (Keyboard keyboardInput in keys)
                keyboardInput.boolValue = Input.GetKey(keyboardInput.keyCode);
        }

        private void UpdateMouse() {
            mouseVerticalInput.floatValue -= UnityEngine.Input.GetAxis("Mouse Y") / 100;
            mouseHorizontalInput.floatValue += UnityEngine.Input.GetAxis("Mouse X") / 100;
        }

        [System.Serializable]
        public class Controller : Passer.InputEvent {
            public override float floatValue {
                set {
                    valueChanged = (value != _floatValue);
                    boolChanged = ((value >= 0.5F) != _boolValue);
                    _boolValue = (value >= 0.5F);
                    _floatValue = value;
                    _intValue = (int)value;
                    Update();
                }
            }
        }

        [System.Serializable]
        public class Keyboard : Passer.InputEvent {
            public KeyCode keyCode;
        }
    }
}