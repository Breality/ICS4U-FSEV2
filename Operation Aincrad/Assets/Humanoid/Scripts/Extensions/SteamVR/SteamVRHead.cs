#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class SteamVRHead : UnityHeadSensor {
        public override string name {
            get { return "SteamVR HMD"; }
        }

        private SteamVrHmdComponent steamVrHmd;

        public override Status status {
            get {
                if (steamVrHmd == null)
                    return Status.Unavailable;
                return steamVrHmd.status;
            }
            set { steamVrHmd.status = value; }
        }

        #region Start
        public override void Init(HeadTarget headTarget) {
            base.Init(headTarget);
            if (headTarget.humanoid != null)
                tracker = headTarget.humanoid.steam;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = headTarget.humanoid.steam;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();

            if (sensorTransform != null) {
                steamVrHmd = sensorTransform.GetComponent<SteamVrHmdComponent>();
                if (steamVrHmd != null)
                    steamVrHmd.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            CreateSensorTransform("SteamVR HMD", headTarget.head2eyes, Quaternion.identity);
            SteamVrHmdComponent steamVrHmd = sensorTransform.GetComponent<SteamVrHmdComponent>();
            if (steamVrHmd == null)
                sensorTransform.gameObject.AddComponent<SteamVrHmdComponent>();
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (steamVrHmd == null) {
                UpdateTarget(headTarget.head.target, sensorTransform);
            }

            steamVrHmd.UpdateComponent();
            if (steamVrHmd.status != Status.Tracking)
                return;

            UpdateTarget(headTarget.head.target, steamVrHmd);
        }
        #endregion
    }
}
#endif