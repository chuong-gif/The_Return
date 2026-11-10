/*
 * Mục đích: Trạng thái CHUNG (mọi người cùng thấy) và thông tin RIÊNG (chỉ một người) của «Hợp xướng» online (T-037).
 * Chung: bản nhạc (dãy ký hiệu trên tường), nốt đang tới, số lần sai/quá giờ, cửa sổ giữa hai nốt, thông báo gần nhất.
 * KHÔNG chứa seed, bảng ký hiệu -> bục hay đáp án. KHÔNG chứa đồng hồ đang chạy: trạng thái ở một Version phải bất biến (hợp đồng C8/N1),
 * máy khách tự ước lượng giây còn lại từ lúc nhận gói có Progress mới và WindowSeconds.
 * Riêng: các ký hiệu người đó giữ thẻ và bục mỗi ký hiệu ứng với (chỉ chủ thẻ được xem). Bất biến: mảng được sao khi tạo và khi đọc.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: (ChoirPublicState) constructor; NoteCount; SymbolAt; SymbolsCopy; (ChoirPrivateState) constructor; OwnedCount; SymbolsCopy; PadsCopy; TryGetPad.
 */
using System;

namespace TheReturn.ChoirNet
{
    /// <summary>Thông báo gần nhất để HUD hiển thị (mã enum, chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum ChoirMessage
    {
        None,
        Correct,
        Completed,
        Wrong,
        TooLate,
        TimedOut,
        Retry
    }

    /// <summary>Thông tin riêng của một ô: ký hiệu người đó giữ thẻ và bục tương ứng.</summary>
    public readonly struct ChoirPrivateState
    {
        readonly int[] symbols;
        readonly int[] pads;

        /// <summary>Nhận danh sách ký hiệu và bục tương ứng (cùng độ dài; null coi là rỗng). Ném lỗi nếu độ dài khác nhau.</summary>
        public ChoirPrivateState(int[] ownedSymbols, int[] padOfEach)
        {
            int a = ownedSymbols == null ? 0 : ownedSymbols.Length;
            int b = padOfEach == null ? 0 : padOfEach.Length;
            if (a != b) throw new ArgumentException("Số ký hiệu và số bục phải bằng nhau.");
            symbols = a == 0 ? new int[0] : (int[])ownedSymbols.Clone();
            pads = a == 0 ? new int[0] : (int[])padOfEach.Clone();
        }

        /// <summary>Số ký hiệu người này giữ thẻ.</summary>
        public int OwnedCount { get { return symbols == null ? 0 : symbols.Length; } }

        /// <summary>Trả bản sao danh sách ký hiệu.</summary>
        public int[] SymbolsCopy()
        {
            return symbols == null ? new int[0] : (int[])symbols.Clone();
        }

        /// <summary>Trả bản sao danh sách bục, cùng thứ tự với SymbolsCopy.</summary>
        public int[] PadsCopy()
        {
            return pads == null ? new int[0] : (int[])pads.Clone();
        }

        /// <summary>Nhận ký hiệu; trả true và gán bục nếu người này giữ thẻ ký hiệu đó, ngược lại false (pad = -1).</summary>
        public bool TryGetPad(int symbol, out int pad)
        {
            pad = -1;
            if (symbols == null) return false;
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] != symbol) continue;
                pad = pads[i];
                return true;
            }
            return false;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ Hợp xướng.</summary>
    public sealed class ChoirPublicState
    {
        /// <summary>Số bục / số loại ký hiệu.</summary>
        public const int PadCount = 4;

        /// <summary>Số nốt tối đa chấp nhận khi giải mã (Domain hiện dùng 6).</summary>
        public const int MaxNotes = 12;

        readonly int[] symbols;

        /// <summary>Số người của đề (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>Số nốt của bản nhạc.</summary>
        public int NoteCount { get { return symbols.Length; } }

        /// <summary>Số nốt đã bấm đúng liên tiếp; bằng NoteCount khi đã thắng.</summary>
        public int Progress { get; }

        /// <summary>Số lần bấm sai bục.</summary>
        public int Mistakes { get; }

        /// <summary>Số lần quá giờ giữa hai nốt.</summary>
        public int Timeouts { get; }

        /// <summary>True khi đã hát đủ bản nhạc.</summary>
        public bool Solved { get; }

        /// <summary>Số giây tối đa giữa hai nốt (0 = không giới hạn).</summary>
        public double WindowSeconds { get; }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get; }

        /// <summary>Thông báo gần nhất.</summary>
        public ChoirMessage LastMessage { get; }

        /// <summary>Ô người bấm/làm lại gần nhất, -1 nếu chưa có hoặc do quá giờ.</summary>
        public int LastActor { get; }

        /// <summary>Bục bấm gần nhất, -1 nếu không phải lần bấm.</summary>
        public int LastPad { get; }

        /// <summary>
        /// Nhận số người (2-4), dãy ký hiệu (độ dài 2..MaxNotes, mỗi giá trị 0-3), nốt đã đúng, số lần sai, số lần quá giờ, cờ thắng,
        /// giây cửa sổ, số lần làm lại, thông báo, ô và bục gần nhất. Ném lỗi nếu dữ liệu sai hình dạng
        /// (thắng mà chưa hết dãy, hết dãy mà chưa thắng, thắng mà thông báo không phải Completed...).
        /// </summary>
        public ChoirPublicState(int playerCount, int[] symbolSequence, int progress, int mistakes, int timeouts, bool solved,
            double windowSeconds, int resetCount, ChoirMessage lastMessage, int lastActor, int lastPad)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            if (symbolSequence == null || symbolSequence.Length < 2 || symbolSequence.Length > MaxNotes) throw new ArgumentException("Bản nhạc phải có 2.." + MaxNotes + " nốt.", "symbolSequence");
            foreach (int s in symbolSequence)
                if (s < 0 || s >= PadCount) throw new ArgumentOutOfRangeException("symbolSequence");
            if (progress < 0 || progress > symbolSequence.Length) throw new ArgumentOutOfRangeException("progress");
            if (mistakes < 0) throw new ArgumentOutOfRangeException("mistakes");
            if (timeouts < 0) throw new ArgumentOutOfRangeException("timeouts");
            if (double.IsNaN(windowSeconds) || double.IsInfinity(windowSeconds) || windowSeconds < 0.0) throw new ArgumentOutOfRangeException("windowSeconds");
            if (resetCount < 0) throw new ArgumentOutOfRangeException("resetCount");
            if (lastActor < -1 || lastActor >= playerCount) throw new ArgumentOutOfRangeException("lastActor");
            if (lastPad < -1 || lastPad >= PadCount) throw new ArgumentOutOfRangeException("lastPad");
            if (solved != (progress == symbolSequence.Length)) throw new ArgumentException("Thắng khi và chỉ khi đã hết dãy.", "solved");
            if (solved && lastMessage != ChoirMessage.Completed) throw new ArgumentException("Đã thắng thì thông báo phải là Completed.", "lastMessage");
            PlayerCount = playerCount;
            symbols = (int[])symbolSequence.Clone();
            Progress = progress;
            Mistakes = mistakes;
            Timeouts = timeouts;
            Solved = solved;
            WindowSeconds = windowSeconds;
            ResetCount = resetCount;
            LastMessage = lastMessage;
            LastActor = lastActor;
            LastPad = lastPad;
        }

        /// <summary>Nhận chỉ số nốt; trả ký hiệu của nốt đó (ngoài khoảng trả -1).</summary>
        public int SymbolAt(int index)
        {
            return index >= 0 && index < symbols.Length ? symbols[index] : -1;
        }

        /// <summary>Trả bản sao dãy ký hiệu của bản nhạc.</summary>
        public int[] SymbolsCopy()
        {
            return (int[])symbols.Clone();
        }
    }
}
