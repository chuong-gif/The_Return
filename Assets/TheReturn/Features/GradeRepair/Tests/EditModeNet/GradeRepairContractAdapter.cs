/*
 * Mục đích: ADAPTER «Sửa bảng điểm» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039 / T-036). Chỉ khai: dựng ván, bot tự giải từ đáp án Domain (đọc đủ tài liệu,
 * chuyển điểm TRƯỚC rồi hai phiếu bổ sung, nộp), một lệnh chắc chắn bị từ chối, codec và các chuỗi BÍ MẬT (sự kiện tài liệu của từng vai).
 * Sửa bảng điểm không có «thua» (sai thì hoàn tác/đặt lại) và không có đồng hồ nên ForceLoss trả false (ca C7 bỏ qua) và Tick luôn trả false.
 * Hàm: GradeRepairContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); GradeRepairContractAdapter; GradeRepairContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.GradeRepairNet.Tests
{
    /// <summary>Một ván sửa bảng điểm cho hợp đồng: luật host + bot dùng đáp án Domain.</summary>
    public sealed class GradeRepairContractGame : IContractGame<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState>
    {
        readonly GradeRepairHostRules rules;

        public GradeRepairContractGame(int teamSize, int seed)
        {
            rules = new GradeRepairHostRules(teamSize, seed, new FakeGradeRepairWorld());
        }

        public IHostRules<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public GradeRepairHostRules Host { get { return rules; } }

        /// <summary>Tài liệu chưa đọc -> chủ của nó đọc; rồi áp phiếu 2, 0, 1 theo đáp án (ô 0 đứng ở bảng); rồi nộp.</summary>
        public bool TryNextSolveMove(out int slot, out GradeRepairCommand command)
        {
            GradeRepairRound round = rules.Round;
            GradeRepairState state = rules.State;
            for (int d = 0; d < GradeRepairPublicState.Documents; d++)
            {
                if (state.Read(d)) continue;
                slot = round.OwnerOf(d);
                command = GradeRepairCommand.Read(d);
                return true;
            }
            int[] order = { 2, 0, 1 };
            for (int i = 0; i < order.Length; i++)
            {
                int v = order[i];
                if (state.Used(v)) continue;
                slot = 0;
                command = GradeRepairCommand.Apply(v, round.CorrectSource(v), round.CorrectTarget(v));
                return true;
            }
            if (!state.Solved)
            {
                slot = 0;
                command = GradeRepairCommand.Submit();
                return true;
            }
            slot = -1;
            command = null;
            return false;
        }

        /// <summary>Người KHÔNG phải chủ đọc tài liệu 0.</summary>
        public bool TryIllegalMove(out int slot, out GradeRepairCommand command)
        {
            slot = (rules.Round.OwnerOf(0) + 1) % rules.PlayerCount;
            command = GradeRepairCommand.Read(0);
            return true;
        }

        public bool ForceLoss() { return false; }

        public bool Tick(float deltaSeconds) { return false; }
    }

    /// <summary>Adapter hợp đồng của «Sửa bảng điểm».</summary>
    public sealed class GradeRepairContractAdapter : MissionContractAdapter<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState>
    {
        public override string MissionName { get { return "GradeRepair"; } }

        public override IContractGame<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState> NewGame(int teamSize, int seed)
        {
            return new GradeRepairContractGame(teamSize, seed);
        }

        public override string EncodeCommand(GradeRepairCommand command) { return GradeRepairCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out GradeRepairCommand command) { return GradeRepairCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(GradeRepairPublicState state) { return GradeRepairCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out GradeRepairPublicState state) { return GradeRepairCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(GradeRepairPrivateState state) { return GradeRepairCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out GradeRepairPrivateState state) { return GradeRepairCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(GradeRepairPublicState state) { return state.Solved; }

        /// <summary>Bí mật của ô: sự kiện của MỌI tài liệu mình phụ trách (kể cả chưa đọc), tức là chuỗi chứa mã hồ sơ của đề (vd «V1.HS-452»).</summary>
        public override string[] SecretsOf(IContractGame<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState> game, int slot)
        {
            GradeRepairHostRules host = ((GradeRepairContractGame)game).Host;
            System.Collections.Generic.List<string> secrets = new System.Collections.Generic.List<string>();
            for (int d = 0; d < GradeRepairPublicState.Documents; d++)
                if (host.Round.OwnerOf(d) == slot) secrets.Add(GradeRepairHostRules.FactsOf(host.Round, d, 2, 2));
            return secrets.ToArray();
        }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho GradeRepair.</summary>
    [TestFixture]
    public sealed class GradeRepairContractTests : MissionContractTests<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState>
    {
        protected override MissionContractAdapter<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState> CreateAdapter()
        {
            return new GradeRepairContractAdapter();
        }
    }
}
