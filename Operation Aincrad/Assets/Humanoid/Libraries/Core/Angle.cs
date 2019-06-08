using UnityEngine;

namespace Passer.Humanoid.Tracking {
    public class Angle {
        // Normalize an angle to the range -180 < angle <= 180
        public static float Normalize(float angle) {
            while (angle <= -180) angle += 360;
            while (angle > 180) angle -= 360;
            return angle;
        }

        // clamp the angle between the given min and max values
        // Angles are normalized
        public static float Clamp(float angle, float min, float max) {
            float normalizedAngle = Normalize(angle);
            return Mathf.Clamp(normalizedAngle, min, max);
        }
    }

    public struct Angles {
        // Normalize all vector angles to the range -180 < angle < 180
        public static Vector Normalize(Vector angles) {
            float x = Angle.Normalize(angles.x);
            float y = Angle.Normalize(angles.y);
            float z = Angle.Normalize(angles.z);
            return new Vector(x, y, z);
        }

        // Clamp all vector acis between the given min and max values
        // Angles are normalized
        public static Vector Clamp(Vector angles, Vector min, Vector max) {
            float x = Angle.Clamp(angles.x, min.x, max.x);
            float y = Angle.Clamp(angles.y, min.y, max.y);
            float z = Angle.Clamp(angles.z, min.z, max.z);
            return new Vector(x, y, z);
        }

        //public static Rotation ToRotation(Vector angles) {
        //    return Rotation.Euler(angles.x, angles.y, angles.z);
        //}
    }
}
