/*
 * Mục đích: Phần MÁY KHÁCH của nhiệm vụ «Hợp xướng» online (T-037 GĐ2), chạy trên mọi máy (cả host vì host cũng là một người chơi). Không chạy luật:
 * gửi lệnh (E bấm bục đang nhắm, R làm lại) lên host qua MissionNetBridge, nhận trạng thái chung (bản nhạc, nốt tới, số lần sai) + THẺ NHẠC RIÊNG
 * của mình (ký hiệu -> bục), rồi vẽ bục/HUD từ bản sao đó. Kế thừa MissionOnlineClientBase (đăng ký cầu nối, ô, bản sao, giải mã, gửi lệnh nằm ở lớp cơ sở).
 * Hàm: Update; OnStateApplied; OnRejectedDetail; RequestPress/RequestRetry; PlaceNearPad; OnGUI; thuộc tính HasState, MySlot, PublicState, Private.
 */
using System.Text;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.ChoirNet.Online
{
    [DefaultExecutionOrder(60)]
    public sealed class ChoirOnlineClient : MissionOnlineClientBase<ChoirPublicState, ChoirPrivateState>
    {
        static readonly string[] SymbolNames = { "A", "B", "C", "D" };

        [SerializeField] private ChoirPad[] pads = new ChoirPad[0];
        [SerializeField] private ChoirSettings settings;
        [SerializeField] private PrototypePartyController party;

        private int lastShownResets = -1;
        private int lastShownProgress = -1;
        private ChoirMessage lastShownMessage = ChoirMessage.None;
        private string status = string.Empty;
        private float statusUntil;
        private int targetPad = -1;

        protected override string LogTag { get { return "ChoirClient"; } }

        protected override bool TryDecodePublic(string payload, out ChoirPublicState state) { return ChoirCodec.TryDecodePublic(payload, out state); }

        protected override bool TryDecodePrivate(string payload, out ChoirPrivateState state) { return ChoirCodec.TryDecodePrivate(payload, out state); }

        protected override void OnSlotAssigned()
        {
            lastShownResets = -1;
            lastShownProgress = -1;
            lastShownMessage = ChoirMessage.None;
        }

        protected override void OnStateApplied(ChoirPublicState state)
        {
            if (state.ResetCount != lastShownResets || state.Progress != lastShownProgress || state.LastMessage != lastShownMessage)
            {
                lastShownResets = state.ResetCount;
                lastShownProgress = state.Progress;
                lastShownMessage = state.LastMessage;
                FlashPad(state);
                string text = MessageText(state);
                if (text.Length > 0) Say(text, 4f);
            }
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
        public ChoirPublicState PublicState { get { return HasState ? replica.Public : null; } }

        /// <summary>Thẻ nhạc RIÊNG của người chơi trên máy này (rỗng nếu chưa có).</summary>
        public ChoirPrivateState Private { get { return HasState ? replica.Private : new ChoirPrivateState(null, null); } }

        /// <summary>Nhận chỉ số bục (0-3); gửi lệnh bấm bục lên host.</summary>
        public void RequestPress(int pad)
        {
            SendEncoded(ChoirCodec.EncodeCommand(ChoirCommand.Press(pad)));
        }

        /// <summary>Gửi lệnh làm lại cùng đề lên host.</summary>
        public void RequestRetry()
        {
            SendEncoded(ChoirCodec.EncodeCommand(ChoirCommand.Retry()));
        }

        /// <summary>Nhận chỉ số bục; đưa nhân vật của mình tới bục (dùng khi kiểm thử tự động).</summary>
        public void PlaceNearPad(int pad)
        {
            if (party == null || mySlot < 0 || pad < 0 || pad >= pads.Length || pads[pad] == null) return;
            party.Teleport(mySlot, pads[pad].transform);
        }

        private void FlashPad(ChoirPublicState state)
        {
            int pad = state.LastPad;
            if (pad < 0 || pad >= pads.Length || pads[pad] == null) return;
            Color color;
            switch (state.LastMessage)
            {
                case ChoirMessage.Correct:
                case ChoirMessage.Completed: color = new Color(.2f, .8f, .3f); break;
                case ChoirMessage.Wrong:
                case ChoirMessage.TooLate: color = new Color(.85f, .2f, .2f); break;
                default: return;
            }
            pads[pad].Flash(color, .6f);
        }

        private string MessageText(ChoirPublicState state)
        {
            switch (state.LastMessage)
            {
                case ChoirMessage.Correct: return "Đúng nốt! Tiếp theo.";
                case ChoirMessage.Completed: return "Hợp xướng hoàn tất!";
                case ChoirMessage.Wrong: return "Sai nốt, bắt đầu lại từ đầu.";
                case ChoirMessage.TooLate: return "Chậm quá, bắt đầu lại từ đầu.";
                case ChoirMessage.TimedOut: return "Hết giờ giữa hai nốt, bắt đầu lại.";
                case ChoirMessage.Retry: return "Đã đặt lại bản nhạc.";
                default: return string.Empty;
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard.rKey.wasPressedThisFrame) RequestRetry();
            if (!keyboard.eKey.wasPressedThisFrame) return;
            if (targetPad < 0) { Say("Hãy nhìn vào một bục ở gần.", 3f); return; }
            RequestPress(targetPad);
        }

        // Chọn bục đang nhắm bằng tia nhìn (như bản 1 máy); host vẫn kiểm lại khoảng cách.
        private void UpdateTarget()
        {
            targetPad = -1;
            if (party == null || mySlot < 0 || mySlot >= party.players.Length || party.players[mySlot] == null) return;
            RaycastHit hit;
            if (!party.TryGetInteractionHit(3f, out hit)) return;
            ChoirPad pad = hit.collider.GetComponentInParent<ChoirPad>();
            if (pad != null) targetPad = pad.index;
        }

        private void Say(string text, float seconds)
        {
            status = text;
            statusUntil = Time.time + seconds;
        }

        private void OnGUI()
        {
            if (replica == null || !replica.HasState) return;
            ChoirPublicState state = replica.Public;
            ChoirPrivateState own = replica.Private;
            StringBuilder text = new StringBuilder("HỢP XƯỚNG  (đội ").Append(state.PlayerCount).Append(" người)\n");
            text.Append("Bản nhạc: ");
            for (int i = 0; i < state.NoteCount; i++)
            {
                int s = state.SymbolAt(i);
                text.Append(i == state.Progress ? "[" : "").Append(s >= 0 && s < SymbolNames.Length ? SymbolNames[s] : "?").Append(i == state.Progress ? "]" : "").Append(' ');
            }
            text.Append("\nThẻ của bạn (ký hiệu -> bục): ");
            int[] syms = own.SymbolsCopy();
            int[] padOf = own.PadsCopy();
            for (int i = 0; i < syms.Length; i++)
                text.Append(syms[i] >= 0 && syms[i] < SymbolNames.Length ? SymbolNames[syms[i]] : "?").Append("=").Append(padOf[i] + 1).Append("  ");
            text.Append("\nTiến độ ").Append(state.Progress).Append('/').Append(state.NoteCount)
                .Append("   Sai ").Append(state.Mistakes).Append("   Quá giờ ").Append(state.Timeouts)
                .Append("   Cửa sổ ").Append(state.WindowSeconds.ToString("0.0")).Append("s\n");
            if (state.Solved) text.Append(">> ĐÃ HÁT XONG.\n");
            text.Append(targetPad >= 0 ? "[E] bấm bục " + (targetPad + 1) + "   " : "Nhìn vào bục   ").Append("[R] làm lại  [Tab] khóa/thả chuột");
            GUI.Box(new Rect(16, 16, 700, 150), text.ToString());
            if (Time.time < statusUntil && status.Length > 0) GUI.Box(new Rect(16, 174, 700, 34), status);
        }
    }
}
