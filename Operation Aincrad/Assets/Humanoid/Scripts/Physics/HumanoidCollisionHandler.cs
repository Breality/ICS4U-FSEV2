using UnityEngine;

namespace Passer {

    public class HumanoidCollisionHandler : MonoBehaviour {
        public HumanoidControl humanoid;

        public void OnCollisionEnter(Collision collision) {
            OnTriggerStay(collision.collider);
        }

        public void OnTriggerEnter(Collider otherCollider) {
            OnTriggerStay(otherCollider);
        }

        public void OnTriggerStay(Collider otherCollider) {
            Rigidbody rigidbody = otherCollider.attachedRigidbody;

            // static colliders
            if (rigidbody == null)
                humanoid.triggerEntered = true;

            if (!otherCollider.isTrigger && !humanoid.IsMyRigidbody(rigidbody)) {
                if (!humanoid.collided)
                    humanoid.hitNormal = DetermineHitNormal(humanoid.velocity);
                humanoid.triggerEntered = true;
            }
        }

        public void OnTriggerExit() {
            humanoid.triggerEntered = false;
        }

        private Vector3 DetermineHitNormal(Vector3 velocity) {
            CapsuleCollider cc = humanoid.bodyCapsule;
            Vector3 capsuleCenter = humanoid.hipsTarget.hips.bone.transform.position + cc.center;
            Vector3 capsuleOffset = ((cc.height - cc.radius) / 2) * (humanoid.hipsTarget.hips.bone.transform.rotation * humanoid.hipsTarget.hips.bone.toTargetRotation * Vector3.up);

            Vector3 backSweep = velocity.normalized * (cc.radius + 0.1F);
            Vector3 top = capsuleCenter + capsuleOffset - backSweep;
            Vector3 bottom = capsuleCenter - capsuleOffset - backSweep;

            Vector3 hitNormal;
            if (CapsulecastAllNormal(top, bottom, cc.radius, velocity.normalized, velocity.magnitude * Time.deltaTime + cc.radius + 0.1F, out hitNormal))
                return hitNormal;

            return -velocity.normalized;
        }

        private bool CapsulecastAllNormal(Vector3 top, Vector3 bottom, float radius, Vector3 direction, float maxDistance, out Vector3 hitNormal) {
            //Debug.DrawRay(top, direction.normalized * maxDistance);
            //Debug.DrawRay(bottom, direction.normalized * maxDistance);
            RaycastHit[] hits = Physics.CapsuleCastAll(top, bottom, radius, direction, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            hitNormal = Vector3.zero;
            for (int i = 0; i < hits.Length; i++) {
                if (!hits[i].collider.isTrigger && hits[i].point.sqrMagnitude > 0) {
                    hitNormal = hits[i].normal;
                    return true;
                }
            }
            return false;
        }

    }
}