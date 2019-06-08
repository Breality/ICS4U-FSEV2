/* InstantVR Animator hand
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.3
 * date: December 29, 2015
 * 
 * - Hand positions corrected for ivr.transform.position != 0
 */

using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class ArmPredictor : UnityArmSensor {

        public override Status status {
            get { return Status.Tracking; }
            set { }
        }

        private Quaternion lastHandOrientation;
        private Quaternion handAngularVelocity;
        private Vector3 handVelocityAxis;
        private float handVelocity;

        private Quaternion lastForearmOrientation;
        private Quaternion forearmAngularVelocity;

        private Quaternion lastUpperArmOrientation;
        private Quaternion upperArmAngularVelocity;

        private float lastTime = 0;

        public Vector3[] positionTrail;
        public Vector3[] predictionTrail;
        private int rr = 0;
        private const int trailLength = 200;
        public Vector3 predictedPosition;

        public Vector3 lastPosition;
        public Vector3 lastVelocity;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            positionTrail = new Vector3[trailLength];
            predictionTrail = new Vector3[trailLength];
        }

        public override void Update() {
            //handTarget.shoulder.target.confidence.degrade(); // = Confidence.none;
            //handTarget.upperArm.target.confidence.degrade(); // = Confidence.none;
            //handTarget.forearm.target.confidence.degrade();// = Confidence.none;
            //handTarget.hand.target.confidence.degrade(); // = Confidence.none;

            return;
            //    float deltaTime = Time.time - lastTime;

            //    if (lastTime != 0) {
            //        DrawTrail();
            //        predict();
            //        //handTarget.upperArm.targetTransform.rotation *= Quaternion.AngleAxis(handTarget.upperArm.angularVelocity * deltaTime, handTarget.upperArm.velocityAxis);
            //        //handTarget.forearm.targetTransform.rotation *= Quaternion.AngleAxis(handTarget.forearm.angularVelocity * deltaTime, handTarget.forearm.velocityAxis);
            //        //handTarget.hand.targetTransform.rotation *= Quaternion.AngleAxis(handTarget.hand.angularVelocity * deltaTime, handTarget.hand.velocityAxis);

            //        //handTarget.hand.target.confidence.orientation = 0.3F;
            //        //handTarget.forearm.target.confidence.orientation = 0.3F;
            //        //handTarget.upperArm.target.confidence.orientation = 0.3F;

            //        //handTarget.hand.targetTransform.position += handPositionVelocity * deltaTime;
            //        //handTarget.hand.target.confidence.position = 0.3F;
            //    }

            //    lastTime = Time.time;
        }

        Vector3 handPositionVelocity;
        private void predict() {
            float deltaTime = Time.time - lastTime;

            if (handTarget.hand.target.transform.position != lastPosition) {
                //Vector3 velocity = (handTarget.hand.targetTransform.position - lastPosition) / deltaTime;
                handPositionVelocity = avgVelocity(10) / (10 * deltaTime);
                Debug.DrawRay(handTarget.hand.target.transform.position, handPositionVelocity * deltaTime, Color.black);
                lastPosition = handTarget.hand.target.transform.position;
            }
        }

        private Vector3 avgVelocity(int nSteps) {
            Vector3 velocity = handTarget.hand.target.transform.position - positionTrail[(rr - nSteps + trailLength) % trailLength];
            return velocity;

            //Vector3 total = Vector3.zero;
            //for (int i = 0; i < nSteps; i++) {
            //    total += positionTrail[(rr - i - 1 + trailLength) % trailLength];
            //}
            //Vector3 avg = total / nSteps;
            //return avg;
        }

        private void DrawTrail() {
            float deltaTime = Time.time - lastTime;
            positionTrail[rr % trailLength] = handTarget.hand.target.transform.position;
            predictionTrail[rr % trailLength] = handTarget.hand.target.transform.position + handPositionVelocity * deltaTime;
            rr++;
            for (int i = 0; i < trailLength - 1; i++) {
                Debug.DrawLine(positionTrail[(i + rr) % trailLength], positionTrail[(i + rr + 1) % trailLength], Color.magenta);
                Debug.DrawLine(predictionTrail[(i + rr) % trailLength], predictionTrail[(i + rr + 1) % trailLength], Color.cyan);
            }
        }
    }

    [System.Serializable]
    public class ArmAnimator : UnityArmSensor {
        private Quaternion handBaseRotation;

        public override Status status {
            get { return Status.Tracking; }
            set { }
        }

        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);
            target = targetTransform.GetComponent<HandTarget>();

            if (handTarget.isLeft)
                handBaseRotation = Quaternion.Euler(-25, 0, 90); // * Quaternion.Euler(0, 90, 0);            
            else
                handBaseRotation = Quaternion.Euler(-25, 0, -90); // * Quaternion.Euler(0, -90, 0);            
        }

        public override void Update() {
            if (!handTarget.humanoid.animatorEnabled || !enabled || handTarget.humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            if (handTarget.hand.bone.transform == null || handTarget.upperArm.bone.transform == null)
                return;

            Vector3 handPosition = handTarget.upperArm.bone.transform.position + new Vector3(0, -(handTarget.upperArm.bone.length + handTarget.forearm.bone.length - 0.05F), 0);

            Quaternion handRotation = handTarget.humanoid.hipsTarget.hips.bone.targetRotation * handBaseRotation;

            handTarget.hand.target.transform.position = handPosition;
            handTarget.hand.target.transform.rotation = handRotation;
            handTarget.hand.target.confidence.position = 0.2F;
            handTarget.hand.target.confidence.rotation = 0.2F;
        }
    }
}