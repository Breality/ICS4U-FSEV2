using System.Collections.Generic;
using UnityEngine;
#if hPHOTON2
using Photon.Realtime;
using Photon.Pun;
#endif

namespace Passer {

#if !hNW_PHOTON
    public class HumanoidPun : MonoBehaviour {
#else
    [RequireComponent(typeof(PhotonView))]
#if hPHOTON2
    public class HumanoidPun : MonoBehaviourPunCallbacks, IHumanoidNetworking, IPunInstantiateMagicCallback, IPunObservable {
#else
    public class HumanoidPun : Photon.MonoBehaviour, IHumanoidNetworking {
#endif
        public bool syncFingerSwing = false;

        public HumanoidNetworking.Debug debug = HumanoidNetworking.Debug.Error;

        public bool isLocal = false;
        public bool IsLocal() { return isLocal; }
        public List<HumanoidControl> humanoids = new List<HumanoidControl>();


        #region Init
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
        public void Awake() {
            DontDestroyOnLoad(this);
            HumanoidNetworking.Start(debug, syncFingerSwing);
        }
        #endregion

        #region Start
        public void OnPhotonInstantiate(PhotonMessageInfo info) {
#if hPHOTON2
            if (photonView.IsMine) {
#else
            if (photonView.isMine) {
#endif
                isLocal = true;
                name = "HumanoidPun(Local)";

                humanoids = HumanoidNetworking.FindLocalHumanoids();
                if (debug <= HumanoidNetworking.Debug.Info)
                    PhotonLog("Found " + humanoids.Count + " Humanoids");

                if (humanoids.Count <= 0)
                    return;

                foreach (HumanoidControl humanoid in humanoids) {
#if hPHOTON2
                    humanoid.nwId = photonView.ViewID;
#else
                    humanoid.nwId = photonView.viewID;
#endif
                    humanoid.humanoidNetworking = this;

                    if (debug <= HumanoidNetworking.Debug.Info)
                        Debug.Log(humanoid.nwId + ": Send Start Humanoid " + humanoid.humanoidId);

#if hPHOTON2
                    photonView.RPC("RpcStartHumanoid", RpcTarget.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#else
                    photonView.RPC("RpcStartHumanoid", PhotonTargets.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#endif
                }

                NetworkingSpawner spawner = FindObjectOfType<NetworkingSpawner>();
                if (spawner != null)
                    spawner.OnNetworkingStarted();
            }
        }

#if hPHOTON2
        public override void OnPlayerEnteredRoom(Player newPlayer) {
#else
        public void OnPhotonPlayerConnected(PhotonPlayer player) {
#endif
            List<HumanoidControl> humanoids = HumanoidNetworking.FindLocalHumanoids();
            if (humanoids.Count <= 0)
                return;

            foreach (HumanoidControl humanoid in humanoids) {
                if (debug <= HumanoidNetworking.Debug.Info)
                    Debug.Log(humanoid.nwId + ": (Re)Send StartHumanoid " + humanoid.humanoidId);

                // Notify new player about my humanoid
#if hPHOTON2
                photonView.RPC("RpcStartHumanoid", RpcTarget.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#else
                photonView.RPC("RpcStartHumanoid", PhotonTargets.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#endif
            }
        }
        #endregion

        #region Update
        PhotonStream stream;

        private float lastPoseTime;
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            this.stream = stream;
#if hPHOTON2
            if (stream.IsWriting) {
#else
            if (stream.isWriting) {
#endif
                SendAvatarPose(stream);
            }
            else {
                ReceiveAvatarPose(stream);
            }
        }
        #endregion

        #region Stop
        private void OnDestroy() {
            if (debug <= HumanoidNetworking.Debug.Info)
                PhotonLog("Destroy Remote Humanoids");

            foreach (HumanoidControl humanoid in humanoids) {
                if (humanoid == null)
                    continue;

                if (humanoid.isRemote) {
                    if (humanoid.gameObject != null)
                        Destroy(humanoid.gameObject);
                }
                else
                    humanoid.nwId = 0;
            }
        }
        #endregion

        #region Instantiate Humanoid
        void IHumanoidNetworking.InstantiateHumanoid(HumanoidControl humanoid) {
            if (debug <= HumanoidNetworking.Debug.Info)
                PhotonLog("Send Instantiate Humanoid " + humanoid.humanoidId);

            humanoids.Add(humanoid);
#if hPHOTON2
            humanoid.nwId = photonView.ViewID;
#else
            humanoid.nwId = photonView.viewID;
#endif

#if hPHOTON2
            photonView.RPC("RpcStartHumanoid", RpcTarget.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#else
            photonView.RPC("RpcStartHumanoid", PhotonTargets.Others, humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
#endif
        }
        #endregion

        #region Destroy Humanoid
        void IHumanoidNetworking.DestroyHumanoid(HumanoidControl humanoid) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(humanoid.nwId + ": Destroy Humanoid " + humanoid.humanoidId);

            humanoids.Remove(humanoid);
            humanoid.nwId = 0;

#if hPHOTON2
            if (PhotonNetwork.IsConnected)
                photonView.RPC("RpcDestroyHumanoid", RpcTarget.Others, humanoid.nwId, humanoid.humanoidId);
#else
            if (PhotonNetwork.connected)
                photonView.RPC("RpcDestroyHumanoid", PhotonTargets.Others, humanoid.nwId, humanoid.humanoidId);
#endif
        }

        [PunRPC]
        public void RpcDestroyHumanoid(int nwId, int humanoidId) {
            HumanoidControl remoteHumanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (remoteHumanoid == null) {
                // Unknown remote humanoid
                return;
            }

            if (remoteHumanoid.gameObject != null)
                Destroy(remoteHumanoid.gameObject);
        }
        #endregion

        #region Start Humanoid
        [PunRPC]
        public void RpcStartHumanoid(int nwId, int humanoidId, string name, string avatarPrefabName, Vector3 position, Quaternion rotation) {
#if hPHOTON2
            if (nwId != photonView.ViewID)
#else
            if (nwId != photonView.viewID)
#endif
                return;

            HumanoidControl remoteHumanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (remoteHumanoid != null) {
                // This remote humanoid already exists
                return;
            }

            humanoids.Add(HumanoidNetworking.StartHumanoid(nwId, humanoidId, name, avatarPrefabName, position, rotation));
        }
        #endregion

        #region Pose
        #region Send
        private void SendAvatarPose(PhotonStream stream) {
            this.stream = stream;

            foreach (HumanoidControl humanoid in humanoids)
                this.SendAvatarPose(humanoid);
        }
        #endregion

        #region Receive
        PhotonStream reader;
        private void ReceiveAvatarPose(PhotonStream reader) {
            this.reader = reader;

            for (int i = 0; i < reader.Count; i++) {
                int nwId = ReceiveInt();
#if hPHOTON2
                if (nwId != photonView.ViewID)
                    return;
#else
                if (nwId != photonView.viewID)
                    return;
#endif
                int humanoidId = ReceiveInt();

                HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
                if (humanoid == null) {
                    if (debug <= HumanoidNetworking.Debug.Warning)
                        Debug.LogWarning(nwId + ": Could not find humanoid: " + humanoidId);
                    return;
                }

                lastPoseTime = this.ReceiveAvatarPose(humanoid, lastPoseTime);
            }
        }

        #endregion
        #endregion

        #region Grab
        void IHumanoidNetworking.Grab(HandTarget handTarget, GameObject obj, bool rangeCheck) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(handTarget.humanoid.nwId + ": Grab " + obj);

            PhotonView objView = obj.GetComponent<PhotonView>();
            if (objView == null) {
                if (debug <= HumanoidNetworking.Debug.Error)
                    Debug.LogError("Photon Grab: Grabbed object does not have a PhotonView");
            }
            else
#if hPHOTON2
                photonView.RPC("RpcGrab", RpcTarget.Others, handTarget.humanoid.humanoidId, objView.ViewID, handTarget.isLeft, rangeCheck);
#else
                photonView.RPC("RpcGrab", PhotonTargets.Others, handTarget.humanoid.humanoidId, objView.viewID, handTarget.isLeft, rangeCheck);
#endif
        }

        [PunRPC]
        public void RpcGrab(int humanoidId, int objViewID, bool isLeft, bool rangeCheck) {
            PhotonView objView = PhotonView.Find(objViewID);
            GameObject obj = objView.gameObject;

            if (debug <= HumanoidNetworking.Debug.Info)
                PhotonLog("RpcGrab " + obj);

            if (obj == null)
                return;

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    PhotonLogWarning("Could not find humanoid: " + humanoidId);
                return;
            }

            HandTarget handTarget = isLeft ? humanoid.leftHandTarget : humanoid.rightHandTarget;
            if (handTarget != null) {
                HandInteraction.LocalGrab(handTarget, obj, rangeCheck);
            }
        }
        #endregion

        #region Let Go
        void IHumanoidNetworking.LetGo(HandTarget handTarget) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(handTarget.humanoid.nwId + ": LetGo ");

#if hPHOTON2
            photonView.RPC("RpcLetGo", RpcTarget.Others, handTarget.humanoid.humanoidId, handTarget.isLeft);
#else
            photonView.RPC("RpcLetGo", PhotonTargets.Others, handTarget.humanoid.humanoidId, handTarget.isLeft);
#endif
        }

        [PunRPC]
        public void RpcLetGo(int humanoidId, bool isLeft) {
            if (debug <= HumanoidNetworking.Debug.Info)
                PhotonLog("RpcLetGo");

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    PhotonLogWarning("Could not find humanoid: " + humanoidId);
                return;
            }

            HandTarget handTarget = isLeft ? humanoid.leftHandTarget : humanoid.rightHandTarget;
            if (handTarget != null) {
                HandInteraction.LocalLetGo(handTarget);
            }
        }
        #endregion

        #region Change Avatar
        void IHumanoidNetworking.ChangeAvatar(HumanoidControl humanoid, string avatarPrefabName) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(humanoid.nwId + ": Change Avatar: " + avatarPrefabName);

#if hPHOTON2
            photonView.RPC("RpcChangeAvatar", RpcTarget.Others, humanoid.humanoidId, avatarPrefabName);
#else
            photonView.RPC("RpcChangeAvatar", PhotonTargets.Others, humanoid.humanoidId, avatarPrefabName);
#endif
        }

        [PunRPC]
        public void RpcChangeAvatar(int humanoidId, string avatarPrefabName) {
            if (debug <= HumanoidNetworking.Debug.Info)
                PhotonLog("RpcChangeAvatar " + avatarPrefabName);

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    PhotonLogWarning("Could not find humanoid: " + humanoidId);
                return;
            }

            GameObject remoteAvatar = (GameObject)Resources.Load(avatarPrefabName);
            if (remoteAvatar == null) {
                if (debug <= HumanoidNetworking.Debug.Error)
                    PhotonLogError("Could not load remote avatar " + avatarPrefabName + ". Is it located in a Resources folder?");
                return;
            }
            humanoid.LocalChangeAvatar(remoteAvatar);
        }
        #endregion

        #region Send
        public void Send(bool b) { stream.SendNext(b); }
        public void Send(byte b) { stream.SendNext(b); }
        public void Send(int x) { stream.SendNext(x); }
        public void Send(float f) { stream.SendNext(f); }
        public void Send(Vector3 v) { stream.SendNext(v); }
        public void Send(Quaternion q) { stream.SendNext(q); }
        #endregion

        #region Receive
        public bool ReceiveBool() { return (bool)reader.ReceiveNext(); }
        public byte ReceiveByte() { return (byte)reader.ReceiveNext(); }
        public int ReceiveInt() { return (int)reader.ReceiveNext(); }
        public float ReceiveFloat() { return (float)reader.ReceiveNext(); }
        public Vector3 ReceiveVector3() { return (Vector3)reader.ReceiveNext(); }
        public Quaternion ReceiveQuaternion() { return (Quaternion)reader.ReceiveNext(); }
        #endregion

        private void PhotonLog(string message) {
#if hPHOTON2
            Debug.Log(photonView.ViewID + ": " + message);
#else
            Debug.Log(photonView.viewID + ": " + message);
#endif
        }

        private void PhotonLogWarning(string message) {
#if hPHOTON2
            Debug.LogWarning(photonView.ViewID + ": " + message);
#else
            Debug.LogWarning(photonView.viewID + ": " + message);
#endif
        }

        private void PhotonLogError(string message) {
#if hPHOTON2
            Debug.LogError(photonView.ViewID + ": " + message);
#else
            Debug.LogError(photonView.viewID + ": " + message);
#endif
        }

#endif
    }
}
