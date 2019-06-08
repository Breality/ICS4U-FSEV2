/* UNITY VR extension
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 4.0.0
 * date: September 4, 2016
 * 
 */

namespace Passer {
    using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
    using UnityEngine.VR;
#endif

    [System.Serializable]
    public class UnityVRTracker : Tracker {
        public override void StartTracker(HumanoidControl _humanoid) {
            base.StartTracker(_humanoid);
            UnityVRDevice.Start();
        }

        public override void Calibrate() {
#if UNITY_ANDROID
            InputTracking.Recenter();
#endif
        }

        public override void AdjustTracking(Vector3 v, Quaternion q) {
        }

    }

    public static class UnityVRDevice {
        public static bool started;
        public static bool present;

        public static GameObject trackerObject;
        public static string trackerName = "UnityVR root";

        public static void Start() {
            xrDevice = DetermineLoadedDevice();
#if UNITY_2017_2_OR_NEWER
            present = XRDevice.isPresent;
#else
            present = VRDevice.isPresent;
#endif
            trackerObject = GameObject.Find(trackerName);
            started = true;
        }

        public enum XRDeviceType {
            None,
            Oculus,
            OpenVR,
            WindowsMR
        };
        public static XRDeviceType xrDevice = XRDeviceType.None;

        private static XRDeviceType DetermineLoadedDevice() {
#if UNITY_2017_2_OR_NEWER
            if (XRSettings.enabled) {
                switch (XRSettings.loadedDeviceName) {

#else
            if (VRSettings.enabled) {
                switch (VRSettings.loadedDeviceName) {
#endif
                    case "OpenVR":
                        return XRDeviceType.OpenVR;
                    case "Oculus":
                        return XRDeviceType.Oculus;
                    case "WindowsMR":
                        return XRDeviceType.WindowsMR;
                }
            }
            return XRDeviceType.None;
        }
    }

}