using System.Collections;
using UnityEngine;

namespace Passer {
    public enum MovementType {
        Teleport,
        Dash
    }

    public class Transport {

        /// <summary>
        /// Dashes the transform to the target position
        /// This needs to be called using MonoBehaviour.StartCoroutine
        /// </summary>
        /// <param name="transform">The transform to move</param>
        /// <param name="targetPosition">The target position of the dash movement</param>
        /// <param name="duration">Duration of the dash</param>
        /// <param name="minSpeed">Minimum speed for the dash, duration may be shortened to reach this speed</param>
        /// <returns>Coroutine result</returns>
        public static IEnumerator DashCoroutine(Transform transform, Vector3 targetPosition, float duration = 0.1F, float minSpeed = 5) {
            float distance = Vector3.Distance(transform.position, targetPosition - transform.forward * 0.5f);

            float minDistance = minSpeed / 0.1F;
            if (distance < minDistance)
                duration = distance / minSpeed;

            Vector3 startPosition = transform.position;
            float elapsedTime = 0;
            float t = 0;

            while (t < 1) {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                t = elapsedTime / duration;
                yield return new WaitForEndOfFrame();
            }
            transform.position = targetPosition;
        }
    }

    public static class TransformTransport {
        public static void MoveTo(this Transform transform, Vector3 position, MovementType movementType = MovementType.Teleport) {
            switch (movementType) {
                case MovementType.Teleport:
                    TransformMovements.Teleport(transform, position);
                    break;
                case MovementType.Dash:
                    MonoBehaviour monoBehaviour = transform.GetComponent<MonoBehaviour>();
                    if (monoBehaviour == null) {
                        Debug.LogError("Dash not possible. No MonoBehaviour found on " + transform);
                    }
                    else
                        monoBehaviour.StartCoroutine(TransformMovements.DashCoroutine(transform, position));
                    break;
                default:
                    break;
            }

        }
    }
}