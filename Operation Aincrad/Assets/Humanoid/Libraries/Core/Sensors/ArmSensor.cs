using System;

namespace Passer.Humanoid.Tracking {
    public enum FingerBones {
        Proximal = 0,
        Intermediate = 1,
        Distal = 2,
        LastBone = 3
    }

    public class ArmSensor : Sensor {
        public bool isLeft;

        public TargetData shoulder;
        public TargetData upperArm;
        public TargetData forearm;
        public TargetData hand;

        public class Finger {
            public float curl;
            public TargetData proximal = new TargetData();
            public TargetData intermediate = new TargetData();
            public TargetData distal = new TargetData();
        }

        public Finger thumb;
        public Finger indexFinger;
        public Finger middleFinger;
        public Finger ringFinger;
        public Finger littleFinger;
        public Finger[] fingers;

        public ArmSensor(bool isLeft, DeviceView deviceView) : base(deviceView) {
            this.isLeft = isLeft;

            shoulder = new TargetData();
            upperArm = new TargetData();
            forearm = new TargetData();
            hand = new TargetData();

            thumb = new Finger();
            indexFinger = new Finger();
            middleFinger = new Finger();
            ringFinger = new Finger();
            littleFinger = new Finger();

            fingers = new Finger[] {
                thumb,
                indexFinger,
                middleFinger,
                ringFinger,
                littleFinger
            };
        }

        protected virtual void UpdateHand() {
            hand.rotation = _sensorRotation * _sensor2TargetRotation;
            hand.confidence.rotation = _rotationConfidence;

            hand.position = _sensorPosition + hand.rotation * _sensor2TargetPosition;
            hand.confidence.position = _positionConfidence;
        }

        private const float Rad2Deg = 57.29578F;
        public static Rotation CalculateUpperArmOrientation(Vector upperArmPosition, float upperArmLength, Vector forearmUp, float forearmLength, Vector handPosition, bool isLeft) {
            Rotation upperArmRotation = Rotation.LookRotation(handPosition - upperArmPosition, forearmUp);

            float upperArm2HandDistance = Vector.Distance(upperArmPosition, handPosition);
            float upperArm2HandDistance2 = upperArm2HandDistance * upperArm2HandDistance;
            float upperArmLength2 = upperArmLength * upperArmLength;
            float forearmLength2 = forearmLength * forearmLength;
            float elbowAngle = (float)Math.Acos((upperArm2HandDistance2 + upperArmLength2 - forearmLength2) / (2 * upperArm2HandDistance * upperArmLength)) * Rad2Deg;
            if (float.IsNaN(elbowAngle)) elbowAngle = 10;
            if (isLeft)
                elbowAngle = -elbowAngle;

            upperArmRotation = Rotation.AngleAxis(elbowAngle, upperArmRotation * Vector.up) * upperArmRotation;

            if (isLeft)
                upperArmRotation *= Rotation.AngleAxis(90, Vector.up); // Euler(0, 90, 0)
            else
                upperArmRotation *= Rotation.AngleAxis(-90, Vector.up); // Euler(0, -90, 0)

            return upperArmRotation;
        }
        public static Rotation CalculateArmOrientation(Vector joint1Position, Vector joint1Up, Vector joint2Position, bool isLeft) {
            Vector boneForward = joint2Position - joint1Position;
            Rotation boneRotation = Rotation.LookRotation(boneForward, joint1Up);

            if (isLeft)
                boneRotation *= Rotation.AngleAxis(90, Vector.up); // Euler(0, 90, 0)
            else
                boneRotation *= Rotation.AngleAxis(-90, Vector.up); // Euler(0, -90, 0)

            return boneRotation;
        }
    }
}
