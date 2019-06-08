using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
#if hOCULUS && UNITY_ANDROID
namespace Passer {
using Humanoid.Tracking;

    [System.Serializable]
    public class GearVRController : UnityController {
        private bool isLeft;

        private Humanoid.Tracking.OculusHand gearVrController;
        private OVRInput.Controller controllerID;

        private Controller controllerInput;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            if (gearVrController == null)
                gearVrController = new Humanoid.Tracking.OculusHand(isLeft, humanoid.oculus.oculusDevice);

            if (sensorTransform == null)
                sensorTransform = CreateControllerObject(humanoid, targetTransform);

            controllerInput = Controllers.GetController(0);
        }

        private Transform CreateControllerObject(HumanoidControl humanoid, Transform targetTransform) {
            //Object controllerPrefab = Resources.Load("GearVR Controller");
            //GameObject controllerObject = (GameObject)Object.Instantiate(controllerPrefab);
            GameObject controllerObject = new GameObject();
            Transform sensorTransform = controllerObject.transform;
            sensorTransform.parent = humanoid.oculus.trackerTransform;

            sensorTransform.position = targetTransform.position;
            sensorTransform.rotation = targetTransform.rotation;

            return sensorTransform;
        }

        public override void Update(Transform targetTransform) {
            if (!enabled)
                return;

            controllerID = OVRInput.GetActiveController();
            isLeft = (controllerID == OVRInput.Controller.LTrackedRemote) || (controllerID == OVRInput.Controller.LTouch);

            UpdateTransform(targetTransform);
            UpdateInput();
        }

        private void UpdateTransform(Transform targetTransform) {
            targetTransform.localScale = (Time.realtimeSinceStartup % 10) * Vector3.one;
            Vector controllerPosition = Target.ToVector(OVRInput.GetLocalControllerPosition(controllerID));
            Rotation controllerRotation = Target.ToRotation(OVRInput.GetLocalControllerRotation(controllerID));
            gearVrController.Update(controllerPosition, controllerRotation);

            targetTransform.position = Target.ToVector3(gearVrController.sensorPosition);
            targetTransform.rotation = Target.ToQuaternion(gearVrController.sensorRotation);
        }

        private void UpdateInput() {
            if (controllerInput == null)
                return;

            if (isLeft)
                UpdateInputSide(controllerInput.left);
            else
                UpdateInputSide(controllerInput.right);
        }

        private void UpdateInputSide(ControllerSide controllerInputSide) {
            controllerInputSide.stickHorizontal = Mathf.Clamp(controllerInputSide.stickHorizontal + OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, controllerID).x, -1, 1);
            controllerInputSide.stickVertical = Mathf.Clamp(controllerInputSide.stickVertical + OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, controllerID).y, -1, 1);
            controllerInputSide.stickButton |= OVRInput.Get(OVRInput.Button.PrimaryTouchpad, controllerID);
            controllerInputSide.stickTouch |= OVRInput.Get(OVRInput.Touch.PrimaryTouchpad, controllerID);

            controllerInputSide.up |= (controllerInputSide.stickVertical > 0.3F);
            controllerInputSide.down |= (controllerInputSide.stickVertical < -0.3F);
            controllerInputSide.left |= (controllerInputSide.stickHorizontal < -0.3F);
            controllerInputSide.right |= (controllerInputSide.stickHorizontal > 0.3F);

            controllerInputSide.trigger1 = Mathf.Max(controllerInputSide.trigger1, OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerID) ? 1 : 0.2F);

            controllerInputSide.option |= OVRInput.Get(OVRInput.Button.Back, controllerID);
        }

    }

}    
#endif
*/
