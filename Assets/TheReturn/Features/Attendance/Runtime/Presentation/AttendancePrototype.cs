/*
 * Mục đích: Điều phối câu điểm danh giữa dữ liệu đề, đội người chơi, ghế và cửa.
 * Danh sách hàm:
 * - Awake / ValidateScene: khởi tạo và phát hiện tham chiếu scene thiếu.
 * - SetParticipantCount / ApplyParticipantRoster: nhận đội 2–4 người; đổi đội thì sinh lại đề.
 * - StartNewRound / SameAnswer: tạo đề mới không lặp đáp án ngay trước.
 * - RetryRound / ResetPresentation: thử lại cùng đề và reset phần nhìn.
 * - BeginSession / SwitchRole: bắt đầu và đổi vai trong chế độ thử một máy.
 * - Update / ReadCommands / UpdateTarget: cập nhật input, mục tiêu tương tác và tiến trình.
 * - TrySitActive / StandActive: thực hiện tương tác có kiểm tra khoảng cách.
 * - TickPuzzle / RefreshVisuals: kiểm tra đủ người, giữ kết quả và cập nhật scene.
 * - OnDisable / OnDestroy: trả con trỏ và cấu hình chạy nền.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    public sealed class AttendancePrototype : MonoBehaviour
    {
        [Header("Dữ liệu và tham chiếu")]
        public AttendanceTextCatalog textCatalog;
        public PrototypePartyController party;
        public AttendanceSeat[] seats;
        public PuzzleDoor exitDoor;
        public BoxCollider exitZone;
        public TextMesh board;
        [Header("Mô phỏng đội; bộ kết nối mạng sau này gọi ApplyParticipantRoster")]
        [Range(2, 4)] public int participantCount = 4;
        [Tooltip("0 tạo seed ngẫu nhiên khi mở Play; số khác cho phép lặp lại phiên kiểm thử.")]
        public int initialSeed;
        public bool continueIntoMap;
        public bool ExternalControl { get; set; }
        public float settleSeconds = 2f;
        public AttendanceState State { get; private set; }
        public int ActivePlayer => party.ActivePlayer;
        public int PartySize => roster.Count;
        public bool Begun { get; private set; }
        public bool Completed { get; private set; }
        public string Status { get; private set; }
        public string[] CluePackets { get; private set; }
        public int TargetSeat { get; private set; } = -1;
        readonly List<string> roster = new List<string>();
        System.Random seeds;
        float settleUntil = -1;
        float errorUntil;
        int observedRevision = -1;
        int evaluatedRevision = -1;
        int visibleRevision = -1;
        bool showingError;
        int hintIndex;
        bool previousBackground;

        /// <summary>Không nhận tham số; kiểm tra scene, khởi tạo bộ điều khiển và đội mặc định.</summary>
        void Awake()
        {
            if (!ValidateScene()) { enabled = false; return; }
            previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            party.Initialize();
            seeds = new System.Random(initialSeed == 0 ? Environment.TickCount : initialSeed);
            SetParticipantCount(participantCount);
            PrototypePartyController.SetCursor(false);
        }

        /// <summary>Không nhận tham số; trả false và báo lỗi cụ thể nếu scene thiếu dữ liệu hoặc 24 ghế.</summary>
        bool ValidateScene()
        {
            string error = "";
            bool valid = textCatalog != null && party != null && exitDoor != null && board != null && exitZone != null;
            valid = valid && seats != null && seats.Length == AttendanceRound.SeatCount;
            if (valid)
            {
                for (int i = 0; i < seats.Length; i++)
                    if (seats[i] == null || seats[i].index != i || seats[i].sitPoint == null ||
                        seats[i].standPoint == null || seats[i].indicator == null || seats[i].label == null) valid = false;
                valid = valid && textCatalog.ValidateCatalog(out error);
            }
            if (!valid) Debug.LogError("Attendance scene is not configured. " + error, this);
            return valid;
        }

        /// <summary>Nhận số người 2–4 trong bản thử; chuyển thành roster ổn định, không tạo người chơi mạng.</summary>
        public void SetParticipantCount(int count)
        {
            if (count < 2 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));
            participantCount = count;
            string[] ids = new string[count];
            for (int i = 0; i < count; i++) ids[i] = "local-" + i;
            ApplyParticipantRoster(ids);
        }

        /// <summary>Nhận ID kết nối theo slot (0–4, không trùng); đội đổi thì hủy lượt cũ và chia đề mới.</summary>
        public void ApplyParticipantRoster(IReadOnlyList<string> participantIds)
        {
            if (participantIds == null || participantIds.Count > 4) throw new ArgumentException("Roster supports 0–4 IDs.");
            var unique = new HashSet<string>();
            for (int i = 0; i < participantIds.Count; i++)
                if (string.IsNullOrWhiteSpace(participantIds[i]) || !unique.Add(participantIds[i]))
                    throw new ArgumentException("Roster IDs must be nonempty and unique.");
            bool unchanged = roster.Count == participantIds.Count;
            for (int i = 0; unchanged && i < roster.Count; i++) unchanged = roster[i] == participantIds[i];
            if (unchanged) return;
            roster.Clear();
            for (int i = 0; i < participantIds.Count; i++) roster.Add(participantIds[i]);
            party.SetPartySize(roster.Count);
            party.ResetPositions();
            if (roster.Count < 2)
            {
                State = null;
                CluePackets = Array.Empty<string>();
                Status = textCatalog.waiting;
                Completed = false;
                board.text = "CHO THEM NGUOI\nCAN IT NHAT 2 NGUOI";
                exitDoor.SetOpen(false, true);
                foreach (AttendanceSeat seat in seats) seat.Show(null, false, false);
                if (roster.Count > 0) party.SwitchRole(0, false);
                PrototypePartyController.SetCursor(false);
                return;
            }
            StartNewRound();
        }

        /// <summary>Nhận seed tùy chọn; sinh đề theo roster, đổi cả đáp án và text, không lặp ngay đáp án cũ.</summary>
        public void StartNewRound(int? seedOverride = null)
        {
            if (roster.Count < 2) return;
            string[] names = new string[roster.Count];
            Array.Copy(textCatalog.roleNames, names, names.Length);
            AttendanceRound round;
            int seed = seedOverride ?? seeds.Next();
            int[] previous = State == null ? null : State.Round.CopySolution();
            do
            {
                round = AttendanceGenerator.Generate(names, seed);
                seed = unchecked(seed + 1);
            }
            while (!seedOverride.HasValue && SameAnswer(previous, round.CopySolution()));
            State = new AttendanceState(round);
            CluePackets = textCatalog.BuildPackets(round);
            ResetPresentation();
            Status = textCatalog.ready;
        }

        /// <summary>Nhận hai đáp án có thể null; trả true khi bằng nhau theo từng slot.</summary>
        static bool SameAnswer(int[] a, int[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        /// <summary>Không nhận tham số; xóa chỗ ngồi để thử lại, giữ seed, đáp án và manh mối.</summary>
        public void RetryRound()
        {
            if (State == null) return;
            State.Reset();
            ResetPresentation();
            Status = "Đã đặt lại chỗ ngồi; manh mối và đáp án giữ nguyên.";
        }

        /// <summary>Không nhận tham số; đưa nhân vật về spawn, đóng cửa, xóa timer và cập nhật cảnh.</summary>
        void ResetPresentation()
        {
            party.ResetPositions();
            Completed = false;
            settleUntil = -1;
            errorUntil = 0;
            observedRevision = -1;
            evaluatedRevision = -1;
            visibleRevision = -1;
            hintIndex = 0;
            exitDoor.SetOpen(false, true);
            SwitchRole(0);
            RefreshVisuals();
        }

        /// <summary>Không nhận tham số; cho phép chơi nếu có đề hợp lệ, khóa chuột vào Game View.</summary>
        public void BeginSession()
        {
            if (State == null) return;
            Begun = true;
            PrototypePartyController.SetCursor(true);
        }

        /// <summary>Nhận ID vai; chuyển camera nếu vai đang trong đội, không ảnh hưởng chỗ ngồi.</summary>
        public void SwitchRole(int index)
        {
            if (State == null || index < 0 || index >= State.PlayerCount) return;
            party.SwitchRole(index, State.SeatOf(index) >= 0);
            TargetSeat = -1;
        }

        /// <summary>Không nhận tham số; điều phối input, kiểm tra đề và vùng kết thúc theo từng frame.</summary>
        void Update()
        {
            if (ExternalControl || !Begun || State == null) return;
            ReadCommands();
            if (!Completed)
            {
                party.TickInput(State.SeatOf(ActivePlayer) >= 0);
                UpdateTarget();
                var keyboard = Keyboard.current;
                if (Cursor.lockState == CursorLockMode.Locked && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                {
                    if (State.SeatOf(ActivePlayer) >= 0) StandActive();
                    else if (TargetSeat >= 0) TrySitActive(TargetSeat);
                }
            }
            TickPuzzle();
            if (!continueIntoMap && State.Solved && !Completed && exitZone.bounds.Contains(party.players[ActivePlayer].position))
            {
                Completed = true;
                Status = "Đã rời lớp. Có thể thử đề mới với số người khác.";
                PrototypePartyController.SetCursor(false);
            }
            RefreshVisuals();
        }

        /// <summary>Không nhận tham số; đọc phím đổi vai, gợi ý, đề mới và thử lại; không xử lý di chuyển.</summary>
        void ReadCommands()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
            if (keyboard.f5Key.wasPressedThisFrame) StartNewRound();
            if (keyboard.rKey.wasPressedThisFrame) RetryRound();
            if (keyboard.hKey.wasPressedThisFrame && textCatalog.hints != null && textCatalog.hints.Length > 0)
                Status = textCatalog.hints[Mathf.Min(hintIndex++, textCatalog.hints.Length - 1)];
            if (keyboard.digit1Key.wasPressedThisFrame) SwitchRole(0);
            if (keyboard.digit2Key.wasPressedThisFrame) SwitchRole(1);
            if (keyboard.digit3Key.wasPressedThisFrame) SwitchRole(2);
            if (keyboard.digit4Key.wasPressedThisFrame) SwitchRole(3);
        }

        /// <summary>Không nhận tham số; raycast từ camera, lưu ID ghế trong tầm 3,2 m hoặc -1.</summary>
        void UpdateTarget()
        {
            TargetSeat = -1;
            RaycastHit hit;
            if (party.TryGetInteractionHit(3.2f, out hit))
            {
                AttendanceSeat seat = hit.collider.GetComponentInParent<AttendanceSeat>();
                if (seat != null) TargetSeat = seat.index;
            }
        }

        /// <summary>Nhận ID ghế 0–23; kiểm tra khoảng cách và quyền chiếm ghế, trả true nếu đã ngồi.</summary>
        public bool TrySitActive(int index)
        {
            if (State == null || index < 0 || index >= seats.Length) return false;
            if (Vector3.Distance(party.players[ActivePlayer].position, seats[index].sitPoint.position) > 3.5f)
            { Status = "Hãy đến gần ghế hơn."; return false; }
            if (!State.TrySit(ActivePlayer, index))
            { Status = "Ghế đã có người, bạn đang ngồi, hoặc lượt đã kết thúc."; return false; }
            party.Teleport(ActivePlayer, seats[index].sitPoint.position);
            SwitchRole(ActivePlayer);
            Status = State.Round.Names[ActivePlayer] + " đã ngồi " + AttendanceRound.SeatCode(index) + ".";
            return true;
        }

        /// <summary>Không nhận tham số; nhả ghế của vai hiện tại và đưa về mốc đứng; trả false nếu chưa ngồi.</summary>
        public bool StandActive()
        {
            if (State == null) return false;
            int seat = State.SeatOf(ActivePlayer);
            if (!State.Stand(ActivePlayer)) return false;
            party.Teleport(ActivePlayer, seats[seat].standPoint.position);
            SwitchRole(ActivePlayer);
            Status = State.Solved ? textCatalog.success : "Đã đứng dậy; có thể chọn chỗ khác.";
            return true;
        }

        /// <summary>Không nhận tham số; đủ đúng số người mới bắt đầu timer, kiểm tra một lần mỗi Revision.</summary>
        public void TickPuzzle()
        {
            if (State == null) return;
            if (observedRevision != State.Revision)
            {
                observedRevision = State.Revision;
                settleUntil = State.OccupiedCount == State.PlayerCount ? Time.time + settleSeconds : -1;
            }
            if (State.Solved || settleUntil < 0 || Time.time < settleUntil || evaluatedRevision == State.Revision) return;
            evaluatedRevision = State.Revision;
            if (State.Evaluate())
            {
                Status = textCatalog.success;
                exitDoor.SetOpen(true);
                visibleRevision = -1;
            }
            else
            {
                Status = textCatalog.wrong;
                errorUntil = Time.time + 3;
            }
        }

        /// <summary>Không nhận tham số; cập nhật nhãn/màu chỉ khi trạng thái hoặc pha báo sai đổi.</summary>
        void RefreshVisuals()
        {
            if (State == null) return;
            bool error = Time.time < errorUntil;
            if (visibleRevision == State.Revision && error == showingError) return;
            visibleRevision = State.Revision;
            showingError = error;
            foreach (AttendanceSeat seat in seats)
            {
                int occupant = State.Occupant(seat.index);
                seat.Show(occupant < 0 ? null : State.Round.Names[occupant], State.Solved, error);
            }
            board.text = "LOP 101  /  24 GHE\nH1 GAN BANG  /  C1 BEN TRAI\n" +
                (State.Solved ? "DIEM DANH HOP LE" : "DOI " + State.PlayerCount + " NGUOI");
        }

        /// <summary>Khi component bị tắt, thả chuột; không thay đổi dữ liệu đề.</summary>
        void OnDisable()
        {
            PrototypePartyController.SetCursor(false);
        }

        /// <summary>Khi hủy component, phục hồi tùy chọn chạy nền trước khi vào scene.</summary>
        void OnDestroy()
        {
            Application.runInBackground = previousBackground;
        }
    }
}
