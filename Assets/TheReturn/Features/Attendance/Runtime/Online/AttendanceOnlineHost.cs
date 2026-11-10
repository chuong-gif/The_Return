/*
 * Mục đích: Phần HOST của nhiệm vụ điểm danh online (T-031). Dựng AttendanceHostRules + HostAuthority theo số người của đội, nhận lệnh từ
 * MissionNetBridge, lấy ô người gửi từ OnlinePartySlots (không tin nội dung lệnh), cung cấp IAttendanceWorld từ vị trí nhân vật và điểm ngồi
 * của ghế trong scene, chạy đồng hồ chấm (Tick) và phát trạng thái đúng từng người mỗi khi phiên bản đổi. Người mất kết nối thì được cho đứng
 * dậy. Chỉ chạy khi máy này là host.
 * Hàm: Update; OnCommand; OnStateRequested; SolutionSeatsForTest; kiểu lồng HostWorld (IAttendanceWorld).
 */
using System.Collections.Generic;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.AttendanceNet.Online
{
    public sealed class AttendanceOnlineHost : MonoBehaviour, IMissionHostEndpoint
    {
        [SerializeField] private AttendanceSeat[] seats = new AttendanceSeat[0];
        [SerializeField] private AttendanceTextCatalog textCatalog;
        [SerializeField] private MissionNetBridge bridge;
        [Tooltip("Khoảng cách tối đa (m) từ nhân vật tới điểm ngồi để được ngồi (giống bản 1 máy: 3,5).")]
        [SerializeField] private float sitReach = 3.5f;
        [SerializeField] private float settleSeconds = 2f;
        [Tooltip("0 = đề ngẫu nhiên mỗi ván; khác 0 = đề cố định để thử.")]
        [SerializeField] private int fixedSeed;

        private AttendanceHostRules rules;
        private HostAuthority<AttendanceCommand, AttendancePublicState, AttendancePrivateState> authority;
        private readonly Dictionary<int, Transform> avatars = new Dictionary<int, Transform>();
        private int lastBroadcastVersion = -1;

        /// <summary>Phiên bản trạng thái hiện tại của host (-1 nếu chưa dựng).</summary>
        public int Version { get { return authority != null ? authority.Version : -1; } }

        /// <summary>Chỉ để KIỂM THỬ TỰ ĐỘNG trên host: đáp án (ghế của từng ô), hoặc null nếu chưa dựng đề. Không bao giờ gửi qua mạng.</summary>
        public int[] SolutionSeatsForTest { get { return rules != null ? rules.Round.CopySolution() : null; } }

        private void OnEnable()
        {
            OnlineAvatar.SlotReady += HandleReady;
            OnlineAvatar.Removed += HandleRemoved;
            OnlineAvatar.ForEachReady(HandleReady);
        }

        private void OnDisable()
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
            if (rules != null && rules.ReleaseSeat(slot)) authority.MarkChanged();
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
            if (rules.Tick(Time.deltaTime)) authority.MarkChanged();
            BroadcastIfChanged();
        }

        private bool TryBuild()
        {
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null || bridge == null || !bridge.IsSpawned || textCatalog == null || seats.Length != AttendanceRound.SeatCount) return false;
            int teamSize = Mathf.Clamp(slots.TeamSize, 2, 4);
            string[] names = new string[teamSize];
            for (int i = 0; i < teamSize; i++) names[i] = textCatalog.roleNames[i];
            int seed = fixedSeed != 0 ? fixedSeed : System.Environment.TickCount;
            rules = new AttendanceHostRules(names, seed, round => textCatalog.BuildPackets(round), new HostWorld(this), settleSeconds);

            HostLog log = new HostLog();
            log.Sink = entry =>
            {
                if (entry.Level == HostLogLevel.Warning) Debug.LogWarning("[AttendanceHost] " + entry.Message);
                else Debug.Log("[AttendanceHost] " + entry.Message);
            };
            authority = new HostAuthority<AttendanceCommand, AttendancePublicState, AttendancePrivateState>(slots.Roster, rules, log);
            bridge.HostEndpoint = this;
            lastBroadcastVersion = -1;
            foreach (KeyValuePair<int, Transform> pair in avatars) authority.ConnectSlot(pair.Key);
            Debug.Log("[AttendanceHost] Bắt đầu nhiệm vụ điểm danh: " + teamSize + " người, seed " + seed);
            return true;
        }

        private void Teardown()
        {
            if (bridge != null && bridge.HostEndpoint == (IMissionHostEndpoint)this) bridge.HostEndpoint = null;
            authority = null;
            rules = null;
            lastBroadcastVersion = -1;
        }

        private void BroadcastIfChanged()
        {
            if (authority.Version == lastBroadcastVersion) return;
            lastBroadcastVersion = authority.Version;
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null) return;
            foreach (StateMessage<AttendancePublicState, AttendancePrivateState> message in authority.BuildBroadcast())
            {
                ulong clientId;
                if (!slots.TryGetClient(message.Slot, out clientId)) continue;
                bridge.SendState(clientId, message.Version, AttendanceCodec.EncodePublic(message.Public), AttendanceCodec.EncodePrivate(message.Private));
            }
        }

        /// <summary>Nhận mã client, số thứ tự và chuỗi lệnh từ mạng; trả null nếu lệnh được chấp nhận, hoặc lý do từ chối.</summary>
        public string OnCommand(ulong senderClientId, int sequence, string payload)
        {
            if (authority == null) return "Host chưa sẵn sàng.";
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            int slot = slots != null ? slots.SlotOf(senderClientId) : -1;
            if (slot < 0) return "Bạn chưa có ô trong đội.";
            AttendanceCommand command;
            if (!AttendanceCodec.TryDecodeCommand(payload, out command)) return "Lệnh sai định dạng.";
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
            StateMessage<AttendancePublicState, AttendancePrivateState> message = authority.BuildStateFor(slot);
            if (message == null) return;
            bridge.SendState(senderClientId, message.Version, AttendanceCodec.EncodePublic(message.Public), AttendanceCodec.EncodePrivate(message.Private));
        }

        private sealed class HostWorld : IAttendanceWorld
        {
            private readonly AttendanceOnlineHost host;

            public HostWorld(AttendanceOnlineHost owner)
            {
                host = owner;
            }

            public bool IsNearSeat(int slot, int seat)
            {
                Transform body;
                if (seat < 0 || seat >= host.seats.Length || host.seats[seat] == null || host.seats[seat].sitPoint == null) return false;
                if (!host.avatars.TryGetValue(slot, out body) || body == null) return false;
                return Vector3.Distance(body.position, host.seats[seat].sitPoint.position) <= host.sitReach;
            }
        }
    }
}
