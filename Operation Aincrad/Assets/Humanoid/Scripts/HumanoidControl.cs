using UnityEngine;
#if hREALSENSE

#endif

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;
#if hREALSENSE
    using Humanoid.Tracking.Realsense;
#endif

    public enum GameControllers {
        Xbox,
        PS4,
        Steelseries,
        GameSmart,
        JustKeyboard
    }

    public enum NetworkingSystems {
        None,
        UnityNetworking
#if hPHOTON1 || hPHOTON2
        , PhotonNetworking
#endif
    }

    [System.Serializable]
    [HelpURL("https://passervr.com/documentation/humanoid-control/humanoid-control-script/")]
    public class HumanoidControl : MonoBehaviour {
        public HeadTarget headTarget;
        public HandTarget leftHandTarget;
        public HandTarget rightHandTarget;
        public HipsTarget hipsTarget;
        public FootTarget leftFootTarget;
        public FootTarget rightFootTarget;

        public enum TargetId {
            Hips,
            Head,
            LeftHand,
            RightHand,
            LeftFoot,
            RightFoot
        }

        public Animator targetsRig;
        public bool showTargetRig = false;
        public float trackingNeckHeight {
            get {
                if (headTarget == null || headTarget.neck.target.transform == null)
                    return 0;

                return headTarget.neck.target.transform.position.y - transform.position.y;
            }
        }

        public Animator avatarRig;
        public bool showAvatarRig = true;

        public float avatarNeckHeight;

        public bool showMuscleTension = false;

        public bool calculateBodyPose = true;

        public bool gameControllerEnabled = true;
        public int gameControllerIndex = 0;
        public Controller controller;
        public GameControllers gameController;
        public static void SetControllerID(HumanoidControl humanoid, int controllerID) {
            if (humanoid.traditionalInput != null) {
                humanoid.gameControllerIndex = controllerID;
                humanoid.controller = humanoid.traditionalInput.SetControllerID(controllerID);
            }
        }

        public bool animatorEnabled = true;
        public RuntimeAnimatorController animatorController = null;

        public Humanoid.Pose pose;
        public bool editPose;

        public IHumanoidNetworking humanoidNetworking;
        public GameObject remoteAvatar;
        public bool isRemote = false;
        public int nwId;

        public int humanoidId = -1;

#region Settings
        [Tooltip("Health value between 0 and 100.")]
        public bool showRealObjects = true;
        public bool physics = true;
        public bool generateColliders = true;
        public bool haptics = false;
        public bool useGravity = true;
        public float stepOffset = 0.3F;

        public bool proximitySpeed = false;
        public float proximitySpeedRate = 0.8f;

        public enum ScalingType {
            None,
            SetHeightToAvatar,
            MoveHeightToAvatar,
            ScaleTrackingToAvatar,
            ScaleAvatarToTracking
        }
        public ScalingType scaling = ScalingType.MoveHeightToAvatar;
        public bool calibrateAtStart = false;
        public bool dontDestroyOnLoad = false;
#endregion

#region Init
        [HideInInspector]
        protected virtual void Awake() {
            //UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.transform.root);

            AddHumanoid();
            CheckTargetRig(this);
            avatarRig = GetAvatar();

            // Move the animator controller to the targets rig for proper animation support
            if (avatarRig != null && avatarRig.runtimeAnimatorController != null && targetsRig.runtimeAnimatorController == null) {
                targetsRig.runtimeAnimatorController = avatarRig.runtimeAnimatorController;
                avatarRig.runtimeAnimatorController = null;
                avatarRig.enabled = false;
            }

            DetermineTargets();
            InitTargets();
            NewTargetComponents();
            RetrieveBones();
            InitAvatar();


            avatarNeckHeight = GetAvatarNeckHeight();
            MatchTargetsToAvatar();

            AddCharacterColliders();

            StartTargets();

            InitTrackers();
            StartTrackers();

            StartSensors();
        }

#endregion

#region Avatar
        private float GetAvatarNeckHeight() {
            if (avatarRig == null)
                return headTarget.transform.localPosition.y;

            Transform avatarNeck = headTarget.neck.bone.transform; // avatarRig.GetBoneTransform(HumanBodyBones.Neck);
            if (avatarNeck != null) {
                float neckHeight = avatarNeck.position.y - avatarRig.transform.position.y;
                return neckHeight;
            }
            else
                return headTarget.transform.localPosition.y;
        }

        public void LocalChangeAvatar(GameObject avatarPrefab) {
            Animator animator = avatarPrefab.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid/* || !animator.avatar.isHuman*/) {
                Debug.LogWarning("Could not detect suitable avatar");
                return;
            }

            // bones of previous avatar are no longer valid
            HeadTarget.ClearBones(headTarget);
            HandTarget.ClearBones(leftHandTarget);
            HandTarget.ClearBones(rightHandTarget);

            if (avatarRig != null) {
                if (avatarRig.transform != this.transform) {
                    DestroyImmediate(avatarRig.gameObject, true);
                    //Destroy(avatarRig.gameObject);
                }
                else {
                    while (this.transform.childCount > 0) {
                        DestroyImmediate(this.transform.GetChild(0).gameObject);
                    }
                    DestroyImmediate(avatarRig);
                }
            }


            GameObject avatarObj = (GameObject)Instantiate(avatarPrefab, this.transform.position, this.transform.rotation);
            avatarObj.transform.SetParent(this.transform);
            avatarObj.transform.localPosition = Vector3.zero;

            // Remove camera from avatar
            Transform t = avatarObj.transform.FindDeepChild("First Person Camera");
            if (t != null)
                Destroy(t.gameObject);

            // Set targetRig to t-pose like
            //headTarget.neck.target.transform.localRotation = Quaternion.identity;

            CheckTargetRig(this);
            InitializeAvatar();

            Calibrate();
        }

        public void ChangeAvatar(GameObject fpAvatarPrefab) {
            ChangeAvatar(fpAvatarPrefab, fpAvatarPrefab);
        }

        public void ChangeAvatar(GameObject fpAvatarPrefab, GameObject tpAvatarPrefab) {
            remoteAvatar = tpAvatarPrefab;
            LocalChangeAvatar(fpAvatarPrefab);

            if (humanoidNetworking != null) {
                if (remoteAvatar != null)
                    humanoidNetworking.ChangeAvatar(this, remoteAvatar.name);
                else
                    humanoidNetworking.ChangeAvatar(this, fpAvatarPrefab.name);
            }
        }

        public void InitializeAvatar() {
            avatarRig = GetAvatar();

            // Move the animator controller to the targets rig for proper animation support
            if (avatarRig.runtimeAnimatorController != null && targetsRig.runtimeAnimatorController == null) {
                targetsRig.runtimeAnimatorController = avatarRig.runtimeAnimatorController;
                avatarRig.runtimeAnimatorController = null;
                avatarRig.gameObject.SetActive(false);
            }

            RetrieveBones();
            InitAvatar();
            MatchTargetsToAvatar();

            avatarNeckHeight = GetAvatarNeckHeight();
            // This will change the target rotations wrongly when changing avatars
            //MatchTargetsToAvatar();

            //InitTargetComponents();

            //InitTargets();
        }

        /// <summary>
        /// Analyses the avatar's properties requires for the movements
        /// </summary>
        public void InitAvatar() {
            hipsTarget.InitAvatar();
            headTarget.InitAvatar();
            leftHandTarget.InitAvatar();
            rightHandTarget.InitAvatar();
            leftFootTarget.InitAvatar();
            rightFootTarget.InitAvatar();
        }

        public void ScaleAvatarToTracking() {
            float neckHeight = headTarget.neck.target.transform.position.y - transform.position.y;
            ScaleAvatar(neckHeight / avatarNeckHeight);
        }

        private void ScaleAvatarToHeight(float height) {
            if (height <= 0)
                return;

            float neckHeight = 0.875F * height;
            ScaleAvatar(neckHeight / avatarNeckHeight);
        }

        private void ScaleAvatar(float scaleFactor) {
            avatarRig.transform.localScale *= scaleFactor;

            Quaternion leftForearmRotation = leftHandTarget.forearm.bone.transform.rotation * leftHandTarget.forearm.bone.toTargetRotation;
            leftHandTarget.hand.bone.transform.position = leftHandTarget.forearm.bone.transform.position + leftForearmRotation * leftHandTarget.outward * (leftHandTarget.forearm.bone.length * scaleFactor);

            Quaternion rightForearmRotation = rightHandTarget.forearm.bone.transform.rotation * rightHandTarget.forearm.bone.toTargetRotation;
            rightHandTarget.hand.bone.transform.position = rightHandTarget.forearm.bone.transform.position + rightForearmRotation * rightHandTarget.outward * (rightHandTarget.forearm.bone.length * scaleFactor);

            leftHandTarget.hand.bone.transform.localScale *= scaleFactor;
            rightHandTarget.hand.bone.transform.localScale *= scaleFactor;

            CheckTargetRig(this);
            InitializeAvatar();
        }

        // only public for Editor...
        public static void CheckTargetRig(HumanoidControl humanoid) {
            if (humanoid.targetsRig == null) {
                Object targetsRigPrefab = Resources.Load("HumanoidTargetsRig");
                GameObject targetsRigObject = (GameObject)Instantiate(targetsRigPrefab);
                humanoid.targetsRig = targetsRigObject.GetComponent<Animator>();

                targetsRigObject.transform.position = humanoid.transform.position;
                targetsRigObject.transform.rotation = humanoid.transform.rotation;
                targetsRigObject.transform.SetParent(humanoid.transform);
            }

            humanoid.targetsRig.runtimeAnimatorController = humanoid.animatorController;
        }

        public Animator GetAvatar() {
            if (avatarRig != null && avatarRig != targetsRig && avatarRig.enabled && avatarRig.gameObject.activeInHierarchy) {
                // We already have a good avatarRig
                return avatarRig;
            }

            Avatar avatar = null;
            Animator animator = GetComponent<Animator>();
            if (animator != null) {
                avatar = animator.avatar;
                if (avatar != null && avatar.isValid/* && avatar.isHuman*/ && animator != targetsRig) {
                    return animator;
                }
            }

            Animator[] animators = GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++) {
                avatar = animators[i].avatar;
                if (avatar != null && avatar.isValid /*&& avatar.isHuman*/ && animators[i] != targetsRig) {
                    return animators[i];
                }
            }
            return null;
        }

        private void ScaleAvatar2Tracking() {
            Animator characterAnimator = avatarRig.GetComponent<Animator>();

            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                Transform sourceBone = targetsRig.GetBoneTransform((HumanBodyBones)i);
                Transform destBone = characterAnimator.GetBoneTransform((HumanBodyBones)i);

                if (sourceBone != null && destBone != null) {
                    float sourceBoneLength = GetBoneLength(sourceBone);
                    float destBoneLength = GetBoneLength(destBone);

                    if (sourceBoneLength > 0 && destBoneLength > 0) {
                        float startScaling = (destBone.localScale.x + destBone.localScale.y + destBone.localScale.z) / 3;
                        float scaling = (sourceBoneLength / destBoneLength);
                        float resultScaling = startScaling * scaling;
                        destBone.localScale = new Vector3(resultScaling, resultScaling, resultScaling);
                    }
                }
            }
        }

        private static float GetBoneLength(Transform bone) {
            if (bone.childCount == 1) {
                Transform childBone = bone.GetChild(0);

                float length = Vector3.Distance(bone.position, childBone.position);
                return length;
            }
            else
                return 0;
        }
#endregion

#region Targets
        private void NewTargetComponents() {
            hipsTarget.NewComponent(this);
            hipsTarget.InitComponent();

            headTarget.NewComponent(this);
            headTarget.InitComponent();

            leftHandTarget.NewComponent(this);
            leftHandTarget.InitComponent();

            rightHandTarget.NewComponent(this);
            rightHandTarget.InitComponent();

            leftFootTarget.NewComponent(this);
            leftFootTarget.InitComponent();

            rightFootTarget.NewComponent(this);
            rightFootTarget.InitComponent();
        }

        //private void InitTargetComponents() {
        //    hipsTarget.InitComponent();
        //    headTarget.InitComponent();
        //    leftHandTarget.InitComponent();
        //    rightHandTarget.InitComponent();
        //    leftFootTarget.InitComponent();
        //    rightFootTarget.InitComponent();
        //}

        public void InitTargets() {
            SetBones();
        }

        private void StartTargets() {
            hipsTarget.StartTarget();
            headTarget.StartTarget();
            leftHandTarget.StartTarget();
            rightHandTarget.StartTarget();
            leftFootTarget.StartTarget();
            rightFootTarget.StartTarget();
        }

        //public static void CheckAvatar(HumanoidControl humanoid) {
        //    //humanoid.GetDefaultConfiguration();
        //    humanoid.DetermineTargets();
        //    humanoid.InitTargetComponents();
        //    humanoid.MatchTargetsToAvatar();
        //}

        /// <summary>
        /// Checks the humanoid for presence of Targets
        /// and adds them if they are not found
        /// </summary>
        public void DetermineTargets() {
            HeadTarget.DetermineTarget(this);
            HandTarget.DetermineTarget(this, true);
            HandTarget.DetermineTarget(this, false);
            HipsTarget.DetermineTarget(this);
            FootTarget.DetermineTarget(this, true);
            FootTarget.DetermineTarget(this, false);

            //RetrieveBones();
            //InitializeAvatar();
            //MatchTargetsToAvatar();
        }

        //private void SetTargets() {
        //    targets[0] = hipsTarget;
        //    targets[1] = headTarget;
        //    targets[2] = leftHandTarget;
        //    targets[3] = rightHandTarget;
        //    targets[4] = leftFootTarget;
        //    targets[5] = rightFootTarget;
        //}

        /// <summary>
        /// Changes the target rig transforms to match the avatar rig
        /// </summary>
        public void MatchTargetsToAvatar() {
            hipsTarget.MatchTargetsToAvatar();
            headTarget.MatchTargetsToAvatar();
            leftHandTarget.MatchTargetsToAvatar();
            rightHandTarget.MatchTargetsToAvatar();
            leftFootTarget.MatchTargetsToAvatar();
            rightFootTarget.MatchTargetsToAvatar();
        }

        public void SetPlayerHeight(float height) {
            if (height <= 0)
                return;

            float neckHeight = 0.875F * height;
            ScaleTracking(avatarNeckHeight / neckHeight);
        }

        private void UpdateTargetsAndMovements() {
            CopyTargetsToRig();

            UpdateTargets();
            UpdateMovements();

            CopyRigToTargets();
        }

        private void UpdateTargets() {
            hipsTarget.UpdateTarget();
            headTarget.UpdateTarget();
            leftHandTarget.UpdateTarget();
            rightHandTarget.UpdateTarget();
            leftFootTarget.UpdateTarget();
            rightFootTarget.UpdateTarget();
        }

        public void UpdateMovements() {
            HeadMovements.Update(headTarget);
            TorsoMovements.Update(hipsTarget);
            leftHandTarget.UpdateMovements(this);
            rightHandTarget.UpdateMovements(this);
            LegMovements.Update(leftFootTarget);
            LegMovements.Update(rightFootTarget);
        }

        private void CopyTargetsToRig() {
            //foreach (HumanoidTarget target in targets)
            //    target.CopyTargetToRig();

            hipsTarget.CopyTargetToRig();
            headTarget.CopyTargetToRig();
            leftHandTarget.CopyTargetToRig();
            rightHandTarget.CopyTargetToRig();
            leftFootTarget.CopyTargetToRig();
            rightFootTarget.CopyTargetToRig();
        }

        public void CopyRigToTargets() {
            //foreach (HumanoidTarget target in targets)
            //    target.CopyRigToTarget();

            hipsTarget.CopyRigToTarget();
            headTarget.CopyRigToTarget();
            leftHandTarget.CopyRigToTarget();
            rightHandTarget.CopyRigToTarget();
            leftFootTarget.CopyRigToTarget();
            rightFootTarget.CopyRigToTarget();
        }

        public void UpdateSensorsFromTargets() {
            hipsTarget.UpdateSensorsFromTarget();
            headTarget.UpdateSensorsFromTarget();
            leftHandTarget.UpdateSensorsFromTarget();
            rightHandTarget.UpdateSensorsFromTarget();
            leftFootTarget.UpdateSensorsFromTarget();
            rightFootTarget.UpdateSensorsFromTarget();
        }

        private HumanoidTarget.TargetedBone[] _bones = null; // = new HumanoidTarget.TargetedBone[(int)Bone.Count];
        public HumanoidTarget.TargetedBone GetBone(Bone boneId) {
            if (_bones == null)
                SetBones();
            return _bones[(int)boneId];
        }
        public HumanoidTarget.TargetedBone GetBone(Side side, SideBone sideBoneId) {
            if (_bones == null)
                SetBones();
            return _bones[(int)BoneReference.HumanoidBone(side, sideBoneId)];
        }
        private void SetBones() {
            _bones = new HumanoidTarget.TargetedBone[(int)Bone.Count] {
                null,
                hipsTarget.hips,
                hipsTarget.spine,
                null,
                null,
                hipsTarget.chest,

                headTarget.neck,
                headTarget.head,

                leftHandTarget.shoulder,
                leftHandTarget.upperArm,
                leftHandTarget.forearm,
                null,
                leftHandTarget.hand,

                leftHandTarget.fingers.thumb.proximal,
                leftHandTarget.fingers.thumb.intermediate,
                leftHandTarget.fingers.thumb.distal,

                null,
                leftHandTarget.fingers.index.proximal,
                leftHandTarget.fingers.index.intermediate,
                leftHandTarget.fingers.index.distal,

                null,
                leftHandTarget.fingers.middle.proximal,
                leftHandTarget.fingers.middle.intermediate,
                leftHandTarget.fingers.middle.distal,

                null,
                leftHandTarget.fingers.ring.proximal,
                leftHandTarget.fingers.ring.intermediate,
                leftHandTarget.fingers.ring.distal,

                null,
                leftHandTarget.fingers.little.proximal,
                leftHandTarget.fingers.little.intermediate,
                leftHandTarget.fingers.little.distal,

                leftFootTarget.upperLeg,
                leftFootTarget.lowerLeg,
                leftFootTarget.foot,
                leftFootTarget.toes,

                rightHandTarget.shoulder,
                rightHandTarget.upperArm,
                rightHandTarget.forearm,
                null,
                rightHandTarget.hand,

                rightHandTarget.fingers.thumb.proximal,
                rightHandTarget.fingers.thumb.intermediate,
                rightHandTarget.fingers.thumb.distal,

                null,
                rightHandTarget.fingers.index.proximal,
                rightHandTarget.fingers.index.intermediate,
                rightHandTarget.fingers.index.distal,

                null,
                rightHandTarget.fingers.middle.proximal,
                rightHandTarget.fingers.middle.intermediate,
                rightHandTarget.fingers.middle.distal,

                null,
                rightHandTarget.fingers.ring.proximal,
                rightHandTarget.fingers.ring.intermediate,
                rightHandTarget.fingers.ring.distal,

                null,
                rightHandTarget.fingers.little.proximal,
                rightHandTarget.fingers.little.intermediate,
                rightHandTarget.fingers.little.distal,

                rightFootTarget.upperLeg,
                rightFootTarget.lowerLeg,
                rightFootTarget.foot,
                rightFootTarget.toes,

#if hFACE
                headTarget.face.leftEye.upperLid,
                headTarget.face.leftEye,
                headTarget.face.leftEye.lowerLid,
                headTarget.face.rightEye.upperLid,
                headTarget.face.rightEye,
                headTarget.face.rightEye.lowerLid,

                headTarget.face.leftBrow.outer,
                headTarget.face.leftBrow.center,
                headTarget.face.leftBrow.inner,
                headTarget.face.rightBrow.inner,
                headTarget.face.rightBrow.center,
                headTarget.face.rightBrow.outer,

                headTarget.face.leftEar,
                headTarget.face.rightEar,

                headTarget.face.leftCheek,
                headTarget.face.rightCheek,

                headTarget.face.nose.top,
                headTarget.face.nose.tip,
                headTarget.face.nose.bottomLeft,
                headTarget.face.nose.bottom,
                headTarget.face.nose.bottomRight,

                headTarget.face.mouth.upperLipLeft,
                headTarget.face.mouth.upperLip,
                headTarget.face.mouth.upperLipRight,
                headTarget.face.mouth.lipLeft,
                headTarget.face.mouth.lipRight,
                headTarget.face.mouth.lowerLipLeft,
                headTarget.face.mouth.lowerLip,
                headTarget.face.mouth.lowerLipRight,

                headTarget.face.jaw,
#else
                null,
                null,
                null,
                null,
                null,
                null,

                null,
                null,
                null,
                null,
                null,
                null,

                null,
                null,

                null,
                null,

                null,
                null,
                null,
                null,
                null,

                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                null,
#endif
                null,
            };
        }

#endregion

#region Trackers
        public UnityVRTracker unityVRTracker = new UnityVRTracker();
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public SteamVRTracker steam = new SteamVRTracker();
#endif
#if hOCULUS
        public OculusTracker oculus = new OculusTracker();
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
        public WindowsMRTracker mixedReality = new WindowsMRTracker();
#endif
#if hVRTK
        public VrtkTracker vrtk = new VrtkTracker();
#endif
#if hNEURON
        public NeuronTracker neuronTracker = new NeuronTracker();
#endif
#if hLEAP
        public LeapTracker leapTracker = new LeapTracker();
#endif
#if hREALSENSE
        public RealsenseTracker realsenseTracker = new RealsenseTracker();
#endif
#if hHYDRA
        public HydraTracker hydra = new HydraTracker();
#endif
#if hKINECT1
        public Kinect1Tracker kinect1 = new Kinect1Tracker();
#endif
#if hKINECT2
        public Kinect2Tracker kinectTracker = new Kinect2Tracker();
#endif
#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_WSA_10_0)
        public AstraTracker astra = new AstraTracker();
#endif
#if hOPTITRACK
        public OptiTracker optitrack = new OptiTracker();
#endif
#if hTOBII
        public TobiiTracker tobiiTracker = new TobiiTracker();
#endif
#if hPUPIL
        public Tracking.Pupil.Tracker pupil = new Tracking.Pupil.Tracker();
#endif
#if hDLIB
        public Tracking.Dlib.Tracker dlib = new Tracking.Dlib.Tracker();
#endif

        private Tracker[] _trackers;
        public Tracker[] trackers {
            get {
                if (_trackers == null)
                    InitTrackers();
                return _trackers;
            }
        }

        private TraditionalDevice traditionalInput;

        private void InitTrackers() {
            _trackers = new Tracker[] {
                unityVRTracker,
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                steam,
#endif
#if hOCULUS
                oculus,
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
                mixedReality,
#endif
#if hVRTK
                vrtk,
#endif
#if hNEURON
                neuronTracker,
#endif
#if hLEAP
                leapTracker,
#endif
#if hREALSENSE
                realsenseTracker,
#endif
#if hHYDRA
                hydra,
#endif
#if hKINECT1
                kinect1,
#endif
#if hKINECT2
                kinectTracker,
#endif
#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                astra,
#endif
#if hOPTITRACK
                optitrack,
#endif
#if hTOBII
                tobiiTracker,
#endif
#if hPUPIL
                pupil,
#endif
#if hDLIB
                dlib,
#endif
            };
        }

        private void EnableTrackers(bool enabled) {
            foreach (Tracker tracker in _trackers)
                tracker.enabled = enabled;
        }

        private void StartTrackers() {
            if (traditionalInput == null)
                traditionalInput = new TraditionalDevice();
            traditionalInput.Start(this);

            for (int i = 0; i < _trackers.Length; i++)
                _trackers[i].StartTracker(this);
        }

        private void UpdateTrackers() {
            if (traditionalInput != null)
                traditionalInput.Update();

            for (int i = 0; i < _trackers.Length; i++)
                _trackers[i].UpdateTracker();
        }

        private void StartSensors() {
            hipsTarget.StartSensors();
            headTarget.StartSensors();
            leftHandTarget.StartSensors();
            rightHandTarget.StartSensors();
            leftFootTarget.StartSensors();
            rightFootTarget.StartSensors();
        }

        private void StopSensors() {
            hipsTarget.StopSensors();
            headTarget.StopSensors();
            leftHandTarget.StopSensors();
            rightHandTarget.StopSensors();
            leftFootTarget.StopSensors();
            rightFootTarget.StopSensors();
        }

        public void ScaleTrackingToAvatar() {
            GameObject realWorld = HumanoidControl.GetRealWorld(transform);
            float neckHeight = headTarget.transform.position.y - transform.position.y;
            neckHeight = neckHeight / realWorld.transform.lossyScale.y;
            ScaleTracking(avatarNeckHeight / neckHeight);
        }

        private void ScaleTracking(float scaleFactor) {
            GameObject realWorld = HumanoidControl.GetRealWorld(transform);
            Vector3 newScale = scaleFactor * Vector3.one; // * realWorld.transform.localScale;

            targetsRig.transform.localScale = newScale;
            realWorld.transform.localScale = newScale;
        }

        /// <summary>Adjust Y position to match the tracking with the avatar</summary>
        /// This function will adjust the vertical position of the tracking origin such that the tracking
        /// matches the avatar. This function should preferably be executed when the player is in a base
        /// position: either standing upright or sitting upright, depending on the playing pose.
        /// This will prevent the avatar being in the air or in a crouching position when the player is
        /// taller or smaller than the avatar itself.
        /// It retains 1:1 tracking and the X/Z position of the player are not affected.
        public void SetTrackingHeightToAvatar() {
            Vector3 localNeckPosition;
            if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.None || headTarget.unityVRHead.cameraTransform == null)
                localNeckPosition = headTarget.neck.target.transform.position - transform.position;
            else
                localNeckPosition = HeadMovements.CalculateNeckPosition(headTarget.unityVRHead.cameraTransform.position, headTarget.unityVRHead.cameraTransform.rotation, -headTarget.neck2eyes) - transform.position;

            AdjustTrackingHeight(avatarNeckHeight - localNeckPosition.y);
        }

        public void MoveTrackingHeightToAvatar() {
            Vector3 localNeckPosition;
            if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.None || headTarget.unityVRHead.cameraTransform == null)
                localNeckPosition = headTarget.neck.target.transform.position - transform.position;
            else
                localNeckPosition = HeadMovements.CalculateNeckPosition(headTarget.unityVRHead.cameraTransform.position, headTarget.unityVRHead.cameraTransform.rotation, -headTarget.neck2eyes) - transform.position;
            Vector3 delta = new Vector3(-localNeckPosition.x, avatarNeckHeight - localNeckPosition.y, -localNeckPosition.z);
            AdjustTracking(delta);
        }

        private void AdjustTrackingHeight(float deltaY) {
            AdjustTracking(new Vector3(0, deltaY, 0));
            ////#if hSTEAMVR || hOCULUS
            //Transform unityVrRoot = headTarget.unityVRHead.GetRoot(this);
            //if (unityVrRoot != null)
            //    unityVrRoot.position += new Vector3(0, deltaY, 0);
            //foreach (Tracker tracker in trackers)
            //    tracker.AdjustTracking()
        }

        /// <summary>Adjust the tracking origin of all trackers</summary>
        /// <param name="delta">The translation to apply to the tracking origin</param>
        public void AdjustTracking(Vector3 delta) {
            Transform unityVrRoot = headTarget.unityVRHead.GetRoot(this);
            unityVrRoot.position += delta;
            foreach (Tracker tracker in trackers)
                tracker.AdjustTracking(delta, Quaternion.identity);
        }
#endregion

#region Configuration
        /// <summary>
        /// Scans the humanoid to retrieve all bones
        /// </summary>
        public void RetrieveBones() {
            hipsTarget.RetrieveBones();
            headTarget.RetrieveBones();

            leftHandTarget.RetrieveBones();
            rightHandTarget.RetrieveBones();
            leftFootTarget.RetrieveBones();
            rightFootTarget.RetrieveBones();
        }
#endregion

#region Update
        private void Update() {
            //SetTargets(); // deal with targets being changed at runtime

            //preTrackingHeadPosition = headTarget.neck.target.transform.position;
            //preTrackingHeadDirection = headTarget.neck.target.transform.eulerAngles.y;
            DetermineCollision();
            CalculateMovement();
            CheckBodyPull();
            Controllers.Clear();
            if (pose != null)
                pose.Show(this);
            UpdateTrackers();
            UpdateTargetsAndMovements();
            CalculateVelocityAcceleration();
            UpdateAnimation();
        }

        private void FixedUpdate() {
            CheckGround();
            //CalculateVelocityAcceleration();

            if (leftHandTarget.handMovements != null)
                leftHandTarget.handMovements.FixedUpdate();
            if (rightHandTarget.handMovements != null)
                rightHandTarget.handMovements.FixedUpdate();
        }

        private void LateUpdate() {
            PostAnimationCorrection();

            CheckUpright();
            Controllers.EndFrame();
        }
#endregion

#region Stop
        public void OnApplicationQuit() {
#if hLEAP
            leapTracker.StopTracker();
#endif
#if hREALSENSE
            RealsenseDevice.Stop();
#endif
#if hNEURON
            neuronTracker.StopTracker();
#endif
#if hKINECT1
            kinect1.StopTracker();
#endif
#if hKINECT2
            kinectTracker.StopTracker();
#endif
#if hORBBEC
            astra.StopTracker();
#endif
#if hOPTITRACK
            optitrack.StopTracker();
#endif
        }
#endregion

        public Vector3 up {
            get {
                return useGravity ? Vector3.up : transform.up;
            }
        }

        [HideInInspector]
        private float lastNeckHeight;
        [HideInInspector]
        private Vector3 lastHeadPosition;
        [HideInInspector]
        private Quaternion lastHeadRotation;
        [HideInInspector]
        private float lastHeadDirection;

        public event OnNewNeckHeight OnNewNeckHeightEvent;
        public delegate void OnNewNeckHeight(float neckHeight);

        private void CheckUpright() {
            if (OnNewNeckHeightEvent == null)
                return;

            GameObject realWorld = HumanoidControl.GetRealWorld(transform);

            // need to unscale the velocity, use localPosition ?
            float headVelocity = (headTarget.neck.target.transform.position - lastHeadPosition).magnitude / Time.deltaTime;
            float angularHeadVelocity = Quaternion.Angle(lastHeadRotation, headTarget.neck.target.transform.rotation) / Time.deltaTime;

            float deviation = Vector3.Angle(up, headTarget.transform.up);

            if (deviation < 4 && headVelocity < 0.02 && angularHeadVelocity < 3 && headVelocity + angularHeadVelocity > 0) {

                float neckHeight = (headTarget.transform.position.y - transform.position.y) / realWorld.transform.localScale.y;
                if (Mathf.Abs(neckHeight - lastNeckHeight) > 0.01F) {
                    lastNeckHeight = neckHeight;
                    if (lastNeckHeight > 0)
                        OnNewNeckHeightEvent(lastNeckHeight);
                }
            }
        }

        public void Calibrate() {
            foreach (Tracker tracker in _trackers)
                tracker.Calibrate();

            switch (scaling) {
                case ScalingType.ScaleAvatarToTracking:
                    ScaleAvatarToTracking();
                    break;
                case ScalingType.ScaleTrackingToAvatar:
                    ScaleTrackingToAvatar();
                    break;
                case ScalingType.SetHeightToAvatar:
                    SetTrackingHeightToAvatar();
                    break;
                case ScalingType.MoveHeightToAvatar:
                    MoveTrackingHeightToAvatar();
                    break;
                default:
                    break;
            }
        }

#region Movement
#region Input/API
        /// <summary>
        /// maximum forward speed in units(meters)/second
        /// </summary>
        public float forwardSpeed = 1;
        /// <summary>
        /// maximum backward speed in units(meters)/second
        /// </summary>
        public float backwardSpeed = 0.6F;
        /// <summary>
        /// maximum sideways speed in units(meters)/second
        /// </summary>
        public float sidewardSpeed = 1;
        /// <summary>
        /// maximum acceleration in units(meters)/second/second
        /// value 0 = no maximum acceleration
        /// </summary>
        public float maxAcceleration = 1;
        /// <summary>
        /// maximum rotational speed in degrees/second
        /// </summary>
        public float rotationSpeed = 60;

        public virtual void MoveForward(float z) {
            if (z > 0)
                z *= forwardSpeed;
            else
                z *= backwardSpeed;

            if (maxAcceleration > 0 && curProximitySpeed >= 1) {
                float accelerationStep = (z - targetVelocity.z);
                float maxAccelerationStep = maxAcceleration * Time.deltaTime;
                accelerationStep = Mathf.Clamp(accelerationStep, -maxAccelerationStep, maxAccelerationStep);
                z = targetVelocity.z + accelerationStep;
            }

            targetVelocity = new Vector3(targetVelocity.x, targetVelocity.y, z);
        }

        public virtual void MoveSideward(float x) {
            x = x * sidewardSpeed;

            if (maxAcceleration > 0 && curProximitySpeed >= 1) {
                float accelerationStep = (x - targetVelocity.x);
                float maxAccelerationStep = maxAcceleration * Time.deltaTime;
                accelerationStep = Mathf.Clamp(accelerationStep, -maxAccelerationStep, maxAccelerationStep);
                x = targetVelocity.x + accelerationStep;
            }

            targetVelocity = new Vector3(x, targetVelocity.y, targetVelocity.z);
        }

        public virtual void Move(Vector3 velocity) {
            targetVelocity = velocity;
        }

        public virtual void Rotate(float angularSpeed) {
            angularSpeed *= Time.deltaTime * rotationSpeed;
            transform.RotateAround(hipsTarget.transform.position, hipsTarget.transform.up, angularSpeed);
        }

        public void Dash(Vector3 targetPosition) {
            MoveTo(targetPosition, MovementType.Dash);
        }

        public void Teleport(Vector3 targetPosition) {
            MoveTo(targetPosition, MovementType.Teleport);
        }

        public void TeleportForward(float distance = 1) {
            MoveTo(transform.position + hipsTarget.transform.forward * distance);
        }

        public void MoveTo(Vector3 position, MovementType movementType = MovementType.Teleport) {
            switch (movementType) {
                case MovementType.Teleport:
                    TransformMovements.Teleport(transform, position);
                    break;
                case MovementType.Dash:
                    StartCoroutine(TransformMovements.DashCoroutine(transform, position));
                    break;
                default:
                    break;
            }
        }
#endregion

#region Checks
        [HideInInspector]
        public Vector3 targetVelocity;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public Vector3 acceleration;
        [HideInInspector]
        public float turningVelocity;

        private void CalculateMovement() {
            Vector3 translationVector = CheckMovement();
            transform.position += translationVector * Time.deltaTime;
        }

        private float curProximitySpeed = 1;

        public Vector3 CheckMovement() {
            Vector3 newVelocity = new Vector3(targetVelocity.x, 0, targetVelocity.z);

            if (proximitySpeed) {
                curProximitySpeed = CalculateProximitySpeed(bodyCapsule, curProximitySpeed);
                newVelocity *= curProximitySpeed;
            }

            Vector3 inputDirection = hipsTarget.transform.TransformDirection(newVelocity);
            inputDirection = new Vector3(inputDirection.x, 0, inputDirection.z);

            if (physics && (collided || (!proximitySpeed && triggerEntered))) {
                float angle = Vector3.Angle(inputDirection, hitNormal);
                if (angle > 90)
                    return Vector3.zero;
            }

            return inputDirection;
        }

        private float CalculateProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
            if (triggerEntered) {
                if (cc.radius > 0.25f && targetVelocity.magnitude > 0)
                    curProximitySpeed = CheckDecreaseProximitySpeed(cc, curProximitySpeed);
            }
            else {
                if (curProximitySpeed < 1 && targetVelocity.magnitude > 0)
                    curProximitySpeed = CheckIncreaseProximitySpeed(cc, curProximitySpeed);
            }
            return curProximitySpeed;
        }

        private float CheckDecreaseProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
            RaycastHit[] hits = Physics.CapsuleCastAll(hipsTarget.transform.position + (cc.radius - 0.8f) * Vector3.up, hipsTarget.transform.position - (cc.radius - 1.2f) * Vector3.up, cc.radius - 0.05f, velocity, 0.04f);
            bool collision = false;
            for (int i = 0; i < hits.Length && collision == false; i++) {
                if (!IsMyRigidbody(hits[i].rigidbody)) {
                    collision = true;
                    cc.radius -= 0.05f / proximitySpeedRate;
                    cc.height += 0.05f / proximitySpeedRate;
                    curProximitySpeed = EaseIn(1, (-0.8f), 1 - cc.radius, 0.75f);
                }
            }
            return curProximitySpeed;
        }

        private float CheckIncreaseProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
            Vector3 capsuleCenter = hipsTarget.hips.bone.transform.position + cc.center;
            Vector3 offset = ((cc.height - cc.radius) / 2) * Vector3.up;
            Vector3 point1 = capsuleCenter + offset;
            Vector3 point2 = capsuleCenter - offset;
            Collider[] results = Physics.OverlapCapsule(point1, point2, cc.radius + 0.05F);

            /*
            RaycastHit[] hits = Physics.CapsuleCastAll(hipsTarget.transform.position + (cc.radius - 0.75f) * Vector3.up, hipsTarget.transform.position - (cc.radius - 1.15f) * Vector3.up, cc.radius, inputDirection, 0.04f);
            bool collision = false;
            for (int i = 0; i < hits.Length && collision == false; i++) {
                if (hits[i].rigidbody == null) {
                    collision = true;
                }
            }
            */

            bool collision = false;
            for (int i = 0; i < results.Length; i++) {
                if (!results[i].isTrigger && !IsMyRigidbody(results[i].attachedRigidbody)) { 
                    //results[i].attachedRigidbody != humanoidRigidbody && results[i].attachedRigidbody != characterRigidbody &&
                    //results[i].attachedRigidbody != headTarget.headRigidbody &&
                    //results[i].attachedRigidbody != leftHandTarget.handRigidbody && results[i].attachedRigidbody != rightHandTarget.handRigidbody
                    //) {

                    collision = true;
                }
            }

            if (collision == false) {
                cc.radius += 0.05f / proximitySpeedRate;
                cc.height -= 0.05f / proximitySpeedRate;
                curProximitySpeed = EaseIn(1, (-0.8f), 1 - cc.radius, 0.75f);
            }
            return curProximitySpeed;
        }

        private static float EaseIn(float start, float distance, float elapsedTime, float duration) {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return distance * elapsedTime * elapsedTime + start;
        }
#endregion

#region Collisions
        public bool triggerEntered;
        public bool collided;
        public Vector3 hitNormal = Vector3.zero;

        [HideInInspector]
        public Rigidbody humanoidRigidbody;
        [HideInInspector]
        public Rigidbody characterRigidbody;
        [HideInInspector]
        public CapsuleCollider bodyCapsule;
        [HideInInspector]
        public CapsuleCollider bodyCollider;
        [HideInInspector]
        private readonly float colliderRadius = 0.15F;

        private void AddCharacterColliders() {
            if (avatarRig == null || hipsTarget.hips.bone.transform == null || isRemote || !physics)
                return;

            Transform collidersTransform = hipsTarget.hips.bone.transform.Find("Character Colliders");
            if (collidersTransform != null)
                return;

            GameObject collidersObject = new GameObject("Character Colliders");
            collidersObject.transform.parent = hipsTarget.hips.bone.transform;
            collidersObject.transform.localPosition = Vector3.zero;
            collidersObject.transform.rotation = this.transform.rotation;
            int layer = LayerMask.NameToLayer("Humanoid");
            if (layer > 0)
                collidersObject.layer = layer;

            HumanoidCollisionHandler collisionHandler = collidersObject.AddComponent<HumanoidCollisionHandler>();
            collisionHandler.humanoid = this;

            characterRigidbody = collidersObject.GetComponent<Rigidbody>();
            if (characterRigidbody == null)
                characterRigidbody = collidersObject.AddComponent<Rigidbody>();
            if (characterRigidbody != null) {
                characterRigidbody.mass = 1;
                characterRigidbody.useGravity = false;
                characterRigidbody.isKinematic = true;
            }

            if (generateColliders) {
                float avatarHeight = avatarNeckHeight * 8 / 7;
                Vector3 colliderCenter = Vector3.up * (stepOffset / 2);

                CheckBodyCollider(collidersObject);

                if (proximitySpeed) {
                    bodyCapsule = collidersObject.AddComponent<CapsuleCollider>();
                    if (bodyCapsule != null) {
                        bodyCapsule.isTrigger = true;
                        if (proximitySpeed) {
                            bodyCapsule.height = 0.80F;
                            bodyCapsule.radius = 1F;
                        }
                        else {
                            bodyCapsule.height = avatarHeight - stepOffset;
                            bodyCapsule.radius = colliderRadius;
                        }
                        bodyCapsule.center = colliderCenter;
                    }
                }
            }

            humanoidRigidbody = gameObject.GetComponent<Rigidbody>();
            if (humanoidRigidbody == null)
                humanoidRigidbody = gameObject.AddComponent<Rigidbody>();
            if (humanoidRigidbody != null) {
                humanoidRigidbody.mass = 1;
                humanoidRigidbody.useGravity = false;
                humanoidRigidbody.isKinematic = true;
            }
        }

        private void CheckBodyCollider(GameObject collidersObject) {
            //float avatarHeight = avatarNeckHeight * 8 / 7;
            //Vector3 colliderCenter = Vector3.up * (stepOffset / 2);

            HumanoidTarget.TargetedBone spineBone = hipsTarget.spine;
            if (spineBone == null)
                spineBone = hipsTarget.hips;

            // Add gameobject with target rotation to ensure the direction of the capsule
            GameObject spineColliderObject = new GameObject("Spine Collider");
            spineColliderObject.transform.parent = spineBone.bone.transform;
            spineColliderObject.transform.localPosition = Vector3.zero;
            spineColliderObject.transform.rotation = spineBone.target.transform.rotation;

            bodyCollider = spineColliderObject.AddComponent<CapsuleCollider>();
            bodyCollider.isTrigger = false;
            bodyCollider.height = avatarNeckHeight - (hipsTarget.hips.bone.transform.position.y - avatarRig.transform.position.y) + 0.1F;
            bodyCollider.radius = colliderRadius - 0.05F;
            bodyCollider.center = new Vector3(0, bodyCollider.height / 2, 0);

            HumanoidTarget.BoneTransform leftUpperLeg = leftFootTarget.upperLeg.bone;
            // Add gameobject with target rotation to ensure the direction of the capsule
            GameObject leftColliderObject = new GameObject("Left Leg Collider");
            leftColliderObject.transform.parent = leftUpperLeg.transform;
            leftColliderObject.transform.localPosition = Vector3.zero;
            leftColliderObject.transform.rotation = leftFootTarget.upperLeg.target.transform.rotation;

            CapsuleCollider leftUpperLegCollider = leftColliderObject.AddComponent<CapsuleCollider>();
            leftUpperLegCollider.isTrigger = false;
            leftUpperLegCollider.height = leftUpperLeg.length;
            leftUpperLegCollider.radius = 0.08F;
            leftUpperLegCollider.center = new Vector3(0, -leftUpperLeg.length / 2, 0);

            HumanoidTarget.BoneTransform rightUpperLeg = rightFootTarget.upperLeg.bone;
            // Add gameobject with target rotation to ensure the direction of the capsule
            GameObject rightColliderObject = new GameObject("Right Leg Collider");
            rightColliderObject.transform.parent = rightUpperLeg.transform;
            rightColliderObject.transform.localPosition = Vector3.zero;
            rightColliderObject.transform.rotation = rightFootTarget.upperLeg.target.transform.rotation;

            CapsuleCollider rightUpperLegCollider = rightColliderObject.AddComponent<CapsuleCollider>();
            rightUpperLegCollider.isTrigger = false;
            rightUpperLegCollider.height = rightUpperLeg.length;
            rightUpperLegCollider.radius = 0.08F;
            rightUpperLegCollider.center = new Vector3(0, -rightUpperLeg.length / 2, 0);
        }

        private void DetermineCollision() {
            if (proximitySpeed) {
                //float angle = Vector3.Angle(hitNormal, targetVelocity);
                collided = (triggerEntered && bodyCapsule.radius <= 0.25f);
            }
            else
                collided = triggerEntered;
        }

        public bool IsMyRigidbody(Rigidbody rigidbody) {
            return
                rigidbody != null && (
                rigidbody == humanoidRigidbody ||
                rigidbody == characterRigidbody ||
                rigidbody == headTarget.headRigidbody ||
                rigidbody == leftHandTarget.handRigidbody ||
                rigidbody == rightHandTarget.handRigidbody
                );
        }
#endregion

#region Ground
        public Transform ground;
        [HideInInspector]
        private Transform lastGround;
        [HideInInspector]
        public Vector3 groundVelocity;
        [HideInInspector]
        public float groundAngularVelocity;
        [HideInInspector]
        private Vector3 lastGroundPosition = Vector3.zero;
        [HideInInspector]
        private float lastGroundAngle = 0;
        [HideInInspector]
        Vector3 gravitationalVelocity;

        private void CheckGround() {
            CheckGrounded();
            CheckGroundMovement();
        }

        private void CheckGrounded() {
            if (!leftHandTarget.GrabbedStaticObject() && !rightHandTarget.GrabbedStaticObject()) {

                Vector3 footBase = GetHumanoidPosition();

                Vector3 groundNormal;
                float distance = GetDistanceToGroundAt(footBase, stepOffset, out ground, out groundNormal);
                if (distance > 0.01F) {
                    gravitationalVelocity = Vector3.zero;
                    transform.Translate(0, distance, 0);
                }
                else if (distance < -0.02F) {
                    ground = null;
                    if (useGravity)
                        Fall();
                }
            }
        }

        public float GetDistanceToGroundAt(Vector3 position, float maxDistance) {
            Transform _ground;
            Vector3 _normal;
            return GetDistanceToGroundAt(position, maxDistance, out _ground, out _normal);
        }

        public float GetDistanceToGroundAt(Vector3 position, float maxDistance, out Transform ground, out Vector3 normal) {
            normal = up;

            Vector3 rayStart = position + normal * maxDistance;
            Vector3 rayDirection = -normal;
            //Debug.DrawRay(rayStart, rayDirection * maxDistance * 2, Color.magenta);
            RaycastHit[] hits = Physics.RaycastAll(rayStart, rayDirection, maxDistance * 2, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            if (hits.Length == 0) {
                ground = null;
                return -maxDistance;
            }

            int closestHit = 0;
            bool foundClosest = false;
            for (int i = 0; i < hits.Length; i++) {
                if (hits[i].collider != bodyCollider && hits[i].transform != headTarget.transform && hits[i].distance <= hits[closestHit].distance) {
                    closestHit = i;
                    foundClosest = true;
                }
            }
            if (!foundClosest) {
                ground = null;
                return -maxDistance;
            }

            ground = hits[closestHit].transform;
            normal = hits[closestHit].normal;
            float distance = maxDistance - hits[closestHit].distance;
            return distance;
        }

        private void CheckGroundMovement() {
            if (ground == null) {
                lastGround = null;
                lastGroundPosition = Vector3.zero;
                lastGroundAngle = 0;
                return;
            }

            if (ground == lastGround) {
                Vector3 groundTranslation = ground.position - lastGroundPosition;
                groundVelocity = groundTranslation / Time.deltaTime;

                float groundRotation = ground.eulerAngles.y - lastGroundAngle;
                groundAngularVelocity = groundRotation / Time.deltaTime;

                if (this.transform.root != ground.root) {
                    transform.Translate(groundTranslation, Space.World);
                    transform.RotateAround(ground.position, Vector3.up, groundRotation);
                }
            }

            lastGround = ground;
            lastGroundPosition = ground.position;
            lastGroundAngle = ground.eulerAngles.y;
        }
#endregion

#region Body Pull
        private void CheckBodyPull() {
            Vector3 leftPullVector = Vector3.zero;
            Vector3 rightPullVector = Vector3.zero;

            if (leftHandTarget.GrabbedStaticObject()) {
                leftPullVector = leftHandTarget.hand.bone.transform.position - leftHandTarget.hand.target.transform.position;

            }
            if (rightHandTarget.GrabbedStaticObject()) {
                rightPullVector = rightHandTarget.hand.bone.transform.position - rightHandTarget.hand.target.transform.position;
                Debug.DrawRay(rightHandTarget.hand.bone.transform.position, rightPullVector);
            }
            Vector3 pullVector = (leftPullVector + rightPullVector) / 2;
            this.transform.Translate(pullVector);
        }
#endregion

        [HideInInspector]
        private float lastTime;

        private void CalculateVelocityAcceleration() {
            if (lastTime > 0) {
                float deltaTime = Time.time - lastTime;

                Vector3 localVelocity = -groundVelocity;
                if (avatarRig != null) {
                    Vector3 headTranslation = headTarget.neck.target.transform.position - lastHeadPosition;//preTrackingHeadPosition;
                    if (headTranslation.magnitude == 0)
                        // We assume we did not get an update - needs to be improved though
                        // Especially with networking, position updates occur less frequent than frame updates
                        return;
                    Vector3 localHeadTranslation = headTarget.neck.target.transform.InverseTransformDirection(headTranslation);
                    localVelocity += localHeadTranslation / deltaTime;
                    //Debug.Log(gameObject.name + " " + localHeadTranslation.z);

                    float headDirection = headTarget.neck.target.transform.eulerAngles.y - lastHeadDirection; //preTrackingHeadDirection;
                    float localHeadDirection = Angle.Normalize(headDirection);
                    turningVelocity = localHeadDirection / deltaTime;
                }

                //acceleration = (localVelocity - velocity) / deltaTime;
                // Acceleration is not correct like this. We get accels like -24.3, 22, 6.7, -34.4, 32.6, -5.0 for linear speed increase...
                // This code is not correct. 
                //if (acceleration.magnitude > 15) { // more than 15 is considered unhuman and will be ignored
                //    localVelocity = Vector3.zero;
                //    acceleration = Vector3.zero;
                //}
                velocity = localVelocity;
            }
            lastTime = Time.time;

            lastHeadPosition = headTarget.neck.target.transform.position;
            lastHeadRotation = headTarget.neck.target.transform.rotation;
            lastHeadDirection = headTarget.neck.target.transform.eulerAngles.y;
        }

#region Animation
        public string animatorParameterForward;
        public string animatorParameterSideward;
        public string animatorParameterRotation;
        public string animatorParameterHeight;

        // needed for the Editor
        public int animatorParameterForwardIndex;
        public int animatorParameterSidewardIndex;
        public int animatorParameterRotationIndex;
        public int animatorParameterHeightIndex;

        private void UpdateAnimation() {
            if (targetsRig.runtimeAnimatorController != null) {
                if (animatorParameterForward != null && animatorParameterForward != "") {
                    targetsRig.SetFloat(animatorParameterForward, velocity.z);
                }
                if (animatorParameterSideward != null && animatorParameterSideward != "") {
                    targetsRig.SetFloat(animatorParameterSideward, velocity.x);
                }
                if (animatorParameterRotation != null && animatorParameterRotation != "")
                    targetsRig.SetFloat(animatorParameterRotation, turningVelocity);

                if (animatorParameterHeight != null && animatorParameterHeight != "") {
                    float relativeHeadHeight = headTarget.neck.target.transform.position.y - avatarNeckHeight;
                    targetsRig.SetFloat(animatorParameterHeight, relativeHeadHeight);
                }
            }
        }

        private void PostAnimationCorrection() {
            return;
            /* Currently disabled because of neck issues
             * 
            // Adjust avatar root to match headbone with headtarget again.
            // The animation controller can move this back between Update and LateUpdate

            // Restore hipstarget from bone
            Vector3 translation = headTarget.neck.bone.transform.position - headTarget.neck.target.transform.position;
            //Debug.DrawLine(headTarget.neck.bone.transform.position, headTarget.neck.targetTransform.position, Color.magenta);
            hipsTarget.hips.target.transform.Translate(translation, Space.World);

            if (headTarget.head.target.confidence.rotation > 0.2F) {
                // Restore headtarget from bone
                Quaternion rotation = headTarget.neck.bone.transform.rotation * headTarget.neck.bone.toTargetRotation;
                headTarget.neck.target.transform.rotation = rotation;
            }

            if (leftHandTarget.hand.target.confidence.position > 0.2F) {
                // Restore hand target from bone
                Quaternion rotation = leftHandTarget.hand.bone.transform.rotation * leftHandTarget.hand.bone.toTargetRotation;
                leftHandTarget.hand.target.transform.rotation = rotation;
                leftHandTarget.hand.target.transform.position = leftHandTarget.hand.bone.transform.position;
            }
            if (rightHandTarget.hand.target.confidence.position > 0.2F) {
                // Restore hand target from bone
                Quaternion rotation = rightHandTarget.hand.bone.transform.rotation * rightHandTarget.hand.bone.toTargetRotation;
                rightHandTarget.hand.target.transform.rotation = rotation;
                rightHandTarget.hand.target.transform.position = rightHandTarget.hand.bone.transform.position;
            }
            */
        }
#endregion

        [HideInInspector]
        private float lastLocalHipY;
        private void Fall() {
            gravitationalVelocity += Physics.gravity * Time.deltaTime;

            // Only fall when the avatar is not moving vertically
            // This to prevent physical falling interfering with virtual falling
            float localHipY = hipsTarget.hips.bone.transform.position.y - transform.position.y;
            float hipsTranslationY = localHipY - lastLocalHipY;
            if (Mathf.Abs(hipsTranslationY) < 0.01F)
                transform.Translate(gravitationalVelocity * Time.deltaTime);

            lastLocalHipY = localHipY;
        }
#endregion

        public static GameObject GetRealWorld(Transform transform) {
            Transform realWorldTransform = transform.Find("Real World");
            if (realWorldTransform != null)
                return realWorldTransform.gameObject;

            GameObject realWorld = new GameObject("Real World");
            realWorld.transform.parent = transform;
            realWorld.transform.localPosition = Vector3.zero;
            realWorld.transform.localRotation = Quaternion.identity;
            return realWorld;
        }

        public static GameObject FindTrackerObject(GameObject realWorld, string trackerName) {
            Transform rwTransform = realWorld.transform;

            for (int i = 0; i < rwTransform.childCount; i++) {
                if (rwTransform.GetChild(i).name == trackerName)
                    return rwTransform.GetChild(i).gameObject;
            }
            return null;
        }

        /// <summary>
        /// The humanoid can be on a differentlocation than the humanoid.transform
        /// because the tracking can move the humanoid around independently
        /// This function takes this into account
        /// </summary>
        /// <returns>The position of the humanoid</returns>
        public Vector3 GetHumanoidPosition() {
            Vector3 footPosition = (leftFootTarget.foot.target.transform.position + rightFootTarget.foot.target.transform.position) / 2;
            Vector3 footBase = new Vector3(footPosition.x, transform.position.y, footPosition.z);
            return footBase;
        }
        public Vector3 GetHumanoidPosition2() {
            Vector3 footPosition = (leftFootTarget.foot.bone.transform.position + rightFootTarget.foot.bone.transform.position) / 2;
            float lowestFoot = Mathf.Min(leftFootTarget.foot.bone.transform.position.y, rightFootTarget.foot.bone.transform.position.y);
            Vector3 footBase = new Vector3(footPosition.x, lowestFoot - leftFootTarget.soleThicknessFoot, footPosition.z);
            return footBase;
        }
        public Vector3 GetHumanoidPosition3() {
            Vector3 hipsPosition = hipsTarget.hips.bone.transform.position;
            Vector3 footPosition = hipsPosition - up * (leftFootTarget.upperLeg.bone.length + leftFootTarget.lowerLeg.bone.length + leftFootTarget.soleThicknessFoot);
            return footPosition;
        }
        public Vector3 GetHumanoidPosition4() {
            Vector3 neckPosition = headTarget.neck.bone.transform.position;
            Vector3 footBase = neckPosition - up * avatarNeckHeight;
            return footBase;
        }
        public Vector3 GetHumanoidPosition5() {
            Vector3 footPosition = (leftFootTarget.foot.target.transform.position + rightFootTarget.foot.target.transform.position) / 2;
            float lowestFoot = Mathf.Min(leftFootTarget.foot.target.transform.position.y, rightFootTarget.foot.target.transform.position.y);
            Vector3 footBase = new Vector3(footPosition.x, lowestFoot - leftFootTarget.soleThicknessFoot, footPosition.z);
            return footBase;
        }


#region Humanoid store
        private static HumanoidControl[] _allHumanoids = new HumanoidControl[0];
        public static HumanoidControl[] allHumanoids {
            get { return _allHumanoids; }
        }

        private void AddHumanoid() {
            if (HumanoidExists(this))
                return;

            ExtendHumanoids(this);

            humanoidNetworking = HumanoidNetworking.GetLocalHumanoidNetworking();
            if (!isRemote && humanoidNetworking != null)
                humanoidNetworking.InstantiateHumanoid(this);
        }

        private static void ExtendHumanoids(HumanoidControl humanoid) {
            HumanoidControl[] newAllHumanoids = new HumanoidControl[_allHumanoids.Length + 1];
            for (int i = 0; i < _allHumanoids.Length; i++) {
                newAllHumanoids[i] = _allHumanoids[i];
            }
            _allHumanoids = newAllHumanoids;
            _allHumanoids[_allHumanoids.Length - 1] = humanoid;
            humanoid.humanoidId = _allHumanoids.Length - 1;
        }

        private void RemoveHumanoid() {
            if (!HumanoidExists(this))
                return;

            if (!isRemote && humanoidNetworking != null)
                humanoidNetworking.DestroyHumanoid(this);

            RemoveHumanoid(this);

        }

        private static void RemoveHumanoid(HumanoidControl humanoid) {
            HumanoidControl[] newAllHumanoids = new HumanoidControl[_allHumanoids.Length - 1];
            int j = 0;
            for (int i = 0; i < _allHumanoids.Length; i++) {
                if (_allHumanoids[i] != humanoid) {
                    newAllHumanoids[j] = _allHumanoids[i];
                    j++;
                }
            }
            _allHumanoids = newAllHumanoids;
        }

        private static bool HumanoidExists(HumanoidControl humanoid) {
            for (int i = 0; i < _allHumanoids.Length; i++) {
                if (humanoid == _allHumanoids[i])
                    return true;
            }
            return false;
        }

        public static HumanoidControl[] AllVisibleHumanoids(Camera camera) {
            HumanoidControl[] visibleHumanoids = new HumanoidControl[_allHumanoids.Length];

            int j = 0;
            for (int i = 0; i < _allHumanoids.Length; i++) {
                if (_allHumanoids[i].IsVisible(camera)) {
                    visibleHumanoids[j] = _allHumanoids[i];
                    j++;
                }
            }

            HumanoidControl[] allVisibleHumanoids = new HumanoidControl[j];
            for (int i = 0; i < j; i++) {
                allVisibleHumanoids[i] = visibleHumanoids[i];
            }
            return allVisibleHumanoids;
        }

        public bool IsVisible(Camera camera) {
            Vector3 screenPosition = camera.WorldToScreenPoint(headTarget.transform.position);
            return (screenPosition.x > 0 && screenPosition.x < camera.pixelWidth &&
                screenPosition.y > 0 && screenPosition.y < camera.pixelHeight);
        }
#endregion
    }
}