/*
 * Mục đích: Trạng thái CHUNG (mọi người cùng thấy) và thông tin RIÊNG (chỉ người nghe đoạn hiện tại) của «Cô nói» online (T-037).
 * Chung: đã bắt đầu/đã thắng, đoạn hiện tại, lệnh loa (Bước/Đứng), AI là người nghe tiền tố, số cảnh cáo, lần bị đưa về, lần làm lại, sự kiện gần nhất.
 * KHÔNG chứa: seed, cờ «Cô nói», hành động đúng phải làm, đồng hồ đếm ngược, trạng thái «đã đứng đủ giờ». Đồng hồ Bước (8 giây) và Đứng (5 giây)
 * khác nhau nên nếu gửi cho mọi người thì ai cũng suy ra đáp án mà không cần người nghe (xem PLANS/T-037-timing-zone-missions.md, mục TeacherSays - mạng).
 * Riêng: người nghe nhận cờ «Cô nói», số mili giây còn lại và cờ đã đứng đủ giờ; người khác nhận trạng thái rỗng.
 * Bất biến sau khi tạo. Thuần C#, không phụ thuộc Unity.
 * Kiểu: TeacherSaysNetEvent, TeacherSaysPrivateState, TeacherSaysPublicState.
 * Hàm: (TeacherSaysPrivateState) constructor, None, RemainingSeconds, TryGetRequiredAction; (TeacherSaysPublicState) constructor, Active.
 */
using System;

namespace TheReturn.TeacherSaysNet
{
    /// <summary>Sự kiện gần nhất để HUD/âm thanh phản ứng (mã enum, chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum TeacherSaysNetEvent
    {
        None = 0,
        /// <summary>Host vừa bắt đầu nhiệm vụ.</summary>
        Started = 1,
        /// <summary>Đã đứng yên đủ giờ ở đoạn Đứng; được đi tự do tới vùng kế.</summary>
        StandOk = 2,
        /// <summary>Cả đội vào vùng đoạn kế; sang đoạn mới.</summary>
        Advanced = 3,
        /// <summary>Qua đủ 5 đoạn: thắng.</summary>
        Completed = 4,
        /// <summary>Vi phạm: cảnh cáo, lặp lại đoạn.</summary>
        Warned = 5,
        /// <summary>Đủ cảnh cáo: cả đội bị đưa về đầu hành lang.</summary>
        SentBack = 6,
        /// <summary>Một người bấm làm lại.</summary>
        Retry = 7
    }

    /// <summary>Thông tin riêng của một ô: chỉ người nghe của đoạn hiện tại có nội dung, người khác nhận None.</summary>
    public readonly struct TeacherSaysPrivateState
    {
        /// <summary>Giới hạn mili giây còn lại hợp lệ (10 phút).</summary>
        public const int MaxRemainingMs = 600000;

        /// <summary>True khi ô này là người nghe tiền tố của đoạn hiện tại.</summary>
        public bool IsListener { get; }

        /// <summary>Chỉ có nghĩa khi IsListener: lệnh loa có «Cô nói» (làm đúng) hay không (làm ngược).</summary>
        public bool TeacherSaid { get; }

        /// <summary>Chỉ có nghĩa khi IsListener: mili giây còn lại của bước hiện tại (Bước: tới hạn chót; Đứng: thời gian đứng yên còn lại); -1 nếu không giới hạn.</summary>
        public int RemainingMs { get; }

        /// <summary>Chỉ có nghĩa khi IsListener: đã đứng đủ giờ ở đoạn Đứng.</summary>
        public bool StandSatisfied { get; }

        /// <summary>
        /// Nhận cờ người nghe, cờ «Cô nói», mili giây còn lại (-1..MaxRemainingMs) và cờ đã đứng đủ giờ. Ném lỗi nếu ngoài khoảng, hoặc nếu người KHÔNG nghe
        /// mà mang thông tin riêng (chặn lỡ tay làm lộ).
        /// </summary>
        public TeacherSaysPrivateState(bool isListener, bool teacherSaid, int remainingMs, bool standSatisfied)
        {
            if (remainingMs < -1 || remainingMs > MaxRemainingMs) throw new ArgumentOutOfRangeException("remainingMs");
            if (!isListener && (teacherSaid || standSatisfied || remainingMs != -1))
                throw new ArgumentException("Người không phải người nghe không được mang thông tin riêng.");
            IsListener = isListener;
            TeacherSaid = teacherSaid;
            RemainingMs = remainingMs;
            StandSatisfied = standSatisfied;
        }

        /// <summary>Không nhận tham số; trả thông tin riêng rỗng (người không nghe, hoặc nhiệm vụ chưa bắt đầu/đã thắng).</summary>
        public static TeacherSaysPrivateState None
        {
            get { return new TeacherSaysPrivateState(false, false, -1, false); }
        }

        /// <summary>Không nhận tham số; trả số giây còn lại, hoặc -1 nếu không phải người nghe hay không giới hạn.</summary>
        public float RemainingSeconds
        {
            get { return IsListener && RemainingMs >= 0 ? RemainingMs / 1000f : -1f; }
        }

        /// <summary>
        /// Nhận lệnh loa đang phát (từ trạng thái chung); trả true và gán hành động ĐỘI PHẢI làm (lệnh thật giữ nguyên, lệnh giả đảo ngược)
        /// nếu ô này là người nghe, ngược lại trả false (required = Stand, không có nghĩa).
        /// </summary>
        public bool TryGetRequiredAction(TeacherCommand shown, out TeacherCommand required)
        {
            required = TeacherCommand.Stand;
            if (!IsListener) return false;
            required = TeacherSaid ? shown : (shown == TeacherCommand.Step ? TeacherCommand.Stand : TeacherCommand.Step);
            return true;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ Cô nói.</summary>
    public sealed class TeacherSaysPublicState
    {
        /// <summary>Số đoạn của hành lang (5).</summary>
        public const int SegmentCount = TeacherSaysRound.SegmentCount;

        /// <summary>Giới hạn số cảnh cáo tối đa chấp nhận khi giải mã.</summary>
        public const int MaxWarningsLimit = 100;

        /// <summary>Giới hạn các bộ đếm (lần bị đưa về, tổng cảnh cáo, lần làm lại) khi giải mã.</summary>
        public const int MaxCounter = 1000000;

        /// <summary>Số người của đề (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>True sau khi host bắt đầu nhiệm vụ.</summary>
        public bool Started { get; }

        /// <summary>True khi đã qua đủ 5 đoạn.</summary>
        public bool Solved { get; }

        /// <summary>Đoạn hiện tại 0-4; bằng 5 khi đã thắng; 0 khi chưa bắt đầu.</summary>
        public int Segment { get; }

        /// <summary>Lệnh loa đang phát (ai cũng nghe). Chỉ có nghĩa khi Active.</summary>
        public TeacherCommand Command { get; }

        /// <summary>Ô người chơi nghe được tiền tố «Cô nói» ở đoạn hiện tại (ai cũng biết AI nghe, nhưng chỉ người đó biết nội dung); -1 khi không Active.</summary>
        public int Listener { get; }

        /// <summary>Số cảnh cáo trong lượt hiện tại (về 0 khi bị đưa về đầu).</summary>
        public int Warnings { get; }

        /// <summary>Số cảnh cáo khiến cả đội bị đưa về đầu.</summary>
        public int MaxWarnings { get; }

        /// <summary>Số lần cả đội bị đưa về đầu hành lang (lớp Unity dựa vào số này để dịch chuyển đội).</summary>
        public int SentBacks { get; }

        /// <summary>Tổng số cảnh cáo từ đầu.</summary>
        public int TotalWarnings { get; }

        /// <summary>Số lần đã bấm làm lại (lớp Unity dựa vào số này để dịch chuyển đội).</summary>
        public int RetryCount { get; }

        /// <summary>Sự kiện gần nhất.</summary>
        public TeacherSaysNetEvent LastEvent { get; }

        /// <summary>True khi đã bắt đầu và chưa thắng (đang có lệnh loa).</summary>
        public bool Active { get { return Started && !Solved; } }

        /// <summary>
        /// Nhận số người, cờ bắt đầu, cờ thắng, đoạn, lệnh loa, người nghe, cảnh cáo, số cảnh cáo tối đa, lần bị đưa về, tổng cảnh cáo, lần làm lại và sự kiện gần nhất.
        /// Ném lỗi nếu dữ liệu sai hình dạng: thắng mà chưa bắt đầu, đoạn không khớp cờ thắng, người nghe sai khi Active/không Active, cảnh cáo không nhỏ hơn mức tối đa, v.v.
        /// </summary>
        public TeacherSaysPublicState(int playerCount, bool started, bool solved, int segment, TeacherCommand command, int listener,
            int warnings, int maxWarnings, int sentBacks, int totalWarnings, int retryCount, TeacherSaysNetEvent lastEvent)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            if (solved && !started) throw new ArgumentException("Thắng thì phải đã bắt đầu.", "solved");
            if (segment < 0 || segment > SegmentCount) throw new ArgumentOutOfRangeException("segment");
            if (solved != (segment == SegmentCount)) throw new ArgumentException("Đoạn bằng 5 khi và chỉ khi đã thắng.", "segment");
            if (!Enum.IsDefined(typeof(TeacherCommand), command)) throw new ArgumentOutOfRangeException("command");
            if (!Enum.IsDefined(typeof(TeacherSaysNetEvent), lastEvent)) throw new ArgumentOutOfRangeException("lastEvent");
            if (maxWarnings < 1 || maxWarnings > MaxWarningsLimit) throw new ArgumentOutOfRangeException("maxWarnings");
            if (warnings < 0 || warnings >= maxWarnings) throw new ArgumentOutOfRangeException("warnings");
            if (sentBacks < 0 || sentBacks > MaxCounter) throw new ArgumentOutOfRangeException("sentBacks");
            if (totalWarnings < warnings || totalWarnings > MaxCounter) throw new ArgumentOutOfRangeException("totalWarnings");
            if (retryCount < 0 || retryCount > MaxCounter) throw new ArgumentOutOfRangeException("retryCount");
            bool active = started && !solved;
            if (active)
            {
                if (listener < 0 || listener >= playerCount) throw new ArgumentOutOfRangeException("listener");
            }
            else if (listener != -1)
            {
                throw new ArgumentException("Không Active thì không có người nghe.", "listener");
            }
            if (!started && (segment != 0 || warnings != 0)) throw new ArgumentException("Chưa bắt đầu thì đoạn và cảnh cáo phải bằng 0.");

            PlayerCount = playerCount;
            Started = started;
            Solved = solved;
            Segment = segment;
            Command = command;
            Listener = listener;
            Warnings = warnings;
            MaxWarnings = maxWarnings;
            SentBacks = sentBacks;
            TotalWarnings = totalWarnings;
            RetryCount = retryCount;
            LastEvent = lastEvent;
        }
    }
}
