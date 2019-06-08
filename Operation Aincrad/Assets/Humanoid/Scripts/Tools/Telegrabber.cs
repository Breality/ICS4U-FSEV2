using UnityEngine;

namespace Passer {

    public class Telegrabber : InteractionPointer {
        protected override void Awake() {
            clickInput.SetMethod(GrabObject, InputEvent.EventType.Start);
            base.Awake();
        }

        public void GrabObject() {
            HandTarget handTarget = transform.GetComponentInParent<HandTarget>();
            if (handTarget == null)
                return;

            Rigidbody rigidbodyInFocus = objectInFocus.GetComponentInParent<Rigidbody>();
            if (rigidbodyInFocus != null)
                HandInteraction.GrabOrLetGo(handTarget, rigidbodyInFocus.gameObject, false);
        }
    }
}