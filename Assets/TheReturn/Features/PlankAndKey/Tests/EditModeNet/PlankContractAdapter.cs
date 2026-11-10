/*
 * Mục đích: ADAPTER «Cầu ván và thẻ từ» vào bộ test HỢP ĐỒNG mạng dùng chung (T-039, nhận ở T-040 P4). Chỉ khai: dựng ván (đề sinh từ PlankGenerator),
 * bot tự giải theo kế hoạch tối ưu của Domain (PlankSolver: nhặt thẻ -> thả lên ván -> đưa người lên ván -> gạt cần), một lệnh chắc chắn bị
 * từ chối (gạt cần khi chưa ai trên ván), một cách ép THUA (gạt qua lại một người tới quá hạn), và codec. Thế giới vật lý là FakeWorld có sẵn.
 * Lưu ý bot: giữa hai lượt qua cầu ván chạy cần thời gian; hợp đồng chỉ cho đồng hồ chạy SAU khi hết nước, nên bot tự cho luật chạy hết các lượt
 * trung gian (chỉ lượt CUỐI để hợp đồng tự tick, để mọi bản sao thấy lúc thắng qua Version). Thông tin riêng của Plank chỉ là tốc độ qua cầu
 * (một con số) nên không khai chuỗi bí mật (C5 không có gì đặc trưng để dò).
 * Hàm: PlankContractGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); PlankContractAdapter; PlankContractTests (fixture).
 */
using NUnit.Framework;
using TheReturn.Network.Contract;
using TheReturn.Network.Framework;

namespace TheReturn.PlankNet.Tests
{
    /// <summary>Một ván Cầu ván cho hợp đồng: luật host + bot theo kế hoạch tối ưu của Domain.</summary>
    public sealed class PlankContractGame : IContractGame<PlankCommand, PlankPublicState, PlankPrivateState>
    {
        const float SecondsPerUnit = 0.5f;
        const float FailResetSeconds = 2.5f;

        readonly PlankHostRules rules;
        readonly FakeWorld world = new FakeWorld();
        readonly PlankPlan plan;
        int moveIndex;

        public PlankContractGame(int teamSize, int seed)
        {
            PlankRound round = PlankGenerator.Generate(teamSize, seed);
            plan = round.OptimalPlan();
            rules = new PlankHostRules(round, SecondsPerUnit, world, FailResetSeconds);
        }

        public IHostRules<PlankCommand, PlankPublicState, PlankPrivateState> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Session.Solved; } }

        /// <summary>
        /// Bot: theo từng lượt của kế hoạch. Chưa có thẻ trên ván thì nhặt rồi thả lên ván; sau đó đặt đúng nhóm lên ván và gạt cần.
        /// Lượt trung gian đang chạy thì cho luật chạy hết; lượt cuối thì trả false để hợp đồng tự tick.
        /// </summary>
        public bool TryNextSolveMove(out int slot, out PlankCommand command)
        {
            slot = -1;
            command = null;
            for (int guard = 0; guard < 8; guard++)
            {
                if (IsSolved) return false;
                PlankPublicState s = rules.BuildPublicState();
                if (s.Phase == PlankPhase.Crossing)
                {
                    if (moveIndex >= plan.Moves.Count) return false;
                    rules.Tick(s.CrossingSecondsLeft + 0.05f);
                    continue;
                }
                if (s.Status != PlankStatus.Playing || moveIndex >= plan.Moves.Count) return false;
                int[] riders = plan.Moves[moveIndex];
                if (!s.CardOnDeck && s.CardHolder < 0)
                {
                    slot = riders[0];
                    command = PlankCommand.PickUpCard();
                    return true;
                }
                if (!s.CardOnDeck)
                {
                    world.DropOnDeck = true;
                    slot = s.CardHolder;
                    command = PlankCommand.DropCard();
                    return true;
                }
                world.OnDeck = (int[])riders.Clone();
                slot = riders[0];
                command = PlankCommand.UseLever(s.DeckAtNear);
                moveIndex++;
                return true;
            }
            return false;
        }

        /// <summary>Ô 0 gạt cần khi chưa ai đứng trên ván: luật luôn từ chối (không đổi Version).</summary>
        public bool TryIllegalMove(out int slot, out PlankCommand command)
        {
            world.OnDeck = new int[0];
            slot = 0;
            command = PlankCommand.UseLever(true);
            return true;
        }

        /// <summary>Ô 0 nhặt thẻ, thả lên ván rồi một mình gạt qua lại tới quá hạn (thua). Đi thẳng vào luật, không qua host.</summary>
        public bool ForceLoss()
        {
            if (!Do(0, PlankCommand.PickUpCard())) return false;
            world.DropOnDeck = true;
            if (!Do(0, PlankCommand.DropCard())) return false;
            for (int i = 0; i < 80; i++)
            {
                PlankPublicState s = rules.BuildPublicState();
                if (s.Status == PlankStatus.Failed) return true;
                if (s.Phase == PlankPhase.Crossing)
                {
                    rules.Tick(s.CrossingSecondsLeft + 0.05f);
                    continue;
                }
                world.OnDeck = new[] { 0 };
                if (!Do(0, PlankCommand.UseLever(s.DeckAtNear))) return false;
            }
            return rules.BuildPublicState().Status == PlankStatus.Failed;
        }

        public bool Tick(float deltaSeconds) { return rules.Tick(deltaSeconds); }

        /// <summary>Kiểm rồi áp một lệnh thẳng vào luật; trả false nếu luật từ chối.</summary>
        bool Do(int slot, PlankCommand command)
        {
            if (!rules.Validate(slot, command).IsAccepted) return false;
            rules.Apply(slot, command);
            return true;
        }
    }

    /// <summary>Adapter hợp đồng của «Cầu ván và thẻ từ».</summary>
    public sealed class PlankContractAdapter : MissionContractAdapter<PlankCommand, PlankPublicState, PlankPrivateState>
    {
        public override string MissionName { get { return "PlankAndKey"; } }

        public override IContractGame<PlankCommand, PlankPublicState, PlankPrivateState> NewGame(int teamSize, int seed)
        {
            return new PlankContractGame(teamSize, seed);
        }

        public override string EncodeCommand(PlankCommand command) { return PlankCodec.EncodeCommand(command); }
        public override bool TryDecodeCommand(string text, out PlankCommand command) { return PlankCodec.TryDecodeCommand(text, out command); }
        public override string EncodePublic(PlankPublicState state) { return PlankCodec.EncodePublic(state); }
        public override bool TryDecodePublic(string text, out PlankPublicState state) { return PlankCodec.TryDecodePublic(text, out state); }
        public override string EncodePrivate(PlankPrivateState state) { return PlankCodec.EncodePrivate(state); }
        public override bool TryDecodePrivate(string text, out PlankPrivateState state) { return PlankCodec.TryDecodePrivate(text, out state); }
        public override bool IsSolvedPublic(PlankPublicState state) { return state.Solved; }
    }

    /// <summary>Chạy bộ hợp đồng C1-C9 + N1 cho PlankAndKey.</summary>
    [TestFixture]
    public sealed class PlankContractTests : MissionContractTests<PlankCommand, PlankPublicState, PlankPrivateState>
    {
        protected override MissionContractAdapter<PlankCommand, PlankPublicState, PlankPrivateState> CreateAdapter()
        {
            return new PlankContractAdapter();
        }
    }
}
