/*
 * Mục đích: Tên hiển thị của 6 ký hiệu «Đèn lồng chung» (T-006f) để biển, bảng nhập và HUD dùng chung. Dùng chữ A-F để chắc chắn
 * hiển thị được; chủ dự án có thể đổi sang mô hình 3D riêng sau.
 * Hàm: Name.
 */
namespace TheReturn
{
    /// <summary>Bảng tên ký hiệu dùng chung cho phần hiển thị.</summary>
    public static class LanternSymbols
    {
        static readonly string[] Names = { "A", "B", "C", "D", "E", "F" };

        /// <summary>Nhận chỉ số ký hiệu (0..5); trả tên chữ, hoặc dấu chấm hỏi nếu chưa biết/sai chỉ số.</summary>
        public static string Name(int symbol) { return symbol >= 0 && symbol < Names.Length ? Names[symbol] : "?"; }
    }
}
