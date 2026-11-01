/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Điểm danh» khi chơi online (T-031). Lệnh KHÔNG mang ô người gửi: host lấy ô từ kết nối
 * (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Sit; Stand; Retry (các hàm tạo lệnh).
 */
namespace TheReturn.AttendanceNet
{
    /// <summary>Loại lệnh của nhiệm vụ điểm danh.</summary>
    public enum AttendanceCommandKind
    {
        /// <summary>Ngồi vào một ghế (0-23).</summary>
        Sit,
        /// <summary>Đứng dậy khỏi ghế đang ngồi.</summary>
        Stand,
        /// <summary>Làm lại cùng đề (mọi người đứng dậy).</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class AttendanceCommand
    {
        /// <summary>Loại lệnh.</summary>
        public AttendanceCommandKind Kind { get; }

        /// <summary>Chỉ có nghĩa với Sit: chỉ số ghế 0-23.</summary>
        public int Seat { get; }

        AttendanceCommand(AttendanceCommandKind kind, int seat)
        {
            Kind = kind;
            Seat = seat;
        }

        /// <summary>Nhận chỉ số ghế; trả lệnh ngồi (host sẽ kiểm ghế có hợp lệ không).</summary>
        public static AttendanceCommand Sit(int seat)
        {
            return new AttendanceCommand(AttendanceCommandKind.Sit, seat);
        }

        /// <summary>Lệnh đứng dậy.</summary>
        public static AttendanceCommand Stand()
        {
            return new AttendanceCommand(AttendanceCommandKind.Stand, 0);
        }

        /// <summary>Lệnh làm lại cùng đề.</summary>
        public static AttendanceCommand Retry()
        {
            return new AttendanceCommand(AttendanceCommandKind.Retry, 0);
        }
    }
}
