/*
 * Mục đích: Các cài đặt người chơi ngoài GameSettingsModel: đồ họa (chất lượng, độ phân giải, VSync, giới hạn FPS), âm lượng theo kênh
 * (nhạc, hiệu ứng, môi trường, giọng nói) và đảo trục nhìn dọc. Lưu bằng PlayerPrefs, tự áp dụng khi chạy BẤT KỲ scene nào (không cần object trong scene).
 * Luật: chưa từng chỉnh thì KHÔNG đụng vào giá trị mặc định của project/scene; chỉnh rồi thì ghi đè và nhớ giữa các lần chơi.
 * Âm lượng tổng và toàn màn hình vẫn do GameSettingsRuntime.Model giữ; phím tắt do GameKeys giữ.
 * Hàm: Boot; OnSceneLoaded; Quality; VSync; FpsCap; SetResolution; InvertY; GetBus; SetBus; ResetAudio; ResetGraphics; ResetControls; ResetAll; QualityLabel.
 */
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn
{
    /// <summary>Cài đặt đồ họa, kênh âm lượng và điều khiển bổ sung, lưu bằng PlayerPrefs.</summary>
    public static class ExtraSettings
    {
        const string P = "TheReturn.Set.";

        /// <summary>Các mức giới hạn khung hình để chọn; 0 = không giới hạn.</summary>
        public static readonly int[] FpsOptions = { 30, 60, 90, 120, 144, 0 };

        /// <summary>Các kênh âm lượng hiển thị trong menu (Giao diện đi chung với Hiệu ứng).</summary>
        static readonly float[] FallbackBus = { 1f, 1f, 0.6f, 1f, 0.7f };

        static bool captured;
        static int defaultQuality;
        static bool defaultVsync;
        static float[] busDefault;

        /// <summary>Không nhận tham số; xóa cờ đã nạp để lần chạy Play sau nạp lại (an toàn khi tắt Domain Reload).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { captured = false; busDefault = null; }

        /// <summary>Không nhận tham số; chạy trước scene đầu: nhớ mặc định của project rồi áp dụng giá trị đã lưu.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            CaptureDefaults();
            ApplyGraphics();
            PrototypePartyController.InvertLookY = InvertY;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>Nhận scene vừa nạp và chế độ nạp; nhớ mặc định kênh âm của scene rồi áp dụng giá trị đã lưu.</summary>
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CaptureBuses();
            ApplyBuses();
        }

        /// <summary>Không nhận tham số; nhớ chất lượng và VSync mặc định của project một lần.</summary>
        static void CaptureDefaults()
        {
            if (captured) return;
            captured = true;
            defaultQuality = QualitySettings.GetQualityLevel();
            defaultVsync = QualitySettings.vSyncCount > 0;
        }

        /// <summary>Không nhận tham số; nhớ hệ số kênh âm mặc định của AudioService trong scene (một lần, trước khi bị ghi đè).</summary>
        static void CaptureBuses()
        {
            if (busDefault != null) return;
            AudioService s = AudioService.Instance;
            if (s == null || s.busVolume == null) return;
            busDefault = (float[])s.busVolume.Clone();
        }

        /// <summary>Không nhận tham số; áp dụng chất lượng, VSync, giới hạn FPS và độ phân giải đã lưu (chỉ phần người chơi đã chỉnh).</summary>
        static void ApplyGraphics()
        {
            if (PlayerPrefs.HasKey(P + "Quality")) QualitySettings.SetQualityLevel(Quality, true);
            ApplyVsyncAndFps();
            if (!Application.isEditor && PlayerPrefs.HasKey(P + "ResW") && PlayerPrefs.HasKey(P + "ResH"))
                Screen.SetResolution(PlayerPrefs.GetInt(P + "ResW"), PlayerPrefs.GetInt(P + "ResH"), Screen.fullScreenMode);
        }

        /// <summary>Không nhận tham số; áp dụng VSync và giới hạn FPS (đổi chất lượng làm Unity đặt lại VSync nên gọi lại sau đó).</summary>
        static void ApplyVsyncAndFps()
        {
            if (PlayerPrefs.HasKey(P + "VSync")) QualitySettings.vSyncCount = PlayerPrefs.GetInt(P + "VSync") != 0 ? 1 : 0;
            if (PlayerPrefs.HasKey(P + "Fps")) { int f = PlayerPrefs.GetInt(P + "Fps"); Application.targetFrameRate = f > 0 ? f : -1; }
        }

        /// <summary>Không nhận tham số; ghi hệ số kênh âm đã lưu vào AudioService đang sống (nếu có).</summary>
        static void ApplyBuses()
        {
            AudioService s = AudioService.Instance;
            if (s == null) return;
            foreach (AudioBus bus in (AudioBus[])System.Enum.GetValues(typeof(AudioBus)))
                if (PlayerPrefs.HasKey(BusKey(bus))) s.SetBusVolume(bus, PlayerPrefs.GetFloat(BusKey(bus)));
        }

        /// <summary>Chỉ số mức chất lượng hiện tại (theo QualitySettings.names); đặt thì áp dụng và lưu.</summary>
        public static int Quality
        {
            get
            {
                CaptureDefaults();
                int max = Mathf.Max(0, QualitySettings.names.Length - 1);
                return Mathf.Clamp(PlayerPrefs.GetInt(P + "Quality", defaultQuality), 0, max);
            }
            set
            {
                CaptureDefaults();
                PlayerPrefs.SetInt(P + "Quality", Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1)));
                QualitySettings.SetQualityLevel(Quality, true);
                ApplyVsyncAndFps();
                PlayerPrefs.Save();
            }
        }

        /// <summary>Đồng bộ dọc bật/tắt; chưa chỉnh thì theo mặc định của project. Đặt thì áp dụng và lưu.</summary>
        public static bool VSync
        {
            get
            {
                CaptureDefaults();
                return PlayerPrefs.HasKey(P + "VSync") ? PlayerPrefs.GetInt(P + "VSync") != 0 : QualitySettings.vSyncCount > 0;
            }
            set
            {
                PlayerPrefs.SetInt(P + "VSync", value ? 1 : 0);
                QualitySettings.vSyncCount = value ? 1 : 0;
                PlayerPrefs.Save();
            }
        }

        /// <summary>Giới hạn khung hình (0 = không giới hạn); đặt thì áp dụng và lưu. Khi VSync bật, Unity bỏ qua giới hạn này.</summary>
        public static int FpsCap
        {
            get { return PlayerPrefs.GetInt(P + "Fps", 0); }
            set
            {
                PlayerPrefs.SetInt(P + "Fps", Mathf.Max(0, value));
                Application.targetFrameRate = value > 0 ? value : -1;
                PlayerPrefs.Save();
            }
        }

        /// <summary>Nhận rộng và cao (px); lưu và đổi độ phân giải theo chế độ cửa sổ hiện tại.</summary>
        public static void SetResolution(int width, int height)
        {
            if (width < 320 || height < 240) return;
            PlayerPrefs.SetInt(P + "ResW", width);
            PlayerPrefs.SetInt(P + "ResH", height);
            PlayerPrefs.Save();
            Screen.SetResolution(width, height, Screen.fullScreenMode);
        }

        /// <summary>Đảo trục nhìn lên/xuống; đặt thì áp dụng vào PrototypePartyController và lưu.</summary>
        public static bool InvertY
        {
            get { return PlayerPrefs.GetInt(P + "InvertY", 0) != 0; }
            set
            {
                PlayerPrefs.SetInt(P + "InvertY", value ? 1 : 0);
                PrototypePartyController.InvertLookY = value;
                PlayerPrefs.Save();
            }
        }

        /// <summary>Nhận kênh; trả khóa PlayerPrefs của kênh.</summary>
        static string BusKey(AudioBus bus) { return P + "Bus." + bus; }

        /// <summary>Nhận kênh; trả hệ số mặc định (của scene nếu đã biết, không thì bảng dự phòng khớp AudioService).</summary>
        static float DefaultBus(AudioBus bus)
        {
            float[] table = busDefault ?? FallbackBus;
            int i = (int)bus;
            return i >= 0 && i < table.Length ? table[i] : 1f;
        }

        /// <summary>Nhận kênh; trả hệ số hiện tại 0..1 (giá trị đã lưu, không thì giá trị đang chạy của scene, không thì mặc định).</summary>
        public static float GetBus(AudioBus bus)
        {
            if (PlayerPrefs.HasKey(BusKey(bus))) return Mathf.Clamp01(PlayerPrefs.GetFloat(BusKey(bus)));
            AudioService s = AudioService.Instance;
            return s != null ? s.GetBusVolume(bus) : DefaultBus(bus);
        }

        /// <summary>Nhận kênh và giá trị 0..1; lưu và áp dụng vào AudioService đang sống (nếu có).</summary>
        public static void SetBus(AudioBus bus, float value)
        {
            CaptureBuses();
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BusKey(bus), value);
            AudioService s = AudioService.Instance;
            if (s != null) s.SetBusVolume(bus, value);
            PlayerPrefs.Save();
        }

        /// <summary>Không nhận tham số; đưa âm lượng tổng và mọi kênh về mặc định.</summary>
        public static void ResetAudio()
        {
            MicDeviceSettings.Reset();
            AudioService s = AudioService.Instance;
            foreach (AudioBus bus in (AudioBus[])System.Enum.GetValues(typeof(AudioBus)))
            {
                PlayerPrefs.DeleteKey(BusKey(bus));
                if (s != null) s.SetBusVolume(bus, DefaultBus(bus));
            }
            GameSettingsRuntime.Model.SetVolume(GameSettingsData.DefaultVolume);
            PlayerPrefs.Save();
        }

        /// <summary>Không nhận tham số; đưa chất lượng, VSync, giới hạn FPS, độ phân giải và toàn màn hình về mặc định.</summary>
        public static void ResetGraphics()
        {
            CaptureDefaults();
            PlayerPrefs.DeleteKey(P + "Quality");
            PlayerPrefs.DeleteKey(P + "VSync");
            PlayerPrefs.DeleteKey(P + "Fps");
            PlayerPrefs.DeleteKey(P + "ResW");
            PlayerPrefs.DeleteKey(P + "ResH");
            QualitySettings.SetQualityLevel(defaultQuality, true);
            QualitySettings.vSyncCount = defaultVsync ? 1 : 0;
            Application.targetFrameRate = -1;
            GameSettingsRuntime.Model.SetFullscreen(GameSettingsData.DefaultFullscreen);
            Resolution native = Screen.currentResolution;
            if (!Application.isEditor) Screen.SetResolution(native.width, native.height, Screen.fullScreenMode);
            PlayerPrefs.Save();
        }

        /// <summary>Không nhận tham số; đưa độ nhạy chuột, đảo trục nhìn và mọi phím về mặc định.</summary>
        public static void ResetControls()
        {
            GameSettingsRuntime.Model.SetMouseSensitivity(GameSettingsData.DefaultSensitivity);
            InvertY = false;
            GameKeys.ResetAll();
        }

        /// <summary>Không nhận tham số; đưa toàn bộ cài đặt (âm thanh, đồ họa, điều khiển) về mặc định.</summary>
        public static void ResetAll()
        {
            ResetAudio();
            ResetGraphics();
            ResetControls();
        }

        /// <summary>Nhận tên mức chất lượng gốc; trả tên tiếng Việt cho các tên quen thuộc, còn lại giữ nguyên.</summary>
        public static string QualityLabel(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "very low": return "Rất thấp";
                case "low": return "Thấp";
                case "medium": return "Trung bình";
                case "high": return "Cao";
                case "very high": return "Rất cao";
                case "ultra": return "Cực cao";
                case "mobile": return "Di động";
                case "pc": return "Máy tính";
                default: return name;
            }
        }
    }
}
