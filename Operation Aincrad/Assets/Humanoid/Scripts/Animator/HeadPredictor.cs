/* Head Predictor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * date: September 20, 2017 
 * 
 */
using UnityEngine;

namespace Passer {

    [System.Serializable]
    public class HeadPredictor : UnityHeadSensor {
        private Vector3 positionalVelocity;
        private Quaternion rotationalVelocity;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
        }

        public override void Update() {
            base.Update();
            if (headTarget.head.bone.transform == null)
                return;

            CalculatePositionalVelocity();
            CalculateRotationalVelocity();

            if (headTarget.neck.target.confidence.rotation > 0.1F) {
                headTarget.neck.target.transform.rotation *= Quaternion.Slerp(Quaternion.identity, rotationalVelocity, Time.deltaTime);
                headTarget.neck.target.confidence.rotation -= Time.deltaTime; // 0.1F;
            }

            if (headTarget.neck.target.confidence.rotation > 0.1F) {
                headTarget.neck.target.transform.position += positionalVelocity * Time.deltaTime;
                headTarget.neck.target.confidence.position -= Time.deltaTime; // 0.1F;
            }
        }

        private Quaternion lastOrientation;
        private float velocityChange;
        private void CalculateRotationalVelocity() {
            if (lastOrientation.w != 0) {
                Quaternion lastRotationalVelocity = rotationalVelocity;

                Quaternion rotation = Quaternion.Inverse(lastOrientation) * headTarget.neck.bone.targetRotation;
                rotationalVelocity = Quaternion.SlerpUnclamped(Quaternion.identity, rotation, 1 / Time.deltaTime);

                velocityChange = (velocityChange + Quaternion.Angle(lastRotationalVelocity, rotationalVelocity)) / 2;

                // stabelize head
                if (velocityChange < 1)
                    rotationalVelocity = Quaternion.Slerp(Quaternion.identity, rotationalVelocity, velocityChange);
            }
            lastOrientation = headTarget.neck.bone.targetRotation;
        }

        private Vector3 lastPosition;
        private float positionalVelocityChange;
        private void CalculatePositionalVelocity() {
            if (lastPosition.sqrMagnitude != 0) {
                Vector3 lastPositionalVelocity = positionalVelocity;

                Vector3 translation = headTarget.neck.bone.transform.position - lastPosition;
                positionalVelocity = translation / Time.deltaTime;

                positionalVelocityChange = (positionalVelocityChange + (positionalVelocity - lastPositionalVelocity).magnitude) / 2;

                // stabelize head
                if (positionalVelocityChange < 10)
                    positionalVelocity = positionalVelocity * (positionalVelocityChange / 10);
            }
            lastPosition = headTarget.neck.bone.transform.position;
        }
    }
}