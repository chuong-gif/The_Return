/*
 * Mục đích: Bộ giải tối ưu cho «Cầu ván và thẻ từ» (T-006b): tìm tổng thời gian nhỏ nhất để cả đội qua cầu khi ván chứa tối đa
 * 2 người, thẻ phải đi cùng nhóm qua cầu và mỗi lượt tốn thời gian của người chậm nhất. Thuần C#.
 * Hàm: PlankPlan (kế hoạch); PlankSolver.Solve; PlankSolver.CountBits; NextUnvisited, MaskToPlayers (nội bộ).
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Kế hoạch tối ưu: tổng thời gian (đơn vị) và các lượt qua cầu theo thứ tự.</summary>
    public sealed class PlankPlan
    {
        /// <summary>Tổng thời gian (đơn vị) của kế hoạch.</summary>
        public int TotalUnits { get; private set; }

        /// <summary>Các lượt theo thứ tự (mỗi lượt là 1-2 chỉ số người chơi); lượt 0, 2, 4... đi bờ gần sang bờ xa, lượt lẻ đi ngược lại.</summary>
        public IList<int[]> Moves { get; private set; }

        /// <summary>Nhận tổng thời gian và danh sách lượt; trả kế hoạch (không sao chép sâu danh sách).</summary>
        public PlankPlan(int totalUnits, IList<int[]> moves)
        {
            TotalUnits = totalUnits;
            Moves = moves;
        }
    }

    /// <summary>Bộ giải Dijkstra trên trạng thái (ai đang ở bờ xa) x (thẻ ở bờ nào).</summary>
    public static class PlankSolver
    {
        /// <summary>Số người tối đa trên ván.</summary>
        public const int Capacity = 2;

        /// <summary>
        /// Nhận thời gian qua cầu của từng người (2-4 người, mỗi giá trị > 0); trả kế hoạch ít tốn thời gian nhất.
        /// Ném ArgumentNullException (null) hoặc ArgumentOutOfRangeException (số người hay giá trị sai).
        /// </summary>
        public static PlankPlan Solve(int[] crossUnits)
        {
            if (crossUnits == null) throw new ArgumentNullException(nameof(crossUnits));
            int n = crossUnits.Length;
            if (n < 2 || n > 4) throw new ArgumentOutOfRangeException(nameof(crossUnits));
            for (int i = 0; i < n; i++)
                if (crossUnits[i] <= 0) throw new ArgumentOutOfRangeException(nameof(crossUnits));

            int full = (1 << n) - 1;
            int states = (1 << n) * 2;
            var dist = new int[states];
            var prev = new int[states];
            var prevGroup = new int[states];
            var done = new bool[states];
            for (int s = 0; s < states; s++) { dist[s] = int.MaxValue; prev[s] = -1; }
            dist[0] = 0;

            int cur = NextUnvisited(dist, done);
            while (cur >= 0)
            {
                done[cur] = true;
                int far = cur >> 1;
                int cardFar = cur & 1;
                int here = cardFar == 1 ? far : (~far & full);
                for (int group = 1; group <= full; group++)
                {
                    if ((group & here) != group || CountBits(group) > Capacity) continue;
                    int cost = 0;
                    for (int i = 0; i < n; i++)
                        if ((group >> i & 1) == 1 && crossUnits[i] > cost) cost = crossUnits[i];
                    int nextFar = cardFar == 1 ? far & ~group : far | group;
                    int next = nextFar * 2 + (1 - cardFar);
                    if (dist[cur] + cost < dist[next])
                    {
                        dist[next] = dist[cur] + cost;
                        prev[next] = cur;
                        prevGroup[next] = group;
                    }
                }
                cur = NextUnvisited(dist, done);
            }

            int goal = full * 2 + 1;
            if (dist[goal] == int.MaxValue) throw new InvalidOperationException("Không có cách qua cầu");
            var moves = new List<int[]>();
            for (int s = goal; prev[s] >= 0; s = prev[s]) moves.Add(MaskToPlayers(prevGroup[s], n));
            moves.Reverse();
            return new PlankPlan(dist[goal], moves);
        }

        /// <summary>Nhận một số nguyên; trả số bit 1.</summary>
        public static int CountBits(int value)
        {
            int c = 0;
            while (value != 0) { c += value & 1; value >>= 1; }
            return c;
        }

        /// <summary>Nhận mảng khoảng cách và cờ đã duyệt; trả chỉ số trạng thái chưa duyệt có khoảng cách nhỏ nhất, hoặc -1 nếu hết.</summary>
        static int NextUnvisited(int[] dist, bool[] done)
        {
            int best = -1;
            for (int s = 0; s < dist.Length; s++)
                if (!done[s] && dist[s] != int.MaxValue && (best < 0 || dist[s] < dist[best])) best = s;
            return best;
        }

        /// <summary>Nhận mặt nạ nhóm và số người; trả mảng chỉ số người trong nhóm (tăng dần).</summary>
        static int[] MaskToPlayers(int mask, int n)
        {
            var list = new List<int>();
            for (int i = 0; i < n; i++)
                if ((mask >> i & 1) == 1) list.Add(i);
            return list.ToArray();
        }
    }
}
