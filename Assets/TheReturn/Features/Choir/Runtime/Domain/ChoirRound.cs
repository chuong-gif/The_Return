/*
 * Mục đích: Một đề của nhiệm vụ Phong_6 «Hợp xướng» (T-006d): bản nhạc là dãy ký hiệu trên tường, mỗi ký hiệu ứng với một bục số 1-4,
 * bảng dịch ký hiệu -> bục được chia thành «thẻ nhạc» cho từng người chơi (bục chỉ hiện SỐ, không hiện ký hiệu).
 * Thuần C#, bất biến sau khi tạo.
 * Hàm: constructor; Length; Symbol; PadOfSymbol; SolutionPad; OwnerOfSymbol; SymbolsOf; DistinctSymbolCount.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Một đề Hợp xướng: dãy ký hiệu cần chơi và bảng ký hiệu -> bục.</summary>
    public sealed class ChoirRound
    {
        /// <summary>Số bục (cũng là số loại ký hiệu).</summary>
        public const int PadCount = 4;

        readonly int[] symbols;
        readonly int[] padOfSymbol;

        public int PlayerCount { get; }
        public int Seed { get; }

        /// <summary>
        /// Nhận số người (2-4), seed, dãy ký hiệu (độ dài ≥ 2, mỗi giá trị 0-3) và bảng ký hiệu -> bục (hoán vị của 0-3).
        /// Sao chép mảng; ném ArgumentException nếu sai.
        /// </summary>
        public ChoirRound(int playerCount, int seed, int[] symbolSequence, int[] symbolToPad)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (symbolSequence == null || symbolSequence.Length < 2) throw new ArgumentException("Dãy ký hiệu quá ngắn", nameof(symbolSequence));
            if (symbolToPad == null || symbolToPad.Length != PadCount) throw new ArgumentException("Bảng ký hiệu -> bục phải có " + PadCount + " phần tử", nameof(symbolToPad));
            var seen = new bool[PadCount];
            foreach (int pad in symbolToPad)
            {
                if (pad < 0 || pad >= PadCount || seen[pad]) throw new ArgumentException("Bảng ký hiệu -> bục phải là hoán vị", nameof(symbolToPad));
                seen[pad] = true;
            }
            foreach (int s in symbolSequence)
                if (s < 0 || s >= PadCount) throw new ArgumentException("Ký hiệu ngoài phạm vi 0-3", nameof(symbolSequence));
            PlayerCount = playerCount;
            Seed = seed;
            symbols = (int[])symbolSequence.Clone();
            padOfSymbol = (int[])symbolToPad.Clone();
        }

        /// <summary>Số nốt trong bản nhạc.</summary>
        public int Length => symbols.Length;

        /// <summary>Nhận chỉ số nốt (từ 0); trả ký hiệu của nốt đó (hiển thị cho cả đội trên bản nhạc).</summary>
        public int Symbol(int index) { return symbols[index]; }

        /// <summary>Nhận ký hiệu 0-3; trả chỉ số bục (0-3) mà ký hiệu ứng với. Chỉ chủ thẻ mới được xem qua ChoirState.TryReadCard.</summary>
        public int PadOfSymbol(int symbol) { return padOfSymbol[symbol]; }

        /// <summary>Nhận chỉ số nốt; trả bục đúng của nốt đó (đáp án; không đưa cho người chơi).</summary>
        public int SolutionPad(int index) { return padOfSymbol[symbols[index]]; }

        /// <summary>Nhận ký hiệu; trả ID người giữ thẻ có ký hiệu đó (ký hiệu % số người): 4 người 1+1+1+1, 3 người 2+1+1, 2 người 2+2.</summary>
        public int OwnerOfSymbol(int symbol) { return symbol % PlayerCount; }

        /// <summary>Nhận ID người; trả danh sách ký hiệu người đó giữ thẻ (tăng dần).</summary>
        public int[] SymbolsOf(int player)
        {
            var list = new List<int>();
            for (int s = 0; s < PadCount; s++)
                if (OwnerOfSymbol(s) == player) list.Add(s);
            return list.ToArray();
        }

        /// <summary>Trả số loại ký hiệu khác nhau xuất hiện trong bản nhạc.</summary>
        public int DistinctSymbolCount
        {
            get
            {
                var seen = new bool[PadCount];
                int n = 0;
                foreach (int s in symbols)
                    if (!seen[s]) { seen[s] = true; n++; }
                return n;
            }
        }
    }
}
