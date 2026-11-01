/*
 * Mục đích: Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-031): ô nào đứng đủ gần một ghế để ngồi.
 * Lớp Unity trên host cài đặt từ vị trí nhân vật (khoảng cách tới điểm ngồi); test cài đặt giả. Client không tự khai vị trí.
 * Hàm: IsNearSeat.
 */
namespace TheReturn.AttendanceNet
{
    /// <summary>Nguồn sự thật vật lý của nhiệm vụ điểm danh, do host cung cấp.</summary>
    public interface IAttendanceWorld
    {
        /// <summary>Nhận ô người chơi và chỉ số ghế (0-23); trả true nếu ô đó đứng đủ gần ghế để ngồi.</summary>
        bool IsNearSeat(int slot, int seat);
    }
}
