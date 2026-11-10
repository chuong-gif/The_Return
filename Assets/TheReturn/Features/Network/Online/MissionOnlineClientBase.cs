/*
 * Mục đích: LỚP CƠ SỞ phần MÁY KHÁCH của mọi nhiệm vụ online (T-039, M5). Gom phần giống nhau của AttendanceOnlineClient/BreakerOnlineClient
 * (trùng ~53%): đăng ký làm đầu nhận của MissionNetBridge, nhận ô của người chơi cục bộ từ OnlineAvatar rồi dựng ClientReplica, giải mã và
 * áp dụng gói trạng thái (chung + riêng), gửi lệnh đã mã hóa với số thứ tự kế tiếp, tự xin host gửi lại trạng thái khi chưa nhận gì.
 * Lớp con chỉ lo phần RIÊNG của nhiệm vụ: giải mã, vẽ lại cảnh/HUD, đọc phím, tia nhìn. Chỉ THÊM, không đổi nhiệm vụ nào khác.
 * Cách dùng: lớp con ĐÓNG kiểu (non-generic) trong file cùng tên, ví dụ
 *   public sealed class XxxOnlineClient : MissionOnlineClientBase<XxxPublicState, XxxPrivateState> { ... }
 * Trường [SerializeField] bridge nằm ở đây (tên 'bridge' giữ nguyên nên scene cũ không mất tham chiếu).
 * Hàm: OnEnable/OnDisable; OnState; OnRejected; SendEncoded; PollForState; thuộc tính MySlot, HasState; hook trừu tượng LogTag, TryDecodePublic,
 * TryDecodePrivate, OnStateApplied; hook ảo OnSlotAssigned, OnSlotLost, OnRejectedDetail.
 */
using TheReturn.Network.Framework;
using UnityEngine;

namespace TheReturn.Network.Online
{
    /// <summary>Khung máy khách dùng chung: lớp con khai codec + cách vẽ.</summary>
    public abstract class MissionOnlineClientBase<TPub, TPriv> : MonoBehaviour, IMissionClientEndpoint
    {
        [SerializeField] protected MissionNetBridge bridge;

        /// <summary>Bản sao trạng thái của người chơi cục bộ (null khi chưa có ô).</summary>
        protected ClientReplica<TPub, TPriv> replica;

        /// <summary>Ô của người chơi cục bộ (-1 nếu chưa có).</summary>
        protected int mySlot = -1;

        /// <summary>Thời điểm (Time.time) nhận gói gần nhất, để ngoại suy đồng hồ giữa hai gói.</summary>
        protected float receivedAt;

        private float nextRequest;

        /// <summary>Ô của người chơi cục bộ (-1 nếu chưa có).</summary>
        public int MySlot { get { return mySlot; } }

        /// <summary>True khi đã nhận ít nhất một gói trạng thái.</summary>
        public bool HasState { get { return replica != null && replica.HasState; } }

        /// <summary>Tên ngắn trong log, ví dụ «BreakerClient».</summary>
        protected abstract string LogTag { get; }

        /// <summary>Giải mã trạng thái chung; false nếu chuỗi hỏng.</summary>
        protected abstract bool TryDecodePublic(string payload, out TPub state);

        /// <summary>Giải mã thông tin riêng; false nếu chuỗi hỏng.</summary>
        protected abstract bool TryDecodePrivate(string payload, out TPriv state);

        /// <summary>Gọi sau khi một gói MỚI hơn được áp dụng: vẽ lại cảnh/HUD từ trạng thái chung.</summary>
        protected abstract void OnStateApplied(TPub state);

        /// <summary>Gọi khi người chơi cục bộ vừa có ô (đặt lại các bộ đếm hiển thị).</summary>
        protected virtual void OnSlotAssigned()
        {
        }

        /// <summary>Gọi khi ô của người chơi cục bộ bị gỡ.</summary>
        protected virtual void OnSlotLost()
        {
        }

        /// <summary>Gọi sau khi ghi log lý do bị từ chối (vd hiện lên HUD).</summary>
        protected virtual void OnRejectedDetail(string detail)
        {
        }

        protected virtual void OnEnable()
        {
            if (bridge != null) bridge.ClientEndpoint = this;
            OnlineAvatar.SlotReady += HandleReady;
            OnlineAvatar.Removed += HandleRemoved;
            OnlineAvatar.ForEachReady(HandleReady);
        }

        protected virtual void OnDisable()
        {
            OnlineAvatar.SlotReady -= HandleReady;
            OnlineAvatar.Removed -= HandleRemoved;
            if (bridge != null && bridge.ClientEndpoint == (IMissionClientEndpoint)this) bridge.ClientEndpoint = null;
        }

        private void HandleReady(OnlineAvatar avatar)
        {
            if (!avatar.IsOwner) return;
            mySlot = avatar.Slot;
            replica = new ClientReplica<TPub, TPriv>(mySlot);
            OnSlotAssigned();
        }

        private void HandleRemoved(OnlineAvatar avatar, int slot)
        {
            if (slot != mySlot) return;
            replica = null;
            mySlot = -1;
            OnSlotLost();
        }

        /// <summary>Nhận phiên bản + chuỗi chung + chuỗi riêng từ host: giải mã, bỏ gói cũ/hỏng, áp dụng gói mới.</summary>
        public void OnState(int version, string publicPayload, string privatePayload)
        {
            if (replica == null) return;
            TPub state;
            TPriv own;
            if (!TryDecodePublic(publicPayload, out state) || !TryDecodePrivate(privatePayload, out own)) return;
            if (!replica.Receive(new StateMessage<TPub, TPriv>(mySlot, version, state, own))) return;
            receivedAt = Time.time;
            OnStateApplied(state);
        }

        /// <summary>Nhận lý do lệnh của mình bị host từ chối: ghi log rồi báo lớp con.</summary>
        public void OnRejected(string detail)
        {
            Debug.Log("[" + LogTag + "] Ô " + mySlot + " bị host từ chối lệnh: " + detail);
            OnRejectedDetail(detail);
        }

        /// <summary>Gửi một lệnh ĐÃ MÃ HÓA lên host với số thứ tự kế tiếp (bỏ qua nếu chưa có ô/cầu nối).</summary>
        protected void SendEncoded(string payload)
        {
            if (replica == null || bridge == null) return;
            bridge.SendCommand(replica.NextSequence(), payload);
        }

        /// <summary>
        /// Gọi đầu Update của lớp con: trả true khi đã có trạng thái để vẽ/nhập phím; nếu chưa thì cứ 1 giây xin host gửi lại và trả false.
        /// </summary>
        protected bool PollForState()
        {
            if (replica == null || bridge == null) return false;
            if (replica.HasState) return true;
            if (Time.time >= nextRequest)
            {
                nextRequest = Time.time + 1f;
                bridge.RequestState();
            }
            return false;
        }
    }
}
