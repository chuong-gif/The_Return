/*
 * Mục đích: Giao diện chung để MissionRegistry điều khiển một nhiệm vụ mà không biết nhiệm vụ cụ thể (T-003).
 * Thuần C#, không phụ thuộc Unity; adapter thật nằm ở Runtime/Missions (assembly SchoolFloor).
 * Thành viên: AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
namespace TheReturn
{
    /// <summary>Một nhiệm vụ gắn với đúng một khu (phòng/hành lang) trên map.</summary>
    public interface IMissionAdapter
    {
        /// <summary>ID khu của nhiệm vụ, trùng tên object trong scene (ví dụ Phong_1, Hanh_Lang_1).</summary>
        string AreaId { get; }

        /// <summary>True khi nhiệm vụ đang chạy.</summary>
        bool Active { get; }

        /// <summary>True khi luật nhiệm vụ báo đã giải xong.</summary>
        bool Solved { get; }

        /// <summary>True khi đã giải xong VÀ an toàn để kết thúc phiên (ví dụ không còn bảng điều khiển đang mở).</summary>
        bool CanHandOff { get; }

        /// <summary>Không nhận tham số; bắt đầu (hoặc tiếp tục) nhiệm vụ.</summary>
        void Begin();

        /// <summary>Nhận cờ reset; kết thúc phiên. reset=false giữ trạng thái đã giải (hoàn thành), reset=true làm sạch để thử lại.</summary>
        void Stop(bool reset);
    }
}
