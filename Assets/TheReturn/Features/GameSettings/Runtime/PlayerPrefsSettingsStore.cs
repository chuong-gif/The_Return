/*
 * Mục đích: Lưu và nạp cài đặt người chơi bằng PlayerPrefs (T-008). Chưa lưu lần nào thì trả cài đặt mặc định.
 * Hàm: Load; Save.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Nơi lưu cài đặt bằng PlayerPrefs.</summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public const string VolumeKey = "TheReturn.Settings.Volume";
        public const string SensitivityKey = "TheReturn.Settings.MouseSensitivity";
        public const string FullscreenKey = "TheReturn.Settings.Fullscreen";

        /// <summary>Không nhận tham số; trả cài đặt đã lưu, mục chưa lưu lấy giá trị mặc định.</summary>
        public GameSettingsData Load()
        {
            return new GameSettingsData(
                PlayerPrefs.GetFloat(VolumeKey, GameSettingsData.DefaultVolume),
                PlayerPrefs.GetFloat(SensitivityKey, GameSettingsData.DefaultSensitivity),
                PlayerPrefs.GetInt(FullscreenKey, GameSettingsData.DefaultFullscreen ? 1 : 0) != 0);
        }

        /// <summary>Nhận cài đặt (không null); ghi ba giá trị vào PlayerPrefs.</summary>
        public void Save(GameSettingsData data)
        {
            if (data == null) throw new System.ArgumentNullException(nameof(data));
            PlayerPrefs.SetFloat(VolumeKey, data.Volume);
            PlayerPrefs.SetFloat(SensitivityKey, data.MouseSensitivity);
            PlayerPrefs.SetInt(FullscreenKey, data.Fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
