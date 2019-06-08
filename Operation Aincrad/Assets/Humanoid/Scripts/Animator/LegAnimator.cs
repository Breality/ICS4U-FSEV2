using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class LegAnimator : UnityLegSensor {
        LegAnimator leftFoot, rightFoot;

        private Vector3 startHeadFoot;
        private float legLength;
        private float footSeparation;

        Vector3 prevPrintPosition = Vector3.zero;
        Quaternion prevPrintOrientation = Quaternion.identity;

        #region Start
        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);
            target = targetTransform.GetComponent<FootTarget>();

            legLength = footTarget.upperLeg.bone.length + footTarget.lowerLeg.bone.length;

            if (Application.isPlaying) {
                startHeadFoot = humanoid.headTarget.neck.target.transform.InverseTransformPoint(targetTransform.position);
                startHeadFoot = new Vector3(footTarget.isLeft ? -0.1F : 0.1F, startHeadFoot.y, 0);
                footSeparation = Vector3.Distance(humanoid.leftFootTarget.foot.target.transform.position, humanoid.rightFootTarget.foot.target.transform.position) / 2;
            }

            lastPosition = targetTransform.position;
            lastOrientation = targetTransform.rotation;
        }
        #endregion

        #region Update
        public override void Update() {
            if (!footTarget.humanoid.animatorEnabled || !enabled || footTarget.humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            status = Status.Tracking;

            leftFoot = footTarget.humanoid.leftFootTarget.animator;
            rightFoot = footTarget.humanoid.rightFootTarget.animator;

            FeetAnimation();
            UpdateAnimation();
        }

        [HideInInspector]
        private Vector3 basePointStart, basePointDelta;
        [HideInInspector]
        private bool movedLast = false;
        [HideInInspector]
        private bool follow = false;

        protected void FeetAnimation() {
            leftFoot.scale = footTarget.humanoid.targetsRig.transform.localScale;
            rightFoot.scale = footTarget.humanoid.targetsRig.transform.localScale;

            CheckGroundChange();

            if (footTarget.humanoid.ground == null)
                basePointDelta = footTarget.humanoid.transform.position - basePointStart;
            else
                basePointDelta = (footTarget.humanoid.transform.position - footTarget.humanoid.ground.position) - basePointStart;
            basePointDelta = new Vector3(basePointDelta.x, 0, basePointDelta.z);

            CheckRotation();
            CheckTranslation();
            CheckStaying();

            FootStepping(this);

            //Debug.Log(footTarget.side + " isMoving:" + footTarget.animator.isMoving + " movedLast:" + footTarget.animator.movedLast +
            //    " follow:" + footTarget.animator.follow);
        }

        private void CheckRotation() {
            // Don't start a step when a foot is moving
            if (footTarget.animator.isMoving || footTarget.otherFoot.animator.isMoving)
                return;

            Quaternion footAngle = Quaternion.FromToRotation(footTarget.humanoid.hipsTarget.hips.target.transform.forward, footTarget.foot.target.transform.forward);
            Vector3 axis;
            float angle;
            footAngle.ToAngleAxis(out angle, out axis);
            if (angle < 45)
                return;

            float rotationDirection = Vector3.Angle(footTarget.humanoid.up, axis);
            if (footTarget.isLeft && rotationDirection < 90) {
                StartFootStep(footTarget);
                footTarget.otherFoot.animator.follow = true;
            }

            else if (!footTarget.isLeft && rotationDirection > 90) {
                StartFootStep(footTarget);
                footTarget.otherFoot.animator.follow = true;
            }
        }

        private void CheckTranslation() {
            Vector3 middleFootPosition = (footTarget.foot.target.transform.position + footTarget.otherFoot.foot.target.transform.position) / 2;
            middleFootPosition = ProjectPointOnPlane(middleFootPosition, footTarget.humanoid.transform.position, Vector3.up);
            Vector3 projectedCOM = ProjectedCenterOfMass();
            Vector3 com2Foot = projectedCOM - middleFootPosition;

            if (Vector3.Distance(projectedCOM, lastPosition) > 1) { // We probably teleported
                ResetFoot(projectedCOM);
                return;
            }

            // Don't start a step when a foot is moving
            if (footTarget.animator.isMoving || footTarget.otherFoot.animator.isMoving)
                return;

            // We need to make a step to get the center of mass between te feet again.
            if (com2Foot.magnitude > 0.10F && footTarget.humanoid.ground != null) {
                if (footTarget.otherFoot.animator.movedLast) {
                    // other foot did its step last, now it is our turn
                    StartFootStep(footTarget);
                    footTarget.otherFoot.animator.follow = true;
                }

                else {
                    // This foot will make the first step now
                    Vector3 footDirection = footTarget.foot.target.transform.position - middleFootPosition;
                    float angle = Vector3.Angle(com2Foot.normalized, footDirection.normalized);

                    // Is this foor in the movement direction, then start a step
                    if (angle < 90) {
                        StartFootStep(footTarget);
                        footTarget.otherFoot.animator.follow = true;
                    }
                }
            }
            else if (footTarget.animator.follow && footTarget.otherFoot.animator.movedLast) {
                StartFootStep(footTarget);
            }
        }

        private void CheckStaying() {
            if (footTarget.animator.isMoving)
                return;

            FootStaying();
        }

        public void ResetFoot(Vector3 com) {
            Quaternion hipsRotation = Quaternion.LookRotation(footTarget.humanoid.hipsTarget.hips.target.transform.forward, footTarget.humanoid.up);

            Vector3 deltaX = footSeparation * (footTarget.isLeft ? Vector3.left : Vector3.right);
            footTarget.foot.target.transform.position = com + (hipsRotation * deltaX);
            lastPosition = footTarget.foot.target.transform.position;

            footTarget.foot.target.transform.rotation = Quaternion.identity;
            lastOrientation = footTarget.foot.target.transform.rotation;
            footTarget.animator.isMoving = false;
            footTarget.animator.movedLast = false;
            footTarget.animator.follow = false;
            basePointStart = footTarget.humanoid.transform.position;
            if (footTarget.humanoid.ground != null)
                basePointStart -= footTarget.humanoid.ground.position;
        }

        private Transform lastGround;
        private void CheckGroundChange() {
            if (footTarget.humanoid.ground != lastGround) {
                if (lastGround != null)
                    basePointStart += lastGround.position;
                if (footTarget.humanoid.ground != null)
                    basePointStart -= footTarget.humanoid.ground.position;
            }
            lastGround = footTarget.humanoid.ground;
        }

        private void StartFootStep(FootTarget footTarget) {
            if (footTarget.animator.isMoving)
                return;

            basePointStart = footTarget.humanoid.transform.position;
            if (footTarget.humanoid.ground != null)
                basePointStart -= footTarget.humanoid.ground.position;

            footTarget.foot.target.transform.position = lastPosition;
            footTarget.foot.target.transform.rotation = lastOrientation;

            prevPrintPosition = footTarget.foot.target.transform.position;
            prevPrintOrientation = footTarget.foot.target.transform.rotation;

            footTarget.animator.isMoving = true;
            footTarget.animator.movedLast = true;
            footTarget.animator.follow = false;
            footTarget.otherFoot.animator.movedLast = false;
        }

        Vector3 lastPosition = Vector3.zero;
        Quaternion lastOrientation = Quaternion.identity;
        private void FootStaying() {
            if (footTarget.humanoid.ground == null && !footTarget.otherFoot.animator.isMoving)
                FeetOffGround();
            else
                FootStaysOnGround();
        }

        private void FootStaysOnGround() {
            footTarget.foot.target.transform.position = lastPosition + (footTarget.humanoid.groundVelocity * Time.deltaTime);
            footTarget.foot.target.transform.rotation = lastOrientation * Quaternion.AngleAxis(footTarget.humanoid.groundAngularVelocity * Time.deltaTime, footTarget.humanoid.up);

            prevPrintPosition = footTarget.foot.target.transform.position;
            prevPrintOrientation = footTarget.foot.target.transform.rotation;
        }

        private void FeetOffGround() {
            HipsTarget hipsTarget = footTarget.humanoid.hipsTarget;
            footTarget.foot.target.transform.rotation = Quaternion.AngleAxis(hipsTarget.hips.target.transform.eulerAngles.y, footTarget.humanoid.up) * Quaternion.AngleAxis(30, Vector3.right);

            Vector3 localFootPosition = new Vector3(footTarget.isLeft ? -footSeparation : footSeparation, -legLength, 0);
            footTarget.foot.target.transform.position = hipsTarget.hips.target.transform.position + hipsTarget.hips.target.transform.rotation * localFootPosition;
        }
        #endregion

        private void FootStepping(LegAnimator footAnimation) {
            if (footAnimation.f >= 1) {
                footAnimation.isMoving = false;
                footAnimation.f = 0;

            }
            else if (footAnimation.f > 0) {
                Vector3 nextFootPrint = CalculateNextFootPrint();

                if (footTarget.humanoid.velocity.x != 0 || footTarget.humanoid.velocity.z != 0) {
                    Vector3 avgSpeed = basePointDelta / footAnimation.f;
                    nextFootPrint += avgSpeed * 0.2F;
                }

                if (footTarget.humanoid.ground != null)
                    prevPrintPosition += footTarget.humanoid.groundVelocity * Time.deltaTime;

                footAnimation.step = nextFootPrint - prevPrintPosition;
                Vector3 newPosition = prevPrintPosition + footAnimation.GetCurrentPosition(footAnimation.step, footAnimation.fWithinFrame);

                Quaternion newOrientation = Quaternion.LookRotation(footTarget.humanoid.hipsTarget.hips.target.transform.forward, footTarget.humanoid.up);
                Quaternion orientationChange = Quaternion.Inverse(prevPrintOrientation) * newOrientation;
                Quaternion newRotation = prevPrintOrientation * footAnimation.GetRotation(orientationChange, footAnimation.f);

                footAnimation.DrawCurve(prevPrintPosition, nextFootPrint - prevPrintPosition, prevPrintOrientation, orientationChange);

                footTarget.foot.target.transform.position = newPosition;
                footTarget.foot.target.transform.rotation = newRotation;
            }
            lastPosition = footTarget.foot.target.transform.position;
            lastOrientation = footTarget.foot.target.transform.rotation;
        }

        private Vector3 CalculateNextFootPrint() {
            Vector3 groundNormal = footTarget.humanoid.up;

            Quaternion hipsRotation = Quaternion.LookRotation(footTarget.humanoid.hipsTarget.hips.target.transform.forward, footTarget.humanoid.up);

            Vector3 com = GetCenterOfMass();
            Vector3 deltaX = footSeparation * (footTarget.isLeft ? Vector3.left : Vector3.right);
            com += (hipsRotation * deltaX);
            Vector3 footPosition = ProjectPointOnPlane(com, footTarget.humanoid.transform.position, groundNormal);

            float distance = footTarget.humanoid.GetDistanceToGroundAt(footPosition, 0.4F);
            Vector3 footPrintPosition = footPosition + (distance + footTarget.soleThicknessFoot) * groundNormal;
            footPrintPosition = PreventLegIntersection(footPrintPosition);
            return footPrintPosition;
        }

        private Vector3 PreventLegIntersection(Vector3 footPrintPosition) {
            Vector3 otherFootLocation = footTarget.otherFoot.foot.target.transform.InverseTransformPoint(footPrintPosition);
            if (footTarget.isLeft && otherFootLocation.x > 0.1F)
                otherFootLocation.z -= -0.2F;
            else if (!footTarget.isLeft && otherFootLocation.x < -0.1F)
                otherFootLocation.z += 0.2F;
            else
                return footPrintPosition;

            footPrintPosition = footTarget.otherFoot.foot.target.transform.TransformPoint(otherFootLocation);
            return footPrintPosition;
        }

        private Vector3 GetCenterOfMass() {
            if (footTarget.humanoid.hipsTarget.spine != null && footTarget.humanoid.hipsTarget.spine.bone.transform != null)
                return footTarget.humanoid.hipsTarget.spine.bone.transform.position;
            else if (footTarget.humanoid.hipsTarget.hips.bone.transform != null) {
                Vector3 hipsUp = footTarget.humanoid.hipsTarget.hips.bone.targetRotation * Vector3.up;
                Vector3 com = footTarget.humanoid.hipsTarget.hips.bone.transform.position + hipsUp * 0.2F;
                return com;
            }
            else {
                Vector3 hipsUp = footTarget.humanoid.hipsTarget.hips.target.transform.rotation * Vector3.up;
                Vector3 com = footTarget.humanoid.hipsTarget.hips.target.transform.position + hipsUp * 0.2F;
                return com;
            }
        }

        private Vector3 ProjectedCenterOfMass() {
            Vector3 com = GetCenterOfMass();
            Vector3 projectedCOM = ProjectPointOnPlane(com, footTarget.humanoid.transform.position, footTarget.humanoid.up);
            return projectedCOM;
        }

        private Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeOrigin, Vector3 planeNormal) {
            // Create the vector from the origin to the point
            Vector3 v = point - planeOrigin;
            // Then project this vector onto the plane
            Vector3 v1 = Vector3.ProjectOnPlane(v, planeNormal);
            // result is the point on the plane
            return planeOrigin + v1;
        }

        #region Animator
        public bool isMoving = false;
        public Vector3 scale = Vector3.one;

        private static int nrFrames = 100;
        private KeyFrame[] keyFrames = {
            new KeyFrame( 0, new Vector3(0F, 0.00F, 0.0F), new Vector3(67,0,0), nrFrames),
            new KeyFrame(10, new Vector3(0F, 0.02F, 0.1F), new Vector3(58,0,0), nrFrames),
            new KeyFrame(20, new Vector3(0F, 0.05F, 0.2F), new Vector3(55,0,0), nrFrames),
            new KeyFrame(30, new Vector3(0F, 0.10F, 0.3F), new Vector3(38,0,0), nrFrames),
            new KeyFrame(40, new Vector3(0F, 0.12F, 0.4F), new Vector3(38,0,0), nrFrames),
            new KeyFrame(50, new Vector3(0F, 0.14F, 0.5F), new Vector3(26,0,0), nrFrames),
            new KeyFrame(60, new Vector3(0F, 0.18F, 0.6F), new Vector3(12,0,0), nrFrames),
            new KeyFrame(70, new Vector3(0F, 0.17F, 0.7F), new Vector3(29,0,0), nrFrames),
            new KeyFrame(80, new Vector3(0F, 0.13F, 0.8F), new Vector3(63,0,0), nrFrames),
            new KeyFrame(90, new Vector3(0F, 0.07F, 0.9F), new Vector3(80,0,0), nrFrames),
            new KeyFrame(100, new Vector3(0F, 0.00F, 1.0F), new Vector3(85,0,0), nrFrames)
        };
        private float frameSpeed = 200;

        public float f = 0;
        private float fWithinFrame;
        public int prevFrame, nextFrame;

        private void UpdateAnimation() {
            if (!isMoving)
                return;

            f += Time.deltaTime * (frameSpeed / nrFrames);

            DetermineKeyFrames(f, out prevFrame, out nextFrame);
            fWithinFrame = CalulateFWithingFrame(f, prevFrame, nextFrame);
        }

        private float CalulateFWithingFrame(float f, int prevFrame, int nextFrame) {
            float frameDuration = keyFrames[nextFrame].f - keyFrames[prevFrame].f;
            return (f - keyFrames[prevFrame].f) / frameDuration;
        }

        private void DetermineKeyFrames(float f, out int prevFrame, out int nextFrame) {
            prevFrame = 0;
            nextFrame = 1;

            for (int i = 0; i < keyFrames.Length - 1; i++) {
                if (f >= keyFrames[i].f && f < keyFrames[i + 1].f) {
                    prevFrame = i;
                    nextFrame = i + 1;
                }
            }
        }

        private Vector3 step = Vector3.one;
        public Vector3 GetCurrentPosition(Vector3 step, float fWithinFrame) {
            return GetCurrentPosition(step, fWithinFrame, prevFrame, nextFrame);
        }
        public Vector3 GetCurrentPosition(Vector3 step, float fWithinFrame, int prevFrame, int nextFrame) {
            Vector3 interpolatedPosition = Vector3.Lerp(keyFrames[prevFrame].position, keyFrames[nextFrame].position, fWithinFrame);

            float scale = step.magnitude;
            if (scale == 0) {
                step = footTarget.foot.target.transform.forward;
                scale = 1;
            }
            Quaternion rotation = Quaternion.LookRotation(step.normalized, footTarget.humanoid.up);

            Vector3 result = rotation * Vector3.Scale(interpolatedPosition, new Vector3(scale, 1, scale));
            return result;
        }

        private float RotationAngle(Quaternion q) {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return angle;
        }
        private Vector3 RotationAxis(Quaternion q) {
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            return axis;
        }


        private Vector3 GetPosition(Vector3 step, float f) {
            int prevFrame;
            int nextFrame;
            DetermineKeyFrames(f, out prevFrame, out nextFrame);
            float fWithinFrame = CalulateFWithingFrame(f, prevFrame, nextFrame);
            return GetCurrentPosition(step, fWithinFrame, prevFrame, nextFrame);
        }

        public Quaternion GetCurrentRotation() {
            return Quaternion.Slerp(keyFrames[prevFrame].rotation, keyFrames[nextFrame].rotation, fWithinFrame);
        }

        private Quaternion GetCurrentRotation(float fWithinFrame, int prevFrame, int nextFrame) {
            Quaternion interpolatedRotation = Quaternion.Slerp(keyFrames[prevFrame].rotation, keyFrames[nextFrame].rotation, fWithinFrame);
            return interpolatedRotation;
        }

        public Quaternion GetRotation(Quaternion orientationChange, float f) {
            int prevFrame;
            int nextFrame;
            DetermineKeyFrames(f, out prevFrame, out nextFrame);
            float fWithinFrame = CalulateFWithingFrame(f, prevFrame, nextFrame);
            Quaternion interpolatedRotation = GetCurrentRotation(fWithinFrame, prevFrame, nextFrame);
            Quaternion interpolatedOrientationChange = Quaternion.Slerp(Quaternion.identity, orientationChange, f);
            return interpolatedOrientationChange * interpolatedRotation;
        }

        private void DrawCurve(Vector3 start, Vector3 step, Quaternion startRot, Quaternion orientationChange) {
            //Debug.DrawRay(start, step, Color.blue);

            //Vector3 lastPos = start + GetPosition(step, 0);
            //for (float f = 0.1F; f <= 1; f += 0.099F) {
            //    Vector3 newPos = start + GetPosition(step, f);
            //    Debug.DrawLine(lastPos, newPos, Color.blue);
            //    Vector3 newOrientation = (startRot * GetRotation(orientationChange, f)) * Vector3.forward;
            //    Debug.DrawRay(newPos, newOrientation * 0.1F, Color.blue);
            //    lastPos = newPos;
            //}
        }

        [System.Serializable]
        public class KeyFrame {
            public float f;
            public Vector3 position;
            public Quaternion rotation;

            public KeyFrame(int _fromFrameNr, Vector3 _position, Vector3 _angles, float totalFrames) {
                position = _position;
                rotation = Quaternion.Euler(73 - _angles.x, _angles.y, _angles.z);
                f = _fromFrameNr / totalFrames;
            }
        }
    }
    #endregion
}