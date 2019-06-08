/* InstantVR PlayMaker support
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: December 11, 2015
 * 
 * =======================
 * To enable PlayMaker support:
 * - ensure you have PlayMaker imported into your project
 * - add PLAYMAKER to Project Settings -> Player -> Other Settings -> Scripting Define Symbols
 */

#if PLAYMAKER
using UnityEngine;
using Passer;

namespace HutongGames.PlayMaker.Actions {
    [ActionCategory("Humanoid Control")]
    [Tooltip("Avatar walking movement")]
    public class HumanoidMove : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HumanoidControl))]
        [Tooltip("The Humanoid GameObject to move.")]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        [Tooltip("The movement vector. Always local space and per second.")]
        public FsmVector3 moveVector;

        private GameObject previousGo; // remember so we can get new controller only when it changes.
        private HumanoidControl humanoid;

        public override void Reset() {
            gameObject = null;
            moveVector = new FsmVector3 { UseVariable = true };
        }

        public override void OnUpdate() {
            GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null) return;

            if (go != previousGo) {
                humanoid = go.GetComponent<HumanoidControl>();
                previousGo = go;
            }

            if (humanoid != null) {
                humanoid.Move(moveVector.Value);
            }
        }
    }
        
    [ActionCategory("Humanoid Control")]
    [Tooltip("Avatar rotation")]
    public class HumanoidRotate : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HumanoidControl))]
        [Tooltip("The HumanoidControl gameObject to rotate.")]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        [Tooltip("The rotation angle. Always over up(=Y) axis and per second.")]
        public FsmFloat angle;

        private GameObject previousGo; // remember so we can get new controller only when it changes.
        private HumanoidControl humanoid;

        public override void Reset() {
            gameObject = null;
            angle = 0;
        }

        public override void OnUpdate() {
            GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null) return;

            if (go != previousGo) {
                humanoid = go.GetComponent<HumanoidControl>();
                previousGo = go;
            }

            if (humanoid != null) {
                humanoid.Rotate(angle.Value);
            }
        }
    }

    [ActionCategory("Humanoid Control")]
    [Tooltip("Sends an Event when the avatar collides with the environment.")]
    public class CharacterCollision : FsmStateAction {
        [RequiredField]
        [CheckForComponent(typeof(HumanoidControl))]
        [Tooltip("The Humanoid gameObject of the avatar who may collide.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Event to send when the avatar collides.")]
        public FsmEvent collisionStartEvent;

        [Tooltip("Event to send when the avatar no longer collides.")]
        public FsmEvent collisionEndEvent;

        [Tooltip("Set to True when the avatar collides.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public override void Reset() {
            base.Reset();
            gameObject = null;
            collisionStartEvent = null;
            collisionEndEvent = null;
            storeResult = null;
        }

        private GameObject previousGo; // remember so we can get new controller only when it changes.
        private HumanoidControl humanoid;
        public override void OnUpdate() {
            GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null) return;

            if (go != previousGo) {
                humanoid = go.GetComponent<HumanoidControl>();
                previousGo = go;
            }

            if (humanoid.collided && !storeResult.Value)
                Fsm.Event(collisionStartEvent);
            else if (!humanoid.collided && storeResult.Value)
                Fsm.Event(collisionEndEvent);

            storeResult.Value = humanoid.collided;
        }
    }

    [ActionCategory("Humanoid Control")]
    [Tooltip("Get limb information")]
    public class GetBoneInformation : FsmStateAction {

        [RequiredField]
        [CheckForComponent(typeof(HumanoidControl))]
        [Tooltip("The HumanoidControl character.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Which body bone?")]
        public HumanBodyBones bone;

        [UIHint(UIHint.Variable)]
        [Tooltip("The bone position")]
        public FsmVector3 position;
        [UIHint(UIHint.Variable)]
        [Tooltip("The bone rotation")]
        public FsmQuaternion rotation;

        [Tooltip("Repeat every frame. Typically this would be set to True.")]
        public bool everyFrame;

        private GameObject previousGo; // remember so we can get new controller only when it changes.
        private HumanoidControl humanoid;
        private Animator animator;
        private Transform hand;

        public override void Reset() {
            base.Reset();
            bone = HumanBodyBones.Head;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            everyFrame = true;

            hand = null;
        }

        public override void OnEnter() {
            DoGetBoneInfo();

            if (!everyFrame)
                Finish();
        }

        public override void OnUpdate() {
            DoGetBoneInfo();
        }

        void DoGetBoneInfo() {
            if (hand == null)
                hand = GetBone();

            if (hand != null) {
                position.Value = hand.position;
                rotation.Value = hand.rotation;
            }
        }

        private Transform GetBone() {
            GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null)
                return null;

            if (go != previousGo) {
                humanoid = go.GetComponent<HumanoidControl>();
                if (humanoid != null) {
                    animator = humanoid.avatarRig;
                }
                previousGo = go;
            }

            if (animator != null) {
                return animator.GetBoneTransform(bone);
            }
            return null;
        }
    }
}
#endif