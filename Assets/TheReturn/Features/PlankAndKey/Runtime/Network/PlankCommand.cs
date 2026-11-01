/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Cầu ván và thẻ từ» khi chơi online (T-027). Lệnh KHÔNG mang ô người gửi: host lấy ô từ
 * kết nối (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: PickUpCard; DropCard; UseLever; Retry (các hàm tạo lệnh).
 */
namespace TheReturn.PlankNet
{
    /// <summary>Loại lệnh của nhiệm vụ cầu ván.</summary>
    public enum PlankCommandKind
    {
        /// <summary>Nhặt thẻ từ đang nằm trên sàn.</summary>
        PickUpCard,
        /// <summary>Thả thẻ đang cầm xuống sàn hoặc lên ván.</summary>
        DropCard,
        /// <summary>Gạt cần để ván chạy sang bờ bên kia.</summary>
        UseLever,
        /// <summary>Làm lại cùng đề (phím R).</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class PlankCommand
    {
        /// <summary>Loại lệnh.</summary>
        public PlankCommandKind Kind { get; }

        /// <summary>Chỉ có nghĩa với UseLever: true là cần gạt ở bờ A.</summary>
        public bool SideA { get; }

        PlankCommand(PlankCommandKind kind, bool sideA)
        {
            Kind = kind;
            SideA = sideA;
        }

        /// <summary>Lệnh nhặt thẻ.</summary>
        public static PlankCommand PickUpCard()
        {
            return new PlankCommand(PlankCommandKind.PickUpCard, false);
        }

        /// <summary>Lệnh thả thẻ.</summary>
        public static PlankCommand DropCard()
        {
            return new PlankCommand(PlankCommandKind.DropCard, false);
        }

        /// <summary>Nhận bờ của cần gạt (true là bờ A); trả lệnh gạt cần.</summary>
        public static PlankCommand UseLever(bool sideA)
        {
            return new PlankCommand(PlankCommandKind.UseLever, sideA);
        }

        /// <summary>Lệnh làm lại cùng đề.</summary>
        public static PlankCommand Retry()
        {
            return new PlankCommand(PlankCommandKind.Retry, false);
        }
    }
}
