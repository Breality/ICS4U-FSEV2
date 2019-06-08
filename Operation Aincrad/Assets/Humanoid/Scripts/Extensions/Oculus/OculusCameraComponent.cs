using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class OculusCameraComponent : SubTracker {
#if hOCULUS
        public static OculusCameraComponent Create(Tracker tracker) {
            GameObject cameraObject;
            Object cameraPrefab = Resources.Load("Oculus Camera");
            if (cameraPrefab == null)
                cameraObject = new GameObject();            
            else
                cameraObject = (GameObject)Instantiate(cameraPrefab);
            cameraObject.name = "Oculus Camera";
            cameraObject.transform.parent = tracker.trackerTransform;

            OculusCameraComponent cameraComponent = cameraObject.AddComponent<OculusCameraComponent>();
            cameraComponent.tracker = tracker;

            return cameraComponent;
        }

        public static int GetCount() {
            int count = 0;

            for (int i = 0; i < (int)OculusDevice.Tracker.Count; ++i) {
                if (OculusDevice.IsPresent(Sensor.ID.Tracker1 + i))
                    count++;
            }

            return count;
        }
#endif
        public override bool IsPresent() {
#if hOCULUS
            return OculusDevice.IsPresent(Sensor.ID.Tracker1 + subTrackerId);
#else
            return false;
#endif
        }

        public override void UpdateTracker(bool showRealObjects) {
#if hOCULUS
            gameObject.SetActive(showRealObjects && IsPresent());

            Vector3 localPosition = Target.ToVector3(OculusDevice.GetPosition(Sensor.ID.Tracker1 + subTrackerId));
            Quaternion localRotation = Target.ToQuaternion(OculusDevice.GetRotation(Sensor.ID.Tracker1 + subTrackerId));
            transform.position = tracker.trackerTransform.TransformPoint(localPosition);
            transform.rotation = tracker.trackerTransform.rotation * localRotation;
#endif
        }
    }
}
