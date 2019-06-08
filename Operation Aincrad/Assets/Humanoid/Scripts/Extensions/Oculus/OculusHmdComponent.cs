using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class OculusHmdComponent : SensorComponent {
#if hOCULUS
        private Sensor.ID sensorId = Sensor.ID.Head;

        public override void UpdateComponent() {
            if (OculusDevice.GetRotationalConfidence(sensorId) == 0) {
                status = OculusDevice.IsPresent(0) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;

            Vector3 localPosition = Target.ToVector3(OculusDevice.GetPosition(sensorId));
            Quaternion localRotation = Target.ToQuaternion(OculusDevice.GetRotation(sensorId));
            transform.position = trackerTransform.TransformPoint(localPosition);
            transform.rotation = trackerTransform.rotation * localRotation;

            positionConfidence = OculusDevice.GetPositionalConfidence(sensorId);
            rotationConfidence = OculusDevice.GetRotationalConfidence(sensorId);
            gameObject.SetActive(true);

            FuseWithUnityCamera();
        }

        protected virtual void FuseWithUnityCamera() {
            Vector3 deltaPos = Camera.main.transform.position - transform.position;

            trackerTransform.position += deltaPos;
        }
#endif
    }
}