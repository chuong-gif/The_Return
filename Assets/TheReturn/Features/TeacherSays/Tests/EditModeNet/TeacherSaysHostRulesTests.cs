/*
 * Mục đích: Kiểm thử LUẬT HOST của «Cô nói» online (T-037) ở EditMode, không cần Unity chạy: host quyết vị trí/chuyển động (client ở xa không được tính),
 * cảnh cáo, đưa về, đứng yên, thắng, làm lại; thông tin riêng CHỈ tới người nghe; trạng thái chung KHÔNG phụ thuộc cờ «Cô nói»;
 * và chơi trọn một ván qua HostAuthority + ClientReplica cho đội 2/3/4 người, cả với đề do Generator sinh.
 * Hàm: FakeTeacherSaysWorld; các test trong TeacherSaysHostRulesTests (MakeRound dùng chung với TeacherSaysCodecTests).
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.TeacherSaysNet.Tests
{
    /// <summary>Thế giới giả: mặc định mọi ô đứng yên và đứng trong mọi vùng; thêm ô vào Moving / OutOfZone để đổi.</summary>
    public sealed class FakeTeacherSaysWorld : ITeacherSaysWorld
    {
        public readonly HashSet<int> Moving = new HashSet<int>();
        public readonly HashSet<int> OutOfZone = new HashSet<int>();
        public int SampleCalls;
        public float LastDelta;

        public void Sample(float deltaSeconds)
        {
            SampleCalls++;
            LastDelta = deltaSeconds;
        }

        public bool IsMoving(int slot)
        {
            return Moving.Contains(slot);
        }

        public bool IsInZone(int slot, int segment)
        {
            return !OutOfZone.Contains(slot);
        }
    }

    [TestFixture]
    public sealed class TeacherSaysHostRulesTests
    {
        FakeTeacherSaysWorld world;
        TeacherSaysHostRules rules;
        HostAuthority<TeacherSaysCommand, TeacherSaysPublicState, TeacherSaysPrivateState> host;
        ClientReplica<TeacherSaysPublicState, TeacherSaysPrivateState>[] replicas;

        /// <summary>
        /// Đề cố định: lệnh Bước,Đứng,Bước,Đứng,Bước; cờ Cô nói T,F,F,T,T (đảo hết nếu invertSaid); người nghe lần lượt theo vòng.
        /// Hành động đúng (không đảo): Bước, Bước, Đứng, Đứng, Bước. Seed = 7.
        /// </summary>
        public static TeacherSaysRound MakeRound(int n, bool invertSaid = false)
        {
            TeacherCommand[] commands = { TeacherCommand.Step, TeacherCommand.Stand, TeacherCommand.Step, TeacherCommand.Stand, TeacherCommand.Step };
            bool[] said = { true, false, false, true, true };
            if (invertSaid) for (int i = 0; i < said.Length; i++) said[i] = !said[i];
            int[] listeners = new int[TeacherSaysRound.SegmentCount];
            for (int i = 0; i < listeners.Length; i++) listeners[i] = i % n;
            return new TeacherSaysRound(n, 7, commands, said, listeners);
        }

        void Setup(int n, TeacherSaysRound round = null)
        {
            world = new FakeTeacherSaysWorld();
            rules = new TeacherSaysHostRules(round ?? MakeRound(n), world);
            host = new HostAuthority<TeacherSaysCommand, TeacherSaysPublicState, TeacherSaysPrivateState>(new TeamRoster(n), rules, new HostLog(64));
            replicas = new ClientReplica<TeacherSaysPublicState, TeacherSaysPrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<TeacherSaysPublicState, TeacherSaysPrivateState>(i);
        }

        void Begin()
        {
            Assert.IsTrue(rules.Begin());
            host.MarkChanged();
        }

        void Advance(float seconds)
        {
            if (rules.Tick(seconds)) host.MarkChanged();
        }

        CommandResult Send(int slot, TeacherSaysCommand command)
        {
            return host.Submit(slot, replicas[slot].NextSequence(), command);
        }

        TeacherSaysPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        /// <summary>Qua đoạn hiện tại bằng hành động đúng (mọi người đứng trong vùng, không ai đi).</summary>
        void PassCurrentSegment()
        {
            int seg = rules.State.Segment;
            if (rules.Round.RequiredAction(seg) == TeacherCommand.Step)
            {
                Advance(0.2f);
            }
            else
            {
                Advance(5.1f);
                Advance(0.1f);
            }
            Assert.AreEqual(seg + 1, rules.State.Segment, "Đoạn " + seg + " phải qua được.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void NotStarted_PublicIsInactive_PrivateIsNone_RetryRejected(int n)
        {
            Setup(n);
            TeacherSaysPublicState s = Pub();
            Assert.IsFalse(s.Started);
            Assert.IsFalse(s.Active);
            Assert.AreEqual(-1, s.Listener);
            Assert.AreEqual(0, s.Segment);
            for (int slot = 0; slot < n; slot++)
            {
                Assert.IsFalse(rules.BuildPrivateState(slot).IsListener, "Chưa bắt đầu thì không ai là người nghe (ô " + slot + ").");
            }
            CommandResult r = Send(0, TeacherSaysCommand.Retry());
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, r.Reason);
            StringAssert.Contains("chưa bắt đầu", r.Detail);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Begin_ShowsCommandAndListener_AndIsIdempotent(int n)
        {
            Setup(n);
            Begin();
            TeacherSaysPublicState s = Pub();
            Assert.IsTrue(s.Started);
            Assert.IsTrue(s.Active);
            Assert.AreEqual(0, s.Segment);
            Assert.AreEqual(rules.Round.Command(0), s.Command);
            Assert.AreEqual(rules.Round.Listener(0), s.Listener);
            Assert.AreEqual(0, s.Warnings);
            Assert.AreEqual(3, s.MaxWarnings, "Mặc định online: 3 cảnh cáo.");
            Assert.AreEqual(TeacherSaysNetEvent.Started, s.LastEvent);
            Assert.IsFalse(rules.Begin(), "Gọi Begin lần hai không làm gì.");
        }

        [Test]
        public void Rules_DefaultIsOnlinePreset()
        {
            Setup(3);
            Assert.AreEqual(8.0, rules.State.Rules.StepWindowSeconds, 0.0001);
            Assert.AreEqual(0.0, rules.State.Rules.SwitchExtraSeconds, 0.0001);
            Assert.AreEqual(5.0, rules.State.Rules.StandSeconds, 0.0001);
            Assert.AreEqual(3, rules.State.Rules.MaxWarnings);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Step_AllInZone_AdvancesSegment(int n)
        {
            Setup(n);
            Begin();
            int before = host.Version;
            Advance(0.3f);
            Assert.AreEqual(1, rules.State.Segment);
            TeacherSaysPublicState s = Pub();
            Assert.AreEqual(1, s.Segment);
            Assert.AreEqual(TeacherSaysNetEvent.Advanced, s.LastEvent);
            Assert.AreEqual(rules.Round.Listener(1), s.Listener);
            Assert.Greater(host.Version, before, "Có sự kiện thì phải tăng phiên bản.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Step_OnePlayerFarAway_DoesNotAdvance_ThenWarnsAtDeadline(int n)
        {
            Setup(n);
            Begin();
            world.OutOfZone.Add(n - 1);
            Advance(7f);
            Assert.AreEqual(0, rules.State.Segment, "Còn một người ở xa thì cả đội chưa qua đoạn (host dựa vào vị trí host thấy).");
            Assert.AreEqual(0, rules.State.Warnings);
            int before = host.Version;
            Advance(1.1f);
            Assert.AreEqual(1, rules.State.Warnings);
            Assert.AreEqual(0, rules.State.Segment);
            Assert.AreEqual(TeacherSaysNetEvent.Warned, Pub().LastEvent);
            Assert.Greater(host.Version, before);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ThreeWarnings_SendsEveryoneBackToStart(int n)
        {
            Setup(n);
            Begin();
            world.OutOfZone.Add(0);
            Advance(8.1f);
            Advance(8.1f);
            Assert.AreEqual(2, Pub().Warnings);
            Assert.AreEqual(0, Pub().SentBacks);
            Advance(8.1f);
            TeacherSaysPublicState s = Pub();
            Assert.AreEqual(0, s.Warnings, "Bị đưa về thì cảnh cáo về 0.");
            Assert.AreEqual(1, s.SentBacks);
            Assert.AreEqual(3, s.TotalWarnings, "Tổng cảnh cáo vẫn được giữ.");
            Assert.AreEqual(0, s.Segment);
            Assert.AreEqual(TeacherSaysNetEvent.SentBack, s.LastEvent);
            Assert.IsFalse(s.Solved);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Stand_MovingAfterReactionWindow_Warns(int n)
        {
            Setup(n);
            Begin();
            PassCurrentSegment();
            PassCurrentSegment();
            Assert.AreEqual(TeacherCommand.Stand, rules.Round.RequiredAction(rules.State.Segment), "Đề thử: đoạn 2 phải ĐỨNG.");
            world.Moving.Add(0);
            Advance(0.5f);
            Assert.AreEqual(0, rules.State.Warnings, "Trong 1 giây đầu (thời gian kịp nghe lệnh) cử động chưa bị tính.");
            Advance(0.6f);
            Assert.AreEqual(1, rules.State.Warnings);
            Assert.AreEqual(2, rules.State.Segment, "Cảnh cáo thì lặp lại đoạn.");
            Assert.AreEqual(TeacherSaysNetEvent.Warned, Pub().LastEvent);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Stand_StillForFiveSeconds_ThenZone_Advances(int n)
        {
            Setup(n);
            Begin();
            PassCurrentSegment();
            PassCurrentSegment();
            Advance(5.1f);
            Assert.AreEqual(TeacherSaysNetEvent.StandOk, Pub().LastEvent);
            Assert.IsTrue(rules.State.StandSatisfied);
            Assert.AreEqual(2, rules.State.Segment);
            Advance(0.1f);
            Assert.AreEqual(3, rules.State.Segment);
            Assert.AreEqual(TeacherSaysNetEvent.Advanced, Pub().LastEvent);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Stand_Done_ButOnePlayerFarAway_DoesNotAdvanceUntilHeArrives(int n)
        {
            Setup(n);
            Begin();
            PassCurrentSegment();
            PassCurrentSegment();
            world.OutOfZone.Add(0);
            Advance(5.1f);
            Advance(3f);
            Assert.AreEqual(2, rules.State.Segment, "Một người còn ở ngoài vùng thì chưa qua đoạn.");
            world.OutOfZone.Clear();
            Advance(0.1f);
            Assert.AreEqual(3, rules.State.Segment);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void FullRun_Solves_AndThenEverythingIsInert(int n)
        {
            Setup(n);
            Begin();
            for (int i = 0; i < TeacherSaysRound.SegmentCount; i++) PassCurrentSegment();
            Assert.IsTrue(rules.Solved);
            TeacherSaysPublicState s = Pub();
            Assert.IsTrue(s.Solved);
            Assert.IsFalse(s.Active);
            Assert.AreEqual(TeacherSaysRound.SegmentCount, s.Segment);
            Assert.AreEqual(-1, s.Listener);
            Assert.AreEqual(TeacherSaysNetEvent.Completed, s.LastEvent);
            Assert.IsFalse(rules.Tick(1f), "Đã thắng thì Tick không báo đổi nữa.");
            for (int slot = 0; slot < n; slot++) Assert.IsFalse(rules.BuildPrivateState(slot).IsListener);
            CommandResult r = Send(0, TeacherSaysCommand.Retry());
            Assert.IsFalse(r.IsAccepted, "Đã thắng thì không làm lại.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Retry_AfterStart_RestartsSegmentZero_KeepsRound_CountsRetry(int n)
        {
            Setup(n);
            Begin();
            PassCurrentSegment();
            PassCurrentSegment();
            int before = host.Version;
            CommandResult r = Send(n - 1, TeacherSaysCommand.Retry());
            Assert.IsTrue(r.IsAccepted, r.Detail);
            Assert.Greater(host.Version, before);
            Assert.AreEqual(0, rules.State.Segment);
            Assert.AreEqual(0, rules.State.Warnings);
            Assert.IsTrue(rules.Started);
            Assert.AreEqual(1, rules.RetryCount);
            Assert.AreEqual(7, rules.Round.Seed, "Làm lại giữ nguyên đề.");
            TeacherSaysPublicState s = Pub();
            Assert.AreEqual(1, s.RetryCount);
            Assert.AreEqual(TeacherSaysNetEvent.Retry, s.LastEvent);
            Assert.AreEqual(rules.Round.Listener(0), s.Listener);
        }

        [Test]
        public void Retry_FromInvalidSlot_Rejected()
        {
            Setup(3);
            Begin();
            Assert.IsFalse(rules.Validate(-1, TeacherSaysCommand.Retry()).IsAccepted);
            Assert.IsFalse(rules.Validate(3, TeacherSaysCommand.Retry()).IsAccepted);
            Assert.IsFalse(rules.Validate(0, null).IsAccepted);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_OnlyListenerHasInfo_OthersGetNone(int n)
        {
            Setup(n);
            Begin();
            for (int i = 0; i < TeacherSaysRound.SegmentCount; i++)
            {
                int seg = rules.State.Segment;
                int listener = rules.Round.Listener(seg);
                int listeners = 0;
                for (int slot = 0; slot < n; slot++)
                {
                    TeacherSaysPrivateState p = rules.BuildPrivateState(slot);
                    if (slot == listener)
                    {
                        listeners++;
                        Assert.IsTrue(p.IsListener);
                        Assert.AreEqual(rules.Round.TeacherSaid(seg), p.TeacherSaid, "Người nghe phải nhận đúng cờ Cô nói của đoạn " + seg);
                        TeacherCommand required;
                        Assert.IsTrue(p.TryGetRequiredAction(rules.Round.Command(seg), out required));
                        Assert.AreEqual(rules.Round.RequiredAction(seg), required);
                    }
                    else
                    {
                        Assert.IsFalse(p.IsListener, "Ô " + slot + " không phải người nghe đoạn " + seg);
                        Assert.IsFalse(p.TeacherSaid);
                        Assert.AreEqual(-1, p.RemainingMs);
                        Assert.IsFalse(p.StandSatisfied);
                        TeacherCommand ignored;
                        Assert.IsFalse(p.TryGetRequiredAction(rules.Round.Command(seg), out ignored));
                    }
                }
                Assert.AreEqual(1, listeners);
                PassCurrentSegment();
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RemainingTime_CountsDown_ForListenerOnly(int n)
        {
            Setup(n);
            Begin();
            world.OutOfZone.Add(0);
            Advance(2f);
            int listener = rules.Round.Listener(0);
            Assert.AreEqual(6000, rules.BuildPrivateState(listener).RemainingMs, 2, "Đoạn Bước: hạn 8 giây, đã trôi 2 giây.");
            for (int slot = 0; slot < n; slot++)
            {
                if (slot != listener) Assert.AreEqual(-1, rules.BuildPrivateState(slot).RemainingMs);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_StandSegment_ShowsStandCountdownThenSatisfied(int n)
        {
            Setup(n);
            Begin();
            PassCurrentSegment();
            PassCurrentSegment();
            int listener = rules.Round.Listener(2);
            Advance(1f);
            TeacherSaysPrivateState p = rules.BuildPrivateState(listener);
            Assert.AreEqual(4000, p.RemainingMs, 5, "Đoạn Đứng: còn 5 - 1 giây.");
            Assert.IsFalse(p.StandSatisfied);
            Advance(4.1f);
            p = rules.BuildPrivateState(listener);
            Assert.IsTrue(p.StandSatisfied);
            Assert.AreEqual(0, p.RemainingMs);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_DoesNotDependOnTeacherSaid(int n)
        {
            FakeTeacherSaysWorld wa = new FakeTeacherSaysWorld();
            FakeTeacherSaysWorld wb = new FakeTeacherSaysWorld();
            wa.OutOfZone.Add(0);
            wb.OutOfZone.Add(0);
            TeacherSaysHostRules a = new TeacherSaysHostRules(MakeRound(n, false), wa);
            TeacherSaysHostRules b = new TeacherSaysHostRules(MakeRound(n, true), wb);
            a.Begin();
            b.Begin();
            Assert.AreEqual(TeacherSaysCodec.EncodePublic(a.BuildPublicState()), TeacherSaysCodec.EncodePublic(b.BuildPublicState()));
            Assert.IsFalse(a.Tick(0.4f));
            Assert.IsFalse(b.Tick(0.4f));
            Assert.IsFalse(a.Tick(2.0f));
            Assert.IsFalse(b.Tick(2.0f));
            Assert.AreEqual(TeacherSaysCodec.EncodePublic(a.BuildPublicState()), TeacherSaysCodec.EncodePublic(b.BuildPublicState()),
                "Hai đề chỉ khác cờ «Cô nói» (đáp án ngược nhau) phải cho trạng thái chung GIỐNG HỆT, nếu không người không nghe suy ra được đáp án.");
            int listener = a.Round.Listener(0);
            Assert.AreNotEqual(TeacherSaysCodec.EncodePrivate(a.BuildPrivateState(listener)), TeacherSaysCodec.EncodePrivate(b.BuildPrivateState(listener)),
                "Riêng người nghe thì phải khác nhau (kiểm tra bài test có ý nghĩa).");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_Encoded_HasTwelveFields_NoSeed(int n)
        {
            TeacherSaysHostRules r = new TeacherSaysHostRules(n, 123456, new FakeTeacherSaysWorld());
            r.Begin();
            string text = TeacherSaysCodec.EncodePublic(r.BuildPublicState());
            Assert.AreEqual(12, text.Split('|').Length);
            Assert.IsFalse(text.Contains("123456"), "Seed không được lộ ra trong trạng thái chung.");
        }

        [Test]
        public void Tick_InvalidDelta_IsIgnored()
        {
            Setup(3);
            Begin();
            double before = rules.Clock;
            Assert.IsFalse(rules.Tick(float.NaN));
            Assert.IsFalse(rules.Tick(-1f));
            Assert.IsFalse(rules.Tick(float.PositiveInfinity));
            Assert.AreEqual(before, rules.Clock, 0.0000001);
            Assert.AreEqual(0, world.SampleCalls, "Delta sai thì không gọi Sample.");
        }

        [Test]
        public void Tick_AlwaysSamplesWorld_EvenBeforeBegin()
        {
            Setup(3);
            Assert.IsFalse(rules.Tick(0.25f));
            Assert.AreEqual(1, world.SampleCalls);
            Assert.AreEqual(0.25f, world.LastDelta, 0.0001f);
            Begin();
            Advance(0.001f);
            Assert.AreEqual(2, world.SampleCalls);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EndToEnd_HostAuthority_ReplicasSeePublicAndOnlyListenerSeesPrivate(int n)
        {
            Setup(n);
            Begin();
            StateMessage<TeacherSaysPublicState, TeacherSaysPrivateState>[] messages = host.BuildBroadcast();
            Assert.AreEqual(n, messages.Length);
            foreach (StateMessage<TeacherSaysPublicState, TeacherSaysPrivateState> m in messages) Assert.IsTrue(replicas[m.Slot].Receive(m));
            Assert.IsFalse(replicas[0].Receive(messages[0]), "Gói trùng phiên bản bị bỏ qua.");

            int listener = rules.Round.Listener(0);
            for (int slot = 0; slot < n; slot++)
            {
                Assert.AreEqual(slot == listener, replicas[slot].Private.IsListener, "Ô " + slot);
                Assert.AreEqual(TeacherSaysCodec.EncodePublic(replicas[0].Public), TeacherSaysCodec.EncodePublic(replicas[slot].Public));
            }

            int oldVersion = replicas[0].Version;
            Assert.IsTrue(Send(0, TeacherSaysCommand.Retry()).IsAccepted);
            foreach (StateMessage<TeacherSaysPublicState, TeacherSaysPrivateState> m in host.BuildBroadcast()) Assert.IsTrue(replicas[m.Slot].Receive(m));
            Assert.Greater(replicas[0].Version, oldVersion);
            for (int slot = 0; slot < n; slot++) Assert.AreEqual(1, replicas[slot].Public.RetryCount);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void GeneratedRounds_PlayThroughToVictory(int n)
        {
            for (int seed = 1; seed <= 40; seed++)
            {
                TeacherSaysHostRules r = new TeacherSaysHostRules(n, seed, new FakeTeacherSaysWorld());
                Assert.IsTrue(r.Begin());
                int guard = 0;
                while (!r.Solved && guard++ < 20)
                {
                    int seg = r.State.Segment;
                    if (r.Round.RequiredAction(seg) == TeacherCommand.Step)
                    {
                        r.Tick(0.2f);
                    }
                    else
                    {
                        r.Tick(5.1f);
                        r.Tick(0.1f);
                    }
                    Assert.AreEqual(seg + 1, r.State.Segment, "Đoạn " + seg + " của seed " + seed + " (n=" + n + ") không qua được.");
                }
                Assert.IsTrue(r.Solved, "Seed " + seed + " (n=" + n + ") phải thắng.");
                Assert.AreEqual(0, r.State.TotalWarnings);
            }
        }
    }
}
