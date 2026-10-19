/*
 * Mục đích: NUnit EditMode test cho PlankSession (T-006b2): thời gian của lượt qua cầu, chặn lượt chồng, đến nơi đúng một lần,
 * đồng hồ HUD liên tục, thắng chỉ sau khi nhóm cuối đến nơi, hết hạn, reset, kiểm tham số, chơi hết kế hoạch tối ưu theo thời gian thực.
 * Hàm: NewSession (trợ giúp); các test Session_*. Không cần scene hay Play Mode.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class PlankSessionTests
    {
        const float Tol = 0.0001f;

        /// <summary>Nhận thời gian qua cầu, phần dư % và giây mỗi đơn vị; trả phiên mới.</summary>
        static PlankSession NewSession(int[] units, int slack, float spu)
        {
            return new PlankSession(new PlankRound(1, units, slack), spu);
        }

        [Test]
        public void Session_StartsIdleWithZeroClock()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.AreEqual(0f, s.DisplayUsedSeconds, Tol);
            Assert.AreEqual(16f, s.DeadlineSeconds, Tol);
            Assert.AreEqual(16f, s.DisplayRemainingSeconds, Tol);
            Assert.IsFalse(s.Solved);
            Assert.IsFalse(s.Failed);
            Assert.IsFalse(s.CanHandOff);
            Assert.AreEqual(2f, s.SecondsPerUnit, Tol);
            Assert.AreEqual(0, s.CrossingGroupCopy().Length);
        }

        [Test]
        public void Session_CrossingDurationFollowsSlowestPlayer()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            Assert.AreEqual(PlankStartResult.Started, s.TryStartCrossing(0, 2));
            Assert.AreEqual(PlankPhase.Crossing, s.Phase);
            Assert.AreEqual(10f, s.CrossingSecondsTotal, Tol);
            Assert.AreEqual(10f, s.CrossingSecondsLeft, Tol);
            Assert.AreEqual(PlankBank.Far, s.CrossingTo);
            CollectionAssert.AreEqual(new[] { 0, 2 }, s.CrossingGroupCopy());
        }

        [Test]
        public void Session_RejectsNewCrossingWhileBusy()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            Assert.AreEqual(PlankStartResult.Busy, s.TryStartCrossing(1));
            Assert.AreEqual(5, s.State.UsedUnits);
            Assert.AreEqual(PlankPhase.Crossing, s.Phase);
        }

        [Test]
        public void Session_TickFinishesCrossingExactlyOnce()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            Assert.IsFalse(s.Tick(4f));
            Assert.AreEqual(6f, s.CrossingSecondsLeft, Tol);
            Assert.IsTrue(s.Tick(6f));
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.IsFalse(s.Tick(1f));
            CollectionAssert.AreEqual(new[] { 0, 2 }, s.LastCrossingGroupCopy());
            Assert.AreEqual(0, s.CrossingGroupCopy().Length);
        }

        [Test]
        public void Session_TickOvershootStillFinishes()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            Assert.IsTrue(s.Tick(100f));
            Assert.AreEqual(0f, s.CrossingSecondsLeft, Tol);
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
        }

        [Test]
        public void Session_TickWhileIdleDoesNothing()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            Assert.IsFalse(s.Tick(5f));
            Assert.AreEqual(0f, s.DisplayUsedSeconds, Tol);
        }

        [Test]
        public void Session_DisplayClockProgressesDuringCrossing()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            Assert.AreEqual(0f, s.DisplayUsedSeconds, Tol);
            s.Tick(5f);
            Assert.AreEqual(5f, s.DisplayUsedSeconds, Tol);
            Assert.AreEqual(11f, s.DisplayRemainingSeconds, Tol);
            s.Tick(5f);
            Assert.AreEqual(10f, s.DisplayUsedSeconds, Tol);
            s.TryStartCrossing(0);
            Assert.AreEqual(10f, s.DisplayUsedSeconds, Tol);
            s.Tick(1f);
            Assert.AreEqual(11f, s.DisplayUsedSeconds, Tol);
            s.Tick(1f);
            Assert.AreEqual(12f, s.DisplayUsedSeconds, Tol);
        }

        [Test]
        public void Session_SolvedOnlyAfterFinalArrival()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            s.Tick(10f);
            s.TryStartCrossing(0);
            s.Tick(2f);
            Assert.AreEqual(PlankStartResult.Started, s.TryStartCrossing(0, 1));
            Assert.AreEqual(PlankStatus.Won, s.State.Status);
            Assert.IsFalse(s.Solved);
            Assert.IsFalse(s.CanHandOff);
            Assert.IsFalse(s.Tick(3.9f));
            Assert.IsFalse(s.Solved);
            Assert.IsTrue(s.Tick(0.2f));
            Assert.IsTrue(s.Solved);
            Assert.IsTrue(s.CanHandOff);
            Assert.AreEqual(16f, s.DisplayUsedSeconds, Tol);
            Assert.AreEqual(0f, s.DisplayRemainingSeconds, Tol);
        }

        [Test]
        public void Session_TimedOutDoesNotStartCrossing()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(1, 2);
            s.Tick(10f);
            s.TryStartCrossing(1);
            s.Tick(4f);
            Assert.AreEqual(PlankStartResult.TimedOut, s.TryStartCrossing(0, 1));
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.IsTrue(s.Failed);
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(PlankStartResult.Finished, s.TryStartCrossing(1));
        }

        [Test]
        public void Session_MapsRuleRejections()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 25, 2f);
            Assert.AreEqual(PlankStartResult.InvalidPlayer, s.TryStartCrossing(-1));
            Assert.AreEqual(PlankStartResult.InvalidPlayer, s.TryStartCrossing(1, 1));
            s.TryStartCrossing(0, 2);
            s.Tick(10f);
            Assert.AreEqual(PlankStartResult.WrongBank, s.TryStartCrossing(0, 1));
            Assert.AreEqual(PlankStartResult.CardNotWithGroup, s.TryStartCrossing(1));
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
        }

        [Test]
        public void Session_ResetClearsCrossingAndState()
        {
            PlankSession s = NewSession(new[] { 1, 2, 5 }, 0, 2f);
            s.TryStartCrossing(0, 2);
            s.Tick(3f);
            s.Reset();
            Assert.AreEqual(PlankPhase.Idle, s.Phase);
            Assert.AreEqual(0f, s.CrossingSecondsLeft, Tol);
            Assert.AreEqual(0f, s.DisplayUsedSeconds, Tol);
            Assert.AreEqual(PlankBank.Near, s.State.BankOf(0));
            Assert.AreEqual(PlankBank.Near, s.State.CardBank);
            Assert.AreEqual(2, s.State.Attempts);
            Assert.AreEqual(0, s.CrossingGroupCopy().Length);
            Assert.AreEqual(0, s.LastCrossingGroupCopy().Length);
            Assert.AreEqual(PlankStartResult.Started, s.TryStartCrossing(0, 2));
        }

        [Test]
        public void Session_RejectsBadArguments()
        {
            var round = new PlankRound(1, new[] { 1, 2, 5 }, 25);
            Assert.Throws<ArgumentNullException>(() => new PlankSession(null, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlankSession(round, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlankSession(round, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlankSession(round, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlankSession(round, float.PositiveInfinity));
            PlankSession s = new PlankSession(round, 1f);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Tick(-0.1f));
        }

        [Test]
        public void Session_OptimalPlanPlayedInRealTimeSolvesWithinDeadline()
        {
            for (int players = 2; players <= 4; players++)
            {
                for (int seed = 1; seed <= 60; seed++)
                {
                    PlankRound round = PlankGenerator.Generate(players, seed);
                    var s = new PlankSession(round, 3f);
                    foreach (int[] m in round.OptimalPlan().Moves)
                    {
                        Assert.AreEqual(PlankStartResult.Started, s.TryStartCrossing(m[0], m.Length > 1 ? m[1] : PlankState.NoPlayer));
                        Assert.IsTrue(s.Tick(s.CrossingSecondsLeft));
                    }
                    Assert.IsTrue(s.Solved, "seed " + seed);
                    Assert.AreEqual(round.OptimalUnits * 3f, s.DisplayUsedSeconds, Tol);
                    Assert.LessOrEqual(s.DisplayUsedSeconds, s.DeadlineSeconds + Tol);
                }
            }
        }
    }
}
