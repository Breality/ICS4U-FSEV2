using System.Runtime.InteropServices;

namespace Passer.Humanoid.Tracking {

    internal static class OculusDevice {
#if UNITY_ANDROID
        public const string name = "Gear VR";
#else
        public const string name = "Oculus";
#endif

        public static Status status;

        private static Sensor.State[] sensorStates;

        public static void Start() {
            status = Status.Unavailable;

            sensorStates = new Sensor.State[(int)Sensor.ID.Count];
            sensorStates[(int)Sensor.ID.Head].sensorID = (int)Node.Head;
            sensorStates[(int)Sensor.ID.LeftHand].sensorID = (int)Node.HandLeft;
            sensorStates[(int)Sensor.ID.RightHand].sensorID = (int)Node.HandRight;
            sensorStates[(int)Sensor.ID.Hips].sensorID = -1;
            sensorStates[(int)Sensor.ID.LeftFoot].sensorID = -1;
            sensorStates[(int)Sensor.ID.RightFoot].sensorID = -1;
            sensorStates[(int)Sensor.ID.Tracker1].sensorID = (int)Node.TrackerZero;
            sensorStates[(int)Sensor.ID.Tracker2].sensorID = (int)Node.TrackerOne;
            sensorStates[(int)Sensor.ID.Tracker3].sensorID = (int)Node.TrackerTwo;
            sensorStates[(int)Sensor.ID.Tracker4].sensorID = (int)Node.TrackerThree;

            // Initial filling of values
            Update();
        }

        public static void Update() {
            if (sensorStates == null)
                return;

            for (int i = 0; i < sensorStates.Length; i++) {
                if (sensorStates[i].sensorID < 0)
                    continue;

                sensorStates[i].present = (ovrp_GetNodePresent(sensorStates[i].sensorID) == Bool.True);
                sensorStates[i].confidence = (ovrp_GetNodeOrientationTracked(sensorStates[i].sensorID) == Bool.True) ? 1 : 0;

                if (sensorStates[i].confidence > 0)
                    status = Status.Tracking;

                Pose pose = ovrp_GetNodePoseState(Step.Render, sensorStates[i].sensorID).Pose;
#if UNITY_ANDROID
                if (sensorStates[i].sensorID != (int)Node.HandLeft && sensorStates[i].sensorID != (int)Node.HandRight)
                    // no positional tracking for GearVR Controllers
#endif
                sensorStates[i].position = new Vector(pose.Position.x, pose.Position.y, -pose.Position.z);
                sensorStates[i].rotation = new Rotation(-pose.Orientation.x, -pose.Orientation.y, pose.Orientation.z, pose.Orientation.w);
            }
        }

        public static Pose GetPose(int sensorID) {
            return ovrp_GetNodePoseState(Step.Render, sensorID).Pose;
        }

        public static Vector GetPosition(Sensor.ID sensorID) {
            if (sensorStates == null)
                return Vector.zero;

            return sensorStates[(int)sensorID].position;
        }

        public static Rotation GetRotation(Sensor.ID sensorID) {
            if (sensorStates == null)
                return Rotation.identity;

            return sensorStates[(int)sensorID].rotation;
        }

        public static float GetPositionalConfidence(Sensor.ID sensorID) {
            if (sensorStates == null)
                return 0;

            return (ovrp_GetNodePositionTracked(sensorStates[(int)sensorID].sensorID) == Bool.True) ? 0.99F : 0;
        }

        public static float GetRotationalConfidence(Sensor.ID sensorID) {
            if (sensorStates == null)
                return 0;

            if (ovrp_GetNodePositionTracked(sensorStates[(int)sensorID].sensorID) == Bool.True)
                return sensorStates[(int)sensorID].confidence;
            else
                return sensorStates[(int)sensorID].confidence * 0.9F; // without positional tracking, there is no drift correction
        }

        public static bool IsPresent(Sensor.ID sensorID) {
            if (sensorStates == null)
                return false;

            return sensorStates[(int)sensorID].present;
        }

        public static float GetConfidence(int sensorID) {
            return (ovrp_GetNodeOrientationTracked(sensorID) == Bool.True) ? 1 : 0;
        }

        public static void GetControllerInput(Sensor.ID sensorID, ref ControllerButtons input) {
            Controller controllerMask;
            switch (sensorID) {
                case Sensor.ID.LeftHand:
#if UNITY_ANDROID
                    controllerMask = Controller.LTrackedRemote;
#else
                    controllerMask = Controller.LTouch;
#endif
                    break;
                case Sensor.ID.RightHand:
#if UNITY_ANDROID
                    controllerMask = Controller.RTrackedRemote;
#else
                    controllerMask = Controller.RTouch;
#endif
                    break;
                default:
                    return;
            }
            bool isLeft = (sensorID == Sensor.ID.LeftHand);


            ControllerState2 controllerState = ovrp_GetControllerState2((uint)controllerMask);
            input.stickHorizontal = GetHorizontal(controllerState, isLeft);
            input.stickVertical = GetVertical(controllerState, isLeft);
            input.stickPress = GetStickPress(controllerState);
            input.stickTouch = GetStickTouch(controllerState);

            input.buttons[0] = GetButton1Press(controllerState);
            input.buttons[1] = GetButton2Press(controllerState);

            input.trigger1 = GetTrigger1(controllerState, isLeft);
            input.trigger2 = GetTrigger2(controllerState, isLeft);

            input.up = (input.stickVertical > 0.3F);
            input.down = (input.stickVertical < -0.3F);
            input.left = (input.stickHorizontal < -0.3F);
            input.right = (input.stickHorizontal > 0.3F);
        }

        public static float GetHorizontal(ControllerState2 controllerState, bool isLeft) {
            float stickHorizontalValue = isLeft ? controllerState.LThumbstick.x : controllerState.RThumbstick.x;
            return stickHorizontalValue;
        }

        public static float GetVertical(ControllerState2 controllerState, bool isLeft) {
            float stickVerticalValue = isLeft ? controllerState.LThumbstick.y : controllerState.RThumbstick.y;
            return stickVerticalValue;
        }

        public static bool GetStickPress(ControllerState2 controllerState) {
            RawButton stickButton = RawButton.LThumbstick | RawButton.RThumbstick;
            bool stickButtonValue = (controllerState.Buttons & (uint)stickButton) != 0;
            return stickButtonValue;
        }

        public static bool GetStickTouch(ControllerState2 controllerState) {
            RawTouch stickTouch = RawTouch.LThumbstick | RawTouch.RThumbstick;
            bool stickTouchValue = (controllerState.Touches & (uint)stickTouch) != 0;
            return stickTouchValue;
        }

        public static bool GetButton1Press(ControllerState2 controllerState) {
            uint button = (uint)RawButton.X | (uint)RawButton.A;
            bool buttonValue = (controllerState.Buttons & button) != 0;
            return buttonValue;
        }

        public static bool GetButton1Touch(ControllerState2 controllerState) {
            uint button = (uint)RawTouch.X | (uint)RawTouch.A;
            bool buttonTouchValue = (controllerState.Touches & button) != 0;
            return buttonTouchValue;
        }

        public static bool GetButton2Press(ControllerState2 controllerState) {
            uint button = (uint)RawButton.Y | (uint)RawButton.B;
            bool buttonValue = (controllerState.Buttons & button) != 0;
            return buttonValue;
        }

        public static bool GetButton2Touch(ControllerState2 controllerState) {
            uint button = (uint)RawTouch.Y | (uint)RawTouch.B;
            bool buttonTouchValue = (controllerState.Touches & button) != 0;
            return buttonTouchValue;
        }

        // always give true... Maybe because I'm using an engineering sample?
        public static bool GetButton2Near(ControllerState2 controllerState) {
            uint mask = (uint)RawNearTouch.LThumbButtons | (uint)RawNearTouch.RThumbButtons;
            bool isNear = (controllerState.NearTouches & mask) != 0;
            return isNear;
        }


        public static float GetTrigger1(ControllerState2 controllerState, bool isLeft) {
            float trigger1Value = isLeft ? controllerState.LIndexTrigger : controllerState.RIndexTrigger;

            // always give true... Maybe because I'm using an engineering sample?
            //uint nearId = (uint)RawNearTouch.LIndexTrigger | (uint)RawNearTouch.RIndexTrigger;
            //bool trigger1Near = (controllerState.NearTouches & nearId) != 0;

            uint touchId = (uint)RawTouch.LIndexTrigger | (uint)RawTouch.RIndexTrigger;
            bool trigger1Touch = (controllerState.Touches & touchId) != 0;
            if (!trigger1Touch)
                trigger1Value = -1F;

            return trigger1Value;
        }

        public static float GetTrigger2(ControllerState2 controllerState, bool isLeft) {
            float trigger2Value = isLeft ? controllerState.LHandTrigger : controllerState.RHandTrigger;
            return trigger2Value;
        }

        public static bool GetThumbRest(ControllerState2 controllerState) {
            RawTouch touchMask = RawTouch.LThumbRest | RawTouch.RThumbRest;
            bool touch = (controllerState.Touches & (uint)touchMask) != 0;
            return touch;
        }

        public enum RawNearTouch {
            None = 0,          ///< Maps to Physical NearTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x00000001, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies finger is in close proximity to LIndexTrigger.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbButtons = 0x00000002, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies thumb is in close proximity to LThumbstick OR X/Y buttons.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RIndexTrigger = 0x00000004, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies finger is in close proximity to RIndexTrigger.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbButtons = 0x00000008, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies thumb is in close proximity to RThumbstick OR A/B buttons.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Any = ~None,      ///< Maps to Physical NearTouch: [Touch, LTouch, RTouch: Any], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
        }

        public enum RawTouch {
            None = 0,                            ///< Maps to Physical Touch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            A = RawButton.A,                  ///< Maps to Physical Touch: [Touch, RTouch: A], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            B = RawButton.B,                  ///< Maps to Physical Touch: [Touch, RTouch: B], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            X = RawButton.X,                  ///< Maps to Physical Touch: [Touch, LTouch: X], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Y = RawButton.Y,                  ///< Maps to Physical Touch: [Touch, LTouch: Y], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x00001000,                   ///< Maps to Physical Touch: [Touch, LTouch: LIndexTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstick = RawButton.LThumbstick,        ///< Maps to Physical Touch: [Touch, LTouch: LThumbstick], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbRest = 0x00000800,                   ///< Maps to Physical Touch: [Touch, LTouch: LThumbRest], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LTouchpad = RawButton.LTouchpad,          ///< Maps to Physical Touch: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
            RIndexTrigger = 0x00000010,                   ///< Maps to Physical Touch: [Touch, RTouch: RIndexTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstick = RawButton.RThumbstick,        ///< Maps to Physical Touch: [Touch, RTouch: RThumbstick], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbRest = 0x00000008,                   ///< Maps to Physical Touch: [Touch, RTouch: RThumbRest], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RTouchpad = RawButton.RTouchpad,          ///< Maps to Physical Touch: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
            Any = ~None,                        ///< Maps to Physical Touch: [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Gamepad, Remote: None]
        }

        public enum RawButton {
            None = 0,          ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            A = 0x00000001, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: A], [LTrackedRemote: LIndexTrigger], [RTrackedRemote: RIndexTrigger], [LTouch, Touchpad, Remote: None]
            B = 0x00000002, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: B], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            X = 0x00000100, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: X], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Y = 0x00000200, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: Y], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Start = 0x00100000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Start], [RTouch: None]
            Back = 0x00200000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back], [Touch, LTouch, RTouch: None]
            LShoulder = 0x00000800, ///< Maps to Physical Button: [Gamepad: LShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x10000000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote: LIndexTrigger], [RTouch, RTrackedRemote, Touchpad, Remote: None]
            LHandTrigger = 0x20000000, ///< Maps to Physical Button: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstick = 0x00000400, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickUp = 0x00000010, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickDown = 0x00000020, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickLeft = 0x00000040, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickRight = 0x00000080, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LTouchpad = 0x40000000, ///< Maps to Physical Button: [LTrackedRemote: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Touchpad, Remote: None]
            RShoulder = 0x00000008, ///< Maps to Physical Button: [Gamepad: RShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RIndexTrigger = 0x04000000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch, RTrackedRemote: RIndexTrigger], [LTouch, LTrackedRemote, Touchpad, Remote: None]
            RHandTrigger = 0x08000000, ///< Maps to Physical Button: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstick = 0x00000004, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickUp = 0x00001000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickUp], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickDown = 0x00002000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickDown], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickLeft = 0x00004000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickLeft], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickRight = 0x00008000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickRight], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RTouchpad = unchecked((int)0x80000000),///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            DpadUp = 0x00010000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp], [Touch, LTouch, RTouch: None]
            DpadDown = 0x00020000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown], [Touch, LTouch, RTouch: None]
            DpadLeft = 0x00040000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft], [Touch, LTouch, RTouch: None]
            DpadRight = 0x00080000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight], [Touch, LTouch, RTouch: None]
            Any = ~None,      ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Any]
        }

        public static void Vibrate(Sensor.ID sensorID, float length, float strength) {
            Controller controllerMask;
            switch (sensorID) {
                case Sensor.ID.LeftHand:
                    controllerMask = Controller.LTouch;
                    break;
                case Sensor.ID.RightHand:
                    controllerMask = Controller.RTouch;
                    break;
                default:
                    return;
            }
            ovrp_SetControllerVibration((uint)controllerMask, 0.5F, strength);
        }

        public static float eyeHeight {
            get { return ovrp_GetUserEyeHeight(); }
        }

        public enum Bool {
            False = 0,
            True
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2f {
            public float x;
            public float y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector3f {
            public float x;
            public float y;
            public float z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Quatf {
            public float x;
            public float y;
            public float z;
            public float w;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Pose {
            public Quatf Orientation;
            public Vector3f Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PoseStatef {
            public Pose Pose;
            public Vector3f Velocity;
            public Vector3f Acceleration;
            public Vector3f AngularVelocity;
            public Vector3f AngularAcceleration;
            double Time;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ControllerState2 {
            public uint ConnectedControllers;
            public uint Buttons;
            public uint Touches;
            public uint NearTouches;
            public float LIndexTrigger;
            public float RIndexTrigger;
            public float LHandTrigger;
            public float RHandTrigger;
            public Vector2f LThumbstick;
            public Vector2f RThumbstick;
            public Vector2f LTouchpad;
            public Vector2f RTouchpad;

            public ControllerState2(ControllerState cs) {
                ConnectedControllers = cs.ConnectedControllers;
                Buttons = cs.Buttons;
                Touches = cs.Touches;
                NearTouches = cs.NearTouches;
                LIndexTrigger = cs.LIndexTrigger;
                RIndexTrigger = cs.RIndexTrigger;
                LHandTrigger = cs.LHandTrigger;
                RHandTrigger = cs.RHandTrigger;
                LThumbstick = cs.LThumbstick;
                RThumbstick = cs.RThumbstick;
                LTouchpad = new Vector2f() { x = 0.0f, y = 0.0f };
                RTouchpad = new Vector2f() { x = 0.0f, y = 0.0f };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ControllerState {
            public uint ConnectedControllers;
            public uint Buttons;
            public uint Touches;
            public uint NearTouches;
            public float LIndexTrigger;
            public float RIndexTrigger;
            public float LHandTrigger;
            public float RHandTrigger;
            public Vector2f LThumbstick;
            public Vector2f RThumbstick;
        }

        public enum Tracker {
            None = -1,
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Count,
        }

        public enum Node {
            None = -1,
            EyeLeft = 0,
            EyeRight = 1,
            EyeCenter = 2,
            HandLeft = 3,
            HandRight = 4,
            TrackerZero = 5,
            TrackerOne = 6,
            TrackerTwo = 7,
            TrackerThree = 8,
            Head = 9,
            Count,
        }

        public enum Controller {
            None = 0,
            LTouch = 0x00000001,
            RTouch = 0x00000002,
            Touch = LTouch | RTouch,
            Remote = 0x00000004,
            Gamepad = 0x00000010,
            Touchpad = 0x08000000,
            LTrackedRemote = 0x01000000,
            RTrackedRemote = 0x02000000,
            Active = unchecked((int)0x80000000),
            All = ~None,
        }

        public enum Step {
            Render = -1,
            Physics = 0,
        }

        private const string pluginName = "OVRPlugin";

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodeOrientationTracked(int nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodePositionTracked(int nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern PoseStatef ovrp_GetNodePoseState(Step stepId, int nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ControllerState2 ovrp_GetControllerState2(uint controllerMask);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodePresent(int nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ovrp_GetUserEyeHeight();
    }
}