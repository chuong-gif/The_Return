/*
 * Mục đích: Phần MÁY KHÁCH của nhiệm vụ «Bảng cầu dao» online (T-031), chạy trên mọi máy (cả host vì host cũng là một người chơi). Không chạy luật:
 * gửi lệnh (E bật/tắt công tắc đang nhắm, R làm lại) lên host qua MissionNetBridge, nhận trạng thái chung + thông tin RIÊNG của mình (công tắc
 * của mình và đèn mỗi công tắc đảo), rồi VẼ lại công tắc/đèn và HUD từ bản sao đó. Tiến độ giữ khớp được ngoại suy cục bộ giữa hai gói. Theo mẫu AttendanceOnlineClient; nay kế thừa MissionOnlineClientBase (T-039 M5: đăng ký cầu nối, ô, bản sao, giải mã gói, gửi lệnh, xin lại trạng thái nằm ở lớp cơ sở).
 * T-056: chữ hướng dẫn do BreakerGuide soạn (đề dễ: công tắc N bật đèn N), khung HUD kiểu ModernGui, đèn có số «ĐÈN n».
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
    public sealed class BreakerOnlineClient : MissionOnlineClientBase<BreakerPublicState, BreakerPrivateState>, IOnlineHudSource
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

        private bool labelsMade;

        // Tạo nhãn «ĐÈN n» một lần, lấy nhãn công tắc đầu tiên làm mẫu.
        private void EnsureLabels()
        {
            if (labelsMade) return;
            TextMesh template = null;
            for (int i = 0; i < switches.Length && template == null; i++)
                if (switches[i] != null) template = switches[i].label;
            if (template == null) return;
            for (int i = 0; i < lamps.Length; i++)
                if (lamps[i] != null) lamps[i].EnsureNumberLabel(template);
            labelsMade = true;
        }

        private void ApplyState(BreakerPublicState state)
        {
            EnsureLabels();
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
                case BreakerMessage.Retry: return "Đã đặt lại bảng cầu dao: mọi công tắc về tắt.";
                case BreakerMessage.Toggled:
                    string what = state.LastSwitch >= 0 ? (state.SwitchOn(state.LastSwitch) ? "Đã BẬT công tắc " : "Đã TẮT công tắc ") + (state.LastSwitch + 1) + " (đèn " + (state.LastSwitch + 1) + "). " : string.Empty;
                    return what + "Đang bật " + state.OnCount + "/" + state.OverloadLimit + " công tắc.";
                default: return string.Empty;
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard.Control(GameAction.Retry).wasPressedThisFrame) RequestRetry();
            if (!keyboard.Control(GameAction.Interact).wasPressedThisFrame) return;
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

        /// <summary>Bật true chỉ khi cần soi lỗi: hiện bảng chữ cũ (IMGUI) chồng lên HUD canvas. Mặc định tắt vì HUD canvas đã hiện cùng dữ liệu.</summary>
        [SerializeField] private bool drawLegacyHud;

        /// <summary>ID khu của nhiệm vụ này để HUD canvas tìm nguồn dữ liệu online.</summary>
        public string AreaId { get { return "Phong_5"; } }

        /// <summary>Nhận model rỗng; điền HUD từ trạng thái thật của host. Trả false nếu chưa có trạng thái.</summary>
        public bool TryFillOnlineHud(AreaHudModel m)
        {
            if (replica == null || !replica.HasState) return false;
            BreakerPublicState state = replica.Public;
            m.TeamSize = state.PlayerCount;
            float hold = HoldNow(state);
            float fraction = state.HoldSeconds > 0f ? Mathf.Clamp01(hold / state.HoldSeconds) : (state.Matching ? 1f : 0f);
            // T-056: đề dễ — công tắc N bật đèn N; chữ hướng dẫn dùng chung với bản một máy (BreakerGuide).
            m.Notebook = BreakerGuide.Describe(state.PlayerCount, state.OnSwitches, state.TargetLamps, state.OverloadLimit, mySlot, fraction, state.Solved);
            m.Progress = "Đang bật " + state.OnCount + "/" + state.OverloadLimit + " • Giữ khớp " + (fraction * 100f).ToString("0") + "%";
            if (Time.time < statusUntil && status.Length > 0) m.Objective = status;
            else if (state.Solved) m.Objective = "Đã ổn định! Bảng cầu dao đã sửa xong.";
            else m.Objective = TodoLine(state);
            m.Prompt = targetSwitch >= 0 ? "[E] Bật/tắt công tắc " + (targetSwitch + 1) : "Nhìn vào công tắc của vai bạn rồi bấm E";
            m.Controls = "WASD đi • E bấm công tắc • R làm lại • N sổ tay • Tab khóa/thả chuột";
            return true;
        }

        // Một dòng việc cần làm ngay cho HUD (luôn hiện, không cần mở sổ tay): công tắc nào cần BẬT/TẮT và của vai nào.
        private string TodoLine(BreakerPublicState state)
        {
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < state.Size; s++)
            {
                bool on = state.SwitchOn(s);
                bool want = state.TargetOn(s);
                if (on == want) continue;
                if (sb.Length > 0) sb.Append(" • ");
                int owner = s % state.PlayerCount;
                sb.Append(want ? "BẬT " : "TẮT ").Append("công tắc ").Append(s + 1).Append(owner == mySlot ? " (của bạn)" : " (nhờ VAI " + (owner + 1) + ")");
            }
            if (sb.Length == 0) return "ĐÃ KHỚP! Giữ yên, đừng bấm gì thêm.";
            return "Việc cần làm: " + sb + ".  Đèn có chấm XANH phải sáng; bật quá " + state.OverloadLimit + " công tắc cùng lúc cầu dao sẽ nhảy.";
        }

        private void OnGUI()
        {
            if (replica == null || !replica.HasState) return;
            BreakerPublicState state = replica.Public;
            float hold = HoldNow(state);
            float fraction = state.HoldSeconds > 0f ? Mathf.Clamp01(hold / state.HoldSeconds) : (state.Matching ? 1f : 0f);
            string rules = BreakerGuide.Rules(state.OverloadLimit, state.HoldSeconds);
            string describe = BreakerGuide.Describe(state.PlayerCount, state.OnSwitches, state.TargetLamps, state.OverloadLimit, mySlot, fraction, state.Solved);
            string controls = (targetSwitch >= 0 ? "[E] bật/tắt công tắc " + (targetSwitch + 1) + "   " : "Nhìn vào công tắc của vai bạn   ") + "[R] làm lại   [Tab] khóa/thả chuột";
            float vw, vh;
            ModernGui.Begin(out vw, out vh);
            int describeLines = 2;
            for (int i = 0; i < describe.Length; i++) if (describe[i] == '\n') describeLines++;
            float height = 150f + 24f * 7f + 28f * describeLines + 50f;
            GUILayout.BeginArea(new Rect(16f, 16f, 660f, height), GUI.skin.box);
            GUILayout.Label("BẢNG CẦU DAO  (đội " + state.PlayerCount + " người)", ModernGui.Title);
            GUILayout.Label(rules, ModernGui.Dim);
            GUILayout.Space(8f);
            GUILayout.Label(describe);
            GUILayout.Space(6f);
            GUILayout.Label(controls, ModernGui.Dim);
            if (Time.time < statusUntil && status.Length > 0) GUILayout.Label(status);
            GUILayout.EndArea();
            ModernGui.End();
        }
    }
}
