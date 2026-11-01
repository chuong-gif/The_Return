/*
 * Mục đích: NUnit EditMode test cho luật host cầu ván (T-027): kiểm điều kiện từng lệnh, ván chạy và đến nơi, hết giờ tự đặt lại, thông tin
 * riêng chỉ gửi đúng người, và một ván THẮNG đầy đủ qua HostAuthority + ClientReplica với đội 2/3/4. Thế giới vật lý là FakeWorld.
 * Hàm: FakeWorld; Setup/Send/Advance (trợ giúp); các test Plank_*. Không cần scene hay Play Mode.
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.PlankNet.Tests
{
    public sealed class FakeWorld : IPlankWorld
    {
        public readonly HashSet<int> NotNearCard = new HashSet<int>();
        public readonly HashSet<int> NotNearLever = new HashSet<int>();
        public int[] OnDeck = new int[0];
        public bool DropOnDeck;

        public bool IsNearCard(int slot) { return !NotNearCard.Contains(slot); }
        public bool IsNearLever(int slot, bool sideA) { return !NotNearLever.Contains(slot); }
        public int[] PlayersOnDeck() { return OnDeck; }
        public PlankDropSpot DropSpot(int slot) { return new PlankDropSpot(DropOnDeck, 0.5f, 1.5f); }
    }

    public sealed class PlankHostRulesTests
    {
        const float SecondsPerUnit = 3f;
        FakeWorld world;
        PlankHostRules rules;
        HostAuthority<PlankCommand, PlankPublicState, PlankPrivateState> host;
        readonly int[] sequence = new int[4];
        int[] speeds;

        static int[] SpeedsFor(int n)
        {
            if (n == 2) return new[] { 2, 5 };
            if (n == 3) return new[] { 1, 2, 5 };
            return new[] { 1, 2, 3, 4 };
        }

        void Setup(int n)
        {
            speeds = SpeedsFor(n);
            world = new FakeWorld();
            rules = new PlankHostRules(new PlankRound(1, speeds, 25), SecondsPerUnit, world, 2.5f);
            host = new HostAuthority<PlankCommand, PlankPublicState, PlankPrivateState>(new TeamRoster(n), rules, null);
            for (int i = 0; i < sequence.Length; i++) sequence[i] = 0;
        }

        CommandResult Send(int slot, PlankCommand command)
        {
            sequence[slot]++;
            return host.Submit(slot, sequence[slot], command);
        }

        void Advance(float seconds)
        {
            if (rules.Tick(seconds)) host.MarkChanged();
        }

        PlankPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Plank_InitialStateIsFreshForTeamOfN(int n)
        {
            Setup(n);
            PlankPublicState s = Pub();
            Assert.AreEqual(n, s.PlayerCount);
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.AreEqual(PlankStatus.Playing, s.Status);
            Assert.IsTrue(s.DeckAtNear);
            Assert.AreEqual(-1, s.CardHolder);
            Assert.IsFalse(s.CardOnDeck);
            Assert.AreEqual(0, s.RidersCopy().Length);
            foreach (PlankBank bank in s.PlayerBanksCopy()) Assert.AreEqual(PlankBank.Near, bank);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Plank_PrivateStateIsOnlyYourOwnBagSpeed(int n)
        {
            Setup(n);
            for (int i = 0; i < n; i++) Assert.AreEqual(speeds[i], rules.BuildPrivateState(i).CrossUnits);
            Assert.AreEqual(0, rules.BuildPrivateState(n).CrossUnits);
            Assert.AreEqual(0, rules.BuildPrivateState(-1).CrossUnits);
        }

        [Test]
        public void Plank_SenderOutsideTheRoundIsRejected()
        {
            Setup(3);
            host = new HostAuthority<PlankCommand, PlankPublicState, PlankPrivateState>(new TeamRoster(4), rules, null);
            CommandResult result = host.Submit(3, 1, PlankCommand.PickUpCard());
            Assert.AreEqual(RejectReason.RejectedByRules, result.Reason);
        }

        [Test]
        public void Plank_PickUpNeedsBeingNearCard()
        {
            Setup(3);
            world.NotNearCard.Add(1);
            Assert.AreEqual(RejectReason.RejectedByRules, Send(1, PlankCommand.PickUpCard()).Reason);
            Assert.AreEqual(-1, Pub().CardHolder);
            Assert.IsTrue(Send(0, PlankCommand.PickUpCard()).IsAccepted);
            Assert.AreEqual(0, Pub().CardHolder);
            Assert.AreEqual(PlankMessage.CardPicked, Pub().LastMessage);
        }

        [Test]
        public void Plank_OnlyOneCardHolderAndOnlyHolderCanDrop()
        {
            Setup(3);
            Assert.IsTrue(Send(0, PlankCommand.PickUpCard()).IsAccepted);
            Assert.IsFalse(Send(1, PlankCommand.PickUpCard()).IsAccepted);
            Assert.IsFalse(Send(1, PlankCommand.DropCard()).IsAccepted);
            world.DropOnDeck = true;
            Assert.IsTrue(Send(0, PlankCommand.DropCard()).IsAccepted);
            PlankPublicState s = Pub();
            Assert.AreEqual(-1, s.CardHolder);
            Assert.IsTrue(s.CardOnDeck);
            Assert.AreEqual(PlankMessage.CardOnDeck, s.LastMessage);
        }

        [Test]
        public void Plank_DropOnFloorIsNotOnDeck()
        {
            Setup(2);
            Send(0, PlankCommand.PickUpCard());
            world.DropOnDeck = false;
            Send(0, PlankCommand.DropCard());
            Assert.IsFalse(Pub().CardOnDeck);
            Assert.AreEqual(PlankMessage.CardDropped, Pub().LastMessage);
        }

        [Test]
        public void Plank_LeverRejectionsLeaveStateUntouched()
        {
            Setup(4);
            int versionBefore = host.Version;
            // Chưa ai trên ván
            Assert.IsFalse(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            // Ba người trên ván
            world.OnDeck = new[] { 0, 1, 2 };
            Send(0, PlankCommand.PickUpCard());
            versionBefore = host.Version;
            Assert.IsFalse(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            // Thẻ không ở trên ván: người cầm thẻ (0) không đứng trên ván
            world.OnDeck = new[] { 1, 2 };
            Assert.IsFalse(Send(1, PlankCommand.UseLever(true)).IsAccepted);
            // Cần ở bờ B khi ván đang ở bờ A
            world.OnDeck = new[] { 0, 1 };
            Assert.IsFalse(Send(0, PlankCommand.UseLever(false)).IsAccepted);
            // Không đứng gần cần
            world.NotNearLever.Add(0);
            Assert.IsFalse(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            Assert.AreEqual(versionBefore, host.Version);
            Assert.AreEqual(PlankPhase.Idle, Pub().Phase);
        }

        [Test]
        public void Plank_LeverStartsCrossingWithSlowestPlayerTime()
        {
            Setup(3);
            Send(0, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 1 };
            Assert.IsTrue(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            PlankPublicState s = Pub();
            Assert.AreEqual(PlankPhase.Crossing, s.Phase);
            CollectionAssert.AreEqual(new[] { 0, 1 }, s.RidersCopy());
            Assert.AreEqual(2 * SecondsPerUnit, s.CrossingSecondsTotal, 0.001f);
            Assert.AreEqual(PlankMessage.CrossingStarted, s.LastMessage);
        }

        [Test]
        public void Plank_WhileCrossingLeverDropAndRidingCardAreRejected()
        {
            Setup(3);
            world.DropOnDeck = true;
            Send(0, PlankCommand.PickUpCard());
            Send(0, PlankCommand.DropCard());
            world.OnDeck = new[] { 0 };
            Assert.IsTrue(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            Assert.IsFalse(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            Assert.IsFalse(Send(1, PlankCommand.PickUpCard()).IsAccepted);
        }

        [Test]
        public void Plank_ArrivalHappensOnceAndMovesPeopleAndDeck()
        {
            Setup(3);
            Send(0, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 2 };
            Send(0, PlankCommand.UseLever(true));
            Advance(5 * SecondsPerUnit - 0.5f);
            Assert.AreEqual(PlankPhase.Crossing, Pub().Phase);
            Assert.IsTrue(rules.Tick(1f));
            Assert.IsFalse(rules.Tick(1f));
            PlankPublicState s = Pub();
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.IsFalse(s.DeckAtNear);
            Assert.AreEqual(PlankMessage.Arrived, s.LastMessage);
            PlankBank[] banks = s.PlayerBanksCopy();
            Assert.AreEqual(PlankBank.Far, banks[0]);
            Assert.AreEqual(PlankBank.Near, banks[1]);
            Assert.AreEqual(PlankBank.Far, banks[2]);
        }

        [Test]
        public void Plank_TimedOutFailsThenAutoResetsAfterDelay()
        {
            Setup(3); // tốc độ {1,2,5}, hạn 10 đơn vị
            Send(2, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 2 };
            Send(2, PlankCommand.UseLever(true));      // 5 đơn vị
            Advance(5 * SecondsPerUnit + 0.1f);
            world.OnDeck = new[] { 2 };
            Send(2, PlankCommand.UseLever(false));     // 5 đơn vị: đủ 10, đúng hạn
            Advance(5 * SecondsPerUnit + 0.1f);
            world.OnDeck = new[] { 1, 2 };
            Assert.IsTrue(Send(2, PlankCommand.UseLever(true)).IsAccepted); // 5 nữa: vượt hạn -> thất bại
            PlankPublicState failed = Pub();
            Assert.AreEqual(PlankStatus.Failed, failed.Status);
            Assert.AreEqual(PlankMessage.TimedOut, failed.LastMessage);
            Assert.IsTrue(rules.IsResetPending);
            Assert.IsFalse(Send(2, PlankCommand.PickUpCard()).IsAccepted);

            Assert.IsFalse(rules.Tick(1f));
            Assert.IsTrue(rules.Tick(2f));
            PlankPublicState again = Pub();
            Assert.AreEqual(PlankStatus.Playing, again.Status);
            Assert.IsTrue(again.DeckAtNear);
            Assert.AreEqual(-1, again.CardHolder);
            Assert.AreEqual(1, rules.ResetCount);
            Assert.AreEqual(PlankMessage.Reset, again.LastMessage);
            foreach (PlankBank bank in again.PlayerBanksCopy()) Assert.AreEqual(PlankBank.Near, bank);
        }

        [Test]
        public void Plank_RetryCommandResetsEvenMidCrossing()
        {
            Setup(3);
            Send(0, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 1 };
            Send(0, PlankCommand.UseLever(true));
            Assert.IsTrue(Send(1, PlankCommand.Retry()).IsAccepted);
            PlankPublicState s = Pub();
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.AreEqual(-1, s.CardHolder);
            Assert.AreEqual(1, s.ResetCount);
            Assert.IsTrue(s.DeckAtNear);
        }

        [Test]
        public void Plank_ThreePlayersSolveItThroughHostAndEveryReplicaSeesTheWin()
        {
            Setup(3); // 1,2,5 -> tối ưu 8, hạn 10
            ClientReplica<PlankPublicState, PlankPrivateState>[] replicas = new ClientReplica<PlankPublicState, PlankPrivateState>[3];
            for (int i = 0; i < 3; i++) replicas[i] = new ClientReplica<PlankPublicState, PlankPrivateState>(i);
            System.Action deliver = () =>
            {
                foreach (StateMessage<PlankPublicState, PlankPrivateState> m in host.BuildBroadcast()) replicas[m.Slot].Receive(m);
            };
            deliver();

            Assert.IsTrue(Send(0, PlankCommand.PickUpCard()).IsAccepted);
            world.OnDeck = new[] { 0, 2 };
            Assert.IsTrue(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            deliver();
            Assert.AreEqual(PlankPhase.Crossing, replicas[1].Public.Phase);
            Advance(5 * SecondsPerUnit + 0.1f);
            deliver();

            world.OnDeck = new[] { 0 };
            Assert.IsTrue(Send(0, PlankCommand.UseLever(false)).IsAccepted);
            Advance(1 * SecondsPerUnit + 0.1f);
            world.OnDeck = new[] { 0, 1 };
            Assert.IsTrue(Send(0, PlankCommand.UseLever(true)).IsAccepted);
            Assert.IsFalse(rules.Session.Solved, "chưa thắng khi nhóm cuối chưa đến nơi");
            Advance(2 * SecondsPerUnit + 0.1f);
            deliver();

            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(replicas[i].Public.Solved);
                Assert.AreEqual(PlankStatus.Won, replicas[i].Public.Status);
                Assert.AreEqual(PlankMessage.Solved, replicas[i].Public.LastMessage);
                Assert.AreEqual(speeds[i], replicas[i].Private.CrossUnits);
                Assert.AreEqual(host.Version, replicas[i].Version);
            }
        }

        [Test]
        public void Plank_TwoPlayersCrossTogetherAndWin()
        {
            Setup(2); // 2,5 -> cả hai qua cùng lúc: 5 đơn vị, hạn 7
            Send(0, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 1 };
            Assert.IsTrue(Send(1, PlankCommand.UseLever(true)).IsAccepted);
            Assert.IsFalse(Pub().Solved);
            Advance(5 * SecondsPerUnit + 0.1f);
            Assert.IsTrue(Pub().Solved);
        }

        [Test]
        public void Plank_LeverAfterWinIsRejectedUntilRetry()
        {
            Setup(2);
            Send(0, PlankCommand.PickUpCard());
            world.OnDeck = new[] { 0, 1 };
            Send(0, PlankCommand.UseLever(true));
            Advance(5 * SecondsPerUnit + 0.1f);
            world.OnDeck = new[] { 0 };
            Assert.IsFalse(Send(0, PlankCommand.UseLever(false)).IsAccepted);
            Assert.IsTrue(Send(0, PlankCommand.Retry()).IsAccepted);
            Assert.IsFalse(Pub().Solved);
        }

        [Test]
        public void Plank_ConstructorRejectsBadArguments()
        {
            FakeWorld w = new FakeWorld();
            PlankRound round = new PlankRound(1, new[] { 2, 5 }, 25);
            Assert.Throws<System.ArgumentNullException>(() => new PlankHostRules(round, 3f, null, 2.5f));
            Assert.Throws<System.ArgumentNullException>(() => new PlankHostRules(null, 3f, w, 2.5f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PlankHostRules(round, 0f, w, 2.5f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PlankHostRules(round, 3f, w, -1f));
        }

        [Test]
        public void Plank_NegativeTickThrows()
        {
            Setup(2);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rules.Tick(-1f));
        }
    }
}
