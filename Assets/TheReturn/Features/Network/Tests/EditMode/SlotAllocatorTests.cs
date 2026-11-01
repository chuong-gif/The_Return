/*
 * Mục đích: NUnit EditMode test cho SlotAllocator (T-025): ô cố định, không đổi vai, đội 2/3/4, người rời chỉ trả ô của mình.
 * Hàm: các test Allocator_*. Không cần scene hay Play Mode.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class SlotAllocatorTests
    {
        [TestCase(1)]
        [TestCase(5)]
        public void Allocator_TeamSizeOutsideTwoToFour_Throws(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SlotAllocator(size));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Allocator_FirstJoinersGetSlotsInOrderAndExtraJoinerIsRejected(int n)
        {
            SlotAllocator allocator = new SlotAllocator(n);
            Assert.AreEqual(0, allocator.AssignedCount);
            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(i, allocator.Assign(100 + (ulong)i));
                Assert.AreEqual(i + 1, allocator.AssignedCount);
            }
            Assert.IsTrue(allocator.IsFull);
            Assert.AreEqual(-1, allocator.Assign(999));
            Assert.AreEqual(n, allocator.AssignedCount);
            Assert.AreEqual(-1, allocator.SlotOf(999));
        }

        [Test]
        public void Allocator_SameClientKeepsItsSlot()
        {
            SlotAllocator allocator = new SlotAllocator(3);
            Assert.AreEqual(0, allocator.Assign(7));
            Assert.AreEqual(1, allocator.Assign(8));
            Assert.AreEqual(1, allocator.Assign(8));
            Assert.AreEqual(2, allocator.AssignedCount);
        }

        [Test]
        public void Allocator_ReleaseFreesOnlyThatSlot_OthersNeverShift()
        {
            SlotAllocator allocator = new SlotAllocator(4);
            allocator.Assign(10);
            allocator.Assign(11);
            allocator.Assign(12);
            Assert.IsTrue(allocator.Release(11));
            Assert.AreEqual(0, allocator.SlotOf(10));
            Assert.AreEqual(-1, allocator.SlotOf(11));
            Assert.AreEqual(2, allocator.SlotOf(12));
            Assert.AreEqual(2, allocator.AssignedCount);
            Assert.IsFalse(allocator.Roster.IsConnected(1));
        }

        [Test]
        public void Allocator_NewJoinerTakesLowestFreeSlot()
        {
            SlotAllocator allocator = new SlotAllocator(4);
            allocator.Assign(10);
            allocator.Assign(11);
            allocator.Assign(12);
            allocator.Release(10);
            allocator.Release(11);
            Assert.AreEqual(0, allocator.Assign(20));
            Assert.AreEqual(1, allocator.Assign(21));
            Assert.AreEqual(2, allocator.SlotOf(12));
        }

        [Test]
        public void Allocator_ReleaseUnknownClient_ReturnsFalse()
        {
            SlotAllocator allocator = new SlotAllocator(2);
            Assert.IsFalse(allocator.Release(5));
            allocator.Assign(5);
            Assert.IsTrue(allocator.Release(5));
            Assert.IsFalse(allocator.Release(5));
        }

        [Test]
        public void Allocator_TryGetClientMatchesAssignments()
        {
            SlotAllocator allocator = new SlotAllocator(3);
            allocator.Assign(42);
            ulong client;
            Assert.IsTrue(allocator.TryGetClient(0, out client));
            Assert.AreEqual(42UL, client);
            Assert.IsFalse(allocator.TryGetClient(1, out client));
            Assert.IsFalse(allocator.TryGetClient(-1, out client));
            Assert.IsFalse(allocator.TryGetClient(3, out client));
        }

        [Test]
        public void Allocator_RosterMirrorsAssignedSlots()
        {
            SlotAllocator allocator = new SlotAllocator(4);
            Assert.AreEqual(0, allocator.Roster.ConnectedCount);
            allocator.Assign(1);
            allocator.Assign(2);
            CollectionAssert.AreEqual(new[] { 0, 1 }, allocator.Roster.ConnectedSlots());
        }
    }
}
