/*
 * Mục đích: Hợp đồng giữa khung mạng (HostAuthority) và luật của từng nhiệm vụ (T-024). Mỗi nhiệm vụ viết MỘT lớp luật
 * (bọc Domain/Session thuần C# đã có, ví dụ PlankSession) theo giao diện này; khung lo kiểm người gửi, số thứ tự lệnh,
 * ghi nhật ký, đánh số phiên bản và gửi trạng thái chung cùng thông tin riêng của từng người.
 * Hàm: Validate; Apply; BuildPublicState; BuildPrivateState.
 */
namespace TheReturn.Network.Framework
{
    /// <summary>
    /// Luật nhiệm vụ chạy trên HOST. TCommand: kiểu lệnh máy khách gửi; TPublic: trạng thái mọi người đều thấy;
    /// TPrivate: thông tin riêng của một người (dùng NoPrivate nếu nhiệm vụ không có).
    /// </summary>
    public interface IHostRules<TCommand, TPublic, TPrivate>
    {
        /// <summary>
        /// Kiểm lệnh, KHÔNG được đổi trạng thái. Nhận ô người gửi (host biết từ kết nối, không đọc từ nội dung lệnh
        /// nên người chơi không giả mạo được) và lệnh. Trả chấp nhận hoặc từ chối kèm lý do.
        /// </summary>
        RuleVerdict Validate(int senderSlot, TCommand command);

        /// <summary>Chạy lệnh vừa được Validate chấp nhận. Nhận ô người gửi và lệnh.</summary>
        void Apply(int senderSlot, TCommand command);

        /// <summary>
        /// Chụp trạng thái chung cho mọi người. Phải trả bản BẤT BIẾN hoặc bản sao: khung giữ lại và gửi
        /// cùng một bản cho nhiều người.
        /// </summary>
        TPublic BuildPublicState();

        /// <summary>Nhận ô người nhận; trả thông tin RIÊNG của đúng người đó, tuyệt đối không chứa thông tin riêng của người khác.</summary>
        TPrivate BuildPrivateState(int slot);
    }
}
