using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(HandTarget))]
    public class HandTarget_Editor : Editor {
        private HandTarget handTarget;
        private HumanoidControl humanoid;

        private TargetProps[] allProps;

        #region Enable
        public void OnEnable() {
            handTarget = (HandTarget)target;

            humanoid = GetHumanoid(handTarget);
            if (humanoid == null)
                return;

            humanoid.InitTargets();

            InitEditors();

            handTarget.InitSensors();

            InitConfiguration(handTarget);
            InitSettings();
            InitOther();

            InitHandPose(handTarget);

            if (!Application.isPlaying)
                SetSensor2Target();
        }

        private void InitEditors() {
            allProps = new TargetProps[] {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
                new SteamVR_Editor.HandTargetProps(serializedObject, handTarget),
#if hVIVETRACKER
                new ViveTracker_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#endif
#if hOCULUS
                new Oculus_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
                new WindowsMR_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hVRTK
                new Vrtk_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hLEAP
                new LeapMotion_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hHYDRA
                new Hydra_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hKINECT1
                new Kinect1_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hKINEC2
                new Kinect2_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hORBBEC
                new Astra_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hNEURON
                new Neuron_Editor.HandTargetProps(serializedObject, handTarget),
#endif
#if hOPTITRACK
                new Optitrack_Editor.HandTargetProps(serializedObject, handTarget),
#endif
            };
        }
        #endregion

        #region Disable
        public void OnDisable() {
            if (humanoid == null) {
                // This target is not connected to a humanoid, so we delete it
                DestroyImmediate(handTarget, true);
                return;
            }

            if (!Application.isPlaying) {
                SetSensor2Target();
            }

            handTarget.poseMixer.Cleanup();
            if (!Application.isPlaying)
                handTarget.poseMixer.ShowPose(humanoid, handTarget.isLeft ? Side.Left : Side.Right);
            handTarget.UpdateMovements(humanoid);
        }

        private void SetSensor2Target() {
            foreach (TargetProps props in allProps)
                props.SetSensor2Target();
        }
        #endregion

        #region Inspector
        public override void OnInspectorGUI() {
            if (handTarget == null || humanoid == null)
                return;

            serializedObject.Update();

            ControllerInspectors(handTarget);
            if (humanoid != null) {
                SubTargetsInspector(handTarget);
                ConfigurationInspector(handTarget);
                serializedObject.ApplyModifiedProperties();
                HandPoseInspector(handTarget);
                serializedObject.Update();
            }
            SettingsInspector(handTarget);

            TouchedObjectInspector(handTarget);
            GrabbedObjectInspector(handTarget);

            InteractionPointerButton(handTarget);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        public static HandTarget Inspector(HandTarget handTarget, string name) {
            if (handTarget == null)
                return handTarget;

            EditorGUILayout.BeginHorizontal();
            Transform defaultTargetTransform = handTarget.GetDefaultTarget(handTarget.humanoid);
            Transform targetTransform = handTarget.transform ?? defaultTargetTransform;

            GUIContent text = new GUIContent(
                name,
                "The transform controlling the " + name
                );
            targetTransform = (Transform)EditorGUILayout.ObjectField(text, targetTransform, typeof(Transform), true);

            if (!Application.isPlaying) {
                if (targetTransform == defaultTargetTransform && GUILayout.Button("Show", GUILayout.MaxWidth(60))) {
                    // Call static method CreateTarget on target
                    handTarget = (HandTarget)handTarget.GetType().GetMethod("CreateTarget").Invoke(null, new object[] { handTarget });
                } else if (targetTransform != handTarget.transform) {
                    handTarget = (HandTarget)handTarget.GetType().GetMethod("SetTarget").Invoke(null, new object[] { handTarget.humanoid, targetTransform, handTarget.isLeft });
                }
            }
            EditorGUILayout.EndHorizontal();
            return handTarget;
        }

        public static HumanoidControl GetHumanoid(HumanoidTarget target) {
            HumanoidControl[] humanoids = GameObject.FindObjectsOfType<HumanoidControl>();
            HumanoidControl foundHumanoid = null;

            for (int i = 0; i < humanoids.Length; i++)
                if (humanoids[i].leftHandTarget.transform == target.transform ||
                    humanoids[i].rightHandTarget.transform == target.transform)
                    foundHumanoid = humanoids[i];

            return foundHumanoid;
        }


        #region Sensors
        public bool showControllers = true;
        private void ControllerInspectors(HandTarget handTarget) {
            showControllers = EditorGUILayout.Foldout(showControllers, "Sensors/Controllers", true);
            if (showControllers) {
                EditorGUI.indentLevel++;

                foreach (TargetProps props in allProps)
                    props.Inspector();

                if (humanoid != null && humanoid.animatorEnabled)
                    handTarget.handAnimator.enabled = EditorGUILayout.ToggleLeft("Procedural animation", handTarget.handAnimator.enabled, GUILayout.MinWidth(80));
                EditorGUI.indentLevel--;
            }
        }

        #endregion

        #region SubTargets
        private bool showSubTargets;
        private void SubTargetsInspector(HandTarget handTarget) {
            showSubTargets = EditorGUILayout.Foldout(showSubTargets, "Sub Targets", true);
            if (showSubTargets) {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Slider("Thumb Curl", handTarget.fingers.thumb.GetCurl(handTarget), -0.5F, 1);
                EditorGUILayout.Slider("Index Finger Curl", handTarget.fingers.index.GetCurl(handTarget), -0.1F, 1);
                EditorGUILayout.Slider("Middle Finger Curl", handTarget.fingers.middle.GetCurl(handTarget), -0.1F, 1);
                EditorGUILayout.Slider("Ring Finger Curl", handTarget.fingers.ring.GetCurl(handTarget), -0.1F, 1);
                EditorGUILayout.Slider("Little Finger Curl", handTarget.fingers.little.GetCurl(handTarget), -0.1F, 1);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Configuration
        private void InitConfiguration(HandTarget handTarget) {
            InitShoulderConfiguration(handTarget.shoulder);
            InitUpperArmConfiguration(handTarget.upperArm);
            InitForearmConfiguration(handTarget.forearm);
            InitHandConfiguration(handTarget.hand);
        }

        private bool showConfiguration;
        private bool showFingerConfiguration;
        private void ConfigurationInspector(HandTarget handTarget) {
            //if (!target.jointLimitations)
            //return;

            //handTarget.RetrieveBones();

            showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration", true);
            if (showConfiguration) {
                EditorGUI.indentLevel++;
                ShoulderConfigurationInspector(ref handTarget.shoulder, handTarget.isLeft);
                UpperArmConfigurationInspector(ref handTarget.upperArm, handTarget.isLeft);
                ForearmConfigurationInspector(ref handTarget.forearm, handTarget.isLeft);
                HandConfigurationInspector(ref handTarget.hand, handTarget.isLeft);
                showFingerConfiguration = EditorGUILayout.Foldout(showFingerConfiguration, "Fingers", true);
                if (showFingerConfiguration) {
                    EditorGUI.indentLevel++;
                    FingersConfigurationInspector(handTarget);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        #region Shoulder
        private SerializedProperty shoulderArmJointLimitations;
        private SerializedProperty shoulderArmLimitationAngle;

        private void InitShoulderConfiguration(HandTarget.TargetedShoulderBone shoulder) {
            shoulderArmJointLimitations = serializedObject.FindProperty("shoulder.bone.jointLimitations");
            shoulderArmLimitationAngle = serializedObject.FindProperty("shoulder.bone.maxAngle");
        }

        private void ShoulderConfigurationInspector(ref HandTarget.TargetedShoulderBone shoulder, bool isLeft) {
            if (shoulder.bone.transform != null)
                GUI.SetNextControlName(shoulder.bone.transform.name + "00");
            shoulder.bone.transform = (Transform)EditorGUILayout.ObjectField("Shoulder Bone", shoulder.bone.transform, typeof(Transform), true);
            if (shoulder.bone.transform != null) {
                EditorGUI.indentLevel++;

                shoulderArmJointLimitations.boolValue = EditorGUILayout.Toggle("Joint Limitations", shoulder.bone.jointLimitations);
                if (shoulder.bone.jointLimitations) {
                    shoulderArmLimitationAngle.floatValue = EditorGUILayout.Slider("Max Angle", shoulder.bone.maxAngle, 0, 180);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void UpdateShoulderBones(HandTarget.TargetedShoulderBone shoulder) {
        }
        #endregion

        #region UpperArm
        private SerializedProperty upperArmJointLimitations;
        private SerializedProperty upperArmLimitationAngle;

        private void InitUpperArmConfiguration(HandTarget.TargetedUpperArmBone upperArm) {
            upperArmJointLimitations = serializedObject.FindProperty("upperArm.bone.jointLimitations");
            upperArmLimitationAngle = serializedObject.FindProperty("upperArm.bone.maxAngle");
        }

        private void UpperArmConfigurationInspector(ref HandTarget.TargetedUpperArmBone upperArm, bool isLeft) {
            if (upperArm.bone.transform != null)
                GUI.SetNextControlName(upperArm.bone.transform.name + "00");
            upperArm.bone.transform = (Transform)EditorGUILayout.ObjectField("Upper Arm Bone", upperArm.bone.transform, typeof(Transform), true);
            if (upperArm.bone.transform != null) {
                EditorGUI.indentLevel++;

                upperArmJointLimitations.boolValue = EditorGUILayout.Toggle("Joint Limitations", upperArm.bone.jointLimitations);
                if (upperArm.bone.jointLimitations) {
                    upperArmLimitationAngle.floatValue = EditorGUILayout.Slider("Max Angle", upperArm.bone.maxAngle, 0, 180);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateUpperArmBones(HandTarget.TargetedUpperArmBone upperArm) {
        }
        #endregion

        #region Forearm
        private SerializedProperty forearmJointLimitations;
        private SerializedProperty forearmLimitationAngle;

        private void InitForearmConfiguration(HandTarget.TargetedForearmBone forearm) {
            forearmJointLimitations = serializedObject.FindProperty("forearm.bone.jointLimitations");
            forearmLimitationAngle = serializedObject.FindProperty("forearm.bone.maxAngle");
        }

        private void ForearmConfigurationInspector(ref HandTarget.TargetedForearmBone forearm, bool isLeft) {
            if (forearm.bone.transform != null)
                GUI.SetNextControlName(forearm.bone.transform.name + "00");
            forearm.bone.transform = (Transform)EditorGUILayout.ObjectField("Forearm Bone", forearm.bone.transform, typeof(Transform), true);
            if (forearm.bone.transform != null) {
                EditorGUI.indentLevel++;

                forearmJointLimitations.boolValue = EditorGUILayout.Toggle("Joint Limitations", forearm.bone.jointLimitations);
                if (forearm.bone.jointLimitations) {
                    forearmLimitationAngle.floatValue = EditorGUILayout.Slider("Max Angle", forearm.bone.maxAngle, 0, 180);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void UpdateForearmBones(HandTarget.TargetedForearmBone forearm) {
        }
        #endregion

        #region Hand
        private SerializedProperty handJointLimitations;
        private SerializedProperty handLimitationAngle;

        private void InitHandConfiguration(HandTarget.TargetedHandBone hand) {
            handJointLimitations = serializedObject.FindProperty("hand.bone.jointLimitations");
            handLimitationAngle = serializedObject.FindProperty("hand.bone.maxAngle");
        }


        private void HandConfigurationInspector(ref HandTarget.TargetedHandBone hand, bool isLeft) {
            hand.bone.transform = (Transform)EditorGUILayout.ObjectField("Hand Bone", hand.bone.transform, typeof(Transform), true);
            if (hand.bone.transform != null) {
                EditorGUI.indentLevel++;

                handJointLimitations.boolValue = EditorGUILayout.Toggle("Joint Limitations", hand.bone.jointLimitations);
                if (hand.bone.jointLimitations) {
                    handLimitationAngle.floatValue = EditorGUILayout.Slider("Max Angle", hand.bone.maxAngle, 0, 180);
                }

                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Fingers
        SerializedProperty proximalMinX;
        SerializedProperty proximalMaxX;
        private void InitFingerConfiguration(FingersTarget fingers) {
        }

        private void FingersConfigurationInspector(HandTarget handTarget) {
            FingerConfigurationInspector(handTarget, handTarget.fingers.thumb, 0, "Proximal Thumb Bone");
            FingerConfigurationInspector(handTarget, handTarget.fingers.index, 1, "Proximal Index Bone");
            FingerConfigurationInspector(handTarget, handTarget.fingers.middle, 2, "Proximal Middle Bone");
            FingerConfigurationInspector(handTarget, handTarget.fingers.ring, 3, "Proximal Ring Bone");
            FingerConfigurationInspector(handTarget, handTarget.fingers.little, 4, "Proximal Little Bone");
        }

        private void FingerConfigurationInspector(HandTarget handTarget, FingersTarget.TargetedFinger digit, int fingerIndex, string label) {
            Transform proximalBone = (Transform)EditorGUILayout.ObjectField(label, digit.proximal.bone.transform, typeof(Transform), true);
            if (proximalBone == null || proximalBone != digit.proximal.bone.transform) {
                if (proximalBone == null)
                    proximalBone = FingersTarget.GetFingerBone(handTarget, (Finger)fingerIndex, Humanoid.Tracking.FingerBones.Proximal);
                if (proximalBone != null && proximalBone.childCount == 1) {
                    digit.intermediate.bone.transform = proximalBone.GetChild(0);
                    if (digit.intermediate.bone.transform != null && digit.intermediate.bone.transform.childCount == 1)
                        digit.distal.bone.transform = digit.intermediate.bone.transform.GetChild(0);
                    else
                        digit.distal.bone.transform = null;
                } else
                    digit.intermediate.bone.transform = null;
            }
            digit.proximal.bone.transform = proximalBone;
        }
        #endregion


        #endregion

        #region Settings
        private SerializedProperty physicsProp;
        private SerializedProperty rotationSpeedLimitationProp;
        private SerializedProperty touchInteractionProp;

        private void InitSettings() {
            // Cannot use serializedPropery because showRealObjects is a getter/setter
            //showRealObjectsProp = serializedObject.FindProperty("showRealObjects");

            physicsProp = serializedObject.FindProperty("physics");
            rotationSpeedLimitationProp = serializedObject.FindProperty("rotationSpeedLimitation");
            touchInteractionProp = serializedObject.FindProperty("touchInteraction");
        }

        public bool showSettings;
        private void SettingsInspector(HandTarget handTarget) {
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);
            if (showSettings) {
                EditorGUI.indentLevel++;

                // Cannot use serializedPropery because showRealObjects is a getter/setter
                bool showRealObjects = EditorGUILayout.Toggle("Show Real Objects", handTarget.showRealObjects);
                if (showRealObjects != handTarget.showRealObjects) {
                    handTarget.showRealObjects = showRealObjects;
                }

                rotationSpeedLimitationProp.boolValue = EditorGUILayout.Toggle("Rotation Speed Limitation", rotationSpeedLimitationProp.boolValue);
                touchInteractionProp.boolValue = EditorGUILayout.Toggle("Touch Interaction", touchInteractionProp.boolValue);

                physicsProp.boolValue = EditorGUILayout.Toggle("Physics", physicsProp.boolValue);
                if (handTarget.humanoid.physics) {
                    handTarget.strength = EditorGUILayout.FloatField("Strength", handTarget.strength);
                }

                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region HandPose
        public static BonePose selectedBone = null;
        private string[] poseNames;

        private void InitHandPose(HandTarget handTarget) {
        }

        private bool showHandPoses = false;
        private void HandPoseInspector(HandTarget handTarget) {
            EditorGUILayout.BeginHorizontal();
            showHandPoses = EditorGUILayout.Foldout(showHandPoses, "Hand Pose", true);

            EditorGUI.indentLevel++;
            GetPoseNames();
            int newPose = EditorGUILayout.Popup(handTarget.pose, poseNames);
            if (newPose != handTarget.pose) {
                handTarget.pose = newPose;
                handTarget.poseMixer.SetPoseValue(handTarget.poseMixer.mixedPoses[newPose]);
                SceneView.RepaintAll();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndHorizontal();
            if (showHandPoses) {
                EditorGUI.indentLevel++;
                Pose_Editor.PoseMixerInspector(handTarget.poseMixer, humanoid);
                EditorGUI.indentLevel--;
            }

            if (!Application.isPlaying)
                handTarget.poseMixer.ShowPose(humanoid, handTarget.isLeft ? Side.Left : Side.Right);
        }

        private void GetPoseNames() {
            poseNames = new string[handTarget.poseMixer.mixedPoses.Count];//.Length];
            for (int i = 0; i < poseNames.Length; i++) {
                if (handTarget.poseMixer.mixedPoses[i].pose == null)
                    poseNames[i] = "";
                else
                    poseNames[i] = handTarget.poseMixer.mixedPoses[i].pose.name;
            }

        }
        #endregion  

        #region Other
        private SerializedProperty grabbedObjectProp;
        private void InitOther() {
            grabbedObjectProp = serializedObject.FindProperty("grabbedObject");
        }

        private void TouchedObjectInspector(HandTarget handTarget) {
            handTarget.touchedObject = (GameObject)EditorGUILayout.ObjectField("Touched Object", handTarget.touchedObject, typeof(GameObject), true);
        }

        private void GrabbedObjectInspector(HandTarget handTarget) {
            GameObject grabbedObject = (GameObject)EditorGUILayout.ObjectField("Grabbed Object", grabbedObjectProp.objectReferenceValue, typeof(GameObject), true);

            if (grabbedObject != grabbedObjectProp.objectReferenceValue) {
                if (grabbedObject != null)
                    GrabObject(handTarget, grabbedObject);
                else
                    LetGoObject(handTarget);
            }
        }

        private void GrabObject(HandTarget handTarget, GameObject obj) {
            if (handTarget.otherHand.grabbedObject == obj)
                LetGoObject(handTarget.otherHand);

            HandInteraction.Grab(handTarget, obj, false);
            if (handTarget.grabbedObject == null)
                Debug.LogWarning("Could not grab object");
            else {
                Handle handle = obj.GetComponent<Handle>();
                if (handle == null) {
                    BarHandle.Create(obj, handTarget);
                }
                //handTarget.hand.MatchTargetToAvatar();
                //ArmMovements.Update(handTarget);
                //handTarget.CopyRigToTarget();
            }
        }

        private void LetGoObject(HandTarget handTarget) {
            HandInteraction.LetGo(handTarget);            
            grabbedObjectProp.objectReferenceValue = null;
        }

        #endregion

        #region Buttons
        private void InteractionPointerButton(HandTarget handTarget) {
            InteractionPointer interactionPointer = handTarget.transform.GetComponentInChildren<InteractionPointer>();
            if (interactionPointer != null)
                return;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Interaction Pointer"))
                AddInteractionPointer();
            if (GUILayout.Button("Add Teleporter"))
                AddTeleporter();
            GUILayout.EndHorizontal();
        }

        private void AddInteractionPointer() {
            InteractionPointer pointer = HumanoidInteractionPointer.Add(handTarget.transform, InteractionPointer.PointerType.Ray);
            if (handTarget.isLeft) {
                pointer.transform.localPosition = new Vector3(-0.21F, -0.02F, 0.01F);
                pointer.transform.localRotation = Quaternion.Euler(-180, 90, 180);
            } else {
                pointer.transform.localPosition = new Vector3(0.21F, -0.02F, 0.01F);
                pointer.transform.localRotation = Quaternion.Euler(-180, -90, -180);
            }

            //HandInput.Add(handTarget.transform);

            ControllerInput controllerInput = handTarget.humanoid.GetComponent<ControllerInput>();
            if (controllerInput != null) {
                if (handTarget.isLeft) {
                    controllerInput.leftButtonOneInput.SetMethod(pointer.Activation, InputEvent.EventType.Change);
                    controllerInput.leftTrigger1Input.SetMethod(pointer.Click, InputEvent.EventType.Change);
                } else {
                    controllerInput.rightButtonOneInput.SetMethod(pointer.Activation, InputEvent.EventType.Change);
                    controllerInput.rightTrigger1Input.SetMethod(pointer.Click, InputEvent.EventType.Change);
                }
            }
        }

        private void AddTeleporter() {
            Teleporter teleporter = Teleporter.Add(handTarget.transform);
            if (handTarget.isLeft) {
                teleporter.transform.localPosition = new Vector3(-0.21F, -0.02F, 0.01F);
                teleporter.transform.localRotation = Quaternion.Euler(-180, 90, 180);
            } else {
                teleporter.transform.localPosition = new Vector3(0.21F, -0.02F, 0.01F);
                teleporter.transform.localRotation = Quaternion.Euler(-180, -90, -180);
            }

            //HandInput.Add(handTarget.transform);

            ControllerInput controllerInput = handTarget.humanoid.GetComponent<ControllerInput>();
            if (controllerInput != null) {
                if (handTarget.isLeft) {
                    controllerInput.leftButtonOneInput.SetMethod(teleporter.Activation, InputEvent.EventType.Change);
                    controllerInput.leftTrigger1Input.SetMethod(teleporter.Click, InputEvent.EventType.Change);
                } else {
                    controllerInput.rightButtonOneInput.SetMethod(teleporter.Activation, InputEvent.EventType.Change);
                    controllerInput.rightTrigger1Input.SetMethod(teleporter.Click, InputEvent.EventType.Change);
                }
            }
        }
        #endregion
        #endregion

        #region Scene
        private Vector3 handPosition;
        private Quaternion handLocalRotation;
        public void OnSceneGUI() {
            HandTarget handTarget = (HandTarget)target;
            if (humanoid == null)
                return;

            if (Application.isPlaying)
                return;

            Pose_Editor.UpdateScene(humanoid, handTarget.fingers, handTarget.poseMixer, ref selectedBone);

            handTarget.poseMixer.ShowPose(humanoid, handTarget.side);
            if (humanoid.pose != null) {
                if (humanoid.editPose)
                    humanoid.pose.UpdatePose(humanoid);
                else {
                    humanoid.pose.Show(humanoid);
                    handTarget.CopyRigToTarget();
                }
            }

            // update the target rig from the current hand target
            handTarget.CopyTargetToRig();
            // update the avatar bones from the target rig
            humanoid.UpdateMovements();
            // match the target rig with the new avatar pose
            humanoid.MatchTargetsToAvatar();
            // and update all targets to match the target rig
            humanoid.CopyRigToTargets();

            // Update the sensors to match the updated targets
            humanoid.UpdateSensorsFromTargets();
#if false
            if (!Application.isPlaying) {
                if (handTarget.transform.position != handPosition) {
                    handTarget.hand.target.transform.rotation = handTarget.forearm.bone.targetRotation * handLocalRotation;
                    handTarget.transform.rotation = handTarget.hand.target.transform.rotation;
                } else {
                    handLocalRotation = Quaternion.Inverse(handTarget.forearm.bone.targetRotation) * handTarget.transform.rotation;
                }
                handPosition = handTarget.transform.position;
            }
#endif
        }
        #endregion

        public abstract class TargetProps {
            public SerializedProperty enabledProp;
            public SerializedProperty sensorTransformProp;
            public SerializedProperty sensor2TargetPositionProp;
            public SerializedProperty sensor2TargetRotationProp;

            public HandTarget handTarget;
            public UnityArmSensor sensor;

            public TargetProps(SerializedObject serializedObject, UnityArmSensor _sensor, HandTarget _handTarget, string unitySensorName) {
                enabledProp = serializedObject.FindProperty(unitySensorName + ".enabled");
                sensorTransformProp = serializedObject.FindProperty(unitySensorName + ".sensorTransform");
                sensor2TargetPositionProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetPosition");
                sensor2TargetRotationProp = serializedObject.FindProperty(unitySensorName + ".sensor2TargetRotation");

                handTarget = _handTarget;
                sensor = _sensor;

                sensor.Init(handTarget);
            }

            public virtual void SetSensor2Target() {
                sensor.SetSensor2Target();
            }

            public abstract void Inspector();
        }
    }
}
