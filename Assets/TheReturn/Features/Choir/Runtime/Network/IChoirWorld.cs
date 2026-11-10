/*
 * Mục đích: Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-037): ô nào đứng đủ gần một bục để bấm.
 * Lớp Unity trên host cài đặt từ vị trí nhân vật (OnlineAvatar); test cài đặt giả. Client không tự khai vị trí.
 * Hàm: IsNearPad.
 */
namespace TheReturn.ChoirNet
{
    /// <summary>Nguồn sự thật vật lý của nhiệm vụ Hợp xướng, do host cung cấp.</summary>
    public interface IChoirWorld
    {
        /// <summary>Nhận ô người chơi và chỉ số bục (0-3); trả true nếu ô đó đứng đủ gần bục để bấm.</summary>
        bool IsNearPad(int slot, int pad);
    }
}
