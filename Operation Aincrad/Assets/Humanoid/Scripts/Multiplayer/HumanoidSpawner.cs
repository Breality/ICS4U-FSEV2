using UnityEngine;

namespace Passer {

    public class HumanoidSpawner : MonoBehaviour {
        public HumanoidControl humanoid;
        protected HumanoidControl[] spawnedHumanoids;
        protected static int nHumanoids;

        public Transform[] spawnPoints;

        public SpawnMethod spawnMethod;
        public enum SpawnMethod {
            SinglePlayer,
            Random,
            RoundRobin
        }
        private static int spawnIndex = 0;

        public virtual void Start() {
            SpawnHumanoid();
        }

        protected virtual HumanoidControl SpawnHumanoid() {
            return Spawn(humanoid, spawnPoints, spawnMethod);
        }

        public static HumanoidControl Spawn(HumanoidControl humanoidPrefab, Transform[] spawnPoints = null, SpawnMethod spawnMethod = SpawnMethod.RoundRobin) {
            if (humanoidPrefab == null || (spawnMethod == SpawnMethod.SinglePlayer && nHumanoids > 0))
                return null;

            GameObject newPlayer = null;
            int spawnPointIndex = FindSpawnPointIndex(spawnPoints, spawnMethod);
            if (spawnPointIndex < 0)
                newPlayer = Instantiate(humanoidPrefab.gameObject);
            else {
                newPlayer = Instantiate(humanoidPrefab.gameObject, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            }


            HumanoidControl humanoid = newPlayer.GetComponent<HumanoidControl>();
            if (humanoid == null) {
                humanoid = AddHumanoidToAvatar(newPlayer);
                if (humanoid == null) {
                    Debug.LogError("Avatar is not a Humanoid!");
                    return null;
                }
            }

            return humanoid;
        }

        private int FindSpawnPointIndex() {
            for (int i = 0; i < spawnPoints.Length; i++) {
                int spawnPointIndex = GetSpawnPointIndex();
                Vector3 spawnPosition = spawnPoints[spawnPointIndex].position;
                if (CheckSpawnPointIsFree(spawnPosition))
                    return spawnPointIndex;
            }
            return -1;
        }

        private static int FindSpawnPointIndex(Transform[] spawnPoints, SpawnMethod spawnMethod) {
            if (spawnPoints == null)
                return -1;

            for (int i = 0; i < spawnPoints.Length; i++) {
                int spawnPointIndex = GetSpawnPointIndex(spawnPoints, spawnMethod);
                if (spawnPointIndex >= 0 && spawnPointIndex < spawnPoints.Length && spawnPoints[spawnPointIndex] != null) {
                    Vector3 spawnPosition = spawnPoints[spawnPointIndex].position;
                    if (CheckSpawnPointIsFree(spawnPosition))
                        return spawnPointIndex;
                }
            }
            return -1;
        }

        private int GetSpawnPointIndex() {
            if (spawnPoints.Length <= 0)
                return -1;

            switch (spawnMethod) {
                case SpawnMethod.RoundRobin:
                    return spawnIndex++ & spawnPoints.Length;

                case SpawnMethod.Random:
                default:
                    return Random.Range(0, spawnPoints.Length - 1);
            }
        }

        private static int GetSpawnPointIndex(Transform[] spawnPoints, SpawnMethod spawnMethod) {
            if (spawnPoints.Length <= 0)
                return -1;

            switch (spawnMethod) {
                case SpawnMethod.RoundRobin:
                    return spawnIndex++ & spawnPoints.Length;

                case SpawnMethod.Random:
                default:
                    return Random.Range(0, spawnPoints.Length - 1);
            }
        }

        private static bool CheckSpawnPointIsFree(Vector3 location) {
            return !Physics.CheckCapsule(location + Vector3.up * 0.3F, location + Vector3.up * 2, 0.2F);
        }

        protected void DestroyHumanoid(HumanoidControl humanoid) {
            Destroy(humanoid.gameObject);
        }

        protected static HumanoidControl AddHumanoidToAvatar(GameObject avatar) {
            Animator animator = avatar.GetComponent<Animator>();
            if (animator == null || !animator.isHuman)
                return null;

            HumanoidControl humanoid = avatar.AddComponent<HumanoidControl>();
            return humanoid;
        }
    }
}