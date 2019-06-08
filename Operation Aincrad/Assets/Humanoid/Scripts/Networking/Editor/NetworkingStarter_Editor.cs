using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer {

    [CustomEditor(typeof(NetworkingStarter))]
    public class NetworkingStarter_Editor : Editor {

#if hNW_UNET
        private SerializedProperty serverTypeProp;
#elif hNW_PHOTON
        private SerializedProperty networkingPrefabProp;
        private SerializedProperty sendRateProp;
#endif
#if hNW_UNET || hNW_PHOTON
        private SerializedProperty roomNameProp;
        private SerializedProperty gameVersionProp;
#endif
#if hNW_UNET
        private SerializedProperty serverIpAddressProp;
        private SerializedProperty roleProp;
#endif

        public virtual void OnEnable() {
#if hNW_UNET
            serverTypeProp = serializedObject.FindProperty("serverType");
            serverIpAddressProp = serializedObject.FindProperty("serverIpAddress");
            roleProp = serializedObject.FindProperty("role");
#elif hNW_PHOTON
            networkingPrefabProp = serializedObject.FindProperty("networkingPrefab");
            sendRateProp = serializedObject.FindProperty("sendRate");
#endif
#if hNW_UNET || hNW_PHOTON
            roomNameProp = serializedObject.FindProperty("roomName");
            gameVersionProp = serializedObject.FindProperty("gameVersion");
#endif
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            NetworkingStarter nwStarter = (NetworkingStarter)target;

#if !hNW_UNET && !hNW_PHOTON
            EditorGUILayout.HelpBox("Networking Support is disabled. Check Preferences to enable it.", MessageType.Warning);
#endif

            Inspector();

            if (Application.isPlaying || !nwStarter.gameObject.activeInHierarchy)
                return;

            GameObject humanoidPrefab = GetHumanoidNetworkingPrefab();

            NetworkingComponentsInspectorUnet(humanoidPrefab);
            NetworkingComponentsInspectorPun(humanoidPrefab);

            serializedObject.ApplyModifiedProperties();
        }

        private void Inspector() {
#if hNW_UNET
            serverTypeProp.intValue = (int)(NetworkingStarter.ServerType)EditorGUILayout.EnumPopup("Server Type", (NetworkingStarter.ServerType)serverTypeProp.intValue);

            if ((NetworkingStarter.ServerType)serverTypeProp.intValue == NetworkingStarter.ServerType.CloudServer)
                CloudServerInspector();
            else
                OwnServerInspector();
#elif hNW_PHOTON
            networkingPrefabProp.objectReferenceValue = (GameObject) EditorGUILayout.ObjectField("Networking Prefab", (GameObject) networkingPrefabProp.objectReferenceValue, typeof(GameObject), true);
            CloudServerInspector();
            sendRateProp.intValue = EditorGUILayout.IntField("Send Rate", sendRateProp.intValue);
#endif
        }

        private void CloudServerInspector() {
#if hNW_UNET || hNW_PHOTON
            roomNameProp.stringValue = EditorGUILayout.TextField("Room Name", roomNameProp.stringValue);
            gameVersionProp.intValue = EditorGUILayout.IntField("Game Version", gameVersionProp.intValue);
#endif
        }

        private void OwnServerInspector() {
#if hNW_UNET
            roleProp.intValue = (int)(NetworkingStarter.Role)EditorGUILayout.EnumPopup("Role", (NetworkingStarter.Role)roleProp.intValue);
            if ((NetworkingStarter.Role)roleProp.intValue == NetworkingStarter.Role.Client)
                serverIpAddressProp.stringValue = EditorGUILayout.TextField("Server IP Address", serverIpAddressProp.stringValue);
#endif
        }

        private GameObject GetHumanoidNetworkingPrefab() {
#if hNW_PHOTON
            GameObject humanoidPrefab = Resources.Load<GameObject>("HumanoidPun");
#elif hNW_UNET
            GameObject humanoidPrefab = Resources.Load<GameObject>("HumanoidUnet");
#else
            GameObject humanoidPrefab = null;
#endif
            return humanoidPrefab;
        }

        private void NetworkingComponentsInspectorUnet(GameObject humanoidPrefab) {
            NetworkManager nwManager = FindObjectOfType<NetworkManager>();
#if hNW_UNET
            if (nwManager == null) {
                NetworkingStarter nwStarter = (NetworkingStarter)target;
                nwManager = nwStarter.gameObject.AddComponent<NetworkManager>();
            }

            if (nwManager.playerPrefab == null && humanoidPrefab != null)
                nwManager.playerPrefab = humanoidPrefab;

#else
            if (nwManager != null) {
                cleanupNetworkManager = nwManager;
            }
#endif
        }

        private void NetworkingComponentsInspectorPun(GameObject humanoidPrefab) {
#if hNW_PHOTON
            if (humanoidPrefab != null && networkingPrefabProp.objectReferenceValue != humanoidPrefab)
                networkingPrefabProp.objectReferenceValue = humanoidPrefab;
#endif
        }

        public void OnDisable() {
            Cleanup();
        }

        private NetworkManager cleanupNetworkManager;
#if hPHOTON1 || hPHOTON2
        private PhotonView cleanupPhotonView;
#endif

        private void Cleanup() {
            if (cleanupNetworkManager) {
                DestroyImmediate(cleanupNetworkManager, true);
                cleanupNetworkManager = null;
            }

#if hPHOTON1 || hPHOTON2
            if (cleanupPhotonView) {
                DestroyImmediate(cleanupPhotonView, true);
                cleanupPhotonView = null;
            }
#endif
        }
    }
}