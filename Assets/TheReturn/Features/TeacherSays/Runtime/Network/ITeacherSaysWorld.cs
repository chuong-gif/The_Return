/*
 * Mục đích: Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-037): từng ô đang đi hay đứng yên, và đang đứng trong vùng
 * của đoạn nào. Lớp Unity trên host cài đặt từ VỊ TRÍ NHÂN VẬT mà host thấy (OnlineAvatar.transform.position); test cài đặt giả.
 * Máy khách KHÔNG tự khai vị trí hay trạng thái di chuyển.
 * Hàm: Sample; IsMoving; IsInZone.
 */
namespace TheReturn.TeacherSaysNet
{
    /// <summary>Nguồn sự thật vật lý của nhiệm vụ Cô nói, do host cung cấp.</summary>
    public interface ITeacherSaysWorld
    {
        /// <summary>
        /// Nhận số giây từ lần gọi trước; host cập nhật phép đo chuyển động (tốc độ đi ngang từng người so với ngưỡng). Luật gọi đúng một lần
        /// mỗi Tick, kể cả trước khi nhiệm vụ bắt đầu, để mốc vị trí luôn mới.
        /// </summary>
        void Sample(float deltaSeconds);

        /// <summary>Nhận ô người chơi; trả true nếu ô đó đang di chuyển nhanh hơn ngưỡng ở lần Sample gần nhất.</summary>
        bool IsMoving(int slot);

        /// <summary>Nhận ô người chơi và chỉ số đoạn 0-4; trả true nếu ô đó đang đứng trong hộp vùng của đoạn ấy (theo vị trí host thấy).</summary>
        bool IsInZone(int slot, int segment);
    }
}
