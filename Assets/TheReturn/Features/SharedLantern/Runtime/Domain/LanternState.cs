/*
 * Mục đích: Trạng thái chơi của «Đèn lồng chung» (T-006f): ai đang cầm đèn, ai đã đọc biển nào (sổ riêng từng người), ai ở
 * ngoài vùng sáng quá lâu, bảng nhập ký hiệu cuối hành lang, và điều kiện hoàn thành. Thuần C#; thời gian và các phép đo không
 * gian (người nào trong vùng sáng, khoảng cách khi trao đèn, biển nào trong vùng sáng) do nơi gọi truyền vào để test và đồng bộ
 * mạng được.
 * Hàm: constructor; TryPass; Read; HasRead; ReadMask; TeamReadMask; SymbolSeen; Tick; OutsideSeconds; SetEntry; Entry;
 * ClearEntries; Submit; LockRemaining; TryComplete; Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Kết quả đọc một biển.</summary>
    public enum LanternReadResult { Read, AlreadyRead, NotInLight, InvalidPlayer, InvalidSign }

    /// <summary>Kết quả trao đèn lồng.</summary>
    public enum LanternPassResult { Passed, NotHolder, TooFar, InvalidPlayer }

    /// <summary>Kết quả nhập bảng ký hiệu cuối hành lang.</summary>
    public enum LanternSubmitResult { Accepted, WrongCode, Incomplete, AlreadyAccepted, Locked }

    /// <summary>Trạng thái một lượt chơi «Đèn lồng chung».</summary>
    public sealed class LanternState
    {
        readonly int[] readMasks;
        readonly double[] outsideSince;
        readonly int[] entries = { -1, -1, -1, -1 };
        double lockedUntil = double.NegativeInfinity;

        /// <summary>Đề đang chơi.</summary>
        public LanternRound Round { get; }

        /// <summary>Số giây tối đa được đứng ngoài vùng sáng (thiết kế: 5).</summary>
        public double OutsideLimit { get; }

        /// <summary>Khoảng cách tối đa (mét) để trao đèn lồng (thiết kế: 2).</summary>
        public double PassRange { get; }

        /// <summary>Số giây bảng nhập bị khóa sau mỗi lần nhập sai (phạt; chủ dự án yêu cầu có phạt, mặc định 10).</summary>
        public double SubmitLockSeconds { get; }

        /// <summary>Chỉ số người đang cầm đèn lồng (ban đầu là người 0).</summary>
        public int Holder { get; private set; }

        /// <summary>Tổng số lần có người bị đưa về điểm đầu vì ở ngoài vùng sáng quá lâu.</summary>
        public int SentBackCount { get; private set; }

        /// <summary>Số lần nhập sai bảng ký hiệu.</summary>
        public int WrongSubmits { get; private set; }

        /// <summary>True khi đã nhập đúng bảng ký hiệu cuối hành lang.</summary>
        public bool CodeAccepted { get; private set; }

        /// <summary>True khi đã nhập đúng và cả đội đã tới cổng.</summary>
        public bool Solved { get; private set; }

        /// <summary>Nhận đề, số giây tối đa ngoài vùng sáng, tầm trao đèn; mọi tham số phải hợp lệ nếu không ném ngoại lệ.</summary>
        public LanternState(LanternRound round, double outsideLimit = 5.0, double passRange = 2.0, double submitLockSeconds = 10.0)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            if (!(outsideLimit > 0)) throw new ArgumentOutOfRangeException(nameof(outsideLimit));
            if (!(passRange > 0)) throw new ArgumentOutOfRangeException(nameof(passRange));
            if (!(submitLockSeconds >= 0)) throw new ArgumentOutOfRangeException(nameof(submitLockSeconds));
            Round = round;
            OutsideLimit = outsideLimit;
            PassRange = passRange;
            SubmitLockSeconds = submitLockSeconds;
            readMasks = new int[round.PlayerCount];
            outsideSince = new double[round.PlayerCount];
            ResetTimers();
        }

        bool ValidPlayer(int player) { return player >= 0 && player < Round.PlayerCount; }

        /// <summary>Không nhận gì; đặt mọi đồng hồ ngoài vùng sáng về trạng thái đang trong vùng sáng.</summary>
        void ResetTimers() { for (int i = 0; i < outsideSince.Length; i++) outsideSince[i] = double.NaN; }

        /// <summary>Nhận người trao, người nhận và khoảng cách giữa họ (mét); chỉ người đang cầm đèn mới trao được, trong tầm PassRange.</summary>
        public LanternPassResult TryPass(int from, int to, double distance)
        {
            if (!ValidPlayer(from) || !ValidPlayer(to) || from == to) return LanternPassResult.InvalidPlayer;
            if (from != Holder) return LanternPassResult.NotHolder;
            if (!(distance <= PassRange)) return LanternPassResult.TooFar;
            Holder = to;
            return LanternPassResult.Passed;
        }

        /// <summary>Nhận người đọc, chỉ số biển (0..3) và biển có đang trong vùng sáng không; ghi vào sổ riêng của người đó nếu đọc được.</summary>
        public LanternReadResult Read(int player, int sign, bool signInLight)
        {
            if (!ValidPlayer(player)) return LanternReadResult.InvalidPlayer;
            if (sign < 0 || sign >= LanternRound.SignCount) return LanternReadResult.InvalidSign;
            if ((readMasks[player] >> sign & 1) == 1) return LanternReadResult.AlreadyRead;
            if (!signInLight) return LanternReadResult.NotInLight;
            readMasks[player] |= 1 << sign;
            return LanternReadResult.Read;
        }

        /// <summary>Nhận người và biển; trả true nếu người đó đã tự đọc biển này.</summary>
        public bool HasRead(int player, int sign)
        {
            return ValidPlayer(player) && sign >= 0 && sign < LanternRound.SignCount && (readMasks[player] >> sign & 1) == 1;
        }

        /// <summary>Nhận người; trả mặt nạ bit các biển người đó đã đọc (bit i = biển i), 0 nếu chỉ số sai.</summary>
        public int ReadMask(int player) { return ValidPlayer(player) ? readMasks[player] : 0; }

        /// <summary>Không nhận gì; trả mặt nạ bit các biển ít nhất một người đã đọc (hợp sổ của cả đội, dùng để kiểm tra đội đủ thông tin).</summary>
        public int TeamReadMask()
        {
            int all = 0;
            for (int i = 0; i < readMasks.Length; i++) all |= readMasks[i];
            return all;
        }

        /// <summary>Nhận người và biển; trả ký hiệu nếu CHÍNH người đó đã đọc biển, ngược lại -1 (sổ không chia sẻ cho người khác).</summary>
        public int SymbolSeen(int player, int sign) { return HasRead(player, sign) ? Round.SymbolOfSign(sign) : -1; }

        /// <summary>Nhận thời điểm hiện tại (giây) và mảng cho biết từng người có đang trong vùng sáng không; trả mặt nạ bit những người bị đưa về điểm đầu ở lần gọi này. Người cầm đèn luôn được coi là trong vùng sáng.</summary>
        public int Tick(double now, bool[] inLight)
        {
            if (inLight == null || inLight.Length != Round.PlayerCount) throw new ArgumentException("inLight phải có đúng số phần tử bằng số người chơi", nameof(inLight));
            int sentBack = 0;
            for (int i = 0; i < outsideSince.Length; i++)
            {
                if (inLight[i] || i == Holder) { outsideSince[i] = double.NaN; continue; }
                if (double.IsNaN(outsideSince[i])) { outsideSince[i] = now; continue; }
                if (now - outsideSince[i] > OutsideLimit)
                {
                    sentBack |= 1 << i;
                    SentBackCount++;
                    outsideSince[i] = now;
                }
            }
            return sentBack;
        }

        /// <summary>Nhận người và thời điểm hiện tại; trả số giây người đó đã ở ngoài vùng sáng liên tục (0 nếu đang trong vùng sáng).</summary>
        public double OutsideSeconds(int player, double now)
        {
            if (!ValidPlayer(player) || double.IsNaN(outsideSince[player])) return 0;
            return Math.Max(0, now - outsideSince[player]);
        }

        /// <summary>Nhận ô nhập (0..3) và ký hiệu (0..5); trả false nếu sai phạm vi hoặc bảng đã được chấp nhận.</summary>
        public bool SetEntry(int slot, int symbol)
        {
            if (CodeAccepted || slot < 0 || slot >= LanternRound.SignCount || symbol < 0 || symbol >= LanternRound.SymbolCount) return false;
            entries[slot] = symbol;
            return true;
        }

        /// <summary>Nhận ô nhập; trả ký hiệu đã nhập hoặc -1 nếu trống hay sai chỉ số.</summary>
        public int Entry(int slot) { return slot >= 0 && slot < LanternRound.SignCount ? entries[slot] : -1; }

        /// <summary>Không nhận gì; xóa cả 4 ô nhập (không được xóa khi bảng đã chấp nhận).</summary>
        public void ClearEntries()
        {
            if (CodeAccepted) return;
            for (int i = 0; i < entries.Length; i++) entries[i] = -1;
        }

        /// <summary>Nhận thời điểm hiện tại (giây); kiểm bảng nhập với đáp án (ký hiệu biển 1,2,3,4 theo thứ tự). Đang bị khóa thì trả Locked. Nhập sai bị PHẠT: xóa cả 4 ô và khóa bảng SubmitLockSeconds giây.</summary>
        public LanternSubmitResult Submit(double now)
        {
            if (CodeAccepted) return LanternSubmitResult.AlreadyAccepted;
            if (now < lockedUntil) return LanternSubmitResult.Locked;
            for (int i = 0; i < entries.Length; i++) if (entries[i] < 0) return LanternSubmitResult.Incomplete;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != Round.SymbolOfSign(i))
                {
                    WrongSubmits++;
                    lockedUntil = now + SubmitLockSeconds;
                    for (int k = 0; k < entries.Length; k++) entries[k] = -1;
                    return LanternSubmitResult.WrongCode;
                }
            }
            CodeAccepted = true;
            return LanternSubmitResult.Accepted;
        }

        /// <summary>Nhận thời điểm hiện tại (giây); trả số giây còn bị khóa sau lần nhập sai (0 nếu không bị khóa).</summary>
        public double LockRemaining(double now) { return now < lockedUntil ? lockedUntil - now : 0; }

        /// <summary>Nhận allAtGate (cả đội đã ở cổng cuối hành lang chưa); hoàn thành khi bảng đã đúng và cả đội đã tới. Trả Solved.</summary>
        public bool TryComplete(bool allAtGate)
        {
            if (CodeAccepted && allAtGate) Solved = true;
            return Solved;
        }

        /// <summary>Không nhận gì; thử lại cùng đề: xóa sổ mọi người, bảng nhập, đồng hồ, bộ đếm; đèn về người 0.</summary>
        public void Reset()
        {
            for (int i = 0; i < readMasks.Length; i++) readMasks[i] = 0;
            for (int i = 0; i < entries.Length; i++) entries[i] = -1;
            ResetTimers();
            Holder = 0;
            SentBackCount = 0;
            WrongSubmits = 0;
            lockedUntil = double.NegativeInfinity;
            CodeAccepted = false;
            Solved = false;
        }
    }
}
