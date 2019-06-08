using UnityEngine;

namespace Passer {
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/foot-target/foot-input/")]
    public class FootInput : MonoBehaviour {
        public enum InputType {
            None,
            Animator,
#if PLAYMAKER
            Playmaker,
#endif
            GameObject,
            Humanoid,
            Foot
        }

        public InputEvent[] events = new InputEvent[2];

        public InputEvent hitGround = new InputEvent();

        public FootTarget footTarget;

        private void Awake() {
            footTarget = GetComponent<FootTarget>();
        }

        void Update() {
            if (footTarget == null)
                return;

            UpdateEvents();
        }

        bool wasGrounded = false;
        private void UpdateEvents() {
            if (!wasGrounded && footTarget.ground != null) {
                wasGrounded = true;
                hitGround.boolValue = wasGrounded;

            } else if (wasGrounded && footTarget.ground == null) {
                wasGrounded = false;
                hitGround.boolValue = wasGrounded;
            }
        }
    }
}