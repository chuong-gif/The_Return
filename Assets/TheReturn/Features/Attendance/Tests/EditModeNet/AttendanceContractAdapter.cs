/*
 * Mục đích: ADAPTER «Điểm danh» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039 M3). Chỉ khai: dựng ván (tên + gói manh mối riêng), bot tự giải
 * (ngồi đúng ghế theo đáp án Domain, đứng dậy nếu đang ngồi sai), lệnh chắc chắn bị từ chối, codec, và chuỗi BÍ MẬT (gói manh mối của từng ô).
 * Điểm danh không có «thua» (sai thì làm lại) nên ForceLoss trả false và ca C7 được bỏ qua.
 * Hàm: AttendanceContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); AttendanceContractAdapter; AttendanceContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.AttendanceNet.Tests
{
    /// <summary>Một ván điểm danh cho hợp đồng: luật host + bot dùng đáp án Domain.</summary>
    public sealed class AttendanceContractGame : IContractGame<AttendanceCommand, AttendancePublicState, AttendancePrivateState>
    {
        static readonly string[] AllNames = { "Minh", "An", "Bình", "Chi" };
        readonly AttendanceHostRules rules;

        public AttendanceContractGame(int teamSize, int seed)
        {
            string[] names = new string[teamSize];
            for (int i = 0; i < teamSize; i++) names[i] = AllNames[i];
            rules = new AttendanceHostRules(names, seed, Packets, new FakeAttendanceWorld(), 1.5f);
        }

        /// <summary>Gói manh mối riêng của mỗi ô: chỉ chữ và số (không bị mã hóa thoát ký tự), khác nhau theo ô và seed.</summary>
        static string[] Packets(AttendanceRound round)
        {
            string[] p = new string[round.PlayerCount];
            for (int i = 0; i < p.Length; i++) p[i] = "ZZ" + i + "ZZ" + round.Seed;
            for (int k = 0; k < round.Rules.Count; k++) p[round.ClueOwners[k]] += "Q" + round.Rules[k].Kind + round.Rules[k].PersonA;
            return p;
        }

        public IHostRules<AttendanceCommand, AttendancePublicState, AttendancePrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public AttendanceHostRules Host { get { return rules; } }

        /// <summary>Người đầu tiên chưa ở đúng ghế đáp án: đang ngồi sai thì đứng dậy, chưa ngồi thì ngồi đúng ghế. Đủ người đúng chỗ = chờ chấm (Tick).</summary>
        public bool TryNextSolveMove(out int slot, out AttendanceCommand command)
        {
            int[] solution = rules.Round.CopySolution();
            for (int p = 0; p < rules.PlayerCount; p++)
            {
                int current = rules.State.SeatOf(p);
                if (current == solution[p]) continue;
                slot = p;
                command = current >= 0 ? AttendanceCommand.Stand() : AttendanceCommand.Sit(solution[p]);
                return true;
            }
            slot = -1;
            command = null;
            return false;
        }

        /// <summary>Ô 0 ngồi lại khi đang ngồi (từ chối), hoặc đứng dậy khi chưa ngồi (từ chối).</summary>
        public bool TryIllegalMove(out int slot, out AttendanceCommand command)
        {
            slot = 0;
            int seat = rules.State.SeatOf(0);
            command = seat >= 0 ? AttendanceCommand.Sit(seat) : AttendanceCommand.Stand();
            return true;
        }

        public bool ForceLoss() { return false; }

        public bool Tick(float deltaSeconds) { return rules.Tick(deltaSeconds); }
    }

    /// <summary>Adapter hợp đồng của «Điểm danh».</summary>
    public sealed class AttendanceContractAdapter : MissionContractAdapter<AttendanceCommand, AttendancePublicState, AttendancePrivateState>
    {
        public override string MissionName { get { return "Attendance"; } }

        public override IContractGame<AttendanceCommand, AttendancePublicState, AttendancePrivateState> NewGame(int teamSize, int seed)
        {
            return new AttendanceContractGame(teamSize, seed);
        }

        public override string EncodeCommand(AttendanceCommand command) { return AttendanceCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out AttendanceCommand command) { return AttendanceCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(AttendancePublicState state) { return AttendanceCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out AttendancePublicState state) { return AttendanceCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(AttendancePrivateState state) { return AttendanceCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out AttendancePrivateState state) { return AttendanceCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(AttendancePublicState state) { return state.Solved; }

        /// <summary>Bí mật của ô: nguyên gói manh mối riêng của ô đó.</summary>
        public override string[] SecretsOf(IContractGame<AttendanceCommand, AttendancePublicState, AttendancePrivateState> game, int slot)
        {
            return new[] { ((AttendanceContractGame)game).Host.BuildPrivateState(slot).Clues };
        }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho Attendance.</summary>
    [TestFixture]
    public sealed class AttendanceContractTests : MissionContractTests<AttendanceCommand, AttendancePublicState, AttendancePrivateState>
    {
        protected override MissionContractAdapter<AttendanceCommand, AttendancePublicState, AttendancePrivateState> CreateAdapter()
        {
            return new AttendanceContractAdapter();
        }
    }
}
