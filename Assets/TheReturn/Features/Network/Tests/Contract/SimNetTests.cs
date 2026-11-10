/*
 * Mục đích: Kiểm chính MẠNG GIẢ SimNet (T-039, M2) bằng nhiệm vụ giả: lệnh trùng bị host từ chối là trùng (không làm hỏng ván),
 * trạng thái đảo/trùng/mất không làm bản sao lùi, cùng seed ra cùng thống kê lỗi, và sau Settle mọi bản sao hội tụ.
 * Hàm: SimNetTests.Calm_NoFaultsCountersStayZero; Rough_DuplicatesAreStaleNotHarmful; Rough_ReplicasNeverRegress_AndConverge;
 * SameSeed_SameFaultStatistics.
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract.Tests
{
    [TestFixture]
    public sealed class SimNetTests
    {
        sealed class Run
        {
            public SimNet<FakeCommand, FakePublic, FakePrivate> Net;
            public HostAuthority<FakeCommand, FakePublic, FakePrivate> Host;
            public ClientReplica<FakePublic, FakePrivate>[] Replicas;
            public FakeGame Game;
        }

        static Run Play(int n, int seed, SimFaults faults)
        {
            var run = new Run();
            run.Game = new FakeGame(n, seed, FakeFlaw.None);
            run.Host = new HostAuthority<FakeCommand, FakePublic, FakePrivate>(new TeamRoster(n), run.Game.Rules, new HostLog(64));
            run.Replicas = new ClientReplica<FakePublic, FakePrivate>[n];
            for (int i = 0; i < n; i++) run.Replicas[i] = new ClientReplica<FakePublic, FakePrivate>(i);
            run.Net = new SimNet<FakeCommand, FakePublic, FakePrivate>(run.Host, run.Replicas, faults, seed);
            run.Net.HostBroadcast();
            int slot;
            FakeCommand command;
            while (!run.Game.IsSolved && run.Game.TryNextSolveMove(out slot, out command))
            {
                run.Net.ClientSend(slot, command);
                for (int w = 0; w < 50 && run.Net.PendingUp > 0; w++) run.Net.Step();
                run.Net.HostBroadcast();
                run.Net.Step();
            }
            run.Net.Settle();
            return run;
        }

        /// <summary>Mạng êm: không mất, không trùng, không lùi, không từ chối nào.</summary>
        [Test]
        public void Calm_NoFaultsCountersStayZero()
        {
            var run = Play(3, 5, SimFaults.Calm);
            Assert.IsTrue(run.Game.IsSolved);
            Assert.AreEqual(0, run.Net.DroppedStates);
            Assert.AreEqual(0, run.Net.DuplicatedPackets);
            Assert.AreEqual(0, run.Net.StaleRejects);
            Assert.AreEqual(0, run.Net.OtherRejects);
            Assert.AreEqual(0, run.Net.Regressions);
            Assert.AreEqual(3, run.Net.AcceptedCommands);
        }

        /// <summary>Mạng xấu: lệnh trùng chỉ thành «từ chối trùng», ván vẫn thắng, mọi lệnh thật chỉ được chấp nhận đúng một lần.</summary>
        [Test]
        public void Rough_DuplicatesAreStaleNotHarmful()
        {
            int totalStale = 0;
            for (int seed = 1; seed <= 60; seed++)
            {
                var run = Play(4, seed, SimFaults.Rough);
                Assert.IsTrue(run.Game.IsSolved, "seed " + seed);
                Assert.AreEqual(4, run.Net.AcceptedCommands, "seed " + seed + ": mỗi lệnh thật phải được chấp nhận đúng một lần.");
                Assert.AreEqual(0, run.Net.OtherRejects, "seed " + seed + ": " + run.Net.FirstOtherReject);
                totalStale += run.Net.StaleRejects;
            }
            Assert.Greater(totalStale, 0, "60 seed mạng xấu mà không có lệnh trùng nào: tiêm lỗi không hoạt động.");
        }

        /// <summary>Mạng xấu: bản sao không lùi Version và hội tụ về host; thực sự có gói bị mất/bỏ qua (tiêm lỗi hoạt động).</summary>
        [Test]
        public void Rough_ReplicasNeverRegress_AndConverge()
        {
            int dropped = 0;
            int ignored = 0;
            for (int seed = 1; seed <= 60; seed++)
            {
                var run = Play(3, seed, SimFaults.Rough);
                Assert.AreEqual(0, run.Net.Regressions, "seed " + seed);
                foreach (var rep in run.Replicas)
                {
                    Assert.IsTrue(rep.HasState, "seed " + seed);
                    Assert.AreEqual(run.Host.Version, rep.Version, "seed " + seed);
                    Assert.IsTrue(rep.Public.Solved, "seed " + seed);
                }
                dropped += run.Net.DroppedStates;
                ignored += run.Net.StatesIgnored;
            }
            Assert.Greater(dropped, 0, "Không có gói trạng thái nào bị mất trong 60 seed.");
            Assert.Greater(ignored, 0, "Không có gói cũ/trùng nào bị bản sao bỏ qua trong 60 seed.");
        }

        /// <summary>Cùng seed cho đúng cùng thống kê lỗi (tái hiện được).</summary>
        [Test]
        public void SameSeed_SameFaultStatistics()
        {
            var a = Play(4, 17, SimFaults.Rough);
            var b = Play(4, 17, SimFaults.Rough);
            Assert.AreEqual(a.Net.DroppedStates, b.Net.DroppedStates);
            Assert.AreEqual(a.Net.DuplicatedPackets, b.Net.DuplicatedPackets);
            Assert.AreEqual(a.Net.StaleRejects, b.Net.StaleRejects);
            Assert.AreEqual(a.Net.StatesApplied, b.Net.StatesApplied);
            Assert.AreEqual(a.Net.StatesIgnored, b.Net.StatesIgnored);
        }
    }
}
