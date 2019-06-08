using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    [HelpURL("https://passervr.com/documentation/humanoid-control/head-target/")]
    public class HeadTarget : HumanoidTarget {

        public HeadTarget() {
            neck = new TargetedNeckBone(this);
            head = new TargetedHeadBone(this);
#if hFACE
            face = new FaceTarget(this);
#endif
        }

        #region Limitations
        public const float maxNeckAngle = 80;
        public const float maxHeadAngle = 50;

        // for future use
        public static readonly float neckTurnRatio = 0.65F;
        public static readonly Vector3 minHeadAngles = new Vector3(0, 0, 0);
        public static readonly Vector3 maxHeadAngles = new Vector3(0, 0, 0);

        public static readonly Vector3 minNeckAngles = new Vector3(-55, -70, -35);
        public static readonly Vector3 maxNeckAngles = new Vector3(80, 70, 35);

        public static readonly Vector minNeckAngles2 = new Vector(-55, -70, 0);
        public static readonly Vector maxNeckAngles2 = new Vector(80, 70, 0);
        #endregion

        #region Sensors
        public UnityVRHead unityVRHead = new UnityVRHead();

        private HeadPredictor headPredictor = new HeadPredictor();

        public HeadAnimator headAnimator = new HeadAnimator();
        public override void EnableAnimator(bool enabled) {
            headAnimator.enabled = enabled;
        }

#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public SteamVRHead steamVR = new SteamVRHead();
#if hVIVETRACKER
        public ViveTrackerHead viveTracker = new ViveTrackerHead();
#endif
#endif
#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
        public OculusHead oculus = new OculusHead();
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
        public WindowsMRHead mixedReality = new WindowsMRHead();
#endif
#if hVRTK
        public VrtkHead vrtk = new VrtkHead();
#endif
#if hNEURON
        public PerceptionNeuronHead neuron = new PerceptionNeuronHead();
#endif
#if hKINECT1
        public Kinect1Head kinect1 = new Kinect1Head();
#endif
#if hKINECT2
        public Kinect2Head kinect = new Kinect2Head();
#endif
#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)
        public AstraHead astra = new AstraHead();
#endif
#if hREALSENSE
        public IntelRealsenseHead realsense = new IntelRealsenseHead();
#endif
#if hOPTITRACK
        public OptitrackHead optitrack = new OptitrackHead();
#endif

#if hFACE
        public MicrophoneHead microphone = new MicrophoneHead();
#if hKINECT2
        public Kinect2Face kinectFace;
#endif
#if hTOBII
        public TobiiHead tobiiHead = new TobiiHead();
#endif
#if hPUPIL
        public Tracking.Pupil.Head pupil = new Tracking.Pupil.Head();
#endif
#if hDLIB
        public Tracking.Dlib.Head dlib = new Tracking.Dlib.Head();
#endif
#endif

        public UnityHeadSensor[] sensors;

        public override void InitSensors() {
            if (unityVRHead.tracker == null)
                unityVRHead.tracker = new Tracker();

            if (sensors == null) {
                sensors = new UnityHeadSensor[] {
                    headPredictor,
#if hOPTITRACK
                    optitrack,
#endif
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                    steamVR,
#if hVIVETRACKER
                    viveTracker,
#endif
#endif
#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                    oculus,
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
                    mixedReality,
#endif
#if hVRTK
                    vrtk,
#endif
#if hKINECT1
                    kinect1,
#endif
#if hKINECT2
                    kinect,
#endif
#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                    astra,
#endif
#if hNEURON
                    neuron,
#endif
#if hREALSENSE
                    realsense,
#endif

#if hFACE
                    microphone,
#if hTOBII
                    tobiiHead,
#endif
#if hPUPIL
                    pupil,
#endif
#if hDLIB
                    dlib,
#endif
#endif
                    unityVRHead,
                    headAnimator,
                };
            }
        }

        public override void StartSensors() {
            headAnimator.Start(humanoid, this.transform);

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Start(humanoid, transform);

            HeadCollisionHandler.AddHeadCollider(this.gameObject);
            HeadCollisionHandler headHandler = this.gameObject.AddComponent<HeadCollisionHandler>();
            // shouldn't this be attached to the target.head.bone?
            headHandler.Initialize(humanoid);

            //SphereCollider sc = HeadCollisionHandler.AddHeadCollider(this.gameObject);
            // another one for receiving raycasting collisions
            //sc.isTrigger = false;
#if hFACE
            face.StartSensors();
#endif
        }

        protected override void UpdateSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                sensors[i].Update();
            }
        }
        #endregion

        #region SubTargets
        public override TargetedBone main {
            get { return head; }
        }

        #region Head
        public TargetedHeadBone head = null;

        [System.Serializable]
        public class TargetedHeadBone : TargetedBone {
            private HeadTarget headTarget;

            public TargetedHeadBone(HeadTarget headTarget) {
                this.headTarget = headTarget;

                boneId = Bone.Head;

                bone.minAngles = minHeadAngles;
                bone.maxAngles = maxHeadAngles;
                bone.length = 0.1F;
            }

            public override void Init() {
                parent = headTarget.neck;
                nextBone = null;
            }

            public override Quaternion DetermineRotation() {
                if (headTarget == null)
                    return Quaternion.identity;

                Vector3 headUp = Vector3.up;
                Vector3 headForward = Vector3.forward;

                if (bone.transform.childCount == 1)
                    headUp = bone.transform.GetChild(0).position - bone.transform.position;
                //else if (parent != null)
                //    headUp = parent.DetermineRotation() * Vector3.up;

                if (target.transform != null)
                    headForward = headTarget.humanoid.hipsTarget.transform.forward;

                Quaternion headRotation = Quaternion.LookRotation(headUp, -headForward) * Quaternion.Euler(90, 0, 0);
                //bone.baseRotation = headRotation;
                // this is only true when parent.baseRotation == identity
                //bone.basePosition = headTarget.head.bone.transform.position - parent.bone.transform.position;
                return headRotation;
            }

            public override float GetTension() {
                Quaternion restRotation = headTarget.neck.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }

            protected override void DetermineBasePosition() {
                if (target.basePosition.sqrMagnitude != 0)
                    // Base Position is already determined
                    return;

                Transform basePositionReference = GetBasePositionReference();
                target.basePosition = basePositionReference.InverseTransformPoint(target.transform.position);
            }

            public override Vector3 TargetBasePosition() {
                Transform basePositionReference = GetBasePositionReference();
                return basePositionReference.TransformPoint(target.basePosition);
            }

            private Transform GetBasePositionReference() {
                return headTarget.humanoid.hipsTarget.hips.target.transform.parent;
            }
        }
        #endregion

        #region Neck
        public TargetedNeckBone neck = null;

        [System.Serializable]
        public class TargetedNeckBone : TargetedBone {
            public HeadTarget headTarget;

            public TargetedNeckBone(HeadTarget headTarget) {
                this.headTarget = headTarget;
                boneId = Bone.Neck;

                bone.minAngles = minNeckAngles;
                bone.maxAngles = maxNeckAngles;
            }

            public override void Init() {
                if (headTarget.humanoid == null || headTarget.humanoid.hipsTarget == null)
                    parent = null;
                else
                    parent = (headTarget.humanoid.hipsTarget.chest.bone.transform != null) ?
                        (TargetedBone)headTarget.humanoid.hipsTarget.chest :
                        (TargetedBone)headTarget.humanoid.hipsTarget.hips;

                nextBone = headTarget.head;
            }

            public override Quaternion DetermineRotation() {
                if (headTarget == null)
                    return Quaternion.identity;

                Vector3 neckUp = Vector3.up;
                if (nextBone != null && nextBone.bone.transform != null)
                    neckUp = nextBone.bone.transform.position - bone.transform.position;
                Quaternion neckRotation = Quaternion.LookRotation(neckUp, -headTarget.humanoid.hipsTarget.transform.forward) * Quaternion.Euler(90, 0, 0);

                //bone.baseRotation = neckRotation;
                return neckRotation;
            }


            public override float GetTension() {
                Quaternion restRotation = headTarget.humanoid.hipsTarget.chest.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        private void InitSubTargets() {
            neck.Init();
            head.Init();
        }

        private void SetTargetPositionsToAvatar() {
            neck.SetTargetPositionToAvatar();
            head.SetTargetPositionToAvatar();
        }

        private void DoMeasurements() {
            neck.DoMeasurements();
            head.DoMeasurements();
        }

#if hFACE
        public FaceTarget face = null;
#endif

        public float smileValue;
        public float puckerValue;
        public float frownValue;

        public float stress;

        public float audioEnergy;

        public Vector3 lookDirection = Vector3.forward;
        public Vector3 localLookDirection = Vector3.forward;

        public void LookTo(Vector3 position, float confidence) {
            Vector3 eyePosition = GetEyePosition();

            Vector3 direction = (position - eyePosition).normalized;
            SetLookDirection(direction, confidence);
        }

        public void SetLookDirection(Vector3 direction, float confidence) {
            lookDirection = direction;
            localLookDirection = humanoid.hipsTarget.hips.target.transform.InverseTransformDirection(direction);
        }
        #endregion

        #region Configuration
        public Vector3 neck2eyes;
        public Vector3 head2eyes;

        public override Transform GetDefaultTarget(HumanoidControl humanoid) {
            Transform targetTransform = null;
            if (humanoid != null)
                GetDefaultHead(humanoid.targetsRig, ref targetTransform);
            return targetTransform;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        public static HeadTarget CreateTarget(HumanoidTarget oldTarget) {
            GameObject targetObject = new GameObject("Head Target");
            Transform targetTransform = targetObject.transform;
            HumanoidControl humanoid = oldTarget.humanoid;

            RemoveFirstPersonCamara((HeadTarget)oldTarget);

            targetTransform.parent = oldTarget.humanoid.transform;
            targetTransform.position = oldTarget.transform.position;
            targetTransform.rotation = oldTarget.transform.rotation;

            HeadTarget headTarget = targetTransform.gameObject.AddComponent<HeadTarget>();
            headTarget.humanoid = humanoid;
            humanoid.headTarget = headTarget;
#if hFACE
            headTarget.face.headTarget = headTarget;
#endif

            headTarget.RetrieveBones();
            headTarget.InitAvatar();
            headTarget.MatchTargetsToAvatar();

            return headTarget;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        // Changes the target transform used for this head target
        // Generates a new headtarget component, so parameters will be lost if transform is changed
        public static HeadTarget SetTarget(HumanoidControl humanoid, Transform targetTransform) {
            HeadTarget currentHeadTarget = humanoid.headTarget;
            if (targetTransform == currentHeadTarget.transform)
                return currentHeadTarget;

            RemoveFirstPersonCamara(currentHeadTarget);

            GetDefaultHead(humanoid.targetsRig, ref targetTransform);
            if (targetTransform == null)
                return currentHeadTarget;

            HeadTarget headTarget = targetTransform.GetComponent<HeadTarget>();
            if (headTarget == null)
                headTarget = targetTransform.gameObject.AddComponent<HeadTarget>();

            headTarget.NewComponent(humanoid);
            headTarget.InitComponent();

            return headTarget;
        }

        public void RetrieveBones() {
            neck.RetrieveBones(humanoid);
            head.RetrieveBones(humanoid);
#if hFACE
            face.RetrieveBones(this);
#endif
        }

        public static void GetDefaultNeck(Animator rig, ref Transform boneTransform) {
            GetDefaultBone(rig, ref boneTransform, HumanBodyBones.Neck, "Neck", "neck");
            if (boneTransform == null) {
                GetDefaultBone(rig, ref boneTransform, HumanBodyBones.Head, "Head", "head");
            }
        }
        public static void GetDefaultHead(Animator rig, ref Transform boneTransform) {
            GetDefaultBone(rig, ref boneTransform, HumanBodyBones.Head, "Head", "head");
        }

        public static void ClearBones(HeadTarget headTarget) {
            headTarget.neck.bone.transform = null;
            headTarget.head.bone.transform = null;
        }
        #endregion

        #region Settings
        public bool collisionFader = false;
        //public bool jointLimitations = true;

        public enum InteractionType {
            None,
            Gazing
        }

        #region Virtual3D
        public bool virtual3d = false;
        [HideInInspector]

        public Transform screenTransform;
        #endregion
        #endregion

        public SkinnedMeshRenderer smRenderer;
        public Rigidbody headRigidbody;
        public HeadMovements headMovements = new HeadMovements();

        #region Init
        /// <summary>Is the head target initialized?</summary>
        public static bool IsInitialized(HumanoidControl humanoid) {
            if (humanoid.headTarget == null || humanoid.headTarget.humanoid == null)
                return false;
            if (humanoid.headTarget.head.target.transform == null)
                return false;
            if (humanoid.headTarget.head.bone.transform == null && humanoid.headTarget.neck.bone.transform == null)
                return false;
            return true;
        }

        private void Reset() {
            humanoid = GetHumanoid();
            if (humanoid == null)
                return;

            NewComponent(humanoid);

            neck.bone.maxAngle = maxNeckAngle;
            head.bone.maxAngle = maxHeadAngle;
        }

        private HumanoidControl GetHumanoid() {
            // This does not work for prefabs
            HumanoidControl[] humanoids = FindObjectsOfType<HumanoidControl>();

            for (int i = 0; i < humanoids.Length; i++) {
                if (humanoids[i].headTarget != null && humanoids[i].headTarget.transform == this.transform)
                    return humanoids[i];
            }

            return null;
        }

        public override void InitAvatar() {
            InitSubTargets();

            neck.DoMeasurements();
            head.DoMeasurements();

            neck2eyes = GetNeckEyeDelta();
            head2eyes = GetHeadEyeDelta();

#if hFACE
            face.InitAvatar(this);
#endif
        }

        public override void NewComponent(HumanoidControl _humanoid) { }

        public override void InitComponent() {
            if (humanoid == null)
                return;

#if hFACE
            face.InitComponent();
#endif
        }

        public override void StartTarget() {
            InitSensors();
#if hSTEAMVR || hOCULUS
            unityVRHead.tracker.enabled = UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.None;
#endif
            neck2eyes = GetNeckEyeDelta();
            head2eyes = GetHeadEyeDelta();

            headMovements.Start(humanoid, this);
        }

        /// <summary> Checks whether the humanoid has an HeadTargetand adds one if none has been found</summary>
        /// <param name="humanoid">The humanoid to check</param>
        public static void DetermineTarget(HumanoidControl humanoid) {
            HeadTarget headTarget = humanoid.headTarget;

            if (headTarget == null) {
                Transform headTargetTransform = humanoid.targetsRig.GetBoneTransform(HumanBodyBones.Head);
                if (headTargetTransform == null) {
                    Debug.LogError("Could not find head bone in targets rig");
                    return;
                }

                headTarget = headTargetTransform.GetComponent<HeadTarget>();
                if (headTarget == null) {
                    headTarget = headTargetTransform.gameObject.AddComponent<HeadTarget>();
                    headTarget.humanoid = humanoid;

                    //headTarget.RetrieveBones();
                    //headTarget.InitAvatar();
                    //headTarget.MatchTargetsToAvatar();
                }
                humanoid.headTarget = headTarget;
            }

            humanoid.headTarget = headTarget;
        }

        private static void RemoveFirstPersonCamara(HeadTarget headTarget) {
            Camera cam = headTarget.GetComponentInChildren<Camera>();
            if (cam != null) {
                if (cam.gameObject.name == "First Person Camera") {
                    DestroyImmediate(cam.gameObject);
                    return;
                }
                DestroyImmediate(cam, true);
            }
            AudioListener listener = headTarget.GetComponentInChildren<AudioListener>();
            if (listener != null)
                DestroyImmediate(listener, true);
        }

        public override void MatchTargetsToAvatar() {
            //base.MatchTargetsToAvatar();
            neck.MatchTargetToAvatar();
            head.MatchTargetToAvatar();
            if (head.bone.transform != null && transform != null && head.target.transform != null) {
                transform.position = head.target.transform.position;
                transform.rotation = head.target.transform.rotation;
            }
#if hFACE
            face.MatchTargetsToAvatar();
#endif
        }
        #endregion

        #region Update
        /// <summary>Update all head sensors</summary>
        public override void UpdateTarget() {
            //base.UpdateTarget();

            neck.target.confidence.Degrade();
            head.target.confidence.Degrade();

            UpdateSensors();
#if hFACE
            face.UpdateTarget();
#endif
        }

        /// <summary>Updates the avatar bones based on the current target rig</summary>
        public override void UpdateMovements(HumanoidControl humanoid) {
            if (humanoid.calculateBodyPose) {
                HeadMovements.Update(this);
#if hFACE
                face.UpdateMovements();
#endif
            }
        }

        /// <summary>Copy the head target to the target rig</summary>
        public override void CopyTargetToRig() {
            if (Application.isPlaying &&
                humanoid.animatorEnabled && humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            if (head.target.transform != null && transform != head.target.transform) {
                head.target.transform.position = transform.position;
                head.target.transform.rotation = transform.rotation;
            }
        }

        /// <summary>Copy the target rig head bone to the head target</summary>
        public override void CopyRigToTarget() {
            if (head.target.transform != null && transform != head.target.transform) {
                transform.position = head.target.transform.position;
                transform.rotation = head.target.transform.rotation;
            }
        }

        /// <summary>Update the sensor locations based on the head target</summary>
        public void UpdateSensorsFromTarget() {
            if (sensors == null)
                return;

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].UpdateSensorTransformFromTarget(this.transform);
        }

        /// <summary>Draw the target rig </summary>
        public override void DrawTargetRig(HumanoidControl humanoid) {
            if (this != humanoid.headTarget)
                return;

            DrawTarget(neck.target.confidence, neck.target.transform, Vector3.up, 0.1F);
            DrawTarget(head.target.confidence, head.target.transform, Vector3.up, 0.1F);
#if hFACE
            if (face != null)
                face.DrawTargetRig();
#endif
        }

        /// <summary>Draw the avatar rig</summary>
        public override void DrawAvatarRig(HumanoidControl humanoid) {
            if (this != humanoid.headTarget)
                return;

            if (neck.bone.transform != null)
                Debug.DrawRay(neck.bone.transform.position, neck.bone.targetRotation * Vector3.up * neck.bone.length, Color.cyan);
            if (head.bone.transform != null)
                Debug.DrawRay(head.bone.transform.position, head.bone.targetRotation * Vector3.up * head.bone.length, Color.cyan);
#if hFACE
            if (face != null)
                face.DrawAvatarRig();
#endif
        }
        #endregion

        #region HeadPose
        private static float maxXangle = 60;
        private static float maxYangle = 70;

        /// <summary>Sets the rotation of the head around the X axis</summary>
        public void RotationX(float angle) {
            //Vector3 angles = head.target.transform.localEulerAngles;
            Vector3 angles = (transform.rotation * Quaternion.Inverse(humanoid.transform.rotation)).eulerAngles;
            float xAngle = angle * maxXangle;
            //head.target.transform.localRotation = Quaternion.Euler(xAngle, angles.y, angles.z);
            transform.rotation = humanoid.transform.rotation * Quaternion.Euler(xAngle, angles.y, angles.z);
        }

        /// <summary>Sets the rotation of the head around the Y axis</summary>
        public void RotationY(float angle) {
            //Vector3 angles = head.target.transform.localEulerAngles;
            Vector3 angles = (transform.rotation * Quaternion.Inverse(humanoid.transform.rotation)).eulerAngles;
            float yAngle = angle * maxYangle;
            //head.target.transform.localRotation = Quaternion.Euler(angles.x, yAngle, angles.z);
            transform.rotation = humanoid.transform.rotation * Quaternion.Euler(angles.x, yAngle, angles.z);
        }
        #endregion

        #region Tools

        /// <summary>Gets the eye position in world coordinates</summary>
        public Vector3 GetEyePosition() {
            if (Application.isPlaying && gameObject != null) {
                Camera camera = gameObject.GetComponentInChildren<Camera>();
                if (camera != null)
                    return camera.transform.position;
            }

#if hFACE
            if (neck.bone.transform != null && face.leftEye.bone.transform != null && face.rightEye.bone.transform != null) {
                Vector3 centerEyePosition = (face.leftEye.bone.transform.transform.position + face.rightEye.bone.transform.position) / 2;
                return centerEyePosition;
            }
#else
            if (humanoid.avatarRig != null) {
                Transform leftEye = humanoid.avatarRig.GetBoneTransform(HumanBodyBones.LeftEye);
                Transform rightEye = humanoid.avatarRig.GetBoneTransform(HumanBodyBones.RightEye);
                if (leftEye != null && rightEye != null) {
                    Vector3 centerEyePosition = (leftEye.position + rightEye.position) / 2;
                    return centerEyePosition;
                }
            }
#endif
            if (this != null && gameObject != null) {
                Camera camera = gameObject.GetComponentInChildren<Camera>();
                if (camera != null)
                    return camera.transform.position;
            }

            if (neck.bone.transform != null)
                return neck.bone.transform.position + neck.target.transform.rotation * new Vector3(0, 0.13F, 0.13F);
            else
                return neck.target.transform.position + neck.target.transform.rotation * new Vector3(0, 0.13F, 0.13F);
        }

        /// <summary>Gets the local eye position relative to the neck bone</summary>
        public Vector3 GetNeckEyeDelta() {
            Vector3 eyePosition = GetEyePosition();
            Vector3 worldNeckEyeDelta = (neck.bone.transform != null) ?
                (eyePosition - neck.bone.transform.position) :
                (eyePosition - neck.target.transform.position);
            Vector3 localNeckEyeDelta = Quaternion.AngleAxis(-neck.target.transform.eulerAngles.y, Vector3.up) * worldNeckEyeDelta;
            return localNeckEyeDelta;
        }

        /// <summary>Gets the local eye position realtive to the head bone</summary>
        public Vector3 GetHeadEyeDelta() {
            Vector3 eyePosition = GetEyePosition();
            Vector3 worldHeadEyeDelta = (neck.bone.transform != null) ?
                (eyePosition - head.bone.transform.position) :
                (eyePosition - head.target.transform.position);
            Vector3 localHeadEyeDelta = Quaternion.AngleAxis(-head.target.transform.eulerAngles.y, Vector3.up) * worldHeadEyeDelta;
            return localHeadEyeDelta;
        }

        /// <summary>Gets the local head position relative to the neck bone</summary>
        public Vector3 GetNeckHeadDelta() {
            if (neck.target.transform != null && head.target.transform != null) {
                Vector3 worldNeckHeadDelta = (head.target.transform.position - neck.target.transform.position);
                Vector3 localNeckHeadDelta = neck.target.transform.InverseTransformDirection(worldNeckHeadDelta);
                return localNeckHeadDelta;
            }

            return Vector3.zero;
        }

        //public Vector3 GetHeadNeckDelta() {
        //    if (neck.bone.transform != null && head.bone.transform != null) {
        //        Vector3 worldHeadNeckDelta = (neck.bone.transform.position - head.bone.transform.position);
        //        Vector3 localHeadNeckDelta = head.target.transform.InverseTransformDirection(worldHeadNeckDelta);
        //        return localHeadNeckDelta;
        //    }

        //    return Vector3.zero;
        //}
        public static SkinnedMeshRenderer[] FindAvatarMeshes(HumanoidControl humanoid) {
            if (humanoid.avatarRig == null)
                return new SkinnedMeshRenderer[0];

            Transform avatar = humanoid.avatarRig.transform;
            SkinnedMeshRenderer[] renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            Mesh[] meshes = new Mesh[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
                meshes[i] = renderers[i].sharedMesh;
            return renderers;
        }

        public static string[] DistillAvatarMeshNames(SkinnedMeshRenderer[] meshes) {
            string[] names = new string[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
                names[i] = meshes[i].name;

            return names;
        }

        public static int FindMeshWithBlendshapes(SkinnedMeshRenderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].sharedMesh != null && renderers[i].sharedMesh.blendShapeCount > 0)
                    return i;

            return 0;
        }

        public static int FindBlendshapemesh(SkinnedMeshRenderer[] renderers, SkinnedMeshRenderer renderer) {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] == renderer)
                    return i;

            return 0;
        }

        public static string[] GetBlendshapes(SkinnedMeshRenderer renderer) {
            if (renderer == null || renderer.sharedMesh == null)
                return new string[0];

            string[] blendShapes = new string[renderer.sharedMesh.blendShapeCount];
            for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++) {
                blendShapes[i] = renderer.sharedMesh.GetBlendShapeName(i);
            }
            return blendShapes;
        }

        public static void FindBlendshapeWith(string[] blendshapes, string namepart1, string namepart2, ref int blendshape) {
            for (int i = 0; i < blendshapes.Length; i++) {
                if (blendshapes[i].Contains(namepart1) && blendshapes[i].Contains(namepart2)) {
                    blendshape = i;
                    return;
                }
            }
        }

        #endregion
    }
}