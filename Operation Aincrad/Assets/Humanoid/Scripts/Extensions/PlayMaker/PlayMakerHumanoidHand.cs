using UnityEngine;
using Passer;
using Passer.Humanoid.Tracking;

#if PLAYMAKER

public class PlayMakerHumanoidHand : MonoBehaviour {

    PlayMakerFSM playerFSM;

    void Start() {
        playerFSM = GetComponent<PlayMakerFSM>();
    }

    void OnGrabbing() {
        if (playerFSM) {
            playerFSM.Fsm.Event("Grabbing");
        }
    }

    void OnLettingGo() {
        if (playerFSM) {
            playerFSM.Fsm.Event("LettingGo");
        }
    }
}


namespace HutongGames.PlayMaker.Actions {
    [ActionCategory("Humanoid Control")]
    [Tooltip("Gets the hand pose")]
    public class GetHandPose : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HandTarget))]
        public FsmOwnerDefault handObject;

        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the thumb is bent")]
        public FsmFloat thumbCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the index finger is bent")]
        public FsmFloat indexCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the middle finger is bent")]
        public FsmFloat middleCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the ring finger is bent")]
        public FsmFloat ringCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the little finger is bent")]
        public FsmFloat littleCurl;

        [Tooltip("Repeat every frame. Typically this would be set to True.")]
        public bool everyFrame;

        private HandTarget handTarget;

        public override void Reset() {
            thumbCurl = null;
            indexCurl = null;
            middleCurl = null;
            ringCurl = null;
            littleCurl = null;

            everyFrame = true;

            handTarget = Owner.GetComponent<HandTarget>();
            handTarget = null;
        }

        public override void OnEnter() {
            DoGetHandPose();

            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            DoGetHandPose();
        }

        void DoGetHandPose() {
            if (handTarget == null) {
                handTarget = FsmHandUtils.GetHandTarget(this.Fsm, handObject);
            }

            if (handTarget != null) {
                thumbCurl.Value = handTarget.fingers.thumb.curl;
                indexCurl.Value = handTarget.fingers.index.curl;
                middleCurl.Value = handTarget.fingers.middle.curl;
                ringCurl.Value = handTarget.fingers.ring.curl;
                littleCurl.Value = handTarget.fingers.little.curl;
            }
        }
    }

    [ActionCategory("Humanoid Control")]
    [Tooltip("Sets the hand pose")]
    public class SetHandPose : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HandTarget))]
        public FsmOwnerDefault handObject;

        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the thumb is bent")]
        public FsmFloat thumbCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the index finger is bent")]
        public FsmFloat indexCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the middle finger is bent")]
        public FsmFloat middleCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the ring finger is bent")]
        public FsmFloat ringCurl;
        [UIHint(UIHint.Variable)]
        [Tooltip("The amount the little finger is bent")]
        public FsmFloat littleCurl;

        private HandTarget handTarget;

        public override void Reset() {
            thumbCurl = null;
            indexCurl = null;
            middleCurl = null;
            ringCurl = null;
            littleCurl = null;

            handTarget = null;
        }

        public override void OnUpdate() {
            if (handTarget == null)
                handTarget = FsmHandUtils.GetHandTarget(Fsm, handObject);

            if (handTarget != null) {
                handTarget.SetFingerCurl(Finger.Thumb, thumbCurl.Value);
                handTarget.SetFingerCurl(Finger.Index, indexCurl.Value);
                handTarget.SetFingerCurl(Finger.Middle, middleCurl.Value);
                handTarget.SetFingerCurl(Finger.Ring, ringCurl.Value);
                handTarget.SetFingerCurl(Finger.Little, littleCurl.Value);
            }
        }
    }
    [ActionCategory("Humanoid Control")]
    [Tooltip("Gets object grabbed by the hand (if any)")]
    public class GetGrabbedObject : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HandTarget))]
        public FsmOwnerDefault handObject;

        [UIHint(UIHint.Variable)]
        [Tooltip("The object grabbed by the hand (if any)")]
        public FsmGameObject grabbedObject;

        private HandTarget handTarget;

        public override void Reset() {
            handObject = null;
            handTarget = null;
        }

        public override void OnEnter() {
            DoGetGrabbedObject();

            Finish();
        }

        public override void OnUpdate() {
            DoGetGrabbedObject();
        }

        void DoGetGrabbedObject() {
            if (handTarget == null) {
                handTarget = FsmHandUtils.GetHandTarget(this.Fsm, handObject);
            }

            if (handTarget != null) {
                grabbedObject.Value = handTarget.grabbedObject;
            }
        }

    }

    public static class FsmHandUtils {
        public static HandTarget GetHandTarget(Fsm fsm, FsmOwnerDefault handObject) {
            GameObject obj = fsm.GetOwnerDefaultTarget(handObject);
            if (obj != null) {
                return obj.GetComponent<HandTarget>();
            }
            return null;
        }
    }
}
#endif