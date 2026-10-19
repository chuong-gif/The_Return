/*
 * Mục đích: Trạng thái chơi của «Hợp xướng» (T-006d): bấm bục theo bản nhạc, hạn chót giữa hai nốt CHỈNH ĐƯỢC (ChoirRules),
 * cộng giờ khi đổi vai, đọc thẻ nhạc (chỉ chủ thẻ), thắng khi bấm đủ dãy.
 * Thuần C#; thời gian do nơi gọi truyền vào (giây) để test và đồng bộ mạng được.
 * Hàm: constructor; ApplyRules; Press; Tick; ExtendForSwitch; RemainingSeconds; TryReadCard; Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Kết quả một lần bấm bục.</summary>
    public enum ChoirResult
    {
        Correct,
        Completed,
        Wrong,
        TooLate,
        InvalidPad,
        AlreadySolved
    }

    /// <summary>Trạng thái một lượt chơi Hợp xướng trên một đề.</summary>
    public sealed class ChoirState
    {
        double deadline = -1;
        double extraUsed;

        public ChoirRound Round { get; }
        /// <summary>Quy tắc thời gian hiện hành (có thể đổi bằng ApplyRules khi đang chơi).</summary>
        public ChoirRules Rules { get; private set; }
        /// <summary>Số nốt đã bấm đúng liên tiếp (0 = chưa bắt đầu dãy).</summary>
        public int Progress { get; private set; }
        /// <summary>Số lần bấm sai bục (mỗi lần reset dãy).</summary>
        public int Mistakes { get; private set; }
        /// <summary>Số lần quá giờ giữa hai nốt (mỗi lần reset dãy).</summary>
        public int Timeouts { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>Nhận đề và quy tắc thời gian (null = ChoirRules.Default); tạo trạng thái mới chưa bấm nốt nào.</summary>
        public ChoirState(ChoirRound round, ChoirRules rules = null)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            Round = round;
            Rules = rules ?? ChoirRules.Default;
        }

        /// <summary>
        /// Nhận quy tắc mới (không null); áp dụng cho các nốt SAU. Hạn chót của nốt đang chờ giữ nguyên.
        /// Dùng để chỉnh thông số ngay trong lúc chơi thử.
        /// </summary>
        public void ApplyRules(ChoirRules rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            Rules = rules;
        }

        /// <summary>Trả số giây còn lại tới hạn chót; -1 nếu không có giới hạn hoặc chưa bắt đầu dãy. Nhận thời gian hiện tại (giây).</summary>
        public double RemainingSeconds(double now)
        {
            if (Progress == 0 || deadline < 0) return -1;
            double r = deadline - now;
            return r < 0 ? 0 : r;
        }

        /// <summary>Nhận thời gian hiện tại; quá hạn chót thì reset dãy và đếm Timeouts. Gọi mỗi frame. Trả true nếu vừa reset vì quá giờ.</summary>
        public bool Tick(double now)
        {
            if (Solved || Progress == 0 || deadline < 0 || now <= deadline) return false;
            ResetSequence();
            Timeouts++;
            return true;
        }

        /// <summary>
        /// Nhận bục (0-3) và thời gian hiện tại. Quá hạn chót thì reset dãy, trả TooLate (lần bấm đó không được tính).
        /// Đúng thì tiến một nốt và đặt hạn chót mới; đúng nốt cuối thì thắng. Sai bục thì reset dãy, đếm Mistakes. Nốt đầu không có hạn chót.
        /// </summary>
        public ChoirResult Press(int pad, double now)
        {
            if (Solved) return ChoirResult.AlreadySolved;
            if (pad < 0 || pad >= ChoirRound.PadCount) return ChoirResult.InvalidPad;
            if (Tick(now)) return ChoirResult.TooLate;
            if (pad != Round.SolutionPad(Progress))
            {
                ResetSequence();
                Mistakes++;
                return ChoirResult.Wrong;
            }
            Progress++;
            if (Progress >= Round.Length)
            {
                Solved = true;
                deadline = -1;
                return ChoirResult.Completed;
            }
            extraUsed = 0;
            deadline = Rules.HasTimeLimit ? now + Rules.StepWindowSeconds : -1;
            return ChoirResult.Correct;
        }

        /// <summary>
        /// Nhận thời gian hiện tại; gọi khi người chơi ĐỔI VAI giữa dãy (phím 1-4 khi thử một máy). Cộng Rules.SwitchExtraSeconds vào hạn chót
        /// nhưng tổng cộng thêm cho nốt hiện tại không quá Rules.MaxExtraPerStep. Không có tác dụng khi không giới hạn thời gian,
        /// chưa bắt đầu dãy, đã thắng hoặc đã quá hạn. Trả số giây thực sự cộng thêm.
        /// </summary>
        public double ExtendForSwitch(double now)
        {
            if (Solved || Progress == 0 || deadline < 0 || !Rules.HasTimeLimit || Rules.SwitchExtraSeconds <= 0 || now > deadline) return 0;
            double room = Rules.MaxExtraPerStep - extraUsed;
            if (room <= 0) return 0;
            double add = Math.Min(Rules.SwitchExtraSeconds, room);
            deadline += add;
            extraUsed += add;
            return add;
        }

        /// <summary>
        /// Nhận người chơi và ký hiệu; chỉ chủ thẻ mới đọc được bục tương ứng. Trả true và gán chỉ số bục nếu được đọc,
        /// false nếu không phải chủ thẻ hoặc ký hiệu sai.
        /// </summary>
        public bool TryReadCard(int player, int symbol, out int pad)
        {
            pad = -1;
            if (symbol < 0 || symbol >= ChoirRound.PadCount || player < 0 || player >= Round.PlayerCount) return false;
            if (Round.OwnerOfSymbol(symbol) != player) return false;
            pad = Round.PadOfSymbol(symbol);
            return true;
        }

        /// <summary>Không nhận tham số; đặt lại về lúc đầu (dãy về nốt đầu, chưa thắng), GIỮ cùng đề, quy tắc và các bộ đếm Mistakes/Timeouts.</summary>
        public void Reset()
        {
            Solved = false;
            ResetSequence();
        }

        /// <summary>Không nhận tham số; đưa dãy về nốt đầu và xóa hạn chót (nội bộ).</summary>
        void ResetSequence()
        {
            Progress = 0;
            deadline = -1;
            extraUsed = 0;
        }
    }
}
