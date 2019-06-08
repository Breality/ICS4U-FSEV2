using UnityEngine;
using System.Collections;

namespace Passer {

    public static class Monitor {

        public static void DrawAxis(Transform transform) {
            DrawAxis(transform.position, transform.rotation);
        }

        public static void DrawAxis(Vector3 position, Quaternion rotation) {
            UnityEngine.Debug.DrawRay(position, rotation * Vector3.right * 0.1F, Color.red);
            UnityEngine.Debug.DrawRay(position, rotation * Vector3.up * 0.1F, Color.green);
            UnityEngine.Debug.DrawRay(position, rotation * Vector3.forward * 0.1F, Color.blue);
        }

        public static void DrawAxisYZ(Vector3 position, Quaternion rotation) {
            UnityEngine.Debug.DrawRay(position, rotation * Vector3.up * 0.1F, Color.green);
            UnityEngine.Debug.DrawRay(position, rotation * Vector3.forward * 0.1F, Color.blue);
        }
    }
}
