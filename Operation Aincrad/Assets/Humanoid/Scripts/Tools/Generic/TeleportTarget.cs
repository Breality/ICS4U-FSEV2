using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer.Humanoid {

    /// <summary> The Teleport Target provides control to where a player can teleport and can be used in combination with a generic Interaction Pointer. </summary>    
    /// The Humanoid Teleport Target can be placed on a static or moving Rigidbody object with a collider.
    /// It has been implemented as a Event Trigger and will teleport when an PointerDown event is received.
    public class TeleportTarget : EventTrigger {
        public enum TargetPosRot {
            Pointer,
            Transform
        }
        /// <summary>
        /// Teleport the root transform instead of the transform itself
        /// </summary>
        /// If this is enabled the root of the transform (the topmost transform)
        /// will be teleported instead of the transform itself.
        public bool teleportRoot = false;

        /// <summary> Check the target location for collisions </summary>
        /// if enabled this will check if the location to which the pointer is pointing contains a collider.
        /// Teleporting will only take place if no collider has been found.
        /// The check is executed using a capsule of 2 meters/units high and 0.2 meters/units radius.
        public bool checkCollision = true;

        /// <summary> The type of movement used to reacht the teleport target </summary> 
        ///  determines how the Transform is moved to the Target Point.
        /// Teleport = direct placement a the target point.
        /// Dash = a quick movement is a short time from the originating point to the target point.
        public MovementType movementType;

        /// <summary> Determines the target position and rotation </summary>
        /// TargetPosRot.Pointer = The interaction pointer location is the target position and rotation.
        /// TargetPosRot.Transform = The targetTransform determines the target position and rototation.
        public TargetPosRot targetPosRot = TargetPosRot.Pointer;

        /// <summary> The target transform for the teleport </summary> 
        /// If targetPosRot is set to TargetPosRot.Transform the targetTransform will determine the 
        /// position and rotation of the transform to teleport.
        /// Next to that, the teleported transform will become a child of this teleportTarget after teleporting
        public Transform targetTransform;

        /// <summary> The pose of a humanoid after it has been teleported </summary>
        /// This is an optional Pose of the Humanoid after it has been teleported.
        /// This enables you to teleport to a different pose like a seating pose for instance.
        public Pose pose;

        /// <summary> Enable humanoid animators after teleporting </summary>
        /// This will enable or disable the hips and foot animator after teleporting.
        /// For poses like seating a walking foot animation is not required.
        /// In such cases the foot animator can be switch off with this setting.
        public bool enableAnimators = true;


        /// <summary> Teleport the transform to this Teleport Target </summary>
        /// This function will teleport the given transform to this teleport target using the
        /// teleport target settings.
        /// If the targetPosRot is set to TargetPosRot.Pointer, the transform will be transported
        /// using the transform of the gameObject as there is no pointer information.
        /// <param name="t">The transform to teleport</param>
        public void TeleportToHere(Transform t) {
            if (targetPosRot == TargetPosRot.Transform && targetTransform != null)
                Teleport(t, targetTransform.position, targetTransform.rotation, movementType, targetTransform);
            else
                Teleport(t, gameObject.transform.position, gameObject.transform.rotation, movementType, null);
        }

        /// <summary> Teleport the Humanoid to this Teleport Target </summary>
        /// This function will teleport the given humanoid to this teleport target using the
        /// teleport target settings.
        /// If the targetPosRot is set to TargetPosRot.Pointer, the humanoid will be transported
        /// using the transform of the gameObject as there is no pointer information.
        /// <param name="humanoid">The humanoid to teleport</param>
        public void TeleportToHere(HumanoidControl humanoid) {
            Teleport(humanoid.transform, gameObject.transform.position, gameObject.transform.rotation, movementType);
        }

        /// <summary> Teleporting initialized by an Unity UI Event </summary>
        public override void OnPointerDown(PointerEventData eventData) {
            Vector3 pointingPosition = eventData.pointerCurrentRaycast.worldPosition;

            if (IsValid(pointingPosition) && eventData.currentInputModule != null) {
                GameObject originator = eventData.currentInputModule.gameObject;

                Transform teleportingTransform = originator.transform;
                HumanoidControl humanoid = originator.transform.GetComponentInParent<HumanoidControl>();
                if (humanoid != null) {
                    teleportingTransform = humanoid.transform;

                    humanoid.hipsTarget.animator.enabled = enableAnimators;
                    humanoid.leftFootTarget.animator.enabled = enableAnimators;
                    humanoid.rightFootTarget.animator.enabled = enableAnimators;
                }

                if (targetPosRot == TargetPosRot.Pointer)
                    Teleport(teleportingTransform, pointingPosition, originator.transform.rotation, movementType, null);
                else if (targetTransform != null)
                    Teleport(teleportingTransform, targetTransform.position, targetTransform.rotation, movementType, targetTransform);
                else
                    Teleport(teleportingTransform, transform.position, transform.rotation, movementType, targetTransform);

                if (humanoid != null) {
                    humanoid.pose = pose;
                    if (pose != null) {
                        float oldNeckHeight = humanoid.headTarget.head.target.transform.position.y;
                        humanoid.pose.Show(humanoid);
                        float deltaNeckHeight = humanoid.headTarget.head.target.transform.position.y - oldNeckHeight;
                        humanoid.AdjustTracking(new Vector3(0, deltaNeckHeight, 0));
                        humanoid.CopyRigToTargets();
                    }
                }
            }

            base.OnPointerDown(eventData);
        }

        /// <summary> Teleport the transform to a position and rotation </summary>
        /// This function will teleport the given transform to this teleport target using the
        /// teleport target settings.
        /// If the targetPosRot is set to TargetPosRot.Pointer, the transform will be transported
        /// using the transform of the gameObject as there is no pointer information.
        /// <param name="transform">The transform to teleport</param>
        /// <param name="position">The new world position of the transform after teleporting</param>
        /// <param name="rotation">The new world rotation of the transform after teleporting</param>
        /// <param name="movementType">The movement type to use to get to the target</param>
        /// <param name="newParent">The new parent of the transform after teleporting</param>
        protected virtual void Teleport(Transform transform, Vector3 position, Quaternion rotation, MovementType movementType = MovementType.Teleport, Transform newParent = null) {
            if (newParent != null && transform.parent == newParent)
                // The transform is already at the teleport target, do not teleport again
                return;

            Vector3 originPosition = transform.position;

            HumanoidControl humanoid = transform.GetComponent<HumanoidControl>();
            if (humanoid != null) {
                Vector3 humanoidPosition = humanoid.GetHumanoidPosition();
                Vector3 deltaHumanoid = humanoidPosition - transform.position;

                // We need to rotate the localHumanoidPosition so that it matches the new humanoid.transform rotation
                Quaternion rotateHumanoid = Quaternion.Inverse(transform.rotation) * rotation;

                deltaHumanoid = rotateHumanoid * deltaHumanoid;

                position -= deltaHumanoid;
            }

            if (teleportRoot) {
                Vector3 translation = position - originPosition; // World coordinates

                transform = transform.root;

                position = transform.position + translation;
            }
            transform.MoveTo(position, movementType);
            transform.rotation = rotation;
            transform.SetParent(newParent, true);
        }

        /// <summary> Check is the target position is valid </summary>
        protected virtual bool IsValid(Vector3 position) {
            if (!checkCollision)
                return true;

            Vector3 start = position + new Vector3(0, 0.3F, 0);
            Vector3 end = position + new Vector3(0, 1.8F, 0);
            bool isOccupied = Physics.CheckCapsule(start, end, 0.2F);
            return (!isOccupied);
        }
    }
}