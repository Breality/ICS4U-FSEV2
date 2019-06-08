namespace Passer.Humanoid.Tracking {
    /// <summary>
    /// Humanoid head tracking sensor
    /// </summary>
    public class HeadSensor : Sensor {
        public TargetData neck;
        public TargetData head;

        //public TrackedBrow leftBrow;
        //public TrackedBrow rightBrow;

        //public TrackedEye leftEye;
        //public TrackedEye rightEye;

        //public TrackedMouth mouth;

        //public TargetData jaw;

        //public float smile;

        public HeadSensor(DeviceView deviceView) : base(deviceView) {
            neck = new TargetData();
            head = new TargetData();

            //    leftBrow = new TrackedBrow();
            //    rightBrow = new TrackedBrow();

            //    leftEye = new TrackedEye();
            //    rightEye = new TrackedEye();

            //    mouth = new TrackedMouth();

            //    jaw = new TargetData();
        }

        //public class TrackedBrow {
        //    public TargetData inner;
        //    public TargetData center;
        //    public TargetData outer;
        //}

        //public class TrackedEye {
        //    public float closed;
        //}

        //public class TrackedMouth {
        //    public TargetData upperLipLeft;
        //    public TargetData upperLip;
        //    public TargetData upperLipRight;

        //    public TargetData lipLeft;
        //    public TargetData lipRight;

        //    public TargetData lowerLipLeft;
        //    public TargetData lowerLip;
        //    public TargetData lowerLipRight;

        //    //public TrackedMouth() {
        //    //    upperLipLeft = new TargetData();
        //    //    upperLip = new TargetData();
        //    //    upperLipRight = new TargetData();

        //    //    lipLeft = new TargetData();
        //    //    lipRight = new TargetData();

        //    //    lowerLipLeft = new TargetData();
        //    //    lowerLip = new TargetData();
        //    //    lowerLipRight = new TargetData();
        //    //}
        //}
    }
}
