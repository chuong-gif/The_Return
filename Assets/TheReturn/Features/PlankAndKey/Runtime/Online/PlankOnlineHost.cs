/*
 * Mục đích: Phần HOST của nhiệm vụ cầu ván online (T-027). Dựng PlankHostRules + HostAuthority theo số người của đội, nhận lệnh từ
 * MissionNetBridge, lấy ô người gửi từ OnlinePartySlots (không tin nội dung lệnh), cung cấp IPlankWorld từ vị trí nhân vật và điểm mốc trong
 * scene, chạy đồng hồ (Tick) và phát trạng thái đúng từng người mỗi khi phiên bản đổi. Chỉ chạy khi máy này là host.
 * Hàm: Update; OnCommand; OnStateRequested; kiểu lồng HostWorld (IPlankWorld).
 */
using System.Collections.Generic;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.PlankNet.Online
{
    public sealed class PlankOnlineHost : MonoBehaviour, IMissionHostEndpoint
    {
        [SerializeField] private PlankAndKeyPrototype prototype;
        [SerializeField] private MissionNetBridge bridge;
        [SerializeField] private float failResetSeconds = 2.5f;
        [Tooltip("0 = đề ngẫu nhiên mỗi ván; khác 0 = đề cố định để thử.")]
        [SerializeField] private int fixedSeed;

        private PlankHostRules rules;
        private HostAuthority<PlankCommand, PlankPublicState, PlankPrivateState> authority;
        private PlankLane lane;
        private readonly Dictionary<int, Transform> avatars = new Dictionary<int, Transform>();
        private PlankInteractable[] interactables = new PlankInteractable[0];
        private int lastBroadcastVersion = -1;

        /// <summary>Phiên bản trạng thái hiện tại của host (-1 nếu chưa dựng).</summary>
        public int Version { get { return authority != null ? authority.Version : -1; } }

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
            if (authority != null) authority.DisconnectSlot(slot);
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
            if (slots == null || bridge == null || !bridge.IsSpawned || prototype == null) return false;
            lane = new PlankLane(prototype);
            if (!lane.IsReady) return false;

            int teamSize = Mathf.Clamp(slots.TeamSize, PlankRound.MinPlayers, PlankRound.MaxPlayers);
            float perUnit = prototype.settings != null ? prototype.settings.secondsPerUnit : 3f;
            int slack = prototype.settings != null ? prototype.settings.slackPercent : PlankGenerator.DefaultSlackPercent;
            int seed = fixedSeed != 0 ? fixedSeed : System.Environment.TickCount;
            rules = new PlankHostRules(PlankGenerator.Generate(teamSize, seed, slack), perUnit, new HostWorld(this), failResetSeconds);

            HostLog log = new HostLog();
            log.Sink = entry =>
            {
                if (entry.Level == HostLogLevel.Warning) Debug.LogWarning("[PlankHost] " + entry.Message);
                else Debug.Log("[PlankHost] " + entry.Message);
            };
            authority = new HostAuthority<PlankCommand, PlankPublicState, PlankPrivateState>(slots.Roster, rules, log);
            interactables = Object.FindObjectsOfType<PlankInteractable>(true);
            bridge.HostEndpoint = this;
            lastBroadcastVersion = -1;
            foreach (KeyValuePair<int, Transform> pair in avatars) authority.ConnectSlot(pair.Key);
            Debug.Log("[PlankHost] Bắt đầu nhiệm vụ cầu ván: " + teamSize + " người, seed " + seed);
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
            foreach (StateMessage<PlankPublicState, PlankPrivateState> message in authority.BuildBroadcast())
            {
                ulong clientId;
                if (!slots.TryGetClient(message.Slot, out clientId)) continue;
                bridge.SendState(clientId, message.Version, PlankCodec.EncodePublic(message.Public), PlankCodec.EncodePrivate(message.Private));
            }
        }

        /// <summary>Nhận mã client, số thứ tự và chuỗi lệnh từ mạng; trả null nếu lệnh được chấp nhận, hoặc lý do từ chối.</summary>
        public string OnCommand(ulong senderClientId, int sequence, string payload)
        {
            if (authority == null) return "Host chưa sẵn sàng.";
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            int slot = slots != null ? slots.SlotOf(senderClientId) : -1;
            if (slot < 0) return "Bạn chưa có ô trong đội.";
            PlankCommand command;
            if (!PlankCodec.TryDecodeCommand(payload, out command)) return "Lệnh sai định dạng.";
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
            StateMessage<PlankPublicState, PlankPrivateState> message = authority.BuildStateFor(slot);
            if (message == null) return;
            bridge.SendState(senderClientId, message.Version, PlankCodec.EncodePublic(message.Public), PlankCodec.EncodePrivate(message.Private));
        }

        private sealed class HostWorld : IPlankWorld
        {
            private readonly PlankOnlineHost host;

            public HostWorld(PlankOnlineHost owner)
            {
                host = owner;
            }

            private float Reach { get { return host.prototype.interactDistance + 0.8f; } }

            public bool IsNearCard(int slot)
            {
                Transform body;
                if (!host.avatars.TryGetValue(slot, out body)) return false;
                return Vector3.Distance(body.position, host.prototype.card.position) <= Reach;
            }

            public bool IsNearLever(int slot, bool sideA)
            {
                Transform body;
                if (!host.avatars.TryGetValue(slot, out body)) return false;
                PlankInteractKind wanted = sideA ? PlankInteractKind.LeverA : PlankInteractKind.LeverB;
                for (int i = 0; i < host.interactables.Length; i++)
                {
                    PlankInteractable item = host.interactables[i];
                    if (item != null && item.kind == wanted && Vector3.Distance(body.position, item.transform.position) <= Reach) return true;
                }
                return false;
            }

            public int[] PlayersOnDeck()
            {
                List<int> on = new List<int>();
                foreach (KeyValuePair<int, Transform> pair in host.avatars)
                {
                    if (pair.Value != null && pair.Value.gameObject.activeInHierarchy && host.lane.InDeckZone(pair.Value.position)) on.Add(pair.Key);
                }
                return on.ToArray();
            }

            public PlankDropSpot DropSpot(int slot)
            {
                Transform body;
                if (!host.avatars.TryGetValue(slot, out body)) return new PlankDropSpot(false, 0f, 0f);
                Vector3 feet = body.position;
                Vector3 ahead = feet + body.forward * 0.7f;
                bool aheadOk = host.lane.InDeckZone(ahead) || Mathf.Abs(host.lane.ToLane(ahead).z) > 4.4f;
                Vector3 pick = aheadOk ? ahead : feet;
                Vector3 pickLane = host.lane.ToLane(pick);
                if (!host.lane.InDeckZone(pick)) return new PlankDropSpot(false, pickLane.x, pickLane.z);
                Vector3 deckLane = host.lane.ToLane(host.prototype.deck.position);
                return new PlankDropSpot(true, pickLane.x - deckLane.x, pickLane.z - deckLane.z);
            }
        }
    }
}
