/*
 * Mục đích: NUnit EditMode test cho Hợp xướng (T-006d): trình sinh đề, thẻ nhạc, luật bấm bục và đặc biệt hạn chót giữa hai nốt
 * CHỈNH ĐƯỢC (StepWindowSeconds, 0 = không giới hạn, SwitchExtraSeconds khi đổi vai). Không cần scene hay Play Mode.
 * Hàm: Press (trợ giúp bấm nốt đúng); các test Rules_*, Generator_*, Cards_*, Sequence_*, Window_*, NoLimit_*, SwitchExtra_*, ApplyRules_*, Reset_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class ChoirTests
    {
        /// <summary>Nhận trạng thái, chỉ số nốt và thời gian; bấm đúng bục của nốt đó và trả kết quả.</summary>
        static ChoirResult PressRight(ChoirState s, int index, double now)
        {
            return s.Press(s.Round.SolutionPad(index), now);
        }

        /// <summary>Nhận bục đúng của nốt hiện tại cộng 1 (mod 4) để cố ý bấm sai.</summary>
        static int WrongPad(ChoirState s)
        {
            return (s.Round.SolutionPad(s.Progress) + 1) % ChoirRound.PadCount;
        }

        // ---------- Quy tắc thời gian ----------

        [Test]
        public void Rules_DefaultsOnlineAndValidation()
        {
            Assert.AreEqual(5.0, ChoirRules.Default.StepWindowSeconds);
            Assert.AreEqual(1.5, ChoirRules.Default.SwitchExtraSeconds);
            Assert.AreEqual(2.0, ChoirRules.Online.StepWindowSeconds);
            Assert.AreEqual(0.0, ChoirRules.Online.SwitchExtraSeconds);
            Assert.IsTrue(ChoirRules.Default.HasTimeLimit);
            Assert.IsFalse(new ChoirRules(0, 0).HasTimeLimit);
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirRules(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirRules(2, -0.1));
        }

        // ---------- Trình sinh đề ----------

        [Test]
        public void Generator_SatisfiesConstraints_ForManySeedsAndTeamSizes()
        {
            for (int players = 2; players <= 4; players++)
                for (int seed = 0; seed < 500; seed++)
                {
                    ChoirRound r = ChoirGenerator.Generate(players, seed);
                    Assert.AreEqual(ChoirGenerator.NoteCount, r.Length);
                    Assert.GreaterOrEqual(r.DistinctSymbolCount, 3, "seed " + seed);
                    for (int i = 1; i < r.Length; i++)
                    {
                        Assert.AreNotEqual(r.Symbol(i - 1), r.Symbol(i), "ký hiệu liền nhau, seed " + seed);
                        Assert.AreNotEqual(r.SolutionPad(i - 1), r.SolutionPad(i), "bục liền nhau, seed " + seed);
                    }
                    var pads = new HashSet<int>();
                    for (int s = 0; s < ChoirRound.PadCount; s++) pads.Add(r.PadOfSymbol(s));
                    Assert.AreEqual(ChoirRound.PadCount, pads.Count, "bảng phải là hoán vị, seed " + seed);
                }
        }

        [Test]
        public void Generator_IsDeterministicAndVaried()
        {
            ChoirRound a = ChoirGenerator.Generate(4, 12345);
            ChoirRound b = ChoirGenerator.Generate(4, 12345);
            for (int i = 0; i < a.Length; i++) Assert.AreEqual(a.Symbol(i), b.Symbol(i));
            for (int s = 0; s < ChoirRound.PadCount; s++) Assert.AreEqual(a.PadOfSymbol(s), b.PadOfSymbol(s));

            var distinct = new HashSet<string>();
            for (int seed = 0; seed < 60; seed++)
            {
                ChoirRound r = ChoirGenerator.Generate(4, seed);
                var key = new System.Text.StringBuilder();
                for (int i = 0; i < r.Length; i++) key.Append(r.SolutionPad(i));
                distinct.Add(key.ToString());
            }
            Assert.GreaterOrEqual(distinct.Count, 20, "đề phải đa dạng theo seed");
        }

        [Test]
        public void Generator_RejectsInvalidTeamSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ChoirGenerator.Generate(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ChoirGenerator.Generate(5, 0));
        }

        // ---------- Thẻ nhạc ----------

        [Test]
        public void Cards_SplitAcrossPlayers_AndCoverEverySymbol()
        {
            var expected = new Dictionary<int, int[]>
            {
                { 4, new[] { 1, 1, 1, 1 } },
                { 3, new[] { 2, 1, 1 } },
                { 2, new[] { 2, 2 } }
            };
            foreach (var kv in expected)
            {
                ChoirRound r = ChoirGenerator.Generate(kv.Key, 7);
                var all = new HashSet<int>();
                for (int p = 0; p < kv.Key; p++)
                {
                    int[] mine = r.SymbolsOf(p);
                    Assert.AreEqual(kv.Value[p], mine.Length, kv.Key + " người, người " + p);
                    foreach (int s in mine) Assert.IsTrue(all.Add(s), "ký hiệu bị chia hai lần");
                }
                Assert.AreEqual(ChoirRound.PadCount, all.Count);
            }
        }

        [Test]
        public void Cards_OnlyOwnerCanRead()
        {
            ChoirRound r = ChoirGenerator.Generate(4, 3);
            var s = new ChoirState(r);
            int pad;
            for (int symbol = 0; symbol < ChoirRound.PadCount; symbol++)
            {
                int owner = r.OwnerOfSymbol(symbol);
                Assert.IsTrue(s.TryReadCard(owner, symbol, out pad));
                Assert.AreEqual(r.PadOfSymbol(symbol), pad);
                Assert.IsFalse(s.TryReadCard((owner + 1) % 4, symbol, out pad));
                Assert.AreEqual(-1, pad);
            }
            Assert.IsFalse(s.TryReadCard(0, 9, out pad));
            Assert.IsFalse(s.TryReadCard(9, 0, out pad));
        }

        [Test]
        public void Round_RejectsBadInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirRound(1, 0, new[] { 0, 1 }, new[] { 0, 1, 2, 3 }));
            Assert.Throws<ArgumentException>(() => new ChoirRound(2, 0, new[] { 0 }, new[] { 0, 1, 2, 3 }));
            Assert.Throws<ArgumentException>(() => new ChoirRound(2, 0, new[] { 0, 1 }, new[] { 0, 1, 2 }));
            Assert.Throws<ArgumentException>(() => new ChoirRound(2, 0, new[] { 0, 1 }, new[] { 0, 0, 2, 3 }));
            Assert.Throws<ArgumentException>(() => new ChoirRound(2, 0, new[] { 0, 4 }, new[] { 0, 1, 2, 3 }));
        }

        // ---------- Luật bấm bục ----------

        [Test]
        public void Sequence_CorrectNotesSolve()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1));
            double t = 0;
            for (int i = 0; i < s.Round.Length - 1; i++)
            {
                Assert.AreEqual(ChoirResult.Correct, PressRight(s, i, t));
                Assert.AreEqual(i + 1, s.Progress);
                t += 1;
            }
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(ChoirResult.Completed, PressRight(s, s.Round.Length - 1, t));
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(ChoirResult.AlreadySolved, s.Press(0, t + 1));
        }

        [Test]
        public void Sequence_WrongPadResetsToFirstNoteAndCounts()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1));
            PressRight(s, 0, 0);
            PressRight(s, 1, 1);
            Assert.AreEqual(ChoirResult.Wrong, s.Press(WrongPad(s), 2));
            Assert.AreEqual(0, s.Progress);
            Assert.AreEqual(1, s.Mistakes);
            Assert.AreEqual(-1, s.RemainingSeconds(2));
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 0, 3), "làm lại từ đầu được");
        }

        [Test]
        public void Sequence_InvalidPadIsIgnored()
        {
            var s = new ChoirState(ChoirGenerator.Generate(3, 1));
            PressRight(s, 0, 0);
            Assert.AreEqual(ChoirResult.InvalidPad, s.Press(-1, 1));
            Assert.AreEqual(ChoirResult.InvalidPad, s.Press(4, 1));
            Assert.AreEqual(1, s.Progress);
            Assert.AreEqual(0, s.Mistakes);
        }

        // ---------- Hạn chót giữa hai nốt (chỉnh được) ----------

        [Test]
        public void Window_FirstNoteHasNoDeadline()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 0, 100000));
        }

        [Test]
        public void Window_AllowsGapUpToLimit_AndRejectsBeyond()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            PressRight(s, 0, 10);
            Assert.AreEqual(2.0, s.RemainingSeconds(10), 1e-9);
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 1, 12), "đúng bằng hạn chót vẫn được");
            Assert.AreEqual(ChoirResult.TooLate, PressRight(s, 2, 14.001));
            Assert.AreEqual(0, s.Progress);
            Assert.AreEqual(1, s.Timeouts);
            Assert.AreEqual(0, s.Mistakes, "quá giờ không tính là bấm sai");
        }

        [Test]
        public void Window_TickResetsWhenDeadlinePasses()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            PressRight(s, 0, 0);
            Assert.IsFalse(s.Tick(1.9));
            Assert.AreEqual(1, s.Progress);
            Assert.IsTrue(s.Tick(2.1));
            Assert.AreEqual(0, s.Progress);
            Assert.AreEqual(1, s.Timeouts);
            Assert.IsFalse(s.Tick(50), "đã reset, không đếm thêm");
            Assert.AreEqual(1, s.Timeouts);
        }

        [Test]
        public void Window_ValueIsConfigurable()
        {
            foreach (double window in new[] { 1.0, 2.0, 5.0, 10.0 })
            {
                var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(window, 0));
                PressRight(s, 0, 0);
                Assert.AreEqual(ChoirResult.Correct, PressRight(s, 1, window), "cửa sổ " + window);
                Assert.AreEqual(ChoirResult.TooLate, PressRight(s, 2, window + window + 0.5), "cửa sổ " + window);
            }
        }

        [Test]
        public void NoLimit_ZeroWindowNeverTimesOut()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(0, 1.5));
            Assert.IsFalse(s.Rules.HasTimeLimit);
            double t = 0;
            for (int i = 0; i < s.Round.Length - 1; i++)
            {
                Assert.AreEqual(ChoirResult.Correct, PressRight(s, i, t));
                Assert.IsFalse(s.Tick(t + 1000000));
                Assert.AreEqual(-1, s.RemainingSeconds(t));
                t += 100000;
            }
            Assert.AreEqual(ChoirResult.Completed, PressRight(s, s.Round.Length - 1, t));
            Assert.AreEqual(0, s.Timeouts);
        }

        // ---------- Cộng giờ khi đổi vai ----------

        [Test]
        public void SwitchExtra_AddsTimeMidSequence()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(5, 1.5));
            PressRight(s, 0, 0);
            Assert.AreEqual(1.5, s.ExtendForSwitch(1), 1e-9);
            Assert.AreEqual(5.5, s.RemainingSeconds(1), 1e-9);
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 1, 6.5), "đổi vai một lần: kịp tới 6,5 giây");
        }

        [Test]
        public void SwitchExtra_IsCappedPerStep()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 1.5));
            PressRight(s, 0, 0);
            Assert.AreEqual(1.5, s.ExtendForSwitch(0.1), 1e-9);
            Assert.AreEqual(0.5, s.ExtendForSwitch(0.2), 1e-9, "tổng cộng thêm tối đa bằng cửa sổ (2 giây)");
            Assert.AreEqual(0.0, s.ExtendForSwitch(0.3), 1e-9, "hết hạn mức cộng thêm");
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 1, 4.0));
        }

        [Test]
        public void SwitchExtra_ResetsAllowanceOnNextNote()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 1.5));
            PressRight(s, 0, 0);
            s.ExtendForSwitch(0.1);
            s.ExtendForSwitch(0.2);
            PressRight(s, 1, 1);
            Assert.AreEqual(1.5, s.ExtendForSwitch(1.1), 1e-9, "nốt mới có hạn mức mới");
        }

        [Test]
        public void SwitchExtra_NoEffectWhenNotApplicable()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(5, 1.5));
            Assert.AreEqual(0.0, s.ExtendForSwitch(0), "chưa bắt đầu dãy");
            PressRight(s, 0, 0);
            Assert.AreEqual(0.0, s.ExtendForSwitch(6), "đã quá hạn");
            var noLimit = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(0, 1.5));
            PressRight(noLimit, 0, 0);
            Assert.AreEqual(0.0, noLimit.ExtendForSwitch(1), "không giới hạn thời gian");
            var online = new ChoirState(ChoirGenerator.Generate(4, 1), ChoirRules.Online);
            PressRight(online, 0, 0);
            Assert.AreEqual(0.0, online.ExtendForSwitch(1), "chế độ online không cộng thêm");
        }

        [Test]
        public void Window_DevScenario_OneSwitchBetweenPadsFitsDefaultRules()
        {
            // Kịch bản thử một máy: bấm bục, đổi vai (phím 1-4), đi tới bục kế trong 6 giây.
            var s = new ChoirState(ChoirGenerator.Generate(4, 9), ChoirRules.Default);
            PressRight(s, 0, 0);
            s.ExtendForSwitch(1.0);
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 1, 6.0));
            Assert.AreEqual(2, s.Progress);
        }

        // ---------- Đổi quy tắc khi đang chơi / đặt lại ----------

        [Test]
        public void ApplyRules_ChangesNextNotes_NotTheCurrentDeadline()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            PressRight(s, 0, 0);
            s.ApplyRules(new ChoirRules(10, 0));
            Assert.AreEqual(2.0, s.RemainingSeconds(0), 1e-9, "hạn chót đang chờ giữ nguyên");
            PressRight(s, 1, 2);
            Assert.AreEqual(10.0, s.RemainingSeconds(2), 1e-9, "nốt sau dùng cửa sổ mới");
            Assert.Throws<ArgumentNullException>(() => s.ApplyRules(null));
        }

        [Test]
        public void ApplyRules_ToNoLimit_RemovesFutureDeadlines()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            PressRight(s, 0, 0);
            s.ApplyRules(new ChoirRules(0, 0));
            PressRight(s, 1, 1);
            Assert.AreEqual(-1, s.RemainingSeconds(1));
            Assert.AreEqual(ChoirResult.Correct, PressRight(s, 2, 99999));
        }

        [Test]
        public void Reset_RestartsSequenceButKeepsRoundRulesAndCounters()
        {
            var s = new ChoirState(ChoirGenerator.Generate(4, 1), new ChoirRules(2, 0));
            PressRight(s, 0, 0);
            s.Press(WrongPad(s), 1);
            for (int i = 0; i < s.Round.Length; i++) PressRight(s, i, 10 + i);
            Assert.IsTrue(s.Solved);
            ChoirRound round = s.Round;
            s.Reset();
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(0, s.Progress);
            Assert.AreSame(round, s.Round);
            Assert.AreEqual(2.0, s.Rules.StepWindowSeconds);
            Assert.AreEqual(1, s.Mistakes);
        }

        [Test]
        public void State_RejectsNullRound()
        {
            Assert.Throws<ArgumentNullException>(() => new ChoirState(null));
        }
    }
}
