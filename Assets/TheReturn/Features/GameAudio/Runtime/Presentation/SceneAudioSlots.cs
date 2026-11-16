/*
 * Mục đích: Object âm thanh RIÊNG của một scene đơn giản (MainMenu, Sanh_Cho) để chủ dự án kéo thả NHẠC NỀN và ÂM MÔI TRƯỜNG riêng của scene đó (Inspector).
 * Âm dùng chung nhiều scene (bấm nút, rê chuột, kéo thanh trượt, bước chân, nhảy) KHÔNG đặt ở đây mà ở asset SharedAudioProfile (ô «Âm dùng chung» bên dưới, bấm để mở).
 * Cách hoạt động: chạy sớm; nhân bản bảng cue đang dùng, ghi đè cue nhạc của scene rồi phát nhạc nền; rời scene thì trả bảng cũ (nhạc scene sau tự chuyển mượt).
 * Hàm: Awake (áp dụng clip); Start (phát nhạc/môi trường); OnDestroy (trả bảng gốc).
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Âm thanh riêng của scene: nhạc nền và âm môi trường.</summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class SceneAudioSlots : MonoBehaviour
    {
        [Header("ÂM DÙNG CHUNG (cả game) — bấm vào ô này để mở và chỉnh")]
        [Tooltip("Asset chứa âm giao diện (bấm/rê chuột/kéo thanh trượt) dùng cho cả 3 scene và bước chân/nhảy dùng cho Sanh_Cho + MapSchoolF. Chỉnh ở đó một lần là mọi scene đổi theo.")]
        public SharedAudioProfile sharedProfile;

        [Header("RIÊNG SCENE NÀY")]
        [Tooltip("Mã cue nhạc nền của scene này: music.menu (MainMenu) hoặc music.lobby (Sanh_Cho). Đổi mã nếu muốn hai scene dùng chung một bài (kéo cùng clip vào cả hai).")]
        public string musicCue = AudioCues.MusicMenu;
        [Tooltip("NHẠC NỀN của scene này (lặp). Để trống = scene im lặng (nhạc scene trước tắt dần).")]
        public AudioClip music;
        [Tooltip("Độ TO nhạc nền (0 = im, 1 = to nhất); còn chịu thêm thanh âm lượng Nhạc trong Cài đặt.")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;

        AudioCueBank original;
        AudioCueBank runtimeBank;

        void Awake()
        {
            var svc = AudioService.Instance;
            if (svc == null || svc.bank == null) return;
            original = svc.bank;
            runtimeBank = Instantiate(original);
            var list = new List<AudioCueBank.Entry>(runtimeBank.entries ?? new AudioCueBank.Entry[0]);
            GameAudioSlots.Override(list, musicCue, music, 0.05f, musicVolume, AudioBus.Music);
            runtimeBank.entries = list.ToArray();
            runtimeBank.Rebuild();
            svc.bank = runtimeBank;
        }

        /// <summary>Không nhận tham số; phát nhạc nền của scene (cue thiếu clip thì tắt nhạc cũ dần).</summary>
        void Start() { AudioHub.Player.PlayMusic(musicCue); }

        void OnDestroy()
        {
            var svc = AudioService.Instance;
            if (svc != null && runtimeBank != null && svc.bank == runtimeBank) svc.bank = original;
            if (runtimeBank != null) Destroy(runtimeBank);
            runtimeBank = null;
        }
    }
}
