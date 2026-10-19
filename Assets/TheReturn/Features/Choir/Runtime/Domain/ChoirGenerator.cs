/*
 * Mục đích: Sinh đề Hợp xướng theo seed (T-006d). Dùng bộ sinh số riêng (xorshift) để cùng seed cho cùng đề trên mọi máy,
 * không phụ thuộc UnityEngine.Random hay System.Random (cần cho đồng bộ mạng sau này).
 * Ràng buộc đề: 6 nốt; hai nốt liền nhau khác ký hiệu (nên khác bục); dùng ít nhất 3 loại ký hiệu; bảng ký hiệu -> bục là hoán vị theo seed.
 * Hàm: Generate; Next (nội bộ).
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề Hợp xướng.</summary>
    public static class ChoirGenerator
    {
        /// <summary>Số nốt trong mỗi bản nhạc.</summary>
        public const int NoteCount = 6;

        /// <summary>Nhận số người (2-4) và seed; trả đề thỏa mọi ràng buộc ở đầu file. Cùng đầu vào luôn cho cùng đề.</summary>
        public static ChoirRound Generate(int playerCount, int seed)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            uint state = (uint)seed * 2654435761u + 0x9E3779B9u;
            if (state == 0) state = 0x6D2B79F5u;

            // Bảng ký hiệu -> bục: Fisher-Yates trên hoán vị 0..3.
            int[] symbolToPad = { 0, 1, 2, 3 };
            for (int i = ChoirRound.PadCount - 1; i > 0; i--)
            {
                int j = (int)(Next(ref state) % (uint)(i + 1));
                int t = symbolToPad[i]; symbolToPad[i] = symbolToPad[j]; symbolToPad[j] = t;
            }

            // Dãy ký hiệu: thử lại tới khi đủ ràng buộc (xác suất đạt cao nên vòng lặp ngắn; có chặn trên an toàn).
            int[] seq = new int[NoteCount];
            for (int attempt = 0; attempt < 1000; attempt++)
            {
                seq[0] = (int)(Next(ref state) % ChoirRound.PadCount);
                for (int k = 1; k < NoteCount; k++)
                {
                    // chọn ký hiệu khác ký hiệu liền trước
                    int s = (int)(Next(ref state) % (ChoirRound.PadCount - 1));
                    if (s >= seq[k - 1]) s++;
                    seq[k] = s;
                }
                var seen = new bool[ChoirRound.PadCount];
                int distinct = 0;
                foreach (int s in seq)
                    if (!seen[s]) { seen[s] = true; distinct++; }
                if (distinct >= 3) break;
            }
            // Nếu hết lượt thử (gần như không xảy ra), đặt dãy cố định hợp lệ: 0 1 2 3 0 1.
            int check = 0;
            var chk = new bool[ChoirRound.PadCount];
            foreach (int s in seq) if (!chk[s]) { chk[s] = true; check++; }
            if (check < 3) seq = new[] { 0, 1, 2, 3, 0, 1 };

            return new ChoirRound(playerCount, seed, seq, symbolToPad);
        }

        /// <summary>Nhận trạng thái bộ sinh (tham chiếu); cập nhật bằng xorshift32 và trả số ngẫu nhiên không dấu kế tiếp.</summary>
        static uint Next(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
