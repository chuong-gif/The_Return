/*
 * Mục đích: Nối luật «Cô nói» (TeacherSaysState) với đội, các vùng đoạn trong hành lang, bảng loa và HUD sổ tay (T-006c).
 * Đo «có ai đang di chuyển» bằng vận tốc ngang từng thành viên; «cả đội trong vùng đoạn kế» bằng hộp cục bộ của vùng (scene xoay).
 * Đổi vai bằng phím 1-4 giữa đoạn BƯỚC gọi ExtendForSwitch (bù giờ khi thử một máy). Thông số lấy từ TeacherSaysSettings, áp lại mỗi frame.
 * Hàm: Begin bắt đầu; Retry làm lại từ đầu; Update xử lý input/thời gian/sự kiện; AllInZone; MeasureMoving; SendToStart đưa đội về đầu;
 * RefreshBoard cập nhật bảng loa và vùng; Notebook sổ tay từng vai; LateUpdate xoay bảng về camera; OnGUI HUD dự phòng; StopSession ngừng.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class TeacherSaysPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public TeacherSaysSettings settings;
        [Tooltip("5 vùng đoạn theo thứ tự từ đầu đến cuối hành lang (BoxCollider trigger).")]
        public BoxCollider[] segmentZones;
        [Tooltip("5 tấm sàn hiển thị vùng đoạn (cùng thứ tự).")]
        public Renderer[] zoneMarkers;
        [Tooltip("4 điểm đầu hành lang để đưa đội về khi đủ cảnh cáo.")]
        public Transform[] startPoints;
        public Transform speakerBoard;
        public TextMesh speakerText, progressText;
        [Tooltip("Vẽ HUD OnGUI dự phòng; Canvas (SchoolFloorCanvasPresenter) tự tắt vì đã hiện sổ tay trong UI.")]
        public bool drawOnGui = true;
        public TeacherSaysState State { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }
        TeacherSaysRound round;
        Vector3[] lastPositions = new Vector3[4];
        MaterialPropertyBlock props;

        /// <summary>Không nhận tham số; tạo đề theo số người hiện có (nếu chưa có hoặc đổi đội) và bắt đầu từ đoạn 0.</summary>
        public void Begin()
        {
            int players = Mathf.Clamp(party.PartySize, 2, 4);
            if (round == null || round.PlayerCount != players)
            {
                int seed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
                round = TeacherSaysGenerator.Generate(players, seed);
                State = null;
            }
            if (State == null) State = new TeacherSaysState(round, settings != null ? settings.ToRules() : TeacherSaysRules.Default);
            else State.Reset();
            State.Begin(Time.timeAsDouble);
            Active = true;
            Status = settings != null ? settings.instructions : "";
            CaptureLastPositions();
            RefreshBoard();
        }

        /// <summary>Không nhận tham số; làm lại từ đầu hành lang (giữ đề), đưa đội về điểm đầu.</summary>
        public void Retry()
        {
            if (State == null) return;
            State.Reset();
            State.Begin(Time.timeAsDouble);
            SendToStart();
            Status = "Đã đặt lại hành lang.";
            RefreshBoard();
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, đo chuyển động, chạy luật và hiển thị sự kiện.</summary>
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
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) { Retry(); return; }
            party.TickInput(false);
            bool moving = MeasureMoving(Time.deltaTime);
            bool inNext = State.Segment < TeacherSaysRound.SegmentCount && AllInZone(State.Segment);
            var result = State.Tick(now, moving, inNext);
            switch (result)
            {
                case TeacherSaysEvent.Advanced: Status = settings != null ? settings.advanced : "Đúng!"; break;
                case TeacherSaysEvent.StandOk: Status = settings != null ? settings.standOk : "Đi tiếp."; break;
                case TeacherSaysEvent.Warned: Status = settings != null ? settings.warned : "Sai!"; break;
                case TeacherSaysEvent.SentBack: Status = settings != null ? settings.sentBack : "Về đầu."; SendToStart(); break;
                case TeacherSaysEvent.Completed: Status = settings != null ? settings.success : "Xong"; break;
            }
            RefreshBoard();
        }

        /// <summary>Nhận chỉ số vùng; trả true nếu mọi thành viên đang hoạt động đứng trong hộp vùng đó (tính theo toạ độ cục bộ của vùng).</summary>
        bool AllInZone(int index)
        {
            if (segmentZones == null || index < 0 || index >= segmentZones.Length || segmentZones[index] == null) return false;
            var zone = segmentZones[index];
            var box = new Bounds(zone.center, zone.size);
            for (int i = 0; i < party.PartySize; i++)
            {
                if (!party.players[i].gameObject.activeInHierarchy) return false;
                if (!box.Contains(zone.transform.InverseTransformPoint(party.players[i].position))) return false;
            }
            return true;
        }

        /// <summary>Nhận delta giây; trả true nếu có thành viên nào di chuyển ngang nhanh hơn ngưỡng trong settings (cập nhật vị trí cũ).</summary>
        bool MeasureMoving(float dt)
        {
            bool moving = false;
            float limit = settings != null ? settings.movingSpeed : 0.4f;
            for (int i = 0; i < party.PartySize && i < lastPositions.Length; i++)
            {
                Vector3 pos = party.players[i].position;
                Vector3 d = pos - lastPositions[i];
                d.y = 0;
                if (dt > 0.0001f && d.magnitude / dt > limit) moving = true;
                lastPositions[i] = pos;
            }
            return moving;
        }

        /// <summary>Không nhận tham số; ghi vị trí hiện tại của đội làm mốc đo chuyển động (sau khi dịch chuyển/bắt đầu).</summary>
        void CaptureLastPositions()
        {
            for (int i = 0; i < party.PartySize && i < lastPositions.Length; i++) lastPositions[i] = party.players[i].position;
        }

        /// <summary>Không nhận tham số; đưa từng thành viên về điểm đầu hành lang, chọn vai 1 (nếu có điểm đầu).</summary>
        void SendToStart()
        {
            if (startPoints != null)
                for (int i = 0; i < party.PartySize && i < startPoints.Length; i++)
                    if (startPoints[i] != null) party.Teleport(i, startPoints[i]);
            party.SwitchRole(0, false);
            CaptureLastPositions();
        }

        /// <summary>Nhận lệnh; trả chữ hiển thị tiếng Việt.</summary>
        static string CommandName(TeacherCommand command)
        {
            return command == TeacherCommand.Step ? "BƯỚC TỚI" : "ĐỨNG YÊN";
        }

        /// <summary>Không nhận tham số; cập nhật bảng loa, tiến độ và màu các vùng đoạn.</summary>
        void RefreshBoard()
        {
            if (State == null) return;
            int seg = State.Segment;
            if (speakerText != null) speakerText.text = State.Solved ? "XONG" : CommandName(round.Command(seg)) + "!";
            if (progressText != null)
                progressText.text = State.Solved ? "" : "Đoạn " + (seg + 1) + "/" + TeacherSaysRound.SegmentCount + "  Cảnh cáo " + State.Warnings + "/" + State.Rules.MaxWarnings;
            if (zoneMarkers == null) return;
            if (props == null) props = new MaterialPropertyBlock();
            for (int i = 0; i < zoneMarkers.Length; i++)
            {
                if (zoneMarkers[i] == null) continue;
                props.SetColor("_BaseColor", i < seg ? new Color(.2f, .8f, .35f) : (i == seg ? new Color(.95f, .8f, .2f) : new Color(.3f, .3f, .35f)));
                zoneMarkers[i].SetPropertyBlock(props);
            }
        }

        /// <summary>Nhận ID vai; trả sổ tay của vai đó: lệnh loa, tiền tố «Cô nói» (chỉ người nghe), đồng hồ, cảnh cáo. Rỗng nếu chưa bắt đầu.</summary>
        public string Notebook(int player)
        {
            if (State == null || round == null) return "";
            if (State.Solved) return "Đã qua đủ " + TeacherSaysRound.SegmentCount + " đoạn.";
            var sb = new System.Text.StringBuilder();
            sb.Append("Loa: ").Append(CommandName(round.Command(State.Segment))).Append("   (đoạn ").Append(State.Segment + 1).Append('/').Append(TeacherSaysRound.SegmentCount).Append(")\n");
            bool said;
            if (State.TryReadPrefix(player, out said))
                sb.Append(said ? "Bạn nghe rõ: CÓ «Cô nói» → làm ĐÚNG lệnh loa.\n" : "Bạn nghe rõ: KHÔNG có «Cô nói» → làm NGƯỢC lệnh loa.\n");
            else sb.Append("Bạn KHÔNG nghe được tiền tố «Cô nói» - hỏi người nghe (vai ").Append(round.Listener(State.Segment) + 1).Append(").\n");
            double left = State.RemainingSeconds(Time.timeAsDouble);
            if (State.RequiredNow == TeacherCommand.Step)
                sb.Append(left >= 0 ? "Còn " + left.ToString("0.0") + " giây để cả đội vào vùng sáng" : "Không giới hạn thời gian: cả đội vào vùng sáng");
            else
                sb.Append(State.StandSatisfied ? "Đứng đủ giờ - đi tới vùng sáng" : "Cả đội đứng yên, còn " + left.ToString("0.0") + " giây");
            sb.Append("\nCảnh cáo ").Append(State.Warnings).Append('/').Append(State.Rules.MaxWarnings);
            return sb.ToString();
        }

        /// <summary>Không nhận tham số; xoay bảng loa quay mặt về camera người chơi.</summary>
/// <summary>Không nhận tham số; đưa bảng loa lên trên vùng đoạn đang nhắm tới và xoay quay mặt về camera.</summary>
        void LateUpdate()
        {
            if (!Active || speakerBoard == null || party == null || party.viewCamera == null) return;
            if (State != null && !State.Solved && segmentZones != null && State.Segment < segmentZones.Length && segmentZones[State.Segment] != null)
                speakerBoard.position = segmentZones[State.Segment].transform.position + Vector3.up * 4f;
            var look = party.viewCamera.transform.position - speakerBoard.position;
            look.y = 0;
            if (look.sqrMagnitude > .01f) speakerBoard.rotation = Quaternion.LookRotation(-look);
        }

        /// <summary>Không nhận tham số; vẽ HUD dự phòng khi drawOnGui bật (Canvas tắt nó và dùng Notebook).</summary>
        void OnGUI()
        {
            if (!drawOnGui || !Active || State == null) return;
            GUI.Box(new Rect(16, 16, 520, 150), "CÔ NÓI - Vai " + (party.ActivePlayer + 1) + "\n" + Notebook(party.ActivePlayer) + "\n" + Status + "\n[R] làm lại  [1-4] đổi vai");
        }

        /// <summary>Nhận cờ reset; ngừng input; reset=true bỏ đề để lần sau sinh đề mới, false giữ kết quả đã giải.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            if (reset) { State = null; round = null; }
        }
    }
}
