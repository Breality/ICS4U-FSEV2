using System.Collections.Generic;
using UnityEngine;

namespace Passer {

    public interface IHumanoidNetworking {
        void Send(bool b);
        void Send(byte b);
        void Send(int x);
        void Send(float f);
        void Send(Vector3 v);
        void Send(Quaternion q);

        bool ReceiveBool();
        byte ReceiveByte();
        int ReceiveInt();
        float ReceiveFloat();
        Vector3 ReceiveVector3();
        Quaternion ReceiveQuaternion();

        bool IsLocal();

        void InstantiateHumanoid(HumanoidControl humanoid); 
        void DestroyHumanoid(HumanoidControl humanoid);

        void Grab(HandTarget handTarget, GameObject obj, bool rangeCheck);
        void LetGo(HandTarget handTarget);

        void ChangeAvatar(HumanoidControl humanoid, string remoteAvatarName);
    }

    public static class HumanoidNetworking {
        public static List<HumanoidControl> FindLocalHumanoids() {
            List<HumanoidControl> humanoidList = new List<HumanoidControl>();
            HumanoidControl[] foundHumanoids = Object.FindObjectsOfType<HumanoidControl>();
            for (int i = 0; i < foundHumanoids.Length; i++) {
                if (!foundHumanoids[i].isRemote) {
                    humanoidList.Add(foundHumanoids[i]);
                }
            }
            return humanoidList;
        }

        private static GameObject remoteHumanoidPrefab;

        public static IHumanoidNetworking GetLocalHumanoidNetworking() {
#if hNW_PHOTON
            HumanoidPun[] pun = Object.FindObjectsOfType<HumanoidPun>();

            IHumanoidNetworking[] humanoidNetworkings = pun;

#else
            IHumanoidNetworking[] humanoidNetworkings = Object.FindObjectsOfType<HumanoidUnet>();
#endif
            foreach (IHumanoidNetworking humanoidNetworking in humanoidNetworkings) {
                if (humanoidNetworking.IsLocal())
                    return humanoidNetworking;
            }
            return null;
        }

        public static HumanoidControl FindRemoteHumanoid(List<HumanoidControl> humanoids, int humanoidId) {
            foreach (HumanoidControl humanoid in humanoids) {
                if (humanoid.isRemote && humanoid.humanoidId == humanoidId)
                    return humanoid;
            }
            return null;
        }

        public enum Debug {
            Info,
            Warning,
            Error,
            None,
        }
        public static Debug debug = Debug.Error;
        public static bool syncFingerSwing = false;

        #region Start
        public static void Start(Debug debug, bool syncFingerSwing) {
            HumanoidNetworking.debug = debug;
            HumanoidNetworking.syncFingerSwing = syncFingerSwing;

            remoteHumanoidPrefab = (GameObject)Resources.Load("RemoteHumanoid");
        }
        #endregion

        #region Start Humanoid
        public static HumanoidControl StartHumanoid(int nwId, int humanoidId, string name, string avatarPrefabName, Vector3 position, Quaternion rotation) {
            if (debug <= Debug.Info)
                UnityEngine.Debug.Log(nwId + ": Receive StartHumanoid " + humanoidId);

            HumanoidControl remoteHumanoid = InstantiateRemoteHumanoid(remoteHumanoidPrefab, name, position, rotation);
            remoteHumanoid.nwId = nwId;
            remoteHumanoid.humanoidId = humanoidId;

            if (debug <= Debug.Info)
                UnityEngine.Debug.Log(remoteHumanoid.nwId + ": Remote Humanoid " + remoteHumanoid.humanoidId +  " Added");

            GameObject remoteAvatar = (GameObject)Resources.Load(avatarPrefabName);
            if (remoteAvatar == null) {
                if (debug <= Debug.Error)
                    UnityEngine.Debug.LogError("Could not load remote avatar " + avatarPrefabName + ". Is it located in a Resources folder?");
                return remoteHumanoid;
            }
            remoteHumanoid.LocalChangeAvatar(remoteAvatar);

            return remoteHumanoid;
        }

        private static HumanoidControl InstantiateRemoteHumanoid(GameObject remoteHumanoidPrefab, string name, Vector3 position, Quaternion rotation) {
            GameObject remoteHumanoidObj = Object.Instantiate(remoteHumanoidPrefab, position, rotation);
            remoteHumanoidObj.name = name + " (Remote)";

            HumanoidControl remoteHumanoid = remoteHumanoidObj.GetComponent<HumanoidControl>();
            remoteHumanoid.isRemote = true;

            return remoteHumanoid;
        }
        #endregion

        #region Pose
        #region Send
        public static byte DetermineActiveTargets(HumanoidControl humanoid) {
            byte targetMask = 0;

            HumanoidTarget[] targets = {
                humanoid.hipsTarget,
                humanoid.headTarget,
                humanoid.leftHandTarget,
                humanoid.rightHandTarget,
                humanoid.leftFootTarget,
                humanoid.rightFootTarget
            };

            for (int i = 0; i < targets.Length; i++) {
                if (targets[i].main != null && (targets[i].main.target.confidence.position > 0.2F || targets[i].main.target.confidence.rotation > 0.2F) || i == 1) {
                    // for now, we always send the head to match the avatar's position well
                    targetMask |= (byte)(1 << (i + 1));
                }
            }
            return targetMask;
        }

        public static byte DetermineActiveTargets(HumanoidControl humanoid, out int activeTargetCount) {
            byte targetMask = 0;

            HumanoidTarget[] targets = {
                humanoid.hipsTarget,
                humanoid.headTarget,
                humanoid.leftHandTarget,
                humanoid.rightHandTarget,
                humanoid.leftFootTarget,
                humanoid.rightFootTarget
            };

            activeTargetCount = 0;
            for (int i = 0; i < targets.Length; i++) {
                if (targets[i].main != null && (targets[i].main.target.confidence.position > 0.2F || targets[i].main.target.confidence.rotation > 0.2F) || i == 1) {
                    // for now, we always send the head to match the avatar's position well
                    targetMask |= (byte)(1 << (i + 1));
                    activeTargetCount++;
                }
            }
            return targetMask;
        }

        public static bool IsTargetActive(byte targetMask, HumanoidControl.TargetId targetIndex) {
            int bitset = targetMask & (byte)(1 << ((int)targetIndex + 1));
            return (bitset != 0);
        }

        public static void SendAvatarPose(this IHumanoidNetworking networking, HumanoidControl humanoid) {
            networking.Send(humanoid.nwId);
            networking.Send(humanoid.humanoidId);
            networking.Send(Time.time); // Pose Time

            byte targetMask = DetermineActiveTargets(humanoid);
            networking.Send(targetMask);

            if (debug <= Debug.Info)
                UnityEngine.Debug.Log(humanoid.nwId + ": Send Pose Humanoid " + humanoid.humanoidId +  ", targetMask = " + targetMask);

            // Humanoid Transform is always sent
            networking.SendTarget(humanoid.transform);

            SendAvatarTargets(networking, humanoid, targetMask);

            if (IsTargetActive(targetMask, HumanoidControl.TargetId.LeftHand))
                networking.SendAvatarHandPose(humanoid.leftHandTarget);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.RightHand))
                networking.SendAvatarHandPose(humanoid.rightHandTarget);
        }

        private static void SendAvatarTargets(IHumanoidNetworking networking, HumanoidControl humanoid, byte targetMask) {
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.Hips))
                networking.SendTarget(humanoid.hipsTarget.transform);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.Head))
                networking.SendTarget(humanoid.headTarget.transform);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.LeftHand))
                networking.SendTarget(humanoid.leftHandTarget.transform);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.RightHand))
                networking.SendTarget(humanoid.rightHandTarget.transform);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.LeftFoot))
                networking.SendTarget(humanoid.leftFootTarget.transform);
            if (IsTargetActive(targetMask, HumanoidControl.TargetId.RightFoot))
                networking.SendTarget(humanoid.rightFootTarget.transform);
        }

        public static void SendAvatarHandPose(this IHumanoidNetworking networking, HandTarget handTarget) {
            networking.Send(handTarget != null);
            if (handTarget != null) {
                FingersTarget fingersTarget = handTarget.fingers;

                networking.Send(fingersTarget.thumb.curl);
                networking.Send(fingersTarget.index.curl);
                networking.Send(fingersTarget.middle.curl);
                networking.Send(fingersTarget.ring.curl);
                networking.Send(fingersTarget.little.curl);

                networking.Send(syncFingerSwing);
                if (syncFingerSwing) {
                    networking.Send(fingersTarget.thumb.swing);
                    networking.Send(fingersTarget.index.swing);
                    networking.Send(fingersTarget.middle.swing);
                    networking.Send(fingersTarget.ring.swing);
                    networking.Send(fingersTarget.little.swing);
                }
            }
        }

        public static void SendTarget(this IHumanoidNetworking networking, Transform transform) {
            networking.Send(transform.position);
            networking.Send(transform.rotation);
        }
        #endregion

        #region Receive
        public static float ReceiveAvatarPose(this IHumanoidNetworking networking, HumanoidControl remoteHumanoid, float lastTime) {

            float poseTime = networking.ReceiveFloat();
            //float deltaTime = poseTime - lastTime;

            byte targetMask = networking.ReceiveByte();
            if (debug <= Debug.Info)
                UnityEngine.Debug.Log(remoteHumanoid.nwId + ": Receive Pose Humanoid " + remoteHumanoid.humanoidId + ", targetMask = " + targetMask);

            // Humanoid Transform is always received
            ReceiveTransform(networking, remoteHumanoid.transform);

            ReceiveTargets(networking, remoteHumanoid, targetMask);

            if (IsTargetActive(targetMask, HumanoidControl.TargetId.LeftHand)) {
                bool leftHandIncluded = networking.ReceiveBool();
                if (leftHandIncluded) {
                    networking.ReceiveAvatarHandPose(remoteHumanoid.leftHandTarget);
                }
            }

            if (IsTargetActive(targetMask, HumanoidControl.TargetId.RightHand)) {
                bool rightHandIncluded = networking.ReceiveBool();
                if (rightHandIncluded) {
                    networking.ReceiveAvatarHandPose(remoteHumanoid.rightHandTarget);
                }
            }
            return poseTime;
        }

        private static void ReceiveTargets(IHumanoidNetworking networking, HumanoidControl humanoid, byte targetMask) {
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.Hips, humanoid.hipsTarget);
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.Head, humanoid.headTarget);
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.LeftHand, humanoid.leftHandTarget);
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.RightHand, humanoid.rightHandTarget);
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.LeftFoot, humanoid.leftFootTarget);
            ReceiveTarget(networking, targetMask, HumanoidControl.TargetId.RightFoot, humanoid.rightFootTarget);
        }

        public static void ReceiveAvatarHandPose(this IHumanoidNetworking networking, HandTarget handTarget) {
            float thumbCurl = networking.ReceiveFloat();
            float indexCurl = networking.ReceiveFloat();
            float middleCurl = networking.ReceiveFloat();
            float ringCurl = networking.ReceiveFloat();
            float littleCurl = networking.ReceiveFloat();

            if (handTarget != null) {
                FingersTarget fingersTarget = handTarget.fingers;

                fingersTarget.thumb.curl = thumbCurl;
                fingersTarget.index.curl = indexCurl;
                fingersTarget.middle.curl = middleCurl;
                fingersTarget.ring.curl = ringCurl;
                fingersTarget.little.curl = littleCurl;
                bool syncFingerSwing = networking.ReceiveBool();
                if (syncFingerSwing) {
                    fingersTarget.thumb.swing = networking.ReceiveFloat();
                    fingersTarget.index.swing = networking.ReceiveFloat();
                    fingersTarget.middle.swing = networking.ReceiveFloat();
                    fingersTarget.ring.swing = networking.ReceiveFloat();
                    fingersTarget.little.swing = networking.ReceiveFloat();
                }
            }
        }

        private static void ReceiveTarget(this IHumanoidNetworking networking, byte targetMask, HumanoidControl.TargetId targetId, HumanoidTarget target) {
            if (IsTargetActive(targetMask, targetId)) {
                target.EnableAnimator(false);
                ReceiveTransform(networking, target.transform);
                target.main.target.confidence.position = 0.6F;
                target.main.target.confidence.rotation = 0.6F;
            }
            else {
                target.EnableAnimator(true);
            }

        }

        private static void ReceiveTransform(IHumanoidNetworking networking, Transform transform) {
            transform.position = networking.ReceiveVector3();
            transform.rotation = networking.ReceiveQuaternion();
        }
        #endregion
        #endregion
    }
}