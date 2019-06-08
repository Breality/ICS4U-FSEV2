#if hVRTK
using UnityEngine;
using VRTK;

namespace Passer {

    [System.Serializable]
    public class VrtkTracker : Tracker {
        public override string name {
            get { return "VRTK"; }
        }

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.vrtk; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.vrtk; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.vrtk; }
        }

        public VRTK_SDKManager sdkManager;

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            if (!enabled || sdkManager == null)
                return;

            sdkManager.LoadedSetupChanged += LoadedSetupChanged;
        }

        private void LoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
            VRTK_SDKSetup loadedSetup = sdkManager.loadedSetup;
            if (loadedSetup == null)
                return;

            trackerTransform = loadedSetup.actualBoundaries.transform;
            headSensor.sensorTransform = loadedSetup.actualHeadset.transform;

            VrtkHand.ControllerType controllerType = VrtkHand.ControllerType.None;
            if (loadedSetup.controllerSDK.GetType() == typeof(SDK_SteamVRController))
                controllerType = VrtkHand.ControllerType.SteamVRController;
            else if (loadedSetup.controllerSDK.GetType() == typeof(SDK_OculusController))
                controllerType = VrtkHand.ControllerType.OculusTouch;

            humanoid.leftHandTarget.vrtk.SetSensorTransform(loadedSetup.actualLeftController.transform, controllerType);
            humanoid.rightHandTarget.vrtk.SetSensorTransform(loadedSetup.actualRightController.transform, controllerType);
        }
        #endregion

        public override void AdjustTracking(Vector3 v, Quaternion q) {
            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }
    }
}
#endif