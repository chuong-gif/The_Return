/*
 * Mục đích: Tự tạo AudioService khi game chạy (T-018) để không phải thêm object vào scene nào; sống xuyên scene.
 * Hàm: Boot (RuntimeInitializeOnLoadMethod, trước khi nạp scene) nạp bảng cue từ Resources/AudioCueBank_Main và tạo AudioService.
 * Thay bảng cue/clip: sửa asset Features/GameAudio/Resources/AudioCueBank_Main.asset, không sửa code.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Khởi tạo AudioService một lần cho cả phiên chạy.</summary>
    public static class AudioBootstrap
    {
        /// <summary>Không nhận tham số; tạo AudioService nếu chưa có và gán bảng cue. Bảng thiếu thì ghi cảnh báo và vẫn tạo (im lặng).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (AudioService.Instance != null) return;
            var go = new GameObject("AudioService");
            Object.DontDestroyOnLoad(go);
            var svc = go.AddComponent<AudioService>();
            svc.bank = Resources.Load<AudioCueBank>("AudioCueBank_Main");
            if (svc.bank == null) Debug.LogWarning("AudioBootstrap: không thấy Resources/AudioCueBank_Main — game sẽ không có tiếng.");
        }
    }
}
