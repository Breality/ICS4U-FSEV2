using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace Passer {

    [System.Serializable]
    public class HandInteraction {
        public const float kinematicMass = 1; // masses < 1 will move kinematic when not colliding
        public const float maxGrabbingMass = 10; // maxMass you can grab is 10

        #region Start
        public static void StartInteraction(HandTarget handTarget) {
            // Remote humanoids should not interact
            if (handTarget.humanoid.isRemote)
                return;

            handTarget.inputModule = handTarget.humanoid.GetComponent<Interaction>();
            if (handTarget.inputModule == null) {
                handTarget.inputModule = Object.FindObjectOfType<Interaction>();
                if (handTarget.inputModule == null) {
                    EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
                    if (eventSystem != null)
                        Object.DestroyImmediate(eventSystem.gameObject);
                    handTarget.inputModule = handTarget.humanoid.gameObject.AddComponent<Interaction>();
                }
            }

            handTarget.inputModule.EnableTouchInput(handTarget.humanoid, handTarget.isLeft, 0);
        }
        #endregion

        #region Update
        public static void UpdateInteraction() {
            // This interferes with the HandPhysics which also sets the touchingObject...

            //InputDeviceIDs inputDeviceID = isLeft ? InputDeviceIDs.LeftHand : InputDeviceIDs.RightHand;
            //touchingObject = inputModule.GetTouchObject(inputDeviceID);
        }
        #endregion

        #region Touching
        public static void OnTouchStart(HandTarget handTarget, GameObject obj) {
            GrabCheck(handTarget, obj);
            if (handTarget.inputModule != null)
                handTarget.inputModule.OnFingerTouchStart(handTarget.isLeft, obj);
        }

        public static void OnTouchEnd(HandTarget handTarget, GameObject obj) {
            if (handTarget.inputModule != null && obj == handTarget.touchedObject)
                handTarget.inputModule.OnFingerTouchEnd(handTarget.isLeft);
        }
        #endregion

        #region Grabbing
        private static bool grabChecking;

        public static void GrabCheck(HandTarget handTarget, GameObject obj) {
            if (grabChecking || handTarget.grabbedObject != null || handTarget.humanoid.isRemote)
                return;

            grabChecking = true;
            float handCurl = handTarget.HandCurl();
            if (handCurl > 2 && CanBeGrabbed(handTarget, obj)) {
                Grab(handTarget, obj);
            }
            grabChecking = false;
        }

        public static void Grab(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            if (handTarget.humanoid.humanoidNetworking != null)
                handTarget.humanoid.humanoidNetworking.Grab(handTarget, obj, rangeCheck);

            LocalGrab(handTarget, obj, rangeCheck);
        }

        public static bool CanBeGrabbed(HandTarget handTarget, GameObject obj) {
            if (obj == null || obj == handTarget.humanoid.gameObject ||
                (handTarget.humanoid.characterRigidbody != null && obj == handTarget.humanoid.characterRigidbody.gameObject) ||
                (handTarget.otherHand.handRigidbody != null && obj == handTarget.otherHand.handRigidbody.gameObject)
                || obj == handTarget.humanoid.headTarget.gameObject
                )
                return false;

            // We cannot grab 2D objects like UI
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
                return false;

            return true;
        }

        public static void LocalGrab(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            Transform objTransform = obj.GetComponent<Transform>();

            if (handTarget.grabbedObject == null) {
                if (objRigidbody != null) {
                    NoGrab noGrab = objRigidbody.GetComponent<NoGrab>();
                    if (noGrab != null)
                        return;
                }

                if (objRigidbody != null && objRigidbody.mass > maxGrabbingMass)
                    return;

                bool grabbed = false;
                if (objRigidbody != null) {
                    grabbed = GrabRigidbody(handTarget, objRigidbody, rangeCheck);
                }
                else {
                    grabbed = GrabStaticObject(handTarget, objTransform);
                }

                if (grabbed) {
                    if (handTarget.humanoid.physics) {
                        HumanoidTarget.SetColliderToTrigger(handTarget.hand.bone.transform.gameObject, true);
                        if (handTarget.handPhysics != null && handTarget.handPhysics.mode != AdvancedHandPhysics.PhysicsMode.ForceLess)
                            handTarget.handPhysics.DeterminePhysicsMode(kinematicMass);
                    }
                    handTarget.grabbedObject = obj;

                    handTarget.SendMessage("OnGrabbing", handTarget.grabbedObject, SendMessageOptions.DontRequireReceiver);
                    handTarget.grabbedObject.SendMessage("OnGrabbed", handTarget, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static bool GrabRigidbody(HandTarget handTarget, Rigidbody objRigidbody, bool rangeCheck = true) {
            //Debug.Log("GrabRigidbody");

            Handle[] handles = objRigidbody.GetComponentsInChildren<Handle>();
            for (int i = 0; i < handles.Length; i++) {
                Vector3 handlePosition = handles[i].transform.TransformPoint(handles[i].position);
                float grabDistance = Vector3.Distance(handTarget.palmPosition, handlePosition);

                if ((handTarget.isLeft && handles[i].hand == Handle.Hand.Right) ||
                    (!handTarget.isLeft && handles[i].hand == Handle.Hand.Left))
                    continue;

                if (grabDistance < handles[i].range || !rangeCheck) {
                    switch (handles[i].grabType) {
                        case Handle.GrabType.DefaultGrab:
                        case Handle.GrabType.BarGrab:
                            GrabRigidbodyHandle(handTarget, objRigidbody, handles[i]);
                            handles[i].handTarget = handTarget;
                            return true;
                        default:
                            return false;
                    }
                }
            }

            if (rangeCheck == false) {
                //float grabDistance = Vector3.Distance(handTarget.handPalm.position, objRigidbody.position);
                float grabDistance = Vector3.Distance(handTarget.hand.bone.transform.position, objRigidbody.position);
                if (grabDistance > 0.2F) // Object is far away, move it into the hand
                    MoveObjectToHand(handTarget, objRigidbody.transform);
            }

            Joint joint = objRigidbody.GetComponent<Joint>();
            AdvancedHandPhysics otherHandPhysics = objRigidbody.GetComponent<AdvancedHandPhysics>();

            if (joint != null || objRigidbody.constraints != RigidbodyConstraints.None || otherHandPhysics != null) {
                GrabRigidbodyJoint(handTarget, objRigidbody);
            }
            else {
                GrabRigidbodyParenting(handTarget, objRigidbody);
            }
            return true;
        }

        private static void GrabRigidbodyHandle(HandTarget handTarget, Rigidbody objRigidbody, Handle handle) {
            Transform objTransform = objRigidbody.transform;

            if (AlreadyGrabbedWithOtherHand(handTarget, objRigidbody)) {
                GrabRigidbodyBarHandle2(handTarget, objRigidbody, handle);
                return;
            }

            Joint joint = objRigidbody.GetComponent<Joint>();
            if (joint != null || objRigidbody.constraints != RigidbodyConstraints.None) {
                MoveHandBoneToHandle(handTarget, handle);

                // To add: if handle.rotation = true
                Vector3 handleWorldPosition = handle.transform.TransformPoint(handle.position);
                Vector3 handleLocalPosition = handTarget.hand.bone.transform.InverseTransformPoint(handleWorldPosition);

                Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
                Vector3 handleRotationAxis = handleWorldRotation * Vector3.up;

                Vector3 handleLocalRotationAxis = handTarget.hand.bone.transform.InverseTransformDirection(handleRotationAxis);

                GrabRigidbodyJoint(handTarget, objRigidbody, handleLocalPosition, handleLocalRotationAxis);
            }
            else {
                MoveObjectToHand(handTarget, objTransform, handle);
                GrabRigidbodyParenting(handTarget, objRigidbody);
            }
            handTarget.grabbedHandle = handle;
            if (handle.pose != null) {
                // change to: handTarget.SetPose(handle.pose) ??
                Humanoid.MixedPose mixedPose = handTarget.poseMixer.Add(handle.pose);
                handTarget.poseMixer.SetPoseValue(mixedPose, 1);
            }
        }

        // Grab with second hand moving to object
        private static void GrabRigidbodyBarHandle2(HandTarget handTarget, Rigidbody objRigidbody, Handle handle) {
            Debug.Log("Grab Second");
            Transform objTransform = handle.transform;

            MoveHandBoneToHandle(handTarget, handle);
            GrabRigidbodyJoint(handTarget, objRigidbody);
            handTarget.grabbedHandle = handle;

            handTarget.handPhysics.mode = AdvancedHandPhysics.PhysicsMode.ForceLess;

            handTarget.handMovements.toOtherHandle = handTarget.grabbedHandle.GetWorldPosition() - handTarget.otherHand.grabbedHandle.GetWorldPosition();
            Quaternion hand2handle = Quaternion.LookRotation(handTarget.handMovements.toOtherHandle);
            handTarget.otherHand.handMovements.hand2handle = Quaternion.Inverse(hand2handle) * handTarget.otherHand.hand.target.transform.rotation;
        }

        public static void MoveHandBoneToHandle(HandTarget handTarget, Handle handle) {
            // Should use GetGrabPosition
            Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
            Quaternion palm2handRot = Quaternion.Inverse(handTarget.handPalm.localRotation);
            handTarget.hand.bone.transform.rotation = handleWorldRotation * palm2handRot;

            Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
            Vector3 palm2handPos = handTarget.hand.bone.transform.position - handTarget.handPalm.position;
            handTarget.hand.bone.transform.position = handleWPos + palm2handPos;
        }

        public static void MoveAndGrabHandle(HandTarget handTarget, Handle handle) {
            if (handTarget == null || handle == null)
                return;

            MoveHandTargetToHandle(handTarget, handle);
            GrabHandle(handTarget, handle);
        }

        public static void MoveHandTargetToHandle(HandTarget handTarget, Handle handle) {
            // Should use GetGrabPosition
            Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
            Quaternion palm2handRot = Quaternion.Inverse(Quaternion.Inverse(handTarget.hand.bone.targetRotation) * handTarget.palmRotation);
            handTarget.hand.target.transform.rotation = handleWorldRotation * palm2handRot;

            Vector3 handleWorldPosition = handle.transform.TransformPoint(handle.position);
            handTarget.hand.target.transform.position = handleWorldPosition - handTarget.hand.target.transform.rotation * handTarget.localPalmPosition;
        }

        public static void GetGrabPosition(HandTarget handTarget, Handle handle, out Vector3 handPosition, out Quaternion handRotation) {
            Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
            Quaternion handleWRot = handle.transform.rotation * Quaternion.Euler(handle.rotation);

            GetGrabPosition(handTarget, handleWPos, handleWRot, out handPosition, out handRotation);
        }

        private static void GetGrabPosition(HandTarget handTarget, Vector3 targetPosition, Quaternion targetRotation, out Vector3 handPosition, out Quaternion handRotation) {
            Quaternion palm2handRot = Quaternion.Inverse(handTarget.handPalm.localRotation) * handTarget.hand.bone.toTargetRotation;
            handRotation = targetRotation * palm2handRot;

            Vector3 hand2palmPos = handTarget.handPalm.localPosition;
            Vector3 hand2palmWorld = handTarget.hand.bone.transform.TransformVector(hand2palmPos);
            Vector3 hand2palmTarget = handTarget.hand.target.transform.InverseTransformVector(hand2palmWorld); // + new Vector3(0, -0.03F, 0); // small offset to prevent fingers colliding with collider
            handPosition = targetPosition + handRotation * -hand2palmTarget;
            Debug.DrawLine(targetPosition, handPosition);
        }

        // This is not fully completed, no parenting of joints are created yet
        public static void GrabHandle(HandTarget handTarget, Handle handle) {
            handTarget.grabbedHandle = handle;
            handTarget.grabbedObject = handle.gameObject;
            handle.handTarget = handTarget;

            if (handle.pose != null)
                handTarget.SetPose(handle.pose);
        }

        public static void MoveObjectToHand(HandTarget handTarget, Transform objTransform, Handle handle) {
            objTransform.rotation = handTarget.palmRotation * Quaternion.Inverse(Quaternion.Euler(handle.rotation));

            Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
            objTransform.Translate(handTarget.palmPosition - handleWPos, Space.World);
        }
        public static void MoveObjectToHand(HandTarget handTarget, Transform objTransform) {
            objTransform.position = handTarget.palmPosition;
        }


        private static bool AlreadyGrabbedWithOtherHand(HandTarget handTarget, Rigidbody objRigidbody) {
            return (handTarget.otherHand != null && handTarget.otherHand.hand.bone.transform != null && objRigidbody.transform == handTarget.otherHand.hand.bone.transform);
        }

        private static void GrabBallHandle(HandTarget handTarget, Rigidbody objRigidbody, Transform objTransform, Handle handle, Transform handTransform, Transform handPalm) {
            Vector3 handleWPos = objTransform.TransformVector(handle.position);
            objTransform.position = handPalm.position - handleWPos;

            CharacterJoint charJoint = handTarget.hand.bone.transform.gameObject.AddComponent<CharacterJoint>();
            charJoint.connectedBody = objRigidbody;

            charJoint.axis = handPalm.forward;
            charJoint.swingAxis = -handPalm.right;
            charJoint.anchor = handTransform.InverseTransformPoint(handPalm.position);

            SoftJointLimit jointLimit2 = charJoint.lowTwistLimit;
            jointLimit2.limit = -60;
            charJoint.lowTwistLimit = jointLimit2;

            SoftJointLimit jointLimit = charJoint.highTwistLimit;
            jointLimit.limit = 60;
            charJoint.highTwistLimit = jointLimit;

            jointLimit.limit = 90;
            charJoint.swing1Limit = jointLimit;
            charJoint.swing2Limit = jointLimit;

            SoftJointLimitSpring jointSpring = new SoftJointLimitSpring {
                spring = 0.5F,
                damper = 0.05F
            };
            charJoint.twistLimitSpring = jointSpring;
            charJoint.swingLimitSpring = jointSpring;

            charJoint.enableProjection = true;
            charJoint.projectionDistance = 0;

            handTarget.grabbedObject = objTransform.gameObject;
            handTarget.SendMessage("OnGrabbing", handTarget.grabbedObject, SendMessageOptions.DontRequireReceiver);
        }

        public static void GrabRigidbodyJoint(HandTarget handTarget, Rigidbody objRigidbody) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            ConfigurableJoint joint = handTarget.hand.bone.transform.gameObject.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.01F;
            joint.projectionAngle = 1;

            Collider c = objRigidbody.transform.GetComponentInChildren<Collider>();
            joint.connectedBody = c.attachedRigidbody;
        }

        private static void GrabRigidbodyJoint(HandTarget handTarget, Rigidbody objRigidbody, Vector3 anchorPoint, Vector3 rotationAxis) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            ConfigurableJoint joint = handTarget.hand.bone.transform.gameObject.AddComponent<ConfigurableJoint>();
            Collider c = objRigidbody.transform.GetComponentInChildren<Collider>();
            joint.connectedBody = c.attachedRigidbody;

            joint.anchor = anchorPoint;
            joint.axis = rotationAxis;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Locked; // Free;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.01F;
            joint.projectionAngle = 1;

            handTarget.storedCOM = objRigidbody.centerOfMass;
            objRigidbody.centerOfMass = joint.connectedAnchor;
        }

        private static void GrabRigidbodyParenting(HandTarget handTarget, Rigidbody objRigidbody) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            handTarget.grabbedRBdata = new StoredRigidbody(objRigidbody);
            objRigidbody.transform.parent = handTarget.handPalm;

            if (Application.isPlaying)
                Object.Destroy(objRigidbody);
            else
                Object.DestroyImmediate(objRigidbody, true);


            //SetColliderToTrigger(objTransform.gameObject, true);

            // Already done in Grab() ???
            //target.grabbedObject = objRigidbody.gameObject;
            //target.SendMessage("OnGrabbing", target.grabbedObject, SendMessageOptions.DontRequireReceiver);
        }

        public static bool GrabStaticObject(HandTarget handTarget, Transform objTransform) {
            //Debug.Log("GrabStaticObject");
            Handle[] handles = objTransform.GetComponentsInChildren<Handle>();
            for (int i = 0; i < handles.Length; i++) {
                if ((handTarget.isLeft && handles[i].hand == Handle.Hand.Right) ||
                    (!handTarget.isLeft && handles[i].hand == Handle.Hand.Left))
                    continue;

                Vector3 handlePosition = handles[i].transform.TransformPoint(handles[i].position);

                if (Vector3.Distance(handTarget.palmPosition, handlePosition) < handles[i].range) {

                    switch (handles[i].grabType) {
                        case Handle.GrabType.DefaultGrab:
                        case Handle.GrabType.BarGrab:
                            GrabStaticHandle(handTarget, handles[i]);
                            handles[i].handTarget = handTarget;
                            return true;
                        default:
                            // Grabbing static objects is only possible with a handle
                            return false;
                    }
                }
            }
            // Grabbing static objects is only possible with a handle
            return false;
        }

        // Grab with hand moving to object

        private static void GrabStaticHandle(HandTarget handTarget, Handle handle) {
            //Debug.Log("GrabStaticBarHandle");
            Transform objTransform = handle.transform;

            MoveHandBoneToHandle(handTarget/*, objTransform*/, handle);
            GrabStaticJoint(handTarget, objTransform);

            handTarget.grabbedHandle = handle;
            if (handle.pose != null) {
                Humanoid.MixedPose mixedPose = handTarget.poseMixer.Add(handle.pose);
                handTarget.poseMixer.SetPoseValue(mixedPose, 1);
            }
        }

        private static void GrabStaticJoint(HandTarget handTarget, Transform objTransform) {
            //FixedJoint joint = target.handBone.parent.gameObject.AddComponent<FixedJoint>();
            FixedJoint joint = handTarget.hand.bone.transform.gameObject.AddComponent<FixedJoint>();

            Collider c = objTransform.GetComponentInChildren<Collider>();
            joint.connectedBody = c.attachedRigidbody;
        }

        private static void GrabMassRedistribution(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            //handRigidbody.mass = KinematicPhysics.CalculateTotalMass(grabbedRigidbody);
            //grabbedRigidbody.mass *= 0.01F;
        }

        private static void GrabMassRestoration(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            //grabbedRigidbody.mass *= 100F;//handRigidbody.mass;
            //handRigidbody.mass = 1F;
        }

        public static void HandGrabPosition(HandTarget handTarget, Vector3 targetPosition, Quaternion targetRotation, Transform handPalm, out Vector3 handPosition, out Quaternion handRotation) {
            Quaternion palm2handRot = Quaternion.Inverse(handPalm.rotation) * handTarget.hand.bone.transform.rotation;
            handRotation = targetRotation * palm2handRot;

            Vector3 localPalmPosition = handTarget.hand.bone.transform.InverseTransformPoint(handPalm.position);
            handPosition = targetPosition - handRotation * localPalmPosition;
        }

        #endregion

        #region Letting go
        public static void CheckLetGo(HandTarget handTarget) {
            if (handTarget.grabbedObject == null || handTarget.grabType == HandTarget.GrabType.Pinch)
                return;

            float handCurl = handTarget.HandCurl();
            bool fingersGrabbing = (handCurl >= 1.5F);
            bool pulledLoose = PulledLoose(handTarget);
            if (!fingersGrabbing || pulledLoose) {
                LetGo(handTarget);
            }
        }

        private static bool PulledLoose(HandTarget handTarget) {
            float forearmStretch = Vector3.Distance(handTarget.hand.bone.transform.position, handTarget.forearm.bone.transform.position) - handTarget.forearm.bone.length;
            if (forearmStretch > 0.15F)
                return true;

            if (handTarget.grabbedHandle != null) {
                Vector3 handlePosition = handTarget.grabbedHandle.worldPosition;
                float handle2palm = Vector3.Distance(handlePosition, handTarget.palmPosition);
                if (handle2palm > 0.15F)
                    return true;
            }
            return false;
        }

        public static void LetGo(HandTarget target) {
            if (target.humanoid.humanoidNetworking != null)
                target.humanoid.humanoidNetworking.LetGo(target);

            LocalLetGo(target);
        }

        public static void LocalLetGo(HandTarget handTarget) {
            //Debug.Log("LetGo");
            if (handTarget.hand.bone.transform == null || handTarget.grabbedObject == null)
                return;


            if (handTarget.humanoid.physics)
                AdvancedHandPhysics.SetKinematic(handTarget.handRigidbody, true);

            Joint joint = handTarget.hand.bone.transform.GetComponent<Joint>();
            if (joint != null)
                LetGoJoint(handTarget, joint);
            else
                LetGoParenting(handTarget);

            if (handTarget.humanoid.dontDestroyOnLoad) {
                // Prevent this object inherites the dontDestroyOnLoad from the humanoid
                Object.DontDestroyOnLoad(handTarget.grabbedObject);
            }

            LetGoGrabbedObject(handTarget);
        }

        private static void LetGoJoint(HandTarget handTarget, Joint joint) {
            Object.DestroyImmediate(joint);
        }

        private static void LetGoParenting(HandTarget handTarget) {
            if (handTarget.grabbedObject.transform.parent == handTarget.hand.bone.transform || handTarget.grabbedObject.transform.parent == handTarget.handPalm)
                handTarget.grabbedObject.transform.parent = null; // originalParent, see InstantVR
        }

        private static void LetGoGrabbedObject(HandTarget handTarget) {
            HandMovements.SetAllColliders(handTarget.grabbedObject, true);
            if (handTarget.humanoid.physics)
                HumanoidTarget.SetColliderToTrigger(handTarget.grabbedObject, false);


            if (handTarget.grabbedRBdata == null)
                LetGoStaticObject(handTarget, handTarget.grabbedObject);
            else
                LetGoRigidbody(handTarget);

            HandTarget.TmpDisableCollisions(handTarget, 0.2F);

            if (handTarget.handPhysics != null)
                handTarget.handPhysics.DeterminePhysicsMode(kinematicMass);

            NetworkTransform nwTransform = handTarget.grabbedObject.GetComponent<NetworkTransform>();
            if (nwTransform != null)
                nwTransform.sendInterval = 1;

            handTarget.SendMessage("OnLettingGo", null, SendMessageOptions.DontRequireReceiver);
            handTarget.grabbedObject.SendMessage("OnLetGo", null, SendMessageOptions.DontRequireReceiver);

            handTarget.grabbedObject = null;
        }

        private static void LetGoRigidbody(HandTarget handTarget) {
            //Debug.Log("LetGoRigidbody");
            Rigidbody grabbedRigidbody = handTarget.grabbedObject.GetComponent<Rigidbody>();
            if (!handTarget.grabbedObject.isStatic && grabbedRigidbody == null) {
                grabbedRigidbody = handTarget.grabbedObject.AddComponent<Rigidbody>();
                if (handTarget.grabbedRBdata != null) {
                    handTarget.grabbedRBdata.CopyToRigidbody(grabbedRigidbody);
                    handTarget.grabbedRBdata = null;
                }
            }

            if (grabbedRigidbody != null) {
                if (handTarget.handRigidbody != null)
                    GrabMassRestoration(handTarget.handRigidbody, grabbedRigidbody);

                Joint[] joints = handTarget.grabbedObject.GetComponents<Joint>();
                for (int i = 0; i < joints.Length; i++) {
                    if (joints[i].connectedBody == handTarget.handRigidbody)
                        Object.Destroy(joints[i]);
                }
                grabbedRigidbody.centerOfMass = handTarget.storedCOM;

                if (handTarget.handRigidbody != null) {
                    if (handTarget.handRigidbody.isKinematic) {
                        grabbedRigidbody.velocity = handTarget.hand.target.velocity;
                        Vector3 targetAngularSpeed = handTarget.hand.target.rotationVelocity.eulerAngles;
                        grabbedRigidbody.angularVelocity = targetAngularSpeed * Mathf.Deg2Rad;
                    }
                    else {
                        grabbedRigidbody.velocity = handTarget.handRigidbody.velocity;
                        grabbedRigidbody.angularVelocity = handTarget.handRigidbody.angularVelocity;
                    }
                }

                if (handTarget.grabbedHandle != null) {
                    LetGoHandle(handTarget, handTarget.grabbedHandle);
                }
            }

        }

        private static void LetGoStaticObject(HandTarget handTarget, GameObject obj) {
            //Debug.Log("LetGoStaticObject");
        }

        private static void LetGoHandle(HandTarget handTarget, Handle handle) {
            handle.handTarget = null;
            handTarget.grabbedHandle = null;
            if (handTarget.transform.parent == handle.transform)
                handTarget.transform.parent = handTarget.humanoid.transform;
        }
        #endregion

        public static void GrabOrLetGo(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            if (handTarget.grabbedObject != null)
                LetGo(handTarget);
            else
                Grab(handTarget, obj, rangeCheck);
        }
    }
}