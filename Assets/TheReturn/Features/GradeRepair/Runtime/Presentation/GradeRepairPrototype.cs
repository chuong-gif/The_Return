/*
 * Mục đích: Điều khiển nhiệm vụ bảng điểm, xác thực tương tác và cập nhật cửa.
 * Hàm: Begin / NewCase tạo lượt; Retry thử lại cùng hồ sơ; StopSession trả điều khiển;
 * Update xử lý đi/đổi vai/tương tác; CanReach kiểm tra tầm và vật cản;
 * TryOpen mở tài liệu hoặc bảng; ClosePanel đóng giao diện; Apply / Undo / Submit gửi lệnh;
 * CanEdit kiểm tra người đang dùng bảng; ShowResult cập nhật phản hồi và cửa; RefreshLabels cập nhật chữ 3D.
 */
using UnityEngine;
using UnityEngine.InputSystem;
namespace TheReturn
{
    [DefaultExecutionOrder(60)]
    public sealed class GradeRepairPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public GradeRepairCatalog catalog;
        public GradeRepairTerminal board;
        public GradeRepairTerminal[] documents;
        public PuzzleDoor exitDoor;
        public Transform[] checkpoints;
        public int initialSeed;
        public GradeRepairState State { get; private set; }
        public bool Active { get; private set; }
        public bool PanelOpen { get; private set; }
        public int OpenDocument { get; private set; } = -1;
        public string Status { get; private set; }
        public GradeRepairTerminal Target { get; private set; }
        int panelOwner;
        System.Random seeds;

        /// <summary>Không nhận tham số; bắt đầu với đội hiện tại 2–4 người, không đổi vị trí người vừa qua hành lang.</summary>
        public void Begin()
        {
            seeds = new System.Random();
            Active = true;
            State = new GradeRepairState(new GradeRepairRound(party.PartySize, initialSeed == 0 ? seeds.Next() : initialSeed));
            Status = catalog.introduction;
            PanelOpen = false;
            RefreshLabels();
        }

        /// <summary>Nhận seed tùy chọn; tạo hồ sơ mới và đưa đội về phòng, giữ các nhiệm vụ trước.</summary>
        public void NewCase(int? seed = null)
        {
            if (!Active) return;
            State = new GradeRepairState(new GradeRepairRound(party.PartySize, seed ?? seeds.Next()));
            Retry();
            RefreshLabels();
        }

        /// <summary>Không nhận tham số; đặt lại điểm và phiếu, giữ tài liệu đã đọc, đưa đội về checkpoint trước khi đóng cửa.</summary>
        public void Retry()
        {
            if (!Active || State == null) return;
            ClosePanel();
            for (int i = 0; i < party.PartySize; i++) party.Teleport(i, checkpoints[i].position);
            party.SwitchRole(party.ActivePlayer, false);
            State.Reset();
            exitDoor.SetOpen(false, true);
            Status = "Đã đặt lại bảng điểm; giữ hồ sơ đã đọc và hai nhiệm vụ trước.";
            RefreshLabels();
        }

        /// <summary>Nhận cờ reset; tắt input và bảng, reset=false giữ điểm và cửa khi chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            PanelOpen = false;
            if (reset) { State = null; exitDoor.SetOpen(false, true); }
        }

        /// <summary>Không nhận tham số; đọc phím, cập nhật tầm tương tác, chỉ di chuyển khi đóng giao diện.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                for (int i = 0; i < party.PartySize; i++)
                {
                    if (!keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) continue;
                    if (PanelOpen) ClosePanel();
                    party.SwitchRole(i, false);
                }
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                {
                    if (PanelOpen) ClosePanel();
                    else PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                }
                if (keyboard.eKey.wasPressedThisFrame && PanelOpen) { ClosePanel(); return; }
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    if (keyboard.rKey.wasPressedThisFrame) Retry();
                    if (keyboard.f5Key.wasPressedThisFrame) NewCase();
                }
            }
            if (PanelOpen)
            {
                GradeRepairTerminal terminal = OpenDocument < 0 ? board : documents[OpenDocument];
                if (panelOwner != party.ActivePlayer || !CanReach(terminal)) ClosePanel();
                return;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            party.TickInput(false);
            Target = null;
            RaycastHit hit;
            if (party.TryGetInteractionHit(3f, out hit)) Target = hit.collider.GetComponentInParent<GradeRepairTerminal>();
            if (Target != null && keyboard != null && keyboard.eKey.wasPressedThisFrame) TryOpen(Target);
        }

        /// <summary>Nhận terminal; trả true khi vai hiện tại còn hoạt động, trong tầm và không có tường chắn.</summary>
        public bool CanReach(GradeRepairTerminal terminal)
        {
            if (terminal == null || party.PartySize < 2 || !party.players[party.ActivePlayer].gameObject.activeInHierarchy) return false;
            Vector3 eye = party.players[party.ActivePlayer].position + Vector3.up * 1.5f;
            Vector3 delta = terminal.transform.position - eye;
            if (delta.magnitude > 3.2f) return false;
            foreach (RaycastHit hit in Physics.RaycastAll(eye, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(party.players[party.ActivePlayer]) && !hit.transform.IsChildOf(terminal.transform)) return false;
            return true;
        }

        /// <summary>Nhận terminal thuộc nhiệm vụ; mở bảng hoặc đúng tài liệu của vai, trả false nếu sai quyền/tầm.</summary>
        public bool TryOpen(GradeRepairTerminal terminal)
        {
            if (!Active || State == null || !CanReach(terminal)) return false;
            int document = terminal.document;
            if (terminal != board)
            {
                if (document < 0 || document >= documents.Length || documents[document] != terminal) return false;
                if (!State.MarkRead(document, party.ActivePlayer))
                {
                    Status = "Tài liệu này dành cho vai " + (State.Round.OwnerOf(document) + 1) + ". Hãy nhờ đồng đội đọc và đối chiếu.";
                    return false;
                }
            }
            else document = -1;
            OpenDocument = document;
            panelOwner = party.ActivePlayer;
            PanelOpen = true;
            PrototypePartyController.SetCursor(false);
            return true;
        }

        /// <summary>Không nhận tham số; đóng bảng/tài liệu và trả chuột cho góc nhìn thứ nhất.</summary>
        public void ClosePanel()
        {
            PanelOpen = false;
            Target = null;
            PrototypePartyController.SetCursor(true);
        }

        /// <summary>Không nhận tham số; trả true khi đang đứng dùng đúng bảng, ngăn lệnh UI từ xa hoặc vai khác.</summary>
        bool CanEdit() => Active && State != null && PanelOpen && OpenDocument == -1 &&
            panelOwner == party.ActivePlayer && CanReach(board);

        /// <summary>Nhận phiếu, nguồn và đích; gửi lệnh sau xác thực bảng, trả mã kết quả.</summary>
        public GradeResult Apply(int voucher, int source, int target)
        {
            if (!CanEdit()) return GradeResult.Invalid;
            return ShowResult(State.Apply(voucher, source, target));
        }

        /// <summary>Không nhận tham số; hoàn tác nếu đang dùng bảng, trả mã kết quả.</summary>
        public GradeResult Undo() => CanEdit() ? ShowResult(State.Undo()) : GradeResult.Invalid;

        /// <summary>Không nhận tham số; nộp bảng nếu đang dùng bảng, trả mã kiểm tra.</summary>
        public GradeResult Submit() => CanEdit() ? ShowResult(State.Submit()) : GradeResult.Invalid;

        /// <summary>Nhận kết quả; cập nhật phản hồi và cửa, trả cùng mã cho người gọi.</summary>
        GradeResult ShowResult(GradeResult result)
        {
            Status = catalog.ResultText(result);
            if (State.Solved) exitDoor.SetOpen(true);
            RefreshLabels();
            return result;
        }

        /// <summary>Không nhận tham số; hiển thị điểm và người phụ trách tài liệu theo số người của lượt.</summary>
        void RefreshLabels()
        {
            string text = "BANG DIEM / E\n";
            for (int i = 0; i < 4; i++) text += (char)('A' + i) + ": " + State.Score(i) + "   ";
            board.SetLabel(text);
            for (int i = 0; i < documents.Length; i++)
                documents[i].SetLabel("HO SO " + (i + 1) + "\nVAI " + (State.Round.OwnerOf(i) + 1) + " / E");
        }
    }
}
