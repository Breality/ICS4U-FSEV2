using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
#if hNW_PHOTON
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer {

    [CustomEditor(typeof(PhotonStarter))]
    public class PunStarter_Editor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            HumanoidControl[] humanoids = FindObjectsOfType<HumanoidControl>();
            if (humanoids.Length != 1)
                // We only support sitatuation with one humanoid in the scene
                return;

            HumanoidControl humanoid = humanoids[0];

            if (Application.isPlaying)
                return;

            GameObject humanoidPrefab = CheckHumanoidPrefab(humanoid);
            NetworkingComponentsInspectorPun(humanoid, humanoidPrefab);
        }

        private GameObject CheckHumanoidPrefab(HumanoidControl humanoid) {
            GameObject humanoidPrefab = Resources.Load<GameObject>(humanoid.gameObject.name + "_generated");
            if (humanoidPrefab == null) {
                humanoidPrefab = PrefabUtility.CreatePrefab("Assets/Humanoid/Prefabs/Networking/Resources/" + humanoid.gameObject.name + "_generated.prefab", humanoid.gameObject);
                humanoidPrefab.gameObject.SetActive(true);
            }
            return humanoidPrefab;
        }

        private void UpdateHumanoidPrefab(HumanoidControl humanoid) {
            if (humanoid != null) {
                GameObject humanoidPrefab = Resources.Load<GameObject>(humanoid.gameObject.name + "_generated");
                if (humanoidPrefab != null && humanoid.gameObject != humanoidPrefab)
                    PrefabUtility.ReplacePrefab(humanoid.gameObject, humanoidPrefab, ReplacePrefabOptions.ConnectToPrefab);
            }
        }

        private void NetworkingComponentsInspectorPun(HumanoidControl humanoid, GameObject humanoidPrefab) {
#if hPHOTON1 || hPHOTON2
#if hNW_PHOTON
            CheckPunStarter(humanoid, humanoidPrefab);
            PhotonView photonView = humanoid.GetComponent<PhotonView>();
            if (photonView == null)
                photonView = humanoid.gameObject.AddComponent<PhotonView>();
            photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
            photonView.ObservedComponents.Add(humanoid);
#else
            cleanupPhotonView = humanoid.GetComponent<PhotonView>();
#endif
#endif
        }

        private void CheckPunStarter(HumanoidControl humanoid, GameObject humanoidPrefab) {
            if (Application.isPlaying)
                return;

#if hNW_PHOTON
            PhotonStarter photonStarter = FindObjectOfType<PhotonStarter>();
            if (photonStarter != null && humanoidPrefab != null && photonStarter.playerPrefab != humanoidPrefab) {
                Undo.RecordObject(photonStarter, "Updated Player Prefab");
                photonStarter.playerPrefab = humanoidPrefab;
            }
#endif
        }
        public void OnDisable() {
            Cleanup();
        }

        private NetworkIdentity cleanupNetworkIdentity;
#if hPHOTON1 || hPHOTON2
        private PhotonView cleanupPhotonView;
#endif
        private GameObject cleanupPunStarter;
        private void Cleanup() {
            if (cleanupNetworkIdentity) {
                DestroyImmediate(cleanupNetworkIdentity, true);
                cleanupNetworkIdentity = null;
            }
#if hPHOTON1 || hPHOTON2
            if (cleanupPhotonView) {
                DestroyImmediate(cleanupPhotonView, true);
                cleanupPhotonView = null;
            }
#endif
            if (cleanupPunStarter) {
                DestroyImmediate(cleanupPunStarter, true);
                cleanupPunStarter = null;
            }
        }
    }
}
#endif