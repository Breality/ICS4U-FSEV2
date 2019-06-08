using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Passer {
    using Humanoid.Tracking;

    public class SensorComponent : MonoBehaviour {
        //public Target target;
        protected Transform trackerTransform;

        //[System.NonSerialized]
        //public UnitySensor sensor;
        public Status status;

        public float rotationConfidence;
        public float positionConfidence;

        public bool autoUpdate = true;

        private void Awake() {
            if (trackerTransform == null)
                trackerTransform = transform.parent;
        }

        public virtual void StartComponent(Transform trackerTransform) {
            // When this function has been called, the sensor will no longer update from Unity Updates.
            // Instead, UpdateComponent needs to be called to update the sensor data
            autoUpdate = false;
            this.trackerTransform = trackerTransform;
        }

        private void Update() {
            if (autoUpdate)
                UpdateComponent();
        }

        public virtual void UpdateComponent() {
            status = Status.Unavailable;
            positionConfidence = 0;
            rotationConfidence = 0;
            gameObject.SetActive(false);
        }

        //public void SetSensor2Target(UnitySensor sensor) {
        //    if (sensor == null || target == null)
        //        return;

        //    sensor.sensor2TargetRotation = Quaternion.Inverse(transform.rotation) * target.transform.rotation;
        //    sensor.sensor2TargetPosition = -InverseTransformPointUnscaled(target.transform, transform.position);
        //}

        //private static Vector3 InverseTransformPointUnscaled(Transform transform, Vector3 position) {
        //    var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
        //    return worldToLocalMatrix.MultiplyPoint3x4(position);
        //}
    }

    [System.Serializable]
    public class UnitySensor {
        public bool enabled = true;

        public UnitySensor() {
            enabled = true;
        }

        public Target target;
        public Tracker tracker;

        protected Sensor sensor;

        [System.NonSerialized]
        public const string _name = "";
        public virtual string name { get { return _name; } }
        public virtual Status status { get; set; }

        public Transform sensorTransform;

        public Vector3 sensor2TargetPosition;
        public Quaternion sensor2TargetRotation;

        #region Start
        public virtual void Init(Tracker _tracker) {
            tracker = _tracker;
        }

        public virtual void Start(HumanoidControl _humanoid, Transform targetTransform) {
            target = targetTransform.GetComponent<Target>();
        }

        public virtual void Start(Transform objectTransform) { }

        public void CheckSensorTransform() {
            if (enabled && sensorTransform == null)
                CreateSensorTransform();
            else if (!enabled && sensorTransform != null)
                RemoveSensorTransform();

            if (sensor2TargetRotation.x + sensor2TargetRotation.y + sensor2TargetRotation.z + sensor2TargetRotation.w == 0)
                SetSensor2Target();
        }

        protected virtual void CreateSensorTransform() {
        }

        protected void CreateSensorTransform(Transform targetTransform, string resourceName, Vector3 _sensor2TargetPosition, Quaternion _sensor2TargetRotation) {
            GameObject sensorObject;
            if (resourceName == null) {
                sensorObject = new GameObject("Sensor");
            }
            else {
                Object controllerPrefab = Resources.Load(resourceName);
                if (controllerPrefab == null)
                    sensorObject = new GameObject("Sensor");
                else
                    sensorObject = (GameObject)Object.Instantiate(controllerPrefab);

                sensorObject.name = resourceName;
            }

            sensorTransform = sensorObject.transform;
            sensorTransform.parent = tracker.trackerTransform;

            sensor2TargetPosition = -_sensor2TargetPosition;
            sensor2TargetRotation = Quaternion.Inverse(_sensor2TargetRotation);

            UpdateSensorTransformFromTarget(targetTransform);
        }

        protected void RemoveSensorTransform() {
            if (Application.isPlaying)
                Object.Destroy(sensorTransform.gameObject);
            else
                Object.DestroyImmediate(sensorTransform.gameObject, true);
        }

        public virtual void SetSensor2Target() {
            if (sensorTransform == null || target == null)
                return;

            sensor2TargetRotation = Quaternion.Inverse(sensorTransform.rotation) * target.transform.rotation;
            sensor2TargetPosition = -InverseTransformPointUnscaled(target.transform, sensorTransform.position);
        }

        private static Vector3 InverseTransformPointUnscaled(Transform transform, Vector3 position) {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }
        #endregion

        #region Update
        public virtual void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            sensor.Update();
            if (sensor.status != Status.Tracking)
                return;

            UpdateSensorTransform(sensor);
            UpdateTargetTransform();
        }

        protected void UpdateSensorTransform(Sensor sensor) {
            if (sensorTransform == null)
                return;

            if (status == Status.Tracking) {
                sensorTransform.gameObject.SetActive(true);
                sensorTransform.position = Target.ToVector3(sensor.sensorPosition);
                sensorTransform.rotation = Target.ToQuaternion(sensor.sensorRotation);
            } else {
                sensorTransform.gameObject.SetActive(false);
            }
        }

        public void UpdateSensorTransformFromTarget(Transform targetTransform) {
            if (sensorTransform == null)
                return;

            sensorTransform.position = TransformPointUnscaled(targetTransform, -sensor2TargetPosition);
            sensorTransform.rotation = targetTransform.rotation * Quaternion.Inverse(sensor2TargetRotation);
        }

        private static Vector3 TransformPointUnscaled(Transform transform, Vector3 position) {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        protected virtual void UpdateTargetTransform() {
            target.transform.rotation = sensorTransform.rotation * sensor2TargetRotation;
            target.transform.position = sensorTransform.position + target.transform.rotation * sensor2TargetPosition;
        }
        #endregion

        #region Stop
        public virtual void Stop() { }
        #endregion  

        public virtual void ShowSensor(bool shown) {
            if (sensorTransform == null)
                return;

            if (!Application.isPlaying)
                sensorTransform.gameObject.SetActive(shown);

            Renderer[] renderers = sensorTransform.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = shown;
        }

        public virtual void ShowSensor(HumanoidTarget target, bool shown) { }
    }

    public class UnityHumanoidSensor : UnitySensor {
        protected virtual void UpdateTarget(HumanoidTarget.TargetTransform target, Transform sensorTransform) {
            if (target.transform == null || sensorTransform == null)
                return;

            target.transform.rotation = GetTargetRotation(sensorTransform);
            target.confidence.rotation = 0.5F;

            target.transform.position = GetTargetPosition(sensorTransform);
            target.confidence.position = 0.5F;
        }

        protected virtual void UpdateTarget(HumanoidTarget.TargetTransform target, SensorComponent sensorComponent) {
            if (target.transform == null || sensorComponent.rotationConfidence + sensorComponent.positionConfidence <= 0)
                return;

            target.transform.rotation = GetTargetRotation(sensorComponent.transform);
            target.confidence.rotation = sensorComponent.rotationConfidence;

            target.transform.position = GetTargetPosition(sensorComponent.transform);
            target.confidence.position = sensorComponent.positionConfidence;
        }

        protected Vector3 GetTargetPosition(Transform sensorTransform) {
            Vector3 targetPosition = sensorTransform.position + sensorTransform.rotation * sensor2TargetRotation * sensor2TargetPosition;
            return targetPosition;
        }

        protected Quaternion GetTargetRotation(Transform sensorTransform) {
            Quaternion targetRotation = sensorTransform.rotation * sensor2TargetRotation;
            return targetRotation;
        }
    }

    public class UnityController : UnitySensor {
    }


    public class UnityHeadSensor : UnityHumanoidSensor {
        protected HeadTarget headTarget {
            get { return (HeadTarget)target;  }
        }
        protected new HeadSensor sensor;

        #region Start
        public virtual void Init(HeadTarget headTarget) {
            target = headTarget;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            target = targetTransform.GetComponent<HeadTarget>();
            base.Start(_humanoid, targetTransform);
        }

#if UNITY_EDITOR
        public void InitController(SerializedProperty sensorProp, HeadTarget target) {
            if (sensorProp == null)
                return;

            Init(target);

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = sensorTransform;

            SerializedProperty targetProp = sensorProp.FindPropertyRelative("target");
            targetProp.objectReferenceValue = base.target;

            if (!tracker.enabled || !enabled)
                return;

            CheckSensorTransform();
            ShowSensor(target.humanoid.showRealObjects && target.showRealObjects);

            SerializedProperty sensor2TargetPositionProp = sensorProp.FindPropertyRelative("sensor2TargetPosition");
            sensor2TargetPositionProp.vector3Value = sensor2TargetPosition;
            SerializedProperty sensor2TargetRotationProp = sensorProp.FindPropertyRelative("sensor2TargetRotation");
            sensor2TargetRotationProp.quaternionValue = sensor2TargetRotation;
        }

        public void RemoveController(SerializedProperty sensorProp) {
            if (sensorProp == null)
                return;

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = null;
        }
#endif

        protected virtual void CreateSensorTransform(string resourceName, Vector3 sensor2TargetPosition, Quaternion sensor2TargetRotation) {
            CreateSensorTransform(headTarget.head.target.transform, resourceName, sensor2TargetPosition, sensor2TargetRotation);
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            status = sensor.Update();
            UpdateSensorTransform(sensor);

            if (status != Status.Tracking)
                return;

            UpdateHeadTargetTransform(sensor);
        }

        protected virtual void UpdateHeadTargetTransform(HeadSensor headTracker) {
            if (headTarget.head.target.transform != null) {
                if (headTracker.head.confidence.rotation > 0)
                    headTarget.head.target.transform.rotation = Target.ToQuaternion(headTracker.head.rotation) * sensor2TargetRotation;
                if (headTracker.head.confidence.position > 0)
                    headTarget.head.target.transform.position = Target.ToVector3(headTracker.head.position) + headTarget.head.target.transform.rotation * sensor2TargetPosition;
                headTarget.head.target.confidence = headTracker.head.confidence;
            }
        }
        #endregion
    }

#if hFACE
    public class UnityFaceSensor : UnityHumanoidSensor {
        protected HeadTarget headTarget;
        [System.NonSerialized]
        protected FaceTarget faceTarget;
        protected FaceSensor faceSensor;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            headTarget = targetTransform.GetComponent<HeadTarget>();
            faceTarget = headTarget.face;
            base.Start(_humanoid, targetTransform);
        }

        /// <summary>
        ///  Ensure that the sensors will update the target data directly
        /// </summary>
        /// <param name="_faceSensor">The FaceSensor which should update the FaceTarget</param>
        protected void SetFaceSensor(FaceSensor _faceSensor) {
            faceSensor = _faceSensor;
            if (faceSensor == null)
                return;

            faceSensor.leftBrow.outer = faceTarget.leftBrow.outer.target;
            faceSensor.leftBrow.center = faceTarget.leftBrow.center.target;
            faceSensor.leftBrow.inner = faceTarget.leftBrow.inner.target;

            faceSensor.rightBrow.outer = faceTarget.rightBrow.outer.target;
            faceSensor.rightBrow.center = faceTarget.rightBrow.center.target;
            faceSensor.rightBrow.inner = faceTarget.rightBrow.inner.target;

            faceSensor.leftCheek = faceTarget.leftCheek.target;
            faceSensor.rightCheek = faceTarget.rightCheek.target;

            faceSensor.nose.top = faceTarget.nose.top.target;
            faceSensor.nose.tip = faceTarget.nose.tip.target;
            faceSensor.nose.bottomLeft = faceTarget.nose.bottomLeft.target;
            faceSensor.nose.bottom = faceTarget.nose.bottom.target;
            faceSensor.nose.bottomRight = faceTarget.nose.bottomRight.target;

            faceSensor.mouth.upperLip = faceTarget.mouth.upperLip.target;
            faceSensor.mouth.upperLipLeft = faceTarget.mouth.upperLipLeft.target;
            faceSensor.mouth.upperLipRight = faceTarget.mouth.upperLipRight.target;
            faceSensor.mouth.lipLeft = faceTarget.mouth.lipLeft.target;
            faceSensor.mouth.lipRight = faceTarget.mouth.lipRight.target;
            faceSensor.mouth.lowerLip = faceTarget.mouth.lowerLip.target;
            faceSensor.mouth.lowerLipLeft = faceTarget.mouth.lowerLipLeft.target;
            faceSensor.mouth.lowerLipRight = faceTarget.mouth.lowerLipRight.target;

            faceSensor.jaw = faceTarget.jaw.target;
        }

        protected virtual void UpdateEyeBrows(FaceTarget face) {
            face.leftBrow.outer.target.transform.localPosition = HumanoidTarget.ToVector3(face.leftBrow.outer.target.position); // ==  kinectFace.leftBrow.outer.position
            face.leftBrow.center.target.transform.localPosition = HumanoidTarget.ToVector3(face.leftBrow.center.target.position); // ==  kinectFace.leftBrow.outer.position
            face.leftBrow.inner.target.transform.localPosition = HumanoidTarget.ToVector3(face.leftBrow.inner.target.position); // ==  kinectFace.leftBrow.outer.position

            face.rightBrow.outer.target.transform.localPosition = HumanoidTarget.ToVector3(face.rightBrow.outer.target.position); // ==  kinectFace.leftBrow.outer.position
            face.rightBrow.center.target.transform.localPosition = HumanoidTarget.ToVector3(face.rightBrow.center.target.position); // ==  kinectFace.leftBrow.outer.position
            face.rightBrow.inner.target.transform.localPosition = HumanoidTarget.ToVector3(face.rightBrow.inner.target.position); // ==  kinectFace.leftBrow.outer.position
        }


        protected virtual void UpdateEyeLids(FaceSensor face) {
            headTarget.face.leftEye.closed = face.leftEye.closed;
            headTarget.face.rightEye.closed = face.rightEye.closed;
        }

        protected virtual void UpdateCheeks(FaceTarget face) {
            face.leftCheek.target.transform.localPosition = HumanoidTarget.ToVector3(face.leftCheek.target.position);
            face.rightCheek.target.transform.localPosition = HumanoidTarget.ToVector3(face.rightCheek.target.position);
        }

        protected virtual void UpdateNose(Nose nose) {
            nose.top.target.transform.localPosition = HumanoidTarget.ToVector3(nose.top.target.position);

            nose.tip.target.transform.localPosition = HumanoidTarget.ToVector3(nose.tip.target.position);

            nose.bottomLeft.target.transform.localPosition = HumanoidTarget.ToVector3(nose.bottomLeft.target.position);
            nose.bottom.target.transform.localPosition = HumanoidTarget.ToVector3(nose.bottom.target.position);
            nose.bottomRight.target.transform.localPosition = HumanoidTarget.ToVector3(nose.bottomRight.target.position);
        }


        protected virtual void UpdateMouth(Mouth mouth) {
            mouth.upperLip.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.upperLip.target.position);
            mouth.upperLipLeft.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.upperLipLeft.target.position);
            mouth.upperLipRight.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.upperLipRight.target.position);

            mouth.lipLeft.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.lipLeft.target.position);
            mouth.lipRight.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.lipRight.target.position);

            mouth.lowerLip.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.lowerLip.target.position);
            mouth.lowerLipLeft.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.lowerLipLeft.target.position);
            mouth.lowerLipRight.target.transform.localPosition = HumanoidTarget.ToVector3(mouth.lowerLipRight.target.position);
        }

        protected virtual void UpdateJaw(FaceTarget.TargetedJawBone jaw) {
            jaw.target.transform.localRotation = HumanoidTarget.ToQuaternion(jaw.target.rotation);
        }

    }
#endif

    public class UnityArmSensor : UnityHumanoidSensor {
        protected HandTarget handTarget {
            get { return (HandTarget)target; }
        }
        protected new ArmSensor sensor;

        //public override Status status {
        //    get {
        //        if (sensor == null)
        //            return Status.Unavailable;
        //        else
        //            return sensor.status;
        //    }
        //    set {
        //        if (sensor != null)
        //            sensor.status = value;
        //    }
        //}

        #region Start
        public virtual void Init(HandTarget handTarget) {
            target = handTarget;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            target = targetTransform.GetComponent<HandTarget>();
        }

#if UNITY_EDITOR
        public void InitController(SerializedProperty sensorProp, HandTarget handTarget) {
            if (sensorProp == null)
                return;

            Init(handTarget);

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = sensorTransform;

            SerializedProperty targetProp = sensorProp.FindPropertyRelative("target");
            targetProp.objectReferenceValue = target;

            if (!tracker.enabled || !enabled)
                return;

            CheckSensorTransform();
            sensorTransformProp.objectReferenceValue = sensorTransform;

            ShowSensor(handTarget.humanoid.showRealObjects && handTarget.showRealObjects);

            SerializedProperty sensor2TargetPositionProp = sensorProp.FindPropertyRelative("sensor2TargetPosition");
            sensor2TargetPositionProp.vector3Value = sensor2TargetPosition;
            SerializedProperty sensor2TargetRotationProp = sensorProp.FindPropertyRelative("sensor2TargetRotation");
            sensor2TargetRotationProp.quaternionValue = sensor2TargetRotation;
        }

        public void RemoveController(SerializedProperty sensorProp) {
            if (sensorProp == null)
                return;

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = null;
        }
#endif

        public void CheckSensorTransform(Transform targetTransform, bool isLeft) {
            if (enabled && sensorTransform == null)
                CreateSensorTransform(targetTransform, isLeft);
            else if (!enabled && sensorTransform != null)
                RemoveSensorTransform();
        }

        public virtual void CreateSensorTransform(Transform targetTransform, bool isLeft) { }

        protected virtual void CreateSensorTransform(string resourceName, Vector3 sensor2TargetPosition, Quaternion sensor2TargetRotation) {
            CreateSensorTransform(handTarget.hand.target.transform, resourceName, sensor2TargetPosition, sensor2TargetRotation);
        }

        public override void SetSensor2Target() {
            if (sensorTransform == null || target == null)
                return;

            sensor2TargetRotation = Quaternion.Inverse(sensorTransform.rotation) * target.transform.rotation;
            sensor2TargetPosition = -target.transform.InverseTransformPoint(sensorTransform.position);
        }
        #endregion

        #region Update
        protected void UpdateArm(ArmSensor armSensor) {
            float armConfidence = ArmConfidence(armSensor);
            if (handTarget.hand.target.confidence.position > armConfidence)
                UpdateArmIK(armSensor);
            else
                UpdateArmDirect(armSensor);
        }

        private void UpdateArmDirect(ArmSensor armSensor) {
            UpdateShoulder(armSensor);
            UpdateUpperArm(armSensor);
            UpdateForearm(armSensor);
            UpdateHand(armSensor);
        }

        private void UpdateArmIK(ArmSensor armSensor) {
            Vector3 handTargetPosition = handTarget.hand.target.transform.position;
            Quaternion handTargetRotation = handTarget.hand.target.transform.rotation;

            Vector3 forearmUpAxis = HumanoidTarget.ToQuaternion(armSensor.upperArm.rotation) * Vector3.up;
            if (handTarget.upperArm.target.confidence.rotation < 0.9F) {
                handTarget.upperArm.target.transform.rotation = ArmMovements.UpperArmRotationIK(handTarget.upperArm.target.transform.position, handTargetPosition, forearmUpAxis, handTarget.upperArm.target.length, handTarget.forearm.target.length, handTarget.isLeft);
                handTarget.upperArm.target.confidence = armSensor.upperArm.confidence;
            }

            if (handTarget.forearm.target.confidence.rotation < 0.9F) {
                handTarget.forearm.target.transform.rotation = ArmMovements.ForearmRotationIK(handTarget.forearm.target.transform.position, handTargetPosition, forearmUpAxis, handTarget.isLeft);
                handTarget.forearm.target.confidence = armSensor.forearm.confidence;
            }

            handTarget.hand.target.transform.rotation = handTargetRotation;
            handTarget.hand.target.confidence.rotation = armSensor.hand.confidence.rotation;
        }

        protected void UpdateShoulder(ArmSensor armSensor) {
            if (handTarget.shoulder.target.transform == null)
                return;

            if (armSensor.shoulder.confidence.position > 0)
                handTarget.shoulder.target.transform.position = Target.ToVector3(armSensor.shoulder.position);
            if (armSensor.shoulder.confidence.rotation > 0)
                handTarget.shoulder.target.transform.rotation = Target.ToQuaternion(armSensor.shoulder.rotation);
            handTarget.shoulder.target.confidence = armSensor.upperArm.confidence;
        }

        protected virtual void UpdateUpperArm(ArmSensor armSensor) {
            if (handTarget.upperArm.target.transform != null) {
                if (armSensor.upperArm.confidence.position > 0)
                    handTarget.upperArm.target.transform.position = Target.ToVector3(armSensor.upperArm.position);
                else
                    handTarget.upperArm.target.transform.position = handTarget.shoulder.target.transform.position + handTarget.shoulder.target.transform.rotation * handTarget.outward * handTarget.shoulder.bone.length;

                if (armSensor.upperArm.confidence.rotation > 0)
                    handTarget.upperArm.target.transform.rotation = Target.ToQuaternion(armSensor.upperArm.rotation);

                handTarget.upperArm.target.confidence = armSensor.upperArm.confidence;
            }
        }

        protected virtual void UpdateForearm(ArmSensor armSensor) {
            if (handTarget.forearm.target.transform != null) {
                if (armSensor.forearm.confidence.position > 0)
                    handTarget.forearm.target.transform.position = Target.ToVector3(armSensor.forearm.position);
                else
                    handTarget.forearm.target.transform.position = handTarget.upperArm.target.transform.position + handTarget.upperArm.target.transform.rotation * handTarget.outward * handTarget.upperArm.bone.length;

                if (armSensor.forearm.confidence.rotation > 0)
                    handTarget.forearm.target.transform.rotation = Target.ToQuaternion(armSensor.forearm.rotation);

                handTarget.forearm.target.confidence = armSensor.forearm.confidence;
            }
        }

        protected virtual void UpdateHand(ArmSensor armSensor) {
            if (handTarget.hand.target.transform != null) {
                if (armSensor.hand.confidence.position > 0 && armSensor.hand.confidence.position >= handTarget.hand.target.confidence.position) {
                    handTarget.hand.target.transform.position = Target.ToVector3(armSensor.hand.position);
                    handTarget.hand.target.confidence.position = armSensor.hand.confidence.position;
                } else if (handTarget.hand.target.confidence.position == 0) // Hmm. I could insert the arm model here when confidence.rotation > 0.5F for example!
                    handTarget.hand.target.transform.position = handTarget.forearm.target.transform.position + handTarget.forearm.target.transform.rotation * handTarget.outward * handTarget.forearm.bone.length;

                if (armSensor.hand.confidence.rotation > 0 && armSensor.hand.confidence.rotation >= handTarget.hand.target.confidence.rotation) {
                    handTarget.hand.target.transform.rotation = Target.ToQuaternion(armSensor.hand.rotation);
                    handTarget.hand.target.confidence.rotation = armSensor.hand.confidence.rotation;
                }
            }
        }
        protected virtual void UpdateHandTargetTransform(ArmSensor armSensor) {
            if (handTarget.hand.target.transform != null) {
                if (armSensor.hand.confidence.rotation > 0 && armSensor.hand.confidence.rotation >= handTarget.hand.target.confidence.rotation) {
                    handTarget.hand.target.transform.rotation = sensorTransform.rotation * sensor2TargetRotation;
                    handTarget.hand.target.confidence.rotation = armSensor.hand.confidence.rotation;
                }
                if (armSensor.hand.confidence.position > 0 && armSensor.hand.confidence.position >= handTarget.hand.target.confidence.position) {
                    handTarget.hand.target.transform.position = sensorTransform.position + handTarget.hand.target.transform.rotation * sensor2TargetPosition;
                    handTarget.hand.target.confidence.position = armSensor.hand.confidence.position;
                } else if (handTarget.hand.target.confidence.position == 0) // Hmm. I could insert the arm model here when confidence.rotation > 0.5F for example!
                    handTarget.hand.target.transform.position = handTarget.forearm.target.transform.position + handTarget.forearm.target.transform.rotation * handTarget.outward * handTarget.forearm.bone.length;

            }
        }

        protected virtual void UpdateFingers(ArmSensor armSensor) {
            for (int i = 0; i < (int)Finger.Count; i++) {
                UpdateFinger(armSensor.fingers[i], i);
            }
        }

        private void UpdateFinger(ArmSensor.Finger fingerSensor, int i) {
            Transform proximalTarget = handTarget.fingers.allFingers[i].proximal.target.transform;
            proximalTarget.rotation = proximalTarget.parent.rotation * Target.ToQuaternion(fingerSensor.proximal.rotation);

            Transform intermediateTarget = handTarget.fingers.allFingers[i].intermediate.target.transform;
            intermediateTarget.rotation = intermediateTarget.parent.rotation * Target.ToQuaternion(fingerSensor.intermediate.rotation);

            Transform distalTarget = handTarget.fingers.allFingers[i].distal.target.transform;
            distalTarget.rotation = distalTarget.parent.rotation * Target.ToQuaternion(fingerSensor.distal.rotation);

            handTarget.DetermineFingerCurl((Finger)i);
        }
        #endregion

        public float ArmConfidence(ArmSensor armSensor) {
            float armOrientationsConfidence =
                //armSensor.shoulder.confidence.rotation *
                armSensor.upperArm.confidence.rotation *
                armSensor.forearm.confidence.rotation;
            return armOrientationsConfidence;
        }


        public virtual void Vibrate(float length, float strength) {
        }
    }

    public class UnityArmController : UnityArmSensor {
        protected Sensor.ID sensorID;
        protected Controller controllerInput;
        public ArmController controller;

        public override Status status {
            get {
                if (controller == null)
                    return Status.Unavailable;
                else
                    return controller.status;
            }
            set {
                if (controller != null)
                    controller.status = value;
            }
        }

        #region Start
        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            sensorID = handTarget.isLeft ? Sensor.ID.LeftHand : Sensor.ID.RightHand;
            controllerInput = Controllers.GetController(0);
        }
        #endregion

        #region Update
        public override void Update() {
            if (tracker == null || !tracker.enabled || !enabled)
                return;

            controller.Update();
            if (controller.status != Status.Tracking)
                return;

            UpdateSensorTransform(controller);
            UpdateTargetTransform();
        }

        protected void UpdateInput(Controller controller, ArmController armController) {
            if (handTarget.isLeft)
                SetControllerInput(controller.left, armController);
            else
                SetControllerInput(controller.right, armController);
        }

        protected void SetControllerInput(ControllerSide controllerSide, ArmController armController) {
            controllerSide.stickHorizontal += armController.input.stickHorizontal;
            controllerSide.stickVertical += armController.input.stickVertical;
            controllerSide.stickButton |= armController.input.stickPress;

            //controllerSide.up |= armController.input.up;
            //controllerSide.down |= armController.input.down;
            //controllerSide.left |= armController.input.left;
            //controllerSide.right |= armController.input.right;

            controllerSide.buttons[0] |= armController.input.buttons[0];
            controllerSide.buttons[1] |= armController.input.buttons[1];
            controllerSide.buttons[2] |= armController.input.buttons[2];
            controllerSide.buttons[3] |= armController.input.buttons[3];

            controllerSide.trigger1 += armController.input.trigger1;
            controllerSide.trigger2 += armController.input.trigger2;

            controllerSide.option |= armController.input.option;
        }
        #endregion
    }

    public class UnityTorsoSensor : UnityHumanoidSensor {
        //protected HipsTarget hipsTarget;
        protected HipsTarget hipsTarget {
            get { return (HipsTarget)target;  }
        }
        protected new TorsoSensor sensor;

        #region Start
        public virtual void Init(HipsTarget hipsTarget) {
            target = hipsTarget;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            target = targetTransform.GetComponent<HipsTarget>();
        }

#if UNITY_EDITOR
        public void InitController(SerializedProperty sensorProp, HipsTarget hipsTarget) {
            if (sensorProp == null)
                return;

            Init(hipsTarget);

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = sensorTransform;

            SerializedProperty targetProp = sensorProp.FindPropertyRelative("target");
            targetProp.objectReferenceValue = target;

            if (!tracker.enabled || !enabled)
                return;

            CheckSensorTransform();
            sensorTransformProp.objectReferenceValue = sensorTransform;

            ShowSensor(hipsTarget.humanoid.showRealObjects && hipsTarget.showRealObjects);

            SerializedProperty sensor2TargetPositionProp = sensorProp.FindPropertyRelative("sensor2TargetPosition");
            sensor2TargetPositionProp.vector3Value = sensor2TargetPosition;
            SerializedProperty sensor2TargetRotationProp = sensorProp.FindPropertyRelative("sensor2TargetRotation");
            sensor2TargetRotationProp.quaternionValue = sensor2TargetRotation;
        }

        public void RemoveController(SerializedProperty sensorProp) {
            if (sensorProp == null)
                return;

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = null;
        }
#endif

        protected virtual void CreateSensorTransform(string resourceName, Vector3 sensor2TargetPosition, Quaternion sensor2TargetRotation) {
            CreateSensorTransform(hipsTarget.hips.target.transform, resourceName, sensor2TargetPosition, sensor2TargetRotation);
        }
        #endregion
        
        #region Update
        /*
        protected virtual void UpdateHipsTarget(HumanoidTarget.TargetTransform target, Transform sensorTransform) {
            if (target.transform == null || sensorTransform == null)
                return;

            target.transform.rotation = GetTargetRotation(sensorTransform);
            target.confidence.rotation = 0.5F;

            target.transform.position = GetHipsTargetPosition(sensorTransform);
            target.confidence.position = 0.5F;
        }

        protected virtual void UpdateHipsTarget(HumanoidTarget.TargetTransform target, SensorComponent sensorComponent) {
            if (target.transform == null || sensorComponent.rotationConfidence + sensorComponent.positionConfidence <= 0)
                return;

            target.transform.rotation = GetTargetRotation(sensorComponent.transform);
            target.confidence.rotation = sensorComponent.rotationConfidence;

            target.transform.position = GetHipsTargetPosition(sensorComponent.transform);
            target.confidence.position = sensorComponent.positionConfidence;
        }

        protected Vector3 GetHipsTargetPosition(Transform sensorTransform) {
            Vector3 targetPosition = sensorTransform.position + sensorTransform.rotation * sensor2TargetRotation * sensor2TargetPosition;
            return targetPosition;
        }

        protected Quaternion GetTargetRotation(Transform sensorTransform) {
            Quaternion targetRotation = sensorTransform.rotation * sensor2TargetRotation;
            return targetRotation;
        }
        */
        #endregion
    }

    public class UnityLegSensor : UnityHumanoidSensor {
        protected FootTarget footTarget {
            get { return (FootTarget)target; }
        }
        protected new LegSensor sensor;

        #region Start
        public virtual void Init(FootTarget footTarget) {
            target = footTarget;
        }

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);
            target = targetTransform.GetComponent<FootTarget>();
        }

#if UNITY_EDITOR
        public void InitController(SerializedProperty sensorProp, FootTarget footTarget) {
            if (sensorProp == null)
                return;

            Init(footTarget);

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = sensorTransform;

            SerializedProperty targetProp = sensorProp.FindPropertyRelative("target");
            targetProp.objectReferenceValue = target;

            if (!tracker.enabled || !enabled)
                return;

            CheckSensorTransform();
            sensorTransformProp.objectReferenceValue = sensorTransform;

            ShowSensor(footTarget.humanoid.showRealObjects && footTarget.showRealObjects);

            SerializedProperty sensor2TargetPositionProp = sensorProp.FindPropertyRelative("sensor2TargetPosition");
            sensor2TargetPositionProp.vector3Value = sensor2TargetPosition;
            SerializedProperty sensor2TargetRotationProp = sensorProp.FindPropertyRelative("sensor2TargetRotation");
            sensor2TargetRotationProp.quaternionValue = sensor2TargetRotation;
        }

        public void RemoveController(SerializedProperty sensorProp) {
            if (sensorProp == null)
                return;

            SerializedProperty sensorTransformProp = sensorProp.FindPropertyRelative("sensorTransform");
            sensorTransformProp.objectReferenceValue = null;
        }
#endif
        protected virtual void CreateSensorTransform(string resourceName, Vector3 sensor2TargetPosition, Quaternion sensor2TargetRotation) {
            CreateSensorTransform(footTarget.foot.target.transform, resourceName, sensor2TargetPosition, sensor2TargetRotation);
        }
        #endregion

        protected virtual void UpdateUpperLeg(LegSensor legSensor) {
            if (footTarget.upperLeg.target.transform != null) {
                if (legSensor.upperLeg.confidence.position > 0)
                    footTarget.upperLeg.target.transform.position = HumanoidTarget.ToVector3(legSensor.upperLeg.position);
                //else
                // footTarget.upperLeg.target.transform.position = footTarget.shoulder.target.transform.position + footTarget.shoulder.target.transform.rotation * footTarget.outward * footTarget.shoulder.bone.length;

                if (legSensor.upperLeg.confidence.rotation > 0)
                    footTarget.upperLeg.target.transform.rotation = HumanoidTarget.ToQuaternion(legSensor.upperLeg.rotation);

                footTarget.upperLeg.target.confidence = legSensor.upperLeg.confidence;
            }
        }

        protected virtual void UpdateLowerLeg(LegSensor legSensor) {
            if (footTarget.lowerLeg.target.transform == null)
                return;

            if (legSensor.lowerLeg.confidence.position > 0)
                footTarget.lowerLeg.target.transform.position = HumanoidTarget.ToVector3(legSensor.lowerLeg.position);
            else
                footTarget.lowerLeg.target.transform.position = footTarget.upperLeg.target.transform.position + footTarget.upperLeg.target.transform.rotation * Vector3.down * footTarget.upperLeg.bone.length;

            if (legSensor.lowerLeg.confidence.rotation > 0)
                footTarget.lowerLeg.target.transform.rotation = HumanoidTarget.ToQuaternion(legSensor.lowerLeg.rotation);

            footTarget.lowerLeg.target.confidence = legSensor.lowerLeg.confidence;
        }

        protected virtual void UpdateFoot(LegSensor legSensor) {
            if (footTarget.foot.target.transform != null) {
                if (legSensor.foot.confidence.position > 0)
                    footTarget.foot.target.transform.position = HumanoidTarget.ToVector3(legSensor.foot.position);
                else
                    footTarget.foot.target.transform.position = footTarget.lowerLeg.target.transform.position + footTarget.lowerLeg.target.transform.rotation * Vector3.down * footTarget.lowerLeg.bone.length;

                if (legSensor.foot.confidence.rotation > 0)
                    footTarget.foot.target.transform.rotation = HumanoidTarget.ToQuaternion(legSensor.foot.rotation);

                footTarget.foot.target.confidence = legSensor.foot.confidence;
            }
        }
    }
}
