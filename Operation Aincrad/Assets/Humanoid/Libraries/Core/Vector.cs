using UnityEngine;

namespace Passer.Humanoid.Tracking {
    public struct Vector {
        public float x;
        public float y;
        public float z;

        public static Vector zero { get { return new Vector(0, 0, 0); } }
        public static Vector right { get { return new Vector(1, 0, 0); } }
        public static Vector up { get { return new Vector(0, 1, 0); } }
        public static Vector forward { get { return new Vector(0, 0, 1); } }
        public static Vector back { get { return new Vector(0, 0, -1); } }

        public Vector(float _x, float _y, float _z) {
            x = _x;
            y = _y;
            z = _z;
        }

        public static float Magnitude(Vector a) {
            return Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        }

        public static float SqrMagnitude(Vector a) {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }

        public static Vector Normalize(Vector v) {
            float num = Magnitude(v);
            Vector result;
            if (num > 1E-05f) {
                result = v / num;
            }
            else {
                result = zero;
            }
            return result;
        }
        public static Vector operator -(Vector p1) {
            return new Vector(-p1.x, -p1.y, -p1.z);
        }
        public static Vector operator -(Vector p1, Vector p2) {
            return new Vector(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }
        public static Vector operator +(Vector p1, Vector t1) {
            return new Vector(p1.x + t1.x, p1.y + t1.y, p1.z + t1.z);
        }
        public static Vector Scale(Vector p1, Vector p2) {
            return new Vector(p1.x * p2.x, p1.y * p2.y, p1.z * p2.z);
        }
        public static Vector operator *(float f, Vector p) {
            return new Vector(f * p.x, f * p.y, f * p.z);
        }
        public static Vector operator *(Vector p, float f) {
            return new Vector(p.x * f, p.y * f, p.z * f);
        }
        public static Vector operator *(Rotation o, Vector p) {
            float num = o.x * 2f;
            float num2 = o.y * 2f;
            float num3 = o.z * 2f;
            float num4 = o.x * num;
            float num5 = o.y * num2;
            float num6 = o.z * num3;
            float num7 = o.x * num2;
            float num8 = o.x * num3;
            float num9 = o.y * num3;
            float num10 = o.w * num;
            float num11 = o.w * num2;
            float num12 = o.w * num3;
            Vector result = Vector.zero;
            result.x = (1f - (num5 + num6)) * p.x + (num7 - num12) * p.y + (num8 + num11) * p.z;
            result.y = (num7 + num12) * p.x + (1f - (num4 + num6)) * p.y + (num9 - num10) * p.z;
            result.z = (num8 - num11) * p.x + (num9 + num10) * p.y + (1f - (num4 + num5)) * p.z;
            return result;
        }
        public static Vector operator /(Vector a, float d) {
            return new Vector(a.x / d, a.y / d, a.z / d);
        }

        public static float Dot(Vector v1, Vector v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static float Distance(Vector p1, Vector p2) {
            return Magnitude(p1 - p2);
        }

        public static Vector Cross(Vector v1, Vector v2) {
            return new Vector(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }

        //public float Length() {
        //    return Mathf.Sqrt(x * x + y * y + z * z);
        //}
    }
}
