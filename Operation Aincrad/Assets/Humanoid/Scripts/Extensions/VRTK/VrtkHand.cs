#if hVRTK
using UnityEngine;

namespace Passer {

    [System.Serializable]
    public class VrtkHand : UnityArmController {
        public override string name {
            get { return "VRTK"; }
        }

        public enum ControllerType {
            None,
            SteamVRController,
            OculusTouch
        }

        private VRTK.VRTK_ControllerEvents vrtkControllerEvents;

        #region Start
        public override void Init(HandTarget _handTarget) {
            base.Init(_handTarget);
            tracker = handTarget.humanoid.vrtk;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            Init(handTarget);

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            sensor2TargetPosition = Vector3.zero;
            sensor2TargetRotation = Quaternion.AngleAxis(handTarget.isLeft ? 90 : -90, Vector3.up);
            sensorTransform = null;

            vrtkControllerEvents = handTarget.GetComponent<VRTK.VRTK_ControllerEvents>();
        }

        public void SetSensorTransform(Transform transform, ControllerType controllerType) {
            if (handTarget.isLeft) {
                switch (controllerType) {
                    case ControllerType.SteamVRController:
                        sensor2TargetPosition = new Vector3(0.16F, 0.04F, -0.055F);
                        sensor2TargetRotation = Quaternion.Euler(300, 0, 90);
                        break;
                    case ControllerType.OculusTouch:
                        sensor2TargetPosition = new Vector3(-0.09F, 0.05F, -0.015F);
                        sensor2TargetRotation = Quaternion.Euler(-90, 90, 0);
                        break;
                }
            }
            else {
                switch (controllerType) {
                    case ControllerType.SteamVRController:
                        sensor2TargetPosition = new Vector3(-0.16F, 0.04F, -0.055F);
                        sensor2TargetRotation = Quaternion.Euler(300, 0, -90);
                        break;
                    case ControllerType.OculusTouch:
                        sensor2TargetPosition = new Vector3(-0.09F, 0.05F, 0.015F);
                        sensor2TargetRotation = Quaternion.Euler(-90, -90, 0);
                        break;
                }
            }
            sensorTransform = transform;
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            UpdateHandTargetTransform();
            UpdateInput();
        }

        private void UpdateHandTargetTransform() {
            if (sensorTransform == null || handTarget.hand.target.transform == null)
                return;

            handTarget.hand.target.transform.rotation = sensorTransform.rotation * sensor2TargetRotation;
            handTarget.hand.target.transform.position = sensorTransform.position + handTarget.hand.target.transform.rotation * sensor2TargetPosition;
            handTarget.hand.target.confidence.position = 1;
            handTarget.hand.target.confidence.rotation = 1;
        }

        private void UpdateInput() {
            if (controllerInput == null)
                return;

            if (handTarget.isLeft)
                UpdateInputSide(controllerInput.left);
            else
                UpdateInputSide(controllerInput.right);
        }

        private void UpdateInputSide(ControllerSide controllerInputSide) {
            if (vrtkControllerEvents == null)
                return;

            controllerInputSide.stickButton = vrtkControllerEvents.touchpadPressed;
            controllerInputSide.stickTouch = vrtkControllerEvents.touchpadTouched;

            controllerInputSide.trigger1 = GetTrigger();
            controllerInputSide.trigger2 = GetGrip();
            controllerInputSide.buttons[0] = vrtkControllerEvents.buttonOnePressed;
            controllerInputSide.buttons[1] = vrtkControllerEvents.buttonTwoPressed;
            controllerInputSide.option = vrtkControllerEvents.startMenuPressed;
        }

        private float GetTrigger() {
            float triggerValue = 0;
            if (vrtkControllerEvents) {
                if (vrtkControllerEvents.triggerClicked)
                    triggerValue = 1;
                else if (vrtkControllerEvents.triggerPressed)
                    triggerValue = 0.6F;
                else if (vrtkControllerEvents.triggerTouched)
                    triggerValue = 0.2F;
            }
            return triggerValue;
        }

        private float GetGrip() {
            float gripValue = 0;
            if (vrtkControllerEvents) {
                if (vrtkControllerEvents.gripClicked)
                    gripValue = 1;
                else if (vrtkControllerEvents.gripPressed)
                    gripValue = 0.6F;
                else if (vrtkControllerEvents.gripTouched)
                    gripValue = 0.2F;
            }
            return gripValue;
        }

        #endregion

    }
}
#endif