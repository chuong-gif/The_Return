/*
 * Mục đích: ADAPTER «Bảng cầu dao» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039 M3). Chỉ khai: dựng ván, bot tự giải từ đáp án Domain,
 * một lệnh chắc chắn bị từ chối, codec, và các chuỗi BÍ MẬT (công tắc + đèn riêng của từng ô). Toàn bộ C1-C9 + N1 chạy tự động cho đội 2/3/4 x 50 seed.
 * Cầu dao không có «thua» (làm lại vô hạn) nên ForceLoss trả false và ca C7 được bỏ qua.
 * Hàm: BreakerContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); BreakerContractAdapter; BreakerContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.BreakerNet.Tests
{
    /// <summary>Một ván cầu dao cho hợp đồng: luật host + bot dùng đáp án Domain.</summary>
    public sealed class BreakerContractGame : IContractGame<BreakerCommand, BreakerPublicState, BreakerPrivateState>
    {
        readonly BreakerHostRules rules;

        public BreakerContractGame(int teamSize, int seed)
        {
            rules = new BreakerHostRules(teamSize, seed, new FakeBreakerWorld(), 3f);
        }

        public IHostRules<BreakerCommand, BreakerPublicState, BreakerPrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public BreakerHostRules Host { get { return rules; } }

        /// <summary>Công tắc đầu tiên đang khác đáp án: chủ của nó bật/tắt. Hết khác = đã khớp, chờ giữ đủ giờ (Tick).</summary>
        public bool TryNextSolveMove(out int slot, out BreakerCommand command)
        {
            int solution = rules.Round.SolutionSwitches;
            for (int sw = 0; sw < rules.Round.Size; sw++)
            {
                bool want = ((solution >> sw) & 1) == 1;
                bool on = ((rules.State.OnSwitches >> sw) & 1) == 1;
                if (want == on) continue;
                slot = rules.Round.OwnerOf(sw);
                command = BreakerCommand.Toggle(sw);
                return true;
            }
            slot = -1;
            command = null;
            return false;
        }

        /// <summary>Người KHÔNG phải chủ bấm công tắc 0.</summary>
        public bool TryIllegalMove(out int slot, out BreakerCommand command)
        {
            slot = (rules.Round.OwnerOf(0) + 1) % rules.PlayerCount;
            command = BreakerCommand.Toggle(0);
            return true;
        }

        public bool ForceLoss() { return false; }

        public bool Tick(float deltaSeconds) { return rules.Tick(deltaSeconds); }
    }

    /// <summary>Adapter hợp đồng của «Bảng cầu dao».</summary>
    public sealed class BreakerContractAdapter : MissionContractAdapter<BreakerCommand, BreakerPublicState, BreakerPrivateState>
    {
        public override string MissionName { get { return "BreakerPanel"; } }

        public override IContractGame<BreakerCommand, BreakerPublicState, BreakerPrivateState> NewGame(int teamSize, int seed)
        {
            return new BreakerContractGame(teamSize, seed);
        }

        public override string EncodeCommand(BreakerCommand command) { return BreakerCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out BreakerCommand command) { return BreakerCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(BreakerPublicState state) { return BreakerCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out BreakerPublicState state) { return BreakerCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(BreakerPrivateState state) { return BreakerCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out BreakerPrivateState state) { return BreakerCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(BreakerPublicState state) { return state.Solved; }

        /// <summary>Bí mật của ô: từng cặp «công tắc=mặt nạ đèn» của công tắc mình sở hữu (chuỗi có dấu '=', không bao giờ xuất hiện ở trạng thái chung).</summary>
        public override string[] SecretsOf(IContractGame<BreakerCommand, BreakerPublicState, BreakerPrivateState> game, int slot)
        {
            BreakerPrivateState p = ((BreakerContractGame)game).Host.BuildPrivateState(slot);
            int[] sw = p.SwitchesCopy();
            int[] mask = p.MasksCopy();
            string[] secrets = new string[sw.Length];
            for (int i = 0; i < sw.Length; i++) secrets[i] = sw[i] + "=" + mask[i];
            return secrets;
        }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho BreakerPanel.</summary>
    [TestFixture]
    public sealed class BreakerContractTests : MissionContractTests<BreakerCommand, BreakerPublicState, BreakerPrivateState>
    {
        protected override MissionContractAdapter<BreakerCommand, BreakerPublicState, BreakerPrivateState> CreateAdapter()
        {
            return new BreakerContractAdapter();
        }
    }
}
