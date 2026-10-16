/*
 * Mục đích: Thông số chỉnh được của nhiệm vụ Phong_6 «Hợp xướng» (T-006d). Chủ dự án yêu cầu khoảng thời gian giữa hai nốt
 * chỉnh được vì giai đoạn phát triển một máy, đổi nhân vật bằng phím 1-4 nên người chơi không kịp phản ứng.
 * Thuần C#; sau này Data/Presentation đọc các giá trị này từ một asset để chỉnh trong Inspector.
 * Thành viên: StepWindowSeconds, SwitchExtraSeconds, HasTimeLimit, MaxExtraPerStep, Default, Online.
 */
using System;

namespace TheReturn
{
    /// <summary>Quy tắc thời gian của Hợp xướng.</summary>
    public sealed class ChoirRules
    {
        /// <summary>Số giây tối đa giữa hai nốt liên tiếp. 0 = KHÔNG giới hạn thời gian (chế độ thử/phát triển).</summary>
        public double StepWindowSeconds { get; }

        /// <summary>Số giây cộng thêm mỗi lần đổi vai khi đang dở dãy (chỉ có tác dụng khi có giới hạn thời gian).</summary>
        public double SwitchExtraSeconds { get; }

        /// <summary>Tổng giây cộng thêm tối đa cho một nốt (bằng StepWindowSeconds) để không câu giờ bằng cách đổi vai liên tục.</summary>
        public double MaxExtraPerStep => StepWindowSeconds;

        /// <summary>True khi có giới hạn thời gian giữa hai nốt.</summary>
        public bool HasTimeLimit => StepWindowSeconds > 0;

        /// <summary>
        /// Nhận giây giữa hai nốt (≥ 0, 0 = không giới hạn) và giây cộng thêm mỗi lần đổi vai (≥ 0).
        /// Mặc định giai đoạn phát triển: 5 giây và 1,5 giây (chủ dự án chỉnh sau).
        /// </summary>
        public ChoirRules(double stepWindowSeconds = 5.0, double switchExtraSeconds = 1.5)
        {
            if (stepWindowSeconds < 0) throw new ArgumentOutOfRangeException(nameof(stepWindowSeconds));
            if (switchExtraSeconds < 0) throw new ArgumentOutOfRangeException(nameof(switchExtraSeconds));
            StepWindowSeconds = stepWindowSeconds;
            SwitchExtraSeconds = switchExtraSeconds;
        }

        /// <summary>Trả quy tắc mặc định cho giai đoạn phát triển một máy (5 giây, +1,5 giây mỗi lần đổi vai).</summary>
        public static ChoirRules Default { get { return new ChoirRules(); } }

        /// <summary>Trả quy tắc gợi ý khi chơi online mỗi người một máy (2 giây, không cộng thêm).</summary>
        public static ChoirRules Online { get { return new ChoirRules(2.0, 0.0); } }
    }
}
