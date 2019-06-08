namespace Passer.Humanoid.Tracking {
    public class TorsoSensor : Sensor {
        public TargetData chest;
        public TargetData spine;
        public TargetData hips;

        public TorsoSensor(DeviceView deviceView) : base(deviceView) {
            chest = new TargetData();
            spine = new TargetData();
            hips = new TargetData();
        }
    }
}
