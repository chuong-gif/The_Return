/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Bảng cầu dao» khi chơi online (T-031). Lệnh KHÔNG mang ô người gửi: host lấy ô từ kết nối
 * (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Toggle; Retry (các hàm tạo lệnh).
 */
namespace TheReturn.BreakerNet
{
    /// <summary>Loại lệnh của nhiệm vụ cầu dao.</summary>
    public enum BreakerCommandKind
    {
        /// <summary>Bật/tắt một công tắc (chỉ chủ công tắc mới được).</summary>
        Toggle,
        /// <summary>Làm lại cùng đề (mọi công tắc về tắt).</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class BreakerCommand
    {
        /// <summary>Loại lệnh.</summary>
        public BreakerCommandKind Kind { get; }

        /// <summary>Chỉ có nghĩa với Toggle: chỉ số công tắc.</summary>
        public int Switch { get; }

        BreakerCommand(BreakerCommandKind kind, int sw)
        {
            Kind = kind;
            Switch = sw;
        }

        /// <summary>Nhận chỉ số công tắc; trả lệnh bật/tắt (host sẽ kiểm công tắc có hợp lệ và có phải của người gửi không).</summary>
        public static BreakerCommand Toggle(int sw)
        {
            return new BreakerCommand(BreakerCommandKind.Toggle, sw);
        }

        /// <summary>Lệnh làm lại cùng đề.</summary>
        public static BreakerCommand Retry()
        {
            return new BreakerCommand(BreakerCommandKind.Retry, 0);
        }
    }
}
