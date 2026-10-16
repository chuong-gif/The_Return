/*
 * Mục đích: NUnit EditMode test cho luật «Bảng cầu dao» (T-006a): trình sinh luôn cho đề có đúng một nghiệm, quyền bấm theo chủ,
 * quá tải, giữ khớp đích đủ giờ, xem công tắc, đặt lại. Không cần scene hay Play Mode.
 * Hàm: ApplySolution (bật đúng tập nghiệm qua chủ từng công tắc); các test Generator_*, Round_*, Toggle_*, Overload_*, Tick_*, Inspect_*, Reset_*.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class BreakerPanelTests
    {
        /// <summary>Nhận trạng thái; bật mọi công tắc trong tập nghiệm, mỗi công tắc do chủ của nó bấm.</summary>
        static void ApplySolution(BreakerState state)
        {
            BreakerRound r = state.Round;
            for (int sw = 0; sw < r.Size; sw++)
                if ((r.SolutionSwitches >> sw & 1) == 1)
                    Assert.AreEqual(BreakerResult.Toggled, state.Toggle(r.OwnerOf(sw), sw));
        }

        // ---------- Trình sinh ----------

        [Test]
        public void Generator_IsDeterministicPerSeed()
        {
            BreakerRound a = BreakerGenerator.Generate(3, 777);
            BreakerRound b = BreakerGenerator.Generate(3, 777);
            Assert.AreEqual(a.StartLamps, b.StartLamps);
            Assert.AreEqual(a.TargetLamps, b.TargetLamps);
            Assert.AreEqual(a.SolutionSwitches, b.SolutionSwitches);
            for (int i = 0; i < a.Size; i++) Assert.AreEqual(a.SwitchMask(i), b.SwitchMask(i));
        }

        [Test]
        public void Generator_SizesFollowPlayerCount()
        {
            Assert.AreEqual(4, BreakerGenerator.Generate(2, 1).Size);
            Assert.AreEqual(5, BreakerGenerator.Generate(3, 1).Size);
            Assert.AreEqual(6, BreakerGenerator.Generate(4, 1).Size);
            Assert.AreEqual(2, BreakerGenerator.Generate(2, 1).OverloadLimit);
            Assert.AreEqual(3, BreakerGenerator.Generate(3, 1).OverloadLimit);
            Assert.AreEqual(3, BreakerGenerator.Generate(4, 1).OverloadLimit);
        }

        [Test]
        public void Generator_RejectsInvalidPlayerCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BreakerGenerator.Generate(1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => BreakerGenerator.Generate(5, 1));
        }

        [Test]
        public void Generator_AlwaysHasUniqueSolutionWithinLimitAndSeveralOwners()
        {
            for (int players = 2; players <= 4; players++)
            {
                for (int seed = 1; seed <= 150; seed++)
                {
                    BreakerRound r = BreakerGenerator.Generate(players, seed);
                    string tag = players + " người, seed " + seed;
                    int solutions = 0;
                    for (int s = 0; s < (1 << r.Size); s++)
                        if ((r.StartLamps ^ r.Combine(s)) == r.TargetLamps) solutions++;
                    Assert.AreEqual(1, solutions, tag + ": phải đúng một nghiệm");
                    int k = BreakerRound.CountBits(r.SolutionSwitches);
                    Assert.GreaterOrEqual(k, 2, tag);
                    Assert.LessOrEqual(k, r.OverloadLimit, tag + ": nghiệm không được gây quá tải");
                    int owners = 0;
                    for (int sw = 0; sw < r.Size; sw++)
                        if ((r.SolutionSwitches >> sw & 1) == 1) owners |= 1 << r.OwnerOf(sw);
                    Assert.GreaterOrEqual(BreakerRound.CountBits(owners), 2, tag + ": cần công tắc của ít nhất hai người");
                    Assert.AreNotEqual(r.StartLamps, r.TargetLamps, tag);
                    for (int sw = 0; sw < r.Size; sw++)
                    {
                        int bits = BreakerRound.CountBits(r.SwitchMask(sw));
                        Assert.IsTrue(bits == 2 || bits == 3, tag + ": mỗi công tắc đảo 2-3 đèn");
                    }
                }
            }
        }

        [Test]
        public void Round_RejectsInconsistentSolutionOrSizes()
        {
            BreakerRound ok = BreakerGenerator.Generate(2, 5);
            var masks = new[] { ok.SwitchMask(0), ok.SwitchMask(1), ok.SwitchMask(2), ok.SwitchMask(3) };
            Assert.Throws<ArgumentException>(() => new BreakerRound(5, 2, masks, ok.StartLamps, ok.TargetLamps, ok.SolutionSwitches ^ 1));
            Assert.Throws<ArgumentException>(() => new BreakerRound(5, 2, new[] { 1, 2, 3 }, 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BreakerRound(5, 1, masks, 0, 0, 0));
        }

        // ---------- Bấm công tắc ----------

        [Test]
        public void Toggle_RejectsNonOwnerAndInvalidIndices()
        {
            var s = new BreakerState(BreakerGenerator.Generate(3, 11));
            Assert.AreEqual(BreakerResult.NotOwner, s.Toggle(1, 0), "Công tắc 0 thuộc người 0");
            Assert.AreEqual(BreakerResult.InvalidSwitch, s.Toggle(0, 99));
            Assert.AreEqual(BreakerResult.InvalidSwitch, s.Toggle(0, -1));
            Assert.AreEqual(BreakerResult.InvalidSwitch, s.Toggle(9, 0));
            Assert.AreEqual(0, s.OnSwitches);
        }

        [Test]
        public void Toggle_OwnerChangesLampsAndSecondPressTurnsOff()
        {
            BreakerRound r = BreakerGenerator.Generate(3, 11);
            var s = new BreakerState(r);
            int before = s.Lamps;
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(0, 0));
            Assert.IsTrue(s.SwitchOn(0));
            Assert.AreEqual(before ^ r.SwitchMask(0), s.Lamps);
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(0, 0));
            Assert.IsFalse(s.SwitchOn(0));
            Assert.AreEqual(before, s.Lamps);
        }

        [Test]
        public void Overload_TripsAndResetsSwitches()
        {
            // 2 người: 4 công tắc, giới hạn 2. Công tắc 0,2 của người 0; 1,3 của người 1.
            var s = new BreakerState(BreakerGenerator.Generate(2, 3));
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(0, 0));
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(1, 1));
            Assert.AreEqual(BreakerResult.Tripped, s.Toggle(0, 2), "Bật công tắc thứ 3 vượt giới hạn 2");
            Assert.AreEqual(0, s.OnSwitches);
            Assert.AreEqual(1, s.Trips);
            Assert.AreEqual(s.Round.StartLamps, s.Lamps, "Đèn về trạng thái đầu");
        }

        [Test]
        public void Overload_TurningOffNeverTrips()
        {
            var s = new BreakerState(BreakerGenerator.Generate(2, 3));
            s.Toggle(0, 0);
            s.Toggle(1, 1);
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(0, 0));
            Assert.AreEqual(BreakerResult.Toggled, s.Toggle(1, 1));
            Assert.AreEqual(0, s.Trips);
        }

        // ---------- Giữ khớp đích ----------

        [Test]
        public void Tick_SolvesOnlyAfterHoldingMatchForHoldSeconds()
        {
            var s = new BreakerState(BreakerGenerator.Generate(4, 21));
            Assert.IsFalse(s.Matches);
            Assert.IsFalse(s.Tick(5));
            ApplySolution(s);
            Assert.IsTrue(s.Matches);
            Assert.IsFalse(s.Tick(10));
            Assert.AreEqual(0.0, s.HoldProgress(10), 1e-9);
            Assert.IsFalse(s.Tick(12.9));
            Assert.AreEqual(2.9 / 3.0, s.HoldProgress(12.9), 1e-9);
            Assert.IsTrue(s.Tick(13.0));
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(1.0, s.HoldProgress(99), 1e-9);
        }

        [Test]
        public void Tick_PressingAnySwitchRestartsTheHold()
        {
            var s = new BreakerState(BreakerGenerator.Generate(3, 5));
            ApplySolution(s);
            s.Tick(0);
            int sw = 0;
            while ((s.Round.SolutionSwitches >> sw & 1) == 0) sw++;
            int owner = s.Round.OwnerOf(sw);
            s.Toggle(owner, sw);
            s.Toggle(owner, sw);
            Assert.IsTrue(s.Matches);
            Assert.IsFalse(s.Tick(2.0), "Hold bắt đầu lại từ lần bấm");
            Assert.IsFalse(s.Tick(4.9));
            Assert.IsTrue(s.Tick(5.0));
        }

        [Test]
        public void Tick_LeavingMatchLosesProgress()
        {
            var s = new BreakerState(BreakerGenerator.Generate(3, 5));
            ApplySolution(s);
            s.Tick(0);
            int sw = 0;
            while ((s.Round.SolutionSwitches >> sw & 1) == 0) sw++;
            s.Toggle(s.Round.OwnerOf(sw), sw);
            Assert.IsFalse(s.Matches);
            Assert.IsFalse(s.Tick(1));
            Assert.AreEqual(0.0, s.HoldProgress(1), 1e-9);
        }

        [Test]
        public void HoldSeconds_ZeroSolvesOnFirstMatchingTick_AndNegativeIsRejected()
        {
            var s = new BreakerState(BreakerGenerator.Generate(2, 8), 0);
            ApplySolution(s);
            Assert.IsTrue(s.Tick(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BreakerState(BreakerGenerator.Generate(2, 8), -1));
            Assert.Throws<ArgumentNullException>(() => new BreakerState(null));
        }

        [Test]
        public void Toggle_AfterSolvedIsRejected()
        {
            var s = new BreakerState(BreakerGenerator.Generate(2, 8), 0);
            ApplySolution(s);
            s.Tick(0);
            Assert.AreEqual(BreakerResult.AlreadySolved, s.Toggle(0, 0));
        }

        // ---------- Xem công tắc / đặt lại ----------

        [Test]
        public void Inspect_OnlyOwnerSeesWhichLampsASwitchFlips()
        {
            BreakerRound r = BreakerGenerator.Generate(3, 11);
            var s = new BreakerState(r);
            int mask;
            Assert.IsTrue(s.Inspect(0, 0, out mask));
            Assert.AreEqual(r.SwitchMask(0), mask);
            Assert.IsFalse(s.Inspect(1, 0, out mask));
            Assert.AreEqual(0, mask);
            Assert.IsFalse(s.Inspect(0, 99, out mask));
        }

        [Test]
        public void Reset_ClearsSwitchesAndSolved_KeepsRoundAndTrips()
        {
            var s = new BreakerState(BreakerGenerator.Generate(2, 8), 0);
            s.Toggle(0, 0);
            s.Toggle(1, 1);
            s.Toggle(0, 2);
            Assert.AreEqual(1, s.Trips);
            ApplySolution(s);
            s.Tick(0);
            Assert.IsTrue(s.Solved);
            s.Reset();
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(0, s.OnSwitches);
            Assert.AreEqual(1, s.Trips);
            Assert.AreEqual(s.Round.StartLamps, s.Lamps);
        }
    }
}
