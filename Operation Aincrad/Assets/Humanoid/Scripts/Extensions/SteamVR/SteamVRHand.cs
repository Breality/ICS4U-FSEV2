#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class SteamVRHand : UnityArmController {
        public override string name {
            get { return "Steam VR Controller"; }
        }

        public SteamVrControllerComponent steamVrController;

        public override Status status {
            get {
                if (steamVrController == null)
                    return Status.Unavailable;
                return steamVrController.status;
            }
            set { steamVrController.status = value; }
        }

        #region Start
        public override void Init(HandTarget handTarget) {
            base.Init(handTarget);
            tracker = handTarget.humanoid.steam;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = handTarget.humanoid.steam;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();

            if (sensorTransform != null) {
                steamVrController = sensorTransform.GetComponent<SteamVrControllerComponent>();
                if (steamVrController != null)
                    steamVrController.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            if (handTarget.isLeft)
                CreateSensorTransform("Vive Controller", new Vector3(-0.14F, -0.04F, 0.08F), Quaternion.Euler(0, -30, -90));
            else
                CreateSensorTransform("Vive Controller", new Vector3(0.14F, -0.04F, 0.08F), Quaternion.Euler(0, 30, 90));

            SteamVrControllerComponent steamVrController = sensorTransform.GetComponent<SteamVrControllerComponent>();
            if (steamVrController == null)
                steamVrController = sensorTransform.gameObject.AddComponent<SteamVrControllerComponent>();
            steamVrController.isLeft = handTarget.isLeft;
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (steamVrController == null) {
                UpdateTarget(handTarget.hand.target, sensorTransform);
                return;
            }

            steamVrController.UpdateComponent();
            if (steamVrController.status != Status.Tracking)
                return;

            UpdateTarget(handTarget.hand.target, steamVrController);
            UpdateInput();
        }

        protected void UpdateInput() {
            if (handTarget.isLeft)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        protected void SetControllerInput(ControllerSide controllerSide) {
            controllerSide.stickHorizontal += steamVrController.touchPad.x;
            controllerSide.stickVertical += steamVrController.touchPad.y;
            controllerSide.stickButton |= (steamVrController.touchPad.z > 0.5F);

            controllerSide.buttons[0] |= steamVrController.aButton > 0.5F;
            controllerSide.buttons[1] |= steamVrController.menuButton > 0.5F;

            controllerSide.trigger1 += steamVrController.trigger;
            controllerSide.trigger2 += steamVrController.gripButton;

            controllerSide.option |= steamVrController.menuButton > 0.5F;
        }
        #endregion

        //length is how long the vibration should go for in seconds
        //strength is vibration strength from 0-1
        public override void Vibrate(float length, float strength) {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            for (float i = 0; i < length; i += Time.deltaTime)
                controller.Vibrate(length, strength);
        }
    }
}

#endif