using UnityEngine; // This should disappear!

namespace Passer.Humanoid.Tracking {
    public class Rotation_ {
        public static Rotation Euler(Vector v) {
            return Euler(v.x, v.y, v.z);
        }

        public static Rotation Euler(float x, float y, float z) {
            // temporary...
            Quaternion q = Quaternion.Euler(x, y, z);
            return new Rotation(q.x, q.y, q.z, q.w);
        }

        public static Rotation FromToRotation(Vector v1, Vector v2) {
            Vector3 uv1 = Passer.Target.ToVector3(v1);
            Vector3 uv2 = Passer.Target.ToVector3(v2);
            Quaternion r = Quaternion.FromToRotation(uv1, uv2);
            return Passer.Target.ToRotation(r);
        }
    }

    /// <summary>
    /// Humanoid Tracking sensor
    /// </summary>
    public class Sensor {
        /// <summary>
        /// The device to which the sensor belongs
        /// </summary>
        public DeviceView device;

        /// <summary>
        /// Status of the sensor
        /// </summary>
        public Status status = Status.Unavailable;

        public struct State {
            public int sensorID;
            public Vector position;
            public Rotation rotation;
            public float confidence;
            public bool present;
        }

        public enum ID {
            Head,
            LeftHand,
            RightHand,
            Hips,
            LeftFoot,
            RightFoot,

            Tracker1,
            Tracker2,
            Tracker3,
            Tracker4,
            Count
        }

        /// <summary>
        /// Create new sensor the the device
        /// </summary>
        /// <param name="_device"></param>
        public Sensor(DeviceView _device) {
            device = _device;
        }

        /// <summary>
        ///  Update the sensor state
        /// </summary>
        /// <returns>Status of the sensor after the update</returns>
        public virtual Status Update() {
            return Status.Unavailable;
        }

        public static Rotation CalculateBoneRotation(Vector bonePosition, Vector parentBonePosition, Vector upDirection) {
            Vector direction = bonePosition - parentBonePosition;
            if (Vector.Magnitude(direction) > 0) {
                return Rotation.LookRotation(direction, upDirection);
            }
            else
                return Rotation.identity;
        }

        // The tracker's position and rotation
        protected Vector _localSensorPosition;
        public Vector localSensorPosition {
            get { return _localSensorPosition; }
        }
        protected Rotation _localSensorRotation;
        public Rotation localSensorRotation {
            get { return _localSensorRotation; }
        }

        protected Vector _sensorPosition;
        public Vector sensorPosition {
            get { return _sensorPosition; }
        }
        protected Rotation _sensorRotation;
        public Rotation sensorRotation {
            get { return _sensorRotation; }
        }

        protected void UpdateSensor() {
            _sensorRotation = device.ToWorldOrientation(_localSensorRotation);
            _sensorPosition = device.ToWorldPosition(_localSensorPosition);
        }

        /// <summary>
        /// Tracking confidence
        /// </summary>
        protected float _positionConfidence;
        public float positionConfidence {
            get { return _positionConfidence; }
        }
        protected float _rotationConfidence;
        public float rotationConfidence {
            get { return _rotationConfidence; }
        }

        /// <summary>
        /// The position of the tracker relative to the origin of the object it is tracking
        /// </summary>
        protected Vector _sensor2TargetPosition = Vector.zero;
        public Vector sensor2TargetPosition {
            set { _sensor2TargetPosition = value; }
            get { return _sensor2TargetPosition; }
        }
        protected Rotation _sensor2TargetRotation = Rotation.identity;
        public Rotation sensor2TargetRotation {
            set { _sensor2TargetRotation = value; }
            get { return _sensor2TargetRotation; }
        }
    }

    public class Controller : Sensor {
        public Controller(DeviceView _device) : base(_device) { }
    }




    // Most of these should move to difference files...

    public enum Status {
        Unavailable,
        Present,
        Tracking
    }

    public class TargetData {
        public Vector position = Vector.zero;
        public Rotation rotation = Rotation.identity;
        public float length;
        public Confidence confidence;

        public Vector startPosition = Vector.zero;
    }

    public struct Confidence {
        public float position { get; set; }
        public float rotation { get; set; }
        public float length { get; set; }

        private const float degradationPerFrame = -0.01F; // PER FRAME!!!!
        public void Degrade() {
            position = Mathf.Clamp01(position + degradationPerFrame);
            rotation = Mathf.Clamp01(rotation + degradationPerFrame);
            length = Mathf.Clamp01(length + degradationPerFrame);
        }

        public static Confidence none {
            get {
                return new Confidence { position = 0, rotation = 0, length = 0 };
            }
        }
    }

    public class DeviceView {
        public Vector position;
        public Rotation orientation;

        public virtual Vector ToWorldPosition(Vector localPosition) {
            return position + orientation * localPosition;
        }

        public virtual Rotation ToWorldOrientation(Rotation localRotation) {
            return orientation * localRotation;
        }
    }

}
