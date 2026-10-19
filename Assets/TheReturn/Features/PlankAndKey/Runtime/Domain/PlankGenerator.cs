/*
 * Mục đích: Sinh đề «Cầu ván và thẻ từ» theo seed (T-006b): chọn một bộ thời gian qua cầu từ danh mục theo số người, xáo gán
 * cho người chơi theo seed, hạn = tối ưu + phần dư (tham số, mặc định 25%). Thuần C#.
 * Hàm: Generate (2 dạng); CatalogSize; CatalogSet; Sets (nội bộ).
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề cầu ván: cùng số người và seed cho cùng đề.</summary>
    public static class PlankGenerator
    {
        /// <summary>Phần dư mặc định (%) của hạn so với thời gian tối ưu (thiết kế: 20-25%).</summary>
        public const int DefaultSlackPercent = 25;

        // Bộ thời gian qua cầu (đơn vị). Bộ 3 người {1,2,5} và 4 người {1,2,5,10} {2,3,4,6} {1,2,3,4} đã có trong thiết kế duyệt;
        // các bộ còn lại do bộ giải tính, chưa duyệt về độ khó (cần playtest).
        static readonly int[][] TwoPlayerSets = { new[] { 2, 5 }, new[] { 3, 6 }, new[] { 2, 4 } };
        static readonly int[][] ThreePlayerSets = { new[] { 1, 2, 5 }, new[] { 1, 3, 6 }, new[] { 2, 3, 6 } };
        static readonly int[][] FourPlayerSets = { new[] { 1, 2, 5, 10 }, new[] { 2, 3, 4, 6 }, new[] { 1, 2, 3, 4 } };

        /// <summary>Nhận số người (2-4); trả số bộ thời gian trong danh mục. Ném ArgumentOutOfRangeException nếu sai số người.</summary>
        public static int CatalogSize(int playerCount)
        {
            return Sets(playerCount).Length;
        }

        /// <summary>Nhận số người và chỉ số bộ; trả BẢN SAO bộ thời gian (chưa xáo). Ném ArgumentOutOfRangeException nếu sai.</summary>
        public static int[] CatalogSet(int playerCount, int index)
        {
            int[][] sets = Sets(playerCount);
            if (index < 0 || index >= sets.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return (int[])sets[index].Clone();
        }

        /// <summary>Nhận số người (2-4) và seed; trả đề với phần dư mặc định.</summary>
        public static PlankRound Generate(int playerCount, int seed)
        {
            return Generate(playerCount, seed, DefaultSlackPercent);
        }

        /// <summary>
        /// Nhận số người (2-4), seed và phần dư % (>= 0); trả đề mới (cùng tham số cho cùng đề).
        /// Ném ArgumentOutOfRangeException nếu số người hoặc phần dư sai.
        /// </summary>
        public static PlankRound Generate(int playerCount, int seed, int slackPercent)
        {
            int[][] sets = Sets(playerCount);
            if (slackPercent < 0) throw new ArgumentOutOfRangeException(nameof(slackPercent));
            var rng = new Random(seed);
            int[] speeds = (int[])sets[rng.Next(sets.Length)].Clone();
            for (int i = speeds.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = speeds[i];
                speeds[i] = speeds[j];
                speeds[j] = tmp;
            }
            return new PlankRound(seed, speeds, slackPercent);
        }

        /// <summary>Nhận số người; trả danh mục bộ thời gian tương ứng.</summary>
        static int[][] Sets(int playerCount)
        {
            switch (playerCount)
            {
                case 2: return TwoPlayerSets;
                case 3: return ThreePlayerSets;
                case 4: return FourPlayerSets;
                default: throw new ArgumentOutOfRangeException(nameof(playerCount));
            }
        }
    }
}
