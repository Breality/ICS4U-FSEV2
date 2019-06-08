using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Passer.Humanoid.Tracking {

    public enum Side {
        AnySide,
        Left,
        Right,
    }

    public enum BoneType {
        AllBones,
        CenterBones,
        SideBones,
        FaceBones
    }

    public enum Bone {
        None,

        #region Center Bones
        // Torso
        Hips,
        Spine,
        Spine1,
        Spine2,
        Chest,

        // Head
        Neck,
        Head,
        #endregion

        #region Left Side Bones
        // Left Arm
        LeftShoulder,
        LeftUpperArm,
        LeftForearm,
        LeftForearmTwist,
        LeftHand,

        // Thumb
        LeftThumbProximal,
        LeftThumbIntermediate,
        LeftThumbDistal,

        // Index Finger
        LeftIndexMetacarpal,
        LeftIndexProximal,
        LeftIndexIntermediate,
        LeftIndexDistal,

        // Middle Finger
        LeftMiddleMetacarpal,
        LeftMiddleProximal,
        LeftMiddleIntermediate,
        LeftMiddleDistal,

        // Ring Finger
        LeftRingMetacarpal,
        LeftRingProximal,
        LeftRingIntermediate,
        LeftRingDistal,

        // Little Finger
        LeftLittleMetacarpal,
        LeftLittleProximal,
        LeftLittleIntermediate,
        LeftLittleDistal,

        // Left Leg
        LeftUpperLeg,
        LeftLowerLeg,
        LeftFoot,
        LeftToes,
        #endregion

        #region Right Side Bones
        // Right Arm
        RightShoulder,
        RightUpperArm,
        RightForearm,
        RightForearmTwist,
        RightHand,

        // Thumb
        RightThumbProximal,
        RightThumbIntermediate,
        RightThumbDistal,

        // Index Finger
        RightIndexMetacarpal,
        RightIndexProximal,
        RightIndexIntermediate,
        RightIndexDistal,

        // Middle Finger
        RightMiddleMetacarpal,
        RightMiddleProximal,
        RightMiddleIntermediate,
        RightMiddleDistal,

        // Ring Finger
        RightRingMetacarpal,
        RightRingProximal,
        RightRingIntermediate,
        RightRingDistal,

        // Little Finger
        RightLittleMetacarpal,
        RightLittleProximal,
        RightLittleIntermediate,
        RightLittleDistal,

        // Right Leg
        RightUpperLeg,
        RightLowerLeg,
        RightFoot,
        RightToes,
        #endregion

        #region Face Bones

        // Eyes
        LeftUpperLid,
        LeftEye,
        LeftLowerLid,
        RightUpperLid,
        RightEye,
        RightLowerLid,

        // Brows
        LeftOuterBrow,
        LeftBrow,
        LeftInnerBrow,
        RightInnerBrow,
        RightBrow,
        RightOuterBrow,

        // Ears
        LeftEar,
        RightEar,

        // Cheeks
        LeftCheek,
        RightCheek,

        // Nose
        NoseTop,
        NoseTip,
        NoseBottomLeft,
        NoseBottom,
        NoseBottomRight,

        // Mouth
        UpperLipLeft,
        UpperLip,
        UpperLipRight,
        LipLeft,
        LipRight,
        LowerLipLeft,
        LowerLip,
        LowerLipRight,

        Jaw,
        Chin,
        #endregion

        Count
    };

    public enum CenterBone {
        Unknown,

        // Torso
        Hips,
        Spine,
        Spine1,
        Spine2,
        Chest,

        // Head
        Neck,
        Head,

        Count
    }

    public enum SideBone {
        None,

        // Arm
        Shoulder,
        UpperArm,
        Forearm,
        ForearmTwist,
        Hand,

        // Thumb
        ThumbProximal,
        ThumbIntermediate,
        ThumbDistal,

        // Index Finger
        IndexMetacarpal,
        IndexProximal,
        IndexIntermediate,
        IndexDistal,

        // Middle Finger
        MiddleMetacarpal,
        MiddleProximal,
        MiddleIntermediate,
        MiddleDistal,

        // Ring Finger
        RingMetacarpal,
        RingProximal,
        RingIntermediate,
        RingDistal,

        // Little Finger
        LittleMetacarpal,
        LittleProximal,
        LittleIntermediate,
        LittleDistal,

        // Left Leg
        UpperLeg,
        LowerLeg,
        Foot,
        Toes,

        Count
    };

    public enum Finger {
        Thumb,
        Index,
        Middle,
        Ring,
        Little,
        Count
    };

    public enum FingerBone {
        Metacarpal,
        Proximal,
        Intermediate,
        Distal,
        Count
    };

    public enum FacialBone {
        Unknown,

        // Eyes
        LeftUpperLid,
        LeftEye,
        LeftLowerLid,
        RightUpperLid,
        RightEye,
        RightLowerLid,

        // Brows
        LeftOuterBrow,
        LeftBrow,
        LeftInnerBrow,
        RightInnerBrow,
        RightBrow,
        RightOuterBrow,

        // Ears
        LeftEar,
        RightEar,

        // Cheeks
        LeftCheek,
        RightCheek,

        // Nose
        NoseTop,
        NoseTip,
        NoseBottomLeft,
        NoseBottom,
        NoseBottomRight,

        // Mouth
        UpperLipLeft,
        UpperLip,
        UpperLipRight,
        LipLeft,
        LipRight,
        LowerLipLeft,
        LowerLip,
        LowerLipRight,

        Jaw,
        Chin,

        Count
    }

    [Serializable]
    public class BoneReference {
        public BoneType type;
        public Side side;
        [SerializeField]
        private Bone _boneId;
        [SerializeField]
        private SideBone _sideBoneId;

        public Bone boneId {
            get { return _boneId; }
            set {
                _boneId = value;
                _sideBoneId = HumanoidSideBone(_boneId);
            }
        }
        public CenterBone centerBoneId {
            get { return HumanoidCenterBone(boneId); }
            set { boneId = HumanoidBone(value); }
        }
        public SideBone sideBoneId {
            get { return _sideBoneId; }
            set {
                _sideBoneId = value;
                _boneId = HumanoidBone(side, _sideBoneId);
            }
        }
        public FacialBone faceBoneId {
            get { return HumanoidFaceBone(boneId); }
            set { boneId = HumanoidBone(value); }
        }
        public HumanBodyBones humanBodyBone {
            get { return humanBodyBones[(int)boneId]; }
        }

        public bool isCenterBone {
            get { return (_boneId >= Bone.Hips && _boneId <= Bone.Head); }
        }
        public bool isSideBone {
            get { return (_boneId >= Bone.LeftShoulder && _boneId <= Bone.RightToes); }
        }
        public bool isLeftSideBone {
            get { return (_boneId >= Bone.LeftShoulder && _boneId <= Bone.LeftToes); }
        }
        public bool isRightSideBone {
            get { return (_boneId >= Bone.RightShoulder && _boneId <= Bone.RightToes); }
        }
        public bool isHandBone {
            get {
                bool boneIsHand = (_boneId >= Bone.LeftThumbProximal && _boneId <= Bone.RightLittleDistal);
                bool sideBoneIsHand = (_sideBoneId >= SideBone.ThumbProximal && _sideBoneId <= SideBone.LittleDistal);
                return boneIsHand || sideBoneIsHand;
                    //(_boneId >= Bone.LeftThumbProximal && _boneId <= Bone.RightLittleDistal) ||
                    //(_sideBoneId >= SideBone.ThumbProximal && _sideBoneId <= SideBone.LittleDistal);
            }
        }
        public bool isLeftHandBone {
            get { return (_boneId >= Bone.LeftThumbProximal && _boneId <= Bone.LeftLittleDistal); }
        }
        public bool isRightHandBone {
            get { return (_boneId >= Bone.RightThumbProximal && _boneId <= Bone.RightLittleDistal); }
        }
        public bool isFacialBone {
            get { return (_boneId >= Bone.LeftUpperLid && _boneId <= Bone.Chin); }
        }

        public static Bone HumanoidBone(CenterBone centerBone) {
            return (Bone)centerBone;
        }

        public static CenterBone HumanoidCenterBone(Bone bone) {
            return (CenterBone)bone;
        }

        public static Bone HumanoidBone(Side side, SideBone sideBone) {
            if (sideBone == Tracking.SideBone.None)
                return Bone.None;

            switch (side) {
                case Side.Left:
                    return (int)Bone.LeftShoulder + ((Bone)sideBone - 1);
                case Side.Right:
                    return (int)Bone.RightShoulder + ((Bone)sideBone - 1);
                default:
                    return Bone.None;
            }
        }

        public static SideBone HumanoidSideBone(Bone bone) {
            if (bone >= Bone.LeftShoulder && bone <= Bone.LeftToes) {
                return (SideBone)(int)bone - (int)Bone.LeftShoulder + 1;
            }
            else if (bone >= Bone.RightShoulder && bone <= Bone.RightToes) {
                return (SideBone)(int)bone - (int)Bone.RightShoulder + 1;
            }
            else {
                return Tracking.SideBone.None;
            }
        }
        public static SideBone HumanoidSideBone(Finger fingerId, FingerBone fingerBoneId) {
            SideBone boneId = (SideBone)(((int)Tracking.SideBone.ThumbProximal - 1) + (int)fingerId * 4 + (int)fingerBoneId);
            return boneId;
        }
        public static SideBone HumanoidSideBone(Bone bone, out Side side) {
            if (bone >= Bone.LeftShoulder && bone <= Bone.LeftToes) {
                side = Side.Left;
                return (SideBone)(int)bone - (int)Bone.LeftShoulder + 1;
            }
            else if (bone >= Bone.RightShoulder && bone <= Bone.RightToes) {
                side = Side.Right;
                return (SideBone)(int)bone - (int)Bone.RightShoulder + 1;
            }
            else {
                side = Side.Left; //AnySide;
                return Tracking.SideBone.None;
            }
        }

        public static Bone HumanoidBone(FacialBone faceBone) {
            return (int)Bone.LeftUpperLid + ((Bone)faceBone - 1);
        }
        public static FacialBone HumanoidFaceBone(Bone bone) {
            return (FacialBone)(int)bone - (int)Bone.LeftUpperLid + 1;
        }

        public static HumanBodyBones HumanBodyBone(Bone bone) {
            return humanBodyBones[(int)bone];
        }
        private static HumanBodyBones[] humanBodyBones = new HumanBodyBones[(int)Bone.Count] {
                HumanBodyBones.LastBone,
                HumanBodyBones.Hips,
                HumanBodyBones.Spine,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.Chest,

                HumanBodyBones.Neck,
                HumanBodyBones.Head,

                HumanBodyBones.LeftShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LastBone,
                HumanBodyBones.LeftHand,

                HumanBodyBones.LeftThumbProximal,
                HumanBodyBones.LeftThumbIntermediate,
                HumanBodyBones.LeftThumbDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.LeftIndexProximal,
                HumanBodyBones.LeftIndexIntermediate,
                HumanBodyBones.LeftIndexDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftMiddleIntermediate,
                HumanBodyBones.LeftMiddleDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.LeftRingProximal,
                HumanBodyBones.LeftRingIntermediate,
                HumanBodyBones.LeftRingDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.LeftLittleProximal,
                HumanBodyBones.LeftLittleIntermediate,
                HumanBodyBones.LeftLittleDistal,

                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.LeftToes,

                HumanBodyBones.RightShoulder,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.LastBone,
                HumanBodyBones.RightHand,

                HumanBodyBones.RightThumbProximal,
                HumanBodyBones.RightThumbIntermediate,
                HumanBodyBones.RightThumbDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.RightIndexProximal,
                HumanBodyBones.RightIndexIntermediate,
                HumanBodyBones.RightIndexDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightMiddleIntermediate,
                HumanBodyBones.RightMiddleDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.RightRingProximal,
                HumanBodyBones.RightRingIntermediate,
                HumanBodyBones.RightRingDistal,

                HumanBodyBones.LastBone,
                HumanBodyBones.RightLittleProximal,
                HumanBodyBones.RightLittleIntermediate,
                HumanBodyBones.RightLittleDistal,

                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot,
                HumanBodyBones.RightToes,

                HumanBodyBones.LastBone,
                HumanBodyBones.LeftEye,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.RightEye,
                HumanBodyBones.LastBone,

                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,

                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,

                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,

                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,

                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,
                HumanBodyBones.LastBone,

                HumanBodyBones.Jaw,

                HumanBodyBones.LastBone,
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2 {
        public float x;
        public float y;

        public Vec2(Vector2 v) {
            x = v.x;
            y = v.y;
        }
        public Vector2 Vector2 {
            get { return new Vector2(x, y); }
        }
    }

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

    public class TrackerTransform {
        private TrackingDevice tracker;

        protected float lastTime = 0;

        public TrackerTransform(TrackingDevice tracker) {
            this.tracker = tracker;
        }

        private void Update() {
            if (Time.frameCount <= lastTime)
                return;

            TrackingDevice.TrackerTransformC trackerTransform = tracker.GetTrackerData();

            _status = trackerTransform.status;
            _actorCount = trackerTransform.actorCount;

            lastTime = Time.frameCount;
        }

        private Status _status;
        public Status status {
            get {
                Update();
                return _status;
            }
        }

        private int _actorCount;
        public int actorCount {
            get {
                Update();
                return _actorCount;
            }
        }

    }


#if hUNSAFE
    unsafe public class SensorBone {
        private TrackingDevice.SensorTransformC* pSensorTransform = null;
        protected float lastTime = 0;

        unsafe public SensorBone(TrackingDevice.SensorTransformC* pSensorTransform) {
            this.pSensorTransform = pSensorTransform;
        }

        private void Update() {
            if (pSensorTransform == null)
                return;
            if (Time.frameCount <= lastTime)
                return;

            _position = pSensorTransform->position.Vector3;
            _positionConfidence = pSensorTransform->positionConfidence;
            _rotation = pSensorTransform->rotation.Quaternion;
            _rotationConfidence = pSensorTransform->rotationConfidence;

            lastTime = Time.frameCount;
        }
#else
    public class SensorBone {
        protected TrackingDevice tracker;
        protected readonly uint actorId;
        protected readonly Bone boneId;
        protected readonly Side side;
        protected readonly SideBone sideBoneId;

        protected float lastTime = 0;

        public SensorBone(TrackingDevice tracker, uint actorId, Side side, SideBone sideBoneId) {
            this.tracker = tracker;
            this.actorId = actorId;
            this.boneId = Bone.None;
            this.side = side;
            this.sideBoneId = sideBoneId;
        }

        public SensorBone(TrackingDevice tracker, uint actorId, Bone boneId) {
            this.tracker = tracker;
            this.actorId = actorId;
            this.side = Side.AnySide;
            this.boneId = boneId;
            this.sideBoneId = SideBone.None;
        }

        protected virtual void Update() {
            if (Time.frameCount <= lastTime)
                return;

            TrackingDevice.SensorTransformC sensorTransform =
                (boneId == Bone.None) ?
                tracker.GetBoneData(actorId, side, sideBoneId) : 
                tracker.GetBoneData(actorId, boneId);


            _position = sensorTransform.position.Vector3;
            _positionConfidence = sensorTransform.positionConfidence;
            _rotation = sensorTransform.rotation.Quaternion;
            _rotationConfidence = sensorTransform.rotationConfidence;

            lastTime = Time.frameCount;
        }
#endif
        protected Vector3 _position;
        public virtual Vector3 position {
            get {
                Update();
                return _position;
            }
        }

        protected float _positionConfidence;
        public virtual float positionConfidence {
            get {
                Update();
                return _positionConfidence;
            }
        }

        protected Quaternion _rotation;
        public virtual Quaternion rotation {
            get {
                Update();
                return _rotation;
            }
        }

        protected float _rotationConfidence;
        public virtual float rotationConfidence {
            get {
                Update();
                return _rotationConfidence;
            }
        }

        private float _lengthConfidence;

        public float length = 0;

        public Vector3 velocity;
        public Quaternion rotationalVelocity;
    }

    public class ControllerState {
        private TrackingDevice tracker;
        private readonly uint actorId;
        private readonly Side side;

        //private float lastTime = 0;

        public ControllerState(TrackingDevice tracker, uint actorId, Side side) {
            this.tracker = tracker;
            this.actorId = actorId;
            this.side = side;
        }

        public void Update() {
            TrackingDevice.ControllerStateC controllerState =
                tracker.GetControllerState(actorId, side);

            //if (lastTime >= targetTransform.timestamp)
            //    return;            

            for (int i = 0; i < input3dCount; i++)
                input3d[i] = controllerState.input3d.Vector3; //[i].Vector3;
            //for (int i = 0; i < input1dCount; i++)
            //    input1d[i] = controllerState.input1d[i];

            //lastTime = controllerState.timestamp;

        }

        public const int input3dCount = 1; //2;
        public const int input1dCount = 7;

        public Vector3[] input3d = new Vector3[input3dCount];
        public float[] input1d = new float[input1dCount];

    }
    public class TrackingDevice {
        [StructLayout(LayoutKind.Sequential)]
        public struct TrackerTransformC {
            public float timestamp;

            public Vec3 position;
            public Quat rotation;

            public Status status;

            public int actorCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SensorTransformC {
            public float timestamp;
            public uint id;

            public Vec3 position;
            public float positionConfidence;
            public Vec3 velocity;

            public Quat rotation;
            public float rotationConfidence;
            public Quat rotationalVelocity;

            public float length;
            public float lengthConfidence;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ControllerStateC {
            public float timestamp;

            public Vec3 input3d;
            //public float[] input1d;
        }

        protected IntPtr device;

        public virtual void Init() { }
        public virtual void Stop() { }

        public virtual void Update() { }

        #region Tracker
        public virtual Status status {
            get { return Status.Unavailable; }
        }
        public virtual Vector3 position {
            set { }
        }
        public virtual Quaternion rotation {
            set { }
        }
        public virtual TrackerTransform GetTracker() {
            return new TrackerTransform(this);
        }
        public virtual TrackerTransformC GetTrackerData() {
            return new TrackerTransformC();
        }
        #endregion

        #region Bone
        public virtual Vector3 GetBonePosition(uint actorId, Bone boneId) {
            return Vector3.zero;
        }
        public virtual Quaternion GetBoneRotation(uint actorId, Bone boneId) {
            return Quaternion.identity;
        }
        public virtual float GetBoneConfidence(uint actorId, Bone boneId) {
            return 0;
        }

#if hUNSAFE
        unsafe public virtual SensorBone GetBone(uint actorId, Bone boneId) {
            return new SensorBone(null);
        }
#else
        public virtual SensorBone GetBone(uint actorId, Bone boneId) {
            return new SensorBone(this, actorId, boneId);
        }
#endif
        public virtual SensorTransformC GetBoneData(uint actorId, Bone boneId) {
            return new SensorTransformC();
        }

#if hUNSAFE
        unsafe public virtual SensorBone GetBone(uint actorId, Side side, SideBone boneId) {
            return new SensorBone(null);
        }
#else
        public virtual SensorBone GetBone(uint actorId, Side side, SideBone boneId) {
            return new SensorBone(this, actorId, side, boneId);
        }
#endif
        public virtual SensorTransformC GetBoneData(uint actorId, Side side, SideBone boneId) {
            return new SensorTransformC();
        }
        public virtual Vector3 GetBonePosition(uint actorId, Side side, SideBone boneId) {
            return Vector3.zero;
        }
        public virtual Quaternion GetBoneRotation(uint actorId, Side side, SideBone boneId) {
            return Quaternion.identity;
        }
        public virtual float GetBoneConfidence(uint actorId, Side side, SideBone boneId) {
            return 0;
        }

        public Quaternion GetBoneRotation(uint actorId, Side side, Finger fingerId, FingerBone fingerboneId) {
            SideBone sideBoneId = BoneReference.HumanoidSideBone(fingerId, fingerboneId);
            Quaternion q = GetBoneRotation(actorId, side, sideBoneId);
            return q;
        }
        #endregion

        #region Controllers
        public virtual ControllerState GetController(uint actorId, Side side) {
            return new ControllerState(this, actorId, side);
        }

        public virtual ControllerStateC GetControllerState(uint actorId, Side side) {
            return new ControllerStateC();
        }
        #endregion
    }

}