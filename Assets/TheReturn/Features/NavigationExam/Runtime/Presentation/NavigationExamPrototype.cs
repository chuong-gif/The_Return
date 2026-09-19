/*
 * Mục đích: Nối luật dẫn đường với sa bàn, người chơi, quân cờ và cửa.
 * Hàm: Begin tạo lượt; ResetRound thử lại/đổi biến thể; StopSession trả điều khiển;
 * CanReach / TryOpen xác thực tầm và vật cản; ClosePanel đóng sa bàn;
 * Step gửi bước đúng người/phiên bản; Update xử lý input; RefreshView cập nhật quân và đích.
 */
using UnityEngine;
using UnityEngine.InputSystem;
namespace TheReturn
{
    [DefaultExecutionOrder(70)]
    public sealed class NavigationExamPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public NavigationExamCatalog catalog;
        public Transform board, pawn;
        public Transform[] cells, checkpoints;
        public TextMesh boardLabel;
        public PuzzleDoor exitDoor;
        public int initialSeed;
        public NavigationExamState State { get; private set; }
        public bool Active { get; private set; }
        public bool PanelOpen { get; private set; }
        public bool Targeted { get; private set; }
        public string Status { get; private set; }
        int panelActor;

        /// <summary>Không nhận tham số; bắt đầu đội hiện tại, giữ vị trí và tiến trình các nhiệm vụ trước.</summary>
        public void Begin()
        {
            State = new NavigationExamState(party.PartySize, initialSeed);
            Active = true;
            PanelOpen = false;
            Status = catalog.introduction.Replace("{start}", NavigationExamState.Code(State.Start))
                .Replace("{goal}", NavigationExamState.Code(State.Goal));
            RefreshView();
        }

        /// <summary>Nhận cờ biến thể mới; đặt lại riêng sa bàn và đưa đội về checkpoint trước khi đóng cửa.</summary>
        public void ResetRound(bool nextVariant = false)
        {
            if (!Active || State == null) return;
            for (int i = 0; i < party.PartySize; i++) party.Teleport(i, checkpoints[i].position);
            party.SwitchRole(party.ActivePlayer, false);
            if (nextVariant) State = new NavigationExamState(party.PartySize, unchecked(State.Seed + 1));
            else State.Reset();
            ClosePanel();
            exitDoor.SetOpen(false, true);
            Status = nextVariant ? "Đã đổi biến thể. Đọc lại manh mối theo tọa độ mới." :
                "Đã đặt lại sa bàn; ba nhiệm vụ trước vẫn được giữ.";
            RefreshView();
        }

        /// <summary>Nhận cờ reset; ngừng điều khiển, giữ kết quả/cửa nếu chuyển nhiệm vụ, đặt lại khi chơi lại map.</summary>
        public void StopSession(bool reset = true)
        {
            Active = PanelOpen = false;
            if (reset) { State = null; exitDoor.SetOpen(false, true); }
        }

        /// <summary>Không nhận tham số; trả true khi người hiện tại ở gần sa bàn và không bị vật cản che.</summary>
        public bool CanReach()
        {
            if (party.PartySize < 2 || !party.players[party.ActivePlayer].gameObject.activeInHierarchy) return false;
            Vector3 eye = party.players[party.ActivePlayer].position + Vector3.up * 1.5f;
            Vector3 delta = board.position - eye;
            if (delta.magnitude > 3.2f) return false;
            foreach (RaycastHit hit in Physics.RaycastAll(eye, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(party.players[party.ActivePlayer]) && !hit.transform.IsChildOf(board)) return false;
            return true;
        }

        /// <summary>Không nhận tham số; mở sa bàn khi đủ tầm, ghi người thao tác và trả kết quả.</summary>
        public bool TryOpen()
        {
            if (!Active || State == null || !CanReach()) return false;
            panelActor = party.ActivePlayer;
            PanelOpen = true;
            PrototypePartyController.SetCursor(false);
            return true;
        }

        /// <summary>Không nhận tham số; đóng UI và trả chuột cho góc nhìn thứ nhất.</summary>
        public void ClosePanel()
        {
            PanelOpen = false;
            PrototypePartyController.SetCursor(true);
        }

        /// <summary>Nhận vector bước và số bước UI đang thấy; từ chối lệnh cũ hoặc từ xa, trả mã kết quả.</summary>
        public ExamResult Step(int dx, int dy, int expectedSteps)
        {
            if (!Active || State == null || !PanelOpen || panelActor != party.ActivePlayer ||
                !CanReach() || expectedSteps != State.Steps) return ExamResult.Invalid;
            ExamResult result = State.Move(party.ActivePlayer, dx, dy);
            Status = catalog.Message(result);
            if (State.Solved) exitDoor.SetOpen(true);
            RefreshView();
            return result;
        }

        /// <summary>Không nhận tham số; đọc đổi vai, mở bảng, thử lại và di chuyển, không nhận bước từ phím đi bộ.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                for (int i = 0; i < party.PartySize; i++)
                {
                    if (!keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) continue;
                    bool reopen = PanelOpen;
                    ClosePanel();
                    party.SwitchRole(i, false);
                    if (reopen) TryOpen();
                }
                if (keyboard.eKey.wasPressedThisFrame && PanelOpen) { ClosePanel(); return; }
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                {
                    if (PanelOpen) ClosePanel();
                    else PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                }
                if (!PanelOpen && Cursor.lockState == CursorLockMode.Locked)
                {
                    if (keyboard.rKey.wasPressedThisFrame) ResetRound();
                    if (keyboard.f5Key.wasPressedThisFrame) ResetRound(true);
                }
            }
            if (PanelOpen)
            {
                if (panelActor != party.ActivePlayer || !CanReach()) ClosePanel();
                return;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            party.TickInput(false);
            RaycastHit hit;
            Targeted = party.TryGetInteractionHit(3.2f, out hit) && hit.transform.IsChildOf(board);
            if (Targeted && keyboard != null && keyboard.eKey.wasPressedThisFrame) TryOpen();
        }

        /// <summary>Không nhận tham số; đặt quân ở ô hiện tại và cập nhật bảng, không tô lộ các ô cấm.</summary>
        void RefreshView()
        {
            pawn.position = cells[State.Position].position + Vector3.up * .17f;
            boardLabel.text = "SA BAN / E\n" + NavigationExamState.Code(State.Start) + " -> " +
                NavigationExamState.Code(State.Goal) + "\n" +
                (State.Solved ? "HOAN THANH" : "LUOT VAI " + (State.Turn + 1));
        }
    }
}
