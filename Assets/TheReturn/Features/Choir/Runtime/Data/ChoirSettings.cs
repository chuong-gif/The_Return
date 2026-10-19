/*
 * Mục đích: Asset chỉnh được trong Inspector cho nhiệm vụ Phong_6 «Hợp xướng» (T-006d2).
 * Chủ dự án yêu cầu khoảng thời gian giữa hai nốt (mặc định 2 giây khi online) chỉnh được vì lúc phát triển một máy
 * chỉ đổi nhân vật bằng phím 1-4. ChoirPrototype đọc lại asset này mỗi frame nên có thể chỉnh ngay khi đang chơi thử.
 * Thành viên: stepWindowSeconds, switchExtraSeconds, fixedSeed, text hướng dẫn; ToRules; ApplyDevPreset; ApplyOnlinePreset.
 */
using UnityEngine;

namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Choir Settings", fileName = "ChoirSettings")]
    public sealed class ChoirSettings : ScriptableObject
    {
        [Header("Thời gian giữa hai nốt")]
        [Tooltip("Số giây tối đa giữa hai nốt liên tiếp. 0 = không giới hạn (chế độ thử). Online gợi ý 2 giây.")]
        [Min(0f)] public float stepWindowSeconds = 5f;
        [Tooltip("Số giây cộng thêm mỗi lần đổi vai (phím 1-4) khi đang dở dãy; tổng cộng thêm mỗi nốt không quá stepWindowSeconds.")]
        [Min(0f)] public float switchExtraSeconds = 1.5f;

        [Header("Đề")]
        [Tooltip("0 = đề ngẫu nhiên mỗi lần bắt đầu; khác 0 = luôn cùng một đề (để test).")]
        public int fixedSeed;

        [Header("Chữ hiển thị")]
        [TextArea] public string instructions = "Bản nhạc trên bảng dùng ký hiệu A-D. Mỗi người chỉ đọc được thẻ nhạc của mình. Đứng gần bục và bấm E đúng thứ tự.";
        public string success = "Hợp xướng hoàn thành!";
        public string wrong = "Sai bục - bản nhạc quay về nốt đầu.";
        public string tooLate = "Quá chậm - bản nhạc quay về nốt đầu.";

        /// <summary>Không nhận tham số; trả quy tắc thời gian cho Domain từ giá trị Inspector.</summary>
        public ChoirRules ToRules() { return new ChoirRules(stepWindowSeconds, switchExtraSeconds); }

        /// <summary>Không nhận tham số; đặt giá trị phát triển một máy (5 giây, +1,5 giây mỗi lần đổi vai).</summary>
        [ContextMenu("Preset: phát triển một máy (5s, +1.5s)")]
        public void ApplyDevPreset() { stepWindowSeconds = 5f; switchExtraSeconds = 1.5f; }

        /// <summary>Không nhận tham số; đặt giá trị online (2 giây, không cộng thêm).</summary>
        [ContextMenu("Preset: online (2s, +0)")]
        public void ApplyOnlinePreset() { stepWindowSeconds = 2f; switchExtraSeconds = 0f; }
    }
}
