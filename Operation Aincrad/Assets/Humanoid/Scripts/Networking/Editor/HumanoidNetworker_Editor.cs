using UnityEngine;
using UnityEditor;

namespace Passer {

    [CustomEditor(typeof(HumanoidUnet))]
    public class HumanoidNetworker_Editor : Editor {
        public void OnEnable() {
            HumanoidUnet probe = (HumanoidUnet)target;

            //if (probe.humanoid == null)
            //    SetHumanoid(probe);
        }

        private void SetHumanoid(HumanoidUnet probe) {
            //HumanoidControl humanoid = FindObjectOfType<HumanoidControl>();
            //probe.humanoid = humanoid;
        }

    }
}