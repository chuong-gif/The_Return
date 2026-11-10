/*
 * Mục đích: Phần MÁY KHÁCH của nhiệm vụ điểm danh online (T-031), chạy trên mọi máy (cả host vì host cũng là một người chơi). Không chạy luật:
 * gửi lệnh (E ngồi/đứng, R làm lại) lên host qua MissionNetBridge, nhận trạng thái chung + manh mối RIÊNG của mình, rồi VẼ lại nhãn/màu ghế,
 * bảng và HUD từ bản sao đó. Khi host báo mình đã ngồi/đứng thì đưa nhân vật của chính mình tới điểm ngồi/đứng (nhân vật do chủ máy điều khiển).
 * Hàm: Update; OnState; OnRejected; RequestSit/RequestStand/RequestRetry; PlaceNearSeat; OnGUI; thuộc tính HasState, MySlot, PublicState, Clues.
 */
using System.Text;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.AttendanceNet.Online
{
    [DefaultExecutionOrder(60)]
    public sealed class AttendanceOnlineClient : MonoBehaviour, IMissionClientEndpoint
    {
        [SerializeField] private AttendanceSeat[] seats = new AttendanceSeat[0];
        [SerializeField] private AttendanceTextCatalog textCatalog;
        [SerializeField] private MissionNetBridge bridge;
        [SerializeField] private PrototypePartyController party;
        [SerializeField] private OnlinePartyLocalDriver localDriver;
        [SerializeField] private TextMesh board;

        private ClientReplica<AttendancePublicState, AttendancePrivateState> replica;
        private int mySlot = -1;
        private int mySeat = -1;
        private int handledResetCount = -1;
        private AttendanceMessage lastShownMessage = AttendanceMessage.None;
        private string status = string.Empty;
        private float statusUntil;
        private float nextRequest;
        private int targetSeat = -1;

        /// <summary>Ô của người chơi trên máy này (-1 nếu chưa có nhân vật).</summary>
        public int MySlot { get { return mySlot; } }

        /// <summary>True khi đã nhận gói trạng thái đầu từ host.</summary>
        public bool HasState { get { return replica != null && replica.HasState; } }

        /// <summary>Trạng thái chung mới nhất (null nếu chưa có).</summary>
        public AttendancePublicState PublicState { get { return HasState ? replica.Public : null; } }

        /// <summary>Manh mối RIÊNG của người chơi trên máy này (rỗng nếu chưa có).</summary>
        public string Clues { get { return HasState ? replica.Private.Clues : string.Empty; } }

        /// <summary>Ghế người chơi trên máy này đang ngồi theo host (-1 nếu không ngồi).</summary>
        public int MySeat { get { return mySeat; } }

        private void OnEnable()
        {
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
            replica = new ClientReplica<AttendancePublicState, AttendancePrivateState>(mySlot);
            handledResetCount = -1;
            mySeat = -1;
        }

        private void HandleRemoved(OnlineAvatar avatar, int slot)
        {
            if (slot != mySlot) return;
            replica = null;
            mySlot = -1;
            mySeat = -1;
            if (localDriver != null) localDriver.Seated = false;
        }

        /// <summary>Nhận phiên bản và hai chuỗi từ host; giải mã, áp dụng vào bản sao nếu mới hơn.</summary>
        public void OnState(int version, string publicPayload, string privatePayload)
        {
            if (replica == null) return;
            AttendancePublicState state;
            AttendancePrivateState own;
            if (!AttendanceCodec.TryDecodePublic(publicPayload, out state) || !AttendanceCodec.TryDecodePrivate(privatePayload, out own)) return;
            if (!replica.Receive(new StateMessage<AttendancePublicState, AttendancePrivateState>(mySlot, version, state, own))) return;
            ApplyState(state);
        }

        /// <summary>Nhận lý do host từ chối lệnh của mình; hiện lên HUD vài giây.</summary>
        public void OnRejected(string detail)
        {
            Debug.Log("[AttClient] Ô " + mySlot + " bị host từ chối lệnh: " + detail);
            Say(detail, 4f);
        }

        /// <summary>Gửi lệnh ngồi vào ghế (0-23) lên host.</summary>
        public void RequestSit(int seat)
        {
            Send(AttendanceCommand.Sit(seat));
        }

        /// <summary>Gửi lệnh đứng dậy lên host.</summary>
        public void RequestStand()
        {
            Send(AttendanceCommand.Stand());
        }

        /// <summary>Gửi lệnh làm lại lên host.</summary>
        public void RequestRetry()
        {
            Send(AttendanceCommand.Retry());
        }

        /// <summary>Đưa nhân vật của mình tới điểm đứng cạnh một ghế (dùng khi kiểm thử tự động). Nhận chỉ số ghế.</summary>
        public void PlaceNearSeat(int seat)
        {
            if (party == null || mySlot < 0 || seat < 0 || seat >= seats.Length || seats[seat] == null || seats[seat].standPoint == null) return;
            party.Teleport(mySlot, seats[seat].standPoint);
        }

        private void Send(AttendanceCommand command)
        {
            if (replica == null || bridge == null) return;
            bridge.SendCommand(replica.NextSequence(), AttendanceCodec.EncodeCommand(command));
        }

        private void ApplyState(AttendancePublicState state)
        {
            int[] seatOwners = state.SeatsCopy();
            string[] names = state.NamesCopy();
            for (int i = 0; i < seats.Length && i < seatOwners.Length; i++)
            {
                if (seats[i] == null) continue;
                int who = seatOwners[i];
                seats[i].Show(who >= 0 && who < names.Length ? names[who] : null, state.Solved, state.Wrong);
            }
            if (board != null)
                board.text = "LOP 101  /  24 GHE\nH1 GAN BANG  /  C1 BEN TRAI\n" + (state.Solved ? "DIEM DANH HOP LE" : "DOI " + state.PlayerCount + " NGUOI");

            int newSeat = -1;
            for (int i = 0; i < seatOwners.Length; i++) if (seatOwners[i] == mySlot) newSeat = i;
            if (newSeat != mySeat) MoveMyAvatar(mySeat, newSeat);
            mySeat = newSeat;
            if (localDriver != null) localDriver.Seated = mySeat >= 0;
            handledResetCount = state.ResetCount;

            if (state.LastMessage != lastShownMessage)
            {
                lastShownMessage = state.LastMessage;
                string text = MessageText(state);
                if (text.Length > 0) Say(text, 5f);
            }
        }

        private void MoveMyAvatar(int fromSeat, int toSeat)
        {
            if (party == null || mySlot < 0 || mySlot >= party.players.Length || party.players[mySlot] == null) return;
            if (toSeat >= 0 && toSeat < seats.Length && seats[toSeat] != null && seats[toSeat].sitPoint != null) party.Teleport(mySlot, seats[toSeat].sitPoint);
            else if (fromSeat >= 0 && fromSeat < seats.Length && seats[fromSeat] != null && seats[fromSeat].standPoint != null) party.Teleport(mySlot, seats[fromSeat].standPoint);
        }

        private void Update()
        {
            if (replica == null || bridge == null) return;
            if (!replica.HasState)
            {
                if (Time.time >= nextRequest)
                {
                    nextRequest = Time.time + 1f;
                    bridge.RequestState();
                }
                return;
            }
            UpdateTarget();
            HandleInput();
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard.rKey.wasPressedThisFrame) RequestRetry();
            if (!keyboard.eKey.wasPressedThisFrame) return;
            if (mySeat >= 0) RequestStand();
            else if (targetSeat >= 0) RequestSit(targetSeat);
            else Say("Hãy đến gần một ghế và nhìn về phía ghế đó.", 3f);
        }

        // Chọn ghế đang nhắm: tia trúng ghế thì lấy ghế đó, nếu không lấy ghế gần nhất phía trước theo mặt phẳng ngang (như bản 1 máy, T-019).
        private void UpdateTarget()
        {
            targetSeat = -1;
            if (party == null || mySlot < 0 || mySlot >= party.players.Length || party.players[mySlot] == null) return;
            RaycastHit hit;
            if (party.TryGetInteractionHit(3.2f, out hit))
            {
                AttendanceSeat hitSeat = hit.collider.GetComponentInParent<AttendanceSeat>();
                if (hitSeat != null) { targetSeat = hitSeat.index; return; }
            }
            Transform body = party.players[mySlot];
            Vector3 fwd = party.viewCamera != null ? party.viewCamera.transform.forward : body.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) { fwd = body.forward; fwd.y = 0f; }
            fwd.Normalize();
            float best = float.MaxValue;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i] == null || seats[i].sitPoint == null) continue;
                Vector3 to = seats[i].sitPoint.position - body.position;
                if (Mathf.Abs(to.y) > 2.5f) continue;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist > 2.6f) continue;
                float angle = dist < 0.3f ? 0f : Vector3.Angle(fwd, to);
                if (angle > 55f) continue;
                float score = angle / 55f + dist / 2.6f;
                if (score < best) { best = score; targetSeat = seats[i].index; }
            }
        }

        private void Say(string text, float seconds)
        {
            status = text;
            statusUntil = Time.time + seconds;
        }

        private string MessageText(AttendancePublicState state)
        {
            string[] names = state.NamesCopy();
            switch (state.LastMessage)
            {
                case AttendanceMessage.Seated: return string.Empty;
                case AttendanceMessage.Stood: return string.Empty;
                case AttendanceMessage.Retry: return "Đã làm lại: mọi người đứng dậy, vẫn là đề cũ.";
                case AttendanceMessage.Wrong: return textCatalog != null ? textCatalog.wrong : "Chưa đúng. Đối chiếu manh mối rồi đứng dậy đổi chỗ.";
                case AttendanceMessage.Solved: return textCatalog != null ? textCatalog.success : "Điểm danh hợp lệ.";
                default: return names.Length > 0 ? string.Empty : string.Empty;
            }
        }

        private void OnGUI()
        {
            if (replica == null || !replica.HasState) return;
            AttendancePublicState state = replica.Public;
            string[] names = state.NamesCopy();
            StringBuilder text = new StringBuilder();
            text.Append(textCatalog != null ? textCatalog.title : "ĐIỂM DANH").Append("  (").Append(state.OccupiedCount()).Append("/").Append(state.PlayerCount).Append(" người đã ngồi)\n");
            text.Append("Bạn là ").Append(mySlot >= 0 && mySlot < names.Length ? names[mySlot] : "?").Append(". Manh mối CHỈ mình bạn đọc được, hãy nói cho đồng đội:\n");
            text.Append(replica.Private.Clues).Append('\n');
            if (state.Solved) text.Append(">> ĐÃ ĐÚNG. ").Append(textCatalog != null ? textCatalog.success : string.Empty).Append('\n');
            else if (state.Settling) text.Append("Đã ngồi đủ, đang kiểm tra...\n");
            else if (state.Wrong) text.Append(">> CHƯA ĐÚNG, hãy đứng dậy đổi chỗ.\n");
            text.Append(mySeat >= 0 ? "[E] đứng dậy   " : (targetSeat >= 0 ? "[E] ngồi ghế " + AttendanceRound.SeatCode(targetSeat) + "   " : "Đến gần một ghế   ")).Append("[R] làm lại  [Tab] khóa/thả chuột");
            GUI.Box(new Rect(16, 16, 700, 190), text.ToString());
            if (Time.time < statusUntil && status.Length > 0) GUI.Box(new Rect(16, 214, 700, 34), status);
        }
    }
}
