using UnityEngine;

namespace Passer {

    public class NetworkingSpawner : NetworkingStarter {
        public HumanoidControl humanoidPrefab;

        public Transform[] spawnPoints;
        public HumanoidSpawner.SpawnMethod spawnMethod;

        public void OnNetworkingStarted() {
            HumanoidSpawner.Spawn(humanoidPrefab, spawnPoints, spawnMethod);
        }
    }
}