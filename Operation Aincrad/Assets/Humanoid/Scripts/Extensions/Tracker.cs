using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class Tracker {
        public HumanoidControl humanoid;
        public System.IntPtr trackerDevice;
        #region Device
        public virtual Vector3 GetBonePosition(uint actorId, Bone boneId) { return Vector3.zero; }
        public virtual Quaternion GetBoneRotation(uint actorId, Bone boneId) { return Quaternion.identity; }
        public virtual float GetBoneConfidence(uint actorId, Bone boneId) { return 0; }

        public virtual Vector3 GetBonePosition(uint actorId, FacialBone boneId) { return Vector3.zero; }
        public virtual Quaternion GetBoneRotation(uint actorId, FacialBone boneId) { return Quaternion.identity; }
        public virtual float GetBoneConfidence(uint actorId, FacialBone boneId) { return 0; }
        #endregion

        public DeviceView deviceView = new DeviceView();

        public const string _name = "";
        public virtual string name { get { return _name; } }
        public Status status;
        public bool enabled;

        public Transform trackerTransform;
        public SubTracker[] subTrackers;

        public virtual void Enable() {
            enabled = true;
        }

        public virtual UnityHeadSensor headSensor {
            get { return null; }
        }
        public virtual UnityArmSensor leftHandSensor {
            get { return null; }
        }
        public virtual UnityArmSensor rightHandSensor {
            get { return null; }
        }
        public virtual UnityTorsoSensor hipsSensor {
            get { return null; }
        }
        public virtual UnityLegSensor leftFootSensor {
            get { return null; }
        }
        public virtual UnityLegSensor rightFootSensor {
            get { return null; }
        }
        private UnitySensor[] _sensors = new UnitySensor[0];
        public virtual UnitySensor[] sensors {
            get { return _sensors; }
        }

        #region Start
        public void Init(Transform _trackerTransform) {
            trackerTransform = _trackerTransform;
        }
        #endregion


        public virtual bool AddTracker(HumanoidControl humanoid, string resourceName) {
            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);

            trackerTransform = FindTrackerObject(realWorld, name);
            if (trackerTransform == null) {
                GameObject model = Resources.Load(resourceName) as GameObject;

                if (model != null) {
                    GameObject trackerObject = GameObject.Instantiate(model);
                    trackerObject.name = name;
                    trackerTransform = trackerObject.transform;
                }
                else {
                    GameObject trackerObject = new GameObject(name);
                    trackerTransform = trackerObject.transform;
                }
                trackerTransform.parent = realWorld.transform;
                trackerTransform.position = humanoid.transform.position;
                trackerTransform.rotation = humanoid.transform.rotation;
                return true;
            }
            return false;
        }

        public virtual bool AddTracker(GameObject realWorld, string resourceName) {
            trackerTransform = FindTrackerObject(realWorld, name);
            if (trackerTransform == null) {
                GameObject model = Resources.Load(resourceName) as GameObject;

                if (model != null) {
                    GameObject trackerObject = GameObject.Instantiate(model);
                    trackerObject.name = name;
                    trackerTransform = trackerObject.transform;
                }
                else {
                    GameObject trackerObject = new GameObject(name);
                    trackerTransform = trackerObject.transform;
                }
                trackerTransform.parent = realWorld.transform;
                trackerTransform.localPosition = Vector3.zero;
                trackerTransform.localRotation = Quaternion.identity;
                return true;
            }
            return false;
        }

        public static Transform FindTrackerObject(GameObject realWorld, string trackerName) {
            Transform[] ancestors = realWorld.GetComponentsInChildren<Transform>();
            for (int i = 0; i < ancestors.Length; i++) {
                if (ancestors[i].name == trackerName)
                    return ancestors[i].transform;
            }
            return null;
        }

        public virtual void ShowTracker(bool shown) {
            if (trackerTransform != null)
                ShowTracker(trackerTransform.gameObject, shown);
        }

        public static void ShowTracker(GameObject trackerObject, bool enabled) {
            if (trackerObject == null)
                return;

            Renderer[] renderers = trackerObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                renderer.enabled = enabled;
        }

        #region Start
        public virtual void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);
            Init(realWorld.transform);
        }

        public virtual void StartTracker(Transform trackerTransform) {
            GameObject realWorld = HumanoidControl.GetRealWorld(trackerTransform);
            Init(realWorld.transform);
        }
        #endregion

        public virtual void UpdateTracker() { }

        public virtual void UpdateSubTracker(int i) {
            if (subTrackers[i] != null)
                subTrackers[i].UpdateTracker(humanoid.showRealObjects);
        }

        protected virtual Vector3 GetSubTrackerPosition(int i) {
            return Vector3.zero;
        }

        protected virtual Quaternion GetSubTrackerRotation(int i) {
            return Quaternion.identity;
        }

        public virtual void StopTracker() { }

        public Vector3 ToWorldPosition(Vector3 localPosition) {
            return trackerTransform.transform.position + trackerTransform.transform.rotation * localPosition;
        }

        public Quaternion ToWorldOrientation(Quaternion localRotation) {
            return trackerTransform.transform.rotation * localRotation;
        }

        public virtual void Calibrate() { }

        public virtual void AdjustTracking(Vector3 v, Quaternion q) {
            if (trackerTransform != null) {
                trackerTransform.position += v;
                trackerTransform.rotation *= q;
            }
        }

    }

    public abstract class SubTracker : MonoBehaviour {
        public Tracker tracker;
        public int subTrackerId = -1;

        public abstract bool IsPresent();
        public abstract void UpdateTracker(bool showRealObjects);
    }
}
