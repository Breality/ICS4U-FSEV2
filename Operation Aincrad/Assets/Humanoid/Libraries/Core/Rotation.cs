using UnityEngine;

namespace Passer.Humanoid.Tracking {
    public struct Rotation {
        public float x;
        public float y;
        public float z;
        public float w;

        public static Rotation identity {
            get {
                return new Rotation(0, 0, 0, 1);
            }
        }

        public Rotation(float _x, float _y, float _z, float _w) {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public Rotation(Vector _xyz, float _w) {
            x = _xyz.x;
            y = _xyz.y;
            z = _xyz.z;
            w = _w;
        }

        public Vector xyz {
            set {
                x = value.x;
                y = value.y;
                z = value.z;
            }
            get {
                return new Vector(x, y, z);
            }
        }

        public float Length {
            get {
                return Mathf.Sqrt(x * x + y * y + z * z + w * w);
            }
        }

        public float LengthSquared {
            get {
                return x * x + y * y + z * z + w * w;
            }
        }

        public void Normalize() {
            float scale = 1.0f / this.Length;
            xyz *= scale;
            w *= scale;
        }
        public static Rotation Normalize(Rotation q) {
            Rotation result;
            Normalize(ref q, out result);
            return result;
        }
        public static void Normalize(ref Rotation q, out Rotation result) {
            float scale = 1.0f / q.Length;
            result = new Rotation(q.xyz * scale, q.w * scale);
        }

        public static float Dot(Rotation a, Rotation b) {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        /* no the same as UnityEngine version!
        public static Rotation Euler(float x, float y, float z) {
            return FromEulerRad(new Vector(x, y, z) * Mathf.Deg2Rad);
        }
        private static Rotation FromEulerRad(Vector euler) {
            var yaw = euler.x;
            var pitch = euler.z;
            var roll = euler.y;

            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = (float)System.Math.Sin(rollOver2);
            float cosRollOver2 = (float)System.Math.Cos(rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = (float)System.Math.Sin(pitchOver2);
            float cosPitchOver2 = (float)System.Math.Cos(pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = (float)System.Math.Sin(yawOver2);
            float cosYawOver2 = (float)System.Math.Cos(yawOver2);

            Rotation result = identity;
            result.x = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
            result.z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.w = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            return result;

        }
        */

        public static Vector ToAngles(Rotation q1) {
            float test = q1.x * q1.y + q1.z * q1.w;
            if (test > 0.499) { // singularity at north pole
                return new Vector(
                    0,
                    2 * Mathf.Atan2(q1.x, q1.w) * Mathf.Rad2Deg,
                    90
                );
            }
            else if (test < -0.499) { // singularity at south pole
                return new Vector(
                    0,
                    -2 * Mathf.Atan2(q1.x, q1.w) * Mathf.Rad2Deg,
                    -90
                );
            }
            else {
                float sqx = q1.x * q1.x;
                float sqy = q1.y * q1.y;
                float sqz = q1.z * q1.z;

                return new Vector(
                    Mathf.Atan2(2 * q1.x * q1.w - 2 * q1.y * q1.z, 1 - 2 * sqx - 2 * sqz) * Mathf.Rad2Deg,
                    Mathf.Atan2(2 * q1.y * q1.w - 2 * q1.x * q1.z, 1 - 2 * sqy - 2 * sqz) * Mathf.Rad2Deg,
                    Mathf.Asin(2 * test) * Mathf.Rad2Deg
                );
            }
        }

        //public static Rotation Clamp(Rotation o, Vector min, Vector max) {
        //    Vector angles = ToAngles(o);
        //    angles = Angles.Clamp(angles, min, max);
        //    return Rotation.Euler(angles); //  Angles.ToRotation(angles);
        //}

        public static Rotation operator *(Rotation r1, Rotation r2) {
            return new Rotation(
                r1.x * r2.w + r1.y * r2.z - r1.z * r2.y + r1.w * r2.x,
                -r1.x * r2.z + r1.y * r2.w + r1.z * r2.x + r1.w * r2.y,
                r1.x * r2.y - r1.y * r2.x + r1.z * r2.w + r1.w * r2.z,
                -r1.x * r2.x - r1.y * r2.y - r1.z * r2.z + r1.w * r2.w
            );
        }

        public static Rotation Inverse(Rotation r) {
            float n = Mathf.Sqrt(r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w);
            return new Rotation(-r.x / n, -r.y / n, -r.z / n, r.w / n);
        }

        public static Rotation LookRotation(Vector forward, Vector upwards) {
            return LookRotation(ref forward, ref upwards);
        }
        public static Rotation LookRotation(Vector forward) {
            Vector up = Vector.up;
            return LookRotation(ref forward, ref up);
        }
        private static Rotation LookRotation(ref Vector forward, ref Vector up) {
            forward = Vector.Normalize(forward);
            Vector right = Vector.Normalize(Vector.Cross(up, forward));
            up = Vector.Cross(forward, right);
            var m00 = right.x;
            var m01 = right.y;
            var m02 = right.z;
            var m10 = up.x;
            var m11 = up.y;
            var m12 = up.z;
            var m20 = forward.x;
            var m21 = forward.y;
            var m22 = forward.z;


            float num8 = (m00 + m11) + m22;
            var quaternion = identity;
            if (num8 > 0f) {
                var num = Mathf.Sqrt(num8 + 1f);
                quaternion.w = num * 0.5f;
                num = 0.5f / num;
                quaternion.x = (m12 - m21) * num;
                quaternion.y = (m20 - m02) * num;
                quaternion.z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22)) {
                var num7 = Mathf.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.x = 0.5f * num7;
                quaternion.y = (m01 + m10) * num4;
                quaternion.z = (m02 + m20) * num4;
                quaternion.w = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22) {
                var num6 = Mathf.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.x = (m10 + m01) * num3;
                quaternion.y = 0.5f * num6;
                quaternion.z = (m21 + m12) * num3;
                quaternion.w = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)System.Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.x = (m20 + m02) * num2;
            quaternion.y = (m21 + m12) * num2;
            quaternion.z = 0.5f * num5;
            quaternion.w = (m01 - m10) * num2;
            return quaternion;
        }

        /* Does not work correctly!
        public static Rotation FromToRotation(Vector fromDirection, Vector toDirection) {
            return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), float.MaxValue);
        }
        //*/

        public static Rotation RotateTowards(Rotation from, Rotation to, float maxDegreesDelta) {
            float num = Angle(from, to);
            if (num == 0f) {
                return to;
            }
            float t = Mathf.Min(1f, maxDegreesDelta / num);
            return SlerpUnclamped(from, to, t);
        }

        public static Rotation AngleAxis(float angle, Vector axis) {
            return AngleAxis(angle, ref axis);
        }
        private static Rotation AngleAxis(float degress, ref Vector axis) {
            if (Vector.SqrMagnitude(axis) == 0.0f)
                return identity;

            Rotation result = identity;
            var radians = degress * Mathf.Deg2Rad;
            radians *= 0.5f;
            Vector.Normalize(axis);
            axis = axis * (float)System.Math.Sin(radians);
            result.x = axis.x;
            result.y = axis.y;
            result.z = axis.z;
            result.w = (float)System.Math.Cos(radians);

            return Normalize(result);
        }

        public static float Angle(Rotation a, Rotation b) {
            float f = Rotation.Dot(a, b);
            return Mathf.Acos(Mathf.Min(Mathf.Abs(f), 1f)) * 2f * Mathf.Rad2Deg;
        }

        public static Rotation Slerp(Rotation a, Rotation b, float t) {
            return Slerp(ref a, ref b, t);
        }
        private static Rotation Slerp(ref Rotation a, ref Rotation b, float t) {
            if (t > 1) t = 1;
            if (t < 0) t = 0;
            return SlerpUnclamped(ref a, ref b, t);
        }

        public static Rotation SlerpUnclamped(Rotation a, Rotation b, float t) {
            return SlerpUnclamped(ref a, ref b, t);
        }
        private static Rotation SlerpUnclamped(ref Rotation a, ref Rotation b, float t) {
            // if either input is zero, return the other.
            if (a.LengthSquared == 0.0f) {
                if (b.LengthSquared == 0.0f) {
                    return identity;
                }
                return b;
            }
            else if (b.LengthSquared == 0.0f) {
                return a;
            }


            float cosHalfAngle = a.w * b.w + Vector.Dot(a.xyz, b.xyz);

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f) {
                // angle = 0.0f, so just return one input.
                return a;
            }
            else if (cosHalfAngle < 0.0f) {
                b.xyz = -b.xyz;
                b.w = -b.w;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f) {
                // do proper slerp for big angles
                float halfAngle = (float)System.Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float)System.Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)System.Math.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
                blendB = (float)System.Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
            }
            else {
                // do lerp if angle is really small.
                blendA = 1.0f - t;
                blendB = t;
            }

            Rotation result = new Rotation(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
            if (result.LengthSquared > 0.0f)
                return Normalize(result);
            else
                return identity;
        }
    }
}
