/*
 * Mục đích: Nối luật giáo viên thật/dối với người chơi, tượng, bốn nút, phụ đề và cổng thoát tầng.
 * Hàm: Begin khởi tạo; StopSession kết thúc; Retry thử lại/đổi biến thể;
 * CanReach kiểm tầm nhìn theo điểm trên vật; Hear / Confirm xử lý tương tác; Update điều khiển và đồng hồ;
 * RefreshWorld cập nhật nút/đèn; ResultText tạo trạng thái tiến trình.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(90)]
    public sealed class TeacherTruthPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public TeacherTruthCatalog catalog;
        public TeacherChannelSource[] teachers;
        public ManualConfirmStation[] stations;
        public Transform[] checkpoints;
        public PuzzleDoor exitDoor;
        public TextMesh syncWarning;
        public float confirmationWindow = 3f;
        public int initialSeed;

        public TeacherTruthState State { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }
        public TeacherChannelSource TargetTeacher { get; private set; }
        public ManualConfirmStation TargetStation { get; private set; }

        /// <summary>Không nhận tham số; tạo biến thể cho đội hiện tại, đặt người chơi tại phòng và đóng cổng.</summary>
        public void Begin()
        {
            int seed = initialSeed == 0 ? Random.Range(1, int.MaxValue) : initialSeed;
            State = new TeacherTruthState(party.PartySize, seed, confirmationWindow);
            Active = true;
            Status = catalog.introduction;
            exitDoor.SetOpen(false, true);
            PlaceParty();
            RefreshWorld();
        }

        /// <summary>Nhận cờ reset mặc định true; ngừng phiên, giữ state/cửa nếu chuyển map sau này.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            TargetTeacher = null;
            TargetStation = null;
            if (reset)
            {
                State = null;
                exitDoor.SetOpen(false, true);
            }
        }

        /// <summary>Nhận cờ đổi biến thể; tạo lại riêng nhiệm vụ 6, giữ kết quả năm nhiệm vụ trước.</summary>
        public void Retry(bool nextVariant = false)
        {
            if (!Active || State == null) return;
            int seed = nextVariant ? unchecked(State.Seed + 1) : State.Seed;
            State = new TeacherTruthState(party.PartySize, seed, confirmationWindow);
            Status = nextVariant ? "Đã đổi bộ lời thông báo. Hãy nghe và đối chiếu lại chứng cứ." : catalog.introduction;
            exitDoor.SetOpen(false, true);
            PlaceParty();
            RefreshWorld();
        }

        /// <summary>Không nhận tham số; đưa đội vào các vị trí đầu phòng và khóa chuột cho góc nhìn thứ nhất.</summary>
        void PlaceParty()
        {
            for (int i = 0; i < party.PartySize; i++) party.Teleport(i, checkpoints[i].position);
            party.SwitchRole(0, false);
            PrototypePartyController.SetCursor(true);
        }

        /// <summary>Nhận transform, độ lệch điểm ngắm và tầm; trả true nếu vai hiện tại nhìn thấy đích, không bị vật cản che.</summary>
        bool CanReach(Transform target, Vector3 localOffset, float distance)
        {
            if (!Active || State == null || target == null || party.PartySize < 2) return false;
            Vector3 eye = party.viewCamera.transform.position;
            Vector3 delta = target.TransformPoint(localOffset) - eye;
            if (delta.magnitude > distance || Vector3.Angle(party.viewCamera.transform.forward, delta) > 24f) return false;
            foreach (RaycastHit hit in Physics.RaycastAll(eye, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(party.players[party.ActivePlayer]) && !hit.transform.IsChildOf(target))
                    return false;
            return true;
        }

        /// <summary>Nhận nguồn giáo viên; xác thực tầm nhìn, ghi phụ đề riêng và trả thành công.</summary>
        public bool Hear(TeacherChannelSource source)
        {
            if (source == null || !CanReach(source.transform, new Vector3(0,1.3f,0), 4f) || !State.Hear(party.ActivePlayer, source.channel))
                return false;
            Status = catalog.Announcement(State.AnnouncementVariant, source.channel);
            return true;
        }

        /// <summary>Nhận nút; xác thực tầm nhìn, gửi người/kênh vào luật, cập nhật phản hồi và cửa.</summary>
        public ManualConfirmResult Confirm(ManualConfirmStation station)
        {
            if (station == null || !CanReach(station.transform, Vector3.zero, 3.2f)) return ManualConfirmResult.Invalid;
            ManualConfirmResult result = State.Confirm(party.ActivePlayer, station.channel);
            Status = catalog.Message(result);
            if (State.Solved) exitDoor.SetOpen(true);
            RefreshWorld();
            return result;
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, di chuyển, raycast, xác nhận và hết cửa sổ.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            if (State.Tick(Time.deltaTime))
            {
                Status = "Hết 3 giây. Bốn kênh đã được đặt lại; phụ đề vẫn được giữ.";
                RefreshWorld();
            }
            else if (State.WindowActive && syncWarning != null)
                syncWarning.text = "XÁC NHẬN " + State.ConfirmedCount + "/4  •  " + State.Remaining.ToString("0.0") + "s";
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                for (int i = 0; i < party.PartySize; i++)
                    if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                    {
                        party.SwitchRole(i, false);
                        Status = catalog.introduction;
                    }
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    if (keyboard.rKey.wasPressedThisFrame) { Retry(); return; }
                    if (keyboard.f5Key.wasPressedThisFrame) { Retry(true); return; }
                }
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            party.TickInput(false);
            TargetTeacher = null;
            TargetStation = null;
            RaycastHit hit;
            if (party.TryGetInteractionHit(4f, out hit))
            {
                TargetTeacher = hit.transform.GetComponentInParent<TeacherChannelSource>();
                TargetStation = hit.transform.GetComponentInParent<ManualConfirmStation>();
                if (TargetTeacher != null && !CanReach(TargetTeacher.transform, new Vector3(0,1.3f,0), 4f)) TargetTeacher = null;
                if (TargetStation != null && !CanReach(TargetStation.transform, Vector3.zero, 3.2f)) TargetStation = null;
            }
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (TargetTeacher != null) Hear(TargetTeacher);
                else if (TargetStation != null) Confirm(TargetStation);
            }
        }

        /// <summary>Không nhận tham số; đồng bộ nhãn bốn nút và đèn mất đồng bộ từ state hiện tại.</summary>
        void RefreshWorld()
        {
            if (State == null) return;
            for (int i = 0; i < stations.Length; i++) stations[i].SetVisual(State.ConfirmedBy(stations[i].channel));
            if (syncWarning != null)
                syncWarning.text = State.Solved ? "XÁC NHẬN THỦ CÔNG: HOÀN TẤT" :
                    State.WindowActive ? "XÁC NHẬN " + State.ConfirmedCount + "/4  •  " + State.Remaining.ToString("0.0") + "s" :
                    "MẤT ĐỒNG BỘ";
        }

        /// <summary>Không nhận tham số; trả dòng tiến trình cho HUD, gồm số kênh, người tham gia và thời gian.</summary>
        public string ResultText()
        {
            if (State == null) return "";
            if (State.Solved) return catalog.success;
            return "Kênh " + State.ConfirmedCount + "/4 • Thành viên " + State.ParticipantCount + "/" + State.PlayerCount +
                (State.WindowActive ? " • Còn " + State.Remaining.ToString("0.0") + " giây" : " • Chưa bắt đầu xác nhận");
        }
    }
}
