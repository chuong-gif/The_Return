/*
 * Mục đích: Cầu nối nhỏ, không phụ thuộc mạng: RoomRoster ghi tên đã chốt của máy này trong phòng, hệ thoại đọc để đặt tên Vivox trùng tên trong danh sách phòng
 * (nhờ vậy ghép được người trong phòng với người trong kênh thoại).
 */
namespace TheReturn.Network.Room
{
    /// <summary>Tên của máy này trong phòng hiện tại (rỗng nếu chưa vào phòng).</summary>
    public static class RoomIdentity
    {
        public static string LocalName = "";
    }
}
