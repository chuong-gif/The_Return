/*
 * Mục đích: Trạng thái CHUNG (mọi người cùng thấy) và thông tin RIÊNG (chỉ một người) của «Đèn lồng chung» online (T-037).
 * Chung: ai cầm đèn, ai đang ngoài vùng sáng, số lần bị đưa về, bảng nhập (4 ô), đã nhập đúng chưa, đã thắng chưa, số lần nhập sai, thông báo gần nhất.
 * KHÔNG chứa seed, ký hiệu trên biển hay đáp án. KHÔNG chứa đồng hồ đang chạy (giây ngoài vùng sáng, giây còn khóa): trạng thái ở một Version phải
 * bất biến (hợp đồng C8/N1, bài học F-001); máy khách tự ước lượng từ lúc nhận gói đổi, dựa vào OutsideLimitSeconds và SubmitLockSeconds.
 * Riêng: sổ của người đó (biển nào đã tự đọc và ký hiệu của biển đó). Bất biến: mảng được sao khi tạo và khi đọc.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: (SharedLanternPublicState) constructor; IsOutside; Entry; EntriesCopy; EntriesComplete; (SharedLanternPrivateState) constructor; ReadCount;
 * SignsCopy; SymbolsCopy; HasRead; TryGetSymbol.
 */
using System;

namespace TheReturn.LanternNet
{
    /// <summary>Thông báo gần nhất để HUD hiển thị (mã enum, chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum SharedLanternMessage
    {
        None,
        Passed,
        Read,
        EntrySet,
        EntriesCleared,
        WrongCode,
        CodeAccepted,
        SentBack,
        Solved,
        Retry
    }

    /// <summary>Thông tin riêng của một ô: các biển người đó đã tự đọc và ký hiệu của từng biển.</summary>
    public readonly struct SharedLanternPrivateState
    {
        readonly int[] signs;
        readonly int[] symbols;

        /// <summary>Nhận danh sách biển đã đọc và ký hiệu tương ứng (cùng độ dài; null coi là rỗng). Ném lỗi nếu độ dài khác nhau.</summary>
        public SharedLanternPrivateState(int[] readSigns, int[] symbolOfEach)
        {
            int a = readSigns == null ? 0 : readSigns.Length;
            int b = symbolOfEach == null ? 0 : symbolOfEach.Length;
            if (a != b) throw new ArgumentException("Số biển và số ký hiệu phải bằng nhau.");
            signs = a == 0 ? new int[0] : (int[])readSigns.Clone();
            symbols = a == 0 ? new int[0] : (int[])symbolOfEach.Clone();
        }

        /// <summary>Số biển người này đã đọc.</summary>
        public int ReadCount { get { return signs == null ? 0 : signs.Length; } }

        /// <summary>Trả bản sao danh sách biển đã đọc.</summary>
        public int[] SignsCopy()
        {
            return signs == null ? new int[0] : (int[])signs.Clone();
        }

        /// <summary>Trả bản sao danh sách ký hiệu, cùng thứ tự với SignsCopy.</summary>
        public int[] SymbolsCopy()
        {
            return symbols == null ? new int[0] : (int[])symbols.Clone();
        }

        /// <summary>Nhận chỉ số biển; trả true nếu người này đã đọc biển đó.</summary>
        public bool HasRead(int sign)
        {
            int symbol;
            return TryGetSymbol(sign, out symbol);
        }

        /// <summary>Nhận chỉ số biển; trả true và gán ký hiệu nếu người này đã đọc biển đó, ngược lại false (symbol = -1).</summary>
        public bool TryGetSymbol(int sign, out int symbol)
        {
            symbol = -1;
            if (signs == null) return false;
            for (int i = 0; i < signs.Length; i++)
            {
                if (signs[i] != sign) continue;
                symbol = symbols[i];
                return true;
            }
            return false;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ Đèn lồng chung.</summary>
    public sealed class SharedLanternPublicState
    {
        /// <summary>Số biển và số ô nhập.</summary>
        public const int SignCount = 4;

        /// <summary>Số ký hiệu khác nhau (0-5).</summary>
        public const int SymbolCount = 6;

        readonly int[] entries;

        /// <summary>Số người của đề (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>Ô người đang cầm đèn lồng.</summary>
        public int Holder { get; }

        /// <summary>Mặt nạ những người đang ở ngoài vùng sáng (bit i = ô i). Người cầm đèn không bao giờ nằm trong mặt nạ.</summary>
        public int OutsideMask { get; }

        /// <summary>Tổng số lần có người bị đưa về điểm đầu.</summary>
        public int SentBackCount { get; }

        /// <summary>Mặt nạ những người bị đưa về ở lần gần nhất (0 nếu chưa lần nào).</summary>
        public int LastSentBackMask { get; }

        /// <summary>True khi bảng nhập đã được chấp nhận (đúng).</summary>
        public bool CodeAccepted { get; }

        /// <summary>True khi bảng đúng và cả đội đã tới cổng (thắng).</summary>
        public bool Solved { get; }

        /// <summary>Số lần nhập sai bảng ký hiệu.</summary>
        public int WrongSubmits { get; }

        /// <summary>Số giây tối đa được đứng ngoài vùng sáng (để máy khách ước lượng đếm ngược).</summary>
        public double OutsideLimitSeconds { get; }

        /// <summary>Số giây bảng bị khóa sau mỗi lần nhập sai (để máy khách ước lượng đếm ngược).</summary>
        public double SubmitLockSeconds { get; }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get; }

        /// <summary>Thông báo gần nhất.</summary>
        public SharedLanternMessage LastMessage { get; }

        /// <summary>Ô người thao tác gần nhất, -1 nếu chưa có hoặc do hệ thống (bị đưa về, thắng).</summary>
        public int LastActor { get; }

        /// <summary>Chỉ số kèm theo thông báo: Passed = ô người nhận; Read = biển; EntrySet = ô nhập; còn lại -1.</summary>
        public int LastIndex { get; }

        /// <summary>
        /// Nhận số người (2-4), người cầm đèn, mặt nạ ngoài vùng sáng, số lần bị đưa về, mặt nạ lần gần nhất, 4 ô nhập (mỗi ô -1 trống hoặc 0-5),
        /// cờ đã đúng, cờ thắng, số lần sai, giây giới hạn ngoài sáng (> 0), giây khóa (>= 0), số lần làm lại, thông báo, ô và chỉ số gần nhất.
        /// Ném lỗi nếu dữ liệu sai hình dạng (người cầm đèn nằm trong mặt nạ ngoài sáng, thắng mà chưa đúng, đúng mà ô còn trống, thông báo Solved lệch cờ thắng...).
        /// </summary>
        public SharedLanternPublicState(int playerCount, int holder, int outsideMask, int sentBackCount, int lastSentBackMask, int[] entryValues,
            bool codeAccepted, bool solved, int wrongSubmits, double outsideLimitSeconds, double submitLockSeconds, int resetCount,
            SharedLanternMessage lastMessage, int lastActor, int lastIndex)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            if (holder < 0 || holder >= playerCount) throw new ArgumentOutOfRangeException("holder");
            int all = (1 << playerCount) - 1;
            if (outsideMask < 0 || outsideMask > all) throw new ArgumentOutOfRangeException("outsideMask");
            if ((outsideMask >> holder & 1) == 1) throw new ArgumentException("Người cầm đèn luôn trong vùng sáng.", "outsideMask");
            if (sentBackCount < 0) throw new ArgumentOutOfRangeException("sentBackCount");
            if (lastSentBackMask < 0 || lastSentBackMask > all) throw new ArgumentOutOfRangeException("lastSentBackMask");
            if (sentBackCount == 0 && lastSentBackMask != 0) throw new ArgumentException("Chưa ai bị đưa về thì mặt nạ phải rỗng.", "lastSentBackMask");
            if (entryValues == null || entryValues.Length != SignCount) throw new ArgumentException("Bảng nhập phải có đúng 4 ô.", "entryValues");
            bool full = true;
            foreach (int v in entryValues)
            {
                if (v < -1 || v >= SymbolCount) throw new ArgumentOutOfRangeException("entryValues");
                if (v < 0) full = false;
            }
            if (codeAccepted && !full) throw new ArgumentException("Bảng đã đúng thì 4 ô phải đầy.", "codeAccepted");
            if (solved && !codeAccepted) throw new ArgumentException("Thắng thì bảng phải đã đúng.", "solved");
            if (wrongSubmits < 0) throw new ArgumentOutOfRangeException("wrongSubmits");
            if (double.IsNaN(outsideLimitSeconds) || double.IsInfinity(outsideLimitSeconds) || outsideLimitSeconds <= 0.0) throw new ArgumentOutOfRangeException("outsideLimitSeconds");
            if (double.IsNaN(submitLockSeconds) || double.IsInfinity(submitLockSeconds) || submitLockSeconds < 0.0) throw new ArgumentOutOfRangeException("submitLockSeconds");
            if (resetCount < 0) throw new ArgumentOutOfRangeException("resetCount");
            if (lastActor < -1 || lastActor >= playerCount) throw new ArgumentOutOfRangeException("lastActor");
            if (lastIndex < -1 || lastIndex >= SymbolCount) throw new ArgumentOutOfRangeException("lastIndex");
            if (solved != (lastMessage == SharedLanternMessage.Solved)) throw new ArgumentException("Cờ thắng và thông báo Solved phải khớp nhau.", "lastMessage");
            PlayerCount = playerCount;
            Holder = holder;
            OutsideMask = outsideMask;
            SentBackCount = sentBackCount;
            LastSentBackMask = lastSentBackMask;
            entries = (int[])entryValues.Clone();
            CodeAccepted = codeAccepted;
            Solved = solved;
            WrongSubmits = wrongSubmits;
            OutsideLimitSeconds = outsideLimitSeconds;
            SubmitLockSeconds = submitLockSeconds;
            ResetCount = resetCount;
            LastMessage = lastMessage;
            LastActor = lastActor;
            LastIndex = lastIndex;
        }

        /// <summary>Nhận ô người chơi; trả true nếu người đó đang ở ngoài vùng sáng (ngoài khoảng trả false).</summary>
        public bool IsOutside(int player)
        {
            return player >= 0 && player < PlayerCount && (OutsideMask >> player & 1) == 1;
        }

        /// <summary>Nhận ô nhập (0-3); trả ký hiệu đã nhập hoặc -1 nếu trống hay ngoài khoảng.</summary>
        public int Entry(int slot)
        {
            return slot >= 0 && slot < SignCount ? entries[slot] : -1;
        }

        /// <summary>Trả bản sao 4 ô nhập (-1 = trống).</summary>
        public int[] EntriesCopy()
        {
            return (int[])entries.Clone();
        }

        /// <summary>Trả true nếu cả 4 ô nhập đều đã có ký hiệu.</summary>
        public bool EntriesComplete
        {
            get
            {
                foreach (int v in entries) if (v < 0) return false;
                return true;
            }
        }
    }
}
