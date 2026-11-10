/*
 * Mục đích: ADAPTER «Đèn lồng chung» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039). Chỉ khai: dựng ván, bot tự giải từ đáp án Domain (ô 0 đọc đủ 4 biển,
 * nhập đúng bảng, nộp; cả đội coi như đã ở cổng), một lệnh chắc chắn bị từ chối (ô đứng xa bảng nhập), codec, và các chuỗi BÍ MẬT (cặp «biển=ký hiệu»
 * trong sổ của từng ô). Toàn bộ C1-C9 + N1 chạy tự động cho đội 2/3/4 x nhiều seed. Đèn lồng chung không có «thua» (bị đưa về chỉ là phạt) nên
 * ForceLoss trả false và ca C7 được bỏ qua.
 * Hàm: SharedLanternContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); SharedLanternContractAdapter; SharedLanternContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.LanternNet.Tests
{
    /// <summary>Một ván Đèn lồng chung cho hợp đồng: luật host + bot dùng đáp án Domain.</summary>
    public sealed class SharedLanternContractGame : IContractGame<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState>
    {
        readonly SharedLanternHostRules rules;
        readonly int teamSize;

        public SharedLanternContractGame(int teamSize, int seed)
        {
            this.teamSize = teamSize;
            FakeLanternWorld world = new FakeLanternWorld();
            world.FarSlots.Add(teamSize - 1);
            rules = new SharedLanternHostRules(teamSize, seed, world);
        }

        public IHostRules<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public SharedLanternHostRules Host { get { return rules; } }

        /// <summary>Ô 0 (luôn đứng gần): đọc biển chưa đọc, rồi nhập ô sai đáp án, rồi nộp. Hết việc = đã thắng, trả false.</summary>
        public bool TryNextSolveMove(out int slot, out SharedLanternCommand command)
        {
            slot = 0;
            command = null;
            if (rules.Solved) return false;
            for (int sign = 0; sign < 4; sign++)
            {
                if (rules.State.HasRead(0, sign)) continue;
                command = SharedLanternCommand.Read(sign);
                return true;
            }
            int[] solution = rules.Round.Solution();
            for (int i = 0; i < solution.Length; i++)
            {
                if (rules.State.Entry(i) == solution[i]) continue;
                command = SharedLanternCommand.SetEntry(i, solution[i]);
                return true;
            }
            command = SharedLanternCommand.Submit();
            return true;
        }

        /// <summary>Ô cuối (đứng xa mọi thứ) đặt ký hiệu vào bảng nhập: host phải bác vì vị trí host thấy là quá xa bảng.</summary>
        public bool TryIllegalMove(out int slot, out SharedLanternCommand command)
        {
            slot = teamSize - 1;
            command = SharedLanternCommand.SetEntry(0, 0);
            return true;
        }

        public bool ForceLoss() { return false; }

        public bool Tick(float deltaSeconds) { return rules.Tick(deltaSeconds); }
    }

    /// <summary>Adapter hợp đồng của «Đèn lồng chung».</summary>
    public sealed class SharedLanternContractAdapter : MissionContractAdapter<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState>
    {
        public override string MissionName { get { return "SharedLantern"; } }

        public override IContractGame<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState> NewGame(int teamSize, int seed)
        {
            return new SharedLanternContractGame(teamSize, seed);
        }

        public override string EncodeCommand(SharedLanternCommand command) { return SharedLanternCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out SharedLanternCommand command) { return SharedLanternCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(SharedLanternPublicState state) { return SharedLanternCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out SharedLanternPublicState state) { return SharedLanternCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(SharedLanternPrivateState state) { return SharedLanternCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out SharedLanternPrivateState state) { return SharedLanternCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(SharedLanternPublicState state) { return state.Solved; }

        /// <summary>Bí mật của ô: từng cặp «biển=ký hiệu» trong sổ của mình (chuỗi có dấu '=', không bao giờ xuất hiện ở trạng thái chung).</summary>
        public override string[] SecretsOf(IContractGame<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState> game, int slot)
        {
            SharedLanternPrivateState p = ((SharedLanternContractGame)game).Host.BuildPrivateState(slot);
            int[] signs = p.SignsCopy();
            int[] symbols = p.SymbolsCopy();
            string[] secrets = new string[signs.Length];
            for (int i = 0; i < signs.Length; i++) secrets[i] = signs[i] + "=" + symbols[i];
            return secrets;
        }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho SharedLantern.</summary>
    [TestFixture]
    public sealed class SharedLanternContractTests : MissionContractTests<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState>
    {
        protected override MissionContractAdapter<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState> CreateAdapter()
        {
            return new SharedLanternContractAdapter();
        }
    }
}
