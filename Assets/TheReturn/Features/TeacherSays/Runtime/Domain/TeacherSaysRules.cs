/*
 * Mục đích: Thông số chỉnh được của nhiệm vụ Hanh_Lang_5 «Cô nói» (T-006c): cửa sổ thời gian khi lệnh là BƯỚC TỚI, thời gian phải
 * đứng yên, số cảnh cáo tối đa. Giai đoạn phát triển một máy đổi vai bằng phím 1-4 nên cửa sổ Bước dài hơn thiết kế online
 * và được cộng thêm mỗi lần đổi vai (cùng tinh thần ChoirRules). Giá trị mặc định do AI chọn, chủ dự án chỉnh sau.
 * Thuần C#; sau này Data/Presentation đọc các giá trị từ một asset.
 * Thành viên: StepWindowSeconds, SwitchExtraSeconds, StandSeconds, MaxWarnings, MaxExtraPerStep, HasStepLimit, Default, Online.
 */
using System;

namespace TheReturn
{
    /// <summary>Quy tắc thời gian và cảnh cáo của Cô nói.</summary>
    public sealed class TeacherSaysRules
    {
        /// <summary>Số giây cả đội có để vào vùng đoạn kế khi lệnh thật là BƯỚC TỚI. 0 = KHÔNG giới hạn (chế độ thử).</summary>
        public double StepWindowSeconds { get; }

        /// <summary>Số giây cộng thêm mỗi lần đổi vai khi đang trong cửa sổ Bước (chỉ có tác dụng khi có giới hạn).</summary>
        public double SwitchExtraSeconds { get; }

        /// <summary>Số giây cả đội phải đứng yên khi lệnh thật là ĐỨNG YÊN.</summary>
        public double StandSeconds { get; }

        /// <summary>Số cảnh cáo khiến cả đội về đầu hành lang (tối thiểu 1).</summary>
        public int MaxWarnings { get; }

        /// <summary>Tổng giây cộng thêm tối đa cho một cửa sổ Bước (bằng StepWindowSeconds) để không câu giờ bằng cách đổi vai liên tục.</summary>
        public double MaxExtraPerStep => StepWindowSeconds;

        /// <summary>True khi cửa sổ Bước có giới hạn thời gian.</summary>
        public bool HasStepLimit => StepWindowSeconds > 0;

        /// <summary>
        /// Nhận giây cửa sổ Bước (≥ 0, 0 = không giới hạn), giây cộng thêm mỗi lần đổi vai (≥ 0), giây đứng yên (&gt; 0) và số cảnh cáo tối đa (≥ 1).
        /// Mặc định giai đoạn phát triển: 20 giây, +3 giây, 5 giây, 3 cảnh cáo.
        /// </summary>
        public TeacherSaysRules(double stepWindowSeconds = 20.0, double switchExtraSeconds = 3.0, double standSeconds = 5.0, int maxWarnings = 3)
        {
            if (stepWindowSeconds < 0) throw new ArgumentOutOfRangeException(nameof(stepWindowSeconds));
            if (switchExtraSeconds < 0) throw new ArgumentOutOfRangeException(nameof(switchExtraSeconds));
            if (standSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(standSeconds));
            if (maxWarnings < 1) throw new ArgumentOutOfRangeException(nameof(maxWarnings));
            StepWindowSeconds = stepWindowSeconds;
            SwitchExtraSeconds = switchExtraSeconds;
            StandSeconds = standSeconds;
            MaxWarnings = maxWarnings;
        }

        /// <summary>Trả quy tắc mặc định cho giai đoạn phát triển một máy (Bước 20 giây, +3 giây mỗi lần đổi vai, Đứng 5 giây, 3 cảnh cáo).</summary>
        public static TeacherSaysRules Default { get { return new TeacherSaysRules(); } }

        /// <summary>Trả quy tắc gợi ý khi chơi online mỗi người một máy (Bước 8 giây, không cộng thêm, Đứng 5 giây, 3 cảnh cáo; đúng thiết kế duyệt).</summary>
        public static TeacherSaysRules Online { get { return new TeacherSaysRules(8.0, 0.0, 5.0, 3); } }
    }
}
