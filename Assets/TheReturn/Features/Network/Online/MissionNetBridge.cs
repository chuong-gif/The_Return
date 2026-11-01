/*
 * Mục đích: Cầu nối mạng DÙNG CHUNG cho mọi nhiệm vụ online (T-027). Chỉ chuyển CHUỖI: máy khách gửi lệnh lên host, host gửi trạng thái
 * (chung + riêng) cho đúng một client và gửi lại lý do khi từ chối lệnh. Không biết nhiệm vụ cụ thể: mỗi nhiệm vụ đăng ký một
 * IMissionHostEndpoint (host) và IMissionClientEndpoint (mọi máy) rồi tự mã hóa/giải mã chuỗi (vd PlankCodec). Đặt trên một NetworkObject
 * nằm sẵn trong scene. Giới hạn độ dài chuỗi để máy khách xấu không làm host tốn tài nguyên.
 * Hàm: SendCommand; RequestState; SendState (chỉ host); HostEndpoint; ClientEndpoint.
 */
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Online
{
    /// <summary>Phía host của một nhiệm vụ: xử lý lệnh và yêu cầu trạng thái từ máy khách.</summary>
    public interface IMissionHostEndpoint
    {
        /// <summary>Nhận mã client gửi, số thứ tự lệnh và chuỗi lệnh; trả null nếu chấp nhận, hoặc lý do từ chối (gửi lại cho người gửi).</summary>
        string OnCommand(ulong senderClientId, int sequence, string payload);

        /// <summary>Nhận mã client vừa hỏi: host nên gửi lại trạng thái đầy đủ cho client đó.</summary>
        void OnStateRequested(ulong senderClientId);
    }

    /// <summary>Phía máy khách (cả host) của một nhiệm vụ: nhận trạng thái và lý do bị từ chối.</summary>
    public interface IMissionClientEndpoint
    {
        /// <summary>Nhận phiên bản, chuỗi trạng thái chung và chuỗi thông tin riêng.</summary>
        void OnState(int version, string publicPayload, string privatePayload);

        /// <summary>Nhận lý do lệnh của mình bị host từ chối.</summary>
        void OnRejected(string detail);
    }

    public sealed class MissionNetBridge : NetworkBehaviour
    {
        private const int MaxPayloadLength = 2048;

        /// <summary>Phía host; chỉ cần đặt trên host.</summary>
        public IMissionHostEndpoint HostEndpoint { get; set; }

        /// <summary>Phía máy khách; đặt trên mọi máy chơi.</summary>
        public IMissionClientEndpoint ClientEndpoint { get; set; }

        /// <summary>Nhận số thứ tự lệnh và chuỗi lệnh; gửi lên host (host tự gửi cho chính mình cũng được). Không làm gì nếu chưa spawn.</summary>
        public void SendCommand(int sequence, string payload)
        {
            if (!IsSpawned) return;
            SubmitCommandRpc(sequence, payload ?? string.Empty);
        }

        /// <summary>Máy khách xin host gửi lại trạng thái đầy đủ (dùng khi vừa vào hoặc chưa nhận gì).</summary>
        public void RequestState()
        {
            if (!IsSpawned) return;
            RequestStateRpc();
        }

        /// <summary>Chỉ HOST. Nhận mã client, phiên bản, chuỗi chung và chuỗi riêng; gửi đúng cho client đó.</summary>
        public void SendState(ulong clientId, int version, string publicPayload, string privatePayload)
        {
            if (!IsServer || !IsSpawned) return;
            StateRpc(version, publicPayload ?? string.Empty, privatePayload ?? string.Empty, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SubmitCommandRpc(int sequence, string payload, RpcParams rpcParams = default)
        {
            IMissionHostEndpoint endpoint = HostEndpoint;
            if (endpoint == null) return;
            ulong sender = rpcParams.Receive.SenderClientId;
            if (payload != null && payload.Length > MaxPayloadLength) return;
            string rejected = endpoint.OnCommand(sender, sequence, payload);
            if (rejected != null) RejectedRpc(rejected, RpcTarget.Single(sender, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void RequestStateRpc(RpcParams rpcParams = default)
        {
            IMissionHostEndpoint endpoint = HostEndpoint;
            if (endpoint != null) endpoint.OnStateRequested(rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void StateRpc(int version, string publicPayload, string privatePayload, RpcParams rpcParams = default)
        {
            IMissionClientEndpoint endpoint = ClientEndpoint;
            if (endpoint != null) endpoint.OnState(version, publicPayload, privatePayload);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void RejectedRpc(string detail, RpcParams rpcParams = default)
        {
            IMissionClientEndpoint endpoint = ClientEndpoint;
            if (endpoint != null) endpoint.OnRejected(detail);
        }
    }
}
