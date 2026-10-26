/*
 * Mục đích: Luật chống phát chồng âm thanh (T-018): mỗi cue có thời gian nghỉ, và tổng số tiếng/giây bị giới hạn.
 * Hàm: TryAllow(cue, now, cooldown) quyết định có phát không; Reset xóa lịch sử. Thời gian do caller truyền (test/mạng xác định).
 */
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Quyết định có phát một cue ngay bây giờ không, dựa trên lần phát trước và mức tải.</summary>
    public sealed class AudioCueRules
    {
        readonly Dictionary<string, double> last = new Dictionary<string, double>();
        readonly Queue<double> recent = new Queue<double>();
        public int MaxPerSecond { get; }

        /// <summary>Nhận số tiếng tối đa mỗi giây (≥ 1).</summary>
        public AudioCueRules(int maxPerSecond = 16) { MaxPerSecond = maxPerSecond < 1 ? 1 : maxPerSecond; }

        /// <summary>Nhận mã cue, thời điểm hiện tại, thời gian nghỉ của cue (giây, ≥ 0); trả true và ghi nhận nếu được phát, false nếu quá gần lần trước hoặc quá tải. Mã rỗng luôn false.</summary>
        public bool TryAllow(string cue, double now, float cooldown)
        {
            if (string.IsNullOrEmpty(cue)) return false;
            while (recent.Count > 0 && now - recent.Peek() >= 1.0) recent.Dequeue();
            if (recent.Count >= MaxPerSecond) return false;
            double prev;
            if (cooldown > 0f && last.TryGetValue(cue, out prev) && now - prev < cooldown) return false;
            last[cue] = now;
            recent.Enqueue(now);
            return true;
        }

        /// <summary>Không nhận tham số; xóa lịch sử phát (khi chơi lại).</summary>
        public void Reset() { last.Clear(); recent.Clear(); }
    }
}
