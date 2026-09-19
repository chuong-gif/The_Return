/*
 * Mục đích: Quản lý hướng dẫn và manh mối qua asset, thay tọa độ theo biến thể.
 * Hàm: Packet ghép manh mối riêng cho một vai; Message diễn giải kết quả bước đi.
 */
using UnityEngine;
namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Navigation Exam Catalog")]
    public sealed class NavigationExamCatalog : ScriptableObject
    {
        public string title = "BÀI KIỂM TRA DẪN ĐƯỜNG";
        [TextArea(3, 6)] public string introduction = "Đưa quân từ {start} tới {goal}. Cột A–C từ trái sang phải, hàng 1–3 từ trên xuống. Mỗi bước đúng chuyển quyền cho vai tiếp theo. Trao đổi manh mối riêng và lấy đủ hai con dấu theo thứ tự.";
        [TextArea(2, 4)] public string[] clues =
        {
            "Ô {blocked1} bị khóa. Không được bước vào.",
            "Ô {blocked2} bị khóa. Không được bước vào.",
            "Ô {blocked3} bị khóa. Không được bước vào.",
            "Lấy SỔ LỚP tại {first} trước CHÌA KHÓA tại {second}. Cạnh {first} → {second} chỉ đi một chiều; không đi ngược."
        };
        public string success = "Đã vượt qua bài kiểm tra. Cửa đã mở; các nhiệm vụ trước được giữ.";
        public string controls = "E mở/đóng sa bàn • 1–4 đổi vai • WASD đi • R thử lại • F5 biến thể tiếp • Tab menu • THỬ MỘT MÁY";

        /// <summary>Nhận state và vai; ghép các manh mối của vai và thay token tọa độ, không tiết lộ gói vai khác.</summary>
        public string Packet(NavigationExamState state, int actor)
        {
            string text = "";
            for (int i = 0; i < 4; i++)
            {
                if (state.OwnerOf(i) != actor) continue;
                string value = clues != null && i < clues.Length ? clues[i] : "";
                text += "• " + (value ?? "") + "\n\n";
            }
            return text.Replace("{blocked1}", NavigationExamState.Code(state.Cell(3)))
                .Replace("{blocked2}", NavigationExamState.Code(state.Cell(7)))
                .Replace("{blocked3}", NavigationExamState.Code(state.Cell(2)))
                .Replace("{first}", NavigationExamState.Code(state.FirstStamp))
                .Replace("{second}", NavigationExamState.Code(state.SecondStamp));
        }

        /// <summary>Nhận mã bước; trả lời phản hồi giải thích vì sao bị chặn hoặc lượt đã chuyển.</summary>
        public string Message(ExamResult result)
        {
            switch (result)
            {
                case ExamResult.Moved: return "Bước hợp lệ. Đã chuyển lượt sang đồng đội.";
                case ExamResult.Solved: return success;
                case ExamResult.WrongTurn: return "Chưa tới lượt vai này. Hãy hướng dẫn đồng đội đang cầm quyền.";
                case ExamResult.Outside: return "Đó là mép sa bàn. Thử hướng khác; lượt vẫn giữ nguyên.";
                case ExamResult.Blocked: return "Ô này bị khóa. Kiểm tra manh mối; lượt vẫn giữ nguyên.";
                case ExamResult.OneWay: return "Cạnh này không cho đi ngược. Lượt vẫn giữ nguyên.";
                case ExamResult.StampOrder: return "Phải lấy SỔ LỚP trước CHÌA KHÓA.";
                case ExamResult.MissingStamps: return "Chưa đủ hai con dấu để tới đích.";
                default: return "Chỉ đi một ô ngang hoặc dọc.";
            }
        }
    }
}
