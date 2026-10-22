/*
 * Mục đích: Áp dụng cài đặt người chơi vào Unity (T-008): âm lượng (AudioListener.volume), độ nhạy chuột (PrototypePartyController.SensitivityScale)
 * và toàn màn hình (Screen.fullScreenMode). Tự áp dụng giá trị đã lưu khi chạy BẤT KỲ scene nào (không cần đặt object trong scene);
 * giao diện Settings chỉ gọi Model.Set... và Apply chạy theo sự kiện Changed.
 * Hàm: Model (mô hình dùng chung); Boot (tự chạy trước scene đầu); Apply.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Áp dụng cài đặt người chơi vào Unity.</summary>
    public static class GameSettingsRuntime
    {
        static GameSettingsModel model;

        /// <summary>Mô hình cài đặt dùng chung (lưu bằng PlayerPrefs); mỗi thay đổi tự áp dụng vào Unity.</summary>
        public static GameSettingsModel Model
        {
            get
            {
                if (model == null)
                {
                    model = new GameSettingsModel(new PlayerPrefsSettingsStore());
                    model.Changed += Apply;
                }
                return model;
            }
        }

        /// <summary>Không nhận tham số; áp dụng cài đặt đã lưu ngay trước khi scene đầu tiên nạp.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot() { Apply(Model.Current); }

        /// <summary>Nhận cài đặt (không null); đặt âm lượng, độ nhạy chuột và chế độ toàn màn hình.</summary>
        public static void Apply(GameSettingsData data)
        {
            if (data == null) return;
            AudioListener.volume = data.Volume;
            PrototypePartyController.SensitivityScale = data.MouseSensitivity;
            Screen.fullScreenMode = data.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
    }
}
