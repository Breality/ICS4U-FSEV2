#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class WindowsMRHand : UnityArmController {

        public override string name {
            get { return "Windows MR Controller"; }
        }

        public WindowsMRControllerComponent mixedRealityController;

        public override Status status {
            get {
                if (mixedRealityController == null)
                    return Status.Unavailable;
                return mixedRealityController.status;
            }
            set { mixedRealityController.status = value; }
        }

#region Start
        public override void Init(HandTarget _handTarget) {
            base.Init(_handTarget);
            tracker = handTarget.humanoid.mixedReality;
        }
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = handTarget.humanoid.mixedReality;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();

            if (sensorTransform != null) {
                mixedRealityController = sensorTransform.GetComponent<WindowsMRControllerComponent>();
                if (mixedRealityController != null)
                    mixedRealityController.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            if (handTarget.isLeft)
                CreateSensorTransform("Windows MR Controller Left", new Vector3(-0.10F, -0.04F, 0.02F), Quaternion.Euler(0, -30, -90));
            else
                CreateSensorTransform("Windows MR Controller Right", new Vector3(0.10F, -0.04F, 0.02F), Quaternion.Euler(0, 30, 90));

            WindowsMRControllerComponent mixedRealityController = sensorTransform.GetComponent<WindowsMRControllerComponent>();
            if (mixedRealityController == null)
                mixedRealityController = sensorTransform.gameObject.AddComponent<WindowsMRControllerComponent>();
            mixedRealityController.isLeft = handTarget.isLeft;
        }
#endregion

#region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (mixedRealityController == null) {
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            mixedRealityController.UpdateComponent();
            if (mixedRealityController.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, mixedRealityController);
            UpdateInput();
        }

        protected void UpdateInput() {
            if (handTarget.isLeft)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        protected void SetControllerInput(ControllerSide controllerSide) {
            controllerSide.stickHorizontal += mixedRealityController.touchPad.x + mixedRealityController.joystick.x;
            controllerSide.stickVertical += mixedRealityController.touchPad.y + mixedRealityController.joystick.y;
            controllerSide.stickButton |= (mixedRealityController.touchPad.z > 0.5F || mixedRealityController.joystick.z > 0.5F) ;

            controllerSide.buttons[0] |= mixedRealityController.menuButton > 0.5F;

            controllerSide.trigger1 += mixedRealityController.trigger;
            controllerSide.trigger2 += mixedRealityController.gripButton;

            controllerSide.option |= mixedRealityController.menuButton > 0.5F;
        }
#endregion
    }
}
#endif