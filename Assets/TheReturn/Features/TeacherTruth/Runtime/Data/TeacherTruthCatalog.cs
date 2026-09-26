/*
 * Mục đích: Quản lý toàn bộ tiêu đề, nội quy, lời bốn giáo viên và phản hồi nhiệm vụ 6 trong Inspector.
 * Hàm: Announcement lấy thoại theo biến thể/kênh; Notebook tạo phụ đề riêng; Message đổi kết quả thành phản hồi.
 */
using System.Text;
using UnityEngine;

namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Teacher Truth Catalog")]
    public sealed class TeacherTruthCatalog : ScriptableObject
    {
        public string title = "GIÁO VIÊN NÓI THẬT / NÓI DỐI";
        [TextArea] public string introduction =
            "Bốn kênh giáo viên đang đưa chỉ dẫn mâu thuẫn. Đọc đèn MẤT ĐỒNG BỘ và nội quy: khi các kênh mâu thuẫn, dùng xác nhận thủ công.";
        [TextArea] public string priorEvidence =
            "BẢN GHI PHÒNG TRƯỚC: Khi các kênh thông báo mâu thuẫn,\nchuyển sang XÁC NHẬN THỦ CÔNG.";
        [TextArea] public string roomEvidence =
            "SƠ ĐỒ KÊNH 1–4: MẤT ĐỒNG BỘ.\nKhông kênh nào được dùng làm nguồn chân lý duy nhất.";
        [TextArea] public string exitRule =
            "NỘI QUY CỬA: Xác nhận đủ bốn kênh trong 3 giây.\nMọi thành viên phải tham gia.";
        [TextArea] public string success = "Xác nhận thủ công hoàn tất. Hệ thống bỏ qua bốn kênh lỗi và mở cổng thoát tầng.";
        public string controls = "WASD di chuyển • E nghe/xác nhận • 1–4 đổi vai • R thử lại • F5 đổi biến thể • Tab menu";
        [Tooltip("Ba bộ × bốn kênh, theo thứ tự biến thể rồi kênh.")]
        [TextArea] public string[] announcements =
        {
            "Kênh 1: Chỉ giáo viên số 1 nói thật. Tuyệt đối không chạm nút số 4.",
            "Kênh 2: Kênh 1 đã bị thay thế. Chỉ nút 2 và 4 là an toàn.",
            "Kênh 3: Không có chế độ thủ công. Hãy chờ chuông đổi tiết.",
            "Kênh 4: Tất cả bốn kênh đang đồng bộ hoàn toàn.",
            "Kênh 1: Cửa chỉ mở nếu bỏ qua kênh 2.",
            "Kênh 2: Không được dùng bất kỳ nút xác nhận nào.",
            "Kênh 3: Chỉ kênh 3 và kênh 4 đang hoạt động.",
            "Kênh 4: Đèn MẤT ĐỒNG BỘ là thông báo giả.",
            "Kênh 1: Kênh 4 luôn nói dối, hãy xác nhận riêng kênh 1.",
            "Kênh 2: Cả bốn giáo viên đều nói cùng một nội dung.",
            "Kênh 3: Chỉ cần một người đứng chờ trước cửa.",
            "Kênh 4: Chế độ thủ công đã bị vô hiệu hóa."
        };

        /// <summary>Nhận biến thể và kênh; trả lời thoại có thể chỉnh trong asset, hoặc thông báo thiếu cấu hình.</summary>
        public string Announcement(int variant, int channel)
        {
            int index = variant * TeacherTruthState.ChannelCount + channel;
            return announcements != null && index >= 0 && index < announcements.Length
                ? announcements[index]
                : "Kênh " + (channel + 1) + ": [Thiếu nội dung]";
        }

        /// <summary>Nhận state và vai; trả phụ đề chỉ gồm những kênh vai đó đã trực tiếp nghe cùng chứng cứ chung.</summary>
        public string Notebook(TeacherTruthState state, int actor)
        {
            var text = new StringBuilder();
            text.Append(priorEvidence).Append("\n\n").Append(roomEvidence).Append("\n\nPHỤ ĐỀ CỦA VAI ")
                .Append(actor + 1).Append("\n");
            for (int channel = 0; channel < TeacherTruthState.ChannelCount; channel++)
                text.Append(state.HasHeard(actor, channel)
                        ? Announcement(state.AnnouncementVariant, channel)
                        : "Kênh " + (channel + 1) + ": chưa nghe")
                    .Append("\n");
            return text.Append("\n").Append(exitRule).ToString();
        }

        /// <summary>Nhận kết quả xác nhận; trả phản hồi hướng dẫn, không đổi trạng thái.</summary>
        public string Message(ManualConfirmResult result)
        {
            switch (result)
            {
                case ManualConfirmResult.Confirmed: return "Đã chốt kênh. Các thành viên còn lại tiếp tục trong cửa sổ.";
                case ManualConfirmResult.AlreadyConfirmed: return "Kênh này đã được xác nhận trong lượt hiện tại.";
                case ManualConfirmResult.NeedEveryPlayer: return "Đã đủ bốn kênh nhưng chưa đủ thành viên tham gia. Bốn nút đã được đặt lại.";
                case ManualConfirmResult.Solved: return success;
                default: return "Tới gần tượng hoặc nút có nhãn để tương tác.";
            }
        }
    }
}
