#if ENABLE_UNET
using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Networking
{
	[AddComponentMenu("Network/NetworkSkeleton")]
	public class NetworkSkeleton : NetworkBehaviour
	{
		public class BoneInfo
		{
			public Transform	m_bone;
			public Vector3		m_TargetSyncPosition;
			public Quaternion	m_TargetSyncRotation3D;
		}

		const short SkeletonMsg = 199;

		[SerializeField]
		Transform m_Target;

		Transform[] m_Bones;

		[SerializeField]
		float m_SendInterval = 0.1f;
		[SerializeField]
		NetworkTransform.AxisSyncMode m_SyncRotationAxis = NetworkTransform.AxisSyncMode.AxisXYZ;
		[SerializeField]
		NetworkTransform.CompressionSyncMode m_RotationSyncCompression = NetworkTransform.CompressionSyncMode.None;

		[SerializeField]
		float m_InterpolateRotation = 0.5f;
		[SerializeField]
		float m_InterpolateMovement = 0.5f;

		[SerializeField]
		int m_SyncLevel = 0;

		[SerializeField]
		int m_NetworkChannel = Channels.DefaultUnreliable;

		// movement smoothing
		public BoneInfo[] m_BoneInfos;

		float m_LastClientSyncTime; // last time client received a sync from server
		float m_LastClientSendTime; // last time client send a sync to server

		const float k_LocalRotationThreshold = 0.00001f;

		public int numBones;
		public int binarySize;

		NetworkWriter m_LocalTransformWriter;

		// settings
		public Transform target { get { return m_Target; } set { m_Target = value; OnValidate(); } }
		public float sendInterval { get { return m_SendInterval; } set { m_SendInterval = value; } }
		public NetworkTransform.AxisSyncMode syncRotationAxis { get { return m_SyncRotationAxis; } set { m_SyncRotationAxis = value; } }
		public NetworkTransform.CompressionSyncMode rotationSyncCompression { get { return m_RotationSyncCompression; } set { m_RotationSyncCompression = value; } }
		public float interpolateRotation { get { return m_InterpolateRotation; } set { m_InterpolateRotation = value; } }
		public float interpolateMovement { get { return m_InterpolateMovement; } set { m_InterpolateMovement = value; } }
		public int syncLevel { get { return m_SyncLevel; } set { m_SyncLevel = value; } }

		// runtime data
		public float lastSyncTime { get { return m_LastClientSyncTime; } }

		void OnValidate()
		{
			if (m_SendInterval < 0)
			{
				m_SendInterval = 0;
			}

			if (m_SyncRotationAxis < NetworkTransform.AxisSyncMode.None || m_SyncRotationAxis > NetworkTransform.AxisSyncMode.AxisXYZ)
			{
				m_SyncRotationAxis = NetworkTransform.AxisSyncMode.None;
			}

			if (interpolateRotation < 0)
			{
				interpolateRotation = 0.01f;
			}
			if (interpolateRotation > 1.0f)
			{
				interpolateRotation = 1.0f;
			}

			if (interpolateMovement < 0)
			{
				interpolateMovement = 0.01f;
			}

			if (interpolateMovement > 1.0f)
			{
				interpolateMovement = 1.0f;
			}
		}


		// builds bones recursively until it reaches SyncLevel
		List<Transform> BuildBones(Transform root, int level)
		{
			List<Transform> bones = new List<Transform>();
			bones.Add(root);

			if (level >= m_SyncLevel)
				return bones;

			var children = GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (child.parent == root)
				{
					// this is a direct child
					var subchildren = BuildBones(child, level + 1);
					foreach (var sub in subchildren)
					{
						bones.Add(sub);
					}
				}
			}
			return bones;
		}

		void Awake()
		{
			if (m_SyncLevel == 0)
			{
				m_Bones = m_Target.GetComponentsInChildren<Transform>();
			} 
			else
			{
				m_Bones = BuildBones(m_Target, 0).ToArray();
			}

			m_BoneInfos = new BoneInfo[m_Bones.Length];
			for (int i = 0; i < m_Bones.Length; i++)
			{
				m_BoneInfos[i] = new BoneInfo();
				m_BoneInfos[i].m_bone = m_Bones[i];
			}

			// cache these to avoid per-frame allocations.
			if (localPlayerAuthority)
			{
				m_LocalTransformWriter = new NetworkWriter();
			}
			numBones = m_Bones.Length;

			NetworkServer.RegisterHandler(SkeletonMsg, HandleSkeleton);
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			if (initialState)
			{
				// dont send in initial data. size is likely too large for default channel
				return true;
			}
			
			if (syncVarDirtyBits == 0)
			{
				writer.WritePackedUInt32(0);
				return false;
			}
			
			// dirty bits
			writer.WritePackedUInt32(1);

			SerializeModeTransform(writer);
			return true;
		}

		void SerializeModeTransform(NetworkWriter writer)
		{
			int start = writer.Position;
			foreach (var bone in m_Bones)
			{
				// position
				writer.Write(bone.localPosition);

				// rotation
				if (m_SyncRotationAxis != NetworkTransform.AxisSyncMode.None)
				{
					NetworkTransform.SerializeRotation3D(writer, bone.localRotation, syncRotationAxis, rotationSyncCompression);
				}
			}

			int sz = writer.Position - start;
			if (sz > 1400 && binarySize == 0)
			{
				// this is only generated once.
				Debug.LogWarning("NetworkSkeleton binary serialization size is very large:" + sz + ". Consider reducing the number of levels being synchronized.");
			}
			binarySize = sz;
		}


		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
				return;

			if (isServer && NetworkServer.localClientActive)
				return;

			if (reader.ReadPackedUInt32() == 0)
				return;

			UnserializeModeTransform(reader, initialState);

			m_LastClientSyncTime = Time.time;
		}

		void UnserializeModeTransform(NetworkReader reader, bool initialState)
		{
			if (hasAuthority)
			{
				// this component must read the data that the server wrote, even if it ignores it.
				// otherwise the NetworkReader stream will still contain that data for the next component.

				for (int i = 0; i < m_Bones.Length; i++)
				{
					// position
					reader.ReadVector3();

					if (syncRotationAxis != NetworkTransform.AxisSyncMode.None)
					{
						NetworkTransform.UnserializeRotation3D(reader, syncRotationAxis, rotationSyncCompression);
					}
				}
				return;
			}

			for (int i = 0; i < m_Bones.Length; i++)
			{
				var boneInfo = m_BoneInfos[i];

				// position
				boneInfo.m_TargetSyncPosition = reader.ReadVector3();

				// rotation
				if (syncRotationAxis != NetworkTransform.AxisSyncMode.None)
				{
					var rot = NetworkTransform.UnserializeRotation3D(reader, syncRotationAxis, rotationSyncCompression);
					boneInfo.m_TargetSyncRotation3D = rot;
				}
			}
		}

		void FixedUpdate()
		{
			if (isServer)
			{
				FixedUpdateServer();
			}
			if (isClient)
			{
				FixedUpdateClient();
			}
		}

		void FixedUpdateServer()
		{
			if (syncVarDirtyBits != 0)
				return;

			// dont run if network isn't active
			if (!NetworkServer.active)
				return;

			// dont run if we haven't been spawned yet
			if (!isServer)
				return;

			// dont' auto-dirty if no send interval
			if (GetNetworkSendInterval() == 0)
				return;

			// This will cause transform to be sent
			SetDirtyBit(1);
		}

		void FixedUpdateClient()
		{
			// dont run if we haven't received any sync data
			if (m_LastClientSyncTime == 0)
				return;

			// dont run if network isn't active
			if (!NetworkServer.active && !NetworkClient.active)
				return;

			// dont run if we haven't been spawned yet
			if (!isServer && !isClient)
				return;

			// dont run if not expecting continuous updates
			if (GetNetworkSendInterval() == 0)
				return;

			// dont run this if this client has authority over this player object
			if (hasAuthority)
				return;

			// interpolate on client
			for (int i = 0; i < m_Bones.Length; i++)
			{
				var bone = m_Bones[i];
				var boneInfo = m_BoneInfos[i];

				bone.localPosition = Vector3.Lerp(bone.localPosition, boneInfo.m_TargetSyncPosition, m_InterpolateMovement);
				bone.localRotation = Quaternion.Slerp(bone.localRotation, boneInfo.m_TargetSyncRotation3D, m_InterpolateRotation);
			}
		}


		// --------------------- local transform sync  ------------------------

		void Update()
		{
			if (!hasAuthority)
				return;

			if (!localPlayerAuthority)
				return;

			if (NetworkServer.active)
				return;

			if (Time.time - m_LastClientSendTime > GetNetworkSendInterval())
			{
				SendTransform();
				m_LastClientSendTime = Time.time;
			}
		}

		bool HasMoved()
		{
			//TODO - idle animation make this useless?
			return true;
		}

		[Client]
		void SendTransform()
		{
			if (!HasMoved() || ClientScene.readyConnection == null)
			{
				return;
			}

			m_LocalTransformWriter.StartMessage(SkeletonMsg);
			m_LocalTransformWriter.Write(netId);
			SerializeModeTransform(m_LocalTransformWriter);

			m_LocalTransformWriter.FinishMessage();

			ClientScene.readyConnection.SendWriter(m_LocalTransformWriter, GetNetworkChannel());
		}

		static internal void HandleSkeleton(NetworkMessage netMsg)
		{
			NetworkInstanceId netId = netMsg.reader.ReadNetworkId();

			GameObject foundObj = NetworkServer.FindLocalObject(netId);
			if (foundObj == null)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkSkeleton no gameObject"); }
				return;
			}

			NetworkSkeleton sk = foundObj.GetComponent<NetworkSkeleton>();
			if (sk == null)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkSkeleton null target"); }
				return;
			}

			if (!netMsg.conn.clientOwnedObjects.Contains(netId))
			{
				if (LogFilter.logWarn) { Debug.LogWarning("NetworkSkeleton netId:" + netId + " is not for a valid player"); }
				return;
			}

			sk.UnserializeModeTransform(netMsg.reader, false);
			sk.m_LastClientSyncTime = Time.time;
		}

		public override int GetNetworkChannel()
		{
			return m_NetworkChannel;
		}
		public override float GetNetworkSendInterval()
		{
			return m_SendInterval;
		}
	}
}
#endif //ENABLE_UNET