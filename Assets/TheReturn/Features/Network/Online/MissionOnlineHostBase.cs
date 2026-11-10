/*
 * Mục đích: LỚP CƠ SỞ phần HOST của mọi nhiệm vụ online (T-039, M5). Gom phần giống hệt nhau của AttendanceOnlineHost/BreakerOnlineHost
 * (trùng ~81%): theo dõi nhân vật vào/ra (OnlineAvatar), dựng HostAuthority khi đã là host và cảnh đã sẵn sàng, chạy đồng hồ, nhận lệnh từ
 * MissionNetBridge (lấy ô người gửi từ OnlinePartySlots, KHÔNG tin nội dung lệnh), phát trạng thái đúng từng người khi Version đổi, gửi lại
 * khi được hỏi, dọn khi không còn là host. Nhiệm vụ chỉ khai: dựng luật, đồng hồ, codec, và hook tùy chọn. Chỉ THÊM, không đổi nhiệm vụ nào khác.
 * Cách dùng: lớp con ĐÓNG kiểu (non-generic) trong file cùng tên (để Unity gắn được component), ví dụ
 *   public sealed class XxxOnlineHost : MissionOnlineHostBase<XxxCommand, XxxPublicState, XxxPrivateState> { ... }
 * Trường [SerializeField] bridge nằm ở đây (tên 'bridge' giữ nguyên nên scene cũ không mất tham chiếu).
 * Hàm: OnEnable/OnDisable; Update; TryBuild; Teardown; BroadcastIfChanged; OnCommand; OnStateRequested; TryGetAvatar; Version;
 * hook trừu tượng LogTag, TryCreateRules, TickRules, EncodePublic, EncodePrivate, TryDecodeCommand; hook ảo OnBuilt, OnSlotRemoved, OnTeardown.
 */
using System.Collections.Generic;
using TheReturn.Network.Framework;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Online
{
    /// <summary>Khung host dùng chung: lớp con khai luật + codec + đồng hồ.</summary>
    public abstract class MissionOnlineHostBase<TCmd, TPub, TPriv> : MonoBehaviour, IMissionHostEndpoint
    {
        [SerializeField] protected MissionNetBridge bridge;

        private HostAuthority<TCmd, TPub, TPriv> authority;
        private readonly Dictionary<int, Transform> avatars = new Dictionary<int, Transform>();
        private int lastBroadcastVersion = -1;

        /// <summary>Phiên bản trạng thái hiện tại của host (-1 nếu chưa dựng).</summary>
        public int Version { get { return authority != null ? authority.Version : -1; } }

        /// <summary>True khi host đã dựng xong ván (có thể kiểm thử/đọc trạng thái).</summary>
        protected bool IsBuilt { get { return authority != null; } }

        /// <summary>Bộ điều phối host (dùng khi lớp con cần MarkChanged ngoài đồng hồ).</summary>
        protected HostAuthority<TCmd, TPub, TPriv> Authority { get { return authority; } }

        /// <summary>Tên ngắn trong log, ví dụ «BreakerHost».</summary>
        protected abstract string LogTag { get; }

        /// <summary>
        /// Dựng luật của ván cho teamSize người (2-4). Trả false (không ném lỗi) nếu cảnh chưa đủ vật thể để dựng; host sẽ thử lại ở khung hình sau.
        /// Lớp con tự giữ tham chiếu luật có kiểu cụ thể nếu cần (Tick, đáp án kiểm thử).
        /// </summary>
        protected abstract bool TryCreateRules(int teamSize, out IHostRules<TCmd, TPub, TPriv> rules);

        /// <summary>Cho đồng hồ của luật chạy deltaSeconds; trả true nếu trạng thái đã đổi.</summary>
        protected abstract bool TickRules(float deltaSeconds);

        /// <summary>Mã hóa trạng thái chung.</summary>
        protected abstract string EncodePublic(TPub state);

        /// <summary>Mã hóa thông tin riêng.</summary>
        protected abstract string EncodePrivate(TPriv state);

        /// <summary>Giải mã lệnh từ mạng; trả false nếu sai định dạng.</summary>
        protected abstract bool TryDecodeCommand(string payload, out TCmd command);

        /// <summary>Gọi một lần ngay sau khi dựng xong (mặc định ghi log số người).</summary>
        protected virtual void OnBuilt(int teamSize)
        {
            Debug.Log("[" + LogTag + "] Bắt đầu nhiệm vụ: " + teamSize + " người");
        }

        /// <summary>Gọi khi một người rời (sau khi đã ngắt ô). Trả true nếu trạng thái đổi (vd nhả ghế).</summary>
        protected virtual bool OnSlotRemoved(int slot)
        {
            return false;
        }

        /// <summary>Gọi khi dọn ván (không còn là host hoặc bị tắt): lớp con bỏ tham chiếu luật cụ thể.</summary>
        protected virtual void OnTeardown()
        {
        }

        /// <summary>Lấy thân nhân vật của ô slot (cho thế giới giả tính khoảng cách); false nếu chưa có.</summary>
        protected bool TryGetAvatar(int slot, out Transform body)
        {
            return avatars.TryGetValue(slot, out body) && body != null;
        }

        protected virtual void OnEnable()
        {
            OnlineAvatar.SlotReady += HandleReady;
            OnlineAvatar.Removed += HandleRemoved;
            OnlineAvatar.ForEachReady(HandleReady);
        }

        protected virtual void OnDisable()
        {
            OnlineAvatar.SlotReady -= HandleReady;
            OnlineAvatar.Removed -= HandleRemoved;
            Teardown();
        }

        private void HandleReady(OnlineAvatar avatar)
        {
            avatars[avatar.Slot] = avatar.transform;
            if (authority != null) authority.ConnectSlot(avatar.Slot);
        }

        private void HandleRemoved(OnlineAvatar avatar, int slot)
        {
            avatars.Remove(slot);
            if (authority == null) return;
            authority.DisconnectSlot(slot);
            if (OnSlotRemoved(slot)) authority.MarkChanged();
        }

        private void Update()
        {
            NetworkManager manager = NetworkManager.Singleton;
            bool hosting = manager != null && manager.IsListening && manager.IsServer;
            if (!hosting)
            {
                if (authority != null) Teardown();
                return;
            }
            if (authority == null && !TryBuild()) return;
            if (TickRules(Time.deltaTime)) authority.MarkChanged();
            BroadcastIfChanged();
        }

        private bool TryBuild()
        {
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null || bridge == null || !bridge.IsSpawned) return false;
            int teamSize = Mathf.Clamp(slots.TeamSize, 2, 4);
            IHostRules<TCmd, TPub, TPriv> built;
            if (!TryCreateRules(teamSize, out built) || built == null) return false;
            string tag = LogTag;
            HostLog log = new HostLog();
            log.Sink = entry =>
            {
                if (entry.Level == HostLogLevel.Warning) Debug.LogWarning("[" + tag + "] " + entry.Message);
                else Debug.Log("[" + tag + "] " + entry.Message);
            };
            authority = new HostAuthority<TCmd, TPub, TPriv>(slots.Roster, built, log);
            bridge.HostEndpoint = this;
            lastBroadcastVersion = -1;
            foreach (KeyValuePair<int, Transform> pair in avatars) authority.ConnectSlot(pair.Key);
            OnBuilt(teamSize);
            return true;
        }

        private void Teardown()
        {
            if (bridge != null && bridge.HostEndpoint == (IMissionHostEndpoint)this) bridge.HostEndpoint = null;
            authority = null;
            lastBroadcastVersion = -1;
            OnTeardown();
        }

        private void BroadcastIfChanged()
        {
            if (authority.Version == lastBroadcastVersion) return;
            lastBroadcastVersion = authority.Version;
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null) return;
            foreach (StateMessage<TPub, TPriv> message in authority.BuildBroadcast())
            {
                ulong clientId;
                if (!slots.TryGetClient(message.Slot, out clientId)) continue;
                bridge.SendState(clientId, message.Version, EncodePublic(message.Public), EncodePrivate(message.Private));
            }
        }

        /// <summary>Nhận mã client, số thứ tự và chuỗi lệnh từ mạng; trả null nếu lệnh được chấp nhận, hoặc lý do từ chối.</summary>
        public string OnCommand(ulong senderClientId, int sequence, string payload)
        {
            if (authority == null) return "Host chưa sẵn sàng.";
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            int slot = slots != null ? slots.SlotOf(senderClientId) : -1;
            if (slot < 0) return "Bạn chưa có ô trong đội.";
            TCmd command;
            if (!TryDecodeCommand(payload, out command)) return "Lệnh sai định dạng.";
            CommandResult result = authority.Submit(slot, sequence, command);
            BroadcastIfChanged();
            return result.IsAccepted ? null : result.Detail;
        }

        /// <summary>Nhận mã client vừa hỏi; gửi lại trạng thái đầy đủ cho client đó.</summary>
        public void OnStateRequested(ulong senderClientId)
        {
            if (authority == null) return;
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            int slot = slots != null ? slots.SlotOf(senderClientId) : -1;
            if (slot < 0) return;
            StateMessage<TPub, TPriv> message = authority.BuildStateFor(slot);
            if (message == null) return;
            bridge.SendState(senderClientId, message.Version, EncodePublic(message.Public), EncodePrivate(message.Private));
        }
    }
}
