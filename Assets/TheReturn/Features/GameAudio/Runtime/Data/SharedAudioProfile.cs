/*
 * Mục đích: BẢNG ÂM THANH DÙNG CHUNG của cả game (một asset duy nhất: Features/GameAudio/Resources/SharedAudioProfile.asset).
 * Chủ dự án kéo clip vào đây MỘT LẦN là cả 3 scene cùng dùng, không phải đặt lại ở từng scene.
 * Quy ước phạm vi âm thanh:
 *   1) TOÀN GAME (MainMenu + Sanh_Cho + MapSchoolF): âm giao diện — bấm nút, rê chuột qua nút, kéo thanh trượt, bật/tắt công tắc. Nằm ở asset này.
 *   2) SẢNH + MAP (Sanh_Cho + MapSchoolF): tiếng bước chân, tiếng nhảy. Cũng nằm ở asset này (MainMenu không có nhân vật nên không dùng).
 *   3) RIÊNG TỪNG SCENE: nhạc nền, âm môi trường, âm vật phẩm của nhiệm vụ... Nằm ở object âm thanh của scene đó
 *      (MainMenu/Sanh_Cho: Audio_Scene = SceneAudioSlots; MapSchoolF: Audio_Slots = GameAudioSlots).
 * Cách hoạt động: AudioBootstrap nhân bản bảng cue gốc rồi ghi đè các cue có clip ở đây ngay khi game chạy; bảng gốc AudioCueBank_Main không bị sửa.
 * Ô để TRỐNG = giữ âm mặc định (nếu có), hoặc im lặng.
 * Hàm: BuildBank (tạo bảng cue đã ghi đè từ bảng gốc).
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Asset chứa clip dùng chung giữa các scene.</summary>
    [CreateAssetMenu(fileName = "SharedAudioProfile", menuName = "The Return/Shared Audio Profile")]
    public sealed class SharedAudioProfile : ScriptableObject
    {
        [Header("1) TOÀN GAME — dùng chung MainMenu, Sanh_Cho, MapSchoolF  (âm giao diện)")]
        [Tooltip("Âm BẤM nút (mọi nút trong game, kể cả menu Cài đặt). Cue: ui.click")]
        public AudioClip uiClick;
        [Tooltip("Âm khi CHUỘT ĐI QUA một nút/công tắc/thanh trượt. Nên ngắn và nhẹ. Cue: ui.hover")]
        public AudioClip uiHover;
        [Range(0f, 1f)] public float uiHoverVolume = 0.5f;
        [Tooltip("Âm 'tích' khi KÉO thanh trượt (âm lượng, độ nhạy...). Phát lặp khi giá trị đổi. Cue: ui.slider")]
        public AudioClip uiSliderTick;
        [Range(0f, 1f)] public float uiSliderVolume = 0.5f;
        [Tooltip("Âm BẬT/TẮT công tắc. Để trống thì dùng âm bấm nút. Cue: ui.toggle")]
        public AudioClip uiToggle;
        [Tooltip("Âm khi ĐỔI sang nhân vật khác (phím 1–4). Cue: ui.switch_player")]
        public AudioClip uiSwitchPlayer;

        [Header("2) SẢNH + MAP — dùng chung Sanh_Cho, MapSchoolF  (nhân vật di chuyển)")]
        [Tooltip("Tiếng BƯỚC CHÂN. Kéo nhiều clip vào để game chọn ngẫu nhiên mỗi bước cho đỡ lặp. Cue: step.floor")]
        public AudioClip[] footsteps;
        [Tooltip("Độ TO của tiếng bước chân khi đã kéo clip riêng vào (0 = im, 1 = to nhất). Cue: step.floor")]
        [Range(0f, 1f)] public float footstepsVolume = 0.8f;
        [Tooltip("Tiếng NHẢY (phím Space). Cue: player.jump")]
        public AudioClip jump;
        [Tooltip("Độ TO của tiếng nhảy khi đã kéo clip riêng vào. Cue: player.jump")]
        [Range(0f, 1f)] public float jumpVolume = 1f;

        /// <summary>Nhận bảng cue gốc; trả BẢN NHÂN BẢN đã ghi đè các cue có clip trong asset này (gốc giữ nguyên). Gốc null thì trả null.</summary>
        public AudioCueBank BuildBank(AudioCueBank original)
        {
            if (original == null) return null;
            AudioCueBank copy = Instantiate(original);
            copy.hideFlags = HideFlags.DontSave;
            var list = new List<AudioCueBank.Entry>(copy.entries ?? new AudioCueBank.Entry[0]);
            GameAudioSlots.Override(list, AudioCues.UiClick, uiClick);
            GameAudioSlots.Override(list, AudioCues.UiHover, uiHover, 0.03f, uiHoverVolume, AudioBus.Ui);
            GameAudioSlots.Override(list, AudioCues.UiSlider, uiSliderTick, 0.04f, uiSliderVolume, AudioBus.Ui);
            GameAudioSlots.Override(list, AudioCues.UiToggle, uiToggle, 0.05f, -1f, AudioBus.Ui);
            GameAudioSlots.Override(list, AudioCues.UiSwitchPlayer, uiSwitchPlayer);
            GameAudioSlots.Override(list, AudioCues.Step, footsteps, 0.05f, footstepsVolume);
            GameAudioSlots.Override(list, AudioCues.Jump, jump, 0.2f, jumpVolume);
            copy.entries = list.ToArray();
            copy.Rebuild();
            return copy;
        }
    }
}
