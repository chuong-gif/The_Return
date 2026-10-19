/*
 * Mục đích: Dữ liệu bất biến của một đề «Cầu ván và thẻ từ» (T-006b): seed, thời gian qua cầu của từng người, thời gian tối ưu
 * và hạn tổng (tối ưu + phần dư theo %). Đơn vị thời gian là «đơn vị» (hệ số ra giây thật thuộc Data/Presentation). Thuần C#.
 * Hàm: constructor; CrossUnits; CrossUnitsCopy; OptimalPlan.
 */
using System;

namespace TheReturn
{
    /// <summary>Một đề cầu ván: người chơi, thời gian qua cầu, thời gian tối ưu và hạn tổng.</summary>
    public sealed class PlankRound
    {
        /// <summary>Số người tối thiểu.</summary>
        public const int MinPlayers = 2;
        /// <summary>Số người tối đa.</summary>
        public const int MaxPlayers = 4;

        readonly int[] crossUnits;
        readonly PlankPlan optimalPlan;

        /// <summary>Seed đã sinh đề.</summary>
        public int Seed { get; private set; }
        /// <summary>Phần dư (%) cộng thêm vào thời gian tối ưu để ra hạn.</summary>
        public int SlackPercent { get; private set; }
        /// <summary>Thời gian tối ưu (đơn vị).</summary>
        public int OptimalUnits { get { return optimalPlan.TotalUnits; } }
        /// <summary>Hạn tổng (đơn vị) = tối ưu + làm tròn lên (tối ưu x slack / 100).</summary>
        public int DeadlineUnits { get; private set; }
        /// <summary>Số người chơi.</summary>
        public int PlayerCount { get { return crossUnits.Length; } }

        /// <summary>
        /// Nhận seed, thời gian qua cầu từng người (2-4 người, mỗi giá trị > 0) và phần dư %; tính kế hoạch tối ưu và hạn.
        /// Ném ArgumentNullException / ArgumentOutOfRangeException nếu tham số sai.
        /// </summary>
        public PlankRound(int seed, int[] crossUnits, int slackPercent)
        {
            if (crossUnits == null) throw new ArgumentNullException(nameof(crossUnits));
            if (slackPercent < 0) throw new ArgumentOutOfRangeException(nameof(slackPercent));
            this.crossUnits = (int[])crossUnits.Clone();
            optimalPlan = PlankSolver.Solve(this.crossUnits);
            Seed = seed;
            SlackPercent = slackPercent;
            int optimal = optimalPlan.TotalUnits;
            DeadlineUnits = optimal + (optimal * slackPercent + 99) / 100;
        }

        /// <summary>Nhận chỉ số người chơi; trả thời gian qua cầu của người đó (đơn vị).</summary>
        public int CrossUnits(int player)
        {
            if (player < 0 || player >= crossUnits.Length) throw new ArgumentOutOfRangeException(nameof(player));
            return crossUnits[player];
        }

        /// <summary>Trả bản sao mảng thời gian qua cầu của mọi người.</summary>
        public int[] CrossUnitsCopy()
        {
            return (int[])crossUnits.Clone();
        }

        /// <summary>Trả kế hoạch tối ưu của đề này.</summary>
        public PlankPlan OptimalPlan()
        {
            return optimalPlan;
        }
    }
}
