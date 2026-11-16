/*
 * Mục đích: Panel ONLINE ở menu chính (T-054): nhập tên, «TẠO PHÒNG» (làm chủ phòng, tối đa 4 người) hoặc nhập mã rồi «VÀO PHÒNG». Dùng OnlineSession.Ensure để sinh phiên online,
 * RoomService.CreateRelay/JoinRelay để tạo/vào phòng qua Unity Relay. Chủ phòng tạo xong thì tự nạp scene Sanh_Cho bằng NetworkSceneManager (khách vào sau được NGO tự đồng bộ sang cùng scene).
 * Esc hoặc «QUAY LẠI» hủy phiên và về menu chính. Lỗi (sai mã, phòng đầy, mạng) hiện ở dòng thông báo.
 * Hàm: Awake; OnEnable; Update; OnCreate; OnJoin; CreateRoutine; JoinRoutine; WaitReady; Fail; Say; OnBack; SavedName.
 */
using System.Collections;
using TheReturn.Network.Room;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheReturn.Network.LobbyOnline
{
    /// <summary>Giao diện tạo/vào phòng online ở menu chính.</summary>
    public sealed class OnlineMenuPanel : MonoBehaviour
    {
        public GameObject mainPanel;
        public InputField nameInput;
        public InputField codeInput;
        public Button createButton;
        public Button joinButton;
        public Button backButton;
        public Text messageText;
        [Tooltip("Scene sảnh chờ (phải có trong Build Settings).")]
        public string lobbySceneName = "Sanh_Cho";
        [Min(2)] public int maxPlayers = 4;

        private const string NameKey = "TheReturn.PlayerName";
        private bool busy;

        /// <summary>Không nhận tham số; nối nút với hành động.</summary>
        private void Awake()
        {
            if (createButton != null) { createButton.onClick.RemoveAllListeners(); createButton.onClick.AddListener(OnCreate); }
            if (joinButton != null) { joinButton.onClick.RemoveAllListeners(); joinButton.onClick.AddListener(OnJoin); }
            if (backButton != null) { backButton.onClick.RemoveAllListeners(); backButton.onClick.AddListener(OnBack); }
        }

        /// <summary>Không nhận tham số; mỗi lần panel hiện thì nạp lại tên đã lưu và xóa thông báo.</summary>
        private void OnEnable()
        {
            busy = false;
            if (nameInput != null && string.IsNullOrEmpty(nameInput.text)) nameInput.text = SavedName();
            SetBusy(false);
            Say("", false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !busy) OnBack();
        }

        private static string SavedName() { return PlayerPrefs.GetString(NameKey, ""); }

        /// <summary>Không nhận tham số; kiểm tên rồi bắt đầu tạo phòng.</summary>
        public void OnCreate()
        {
            if (busy) return;
            string name = CleanName();
            StartCoroutine(CreateRoutine(name));
        }

        /// <summary>Không nhận tham số; kiểm tên + mã rồi bắt đầu vào phòng.</summary>
        public void OnJoin()
        {
            if (busy) return;
            string code = codeInput != null ? codeInput.text.Trim().ToUpperInvariant() : "";
            if (code.Length < 4) { Say("Nhập mã phòng mà chủ phòng gửi cho bạn.", true); return; }
            StartCoroutine(JoinRoutine(CleanName(), code));
        }

        private string CleanName()
        {
            string name = nameInput != null ? nameInput.text.Trim() : "";
            if (name.Length == 0) name = "Người chơi";
            PlayerPrefs.SetString(NameKey, name);
            return name;
        }

        private IEnumerator CreateRoutine(string name)
        {
            SetBusy(true);
            Say("Đang tạo phòng...", false);
            RoomService service = OnlineSession.Ensure();
            if (service == null) { Fail("Không khởi tạo được phiên online (thiếu prefab)."); yield break; }
            yield return WaitReady(service);
            if (!service.Ready) { Fail("Phiên online chưa sẵn sàng, thử lại."); yield break; }
            service.CreateRelay(maxPlayers, name);
            float t = 0f;
            yield return null;
            NetworkSessionService session = service.Session;
            while (session.State == NetworkSessionState.Working && t < 30f) { t += Time.unscaledDeltaTime; yield return null; }
            if (session.State != NetworkSessionState.Hosting) { Fail(session.StatusText); yield break; }
            Say("Đã tạo phòng, đang vào sảnh...", false);
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || nm.SceneManager == null) { Fail("Mất NetworkManager."); yield break; }
            nm.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        }

        private IEnumerator JoinRoutine(string name, string code)
        {
            SetBusy(true);
            Say("Đang vào phòng " + code + "...", false);
            RoomService service = OnlineSession.Ensure();
            if (service == null) { Fail("Không khởi tạo được phiên online (thiếu prefab)."); yield break; }
            yield return WaitReady(service);
            if (!service.Ready) { Fail("Phiên online chưa sẵn sàng, thử lại."); yield break; }
            service.JoinRelay(code, name);
            float t = 0f;
            yield return null;
            NetworkSessionService session = service.Session;
            while (session.State == NetworkSessionState.Working && t < 30f) { t += Time.unscaledDeltaTime; yield return null; }
            if (session.State == NetworkSessionState.Joined)
            {
                Say("Đã vào phòng. Đang chờ chủ phòng chuyển sang sảnh...", false);
                float wait = 0f;
                while (wait < 20f && session.State == NetworkSessionState.Joined) { wait += Time.unscaledDeltaTime; yield return null; }
            }
            if (session.State != NetworkSessionState.Joined)
            {
                string reason = !string.IsNullOrEmpty(service.Notice) ? service.Notice : session.StatusText;
                Fail(string.IsNullOrEmpty(reason) ? "Không vào được phòng." : reason);
            }
            else Fail("Chủ phòng chưa chuyển sang sảnh. Thử lại.");
        }

        private static IEnumerator WaitReady(RoomService service)
        {
            float t = 0f;
            while (!service.Ready && t < 5f) { t += Time.unscaledDeltaTime; yield return null; }
        }

        /// <summary>Nhận câu lỗi; hiện ra, hủy phiên online dở dang và cho bấm lại.</summary>
        private void Fail(string message)
        {
            Say(string.IsNullOrEmpty(message) ? "Có lỗi, thử lại." : message, true);
            OnlineSession.Shutdown();
            SetBusy(false);
        }

        private void SetBusy(bool value)
        {
            busy = value;
            if (createButton != null) createButton.interactable = !value;
            if (joinButton != null) joinButton.interactable = !value;
            if (backButton != null) backButton.interactable = !value;
        }

        /// <summary>Nhận câu và cờ lỗi; hiện ở dòng thông báo (lỗi = đỏ, thường = xám).</summary>
        private void Say(string text, bool error)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageText.color = error ? ModernUI.Danger : ModernUI.TextDim;
        }

        /// <summary>Không nhận tham số; hủy phiên online (nếu có) và về menu chính.</summary>
        public void OnBack()
        {
            if (busy) return;
            OnlineSession.Shutdown();
            gameObject.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }
    }
}
