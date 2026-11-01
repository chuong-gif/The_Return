/*
 * Mục đích: NUnit EditMode test cho HostAuthority (T-024): kiểm người gửi và số thứ tự, từ chối lệnh sai có ghi nhật ký,
 * luật ném lỗi không làm sập host, gói trạng thái chung + thông tin riêng đúng người với đội 2/3/4, người vào muộn nhận gói đầy đủ,
 * kết nối lại đặt lại số thứ tự.
 * Hàm: NewHost (trợ giúp); các test Submit_*, Log_*, Broadcast_*, BuildStateFor_*, ConnectSlot_*, DisconnectSlot_*, MarkChanged_*.
 * Không cần scene hay Play Mode.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class HostAuthorityTests
    {
        /// <summary>Nhận số người, số món; trả host mới (mọi người đang kết nối) và luật giả qua tham số out.</summary>
        static HostAuthority<int, int[], int> NewHost(int teamSize, int items, out ClaimRules rules)
        {
            rules = new ClaimRules(items);
            return new HostAuthority<int, int[], int>(new TeamRoster(teamSize), rules, null);
        }

        [Test]
        public void Constructor_NullRosterOrRules_Throws()
        {
            ClaimRules rules = new ClaimRules(2);
            Assert.Throws<ArgumentNullException>(() => new HostAuthority<int, int[], int>(null, rules, null));
            Assert.Throws<ArgumentNullException>(() => new HostAuthority<int, int[], int>(new TeamRoster(2), null, null));
        }

        [Test]
        public void Constructor_NullLog_CreatesOwnLog()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            Assert.IsNotNull(host.Log);
            Assert.AreEqual(0, host.Version);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Submit_EachMemberClaimsOwnItem_AppliedWithSenderSlot(int n)
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(n, n, out rules);
            for (int slot = 0; slot < n; slot++)
            {
                CommandResult result = host.Submit(slot, 1, slot);
                Assert.IsTrue(result.IsAccepted);
                Assert.AreEqual(RejectReason.None, result.Reason);
                Assert.AreEqual(slot + 1, result.Version);
                Assert.AreEqual(slot, rules.LastSender);
            }
            int[] claimed = rules.ClaimedBy;
            for (int slot = 0; slot < n; slot++) Assert.AreEqual(slot, claimed[slot]);
        }

        [TestCase(-1)]
        [TestCase(3)]
        [TestCase(99)]
        public void Submit_InvalidSlot_RejectedAndRulesNeverCalled(int slot)
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(3, 3, out rules);
            CommandResult result = host.Submit(slot, 1, 0);
            Assert.IsFalse(result.IsAccepted);
            Assert.AreEqual(RejectReason.InvalidSlot, result.Reason);
            Assert.AreEqual(0, rules.ValidateCalls);
            Assert.AreEqual(0, rules.ApplyCalls);
            Assert.AreEqual(0, host.Version);
            Assert.AreEqual(1, host.Log.CountOf(HostLogLevel.Warning));
        }

        [Test]
        public void Submit_DisconnectedSlot_Rejected()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(3, 3, out rules);
            host.DisconnectSlot(1);
            CommandResult result = host.Submit(1, 1, 0);
            Assert.IsFalse(result.IsAccepted);
            Assert.AreEqual(RejectReason.NotConnected, result.Reason);
            Assert.AreEqual(0, rules.ValidateCalls);
        }

        [Test]
        public void Submit_NullCommand_Rejected()
        {
            HostAuthority<string, string, NoPrivate> host = new HostAuthority<string, string, NoPrivate>(new TeamRoster(2), new EchoRules(), null);
            CommandResult result = host.Submit(0, 1, null);
            Assert.IsFalse(result.IsAccepted);
            Assert.AreEqual(RejectReason.NullCommand, result.Reason);
            Assert.AreEqual(0, host.Version);
        }

        [Test]
        public void Submit_StaleOrDuplicateSequence_RejectedGapAccepted()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 4, out rules);
            Assert.IsTrue(host.Submit(0, 5, 0).IsAccepted);
            Assert.AreEqual(RejectReason.StaleSequence, host.Submit(0, 5, 1).Reason);
            Assert.AreEqual(RejectReason.StaleSequence, host.Submit(0, 4, 1).Reason);
            Assert.AreEqual(1, rules.ApplyCalls);
            Assert.IsTrue(host.Submit(0, 7, 1).IsAccepted);
            Assert.AreEqual(2, host.Version);
        }

        [Test]
        public void Submit_SequencesAreIndependentPerSlot()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            Assert.IsTrue(host.Submit(0, 3, 0).IsAccepted);
            Assert.IsTrue(host.Submit(1, 1, 1).IsAccepted);
        }

        [Test]
        public void Submit_RuleRejection_ReasonCarriedStateUnchangedSequenceConsumed()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 3, out rules);
            Assert.IsTrue(host.Submit(0, 1, 2).IsAccepted);

            CommandResult taken = host.Submit(1, 1, 2);
            Assert.IsFalse(taken.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, taken.Reason);
            StringAssert.Contains("đã có người nhận", taken.Detail);
            Assert.AreEqual(1, taken.Version);
            Assert.AreEqual(1, host.Version);
            Assert.AreEqual(1, rules.ApplyCalls);
            Assert.AreEqual(0, rules.ClaimsOf(1));

            Assert.AreEqual(RejectReason.StaleSequence, host.Submit(1, 1, 0).Reason);
            Assert.IsTrue(host.Submit(1, 2, 0).IsAccepted);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Submit_ItemOutOfRange_RejectedByRules(int item)
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 3, out rules);
            CommandResult result = host.Submit(0, 1, item);
            Assert.AreEqual(RejectReason.RejectedByRules, result.Reason);
            Assert.AreEqual(0, rules.ApplyCalls);
        }

        [Test]
        public void Submit_ValidateThrows_RejectedAndHostSurvives()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            rules.ThrowOnValidate = true;
            CommandResult result = host.Submit(0, 1, 0);
            Assert.IsFalse(result.IsAccepted);
            Assert.AreEqual(RejectReason.RuleError, result.Reason);
            Assert.AreEqual(0, rules.ApplyCalls);
            Assert.AreEqual(0, host.Version);

            rules.ThrowOnValidate = false;
            Assert.IsTrue(host.Submit(0, 2, 0).IsAccepted);
        }

        [Test]
        public void Submit_ApplyThrowsBeforeChange_RejectedButVersionAdvances()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            rules.ThrowOnApply = true;
            CommandResult result = host.Submit(0, 1, 0);
            Assert.AreEqual(RejectReason.RuleError, result.Reason);
            Assert.AreEqual(1, host.Version);
            Assert.AreEqual(-1, rules.ClaimedBy[0]);
        }

        [Test]
        public void Submit_ApplyThrowsAfterPartialChange_VersionAdvancesSoClientsSeeTruth()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 3, out rules);
            rules.MutateThenThrow = true;
            CommandResult result = host.Submit(0, 1, 2);
            Assert.AreEqual(RejectReason.RuleError, result.Reason);
            Assert.AreEqual(1, host.Version);
            Assert.AreEqual(0, rules.ClaimedBy[2]);

            StateMessage<int[], int> message = host.BuildStateFor(1);
            Assert.AreEqual(1, message.Version);
            Assert.AreEqual(0, message.Public[2]);
        }

        [Test]
        public void Log_RejectionWrittenAsWarningWithSlotAndReason()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            host.Submit(0, 1, 9);
            Assert.AreEqual(1, host.Log.Count);
            Assert.AreEqual(HostLogLevel.Warning, host.Log[0].Level);
            StringAssert.Contains("RejectedByRules", host.Log[0].Message);
            StringAssert.Contains("ô 0", host.Log[0].Message);
        }

        [Test]
        public void Log_AcceptedCommandsLoggedOnlyWhenEnabled()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            host.Submit(0, 1, 0);
            Assert.AreEqual(0, host.Log.Count);
            host.LogAccepted = true;
            host.Submit(1, 1, 1);
            Assert.AreEqual(1, host.Log.CountOf(HostLogLevel.Info));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Broadcast_OneMessagePerConnectedSlot_PrivateOnlyForRecipient(int n)
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(n, n + 1, out rules);
            for (int i = 0; i < n; i++) Assert.IsTrue(host.Submit(i, 1, i).IsAccepted);
            Assert.IsTrue(host.Submit(0, 2, n).IsAccepted);

            rules.PrivateRequests.Clear();
            int buildsBefore = rules.PublicBuilds;
            StateMessage<int[], int>[] messages = host.BuildBroadcast();

            Assert.AreEqual(n, messages.Length);
            Assert.AreEqual(buildsBefore + 1, rules.PublicBuilds);
            int[] expectedOrder = new int[n];
            for (int k = 0; k < n; k++) expectedOrder[k] = k;
            CollectionAssert.AreEqual(expectedOrder, rules.PrivateRequests);

            for (int k = 0; k < n; k++)
            {
                Assert.AreEqual(k, messages[k].Slot);
                Assert.AreEqual(n + 1, messages[k].Version);
                Assert.AreEqual(ClaimRules.SecretFor(k, k == 0 ? 2 : 1), messages[k].Private);
                Assert.AreSame(messages[0].Public, messages[k].Public);
                for (int j = 0; j < n; j++)
                {
                    if (j != k) Assert.AreNotEqual(messages[j].Private, messages[k].Private);
                }
            }
        }

        [Test]
        public void Broadcast_SkipsDisconnectedSlotsAndNeverBuildsTheirPrivateInfo()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(4, 4, out rules);
            host.DisconnectSlot(1);
            host.DisconnectSlot(3);
            rules.PrivateRequests.Clear();

            StateMessage<int[], int>[] messages = host.BuildBroadcast();

            Assert.AreEqual(2, messages.Length);
            Assert.AreEqual(0, messages[0].Slot);
            Assert.AreEqual(2, messages[1].Slot);
            CollectionAssert.AreEqual(new[] { 0, 2 }, rules.PrivateRequests);
        }

        [Test]
        public void Broadcast_NobodyConnected_EmptyAndNothingBuilt()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            host.DisconnectSlot(0);
            host.DisconnectSlot(1);
            int buildsBefore = rules.PublicBuilds;
            Assert.AreEqual(0, host.BuildBroadcast().Length);
            Assert.AreEqual(buildsBefore, rules.PublicBuilds);
        }

        [Test]
        public void Broadcast_WithoutPrivateInfoUsesNoPrivate()
        {
            HostAuthority<string, string, NoPrivate> host = new HostAuthority<string, string, NoPrivate>(new TeamRoster(3), new EchoRules(), null);
            Assert.IsTrue(host.Submit(1, 1, "xin chào").IsAccepted);
            StateMessage<string, NoPrivate>[] messages = host.BuildBroadcast();
            Assert.AreEqual(3, messages.Length);
            for (int i = 0; i < 3; i++) Assert.AreEqual("xin chào", messages[i].Public);
        }

        [Test]
        public void BuildStateFor_ConnectedSlotGetsFullState_OthersGetNull()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(3, 3, out rules);
            host.Submit(1, 1, 2);

            StateMessage<int[], int> message = host.BuildStateFor(1);
            Assert.IsNotNull(message);
            Assert.AreEqual(1, message.Slot);
            Assert.AreEqual(1, message.Version);
            CollectionAssert.AreEqual(new[] { -1, -1, 1 }, message.Public);
            Assert.AreEqual(ClaimRules.SecretFor(1, 1), message.Private);

            host.DisconnectSlot(2);
            Assert.IsNull(host.BuildStateFor(2));
            Assert.IsNull(host.BuildStateFor(-1));
            Assert.IsNull(host.BuildStateFor(3));
        }

        [Test]
        public void ConnectSlot_ResetsSequenceSoRejoinedClientStartsAtOne()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 3, out rules);
            Assert.IsTrue(host.Submit(1, 3, 1).IsAccepted);
            host.DisconnectSlot(1);
            Assert.AreEqual(RejectReason.NotConnected, host.Submit(1, 4, 2).Reason);
            host.ConnectSlot(1);
            Assert.IsTrue(host.Submit(1, 1, 2).IsAccepted);
            Assert.AreEqual(2, host.Log.CountOf(HostLogLevel.Info));
        }

        [Test]
        public void ConnectAndDisconnect_InvalidSlot_Throws()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            Assert.Throws<ArgumentOutOfRangeException>(() => host.ConnectSlot(9));
            Assert.Throws<ArgumentOutOfRangeException>(() => host.DisconnectSlot(-1));
        }

        [Test]
        public void DisconnectSlot_OthersKeepWorking()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(3, 3, out rules);
            host.DisconnectSlot(2);
            Assert.IsTrue(host.Submit(0, 1, 0).IsAccepted);
            Assert.IsTrue(host.Submit(1, 1, 1).IsAccepted);
            Assert.AreEqual(2, host.BuildBroadcast().Length);
        }

        [Test]
        public void MarkChanged_IncrementsVersionForNextBroadcast()
        {
            ClaimRules rules;
            HostAuthority<int, int[], int> host = NewHost(2, 2, out rules);
            host.MarkChanged();
            Assert.AreEqual(1, host.Version);
            Assert.AreEqual(1, host.BuildBroadcast()[0].Version);
        }
    }
}
