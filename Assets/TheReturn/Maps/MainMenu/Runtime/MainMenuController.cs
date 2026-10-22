/*
 * Mục đích: Màn hình bắt đầu của game (T-008): menu chính, lập nhóm (tạo phòng / vào bằng mã / phòng chờ nhóm) và Settings.
 * Chỉ nối giao diện với PartySession.Service (IPartyService) và GameSettingsRuntime.Model; KHÔNG chứa luật nhóm hay luật cài đặt.
 * Chế độ MỘT MÁY: nút '+ Người giả lập' thêm người chơi giả để thử luồng 2-4 người khi chưa có mạng.
 * Luồng: MainMenu -> Chơi -> (Tạo phòng | Vào phòng bằng mã) -> phòng chờ (Sẵn sàng, chủ phòng Bắt đầu) -> nạp scene Sanh_Cho.
 * Hàm: Awake; OnDestroy; Update (Esc quay lại); Show; OnPlay; OnSettings; OnQuit; OnBack; OnCreateParty; OnJoinParty; OnToggleReady;
 * OnLeaveParty; OnStartGame; OnAddSimulated; OnRemoveSimulated; OnPartyChanged; RenderRoom; RenderSettings; OnVolume; OnSensitivity;
 * OnFullscreen; OnResetSettings; Describe (câu thông báo theo PartyResult); DisplayName.
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Điều khiển các màn hình của menu bắt đầu.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        public enum Screen { Main, PartyEntry, PartyRoom, Settings }

        [Header("Tên scene")]
        public string lobbySceneName = "Sanh_Cho";

        [Header("Màn hình")]
        public GameObject mainPanel;
        public GameObject partyEntryPanel;
        public GameObject partyRoomPanel;
        public GameObject settingsPanel;

        [Header("Menu chính")]
        public Button playButton;
        public Button settingsButton;
        public Button quitButton;

        [Header("Lập nhóm")]
        public InputField nameInput;
        public InputField codeInput;
        public Button createButton;
        public Button joinButton;
        public Button entryBackButton;
        public Text entryMessage;

        [Header("Phòng chờ")]
        public Text roomCodeLabel;
        public Text[] memberRows;
        public Button readyButton;
        public Text readyButtonLabel;
        public Button leaveButton;
        public Button startButton;
        public Button addSimulatedButton;
        public Button removeSimulatedButton;
        public Text roomMessage;

        [Header("Settings")]
        public Slider volumeSlider;
        public Text volumeLabel;
        public Slider sensitivitySlider;
        public Text sensitivityLabel;
        public Toggle fullscreenToggle;
        public Button resetSettingsButton;
        public Button settingsBackButton;

        Screen current = Screen.Main;
        bool rendering;
        bool loadingScene;

        /// <summary>Không nhận tham số; đặt lại nhóm cũ, mở khóa chuột, nối nút và hiện menu chính.</summary>
        void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PartySession.Reset();
            PartySession.Service.Changed += OnPartyChanged;
            Bind(playButton, OnPlay); Bind(settingsButton, OnSettings); Bind(quitButton, OnQuit);
            Bind(createButton, OnCreateParty); Bind(joinButton, OnJoinParty); Bind(entryBackButton, OnBack);
            Bind(readyButton, OnToggleReady); Bind(leaveButton, OnLeaveParty); Bind(startButton, OnStartGame);
            Bind(addSimulatedButton, OnAddSimulated); Bind(removeSimulatedButton, OnRemoveSimulated);
            Bind(resetSettingsButton, OnResetSettings); Bind(settingsBackButton, OnBack);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(OnVolume);
            if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(OnSensitivity);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreen);
            if (nameInput != null && string.IsNullOrEmpty(nameInput.text)) nameInput.text = "Học sinh 1";
            Show(Screen.Main);
        }

        /// <summary>Không nhận tham số; gỡ đăng ký sự kiện nhóm khi menu bị hủy.</summary>
        void OnDestroy()
        {
            if (PartySession.Service != null) PartySession.Service.Changed -= OnPartyChanged;
        }

        /// <summary>Không nhận tham số; phím Esc quay lại màn hình trước (từ phòng chờ thì rời phòng).</summary>
        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (current == Screen.PartyRoom) OnLeaveParty();
            else if (current != Screen.Main) OnBack();
        }

        /// <summary>Nhận màn hình cần hiện; bật đúng panel và vẽ lại nội dung của nó.</summary>
        public void Show(Screen screen)
        {
            current = screen;
            SetActive(mainPanel, screen == Screen.Main);
            SetActive(partyEntryPanel, screen == Screen.PartyEntry);
            SetActive(partyRoomPanel, screen == Screen.PartyRoom);
            SetActive(settingsPanel, screen == Screen.Settings);
            if (screen == Screen.PartyEntry && entryMessage != null) entryMessage.text = "Chế độ MỘT MÁY (giả lập). Online sẽ nối sau.";
            if (screen == Screen.PartyRoom) RenderRoom(PartySession.Service.Current);
            if (screen == Screen.Settings) RenderSettings();
        }

        /// <summary>Không nhận tham số; vào lập nhóm (vào thẳng phòng chờ nếu đã ở trong nhóm).</summary>
        public void OnPlay() { Show(PartySession.Service.Current != null ? Screen.PartyRoom : Screen.PartyEntry); }

        /// <summary>Không nhận tham số; mở màn hình Settings.</summary>
        public void OnSettings() { Show(Screen.Settings); }

        /// <summary>Không nhận tham số; thoát game (trong Editor thì dừng Play Mode).</summary>
        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Không nhận tham số; quay về menu chính.</summary>
        public void OnBack() { Show(Screen.Main); }

        /// <summary>Không nhận tham số; tạo phòng với tên đã nhập; lỗi thì hiện thông báo.</summary>
        public void OnCreateParty()
        {
            PartyResult r = PartySession.Service.CreateParty(DisplayName());
            if (r != PartyResult.Ok && entryMessage != null) entryMessage.text = Describe(r);
        }

        /// <summary>Không nhận tham số; vào phòng bằng mã đã nhập; lỗi thì hiện thông báo.</summary>
        public void OnJoinParty()
        {
            PartyResult r = PartySession.Service.JoinParty(codeInput != null ? codeInput.text : null, DisplayName());
            if (r != PartyResult.Ok && entryMessage != null) entryMessage.text = Describe(r);
        }

        /// <summary>Không nhận tham số; đảo trạng thái Sẵn sàng của người chơi trên máy này.</summary>
        public void OnToggleReady()
        {
            PartySnapshot s = PartySession.Service.Current;
            if (s == null) return;
            int slot = s.LocalSlot;
            PartyMemberInfo me = slot >= 0 ? s.MemberAtSlot(slot) : null;
            PartyResult r = PartySession.Service.SetReady(me == null || !me.Ready);
            if (r != PartyResult.Ok && roomMessage != null) roomMessage.text = Describe(r);
        }

        /// <summary>Không nhận tham số; rời phòng và quay lại màn hình lập nhóm.</summary>
        public void OnLeaveParty() { PartySession.Service.Leave(); }

        /// <summary>Không nhận tham số; chủ phòng bắt đầu; lỗi thì hiện thông báo, thành công thì nạp sảnh chờ.</summary>
        public void OnStartGame()
        {
            PartyResult r = PartySession.Service.StartGame();
            if (r != PartyResult.Ok && roomMessage != null) roomMessage.text = Describe(r);
        }

        /// <summary>Không nhận tham số; thêm một người chơi giả lập (chỉ có ở chế độ một máy).</summary>
        public void OnAddSimulated()
        {
            var local = PartySession.Service as LocalPartyService;
            if (local == null) return;
            PartyResult r = local.AddSimulatedPlayer();
            if (r != PartyResult.Ok && roomMessage != null) roomMessage.text = Describe(r);
        }

        /// <summary>Không nhận tham số; bớt một người chơi giả lập (chỉ có ở chế độ một máy).</summary>
        public void OnRemoveSimulated()
        {
            var local = PartySession.Service as LocalPartyService;
            if (local != null) local.RemoveSimulatedPlayer();
        }

        /// <summary>Nhận ảnh chụp nhóm (null = đã rời nhóm); chuyển màn hình, vẽ lại phòng và nạp sảnh khi đã bắt đầu.</summary>
        void OnPartyChanged(PartySnapshot snapshot)
        {
            if (snapshot == null) { if (current == Screen.PartyRoom) Show(Screen.PartyEntry); return; }
            if (current != Screen.PartyRoom) Show(Screen.PartyRoom); else RenderRoom(snapshot);
            if (snapshot.Started && !loadingScene)
            {
                loadingScene = true;
                if (SceneTransitionService.Instance == null) new GameObject("SceneTransitionService").AddComponent<SceneTransitionService>();
                SceneTransitionService.Instance.LoadScene(lobbySceneName);
            }
        }

        /// <summary>Nhận ảnh chụp nhóm (có thể null); vẽ mã phòng, bốn dòng thành viên và trạng thái các nút.</summary>
        void RenderRoom(PartySnapshot s)
        {
            if (s == null) return;
            if (roomCodeLabel != null) roomCodeLabel.text = "MÃ PHÒNG: " + s.Code;
            for (int i = 0; memberRows != null && i < memberRows.Length; i++)
            {
                PartyMemberInfo m = s.MemberAtSlot(i);
                memberRows[i].text = m == null
                    ? (i + 1) + ".  — Trống —"
                    : (i + 1) + ".  " + m.Name + (m.IsLocal ? "  (Bạn)" : "") + (m.IsHost ? "  [CHỦ PHÒNG]" : "") + "   " + (m.Ready ? "SẴN SÀNG" : "đang chờ");
            }
            PartyMemberInfo me = s.LocalSlot >= 0 ? s.MemberAtSlot(s.LocalSlot) : null;
            if (readyButtonLabel != null) readyButtonLabel.text = me != null && me.Ready ? "Hủy sẵn sàng" : "Sẵn sàng";
            if (readyButton != null) readyButton.interactable = !s.Started;
            if (startButton != null) startButton.interactable = s.IsLocalHost && s.CanStart;
            bool isLocalService = PartySession.Service is LocalPartyService;
            if (addSimulatedButton != null) { addSimulatedButton.gameObject.SetActive(isLocalService); addSimulatedButton.interactable = !s.Started && s.Count < PartyState.MaxPlayers; }
            if (removeSimulatedButton != null) { removeSimulatedButton.gameObject.SetActive(isLocalService); removeSimulatedButton.interactable = !s.Started && s.Count > 1; }
            if (roomMessage != null)
                roomMessage.text = s.Started ? "Đang vào sảnh chờ..." :
                    s.Count < PartyState.MinPlayers ? "Cần từ 2 người. Dùng '+ Người giả lập' để thử một máy." :
                    s.CanStart ? (s.IsLocalHost ? "Mọi người đã sẵn sàng. Bấm BẮT ĐẦU." : "Chờ chủ phòng bắt đầu.") :
                    "Chờ mọi người bấm Sẵn sàng.";
        }

        /// <summary>Không nhận tham số; đặt thanh trượt, công tắc và nhãn theo cài đặt hiện tại mà không kích hoạt sự kiện đổi giá trị.</summary>
        void RenderSettings()
        {
            GameSettingsData d = GameSettingsRuntime.Model.Current;
            rendering = true;
            if (volumeSlider != null) { volumeSlider.minValue = 0f; volumeSlider.maxValue = 1f; volumeSlider.value = d.Volume; }
            if (sensitivitySlider != null) { sensitivitySlider.minValue = GameSettingsData.MinSensitivity; sensitivitySlider.maxValue = GameSettingsData.MaxSensitivity; sensitivitySlider.value = d.MouseSensitivity; }
            if (fullscreenToggle != null) fullscreenToggle.isOn = d.Fullscreen;
            rendering = false;
            if (volumeLabel != null) volumeLabel.text = "Âm lượng: " + d.VolumePercent + "%";
            if (sensitivityLabel != null) sensitivityLabel.text = "Độ nhạy chuột: x" + d.MouseSensitivity.ToString("0.0");
        }

        /// <summary>Nhận giá trị thanh âm lượng 0-1; ghi vào mô hình cài đặt và cập nhật nhãn.</summary>
        void OnVolume(float value) { if (rendering) return; GameSettingsRuntime.Model.SetVolume(value); RenderLabels(); }

        /// <summary>Nhận giá trị thanh độ nhạy; ghi vào mô hình cài đặt và cập nhật nhãn.</summary>
        void OnSensitivity(float value) { if (rendering) return; GameSettingsRuntime.Model.SetMouseSensitivity(value); RenderLabels(); }

        /// <summary>Nhận cờ công tắc toàn màn hình; ghi vào mô hình cài đặt.</summary>
        void OnFullscreen(bool value) { if (rendering) return; GameSettingsRuntime.Model.SetFullscreen(value); }

        /// <summary>Không nhận tham số; đưa cài đặt về mặc định và vẽ lại.</summary>
        public void OnResetSettings() { GameSettingsRuntime.Model.ResetToDefaults(); RenderSettings(); }

        /// <summary>Không nhận tham số; cập nhật hai nhãn phần trăm và hệ số theo giá trị hiện tại.</summary>
        void RenderLabels()
        {
            GameSettingsData d = GameSettingsRuntime.Model.Current;
            if (volumeLabel != null) volumeLabel.text = "Âm lượng: " + d.VolumePercent + "%";
            if (sensitivityLabel != null) sensitivityLabel.text = "Độ nhạy chuột: x" + d.MouseSensitivity.ToString("0.0");
        }

        /// <summary>Không nhận tham số; trả tên đã nhập (mặc định 'Học sinh 1' nếu để trống).</summary>
        string DisplayName()
        {
            string n = nameInput != null ? nameInput.text : null;
            return string.IsNullOrWhiteSpace(n) ? "Học sinh 1" : n.Trim();
        }

        /// <summary>Nhận kết quả thao tác nhóm; trả câu thông báo tiếng Việt.</summary>
        public static string Describe(PartyResult r)
        {
            switch (r)
            {
                case PartyResult.Ok: return "";
                case PartyResult.NoParty: return "Bạn chưa ở trong nhóm nào.";
                case PartyResult.AlreadyInParty: return "Bạn đang ở trong một nhóm rồi.";
                case PartyResult.NotFound: return "Không tìm thấy phòng với mã này.";
                case PartyResult.Full: return "Phòng đã đủ 4 người.";
                case PartyResult.Started: return "Phòng đã bắt đầu, không thay đổi được nữa.";
                case PartyResult.InvalidName: return "Tên không hợp lệ (không được để trống).";
                case PartyResult.InvalidCode: return "Mã phòng phải có 4 ký tự (chữ hoặc số).";
                case PartyResult.NotHost: return "Chỉ chủ phòng mới bắt đầu được.";
                case PartyResult.NotEnoughPlayers: return "Cần ít nhất 2 người để bắt đầu.";
                case PartyResult.NotAllReady: return "Chưa phải mọi người đều sẵn sàng.";
                default: return r.ToString();
            }
        }

        /// <summary>Nhận nút và hành động; gỡ hành động cũ rồi gắn hành động mới (bỏ qua nút null).</summary>
        static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// <summary>Nhận GameObject (có thể null) và cờ; bật/tắt nếu tồn tại.</summary>
        static void SetActive(GameObject go, bool active) { if (go != null) go.SetActive(active); }
    }
}
