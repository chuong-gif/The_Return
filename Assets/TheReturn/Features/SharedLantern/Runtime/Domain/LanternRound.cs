/*
 * Mục đích: Dữ liệu một đề của nhiệm vụ Hanh_Lang_4 «Đèn lồng chung» (T-006f): bao nhiêu người chơi, mỗi biển số 1-4 mang
 * ký hiệu nào. Thuần C#, không phụ thuộc Unity. Biển và ký hiệu đánh số từ 0 (biển 0 là biển số 1 trên tường).
 * Hàm: constructor; SymbolOfSign; Solution.
 */
using System;

namespace TheReturn
{
    /// <summary>Một đề «Đèn lồng chung»: 4 biển, mỗi biển 1 ký hiệu khác nhau trong 6 ký hiệu.</summary>
    public sealed class LanternRound
    {
        /// <summary>Số biển cố định của hành lang (thiết kế đã duyệt).</summary>
        public const int SignCount = 4;

        /// <summary>Số ký hiệu khác nhau có thể xuất hiện trên biển và trên bảng nhập.</summary>
        public const int SymbolCount = 6;

        readonly int[] signSymbols;

        /// <summary>Hạt giống đã sinh ra đề này (0 nếu tạo tay).</summary>
        public int Seed { get; }

        /// <summary>Số người chơi, từ 2 đến 4.</summary>
        public int PlayerCount { get; }

        /// <summary>Nhận hạt giống, số người (2-4) và 4 ký hiệu của 4 biển (khác nhau, mỗi cái trong 0..5); lỗi tham số thì ném ngoại lệ.</summary>
        public LanternRound(int seed, int playerCount, int[] signSymbols)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount), "Số người chơi phải từ 2 đến 4");
            if (signSymbols == null || signSymbols.Length != SignCount) throw new ArgumentException("Cần đúng 4 ký hiệu", nameof(signSymbols));
            var seen = new bool[SymbolCount];
            for (int i = 0; i < SignCount; i++)
            {
                int s = signSymbols[i];
                if (s < 0 || s >= SymbolCount) throw new ArgumentException("Ký hiệu ngoài phạm vi 0..5", nameof(signSymbols));
                if (seen[s]) throw new ArgumentException("Các ký hiệu phải khác nhau", nameof(signSymbols));
                seen[s] = true;
            }
            Seed = seed;
            PlayerCount = playerCount;
            this.signSymbols = (int[])signSymbols.Clone();
        }

        /// <summary>Nhận chỉ số biển (0..3); trả ký hiệu của biển đó, hoặc -1 nếu chỉ số sai.</summary>
        public int SymbolOfSign(int sign) { return sign >= 0 && sign < SignCount ? signSymbols[sign] : -1; }

        /// <summary>Không nhận gì; trả bản sao đáp án: ký hiệu của biển 0,1,2,3 theo thứ tự (đúng thứ tự phải nhập ở bảng cuối).</summary>
        public int[] Solution() { return (int[])signSymbols.Clone(); }
    }
}
