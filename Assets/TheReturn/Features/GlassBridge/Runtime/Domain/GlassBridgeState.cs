/*
 * Mục đích: Trạng thái chơi của «Cầu kính» (T-006e): mỗi người đang ở đâu (đài đầu, hàng nào, đài cuối), ô nào đã vỡ,
 * ai được xem sơ đồ ô an toàn, cả đội qua hết chưa. Thuần C#, không có thời gian (nhiệm vụ không đếm giờ).
 * Quy ước vị trí: -1 = đài đầu, 0..Rows-1 = đang đứng trên hàng đó, Rows = đài cuối.
 * Luật: chỉ người đứng trên một đài (đầu hoặc cuối) xem được sơ đồ; người đang trên cầu không xem được. Bước ô sai: ô vỡ
 * (vỡ cho cả đội, không bước lại được), CHỈ người đó về đài đầu; ô đúng không để lại dấu vết. Thắng khi mọi người ở đài cuối.
 * Hàm: constructor; RowOf; OnBridge; CanSeeDiagram; TryGetDiagram; IsBroken; FallsOf; AcrossCount; Step; Disembark; Retreat; Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Kết quả một thao tác trên cầu.</summary>
    public enum GlassBridgeResult
    {
        Advanced,
        Fell,
        TileBroken,
        UseDisembark,
        NotAtLastRow,
        ReachedEnd,
        AlreadyAcross,
        InvalidPlayer,
        InvalidTile,
        AlreadySolved
    }

    /// <summary>Trạng thái một lượt chơi cầu kính trên một đề.</summary>
    public sealed class GlassBridgeState
    {
        public const int StartPlatform = -1;

        readonly int[] rowOf;
        readonly int[] falls;
        readonly bool[,] broken;

        public GlassBridgeRound Round { get; }
        /// <summary>Tổng số lần té (mọi người), giữ nguyên qua Reset để thống kê.</summary>
        public int Falls { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>Nhận đề; tạo trạng thái mới, mọi người ở đài đầu, chưa ô nào vỡ.</summary>
        public GlassBridgeState(GlassBridgeRound round)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            Round = round;
            rowOf = new int[round.PlayerCount];
            falls = new int[round.PlayerCount];
            broken = new bool[round.Rows, 2];
            for (int i = 0; i < rowOf.Length; i++) rowOf[i] = StartPlatform;
        }

        /// <summary>Vị trí đài cuối (bằng số hàng).</summary>
        public int EndPlatform => Round.Rows;

        /// <summary>Nhận chỉ số người; trả vị trí (-1 đài đầu, 0..Rows-1 trên cầu, Rows đài cuối). Ném ArgumentOutOfRangeException nếu người sai.</summary>
        public int RowOf(int player)
        {
            if (player < 0 || player >= rowOf.Length) throw new ArgumentOutOfRangeException(nameof(player));
            return rowOf[player];
        }

        /// <summary>Nhận chỉ số người; trả true nếu người đó đang đứng trên một hàng ô kính (không phải đài).</summary>
        public bool OnBridge(int player)
        {
            int r = RowOf(player);
            return r >= 0 && r < Round.Rows;
        }

        /// <summary>Nhận chỉ số người; trả true nếu đang ở một đài nên xem được bảng sơ đồ.</summary>
        public bool CanSeeDiagram(int player) { return !OnBridge(player); }

        /// <summary>Nhận chỉ số người; nếu đang ở đài thì gán bản sao dãy ô an toàn và trả true, ngược lại gán null và trả false.</summary>
        public bool TryGetDiagram(int player, out int[] safeTiles)
        {
            safeTiles = null;
            if (!CanSeeDiagram(player)) return false;
            safeTiles = Round.CopySafeTiles();
            return true;
        }

        /// <summary>Nhận hàng và ô (0 hoặc 1); trả true nếu ô đó đã vỡ. Ngoài khoảng trả false.</summary>
        public bool IsBroken(int row, int tile)
        {
            return row >= 0 && row < Round.Rows && (tile == 0 || tile == 1) && broken[row, tile];
        }

        /// <summary>Nhận chỉ số người; trả số lần người đó đã té. Ném ArgumentOutOfRangeException nếu người sai.</summary>
        public int FallsOf(int player)
        {
            if (player < 0 || player >= falls.Length) throw new ArgumentOutOfRangeException(nameof(player));
            return falls[player];
        }

        /// <summary>Không nhận tham số; trả số người đã tới đài cuối.</summary>
        public int AcrossCount
        {
            get
            {
                int c = 0;
                foreach (int r in rowOf) if (r == Round.Rows) c++;
                return c;
            }
        }

        /// <summary>
        /// Nhận người và ô muốn bước (0 trái, 1 phải) sang hàng kế tiếp. Ô đã vỡ: TileBroken, không đổi gì. Ô đúng: Advanced.
        /// Ô sai: ô vỡ, người đó về đài đầu, Fell. Đang ở hàng cuối thì phải dùng Disembark (UseDisembark).
        /// </summary>
        public GlassBridgeResult Step(int player, int tile)
        {
            if (Solved) return GlassBridgeResult.AlreadySolved;
            if (player < 0 || player >= rowOf.Length) return GlassBridgeResult.InvalidPlayer;
            if (tile != 0 && tile != 1) return GlassBridgeResult.InvalidTile;
            int row = rowOf[player];
            if (row >= Round.Rows) return GlassBridgeResult.AlreadyAcross;
            if (row == Round.Rows - 1) return GlassBridgeResult.UseDisembark;
            int next = row + 1;
            if (broken[next, tile]) return GlassBridgeResult.TileBroken;
            if (tile == Round.SafeTile(next))
            {
                rowOf[player] = next;
                return GlassBridgeResult.Advanced;
            }
            broken[next, tile] = true;
            rowOf[player] = StartPlatform;
            Falls++;
            falls[player]++;
            return GlassBridgeResult.Fell;
        }

        /// <summary>Nhận người; từ hàng cuối bước lên đài cuối (ReachedEnd). Mọi người đã qua thì đề thắng. Chỗ khác: NotAtLastRow.</summary>
        public GlassBridgeResult Disembark(int player)
        {
            if (Solved) return GlassBridgeResult.AlreadySolved;
            if (player < 0 || player >= rowOf.Length) return GlassBridgeResult.InvalidPlayer;
            int row = rowOf[player];
            if (row >= Round.Rows) return GlassBridgeResult.AlreadyAcross;
            if (row != Round.Rows - 1) return GlassBridgeResult.NotAtLastRow;
            rowOf[player] = Round.Rows;
            if (AcrossCount == rowOf.Length) Solved = true;
            return GlassBridgeResult.ReachedEnd;
        }

        /// <summary>Nhận người; lùi một hàng (từ hàng 0 thì về đài đầu), không té. Trả false nếu không lùi được (đang ở đài, đã thắng, người sai).</summary>
        public bool Retreat(int player)
        {
            if (Solved || player < 0 || player >= rowOf.Length) return false;
            int row = rowOf[player];
            if (row < 0 || row >= Round.Rows) return false;
            rowOf[player] = row - 1;
            return true;
        }

        /// <summary>Không nhận tham số; đặt lại: mọi người về đài đầu, mọi ô lành, chưa thắng. GIỮ cùng đề và số lần té.</summary>
        public void Reset()
        {
            for (int i = 0; i < rowOf.Length; i++) rowOf[i] = StartPlatform;
            Array.Clear(broken, 0, broken.Length);
            Solved = false;
        }
    }
}
