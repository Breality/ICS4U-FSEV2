using UnityEngine;
#if hPHOTON2
using Photon.Pun;
using Photon.Realtime;
#endif

namespace Passer {

#if !hNW_PHOTON
    public class PhotonStarter : MonoBehaviour {
#else
#if hPHOTON2
        public class PhotonStarter : MonoBehaviourPunCallbacks {
#else
    public class PhotonStarter : MonoBehaviour {
#endif


        public GameObject playerPrefab;

        public string roomName;
        public int gameVersion;
        public int sendRate;

#if hPHOTON2
        public override void OnEnable() {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable() {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }
#endif

        public void StartNetworking(string _roomName, int _gameVersion, GameObject _playerPrefab, int _sendRate) {
            roomName = _roomName;
            gameVersion = _gameVersion;
            playerPrefab = _playerPrefab;
            sendRate = _sendRate;

#if hPHOTON2            
            PhotonNetwork.SendRate = sendRate;
            PhotonNetwork.SerializationRate = sendRate;
            PhotonNetwork.GameVersion = gameVersion.ToString();
            PhotonNetwork.ConnectUsingSettings();
#else
            PhotonNetwork.sendRate = sendRate;
            PhotonNetwork.sendRateOnSerialize = sendRate;
            PhotonNetwork.ConnectUsingSettings(gameVersion.ToString());
#endif
        }

#if hPHOTON2
        public override void OnConnectedToMaster() {
#else
        public void OnConnectedToMaster() {
#endif
            RoomOptions roomOptions = new RoomOptions() { IsVisible = false, MaxPlayers = 4 };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

#if hPHOTON2
        public override void OnJoinRoomFailed(short returnCode, string message) {
#else
        public void OnPhotonJoinRoomFailed() {
#endif
            Debug.LogError("Could not joint the " + roomName + " room");
        }

#if hPHOTON2
        public override void OnJoinedRoom() {
#else
        public virtual void OnJoinedRoom() {
#endif
            if (playerPrefab != null)
                PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);

            NetworkingSpawner spawner = FindObjectOfType<NetworkingSpawner>();
            if (spawner != null)
                spawner.OnNetworkingStarted();
        }
#endif
    }
}
