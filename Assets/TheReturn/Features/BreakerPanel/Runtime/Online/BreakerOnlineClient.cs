/*
 * Mục đích: Phần MÁY KHÁCH của nhiệm vụ «Bảng cầu dao» online (T-031), chạy trên mọi máy (cả host vì host cũng là một người chơi). Không chạy luật:
 * gửi lệnh (E bật/tắt công tắc đang nhắm, R làm lại) lên host qua MissionNetBridge, nhận trạng thái chung + thông tin RIÊNG của mình (công tắc
 * của mình và đèn mỗi công tắc đảo), rồi VẼ lại công tắc/đèn và HUD từ bản sao đó. Tiến độ giữ khớp được ngoại suy cục bộ giữa hai gói. Theo mẫu AttendanceOnlineClient; nay kế thừa MissionOnlineClientBase (T-039 M5: đăng ký cầu nối, ô, bản sao, giải mã gói, gửi lệnh, xin lại trạng thái nằm ở lớp cơ sở).
 * Hàm: Update; OnState; OnRejected; RequestToggle/RequestRetry; PlaceNearSwitch; OnGUI; thuộc tính HasState, MySlot, PublicState, Private.
 */
using System.Text;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.BreakerNet.Online
{
    [DefaultExecutionOrder(60)]
    public sealed class BreakerOnlineClient : MissionOnlineClientBase<BreakerPublicState, BreakerPrivateState>
    {
        [SerializeField] private BreakerSwitch[] switches = new BreakerSwitch[0];
        [SerializeField] private BreakerLamp[] lamps = new BreakerLamp[0];
        [SerializeField] private BreakerSettings settings;
        [SerializeField] private PrototypePartyController party;

        private BreakerMessage lastShownMessage = BreakerMessage.None;
        private int lastShownTrips;
        private int lastShownResets;
        private string status = string.Empty;
        private float statusUntil;
        private int targetSwitch = -1;

        protected override string LogTag { get { return "BreakerClient"; } }

        protected override bool TryDecodePublic(string payload, out BreakerPublicState state) { return BreakerCodec.TryDecodePublic(payload, out state); }

        protected override bool TryDecodePrivate(string payload, out BreakerPrivateState state) { return BreakerCodec.TryDecodePrivate(payload, out state); }

        protected override void OnSlotAssigned()
        {
            lastShownMessage = BreakerMessage.None;
            lastShownTrips = 0;
            lastShownResets = 0;
        }

        protected override void OnStateApplied(BreakerPublicState state)
        {
            ApplyState(state);
        }

        protected override void OnRejectedDetail(string detail)
        {
            Say(detail, 4f);
        }

        private void Update()
        {
            if (!PollForState()) return;
            UpdateTarget();
            HandleInput();
        }



        /// <summary>Trạng thái chung mới nhất (null nếu chưa có).</summary>
        public BreakerPublicState PublicState { get { return HasState ? replica.Public : null; } }

        /// <summary>Thông tin RIÊNG của người chơi trên máy này (rỗng nếu chưa có).</summary>
        public BreakerPrivateState Private { get { return HasState ? replica.Private : new BreakerPrivateState(null, null); } }

        /// <summary>Gửi lệnh bật/tắt công tắc (0-5) lên host.</summary>
        public void RequestToggle(int sw)
        {
            SendEncoded(BreakerCodec.EncodeCommand(BreakerCommand.Toggle(sw)));
        }

        /// <summary>Gửi lệnh làm lại cùng đề lên host.</summary>
        public void RequestRetry()
        {
            SendEncoded(BreakerCodec.EncodeCommand(BreakerCommand.Retry()));
        }

        /// <summary>Đưa nhân vật của mình tới công tắc (dùng khi kiểm thử tự động). Nhận chỉ số công tắc.</summary>
        public void PlaceNearSwitch(int sw)
        {
            if (party == null || mySlot < 0 || sw < 0 || sw >= switches.Length || switches[sw] == null) return;
            party.Teleport(mySlot, switches[sw].transform);
        }

        private void ApplyState(BreakerPublicState state)
        {
            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] == null) continue;
                bool used = i < state.Size;
                switches[i].gameObject.SetActive(used);
                if (used) switches[i].Show(state.SwitchOn(i), i % state.PlayerCount);
            }
            for (int i = 0; i < lamps.Length; i++)
            {
                if (lamps[i] == null) continue;
                bool used = i < state.Size;
                lamps[i].gameObject.SetActive(used);
                if (used) lamps[i].Show(state.LampOn(i), state.TargetOn(i));
            }
            if (state.LastMessage != lastShownMessage || state.Trips != lastShownTrips || state.ResetCount != lastShownResets)
            {
                lastShownMessage = state.LastMessage;
                lastShownTrips = state.Trips;
                lastShownResets = state.ResetCount;
                string text = MessageText(state);
                if (text.Length > 0) Say(text, 5f);
            }
        }

        private string MessageText(BreakerPublicState state)
        {
            switch (state.LastMessage)
            {
                case BreakerMessage.Tripped: return settings != null ? settings.tripped : "Cầu dao nhảy! Bật quá nhiều công tắc, tất cả về tắt.";
                case BreakerMessage.Solved: return settings != null ? settings.success : "Bảng cầu dao ổn định!";
                case BreakerMessage.Retry: return "Đã đặt lại bảng cầu dao.";
                case BreakerMessage.Toggled: return "Đang bật " + state.OnCount + "/" + state.OverloadLimit + " công tắc.";
                default: return string.Empty;
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard.rKey.wasPressedThisFrame) RequestRetry();
            if (!keyboard.eKey.wasPressedThisFrame) return;
            if (targetSwitch < 0) { Say("Hãy nhìn vào một công tắc ở gần.", 3f); return; }
            if (!replica.Private.Owns(targetSwitch)) { Say(settings != null ? settings.notOwner : "Công tắc này không phải của bạn.", 3f); return; }
            RequestToggle(targetSwitch);
        }

        // Chọn công tắc đang nhắm bằng tia nhìn (như bản 1 máy); host vẫn kiểm lại chủ và khoảng cách.
        private void UpdateTarget()
        {
            targetSwitch = -1;
            if (party == null || mySlot < 0 || mySlot >= party.players.Length || party.players[mySlot] == null) return;
            RaycastHit hit;
            if (!party.TryGetInteractionHit(3f, out hit)) return;
            BreakerSwitch sw = hit.collider.GetComponentInParent<BreakerSwitch>();
            if (sw != null) targetSwitch = sw.index;
        }

        private void Say(string text, float seconds)
        {
            status = text;
            statusUntil = Time.time + seconds;
        }

        private float HoldNow(BreakerPublicState state)
        {
            if (state.Solved) return state.HoldSeconds;
            if (!state.Matching) return 0f;
            return Mathf.Min(state.HoldSeconds, state.HoldElapsed + (Time.time - receivedAt));
        }

        private void OnGUI()
        {
            if (replica == null || !replica.HasState) return;
            BreakerPublicState state = replica.Public;
            BreakerPrivateState own = replica.Private;
            StringBuilder text = new StringBuilder("BẢNG CẦU DAO  (đội ").Append(state.PlayerCount).Append(" người)\n");
            text.Append("Bạn là vai ").Append(mySlot + 1).Append(". Công tắc CHỈ mình bạn xem được, hãy nói cho đồng đội:\n");
            int[] sw = own.SwitchesCopy();
            int[] mask = own.MasksCopy();
            for (int i = 0; i < sw.Length; i++)
            {
                text.Append("Công tắc ").Append(sw[i] + 1).Append(" đảo đèn: ");
                for (int l = 0; l < state.Size; l++) if (((mask[i] >> l) & 1) == 1) text.Append(l + 1).Append(' ');
                text.Append('\n');
            }
            float hold = HoldNow(state);
            text.Append("Đang bật ").Append(state.OnCount).Append('/').Append(state.OverloadLimit).Append("   Giữ khớp: ")
                .Append(state.HoldSeconds > 0f ? (hold / state.HoldSeconds * 100f).ToString("0") : (state.Matching ? "100" : "0")).Append("%\n");
            if (state.Solved) text.Append(">> ĐÃ ỔN ĐỊNH.\n");
            text.Append(targetSwitch >= 0 ? "[E] bật/tắt công tắc " + (targetSwitch + 1) + "   " : "Nhìn vào công tắc của bạn   ").Append("[R] làm lại  [Tab] khóa/thả chuột");
            GUI.Box(new Rect(16, 16, 700, 40 + 22 * (sw.Length + 4)), text.ToString());
            if (Time.time < statusUntil && status.Length > 0) GUI.Box(new Rect(16, 64 + 22 * (sw.Length + 4), 700, 34), status);
        }
    }
}
