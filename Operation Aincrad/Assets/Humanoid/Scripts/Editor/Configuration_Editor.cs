using System.IO;
using UnityEditor;
using UnityEngine;

namespace Passer.Humanoid {

    [CustomEditor(typeof(Configuration))]
    public class Configuration_Editor : Editor {
        private Configuration configuration;

        private static string humanoidPath;

        private const string vivetrackerPath = "Extensions/ViveTrackers/ViveTracker.cs";
        private const string steamVRPath = "Extensions/SteamVR/SteamVR.cs";
        private const string oculusPath = "Extensions/Oculus/Oculus.cs";
        private const string windowsMRPath = "Extensions/WindowsMR/WindowsMR.cs";
        private const string vrtkPath = "Extensions/VRTK/Vrtk.cs";
        private const string neuronPath = "Extensions/PerceptionNeuron/PerceptionNeuron.cs";
        private const string realsensePath = "Extensions/IntelRealsense/IntelRealsense.cs";
        private const string leapPath = "Extensions/LeapMotion/LeapMotion.cs";
        private const string kinect1Path = "Extensions/MicrosoftKinect1/MicrosoftKinect1.cs";
        private const string kinectPath = "Extensions/MicrosoftKinect2/MicrosoftKinect2.cs";
        private const string astraPath = "Extensions/OrbbecAstra/OrbbecAstra.cs";
        private const string hydraPath = "Extensions/RazerHydra/RazerHydra.cs";
        private const string tobiiPath = "Extensions/Tobii/Tobii.cs";
        private const string optitrackPath = "Extensions/OptiTrack/OptiTrack.cs";
        private const string pupilPath = "Extensions/Pupil/PupilTracker.cs";

        private const string facePath = "FaceControl/EyeTarget.cs";

        #region Enable 
        public void OnEnable() {
            configuration = (Configuration)target;

            humanoidPath = FindHumanoidFolder();
        }

        public static string FindHumanoidFolder() {
            // Path is correct
            if (IsFileAvailable("HumanoidControl.cs"))
                return humanoidPath;

            // Determine in which (sub)folder HUmanoid Control has been placed
            // This makes it possible to place Humanoid Control is a different folder
            string[] hcScripts = AssetDatabase.FindAssets("HumanoidControl");
            for (int i = 0; i < hcScripts.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(hcScripts[i]);
                if (assetPath.Length > 36 && assetPath.Substring(assetPath.Length - 27, 27) == "/Scripts/HumanoidControl.cs") {
                    humanoidPath = assetPath.Substring(6, assetPath.Length - 24);
                    return humanoidPath;
                }
            }

            // Defaulting to standard folder
            humanoidPath = "/Humanoid/Scripts/";
            return humanoidPath;
        }
        #endregion

        public override void OnInspectorGUI() {
            bool anyChanged = ConfigurationGUI(configuration);
            if (GUI.changed || anyChanged) {
                EditorUtility.SetDirty(configuration);
            }

        }

        public static bool ConfigurationGUI(Configuration configuration) {
            bool anyChanged = false;

            FindHumanoidFolder();

            // Preferences GUI
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            configuration.steamVRSupport = IsFileAvailable(steamVRPath) && EditorGUILayout.Toggle("SteamVR Support", configuration.steamVRSupport);
            EditorGUI.BeginDisabledGroup(!configuration.steamVRSupport);
            configuration.viveTrackerSupport = IsFileAvailable(vivetrackerPath) && EditorGUILayout.Toggle("Vive Tracker Support", configuration.viveTrackerSupport);
            EditorGUI.EndDisabledGroup();
#else
            if (configuration.steamVRSupport | configuration.viveTrackerSupport)
                anyChanged = true;
            configuration.steamVRSupport = false;
            configuration.viveTrackerSupport = false;
#endif

#if UNITY_STANDALONE_WIN || UNITY_ANDROID
            configuration.oculusSupport = IsFileAvailable(oculusPath) && EditorGUILayout.Toggle("Oculus Support", configuration.oculusSupport);
#else
            if (configuration.oculusSupport)
                anyChanged = true;
            configuration.oculusSupport = false;
#endif

#if UNITY_WSA_10_0
            configuration.windowsMRSupport = IsFileAvailable(windowsMRPath) && EditorGUILayout.Toggle("Windows MR Support", configuration.windowsMRSupport);
#else
            if (configuration.windowsMRSupport)
                anyChanged = true;
            configuration.windowsMRSupport = false;
#endif
            configuration.vrtkSupport = IsFileAvailable(vrtkPath) && VrtkConfiguration(configuration.vrtkSupport);

#if UNITY_STANDALONE_WIN
            configuration.astraSupport = IsFileAvailable(astraPath) && AstraConfiguration(configuration.astraSupport);
            configuration.realsenseSupport = IsFileAvailable(realsensePath) && EditorGUILayout.Toggle("Intel RealSense Support", configuration.realsenseSupport);
            configuration.pupilSupport = IsFileAvailable(pupilPath) && PupilConfiguration(configuration.pupilSupport);
#else
            if (configuration.realsenseSupport || configuration.pupilSupport)
                anyChanged = true;
            configuration.realsenseSupport = false;
            configuration.pupilSupport = false;
#endif

#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            configuration.neuronSupport = IsFileAvailable(neuronPath) && NeuronConfiguration(configuration.neuronSupport);

            configuration.leapSupport = IsFileAvailable(leapPath) && LeapConfiguration(configuration.leapSupport);
            configuration.kinect1Support = IsFileAvailable(kinect1Path) && Kinect1Configuration(configuration.kinect1Support);
            configuration.kinectSupport = IsFileAvailable(kinectPath) && KinectConfiguration(configuration.kinectSupport);
            configuration.hydraSupport = IsFileAvailable(hydraPath) && HydraConfiguration(configuration.hydraSupport);
            configuration.tobiiSupport = IsFileAvailable(facePath) && IsFileAvailable(tobiiPath) && TobiiConfiguration(configuration.tobiiSupport);
            configuration.optitrackSupport = IsFileAvailable(optitrackPath) && OptitrackConfiguration(configuration.optitrackSupport);
#else
            if (configuration.neuronSupport ||
                configuration.realsenseSupport ||
                configuration.leapSupport ||
                configuration.kinectSupport ||
                configuration.hydraSupport || 
                configuration.optitrackSupport) {

                anyChanged = true;
            }
            configuration.neuronSupport = false;

            configuration.leapSupport = false;
            configuration.kinectSupport = false;
            configuration.hydraSupport = false;
            configuration.tobiiSupport = false;
            configuration.optitrackSupport = false;
#endif
            configuration.networkingSupport = (NetworkingSystems)EditorGUILayout.EnumPopup("Networking Support", configuration.networkingSupport);

            return anyChanged;
        }

        private static bool IsFileAvailable(string filePath) {
            string path = Application.dataPath + humanoidPath + filePath;
            bool fileAvailable = File.Exists(path);
            return fileAvailable;
        }

        #region Configurations
        public static bool NeuronConfiguration(bool neuronSupport) {
            return EditorGUILayout.Toggle("Perception Neuron Support", neuronSupport);
        }

        public static bool LeapConfiguration(bool leapSupport) {
            if (!isLeapAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                leapSupport = false;
                leapSupport = EditorGUILayout.Toggle("Leap Motion Support", leapSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Leap Motion Core Assets are not available. Please download the Core Assets using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Leap Motion Unity Core Assets"))
                    Application.OpenURL("https://developer.leapmotion.com/unity");
            }
            else
                leapSupport = EditorGUILayout.Toggle("Leap Motion Support", leapSupport);
            return leapSupport;
        }

        public static bool Kinect1Configuration(bool kinect1Support) {
            return EditorGUILayout.Toggle("Kinect 1 Support", kinect1Support);
        }

        public static bool KinectConfiguration(bool kinectSupport) {
            return EditorGUILayout.Toggle("Kinect 2 Support", kinectSupport);
        }

        public static bool AstraConfiguration(bool astraSupport) {
            if (!isAstraAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                astraSupport = false;
                astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", astraSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Astra SDK is not available. Please download the Astra Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Orbbec Astra SDK"))
                    Application.OpenURL("https://orbbec3d.com/develop/");
            }
            else
                astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", astraSupport);
            return astraSupport;
        }

        public static bool HydraConfiguration(bool hydraSupport) {
            return EditorGUILayout.Toggle("Hydra Support", hydraSupport);
        }
        public static bool TobiiConfiguration(bool tobiiSupport) {
            if (!isTobiiAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                tobiiSupport = false;
                tobiiSupport = EditorGUILayout.Toggle("Tobii Support", tobiiSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Tobii Framework is not available. Please download the Tobii Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Tobii Unity SDK"))
                    Application.OpenURL("http://developer.tobii.com/tobii-unity-sdk/");
            }
            else if (IsFileAvailable(facePath)) //(isFaceTrackingAvailable())
                tobiiSupport = EditorGUILayout.Toggle("Tobii Support", tobiiSupport);
            return tobiiSupport;
        }

        public static bool OptitrackConfiguration(bool optitrackSupport) {
            if (!isOptitrackAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                optitrackSupport = false;
                optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", optitrackSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("OptiTrack Unity plugin is not available. Please download the plugin using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download OptiTrack Unity Plugin"))
                    Application.OpenURL("https://optitrack.com/downloads/plugins.html#unity-plugin");
            }
            else
                optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", optitrackSupport);
            return optitrackSupport;
        }

        public static bool PupilConfiguration(bool pupilSupport) {
            if (!isPupilAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                pupilSupport = false;
                pupilSupport = EditorGUILayout.Toggle("Pupil Labs Support", pupilSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Pupil Labs plugin is not available. Please download the plugin using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Pupil Unity Plugin"))
                    Application.OpenURL("https://github.com/pupil-labs/hmd-eyes/releases/tag/v0.5.1");
            }
            else
                pupilSupport = EditorGUILayout.Toggle("Pupil Labs Support", pupilSupport);
            return pupilSupport;
        }

        public static bool VrtkConfiguration(bool vrtkSupport) {
            if (!isVrtkAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                vrtkSupport = false;
                EditorGUILayout.Toggle("VRTK Supoort", vrtkSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("VRTK is not available. Please download it from the Asset Store.", MessageType.Warning, true);
            }
            else
                vrtkSupport = EditorGUILayout.Toggle("VRTK Support", vrtkSupport);
            return vrtkSupport;
        }
        #endregion

        #region Availability
        private static bool isLeapAvailable() {
            // Location for the Leap Core Assets < v4.2
            string path1 = Application.dataPath + "/Plugins/x86/LeapC.dll";
            string path2 = Application.dataPath + "/Plugins/x86_64/LeapC.dll";
            if (File.Exists(path1) || File.Exists(path2))
                return true;

            // Location for Leap Core Assets >= v4.2
            path1 = Application.dataPath + "/LeapMotion/Core/Plugins/x86/LeapC.dll";
            path2 = Application.dataPath + "/LeapMotion/Core/Plugins/x86_64/LeapC.dll";
            return (File.Exists(path1) || File.Exists(path2));
        }

        private static bool isAstraAvailable() {
            string path1 = Application.dataPath + "/Plugins/x86_64/astra.dll";
            return File.Exists(path1);
        }

        private static bool isTobiiAvailable() {
            string path1 = Application.dataPath + "/Tobii/Framework/TobiiAPI.cs";
            string path2 = Application.dataPath + "/Tobii/Plugins/x64/Tobii.GameIntegration.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        private static bool isOptitrackAvailable() {
            string path1 = Application.dataPath + "/OptiTrack/Scripts/OptitrackStreamingClient.cs";
            string path2 = Application.dataPath + "/OptiTrack/Plugins/x86_64/NatNetLib.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        private static bool isPupilAvailable() {
            string path1 = Application.dataPath + "/pupil_plugin/Scripts/Networking/PupilTools.cs";
            string path2 = Application.dataPath + "/pupil_plugin/Plugins/x86_64/NetMQ.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        private static bool isPhotonAvailable() {
            string path = Application.dataPath + "/Plugins/Photon3Unity3D.dll";
            return File.Exists(path);
        }

        private static bool isPhoton2Available() {
            string path = Application.dataPath + "/Photon/PhotonUnityNetworking/Code/PunClasses.cs";
            return File.Exists(path);
        }

        private static bool isVrtkAvailable() {
            string path = Application.dataPath + "/VRTK/Scripts/Utilities/SDK/VRTK_SDKManager.cs";
            return File.Exists(path);
        }
        #endregion

        #region Extension Checks   

        public static void CheckExtensions(Configuration configuration) {
            configuration.steamVRSupport = CheckExtensionSteamVR(configuration);
            configuration.viveTrackerSupport = CheckExtensionViveTracker(configuration);
            configuration.oculusSupport = CheckExtensionOculus(configuration);
            configuration.windowsMRSupport = CheckExtensionWindowsMR(configuration);
            configuration.vrtkSupport = CheckExtensionVRTK(configuration);
            configuration.neuronSupport = CheckExtensionNeuron(configuration);
            configuration.realsenseSupport = CheckExtensionRealsense(configuration);
            configuration.leapSupport = CheckExtensionLeap(configuration);
            configuration.kinect1Support = CheckExtensionKinect1(configuration);
            configuration.kinectSupport = CheckExtensionKinect(configuration);
            configuration.astraSupport = CheckExtensionAstra(configuration);
            configuration.hydraSupport = CheckExtensionHydra(configuration);
            configuration.tobiiSupport = CheckExtensionTobii(configuration);
            configuration.optitrackSupport = CheckExtensionOptitrack(configuration);
            configuration.pupilSupport = CheckExtensionPupil(configuration);

            CheckExtensionNetworking(configuration);
            CheckFaceTracking(configuration);
        }

        public static bool CheckExtensionSteamVR(Configuration configuration) {
            return CheckExtension(configuration.steamVRSupport, steamVRPath, "hSTEAMVR");
        }

        public static bool CheckExtensionViveTracker(Configuration configuration) {
            return CheckExtension(configuration.viveTrackerSupport, vivetrackerPath, "hVIVETRACKER");
        }

        public static bool CheckExtensionOculus(Configuration configuration) {
            return CheckExtension(configuration.oculusSupport, oculusPath, "hOCULUS");
        }

        public static bool CheckExtensionWindowsMR(Configuration configuration) {
            return CheckExtension(configuration.windowsMRSupport, windowsMRPath, "hWINDOWSMR");
        }

        public static bool CheckExtensionVRTK(Configuration configuration) {
            return CheckExtension(configuration.vrtkSupport, vrtkPath, "hVRTK");
        }

        public static bool CheckExtensionNeuron(Configuration configuration) {
            return CheckExtension(configuration.neuronSupport, neuronPath, "hNEURON");
        }

        public static bool CheckExtensionRealsense(Configuration configuration) {
            return CheckExtension(configuration.realsenseSupport, realsensePath, "hREALSENSE");
        }

        public static bool CheckExtensionLeap(Configuration configuration) {
            if (isLeapAvailable())
                return CheckExtension(configuration.leapSupport, leapPath, "hLEAP");

            GlobalUndefine("hLEAP");
            return false;
        }

        public static bool CheckExtensionKinect1(Configuration configuration) {
            return CheckExtension(configuration.kinect1Support, kinect1Path, "hKINECT1");
        }

        public static bool CheckExtensionKinect(Configuration configuration) {
            return CheckExtension(configuration.kinectSupport, kinectPath, "hKINECT2");
        }

        public static bool CheckExtensionAstra(Configuration configuration) {
            return CheckExtension(configuration.astraSupport, astraPath, "hORBBEC");
        }

        public static bool CheckExtensionHydra(Configuration configuration) {
            return CheckExtension(configuration.hydraSupport, hydraPath, "hHYDRA");
        }

        public static bool CheckExtensionTobii(Configuration configuration) {
            if (isTobiiAvailable())
                return CheckExtension(configuration.tobiiSupport, tobiiPath, "hTOBII");

            GlobalUndefine("hTOBII");
            return false;
        }

        public static bool CheckExtensionOptitrack(Configuration configuration) {
            if (isOptitrackAvailable())
                return CheckExtension(configuration.optitrackSupport, optitrackPath, "hOPTITRACK");

            GlobalUndefine("hOPTITRACK");
            return false;
        }

        public static bool CheckExtensionPupil(Configuration configuration) {
            if (isPupilAvailable())
                return CheckExtension(configuration.pupilSupport, pupilPath, "hPUPIL");

            GlobalUndefine("hPUPIL");
            return false;
        }

        private static void CheckExtensionNetworking(Configuration configuration) {
            if (isPhoton2Available()) {
                GlobalDefine("hPHOTON2");
                GlobalUndefine("hPHOTON1");
            }
            else if (isPhotonAvailable()) {
                GlobalDefine("hPHOTON1");
                GlobalUndefine("hPHOTON2");
            }
            else {
                GlobalUndefine("hPHOTON1");
                GlobalUndefine("hPHOTON2");
            }

            if (configuration.networkingSupport == NetworkingSystems.UnityNetworking)
                GlobalDefine("hNW_UNET");
            else
                GlobalUndefine("hNW_UNET");
#if hPHOTON1 || hPHOTON2
            if (configuration.networkingSupport == NetworkingSystems.PhotonNetworking)
                GlobalDefine("hNW_PHOTON");
            else
                GlobalUndefine("hNW_PHOTON");
#endif
        }

        private static void CheckFaceTracking(Configuration configuration) {
            if (IsFileAvailable(facePath)) {
                GlobalDefine("hFACE");
            }
            else {
                GlobalUndefine("hFACE");
            }
        }

        private static bool CheckExtension(bool enabled, string filePath, string define) {
            if (enabled) {
                if (IsFileAvailable(filePath)) {
                    GlobalDefine(define);
                    return true;
                }
                else {
                    GlobalUndefine(define);
                    return false;
                }

            }
            else {
                GlobalUndefine(define);
                return false;
            }
        }

        public static void GlobalDefine(string name) {
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!scriptDefines.Contains(name)) {
                string newScriptDefines = scriptDefines + " " + name;
                if (EditorUserBuildSettings.selectedBuildTargetGroup != 0)
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }
        }

        public static void GlobalUndefine(string name) {
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (scriptDefines.Contains(name)) {
                int playMakerIndex = scriptDefines.IndexOf(name);
                string newScriptDefines = scriptDefines.Remove(playMakerIndex, name.Length);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }

        }

        #endregion
    }
    public static class CustomAssetUtility {
        public static void CreateAsset<T>() where T : ScriptableObject {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetTypeName = typeof(T).ToString();
            int dotIndex = assetTypeName.LastIndexOf('.');
            if (dotIndex >= 0)
                assetTypeName = assetTypeName.Substring(dotIndex + 1); // leave just text behind '.'
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + assetTypeName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

}