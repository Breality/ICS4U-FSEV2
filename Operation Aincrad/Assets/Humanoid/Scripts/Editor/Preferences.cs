#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Passer {
    using Humanoid;
    using Humanoid.Tracking;

    [InitializeOnLoad]
    public class HumanoidConfiguration : MonoBehaviour {
        static HumanoidConfiguration() {
            //Debug.Log("Initializing Humanoid Dlls");
#if hLEAP
            LeapDevice.LoadDlls();
#endif
#if hORBBEC
            AstraDevice.LoadDlls();
#endif
#if hNEURON
            NeuronDevice.LoadDlls();
#endif
        }
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;
        private static string configurationString = "DefaultConfiguration";
        public static Configuration configuration;

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("Humanoid")]
        public static void PreferencesGUI() {

            if (!prefsLoaded) {
                string humanoidPath = Configuration_Editor.FindHumanoidFolder();
                configurationString = EditorPrefs.GetString("HumanoidConfigurationKey", "DefaultConfiguration");

                LoadConfiguration(configurationString);
                if (configuration == null) {
                    configurationString = "DefaultConfiguration";
                    LoadConfiguration(configurationString);
                    if (configuration == null) {
                        Debug.Log("Created new Default Configuration");
                        // Create new Default Configuration
                        configuration = ScriptableObject.CreateInstance<Configuration>();
                        humanoidPath = humanoidPath.Substring(0, humanoidPath.Length - 1); // strip last /
                        humanoidPath = humanoidPath.Substring(0, humanoidPath.LastIndexOf('/') + 1); // strip Scripts;
                        string path = "Assets" + humanoidPath + configurationString + ".asset";
                        AssetDatabase.CreateAsset(configuration, path);
                    }
                }
                prefsLoaded = true;
            }

            configuration = (Configuration)EditorGUILayout.ObjectField("Configuration", configuration, typeof(Configuration), false);

            bool anyChanged = Configuration_Editor.ConfigurationGUI(configuration);

            if (GUI.changed) {
                configurationString = configuration.name;
                EditorPrefs.SetString("HumanoidConfigurationKey", configurationString);
            }

            if (GUI.changed || anyChanged) {
                EditorUtility.SetDirty(configuration);
                Configuration_Editor.CheckExtensions(configuration);                
            }
        }

        private static void LoadConfiguration(string configurationName) {
            string[] foundAssets = AssetDatabase.FindAssets(configurationName + " t:Configuration");
            if (foundAssets.Length == 0)
                return;

            string path = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
            configuration = AssetDatabase.LoadAssetAtPath<Configuration>(path);
        }
    }

}
#endif