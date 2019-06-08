/* Hand physics
 * copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: December 29, 2016
 *
 */


//#define DEBUG_FORCE
//#define DEBUG_TORQUE
//#define IMPULSE

using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer {

    public class BasicHandPhysics : EventTrigger {
        public HandTarget target;

        public virtual void OnCollisionEnter(Collision collision) {
            Rigidbody objRigidbody = collision.rigidbody;
            if (objRigidbody != null)
                HandInteraction.OnTouchStart(target, objRigidbody.gameObject);
            else
                HandInteraction.OnTouchStart(target, collision.gameObject);
        }

        public virtual void OnCollisionExit(Collision collision) {
            Rigidbody objRigidbody = collision.rigidbody;
            if (objRigidbody != null)
                HandInteraction.OnTouchEnd(target, objRigidbody.gameObject);
            else
                HandInteraction.OnTouchEnd(target, collision.gameObject);
        }

        // forwarding events to grabbed objects
        // This is needed because grabbed objects can be parented to the hands
        // If we send an event to the hand and we have grabbed on object, we forward the event to the grabbed object
        public override void OnPointerDown(PointerEventData eventata) {
            // disabled because it can result in a cycle in spine
            //if (target != null && target.grabbedObject != null)
            //Debug.Log(target.grabbedObject);
            //ExecuteEvents.ExecuteHierarchy(target.grabbedObject, eventData, ExecuteEvents.pointerDownHandler);
        }
        public override void OnPointerUp(PointerEventData eventData) {
            // disabled because it can result in a cycle in spine
            //if (target != null && target.grabbedObject != null) ;
            //ExecuteEvents.ExecuteHierarchy(target.grabbedObject, eventData, ExecuteEvents.pointerUpHandler);
        }
    }

    public class AdvancedHandPhysics : BasicHandPhysics {

        public enum PhysicsMode {
            Kinematic,
            NonKinematic,
            HybridKinematic,
            ForceLess,
        }
        public PhysicsMode mode = PhysicsMode.HybridKinematic;

        [HideInInspector]
        private Rigidbody handRigidbody;

        private bool colliding;
        public bool hasCollided = false;
        public Vector3 contactPoint;

        public Vector3 force;
        public Vector3 torque;

        private void Initialize() {
            if (target == null)
                return;

            if (enabled) {
                handRigidbody = GetComponent<Rigidbody>();
                if (handRigidbody != null) {
                    Kinematize(handRigidbody, mode);
                    handRigidbody.maxAngularVelocity = 20;
                }
            }
        }

        #region Update
        public void FixedUpdate() {
            CalculateVelocity();
        }

        public virtual void ManualFixedUpdate(HandTarget _handTarget) {
            target = _handTarget;

            if (hasCollided && !colliding) {
                HandInteraction.OnTouchEnd(target, target.touchedObject);
                target.touchedObject = null;
            }

            if (target.touchedObject == null) { // Object may be destroyed
                hasCollided = false;
            }

            if (handRigidbody == null)
                Initialize();

            if (target.forearm.bone.transform != null) {
                float distance = Vector3.Distance(target.hand.bone.transform.position, target.forearm.bone.transform.position) - target.forearm.bone.length;
                if (distance > 0.05F) {
                    SetKinematic(handRigidbody, true);
                }
            }

            UpdateRigidbody();

            colliding = false;
        }

        public void UpdateRigidbody() {
            if (handRigidbody == null)
                return;

            if ((mode == PhysicsMode.NonKinematic || mode == PhysicsMode.ForceLess) && handRigidbody.isKinematic)
                SetKinematic(handRigidbody, false);

            Quaternion targetRotation = target.transform.rotation;

            Quaternion rot = Quaternion.Inverse(handRigidbody.rotation) * targetRotation;
            float angle;
            Vector3 axis;
            rot.ToAngleAxis(out angle, out axis);

            if (handRigidbody.isKinematic)
                UpdateKinematicRigidbody();
            else
                UpdateNonKinematicRigidbody();
        }

        private void UpdateKinematicRigidbody() {
            force = Vector3.zero;
            torque = Vector3.zero;
        }

        private void UpdateNonKinematicRigidbody() {
            if (mode != PhysicsMode.ForceLess) {
                torque = CalculateTorque();
                ApplyTorqueAtPosition(torque, target.handPalm.position);

                Vector3 wristTorque = CalculateWristTorque();
                ApplyTorqueAtPosition(wristTorque, target.hand.bone.transform.position);

                force = CalculateForce();
                ApplyForce(force);

                if (target.humanoid.haptics)
                    target.Vibrate(force.magnitude / 25);
            }
            else {
                force = Vector3.zero;
                torque = Vector3.zero;
            }

            if (!hasCollided && !handRigidbody.useGravity && (mode != PhysicsMode.NonKinematic && mode != PhysicsMode.ForceLess)) {
                SetKinematic(handRigidbody, true);
            }
        }
        #endregion Update

        #region Events
        public void OnTriggerEnter(Collider collider) {
            bool otherHasKinematicPhysics = false;
            bool otherIsHumanoid = false;

            Rigidbody otherRigidbody = collider.attachedRigidbody;
            if (otherRigidbody != null) {
                AdvancedHandPhysics kp = otherRigidbody.GetComponent<AdvancedHandPhysics>();
                otherHasKinematicPhysics = (kp != null);
                HumanoidControl humanoid = otherRigidbody.GetComponent<HumanoidControl>();
                otherIsHumanoid = (humanoid != null);
            }

            if (handRigidbody != null && handRigidbody.isKinematic && (!collider.isTrigger || otherHasKinematicPhysics) && !otherIsHumanoid) {
                colliding = true;
                hasCollided = true;
                if (otherRigidbody != null) {
                    target.touchedObject = otherRigidbody.gameObject;
                    if (!otherRigidbody.isKinematic)
                        SetKinematic(handRigidbody, false);
                }
                else {
                    target.touchedObject = collider.gameObject;
                    SetKinematic(handRigidbody, false);
                }

                ProcessFirstCollision(handRigidbody, collider);
            }

            if (hasCollided) {
                Rigidbody objRigidbody = collider.attachedRigidbody;
                if (objRigidbody != null) {
                    HandInteraction.GrabCheck(target, objRigidbody.gameObject);
                }
                else
                    HandInteraction.GrabCheck(target, collider.gameObject);
            }
        }

        public override void OnCollisionEnter(Collision collision) {
            colliding = true;
            if (collision.contacts.Length > 0)
                contactPoint = collision.contacts[0].point;
            base.OnCollisionEnter(collision);
        }

        public void OnCollisionStay(Collision collision) {
            colliding = true;
            if (collision.contacts.Length > 0)
                contactPoint = collision.contacts[0].point;
        }

        public override void OnCollisionExit(Collision collision) {
            if (handRigidbody != null && !handRigidbody.useGravity) {
                RaycastHit hit;
                if (handRigidbody.SweepTest(target.transform.position - handRigidbody.position, out hit)) {
                    ;
                }
                else {
                    hasCollided = false;
                    contactPoint = Vector3.zero;
                    target.touchedObject = null;
                }
            }
        }
        #endregion

        public static void Kinematize(Rigidbody rigidbody, PhysicsMode mode) {
            if (rigidbody != null) {
                if (rigidbody.useGravity || mode == PhysicsMode.NonKinematic)
                    SetKinematic(rigidbody, false);
                else
                    SetKinematic(rigidbody, true);
            }
        }

        public static void Unkinematize(Rigidbody rigidbody) {
            SetKinematic(rigidbody, false);
        }

        public void DeterminePhysicsMode(float kinematicMass = 1) {
            mode = DeterminePhysicsMode(handRigidbody, kinematicMass);
        }

        public static PhysicsMode DeterminePhysicsMode(Rigidbody rigidbody, float kinematicMass = 1) {
            if (rigidbody == null)
                return PhysicsMode.Kinematic;

            PhysicsMode physicsMode;
            if (rigidbody.useGravity) {
                physicsMode = PhysicsMode.NonKinematic;
            }
            else {
                float mass = CalculateTotalMass(rigidbody);
                if (mass > kinematicMass)
                    physicsMode = PhysicsMode.NonKinematic;
                else
                    physicsMode = PhysicsMode.HybridKinematic;
            }
            return physicsMode;
        }

        public static float CalculateTotalMass(Rigidbody rigidbody) {
            if (rigidbody == null)
                return 0;

            float mass = rigidbody.gameObject.isStatic ? Mathf.Infinity : rigidbody.mass;
            Joint[] joints = rigidbody.GetComponents<Joint>();
            for (int i = 0; i < joints.Length; i++) {
                // Seems to result in cycle in spine in some cases
                //if (joints[i].connectedBody != null)
                //    mass += CalculateTotalMass(joints[i].connectedBody);
                //else
                mass = Mathf.Infinity;
            }
            return mass;
        }

        public Vector3 boneVelocity;
        private Vector3 lastPosition = Vector3.zero;
        private void CalculateVelocity() {
            if (lastPosition != Vector3.zero) {
                boneVelocity = (target.hand.bone.transform.position - lastPosition) / Time.fixedDeltaTime;
            }
            lastPosition = target.hand.bone.transform.position;
        }

        #region Force
        private Vector3 CalculateForce() {
            Vector3 locationDifference = target.stretchlessTarget.position - handRigidbody.position;
            Debug.DrawRay(handRigidbody.position, locationDifference);
            Vector3 force = locationDifference * target.strength;

            //force += CalculateForceDamper();
            return force;
        }

        private const float damping = 30;
        private float lastDistanceTime;
        private Vector3 lastDistanceToTarget;
        private Vector3 CalculateForceDamper() {
            Vector3 distanceToTarget = target.hand.bone.transform.position - target.hand.target.transform.position;

            float deltaTime = Time.fixedTime - lastDistanceTime;

            Vector3 damper = Vector3.zero;
            if (deltaTime < 0.1F) {
                Vector3 velocityTowardsTarget = (distanceToTarget - lastDistanceToTarget) / deltaTime;

                damper = -velocityTowardsTarget * damping;

                //Compensate for absolute rigidbody speed (specifically when on a moving platform)
                Vector3 residualVelocity = handRigidbody.velocity - velocityTowardsTarget;
                damper += residualVelocity * 10;
            }
            lastDistanceToTarget = distanceToTarget;
            lastDistanceTime = Time.fixedTime;

            return damper;
        }

        private void ApplyForce(Vector3 force) {
            if (float.IsNaN(force.magnitude))
                return;

            /*
            if (contactPoint.sqrMagnitude > 0) {
                // The contact point is OK, but the force here is not OK, because this is the force from the hand
                // The force needs to be projected on the contactPoint !
                //handRigidbody.AddForceAtPosition(force, contactPoint);
                //#if DEBUG_FORCE
                Debug.DrawRay(contactPoint, force / 10, Color.yellow);
                //#endif
            }
            else {
                // The contact point is OK, but the force here is not OK, because this is the force from the hand
                // The force needs to be projected on the contactPoint !
                //handRigidbody.AddForceAtPosition(force, target.handPalm.position);
                handRigidbody.AddForce(force);
                //#if DEBUG_FORCE
                Debug.DrawRay(target.handPalm.position, force / 10, Color.yellow);
                //#endif
            }
            */
            handRigidbody.AddForce(force);
            //#if DEBUG_FORCE
            Debug.DrawRay(handRigidbody.position, force / 10, Color.yellow);
            //#endif

        }
        #endregion

        #region Torque
        private Vector3 CalculateTorque() {
            Quaternion sollRotation = target.hand.target.transform.rotation * target.hand.target.toBoneRotation;
            Quaternion istRotation = target.hand.bone.transform.rotation;
            Quaternion dRot = sollRotation * Quaternion.Inverse(istRotation);

            float angle;
            Vector3 axis;
            dRot.ToAngleAxis(out angle, out axis);
            angle = UnityAngles.Normalize(angle);

            Vector3 angleDifference = axis.normalized * (angle * Mathf.Deg2Rad);
            Vector3 torque = angleDifference * target.strength * 0.1F;
            return torque;
        }

        private Vector3 CalculateWristTorque() {
            //Vector3 wristTension = target.GetWristTension();

            // Not stable
            //Vector3 forces = new Vector3(-(wristTension.x * wristTension.x * 10), -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);
            //Vector3 forces = new Vector3(0, -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);

            Vector3 torque = Vector3.zero; // (0, 0, -wristTension.z * wristTension.z * 10);
            return torque;
        }

        private void ApplyTorque(Vector3 torque) {
            //AddTorqueAtPosition(torque, target.handPalm.position);
            ApplyTorqueAtPosition(torque, target.hand.bone.transform.position);
        }

        private void ApplyTorqueAtPosition(Vector3 torque, Vector3 posToApply) {
            if (float.IsNaN(torque.magnitude))
                return;

            Vector3 torqueAxis = torque.normalized;
            Vector3 ortho = new Vector3(1, 0, 0);

            // prevent torqueAxis and ortho from pointing in the same direction
            if (((torqueAxis - ortho).sqrMagnitude < Mathf.Epsilon) || ((torqueAxis + ortho).sqrMagnitude < Mathf.Epsilon)) {
                ortho = new Vector3(0, 1, 0);
            }

            ortho = Vector3OrthoNormalize(torqueAxis, ortho);
            // calculate force 
            Vector3 force = Vector3.Cross(0.5f * torque, ortho);

            handRigidbody.AddForceAtPosition(force, posToApply + ortho);
            handRigidbody.AddForceAtPosition(-force, posToApply - ortho);

#if DEBUG_TORQUE
            UnityEngine.Debug.DrawRay(posToApply + ortho / 20, force / 10, Color.yellow);
            UnityEngine.Debug.DrawLine(posToApply + ortho / 20, posToApply - ortho / 20, Color.yellow);
            UnityEngine.Debug.DrawRay(posToApply - ortho / 20, -force / 10, Color.yellow);
#endif
        }

        private Vector3 Vector3OrthoNormalize(Vector3 a, Vector3 b) {
            Vector3 tmp = Vector3.Cross(a.normalized, b).normalized;
            return tmp;
        }
        #endregion

        public void ProcessFirstCollision(Rigidbody rigidbody, Collider otherCollider) {

#if IMPULSE
		CalculateCollisionImpuls(rigidbody, otherRigidbody, collisionPoint);
#endif
        }

#if IMPULSE
	private static void CalculateCollisionImpuls(Rigidbody rigidbody, Rigidbody otherRigidbody, Vector3 collisionPoint) {
		if (otherRigidbody != null) {
			Vector3 myImpuls = (rigidbody.mass / 10) * rigidbody.velocity;
			otherRigidbody.AddForceAtPosition(myImpuls, collisionPoint, ForceMode.Impulse);
		}
	}
#endif

        public static void SetKinematic(Rigidbody rigidbody, bool b) {
            if (rigidbody == null)
                return;

            GameObject obj = rigidbody.gameObject;
            if (obj.isStatic == false) {
                rigidbody.isKinematic = b;
                HumanoidTarget.SetColliderToTrigger(obj, b);
            }
        }

    }
}