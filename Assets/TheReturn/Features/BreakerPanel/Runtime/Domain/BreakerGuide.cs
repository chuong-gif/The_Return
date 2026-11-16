/*
 * Mục đích: Soạn chữ HƯỚNG DẪN tiếng Việt cho «Bảng cầu dao» dễ hiểu (T-056), dùng chung cho bản một máy (sổ tay HUD) và bản online (khung HUD).
 * Dựa trên luật đề dễ: công tắc N bật đúng đèn N, đèn đầu tắt hết, đích = các đèn phải sáng. Từ trạng thái công tắc đang bật và đích, tính ra
 * «việc cần làm»: BẬT/TẮT công tắc nào, của vai nào. Thuần C#, không phụ thuộc Unity (chỉ tính chuỗi).
 * Hàm: Rules; Describe; ListNumbers; OwnedSwitches.
 */
using System.Text;

namespace TheReturn
{
    /// <summary>Bộ soạn chữ hướng dẫn cho nhiệm vụ cầu dao (chỉ đúng với đề do BreakerGenerator.Generate sinh).</summary>
    public static class BreakerGuide
    {
        /// <summary>Nhận giới hạn quá tải và số giây giữ khớp; trả đoạn «cách chơi» ngắn 4 dòng.</summary>
        public static string Rules(int overloadLimit, float holdSeconds)
        {
            return "CÁCH CHƠI: công tắc số mấy bật đèn số đó (đèn nằm ngay trên công tắc).\n" +
                   "Bật đúng các đèn có chấm XANH rồi giữ yên " + holdSeconds.ToString("0.#") + " giây là xong.\n" +
                   "Mỗi người chỉ bấm được công tắc của VAI mình (màu riêng): hãy gọi đồng đội bật phần còn lại.\n" +
                   "Đừng bật quá " + overloadLimit + " công tắc cùng lúc, cầu dao sẽ nhảy.";
        }

        /// <summary>
        /// Nhận số người, mặt nạ công tắc đang bật, mặt nạ đèn đích, giới hạn quá tải, vai của người xem (0..n-1), tỉ lệ giữ khớp 0..1 và cờ đã thắng;
        /// trả chữ trạng thái: đèn cần sáng, công tắc đang bật, công tắc của bạn và danh sách việc cần làm.
        /// </summary>
        public static string Describe(int playerCount, int onSwitches, int targetLamps, int overloadLimit, int self, float holdFraction, bool solved)
        {
            int size = playerCount + 2;
            var sb = new StringBuilder();
            if (solved)
            {
                sb.Append("ĐÃ ỔN ĐỊNH! Bảng cầu dao đã sửa xong.");
                return sb.ToString();
            }
            sb.Append("Đèn cần SÁNG (chấm xanh): ").Append(ListNumbers(targetLamps, size)).Append('\n');
            sb.Append("Công tắc đang bật: ").Append(ListNumbers(onSwitches, size)).Append("  (").Append(BreakerRound.CountBits(onSwitches)).Append('/').Append(overloadLimit).Append(")\n");
            sb.Append("Bạn là VAI ").Append(self + 1).Append(" - công tắc của bạn: ").Append(ListNumbers(OwnedSwitches(playerCount, self), size)).Append('\n');
            int todo = 0;
            var work = new StringBuilder();
            for (int s = 0; s < size; s++)
            {
                bool on = (onSwitches >> s & 1) == 1;
                bool want = (targetLamps >> s & 1) == 1;
                if (on == want) continue;
                int owner = s % playerCount;
                work.Append("  - ").Append(want ? "BẬT" : "TẮT").Append(" công tắc ").Append(s + 1)
                    .Append(owner == self ? " (của bạn)" : " (nhờ VAI " + (owner + 1) + ")").Append('\n');
                todo++;
            }
            if (todo == 0) sb.Append("ĐÃ KHỚP! Giữ yên, đừng bấm gì thêm.\n");
            else sb.Append("VIỆC CẦN LÀM:\n").Append(work);
            sb.Append("Giữ khớp: ").Append((holdFraction * 100f).ToString("0")).Append('%');
            return sb.ToString();
        }

        /// <summary>Nhận mặt nạ bit và số bit tối đa; trả danh sách số (bắt đầu từ 1) cách nhau dấu phẩy, hoặc «không có».</summary>
        public static string ListNumbers(int mask, int size)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                if ((mask >> i & 1) == 0) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(i + 1);
            }
            return sb.Length == 0 ? "không có" : sb.ToString();
        }

        /// <summary>Nhận số người và vai (0..n-1); trả mặt nạ các công tắc thuộc vai đó (công tắc i thuộc vai i % số người).</summary>
        public static int OwnedSwitches(int playerCount, int self)
        {
            int mask = 0;
            for (int s = 0; s < playerCount + 2; s++)
                if (s % playerCount == self) mask |= 1 << s;
            return mask;
        }
    }
}
