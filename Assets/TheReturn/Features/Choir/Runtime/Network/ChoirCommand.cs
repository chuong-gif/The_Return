/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Hợp xướng» khi chơi online (T-037). Lệnh KHÔNG mang ô người gửi: host lấy ô từ kết nối
 * (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Press; Retry (các hàm tạo lệnh).
 */
namespace TheReturn.ChoirNet
{
    /// <summary>Loại lệnh của nhiệm vụ Hợp xướng.</summary>
    public enum ChoirCommandKind
    {
        /// <summary>Bấm một bục (người bấm phải đứng gần bục, host kiểm).</summary>
        Press,
        /// <summary>Làm lại cùng đề (dãy về nốt đầu).</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class ChoirCommand
    {
        /// <summary>Loại lệnh.</summary>
        public ChoirCommandKind Kind { get; }

        /// <summary>Chỉ có nghĩa với Press: chỉ số bục 0-3.</summary>
        public int Pad { get; }

        ChoirCommand(ChoirCommandKind kind, int pad)
        {
            Kind = kind;
            Pad = pad;
        }

        /// <summary>Nhận chỉ số bục; trả lệnh bấm bục (host sẽ kiểm bục hợp lệ và người gửi đứng đủ gần).</summary>
        public static ChoirCommand Press(int pad)
        {
            return new ChoirCommand(ChoirCommandKind.Press, pad);
        }

        /// <summary>Trả lệnh làm lại cùng đề.</summary>
        public static ChoirCommand Retry()
        {
            return new ChoirCommand(ChoirCommandKind.Retry, 0);
        }
    }
}
