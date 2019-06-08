#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class OculusHead : UnityHeadSensor {
        public override string name {
            get { return "Oculus HMD"; }
        }

        private OculusHmdComponent oculusHmd;

        public override Status status {
            get {
                if (oculusHmd == null)
                    return Status.Unavailable;
                return oculusHmd.status;
            }
            set { oculusHmd.status = value; }
        }

        public bool overrideOptitrackPosition = true;

        #region Start
        public override void Init(HeadTarget headTarget) {
            base.Init(headTarget);
            if (headTarget.humanoid != null)
                tracker = headTarget.humanoid.oculus;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            tracker = headTarget.humanoid.oculus;

            if (tracker == null || !tracker.enabled || !enabled)
                return;

            SetSensor2Target();
            CheckSensorTransform();

            if (sensorTransform != null) {
                oculusHmd = sensorTransform.GetComponent<OculusHmdComponent>();
                if (oculusHmd != null)
                    oculusHmd.StartComponent(tracker.trackerTransform);
            }
        }

        protected override void CreateSensorTransform() {
            CreateSensorTransform("Oculus HMD", headTarget.head2eyes, Quaternion.identity);

            OculusHmdComponent oculusHmd = sensorTransform.GetComponent<OculusHmdComponent>();
            if (oculusHmd == null)
                sensorTransform.gameObject.AddComponent<OculusHmdComponent>();
        }
        #endregion

        #region Update
        bool calibrated = false;

        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            if (oculusHmd == null) {
                //UpdateHeadTarget(headTarget.head.target, sensorTransform);
                return;
            }

            oculusHmd.UpdateComponent();
            if (oculusHmd.status != Status.Tracking)
                return;

            if (!calibrated && tracker.humanoid.calibrateAtStart) {
                tracker.humanoid.Calibrate();
                calibrated = true;
            }

            if (oculusHmd.rotationConfidence < headTarget.head.target.confidence.rotation)
                FuseRotation();

            if (oculusHmd.positionConfidence < headTarget.head.target.confidence.position && !overrideOptitrackPosition)
                FusePosition();

            UpdateTarget(headTarget.head.target, oculusHmd);
        }

        // Oculus has no positional tracking and drift correction
        private void FuseRotation() {
        Quaternion oculusHeadRotation = GetTargetRotation(oculusHmd.transform);
            Quaternion rotation = Quaternion.FromToRotation(oculusHeadRotation * Vector3.forward, headTarget.head.target.transform.forward);
            float rotY = Angle.Normalize(rotation.eulerAngles.y);
            if (rotY > 10 || rotY < -10)
                // we do snap rotation for large differences
                tracker.trackerTransform.RotateAround(headTarget.head.target.transform.position, headTarget.humanoid.up, rotY);
            else
                tracker.trackerTransform.RotateAround(headTarget.head.target.transform.position, headTarget.humanoid.up, rotY * 0.001F);
        }

        // Oculus has no positional tracking and drift correction
        private void FusePosition() {
            Vector3 oculusHeadPosition = GetTargetPosition(oculusHmd.transform);
            Vector3 delta = headTarget.head.target.transform.position - oculusHeadPosition;
            tracker.trackerTransform.transform.position += delta; // (delta * 0.01F);
        }
        #endregion
    }
}
#endif