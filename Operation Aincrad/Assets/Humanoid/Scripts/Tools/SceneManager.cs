using UnityEngine;
using UnityEngine.Networking;
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer {

#if hNW_PHOTON
    [RequireComponent(typeof(PhotonView))]
#if hPHOTON2
    public class SceneManager : MonoBehaviourPunCallbacks {
#else
    public class SceneManager : Photon.MonoBehaviour {
#endif
#elif hNW_UNET
    [RequireComponent(typeof(NetworkIdentity))]
    public class SceneManager : NetworkBehaviour {
#else
    public class SceneManager : MonoBehaviour {
#endif
        public int currentScene = 0;

#if hNW_UNET
        private NetworkManager nwManager;
#endif
        public string[] sceneNames;
        private string[] staticSceneNames;

        public bool dontDestroyOnLoad = false;

        private void Awake() {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.transform.root);

            // This is needed to sync all Scene Managers
            // Cannot use static sceneNames directly,
            // because somehow it gets reset to null when Don't Destroy on Load is enabled.
            staticSceneNames = sceneNames;

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            currentScene = scene.buildIndex;
        }

        public void LoadScene(int sceneId) {
            if (staticSceneNames == null || sceneId < 0 || sceneId >= staticSceneNames.Length)
                return;

#if hNW_PHOTON
#if hPHOTON2
            PhotonNetwork.AutomaticallySyncScene = true;
#else
            PhotonNetwork.automaticallySyncScene = true;
#endif
            PhotonNetwork.LoadLevel(staticSceneNames[sceneId]);
#elif hNW_UNET
            if (nwManager == null)
                nwManager = FindObjectOfType<NetworkManager>();
            if (nwManager == null) {
                Debug.LogError("Cannot change scene without an Network Manager for Unity Networking");
                return;
            }
            nwManager.ServerChangeScene(staticSceneNames[sceneId]);

#else
            //UnityEngine.SceneManagement.SceneManager.LoadScene(staticSceneNames[sceneId]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
#endif
            currentScene = sceneId;
        }

        public void NextScene() {
            currentScene = mod(currentScene + 1, staticSceneNames.Length);
            LoadScene(currentScene);
        }

        public void PreviousScene() {
            currentScene = mod(currentScene + 1, staticSceneNames.Length);
            LoadScene(currentScene);
        }

#if hNW_PHOTON
        [PunRPC]
        private void RpcLoadScene(string sceneName) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
#elif hNW_UNET
        [Command] // @ server
        private void CmdLoadScene(string sceneName) {
            RpcClientLoadScene(sceneName);
        }

        [ClientRpc] // @ remote client
        private void RpcClientLoadScene(string sceneName) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
#endif

        private static int mod(int k, int n) {
            k %= n;
            return (k < 0) ? k + n : k;
        }
    }
}