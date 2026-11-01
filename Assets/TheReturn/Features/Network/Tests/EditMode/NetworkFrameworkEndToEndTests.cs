/*
 * Mục đích: NUnit EditMode test ĐẦU-CUỐI cho khung mạng host-authoritative (T-024): một HostAuthority nối với N ClientReplica
 * (N = 2, 3, 4), không có mạng thật. Kiểm lệnh đi lên host, gói trạng thái đi xuống đúng người, mọi máy hội tụ cùng trạng thái chung,
 * thông tin riêng không lẫn, lệnh gửi lại bị bỏ qua, hai người tranh một món, mất kết nối rồi vào lại.
 * Hàm: Deliver (trợ giúp); các test EndToEnd_*. Không cần scene hay Play Mode.
 */
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class NetworkFrameworkEndToEndTests
    {
        static ClientReplica<int[], int>[] NewReplicas(int n)
        {
            ClientReplica<int[], int>[] replicas = new ClientReplica<int[], int>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<int[], int>(i);
            return replicas;
        }

        /// <summary>Nhận host và các máy khách; gửi gói của host cho từng người đang kết nối.</summary>
        static void Deliver(HostAuthority<int, int[], int> host, ClientReplica<int[], int>[] replicas)
        {
            foreach (StateMessage<int[], int> message in host.BuildBroadcast())
                replicas[message.Slot].Receive(message);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EndToEnd_EveryoneClaimsAnItem_AllReplicasConverge(int n)
        {
            ClaimRules rules = new ClaimRules(n);
            HostAuthority<int, int[], int> host = new HostAuthority<int, int[], int>(new TeamRoster(n), rules, null);
            ClientReplica<int[], int>[] replicas = NewReplicas(n);
            Deliver(host, replicas);

            for (int i = 0; i < n; i++)
            {
                CommandResult result = host.Submit(i, replicas[i].NextSequence(), i);
                Assert.IsTrue(result.IsAccepted, "người " + i);
                Deliver(host, replicas);
            }

            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(host.Version, replicas[i].Version);
                CollectionAssert.AreEqual(rules.ClaimedBy, replicas[i].Public);
                Assert.AreEqual(ClaimRules.SecretFor(i, 1), replicas[i].Private);
                Assert.AreEqual(i, replicas[i].Public[i]);
            }
        }

        [Test]
        public void EndToEnd_TwoPlayersClaimSameItem_FirstWinsAndEveryoneAgrees()
        {
            ClaimRules rules = new ClaimRules(3);
            HostAuthority<int, int[], int> host = new HostAuthority<int, int[], int>(new TeamRoster(3), rules, null);
            ClientReplica<int[], int>[] replicas = NewReplicas(3);

            CommandResult first = host.Submit(0, replicas[0].NextSequence(), 0);
            CommandResult second = host.Submit(1, replicas[1].NextSequence(), 0);
            Deliver(host, replicas);

            Assert.IsTrue(first.IsAccepted);
            Assert.IsFalse(second.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, second.Reason);
            for (int i = 0; i < 3; i++) Assert.AreEqual(0, replicas[i].Public[0]);
            Assert.AreEqual(ClaimRules.SecretFor(1, 0), replicas[1].Private);
        }

        [Test]
        public void EndToEnd_RetransmittedCommandIsIgnored()
        {
            ClaimRules rules = new ClaimRules(2);
            HostAuthority<int, int[], int> host = new HostAuthority<int, int[], int>(new TeamRoster(2), rules, null);
            ClientReplica<int[], int>[] replicas = NewReplicas(2);

            int sequence = replicas[0].NextSequence();
            Assert.IsTrue(host.Submit(0, sequence, 0).IsAccepted);
            int versionAfter = host.Version;
            CommandResult again = host.Submit(0, sequence, 0);

            Assert.IsFalse(again.IsAccepted);
            Assert.AreEqual(RejectReason.StaleSequence, again.Reason);
            Assert.AreEqual(versionAfter, host.Version);
            Assert.AreEqual(1, rules.ApplyCalls);
        }

        [Test]
        public void EndToEnd_DisconnectedPlayerMissesUpdates_ReconnectCatchesUp()
        {
            ClaimRules rules = new ClaimRules(3);
            HostAuthority<int, int[], int> host = new HostAuthority<int, int[], int>(new TeamRoster(3), rules, null);
            ClientReplica<int[], int>[] replicas = NewReplicas(3);
            Deliver(host, replicas);

            host.DisconnectSlot(2);
            Assert.IsTrue(host.Submit(0, replicas[0].NextSequence(), 0).IsAccepted);
            Deliver(host, replicas);

            Assert.AreEqual(1, replicas[0].Version);
            Assert.AreEqual(1, replicas[1].Version);
            Assert.AreEqual(0, replicas[2].Version);
            Assert.AreEqual(-1, replicas[2].Public[0]);

            host.ConnectSlot(2);
            replicas[2].Reset();
            Assert.IsTrue(replicas[2].Receive(host.BuildStateFor(2)));
            Assert.AreEqual(1, replicas[2].Version);
            Assert.AreEqual(0, replicas[2].Public[0]);

            int sequence = replicas[2].NextSequence();
            Assert.AreEqual(1, sequence);
            Assert.IsTrue(host.Submit(2, sequence, 2).IsAccepted);
        }

        [Test]
        public void EndToEnd_PrivateInfoNeverLeaksToOtherPlayers()
        {
            ClaimRules rules = new ClaimRules(4);
            HostAuthority<int, int[], int> host = new HostAuthority<int, int[], int>(new TeamRoster(4), rules, null);
            ClientReplica<int[], int>[] replicas = NewReplicas(4);
            host.Submit(2, replicas[2].NextSequence(), 1);
            host.Submit(2, replicas[2].NextSequence(), 3);
            Deliver(host, replicas);

            Assert.AreEqual(ClaimRules.SecretFor(2, 2), replicas[2].Private);
            for (int i = 0; i < 4; i++)
            {
                if (i == 2) continue;
                Assert.AreEqual(ClaimRules.SecretFor(i, 0), replicas[i].Private);
                Assert.AreNotEqual(replicas[2].Private, replicas[i].Private);
            }
        }
    }
}
