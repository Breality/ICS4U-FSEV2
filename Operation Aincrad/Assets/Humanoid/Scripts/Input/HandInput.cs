using UnityEngine;
using Humanoid;

namespace Passer {
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/hand-target/hand-input/")]
    public class HandInput : MonoBehaviour {
        public enum InputType {
            None,
            Animator,
#if PLAYMAKER
            Playmaker,
#endif
            GameObject,
            Humanoid,
            Hand
        }
        public HandTarget handTarget;

        public PoseInput[] handPoseInput = new PoseInput[1];
        public InputEvent touchInput = new InputEvent();
        public InputEvent grabInput = new InputEvent();
        public InputEvent[] controllerInput = new InputEvent[11];

        public static HandInput Add(Transform parentTransform) {
            HandInput handInput = parentTransform.GetComponentInChildren<HandInput>();
            if (handInput != null)
                return handInput;

            handInput = parentTransform.gameObject.AddComponent<HandInput>();
            return handInput;
        }

        #region Update
        void Update() {
            if (handTarget == null)
                return;

            UpdateTouch();
            UpdateGrab();
            UpdateHandPoses();
        }

        private void UpdateHandPoses() {
            for (int i = 0; i < handPoseInput.Length; i++) {
                bool handPoseActive = (handTarget.pose == handPoseInput[i].poseId);
                handPoseInput[i].boolValue = handPoseActive;
            }
        }

        private void UpdateTouch() {
            touchInput.boolValue = (handTarget.touchedObject != null);
        }

        private void UpdateGrab() {
            //grabInput.gameObjectValue = handTarget.grabbedObject;
            grabInput.boolValue = (handTarget.grabbedObject != null);
        }
        #endregion

        [System.Serializable]
        public class PoseInput : Passer.InputEvent {
            public /*HandPoses.PoseId*/int poseId;
        }
    }
}