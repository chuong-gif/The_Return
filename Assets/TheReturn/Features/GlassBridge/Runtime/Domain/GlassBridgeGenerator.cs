/*
 * Mục đích: Sinh đề «Cầu kính» theo seed (T-006e): mỗi hàng chọn ngẫu nhiên một ô an toàn, loại các dãy quá dễ đoán
 * (cả cầu cùng một bên, hoặc một bên liền nhau quá 3 hàng). Thuần C#.
 * Hàm: Generate; IsAcceptable.
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề cầu kính: cùng seed cho cùng đề.</summary>
    public static class GlassBridgeGenerator
    {
        const int MaxRun = 3;

        /// <summary>
        /// Nhận số người (2-4), seed và số hàng (mặc định 6, từ 3 tới 10); trả đề mới. Cả hai bên đều xuất hiện và không có
        /// quá 3 hàng liền cùng một bên. Ném ArgumentOutOfRangeException nếu tham số sai; InvalidOperationException nếu không
        /// sinh được (không xảy ra với tham số hợp lệ).
        /// </summary>
        public static GlassBridgeRound Generate(int playerCount, int seed, int rows = GlassBridgeRound.DefaultRows)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (rows < GlassBridgeRound.MinRows || rows > GlassBridgeRound.MaxRows) throw new ArgumentOutOfRangeException(nameof(rows));
            var rng = new Random(seed);
            for (int attempt = 0; attempt < 1000; attempt++)
            {
                var tiles = new int[rows];
                for (int i = 0; i < rows; i++) tiles[i] = rng.Next(2);
                if (IsAcceptable(tiles)) return new GlassBridgeRound(seed, playerCount, tiles);
            }
            throw new InvalidOperationException("Không sinh được đề cầu kính hợp lệ");
        }

        /// <summary>Nhận dãy ô an toàn; trả true nếu cả hai bên đều có và không có hơn 3 hàng liền cùng một bên.</summary>
        public static bool IsAcceptable(int[] tiles)
        {
            if (tiles == null || tiles.Length == 0) return false;
            bool hasLeft = false, hasRight = false;
            int run = 0, last = -1;
            foreach (int t in tiles)
            {
                if (t == 0) hasLeft = true; else hasRight = true;
                run = t == last ? run + 1 : 1;
                last = t;
                if (run > MaxRun) return false;
            }
            return hasLeft && hasRight;
        }
    }
}
