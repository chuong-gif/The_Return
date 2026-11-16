/*
 * Mục đích: Menu Cài đặt dùng chung cho mọi scene. TOÀN BỘ giao diện (bánh răng, khung, 4 tab, từng dòng cài đặt, hộp xác nhận) là prefab
 * Resources/PF_SettingsMenu, chỉnh bằng Inspector/Prefab Mode; script này chỉ nối logic vào các điều khiển đã gắn sẵn, KHÔNG dựng giao diện bằng code.
 * Sống xuyên scene (DontDestroyOnLoad), tự được tạo từ prefab ở scene đầu tiên nên không cần đặt object nào vào scene.
 * Mở bằng nút bánh răng góc phải trên (chỉ hiện ở các scene trong gearScenes), phím F10, hoặc SettingsMenu.Open(). Esc/F10/nút Xong để đóng.
 * Gồm 4 tab: Âm thanh, Đồ họa, Điều khiển (đổi phím tắt), Hệ thống (về menu chính, khôi phục mặc định, thoát game).
 * Khi mở thì thả chuột, khi đóng thì trả chuột về trạng thái trước. Không chứa luật game: chỉ gọi GameSettingsRuntime.Model, ExtraSettings, GameKeys, SessionExit.
 * Hàm: Open; Close; Toggle; Boot; Ensure; Awake; OnDestroy; OnSceneLoaded; Update; LateUpdate; Bind; WireSlider; WireToggle; WireAction;
 * OpenInternal; CloseInternal; RestoreCursor; RefreshGear; RefreshAll; RefreshSliders; RefreshToggles; RefreshSelectors; BuildResolutions; RefreshKeys;
 * ShowTab; OnResetTab; BeginListen; CancelListening; UpdateListening; ShowConfirm; HideConfirm; Toast; GoMainMenu; QuitGame.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Menu Cài đặt toàn game: một object duy nhất sống xuyên scene, giao diện lấy từ prefab.</summary>
    public sealed class SettingsMenu : MonoBehaviour
    {
        const string PrefabResource = "PF_SettingsMenu";
        const string MainMenuScene = "MainMenu";

        [Header("Hiển thị nút bánh răng")]
        [Tooltip("Tên các scene có nút bánh răng. F10 vẫn mở được menu ở mọi scene.")]
        [SerializeField] string[] gearScenes = { "MainMenu", "Sanh_Cho", "MapSchoolF", "OnlineRoom" };
        [Tooltip("Gốc của nút bánh răng (Canvas riêng, thấp hơn menu).")]
        [SerializeField] GameObject gearRoot;
        [SerializeField] Button gearButton;

        [Header("Cửa sổ")]
        [SerializeField] GameObject menuRoot;
        [SerializeField] Button closeButton;
        [SerializeField] Button doneButton;
        [SerializeField] Text toast;
        [SerializeField] Text versionText;

        [Header("Tab (đúng 4 phần tử, cùng thứ tự: Âm thanh, Đồ họa, Điều khiển, Hệ thống)")]
        [SerializeField] Button[] tabButtons = new Button[4];
        [SerializeField] Image[] tabIndicators = new Image[4];
        [SerializeField] Text[] tabLabels = new Text[4];
        [SerializeField] GameObject[] pages = new GameObject[4];
        [SerializeField] Button resetButton;
        [SerializeField] Text resetLabel;

        [Header("Hộp xác nhận")]
        [SerializeField] GameObject confirmRoot;
        [SerializeField] Text confirmTitle;
        [SerializeField] Text confirmBody;
        [SerializeField] Button confirmYes;
        [SerializeField] Text confirmYesLabel;
        [SerializeField] Button confirmNo;

        [Header("Màu trạng thái (chỉnh ở đây thay vì trong code)")]
        [SerializeField] Color tabActiveBackground = new Color32(27, 33, 43, 255);
        [SerializeField] Color tabInactiveBackground = new Color(1f, 1f, 1f, 0f);
        [SerializeField] Color tabActiveText = new Color32(64, 214, 179, 255);
        [SerializeField] Color tabInactiveText = new Color32(142, 154, 171, 255);
        [SerializeField] Color keyNormalColor = new Color32(27, 33, 43, 255);
        [SerializeField] Color keyListeningColor = new Color32(64, 214, 179, 255);
        [SerializeField] Color keyNormalText = new Color32(234, 240, 246, 255);
        [SerializeField] Color keyListeningText = new Color32(6, 28, 24, 255);
        [SerializeField] Color toastGoodColor = new Color32(64, 214, 179, 255);
        [SerializeField] Color toastBadColor = new Color32(244, 106, 110, 255);
        [SerializeField] Color confirmDangerColor = new Color32(229, 72, 77, 255);
        [SerializeField] Color confirmPrimaryColor = new Color32(64, 214, 179, 255);
        [SerializeField] Color confirmDangerText = Color.white;
        [SerializeField] Color confirmPrimaryText = new Color32(6, 28, 24, 255);

        static SettingsMenu instance;

        readonly Dictionary<string, SettingsSliderRow> sliders = new Dictionary<string, SettingsSliderRow>();
        readonly Dictionary<string, SettingsToggleRow> toggles = new Dictionary<string, SettingsToggleRow>();
        readonly Dictionary<string, SettingsSelectorRow> selectors = new Dictionary<string, SettingsSelectorRow>();
        readonly Dictionary<string, SettingsActionRow> actions = new Dictionary<string, SettingsActionRow>();
        readonly List<SettingsKeyRow> keyRows = new List<SettingsKeyRow>();
        List<Vector2Int> resolutions;
        Action confirmAction;
        float toastUntil;
        bool open;
        int tab;
        GameAction? listening;
        CursorLockMode prevLock;
        bool prevVisible;
        Coroutine restoreRoutine;

        /// <summary>Không nhận tham số; true khi menu Cài đặt đang mở.</summary>
        public static bool IsOpen { get { return instance != null && instance.open; } }

        /// <summary>Không nhận tham số; mở menu Cài đặt (tự tạo từ prefab nếu chưa có).</summary>
        public static void Open() { SettingsMenu m = Ensure(); if (m != null) m.OpenInternal(); }

        /// <summary>Không nhận tham số; đóng menu Cài đặt và trả chuột về trạng thái trước.</summary>
        public static void Close() { if (instance != null) instance.CloseInternal(true); }

        /// <summary>Không nhận tham số; mở nếu đang đóng, đóng nếu đang mở.</summary>
        public static void Toggle()
        {
            SettingsMenu m = Ensure();
            if (m == null) return;
            if (m.open) m.CloseInternal(true); else m.OpenInternal();
        }

        /// <summary>Không nhận tham số; xóa tham chiếu tĩnh để lần Play sau tạo lại (an toàn khi tắt Domain Reload).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { instance = null; }

        /// <summary>Không nhận tham số; tạo menu ngay sau scene đầu tiên để phím F10 và nút bánh răng có sẵn ở mọi scene.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { Ensure(); }

        /// <summary>Không nhận tham số; trả menu duy nhất, tạo từ prefab Resources nếu chưa có (null nếu thiếu prefab).</summary>
        static SettingsMenu Ensure()
        {
            if (instance != null) return instance;
            GameObject prefab = Resources.Load<GameObject>(PrefabResource);
            if (prefab == null)
            {
                Debug.LogError("SettingsMenu: không tìm thấy prefab Resources/" + PrefabResource + ". Chạy menu The Return > UI > Dựng prefab Cài đặt để tạo lại.");
                return null;
            }
            GameObject go = Instantiate(prefab);
            go.name = "SettingsMenu";
            return instance;
        }

        /// <summary>Không nhận tham số; giữ bản duy nhất qua các scene, nối logic vào điều khiển của prefab và ẩn cửa sổ.</summary>
        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            Bind();
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (menuRoot != null) menuRoot.SetActive(false);
            RefreshGear();
        }

        /// <summary>Không nhận tham số; gỡ đăng ký sự kiện khi bị hủy.</summary>
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        /// <summary>Nhận scene vừa nạp; đóng menu (scene mới tự đặt chuột) và cập nhật nút bánh răng.</summary>
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (open) CloseInternal(false);
            RefreshGear();
        }

        /// <summary>Không nhận tham số; xử lý F10, Esc, gán phím và tắt thông báo sau vài giây.</summary>
        void Update()
        {
            if (toast != null && toastUntil > 0f && Time.unscaledTime > toastUntil) { toast.text = ""; toastUntil = 0f; }
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (listening.HasValue) { UpdateListening(kb); return; }
            if (kb.f10Key.wasPressedThisFrame) { if (open) CloseInternal(true); else OpenInternal(); return; }
            if (open && kb.escapeKey.wasPressedThisFrame)
            {
                if (confirmRoot != null && confirmRoot.activeSelf) HideConfirm(); else CloseInternal(true);
            }
        }

        /// <summary>Không nhận tham số; khi menu mở, giữ chuột luôn thả (các script game có thể khóa lại khi bấm Esc/Tab).</summary>
        void LateUpdate()
        {
            if (!open) return;
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible) Cursor.visible = true;
        }

        /// <summary>Không nhận tham số; nối sự kiện của mọi điều khiển trong prefab (một lần) theo component và mã dòng.</summary>
        void Bind()
        {
            if (gearButton != null) gearButton.onClick.AddListener(OpenInternal);
            if (closeButton != null) closeButton.onClick.AddListener(() => CloseInternal(true));
            if (doneButton != null) doneButton.onClick.AddListener(() => CloseInternal(true));
            if (resetButton != null) resetButton.onClick.AddListener(OnResetTab);
            if (confirmNo != null) confirmNo.onClick.AddListener(HideConfirm);
            if (confirmYes != null) confirmYes.onClick.AddListener(() => { Action a = confirmAction; HideConfirm(); if (a != null) a(); });
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                if (tabButtons[i] != null) tabButtons[i].onClick.AddListener(() => { AudioHub.Player.Play(AudioCues.UiClick); ShowTab(index); });
            }
            foreach (SettingsSliderRow r in GetComponentsInChildren<SettingsSliderRow>(true)) WireSlider(r);
            foreach (SettingsToggleRow r in GetComponentsInChildren<SettingsToggleRow>(true)) WireToggle(r);
            foreach (SettingsSelectorRow r in GetComponentsInChildren<SettingsSelectorRow>(true)) selectors[r.id] = r;
            foreach (SettingsActionRow r in GetComponentsInChildren<SettingsActionRow>(true)) WireAction(r);
            foreach (SettingsKeyRow r in GetComponentsInChildren<SettingsKeyRow>(true))
            {
                SettingsKeyRow row = r;
                keyRows.Add(row);
                if (row.button != null) row.button.onClick.AddListener(() => { AudioHub.Player.Play(AudioCues.UiClick); BeginListen(row.action); });
            }
            if (versionText != null) versionText.text = string.IsNullOrEmpty(Application.version) ? "Bản thử" : Application.version;
        }

        /// <summary>Nhận dòng thanh trượt; ghi nhớ và nối thao tác kéo vào đúng cài đặt theo mã dòng.</summary>
        void WireSlider(SettingsSliderRow row)
        {
            sliders[row.id] = row;
            switch (row.id)
            {
                case SettingsRowIds.MasterVolume: row.Listen(v => GameSettingsRuntime.Model.SetVolume(v)); break;
                case SettingsRowIds.MusicVolume: row.Listen(v => ExtraSettings.SetBus(AudioBus.Music, v)); break;
                case SettingsRowIds.SfxVolume: row.Listen(v => { ExtraSettings.SetBus(AudioBus.Sfx, v); ExtraSettings.SetBus(AudioBus.Ui, v); }); break;
                case SettingsRowIds.AmbienceVolume: row.Listen(v => ExtraSettings.SetBus(AudioBus.Ambience, v)); break;
                case SettingsRowIds.VoiceVolume: row.Listen(v => ExtraSettings.SetBus(AudioBus.Voice, v)); break;
                case SettingsRowIds.Sensitivity: row.Listen(v => GameSettingsRuntime.Model.SetMouseSensitivity(v)); break;
            }
        }

        /// <summary>Nhận dòng công tắc; ghi nhớ và nối thao tác bật/tắt vào đúng cài đặt theo mã dòng.</summary>
        void WireToggle(SettingsToggleRow row)
        {
            toggles[row.id] = row;
            switch (row.id)
            {
                case SettingsRowIds.Fullscreen: row.Listen(v => { AudioHub.Player.Play(AudioCues.UiClick); GameSettingsRuntime.Model.SetFullscreen(v); }); break;
                case SettingsRowIds.VSync: row.Listen(v => { AudioHub.Player.Play(AudioCues.UiClick); ExtraSettings.VSync = v; }); break;
                case SettingsRowIds.InvertY: row.Listen(v => { AudioHub.Player.Play(AudioCues.UiClick); ExtraSettings.InvertY = v; }); break;
            }
        }

        /// <summary>Nhận dòng hành động; ghi nhớ và nối nút bấm: về menu chính, khôi phục tất cả, thoát game (đều hỏi xác nhận).</summary>
        void WireAction(SettingsActionRow row)
        {
            actions[row.id] = row;
            if (row.button == null) return;
            switch (row.id)
            {
                case SettingsRowIds.BackToMenu:
                    row.button.onClick.AddListener(() => { AudioHub.Player.Play(AudioCues.UiClick);
                        ShowConfirm("Về menu chính?", "Tiến độ nhiệm vụ hiện tại sẽ không được lưu.", "Về menu chính", false, GoMainMenu); });
                    break;
                case SettingsRowIds.ResetAll:
                    row.button.onClick.AddListener(() => { AudioHub.Player.Play(AudioCues.UiClick);
                        ShowConfirm("Khôi phục mặc định?", "Toàn bộ cài đặt sẽ trở về như lúc mới cài game.", "Khôi phục", false,
                            () => { ExtraSettings.ResetAll(); RefreshAll(); Toast("Đã khôi phục mọi cài đặt về mặc định.", true); }); });
                    break;
                case SettingsRowIds.QuitGame:
                    row.button.onClick.AddListener(() => { AudioHub.Player.Play(AudioCues.UiClick);
                        ShowConfirm("Thoát game?", "Bạn có chắc muốn đóng The Return không?", "Thoát game", true, QuitGame); });
                    break;
            }
        }

        /// <summary>Không nhận tham số; mở cửa sổ, đặt các dòng theo giá trị hiện tại và thả chuột.</summary>
        void OpenInternal()
        {
            if (open || menuRoot == null) return;
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            if (restoreRoutine != null) { StopCoroutine(restoreRoutine); restoreRoutine = null; }
            prevLock = Cursor.lockState;
            prevVisible = Cursor.visible;
            open = true;
            menuRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UiFontApplier fonts = GetComponent<UiFontApplier>();
            if (fonts != null) fonts.Apply();
            HideConfirm();
            RefreshAll();
            ShowTab(tab);
            RefreshGear();
        }

        /// <summary>Nhận cờ trả chuột; ẩn cửa sổ, hủy gán phím dở và (nếu cần) trả chuột về trạng thái trước ở khung hình sau.</summary>
        void CloseInternal(bool restoreCursor)
        {
            if (!open) return;
            open = false;
            listening = null;
            if (menuRoot != null) menuRoot.SetActive(false);
            RefreshGear();
            if (restoreCursor && isActiveAndEnabled) restoreRoutine = StartCoroutine(RestoreCursor());
        }

        /// <summary>Không nhận tham số; chờ một khung hình (để script khác xử lý Esc xong) rồi trả chuột như trước khi mở.</summary>
        IEnumerator RestoreCursor()
        {
            yield return null;
            restoreRoutine = null;
            if (open) yield break;
            Cursor.lockState = prevLock;
            Cursor.visible = prevLock == CursorLockMode.Locked ? false : prevVisible;
        }

        /// <summary>Không nhận tham số; hiện nút bánh răng chỉ ở các scene trong gearScenes và khi menu đang đóng.</summary>
        void RefreshGear()
        {
            if (gearRoot == null) return;
            bool wanted = Array.IndexOf(gearScenes, SceneManager.GetActiveScene().name) >= 0;
            gearRoot.SetActive(wanted && !open);
        }

        // ─────────────── Đặt giá trị các dòng ───────────────

        /// <summary>Không nhận tham số; đặt lại mọi dòng theo cài đặt hiện tại (khi mở và sau khi khôi phục mặc định).</summary>
        void RefreshAll()
        {
            listening = null;
            RefreshSliders();
            RefreshToggles();
            RefreshSelectors();
            RefreshKeys();
            SettingsActionRow back;
            if (actions.TryGetValue(SettingsRowIds.BackToMenu, out back))
                back.gameObject.SetActive(SceneManager.GetActiveScene().name != MainMenuScene);
        }

        /// <summary>Không nhận tham số; đặt khoảng và giá trị của mọi thanh trượt (không bắn sự kiện).</summary>
        void RefreshSliders()
        {
            GameSettingsData d = GameSettingsRuntime.Model.Current;
            foreach (KeyValuePair<string, SettingsSliderRow> pair in sliders)
            {
                float min = 0f, max = 1f, v;
                switch (pair.Key)
                {
                    case SettingsRowIds.MasterVolume: v = d.Volume; break;
                    case SettingsRowIds.MusicVolume: v = ExtraSettings.GetBus(AudioBus.Music); break;
                    case SettingsRowIds.SfxVolume: v = ExtraSettings.GetBus(AudioBus.Sfx); break;
                    case SettingsRowIds.AmbienceVolume: v = ExtraSettings.GetBus(AudioBus.Ambience); break;
                    case SettingsRowIds.VoiceVolume: v = ExtraSettings.GetBus(AudioBus.Voice); break;
                    case SettingsRowIds.Sensitivity: min = GameSettingsData.MinSensitivity; max = GameSettingsData.MaxSensitivity; v = d.MouseSensitivity; break;
                    default: continue;
                }
                pair.Value.Set(min, max, v);
            }
        }

        /// <summary>Không nhận tham số; đặt trạng thái của mọi công tắc (không bắn sự kiện).</summary>
        void RefreshToggles()
        {
            foreach (KeyValuePair<string, SettingsToggleRow> pair in toggles)
            {
                switch (pair.Key)
                {
                    case SettingsRowIds.Fullscreen: pair.Value.Set(GameSettingsRuntime.Model.Current.Fullscreen); break;
                    case SettingsRowIds.VSync: pair.Value.Set(ExtraSettings.VSync); break;
                    case SettingsRowIds.InvertY: pair.Value.Set(ExtraSettings.InvertY); break;
                }
            }
        }

        /// <summary>Không nhận tham số; đặt danh sách và lựa chọn hiện tại cho độ phân giải, giới hạn FPS và chất lượng.</summary>
        void RefreshSelectors()
        {
            SettingsSelectorRow row;
            if (selectors.TryGetValue(SettingsRowIds.Resolution, out row))
            {
                resolutions = BuildResolutions();
                string[] names = new string[resolutions.Count];
                int index = 0;
                var current = new Vector2Int(Screen.width, Screen.height);
                for (int i = 0; i < resolutions.Count; i++)
                {
                    names[i] = resolutions[i].x + " × " + resolutions[i].y;
                    if (resolutions[i] == current) index = i;
                }
                List<Vector2Int> list = resolutions;
                row.Setup(names, index, i => ExtraSettings.SetResolution(list[i].x, list[i].y));
            }
            if (selectors.TryGetValue(SettingsRowIds.FpsCap, out row))
            {
                int[] options = ExtraSettings.FpsOptions;
                string[] names = new string[options.Length];
                int index = 0;
                for (int i = 0; i < options.Length; i++)
                {
                    names[i] = options[i] > 0 ? options[i] + " FPS" : "Không giới hạn";
                    if (options[i] == ExtraSettings.FpsCap) index = i;
                }
                row.Setup(names, index, i => ExtraSettings.FpsCap = options[i]);
            }
            if (selectors.TryGetValue(SettingsRowIds.MicDevice, out row))
            {
                // T-055: chọn micro thu âm (rỗng = mặc định hệ thống). Bỏ các mục «Default...» và «No Device» của Vivox (đã có mục mặc định riêng; «No Device» sẽ tắt hẳn mic).
                List<string> found = MicDeviceSettings.AvailableNames();
                found.RemoveAll(n => n.StartsWith("Default", StringComparison.OrdinalIgnoreCase) || string.Equals(n, "No Device", StringComparison.OrdinalIgnoreCase));
                List<string> full = new List<string> { "" };
                full.AddRange(found);
                string[] shown = new string[full.Count];
                for (int i = 0; i < full.Count; i++) shown[i] = i == 0 ? "Mặc định của hệ thống" : ShortenName(full[i], 60);
                int micIndex = Mathf.Max(0, MicDeviceSettings.FindMatch(full, MicDeviceSettings.Preferred, 1));
                row.Setup(shown, micIndex, i =>
                {
                    MicDeviceSettings.Preferred = full[i];
                    Toast(i == 0 ? "Micro: dùng mặc định của hệ thống." : "Đã chọn micro: " + shown[i], true);
                });
            }
            if (selectors.TryGetValue(SettingsRowIds.Quality, out row))
            {
                string[] raw = QualitySettings.names;
                string[] names = new string[raw.Length];
                for (int i = 0; i < raw.Length; i++) names[i] = ExtraSettings.QualityLabel(raw[i]);
                row.Setup(names, ExtraSettings.Quality, i => ExtraSettings.Quality = i);
            }
        }

        /// <summary>Nhận tên và độ dài tối đa; trả tên đã cắt gọn bằng dấu … nếu quá dài (chỉ để hiển thị).</summary>
        static string ShortenName(string name, int max)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= max) return name;
            return name.Substring(0, max - 1).TrimEnd() + "…";
        }

        /// <summary>Không nhận tham số; trả danh sách độ phân giải không trùng của màn hình, tăng dần, luôn gồm độ phân giải hiện tại.</summary>
        List<Vector2Int> BuildResolutions()
        {
            var list = new List<Vector2Int>();
            foreach (Resolution r in Screen.resolutions)
            {
                if (r.height < 600) continue;
                var v = new Vector2Int(r.width, r.height);
                if (!list.Contains(v)) list.Add(v);
            }
            var cur = new Vector2Int(Screen.width, Screen.height);
            if (!list.Contains(cur)) list.Add(cur);
            list.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return list;
        }

        /// <summary>Không nhận tham số; vẽ lại chữ và màu mọi ô phím theo GameKeys.</summary>
        void RefreshKeys()
        {
            foreach (SettingsKeyRow row in keyRows)
            {
                bool wait = listening.HasValue && listening.Value == row.action;
                if (row.label != null)
                {
                    row.label.text = wait ? "Nhấn phím..." : GameKeys.DisplayName(GameKeys.Get(row.action));
                    row.label.color = wait ? keyListeningText : keyNormalText;
                }
                if (row.button != null && row.button.targetGraphic != null) row.button.targetGraphic.color = wait ? keyListeningColor : keyNormalColor;
            }
        }

        // ─────────────── Tab ───────────────

        /// <summary>Nhận chỉ số tab; hiện đúng trang, tô tab đang chọn, đổi nút Khôi phục theo tab và hủy việc gán phím dở.</summary>
        void ShowTab(int index)
        {
            tab = Mathf.Clamp(index, 0, pages.Length - 1);
            CancelListening();
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null) pages[i].SetActive(i == tab);
                bool on = i == tab;
                if (i < tabButtons.Length && tabButtons[i] != null && tabButtons[i].targetGraphic != null)
                    tabButtons[i].targetGraphic.color = on ? tabActiveBackground : tabInactiveBackground;
                if (i < tabLabels.Length && tabLabels[i] != null)
                {
                    tabLabels[i].color = on ? tabActiveText : tabInactiveText;
                    tabLabels[i].fontStyle = on ? FontStyle.Bold : FontStyle.Normal;
                }
                if (i < tabIndicators.Length && tabIndicators[i] != null) tabIndicators[i].enabled = on;
            }
            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(tab != 3);
                if (resetLabel != null) resetLabel.text = tab == 0 ? "Khôi phục âm thanh" : tab == 1 ? "Khôi phục đồ họa" : "Khôi phục điều khiển";
            }
            if (pages[tab] != null)
            {
                ScrollRect scroll = pages[tab].GetComponent<ScrollRect>();
                if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>Không nhận tham số; khôi phục mặc định riêng cho tab đang mở rồi đặt lại các dòng.</summary>
        void OnResetTab()
        {
            AudioHub.Player.Play(AudioCues.UiClick);
            if (tab == 0) ExtraSettings.ResetAudio();
            else if (tab == 1) ExtraSettings.ResetGraphics();
            else if (tab == 2) ExtraSettings.ResetControls();
            RefreshAll();
            Toast("Đã khôi phục mặc định.", true);
        }

        // ─────────────── Đổi phím ───────────────

        /// <summary>Nhận hành động; chuyển ô phím sang trạng thái chờ nhấn phím mới (hủy ô đang chờ trước đó).</summary>
        void BeginListen(GameAction action)
        {
            listening = action;
            RefreshKeys();
            Toast("Nhấn phím mới cho «" + GameKeys.Label(action) + "». Esc để hủy.", true);
        }

        /// <summary>Không nhận tham số; thoát trạng thái chờ phím và vẽ lại tên phím.</summary>
        void CancelListening()
        {
            if (!listening.HasValue) return;
            listening = null;
            RefreshKeys();
        }

        /// <summary>Nhận bàn phím; đang chờ gán thì lấy phím vừa bấm: Esc hủy, phím bị cấm báo lỗi, còn lại gán (hoán đổi nếu trùng).</summary>
        void UpdateListening(Keyboard kb)
        {
            if (kb.escapeKey.wasPressedThisFrame) { CancelListening(); Toast("Đã hủy.", true); return; }
            foreach (var key in kb.allKeys)
            {
                if (!key.wasPressedThisFrame) continue;
                GameAction action = listening.Value;
                GameAction? swapped;
                bool ok = GameKeys.Set(action, key.keyCode, out swapped);
                listening = null;
                RefreshKeys();
                if (!ok) { Toast("Không thể gán phím " + GameKeys.DisplayName(key.keyCode) + " (phím dành riêng).", false); return; }
                Toast(swapped.HasValue
                    ? "«" + GameKeys.Label(action) + "» = " + GameKeys.DisplayName(key.keyCode) + ". Đã đổi phím với «" + GameKeys.Label(swapped.Value) + "»."
                    : "«" + GameKeys.Label(action) + "» = " + GameKeys.DisplayName(key.keyCode) + ".", true);
                return;
            }
        }

        // ─────────────── Hộp xác nhận, thông báo, thoát ───────────────

        /// <summary>Nhận tiêu đề, nội dung, chữ nút, cờ nguy hiểm (nút đỏ) và hành động; hiện hộp xác nhận, chỉ chạy hành động khi bấm nút xác nhận.</summary>
        void ShowConfirm(string title, string body, string yesText, bool danger, Action onYes)
        {
            if (confirmRoot == null) { if (onYes != null) onYes(); return; }
            if (confirmTitle != null) confirmTitle.text = title;
            if (confirmBody != null) confirmBody.text = body;
            if (confirmYesLabel != null) { confirmYesLabel.text = yesText; confirmYesLabel.color = danger ? confirmDangerText : confirmPrimaryText; }
            if (confirmYes != null && confirmYes.targetGraphic != null) confirmYes.targetGraphic.color = danger ? confirmDangerColor : confirmPrimaryColor;
            confirmAction = onYes;
            confirmRoot.SetActive(true);
            confirmRoot.transform.SetAsLastSibling();
        }

        /// <summary>Không nhận tham số; ẩn hộp xác nhận và bỏ hành động đang chờ.</summary>
        void HideConfirm()
        {
            confirmAction = null;
            if (confirmRoot != null) confirmRoot.SetActive(false);
        }

        /// <summary>Nhận nội dung và cờ thành công; hiện thông báo ở chân trang trong vài giây (xanh = ổn, đỏ = lỗi).</summary>
        void Toast(string message, bool good)
        {
            if (toast == null) return;
            toast.text = message;
            toast.color = good ? toastGoodColor : toastBadColor;
            toastUntil = Time.unscaledTime + 3.5f;
        }

        /// <summary>Không nhận tham số; đóng menu rồi về Menu chính qua SessionExit (đang online thì rời phòng và dọn mạng trước).</summary>
        void GoMainMenu()
        {
            CloseInternal(false);
            SessionExit.ReturnToMenu(MainMenuScene);
        }

        /// <summary>Không nhận tham số; thoát game (trong Editor thì dừng Play Mode).</summary>
        static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
