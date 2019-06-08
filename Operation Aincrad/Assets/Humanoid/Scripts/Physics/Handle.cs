using UnityEngine;

namespace Passer {

    public class Handle : MonoBehaviour {
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;

        public enum Hand {
            Both,
            Left,
            Right
        }
        public Hand hand;

        public enum GrabType {
            DefaultGrab,
            BarGrab,
            NoGrab
        }
        public GrabType grabType;

        public float range = 0.2f;

        public Humanoid.Pose pose;

#if hNEARHANDLE
        public bool useNearPose;
        public int nearPose;
#endif
        public Vector3 GetWorldPosition() {
            return transform.TransformPoint(position);
        }

        public HandTarget handTarget;

        public void Grab() {
            if (handTarget != null && handTarget.grabbedObject == null) {
                Debug.Log("Grab" + handTarget + " " + handTarget.grabbedObject + " " + this.gameObject);
                HandInteraction.Grab(handTarget, this.gameObject, false);
            }
        }


        public void LetGo() {
            if (handTarget != null && handTarget.grabbedObject != null) {
                Debug.Log("LetGo" + handTarget + " " + handTarget.grabbedObject);
                HandInteraction.LetGo(handTarget);
            }
        }

        public void CheckHandTarget() {
            if (handTarget == null)
                return;

            GrabMe(this);
        }

        private static void GrabMe(Handle handle) {
            HandInteraction.MoveHandTargetToHandle(handle.handTarget, handle);
            handle.SetHandPose(handle.handTarget);
            //if (handle.setHandPose)
                //handle.handTarget.poses.Set(handle.handPose, 1);
        }

        public void SetHandPose(HandTarget handTarget) {
            if (handTarget == null || pose == null)
                return;

            //Humanoid.MixedPose mixedPose = handTarget.poseMixer.Add(pose);
            //handTarget.poseMixer.SetPoseValue(mixedPose, 1);
            handTarget.SetPose(pose);
        }

        public void UpdateRotationPosition() {
            rotation = Quaternion.Inverse(Quaternion.Inverse(handTarget.palmRotation) * transform.rotation).eulerAngles;
            position = transform.InverseTransformPoint(handTarget.palmPosition);
        }

        public Vector3 worldPosition {
            get {
                return transform.TransformPoint(position);
            }
        }
        public Quaternion worldRotation {
            get {
                return transform.rotation * Quaternion.Euler(rotation);
            }
        }

        public Vector3 TranslationTo(Vector3 position) {
            Vector3 handlePosition = worldPosition;
            Vector3 translation = position - handlePosition;
            return translation;
        }

        public Quaternion RotationTo(Quaternion orientation) {
            Quaternion handleOrientation = worldRotation;
            Quaternion rotation = Quaternion.Inverse(handleOrientation) * orientation;
            return rotation;
        }

#if hNEARHANDLE
        private BasicHandPhysics nearHand;

        public void OnTriggerEnter(Collider other) {
            Rigidbody rigidbody = other.attachedRigidbody;
            if (rigidbody == null)
                return;

            nearHand = rigidbody.GetComponent<BasicHandPhysics>();
        }

        private void Update() {
            if (nearHand != null) {
                Vector3 handlePosition = transform.TransformPoint(position);
                float distance = Vector3.Distance(nearHand.target.handPalm.position, handlePosition) * 2;
                float f = Mathf.Clamp01((distance + 0.25F) / range);
                f = f * f * f;
                nearHand.target.SetHandPose(nearPose, 1 - f);
                if (1 - f <= 0) {
                    nearHand.target.SetHandPose1(1);
                    nearHand = null;
                }
            }
        }
#endif
        #region Gizmos
        void OnDrawGizmos() {
            if (enabled) {
                Matrix4x4 m = Matrix4x4.identity;
                Vector3 p = transform.TransformPoint(position);
                Quaternion q = Quaternion.Euler(rotation);
                m.SetTRS(p, transform.rotation * q, Vector3.one);
                Gizmos.color = Color.yellow;
                Gizmos.matrix = m;

                Gizmos.DrawCube(Vector3.zero, new Vector3(0.02f, 0.06f, 0.02f));
            }
        }
        #endregion
    }
}
