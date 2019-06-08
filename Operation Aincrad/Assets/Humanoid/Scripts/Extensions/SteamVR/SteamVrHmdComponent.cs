using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class SteamVrHmdComponent : SensorComponent {
#if hSTEAMVR
        public int trackerId = 0;

        public override void UpdateComponent() {
            if (SteamDevice.status == Status.Unavailable)
                status = Status.Unavailable;

            if (SteamDevice.GetConfidence(trackerId) == 0) {
                status = SteamDevice.IsPresent(trackerId) ? Status.Present : Status.Unavailable;
                positionConfidence = 0;
                rotationConfidence = 0;
                gameObject.SetActive(false);
                return;
            }

            status = Status.Tracking;
            Vector3 localSensorPosition = Target.ToVector3(SteamDevice.GetPosition(trackerId));
            Quaternion localSensorRotation = Target.ToQuaternion(SteamDevice.GetRotation(trackerId));
            transform.position = trackerTransform.TransformPoint(localSensorPosition);
            transform.rotation = trackerTransform.rotation * localSensorRotation;

            positionConfidence = SteamDevice.GetConfidence(trackerId);
            rotationConfidence = SteamDevice.GetConfidence(trackerId);
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