/*
 * Mục đích: ADAPTER «Hợp xướng» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039). Chỉ khai: dựng ván, bot tự giải từ đáp án Domain (bấm bục đúng theo nốt),
 * một lệnh chắc chắn bị từ chối (ô đứng xa bấm bục), codec, và các chuỗi BÍ MẬT (cặp «ký hiệu=bục» của thẻ từng ô). Toàn bộ C1-C9 + N1 chạy tự động
 * cho đội 2/3/4 x nhiều seed. Hợp xướng không có «thua» (quá giờ chỉ đưa dãy về đầu) nên ForceLoss trả false và ca C7 được bỏ qua.
 * Cửa sổ giữa hai nốt của ván hợp đồng đặt 30 giây để bộ chạy không tự gây quá giờ; luật cửa sổ thật đã có test riêng.
 * Hàm: ChoirContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); ChoirContractAdapter; ChoirContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.ChoirNet.Tests
{
    /// <summary>Một ván Hợp xướng cho hợp đồng: luật host + bot dùng đáp án Domain.</summary>
    public sealed class ChoirContractGame : IContractGame<ChoirCommand, ChoirPublicState, ChoirPrivateState>
    {
        readonly ChoirHostRules rules;
        readonly int teamSize;

        public ChoirContractGame(int teamSize, int seed)
        {
            this.teamSize = teamSize;
            FakeChoirWorld world = new FakeChoirWorld();
            world.FarSlots.Add(teamSize - 1);
            rules = new ChoirHostRules(teamSize, seed, world, new ChoirRules(30.0, 0.0));
        }

        public IHostRules<ChoirCommand, ChoirPublicState, ChoirPrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public ChoirHostRules Host { get { return rules; } }

        /// <summary>Ô 0 (luôn đứng gần) bấm bục đúng của nốt hiện tại. Hết dãy = đã thắng, trả false.</summary>
        public bool TryNextSolveMove(out int slot, out ChoirCommand command)
        {
            if (rules.Solved)
            {
                slot = -1;
                command = null;
                return false;
            }
            slot = 0;
            command = ChoirCommand.Press(rules.Round.SolutionPad(rules.State.Progress));
            return true;
        }

        /// <summary>Ô cuối (đứng xa mọi bục) bấm bục 0: host phải bác vì vị trí host thấy là quá xa.</summary>
        public bool TryIllegalMove(out int slot, out ChoirCommand command)
        {
            slot = teamSize - 1;
            command = ChoirCommand.Press(0);
            return true;
        }

        public bool ForceLoss() { return false; }

        public bool Tick(float deltaSeconds) { return rules.Tick(deltaSeconds); }
    }

    /// <summary>Adapter hợp đồng của «Hợp xướng».</summary>
    public sealed class ChoirContractAdapter : MissionContractAdapter<ChoirCommand, ChoirPublicState, ChoirPrivateState>
    {
        public override string MissionName { get { return "Choir"; } }

        public override IContractGame<ChoirCommand, ChoirPublicState, ChoirPrivateState> NewGame(int teamSize, int seed)
        {
            return new ChoirContractGame(teamSize, seed);
        }

        public override string EncodeCommand(ChoirCommand command) { return ChoirCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out ChoirCommand command) { return ChoirCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(ChoirPublicState state) { return ChoirCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out ChoirPublicState state) { return ChoirCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(ChoirPrivateState state) { return ChoirCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out ChoirPrivateState state) { return ChoirCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(ChoirPublicState state) { return state.Solved; }

        /// <summary>Bí mật của ô: từng cặp «ký hiệu=bục» trên thẻ mình (chuỗi có dấu '=', không bao giờ xuất hiện ở trạng thái chung).</summary>
        public override string[] SecretsOf(IContractGame<ChoirCommand, ChoirPublicState, ChoirPrivateState> game, int slot)
        {
            ChoirPrivateState p = ((ChoirContractGame)game).Host.BuildPrivateState(slot);
            int[] symbols = p.SymbolsCopy();
            int[] pads = p.PadsCopy();
            string[] secrets = new string[symbols.Length];
            for (int i = 0; i < symbols.Length; i++) secrets[i] = symbols[i] + "=" + pads[i];
            return secrets;
        }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho Choir.</summary>
    [TestFixture]
    public sealed class ChoirContractTests : MissionContractTests<ChoirCommand, ChoirPublicState, ChoirPrivateState>
    {
        protected override MissionContractAdapter<ChoirCommand, ChoirPublicState, ChoirPrivateState> CreateAdapter()
        {
            return new ChoirContractAdapter();
        }
    }
}
