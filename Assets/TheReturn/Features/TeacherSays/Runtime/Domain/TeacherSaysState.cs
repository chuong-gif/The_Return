/*
 * Mục đích: Trạng thái chơi của «Cô nói» (T-006c): đoạn hiện tại, hạn chót khi phải BƯỚC, thời gian phải ĐỨNG YÊN, cảnh cáo,
 * về đầu hành lang khi đủ cảnh cáo, thắng khi qua đủ 5 đoạn. Thuần C#: thời gian và dữ liệu chuyển động do nơi gọi truyền vào
 * (giây, có ai đang đi không, cả đội đã vào vùng đoạn kế chưa) để test và đồng bộ mạng được.
 * Kiểu: TeacherSaysEvent, TeacherSaysState.
 * Hàm TeacherSaysState: constructor; ApplyRules; Begin; Tick; ExtendForSwitch; TryReadPrefix; RemainingSeconds; Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Sự kiện do Tick trả về (None khi không có gì đổi).</summary>
    public enum TeacherSaysEvent
    {
        None,
        /// <summary>Đã đứng yên đủ giờ; giờ được đi tự do tới vùng đoạn kế.</summary>
        StandOk,
        /// <summary>Cả đội vào vùng đoạn kế; sang đoạn mới.</summary>
        Advanced,
        /// <summary>Qua đoạn cuối: thắng.</summary>
        Completed,
        /// <summary>Vi phạm (hết giờ Bước hoặc cử động khi phải Đứng): cảnh cáo, lặp lại đoạn.</summary>
        Warned,
        /// <summary>Đủ cảnh cáo: cả đội về đầu hành lang (đề giữ nguyên).</summary>
        SentBack
    }

    /// <summary>Trạng thái một lượt chơi Cô nói trên một đề.</summary>
    public sealed class TeacherSaysState
    {
        /// <summary>Số giây đầu mỗi đoạn mà cử động vẫn chưa bị tính khi phải Đứng (người chơi kịp nghe lệnh và dừng).</summary>
        public const double ReactionSeconds = 1.0;

        double segmentStart;
        double deadline = -1;
        double extraUsed;
        bool standDone;

        public TeacherSaysRound Round { get; }
        /// <summary>Quy tắc hiện hành (có thể đổi bằng ApplyRules khi đang chơi).</summary>
        public TeacherSaysRules Rules { get; private set; }
        /// <summary>Đoạn hiện tại 0-4 (5 khi đã thắng).</summary>
        public int Segment { get; private set; }
        /// <summary>Số cảnh cáo trong lượt hiện tại (về 0 khi bị đưa về đầu).</summary>
        public int Warnings { get; private set; }
        /// <summary>Tổng số lần bị đưa về đầu hành lang.</summary>
        public int SentBacks { get; private set; }
        /// <summary>Tổng số cảnh cáo từ đầu tới giờ (không reset khi về đầu).</summary>
        public int TotalWarnings { get; private set; }
        public bool Started { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>Nhận đề và quy tắc (null = Default); tạo trạng thái chưa bắt đầu (gọi Begin).</summary>
        public TeacherSaysState(TeacherSaysRound round, TeacherSaysRules rules = null)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            Round = round;
            Rules = rules ?? TeacherSaysRules.Default;
        }

        /// <summary>Nhận quy tắc mới (không null); áp cho các đoạn SAU và cho cảnh cáo; hạn chót đang chạy giữ nguyên.</summary>
        public void ApplyRules(TeacherSaysRules rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            Rules = rules;
        }

        /// <summary>Nhận thời gian hiện tại (giây); bắt đầu đoạn 0. Không có tác dụng nếu đã thắng.</summary>
        public void Begin(double now)
        {
            if (Solved) return;
            Started = true;
            StartSegment(now);
        }

        /// <summary>Hành động đội PHẢI làm ở đoạn hiện tại (Step khi đã thắng không có nghĩa, trả Stand).</summary>
        public TeacherCommand RequiredNow { get { return Solved ? TeacherCommand.Stand : Round.RequiredAction(Segment); } }

        /// <summary>
        /// Nhận thời gian (giây), cờ «có ai đang di chuyển», cờ «cả đội đã trong vùng đoạn kế»; gọi mỗi frame.
        /// BƯỚC: vào vùng kế đúng hạn thì sang đoạn mới, quá hạn thì cảnh cáo. ĐỨNG: cử động (sau ReactionSeconds) thì cảnh cáo;
        /// đứng yên đủ StandSeconds thì StandOk, sau đó vào vùng kế (tự do) mới sang đoạn mới. Trả sự kiện vừa xảy ra.
        /// </summary>
        public TeacherSaysEvent Tick(double now, bool anyMoving, bool allInNextZone)
        {
            if (!Started || Solved) return TeacherSaysEvent.None;
            double elapsed = now - segmentStart;
            if (Round.RequiredAction(Segment) == TeacherCommand.Step)
            {
                if (allInNextZone) return Advance(now);
                if (Rules.HasStepLimit && deadline >= 0 && now > deadline) return Warn(now);
                return TeacherSaysEvent.None;
            }
            if (standDone) return allInNextZone ? Advance(now) : TeacherSaysEvent.None;
            if (anyMoving && elapsed > ReactionSeconds) return Warn(now);
            if (elapsed >= Rules.StandSeconds) { standDone = true; return TeacherSaysEvent.StandOk; }
            return TeacherSaysEvent.None;
        }

        /// <summary>
        /// Nhận thời gian; gọi khi ĐỔI VAI giữa đoạn BƯỚC (phím 1-4 khi thử một máy). Cộng Rules.SwitchExtraSeconds vào hạn chót,
        /// tổng cộng thêm mỗi đoạn không quá Rules.MaxExtraPerStep. Không tác dụng khi không phải Bước, không giới hạn, đã quá hạn hoặc đã thắng.
        /// Trả số giây thực sự cộng thêm.
        /// </summary>
        public double ExtendForSwitch(double now)
        {
            if (!Started || Solved || deadline < 0 || !Rules.HasStepLimit || Rules.SwitchExtraSeconds <= 0 || now > deadline) return 0;
            if (Round.RequiredAction(Segment) != TeacherCommand.Step) return 0;
            double room = Rules.MaxExtraPerStep - extraUsed;
            if (room <= 0) return 0;
            double add = Math.Min(Rules.SwitchExtraSeconds, room);
            deadline += add;
            extraUsed += add;
            return add;
        }

        /// <summary>
        /// Nhận người chơi; chỉ người nghe của đoạn hiện tại biết lệnh có «Cô nói» hay không. Trả true và gán cờ nếu được biết,
        /// false nếu không phải người nghe hoặc đã thắng.
        /// </summary>
        public bool TryReadPrefix(int player, out bool teacherSaid)
        {
            teacherSaid = false;
            if (Solved || Segment >= TeacherSaysRound.SegmentCount || Round.Listener(Segment) != player) return false;
            teacherSaid = Round.TeacherSaid(Segment);
            return true;
        }

        /// <summary>
        /// Nhận thời gian; trả số giây còn lại của bước hiện tại: khi Bước là tới hạn chót, khi Đứng là thời gian đứng yên còn lại
        /// (0 khi đã đứng đủ). -1 nếu không giới hạn hoặc chưa bắt đầu/đã thắng.
        /// </summary>
        public double RemainingSeconds(double now)
        {
            if (!Started || Solved) return -1;
            if (Round.RequiredAction(Segment) == TeacherCommand.Step)
            {
                if (!Rules.HasStepLimit || deadline < 0) return -1;
                double r = deadline - now;
                return r < 0 ? 0 : r;
            }
            if (standDone) return 0;
            double left = segmentStart + Rules.StandSeconds - now;
            return left < 0 ? 0 : left;
        }

        /// <summary>True khi ở đoạn ĐỨNG và đã đứng đủ giờ (được đi tự do tới vùng kế).</summary>
        public bool StandSatisfied { get { return standDone; } }

        /// <summary>Không nhận tham số; về đoạn 0, chưa bắt đầu, xóa cảnh cáo và cờ thắng; GIỮ đề, quy tắc và bộ đếm SentBacks/TotalWarnings.</summary>
        public void Reset()
        {
            Segment = 0;
            Warnings = 0;
            Solved = false;
            Started = false;
            deadline = -1;
            extraUsed = 0;
            standDone = false;
        }

        /// <summary>Nhận thời gian; sang đoạn kế hoặc thắng nếu hết đoạn (nội bộ).</summary>
        TeacherSaysEvent Advance(double now)
        {
            Segment++;
            if (Segment >= TeacherSaysRound.SegmentCount)
            {
                Solved = true;
                deadline = -1;
                return TeacherSaysEvent.Completed;
            }
            StartSegment(now);
            return TeacherSaysEvent.Advanced;
        }

        /// <summary>Nhận thời gian; tính một cảnh cáo, về đầu nếu đủ, lặp lại đoạn (nội bộ).</summary>
        TeacherSaysEvent Warn(double now)
        {
            Warnings++;
            TotalWarnings++;
            if (Warnings >= Rules.MaxWarnings)
            {
                Warnings = 0;
                SentBacks++;
                Segment = 0;
                StartSegment(now);
                return TeacherSaysEvent.SentBack;
            }
            StartSegment(now);
            return TeacherSaysEvent.Warned;
        }

        /// <summary>Nhận thời gian; đặt lại đồng hồ cho đoạn hiện tại (nội bộ).</summary>
        void StartSegment(double now)
        {
            segmentStart = now;
            extraUsed = 0;
            standDone = false;
            deadline = Round.RequiredAction(Segment) == TeacherCommand.Step && Rules.HasStepLimit ? now + Rules.StepWindowSeconds : -1;
        }
    }
}
