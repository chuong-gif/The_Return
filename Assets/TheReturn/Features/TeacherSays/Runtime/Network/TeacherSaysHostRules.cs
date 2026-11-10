/*
 * Mục đích: LUẬT HOST của «Cô nói» khi chơi online (T-037): bọc TeacherSaysState + TeacherSaysGenerator (Domain có sẵn, KHÔNG đổi) theo giao diện
 * IHostRules của khung T-024. ĐỒNG HỒ và «ai đứng đâu, ai đang đi» đều do HOST quyết từ ITeacherSaysWorld (vị trí host thấy), máy khách không khai gì.
 * Máy khách chỉ gửi lệnh làm lại. Trạng thái chung (đoạn, lệnh loa, người nghe, cảnh cáo...) gửi mọi người; cờ «Cô nói», đồng hồ và cờ đã đứng đủ giờ
 * CHỈ gửi RIÊNG cho người nghe của đoạn hiện tại. Seed và hành động đúng không bao giờ rời host. Mặc định dùng quy tắc online (Bước 8 giây, Đứng 5 giây, 3 cảnh cáo).
 * Theo mẫu PLANS/T-027-plank-network-pattern.md và Breaker (T-031). Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor (hai dạng); Begin; Validate; Apply; BuildPublicState; BuildPrivateState; Tick; thuộc tính State, Round, PlayerCount, Started, Solved, RetryCount, SentBacks, LastEvent, Clock.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.TeacherSaysNet
{
    /// <summary>Luật host của nhiệm vụ Cô nói (lệnh TeacherSaysCommand, trạng thái chung TeacherSaysPublicState, riêng TeacherSaysPrivateState).</summary>
    public sealed class TeacherSaysHostRules : IHostRules<TeacherSaysCommand, TeacherSaysPublicState, TeacherSaysPrivateState>
    {
        readonly TeacherSaysState state;
        readonly ITeacherSaysWorld world;
        double clock;
        int retryCount;
        TeacherSaysNetEvent lastEvent;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh, Begin và Tick).</summary>
        public TeacherSaysState State { get { return state; } }

        /// <summary>Đề đang chơi (chỉ host/test được đọc; KHÔNG gửi cho máy khách).</summary>
        public TeacherSaysRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.Round.PlayerCount; } }

        /// <summary>True sau khi Begin.</summary>
        public bool Started { get { return state.Started; } }

        /// <summary>True sau khi qua đủ 5 đoạn.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>Số lần đã làm lại.</summary>
        public int RetryCount { get { return retryCount; } }

        /// <summary>Số lần cả đội bị đưa về đầu hành lang.</summary>
        public int SentBacks { get { return state.SentBacks; } }

        /// <summary>Sự kiện gần nhất.</summary>
        public TeacherSaysNetEvent LastEvent { get { return lastEvent; } }

        /// <summary>Đồng hồ của host (giây cộng dồn qua Tick).</summary>
        public double Clock { get { return clock; } }

        /// <summary>
        /// Nhận số người (2-4), seed, nguồn vật lý của host và quy tắc (null = TeacherSaysRules.Online). Sinh đề theo seed.
        /// Ném lỗi nếu tham số sai.
        /// </summary>
        public TeacherSaysHostRules(int playerCount, int seed, ITeacherSaysWorld world, TeacherSaysRules rules = null)
            : this(TeacherSaysGenerator.Generate(playerCount, seed), world, rules)
        {
        }

        /// <summary>Nhận đề có sẵn (cho test), nguồn vật lý của host và quy tắc (null = TeacherSaysRules.Online). Ném lỗi nếu đề hoặc nguồn là null.</summary>
        public TeacherSaysHostRules(TeacherSaysRound round, ITeacherSaysWorld world, TeacherSaysRules rules = null)
        {
            if (round == null) throw new ArgumentNullException("round");
            if (world == null) throw new ArgumentNullException("world");
            state = new TeacherSaysState(round, rules ?? TeacherSaysRules.Online);
            this.world = world;
        }

        /// <summary>
        /// Không nhận tham số; bắt đầu nhiệm vụ ở đoạn 0 (do lớp Unity gọi khi đội vào khu, KHÔNG phải lệnh của máy khách). Gọi lại khi đã bắt đầu
        /// hoặc đã thắng không làm gì. Trả true nếu vừa bắt đầu (lớp Unity phải gọi HostAuthority.MarkChanged()).
        /// </summary>
        public bool Begin()
        {
            if (state.Started || state.Solved) return false;
            state.Begin(clock);
            lastEvent = TeacherSaysNetEvent.Started;
            return true;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, TeacherSaysCommand command)
        {
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            if (command == null || command.Kind != TeacherSaysCommandKind.Retry) return RuleVerdict.Reject("Lệnh không hợp lệ.");
            if (!state.Started) return RuleVerdict.Reject("Nhiệm vụ chưa bắt đầu.");
            if (state.Solved) return RuleVerdict.Reject("Đã qua đủ 5 đoạn, không cần làm lại.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, TeacherSaysCommand command)
        {
            if (command.Kind != TeacherSaysCommandKind.Retry) throw new InvalidOperationException("Lệnh chưa được Validate: " + command.Kind);
            state.Reset();
            state.Begin(clock);
            retryCount++;
            lastEvent = TeacherSaysNetEvent.Retry;
        }

        /// <summary>
        /// Chụp trạng thái chung (bản mới, bất biến). KHÔNG chứa seed, cờ «Cô nói», hành động đúng, đồng hồ hay cờ đã đứng đủ giờ:
        /// các thứ đó chỉ nằm trong BuildPrivateState của người nghe.
        /// </summary>
        public TeacherSaysPublicState BuildPublicState()
        {
            bool active = state.Started && !state.Solved;
            int segment = state.Segment;
            TeacherCommand command = active ? state.Round.Command(segment) : TeacherCommand.Step;
            int listener = active ? state.Round.Listener(segment) : -1;
            return new TeacherSaysPublicState(PlayerCount, state.Started, state.Solved, segment, command, listener,
                state.Warnings, state.Rules.MaxWarnings, state.SentBacks, state.TotalWarnings, retryCount, lastEvent);
        }

        /// <summary>
        /// Nhận ô; trả thông tin riêng của đúng ô đó. Chỉ người nghe của đoạn hiện tại (khi nhiệm vụ đang chạy) nhận cờ «Cô nói», giờ còn lại và cờ đã đứng đủ giờ;
        /// mọi ô khác (và mọi ô khi chưa bắt đầu/đã thắng/ô sai) nhận None.
        /// </summary>
        public TeacherSaysPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return TeacherSaysPrivateState.None;
            if (!state.Started || state.Solved) return TeacherSaysPrivateState.None;
            bool said;
            if (!state.TryReadPrefix(slot, out said)) return TeacherSaysPrivateState.None;
            double left = state.RemainingSeconds(clock);
            int ms = -1;
            if (left >= 0)
            {
                double rounded = Math.Round(left * 1000.0);
                ms = rounded > TeacherSaysPrivateState.MaxRemainingMs ? TeacherSaysPrivateState.MaxRemainingMs : (int)rounded;
            }
            return new TeacherSaysPrivateState(true, said, ms, state.StandSatisfied);
        }

        /// <summary>
        /// Đồng hồ chạy trên host. Nhận số giây đã trôi (giá trị âm, NaN hoặc vô cực bị bỏ qua). Luôn gọi world.Sample (giữ mốc đo chuyển động mới), rồi nếu đang chạy thì
        /// hỏi world từng ô «đang đi không» và «đã trong vùng đoạn hiện tại chưa», đưa vào TeacherSaysState.Tick. Trả true khi có sự kiện làm trạng thái đổi mà không do
        /// lệnh nào (cảnh cáo, đưa về, qua đoạn, thắng, đứng đủ giờ), để lớp Unity gọi HostAuthority.MarkChanged().
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f) return false;
            clock += deltaSeconds;
            world.Sample(deltaSeconds);
            if (!state.Started || state.Solved) return false;

            bool anyMoving = false;
            for (int slot = 0; slot < PlayerCount; slot++)
            {
                if (world.IsMoving(slot)) { anyMoving = true; break; }
            }
            bool allInZone = state.Segment < TeacherSaysRound.SegmentCount;
            for (int slot = 0; allInZone && slot < PlayerCount; slot++)
            {
                if (!world.IsInZone(slot, state.Segment)) allInZone = false;
            }

            TeacherSaysEvent result = state.Tick(clock, anyMoving, allInZone);
            TeacherSaysNetEvent mapped = Map(result);
            if (mapped == TeacherSaysNetEvent.None) return false;
            lastEvent = mapped;
            return true;
        }

        /// <summary>Nhận sự kiện của Domain; trả sự kiện mạng tương ứng (None nếu không có gì đổi).</summary>
        static TeacherSaysNetEvent Map(TeacherSaysEvent e)
        {
            switch (e)
            {
                case TeacherSaysEvent.StandOk: return TeacherSaysNetEvent.StandOk;
                case TeacherSaysEvent.Advanced: return TeacherSaysNetEvent.Advanced;
                case TeacherSaysEvent.Completed: return TeacherSaysNetEvent.Completed;
                case TeacherSaysEvent.Warned: return TeacherSaysNetEvent.Warned;
                case TeacherSaysEvent.SentBack: return TeacherSaysNetEvent.SentBack;
                default: return TeacherSaysNetEvent.None;
            }
        }
    }
}
