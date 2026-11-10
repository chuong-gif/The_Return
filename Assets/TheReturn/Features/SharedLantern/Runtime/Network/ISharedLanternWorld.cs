/*
 * Mục đích: Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-037): ai đứng gần biển/bảng nhập, biển nào nằm trong vùng sáng
 * của đèn lồng, ai đang trong vùng sáng, khoảng cách giữa hai người, cả đội đã tới cổng chưa. Lớp Unity trên host cài đặt từ vị trí nhân vật
 * (OnlineAvatar) và vị trí/bán kính đèn lồng; test cài đặt giả. Client không tự khai vị trí.
 * Hàm: IsNearSign; IsSignInLight; IsInLight; DistanceBetween; IsNearPanel; AllAtGate.
 */
namespace TheReturn.LanternNet
{
    /// <summary>Nguồn sự thật vật lý của nhiệm vụ Đèn lồng chung, do host cung cấp.</summary>
    public interface ISharedLanternWorld
    {
        /// <summary>Nhận ô người chơi và chỉ số biển (0-3); trả true nếu ô đó đứng đủ gần biển để đọc.</summary>
        bool IsNearSign(int slot, int sign);

        /// <summary>Nhận chỉ số biển (0-3); trả true nếu biển đang nằm trong vùng sáng của đèn lồng (đọc được).</summary>
        bool IsSignInLight(int sign);

        /// <summary>Nhận ô người chơi; trả true nếu người đó đang trong vùng sáng của đèn lồng (người cầm đèn luôn được luật coi là trong vùng sáng).</summary>
        bool IsInLight(int slot);

        /// <summary>Nhận hai ô người chơi; trả khoảng cách giữa họ theo mét (dùng cho tầm trao đèn). Không biết thì trả số rất lớn.</summary>
        double DistanceBetween(int slotA, int slotB);

        /// <summary>Nhận ô người chơi; trả true nếu đứng đủ gần bảng nhập ký hiệu cuối hành lang.</summary>
        bool IsNearPanel(int slot);

        /// <summary>Trả true nếu cả đội đã ở cổng cuối hành lang.</summary>
        bool AllAtGate();
    }
}
