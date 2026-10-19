/*
 * Mục đích: NUnit EditMode test cho luật «Đèn lồng chung» (T-006f): trình sinh xác định, sổ riêng từng người, trao đèn trong tầm,
 * ngoài vùng sáng quá giờ bị đưa về đầu, bảng nhập cuối hành lang, điều kiện hoàn thành, thử lại. Không cần scene hay Play Mode.
 * Hàm: Make; Lit; các test Generator_*, Round_*, Read_*, Pass_*, Tick_*, Submit_*, Complete_*, Reset_*, Ctor_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class SharedLanternTests
    {
        /// <summary>Nhận số người và seed; trả trạng thái mới với đề sinh từ seed (giới hạn mặc định 5 giây, tầm trao 2 mét).</summary>
        static LanternState Make(int players = 3, int seed = 42) { return new LanternState(LanternGenerator.Generate(players, seed)); }

        /// <summary>Nhận danh sách true/false theo từng người; trả mảng inLight cho Tick.</summary>
        static bool[] Lit(params bool[] values) { return values; }

        /// <summary>Nhận trạng thái; nhập đúng đáp án vào cả 4 ô.</summary>
        static void EnterSolution(LanternState s)
        {
            for (int i = 0; i < LanternRound.SignCount; i++) Assert.IsTrue(s.SetEntry(i, s.Round.SymbolOfSign(i)));
        }

        // ---------- Trình sinh và đề ----------

        [Test]
        public void Generator_IsDeterministicPerSeed()
        {
            CollectionAssert.AreEqual(LanternGenerator.Generate(3, 777).Solution(), LanternGenerator.Generate(3, 777).Solution());
        }

        [Test]
        public void Generator_SymbolsAreDistinctAndInRange()
        {
            for (int players = 2; players <= 4; players++)
                for (int seed = -50; seed < 200; seed++)
                {
                    int[] sol = LanternGenerator.Generate(players, seed).Solution();
                    Assert.AreEqual(LanternRound.SignCount, sol.Length);
                    var seen = new HashSet<int>();
                    foreach (int s in sol)
                    {
                        Assert.GreaterOrEqual(s, 0);
                        Assert.Less(s, LanternRound.SymbolCount);
                        Assert.IsTrue(seen.Add(s), "ký hiệu trùng ở seed " + seed);
                    }
                }
        }

        [Test]
        public void Generator_DifferentSeedsGiveVariety()
        {
            var distinct = new HashSet<string>();
            for (int seed = 0; seed < 100; seed++) distinct.Add(string.Join(",", LanternGenerator.Generate(3, seed).Solution()));
            Assert.Greater(distinct.Count, 20);
        }

        [Test]
        public void Generator_RejectsBadPlayerCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LanternGenerator.Generate(1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => LanternGenerator.Generate(5, 1));
        }

        [Test]
        public void Round_RejectsBadSymbols()
        {
            Assert.Throws<ArgumentException>(() => new LanternRound(1, 3, new[] { 0, 0, 1, 2 }));
            Assert.Throws<ArgumentException>(() => new LanternRound(1, 3, new[] { 0, 1, 2, 6 }));
            Assert.Throws<ArgumentException>(() => new LanternRound(1, 3, new[] { 0, 1, 2 }));
            Assert.Throws<ArgumentException>(() => new LanternRound(1, 3, null));
        }

        [Test]
        public void Round_SymbolOfSignAndSolutionCopy()
        {
            var r = new LanternRound(1, 2, new[] { 5, 3, 1, 0 });
            Assert.AreEqual(5, r.SymbolOfSign(0));
            Assert.AreEqual(0, r.SymbolOfSign(3));
            Assert.AreEqual(-1, r.SymbolOfSign(4));
            Assert.AreEqual(-1, r.SymbolOfSign(-1));
            int[] sol = r.Solution();
            sol[0] = 2;
            Assert.AreEqual(5, r.SymbolOfSign(0));
        }

        // ---------- Đọc biển, sổ riêng ----------

        [Test]
        public void Read_InLight_RecordsOnlyInOwnNotebook()
        {
            LanternState s = Make();
            Assert.AreEqual(LanternReadResult.Read, s.Read(0, 2, true));
            Assert.IsTrue(s.HasRead(0, 2));
            Assert.AreEqual(s.Round.SymbolOfSign(2), s.SymbolSeen(0, 2));
            Assert.IsFalse(s.HasRead(1, 2));
            Assert.AreEqual(-1, s.SymbolSeen(1, 2));
            Assert.AreEqual(4, s.ReadMask(0));
            Assert.AreEqual(0, s.ReadMask(1));
        }

        [Test]
        public void Read_NotInLight_IsRefusedAndNotRecorded()
        {
            LanternState s = Make();
            Assert.AreEqual(LanternReadResult.NotInLight, s.Read(0, 1, false));
            Assert.IsFalse(s.HasRead(0, 1));
            Assert.AreEqual(0, s.ReadMask(0));
        }

        [Test]
        public void Read_Twice_IsAlreadyRead()
        {
            LanternState s = Make();
            s.Read(0, 0, true);
            Assert.AreEqual(LanternReadResult.AlreadyRead, s.Read(0, 0, true));
            Assert.AreEqual(LanternReadResult.AlreadyRead, s.Read(0, 0, false));
        }

        [Test]
        public void Read_InvalidPlayerOrSign()
        {
            LanternState s = Make(3);
            Assert.AreEqual(LanternReadResult.InvalidPlayer, s.Read(3, 0, true));
            Assert.AreEqual(LanternReadResult.InvalidPlayer, s.Read(-1, 0, true));
            Assert.AreEqual(LanternReadResult.InvalidSign, s.Read(0, 4, true));
            Assert.AreEqual(LanternReadResult.InvalidSign, s.Read(0, -1, true));
        }

        [Test]
        public void Read_TwoPlayers_OnePlayerMayReadAllSigns()
        {
            LanternState s = Make(2);
            for (int sign = 0; sign < LanternRound.SignCount; sign++) Assert.AreEqual(LanternReadResult.Read, s.Read(1, sign, true));
            Assert.AreEqual(15, s.ReadMask(1));
            Assert.AreEqual(15, s.TeamReadMask());
        }

        [Test]
        public void TeamReadMask_IsUnionOfNotebooks()
        {
            LanternState s = Make(3);
            s.Read(0, 0, true);
            s.Read(1, 3, true);
            s.Read(2, 3, true);
            Assert.AreEqual(1 | 8, s.TeamReadMask());
        }

        // ---------- Trao đèn ----------

        [Test]
        public void Pass_HolderStartsAsPlayerZero_AndPassesWithinRange()
        {
            LanternState s = Make();
            Assert.AreEqual(0, s.Holder);
            Assert.AreEqual(LanternPassResult.Passed, s.TryPass(0, 2, 2.0));
            Assert.AreEqual(2, s.Holder);
        }

        [Test]
        public void Pass_TooFarOrNotHolderOrInvalid()
        {
            LanternState s = Make();
            Assert.AreEqual(LanternPassResult.TooFar, s.TryPass(0, 1, 2.01));
            Assert.AreEqual(LanternPassResult.TooFar, s.TryPass(0, 1, double.NaN));
            Assert.AreEqual(0, s.Holder);
            Assert.AreEqual(LanternPassResult.NotHolder, s.TryPass(1, 2, 0.5));
            Assert.AreEqual(LanternPassResult.InvalidPlayer, s.TryPass(0, 0, 0.5));
            Assert.AreEqual(LanternPassResult.InvalidPlayer, s.TryPass(0, 3, 0.5));
            Assert.AreEqual(LanternPassResult.InvalidPlayer, s.TryPass(-1, 1, 0.5));
        }

        // ---------- Ngoài vùng sáng ----------

        [Test]
        public void Tick_InsideLight_NeverSent()
        {
            LanternState s = Make(3);
            for (double t = 0; t < 100; t += 1) Assert.AreEqual(0, s.Tick(t, Lit(true, true, true)));
            Assert.AreEqual(0, s.SentBackCount);
        }

        [Test]
        public void Tick_OutsideAtOrUnderLimit_NotSent()
        {
            LanternState s = Make(3);
            Assert.AreEqual(0, s.Tick(10.0, Lit(true, false, true)));
            Assert.AreEqual(0, s.Tick(15.0, Lit(true, false, true)));
            Assert.AreEqual(5.0, s.OutsideSeconds(1, 15.0), 1e-9);
        }

        [Test]
        public void Tick_OutsideOverLimit_SentBackAndTimerRestarts()
        {
            LanternState s = Make(3);
            s.Tick(0.0, Lit(true, false, true));
            Assert.AreEqual(1 << 1, s.Tick(5.01, Lit(true, false, true)));
            Assert.AreEqual(1, s.SentBackCount);
            Assert.AreEqual(0, s.Tick(6.0, Lit(true, false, true)));
            Assert.AreEqual(1 << 1, s.Tick(10.02, Lit(true, false, true)));
            Assert.AreEqual(2, s.SentBackCount);
        }

        [Test]
        public void Tick_ReturningToLightResetsTimer()
        {
            LanternState s = Make(3);
            s.Tick(0.0, Lit(true, false, true));
            s.Tick(4.0, Lit(true, false, true));
            s.Tick(4.5, Lit(true, true, true));
            Assert.AreEqual(0, s.OutsideSeconds(1, 4.5), 1e-9);
            s.Tick(5.0, Lit(true, false, true));
            Assert.AreEqual(0, s.Tick(9.9, Lit(true, false, true)));
            Assert.AreEqual(1 << 1, s.Tick(10.1, Lit(true, false, true)));
        }

        [Test]
        public void Tick_HolderIsNeverSentBack()
        {
            LanternState s = Make(3);
            s.Tick(0.0, Lit(false, true, true));
            Assert.AreEqual(0, s.Tick(100.0, Lit(false, true, true)));
            Assert.AreEqual(0, s.SentBackCount);
        }

        [Test]
        public void Tick_PassingLightRestartsOldHolderTimer()
        {
            LanternState s = Make(3);
            Assert.AreEqual(LanternPassResult.Passed, s.TryPass(0, 1, 1.0));
            s.Tick(0.0, Lit(false, true, true));
            Assert.AreEqual(0, s.Tick(5.0, Lit(false, true, true)));
            Assert.AreEqual(1, s.Tick(5.5, Lit(false, true, true)));
        }

        [Test]
        public void Tick_MultiplePlayersAtOnce()
        {
            LanternState s = Make(4);
            s.Tick(0.0, Lit(true, false, false, true));
            Assert.AreEqual((1 << 1) | (1 << 2), s.Tick(5.5, Lit(true, false, false, true)));
            Assert.AreEqual(2, s.SentBackCount);
        }

        [Test]
        public void Tick_WrongArrayLength_Throws()
        {
            LanternState s = Make(3);
            Assert.Throws<ArgumentException>(() => s.Tick(0.0, Lit(true, true)));
            Assert.Throws<ArgumentException>(() => s.Tick(0.0, null));
        }

        [Test]
        public void SentBack_KeepsNotebook()
        {
            LanternState s = Make(3);
            s.Read(1, 0, true);
            s.Tick(0.0, Lit(true, false, true));
            s.Tick(6.0, Lit(true, false, true));
            Assert.IsTrue(s.HasRead(1, 0));
        }

        // ---------- Bảng nhập cuối hành lang ----------

        [Test]
        public void Submit_IncompleteUntilAllFourSlotsFilled()
        {
            LanternState s = Make();
            Assert.AreEqual(LanternSubmitResult.Incomplete, s.Submit(0.0));
            s.SetEntry(0, 0); s.SetEntry(1, 1); s.SetEntry(2, 2);
            Assert.AreEqual(LanternSubmitResult.Incomplete, s.Submit(0.0));
            Assert.AreEqual(0, s.WrongSubmits);
        }

        [Test]
        public void Submit_Wrong_CountsClearsEntriesAndLocks()
        {
            LanternState s = Make();
            int[] sol = s.Round.Solution();
            for (int i = 0; i < 4; i++) s.SetEntry(i, sol[i]);
            s.SetEntry(0, (sol[0] + 1) % LanternRound.SymbolCount);
            Assert.AreEqual(LanternSubmitResult.WrongCode, s.Submit(100.0));
            Assert.AreEqual(1, s.WrongSubmits);
            for (int i = 0; i < 4; i++) Assert.AreEqual(-1, s.Entry(i));
            Assert.IsFalse(s.CodeAccepted);
            Assert.AreEqual(10.0, s.LockRemaining(100.0), 1e-9);
        }

        [Test]
        public void Submit_WhileLocked_IsRefusedEvenIfCorrect()
        {
            LanternState s = Make();
            s.SetEntry(0, (s.Round.SymbolOfSign(0) + 1) % LanternRound.SymbolCount);
            s.SetEntry(1, s.Round.SymbolOfSign(1)); s.SetEntry(2, s.Round.SymbolOfSign(2)); s.SetEntry(3, s.Round.SymbolOfSign(3));
            Assert.AreEqual(LanternSubmitResult.WrongCode, s.Submit(50.0));
            EnterSolution(s);
            Assert.AreEqual(LanternSubmitResult.Locked, s.Submit(59.9));
            Assert.AreEqual(1, s.WrongSubmits);
            Assert.AreEqual(0.1, s.LockRemaining(59.9), 1e-9);
            Assert.IsFalse(s.CodeAccepted);
        }

        [Test]
        public void Submit_AfterLockExpires_CanBeAccepted()
        {
            LanternState s = Make();
            s.SetEntry(0, (s.Round.SymbolOfSign(0) + 1) % LanternRound.SymbolCount);
            s.SetEntry(1, s.Round.SymbolOfSign(1)); s.SetEntry(2, s.Round.SymbolOfSign(2)); s.SetEntry(3, s.Round.SymbolOfSign(3));
            s.Submit(50.0);
            EnterSolution(s);
            Assert.AreEqual(0.0, s.LockRemaining(60.0), 1e-9);
            Assert.AreEqual(LanternSubmitResult.Accepted, s.Submit(60.0));
            Assert.IsTrue(s.CodeAccepted);
        }

        [Test]
        public void Submit_SecondWrong_LocksAgain()
        {
            LanternState s = Make();
            for (int round = 0; round < 2; round++)
            {
                s.SetEntry(0, (s.Round.SymbolOfSign(0) + 1) % LanternRound.SymbolCount);
                s.SetEntry(1, s.Round.SymbolOfSign(1)); s.SetEntry(2, s.Round.SymbolOfSign(2)); s.SetEntry(3, s.Round.SymbolOfSign(3));
                Assert.AreEqual(LanternSubmitResult.WrongCode, s.Submit(100.0 + round * 20));
            }
            Assert.AreEqual(2, s.WrongSubmits);
            Assert.AreEqual(10.0, s.LockRemaining(120.0), 1e-9);
        }

        [Test]
        public void Submit_IncompleteDoesNotLockOrCount()
        {
            LanternState s = Make();
            s.SetEntry(0, 1);
            Assert.AreEqual(LanternSubmitResult.Incomplete, s.Submit(10.0));
            Assert.AreEqual(0, s.WrongSubmits);
            Assert.AreEqual(0.0, s.LockRemaining(10.0), 1e-9);
        }

        [Test]
        public void Submit_OrderMatters()
        {
            LanternState s = Make();
            int[] sol = s.Round.Solution();
            s.SetEntry(0, sol[1]); s.SetEntry(1, sol[0]); s.SetEntry(2, sol[2]); s.SetEntry(3, sol[3]);
            Assert.AreEqual(LanternSubmitResult.WrongCode, s.Submit(0.0));
        }

        [Test]
        public void Submit_Correct_AcceptedThenLocked()
        {
            LanternState s = Make();
            EnterSolution(s);
            Assert.AreEqual(LanternSubmitResult.Accepted, s.Submit(0.0));
            Assert.IsTrue(s.CodeAccepted);
            Assert.AreEqual(LanternSubmitResult.AlreadyAccepted, s.Submit(0.0));
            Assert.IsFalse(s.SetEntry(0, 0));
            s.ClearEntries();
            Assert.AreEqual(s.Round.SymbolOfSign(0), s.Entry(0));
        }

        [Test]
        public void SetEntry_RejectsOutOfRange()
        {
            LanternState s = Make();
            Assert.IsFalse(s.SetEntry(-1, 0));
            Assert.IsFalse(s.SetEntry(4, 0));
            Assert.IsFalse(s.SetEntry(0, -1));
            Assert.IsFalse(s.SetEntry(0, 6));
            Assert.AreEqual(-1, s.Entry(9));
        }

        [Test]
        public void ClearEntries_EmptiesBeforeAcceptance()
        {
            LanternState s = Make();
            s.SetEntry(0, 1); s.SetEntry(3, 2);
            s.ClearEntries();
            for (int i = 0; i < 4; i++) Assert.AreEqual(-1, s.Entry(i));
        }

        // ---------- Hoàn thành ----------

        [Test]
        public void Complete_NeedsAcceptedCodeAndWholeTeamAtGate()
        {
            LanternState s = Make();
            Assert.IsFalse(s.TryComplete(true));
            EnterSolution(s);
            Assert.IsFalse(s.TryComplete(true));
            Assert.AreEqual(LanternSubmitResult.Accepted, s.Submit(0.0));
            Assert.IsFalse(s.TryComplete(false));
            Assert.IsFalse(s.Solved);
            Assert.IsTrue(s.TryComplete(true));
            Assert.IsTrue(s.Solved);
        }

        // ---------- Thử lại và tham số ----------

        [Test]
        public void Reset_ClearsEverythingButKeepsRound()
        {
            LanternState s = Make();
            LanternRound round = s.Round;
            s.Read(0, 0, true);
            s.TryPass(0, 1, 1.0);
            s.Tick(0.0, Lit(false, true, false));
            s.Tick(6.0, Lit(false, true, false));
            EnterSolution(s);
            s.Submit(0.0);
            s.TryComplete(true);
            s.Reset();
            Assert.AreSame(round, s.Round);
            Assert.AreEqual(0, s.Holder);
            Assert.AreEqual(0, s.TeamReadMask());
            Assert.AreEqual(0, s.SentBackCount);
            Assert.AreEqual(0, s.WrongSubmits);
            Assert.IsFalse(s.CodeAccepted);
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(-1, s.Entry(0));
            Assert.AreEqual(0, s.OutsideSeconds(0, 100.0), 1e-9);
            Assert.AreEqual(0.0, s.LockRemaining(0.0), 1e-9);
        }

        [Test]
        public void Ctor_RejectsBadArguments()
        {
            LanternRound r = LanternGenerator.Generate(3, 1);
            Assert.Throws<ArgumentNullException>(() => new LanternState(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LanternState(r, 0, 2.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LanternState(r, 5.0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LanternState(r, double.NaN, 2.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LanternState(r, 5.0, 2.0, -1.0));
        }

        [Test]
        public void CustomLimits_AreHonored()
        {
            var s = new LanternState(LanternGenerator.Generate(2, 1), 2.0, 3.5);
            Assert.AreEqual(LanternPassResult.Passed, s.TryPass(0, 1, 3.5));
            s.Tick(0.0, Lit(true, true));
            s.Tick(1.0, Lit(false, true));
            Assert.AreEqual(0, s.Tick(3.0, Lit(false, true)));
            Assert.AreEqual(1, s.Tick(3.1, Lit(false, true)));
        }
    }
}
