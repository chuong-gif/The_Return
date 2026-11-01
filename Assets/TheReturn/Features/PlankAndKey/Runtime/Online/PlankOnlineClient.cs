/*
 * Mục đích: Phần MÁY KHÁCH của nhiệm vụ cầu ván online (T-027), chạy trên mọi máy (cả host vì host cũng là một người chơi). Không chạy luật:
 * gửi lệnh (E nhặt thẻ/gạt cần, G thả thẻ, R làm lại) lên host qua MissionNetBridge, nhận trạng thái chung + thông tin riêng, rồi VẼ lại
 * ván, thẻ từ, nhãn và HUD từ bản sao đó. Người ngồi trên ván được đưa đi theo ván trên chính máy của họ (nhân vật do chủ máy điều khiển). Về
 * đầu khe khi host báo đặt lại. PlankAndKeyPrototype cũ vẫn nằm trong scene nhưng ở trạng thái tắt (không Begin), chỉ dùng làm nơi giữ điểm mốc.
 * Hàm: Update; LateUpdate; OnState; OnRejected; OnGUI.
 */
using System.Text;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.PlankNet.Online
{
    [DefaultExecutionOrder(60)]
    public sealed class PlankOnlineClient : MonoBehaviour, IMissionClientEndpoint
    {
        [SerializeField] private PlankAndKeyPrototype prototype;
        [SerializeField] private MissionNetBridge bridge;
        [SerializeField] private PrototypePartyController party;
        [SerializeField] private OnlinePartyLocalDriver localDriver;

        private PlankLane lane;
        private ClientReplica<PlankPublicState, PlankPrivateState> replica;
        private int mySlot = -1;
        private float snapshotTime;
        private float leftAtSnapshot;
        private int handledResetCount = -1;
        private bool wasRider;
        private Vector3 riderLocal;
        private string status = string.Empty;
        private float statusUntil;
        private float nextRequest;
        private PlankMessage lastShownMessage = PlankMessage.None;
        private const float RideAnimationHold = 0.4f;
        private readonly PlayerAnimationDriver[] riderDrivers = new PlayerAnimationDriver[PlankRound.MaxPlayers];
        private readonly Transform[] riderDriverOwners = new Transform[PlankRound.MaxPlayers];
        private readonly float[] rideHoldUntil = new float[PlankRound.MaxPlayers];

        /// <summary>Bản sao trạng thái hiện có (null nếu chưa có nhân vật của mình); để test và HUD đọc.</summary>
        public ClientReplica<PlankPublicState, PlankPrivateState> Replica { get { return replica; } }

        private void OnEnable()
        {
            lane = new PlankLane(prototype);
            if (bridge != null) bridge.ClientEndpoint = this;
            OnlineAvatar.SlotReady += HandleReady;
            OnlineAvatar.Removed += HandleRemoved;
            OnlineAvatar.ForEachReady(HandleReady);
        }

        private void OnDisable()
        {
            OnlineAvatar.SlotReady -= HandleReady;
            OnlineAvatar.Removed -= HandleRemoved;
            if (bridge != null && bridge.ClientEndpoint == (IMissionClientEndpoint)this) bridge.ClientEndpoint = null;
        }

        private void HandleReady(OnlineAvatar avatar)
        {
            if (!avatar.IsOwner) return;
            mySlot = avatar.Slot;
            replica = new ClientReplica<PlankPublicState, PlankPrivateState>(mySlot);
            handledResetCount = -1;
            wasRider = false;
        }

        private void HandleRemoved(OnlineAvatar avatar, int slot)
        {
            if (slot != mySlot) return;
            replica = null;
            mySlot = -1;
            if (localDriver != null) localDriver.Seated = false;
        }

        /// <summary>Nhận phiên bản và hai chuỗi từ host; giải mã, áp dụng vào bản sao nếu mới hơn.</summary>
        public void OnState(int version, string publicPayload, string privatePayload)
        {
            if (replica == null) return;
            PlankPublicState state;
            PlankPrivateState own;
            if (!PlankCodec.TryDecodePublic(publicPayload, out state) || !PlankCodec.TryDecodePrivate(privatePayload, out own)) return;
            if (!replica.Receive(new StateMessage<PlankPublicState, PlankPrivateState>(mySlot, version, state, own))) return;
            snapshotTime = Time.time;
            leftAtSnapshot = state.CrossingSecondsLeft;
            ApplyTransitions(state);
        }

        /// <summary>Nhận lý do host từ chối lệnh của mình; hiện lên HUD vài giây.</summary>
        public void OnRejected(string detail)
        {
            Say(detail, 4f);
        }

        private void ApplyTransitions(PlankPublicState state)
        {
            if (handledResetCount != state.ResetCount)
            {
                bool first = handledResetCount < 0;
                handledResetCount = state.ResetCount;
                PlaceAtStart(state.PlayerCount);
                if (!first) Say("Đã đặt lại: cả đội về đầu khe.", 4f);
            }
            bool rider = state.Phase == PlankPhase.Crossing && Contains(state.RidersCopy(), mySlot);
            if (rider && !wasRider && party != null && mySlot < party.players.Length && party.players[mySlot] != null)
                riderLocal = prototype.deck.InverseTransformPoint(party.players[mySlot].position);
            wasRider = rider;
            if (state.LastMessage != lastShownMessage)
            {
                lastShownMessage = state.LastMessage;
                string text = MessageText(state);
                if (text.Length > 0) Say(text, 5f);
            }
        }

        private void PlaceAtStart(int count)
        {
            if (party == null || mySlot < 0 || mySlot >= party.players.Length || party.players[mySlot] == null || !lane.IsReady) return;
            party.Teleport(mySlot, lane.StandPoint(mySlot, count));
            party.players[mySlot].rotation = Quaternion.Euler(0f, lane.StartYaw(), 0f);
        }

        private void Update()
        {
            if (replica == null || party == null || bridge == null || !lane.IsReady) return;
            if (!replica.HasState)
            {
                if (Time.time >= nextRequest)
                {
                    nextRequest = Time.time + 1f;
                    bridge.RequestState();
                }
                return;
            }
            PlankPublicState state = replica.Public;
            float left = state.Phase == PlankPhase.Crossing ? Mathf.Max(0f, leftAtSnapshot - (Time.time - snapshotTime)) : 0f;
            prototype.deck.position = lane.DeckPosition(state, left);
            prototype.deck.rotation = lane.DeckRestRotation();
            bool rides = state.Phase == PlankPhase.Crossing && Contains(state.RidersCopy(), mySlot);
            if (rides && mySlot < party.players.Length && party.players[mySlot] != null)
                party.Teleport(mySlot, prototype.deck.TransformPoint(riderLocal), false);
            if (localDriver != null) localDriver.Seated = rides;
            UpdateRiderAnimations(state);
            HandleInput();
        }

        // Người ngồi trên ván bị ván chở đi nên vị trí đổi, nhưng họ đứng yên: tắt animation đi/chạy của họ trên mọi máy.
        private void UpdateRiderAnimations(PlankPublicState state)
        {
            int[] riders = state.Phase == PlankPhase.Crossing ? state.RidersCopy() : new int[0];
            int count = Mathf.Min(state.PlayerCount, riderDrivers.Length);
            for (int slot = 0; slot < count; slot++)
            {
                Transform body = slot < party.players.Length ? party.players[slot] : null;
                if (body == null)
                {
                    riderDrivers[slot] = null;
                    riderDriverOwners[slot] = null;
                    continue;
                }
                if (riderDriverOwners[slot] != body)
                {
                    riderDriverOwners[slot] = body;
                    riderDrivers[slot] = body.GetComponentInChildren<PlayerAnimationDriver>(true);
                }
                if (riderDrivers[slot] == null) continue;
                if (Contains(riders, slot)) rideHoldUntil[slot] = Time.time + RideAnimationHold;
                riderDrivers[slot].SuppressLocomotion = Time.time < rideHoldUntil[slot];
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard.rKey.wasPressedThisFrame) Send(PlankCommand.Retry());
            if (keyboard.gKey.wasPressedThisFrame) Send(PlankCommand.DropCard());
            if (!keyboard.eKey.wasPressedThisFrame) return;
            RaycastHit hit;
            if (!party.TryGetInteractionHit(prototype.interactDistance, out hit)) return;
            PlankInteractable item = hit.collider.GetComponentInParent<PlankInteractable>();
            if (item == null) return;
            if (item.kind == PlankInteractKind.Card) Send(PlankCommand.PickUpCard());
            else Send(PlankCommand.UseLever(item.kind == PlankInteractKind.LeverA));
        }

        private void Send(PlankCommand command)
        {
            bridge.SendCommand(replica.NextSequence(), PlankCodec.EncodeCommand(command));
        }

        private void LateUpdate()
        {
            if (replica == null || !replica.HasState || party == null || !lane.IsReady) return;
            PlankPublicState state = replica.Public;
            Transform holder = null;
            if (state.CardMode == PlankCardMode.Held && state.CardHolder >= 0 && state.CardHolder < party.players.Length)
                holder = party.players[state.CardHolder];
            Transform card = prototype.card;
            card.position = lane.CardPosition(state, prototype.deck.position, holder);
            if (holder != null) card.rotation = Quaternion.Euler(0f, holder.eulerAngles.y, 0f);
            Collider collider = card.GetComponent<Collider>();
            if (collider != null) collider.enabled = state.CardMode != PlankCardMode.Held;

            if (prototype.labels == null || party.viewCamera == null) return;
            foreach (TextMesh label in prototype.labels)
            {
                if (label == null) continue;
                Vector3 look = party.viewCamera.transform.position - label.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f) label.transform.rotation = Quaternion.LookRotation(-look);
            }
        }

        private void Say(string text, float seconds)
        {
            status = text;
            statusUntil = Time.time + seconds;
        }

        private static bool Contains(int[] values, int wanted)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == wanted) return true;
            }
            return false;
        }

        private static string MessageText(PlankPublicState state)
        {
            switch (state.LastMessage)
            {
                case PlankMessage.CardPicked: return "Vai " + (state.CardHolder + 1) + " đã cầm thẻ từ.";
                case PlankMessage.CardDropped: return "Thẻ từ đã được thả xuống sàn.";
                case PlankMessage.CardOnDeck: return "Thẻ từ đã nằm trên ván.";
                case PlankMessage.CrossingStarted: return "Ván chạy sang bờ bên kia trong " + state.CrossingSecondsTotal.ToString("0") + " giây.";
                case PlankMessage.Arrived: return "Ván đã tới bờ.  Còn " + Mathf.Max(0f, state.DeadlineSeconds - state.DisplayUsedSeconds).ToString("0") + " giây.";
                case PlankMessage.Solved: return "Cả đội đã qua khe!";
                case PlankMessage.TimedOut: return "Hết thời gian! Cả đội về đầu khe.";
                default: return string.Empty;
            }
        }

        private string CardText(PlankPublicState state)
        {
            switch (state.CardMode)
            {
                case PlankCardMode.Held: return "vai " + (state.CardHolder + 1) + " đang cầm";
                case PlankCardMode.OnDeck: return "đang nằm trên ván";
                case PlankCardMode.OnFloor: return lane.ToLane(prototype.card.position).z < 0f ? "đang nằm ở bờ đầu khe" : "đang nằm ở bờ bên kia";
                default: return "đang nằm ở bờ đầu khe";
            }
        }

        private void OnGUI()
        {
            if (replica == null || !replica.HasState || prototype == null) return;
            PlankPublicState state = replica.Public;
            float perUnit = prototype.settings != null ? prototype.settings.secondsPerUnit : 3f;
            StringBuilder text = new StringBuilder();
            text.Append(prototype.settings != null ? prototype.settings.title : "CẦU VÁN VÀ THẺ TỪ").Append("  (lần ").Append(state.Attempts).Append(")\n");
            text.Append("Thời gian qua khe: ").Append(state.DisplayUsedSeconds.ToString("0.0")).Append(" / ").Append(state.DeadlineSeconds.ToString("0")).Append(" giây\n");
            text.Append("Vai ").Append(mySlot + 1).Append(": cặp sách của bạn mất ").Append((replica.Private.CrossUnits * perUnit).ToString("0"))
                .Append(" giây để qua (đi đôi thì tính người chậm hơn). Chỉ bạn biết, hãy hỏi đồng đội.\n");
            text.Append("Thẻ từ: ").Append(CardText(state)).Append('\n');
            text.Append("[E] nhặt thẻ / gạt cần  [G] thả thẻ  [R] làm lại  [Tab] khóa/thả chuột");
            GUI.Box(new Rect(16, 16, 640, 130), text.ToString());
            if (Time.time < statusUntil && status.Length > 0) GUI.Box(new Rect(16, 154, 640, 34), status);
        }
    }
}
