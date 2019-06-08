namespace Passer.Humanoid.Tracking {

    public class LegSensor : Sensor {
        public bool isLeft;

        public TargetData upperLeg;
        public TargetData lowerLeg;
        public TargetData foot;

        public LegSensor(bool isLeft, DeviceView deviceView) : base(deviceView) {
            this.isLeft = isLeft;

            upperLeg = new TargetData();
            lowerLeg = new TargetData();
            foot = new TargetData();
        }

        public static Rotation CalculateLegOrientation(Vector joint1Position, Vector joint2Position, Rotation hipsRotation) {
            Vector boneUp = hipsRotation * Vector.back;
            Vector boneForward = Rotation.AngleAxis(180, Vector.up) * (joint2Position - joint1Position);
            Rotation boneRotation = Rotation.LookRotation(boneForward, boneUp);
            boneRotation *= Rotation.AngleAxis(270, Vector.right);
            return boneRotation;
        }
    }

}
