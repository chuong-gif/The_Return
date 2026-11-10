/*
 * Mục đích: Kiểm thử LUẬT HOST của «Bảng cầu dao» online (T-031) ở EditMode, không cần Unity chạy: kiểm chủ công tắc, khoảng cách, quá tải,
 * giữ khớp đủ giờ, làm lại, thông tin riêng không lộ công tắc người khác, và chơi trọn một ván qua HostAuthority + ClientReplica cho đội 2/3/4 người.
 * Hàm: FakeBreakerWorld; các test trong BreakerHostRulesTests.
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.BreakerNet.Tests
{
    /// <summary>Thế giới giả: mặc định mọi ô đứng gần mọi công tắc; thêm ô vào FarSlots để cho ô đó đứng xa.</summary>
    public sealed class FakeBreakerWorld : IBreakerWorld
    {
        public readonly HashSet<int> FarSlots = new HashSet<int>();

        public bool IsNearSwitch(int slot, int sw)
        {
            return !FarSlots.Contains(slot);
        }
    }

    [TestFixture]
    public sealed class BreakerHostRulesTests
    {
        FakeBreakerWorld world;
        BreakerHostRules rules;
        HostAuthority<BreakerCommand, BreakerPublicState, BreakerPrivateState> host;
        ClientReplica<BreakerPublicState, BreakerPrivateState>[] replicas;

        void Setup(int n, int seed = 11, float hold = 3f)
        {
            world = new FakeBreakerWorld();
            rules = new BreakerHostRules(n, seed, world, hold);
            host = new HostAuthority<BreakerCommand, BreakerPublicState, BreakerPrivateState>(new TeamRoster(n), rules, new HostLog(64));
            replicas = new ClientReplica<BreakerPublicState, BreakerPrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<BreakerPublicState, BreakerPrivateState>(i);
        }

        CommandResult Send(int slot, BreakerCommand command)
        {
            return host.Submit(slot, replicas[slot].NextSequence(), command);
        }

        BreakerPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        void AdvanceHost(float seconds)
        {
            if (rules.Tick(seconds)) host.MarkChanged();
        }

        void ToggleSolution()
        {
            int solution = rules.Round.SolutionSwitches;
            for (int sw = 0; sw < rules.Round.Size; sw++)
            {
                if (((solution >> sw) & 1) == 0) continue;
                CommandResult r = Send(rules.Round.OwnerOf(sw), BreakerCommand.Toggle(sw));
                Assert.IsTrue(r.IsAccepted, "Bật công tắc đáp án " + sw + " bị từ chối: " + r.Detail);
            }
        }

        int FirstSolutionSwitch()
        {
            int solution = rules.Round.SolutionSwitches;
            for (int sw = 0; sw < rules.Round.Size; sw++) if (((solution >> sw) & 1) == 1) return sw;
            Assert.Fail("Đề không có công tắc đáp án.");
            return -1;
        }

        void SolveFully()
        {
            ToggleSolution();
            AdvanceHost(0.5f);
            AdvanceHost(2.9f);
            AdvanceHost(0.2f);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Toggle_Owner_Accepted_FlipsLampsAndBumpsVersion(int n)
        {
            Setup(n);
            int owner = rules.Round.OwnerOf(0);
            int before = host.Version;
            CommandResult r = Send(owner, BreakerCommand.Toggle(0));
            Assert.IsTrue(r.IsAccepted, r.Detail);
            Assert.Greater(host.Version, before);
            BreakerPublicState s = Pub();
            Assert.IsTrue(s.SwitchOn(0));
            Assert.AreEqual(rules.Round.StartLamps ^ rules.Round.SwitchMask(0), s.Lamps);
            Assert.AreEqual(BreakerMessage.Toggled, s.LastMessage);
            Assert.AreEqual(owner, s.LastActor);
            Assert.AreEqual(0, s.LastSwitch);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Toggle_NonOwner_Rejected_StateUnchanged(int n)
        {
            Setup(n);
            int owner = rules.Round.OwnerOf(0);
            int other = (owner + 1) % n;
            int before = host.Version;
            CommandResult r = Send(other, BreakerCommand.Toggle(0));
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, r.Reason);
            Assert.IsNotEmpty(r.Detail);
            Assert.AreEqual(before, host.Version);
            Assert.AreEqual(0, Pub().OnSwitches);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Toggle_TooFar_Rejected(int n)
        {
            Setup(n);
            int owner = rules.Round.OwnerOf(0);
            world.FarSlots.Add(owner);
            int before = host.Version;
            CommandResult r = Send(owner, BreakerCommand.Toggle(0));
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, r.Reason);
            StringAssert.Contains("xa", r.Detail);
            Assert.AreEqual(before, host.Version);
            Assert.AreEqual(0, Pub().OnSwitches);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Toggle_OutOfRangeSwitch_Rejected(int n)
        {
            Setup(n);
            Assert.IsFalse(Send(0, BreakerCommand.Toggle(-1)).IsAccepted);
            Assert.IsFalse(Send(0, BreakerCommand.Toggle(rules.Round.Size)).IsAccepted);
            Assert.AreEqual(0, Pub().OnSwitches);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Overload_TripsCircuit_TurnsEverythingOff(int n)
        {
            Setup(n);
            int limit = rules.Round.OverloadLimit;
            for (int sw = 0; sw <= limit; sw++)
            {
                CommandResult r = Send(rules.Round.OwnerOf(sw), BreakerCommand.Toggle(sw));
                Assert.IsTrue(r.IsAccepted, "Lệnh bật công tắc " + sw + " phải được nhận: " + r.Detail);
            }
            BreakerPublicState s = Pub();
            Assert.AreEqual(0, s.OnSwitches);
            Assert.AreEqual(1, s.Trips);
            Assert.AreEqual(BreakerMessage.Tripped, s.LastMessage);
            Assert.AreEqual(rules.Round.StartLamps, s.Lamps);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Solution_HeldLongEnough_Solves_Once(int n)
        {
            Setup(n);
            ToggleSolution();
            Assert.IsTrue(Pub().Matching);
            Assert.IsFalse(Pub().Solved);
            Assert.IsFalse(rules.Tick(0.5f));
            Assert.IsFalse(rules.Tick(2.9f));
            Assert.IsTrue(rules.Tick(0.2f), "Giữ khớp đủ 3 giây phải thắng.");
            Assert.IsFalse(rules.Tick(1f), "Đã thắng thì Tick không báo đổi nữa.");
            BreakerPublicState s = Pub();
            Assert.IsTrue(s.Solved);
            Assert.AreEqual(s.HoldSeconds, s.HoldElapsed, 0.0001f);
            Assert.AreEqual(BreakerMessage.Solved, s.LastMessage);
            int before = host.Version;
            host.MarkChanged();
            Assert.Greater(host.Version, before);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void BrokenMatch_CancelsHold_AndNeedsFullHoldAgain(int n)
        {
            Setup(n);
            ToggleSolution();
            Assert.IsFalse(rules.Tick(0.1f));
            Assert.IsFalse(rules.Tick(2f));
            int sw = FirstSolutionSwitch();
            Assert.IsTrue(Send(rules.Round.OwnerOf(sw), BreakerCommand.Toggle(sw)).IsAccepted);
            Assert.IsFalse(Pub().Matching);
            Assert.AreEqual(0f, Pub().HoldElapsed, 0.0001f);
            Assert.IsFalse(rules.Tick(5f), "Đèn không khớp thì không được thắng.");
            Assert.IsFalse(Pub().Solved);
            Assert.IsTrue(Send(rules.Round.OwnerOf(sw), BreakerCommand.Toggle(sw)).IsAccepted);
            Assert.IsTrue(Pub().Matching);
            Assert.IsFalse(rules.Tick(0.1f));
            Assert.IsFalse(rules.Tick(2f), "Phải giữ lại từ đầu, chưa đủ 3 giây.");
            Assert.IsTrue(rules.Tick(1.5f));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AfterSolved_ToggleAndRetry_Rejected(int n)
        {
            Setup(n);
            SolveFully();
            Assert.IsTrue(Pub().Solved);
            int sw = FirstSolutionSwitch();
            Assert.IsFalse(Send(rules.Round.OwnerOf(sw), BreakerCommand.Toggle(sw)).IsAccepted);
            Assert.IsFalse(Send(0, BreakerCommand.Retry()).IsAccepted);
            Assert.IsTrue(Pub().Solved);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Retry_ClearsSwitches_CountsReset(int n)
        {
            Setup(n);
            Assert.IsTrue(Send(rules.Round.OwnerOf(0), BreakerCommand.Toggle(0)).IsAccepted);
            Assert.IsTrue(Pub().SwitchOn(0));
            int who = n - 1;
            Assert.IsTrue(Send(who, BreakerCommand.Retry()).IsAccepted);
            BreakerPublicState s = Pub();
            Assert.AreEqual(0, s.OnSwitches);
            Assert.AreEqual(1, s.ResetCount);
            Assert.AreEqual(BreakerMessage.Retry, s.LastMessage);
            Assert.AreEqual(who, s.LastActor);
            Assert.AreEqual(rules.Round.StartLamps, s.Lamps);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void PrivateState_HasOnlyOwnSwitches_AndTogetherCoverAll(int n)
        {
            Setup(n);
            int total = 0;
            for (int slot = 0; slot < n; slot++)
            {
                BreakerPrivateState p = rules.BuildPrivateState(slot);
                int[] sw = p.SwitchesCopy();
                int[] mask = p.MasksCopy();
                Assert.AreEqual(sw.Length, mask.Length);
                for (int i = 0; i < sw.Length; i++)
                {
                    Assert.AreEqual(slot, rules.Round.OwnerOf(sw[i]), "Ô " + slot + " được biết công tắc không phải của mình.");
                    Assert.AreEqual(rules.Round.SwitchMask(sw[i]), mask[i]);
                    Assert.IsTrue(p.Owns(sw[i]));
                }
                for (int other = 0; other < rules.Round.Size; other++)
                {
                    if (rules.Round.OwnerOf(other) == slot) continue;
                    int m;
                    Assert.IsFalse(p.TryGetLampMask(other, out m));
                    Assert.AreEqual(0, m);
                }
                total += sw.Length;
            }
            Assert.AreEqual(rules.Round.Size, total, "Mỗi công tắc phải có đúng một chủ.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void PrivateState_OutsiderSlot_IsEmpty(int n)
        {
            Setup(n);
            Assert.AreEqual(0, rules.BuildPrivateState(-1).OwnedCount);
            Assert.AreEqual(0, rules.BuildPrivateState(n).OwnedCount);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void FullGame_Win_ReachesEveryReplica(int n)
        {
            Setup(n);
            SolveFully();
            StateMessage<BreakerPublicState, BreakerPrivateState>[] all = host.BuildBroadcast();
            Assert.AreEqual(n, all.Length);
            foreach (StateMessage<BreakerPublicState, BreakerPrivateState> msg in all)
                Assert.IsTrue(replicas[msg.Slot].Receive(msg));
            for (int slot = 0; slot < n; slot++)
            {
                ClientReplica<BreakerPublicState, BreakerPrivateState> rep = replicas[slot];
                Assert.IsTrue(rep.HasState);
                Assert.AreEqual(host.Version, rep.Version);
                Assert.IsTrue(rep.Public.Solved);
                Assert.IsTrue(rep.Public.Matching);
                int[] owned = rep.Private.SwitchesCopy();
                for (int i = 0; i < owned.Length; i++) Assert.AreEqual(slot, rules.Round.OwnerOf(owned[i]));
                Assert.AreEqual(rules.BuildPrivateState(slot).OwnedCount, rep.Private.OwnedCount);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void LateJoin_BuildStateFor_GivesFullState(int n)
        {
            Setup(n);
            Assert.IsTrue(Send(rules.Round.OwnerOf(0), BreakerCommand.Toggle(0)).IsAccepted);
            StateMessage<BreakerPublicState, BreakerPrivateState> msg = host.BuildStateFor(n - 1);
            Assert.IsTrue(replicas[n - 1].Receive(msg));
            Assert.IsTrue(replicas[n - 1].Public.SwitchOn(0));
            Assert.AreEqual(rules.Round.TargetLamps, replicas[n - 1].Public.TargetLamps);
            Assert.AreEqual(host.Version, replicas[n - 1].Version);
        }

        [TestCase(3)]
        [TestCase(4)]
        public void DisconnectedSlot_Rejected_AndSkippedInBroadcast(int n)
        {
            Setup(n);
            host.DisconnectSlot(1);
            CommandResult r = Send(1, BreakerCommand.Retry());
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.NotConnected, r.Reason);
            Assert.AreEqual(n - 1, host.BuildBroadcast().Length);
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            FakeBreakerWorld w = new FakeBreakerWorld();
            Assert.Catch<System.ArgumentException>(() => new BreakerHostRules(3, 1, null, 3f));
            Assert.Catch<System.ArgumentException>(() => new BreakerHostRules(3, 1, w, -1f));
            Assert.Catch<System.ArgumentException>(() => new BreakerHostRules(3, 1, w, float.NaN));
            Assert.Catch<System.Exception>(() => new BreakerHostRules(1, 1, w, 3f));
            Assert.Catch<System.Exception>(() => new BreakerHostRules(5, 1, w, 3f));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void PublicState_SameVersion_IsImmutable(int n)
        {
            Setup(n);
            ToggleSolution();
            string before = BreakerCodec.EncodePublic(Pub());
            int version = host.Version;
            Assert.IsFalse(rules.Tick(0.7f));
            Assert.AreEqual(version, host.Version);
            Assert.AreEqual(before, BreakerCodec.EncodePublic(Pub()), "Cùng Version thì trạng thái chung không được đổi theo đồng hồ.");
        }

        [Test]
        public void PrivateState_CopiesAreIndependent()
        {
            BreakerPrivateState p = new BreakerPrivateState(new[] { 1, 4 }, new[] { 3, 5 });
            int[] copy = p.SwitchesCopy();
            copy[0] = 99;
            Assert.AreEqual(1, p.SwitchesCopy()[0]);
            Assert.Throws<System.ArgumentException>(() => new BreakerPrivateState(new[] { 1 }, new[] { 1, 2 }));
            Assert.AreEqual(0, default(BreakerPrivateState).OwnedCount);
        }
    }
}
