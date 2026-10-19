/*
 * Mục đích: Nối luật Hợp xướng (ChoirState) với người chơi, bục, bản nhạc trên bảng và HUD thẻ nhạc (T-006d2).
 * Thời gian giữa hai nốt lấy từ ChoirSettings và được áp lại mỗi frame (chỉnh được khi đang chơi thử).
 * Đổi vai bằng phím 1-4 giữa dãy sẽ gọi ChoirState.ExtendForSwitch để bù giờ khi thử một máy.
 * Hàm: Begin bắt đầu; Retry đặt lại dãy; Update xử lý input/thời gian; TryPress bấm bục;
 * LateUpdate xoay bảng nhạc về phía camera; OnGUI vẽ thẻ nhạc và đồng hồ; StopSession ngừng nhiệm vụ.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class ChoirPrototype : MonoBehaviour
    {
        static readonly string[] SymbolNames = { "A", "B", "C", "D" };
        public PrototypePartyController party;
        public ChoirSettings settings;
        public ChoirPad[] pads;
        [Tooltip("Sáu chữ trên bảng nhạc, theo thứ tự nốt.")]
        public TextMesh[] scoreNotes;
        [Tooltip("Gốc bảng nhạc; được xoay về phía camera.")]
        public Transform scoreBoard;
        public TextMesh statusBoard;
        [Tooltip("Vẽ HUD OnGUI dự phòng; Canvas (SchoolFloorCanvasPresenter) tự tắt vì đã hiện sổ tay trong UI.")]
        public bool drawOnGui = true;
        public ChoirState State { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }
        public int TargetPad { get; private set; } = -1;
        ChoirRound round;

        /// <summary>Không nhận tham số; tạo đề mới theo số người hiện có (nếu chưa có hoặc đổi đội) và bắt đầu.</summary>
        public void Begin()
        {
            int players = Mathf.Clamp(party.PartySize, 2, 4);
            if (round == null || round.PlayerCount != players)
            {
                int seed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
                round = ChoirGenerator.Generate(players, seed);
                State = null;
            }
            if (State == null) State = new ChoirState(round, settings != null ? settings.ToRules() : ChoirRules.Default);
            else State.Reset();
            Active = true;
            Status = settings != null ? settings.instructions : "";
            RefreshBoard();
        }

        /// <summary>Không nhận tham số; đặt lại dãy về nốt đầu, giữ đề.</summary>
        public void Retry()
        {
            if (State == null) return;
            State.Reset();
            Status = "Đã đặt lại bản nhạc.";
            RefreshBoard();
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, bấm bục (E) và hết giờ giữa hai nốt.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            double now = Time.timeAsDouble;
            if (settings != null) State.ApplyRules(settings.ToRules());
            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                int before = party.ActivePlayer;
                if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
                if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
                if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
                if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
                if (party.ActivePlayer != before) State.ExtendForSwitch(now);
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Retry();
            TargetPad = -1;
            RaycastHit hit;
            if (party.TryGetInteractionHit(3f, out hit))
            {
                var pad = hit.collider.GetComponentInParent<ChoirPad>();
                if (pad != null) TargetPad = pad.index;
            }
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && TargetPad >= 0) TryPress(TargetPad, now);
            if (State.Tick(now)) { Status = settings != null ? settings.tooLate : "Quá chậm."; Flash(-1, Color.red); }
            party.TickInput(false);
            RefreshBoard();
        }

        /// <summary>Nhận bục và thời gian giây; bấm bục, hiển thị kết quả, trả kết quả của Domain.</summary>
        public ChoirResult TryPress(int pad, double now)
        {
            if (!Active || State == null) return ChoirResult.InvalidPad;
            var result = State.Press(pad, now);
            switch (result)
            {
                case ChoirResult.Correct: Flash(pad, Color.green); Status = "Đúng - nốt " + State.Progress + "/" + round.Length; break;
                case ChoirResult.Completed: Flash(pad, Color.green); Status = settings != null ? settings.success : "Xong"; break;
                case ChoirResult.Wrong: Flash(pad, Color.red); Status = settings != null ? settings.wrong : "Sai bục."; break;
                case ChoirResult.TooLate: Flash(pad, Color.red); Status = settings != null ? settings.tooLate : "Quá chậm."; break;
            }
            RefreshBoard();
            return result;
        }

        /// <summary>Nhận bục (-1 = tất cả) và màu; làm bục nháy phản hồi.</summary>
        void Flash(int pad, Color color)
        {
            for (int i = 0; i < pads.Length; i++)
                if ((pad < 0 || i == pad) && pads[i] != null) pads[i].Flash(color, .5f);
        }

        /// <summary>Không nhận tham số; cập nhật chữ trên bảng nhạc, đánh dấu nốt đã chơi.</summary>
        void RefreshBoard()
        {
            if (State == null) return;
            for (int i = 0; scoreNotes != null && i < scoreNotes.Length && i < round.Length; i++)
            {
                scoreNotes[i].text = SymbolNames[round.Symbol(i)];
                scoreNotes[i].color = i < State.Progress ? new Color(.2f, 1f, .4f) : (i == State.Progress ? Color.yellow : Color.white);
            }
            if (statusBoard != null) statusBoard.text = State.Solved ? "XONG" : "Nốt " + (State.Progress + 1) + "/" + round.Length;
        }

        /// <summary>Không nhận tham số; xoay bảng nhạc quay mặt về camera người chơi.</summary>
        void LateUpdate()
        {
            if (!Active || scoreBoard == null || party == null || party.viewCamera == null) return;
            var look = party.viewCamera.transform.position - scoreBoard.position;
            look.y = 0;
            if (look.sqrMagnitude > .01f) scoreBoard.rotation = Quaternion.LookRotation(-look);
        }

        /// <summary>Không nhận tham số; vẽ thẻ nhạc của vai đang điều khiển, đồng hồ và trạng thái.</summary>
/// <summary>Không nhận tham số; vẽ HUD dự phòng khi drawOnGui bật (Canvas tắt nó và dùng Notebook).</summary>
        void OnGUI()
        {
            if (!drawOnGui || !Active || State == null) return;
            int player = party.ActivePlayer;
            GUI.Box(new Rect(16, 16, 440, 150), "HỢP XƯỚNG - Vai " + (player + 1) + "\n" + Notebook(player) + "\n" + Status + "\n[E] bấm bục  [R] làm lại  [1-4] đổi vai");
        }

        /// <summary>Nhận ID vai; trả thẻ nhạc của vai đó (ký hiệu = bục) và đồng hồ còn lại giữa hai nốt. Rỗng nếu chưa bắt đầu.</summary>
        public string Notebook(int player)
        {
            if (State == null) return "";
            var sb = new System.Text.StringBuilder();
            for (int s = 0; s < ChoirRound.PadCount; s++)
            {
                int pad;
                if (State.TryReadCard(player, s, out pad)) sb.Append("Thẻ nhạc: ").Append(SymbolNames[s]).Append(" = bục ").Append(pad + 1).Append('\n');
            }
            double left = State.RemainingSeconds(Time.timeAsDouble);
            if (left >= 0) sb.Append("Còn ").Append(left.ToString("0.0")).Append(" giây");
            else if (!State.Rules.HasTimeLimit) sb.Append("Không giới hạn thời gian");
            else sb.Append("Hạn giữa hai nốt: ").Append(State.Rules.StepWindowSeconds.ToString("0.#")).Append(" giây");
            return sb.ToString();
        }

        /// <summary>Nhận cờ reset; ngừng input; reset=false giữ kết quả đã giải khi chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            if (reset) { State = null; round = null; } // chơi lại cả map: lần Begin sau sinh đề mới
        }
    }
}
