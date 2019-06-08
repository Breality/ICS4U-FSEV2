using UnityEngine;
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
using UnityEngine.XR.WSA.Input;
#endif

namespace Passer {
    using Humanoid.Tracking;

    public class WindowsMRControllerComponent : SensorComponent {
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
        public bool isLeft;

        public Vector3 touchPad;
        public Vector3 joystick;
        public float trigger;
        public float gripButton;
        public float menuButton;
        public float aButton;

        public override void UpdateComponent() {
            WindowsMRDevice.SensorState controllerState = WindowsMRDevice.GetControllerState(isLeft);

            if (!controllerState.tracked) {
                status = Status.Present;
                return;
            }

            transform.position = trackerTransform.TransformPoint(controllerState.position);
            transform.rotation = trackerTransform.rotation * controllerState.rotation;

            positionConfidence = controllerState.positionConfidence;
            rotationConfidence = controllerState.rotationConfidence;

            status = Status.Tracking;
            gameObject.SetActive(true);

            UpdateInput();
        }

        private InteractionSourceState[] sourceStates;
        private void UpdateInput() {
            if (sourceStates == null)
                sourceStates = new InteractionSourceState[InteractionManager.numSourceStates];

            int n = InteractionManager.GetCurrentReading(sourceStates);
            for (int i = 0; i < n; i++) {
                InteractionSourceState sourceState = sourceStates[i];
                if (sourceState.source.kind != InteractionSourceKind.Controller)
                    continue;

                if ((isLeft && sourceState.source.handedness == InteractionSourceHandedness.Left) ||
                    (!isLeft && sourceState.source.handedness == InteractionSourceHandedness.Right)) {
                        Vector2 touchPadPosition = sourceState.touchpadPosition;
                    float touchPadButton =
                        sourceState.touchpadPressed ? 1 :
                        (sourceState.touchpadTouched ? 0 : -1);
                    touchPad = new Vector3(touchPadPosition.x, touchPadPosition.y, touchPadButton);

                    Vector2 joystickPosition = sourceState.thumbstickPosition;
                    float joystickButton = sourceState.thumbstickPressed ? 1 : 0;
                    joystick = new Vector3(joystickPosition.x, joystickPosition.y, joystickButton);

                    trigger = sourceState.selectPressedAmount;
                    gripButton = sourceState.grasped ? 1 : 0;

                    menuButton = sourceState.menuPressed ? 1 : 0;

                }
            }

        }
#endif
    }
}