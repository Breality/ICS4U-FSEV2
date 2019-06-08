using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer {
    using Humanoid;

    [CanEditMultipleObjects]
    [HelpURLAttribute("http://passervr.com/documentation/humanoid-control/")]
    [CustomEditor(typeof(HumanoidControl))]
    public class HumanoidControl_Editor : Editor {

        private HumanoidControl humanoid;

        private bool settingAvatar;

        #region Enable
        public void OnEnable() {
            humanoid = (HumanoidControl)target;
            if (humanoid == null)
                return;

            if (humanoid.gameObject.name.EndsWith("_prefab"))
                humanoid.gameObject.name = humanoid.gameObject.name.Substring(0, humanoid.gameObject.name.Length - 7);

#if UNITY_2018_3_OR_NEWER
            while (PrefabUtility.GetPrefabAssetType(humanoid.gameObject) != PrefabAssetType.NotAPrefab) {
                Debug.Log("Unpacking Prefab Instance for Humanoid");
                PrefabUtility.UnpackPrefabInstance(humanoid.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
#else
            if (PrefabUtility.GetPrefabObject(humanoid.gameObject) != null) {
                PrefabType prefabType = PrefabUtility.GetPrefabType(humanoid);
                if (prefabType != PrefabType.Prefab) {
                    // Only when it is not a prefab
                    Debug.Log("Breaking Prefab Instance for Humanoid");
                    PrefabUtility.DisconnectPrefabInstance(humanoid.gameObject);
                }
            }
#endif

            CheckHumanoidId(humanoid);

            CheckAvatar(humanoid);
            InitTargets();
            InitTrackers(humanoid);
            InitPose();
            InitMovement();
            InitSettings();
            InitNetworking();
            SetScriptingOrder((MonoBehaviour)target);
        }

        private void SetScriptingOrder(MonoBehaviour target) {
            MonoScript monoScript = MonoScript.FromMonoBehaviour(target);
            int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

            if (currentExecutionOrder <= 0) {
                MonoImporter.SetExecutionOrder(monoScript, 1000);
            }
        }

        #region HumanoidId
        private static void CheckHumanoidId(HumanoidControl humanoid) {
            if (humanoid.humanoidId >= 0)
                return;

            HumanoidControl[] humanoids = FindObjectsOfType<HumanoidControl>();
            for (int i = 0; i < humanoids.Length; i++) {
                humanoids[i].humanoidId = i;
            }
        }
        #endregion
        #endregion

        #region Disable
        public void OnDisable() {
            Cleanup();

            if (Application.isPlaying)
                return;

            HumanoidControl humanoid = (HumanoidControl)target;
            if (humanoid == null)
                return;
        }

        private NetworkIdentity cleanupNetworkIdentity;
        private GameObject cleanupUnetStarter;
#if hPHOTON1 || hPHOTON2
        private PhotonView cleanupPhotonView;
#endif
        private void Cleanup() {
            if (cleanupNetworkIdentity) {
                DestroyImmediate(cleanupNetworkIdentity, true);
                cleanupNetworkIdentity = null;
            }

            if (cleanupUnetStarter) {
                DestroyImmediate(cleanupUnetStarter, true);
                cleanupUnetStarter = null;
            }
#if hPHOTON1 || hPHOTON2
            if (cleanupPhotonView) {
                DestroyImmediate(cleanupPhotonView, true);
                cleanupPhotonView = null;
            }
#endif
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            serializedObject.Update();

            HumanoidControl humanoid = (HumanoidControl)target;

#if hSTEAMVR || hOCULUS || hWINDOWSMR
            if (!PlayerSettings.virtualRealitySupported)
                EditorGUILayout.HelpBox("Virtual Reality is disabled", MessageType.None);
#endif
            if (humanoid.gameObject.name.EndsWith("_prefab"))
                humanoid.gameObject.name = humanoid.gameObject.name.Substring(0, humanoid.gameObject.name.Length - 7);

            if (humanoid.avatarRig == null)
                EditorGUILayout.HelpBox("Could not detect suitable avatar", MessageType.Warning);

            TargetsInspector(humanoid);

            if (humanoid.headTarget == null || humanoid.leftHandTarget == null || humanoid.rightHandTarget == null || humanoid.hipsTarget == null ||
                humanoid.leftFootTarget == null || humanoid.rightFootTarget == null)
                // targets could have been deleted in the hierarchy
                return;

            TrackerInspectors(humanoid);
            PoseInspector();
            NetworkingInspector(humanoid);
            MovementInspector();
            Settings(humanoid);
            Buttons();

            serializedObject.ApplyModifiedProperties();

        }

        #region Avatar
        public static void CheckAvatar(HumanoidControl humanoid) {
            if (humanoid == null)
                return;

            humanoid.avatarRig = humanoid.GetAvatar();
            if (humanoid.avatarRig != null) {
                // these need to be zero to avoid problems with the avatar being at an different position than the player
                if (humanoid.avatarRig.transform != humanoid.transform) {
                    humanoid.avatarRig.transform.localPosition = Vector3.zero;
                    humanoid.avatarRig.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private void SetAvatar(HumanoidControl ivr) {
            if (GUILayout.Button("Set Avatar")) {
                int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
                settingAvatar = true;
                EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "avatar", controlID);
            }
            if (settingAvatar && Event.current.commandName == "ObjectSelectorUpdated") {
                Animator[] animators = ivr.GetComponentsInChildren<Animator>();
                foreach (Animator animator in animators)
                    DestroyImmediate(animator.gameObject);
                GameObject prefab = (GameObject)EditorGUIUtility.GetObjectPickerObject();
                if (prefab != null) {
                    GameObject avatar = Instantiate(prefab);
                    avatar.name = prefab.name;
                    avatar.transform.parent = ivr.transform;
                    avatar.transform.localPosition = Vector3.zero;
                    avatar.transform.localRotation = Quaternion.identity;
                }
            }
            if (settingAvatar && Event.current.commandName == "ObjectSelectorClosed") {
                settingAvatar = false;
            }

        }
        #endregion

        #region Targets
        private void InitTargets() {
            TargetsRigInspector(humanoid);

            if (!HeadTarget.IsInitialized(humanoid) ||
                !HandTarget.IsInitialized(humanoid) ||
                !HipsTarget.IsInitialized(humanoid) ||
                !FootTarget.IsInitialized(humanoid)) {

                humanoid.DetermineTargets();
                humanoid.RetrieveBones();
                humanoid.InitAvatar();
                humanoid.MatchTargetsToAvatar();
            }

            humanoid.InitTargets();
        }

        private static void TargetsRigInspector(HumanoidControl humanoid) {
            HumanoidControl.CheckTargetRig(humanoid);

            if (humanoid.showTargetRig && humanoid.targetsRig.gameObject.hideFlags != HideFlags.None) {
                humanoid.targetsRig.gameObject.hideFlags = HideFlags.None;
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            else if (!humanoid.showTargetRig && humanoid.targetsRig.gameObject.hideFlags != HideFlags.HideInHierarchy) {
                humanoid.targetsRig.gameObject.hideFlags = HideFlags.HideInHierarchy;
                EditorApplication.DirtyHierarchyWindowSorting();
            }
        }

        static bool showTargets;
        private static void TargetsInspector(HumanoidControl humanoid) {
            GUIContent text = new GUIContent(
                "Targets",
                "The target transforms controlling the body parts"
                );
            showTargets = EditorGUILayout.Foldout(showTargets, text, true);

            if (showTargets) {
                EditorGUI.indentLevel++;
                humanoid.headTarget = (HeadTarget)Target_Editor.Inspector(humanoid.headTarget, "Head Target");
                humanoid.leftHandTarget = (HandTarget)HandTarget_Editor.Inspector(humanoid.leftHandTarget, "Left Hand Target");
                humanoid.rightHandTarget = (HandTarget)HandTarget_Editor.Inspector(humanoid.rightHandTarget, "Right Hand Target");
                humanoid.hipsTarget = (HipsTarget)Target_Editor.Inspector(humanoid.hipsTarget, "Hips Target");
                humanoid.leftFootTarget = (FootTarget)FootTarget_Editor.Inspector(humanoid.leftFootTarget, "Left Foot Target");
                humanoid.rightFootTarget = (FootTarget)FootTarget_Editor.Inspector(humanoid.rightFootTarget, "Right Foot Target");
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Trackers
        private HumanoidTargetObjs targetObjs;
        private HumanoidTrackerProps[] allTrackerProps;

        private void InitTrackers(HumanoidControl humanoid) {
            targetObjs = new HumanoidTargetObjs(humanoid);

            allTrackerProps = new HumanoidTrackerProps[] {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                new SteamVR_Editor.TrackerProps(serializedObject, targetObjs, humanoid.steam),
#endif
#if hOCULUS && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                new Oculus_Editor.TrackerProps(serializedObject, targetObjs, humanoid.oculus),
#endif
#if hWINDOWSMR && (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0)
                new WindowsMR_Editor.TrackerProps(serializedObject, targetObjs, humanoid.mixedReality),

#endif
#if hVRTK
                new Vrtk_Editor.TrackerProps(serializedObject, targetObjs, humanoid.vrtk),
#endif
#if hHYDRA && (UNITY_STANDALONE_WIN || (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0))
                new Hydra_Editor.TrackerProps(serializedObject, targetObjs, humanoid.hydra),
#endif
#if hLEAP && (UNITY_STANDALONE_WIN || (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0))
                new LeapMotion_Editor.TrackerProps(serializedObject, targetObjs, humanoid.leapTracker),
#endif
#if hKINECT1 && (UNITY_STANDALONE_WIN || (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0))
                new Kinect1_Editor.TrackerProps(serializedObject, targetObjs, humanoid.kinect1),
#endif
#if hKINECT2 && (UNITY_STANDALONE_WIN || (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0))
                new Kinect2_Editor.TrackerProps(serializedObject, targetObjs, humanoid.kinectTracker),
#endif
#if hORBBEC && (UNITY_STANDALONE_WIN || UNITY_ANDROID)
                new Astra_Editor.TrackerProps(serializedObject, targetObjs, humanoid.astra),
#endif
#if hREALSENSE && (UNITY_STANDALONE_WIN || (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0))
                new Realsense_Editor.TrackerProps(serializedObject, targetObjs, humanoid.realsenseTracker),
#endif
#if hOPTITRACK && (UNITY_STANDALONE_WIN)
                new Optitrack_Editor.TrackerProps(serializedObject, targetObjs, humanoid.optitrack),
#endif
#if hNEURON && (UNITY_STANDALONE_WIN)
                new Neuron_Editor.TrackerProps(serializedObject, targetObjs, humanoid.neuronTracker),
#endif
#if hTOBII && (UNITY_STANDALONE_WIN)
                new Tobii_Editor.TrackerProps(serializedObject, targetObjs, humanoid.tobiiTracker),
#endif
#if hDLIB && (UNITY_STANDALONE_WIN)
                new Tracking.Dlib_Editor.TrackerProps(serializedObject, targetObjs, humanoid.dlib),
#endif
#if hPUPIL && (UNITY_STANDALONE_WIN)
                new Tracking.Pupil.Pupil_Editor.TrackerProps(serializedObject, targetObjs, humanoid.pupil),
#endif
            };

            InitAnimations(humanoid);
        }

        private bool showTrackers = true;
        private void TrackerInspectors(HumanoidControl humanoid) {
            GUIContent text = new GUIContent(
                "Input",
                "Activate supported tracking devices for this humanoid"
                );
            showTrackers = EditorGUILayout.Foldout(showTrackers, text, true);

            if (showTrackers) {
                EditorGUI.indentLevel++;
                UnityVR_Editor.Inspector(humanoid);

                if (targetObjs != null)
                    targetObjs.Update();
                if (allTrackerProps != null) {
                    foreach (HumanoidTrackerProps props in allTrackerProps)
                        props.Inspector(humanoid);
                }
                if (targetObjs != null)
                    targetObjs.ApplyModifiedProperties();

                AnimatorInspector(humanoid);
                EditorGUI.indentLevel--;

            }
        }

        #region Animations
        private SerializedProperty animatorEnabledProp;
        private SerializedProperty animatorParamForwardProp;
        private SerializedProperty animatorParamSidewardProp;
        private SerializedProperty animatorParamRotationProp;
        private SerializedProperty animatorParamHeightProp;

        private SerializedProperty animatorControllerProp;

        private void InitAnimations(HumanoidControl humanoid) {
            animatorEnabledProp = serializedObject.FindProperty("animatorEnabled");
            if (animatorEnabledProp == null)
                Debug.Log(animatorControllerProp);
            animatorParamForwardProp = serializedObject.FindProperty("animatorParameterForwardIndex");
            animatorParamSidewardProp = serializedObject.FindProperty("animatorParameterSidewardIndex");
            animatorParamRotationProp = serializedObject.FindProperty("animatorParameterRotationIndex");
            animatorParamHeightProp = serializedObject.FindProperty("animatorParameterHeightIndex");

            animatorControllerProp = serializedObject.FindProperty("animatorController");
        }

        bool showAnimatorParameters = false;
        private void AnimatorInspector(HumanoidControl humanoid) {
            AnimatorControllerInspector(humanoid);
            AnimatorParametersInspector(humanoid);
        }

        private void AnimatorControllerInspector(HumanoidControl humanoid) {
            EditorGUILayout.BeginHorizontal();

            GUIContent text = new GUIContent(
                "Animator",
                "Standard Unity Animator Controller for animating the character"
                );
            animatorEnabledProp.boolValue = EditorGUILayout.ToggleLeft(text, animatorEnabledProp.boolValue, GUILayout.Width(120));

            if (animatorEnabledProp.boolValue) {
                if (humanoid.targetsRig.runtimeAnimatorController != null)
                    showAnimatorParameters = EditorGUILayout.Foldout(showAnimatorParameters, "Params", true);
                animatorControllerProp.objectReferenceValue = (RuntimeAnimatorController)EditorGUILayout.ObjectField(humanoid.targetsRig.runtimeAnimatorController, typeof(RuntimeAnimatorController), true);
                if (animatorControllerProp.objectReferenceValue != humanoid.targetsRig.runtimeAnimatorController)
                    animatorParameterNames = null;
                humanoid.targetsRig.runtimeAnimatorController = (RuntimeAnimatorController)animatorControllerProp.objectReferenceValue;
            }
            else
                humanoid.targetsRig.runtimeAnimatorController = null;
            EditorGUILayout.EndHorizontal();
        }

        private void AnimatorParametersInspector(HumanoidControl humanoid) {
            if (showAnimatorParameters && animatorEnabledProp.boolValue && humanoid.targetsRig.runtimeAnimatorController != null) {
                if (animatorParameterNames == null)
                    animatorParameterNames = GetAnimatorParameters(humanoid);

                EditorGUI.indentLevel++;

                GUIContent forwardSpeedText = new GUIContent(
                    "Forward Speed",
                    "Animator parameter controlling the forward motion animation"
                    );
                animatorParamForwardProp.intValue = SetAnimatorInput(forwardSpeedText, animatorParamForwardProp.intValue, ref humanoid.animatorParameterForward);

                GUIContent sidewardSpeedText = new GUIContent(
                    "Sideward Speed",
                    "Animator parameter controlling the sideward motion animation"
                    );
                animatorParamSidewardProp.intValue = SetAnimatorInput(sidewardSpeedText, animatorParamSidewardProp.intValue, ref humanoid.animatorParameterSideward);

                GUIContent turnSpeedText = new GUIContent(
                    "Turn Speed",
                    "Animator parameter controlling the rotation animation"
                    );
                animatorParamRotationProp.intValue = SetAnimatorInput(turnSpeedText, animatorParamRotationProp.intValue, ref humanoid.animatorParameterRotation);

                GUIContent headHeightText = new GUIContent(
                    "Head Height",
                    "Animation parameter controlling the squatting animation"
                    );
                animatorParamHeightProp.intValue = SetAnimatorInput(headHeightText, animatorParamHeightProp.intValue, ref humanoid.animatorParameterHeight);

                EditorGUI.indentLevel--;
            }
            else
                showAnimatorParameters = false;

        }

        private GUIContent[] animatorParameterNames;
        public GUIContent[] GetAnimatorParameters(HumanoidControl humanoid) {
            if (humanoid == null || humanoid.targetsRig.runtimeAnimatorController == null)
                return null;

            AnimatorControllerParameter[] animatorParameters = humanoid.targetsRig.parameters;
            GUIContent[] fullAnimatorParameterNames = new GUIContent[animatorParameters.Length + 1];
            fullAnimatorParameterNames[0] = new GUIContent(" ");
            int j = 1;
            for (int i = 0; i < animatorParameters.Length; i++)
                if (animatorParameters[i].type == AnimatorControllerParameterType.Float)
                    fullAnimatorParameterNames[j++] = new GUIContent(animatorParameters[i].name);

            GUIContent[] truncatedParameterNames = new GUIContent[j];
            for (int i = 0; i < j; i++)
                truncatedParameterNames[i] = fullAnimatorParameterNames[i];
            return truncatedParameterNames;
        }

        private int SetAnimatorInput(GUIContent label, int parameterIndex, ref string parameterName) {
            if (parameterIndex > animatorParameterNames.Length)
                parameterIndex = 0;
            int newParameterIndex = EditorGUILayout.Popup(label, parameterIndex, animatorParameterNames, GUILayout.MinWidth(80));

            if (newParameterIndex > 0 && newParameterIndex < animatorParameterNames.Length) {
                parameterName = animatorParameterNames[newParameterIndex].text;
            }
            else {
                parameterName = null;
            }
            return newParameterIndex;
        }
        #endregion

        #region Networking
#if hNW_UNET || hNW_PHOTON
        private SerializedProperty remoteAvatarProp;
#endif

        private void InitNetworking() {
#if hNW_UNET || hNW_PHOTON
            remoteAvatarProp = serializedObject.FindProperty("remoteAvatar");
#endif
        }

        private void NetworkingInspector(HumanoidControl humanoid) {
#if hNW_UNET || hNW_PHOTON
            GUIContent text = new GUIContent(
                "Networking",
                "Settings for networking"
                );
            EditorGUILayout.LabelField(text, GUILayout.Width(100));

            EditorGUI.indentLevel++;
            RemoteAvatarInspector();
            EditorGUI.indentLevel--;
#endif
        }

        private void RemoteAvatarInspector() {
            if (humanoid.isRemote)
                return;
#if hNW_UNET || hNW_PHOTON
            GUIContent text = new GUIContent(
                "Remote Avatar",
                "Determines how the avatar looks like on remote clients. Required for networking setups"
                );
            remoteAvatarProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(text, remoteAvatarProp.objectReferenceValue, typeof(GameObject), false);

            GameObject remoteAvatarPrefab = (GameObject)remoteAvatarProp.objectReferenceValue;
            if (remoteAvatarPrefab == null)
                EditorGUILayout.HelpBox("Remote Avatar cannot be null for networking", MessageType.Error);
            else {
                GameObject remoteAvatarTest = (GameObject)Resources.Load(remoteAvatarPrefab.name);
                if (remoteAvatarTest == null)
                    EditorGUILayout.HelpBox(remoteAvatarPrefab.name + " is not located in a Resources folder", MessageType.Error);
            }
#endif
        }
        #endregion
        #endregion

        #region Pose
        private SerializedProperty poseProp;

        private void InitPose() {
            poseProp = serializedObject.FindProperty("pose");

            if (!Application.isPlaying && humanoid.pose != null)
                humanoid.pose.Show(humanoid);
        }

        private void PoseInspector() {
            EditorGUILayout.BeginHorizontal();
            Pose newHumanoidPose = (Pose)EditorGUILayout.ObjectField("Pose", poseProp.objectReferenceValue, typeof(Pose), false);
            if (newHumanoidPose != humanoid.pose) {
                poseProp.objectReferenceValue = newHumanoidPose;
                if (humanoid.pose != null) {
                    humanoid.pose.Show(humanoid);
                    humanoid.CopyRigToTargets();
                    humanoid.UpdateMovements();
                }
            }

            if (!Application.isPlaying && humanoid.pose != null) {
                GUIContent text = new GUIContent(
                    "",
                    "Edit Pose"
                    );
                bool isEdited = EditorGUILayout.Toggle(text, humanoid.editPose, "button", GUILayout.Width(19));
                if (humanoid.editPose != isEdited)
                    SceneView.RepaintAll();
                humanoid.editPose = isEdited;

                if (humanoid.editPose)
                    humanoid.pose.UpdatePose(humanoid);
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Movement
        private SerializedProperty forwardSpeedProp;
        private SerializedProperty backwardSpeedProp;
        private SerializedProperty sidewardSpeedPop;
        private SerializedProperty maxAccelerationProp;
        private SerializedProperty rotationSpeedProp;

        private void InitMovement() {
            forwardSpeedProp = serializedObject.FindProperty("forwardSpeed");
            backwardSpeedProp = serializedObject.FindProperty("backwardSpeed");
            sidewardSpeedPop = serializedObject.FindProperty("sidewardSpeed");
            maxAccelerationProp = serializedObject.FindProperty("maxAcceleration");
            rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
        }

        private bool showMovement = false;
        private void MovementInspector() {
            GUIContent text = new GUIContent(
                "Movement",
                "Settings related to moving the humanoid around"
                );
            showMovement = EditorGUILayout.Foldout(showMovement, text, true);
            if (showMovement) {
                EditorGUI.indentLevel++;
                ForwardSpeedInspector();
                BackwardSpeedInspector();
                SidewardSpeedInspector();
                MaxAccelerationInspector();
                RotationSpeedInspector();
                EditorGUI.indentLevel--;
            }
        }

        private void ForwardSpeedInspector() {
            GUIContent text = new GUIContent(
                "Forward Speed",
                "Maximum forward speed in units(meters)/second"
                );
            forwardSpeedProp.floatValue = EditorGUILayout.FloatField(text, forwardSpeedProp.floatValue);
        }

        private void BackwardSpeedInspector() {
            GUIContent text = new GUIContent(
                "Backward Speed",
                "Maximum backward speed in units(meters)/second"
                );
            backwardSpeedProp.floatValue = EditorGUILayout.FloatField(text, backwardSpeedProp.floatValue);
        }

        private void SidewardSpeedInspector() {
            GUIContent text = new GUIContent(
                "Sideward Speed",
                "Maximum sideward speed in units(meters)/second"
                );
            sidewardSpeedPop.floatValue = EditorGUILayout.FloatField(text, sidewardSpeedPop.floatValue);
        }

        private void MaxAccelerationInspector() {
            GUIContent text = new GUIContent(
                "Maximum Acceleration",
                "Maximum acceleration in units(meters)/second/second, 0 = no maximum acceleration"
                );
            maxAccelerationProp.floatValue = EditorGUILayout.FloatField(text, maxAccelerationProp.floatValue);
        }

        private void RotationSpeedInspector() {
            GUIContent text = new GUIContent(
                "Rotation Speed",
                "Maximum rotational speed in degrees/second"
                );
            rotationSpeedProp.floatValue = EditorGUILayout.FloatField(text, rotationSpeedProp.floatValue);
        }
        #endregion

        #region Settings
        private SerializedProperty showRealObjectsProp;
        private SerializedProperty physicsProp;
        private SerializedProperty hapticsProp;
        private SerializedProperty useGravityProp;
        private SerializedProperty proximitySpeedProp;
        private SerializedProperty scalingProp;
        private SerializedProperty calibrateAtStartProp;
        private SerializedProperty dontDestroyProp;

        private void InitSettings() {
            showRealObjectsProp = serializedObject.FindProperty("showRealObjects");
            physicsProp = serializedObject.FindProperty("physics");
            hapticsProp = serializedObject.FindProperty("haptics");
            useGravityProp = serializedObject.FindProperty("useGravity");
            proximitySpeedProp = serializedObject.FindProperty("proximitySpeed");
            scalingProp = serializedObject.FindProperty("scaling");
            calibrateAtStartProp = serializedObject.FindProperty("calibrateAtStart");
            dontDestroyProp = serializedObject.FindProperty("dontDestroyOnLoad");
        }

        public bool showSettings = false;
        private void Settings(HumanoidControl humanoid) {
            GUIContent text = new GUIContent(
                "Settings",
                "To contract various aspects of the script"
                );
            showSettings = EditorGUILayout.Foldout(showSettings, text, true);

            if (showSettings) {
                EditorGUI.indentLevel++;

                RealWorldObjects(humanoid);

                // Physics cannot bet changes during runtime
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                PhysicsSetting();
                EditorGUI.EndDisabledGroup();

                UseGravitySetting();
                HapticsSetting();
                ProximitySpeedSetting();
                ScalingSetting();
                CalibrateAtStartSetting();
                DontDestroySetting();

                EditorGUI.indentLevel--;
            }
            ShowTrackers(humanoid, showRealObjectsProp.boolValue);
        }

        private void RealWorldObjects(HumanoidControl humanoid) {
            bool lastShowRealObjects = showRealObjectsProp.boolValue;

            GUIContent text = new GUIContent(
                "Show Real Objects",
                "Shows real physical objects like trackers, controllers and camera's at their actual location"
                );
            showRealObjectsProp.boolValue = EditorGUILayout.Toggle(text, lastShowRealObjects);

            if (!lastShowRealObjects && showRealObjectsProp.boolValue) { // we turned real world objects on
                humanoid.leftHandTarget.showRealObjects = true;
                humanoid.leftHandTarget.ShowSensors(true);
                humanoid.rightHandTarget.showRealObjects = true;
                humanoid.rightHandTarget.ShowSensors(true);
            }
            else {
                humanoid.leftHandTarget.ShowSensors(showRealObjectsProp.boolValue && humanoid.leftHandTarget.showRealObjects);
                humanoid.rightHandTarget.ShowSensors(showRealObjectsProp.boolValue && humanoid.rightHandTarget.showRealObjects);
            }
        }

        private void PhysicsSetting() {
            GUIContent text = new GUIContent(
                "Physics",
                "Enables collisions of the humanoid with the environment using the physics engine"
                );
            physicsProp.boolValue = EditorGUILayout.Toggle(text, physicsProp.boolValue);

            if (physicsProp.boolValue && PlayerSettings.virtualRealitySupported && Time.fixedDeltaTime > 0.01F) {
                EditorGUILayout.HelpBox("Project Settings->Time->Fixed Timestep is too high.\nPlease set to 0.01 or smaller for stable physics in VR.", MessageType.Warning);
            }
        }

        private void UseGravitySetting() {
            GUIContent text = new GUIContent(
                "Use Gravity",
                "Implements downward motion when the humanoid is not on solid ground"
                );
            useGravityProp.boolValue = EditorGUILayout.Toggle(text, useGravityProp.boolValue);
        }

        private void HapticsSetting() {
            GUIContent text = new GUIContent(
                "Haptics",
                "Uses haptic feedback when colliding with objects"
                );
            hapticsProp.boolValue = EditorGUILayout.Toggle(text, hapticsProp.boolValue);
        }

        private void ProximitySpeedSetting() {
            GUIContent text = new GUIContent(
                "Proximity Speed",
                "Decreases movement speed when the humanoid is close to static objects to reduce motion sickness"
                );
            proximitySpeedProp.boolValue = EditorGUILayout.Toggle(text, proximitySpeedProp.boolValue);
        }

        private void ScalingSetting() {
            GUIContent text = new GUIContent(
                "Scaling",
                "Determines how differences between the player size and the avatar are resolved"
                );
            scalingProp.intValue = (int)(HumanoidControl.ScalingType)EditorGUILayout.EnumPopup(text, (HumanoidControl.ScalingType)scalingProp.intValue);
        }

        private void CalibrateAtStartSetting() {
            GUIContent text = new GUIContent(
                "Calibrate at Start",
                "Will calibrate the humanoid when the tracking starts."
                );
            calibrateAtStartProp.boolValue = EditorGUILayout.Toggle(text, calibrateAtStartProp.boolValue);
        }

        private void DontDestroySetting() {
            GUIContent text = new GUIContent(
                "Don't Destroy on Load",
                "Ensures that the Humanoid survives a scene change"
                );
            dontDestroyProp.boolValue = EditorGUILayout.Toggle(text, dontDestroyProp.boolValue);
        }

        private void ShowTrackers(HumanoidControl humanoid, bool shown) {
            foreach (Tracker tracker in humanoid.trackers)
                tracker.ShowTracker(shown);
        }
        #endregion

        #region Buttons
        private void Buttons() {
            GUILayout.BeginHorizontal();
            if (Application.isPlaying && GUILayout.Button("Calibrate"))
                humanoid.Calibrate();
            GUILayout.EndHorizontal();
        }
        #endregion
        #endregion

        #region Scene
        public void OnSceneGUI() {
            if (Application.isPlaying)
                return;
            if (humanoid == null)
                return;

            //Debug.Log("A " + humanoid.headTarget.face.nose.top.target.transform.position.y);
            if (humanoid.pose != null) {
                if (humanoid.editPose)
                    humanoid.pose.UpdatePose(humanoid);
                else {
                    humanoid.pose.Show(humanoid);
                    humanoid.CopyRigToTargets();
                }
            }
            //Debug.Log("B " + humanoid.headTarget.face.nose.top.target.transform.position.y);

            // update the avatar bones from the target rig
            humanoid.UpdateMovements();
            // match the target rig with the new avatar pose
            //Debug.Log("C " + humanoid.headTarget.face.nose.top.target.transform.position.y);

            humanoid.MatchTargetsToAvatar();
            //Debug.Log("D " + humanoid.headTarget.face.nose.top.target.transform.position.y);

            // and update all targets to match the target rig
            humanoid.CopyRigToTargets();
            //Debug.Log("E " + humanoid.headTarget.face.nose.top.target.transform.position.y);

            // Update the sensors to match the updated targets
            humanoid.UpdateSensorsFromTargets();
        }
        #endregion

        public static void ShowTracker(GameObject trackerObject, bool enabled) {
            Renderer renderer = trackerObject.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = enabled;
        }

        public abstract class TrackerProps {
            public SerializedProperty enabledProp;
            public SerializedProperty trackerTransfromProp;
            public Tracker tracker;

            public TrackerProps(SerializedObject serializedObject, Tracker _tracker, string trackerName) {
                enabledProp = serializedObject.FindProperty(trackerName + ".enabled");
                trackerTransfromProp = serializedObject.FindProperty(trackerName + ".trackerTransform");

                tracker = _tracker;
            }
        }

        public class HumanoidTargetObjs {
            public SerializedObject headTargetObj;
            public SerializedObject leftHandTargetObj;
            public SerializedObject rightHandTargetObj;
            public SerializedObject hipsTargetObj;
            public SerializedObject leftFootTargetObj;
            public SerializedObject rightFootTargetObj;

            public HumanoidTargetObjs(HumanoidControl humanoid) {
                headTargetObj = new SerializedObject(humanoid.headTarget);
                leftHandTargetObj = new SerializedObject(humanoid.leftHandTarget);
                rightHandTargetObj = new SerializedObject(humanoid.rightHandTarget);
                hipsTargetObj = new SerializedObject(humanoid.hipsTarget);
                leftFootTargetObj = new SerializedObject(humanoid.leftFootTarget);
                rightFootTargetObj = new SerializedObject(humanoid.rightFootTarget);
            }

            public void Update() {
                if (headTargetObj != null)
                    headTargetObj.Update();
                if (leftHandTargetObj != null)
                    leftHandTargetObj.Update();
                if (rightHandTargetObj != null)
                    rightHandTargetObj.Update();
                if (hipsTargetObj != null)
                    hipsTargetObj.Update();
                if (leftFootTargetObj != null)
                    leftFootTargetObj.Update();
                if (rightFootTargetObj != null)
                    rightFootTargetObj.Update();
            }

            public void ApplyModifiedProperties() {
                if (headTargetObj != null)
                    headTargetObj.ApplyModifiedProperties();
                if (leftHandTargetObj != null)
                    leftHandTargetObj.ApplyModifiedProperties();
                if (rightHandTargetObj != null)
                    rightHandTargetObj.ApplyModifiedProperties();
                if (hipsTargetObj != null)
                    hipsTargetObj.ApplyModifiedProperties();
                if (leftFootTargetObj != null)
                    leftFootTargetObj.ApplyModifiedProperties();
                if (rightFootTargetObj != null)
                    rightFootTargetObj.ApplyModifiedProperties();
            }
        }

        public abstract class HumanoidTrackerProps : TrackerProps {
            protected HumanoidTargetObjs targetObjs;

            protected SerializedProperty headSensorProp;
            protected SerializedProperty leftHandSensorProp;
            protected SerializedProperty rightHandSensorProp;
            protected SerializedProperty hipsSensorProp;
            protected SerializedProperty leftFootSensorProp;
            protected SerializedProperty rightFootSensorProp;

            public HumanoidTrackerProps(SerializedObject serializedObject, HumanoidTargetObjs _targetObjs, Tracker _tracker, string trackerName) :
                base(serializedObject, _tracker, trackerName) {
                targetObjs = _targetObjs;

                headSensorProp = targetObjs.headTargetObj.FindProperty(trackerName);
                leftHandSensorProp = targetObjs.leftHandTargetObj.FindProperty(trackerName);
                rightHandSensorProp = targetObjs.rightHandTargetObj.FindProperty(trackerName);
                hipsSensorProp = targetObjs.hipsTargetObj.FindProperty(trackerName);
                leftFootSensorProp = targetObjs.leftFootTargetObj.FindProperty(trackerName);
                rightFootSensorProp = targetObjs.rightFootTargetObj.FindProperty(trackerName);
            }

            public abstract void Inspector(HumanoidControl humanoid);

            public void Inspector(HumanoidControl humanoid, string resourceName) {
                EditorGUILayout.BeginHorizontal();
                bool wasEnabled = enabledProp.boolValue;

                GUIContent text = new GUIContent(
                    tracker.name,
                    "Activate " + tracker.name + " support for this humanoid"
                    );
                enabledProp.boolValue = EditorGUILayout.ToggleLeft(text, tracker.enabled, GUILayout.Width(200));

                if (Application.isPlaying && enabledProp.boolValue)
                    EditorGUILayout.EnumPopup(tracker.status);

                EditorGUILayout.EndHorizontal();

                if (Application.isPlaying)
                    return;

#if UNITY_2018_3_OR_NEWER
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(humanoid);
                if (prefabType != PrefabAssetType.NotAPrefab)
                    return;
#else
                PrefabType prefabType = PrefabUtility.GetPrefabType(humanoid);
                if (prefabType == PrefabType.Prefab)
                    return;
#endif

                if (tracker.humanoid == null)
                    tracker.humanoid = humanoid;

                if (!Application.isPlaying) {
                    if (enabledProp.boolValue)
                        tracker.AddTracker(humanoid, resourceName);
                    else if (wasEnabled) {
                        RemoveTracker();
                    }
                }

                tracker.ShowTracker(humanoid.showRealObjects && enabledProp.boolValue);

                if (!wasEnabled && enabledProp.boolValue)
                    InitControllers();
                else if (wasEnabled && !enabledProp.boolValue)
                    RemoveControllers();

                if (enabledProp.boolValue && !Application.isPlaying) {
                    SetSensors2Target();
                }
            }

            protected virtual void RemoveTracker() {
                if (tracker.trackerTransform == null)
                    return;
                DestroyImmediate(tracker.trackerTransform.gameObject, true);
            }

            public virtual void InitControllers() {
                tracker.enabled = enabledProp.boolValue;
                // this is necessary because the serializedproperty has not been processed yet
                // and is used in InitController

                if (tracker.headSensor != null)
                    tracker.headSensor.InitController(headSensorProp, tracker.humanoid.headTarget);
                if (tracker.leftHandSensor != null)
                    tracker.leftHandSensor.InitController(leftHandSensorProp, tracker.humanoid.leftHandTarget);
                if (tracker.rightHandSensor != null)
                    tracker.rightHandSensor.InitController(rightHandSensorProp, tracker.humanoid.rightHandTarget);
                if (tracker.hipsSensor != null)
                    tracker.hipsSensor.InitController(hipsSensorProp, tracker.humanoid.hipsTarget);
                if (tracker.leftFootSensor != null)
                    tracker.leftFootSensor.InitController(leftFootSensorProp, tracker.humanoid.leftFootTarget);
                if (tracker.rightFootSensor != null)
                    tracker.rightFootSensor.InitController(rightFootSensorProp, tracker.humanoid.rightFootTarget);
            }

            public virtual void RemoveControllers() {
                foreach (UnitySensor sensor in tracker.sensors)
                    RemoveTransform(sensor.sensorTransform);

                if (tracker.headSensor != null)
                    tracker.headSensor.RemoveController(headSensorProp);
                if (tracker.leftHandSensor != null)
                    tracker.leftHandSensor.RemoveController(leftHandSensorProp);
                if (tracker.rightHandSensor != null)
                    tracker.rightHandSensor.RemoveController(rightHandSensorProp);
                if (tracker.hipsSensor != null)
                    tracker.hipsSensor.RemoveController(hipsSensorProp);
                if (tracker.leftFootSensor != null)
                    tracker.leftFootSensor.RemoveController(leftFootSensorProp);
                if (tracker.rightFootSensor != null)
                    tracker.rightFootSensor.RemoveController(rightFootSensorProp);
            }

            private void RemoveTransform(Transform trackerTransform) {
                if (trackerTransform != null)
                    DestroyImmediate(trackerTransform.gameObject, true);
            }

            public virtual void SetSensors2Target() {
                foreach (UnitySensor sensor in tracker.sensors) {
                    sensor.SetSensor2Target();
                }
            }
        }
    }
}
