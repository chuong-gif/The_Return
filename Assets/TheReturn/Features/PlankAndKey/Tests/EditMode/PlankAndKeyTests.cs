/*
 * Mục đích: NUnit EditMode test cho luật «Cầu ván và thẻ từ» (T-006b): bộ giải khớp số liệu thiết kế đã duyệt và một bộ kiểm vét cạn
 * độc lập, trình sinh đề tất định, luật qua cầu (bờ, thẻ, hạn, thắng/thất bại), phát lại kế hoạch tối ưu. Không cần scene hay Play Mode.
 * Hàm: Permutations, BruteForce, Explore, Replay, NewState (trợ giúp); các test Solver_*, Round_*, Generator_*, State_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class PlankAndKeyTests
    {
        /// <summary>Nhận mảng; trả mọi hoán vị của nó.</summary>
        static List<int[]> Permutations(int[] items)
        {
            var result = new List<int[]>();
            Permute((int[])items.Clone(), 0, result);
            return result;
        }

        static void Permute(int[] a, int k, List<int[]> result)
        {
            if (k == a.Length) { result.Add((int[])a.Clone()); return; }
            for (int i = k; i < a.Length; i++)
            {
                int t = a[k]; a[k] = a[i]; a[i] = t;
                Permute(a, k + 1, result);
                t = a[k]; a[k] = a[i]; a[i] = t;
            }
        }

        /// <summary>Nhận thời gian qua cầu; trả thời gian tối thiểu bằng duyệt vét cạn có nhớ (độc lập với PlankSolver).</summary>
        static int BruteForce(int[] u)
        {
            int n = u.Length;
            var best = new int[(1 << n) * 2];
            for (int i = 0; i < best.Length; i++) best[i] = int.MaxValue;
            int result = int.MaxValue;
            Explore(u, n, 0, 0, 0, best, ref result);
            return result;
        }

        static void Explore(int[] u, int n, int far, int cardFar, int time, int[] best, ref int result)
        {
            int id = far * 2 + cardFar;
            if (time >= best[id]) return;
            best[id] = time;
            int full = (1 << n) - 1;
            if (far == full && cardFar == 1)
            {
                if (time < result) result = time;
                return;
            }
            for (int a = 0; a < n; a++)
            {
                if (((far >> a & 1) == 1) != (cardFar == 1)) continue;
                for (int b = a; b < n; b++)
                {
                    if (((far >> b & 1) == 1) != (cardFar == 1)) continue;
                    int cost = Math.Max(u[a], u[b]);
                    int group = (1 << a) | (1 << b);
                    int nextFar = cardFar == 1 ? far & ~group : far | group;
                    Explore(u, n, nextFar, 1 - cardFar, time + cost, best, ref result);
                }
            }
        }

        /// <summary>Nhận đề; phát lại kế hoạch tối ưu qua PlankState (mọi lượt phải hợp lệ, lượt cuối thắng); trả trạng thái cuối.</summary>
        static PlankState Replay(PlankRound round)
        {
            var state = new PlankState(round);
            IList<int[]> moves = round.OptimalPlan().Moves;
            for (int i = 0; i < moves.Count; i++)
            {
                int[] m = moves[i];
                PlankResult r = state.TryCross(m[0], m.Length > 1 ? m[1] : PlankState.NoPlayer);
                Assert.AreEqual(i == moves.Count - 1 ? PlankResult.Won : PlankResult.Crossed, r, "lượt " + i);
            }
            return state;
        }

        /// <summary>Nhận thời gian qua cầu và phần dư %; trả trạng thái mới trên đề tương ứng.</summary>
        static PlankState NewState(int[] units, int slack)
        {
            return new PlankState(new PlankRound(1, units, slack));
        }

        // ---------- Bộ giải ----------

        [Test]
        public void Solver_MatchesApprovedDesignNumbers()
        {
            foreach (int[] p in Permutations(new[] { 1, 2, 5 })) Assert.AreEqual(8, PlankSolver.Solve(p).TotalUnits);
            foreach (int[] p in Permutations(new[] { 1, 2, 5, 10 })) Assert.AreEqual(17, PlankSolver.Solve(p).TotalUnits);
            foreach (int[] p in Permutations(new[] { 2, 3, 4, 6 })) Assert.AreEqual(17, PlankSolver.Solve(p).TotalUnits);
            foreach (int[] p in Permutations(new[] { 1, 2, 3, 4 })) Assert.AreEqual(11, PlankSolver.Solve(p).TotalUnits);
        }

        [Test]
        public void Solver_TwoPlayersCrossTogetherInSlowerTime()
        {
            PlankPlan plan = PlankSolver.Solve(new[] { 4, 3 });
            Assert.AreEqual(4, plan.TotalUnits);
            Assert.AreEqual(1, plan.Moves.Count);
            CollectionAssert.AreEqual(new[] { 0, 1 }, plan.Moves[0]);
            Assert.AreEqual(5, PlankSolver.Solve(new[] { 2, 5 }).TotalUnits);
        }

        [Test]
        public void Solver_MatchesIndependentBruteForceOnWholeCatalog()
        {
            for (int players = PlankRound.MinPlayers; players <= PlankRound.MaxPlayers; players++)
            {
                for (int i = 0; i < PlankGenerator.CatalogSize(players); i++)
                {
                    foreach (int[] p in Permutations(PlankGenerator.CatalogSet(players, i)))
                        Assert.AreEqual(BruteForce(p), PlankSolver.Solve(p).TotalUnits, string.Join(",", p));
                }
            }
        }

        [Test]
        public void Solver_PlanHasOddMovesAndSmallGroups()
        {
            for (int players = PlankRound.MinPlayers; players <= PlankRound.MaxPlayers; players++)
            {
                for (int i = 0; i < PlankGenerator.CatalogSize(players); i++)
                {
                    PlankPlan plan = PlankSolver.Solve(PlankGenerator.CatalogSet(players, i));
                    Assert.AreEqual(1, plan.Moves.Count % 2);
                    foreach (int[] m in plan.Moves)
                    {
                        Assert.IsTrue(m.Length == 1 || m.Length == 2);
                        foreach (int who in m) Assert.IsTrue(who >= 0 && who < players);
                    }
                }
            }
        }

        [Test]
        public void Solver_RejectsInvalidInput()
        {
            Assert.Throws<ArgumentNullException>(() => PlankSolver.Solve(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankSolver.Solve(new[] { 3 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankSolver.Solve(new[] { 1, 2, 3, 4, 5 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankSolver.Solve(new[] { 1, 0, 3 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankSolver.Solve(new[] { 1, -2 }));
        }

        // ---------- Đề ----------

        [Test]
        public void Round_DeadlineIsOptimalPlusCeilingOfSlack()
        {
            Assert.AreEqual(10, new PlankRound(1, new[] { 1, 2, 5 }, 25).DeadlineUnits);
            Assert.AreEqual(22, new PlankRound(1, new[] { 1, 2, 5, 10 }, 25).DeadlineUnits);
            Assert.AreEqual(21, new PlankRound(1, new[] { 1, 2, 5, 10 }, 20).DeadlineUnits);
            Assert.AreEqual(8, new PlankRound(1, new[] { 1, 2, 5 }, 0).DeadlineUnits);
            Assert.AreEqual(16, new PlankRound(1, new[] { 1, 2, 5 }, 100).DeadlineUnits);
        }

        [Test]
        public void Round_RejectsBadArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlankRound(1, new[] { 1, 2, 5 }, -1));
            Assert.Throws<ArgumentNullException>(() => new PlankRound(1, null, 25));
            var r = new PlankRound(1, new[] { 1, 2, 5 }, 25);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.CrossUnits(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => r.CrossUnits(3));
        }

        [Test]
        public void Round_CopiesInputArray()
        {
            int[] src = { 1, 2, 5 };
            var r = new PlankRound(1, src, 25);
            src[0] = 99;
            Assert.AreEqual(1, r.CrossUnits(0));
            int[] copy = r.CrossUnitsCopy();
            copy[1] = 77;
            Assert.AreEqual(2, r.CrossUnits(1));
            Assert.AreEqual(3, r.PlayerCount);
        }

        // ---------- Trình sinh ----------

        [Test]
        public void Generator_IsDeterministicPerSeed()
        {
            for (int players = 2; players <= 4; players++)
            {
                PlankRound a = PlankGenerator.Generate(players, 4242);
                PlankRound b = PlankGenerator.Generate(players, 4242);
                CollectionAssert.AreEqual(a.CrossUnitsCopy(), b.CrossUnitsCopy());
                Assert.AreEqual(a.DeadlineUnits, b.DeadlineUnits);
                Assert.AreEqual(players, a.PlayerCount);
            }
        }

        [Test]
        public void Generator_UsesCatalogSetsAndShufflesAssignment()
        {
            for (int players = 2; players <= 4; players++)
            {
                var distinct = new HashSet<string>();
                for (int seed = 1; seed <= 150; seed++)
                {
                    int[] u = PlankGenerator.Generate(players, seed).CrossUnitsCopy();
                    distinct.Add(string.Join(",", u));
                    int[] sorted = (int[])u.Clone();
                    Array.Sort(sorted);
                    bool inCatalog = false;
                    for (int i = 0; i < PlankGenerator.CatalogSize(players); i++)
                    {
                        int[] set = PlankGenerator.CatalogSet(players, i);
                        Array.Sort(set);
                        if (string.Join(",", set) == string.Join(",", sorted)) inCatalog = true;
                    }
                    Assert.IsTrue(inCatalog, "seed " + seed);
                }
                Assert.GreaterOrEqual(distinct.Count, players == 2 ? 4 : 6, "số cách gán khác nhau với " + players + " người");
            }
        }

        [Test]
        public void Generator_DeadlineNeverBelowOptimalAndAlwaysWinnable()
        {
            for (int players = 2; players <= 4; players++)
            {
                for (int seed = 1; seed <= 150; seed++)
                {
                    PlankRound r = PlankGenerator.Generate(players, seed);
                    Assert.GreaterOrEqual(r.DeadlineUnits, r.OptimalUnits);
                    PlankState s = Replay(r);
                    Assert.AreEqual(PlankStatus.Won, s.Status);
                    Assert.AreEqual(r.OptimalUnits, s.UsedUnits);
                    Assert.LessOrEqual(s.UsedUnits, r.DeadlineUnits);
                }
            }
        }

        [Test]
        public void Generator_SlackIsAParameter()
        {
            PlankRound tight = PlankGenerator.Generate(3, 5, 0);
            Assert.AreEqual(tight.OptimalUnits, tight.DeadlineUnits);
            PlankRound loose = PlankGenerator.Generate(3, 5, 50);
            Assert.AreEqual(loose.OptimalUnits + (loose.OptimalUnits * 50 + 99) / 100, loose.DeadlineUnits);
            Assert.AreEqual(25, PlankGenerator.Generate(3, 5).SlackPercent);
        }

        [Test]
        public void Generator_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankGenerator.Generate(1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankGenerator.Generate(5, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankGenerator.Generate(3, 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankGenerator.CatalogSize(6));
            Assert.Throws<ArgumentOutOfRangeException>(() => PlankGenerator.CatalogSet(3, 3));
        }

        [Test]
        public void Generator_CatalogSetsAreCopies()
        {
            int[] a = PlankGenerator.CatalogSet(4, 0);
            a[0] = 1000;
            Assert.AreNotEqual(1000, PlankGenerator.CatalogSet(4, 0)[0]);
            for (int players = 2; players <= 4; players++) Assert.AreEqual(3, PlankGenerator.CatalogSize(players));
        }

        // ---------- Luật qua cầu ----------

        [Test]
        public void State_StartsAllNearWithCardNearAndPlaying()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            for (int i = 0; i < 3; i++) Assert.AreEqual(PlankBank.Near, s.BankOf(i));
            Assert.AreEqual(PlankBank.Near, s.CardBank);
            Assert.AreEqual(PlankStatus.Playing, s.Status);
            Assert.AreEqual(0, s.UsedUnits);
            Assert.AreEqual(10, s.RemainingUnits);
            Assert.AreEqual(1, s.Attempts);
            Assert.Throws<ArgumentNullException>(() => new PlankState(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.BankOf(3));
        }

        [Test]
        public void State_CrossCostsSlowerPlayerAndCarriesCard()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(0, 2));
            Assert.AreEqual(5, s.UsedUnits);
            Assert.AreEqual(PlankBank.Far, s.BankOf(0));
            Assert.AreEqual(PlankBank.Far, s.BankOf(2));
            Assert.AreEqual(PlankBank.Near, s.BankOf(1));
            Assert.AreEqual(PlankBank.Far, s.CardBank);
            Assert.AreEqual(5, s.RemainingUnits);
        }

        [Test]
        public void State_RejectsInvalidPlayersWithoutChangingState()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            Assert.AreEqual(PlankResult.InvalidPlayer, s.TryCross(-1));
            Assert.AreEqual(PlankResult.InvalidPlayer, s.TryCross(3));
            Assert.AreEqual(PlankResult.InvalidPlayer, s.TryCross(1, 1));
            Assert.AreEqual(PlankResult.InvalidPlayer, s.TryCross(0, 9));
            Assert.AreEqual(PlankResult.InvalidPlayer, s.TryCross(0, -5));
            Assert.AreEqual(0, s.UsedUnits);
            Assert.AreEqual(PlankStatus.Playing, s.Status);
            for (int i = 0; i < 3; i++) Assert.AreEqual(PlankBank.Near, s.BankOf(i));
        }

        [Test]
        public void State_RejectsGroupAcrossBanks()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            s.TryCross(0, 2);
            Assert.AreEqual(PlankResult.WrongBank, s.TryCross(0, 1));
            Assert.AreEqual(5, s.UsedUnits);
        }

        [Test]
        public void State_RejectsCrossingWithoutCard()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            s.TryCross(0, 2);
            Assert.AreEqual(PlankResult.CardNotWithGroup, s.TryCross(1));
            Assert.AreEqual(PlankBank.Near, s.BankOf(1));
            Assert.AreEqual(5, s.UsedUnits);
        }

        [Test]
        public void State_ReturnTripBringsCardBack()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 25);
            s.TryCross(0, 2);
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(0));
            Assert.AreEqual(PlankBank.Near, s.CardBank);
            Assert.AreEqual(PlankBank.Near, s.BankOf(0));
            Assert.AreEqual(6, s.UsedUnits);
        }

        [Test]
        public void State_WinsOnLastCrossingExactlyAtDeadline()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 0);
            Assert.AreEqual(8, s.Round.DeadlineUnits);
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(0, 2));
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(0));
            Assert.AreEqual(PlankResult.Won, s.TryCross(0, 1));
            Assert.AreEqual(PlankStatus.Won, s.Status);
            Assert.AreEqual(8, s.UsedUnits);
            Assert.AreEqual(0, s.RemainingUnits);
        }

        [Test]
        public void State_OverDeadlineFailsAndMoveIsNotApplied()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 0);
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(1, 2));
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(1));
            Assert.AreEqual(7, s.UsedUnits);
            Assert.AreEqual(PlankResult.TimedOut, s.TryCross(0, 1));
            Assert.AreEqual(PlankStatus.Failed, s.Status);
            Assert.AreEqual(7, s.UsedUnits);
            Assert.AreEqual(PlankBank.Near, s.BankOf(0));
            Assert.AreEqual(PlankBank.Near, s.BankOf(1));
            Assert.AreEqual(PlankBank.Near, s.CardBank);
        }

        [Test]
        public void State_FinishedStateRejectsMovesUntilReset()
        {
            PlankState won = NewState(new[] { 2, 5 }, 25);
            Assert.AreEqual(PlankResult.Won, won.TryCross(0, 1));
            Assert.AreEqual(PlankResult.Finished, won.TryCross(0));

            PlankState failed = NewState(new[] { 1, 2, 5 }, 0);
            failed.TryCross(1, 2);
            failed.TryCross(1);
            failed.TryCross(0, 1);
            Assert.AreEqual(PlankStatus.Failed, failed.Status);
            Assert.AreEqual(PlankResult.Finished, failed.TryCross(1));
        }

        [Test]
        public void State_ResetRestartsSameRound()
        {
            PlankState s = NewState(new[] { 1, 2, 5 }, 0);
            s.TryCross(1, 2);
            s.TryCross(1);
            s.TryCross(0, 1);
            Assert.AreEqual(PlankStatus.Failed, s.Status);
            PlankRound round = s.Round;
            s.Reset();
            Assert.AreSame(round, s.Round);
            Assert.AreEqual(PlankStatus.Playing, s.Status);
            Assert.AreEqual(0, s.UsedUnits);
            Assert.AreEqual(PlankBank.Near, s.CardBank);
            for (int i = 0; i < 3; i++) Assert.AreEqual(PlankBank.Near, s.BankOf(i));
            Assert.AreEqual(2, s.Attempts);
            Assert.AreEqual(PlankResult.Crossed, s.TryCross(0, 2));
        }

        [Test]
        public void State_TwoPlayersLightVariantWinsInOneCrossing()
        {
            PlankState s = NewState(new[] { 2, 5 }, PlankGenerator.DefaultSlackPercent);
            Assert.AreEqual(7, s.Round.DeadlineUnits);
            Assert.AreEqual(PlankResult.Won, s.TryCross(1, 0));
            Assert.AreEqual(5, s.UsedUnits);
        }
    }
}
