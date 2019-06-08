using UnityEngine;

namespace Passer {

    public static class UnityAngles {
        // Clamp all vector axis between the given min and max values
        // Angles are normalized
        public static Vector3 Clamp(Vector3 angles, Vector3 min, Vector3 max) {
            float x = Clamp(angles.x, min.x, max.x);
            float y = Clamp(angles.y, min.y, max.y);
            float z = Clamp(angles.z, min.z, max.z);
            return new Vector3(x, y, z);
        }

        // clamp the angle between the given min and max values
        // Angles are normalized
        public static float Clamp(float angle, float min, float max) {
            float normalizedAngle = Normalize(angle);
            return Mathf.Clamp(normalizedAngle, min, max);
        }

        // Determine the angle difference, result is a normalized angle
        public static float Difference(float a, float b) {
            float r = Normalize(b - a);
            return r;
        }

        // Normalize an angle to the range -180 < angle <= 180
        public static float Normalize(float angle) {
            while (angle <= -180) angle += 360;
            while (angle > 180) angle -= 360;
            return angle;
        }

        // Normalize all vector angles to the range -180 < angle < 180
        public static Vector3 Normalize(Vector3 angles) {
            float x = Normalize(angles.x);
            float y = Normalize(angles.y);
            float z = Normalize(angles.z);
            return new Vector3(x, y, z);
        }

        // Returns the signed angle in degrees between from and to.
        public static float SignedAngle(Vector3 from, Vector3 to) {
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            if (cross.y < 0) angle = -angle;
            return angle;
        }

        // Returns the signed angle in degrees between from and to.
        public static float SignedAngle(Vector2 from, Vector2 to) {
            float sign = Mathf.Sign(from.y * to.x - from.x * to.y);
            return Vector2.Angle(from, to) * sign;
        }

        //public static Quaternion ToQuaternion(Rotation orientation) {
        //    return new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w);
        //}
    }

    public static class Rotations {
        /// <summary>
        /// Rotate a rotation.
        /// Rotates rotation1 according to rotation2.
        /// This is needed, because rotation1 * rotation2 rotates the orientation.
        /// </summary>
        /// <param name="rotation1">The rotation to rotate</param>
        /// <param name="rotation2">The rotation</param>
        /// <returns></returns>
        public static Quaternion Rotate(Quaternion rotation1, Quaternion rotation2) {
            float angle;
            Vector3 axis;
            rotation1.ToAngleAxis(out angle, out axis);

            Vector3 newAxis = rotation2 * axis;
            Quaternion newRotation1 = Quaternion.AngleAxis(angle, newAxis);

            return newRotation1;
        }
    }

    public static class Vectors {
        public static float DistanceToRay(Ray ray, Vector3 point) {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }
    }
    public static class Transforms {
        // transform local rotation to world rotation
        public static Quaternion TransformRotation(Transform transform, Quaternion localRotation) {
            if (transform.parent == null)
                return localRotation;
            else
                return transform.parent.rotation * localRotation;
        }

        //
        // Summary:
        //     ///
        //     Transforms rotation from local space to world space.
        //     ///
        //
        // Parameters:
        //   transform:
        public static Quaternion InverseTransformRotation(Transform transform, Quaternion rotation) {
            if (transform.parent == null)
                return rotation;
            else
                return Quaternion.Inverse(transform.parent.rotation) * rotation;
        }
    }

    public class StoredRigidbody {
        public float mass = 1;
        public float drag;
        public float angularDrag = 0.05F;
        public bool useGravity = true;
        public bool isKinematic;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;
        public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
        public RigidbodyConstraints constraints = RigidbodyConstraints.None;
        public Transform parent;

        public StoredRigidbody(Rigidbody rb) {
            mass = rb.mass;
            drag = rb.drag;
            angularDrag = rb.angularDrag;
            useGravity = rb.useGravity;
            isKinematic = rb.isKinematic;
            interpolation = rb.interpolation;
            collisionDetectionMode = rb.collisionDetectionMode;
            constraints = rb.constraints;

            parent = rb.transform.parent;
        }

        public void CopyToRigidbody(Rigidbody rb) {
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            rb.interpolation = interpolation;
            rb.collisionDetectionMode = collisionDetectionMode;
            rb.constraints = constraints;

            rb.transform.parent = parent;
        }
    }

}