/* InstantVR Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 */

using UnityEngine;

namespace Passer {

    public abstract class Movements {
        public Movements() { }

        public virtual void Start(HumanoidControl _humanoid, HumanoidTarget _target) {
        }

        private static float maxAngularSpeed = 360;

        protected Quaternion LimitRotationSpeed(Quaternion oldRotation, Quaternion newRotation, float thisMaxAngularSpeed) {
            newRotation = Quaternion.RotateTowards(oldRotation, newRotation, maxAngularSpeed * Time.deltaTime);
            return newRotation;
        }

        //float last;
        protected Quaternion MeasureRotationSpeed(Quaternion oldRotation, Quaternion newRotation) {
            //Debug.Log(maxAngularSpeed * Time.deltaTime + ": "+ Quaternion.Angle(oldRotation, newRotation) + " T " + newRotation.eulerAngles);
            //last = Time.time;
            return newRotation;
        }
        protected static Quaternion LimitRotationSpeed(Quaternion oldRotation, Quaternion newRotation) {
            newRotation = Quaternion.RotateTowards(oldRotation, newRotation, maxAngularSpeed * Time.deltaTime);
            return newRotation;
        }

        protected static Quaternion LimitAngle(HumanoidTarget.TargetedBone targetedBone, ref Quaternion lastLocalBoneRotation, Quaternion boneRotation) {
            if (targetedBone.parent == null || targetedBone.parent.bone.transform == null)
                return boneRotation;

            Quaternion parentRotation = targetedBone.parent.bone.targetRotation;
            Quaternion localRotation = Quaternion.Inverse(parentRotation) * boneRotation;

            float lastAngle = Quaternion.Angle(Quaternion.identity, lastLocalBoneRotation);
            float angle = Quaternion.Angle(parentRotation, boneRotation);
            if (angle < lastAngle) {
                // Decreasing angle

                // No damping or limit

                lastLocalBoneRotation = localRotation;
                return boneRotation;

            } else {
                // Increasing angle
                Quaternion deltaLocalRotation = Quaternion.Inverse(lastLocalBoneRotation) * localRotation;

                // Damping tension based limit
                float tension = targetedBone.GetTension();
                float f = Mathf.Clamp01(1 - tension * tension);
                deltaLocalRotation = Quaternion.Slerp(Quaternion.identity, deltaLocalRotation, f);
                lastLocalBoneRotation = lastLocalBoneRotation * deltaLocalRotation;

                // Hard limit
                lastLocalBoneRotation = Quaternion.RotateTowards(Quaternion.identity, lastLocalBoneRotation, targetedBone.bone.maxAngle);

                return parentRotation * lastLocalBoneRotation;
            }
        }
        protected Vector3 LimitAngles(HumanoidTarget.TargetedBone bone, Vector3 angles) {
            angles = UnityAngles.Clamp(angles, bone.bone.minAngles, bone.bone.maxAngles);
            return angles;
        }

        public static void ClampRotation(Transform bone, Quaternion referenceRotation, Vector3 ROMnegative, Vector3 ROMpositive) {
            Quaternion localRotation = Quaternion.Inverse(referenceRotation) * bone.rotation;
            Vector3 localEuler = localRotation.eulerAngles;

            localRotation = Quaternion.Euler(
                Mathf.Clamp(Angle180(localEuler.x), ROMnegative.x, ROMpositive.x),
                Mathf.Clamp(Angle180(localEuler.y), ROMnegative.y, ROMpositive.y),
                Mathf.Clamp(Angle180(localEuler.z), ROMnegative.z, ROMpositive.z));

            bone.rotation = referenceRotation * localRotation;
        }

        public static Vector3 MuscleTension(Transform bone, Quaternion referenceRotation, Vector3 ROMnegative, Vector3 ROMpositive) {
            Quaternion localRotation = Quaternion.Inverse(referenceRotation) * bone.rotation;
            Vector3 localEuler = localRotation.eulerAngles;

            Vector3 tension = new Vector3(
                AxisTension(Angle180(localEuler.x), ROMnegative.x, ROMpositive.x),
                AxisTension(Angle180(localEuler.y), ROMnegative.y, ROMpositive.y),
                AxisTension(Angle180(localEuler.z), ROMnegative.z, ROMpositive.z));

            return tension;
        }

        private static float AxisTension(float angle, float ROMnegative, float ROMpositive) {
            return AxisTension(angle, ROMnegative, ROMpositive, 1);
        }

        private static float AxisTension(float angle, float ROMnegative, float ROMpositive, float MaxTension) {
            float f;
            if (angle < 0)
                f = angle / ROMnegative;
            else {
                f = angle / ROMpositive;
            }

            return (f * f) * MaxTension;
        }

        public static Vector3 Euler180(Vector3 eulerAngles) {
            return new Vector3(
                Angle180(eulerAngles.x),
                Angle180(eulerAngles.y),
                Angle180(eulerAngles.z));
        }

        public static float Angle180(float _angle) {
            float angle = _angle;
            while (angle > 180)
                angle -= 360;
            while (angle < -180)
                angle += 360;
            return angle;
        }

        public static float AngleDifference(float a, float b) {
            float r = b - a;
            while (r < -180)
                r += 360;
            while (r > 180)
                r -= 360;
            return r;
        }

    }
}