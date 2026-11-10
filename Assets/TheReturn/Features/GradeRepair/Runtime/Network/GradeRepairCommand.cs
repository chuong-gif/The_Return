/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Sửa bảng điểm» khi chơi online (T-036). Lệnh KHÔNG mang ô người gửi: host lấy ô từ kết nối
 * (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Read; Apply; Undo; Submit; Reset (các hàm tạo lệnh). Kèm IGradeRepairWorld (điều host quan sát được).
 */
namespace TheReturn.GradeRepairNet
{
    /// <summary>Loại lệnh của nhiệm vụ sửa bảng điểm.</summary>
    public enum GradeRepairCommandKind
    {
        /// <summary>Đọc một tài liệu (chỉ vai phụ trách, đứng gần bàn tài liệu).</summary>
        Read,
        /// <summary>Áp một phiếu lên bảng (đứng gần bảng).</summary>
        Apply,
        /// <summary>Hoàn tác thao tác cuối (đứng gần bảng).</summary>
        Undo,
        /// <summary>Nộp bảng (đứng gần bảng).</summary>
        Submit,
        /// <summary>Đặt lại điểm và phiếu, giữ tài liệu đã đọc (như phím R bản một máy).</summary>
        Reset
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class GradeRepairCommand
    {
        /// <summary>Loại lệnh.</summary>
        public GradeRepairCommandKind Kind { get; }

        /// <summary>Read: tài liệu 0-3. Apply: phiếu 0-2. Các lệnh khác: 0.</summary>
        public int Index { get; }

        /// <summary>Chỉ Apply: hồ sơ nguồn 0-3, hoặc -1 với phiếu bổ sung; các lệnh khác: 0.</summary>
        public int Source { get; }

        /// <summary>Chỉ Apply: hồ sơ đích 0-3; các lệnh khác: 0.</summary>
        public int Target { get; }

        GradeRepairCommand(GradeRepairCommandKind kind, int index, int source, int target)
        {
            Kind = kind;
            Index = index;
            Source = source;
            Target = target;
        }

        /// <summary>Nhận tài liệu 0-3; trả lệnh đọc tài liệu đó.</summary>
        public static GradeRepairCommand Read(int document)
        {
            return new GradeRepairCommand(GradeRepairCommandKind.Read, document, 0, 0);
        }

        /// <summary>Nhận phiếu 0-2, hồ sơ nguồn (-1 nếu phiếu bổ sung) và hồ sơ đích; trả lệnh áp phiếu.</summary>
        public static GradeRepairCommand Apply(int voucher, int source, int target)
        {
            return new GradeRepairCommand(GradeRepairCommandKind.Apply, voucher, source, target);
        }

        /// <summary>Lệnh hoàn tác thao tác cuối.</summary>
        public static GradeRepairCommand Undo()
        {
            return new GradeRepairCommand(GradeRepairCommandKind.Undo, 0, 0, 0);
        }

        /// <summary>Lệnh nộp bảng.</summary>
        public static GradeRepairCommand Submit()
        {
            return new GradeRepairCommand(GradeRepairCommandKind.Submit, 0, 0, 0);
        }

        /// <summary>Lệnh đặt lại bảng (giữ tài liệu đã đọc).</summary>
        public static GradeRepairCommand Reset()
        {
            return new GradeRepairCommand(GradeRepairCommandKind.Reset, 0, 0, 0);
        }
    }

    /// <summary>
    /// Điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết: ô nào đang đứng đủ gần bảng hoặc một bàn tài liệu.
    /// Lớp Unity trên host cài đặt từ vị trí nhân vật; test cài đặt giả. Máy khách không tự khai vị trí.
    /// </summary>
    public interface IGradeRepairWorld
    {
        /// <summary>Nhận ô người chơi; trả true nếu đứng đủ gần bảng sửa điểm để thao tác.</summary>
        bool IsNearBoard(int slot);

        /// <summary>Nhận ô người chơi và tài liệu 0-3; trả true nếu đứng đủ gần bàn tài liệu đó để đọc.</summary>
        bool IsNearDocument(int slot, int document);
    }
}
