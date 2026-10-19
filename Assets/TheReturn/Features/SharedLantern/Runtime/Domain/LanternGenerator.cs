/*
 * Mục đích: Sinh đề «Đèn lồng chung» (T-006f) theo hạt giống: chọn 4 ký hiệu khác nhau trong 6 và gán cho 4 biển. Thuần C#,
 * dùng bộ số ngẫu nhiên tự cài (không dùng System.Random) để cùng seed cho cùng đề trên mọi máy, phục vụ đồng bộ mạng sau này.
 * Hàm: Generate; Next.
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề xác định theo seed.</summary>
    public static class LanternGenerator
    {
        /// <summary>Nhận số người (2-4) và seed; trả đề có 4 ký hiệu khác nhau, cùng đầu vào luôn cho cùng kết quả.</summary>
        public static LanternRound Generate(int playerCount, int seed)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount), "Số người chơi phải từ 2 đến 4");
            uint state = unchecked((uint)seed * 747796405u + 2891336453u);
            var pool = new int[LanternRound.SymbolCount];
            for (int i = 0; i < pool.Length; i++) pool[i] = i;
            for (int i = pool.Length - 1; i > 0; i--)
            {
                int j = Next(ref state, i + 1);
                int tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            }
            var symbols = new int[LanternRound.SignCount];
            for (int i = 0; i < symbols.Length; i++) symbols[i] = pool[i];
            return new LanternRound(seed, playerCount, symbols);
        }

        /// <summary>Nhận trạng thái bộ số (cập nhật tại chỗ) và cận trên loại trừ; trả số nguyên trong [0, max) theo xorshift32.</summary>
        static int Next(ref uint state, int max)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)max);
        }
    }
}
