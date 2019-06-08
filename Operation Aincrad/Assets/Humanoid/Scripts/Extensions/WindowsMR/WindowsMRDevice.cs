#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Passer.Humanoid.Tracking {
    public class WindowsMRDevice {
        public const string name = "Windows MR";

        public static void Start() {
        }

        public struct SensorState {
            public bool tracked;
            public Vector3 position;
            public Quaternion rotation;
            public float positionConfidence;
            public float rotationConfidence;
        }
        private static SensorState hmdState;
        private static SensorState leftControllerState;
        private static SensorState rightControllerState;

        private static List<XRNodeState> nodeStates = new List<XRNodeState>();
        public static void Update() {
            InputTracking.GetNodeStates(nodeStates);

            foreach(XRNodeState nodeState in nodeStates) {
                switch (nodeState.nodeType) {
                    case XRNode.CenterEye:
                        UpdateSensorState(nodeState, ref hmdState);
                        break;
                    case XRNode.LeftHand:
                        UpdateSensorState(nodeState, ref leftControllerState);
                        break;
                    case XRNode.RightHand:
                        UpdateSensorState(nodeState, ref rightControllerState);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void UpdateSensorState(XRNodeState nodeState, ref SensorState sensorState) {
            sensorState.tracked = nodeState.tracked;
            sensorState.positionConfidence = nodeState.TryGetPosition(out sensorState.position) ? 0.9F : 0;
            sensorState.rotationConfidence = nodeState.TryGetRotation(out sensorState.rotation) ? 0.9F : 0;
        }

        public static SensorState GetHmdState() {
            return hmdState;
        }

        public static SensorState GetControllerState(bool isLeft) {
            SensorState controllerState = isLeft ? leftControllerState : rightControllerState;
            return controllerState;
        }
    }
}
#endif