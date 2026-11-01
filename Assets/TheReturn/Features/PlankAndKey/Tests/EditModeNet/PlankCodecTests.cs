/*
 * Mục đích: NUnit EditMode test cho PlankCodec và vị trí thẻ (T-027): khứ hồi lệnh/trạng thái/thông tin riêng, đọc đúng dù máy dùng dấu phẩy
 * thập phân, và từ chối dữ liệu xấu từ mạng; thêm test chế độ thẻ (AtHome/Held/OnFloor/OnDeck) của luật host.
 * Hàm: các test Codec_* và Card_*. Không cần scene hay Play Mode.
 */
using System.Globalization;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.PlankNet.Tests
{
    public sealed class PlankCodecTests
    {
        static PlankHostRules NewRules(FakeWorld world)
        {
            return new PlankHostRules(new PlankRound(1, new[] { 1, 2, 5 }, 25), 3f, world, 2.5f);
        }

        static void Do(PlankHostRules rules, int slot, PlankCommand command)
        {
            RuleVerdict verdict = rules.Validate(slot, command);
            Assert.IsTrue(verdict.IsAccepted, verdict.Reason);
            rules.Apply(slot, command);
        }

        [TestCase(PlankCommandKind.PickUpCard, false)]
        [TestCase(PlankCommandKind.DropCard, false)]
        [TestCase(PlankCommandKind.UseLever, true)]
        [TestCase(PlankCommandKind.UseLever, false)]
        [TestCase(PlankCommandKind.Retry, false)]
        public void Codec_CommandRoundTrips(PlankCommandKind kind, bool sideA)
        {
            PlankCommand source = kind == PlankCommandKind.UseLever ? PlankCommand.UseLever(sideA)
                : kind == PlankCommandKind.PickUpCard ? PlankCommand.PickUpCard()
                : kind == PlankCommandKind.DropCard ? PlankCommand.DropCard() : PlankCommand.Retry();
            PlankCommand back;
            Assert.IsTrue(PlankCodec.TryDecodeCommand(PlankCodec.EncodeCommand(source), out back));
            Assert.AreEqual(source.Kind, back.Kind);
            Assert.AreEqual(source.SideA, back.SideA);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("x")]
        [TestCase("9|0")]
        [TestCase("-1|0")]
        [TestCase("0|2")]
        [TestCase("0")]
        [TestCase("0|0|0")]
        [TestCase("a|b")]
        public void Codec_BadCommandsAreRejectedNotThrown(string text)
        {
            PlankCommand command;
            Assert.IsFalse(PlankCodec.TryDecodeCommand(text, out command));
            Assert.IsNull(command);
        }

        [Test]
        public void Codec_PublicStateRoundTripsMidCrossingWithCardOnDeck()
        {
            FakeWorld world = new FakeWorld();
            PlankHostRules rules = NewRules(world);
            Do(rules, 0, PlankCommand.PickUpCard());
            world.DropOnDeck = true;
            Do(rules, 0, PlankCommand.DropCard());
            world.OnDeck = new[] { 0, 1 };
            Do(rules, 1, PlankCommand.UseLever(true));
            rules.Tick(1.25f);

            PlankPublicState source = rules.BuildPublicState();
            PlankPublicState back;
            Assert.IsTrue(PlankCodec.TryDecodePublic(PlankCodec.EncodePublic(source), out back));
            AssertSame(source, back);
            Assert.AreEqual(PlankPhase.Crossing, back.Phase);
            Assert.AreEqual(PlankCardMode.OnDeck, back.CardMode);
        }

        [Test]
        public void Codec_PublicStateReadsCorrectlyWithCommaDecimalCulture()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("vi-VN");
                FakeWorld world = new FakeWorld();
                PlankHostRules rules = NewRules(world);
                Do(rules, 0, PlankCommand.PickUpCard());
                world.OnDeck = new[] { 0, 1 };
                Do(rules, 1, PlankCommand.UseLever(true));
                rules.Tick(0.37f);
                PlankPublicState source = rules.BuildPublicState();
                string text = PlankCodec.EncodePublic(source);
                PlankPublicState back;
                Assert.IsTrue(PlankCodec.TryDecodePublic(text, out back));
                AssertSame(source, back);
                Assert.AreEqual(18, text.Split('|').Length);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Test]
        public void Codec_PrivateStateRoundTripsAndRejectsBadInput()
        {
            PlankPrivateState back;
            Assert.IsTrue(PlankCodec.TryDecodePrivate(PlankCodec.EncodePrivate(new PlankPrivateState(5)), out back));
            Assert.AreEqual(5, back.CrossUnits);
            Assert.IsFalse(PlankCodec.TryDecodePrivate("-1", out back));
            Assert.IsFalse(PlankCodec.TryDecodePrivate("abc", out back));
            Assert.IsFalse(PlankCodec.TryDecodePrivate(null, out back));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("1|2|3")]
        [TestCase("garbage")]
        public void Codec_BadPublicStatesAreRejectedNotThrown(string text)
        {
            PlankPublicState state;
            Assert.IsFalse(PlankCodec.TryDecodePublic(text, out state));
            Assert.IsNull(state);
        }

        [Test]
        public void Codec_PublicStateRejectsTamperedFields()
        {
            PlankHostRules rules = NewRules(new FakeWorld());
            string good = PlankCodec.EncodePublic(rules.BuildPublicState());
            string[] f = good.Split('|');
            PlankPublicState state;
            Assert.IsTrue(PlankCodec.TryDecodePublic(good, out state));

            Assert.IsFalse(PlankCodec.TryDecodePublic(good + "|0", out state), "thừa trường");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 0, "9"), out state), "số người ngoài 2-4");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 1, "99"), out state), "pha không có");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 4, "NaN"), out state), "số không hữu hạn");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 5, "-3"), out state), "thời gian âm");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 6, "7"), out state), "người trên ván ngoài đội");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 6, "0,1,2"), out state), "quá 2 người trên ván");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 7, "0,0"), out state), "số bờ không khớp số người");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 8, "9"), out state), "người cầm thẻ ngoài đội");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 9, "9"), out state), "chế độ thẻ không có");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 16, "2"), out state), "cờ bool sai");
            Assert.IsFalse(PlankCodec.TryDecodePublic(Tamper(f, 17, "99"), out state), "thông báo không có");
        }

        static string Tamper(string[] fields, int index, string value)
        {
            string[] copy = (string[])fields.Clone();
            copy[index] = value;
            return string.Join("|", copy);
        }

        static void AssertSame(PlankPublicState a, PlankPublicState b)
        {
            Assert.AreEqual(a.PlayerCount, b.PlayerCount);
            Assert.AreEqual(a.Phase, b.Phase);
            Assert.AreEqual(a.Status, b.Status);
            Assert.AreEqual(a.DeckAtNear, b.DeckAtNear);
            Assert.AreEqual(a.CrossingSecondsTotal, b.CrossingSecondsTotal);
            Assert.AreEqual(a.CrossingSecondsLeft, b.CrossingSecondsLeft);
            CollectionAssert.AreEqual(a.RidersCopy(), b.RidersCopy());
            CollectionAssert.AreEqual(a.PlayerBanksCopy(), b.PlayerBanksCopy());
            Assert.AreEqual(a.CardHolder, b.CardHolder);
            Assert.AreEqual(a.CardMode, b.CardMode);
            Assert.AreEqual(a.CardLocalX, b.CardLocalX);
            Assert.AreEqual(a.CardLocalZ, b.CardLocalZ);
            Assert.AreEqual(a.DisplayUsedSeconds, b.DisplayUsedSeconds);
            Assert.AreEqual(a.DeadlineSeconds, b.DeadlineSeconds);
            Assert.AreEqual(a.Attempts, b.Attempts);
            Assert.AreEqual(a.ResetCount, b.ResetCount);
            Assert.AreEqual(a.Solved, b.Solved);
            Assert.AreEqual(a.LastMessage, b.LastMessage);
        }

        [Test]
        public void Card_ModeFollowsPickDropAndReset()
        {
            FakeWorld world = new FakeWorld();
            PlankHostRules rules = NewRules(world);
            Assert.AreEqual(PlankCardMode.AtHome, rules.BuildPublicState().CardMode);

            Do(rules, 0, PlankCommand.PickUpCard());
            Assert.AreEqual(PlankCardMode.Held, rules.BuildPublicState().CardMode);

            world.DropOnDeck = false;
            Do(rules, 0, PlankCommand.DropCard());
            PlankPublicState floor = rules.BuildPublicState();
            Assert.AreEqual(PlankCardMode.OnFloor, floor.CardMode);
            Assert.AreEqual(0.5f, floor.CardLocalX);
            Assert.AreEqual(1.5f, floor.CardLocalZ);

            Do(rules, 1, PlankCommand.PickUpCard());
            Assert.AreEqual(PlankCardMode.Held, rules.BuildPublicState().CardMode);
            Assert.AreEqual(0f, rules.BuildPublicState().CardLocalX);

            world.DropOnDeck = true;
            Do(rules, 1, PlankCommand.DropCard());
            Assert.AreEqual(PlankCardMode.OnDeck, rules.BuildPublicState().CardMode);

            Do(rules, 2, PlankCommand.Retry());
            Assert.AreEqual(PlankCardMode.AtHome, rules.BuildPublicState().CardMode);
        }

        [Test]
        public void Card_OnDeckStaysOnDeckAfterArrivalAndCanBePickedUp()
        {
            FakeWorld world = new FakeWorld();
            PlankHostRules rules = NewRules(world);
            Do(rules, 0, PlankCommand.PickUpCard());
            world.DropOnDeck = true;
            Do(rules, 0, PlankCommand.DropCard());
            world.OnDeck = new[] { 0 };
            Do(rules, 0, PlankCommand.UseLever(true));
            Assert.IsTrue(rules.Tick(3f * 1f + 0.1f));
            Assert.AreEqual(PlankCardMode.OnDeck, rules.BuildPublicState().CardMode);
            Assert.IsTrue(rules.Validate(0, PlankCommand.PickUpCard()).IsAccepted);
        }
    }
}
