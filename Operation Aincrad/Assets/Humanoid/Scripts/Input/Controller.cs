/* InstantVR Input
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.7.0
 * date: September 26, 2016
 * 
 * - Replaced button constants with enum
 */

using UnityEngine;

namespace Passer {

    public static class Controllers {
        private static int maxControllers = 4;
        public static Controller[] controllers;

        public static void Update() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++) {
                    if (controllers[i] != null)
                        controllers[i].Update();
                }
            }
        }

        public static Controller GetController(int controllerID) {
            if (controllers == null)
                controllers = new Controller[maxControllers];
            if (controllers[controllerID] == null)
                controllers[controllerID] = new Controller();            
       
            return controllers[controllerID];
        }

        public static void Clear() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++) {
                    if (controllers[i] != null)
                        controllers[i].Clear();
                }
            }
        }

        public static void EndFrame() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++)
                    if (controllers[i] != null)
                        controllers[i].EndFrame();
            }
        }
    }

    public class Controller {
        public enum Side {
            Left,
            Right
        }
        public enum Button {
            ButtonOne = 0,
            ButtonTwo = 1,
            ButtonThree = 2,
            ButtonFour = 3,
            Bumper = 10,
            BumperTouch = 11,
            Trigger = 12,
            TriggerTouch = 13,
            StickButton = 14,
            StickTouch = 15,
            //Up = 20,
            //Down = 21,
            //Left = 22,
            //Right = 23,
            Option = 30,
            None = 9999
        }

        public ControllerSide left;
        public ControllerSide right;

        public void Update() {
            left.Update();
            right.Update();
        }

        public Controller() {
            left = new ControllerSide();
            right = new ControllerSide();
        }

        private bool cleared;
        public void Clear() {
            if (cleared)
                return;

            cleared = true;
            left.Clear();
            right.Clear();
        }

        public void EndFrame() {
            cleared = false;
        }

        public bool GetButton(Side side, Button buttonID) {
            switch (side) {
                case Side.Left:
                    return left.GetButton(buttonID);
                case Side.Right:
                    return right.GetButton(buttonID);
                default:
                    return false;

            }
        }
    }

    public class ControllerSide {
        public float stickHorizontal;
        public float stickVertical;
        public bool stickButton;
        public bool stickTouch;
        //public bool up;
        //public bool down;
        //public bool left;
        //public bool right;

        public bool[] buttons = new bool[4];

        public float trigger1;
        public float trigger2;

        public bool option;

        public event OnButtonDown OnButtonDownEvent;
        public event OnButtonUp OnButtonUpEvent;

        public delegate void OnButtonDown(Controller.Button buttonNr);
        public delegate void OnButtonUp(Controller.Button buttonNr);

        private bool[] lastButtons = new bool[4];
        private bool lastBumper;
        private bool lastTrigger;
        private bool lastStickButton;
        private bool lastOption;

        public void Update() {
            for (int i = 0; i < 4; i++) {
                if (buttons[i] && !lastButtons[i]) {
                    if (OnButtonDownEvent != null)
                        OnButtonDownEvent((Controller.Button) i);

                } else if (!buttons[i] && lastButtons[i]) {
                    if (OnButtonUpEvent != null)
                        OnButtonUpEvent((Controller.Button) i);
                }
                lastButtons[i] = buttons[i];
            }

            if (trigger1 > 0.9F && !lastBumper) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(Controller.Button.Bumper);
                lastBumper = true;
            } else if (trigger1 < 0.1F && lastBumper) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(Controller.Button.Bumper);
                lastBumper = false;
            }

            if (trigger2 > 0.9F && !lastTrigger) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(Controller.Button.Trigger);
                lastTrigger = true;
            } else if (trigger2 < 0.1F && lastTrigger) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(Controller.Button.Trigger);
                lastTrigger = false;
            }

            if (stickButton && !lastStickButton) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(Controller.Button.StickButton);
            } else if (!stickButton && lastStickButton) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(Controller.Button.StickButton);
            }
            lastStickButton = stickButton;

            if (option && !lastOption) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(Controller.Button.Option);
            } else if (!option && lastOption) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(Controller.Button.Option);
            }
            lastOption = option;
        }

        public void Clear() {
            stickHorizontal = 0;
            stickVertical = 0;
            stickButton = false;
            stickTouch = false;

            //up = false;
            //down = false;
            //left = false;
            //right = false;

            for (int i = 0; i < 4; i++)
                buttons[i] = false;

            trigger1 = 0;
            trigger2 = 0;

            option = false;
        }

        public bool GetButton(Controller.Button buttonID) {
            switch (buttonID) {
                case Controller.Button.ButtonOne:
                    return buttons[0];
                case Controller.Button.ButtonTwo:
                    return buttons[1];
                case Controller.Button.ButtonThree:
                    return buttons[2];
                case Controller.Button.ButtonFour:
                    return buttons[3];
                case Controller.Button.Bumper:
                    return trigger1 > 0.9F;
                case Controller.Button.Trigger:
                    return trigger2 > 0.9F;
                case Controller.Button.StickButton:
                    return stickButton;
                case Controller.Button.StickTouch:
                    return stickTouch;
                case Controller.Button.Option:
                    return option;
                //case Controller.Button.Up:
                //    return up;
                //case Controller.Button.Down:
                //    return down;
                //case Controller.Button.Left:
                //    return left;
                //case Controller.Button.Right:
                //    return right;
                default:
                    return false;
            }
        }
    }
}

/*
#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions {

    [ActionCategory("InstantVR")]
    [Tooltip("Controller input axis")]
    public class GetControllerAxis : FsmStateAction {

        [RequiredField]
        [Tooltip("Left or right (side) controller")]
        public BodySide controllerSide = BodySide.Left;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Store the direction vector.")]
        public FsmVector3 storeVector;

        private IVR.ControllerInput controller0;

        public override void Awake() {
            controller0 = IVR.Controllers.GetController(0);
        }

        public override void OnUpdate() {
            IVR.ControllerInputSide controller = (controllerSide == BodySide.Left) ? controller0.left : controller0.right;

            storeVector.Value = new Vector3(controller.stickHorizontal, 0, controller.stickVertical);
        }
    }

    [ActionCategory("InstantVR")]
    [Tooltip("Controller input button")]
    public class GetControllerButton : FsmStateAction {

        [RequiredField]
        [Tooltip("Left or right (side) controller")]
        public BodySide controllerSide = BodySide.Right;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Controller Button")]
        public ControllerButton button;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store Result Bool")]
        public FsmBool storeBool;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store Result Float")]
        public FsmFloat storeFloat;

        [Tooltip("Event to send when the button is pressed.")]
        public FsmEvent buttonPressed;

        [Tooltip("Event to send when the button is released.")]
        public FsmEvent buttonReleased;

        private IVR.ControllerInput controller0;

        public override void Awake() {
            controller0 = IVR.Controllers.GetController(0);
        }

        public override void OnUpdate() {
            IVR.ControllerInputSide controller = (controllerSide == BodySide.Left) ? controller0.left : controller0.right;

            bool oldBool = storeBool.Value;

            switch (button) {
                case ControllerInput.Button.StickButton:
                    storeBool.Value = controller.stickButton;
                    storeFloat.Value = controller.stickButton ? 1 : 0;
                    break;
                case ControllerInput.Button.Up:
                    storeBool.Value = controller.up;
                    storeFloat.Value = controller.up ? 1 : 0;
                    break;
                case ControllerInput.Button.Down:
                    storeBool.Value = controller.down;
                    storeFloat.Value = controller.down ? 1 : 0;
                    break;
                case ControllerInput.Button.Left:
                    storeBool.Value = controller.left;
                    storeFloat.Value = controller.left ? 1 : 0;
                    break;
                case ControllerInput.Button.Right:
                    storeBool.Value = controller.right;
                    storeFloat.Value = controller.right ? 1 : 0;
                    break;
                case ControllerInput.Button.Button0:
                    storeBool.Value = controller.buttons[0];
                    storeFloat.Value = controller.buttons[0] ? 1 : 0;
                    break;
                case ControllerInput.Button.Button1:
                    storeBool.Value = controller.buttons[1];
                    storeFloat.Value = controller.buttons[1] ? 1 : 0;
                    break;
                case ControllerInput.Button.Button2:
                    storeBool.Value = controller.buttons[2];
                    storeFloat.Value = controller.buttons[2] ? 1 : 0;
                    break;
                case ControllerInput.Button.Button3:
                    storeBool.Value = controller.buttons[3];
                    storeFloat.Value = controller.buttons[3] ? 1 : 0;
                    break;
                case ControllerInput.Button.Option:
                    storeBool.Value = controller.option;
                    storeFloat.Value = controller.option ? 1 : 0;
                    break;
                case ControllerInput.Button.Bumper:
                    storeBool.Value = controller.bumper > 0.9F;
                    storeFloat.Value = controller.bumper;
                    break;
                case ControllerInput.Button.Trigger:
                    storeBool.Value = controller.trigger > 0.9F;
                    storeFloat.Value = controller.trigger;
                    break;
            }

            if (storeBool.Value && !oldBool)
                Fsm.Event(buttonPressed);
            else if (!storeBool.Value && oldBool)
                Fsm.Event(buttonReleased);
        }
    }
}
#endif
*/