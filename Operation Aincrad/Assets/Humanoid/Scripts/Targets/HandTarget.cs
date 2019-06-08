using System.Collections;
using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    public enum ArmBones {
        Hand,
        Forearm,
        UpperArm,
        Shoulder
    }

    public enum HandBones {
        ThumbProximal = 0,
        ThumbIntermediate = 1,
        ThumbDistal = 2,
        IndexProximal = 3,
        IndexIntermediate = 4,
        IndexDistal = 5,
        MiddleProximal = 6,
        MiddleIntermediate = 7,
        MiddleDistal = 8,
        RingProximal = 9,
        RingIntermediate = 10,
        RingDistal = 11,
        LittleProximal = 12,
        LittleIntermediate = 13,
        LittleDistal = 14,
        LastHandBone = 15
    }

    [System.Serializable]
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/hand-target/")]
    public class HandTarget : HumanoidTarget {
#if LIB
        [System.NonSerialized]
        private HumanoidMovements.Arm arm;
#endif

        public bool isLeft;
        public Side side;
        public Vector3 outward;
        public Vector3 up;

        public FingersTarget fingers = null;

        public HandTarget() {
            shoulder = new TargetedShoulderBone(this);
            upperArm = new TargetedUpperArmBone(this);
            forearm = new TargetedForearmBone(this);
            hand = new TargetedHandBone(this);
            subTargets = new TargetedBone[] {
                shoulder,
                upperArm,
                forearm,
                hand
            };

            fingers = new FingersTarget(this);
        }

        #region Limitations
        public bool rotationSpeedLimitation = false;

        public const float maxShoulderAngle = 30;
        public const float maxUpperArmAngle = 120;
        public const float maxForearmAngle = 130;
        public const float maxHandAngle = 100;

        // for future use
        public static readonly Vector3 minLeftShoulderAngles = new Vector3(0, 0, -45);
        public static readonly Vector3 maxLeftShoulderAngles = new Vector3(0, 45, 0);
        public static readonly Vector3 minRightShoulderAngles = new Vector3(0, -45, 0);
        public static readonly Vector3 maxRightShoulderAngles = new Vector3(0, 0, 45);

        public static readonly Vector3 minLeftUpperArmAngles = new Vector3(-180, -45, -180);
        public static readonly Vector3 maxLeftUpperArmAngles = new Vector3(60, 130, 45);
        public static readonly Vector3 minRightUpperArmAngles = new Vector3(-180, -130, -45);
        public static readonly Vector3 maxRightUpperArmAngles = new Vector3(60, 45, 180);

        public static readonly Vector3 minLeftForearmAngles = new Vector3(0, 0, 0);
        public static readonly Vector3 maxLeftForearmAngles = new Vector3(0, 150, 0);
        public static readonly Vector3 minRightForearmAngles = new Vector3(0, -150, 0);
        public static readonly Vector3 maxRightForearmAngles = new Vector3(0, 0, 0);

        public static readonly Vector3 minLeftHandAngles = new Vector3(-180, -50, -70);
        public static readonly Vector3 maxLeftHandAngles = new Vector3(90, 20, 90);
        public static readonly Vector3 minRightHandAngles = new Vector3(-90, -20, -70);
        public static readonly Vector3 maxRightHandAngles = new Vector3(45, 50, 70);
        #endregion

        #region Sensors
        private ArmPredictor armPredictor = new ArmPredictor();
        public ArmAnimator handAnimator = new ArmAnimator();
        public override void EnableAnimator(bool enabled) {
            handAnimator.enabled = enabled;
        }

#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public SteamVRHand steamVR = new SteamVRHand();
#if hVIVETRACKER
        public ViveTrackerArm viveTracker = new ViveTrackerArm();
#endif
#endif
#if hOCULUS
        public OculusHand oculus = new OculusHand();
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
        public WindowsMRHand mixedReality = new WindowsMRHand();
#endif
#if hVRTK
        public VrtkHand vrtk = new VrtkHand();
#endif
#if hNEURON
        public NeuronHand neuron = new NeuronHand();
#endif
#if hLEAP
        public LeapMotionHand leap = new LeapMotionHand();
#endif
#if hKINECT1
        public Kinect1Arm kinect1 = new Kinect1Arm();
#endif
#if hKINECT2
        public Kinect2Arm kinect = new Kinect2Arm();
#endif
#if hORBBEC
        public AstraArm astra = new AstraArm();
#endif
#if hHYDRA
        public RazerHydraHand hydra = new RazerHydraHand();
#endif
#if hOPTITRACK
        public OptitrackArm optitrack = new OptitrackArm();
#endif

        private UnityArmSensor[] sensors;

        public override void InitSensors() {
            if (sensors == null) {
                sensors = new UnityArmSensor[] {
                    armPredictor,
                    handAnimator,
#if hMINDSTORMS
                    mindstormsHand,
#endif
#if hHYDRA
                    hydra,
#endif
#if hLEAP
                    leap,
#endif
#if hKINECT1
                    kinect1,
#endif
#if hKINECT2
                    kinect,
#endif
#if hORBBEC
                    astra,
#endif
#if hOCULUS
                    oculus,
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
                    mixedReality,
#endif
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                    steamVR,
#if hVIVETRACKER
                    viveTracker,
#endif
#endif
#if hVRTK
                    vrtk,
#endif
#if hREALSENSE
                    //realsenseHand,
#endif
#if hNEURON
                    neuron,
#endif
#if hOPTITRACK
                    optitrack,
#endif
                    };
            }
        }

        public override void StartSensors() {
            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Start(humanoid, this.transform);
        }

        protected override void UpdateSensors() {
            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Update();
        }

        public override void StopSensors() {
            if (sensors == null)
                return;

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Stop();
        }
        #endregion

        #region SubTargets
        public override TargetedBone main {
            get { return hand; }
        }
        public Transform stretchlessTarget;
        private readonly TargetedBone[] subTargets;
        #region Shoulder
        public TargetedShoulderBone shoulder;

        [System.Serializable]
        public class TargetedShoulderBone : TargetedBone {
            private HandTarget handTarget;

            public TargetedShoulderBone(HandTarget handTarget) : base(handTarget.upperArm) {
                this.handTarget = handTarget;
            }

            public override void Init() {
                if (handTarget.humanoid == null || handTarget.humanoid.hipsTarget == null)
                    parent = null;
                else
                    parent = (handTarget.humanoid.hipsTarget.chest.bone.transform != null) ?
                        (TargetedBone)handTarget.humanoid.hipsTarget.chest :
                        (TargetedBone)handTarget.humanoid.hipsTarget.hips;

                nextBone = handTarget.upperArm;

                boneId = handTarget.isLeft ? Bone.LeftShoulder : Bone.RightShoulder;

                bone.maxAngle = maxShoulderAngle;
                if (handTarget.isLeft) {
                    bone.minAngles = minLeftShoulderAngles;
                    bone.maxAngles = maxLeftShoulderAngles;
                }
                else {
                    bone.minAngles = minRightShoulderAngles;
                    bone.maxAngles = maxRightShoulderAngles;
                }
            }

            public override Quaternion DetermineRotation() {
                Quaternion armRotation = handTarget.humanoid.transform.rotation;

                Vector3 shoulderOutwardDirection = handTarget.upperArm.bone.transform.position - bone.transform.position;

                Quaternion shoulderRotation = Quaternion.LookRotation(shoulderOutwardDirection, Vector3.up) * Quaternion.AngleAxis(handTarget.isLeft ? 90 : -90, Vector3.up);
                bone.baseRotation = Quaternion.Inverse(armRotation) * shoulderRotation;
                return shoulderRotation;
            }

            public override float GetTension() {
                if (parent == null)
                    return 0;

                Quaternion restRotation = parent.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        #region UpperArm
        public TargetedUpperArmBone upperArm;

        [System.Serializable]
        public class TargetedUpperArmBone : TargetedBone {
            private HandTarget handTarget;

            public TargetedUpperArmBone(HandTarget handTarget) : base(handTarget.forearm) {
                this.handTarget = handTarget;
            }

            public override void Init() {
                parent = handTarget.shoulder;
                nextBone = handTarget.forearm;

                boneId = handTarget.isLeft ? Bone.LeftUpperArm : Bone.RightUpperArm;

                bone.maxAngle = maxUpperArmAngle;
                if (handTarget.isLeft) {
                    bone.minAngles = minLeftUpperArmAngles;
                    bone.maxAngles = maxLeftUpperArmAngles;
                }
                else {
                    bone.minAngles = minRightUpperArmAngles;
                    bone.maxAngles = maxRightUpperArmAngles;
                }
            }

            public override Quaternion DetermineRotation() {
                Vector3 upperArmBoneDirection = (handTarget.forearm.bone.transform.position - bone.transform.position).normalized;

                Vector3 upperArmUp = ArmMovements.CalculateUpperArmUp(handTarget.forearm.bone.targetRotation);

                Quaternion rotation = Quaternion.LookRotation(upperArmBoneDirection, upperArmUp);
                if (handTarget.isLeft)
                    return rotation * Quaternion.Euler(0, 90, 0);
                else
                    return rotation * Quaternion.Euler(0, -90, 0);
            }

            public override float GetTension() {
                Quaternion restRotation = handTarget.shoulder.bone.targetRotation * Quaternion.AngleAxis(handTarget.isLeft ? 45 : -45, Vector3.forward);
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        #region Forearm
        public TargetedForearmBone forearm;

        [System.Serializable]
        public class TargetedForearmBone : TargetedBone {
            private HandTarget handTarget;

            public TargetedForearmBone(HandTarget handTarget) : base(handTarget.hand) {
                this.handTarget = handTarget;
            }

            public override void Init() {
                parent = handTarget.upperArm;
                nextBone = handTarget.hand;

                boneId = handTarget.isLeft ? Bone.LeftForearm : Bone.RightForearm;

                bone.maxAngle = maxForearmAngle;
                if (handTarget.isLeft) {
                    bone.minAngles = minLeftForearmAngles;
                    bone.maxAngles = maxLeftForearmAngles;
                }
                else {
                    bone.minAngles = minRightForearmAngles;
                    bone.maxAngles = maxRightForearmAngles;
                }
            }

            public override Quaternion DetermineRotation() {
                if (handTarget.hand.bone.transform == null)
                    return Quaternion.identity;

                Vector3 forearmBoneDirection = (handTarget.hand.bone.transform.position - bone.transform.position).normalized;
                Vector3 upperArmBoneDirection = (bone.transform.position - handTarget.upperArm.bone.transform.position).normalized;
                Vector3 rotationAxis = Vector3.Cross(upperArmBoneDirection, forearmBoneDirection);

                if (handTarget.isLeft)
                    return Quaternion.LookRotation(forearmBoneDirection, rotationAxis) * Quaternion.Euler(0, 90, 0);
                else
                    return Quaternion.LookRotation(forearmBoneDirection, -rotationAxis) * Quaternion.Euler(0, -90, 0);
            }

            public override float GetTension() {
                if (handTarget.upperArm.bone.transform == null)
                    return 0;

                Quaternion restRotation = handTarget.upperArm.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        #region Hand
        public TargetedHandBone hand;

        [System.Serializable]
        public class TargetedHandBone : TargetedBone {
            private HandTarget handTarget;

            public TargetedHandBone(HandTarget handTarget) {
                this.handTarget = handTarget;
            }

            public override void Init() {
                parent = handTarget.forearm;

                boneId = handTarget.isLeft ? Bone.LeftHand : Bone.RightHand;

                bone.maxAngle = maxHandAngle;
                if (handTarget.isLeft) {
                    bone.minAngles = minLeftHandAngles;
                    bone.maxAngles = maxLeftHandAngles;
                }
                else {
                    bone.minAngles = minRightHandAngles;
                    bone.maxAngles = maxRightHandAngles;
                }
            }

            public override Quaternion DetermineRotation() {
                Vector3 outward = handTarget.HandBoneOutwardAxis();
                Vector3 right = handTarget.HandBoneRightAxis();
                Vector3 up = Vector3.Cross(outward, right);

                if (handTarget.isLeft)
                    return Quaternion.LookRotation(outward, up) * Quaternion.Euler(0, 90, 0);
                else
                    return Quaternion.LookRotation(outward, up) * Quaternion.Euler(0, -90, 0);
            }

            public override float GetTension() {
                if (handTarget.forearm.bone.transform == null)
                    return 0;

                Quaternion restRotation = handTarget.forearm.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }

            protected override void DetermineBasePosition() {
                if (target.basePosition.sqrMagnitude != 0)
                    // Base Position is already determined
                    return;

                Transform basePositionReference = GetBasePositionReference();
                target.basePosition = basePositionReference.InverseTransformPoint(target.transform.position);
            }

            public override Vector3 TargetBasePosition() {
                Transform basePositionReference = GetBasePositionReference();
                return basePositionReference.TransformPoint(target.basePosition);
            }

            private Transform GetBasePositionReference() {
                return handTarget.humanoid.headTarget.neck.target.transform;
            }
        }
        #endregion

        private void InitSubTargets() {
            //foreach (TargetedBone subTarget in subTargets)
            //    subTarget.Init();
            shoulder.Init();
            upperArm.Init();
            forearm.Init();
            hand.Init();
        }

        private void SetTargetPositionsToAvatar() {
            hand.SetTargetPositionToAvatar();
            forearm.SetTargetPositionToAvatar();
            upperArm.SetTargetPositionToAvatar();
            shoulder.SetTargetPositionToAvatar();
        }

        private void DoMeasurements() {
            hand.DoMeasurements();
            forearm.DoMeasurements();
            upperArm.DoMeasurements();
            shoulder.DoMeasurements();
        }

        public override Transform GetDefaultTarget(HumanoidControl humanoid) {
            Transform targetTransform = null;
            if (humanoid != null)
                GetDefaultBone(humanoid.targetsRig, ref targetTransform, isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            return targetTransform;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        public static HandTarget CreateTarget(HandTarget oldTarget) {
            HumanoidControl humanoid = oldTarget.humanoid;

            GameObject targetObject = new GameObject();
            if (oldTarget.isLeft)
                targetObject.name = "Left Hand Target";
            else
                targetObject.name = "Right Hand Target";
            Transform targetTransform = targetObject.transform;

            targetTransform.parent = humanoid.transform;
            targetTransform.position = oldTarget.transform.position;
            targetTransform.rotation = oldTarget.transform.rotation;

            HandTarget handTarget = Constructor(humanoid, oldTarget.isLeft, targetTransform);
            if (oldTarget.isLeft) {
                humanoid.leftHandTarget = handTarget;
                //handTarget.otherHand = humanoid.rightHandTarget;
            }
            else {
                humanoid.rightHandTarget = handTarget;
                //handTarget.otherHand = humanoid.leftHandTarget;
            }

            handTarget.RetrieveBones();
            handTarget.InitAvatar();
            handTarget.MatchTargetsToAvatar();

            return handTarget;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        // Changes the target transform used for this head target
        // Generates a new headtarget component, so parameters will be lost if transform is changed
        public static HandTarget SetTarget(HumanoidControl humanoid, Transform targetTransform, bool isLeft) {
            HandTarget currentHandTarget = isLeft ? humanoid.leftHandTarget : humanoid.rightHandTarget;
            if (targetTransform == currentHandTarget.transform)
                return currentHandTarget;

            GetDefaultBone(humanoid.targetsRig, ref targetTransform, isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            if (targetTransform == null)
                return currentHandTarget;

            HandTarget handTarget = targetTransform.GetComponent<HandTarget>();
            if (handTarget == null)
                handTarget = targetTransform.gameObject.AddComponent<HandTarget>();

            if (isLeft)
                humanoid.leftHandTarget = handTarget;
            else
                humanoid.rightHandTarget = handTarget;

            handTarget.NewComponent(humanoid);

            return handTarget;
        }

        public TargetedBone GetTargetBone(ArmBones boneID) {
            switch (boneID) {
                case ArmBones.Hand:
                    return hand;
                case ArmBones.Forearm:
                    return forearm;
                case ArmBones.UpperArm:
                    return upperArm;
                case ArmBones.Shoulder:
                    return shoulder;
                default:
                    return null;
            }
        }
        #endregion

        #region Configuration
        public bool IsInTPose() {
            if (hand.bone.transform != null) {
                float d;
                Ray upper2hand = new Ray(upperArm.bone.transform.position, hand.bone.transform.position - upperArm.bone.transform.position);

                // Horizontal?
                if (Mathf.Abs(upper2hand.direction.y) > 0.05F)
                    return false;

                // All lined up?
                d = Vectors.DistanceToRay(upper2hand, forearm.bone.transform.position);
                if (d > 0.05F)
                    return false;

                // Arms stretched?
                d = Vector3.Distance(upperArm.bone.transform.position, hand.bone.transform.position);
                if (d < upperArm.bone.length - 0.05F)
                    return false;

                //Debug.Log("Arm is in T-pose");
                return true;
            }
            else
                return false;
        }

        public static void ClearBones(HandTarget handTarget) {
            handTarget.handMovements.ReattachHand();
            handTarget.shoulder.bone.transform = null;
            handTarget.upperArm.bone.transform = null;
            handTarget.forearm.bone.transform = null;
            handTarget.hand.bone.transform = null;
            handTarget.ClearHandBones();
        }

        private void ClearHandBones() {
            fingers.thumb.proximal.bone.transform = null;
            fingers.index.proximal.bone.transform = null;
            fingers.middle.proximal.bone.transform = null;
            fingers.ring.proximal.bone.transform = null;
            fingers.little.proximal.bone.transform = null;
        }

        public void RetrieveBones() {
            foreach (TargetedBone subTarget in subTargets)
                subTarget.RetrieveBones(humanoid);

            fingers.RetrieveBones(this);
        }
        #endregion

        #region Settings
        //public bool jointLimitations = true;

        public enum PoseMethod {
            Position,
            Rotation
        }
        public PoseMethod poseMethod;

        public override bool showRealObjects {
            get { return base.showRealObjects; }
            set {
                if (value != base.showRealObjects) {
                    base.showRealObjects = value;
                    ShowSensors(value);
                }
            }
        }
        public void ShowSensors(bool shown) {
            if (sensors == null)
                InitSensors();

            if (sensors == null)
                return;

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].ShowSensor(shown);
        }

        public bool physics = true;
        public bool touchInteraction = true;
        public float strength = 100;
        #endregion

        #region Poses
        public int pose;
        public PoseMixer poseMixer = new PoseMixer();
        public enum PoseIndex {
            Pose1,
            Pose2,
            Pose3,
            Pose4
        }
        public void SetPoseI(PoseIndex poseIx, float weight = 1) {
            int i = (int)poseIx;
            if (i < 0 || i >= poseMixer.mixedPoses.Count)
                return;
            poseMixer.SetPoseValue(poseMixer.mixedPoses[(int)poseIx], weight);
        }
        public void SetPose(Pose pose, float weight = 1) {
            poseMixer.SetPoseValue(pose, weight);
        }
        #endregion

        #region Interaction
        [System.NonSerialized]
        public Interaction inputModule;

        [System.NonSerialized]
        public GameObject touchedObject = null;
        public GameObject grabbedObject = null;
        //[HideInInspector]
        public Handle grabbedHandle = null;
        public enum GrabType {
            None,
            Palm,
            Pinch
        }
        public GrabType grabType;

        public StoredRigidbody grabbedRBdata;
        public Vector3 storedCOM;

        public bool GrabbedStaticObject() {
            return (grabbedObject != null && grabbedRBdata == null);
        }

        public static void TmpDisableCollisions(HandTarget handTarget, float duration) {
            handTarget.StartCoroutine(TmpDisableCollisions(handTarget.hand.bone.transform.gameObject, duration));
        }

        private static IEnumerator TmpDisableCollisions(GameObject handObj, float duration) {
            HandMovements.SetAllColliders(handObj, false);
            yield return new WaitForSeconds(duration);
            HandMovements.SetAllColliders(handObj, true);
        }
        #endregion

        public Transform handPalm;
        public Rigidbody handRigidbody;

        public AdvancedHandPhysics handPhysics;

        public HandMovements handMovements = new HandMovements();
        public ArmMovements armMovements = new ArmMovements();

        public HandTarget otherHand {
            get {
                return isLeft ? humanoid.rightHandTarget : humanoid.leftHandTarget;
            }
        }

        private Vector3 _localPalmPosition = Vector3.zero;
        public Vector3 localPalmPosition {
            get {
                if (_localPalmPosition == Vector3.zero)
                    CalculatePalm();
                return _localPalmPosition;
            }
        }
        private Quaternion localPalmRotation = Quaternion.identity;
        private void CalculatePalm() {
            Transform indexFingerBone = fingers.index.proximal.bone.transform;
            Transform middleFingerBone = fingers.middle.proximal.bone.transform;

            // Determine position
            if (indexFingerBone)
                _localPalmPosition = (indexFingerBone.position - hand.bone.transform.position) * 0.9F + new Vector3(0, 0, 0);
            else if (middleFingerBone)
                _localPalmPosition = (middleFingerBone.position - hand.bone.transform.position) * 0.9F + new Vector3(0, 0, 0);
            else
                _localPalmPosition = new Vector3(0.1F, 0, 0);

            Vector3 handPalmPosition = hand.bone.transform.position + _localPalmPosition;

            Vector3 handUp = hand.bone.targetRotation * Vector3.up;
            Vector3 handForward = Vector3.zero;

            if (indexFingerBone)
                handForward = indexFingerBone.position - handPalmPosition;
            else if (middleFingerBone)
                handForward = middleFingerBone.position - handPalmPosition;
            else if (isLeft)
                handForward = -humanoid.avatarRig.transform.right;
            else
                handForward = humanoid.avatarRig.transform.right;

            Quaternion worldPalmRotation = Quaternion.LookRotation(handForward, handUp);
            localPalmRotation = Quaternion.Inverse(hand.target.transform.rotation) * worldPalmRotation;
            _localPalmPosition = Quaternion.Inverse(hand.target.transform.rotation) * _localPalmPosition;

            // Now get it in the palm
            if (isLeft) {
                localPalmRotation *= Quaternion.Euler(0, -45, -90);
                _localPalmPosition += localPalmRotation * new Vector3(0.02F, -0.04F, 0);
            }
            else {
                localPalmRotation *= Quaternion.Euler(0, 45, 90);
                _localPalmPosition += localPalmRotation * new Vector3(-0.02F, -0.04F, 0);
            }
        }

        public Vector3 palmPosition {
            get {
                //if (localPalmPosition.sqrMagnitude == 0)
                //    CalculatePalm();

                Vector3 handPalmPosition = hand.bone.transform.position + hand.bone.targetRotation * localPalmPosition;
                return handPalmPosition;
            }
        }
        public Quaternion palmRotation {
            get {
                if (localPalmPosition.sqrMagnitude == 0)
                    CalculatePalm();

                Quaternion handPalmRotation = hand.bone.targetRotation * localPalmRotation;
                return handPalmRotation;
            }
        }

        public static void DeterminePalmPosition(HandTarget handTarget) {
            if (handTarget.hand.bone.transform == null)
                return;

            if (handTarget.handPalm == null) {
                handTarget.handPalm = handTarget.hand.bone.transform.Find("Hand Palm");
                if (handTarget.handPalm == null) {
                    GameObject handPalmObj = new GameObject("Hand Palm");
                    handTarget.handPalm = handPalmObj.transform;
                    handTarget.handPalm.parent = handTarget.hand.bone.transform;
                }
            }


            Transform indexFingerBone = handTarget.fingers.index.proximal.bone.transform; // handTarget.fingers.indexFinger.bones[(int)FingerBones.Proximal];
            Transform middleFingerBone = handTarget.fingers.middle.proximal.bone.transform; //.middleFinger.bones[(int)FingerBones.Proximal];

            // Determine position
            Vector3 palmOffset;
            if (indexFingerBone)
                palmOffset = (indexFingerBone.position - handTarget.hand.bone.transform.position) * 0.9F;
            else if (middleFingerBone)
                palmOffset = (middleFingerBone.position - handTarget.hand.bone.transform.position) * 0.9F;
            else
                palmOffset = new Vector3(0.1F, 0, 0);

            handTarget.handPalm.position = handTarget.hand.bone.transform.position + palmOffset;

            Vector3 handUp = handTarget.hand.bone.targetRotation * Vector3.up;

            // Determine rotation
            if (indexFingerBone)
                handTarget.handPalm.LookAt(indexFingerBone, handUp);
            else if (middleFingerBone)
                handTarget.handPalm.LookAt(middleFingerBone, handUp);
            else if (handTarget.isLeft)
                handTarget.handPalm.LookAt(handTarget.handPalm.position - handTarget.humanoid.avatarRig.transform.right, handUp);
            else
                handTarget.handPalm.LookAt(handTarget.handPalm.position + handTarget.humanoid.avatarRig.transform.right, handUp);

            // Now get it in the palm
            if (handTarget.isLeft) {
                handTarget.handPalm.rotation *= Quaternion.Euler(0, -45, -90);
                handTarget.handPalm.position += handTarget.handPalm.rotation * new Vector3(0.02F, -0.02F, 0);
            }
            else {
                handTarget.handPalm.rotation *= Quaternion.Euler(0, 45, 90);
                handTarget.handPalm.position += handTarget.handPalm.rotation * new Vector3(-0.02F, -0.02F, 0);
            }
        }

        // index<->little
        public Vector3 HandBoneRightAxis() {
            if (fingers.index.proximal.bone.transform == null || fingers.little.proximal.bone.transform == null)
                return isLeft ? Vector3.forward : Vector3.back;

            Transform indexFingerBone = fingers.index.proximal.bone.transform;
            Transform littleFingerBone = fingers.little.proximal.bone.transform;

            if (indexFingerBone == null || littleFingerBone == null)
                return Vector3.zero;

            Vector3 fingersDirection;
            if (isLeft)
                fingersDirection = (indexFingerBone.position - littleFingerBone.position).normalized;
            else
                fingersDirection = (littleFingerBone.position - indexFingerBone.position).normalized;

            return fingersDirection;//humanoid.transform.InverseTransformDirection(fingersDirection);
        }

        public Vector3 HandBoneOutwardAxis() {
            Transform fingerBone = null;
            //if (fingers.middleFinger != null && fingers.middleFinger.bones.Length > 0 && fingers.middleFinger.bones[0] != null)
            if (fingers.middle.proximal.bone.transform != null)
                fingerBone = fingers.middle.proximal.bone.transform; // middleFinger.bones[0];
            //else if (fingers.indexFinger != null && fingers.indexFinger.bones.Length > 0 && fingers.indexFinger.bones[0] != null)
            else if (fingers.index.proximal.bone.transform != null)
                fingerBone = fingers.index.proximal.bone.transform; // fingers.indexFinger.bones[0];

            if (fingerBone == null)
                return Vector3.forward;

            Vector3 outward = (fingerBone.position - hand.bone.transform.position).normalized;
            return outward;
        }

        #region Init
        public static bool IsInitialized(HumanoidControl humanoid) {
            if (humanoid.leftHandTarget == null || humanoid.leftHandTarget.humanoid == null)
                return false;
            if (humanoid.leftHandTarget.hand.target.transform == null || humanoid.rightHandTarget.hand.target.transform == null)
                return false;
            if (humanoid.rightHandTarget == null || humanoid.rightHandTarget.humanoid == null)
                return false;
            return true;
        }
        private void Reset() {
            humanoid = GetHumanoid();
            if (humanoid == null)
                return;

            //poses.poses = null;
            NewComponent(humanoid);

            shoulder.bone.maxAngle = maxShoulderAngle;
            upperArm.bone.maxAngle = maxUpperArmAngle;
            forearm.bone.maxAngle = maxForearmAngle;
            hand.bone.maxAngle = maxHandAngle;
        }

        private HumanoidControl GetHumanoid() {
            // This does not work for prefabs
            HumanoidControl[] humanoids = FindObjectsOfType<HumanoidControl>();

            for (int i = 0; i < humanoids.Length; i++) {
                if ((humanoids[i].leftHandTarget != null && humanoids[i].leftHandTarget.transform == this.transform) ||
                    (humanoids[i].rightHandTarget != null && humanoids[i].rightHandTarget.transform == this.transform)) {

                    return humanoids[i];
                }
            }

            return null;
        }

        public override void InitAvatar() {
            InitSubTargets();

            shoulder.DoMeasurements();
            upperArm.DoMeasurements();
            forearm.DoMeasurements();
            hand.DoMeasurements();

            fingers.InitAvatar();

            DeterminePalmPosition(this);
            HandMovements.DetachHand(this);
        }

        // This function is called only when the humanoid is created
        private static HandTarget Constructor(HumanoidControl humanoid, bool isLeft, Transform handTargetTransform) {
            HandTarget handTarget = handTargetTransform.gameObject.AddComponent<HandTarget>();
            handTarget.humanoid = humanoid;
            handTarget.isLeft = isLeft;
            handTarget.side = isLeft ? Side.Left : Side.Right;
            handTarget.outward = handTarget.isLeft ? Vector3.left : Vector3.right;

            handTarget.InitSubTargets();
            return handTarget;
        }

        public override void NewComponent(HumanoidControl _humanoid) {
            humanoid = _humanoid;
            isLeft = (this == humanoid.leftHandTarget);
            if (isLeft)
                outward = Vector3.left;
            else
                outward = Vector3.right;

            fingers.NewComponent(this);

            if (hand == null)
                hand = new TargetedHandBone(this);
            if (forearm == null)
                forearm = new TargetedForearmBone(this);
            if (upperArm == null)
                upperArm = new TargetedUpperArmBone(this);

            //otherHand = isLeft ? humanoid.rightHandTarget : humanoid.leftHandTarget;

            InitComponent();
        }

        // This function is called every time the avatar is changed
        public override void InitComponent() {
            if (humanoid == null)
                return;

            //bones = new TargetedBone[] { hand, forearm, upperArm, shoulder };
            //bonesReverse = new TargetedBone[] { hand, forearm, upperArm, shoulder };

            InitSubTargets();
            //foreach (TargetedBone bone in bones)
            //    bone.Init(this);

            //RetrieveBones();
#if LIB
            arm = new HumanoidMovements.Arm();
            RetrieveBoneTransforms();            
            //arm.Init();
#endif
            DeterminePalmPosition(this);

            // We need to do this before the measurements
            //foreach (TargetedBone bone in bones)
            //    bone.SetTargetPositionToAvatar();
            SetTargetPositionsToAvatar();
            //foreach (TargetedBone bone in bones)
            //    bone.DoMeasurements();
            DoMeasurements();

            if (stretchlessTarget == null) {
                stretchlessTarget = hand.target.transform.Find("Stretchless Target");
                if (stretchlessTarget == null) {

                    GameObject stretchlessTargetObj = new GameObject("Stretchless Target");
                    stretchlessTarget = stretchlessTargetObj.transform;
                    stretchlessTarget.parent = hand.target.transform;
                    stretchlessTarget.localPosition = Vector3.zero;
                    stretchlessTarget.localRotation = Quaternion.identity;
                }
            }

            //poses.InitPoses(fingers);

        }

        public override void StartTarget() {
            side = isLeft ? Side.Left : Side.Right;

            InitSensors();
            //RetrieveBones();

            //#if LIB
            //            arm = new HumanoidMovements.Arm();
            //            RetrieveBoneTransforms();
            //            arm.Init();
            //#endif
            //fingers.CalculateFingerRetargeting();

            CheckColliders();
#if hVRTK
            // VRTK is not compatible with touch interaction
            // because it uses its own EventSystem
            // which will be destroyed by humanoid control
            if (touchInteraction && !(humanoid.vrtk.enabled && vrtk.enabled))
#else
            if (touchInteraction)
#endif
                HandInteraction.StartInteraction(this);


            if (humanoid.avatarRig != null) {
                Vector3 handRightAxis = HandBoneRightAxis();
                Vector3 handOutwardAxis = HandBoneOutwardAxis();
                up = Vector3.Cross(handOutwardAxis, handRightAxis);
            }

            handMovements.Start(humanoid, this);

            if (humanoid.physics && physics && hand.bone.transform != null)
                handPhysics = hand.bone.transform.GetComponent<AdvancedHandPhysics>();
        }

        /// <summary>
        /// Checks whether the humanoid has an HandTarget
        /// and adds one if none has been found
        /// </summary>
        /// <param name="humanoid">The humanoid to check</param>
        /// <param name="isLeft">Is this the left hand?</param>
        public static void DetermineTarget(HumanoidControl humanoid, bool isLeft) {
            HandTarget handTarget = isLeft ? humanoid.leftHandTarget : humanoid.rightHandTarget;

            if (handTarget == null) {
                Transform handTargetTransform = humanoid.targetsRig.GetBoneTransform(isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                if (handTargetTransform == null) {
                    Debug.LogError("Could not find hand bone in targets rig");
                    return;
                }

                handTarget = handTargetTransform.GetComponent<HandTarget>();
                if (handTarget == null)
                    handTarget = Constructor(humanoid, isLeft, handTargetTransform);
            }

            if (isLeft)
                humanoid.leftHandTarget = handTarget;
            else
                humanoid.rightHandTarget = handTarget;
        }

#if LIB
        private void RetrieveBoneTransforms() {
            arm.SetBonePosition(ArmBones.Shoulder, shoulder.bone.transform.position);
            arm.SetBoneRotation(ArmBones.Shoulder, shoulder.bone.transform.rotation);

            arm.SetBonePosition(ArmBones.UpperArm, upperArm.bone.transform.position);
            arm.SetBoneRotation(ArmBones.UpperArm, upperArm.bone.transform.rotation);

            arm.SetBonePosition(ArmBones.Forearm, forearm.bone.transform.position);
            arm.SetBoneRotation(ArmBones.Forearm, forearm.bone.transform.rotation);

            arm.SetBonePosition(ArmBones.Hand, hand.bone.transform.position);
            arm.SetBoneRotation(ArmBones.Hand, hand.bone.transform.rotation);
        }
#endif

        public override void MatchTargetsToAvatar() {
            if (shoulder != null)
                shoulder.MatchTargetToAvatar();
            if (upperArm != null)
                upperArm.MatchTargetToAvatar();
            if (forearm != null)
                forearm.MatchTargetToAvatar();
            MatchHandTargetToAvatar();
#if LIB
            if (shoulder != null)
                CopyBoneToTarget(ArmBones.Shoulder, shoulder.bone);
            arm.SetTargetRotation(ArmBones.Shoulder, shoulder.target.transform.rotation);
            if (upperArm != null)
                CopyBoneToTarget(ArmBones.UpperArm, upperArm.bone);
            if (forearm != null)
                CopyBoneToTarget(ArmBones.Forearm, forearm.bone);
            if (hand != null)
                CopyBoneToTarget(ArmBones.Hand, hand.bone);

            Debug.Log(hand.target.transform.eulerAngles + " " + arm.GetTargetRotation(ArmBones.Shoulder).eulerAngles);
            arm.Init();
#endif

            fingers.MatchTargetsToAvatar();
        }
#if LIB
        public void CopyBoneToTarget(ArmBones boneId, BoneTransform bone) {
            if (bone.transform != null) {
                arm.SetTargetPosition(boneId, bone.transform.position);
                Debug.Log(bone.targetRotation.eulerAngles);
                arm.SetTargetRotation(boneId, bone.targetRotation);
            }
        }
#endif

        private void MatchHandTargetToAvatar() {
            if (hand == null)
                return;

            //hand.DoMeasurements();
            if (hand.bone.transform != null) {
                transform.position = hand.bone.transform.position;
                transform.rotation = hand.bone.targetRotation;
                if (hand.target.transform != null) {
                    hand.target.transform.position = transform.position;
                    hand.target.transform.rotation = transform.rotation;
                }
            }
        }

        private void MatchFingersToAvatar() {

        }
        #endregion

        #region Update
        public override void UpdateTarget() {
            hand.target.confidence.Degrade();
            forearm.target.confidence.Degrade();
            upperArm.target.confidence.Degrade();
            shoulder.target.confidence.Degrade();

            poseMixer.ShowPose(humanoid, isLeft ? Side.Left : Side.Right);

            for (int i = 0; i < sensors.Length; i++) {
                sensors[i].Update();
            }

#if LIB
            arm.SetTargets(this);
#endif
            hand.target.CalculateVelocity();
        }

        public override void UpdateMovements(HumanoidControl humanoid) {
            if (humanoid == null || !humanoid.calculateBodyPose)
                return;

            ArmMovements.Update(this);
            HandMovements.Update(this);
            FingerMovements.Update(this);
#if LIB
                if (arm != null) {
                    arm.UpdateMovements();
                    arm.CopyToRig(this);
                }
#endif
        }

        [HideInInspector]
        public bool directFingerMovements = true;
        public override void CopyTargetToRig() {
            if (humanoid == null)
                return;

            if (Application.isPlaying &&
                humanoid.animatorEnabled && humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            if (hand.target.transform != null && transform != hand.target.transform) {
                hand.target.transform.position = transform.position;
                hand.target.transform.rotation = transform.rotation;
            }
#if LIB
            arm.SetTargets(this);
#endif


            if (directFingerMovements)
                FingersTarget.CopyFingerTargetsToRig(this);
        }

        public override void CopyRigToTarget() {
            if (hand.target.transform != null && transform != hand.target.transform) {
                transform.position = hand.target.transform.position;
                transform.rotation = hand.target.transform.rotation;
            }

            FingersTarget.CopyRigToFingerTargets(this);
            //pose = HandPoses.DetermineHandPose(fingers, out poseConfidence);

            HandInteraction.UpdateInteraction();

            // Wierd place for this, but it needs the finger subtargets to work
            if (humanoid.avatarRig != null) {
                FingerMovements.Update(this);
            }
        }

        public void UpdateSensorsFromTarget() {
            if (sensors == null)
                return;

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].UpdateSensorTransformFromTarget(this.transform);
        }

        public float GetFingerCurl(Finger fingerID) {
            return fingers.GetFingerCurl(fingerID);
        }
        public float GetFingerCurl(FingersTarget.TargetedFinger finger) {
            return finger.GetCurl(fingers.handTarget);
        }

        public void AddFingerCurl(Finger fingerID, float curlValue) {
            fingers.AddFingerCurl(fingerID, curlValue);
        }

        public void SetFingerCurl(Finger fingerID, float curlValue) {
            fingers.SetFingerCurl(fingerID, curlValue);
        }

        public void SetFingerGroupCurl(FingersTarget.FingerGroup fingerGroupID, float curlValue) {
            fingers.SetFingerGroupCurl(fingerGroupID, curlValue);
        }

        public void DetermineFingerCurl(Finger fingerID) {
            fingers.DetermineFingerCurl(fingerID);
        }

        public float HandCurl() {
            float indexCurl = fingers.index.GetCurl(this);
            float middleCurl = fingers.middle.GetCurl(this);
            float ringCurl = fingers.ring.GetCurl(this);
            float littleCurl = fingers.little.GetCurl(this);
            return indexCurl + middleCurl + ringCurl + littleCurl;
        }
        #endregion

        #region DrawRigs
        public override void DrawTargetRig(HumanoidControl humanoid) {
            if (this != humanoid.leftHandTarget && this != humanoid.rightHandTarget)
                return;

            if (shoulder != null)
                DrawTargetBone(shoulder, outward);
            if (upperArm != null)
                DrawTargetBone(upperArm, outward);
            if (forearm != null)
                DrawTargetBone(forearm, outward);
            if (hand != null)
                DrawTargetBone(hand, outward);

            fingers.DrawTargetRig(this);
        }

        public override void DrawAvatarRig(HumanoidControl humanoid) {
            if (this != humanoid.leftHandTarget && this != humanoid.rightHandTarget)
                return;

            if (shoulder != null)
                DrawAvatarBone(shoulder, outward);
            if (upperArm != null)
                DrawAvatarBone(upperArm, outward);
            if (forearm != null)
                DrawAvatarBone(forearm, outward);
            if (hand != null)
                DrawAvatarBone(hand, outward);

            fingers.DrawAvatarRig(this);
        }
        #endregion

        #region Collisions
        private void CheckColliders() {
            if (humanoid.physics && physics && hand.bone.transform != null) {
                Collider c = hand.bone.transform.GetComponent<Collider>();
                // Does not work if the hand has grabbed an object with colliders...
                if (c == null)
                    GenerateColliders();
            }
        }

        // assumes hand scale is uniform!
        private void GenerateColliders() {
            float unscale = 1 / hand.bone.transform.lossyScale.x;

            if (fingers.middle.proximal.bone.transform == null)
                return;

            BoxCollider hc = hand.bone.transform.gameObject.AddComponent<BoxCollider>();
            hc.center = hand.bone.toTargetRotation * (isLeft ? new Vector3(-0.05F * unscale, 0, 0) : new Vector3(0.05F * unscale, 0, 0));
            Vector3 hcSize = hand.bone.toTargetRotation * new Vector3(0.1F * unscale, 0.03F * unscale, 0.05F * unscale);

            hc.size = new Vector3(Mathf.Abs(hcSize.x), Mathf.Abs(hcSize.y), Mathf.Abs(hcSize.z));


            // TO DO: thumb
            for (int i = 1; i < 5; i++) {
                FingersTarget.TargetedFinger finger = fingers.allFingers[i];

                Transform proximal = finger.proximal.bone.transform;
                if (proximal == null)
                    continue;

                Transform intermediate = finger.intermediate.bone.transform;
                Transform distal = finger.distal.bone.transform;

                if (intermediate != null) {
                    Vector3 localIntermediatePosition = proximal.InverseTransformPoint(intermediate.position);
                    //Quaternion fingerRotation = Quaternion.FromToRotation(localIntermediatePosition, Vector3.forward);
                    float proximalLength = Vector3.Distance(proximal.position, intermediate.position);

                    GameObject proximalColliderObj = new GameObject("Proximal Collider");
                    proximalColliderObj.transform.parent = proximal;
                    proximalColliderObj.transform.localPosition = localIntermediatePosition / 2;
                    proximalColliderObj.transform.localRotation = Quaternion.LookRotation(localIntermediatePosition);

                    CapsuleCollider cc = proximalColliderObj.AddComponent<CapsuleCollider>();
                    cc.height = proximalLength * unscale;
                    cc.radius = 0.01F * unscale;
                    cc.direction = 2; // Z-axis

                    if (distal != null) {
                        GameObject distalColliderObj = new GameObject("Distal Collider");
                        distalColliderObj.transform.parent = distal;
                        distalColliderObj.transform.localPosition = localIntermediatePosition.normalized * 0.01F;
                        distalColliderObj.transform.localRotation = Quaternion.identity;

                        SphereCollider sc = distalColliderObj.AddComponent<SphereCollider>();
                        //sc.center = fingerRotation * new Vector3(0, 0, -0.01F * unscale);
                        sc.radius = 0.01F * unscale;
                    }
                }
            }
        }
        #endregion

        public void Vibrate(float strength) {
            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Vibrate(1, strength);
        }
    }
}

#if LIB
namespace Passer.HumanoidMovements {
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3 {
        public float x;
        public float y;
        public float z;

        public Vec3(Vector3 v) {
            x = v.x;
            y = v.y;
            z = v.z;
        }
        public Vector3 Vector3 {
            get { return new Vector3(x, y, z); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Quat {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quat(Quaternion q) {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }
        public Quaternion Quaternion {
            get { return new Quaternion(x, y, z, w); }
        }
    }

    public class Arm {
        private System.IntPtr pArm;

        public Arm() {
            pArm = Arm_Constructor();
        }
        [DllImport("HumanoidMovements")]
        private static extern System.IntPtr Arm_Constructor();

        ~Arm() {
            Arm_Destructor(pArm);
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_Destructor(System.IntPtr pArm);

#region Parameters
        public bool isLeft {
            get { return Arm_GetIsLeft(pArm); }
            set { Arm_SetIsLeft(pArm, value); }
        }
        [DllImport("HumanoidMovements")]
        private static extern bool Arm_GetIsLeft(System.IntPtr pArm);
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetIsLeft(System.IntPtr pArm, bool isLeft);

#region Main Target
        public Vector3 targetPosition {
            get { return Arm_GetMainTargetPosition(pArm).Vector3; }
            set { Arm_SetMainTargetPosition(pArm, new Vec3(value)); }
        }
        [DllImport("HumanoidMovements")]
        private static extern Vec3 Arm_GetMainTargetPosition(System.IntPtr pArm);
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetMainTargetPosition(System.IntPtr pArm, Vec3 position);

        public Quaternion targetRotation {
            get { return Arm_GetMainTargetRotation(pArm).Quaternion; }
            set { Arm_SetMainTargetRotation(pArm, new Quat(value)); }
        }
        [DllImport("HumanoidMovements")]
        private static extern Quat Arm_GetMainTargetRotation(System.IntPtr pArm);
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetMainTargetRotation(System.IntPtr pArm, Quat rotation);
#endregion

#region Sub Target
        public void SetTargetPosition(ArmBones boneId, Vector3 position) {
            Arm_SetTargetPosition(pArm, (int)boneId, new Vec3(position));
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetTargetPosition(System.IntPtr pArm, int boneId, Vec3 position);

        public Vector3 GetTargetPosition(ArmBones boneId) {
            return Arm_GetTargetPosition(pArm, (int)boneId).Vector3;
        }
        [DllImport("HumanoidMovements")]
        private static extern Vec3 Arm_GetTargetPosition(System.IntPtr pArm, int boneId);

        public void SetTargetRotation(ArmBones boneId, Quaternion rotation) {
            Arm_SetTargetRotation(pArm, (int)boneId, new Quat(rotation));
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetTargetRotation(System.IntPtr pArm, int boneId, Quat rotation);

        public Quaternion GetTargetRotation(ArmBones boneId) {
            return Arm_GetTargetRotation(pArm, (int)boneId).Quaternion;
        }
        [DllImport("HumanoidMovements")]
        private static extern Quat Arm_GetTargetRotation(System.IntPtr pArm, int boneId);
#endregion

#region Bone
        // Set Bone Position
        public void SetBonePosition(ArmBones boneId, Vector3 position) {
            Arm_SetBonePosition(pArm, (int)boneId, new Vec3(position));
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetBonePosition(System.IntPtr pArm, int boneId, Vec3 position);

        // Get Bone Position
        public Vector3 GetBonePosition(ArmBones boneId) {
            return Arm_GetBonePosition(pArm, (int)boneId).Vector3;
        }
        [DllImport("HumanoidMovements")]
        private static extern Vec3 Arm_GetBonePosition(System.IntPtr pArm, int boneId);

        // Set Bone Rotation
        public void SetBoneRotation(ArmBones boneId, Quaternion rotation) {
            Arm_SetBoneRotation(pArm, (int)boneId, new Quat(rotation));
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_SetBoneRotation(System.IntPtr pArm, int boneId, Quat rotation);

        // Get Bone Rotation
        public Quaternion GetBoneRotation(ArmBones boneId) {
            return Arm_GetBoneRotation(pArm, (int)boneId).Quaternion;
        }
        [DllImport("HumanoidMovements")]
        private static extern Quat Arm_GetBoneRotation(System.IntPtr pArm, int boneId);
#endregion
#endregion

#region Init
        public void Init() {
            Arm_Init(pArm);
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_Init(System.IntPtr pArm);
#endregion

#region Update
        public void Update() {
            Debug.Log("Update");
            Arm_Update(pArm);
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_Update(System.IntPtr pArm);

        public void UpdateMovements() {
            Arm_UpdateMovements(pArm);
        }
        [DllImport("HumanoidMovements")]
        private static extern void Arm_UpdateMovements(System.IntPtr pArm);
#endregion

#region Tools
        public void SetTargets(HandTarget handTarget) {
            //SetTargetRotation(ArmBones.Shoulder, handTarget.shoulder.target.transform.rotation);
            //SetTargetRotation(ArmBones.UpperArm, handTarget.upperArm.target.transform.rotation);
            //SetTargetRotation(ArmBones.Forearm, handTarget.forearm.target.transform.rotation);
            //SetTargetRotation(ArmBones.Hand, handTarget.hand.target.transform.rotation);
            targetPosition = handTarget.transform.position;
            targetRotation = handTarget.transform.rotation;
        }
        public void CopyToRig(HandTarget handTarget) {
            //Debug.Log(handTarget.hand.bone.transform.rotation + " " + GetBoneRotation(ArmBones.Hand));
            handTarget.hand.bone.transform.rotation = GetBoneRotation(ArmBones.Hand);
            //handTarget.forearm.bone.transform.rotation = GetBoneRotation(ArmBones.Forearm);
            //handTarget.upperArm.bone.transform.rotation = GetBoneRotation(ArmBones.UpperArm);
            //handTarget.shoulder.bone.transform.rotation = GetBoneRotation(ArmBones.Shoulder);
        }
#endregion
    }
}
#endif