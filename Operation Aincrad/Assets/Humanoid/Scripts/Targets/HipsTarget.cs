using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/hips-target/")]
    public class HipsTarget : HumanoidTarget {

        public HipsTarget() {
            chest = new TargetedChestBone(this);
            spine = new TargetedSpineBone(this);
            hips = new TargetedHipsBone(this);
        }

        public bool newSpineIK = false;
        public TorsoMovements torsoMovements = new TorsoMovements();

        #region Limitations
        public const float maxSpineAngle = 20;
        public const float maxChestAngle = 20;
        #endregion

        #region Sensors
        public TorsoAnimator animator = new TorsoAnimator();
        public override void EnableAnimator(bool enabled) {
            animator.enabled = enabled;
        }

#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
        public ViveTrackerTorso viveTracker = new ViveTrackerTorso();
#endif
#if hNEURON
        public PerceptionNeuronTorso neuron = new PerceptionNeuronTorso();
#endif
#if hKINECT1
        public Humanoid.Kinect1Torso kinect1 = new Humanoid.Kinect1Torso();
#endif
#if hKINECT2
        public Kinect2Torso kinect = new Kinect2Torso();
#endif
#if hORBBEC
        public Humanoid.AstraTorso astra = new Humanoid.AstraTorso();
#endif
#if hOPTITRACK
        public OptitrackTorso optitrack = new OptitrackTorso();
#endif

        private UnityTorsoSensor[] sensors;

        public override void InitSensors() {
            if (sensors == null) {
                sensors = new UnityTorsoSensor[] {
                    animator,
#if hSTEAMVR && hVIVETRACKER && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                    viveTracker,
#endif
#if hKINECT1
                    kinect1,
#endif
#if hKINECT2
                    kinect,
#endif
#if hORBBEC
                    astra,
#endif
#if hNEURON
                    neuron,
#endif
#if hOPTITRACK
                    optitrack,
#endif
                };
            }
        }

        public override void StartSensors() {
            animator.Start(humanoid, transform);

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Start(humanoid, this.transform);
        }

        protected override void UpdateSensors() {
            for (int i = 0; i < sensors.Length; i++)
                sensors[i].Update();
        }
        #endregion

        #region SubTargets
        public override TargetedBone main {
            get { return hips; }
        }

        #region Chest
        public TargetedChestBone chest = null;

        [System.Serializable]
        public class TargetedChestBone : TargetedBone {
            private HipsTarget hipsTarget;

            public TargetedChestBone(HipsTarget hipsTarget) {
                this.hipsTarget = hipsTarget;
                boneId = Bone.Chest;
            }

            public override void Init() {
                parent = hipsTarget.spine;
                nextBone = (hipsTarget.humanoid.headTarget.neck.bone.transform != null) ?
                    (TargetedBone) hipsTarget.humanoid.headTarget.neck : 
                    (TargetedBone) hipsTarget.humanoid.headTarget.head;
            }

            public override Quaternion DetermineRotation() {
                Vector3 backUp = nextBone.bone.transform.position - hipsTarget.hips.bone.transform.position;
                Quaternion backRotation = Quaternion.LookRotation(backUp, -hipsTarget.hips.target.transform.forward) * Quaternion.AngleAxis(90, Vector3.right);

                Vector3 chestUpDirection = Vector3.up;
                if (nextBone != null && nextBone.bone.transform != null)
                    chestUpDirection = (nextBone.bone.transform.position - bone.transform.position).normalized;
                Quaternion chestRotation = Quaternion.LookRotation(chestUpDirection, target.transform.rotation * Vector3.back) * Quaternion.Euler(90, 0, 0);

                Quaternion chestTargetRotation = Quaternion.Inverse(backRotation) * chestRotation;
                bone.baseRotation = chestTargetRotation;
                return chestRotation;
            }

            public override float GetTension() {
                Quaternion restRotation = hipsTarget.spine.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        #region Spine
        public TargetedSpineBone spine = null;

        [System.Serializable]
        public class TargetedSpineBone : TargetedBone {
            private HipsTarget hipsTarget;

            public TargetedSpineBone(HipsTarget hipsTarget) {
                this.hipsTarget = hipsTarget;
                boneId = Bone.Spine;
            }

            public override void Init() {
                parent = hipsTarget.hips;
                if (hipsTarget.chest.bone.transform != null)
                    nextBone = hipsTarget.chest;
                else
                    nextBone = hipsTarget.humanoid.headTarget.neck;
            }

            public override Quaternion DetermineRotation() {
                HeadTarget headTarget = hipsTarget.humanoid.headTarget;
                Vector3 neckPosition = (headTarget.neck.bone.transform != null) ? headTarget.neck.bone.transform.position : headTarget.head.bone.transform.position;

                Vector3 backUp = neckPosition - hipsTarget.hips.bone.transform.position;
                Quaternion backRotation = Quaternion.LookRotation(backUp, -hipsTarget.hips.target.transform.forward) * Quaternion.AngleAxis(90, Vector3.right);

                Vector3 spineUpDirection = Vector3.up;
                if (nextBone != null && nextBone.bone.transform != null)
                    spineUpDirection = nextBone.bone.transform.position - bone.transform.position;
                Quaternion spineRotation = Quaternion.LookRotation(spineUpDirection, target.transform.rotation * Vector3.back) * Quaternion.Euler(90, 0, 0);

                Quaternion spineTargetRotation = Quaternion.Inverse(backRotation) * spineRotation;

                bone.baseRotation = spineTargetRotation;

                return spineRotation;
            }

            public override float GetTension() {
                Quaternion restRotation = hipsTarget.hips.bone.targetRotation;
                float tension = GetTension(restRotation, this);
                return tension;
            }
        }
        #endregion

        #region Hips
        public TargetedHipsBone hips = null;

        [System.Serializable]
        public class TargetedHipsBone : TargetedBone {
            public HipsTarget hipsTarget;

            public TargetedHipsBone(HipsTarget hipsTarget) {
                this.hipsTarget = hipsTarget;
                boneId = Bone.Hips;
            }

            public override void Init() {
                parent = null;
                if (hipsTarget.spine.bone.transform != null)
                    nextBone = hipsTarget.spine;
                else if (hipsTarget.chest.bone.transform != null)
                    nextBone = hipsTarget.chest;
                else
                    nextBone = hipsTarget.humanoid.headTarget.neck;
            }

            public override Quaternion DetermineRotation() {
                HeadTarget headTarget = hipsTarget.humanoid.headTarget;
                Vector3 neckPosition = (headTarget.neck.bone.transform != null) ? headTarget.neck.bone.transform.position : headTarget.head.bone.transform.position;
                Vector3 backUp = neckPosition - hipsTarget.hips.bone.transform.position;
                Quaternion backRotation = Quaternion.LookRotation(backUp, -hipsTarget.hips.target.transform.forward) * Quaternion.AngleAxis(90, Vector3.right);

                Vector3 hipsUpDirection = backUp; // Vector3.up;
                //if (nextBone != null && nextBone.bone.transform != null)
                //    hipsUpDirection = nextBone.bone.transform.position - bone.transform.position;
                //else
                //if (hipsTarget.humanoid.headTarget.neck.bone.transform != null)
                //    hipsUp = hipsTarget.humanoid.headTarget.neck.bone.transform.position - bone.transform.position;

                Quaternion hipsRotation = Quaternion.LookRotation(hipsUpDirection, target.transform.rotation * Vector3.back) * Quaternion.Euler(90, 0, 0);

                Quaternion hipsTargetRotation = Quaternion.Inverse(backRotation) * hipsRotation;
                bone.baseRotation = hipsTargetRotation;
                return hipsRotation;
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
                return hipsTarget.humanoid.transform;
            }

            public override void MatchTargetToAvatar() {
                if (bone.transform == null || target.transform == null)
                    return;

                target.transform.position = new Vector3(target.transform.position.x, bone.transform.position.y, target.transform.position.z);
                //target.transform.rotation = bone.targetRotation;

                DetermineBasePosition();
                DetermineBaseRotation();
            }
        }
        #endregion

        private void InitSubTargets() {
            hips.hipsTarget = this;

            hips.Init();
            spine.Init();
            chest.Init();
        }

        private void SetTargetPositionsToAvatar() {
            hips.SetTargetPositionToAvatar();
            spine.SetTargetPositionToAvatar();
            chest.SetTargetPositionToAvatar();

            // We need to set neck target here too, because HeadTarget.InitComponent is called later and the chest direction depends on the neck.target.position...
            humanoid.headTarget.neck.SetTargetPositionToAvatar();
        }

        private void DoMeasurements() {
            hips.DoMeasurements();
            spine.DoMeasurements();
            chest.DoMeasurements();
        }
        #endregion

        #region Configuration
        public override Transform GetDefaultTarget(HumanoidControl humanoid) {
            Transform targetTransform = null;
            GetDefaultBone(humanoid.targetsRig, ref targetTransform, HumanBodyBones.Hips);
            return targetTransform;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        public static HipsTarget CreateTarget(HumanoidTarget oldTarget) {
            GameObject targetObject = new GameObject("Hips Target");
            Transform targetTransform = targetObject.transform;
            HumanoidControl humanoid = oldTarget.humanoid;

            targetTransform.parent = oldTarget.humanoid.transform;
            targetTransform.position = oldTarget.transform.position;
            targetTransform.rotation = oldTarget.transform.rotation;

            HipsTarget hipsTarget = targetTransform.gameObject.AddComponent<HipsTarget>();
            hipsTarget.humanoid = humanoid;
            humanoid.hipsTarget = hipsTarget;

            hipsTarget.RetrieveBones();
            hipsTarget.InitAvatar();
            hipsTarget.MatchTargetsToAvatar();
            //hipsTarget.NewComponent(oldTarget.humanoid);
            //hipsTarget.InitComponent();

            return hipsTarget;
        }

        // Do not remove this, this is dynamically called from Target_Editor!
        // Changes the target transform used for this head target
        // Generates a new headtarget component, so parameters will be lost if transform is changed
        public static HipsTarget SetTarget(HumanoidControl humanoid, Transform targetTransform, bool isLeft) {
            HipsTarget currentHipsTarget = humanoid.hipsTarget;
            if (targetTransform == currentHipsTarget.transform)
                return currentHipsTarget;

            GetDefaultBone(humanoid.targetsRig, ref targetTransform, HumanBodyBones.Hips);
            if (targetTransform == null)
                return currentHipsTarget;

            HipsTarget hipsTarget = targetTransform.GetComponent<HipsTarget>();
            if (hipsTarget == null)
                hipsTarget = targetTransform.gameObject.AddComponent<HipsTarget>();

            hipsTarget.NewComponent(humanoid);
            hipsTarget.InitComponent();

            return hipsTarget;
        }

        public void RetrieveBones() {
            hips.RetrieveBones(humanoid);
            spine.RetrieveBones(humanoid);
            chest.RetrieveBones(humanoid);
        }
        #endregion

        #region Settings
        #endregion

        #region Init
        public static bool IsInitialized(HumanoidControl humanoid) {
            if (humanoid.hipsTarget == null || humanoid.hipsTarget.humanoid == null || humanoid.hipsTarget.hips.hipsTarget == null)
                return false;
            if (humanoid.hipsTarget.hips.target.transform == null)
                return false;
            if (humanoid.hipsTarget.hips.bone.transform == null && humanoid.hipsTarget.spine.bone.transform == null && humanoid.hipsTarget.chest.bone.transform == null)
                return false;
            return true;
        }

        private void Reset() {
            humanoid = GetHumanoid();
            if (humanoid == null)
                return;

            NewComponent(humanoid);

            spine.bone.maxAngle = maxSpineAngle;
            chest.bone.maxAngle = maxChestAngle;
        }

        private HumanoidControl GetHumanoid() {
            // This does not work for prefabs
            HumanoidControl[] humanoids = FindObjectsOfType<HumanoidControl>();

            for (int i = 0; i < humanoids.Length; i++) {
                if (humanoids[i].hipsTarget != null && humanoids[i].hipsTarget.transform == this.transform)
                    return humanoids[i];
            }

            return null;
        }

        public override void InitAvatar() {
            InitSubTargets();
            DoMeasurements();

            torsoLength = DetermineTorsoLength();
            spine2HipsRotation = DetermineSpine2HipsRotation();
        }

        public float torsoLength;
        // This is the hipsRotation when the neck is exactly above the hips.
        // This depends on the default curvature of the spine.
        //When the spine is straight, deltaHipRotation = 0
        public Quaternion spine2HipsRotation;

        public override void InitComponent() {
            //bones = new TargetedBone[] { hips, spine, chest };
            //bonesReverse = new TargetedBone[] { chest, spine, hips };

            //foreach (TargetedBone bone in bones)
            //    bone.Init(this);
            InitSubTargets();

            RetrieveBones();

            // We need to do this before the measurements
            //foreach (TargetedBone bone in bones)
            //    bone.SetTargetPositionToAvatar();
            SetTargetPositionsToAvatar();

            // We need the neck.bone to measure the chest length. This can be null when the avatar is changed
            if (humanoid.headTarget.neck.bone.transform == null)
                humanoid.headTarget.neck.RetrieveBones(humanoid);
            //HeadTarget.GetDefaultNeck(humanoid.avatarRig, ref humanoid.headTarget.neck.bone.transform);
            humanoid.headTarget.neck.SetTargetPositionToAvatar();

            //foreach (TargetedBone bone in bones)
            //    bone.DoMeasurements();
            DoMeasurements();

            if (humanoid.headTarget.neck.bone.transform != null && hips.bone.transform != null)
                torsoLength = Vector3.Distance(humanoid.headTarget.neck.bone.transform.position, hips.bone.transform.position);
            else if (humanoid.headTarget.neck.target.transform != null)
                torsoLength = Vector3.Distance(humanoid.headTarget.neck.target.transform.position, hips.target.transform.position);
            else
                return;

            Vector3 spineTop;
            //if (chest.bone.transform != null)
            //    spineTop = chest.bone.transform.position + chest.bone.targetRotation * Vector3.up * chest.bone.length;
            //else
            //    spineTop = hips.bone.transform.position + hips.bone.targetRotation * Vector3.up * torsoLength;

            if (humanoid.headTarget.neck.bone.transform != null)
                spineTop = humanoid.headTarget.neck.bone.transform.position;
            else
                spineTop = humanoid.headTarget.neck.target.transform.position;

            if (hips.bone.transform != null) {
                Vector3 spineDirection = spineTop - hips.bone.transform.position;
                Quaternion spineRotation = Quaternion.LookRotation(spineDirection, humanoid.transform.rotation * Vector3.back) * Quaternion.AngleAxis(90, Vector3.right);
                spine2HipsRotation = Quaternion.Inverse(spineRotation) * hips.bone.targetRotation;
            }
        }

        private float DetermineTorsoLength() {
            if (humanoid.headTarget.neck.bone.transform != null && hips.bone.transform != null)
                return Vector3.Distance(humanoid.headTarget.neck.bone.transform.position, hips.bone.transform.position);
            else if (humanoid.headTarget.neck.target.transform != null)
                return Vector3.Distance(humanoid.headTarget.neck.target.transform.position, hips.target.transform.position);
            else
                return 0.5F;
        }

        private Quaternion DetermineSpine2HipsRotation() {
            Vector3 spineTop;
            //if (chest.bone.transform != null)
            //    spineTop = chest.bone.transform.position + chest.bone.targetRotation * Vector3.up * chest.bone.length;
            //else
            //    spineTop = hips.bone.transform.position + hips.bone.targetRotation * Vector3.up * torsoLength;

            if (humanoid.headTarget.neck.bone.transform != null)
                spineTop = humanoid.headTarget.neck.bone.transform.position;
            else
                spineTop = humanoid.headTarget.neck.target.transform.position;

            if (hips.bone.transform != null) {
                Vector3 spineDirection = spineTop - hips.bone.transform.position;
                Quaternion spineRotation = Quaternion.LookRotation(spineDirection, hips.target.transform.rotation * Vector3.back) * Quaternion.AngleAxis(90, Vector3.right);
                return Quaternion.Inverse(spineRotation) * hips.bone.targetRotation;
            }
            else
                return Quaternion.identity;
        }

        public override void StartTarget() {
            InitSensors();

            torsoMovements.Start(humanoid, this);
        }

        /// <summary>
        /// Checks whether the humanoid has an HipsTarget
        /// and adds one if none has been found
        /// </summary>
        /// <param name="humanoid">The humanoid to check</param>
        public static void DetermineTarget(HumanoidControl humanoid) {
            HipsTarget hipsTarget = humanoid.hipsTarget;

            if (hipsTarget == null) {
                Transform hipsTargetTransform = humanoid.targetsRig.GetBoneTransform(HumanBodyBones.Hips);
                if (hipsTargetTransform == null) {
                    Debug.LogError("Could not find hips bone in targets rig");
                    return;
                }

                hipsTarget = hipsTargetTransform.GetComponent<HipsTarget>();
                if (hipsTarget == null) {
                    hipsTarget = hipsTargetTransform.gameObject.AddComponent<HipsTarget>();
                    hipsTarget.humanoid = humanoid;
                }
            }

            humanoid.hipsTarget = hipsTarget;
        }

        public override void MatchTargetsToAvatar() {
            hips.MatchTargetToAvatar();
            if (main.bone.transform != null && transform != null) {
                transform.position = main.target.transform.position;
                // This is disabled, because the hips rotation is more dependent on the head target
                // than the hips target. Enabling this will make the posing instable.
                transform.rotation = main.target.transform.rotation;
            }
            spine.MatchTargetToAvatar();
            chest.MatchTargetToAvatar();
        }


        #endregion

        #region Update
        public override void UpdateTarget() {
            hips.target.confidence.Degrade();
            spine.target.confidence = Confidence.none;
            chest.target.confidence = Confidence.none;

            UpdateSensors();
        }

        public override void UpdateMovements(HumanoidControl humanoid) {
            TorsoMovements.Update(this);
        }

        public override void CopyTargetToRig() {
            if (Application.isPlaying &&
                humanoid.animatorEnabled && humanoid.targetsRig.runtimeAnimatorController != null)
                return;

            if (hips.target.transform != null && transform != hips.target.transform) {
                hips.target.transform.position = transform.position;
                hips.target.transform.rotation = transform.rotation;
            }
        }

        public override void CopyRigToTarget() {
            if (hips.target.transform != null && transform != hips.target.transform) {
                transform.position = hips.target.transform.position;
                // This is disabled, because the hips rotation is more dependent on the head target
                // than the hips target. Enabling this will make the posing instable.
                transform.rotation = hips.target.transform.rotation;
            }
        }

        public void UpdateSensorsFromTarget() {
            if (sensors == null)
                return;

            for (int i = 0; i < sensors.Length; i++)
                sensors[i].UpdateSensorTransformFromTarget(this.transform);
        }
        #endregion

        #region DrawRigs
        public override void DrawTargetRig(HumanoidControl humanoid) {
            if (this != humanoid.hipsTarget)
                return;

            DrawTarget(hips.target.confidence, hips.target.transform, Vector3.up, hips.target.length);
            DrawTarget(spine.target.confidence, spine.target.transform, Vector3.up, spine.target.length);
            DrawTarget(chest.target.confidence, chest.target.transform, Vector3.up, chest.target.length);
        }

        public override void DrawAvatarRig(HumanoidControl humanoid) {
            if (this != humanoid.hipsTarget)
                return;

            if (chest.bone.transform != null)
                Debug.DrawRay(chest.bone.transform.position, chest.bone.targetRotation * Vector3.up * chest.bone.length, Color.cyan);
            if (spine.bone.transform != null)
                Debug.DrawRay(spine.bone.transform.position, spine.bone.targetRotation * Vector3.up * spine.bone.length, Color.cyan);
            if (hips.bone.transform != null)
                Debug.DrawRay(hips.bone.transform.position, hips.bone.targetRotation * Vector3.up * hips.bone.length, Color.cyan);
        }
        #endregion
    }
}