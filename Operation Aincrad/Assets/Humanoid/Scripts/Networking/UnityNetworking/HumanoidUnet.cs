using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Passer {

    [RequireComponent(typeof(NetworkIdentity))]
    public class HumanoidUnet : NetworkBehaviour, IHumanoidNetworking {
        public float sendRate = 25;
        public bool syncFingerSwing = false;
        public bool createLocalRemotes = false;

        public HumanoidNetworking.Debug debug = HumanoidNetworking.Debug.Error;

        public bool isLocal = false;
        public bool IsLocal() { return isLocal; }
        public List<HumanoidControl> humanoids = new List<HumanoidControl>();

        public bool syncTracking;

        #region Init
        public void Awake() {
            DontDestroyOnLoad(this);
            HumanoidNetworking.Start(debug, syncFingerSwing);
        }

        public override void OnStartClient() {
            name = name + " " + netId;

            NetworkManager nwManager = FindObjectOfType<NetworkManager>();
            short msgType = MsgType.Highest + 2;
            nwManager.client.RegisterHandler(msgType, ClientProcessAvatarPose);
        }

        public override void OnStartServer() {
            short msgType = MsgType.Highest + 1;
            NetworkServer.RegisterHandler(msgType, ForwardAvatarPose);
        }

        #endregion

        #region Start
        public override void OnStartLocalPlayer() {
            isLocal = true;
            name = "HumanoidUnet(Local)";

            humanoids = HumanoidNetworking.FindLocalHumanoids();
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log((int)netId.Value + ": Found " + humanoids.Count + " Humanoids");

            if (humanoids.Count <= 0)
                return;

            foreach (HumanoidControl humanoid in humanoids) {
                humanoid.nwId = (int)netId.Value;
                humanoid.humanoidNetworking = this;

                if (debug <= HumanoidNetworking.Debug.Info)
                    Debug.Log(humanoid.nwId + ": Send Start Humanoid " + humanoid.humanoidId);

                CmdServerStartHumanoid(humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
            }
            NetworkingSpawner spawner = FindObjectOfType<NetworkingSpawner>();
            if (spawner != null)
                spawner.OnNetworkingStarted();
        }

        #endregion

        #region Update
        private float lastSend;
        public void LateUpdate() {
            if (!isLocalPlayer)
                return;

            NetworkIdentity identity = GetComponent<NetworkIdentity>();
            if (Time.time > lastSend + 1 / sendRate) {
                foreach (HumanoidControl humanoid in humanoids) {
                    if (!humanoid.isRemote) {
                        SendAvatarPose2Server(identity, humanoid);
                        if (syncTracking)
                            SendTracking2Server(identity, humanoid);
                    }
                }
                lastSend = Time.time;
            }
        }
        #endregion

        #region Stop
        public void OnDestroy() {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log((int)netId.Value + ": Destroy Remote Humanoid");

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
                Debug.Log(netId + ": Instantiate Humanoid " + humanoid.humanoidId);

            humanoids.Add(humanoid);
            humanoid.nwId = (int)netId.Value;

            CmdServerStartHumanoid(humanoid.nwId, humanoid.humanoidId, humanoid.gameObject.name, humanoid.remoteAvatar.name, humanoid.transform.position, humanoid.transform.rotation);
        }
        #endregion

        #region Destroy Humanoid
        void IHumanoidNetworking.DestroyHumanoid(HumanoidControl humanoid) {
            if (humanoid == null)
                return;

            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(netId + ": Destroy Humanoid " + humanoid.humanoidId);

            humanoids.Remove(humanoid);
            humanoid.nwId = 0;

            if (this != null)
                CmdServerDestroyHumanoid(humanoid.nwId, humanoid.humanoidId);
        }

        [Command] // @ server
        private void CmdServerDestroyHumanoid(int nwId, int humanoidId) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(nwId + ": Forward DestroyHumanoid " + humanoidId);

            RpcClientDestroyHumanoid(nwId, humanoidId);
        }

        [ClientRpc]
        private void RpcClientDestroyHumanoid(int nwId, int humanoidId) {
            if (isLocalPlayer && !createLocalRemotes)
                return;

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
        private int serverNwId;
        private int serverHumanoidId;
        private string serverName;
        private string serverAvatarPrefabName;
        private Vector3 serverPosition;
        private Quaternion serverRotation;
        [Command] // @ server
        private void CmdServerStartHumanoid(int nwId, int humanoidId, string name, string avatarPrefabName, Vector3 position, Quaternion rotation) {
            serverNwId = nwId;
            serverHumanoidId = humanoidId;
            serverName = name;
            serverAvatarPrefabName = avatarPrefabName;
            serverPosition = position;
            serverRotation = rotation;

            HumanoidUnet[] nwHumanoids = FindObjectsOfType<HumanoidUnet>();
            foreach (HumanoidUnet nwHumanoid in nwHumanoids)
                nwHumanoid.NewClientStarted();
        }

        public void NewClientStarted() {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(serverNwId + ": Forward StartHumanoid " + serverHumanoidId);

            RpcClientStartHumanoid(serverNwId, serverHumanoidId, serverName, serverAvatarPrefabName, serverPosition, serverRotation);
        }

        [ClientRpc] // @ remote client
        private void RpcClientStartHumanoid(int nwId, int humanoidId, string name, string avatarPrefabName, Vector3 position, Quaternion rotation) {
            if (isLocalPlayer && !createLocalRemotes)
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
        #region Send @ local client
        NetworkWriter writer;
        public void SendAvatarPose2Server(NetworkIdentity identity, HumanoidControl humanoid) {
            short msgType = MsgType.Highest + 1;
            writer = new NetworkWriter();

            writer.StartMessage(msgType);
            this.SendAvatarPose(humanoid);
            writer.FinishMessage();

            identity.connectionToServer.SendWriter(writer, Channels.DefaultUnreliable);
        }
        #endregion

        #region Forward @ server
        [ServerCallback] // @ server
        public void ForwardAvatarPose(NetworkMessage msg) {
            short msgType = MsgType.Highest + 2;
            NetworkWriter sWriter = new NetworkWriter();

            sWriter.StartMessage(msgType);
            ForwardAvatarPose(msg.reader, sWriter);
            sWriter.FinishMessage();

            NetworkServer.SendWriterToReady(null, sWriter, Channels.DefaultUnreliable);
        }

        public void ForwardAvatarPose(NetworkReader sReader, NetworkWriter sWriter) {
            int nwId = sReader.ReadInt32();
            sWriter.Write(nwId); // NwId
            int humanoidId = sReader.ReadInt32();
            sWriter.Write(humanoidId); // HumanoidId

            sWriter.Write(sReader.ReadSingle()); // Pose Time

            byte targetMask = sReader.ReadByte();
            sWriter.Write(targetMask);

            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(nwId + ": Forward Pose Humanoid = " + humanoidId + ", targetMask = " + targetMask);

            ForwardTransform(sReader, sWriter); // humanoid.transform is always sent

            ForwardTargets(sReader, sWriter, targetMask);

            if (HumanoidNetworking.IsTargetActive(targetMask, HumanoidControl.TargetId.LeftHand)) {
                bool leftHandIncluded = sReader.ReadBoolean();
                sWriter.Write(leftHandIncluded);
                if (leftHandIncluded) {
                    ForwardAvatarHandPose(sReader, sWriter);
                }
            }

            if (HumanoidNetworking.IsTargetActive(targetMask, HumanoidControl.TargetId.RightHand)) {
                bool rightHandIncluded = sReader.ReadBoolean();
                sWriter.Write(rightHandIncluded);
                if (rightHandIncluded) {
                    ForwardAvatarHandPose(sReader, sWriter);
                }
            }
        }

        private void ForwardAvatarHandPose(NetworkReader sReader, NetworkWriter sWriter) {
            sWriter.Write(sReader.ReadSingle()); // thumb
            sWriter.Write(sReader.ReadSingle()); // index
            sWriter.Write(sReader.ReadSingle()); // middle
            sWriter.Write(sReader.ReadSingle()); // ring
            sWriter.Write(sReader.ReadSingle()); // little
            bool syncFingerSwing = sReader.ReadBoolean();
            sWriter.Write(syncFingerSwing);
            if (syncFingerSwing) {
                sWriter.Write(sReader.ReadSingle());
                sWriter.Write(sReader.ReadSingle());
                sWriter.Write(sReader.ReadSingle());
                sWriter.Write(sReader.ReadSingle());
                sWriter.Write(sReader.ReadSingle());
            }
        }

        private void ForwardTargets(NetworkReader sReader, NetworkWriter sWriter, byte targetMask) {
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.Hips);
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.Head);
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.LeftHand);
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.RightHand);
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.LeftFoot);
            ForwardTarget(sReader, sWriter, targetMask, HumanoidControl.TargetId.RightFoot);
        }

        private void ForwardTarget(NetworkReader sReader, NetworkWriter sWriter, byte targetMask, HumanoidControl.TargetId targetId) {
            if (HumanoidNetworking.IsTargetActive(targetMask, targetId))
                ForwardTransform(sReader, sWriter);
        }

        private void ForwardTransform(NetworkReader sReader, NetworkWriter sWriter) {
            sWriter.Write(sReader.ReadVector3()); // position;
            sWriter.Write(sReader.ReadQuaternion()); // rotation;
        }

        #endregion

        #region Receive @ remote client
        NetworkReader reader;
        private float lastPoseTime;
        [ClientCallback] // @ remote client
        public void ClientProcessAvatarPose(NetworkMessage msg) {
            reader = msg.reader;

            int nwId = ReceiveInt();
            int humanoidId = ReceiveInt();

            NetworkInstanceId netId = new NetworkInstanceId((uint)nwId);

            // NetworkMessages are not send to a specific player, but are received by the last
            // instantiated player. We need to find the player with the correct NetworkInstanceId
            // first and then let that player receive the message.
            GameObject nwObject = ClientScene.FindLocalObject(netId);
            if (nwObject != null) {
                HumanoidUnet humanoidUnet = nwObject.GetComponent<HumanoidUnet>();
                humanoidUnet.ReceiveAvatarPose(msg, humanoidId);
            }
            else {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    Debug.LogWarning(nwId + ": Could not find HumanoidNetworking object");
            }
        }

        public void ReceiveAvatarPose(NetworkMessage msg, int humanoidId) {
            if (isLocalPlayer && !createLocalRemotes)
                return;

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    Debug.LogWarning(netId.Value + ": Could not find humanoid: " + humanoidId);
                return;
            }

            reader = msg.reader;
            lastPoseTime = this.ReceiveAvatarPose(humanoid, lastPoseTime);
        }
        #endregion
        #endregion

        #region Tracking
        public void SendTracking2Server(NetworkIdentity identity, HumanoidControl humanoid) {
            Transform trackingTransform = GetTrackingTransform(humanoid);
            if (trackingTransform == null)
                return;

            CmdTracking(humanoid.nwId, humanoid.humanoidId, trackingTransform.position, trackingTransform.rotation);
        }

        [Command] // @ server
        private void CmdTracking(int nwId, int humanoidId, Vector3 position, Quaternion rotation) {
            RpcTracking(nwId, humanoidId, position, rotation);
        }

        [ClientRpc] // @ remote client
        private void RpcTracking(int nwId, int humanoidId, Vector3 position, Quaternion rotation) {
            foreach (HumanoidControl humanoid in HumanoidControl.allHumanoids) {
                if (humanoid.isRemote || humanoid.nwId == nwId)
                    continue;

                // The lowest (= earliest) nwId is the boss
                // NOT ATM FOR TESTING
                if (nwId > humanoid.nwId)
                    SyncTracking(humanoid, position, rotation);
            }
        }

        private Transform GetTrackingTransform(HumanoidControl humanoid) {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            if (humanoid.steam != null)
                return humanoid.steam.GetTrackingTransform();
#endif
            return null;
        }

        private void SyncTracking(HumanoidControl humanoid, Vector3 position, Quaternion rotation) {
#if hSTEAMVR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            if (humanoid.steam != null) {
                humanoid.steam.SyncTracking(position, rotation);
            }
#endif
        }
        #endregion

        #region Grab
        void IHumanoidNetworking.Grab(HandTarget handTarget, GameObject obj, bool rangeCheck) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(handTarget.humanoid.nwId + ": Grab " + obj);

            NetworkIdentity nwIdentity = obj.GetComponent<NetworkIdentity>();
            if (nwIdentity == null) {
                if (debug <= HumanoidNetworking.Debug.Error)
                    Debug.LogError("Grabbed object " + obj + " does not have a network identity");
            }
            else
                CmdGrab(handTarget.humanoid.nwId, handTarget.humanoid.humanoidId, obj, handTarget.isLeft);
        }

        [Command] // @ server
        public void CmdGrab(int nwId, int humanoidId, GameObject obj, bool leftHanded) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log("Server: CmdGrab " + obj);

            RpcClientGrab(nwId, humanoidId, obj, leftHanded);
        }

        [ClientRpc] // @ remote client
        public void RpcClientGrab(int nwId, int humanoidId, GameObject obj, bool leftHanded) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(nwId + ": RpcGrab " + obj);

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    Debug.LogWarning(netId.Value + ": Could not find humanoid: " + humanoidId);
                return;
            }

            HandTarget handTarget = leftHanded ? humanoid.leftHandTarget : humanoid.rightHandTarget;
            HandInteraction.LocalGrab(handTarget, obj);
        }
        #endregion

        #region Let Go
        void IHumanoidNetworking.LetGo(HandTarget handTarget) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(handTarget.humanoid.nwId + ": LetGo ");

            CmdLetGo(handTarget.humanoid.nwId, handTarget.humanoid.humanoidId, handTarget.isLeft);
        }

        [Command] // @ server
        public void CmdLetGo(int nwId, int humanoidId, bool leftHanded) {
            if (debug <= HumanoidNetworking.Debug.Warning)
                Debug.Log("Server: CmdLetGo ");

            RpcClientLetGo(nwId, humanoidId, leftHanded);
        }

        [ClientRpc] // @ remote client
        public void RpcClientLetGo(int nwId, int humanoidId, bool leftHanded) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(netId + ": RpcLetGo " + humanoidId);

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    Debug.LogWarning(netId.Value + ": Could not find humanoid: " + humanoidId);
                return;
            }

            HandTarget handTarget = leftHanded ? humanoid.leftHandTarget : humanoid.rightHandTarget;
            HandInteraction.LocalLetGo(handTarget);
        }
        #endregion

        #region ChangeAvatar
        void IHumanoidNetworking.ChangeAvatar(HumanoidControl humanoid, string avatarPrefabName) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(humanoid.nwId + ": Change Avatar: " + avatarPrefabName);

            CmdChangeNamedAvatar(humanoid.humanoidId, avatarPrefabName);
        }

        [Command] // @ server
        public void CmdChangeNamedAvatar(int humanoidId, string avatarPrefabName) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log("Server: Change Avatar " + humanoidId + " to: " + avatarPrefabName);

            RpcClientChangeNamedAvatar(humanoidId, avatarPrefabName);
        }

        [ClientRpc] // @ remote client
        public void RpcClientChangeNamedAvatar(int humanoidId, string avatarPrefabName) {
            if (debug <= HumanoidNetworking.Debug.Info)
                Debug.Log(netId.Value + ": RpcChangeAvatar " + humanoidId + "to: " + avatarPrefabName);

            HumanoidControl humanoid = HumanoidNetworking.FindRemoteHumanoid(humanoids, humanoidId);
            if (humanoid == null) {
                if (debug <= HumanoidNetworking.Debug.Warning)
                    Debug.LogWarning(netId.Value + ": Could not find humanoid: " + humanoidId);
                return;
            }

            GameObject remoteAvatar = (GameObject)Resources.Load(avatarPrefabName);
            if (remoteAvatar == null) {
                if (debug <= HumanoidNetworking.Debug.Error)
                    Debug.LogError("Could not load remote avatar " + avatarPrefabName + ". Is it located in a Resources folder?");
                return;
            }
            humanoid.LocalChangeAvatar(remoteAvatar);
        }
        #endregion

        #region Send
        public void Send(bool b) { writer.Write(b); }
        public void Send(byte b) { writer.Write(b); }
        public void Send(int x) { writer.Write(x); }
        public void Send(float f) { writer.Write(f); }
        public void Send(Vector3 v) { writer.Write(v); }
        public void Send(Quaternion q) { writer.Write(q); }
        #endregion

        #region Receive
        public bool ReceiveBool() { return reader.ReadBoolean(); }
        public byte ReceiveByte() { return reader.ReadByte(); }
        public int ReceiveInt() { return reader.ReadInt32(); }
        public float ReceiveFloat() { return reader.ReadSingle(); }
        public Vector3 ReceiveVector3() { return reader.ReadVector3(); }
        public Quaternion ReceiveQuaternion() { return reader.ReadQuaternion(); }
        #endregion
    }
}