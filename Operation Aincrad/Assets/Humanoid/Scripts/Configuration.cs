using UnityEngine;

namespace Passer.Humanoid {

    [CreateAssetMenu(menuName = "Humanoid/Configuration", fileName = "HumanoidConfiguration", order = 100)]
    [System.Serializable]
    public class Configuration : ScriptableObject {
        public bool steamVRSupport = true;
        public bool viveTrackerSupport = false;
        public bool oculusSupport = true;
        public bool windowsMRSupport = true;
        public bool vrtkSupport = true;
        public bool neuronSupport = false;
        public bool realsenseSupport = false;
        public bool leapSupport = false;
        public bool kinect1Support = false;
        public bool kinectSupport = false;
        public bool astraSupport = false;
        public bool hydraSupport = false;
        public bool tobiiSupport = false;
        public bool optitrackSupport = false;
        public bool pupilSupport = false;

        public Passer.NetworkingSystems networkingSupport = Passer.NetworkingSystems.None;
    }
}