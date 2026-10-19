/*
 * Mục đích: Test NUnit EditMode cho luật «Cô nói» (T-006c): đề, lệnh thật/giả, Bước, Đứng, cảnh cáo, về đầu, đổi vai, thắng.
 */
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class TeacherSaysTests
    {
        // Lệnh {Step,Stand,Step,Stand,Step}, cờ {thật,thật,giả,giả,thật} => hành động đúng {Step,Stand,Stand,Step,Step}.
        static TeacherSaysRound MakeRound(int players = 4)
        {
            return new TeacherSaysRound(players, 1,
                new[] { TeacherCommand.Step, TeacherCommand.Stand, TeacherCommand.Step, TeacherCommand.Stand, TeacherCommand.Step },
                new[] { true, true, false, false, true },
                new[] { 0, 1, 2, 3 % players, 0 });
        }

        static TeacherSaysState Make(double step = 10, double extra = 3, double stand = 5, int warns = 3)
        {
            var s = new TeacherSaysState(MakeRound(), new TeacherSaysRules(step, extra, stand, warns));
            s.Begin(0);
            return s;
        }

        [Test] public void Round_RequiredAction_InvertsFakeCommands()
        {
            var r = MakeRound();
            Assert.AreEqual(TeacherCommand.Step, r.RequiredAction(0));
            Assert.AreEqual(TeacherCommand.Stand, r.RequiredAction(1));
            Assert.AreEqual(TeacherCommand.Stand, r.RequiredAction(2));
            Assert.AreEqual(TeacherCommand.Step, r.RequiredAction(3));
            Assert.AreEqual(TeacherCommand.Step, r.RequiredAction(4));
            Assert.AreEqual(3, r.RealCount);
            Assert.AreEqual(3, r.RequiredStepCount);
        }

        [Test] public void Generator_ProducesValidDeterministicRounds()
        {
            for (int players = 2; players <= 4; players++)
                for (int seed = 0; seed < 150; seed++)
                {
                    var a = TeacherSaysGenerator.Generate(players, seed);
                    var b = TeacherSaysGenerator.Generate(players, seed);
                    Assert.IsTrue(TeacherSaysGenerator.IsValid(a), "players " + players + " seed " + seed);
                    for (int i = 0; i < TeacherSaysRound.SegmentCount; i++)
                    {
                        Assert.AreEqual(a.Command(i), b.Command(i));
                        Assert.AreEqual(a.TeacherSaid(i), b.TeacherSaid(i));
                        Assert.AreEqual(a.Listener(i), b.Listener(i));
                    }
                }
        }

        [Test] public void Generator_ListenerChangesEverySegment()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var r = TeacherSaysGenerator.Generate(3, seed);
                for (int i = 1; i < TeacherSaysRound.SegmentCount; i++) Assert.AreNotEqual(r.Listener(i - 1), r.Listener(i));
            }
        }

        [Test] public void Tick_BeforeBegin_DoesNothing()
        {
            var s = new TeacherSaysState(MakeRound());
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(100, true, true));
            Assert.AreEqual(0, s.Segment);
        }

        [Test] public void Step_AdvancesWhenAllInNextZone()
        {
            var s = Make();
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(1, true, false));
            Assert.AreEqual(TeacherSaysEvent.Advanced, s.Tick(2, true, true));
            Assert.AreEqual(1, s.Segment);
        }

        [Test] public void Step_TimeoutWarnsAndRepeatsSegment()
        {
            var s = Make(step: 10);
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(10, false, false));
            Assert.AreEqual(TeacherSaysEvent.Warned, s.Tick(10.5, false, false));
            Assert.AreEqual(1, s.Warnings);
            Assert.AreEqual(0, s.Segment);
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(20, false, false)); // hạn mới 10.5 + 10 = 20.5
            Assert.AreEqual(TeacherSaysEvent.Warned, s.Tick(21, false, false));
        }

        [Test] public void Warnings_ThreeSendBackToStart()
        {
            var s = Make(step: 1, warns: 3);
            s.Tick(1.5, false, false);
            s.Tick(3, false, false);
            Assert.AreEqual(TeacherSaysEvent.SentBack, s.Tick(5, false, false));
            Assert.AreEqual(0, s.Segment);
            Assert.AreEqual(0, s.Warnings);
            Assert.AreEqual(1, s.SentBacks);
            Assert.AreEqual(3, s.TotalWarnings);
        }

        [Test] public void SentBack_FromLaterSegment_ReturnsToSegmentZero()
        {
            var s = Make(step: 1, warns: 1);
            s.Tick(0.1, false, true); // đoạn 0 -> 1 (Đứng)
            Assert.AreEqual(1, s.Segment);
            Assert.AreEqual(TeacherSaysEvent.SentBack, s.Tick(3, true, false));
            Assert.AreEqual(0, s.Segment);
        }

        [Test] public void Stand_MovingAfterReactionWarns()
        {
            var s = Make();
            s.Tick(0.1, false, true); // sang đoạn 1 (Đứng), bắt đầu t=0.1
            Assert.AreEqual(TeacherSaysEvent.Warned, s.Tick(2.0, true, false));
            Assert.AreEqual(1, s.Warnings);
            Assert.AreEqual(1, s.Segment);
        }

        [Test] public void Stand_MovingInsideReactionWindowIsIgnored()
        {
            var s = Make();
            s.Tick(0.1, false, true);
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(0.6, true, false));
            Assert.AreEqual(0, s.Warnings);
        }

        [Test] public void Stand_StillForEnoughSecondsThenFreeToWalk()
        {
            var s = Make(stand: 5);
            s.Tick(0.1, false, true); // đoạn 1 từ t=0.1
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(4, false, false));
            Assert.AreEqual(TeacherSaysEvent.StandOk, s.Tick(5.2, false, false));
            Assert.IsTrue(s.StandSatisfied);
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(6, true, false)); // đi tự do, không phạt
            Assert.AreEqual(TeacherSaysEvent.Advanced, s.Tick(7, true, true));
            Assert.AreEqual(2, s.Segment);
        }

        [Test] public void Stand_EnteringNextZoneTooEarlyDoesNotAdvance()
        {
            var s = Make(stand: 5);
            s.Tick(0.1, false, true);
            var e = s.Tick(0.5, false, true); // đứng yên nhưng đã ở vùng kế (dịch chuyển): chưa đủ giờ
            Assert.AreNotEqual(TeacherSaysEvent.Advanced, e);
            Assert.AreEqual(1, s.Segment);
        }

        [Test] public void ExtendForSwitch_AddsTimeUpToCap()
        {
            var s = Make(step: 10, extra: 3);
            Assert.AreEqual(3, s.ExtendForSwitch(1), 1e-9);
            Assert.AreEqual(3, s.ExtendForSwitch(2), 1e-9);
            Assert.AreEqual(3, s.ExtendForSwitch(3), 1e-9);
            Assert.AreEqual(1, s.ExtendForSwitch(4), 1e-9); // đã 9, cộng tối đa 10
            Assert.AreEqual(0, s.ExtendForSwitch(5), 1e-9);
            Assert.AreEqual(20, s.RemainingSeconds(0), 1e-9);
        }

        [Test] public void ExtendForSwitch_NoEffectOnStandOrUnlimited()
        {
            var s = Make();
            s.Tick(0.1, false, true); // đoạn Đứng
            Assert.AreEqual(0, s.ExtendForSwitch(1), 1e-9);
            var u = Make(step: 0);
            Assert.AreEqual(0, u.ExtendForSwitch(1), 1e-9);
            Assert.AreEqual(TeacherSaysEvent.None, u.Tick(1000, false, false)); // không giới hạn: không bao giờ hết giờ
        }

        [Test] public void FiveSegments_Solves()
        {
            var s = Make(stand: 2);
            double t = 0;
            for (int seg = 0; seg < 5; seg++)
            {
                t += 0.1;
                if (s.Round.RequiredAction(seg) == TeacherCommand.Stand) { t += 2.5; Assert.AreEqual(TeacherSaysEvent.StandOk, s.Tick(t, false, false)); }
                var e = s.Tick(t + 0.1, false, true);
                Assert.AreEqual(seg == 4 ? TeacherSaysEvent.Completed : TeacherSaysEvent.Advanced, e, "đoạn " + seg);
                t += 0.1;
            }
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(TeacherSaysEvent.None, s.Tick(t + 5, true, true));
        }

        [Test] public void TryReadPrefix_OnlyListenerOfCurrentSegment()
        {
            var s = Make();
            bool said;
            Assert.IsTrue(s.TryReadPrefix(0, out said));
            Assert.IsTrue(said);
            Assert.IsFalse(s.TryReadPrefix(1, out said));
            s.Tick(0.1, false, true);
            Assert.IsTrue(s.TryReadPrefix(1, out said));
            Assert.IsFalse(s.TryReadPrefix(0, out said));
        }

        [Test] public void Reset_KeepsRoundAndCounters()
        {
            var s = Make(step: 1);
            s.Tick(2, false, false);
            s.Reset();
            Assert.AreEqual(0, s.Segment);
            Assert.AreEqual(0, s.Warnings);
            Assert.IsFalse(s.Started);
            Assert.AreEqual(1, s.TotalWarnings);
            s.Begin(10);
            Assert.AreEqual(TeacherSaysEvent.Advanced, s.Tick(10.5, false, true));
        }

        [Test] public void Rules_ValidateArguments()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TeacherSaysRules(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TeacherSaysRules(10, -1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TeacherSaysRules(10, 1, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TeacherSaysRules(10, 1, 5, 0));
            Assert.AreEqual(8.0, TeacherSaysRules.Online.StepWindowSeconds);
            Assert.AreEqual(0.0, TeacherSaysRules.Online.SwitchExtraSeconds);
        }
    }
}
