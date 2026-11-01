/*
 * Mục đích: NUnit EditMode test cho ClientReplica (T-024): số thứ tự lệnh, nhận gói trạng thái đúng ô, bỏ gói cũ/trùng/rỗng/ô khác,
 * gói đến sai thứ tự không làm trạng thái lùi, đặt lại khi kết nối lại.
 * Hàm: Msg (trợ giúp); các test Replica_*. Không cần scene hay Play Mode.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class ClientReplicaTests
    {
        static StateMessage<int[], int> Msg(int slot, int version, int marker, int secret)
        {
            return new StateMessage<int[], int>(slot, version, new[] { marker }, secret);
        }

        [Test]
        public void Replica_NegativeSlot_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ClientReplica<int[], int>(-1));
        }

        [Test]
        public void Replica_StartsEmpty()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(2);
            Assert.AreEqual(2, replica.Slot);
            Assert.IsFalse(replica.HasState);
            Assert.AreEqual(0, replica.Version);
            Assert.IsNull(replica.Public);
        }

        [Test]
        public void Replica_SequencesCountUpFromOneAndResetRestarts()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(0);
            Assert.AreEqual(1, replica.NextSequence());
            Assert.AreEqual(2, replica.NextSequence());
            Assert.AreEqual(3, replica.NextSequence());
            replica.Reset();
            Assert.AreEqual(1, replica.NextSequence());
        }

        [Test]
        public void Replica_FirstMessageAcceptedEvenAtVersionZero()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(1);
            Assert.IsTrue(replica.Receive(Msg(1, 0, 7, 105)));
            Assert.IsTrue(replica.HasState);
            Assert.AreEqual(0, replica.Version);
            Assert.AreEqual(7, replica.Public[0]);
            Assert.AreEqual(105, replica.Private);
        }

        [Test]
        public void Replica_MessageForAnotherSlotIgnored()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(1);
            Assert.IsFalse(replica.Receive(Msg(0, 5, 7, 100)));
            Assert.IsFalse(replica.HasState);
            Assert.AreEqual(0, replica.Version);
        }

        [Test]
        public void Replica_NullMessageIgnored()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(0);
            Assert.IsFalse(replica.Receive(null));
            Assert.IsFalse(replica.HasState);
        }

        [Test]
        public void Replica_StaleDuplicateAndOutOfOrderMessagesIgnored()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(0);
            Assert.IsTrue(replica.Receive(Msg(0, 3, 30, 1)));
            Assert.IsFalse(replica.Receive(Msg(0, 3, 31, 2)));
            Assert.IsFalse(replica.Receive(Msg(0, 2, 20, 3)));
            Assert.AreEqual(3, replica.Version);
            Assert.AreEqual(30, replica.Public[0]);
            Assert.IsTrue(replica.Receive(Msg(0, 4, 40, 4)));
            Assert.AreEqual(40, replica.Public[0]);
            Assert.AreEqual(4, replica.Private);
        }

        [Test]
        public void Replica_ResetClearsStateAndAcceptsOlderVersionAgain()
        {
            ClientReplica<int[], int> replica = new ClientReplica<int[], int>(0);
            replica.Receive(Msg(0, 5, 50, 9));
            replica.Reset();
            Assert.IsFalse(replica.HasState);
            Assert.AreEqual(0, replica.Version);
            Assert.IsNull(replica.Public);
            Assert.AreEqual(0, replica.Private);
            Assert.IsTrue(replica.Receive(Msg(0, 1, 10, 2)));
            Assert.AreEqual(1, replica.Version);
        }
    }
}
