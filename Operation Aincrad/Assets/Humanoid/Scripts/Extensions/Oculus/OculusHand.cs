#if hOCULUS
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OculusHand : UnityArmController {
        public override string name {
            get { return "Oculus Controller"; }
        }

        public enum ControllerType {
            GearVR,
            OculusGo
        }
        public ControllerType controllerType;

        private OculusControllerComponent oculusController;

        public override Status status {
            get {
                if (oculusController == null)
                    return Status.Unavailable;
                return oculusController.status;
            }
            set { oculusController.status = value; }
        }

        #region Start
        public override void Init(HandTarget handTarget) {
            base.Init(handTarget);
            tracker = handTarget.humanoid.oculus;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = handTarget.humanoid.oculus;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();

            if (sensorTransform != null) {
                oculusController = sensorTransform.GetComponent<OculusControllerComponent>();
                if (oculusController != null)
                    oculusController.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
#if UNITY_ANDROID
            string controllerName;
            switch (controllerType) {
                case ControllerType.GearVR:
                    controllerName = "GearVR Controller";
                    break;
                case ControllerType.OculusGo:
                default:
                    controllerName = "OculusGo Controller";
                    break;
            }
            if (handTarget.isLeft)
                CreateSensorTransform(controllerName, new Vector3(-0.1F, -0.05F, 0.04F), Quaternion.Euler(180, 90, 90));
            else
                CreateSensorTransform(controllerName, new Vector3(0.1F, -0.05F, 0.04F), Quaternion.Euler(180, -90, -90));
#else
            if (handTarget.isLeft)
                CreateSensorTransform("Left Touch Controller", new Vector3(-0.1F, -0.05F, 0.04F), Quaternion.Euler(180, 90, 90));
            else
                CreateSensorTransform("Right Touch Controller", new Vector3(0.1F, -0.05F, 0.04F), Quaternion.Euler(180, -90, -90));
#endif
            OculusControllerComponent oculusController = sensorTransform.GetComponent<OculusControllerComponent>();
            if (oculusController == null)
                oculusController = sensorTransform.gameObject.AddComponent<OculusControllerComponent>();
            oculusController.isLeft = handTarget.isLeft;
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (oculusController == null) {
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            oculusController.UpdateComponent();
            if (oculusController.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, oculusController);

#if UNITY_ANDROID
            // arm model: position is calculated from rotation
            Quaternion hipsYRotation = Quaternion.AngleAxis(handTarget.humanoid.hipsTarget.transform.eulerAngles.y, handTarget.humanoid.up);

            Vector3 pivotPoint = handTarget.humanoid.hipsTarget.transform.position + hipsYRotation * (handTarget.isLeft ? new Vector3(-0.25F, 0.15F, -0.05F) : new Vector3(0.25F, 0.15F, -0.05F));
            Quaternion forearmRotation = handTarget.hand.target.transform.rotation * (handTarget.isLeft ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0));

            Vector3 localForearmDirection = handTarget.humanoid.hipsTarget.transform.InverseTransformDirection(forearmRotation * Vector3.forward);

            if (localForearmDirection.x < 0 || localForearmDirection.y > 0) {
                pivotPoint += hipsYRotation * Vector3.forward * Mathf.Lerp(0, 0.15F, -localForearmDirection.x * 3 + localForearmDirection.y);
            }
            if (localForearmDirection.y > 0) {
                pivotPoint += hipsYRotation * Vector3.up * Mathf.Lerp(0, 0.2F, localForearmDirection.y);
            }

            if (localForearmDirection.z < 0.2F) {
                localForearmDirection = new Vector3(localForearmDirection.x, localForearmDirection.y, 0.2F);
                forearmRotation = Quaternion.LookRotation(handTarget.humanoid.hipsTarget.transform.TransformDirection(localForearmDirection), forearmRotation * Vector3.up);
            }

            handTarget.hand.target.transform.position = pivotPoint + forearmRotation * Vector3.forward * handTarget.forearm.bone.length;

            sensorTransform.position = handTarget.hand.target.transform.TransformPoint(-sensor2TargetPosition);
#endif

            UpdateInput();
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
            controllerInputSide.stickHorizontal = oculusController.joystick.x;
            controllerInputSide.stickVertical = oculusController.joystick.y;
            controllerInputSide.stickButton |= (oculusController.joystick.z > 0.5F);
            controllerInputSide.stickTouch |= (oculusController.joystick.z > -0.5F);

            controllerInputSide.buttons[0] |= (oculusController.buttonAX > 0.5F);
            controllerInputSide.buttons[1] |= (oculusController.buttonBY > 0.5F);

            controllerInputSide.trigger1 = oculusController.indexTrigger;
            controllerInputSide.trigger2 = oculusController.handTrigger;
        }

        public override void Vibrate(float length, float strength) {
            if (controller != null)
                controller.Vibrate(length, strength);
        }
        #endregion
    }


}
#endif