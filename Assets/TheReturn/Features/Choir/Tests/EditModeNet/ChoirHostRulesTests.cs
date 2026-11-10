/*
 * Mục đích: Kiểm thử LUẬT HOST của «Hợp xướng» online (T-037) ở EditMode, không cần Unity chạy: bấm đúng cả bản nhạc thì thắng (đội 2/3/4),
 * cửa sổ giữa hai nốt theo số người, bấm sai/quá giờ/làm lại, bục ở xa bị host bác, thẻ nhạc riêng không lộ cho người khác,
 * trạng thái chung không đổi khi đồng hồ chạy (bất biến theo Version), cùng seed cùng đề.
 * Hàm: FakeChoirWorld; các test trong ChoirHostRulesTests.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.ChoirNet.Tests
{
    /// <summary>Thế giới giả: mặc định mọi ô đứng gần mọi bục; thêm ô vào FarSlots để cho ô đó đứng xa.</summary>
    public sealed class FakeChoirWorld : IChoirWorld
    {
        public readonly HashSet<int> FarSlots = new HashSet<int>();

        public bool IsNearPad(int slot, int pad)
        {
            return !FarSlots.Contains(slot);
        }
    }

    [TestFixture]
    public sealed class ChoirHostRulesTests
    {
        FakeChoirWorld world;
        ChoirHostRules rules;
        HostAuthority<ChoirCommand, ChoirPublicState, ChoirPrivateState> host;
        ClientReplica<ChoirPublicState, ChoirPrivateState>[] replicas;

        void Setup(int n, int seed = 7, ChoirRules choirRules = null)
        {
            world = new FakeChoirWorld();
            rules = new ChoirHostRules(n, seed, world, choirRules);
            host = new HostAuthority<ChoirCommand, ChoirPublicState, ChoirPrivateState>(new TeamRoster(n), rules, new HostLog(64));
            replicas = new ClientReplica<ChoirPublicState, ChoirPrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<ChoirPublicState, ChoirPrivateState>(i);
        }

        CommandResult Send(int slot, ChoirCommand command)
        {
            return host.Submit(slot, replicas[slot].NextSequence(), command);
        }

        ChoirPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        /// <summary>Bấm bục đúng của nốt hiện tại bằng ô cho trước; kiểm lệnh được nhận.</summary>
        void PressRight(int slot)
        {
            int pad = rules.Round.SolutionPad(rules.State.Progress);
            CommandResult r = Send(slot, ChoirCommand.Press(pad));
            Assert.IsTrue(r.IsAccepted, "Bấm bục đúng bị từ chối: " + r.Detail);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Solve_FullSong_WinsForEveryTeamSize(int n)
        {
            Setup(n);
            int notes = rules.Round.Length;
            for (int i = 0; i < notes; i++) PressRight(i % n);
            Assert.IsTrue(rules.Solved);
            ChoirPublicState p = Pub();
            Assert.IsTrue(p.Solved);
            Assert.AreEqual(notes, p.Progress);
            Assert.AreEqual(ChoirMessage.Completed, p.LastMessage);
            Assert.AreEqual(0, p.Mistakes);
            Assert.AreEqual(0, p.Timeouts);
        }

        [TestCase(2, 3.0)]
        [TestCase(3, 2.5)]
        [TestCase(4, 2.0)]
        public void Window_FollowsOnlineRulesByTeamSize(int n, double expected)
        {
            Setup(n);
            Assert.AreEqual(expected, Pub().WindowSeconds, 1e-9);
        }

        [Test]
        public void CustomRules_AreHonored()
        {
            Setup(2, 3, new ChoirRules(1.0, 0.0));
            Assert.AreEqual(1.0, Pub().WindowSeconds, 1e-9);
        }

        [Test]
        public void Press_FromFarSlot_IsRejectedAndChangesNothing()
        {
            Setup(3);
            world.FarSlots.Add(1);
            string before = ChoirCodec.EncodePublic(Pub());
            CommandResult r = Send(1, ChoirCommand.Press(rules.Round.SolutionPad(0)));
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(before, ChoirCodec.EncodePublic(Pub()), "Lệnh bị bác không được đổi trạng thái chung.");
            Assert.AreEqual(0, rules.State.Progress);
            PressRight(0);
            Assert.AreEqual(1, rules.State.Progress, "Ô đứng gần vẫn bấm được.");
        }

        [Test]
        public void Press_BadPad_IsRejected()
        {
            Setup(2);
            Assert.IsFalse(Send(0, ChoirCommand.Press(4)).IsAccepted);
            Assert.IsFalse(Send(0, ChoirCommand.Press(-1)).IsAccepted);
            Assert.AreEqual(0, Pub().Mistakes);
        }

        [Test]
        public void WrongPad_ResetsProgress_AndCountsMistake()
        {
            Setup(4);
            PressRight(0);
            Assert.AreEqual(1, rules.State.Progress);
            int wrong = (rules.Round.SolutionPad(1) + 1) % ChoirRound.PadCount;
            Assert.IsTrue(Send(2, ChoirCommand.Press(wrong)).IsAccepted, "Bấm sai vẫn là lệnh hợp lệ, luật chấm là sai.");
            ChoirPublicState p = Pub();
            Assert.AreEqual(0, p.Progress);
            Assert.AreEqual(1, p.Mistakes);
            Assert.AreEqual(ChoirMessage.Wrong, p.LastMessage);
            Assert.AreEqual(2, p.LastActor);
            Assert.AreEqual(wrong, p.LastPad);
        }

        [Test]
        public void Tick_PastWindow_ResetsSong_AndReportsOnce()
        {
            Setup(4);
            PressRight(0);
            Assert.IsFalse(rules.Tick(1.9f), "Còn trong cửa sổ 2 giây.");
            Assert.AreEqual(1, rules.State.Progress);
            Assert.IsTrue(rules.Tick(0.2f), "Quá cửa sổ thì dãy về đầu.");
            ChoirPublicState p = Pub();
            Assert.AreEqual(0, p.Progress);
            Assert.AreEqual(1, p.Timeouts);
            Assert.AreEqual(ChoirMessage.TimedOut, p.LastMessage);
            Assert.AreEqual(-1, p.LastActor);
            Assert.IsFalse(rules.Tick(5f), "Không còn hạn chót thì Tick không báo đổi nữa.");
        }

        [Test]
        public void Tick_BeforeFirstNote_HasNoDeadline()
        {
            Setup(3);
            Assert.IsFalse(rules.Tick(100f));
            Assert.AreEqual(0, Pub().Timeouts);
        }

        [Test]
        public void Tick_IgnoresBadDeltaAndSolvedState()
        {
            Setup(2);
            PressRight(0);
            Assert.IsFalse(rules.Tick(float.NaN));
            Assert.IsFalse(rules.Tick(-1f));
            Assert.IsFalse(rules.Tick(float.PositiveInfinity));
            Assert.AreEqual(1, rules.State.Progress);
            for (int i = 1; i < rules.Round.Length; i++) PressRight(i % 2);
            Assert.IsTrue(rules.Solved);
            Assert.IsFalse(rules.Tick(100f), "Đã thắng thì đồng hồ dừng.");
            Assert.IsTrue(Pub().Solved);
        }

        [Test]
        public void Retry_ResetsSong_AndIsRejectedOnceSolved()
        {
            Setup(3);
            PressRight(0);
            Assert.IsTrue(Send(1, ChoirCommand.Retry()).IsAccepted);
            ChoirPublicState p = Pub();
            Assert.AreEqual(0, p.Progress);
            Assert.AreEqual(1, p.ResetCount);
            Assert.AreEqual(ChoirMessage.Retry, p.LastMessage);
            Assert.AreEqual(1, p.LastActor);
            for (int i = 0; i < rules.Round.Length; i++) PressRight(i % 3);
            Assert.IsTrue(rules.Solved);
            Assert.IsFalse(Send(0, ChoirCommand.Retry()).IsAccepted, "Đã hát xong thì không làm lại.");
            Assert.IsFalse(Send(0, ChoirCommand.Press(0)).IsAccepted, "Đã hát xong thì không bấm thêm.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_CardsPartitionTheFourSymbols_AndMatchDomain(int n)
        {
            Setup(n, 21);
            bool[] seen = new bool[ChoirRound.PadCount];
            for (int slot = 0; slot < n; slot++)
            {
                ChoirPrivateState mine = rules.BuildPrivateState(slot);
                Assert.Greater(mine.OwnedCount, 0, "Mỗi người giữ ít nhất một thẻ.");
                int[] symbols = mine.SymbolsCopy();
                for (int i = 0; i < symbols.Length; i++)
                {
                    Assert.IsFalse(seen[symbols[i]], "Ký hiệu " + symbols[i] + " bị hai người giữ.");
                    seen[symbols[i]] = true;
                    int pad;
                    Assert.IsTrue(mine.TryGetPad(symbols[i], out pad));
                    Assert.AreEqual(rules.Round.PadOfSymbol(symbols[i]), pad);
                    for (int other = 0; other < n; other++)
                    {
                        if (other == slot) continue;
                        int leaked;
                        Assert.IsFalse(rules.BuildPrivateState(other).TryGetPad(symbols[i], out leaked), "Thẻ của ô " + slot + " lộ sang ô " + other);
                    }
                }
            }
            foreach (bool s in seen) Assert.IsTrue(s, "Còn ký hiệu không ai giữ.");
            Assert.AreEqual(0, rules.BuildPrivateState(n).OwnedCount, "Ô ngoài đội không có thẻ.");
        }

        [Test]
        public void PublicState_NeverContainsPadTable()
        {
            Setup(4, 5);
            PressRight(0);
            string text = ChoirCodec.EncodePublic(Pub());
            Assert.IsFalse(text.Contains("="), "Cặp «ký hiệu=bục» chỉ được gửi riêng.");
            Assert.IsFalse(text.StartsWith("o:", StringComparison.Ordinal));
            CollectionAssert.AreEqual(new[] { rules.Round.Symbol(0), rules.Round.Symbol(1), rules.Round.Symbol(2), rules.Round.Symbol(3), rules.Round.Symbol(4), rules.Round.Symbol(5) }, Pub().SymbolsCopy());
        }

        [Test]
        public void PublicState_IsStableWhileClockRuns()
        {
            Setup(4);
            PressRight(0);
            string a = ChoirCodec.EncodePublic(Pub());
            Assert.IsFalse(rules.Tick(0.5f));
            string b = ChoirCodec.EncodePublic(Pub());
            Assert.AreEqual(a, b, "Cùng một trạng thái thì cùng một chuỗi: đồng hồ đang chạy không được lọt vào gói.");
        }

        [Test]
        public void SameSeed_GivesSameSong_AndDifferentSeedsDiffer()
        {
            ChoirHostRules a = new ChoirHostRules(3, 99, new FakeChoirWorld());
            ChoirHostRules b = new ChoirHostRules(3, 99, new FakeChoirWorld());
            Assert.AreEqual(ChoirCodec.EncodePublic(a.BuildPublicState()), ChoirCodec.EncodePublic(b.BuildPublicState()));
            for (int slot = 0; slot < 3; slot++)
                Assert.AreEqual(ChoirCodec.EncodePrivate(a.BuildPrivateState(slot)), ChoirCodec.EncodePrivate(b.BuildPrivateState(slot)));
            bool anyDiffer = false;
            for (int seed = 100; seed < 120; seed++)
            {
                ChoirHostRules c = new ChoirHostRules(3, seed, new FakeChoirWorld());
                if (ChoirCodec.EncodePrivate(c.BuildPrivateState(0)) != ChoirCodec.EncodePrivate(a.BuildPrivateState(0))
                    || ChoirCodec.EncodePublic(c.BuildPublicState()) != ChoirCodec.EncodePublic(a.BuildPublicState())) anyDiffer = true;
            }
            Assert.IsTrue(anyDiffer, "20 seed khác nhau mà đề y hệt: bộ sinh có vấn đề.");
        }

        [Test]
        public void Constructor_RejectsBadArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ChoirHostRules(2, 1, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirHostRules(1, 1, new FakeChoirWorld()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirHostRules(5, 1, new FakeChoirWorld()));
            Assert.Throws<ArgumentNullException>(() => new ChoirHostRules((ChoirRound)null, new FakeChoirWorld()));
        }
    }
}
