using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace Passer {
    [RequireComponent(typeof(NetworkManager))]
    public class UnetStarter : MonoBehaviour {
        bool matchCreated;
        private NetworkManager networkManager;
        private NetworkMatch networkMatch;

        private string roomName;
        private int gameVersion;

        public void StartNetworking(GameObject gameObject, string _roomName, int _gameVersion) {
            roomName = _roomName;
            gameVersion = _gameVersion;

            networkMatch = gameObject.AddComponent<NetworkMatch>();
            networkManager = gameObject.GetComponent<NetworkManager>();

            networkMatch.ListMatches(0, 10, "", true, 0, gameVersion, OnMatchList);
        }

        public void StartNetworking(GameObject gameObject, string serverIpAddress) {
            networkManager = gameObject.GetComponent<NetworkManager>();

            if (serverIpAddress == "127.0.0.1" || serverIpAddress == "localhost")
                networkManager.StartHost();
            else {
                NetworkClient nwClient = networkManager.StartClient();
                nwClient.Connect(serverIpAddress, networkManager.networkPort);
            }
        }

        public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
            if (success && matches != null) {
                int foundRoom = -1;
                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].name == roomName)
                        foundRoom = i;
                }

                if (foundRoom == -1) {
                    networkMatch.CreateMatch(roomName, 1000, true, "", "", "", 0, gameVersion, OnMatchCreated);
                } else {
                    networkMatch.JoinMatch(matches[foundRoom].networkId, "", "", "", 0, 0, OnMatchJoined);

                }
            } else if (!success) {
                Debug.LogError("List match failed: " + extendedInfo);
            }
        }

        public void OnMatchCreated(bool success, string extendedInfo, MatchInfo matchInfo) {
            if (success) {
                matchCreated = true;
                networkManager.StartHost(matchInfo);
            } else {
                Debug.LogError("Create match failed: " + extendedInfo);
            }
        }

        bool joined;
        public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {
            if (success) {
                if (matchCreated) {
                    Debug.LogWarning("Match already set up, aborting...");
                    return;
                }
                joined = true;
                NetworkClient nwClient = networkManager.StartClient(matchInfo);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
                //nwClient.Connect(matchInfo); not supported on WSA...
#else
                nwClient.Connect(matchInfo);
#endif
            } else {
                Debug.LogError("Join match failed " + extendedInfo);
            }
        }

        private void JoinTimeout() {
            if (!joined) {
                Debug.LogWarning("Timeout occured while joining. Creating new match");
                networkMatch.CreateMatch(roomName, 1000, true, "", "", "", 0, gameVersion, OnMatchCreated);
            }
        }

        public void OnMatchDestroyed(bool success, string extendedInfo) {
            Debug.Log("Match Destroyed: " + extendedInfo);
        }

        public void OnError(NetworkMessage conn) {
            Debug.LogError("Error connecting");
        }
    }
}