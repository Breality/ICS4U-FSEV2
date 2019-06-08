using System.Collections;
using UnityEngine;

namespace Passer {

    public class Transportation : MonoBehaviour {        
        public float forwardSpeed = 1;
        public float sidewardSpeed = 1;
        public float rotationalSpeed = 60;

        public virtual void MoveForward(float z) {
            transform.MoveForward(forwardSpeed * z * Time.deltaTime);
        }
        public virtual void MoveSideward(float x) {
            transform.MoveSidewards(sidewardSpeed * x * Time.deltaTime);
        }
        public virtual void Rotate(float angularSpeed) {
            transform.Turn(rotationalSpeed * angularSpeed * Time.deltaTime);
        }

        public void Dash(Vector3 targetPosition) {
            StartCoroutine(TransformMovements.DashCoroutine(transform, targetPosition));
        }
        public void Teleport(Vector3 targetPosition) {
            TransformMovements.Teleport(this.transform, targetPosition);
        }

        public void Teleport() {
            TransformMovements.Teleport(this.transform, this.transform.position + this.transform.forward * 2);
        }
    }

    public static class TransformMovements {
        /// <summary>
        /// Move the transform forward
        /// </summary>
        /// <param name="transform">The transform to move</param>
        /// <param name="z">The distance to move. Negative is moving backwards</param>
        public static void MoveForward(this Transform transform, float z) {
            transform.Translate(Vector3.forward * z);
        }

        /// <summary>
        /// Move the transform sidewards
        /// </summary>
        /// <param name="transform">The transform to move</param>
        /// <param name="x">The distance to move. Positive is move to the right</param>
        public static void MoveSidewards(this Transform transform, float x) {
            transform.Translate(Vector3.right * x);
        }

        /// <summary>
        /// Rotate the transform along the Y axis
        /// </summary>
        /// <param name="transform">The transform to turn</param>
        /// <param name="angle">The angle in degrees. Positive is rotate to the right</param>
        public static void Turn(this Transform transform, float angle) {
            if (angle != 0)
                transform.Rotate(0, angle, 0);
        }

        /// <summary>
        /// Teleport the transform to the target position
        /// </summary>
        /// <param name="transform">The transform to teleport</param>
        /// <param name="targetPosition">The target position to teleport to</param>
        public static void Teleport(this Transform transform, Vector3 targetPosition) {
            transform.position = targetPosition;
        }

        private const float minSpeedMps = 50.0f; // clamped to minimum speed 50m/s to avoid sickness
        private const float normalLerpTime = 0.1f; // 100ms for every dash above minDistanceForNormalLerp
        private const float minDistanceForNormalLerp = minSpeedMps * normalLerpTime; // default values give 5.0f;
        private static float lerpTime = 5; //0.1f;

        /// <summary>
        /// Dashes the transform to the target position
        /// This needs to be called using MonoBehaviour.StartCoroutine
        /// </summary>
        /// <param name="transform">The transform to move</param>
        /// <param name="targetPosition">The target position of the dash movement</param>
        /// <returns>Coroutine result</returns>
        public static IEnumerator DashCoroutine(Transform transform, Vector3 targetPosition) {
            float maxDistance = Vector3.Distance(transform.position, targetPosition - transform.forward * 0.5f);

            if (maxDistance >= minDistanceForNormalLerp)
                lerpTime = normalLerpTime; // fixed time for all bigger dashes
            else
                lerpTime = maxDistance / minSpeedMps; // clamped to speed for small dashes

            Vector3 startPosition = transform.position;
            float elapsedTime = 0;
            float t = 0;

            while (t < 1) {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                t = elapsedTime / lerpTime;
                yield return new WaitForEndOfFrame();
            }
            transform.position = targetPosition;
        }
    }
}
