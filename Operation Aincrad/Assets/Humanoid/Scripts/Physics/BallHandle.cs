using UnityEngine;
using System.Collections;

namespace Passer {

    public class BallHandle : Handle {

        public float radius = 0.04f;

        void OnDrawGizmos() {
            Matrix4x4 m = Matrix4x4.identity;
            Vector3 p = transform.TransformPoint(position);
            Quaternion q = Quaternion.Euler(rotation);
            m.SetTRS(p, transform.rotation * q, Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.matrix = m;
            Gizmos.DrawSphere(Vector3.zero, radius);
        }
    }
}