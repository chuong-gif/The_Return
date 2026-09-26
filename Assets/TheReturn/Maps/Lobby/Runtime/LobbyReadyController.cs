/*
 * Mục đích: Quản lý đội 2–4 người trong sảnh, trạng thái sẵn sàng, đếm ngược và yêu cầu chuyển tới tầng học.
 * Danh sách hàm:
 * - Awake / Start: kiểm tra tham chiếu, khởi tạo đội và UI.
 * - Update: xử lý di chuyển thử nghiệm, phím đổi vai và phím sẵn sàng.
 * - SetPartySize: nhận số người 2–4; cập nhật avatar và xóa trạng thái slot ngoài đội.
 * - SetReady / ToggleReady: nhận slot và trạng thái; cập nhật tiến trình sẵn sàng.
 * - StartCountdown / CancelCountdown: bắt đầu hoặc dừng đếm ngược an toàn.
 * - CountdownRoutine: trả IEnumerator; chuyển scene khi toàn đội vẫn sẵn sàng.
 * - RefreshView: đẩy dữ liệu hiện tại sang Canvas, không trả dữ liệu.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    public sealed class LobbyReadyController : MonoBehaviour
    {
        [Header("Tham chiếu scene")]
        public PrototypePartyController party;
        public LobbyCanvasView view;
        public Renderer portalRenderer;
        [Header("Thiết lập phiên")]
        [Range(2, 4)] public int participantCount = 4;
        public string schoolSceneName = "SchoolFloorPrototype";
        [Min(1f)] public float countdownSeconds = 5f;

        readonly bool[] ready = new bool[4];
        Coroutine countdown;
        float remaining;

        /// <summary>Không nhận đầu vào; kiểm tra các tham chiếu bắt buộc trước khi sảnh hoạt động.</summary>
        void Awake()
        {
            if (party == null || view == null)
            {
                Debug.LogError("Lobby chưa được cấu hình đầy đủ.", this);
                enabled = false;
            }
        }

        /// <summary>Không nhận đầu vào; khởi tạo đội mặc định, khóa góc nhìn và cập nhật Canvas.</summary>
/// <summary>Không nhận đầu vào; bật chạy nền, khởi tạo đội mặc định, khóa góc nhìn và cập nhật Canvas.</summary>
        void Start()
        {
            Application.runInBackground = true;
            party.Initialize();
            SetPartySize(participantCount);
            party.SwitchRole(0, false);
            PrototypePartyController.SetCursor(false);
            RefreshView();
        }

        /// <summary>Không nhận đầu vào; đọc điều khiển thử một máy và cập nhật vị trí người chơi.</summary>
        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame)
                PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
            if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
            if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
            if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
            if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
            if (keyboard.f1Key.wasPressedThisFrame) ToggleReady(0);
            if (keyboard.f2Key.wasPressedThisFrame) ToggleReady(1);
            if (keyboard.f3Key.wasPressedThisFrame) ToggleReady(2);
            if (keyboard.f4Key.wasPressedThisFrame) ToggleReady(3);
            party.TickInput(false);
        }

        /// <summary>Nhận số người 2–4; bật đúng avatar và hủy đếm ngược nếu thành phần đội thay đổi.</summary>
        public void SetPartySize(int count)
        {
            participantCount = Mathf.Clamp(count, 2, 4);
            party.SetPartySize(participantCount);
            for (int i = participantCount; i < ready.Length; i++) ready[i] = false;
            CancelCountdown();
            RefreshView();
        }

        /// <summary>Nhận slot 0–3 và trạng thái; bỏ qua slot ngoài đội rồi kiểm tra toàn đội.</summary>
        public void SetReady(int slot, bool value)
        {
            if (slot < 0 || slot >= participantCount) return;
            ready[slot] = value;
            if (AllReady()) StartCountdown();
            else CancelCountdown();
            RefreshView();
        }

        /// <summary>Nhận slot 0–3; đảo trạng thái sẵn sàng và không trả dữ liệu.</summary>
        public void ToggleReady(int slot) { SetReady(slot, slot >= 0 && slot < ready.Length && !ready[slot]); }

        /// <summary>Không nhận đầu vào; bắt đầu đếm ngược một lần nếu đủ người sẵn sàng.</summary>
        public void StartCountdown()
        {
            if (!AllReady() || countdown != null) return;
            countdown = StartCoroutine(CountdownRoutine());
        }

        /// <summary>Không nhận đầu vào; dừng đếm ngược hiện tại và trả portal về trạng thái chờ.</summary>
        public void CancelCountdown()
        {
            if (countdown != null) StopCoroutine(countdown);
            countdown = null;
            remaining = 0f;
            RefreshPortal(false);
        }

        /// <summary>Không nhận đầu vào; trả IEnumerator đếm ngược và tải tầng học nếu đội không đổi.</summary>
        IEnumerator CountdownRoutine()
        {
            remaining = countdownSeconds;
            RefreshPortal(true);
            while (remaining > 0f && AllReady())
            {
                remaining -= Time.unscaledDeltaTime;
                RefreshView();
                yield return null;
            }
            if (AllReady())
            {
                view.SetStatus("CỔNG ĐÃ MỞ — ĐANG CHUYỂN TỚI TẦNG HỌC");
                if (SceneTransitionService.Instance == null)
                    new GameObject("SceneTransitionService").AddComponent<SceneTransitionService>();
                SceneTransitionService.Instance.LoadScene(schoolSceneName);
            }
            countdown = null;
        }

        /// <summary>Không nhận đầu vào; trả true khi mọi slot đang tham gia đã sẵn sàng.</summary>
        bool AllReady()
        {
            for (int i = 0; i < participantCount; i++) if (!ready[i]) return false;
            return participantCount >= 2;
        }

        /// <summary>Không nhận đầu vào; gửi roster, trạng thái và thời gian còn lại sang Canvas.</summary>
        void RefreshView()
        {
            if (view != null) view.Render(participantCount, ready, countdown != null ? Mathf.CeilToInt(remaining) : -1, this);
        }

        /// <summary>Nhận trạng thái mở; đổi màu portal để phản hồi trực quan.</summary>
        void RefreshPortal(bool open)
        {
            if (portalRenderer == null) return;
            portalRenderer.material.SetColor("_BaseColor", open ? new Color(.2f, 1f, .85f) : new Color(.12f, .35f, .5f));
            portalRenderer.material.SetColor("_EmissionColor", open ? new Color(0f, 4f, 3f) : new Color(0f, .5f, .8f));
        }
    }
}
