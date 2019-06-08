#if hSTEAMVR
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class SteamVRSubTracker : SubTracker {

        public static SteamVRSubTracker Create(Tracker tracker) {            
            Object lighthousePrefab = Resources.Load("Lighthouse");
            GameObject lighthouseObject = (GameObject)Instantiate(lighthousePrefab);
            lighthouseObject.transform.parent = tracker.trackerTransform;

            lighthouseObject.SetActive(false);

            SteamVRSubTracker subTracker = lighthouseObject.AddComponent<SteamVRSubTracker>();
            subTracker.tracker = tracker;

            return subTracker;
        }

        public override bool IsPresent() {
            bool isPresent = SteamDevice.IsPresent(subTrackerId);
            return isPresent;
        }

        public override void UpdateTracker(bool showRealObjects) {
            if (subTrackerId == -1)
                return;

            bool isPresent = IsPresent();
            if (!isPresent)
                return;

            gameObject.SetActive(showRealObjects);

            transform.localPosition = Target.ToVector3(SteamDevice.GetPosition(subTrackerId));
            transform.localRotation = Target.ToQuaternion(SteamDevice.GetRotation(subTrackerId));
        }
    }
}
#endif