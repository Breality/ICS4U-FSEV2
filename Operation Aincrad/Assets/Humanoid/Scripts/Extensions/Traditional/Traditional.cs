/* InstantVR default target controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.0
 * date: December 31, 2014
 */

using UnityEngine;
using System.Collections;
using Passer;

namespace Passer {

    public class Traditional : MonoBehaviour { }

    public class TraditionalDevice {

        private HumanoidControl humanoid;

        private Controller controller;

        public static bool gameControllerEnabled = true;

        public void Start(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            humanoid.controller = SetControllerID(humanoid.gameControllerIndex);
        }

        public void Update() {
            //    UpdateMouse();
            
            if (gameControllerEnabled)
                UpdateGameController();
        }

        public void Clear() {
            controller.Clear();
        }

        public Controller SetControllerID(int controllerID) {
            InitAxis(controllerID);
            controller = Controllers.GetController(controllerID);
            return controller;
        }

        public enum LeftRight {
            Left,
            Right
        }

#region mouse
        public LeftRight mouseIsControllerStick = LeftRight.Right;
        public bool mouseAccumulation = true;
        private float mouseSensitivity = 0.1F;
        private float mouseX;
        private float mouseY;
        private void UpdateMouse() {
            if (!mouseAccumulation) {
                mouseX = 0;
                mouseY = 0;
                mouseSensitivity = 10F;
            } else {
                mouseSensitivity = 0.1F;
            }

            mouseX += UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY += UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (mouseIsControllerStick == LeftRight.Left) {
                controller.left.stickHorizontal = controller.left.stickHorizontal + mouseX;
                controller.left.stickVertical = controller.left.stickVertical + mouseY;

                //controller.left.left = mouseX < 0;
                //controller.left.right = mouseX > 0;
                //controller.left.up = mouseY > 0;
                //controller.left.down = mouseY < 0;
            } else {
                controller.right.stickHorizontal = controller.right.stickHorizontal + mouseX;
                controller.right.stickVertical = controller.right.stickVertical + mouseY;

                //controller.right.left = mouseX < 0;
                //controller.right.right = mouseX > 0;
                //controller.right.up = mouseY > 0;
                //controller.right.down = mouseY < 0;
            }
            controller.right.buttons[0] |= UnityEngine.Input.GetMouseButton(0);
            controller.right.buttons[1] |= UnityEngine.Input.GetMouseButton(1);
            controller.right.buttons[2] |= UnityEngine.Input.GetMouseButton(2);
        }
#endregion

#region GameController
        private void UpdateGameController() {
            if (controller != null) {
                switch (humanoid.gameController) {
                    case GameControllers.Xbox:
                        UpdateXboxController();
                        break;
                    case GameControllers.PS4:
                        UpdatePS4Controller();
                        break;
                    case GameControllers.Steelseries:
                        UpdateSteelseriesController();
                        break;
                    case GameControllers.GameSmart:
                        UpdateGameSmartController();
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateXboxController() {
            if (axisAvailable[0] && axisAvailable[1]) {
                controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + UnityEngine.Input.GetAxis(axisName[0]), -1, 1);
                controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + UnityEngine.Input.GetAxis(axisName[1]), -1, 1);
            }

            //if (axisAvailable[5] && axisAvailable[6]) {
            //    controller.left.left |= UnityEngine.Input.GetAxis(axisName[5]) < 0;
            //    controller.left.right |= UnityEngine.Input.GetAxis(axisName[5]) > 0;
            //    controller.left.up |= UnityEngine.Input.GetAxis(axisName[6]) > 0;
            //    controller.left.down |= UnityEngine.Input.GetAxis(axisName[6]) < 0;
            //}

            if (axisAvailable[3] && axisAvailable[4]) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + UnityEngine.Input.GetAxis(axisName[3]), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + UnityEngine.Input.GetAxis(axisName[4]), -1, 1);
            }

            if (axisAvailable[8] && axisAvailable[9]) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + UnityEngine.Input.GetAxis(axisName[8]), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + UnityEngine.Input.GetAxis(axisName[9]), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            // Except for the bumpers, Xbox buttons are trigger butons. No info is sent that the button is held down.
            controller.left.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton6);
            controller.right.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton7);

            controller.right.buttons[0] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton8);
            controller.right.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton9);
        }

        public void UpdatePS4Controller() {
            if (axisAvailable[0] && axisAvailable[1]) {
                controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + UnityEngine.Input.GetAxis(axisName[0]), -1, 1);
                controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + UnityEngine.Input.GetAxis(axisName[1]), -1, 1);
            }

            //if (axisAvailable[6] && axisAvailable[7]) {
            //    controller.left.left |= UnityEngine.Input.GetAxis(axisName[6]) < 0;
            //    controller.left.right |= UnityEngine.Input.GetAxis(axisName[6]) > 0;
            //    controller.left.up |= UnityEngine.Input.GetAxis(axisName[7]) > 0;
            //    controller.left.down |= UnityEngine.Input.GetAxis(axisName[7]) < 0;
            //}

            if (axisAvailable[2] && axisAvailable[5]) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + UnityEngine.Input.GetAxis(axisName[2]), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + UnityEngine.Input.GetAxis(axisName[5]), -1, 1);
            }

            if (axisAvailable[3] && axisAvailable[4]) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + UnityEngine.Input.GetAxis(axisName[3]), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + UnityEngine.Input.GetAxis(axisName[4]), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton8);
            controller.right.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton9);

            controller.right.buttons[0] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[1] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[2] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[3] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton10);
            controller.right.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton11);
        }

        public void UpdateSteelseriesController() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + UnityEngine.Input.GetAxis(axisName[0]), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + UnityEngine.Input.GetAxis(axisName[1]), -1, 1);

            //if (axisAvailable[4] && axisAvailable[5]) {
            //    controller.left.left |= UnityEngine.Input.GetAxis(axisName[4]) < 0;
            //    controller.left.right |= UnityEngine.Input.GetAxis(axisName[4]) > 0;
            //    controller.left.up |= UnityEngine.Input.GetAxis(axisName[5]) > 0;
            //    controller.left.down |= UnityEngine.Input.GetAxis(axisName[5]) < 0;
            //}

            if (axisAvailable[2] && axisAvailable[3]) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + UnityEngine.Input.GetAxis(axisName[2]), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + UnityEngine.Input.GetAxis(axisName[3]), -1, 1);
            }

            if (axisAvailable[12] && axisAvailable[11]) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + UnityEngine.Input.GetAxis(axisName[12]), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + UnityEngine.Input.GetAxis(axisName[11]), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton10);

            controller.right.buttons[0] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton8);
            controller.right.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton9);
        }

        public void UpdateGameSmartController() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + UnityEngine.Input.GetAxis(axisName[0]), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + UnityEngine.Input.GetAxis(axisName[1]), -1, 1);

            //if (axisAvailable[4] && axisAvailable[5]) {
            //    controller.left.left |= UnityEngine.Input.GetAxis(axisName[4]) < 0;
            //    controller.left.right |= UnityEngine.Input.GetAxis(axisName[4]) > 0;
            //    controller.left.up |= UnityEngine.Input.GetAxis(axisName[5]) > 0;
            //    controller.left.down |= UnityEngine.Input.GetAxis(axisName[5]) < 0;
            //}

            if (axisAvailable[2] && axisAvailable[3]) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + UnityEngine.Input.GetAxis(axisName[2]), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical - UnityEngine.Input.GetAxis(axisName[3]), -1, 1);
            }

            controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton6) ? 1 : 0), -1, 1);
            controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton7) ? 1 : 0), -1, 1);

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton8);
            controller.right.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton9);

            controller.right.buttons[0] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[1] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[2] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[3] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton10);
            controller.right.stickButton |= UnityEngine.Input.GetKey(KeyCode.JoystickButton11);
        }

        public void UpdateGA100Controller() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + UnityEngine.Input.GetAxis(axisName[0]), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + UnityEngine.Input.GetAxis(axisName[1]), -1, 1);

            controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton7) ? 1 : 0), -1, 1);
            controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton9) ? 1 : 0), -1, 1);

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton6) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (UnityEngine.Input.GetKey(KeyCode.JoystickButton8) ? 1 : 0), -1, 1);

            controller.left.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton10);
            controller.right.option |= UnityEngine.Input.GetKey(KeyCode.JoystickButton11);

            controller.right.buttons[0] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= UnityEngine.Input.GetKey(KeyCode.JoystickButton3);
        }
#endregion

        private bool[] axisAvailable;
        private string[] axisName;
        private void InitAxis(int controllerID) {
            InitAxisNames(controllerID);
            InitAxisAvailable();
        }

        private void InitAxisNames(int controllerID) {
            if (axisName == null)
                axisName = new string[13];

            axisName[0] = "Horizontal";
            axisName[1] = "Vertical";
            axisName[2] = "Axis 3";
            axisName[3] = "Axis 4";
            axisName[4] = "Axis 5";
            axisName[5] = "Axis 6";
            axisName[6] = "Axis 7";
            axisName[7] = "Axis 8";
            axisName[8] = "Axis 9";
            axisName[9] = "Axis 10";
            axisName[10] = "Axis 11";
            axisName[11] = "Axis 12";
            axisName[12] = "Axis 13";
            
            if (controllerID > 0) {
                for (int i = 0; i < 13; i++) {
                    axisName[i] += "[" + (controllerID + 1) + "]";
                }            
            }
        }

        private void InitAxisAvailable() {
            if (axisAvailable == null)
                axisAvailable = new bool[13];
            for (int i = 0; i < 13; i++) {
                axisAvailable[i] = IsAxisAvailable(axisName[i]);
                if (!axisAvailable[i]) {
                    //Debug.Log(axisName[i]);
                }
            }
        }

        private static bool IsAxisAvailable(string axisName) {
            try {
                UnityEngine.Input.GetAxis(axisName);
                return true;
            }
            catch (System.Exception) {
                return false;
            }
        }
    }
}
