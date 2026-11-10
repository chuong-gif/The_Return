/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Cô nói» khi chơi online (T-037). Nhiệm vụ này chấm theo VỊ TRÍ và THỜI GIAN do host đo,
 * nên người chơi chỉ có một lệnh: làm lại hành lang. Lệnh KHÔNG mang ô người gửi (host lấy ô từ kết nối, HostAuthority.Submit) nên không ai
 * giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Retry (hàm tạo lệnh).
 */
namespace TheReturn.TeacherSaysNet
{
    /// <summary>Loại lệnh của nhiệm vụ Cô nói.</summary>
    public enum TeacherSaysCommandKind
    {
        /// <summary>Làm lại từ đầu hành lang (giữ nguyên đề).</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class TeacherSaysCommand
    {
        /// <summary>Loại lệnh.</summary>
        public TeacherSaysCommandKind Kind { get; }

        TeacherSaysCommand(TeacherSaysCommandKind kind)
        {
            Kind = kind;
        }

        /// <summary>Không nhận tham số; trả lệnh làm lại (host chỉ chấp nhận khi nhiệm vụ đã bắt đầu và chưa thắng).</summary>
        public static TeacherSaysCommand Retry()
        {
            return new TeacherSaysCommand(TeacherSaysCommandKind.Retry);
        }
    }
}
