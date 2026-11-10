/*
 * Mục đích: Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-031): ô nào đứng đủ gần một công tắc để bấm.
 * Lớp Unity trên host cài đặt từ vị trí nhân vật; test cài đặt giả. Client không tự khai vị trí.
 * Hàm: IsNearSwitch.
 */
namespace TheReturn.BreakerNet
{
    /// <summary>Nguồn sự thật vật lý của nhiệm vụ cầu dao, do host cung cấp.</summary>
    public interface IBreakerWorld
    {
        /// <summary>Nhận ô người chơi và chỉ số công tắc; trả true nếu ô đó đứng đủ gần công tắc để bấm.</summary>
        bool IsNearSwitch(int slot, int sw);
    }
}
