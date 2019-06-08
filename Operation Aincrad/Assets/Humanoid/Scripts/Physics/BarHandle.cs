/* InstantVR
 * author: Pascal Serrarnes
 * email: unity@serrarens.nl
 * version: 1.0.0
 * date: September 12, 2014
 * 
 */

using UnityEngine;

namespace Passer {

    public class BarHandle : Handle {

        void OnDrawGizmos() {
            if (enabled) {
                Matrix4x4 m = Matrix4x4.identity;
                Vector3 p = transform.TransformPoint(position);
                Quaternion q = Quaternion.Euler(rotation);
                m.SetTRS(p, transform.rotation * q, Vector3.one);
                Gizmos.color = Color.yellow;
                Gizmos.matrix = m;

                Gizmos.DrawCube(Vector3.zero, new Vector3(0.03f, 0.10f, 0.04f));
                //	Gizmos.DrawWireSphere(Vector3.zero, range);
            }
        }

        public static void Create(GameObject gameObject, HandTarget handTarget) {
            Handle handle = gameObject.AddComponent<BarHandle>();
            handle.grabType = GrabType.BarGrab;

            handle.rotation = Quaternion.Inverse(Quaternion.Inverse(handTarget.handPalm.rotation * gameObject.transform.rotation)).eulerAngles;
            handle.position = gameObject.transform.InverseTransformPoint(handTarget.handPalm.position);
            handle.handTarget = handTarget;
        }
    }
}