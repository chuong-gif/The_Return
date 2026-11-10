/*
 * Mục đích: LUẬT HOST của «Bảng cầu dao» khi chơi online (T-031): bọc BreakerState + BreakerGenerator (Domain có sẵn, KHÔNG đổi) theo giao diện
 * IHostRules của khung T-024. Host nhận lệnh bật/tắt công tắc và làm lại, kiểm điều kiện (đúng chủ công tắc, đứng gần bằng IBreakerWorld, chưa thắng),
 * rồi chạy luật (quá tải thì cầu dao nhảy). Đồng hồ giữ khớp chạy ở host (Tick). Trạng thái chung (công tắc, đèn, đích, tiến độ) gửi mọi người;
 * công tắc của ai và đèn nó đảo CHỈ gửi RIÊNG cho chủ. Seed và đáp án không bao giờ rời host. Theo mẫu PLANS/T-027-plank-network-pattern.md.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor (hai dạng); Validate; Apply; BuildPublicState; BuildPrivateState; Tick; thuộc tính State, Round, PlayerCount, ResetCount, Solved.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.BreakerNet
{
    /// <summary>Luật host của nhiệm vụ cầu dao (lệnh BreakerCommand, trạng thái chung BreakerPublicState, riêng BreakerPrivateState).</summary>
    public sealed class BreakerHostRules : IHostRules<BreakerCommand, BreakerPublicState, BreakerPrivateState>
    {
        readonly BreakerState state;
        readonly IBreakerWorld world;
        double clock;
        int resetCount;
        BreakerMessage lastMessage;
        int lastActor = -1;
        int lastSwitch = -1;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh và Tick).</summary>
        public BreakerState State { get { return state; } }

        /// <summary>Đề đang chơi (chỉ host/test được đọc; KHÔNG gửi cho máy khách).</summary>
        public BreakerRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.Round.PlayerCount; } }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True sau khi đã giữ khớp đủ giờ.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>Nhận số người (2-4), seed, nguồn vật lý của host và số giây giữ khớp (>= 0). Sinh đề theo seed. Ném lỗi nếu tham số sai.</summary>
        public BreakerHostRules(int playerCount, int seed, IBreakerWorld world, float holdSeconds)
            : this(BreakerGenerator.Generate(playerCount, seed), world, holdSeconds)
        {
        }

        /// <summary>Nhận đề có sẵn (cho test), nguồn vật lý của host và số giây giữ khớp (>= 0). Ném lỗi nếu tham số sai.</summary>
        public BreakerHostRules(BreakerRound round, IBreakerWorld world, float holdSeconds)
        {
            if (round == null) throw new ArgumentNullException("round");
            if (world == null) throw new ArgumentNullException("world");
            if (float.IsNaN(holdSeconds) || float.IsInfinity(holdSeconds) || holdSeconds < 0f) throw new ArgumentOutOfRangeException("holdSeconds");
            state = new BreakerState(round, holdSeconds);
            this.world = world;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, BreakerCommand command)
        {
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            switch (command.Kind)
            {
                case BreakerCommandKind.Retry:
                    return state.Solved ? RuleVerdict.Reject("Đã sửa xong bảng cầu dao, không cần làm lại.") : RuleVerdict.Accept();
                case BreakerCommandKind.Toggle:
                    return ValidateToggle(senderSlot, command.Switch);
                default:
                    return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        RuleVerdict ValidateToggle(int slot, int sw)
        {
            if (state.Solved) return RuleVerdict.Reject("Đã sửa xong bảng cầu dao.");
            if (sw < 0 || sw >= state.Round.Size) return RuleVerdict.Reject("Công tắc này không tồn tại.");
            if (state.Round.OwnerOf(sw) != slot) return RuleVerdict.Reject("Công tắc này không phải của bạn.");
            if (!world.IsNearSwitch(slot, sw)) return RuleVerdict.Reject("Bạn đứng quá xa công tắc này.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, BreakerCommand command)
        {
            switch (command.Kind)
            {
                case BreakerCommandKind.Retry:
                    state.Reset();
                    resetCount++;
                    lastMessage = BreakerMessage.Retry;
                    lastActor = senderSlot;
                    lastSwitch = -1;
                    break;
                case BreakerCommandKind.Toggle:
                    BreakerResult result = state.Toggle(senderSlot, command.Switch);
                    if (result != BreakerResult.Toggled && result != BreakerResult.Tripped)
                        throw new InvalidOperationException("Domain từ chối lệnh đã được Validate: " + result);
                    lastMessage = result == BreakerResult.Tripped ? BreakerMessage.Tripped : BreakerMessage.Toggled;
                    lastActor = senderSlot;
                    lastSwitch = command.Switch;
                    break;
            }
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến). Không chứa seed, đáp án hay mặt nạ công tắc.</summary>
        public BreakerPublicState BuildPublicState()
        {
            double hold = state.HoldSeconds;
            // Trạng thái ở MỘT Version phải bất biến (hợp đồng C8/N1): không đưa đồng hồ đang chạy vào gói. Đang giữ khớp thì gửi 0 (máy khách tự
            // ước lượng từ lúc nhận gói; host mới quyết định thắng), đã thắng thì gửi đủ HoldSeconds.
            float elapsed = state.Solved ? (float)hold : 0f;
            return new BreakerPublicState(PlayerCount, state.OnSwitches, state.Lamps, state.Round.TargetLamps, state.Trips, state.Solved,
                elapsed, (float)hold, resetCount, lastMessage, lastActor, lastSwitch);
        }

        /// <summary>Nhận ô; trả các công tắc của ô đó và đèn mỗi công tắc đảo (rỗng nếu ô không tham gia). Tuyệt đối không chứa công tắc của người khác.</summary>
        public BreakerPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new BreakerPrivateState(null, null);
            int size = state.Round.Size;
            int[] sw = new int[size];
            int[] mask = new int[size];
            int count = 0;
            for (int i = 0; i < size; i++)
            {
                int m;
                if (!state.Inspect(slot, i, out m)) continue;
                sw[count] = i;
                mask[count] = m;
                count++;
            }
            int[] ownedSwitches = new int[count];
            int[] ownedMasks = new int[count];
            Array.Copy(sw, ownedSwitches, count);
            Array.Copy(mask, ownedMasks, count);
            return new BreakerPrivateState(ownedSwitches, ownedMasks);
        }

        /// <summary>
        /// Đồng hồ chạy trên host. Nhận số giây đã trôi. Giữ khớp đích liên tục đủ HoldSeconds thì thắng.
        /// Trả true khi vừa thắng (trạng thái đổi mà không lệnh nào gây ra), để lớp Unity gọi HostAuthority.MarkChanged().
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) || deltaSeconds < 0f) return false;
            if (state.Solved) return false;
            clock += deltaSeconds;
            if (!state.Tick(clock)) return false;
            lastMessage = BreakerMessage.Solved;
            return true;
        }
    }
}
