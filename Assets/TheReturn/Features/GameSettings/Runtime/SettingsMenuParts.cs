/*
 * Mục đích: Các thành phần nhỏ gắn lên từng DÒNG của prefab Cài đặt (Resources/PF_SettingsMenu) để SettingsMenu nối logic mà không phải dựng giao diện bằng code.
 * Mỗi thành phần chỉ giữ tham chiếu tới điều khiển của dòng đó và một mã (id); bố cục, màu, chữ, kích thước chỉnh tự do trong Inspector/Prefab Mode.
 * Kiểu: SettingsRowIds (mã dòng), SettingsSliderRow, SettingsToggleRow, SettingsSelectorRow, SettingsKeyRow, SettingsActionRow.
 */
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Mã các dòng cài đặt; SettingsMenu nối logic theo các mã này nên đổi tên object tùy ý nhưng giữ nguyên mã trong component.</summary>
    public static class SettingsRowIds
    {
        public const string MasterVolume = "audio.master";
        public const string MusicVolume = "audio.music";
        public const string SfxVolume = "audio.sfx";
        public const string AmbienceVolume = "audio.ambience";
        public const string VoiceVolume = "audio.voice";
        public const string MicDevice = "audio.mic";
        public const string Resolution = "gfx.resolution";
        public const string Fullscreen = "gfx.fullscreen";
        public const string VSync = "gfx.vsync";
        public const string FpsCap = "gfx.fps";
        public const string Quality = "gfx.quality";
        public const string Sensitivity = "ctl.sensitivity";
        public const string InvertY = "ctl.invertY";
        public const string BackToMenu = "sys.backToMenu";
        public const string ResetAll = "sys.resetAll";
        public const string QuitGame = "sys.quit";
        public const string VersionText = "sys.version";
    }

    /// <summary>Cách hiển thị số bên phải thanh trượt.</summary>
    public enum SliderFormat { Percent, Multiplier }

}
