/*
 * Mục đích: Dữ liệu một đề của nhiệm vụ Hanh_Lang_2 «Cầu kính» (T-006e): cầu có bao nhiêu hàng và ô nào (trái/phải) chịu được
 * ở mỗi hàng. Thuần C#, không phụ thuộc Unity.
 * Hàng đánh số từ 0 (sát đài đầu) tới Rows-1 (sát đài cuối); ô 0 = trái, ô 1 = phải; mỗi hàng đúng một ô chịu được.
 * Hàm: constructor; SafeTile; CopySafeTiles.
 */
using System;

namespace TheReturn
{
    /// <summary>Một đề cầu kính: dãy ô an toàn, mỗi hàng đúng một ô trong hai ô.</summary>
    public sealed class GlassBridgeRound
    {
        public const int MinRows = 3;
        public const int MaxRows = 10;
        public const int DefaultRows = 6;

        readonly int[] safeTiles;

        public int Seed { get; }
        public int PlayerCount { get; }
        public int Rows { get; }

        /// <summary>
        /// Nhận seed, số người (2-4) và ô an toàn của từng hàng (mỗi phần tử 0 hoặc 1, số hàng từ MinRows tới MaxRows).
        /// Ném ArgumentOutOfRangeException nếu số người sai; ArgumentException nếu dãy ô null, số hàng sai hoặc có ô ngoài {0,1}.
        /// </summary>
        public GlassBridgeRound(int seed, int playerCount, int[] safeTiles)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (safeTiles == null) throw new ArgumentException("Thiếu dãy ô an toàn", nameof(safeTiles));
            if (safeTiles.Length < MinRows || safeTiles.Length > MaxRows)
                throw new ArgumentException("Số hàng phải từ " + MinRows + " tới " + MaxRows, nameof(safeTiles));
            foreach (int tile in safeTiles)
                if (tile != 0 && tile != 1) throw new ArgumentException("Ô an toàn chỉ là 0 (trái) hoặc 1 (phải)", nameof(safeTiles));
            Seed = seed;
            PlayerCount = playerCount;
            Rows = safeTiles.Length;
            this.safeTiles = (int[])safeTiles.Clone();
        }

        /// <summary>Nhận chỉ số hàng; trả ô an toàn của hàng đó (0 hoặc 1), ngoài khoảng trả -1.</summary>
        public int SafeTile(int row) { return row >= 0 && row < Rows ? safeTiles[row] : -1; }

        /// <summary>Không nhận tham số; trả bản sao dãy ô an toàn (sửa bản sao không đổi đề).</summary>
        public int[] CopySafeTiles() { return (int[])safeTiles.Clone(); }
    }
}
