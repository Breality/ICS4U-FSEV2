#if hOCULUS

using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OculusTracker : Tracker {
        public OculusTracker() {
            deviceView = new DeviceView();
        }

        public override string name {
            get { return OculusDevice.name; }
        }

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.oculus; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.oculus; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.oculus; }
        }
        [System.NonSerialized]
        private UnitySensor[] _sensors;
        public override UnitySensor[] sensors {
            get {
                if (_sensors == null)
                    _sensors = new UnitySensor[] {
                        headSensor,
                        leftHandSensor,
                        rightHandSensor
                    };

                return _sensors;
            }
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.Oculus)
                return;

            OculusDevice.Start();

            AddTracker(humanoid, "Oculus");
            StartCameras(trackerTransform);
        }

        private void StartCameras(Transform trackerTransform) {
            subTrackers = new OculusCameraComponent[(int)OculusDevice.Tracker.Count];
            for (int i = 0; i < OculusCameraComponent.GetCount(); i++) {
                subTrackers[i] = OculusCameraComponent.Create(this);
                subTrackers[i].subTrackerId = i;
            }
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled || trackerTransform == null)
                return;

            if (UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.Oculus) {
                status = Status.Unavailable;
                return;
            }

            status = OculusDevice.status;

            trackerTransform.localPosition = new Vector3(0, OculusDevice.eyeHeight, 0);
            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            OculusDevice.Update();

            for (int i = 0; i < OculusCameraComponent.GetCount(); i++) {
                if (subTrackers[i] != null)
                    subTrackers[i].UpdateTracker(humanoid.showRealObjects);
            }
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