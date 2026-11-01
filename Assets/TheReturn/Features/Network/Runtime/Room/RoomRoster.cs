/*
 * Mục đích: Danh sách phòng ĐỒNG BỘ qua NGO (T-026). Một đối tượng mạng do HOST sinh ra khi bắt đầu host. Host giữ luật thật (RoomState);
 * client chỉ gửi tên của mình và yêu cầu Bắt đầu, host kiểm rồi cập nhật NetworkList cho mọi người.
 * Hàm: Awake; OnNetworkSpawn; OnNetworkDespawn; SubmitNameServerRpc; StartGameServerRpc; RequestStart; CanAdmit; RefreshRows; thuộc tính Current, TeamSize, Started, Rows.
 */
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Room
{
    /// <summary>Một dòng phòng gửi qua mạng (ô, mã client, tên, có phải chủ phòng).</summary>
    public struct RoomNetRow : INetworkSerializable, IEquatable<RoomNetRow>
    {
        public ulong ClientId;
        public int Slot;
        public FixedString128Bytes Name;
        public bool IsHost;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Slot);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref IsHost);
        }

        public bool Equals(RoomNetRow other)
        {
            return ClientId == other.ClientId && Slot == other.Slot && Name.Equals(other.Name) && IsHost == other.IsHost;
        }
    }

    public sealed class RoomRoster : NetworkBehaviour
    {
        /// <summary>Danh sách phòng đang hoạt động (null nếu chưa vào phòng).</summary>
        public static RoomRoster Current { get; private set; }

        /// <summary>Số người của đội (2-4), do host đặt.</summary>
        public NetworkVariable<int> TeamSize = new NetworkVariable<int>(2);

        /// <summary>True sau khi chủ phòng đã bắt đầu ván.</summary>
        public NetworkVariable<bool> Started = new NetworkVariable<bool>(false);

        /// <summary>Các dòng phòng, chỉ host ghi.</summary>
        public NetworkList<RoomNetRow> Rows;

        private RoomState state; // chỉ host có

        private void Awake()
        {
            Rows = new NetworkList<RoomNetRow>();
        }

        public override void OnNetworkSpawn()
        {
            Current = this;
            if (IsServer)
            {
                state = new RoomState(Mathf.Clamp(RoomService.PendingTeamSize, 2, 4), NetworkManager.ServerClientId);
                TeamSize.Value = state.TeamSize;
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
                state.Join(NetworkManager.LocalClientId, RoomService.PendingName);
                RefreshRows();
            }
            else
            {
                SubmitNameServerRpc(RoomService.PendingName ?? "");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
            if (IsServer && NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        /// <summary>Chỉ host dùng khi duyệt kết nối. Nhận số kết nối hiện có. Trả true nếu còn chỗ và ván chưa bắt đầu.</summary>
        public bool CanAdmit(int currentConnected)
        {
            return state != null ? state.CanAdmit(currentConnected) : currentConnected < Mathf.Clamp(RoomService.PendingTeamSize, 2, 4);
        }

        /// <summary>Nút Bắt đầu: host gọi trực tiếp, client gửi yêu cầu lên host. Host kiểm đủ người + đúng chủ phòng.</summary>
        public void RequestStart()
        {
            if (IsServer) TryStartOnHost(NetworkManager.LocalClientId);
            else StartGameServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            if (state == null) return;
            ulong id = rpcParams.Receive.SenderClientId;
            if (state.Join(id, playerName) < 0)
            {
                NetworkManager.DisconnectClient(id, "Phòng đã đủ người hoặc ván đã bắt đầu");
                return;
            }
            RefreshRows();
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams rpcParams = default)
        {
            TryStartOnHost(rpcParams.Receive.SenderClientId);
        }

        private void TryStartOnHost(ulong requester)
        {
            if (state != null && state.TryStart(requester)) Started.Value = true;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (state == null) return;
            if (state.Leave(clientId)) RefreshRows();
        }

        private void RefreshRows()
        {
            Rows.Clear();
            foreach (RoomRow row in state.Rows())
            {
                Rows.Add(new RoomNetRow { ClientId = row.ClientId, Slot = row.Slot, Name = new FixedString128Bytes(row.Name), IsHost = row.IsHost });
            }
        }
    }
}
