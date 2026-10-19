/*
 * Mục đích: Test NUnit EditMode cho Domain «Cầu kính» (T-006e): trình sinh đề theo seed, luật bước/té/vỡ ô, quyền xem sơ đồ,
 * lùi và xuống đài, điều kiện thắng, Reset, và kịch bản hai người dẫn nhau.
 * Hàm: Cross (phụ trợ); các test Generator_*, Round_*, Step_*, Disembark_*, Diagram_*, Retreat_*, Reset_*, Solved_*, TwoPlayers_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class GlassBridgeTests
    {
        /// <summary>Nhận trạng thái và người; đi đúng hết cầu theo đề rồi xuống đài cuối.</summary>
        static void Cross(GlassBridgeState s, int player)
        {
            GlassBridgeRound r = s.Round;
            for (int row = 0; row < r.Rows; row++)
                Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(player, r.SafeTile(row)), "hàng " + row);
            Assert.AreEqual(GlassBridgeResult.ReachedEnd, s.Disembark(player));
        }

        /// <summary>Nhận đề; trả ô SAI của hàng đó (ô còn lại ngoài ô an toàn).</summary>
        static int WrongTile(GlassBridgeRound r, int row) { return 1 - r.SafeTile(row); }

        static GlassBridgeState NewState(int players = 3, int seed = 7) { return new GlassBridgeState(GlassBridgeGenerator.Generate(players, seed)); }

        // ---------- Generator / Round ----------

        [Test]
        public void Generator_SameSeedGivesSameBridge()
        {
            for (int seed = 0; seed < 50; seed++)
                CollectionAssert.AreEqual(GlassBridgeGenerator.Generate(3, seed).CopySafeTiles(), GlassBridgeGenerator.Generate(3, seed).CopySafeTiles());
        }

        [Test]
        public void Generator_ProducesValidBridgesForEveryPlayerCount()
        {
            for (int players = 2; players <= 4; players++)
                for (int seed = 1; seed <= 200; seed++)
                {
                    GlassBridgeRound r = GlassBridgeGenerator.Generate(players, seed);
                    Assert.AreEqual(GlassBridgeRound.DefaultRows, r.Rows);
                    Assert.AreEqual(players, r.PlayerCount);
                    Assert.AreEqual(seed, r.Seed);
                    Assert.IsTrue(GlassBridgeGenerator.IsAcceptable(r.CopySafeTiles()), "seed " + seed);
                    for (int row = 0; row < r.Rows; row++) Assert.That(r.SafeTile(row), Is.EqualTo(0).Or.EqualTo(1));
                }
        }

        [Test]
        public void Generator_BothSidesAppearAndNoLongRuns()
        {
            for (int seed = 1; seed <= 300; seed++)
            {
                int[] t = GlassBridgeGenerator.Generate(2, seed, 10).CopySafeTiles();
                Assert.Contains(0, t);
                Assert.Contains(1, t);
                int run = 1;
                for (int i = 1; i < t.Length; i++)
                {
                    run = t[i] == t[i - 1] ? run + 1 : 1;
                    Assert.LessOrEqual(run, 3, "seed " + seed);
                }
            }
        }

        [Test]
        public void Generator_DifferentSeedsGiveVariety()
        {
            var seen = new HashSet<string>();
            for (int seed = 1; seed <= 100; seed++) seen.Add(string.Join(",", GlassBridgeGenerator.Generate(3, seed).CopySafeTiles()));
            Assert.GreaterOrEqual(seen.Count, 10);
        }

        [Test]
        public void Generator_AcceptsRowLimitsAndRejectsOutside()
        {
            Assert.AreEqual(GlassBridgeRound.MinRows, GlassBridgeGenerator.Generate(2, 1, GlassBridgeRound.MinRows).Rows);
            Assert.AreEqual(GlassBridgeRound.MaxRows, GlassBridgeGenerator.Generate(2, 1, GlassBridgeRound.MaxRows).Rows);
            Assert.Throws<ArgumentOutOfRangeException>(() => GlassBridgeGenerator.Generate(2, 1, GlassBridgeRound.MinRows - 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => GlassBridgeGenerator.Generate(2, 1, GlassBridgeRound.MaxRows + 1));
        }

        [Test]
        public void Generator_RejectsInvalidPlayerCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GlassBridgeGenerator.Generate(1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => GlassBridgeGenerator.Generate(5, 1));
        }

        [Test]
        public void IsAcceptable_RejectsTrivialAndLongRuns()
        {
            Assert.IsFalse(GlassBridgeGenerator.IsAcceptable(null));
            Assert.IsFalse(GlassBridgeGenerator.IsAcceptable(new int[0]));
            Assert.IsFalse(GlassBridgeGenerator.IsAcceptable(new[] { 0, 0, 0, 0, 0, 0 }));
            Assert.IsFalse(GlassBridgeGenerator.IsAcceptable(new[] { 0, 0, 0, 0, 1, 0 }));
            Assert.IsTrue(GlassBridgeGenerator.IsAcceptable(new[] { 0, 0, 0, 1, 1, 0 }));
        }

        [Test]
        public void Round_RejectsBadInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GlassBridgeRound(1, 1, new[] { 0, 1, 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GlassBridgeRound(1, 5, new[] { 0, 1, 0 }));
            Assert.Throws<ArgumentException>(() => new GlassBridgeRound(1, 2, null));
            Assert.Throws<ArgumentException>(() => new GlassBridgeRound(1, 2, new[] { 0, 1 }));
            Assert.Throws<ArgumentException>(() => new GlassBridgeRound(1, 2, new[] { 0, 1, 2 }));
            Assert.Throws<ArgumentException>(() => new GlassBridgeRound(1, 2, new int[GlassBridgeRound.MaxRows + 1]));
        }

        [Test]
        public void Round_ProtectsItsTilesFromOutsideEdits()
        {
            var source = new[] { 0, 1, 0, 1 };
            var r = new GlassBridgeRound(1, 2, source);
            source[0] = 1;
            Assert.AreEqual(0, r.SafeTile(0));
            r.CopySafeTiles()[1] = 0;
            Assert.AreEqual(1, r.SafeTile(1));
            Assert.AreEqual(-1, r.SafeTile(-1));
            Assert.AreEqual(-1, r.SafeTile(4));
        }

        // ---------- Step ----------

        [Test]
        public void NewState_EveryoneAtStartCanSeeDiagram()
        {
            GlassBridgeState s = NewState(4);
            for (int p = 0; p < 4; p++)
            {
                Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(p));
                Assert.IsFalse(s.OnBridge(p));
                Assert.IsTrue(s.CanSeeDiagram(p));
            }
            Assert.AreEqual(0, s.Falls);
            Assert.AreEqual(0, s.AcrossCount);
            Assert.IsFalse(s.Solved);
        }

        [Test]
        public void Step_CorrectTileAdvancesOneRow()
        {
            GlassBridgeState s = NewState();
            Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(0, s.Round.SafeTile(0)));
            Assert.AreEqual(0, s.RowOf(0));
            Assert.IsTrue(s.OnBridge(0));
            Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(0, s.Round.SafeTile(1)));
            Assert.AreEqual(1, s.RowOf(0));
            Assert.AreEqual(0, s.Falls);
        }

        [Test]
        public void Step_WrongTileBreaksItAndSendsOnlyThatPlayerBack()
        {
            GlassBridgeState s = NewState(3);
            s.Step(0, s.Round.SafeTile(0));
            s.Step(0, s.Round.SafeTile(1));
            Assert.AreEqual(GlassBridgeResult.Fell, s.Step(1, WrongTile(s.Round, 0)));
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(1));
            Assert.IsTrue(s.IsBroken(0, WrongTile(s.Round, 0)));
            Assert.IsFalse(s.IsBroken(0, s.Round.SafeTile(0)));
            Assert.AreEqual(1, s.RowOf(0), "người khác giữ nguyên chỗ");
            Assert.AreEqual(1, s.Falls);
            Assert.AreEqual(1, s.FallsOf(1));
            Assert.AreEqual(0, s.FallsOf(0));
        }

        [Test]
        public void Step_BrokenTileCannotBeSteppedAgainButOtherTileWorks()
        {
            GlassBridgeState s = NewState(2);
            int wrong = WrongTile(s.Round, 0);
            s.Step(0, wrong);
            Assert.AreEqual(GlassBridgeResult.TileBroken, s.Step(1, wrong));
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(1), "ô vỡ không đổi vị trí");
            Assert.AreEqual(1, s.Falls, "bước vào ô vỡ không tính té");
            Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(1, s.Round.SafeTile(0)));
        }

        [Test]
        public void Step_CorrectTilesLeaveNoTrace()
        {
            GlassBridgeState s = NewState(2);
            s.Step(0, s.Round.SafeTile(0));
            for (int tile = 0; tile < 2; tile++) Assert.IsFalse(s.IsBroken(0, tile));
        }

        [Test]
        public void Step_InvalidInputsDoNothing()
        {
            GlassBridgeState s = NewState(2);
            Assert.AreEqual(GlassBridgeResult.InvalidPlayer, s.Step(-1, 0));
            Assert.AreEqual(GlassBridgeResult.InvalidPlayer, s.Step(2, 0));
            Assert.AreEqual(GlassBridgeResult.InvalidTile, s.Step(0, 2));
            Assert.AreEqual(GlassBridgeResult.InvalidTile, s.Step(0, -1));
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(0));
            Assert.AreEqual(0, s.Falls);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.RowOf(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.FallsOf(-1));
        }

        [Test]
        public void Step_FromLastRowRequiresDisembark()
        {
            GlassBridgeState s = NewState(2);
            int last = s.Round.Rows - 1;
            for (int row = 0; row <= last; row++) s.Step(0, s.Round.SafeTile(row));
            Assert.AreEqual(last, s.RowOf(0));
            Assert.AreEqual(GlassBridgeResult.UseDisembark, s.Step(0, 0));
            Assert.AreEqual(last, s.RowOf(0));
        }

        // ---------- Disembark ----------

        [Test]
        public void Disembark_OnlyFromLastRow()
        {
            GlassBridgeState s = NewState(2);
            Assert.AreEqual(GlassBridgeResult.NotAtLastRow, s.Disembark(0), "đang ở đài đầu");
            s.Step(0, s.Round.SafeTile(0));
            Assert.AreEqual(GlassBridgeResult.NotAtLastRow, s.Disembark(0), "mới hàng 0");
            Assert.AreEqual(GlassBridgeResult.InvalidPlayer, s.Disembark(5));
        }

        [Test]
        public void Disembark_PlayerAtEndCannotMoveAgain()
        {
            GlassBridgeState s = NewState(3);
            Cross(s, 0);
            Assert.AreEqual(s.EndPlatform, s.RowOf(0));
            Assert.AreEqual(GlassBridgeResult.AlreadyAcross, s.Step(0, 0));
            Assert.AreEqual(GlassBridgeResult.AlreadyAcross, s.Disembark(0));
            Assert.IsFalse(s.Retreat(0));
            Assert.AreEqual(1, s.AcrossCount);
        }

        [Test]
        public void Solved_OnlyWhenEveryoneIsAcross()
        {
            GlassBridgeState s = NewState(3);
            Cross(s, 0);
            Cross(s, 1);
            Assert.IsFalse(s.Solved);
            Cross(s, 2);
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(3, s.AcrossCount);
            Assert.AreEqual(GlassBridgeResult.AlreadySolved, s.Step(0, 0));
            Assert.AreEqual(GlassBridgeResult.AlreadySolved, s.Disembark(1));
            Assert.IsFalse(s.Retreat(2));
        }

        // ---------- Diagram ----------

        [Test]
        public void Diagram_VisibleOnPlatformsHiddenOnBridge()
        {
            GlassBridgeState s = NewState(2);
            int[] seen;
            Assert.IsTrue(s.TryGetDiagram(0, out seen));
            CollectionAssert.AreEqual(s.Round.CopySafeTiles(), seen);
            s.Step(0, s.Round.SafeTile(0));
            Assert.IsFalse(s.CanSeeDiagram(0));
            Assert.IsFalse(s.TryGetDiagram(0, out seen));
            Assert.IsNull(seen);
            Assert.IsTrue(s.CanSeeDiagram(1), "người ở đài vẫn xem được");
            Cross(s, 1);
            Assert.IsTrue(s.TryGetDiagram(1, out seen), "người đã qua xem được bảng đài cuối");
        }

        [Test]
        public void Diagram_FallenPlayerSeesItAgainAtStart()
        {
            GlassBridgeState s = NewState(2);
            s.Step(0, s.Round.SafeTile(0));
            s.Step(0, s.Round.SafeTile(1));
            Assert.IsFalse(s.CanSeeDiagram(0));
            s.Step(0, WrongTile(s.Round, 2));
            Assert.IsTrue(s.CanSeeDiagram(0));
        }

        [Test]
        public void Diagram_ReturnedArrayIsACopy()
        {
            GlassBridgeState s = NewState(2);
            int[] seen;
            s.TryGetDiagram(0, out seen);
            int original = s.Round.SafeTile(0);
            seen[0] = 1 - original;
            Assert.AreEqual(original, s.Round.SafeTile(0));
        }

        // ---------- Retreat ----------

        [Test]
        public void Retreat_StepsBackOneRowThenToStartWithoutFalling()
        {
            GlassBridgeState s = NewState(2);
            s.Step(0, s.Round.SafeTile(0));
            s.Step(0, s.Round.SafeTile(1));
            Assert.IsTrue(s.Retreat(0));
            Assert.AreEqual(0, s.RowOf(0));
            Assert.IsTrue(s.Retreat(0));
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(0));
            Assert.IsFalse(s.Retreat(0), "đã ở đài đầu");
            Assert.AreEqual(0, s.Falls);
        }

        [Test]
        public void Retreat_InvalidPlayerReturnsFalse()
        {
            GlassBridgeState s = NewState(2);
            Assert.IsFalse(s.Retreat(-1));
            Assert.IsFalse(s.Retreat(2));
        }

        // ---------- Reset ----------

        [Test]
        public void Reset_RestoresBridgeButKeepsRoundAndFallStats()
        {
            GlassBridgeState s = NewState(2);
            GlassBridgeRound round = s.Round;
            s.Step(0, WrongTile(round, 0));
            Cross(s, 1);
            Cross(s, 0);
            Assert.IsTrue(s.Solved);
            s.Reset();
            Assert.AreSame(round, s.Round);
            Assert.IsFalse(s.Solved);
            Assert.AreEqual(0, s.AcrossCount);
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(0));
            Assert.AreEqual(GlassBridgeState.StartPlatform, s.RowOf(1));
            Assert.IsFalse(s.IsBroken(0, WrongTile(round, 0)));
            Assert.AreEqual(1, s.Falls);
            Assert.AreEqual(1, s.FallsOf(0));
        }

        // ---------- Kịch bản ----------

        [Test]
        public void TwoPlayers_GuideThenSwapSolvesWithoutFalls()
        {
            GlassBridgeState s = NewState(2, 11);
            int[] diagram;
            Assert.IsTrue(s.TryGetDiagram(1, out diagram), "người 1 ở đài đầu đọc bảng và dẫn");
            for (int row = 0; row < s.Round.Rows; row++)
                Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(0, diagram[row]));
            Assert.AreEqual(GlassBridgeResult.ReachedEnd, s.Disembark(0));
            Assert.IsFalse(s.Solved);
            Assert.IsTrue(s.TryGetDiagram(0, out diagram), "người 0 ở đài cuối đọc bảng và dẫn lại");
            for (int row = 0; row < s.Round.Rows; row++)
                Assert.AreEqual(GlassBridgeResult.Advanced, s.Step(1, diagram[row]));
            Assert.AreEqual(GlassBridgeResult.ReachedEnd, s.Disembark(1));
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(0, s.Falls);
        }

        [Test]
        public void FourPlayers_FallsThenRetryStillSolvable()
        {
            GlassBridgeState s = NewState(4, 3);
            s.Step(0, WrongTile(s.Round, 0));
            s.Step(1, s.Round.SafeTile(0));
            s.Step(1, WrongTile(s.Round, 1));
            Assert.AreEqual(2, s.Falls);
            for (int p = 0; p < 4; p++) Cross(s, p);
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(2, s.Falls);
        }
    }
}
