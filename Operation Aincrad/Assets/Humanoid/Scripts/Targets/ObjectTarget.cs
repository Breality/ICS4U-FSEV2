/*
using UnityEngine;

namespace Passer {
    public class ObjectTarget : Target {

        #region Sensors
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public SteamVrHandController steamVR = new SteamVrHandController();
#if hVIVETRACKER
        public ViveTrackerSensor viveTracker = new ViveTrackerSensor();
#endif
#endif
#if hOCULUS
        public OculusController oculus = new OculusController();
#endif
#if hHYDRA
        public RazerHydraController hydra = new RazerHydraController();
#endif
        public UnitySensor[] sensors;

        public override void InitSensors() {
            if (sensors == null) {
                sensors = new UnitySensor[] {
#if hHYDRA
                    hydra,
#endif
#if hOCULUS
                    oculus,
#endif
//#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
//                    mrHand,
//#endif
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                    steamVR,
#if hVIVETRACKER
                    viveTracker,
#endif
#endif
                    null
                    };
            }
        }

        public override void StartSensors() {
            for (int i = 0; i < sensors.Length - 1; i++)
                sensors[i].Start(this.transform);
        }

        protected override void UpdateSensors() {
            for (int i = 0; i < sensors.Length - 1; i++)
                sensors[i].Update();
        }

        public override void UpdateMovements() {
        }

        public override void StartTarget() {
            InitSensors();
        }

        public override Transform GetDefaultTarget() {
            return this.transform;
        }
        #endregion

        #region Start
        public void Awake() {
            StartTarget();
        }

        public void Start() {
            StartSensors();
        }
        #endregion

        #region Update
        public void Update() {
            UpdateSensors();
        }

        public override void UpdateTarget() {            
        }

        #endregion
    }
}
*/