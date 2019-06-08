#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;
using Valve.VR;

namespace Passer {
    using Humanoid.Tracking;

    public class SteamVRTrackerComponent : MonoBehaviour {
        public SteamVRTracker tracker = new SteamVRTracker();

        private void Start() {
            tracker.trackerTransform = this.transform;
            tracker.StartTracker();
        }

        private void Update() {
            tracker.UpdateTracker();
        }
    }

    [System.Serializable]
    public class SteamVRTracker : Tracker {
        public SteamVRTracker() {
            deviceView = new DeviceView();
        }

        public override string name {
            get { return SteamDevice.name; }
        }

        public override UnityHeadSensor headSensor {
            get { return humanoid.headTarget.steamVR; }
        }
        public override UnityArmSensor leftHandSensor {
            get { return humanoid.leftHandTarget.steamVR; }
        }
        public override UnityArmSensor rightHandSensor {
            get { return humanoid.rightHandTarget.steamVR; }
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
#if hVIVETRACKER
        public UnityHeadSensor headSensorVive {
            get { return humanoid.headTarget.viveTracker; }
        }
        public UnityArmSensor leftHandSensorVive {
            get { return humanoid.leftHandTarget.viveTracker; }
        }
        public UnityArmSensor rightHandSensorVive {
            get { return humanoid.rightHandTarget.viveTracker; }
        }
        public UnityTorsoSensor hipsSensorVive {
            get { return humanoid.hipsTarget.viveTracker; }
        }
        public UnityLegSensor leftFootSensorVive {
            get { return humanoid.leftFootTarget.viveTracker; }
        }
        public UnityLegSensor rightFootSensorVive {
            get { return humanoid.rightFootTarget.viveTracker; }
        }
#endif

        public override bool AddTracker(HumanoidControl humanoid, string resourceName) {
            bool trackerAdded = base.AddTracker(humanoid, resourceName);
            if (trackerAdded) {
                /*SteamVRTrackerComponent trackerComponent = */
                trackerTransform.gameObject.AddComponent<SteamVRTrackerComponent>();
            }
            return trackerAdded;
        }

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR)
                return;

            TraditionalDevice.gameControllerEnabled = false;
            // Game controllers interfere with SteamVR Controller Input ... :-(

            SteamDevice.Start();

            AddTracker(humanoid, "SteamVR");
            StartLighthouses();
#if hVIVETRACKER
            Debug.Log("Detecting Vive Tracker positions.\nMake sure the Vive HMD is looking in the same direction as the user!");
#endif
        }

        public void StartTracker() {
            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR)
                return;

            TraditionalDevice.gameControllerEnabled = false;
            // Game controllers interfere with SteamVR Controller Input ... :-(

            SteamDevice.Start();

            StartLighthouses();
        }
        #endregion

        public override void ShowTracker(bool shown) {
            if (!enabled)
                return;
#if hVIVETRACKER
            ViveTracker.ShowTracker(humanoid, shown);
#endif
        }

        private void StartLighthouses() {
            subTrackers = new SubTracker[2];
            subTrackers[0] = SteamVRSubTracker.Create(this);
            subTrackers[1] = SteamVRSubTracker.Create(this);
        }

        #region Update
        public override void UpdateTracker() {
            if (!enabled || UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.OpenVR) {
                status = Status.Unavailable;
                return;
            }

            status = SteamDevice.status;

            SteamDevice.Update();

            deviceView.position = Target.ToVector(trackerTransform.position);
            deviceView.orientation = Target.ToRotation(trackerTransform.rotation);

            UpdateLighthouses();
        }

        private void UpdateLighthouses() {
            UpdateLighthouse(0);
            UpdateLighthouse(1);
        }

        private void UpdateLighthouse(int index) {
            FindLighthouses();

            bool showRealObjects = humanoid == null ? true : humanoid.showRealObjects;
            for (int i = 0; i < 2; i++)
                subTrackers[i].UpdateTracker(showRealObjects);
        }

        private void FindLighthouses() {
            if (subTrackers[1].subTrackerId != -1)
                return;

            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++) {
                if (SteamDevice.GetDeviceClass(i) != ETrackedDeviceClass.TrackingReference)
                    continue;
                if (subTrackers[0].subTrackerId == i || subTrackers[1].subTrackerId == i)
                    continue;
                if (subTrackers[0].subTrackerId == -1)
                    subTrackers[0].subTrackerId = i;
                else if (subTrackers[1].subTrackerId == -1)
                    subTrackers[1].subTrackerId = i;
            }
        }

        private bool IsTracking() {
            if (!humanoid.leftHandTarget.steamVR.enabled || humanoid.leftHandTarget.steamVR.status == Status.Tracking ||
#if hVIVETRACKER
                humanoid.headTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.rightHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftHandTarget.viveTracker.status == Status.Tracking ||
                humanoid.hipsTarget.viveTracker.status == Status.Tracking ||
                humanoid.leftFootTarget.viveTracker.status == Status.Tracking ||
                humanoid.rightFootTarget.viveTracker.status == Status.Tracking ||
#endif
                !humanoid.rightHandTarget.steamVR.enabled || humanoid.rightHandTarget.steamVR.status == Status.Tracking)
                return true;
            else
                return false;
        }
        #endregion 

        public static GameObject CreateControllerObject() {
            GameObject trackerPrefab = Resources.Load("Vive Controller") as GameObject;
            if (trackerPrefab == null)
                return null;

            GameObject trackerObject = Object.Instantiate(trackerPrefab);
            trackerObject.name = "Vive Controller";

            return trackerObject;
        }

        public override void Calibrate() {
            SteamDevice.ResetSensors();
        }

        public override void AdjustTracking(Vector3 v, Quaternion q) {
            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }

        public Transform GetTrackingTransform() {
            if (!enabled || subTrackers[0].subTrackerId == -1)
                return null;
            return subTrackers[0].transform;
        }

        public void SyncTracking(Vector3 position, Quaternion rotation) {
            if (!enabled)
                return;


            // rotation

            // Not stable
            //Quaternion deltaRotation = Quaternion.Inverse(lighthouses[0].transform.rotation) * rotation;
            //unityVRroot.rotation *= deltaRotation;

            // stable
            float angle = (-subTrackers[0].transform.eulerAngles.y) + rotation.eulerAngles.y;
            trackerTransform.Rotate(Vector3.up, angle, Space.World);

            // position
            Vector3 deltaPosition = position - subTrackers[0].transform.position;

            trackerTransform.Translate(deltaPosition, Space.World);
        }
    }
}
#endif