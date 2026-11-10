/*
 * Mục đích: LUẬT HOST của «Hợp xướng» khi chơi online (T-037): bọc ChoirState + ChoirGenerator (Domain có sẵn, KHÔNG đổi) theo giao diện
 * IHostRules của khung T-024. Host nhận lệnh bấm bục và làm lại, kiểm điều kiện (ô hợp lệ, bục hợp lệ, đứng gần bục bằng IChoirWorld, chưa thắng)
 * rồi chạy luật. Đồng hồ giữa hai nốt chạy ở host (Tick) với cửa sổ ChoirRules.OnlineFor(số người): 3 / 2,5 / 2 giây (P-003, PROVISIONAL).
 * Trạng thái chung (bản nhạc, nốt tới, số lần sai/quá giờ) gửi mọi người; thẻ nhạc (ký hiệu -> bục) CHỈ gửi RIÊNG cho chủ thẻ.
 * Seed, bảng ký hiệu -> bục và đáp án không bao giờ rời host. Theo mẫu BreakerHostRules / PLANS/T-027-plank-network-pattern.md.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor (hai dạng); Validate; Apply; BuildPublicState; BuildPrivateState; Tick; thuộc tính State, Round, PlayerCount, ResetCount, Solved.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.ChoirNet
{
    /// <summary>Luật host của nhiệm vụ Hợp xướng (lệnh ChoirCommand, trạng thái chung ChoirPublicState, riêng ChoirPrivateState).</summary>
    public sealed class ChoirHostRules : IHostRules<ChoirCommand, ChoirPublicState, ChoirPrivateState>
    {
        readonly ChoirState state;
        readonly IChoirWorld world;
        double clock;
        int resetCount;
        ChoirMessage lastMessage;
        int lastActor = -1;
        int lastPad = -1;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh và Tick).</summary>
        public ChoirState State { get { return state; } }

        /// <summary>Đề đang chơi (chỉ host/test được đọc; KHÔNG gửi cho máy khách).</summary>
        public ChoirRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.Round.PlayerCount; } }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True sau khi đã hát đủ bản nhạc.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>
        /// Nhận số người (2-4), seed, nguồn vật lý của host và quy tắc thời gian (null = ChoirRules.OnlineFor(số người)).
        /// Sinh đề theo seed. Ném lỗi nếu tham số sai.
        /// </summary>
        public ChoirHostRules(int playerCount, int seed, IChoirWorld world, ChoirRules rules = null)
            : this(ChoirGenerator.Generate(playerCount, seed), world, rules)
        {
        }

        /// <summary>Nhận đề có sẵn (cho test), nguồn vật lý của host và quy tắc thời gian (null = OnlineFor theo số người của đề). Ném lỗi nếu tham số sai.</summary>
        public ChoirHostRules(ChoirRound round, IChoirWorld world, ChoirRules rules = null)
        {
            if (round == null) throw new ArgumentNullException("round");
            if (world == null) throw new ArgumentNullException("world");
            state = new ChoirState(round, rules ?? ChoirRules.OnlineFor(round.PlayerCount));
            this.world = world;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, ChoirCommand command)
        {
            if (command == null) return RuleVerdict.Reject("Lệnh không hợp lệ.");
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            switch (command.Kind)
            {
                case ChoirCommandKind.Retry:
                    return state.Solved ? RuleVerdict.Reject("Đã hát xong, không cần làm lại.") : RuleVerdict.Accept();
                case ChoirCommandKind.Press:
                    return ValidatePress(senderSlot, command.Pad);
                default:
                    return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        /// <summary>Nhận ô và bục; trả chấp nhận nếu chưa thắng, bục tồn tại và ô đứng đủ gần bục (theo vị trí host thấy).</summary>
        RuleVerdict ValidatePress(int slot, int pad)
        {
            if (state.Solved) return RuleVerdict.Reject("Đã hát xong bản nhạc.");
            if (pad < 0 || pad >= ChoirRound.PadCount) return RuleVerdict.Reject("Bục này không tồn tại.");
            if (!world.IsNearPad(slot, pad)) return RuleVerdict.Reject("Bạn đứng quá xa bục này.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, ChoirCommand command)
        {
            switch (command.Kind)
            {
                case ChoirCommandKind.Retry:
                    state.Reset();
                    resetCount++;
                    lastMessage = ChoirMessage.Retry;
                    lastActor = senderSlot;
                    lastPad = -1;
                    break;
                case ChoirCommandKind.Press:
                    ChoirResult result = state.Press(command.Pad, clock);
                    lastMessage = MessageOf(result);
                    lastActor = senderSlot;
                    lastPad = command.Pad;
                    break;
            }
        }

        /// <summary>Nhận kết quả Domain; trả thông báo tương ứng. Ném lỗi nếu Domain từ chối lệnh đã được Validate (InvalidPad/AlreadySolved).</summary>
        static ChoirMessage MessageOf(ChoirResult result)
        {
            switch (result)
            {
                case ChoirResult.Correct: return ChoirMessage.Correct;
                case ChoirResult.Completed: return ChoirMessage.Completed;
                case ChoirResult.Wrong: return ChoirMessage.Wrong;
                case ChoirResult.TooLate: return ChoirMessage.TooLate;
                default: throw new InvalidOperationException("Domain từ chối lệnh đã được Validate: " + result);
            }
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến). Không chứa seed, bảng ký hiệu -> bục, đáp án hay đồng hồ đang chạy.</summary>
        public ChoirPublicState BuildPublicState()
        {
            ChoirRound round = state.Round;
            int[] symbols = new int[round.Length];
            for (int i = 0; i < symbols.Length; i++) symbols[i] = round.Symbol(i);
            return new ChoirPublicState(PlayerCount, symbols, state.Progress, state.Mistakes, state.Timeouts, state.Solved,
                state.Rules.StepWindowSeconds, resetCount, lastMessage, lastActor, lastPad);
        }

        /// <summary>Nhận ô; trả các ký hiệu ô đó giữ thẻ và bục mỗi ký hiệu ứng với (rỗng nếu ô không tham gia). Tuyệt đối không chứa thẻ người khác.</summary>
        public ChoirPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new ChoirPrivateState(null, null);
            int[] owned = state.Round.SymbolsOf(slot);
            int[] pads = new int[owned.Length];
            for (int i = 0; i < owned.Length; i++)
            {
                int pad;
                if (!state.TryReadCard(slot, owned[i], out pad)) throw new InvalidOperationException("Chủ thẻ không đọc được thẻ của mình.");
                pads[i] = pad;
            }
            return new ChoirPrivateState(owned, pads);
        }

        /// <summary>
        /// Đồng hồ chạy trên host. Nhận số giây đã trôi. Quá cửa sổ giữa hai nốt thì dãy về nốt đầu (Timeouts tăng).
        /// Trả true khi vừa quá giờ (trạng thái đổi mà không lệnh nào gây ra), để lớp Unity gọi HostAuthority.MarkChanged().
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f) return false;
            if (state.Solved) return false;
            clock += deltaSeconds;
            if (!state.Tick(clock)) return false;
            lastMessage = ChoirMessage.TimedOut;
            lastActor = -1;
            lastPad = -1;
            return true;
        }
    }
}
