using UnityEngine;

namespace Passer {
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/head-target/head-input/")]
    public class HeadInput : MonoBehaviour {
        public enum InputType {
            None,
            Animator,
#if PLAYMAKER
            Playmaker,
#endif
            GameObject,
            Humanoid,
            Head
        }
        public HeadTarget headTarget;

#if hFACE
        public InputEvent blinkInput = new InputEvent();
#endif

        public float audioLevel = 0.5F;
        public InputEvent audioInput = new InputEvent();

        public static HeadInput Add(Transform parentTransform) {
            HeadInput headInput = parentTransform.GetComponentInChildren<HeadInput>();
            if (headInput != null)
                return headInput;

            headInput = parentTransform.gameObject.AddComponent<HeadInput>();
            return headInput;
        }

        #region Update
        void Update() {
            if (headTarget == null)
                return;

            UpdateAudio();
#if hFACE
            UpdateBlink();
#endif
        }

#if hFACE
        private void UpdateBlink() {
            blinkInput.floatValue = headTarget.face.leftEye.closed;
        }
#endif

        private void UpdateAudio() {
            audioInput.floatTriggerLevel = audioLevel;
            audioInput.floatValue = headTarget.audioEnergy;
        }
        #endregion
    }
}