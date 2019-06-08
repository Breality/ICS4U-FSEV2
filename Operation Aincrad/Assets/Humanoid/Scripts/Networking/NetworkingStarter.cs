using UnityEngine;

namespace Passer {
    [System.Serializable]
    [HelpURLAttribute("https://passervr.com/documentation/humanoid-control/networking-support/")]
#if hNW_PHOTON
#if hPHOTON2
    public class NetworkingStarter : Photon.Pun.MonoBehaviourPunCallbacks {
#else
    public class NetworkingStarter : Photon.PunBehaviour {
#endif
#else
    public class NetworkingStarter : MonoBehaviour {
#endif

#if hNW_UNET || hNW_PHOTON
        public string serverIpAddress = "127.0.0.1";
        public string roomName = "default";
        public int gameVersion = 1;

        public GameObject networkingPrefab;
#endif

#if hNW_UNET
        public enum ServerType {
            CloudServer,
            OwnServer
        }
        public ServerType serverType;
        public enum Role {
            Host,
            Client,
            //Server,
        }
        public Role role;
#elif hNW_PHOTON
        public int sendRate = 25;
#endif

        public void Start() {
#if hNW_PHOTON
            PhotonStarter punStarter = GetComponent<PhotonStarter>();
            if (punStarter == null) {
                punStarter = gameObject.AddComponent<PhotonStarter>();
                //punStarter.hideFlags = HideFlags.HideInInspector;
            }
            punStarter.StartNetworking(roomName, gameVersion, networkingPrefab, sendRate);
#elif hNW_UNET
            UnetStarter unetStarter = GetComponent<UnetStarter>();
            if (unetStarter == null) {
                unetStarter = gameObject.AddComponent<UnetStarter>();
                unetStarter.hideFlags = HideFlags.HideInInspector;
            }
            if (serverType == ServerType.CloudServer)
                unetStarter.StartNetworking(this.gameObject, roomName, gameVersion);
            else {
                if (role == Role.Host)
                    unetStarter.StartNetworking(this.gameObject, "127.0.0.1");
                else
                    unetStarter.StartNetworking(this.gameObject, serverIpAddress);
            }
#endif
        }
    }
}