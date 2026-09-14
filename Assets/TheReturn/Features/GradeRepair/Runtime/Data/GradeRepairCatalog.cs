/*
 * Mục đích: Quản lý lời hướng dẫn, biến thể chứng từ và thông báo bằng asset Inspector.
 * Hàm: DocumentText dựng nội dung theo đề; ResultText đổi mã kết quả thành lời giải thích.
 */
using UnityEngine;
namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Grade Repair Catalog")]
    public sealed class GradeRepairCatalog : ScriptableObject
    {
        public string title = "SỬA BẢNG ĐIỂM";
        [TextArea(3, 6)] public string introduction = "Tra hồ sơ trên bốn bàn. Mỗi vai phụ trách một phần tài liệu. Đối chiếu mã học sinh rồi dùng hai phiếu bổ sung và biên bản chuyển điểm tại bảng sửa. Khi đủ chứng từ, nộp bảng có tổng 32, không ai dưới 6.";
        public string[] documentTitles = { "Sổ đối chiếu mã", "Phiếu bổ sung 01", "Phiếu bổ sung 02", "Biên bản nhập nhầm" };
        [TextArea(2, 5)] public string[] supplementVariants =
        {
            "Học sinh {code}: bài thực hành chưa được cộng. Bổ sung đúng 2 điểm vào hồ sơ này. Phiếu chỉ sử dụng một lần.",
            "Đã xác minh bài nộp muộn của {code}. Cho phép bổ sung 2 điểm; không áp dụng cho học sinh khác."
        };
        [TextArea(2, 5)] public string[] transferVariants =
        {
            "Có 1 điểm của {target} bị ghi vào {source}. Trừ 1 tại hồ sơ {source} và trả 1 cho {target}. Không tạo thêm điểm.",
            "Lỗi nhập liệu: {source} đang thừa 1 điểm thuộc về {target}. Chuyển lại đúng 1 điểm theo biên bản này."
        };
        public string success = "Bảng điểm đã được khôi phục. Cửa phòng giáo viên đã mở.";
        public string controls = "E xem/đóng • 1–4 đổi vai • WASD đi • R thử lại bảng • F5 hồ sơ mới • Tab menu • BẢN THỬ MỘT MÁY";
        public string wrongRecord = "Có chứng từ áp sai hồ sơ hoặc sai chiều chuyển. Đối chiếu mã; có thể hoàn tác.";
        public string boundsError = "Thao tác làm điểm ra ngoài 0–10. Kiểm tra thứ tự chuyển và bổ sung.";
        public string missingEvidence = "Chưa đủ hồ sơ được các vai phụ trách đọc. Kiểm tra bốn bàn chứng từ.";
        public string missingVouchers = "Chưa áp dụng đủ hai phiếu bổ sung và biên bản chuyển điểm.";

        /// <summary>Nhận đề và ID tài liệu; trả chữ có mã theo seed, dùng mẫu dự phòng nếu asset thiếu biến thể.</summary>
        public string DocumentText(GradeRepairRound round, int document)
        {
            if (document == 0)
            {
                string text = "SỔ ĐỐI CHIẾU — chỉ mã hồ sơ trên bảng được phép sửa.\n";
                for (int i = 0; i < 4; i++) text += "\nHồ sơ " + (char)('A' + i) + " ↔ " + round.Codes[i];
                return text + "\n\nChỉ nộp khi đủ chứng từ; tổng 32, từng hồ sơ từ 6 đến 10.";
            }
            int voucher = document - 1;
            if (voucher < 0 || voucher > 2) return "";
            string[] variants = voucher == 2 ? transferVariants : supplementVariants;
            string fallback = voucher == 2 ? "Chuyển 1 điểm từ {source} sang {target}." : "Bổ sung 2 điểm cho {code}.";
            int choice = (int)((uint)round.Seed % (uint)Mathf.Max(1, variants == null ? 0 : variants.Length));
            string template = variants == null || variants.Length == 0 ? fallback : variants[choice];
            if (string.IsNullOrWhiteSpace(template)) template = fallback;
            string target = round.Codes[round.CorrectTarget(voucher)];
            return template.Replace("{code}", target).Replace("{target}", target)
                .Replace("{source}", voucher == 2 ? round.Codes[round.CorrectSource(voucher)] : "");
        }

        /// <summary>Nhận mã kết quả; trả lời phản hồi, không sửa điểm hoặc tiết lộ đáp án cụ thể.</summary>
        public string ResultText(GradeResult result)
        {
            switch (result)
            {
                case GradeResult.Ok: return "Đã ghi thao tác. Có thể hoàn tác trước khi nộp.";
                case GradeResult.AlreadyUsed: return "Phiếu đã dùng; hoàn tác để lấy lại.";
                case GradeResult.OutOfRange: return boundsError;
                case GradeResult.MissingEvidence: return missingEvidence;
                case GradeResult.MissingVouchers: return missingVouchers;
                case GradeResult.WrongRecord: return wrongRecord;
                case GradeResult.Requirements: return "Bảng chưa đạt tổng 32 và từng điểm 6–10.";
                case GradeResult.Solved: return success;
                case GradeResult.NothingToUndo: return "Chưa có thao tác để hoàn tác.";
                default: return "Chọn hồ sơ hợp lệ; nguồn và đích chuyển điểm phải khác nhau.";
            }
        }
    }
}
