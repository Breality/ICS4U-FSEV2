namespace Passer.Humanoid.Tracking {

    public class ControllerButtons {
        public float stickHorizontal;
        public float stickVertical;
        public bool stickPress;
        public bool stickTouch;
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        public bool[] buttons = new bool[4];

        public float trigger1;
        public float trigger2;

        public bool option;
    }

    public class ArmController : ArmSensor {


        public ControllerButtons input;

        public ArmController(bool isLeft, DeviceView deviceView) : base(isLeft, deviceView) {
            input = new ControllerButtons();
        }

        public virtual void UpdateInput() { }

        public virtual void Vibrate(float length, float strength) { }
    }

}
