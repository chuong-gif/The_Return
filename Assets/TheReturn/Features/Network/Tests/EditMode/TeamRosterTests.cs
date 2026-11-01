/*
 * Mục đích: NUnit EditMode test cho TeamRoster (T-024): đội chỉ 2-4 người, mặc định mọi người kết nối, bật/tắt kết nối,
 * ô không hợp lệ, danh sách ô đang kết nối tăng dần và là bản sao.
 * Hàm: các test Ctor_*, IsValidSlot_*, IsConnected_*, SetConnected_*, ConnectedSlots_*. Không cần scene hay Play Mode.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class TeamRosterTests
    {
        [TestCase(-3)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public void Ctor_SizeOutsideTwoToFour_Throws(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TeamRoster(size));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Ctor_ValidSize_EveryoneConnectedByDefault(int size)
        {
            TeamRoster roster = new TeamRoster(size);
            Assert.AreEqual(size, roster.Size);
            Assert.AreEqual(size, roster.ConnectedCount);
            for (int i = 0; i < size; i++) Assert.IsTrue(roster.IsConnected(i));
        }

        [TestCase(2)]
        [TestCase(4)]
        public void Ctor_AllConnectedFalse_NobodyConnected(int size)
        {
            TeamRoster roster = new TeamRoster(size, false);
            Assert.AreEqual(0, roster.ConnectedCount);
            Assert.AreEqual(0, roster.ConnectedSlots().Length);
        }

        [Test]
        public void IsValidSlot_OnlyInsideTeam()
        {
            TeamRoster roster = new TeamRoster(3);
            Assert.IsFalse(roster.IsValidSlot(-1));
            Assert.IsTrue(roster.IsValidSlot(0));
            Assert.IsTrue(roster.IsValidSlot(2));
            Assert.IsFalse(roster.IsValidSlot(3));
        }

        [Test]
        public void IsConnected_InvalidSlot_IsFalse()
        {
            TeamRoster roster = new TeamRoster(2);
            Assert.IsFalse(roster.IsConnected(-1));
            Assert.IsFalse(roster.IsConnected(2));
        }

        [Test]
        public void SetConnected_TogglesOneSlotAndCounts()
        {
            TeamRoster roster = new TeamRoster(4);
            roster.SetConnected(2, false);
            Assert.IsFalse(roster.IsConnected(2));
            Assert.IsTrue(roster.IsConnected(1));
            Assert.AreEqual(3, roster.ConnectedCount);
            roster.SetConnected(2, true);
            Assert.AreEqual(4, roster.ConnectedCount);
        }

        [Test]
        public void SetConnected_InvalidSlot_Throws()
        {
            TeamRoster roster = new TeamRoster(2);
            Assert.Throws<ArgumentOutOfRangeException>(() => roster.SetConnected(-1, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => roster.SetConnected(2, false));
        }

        [Test]
        public void ConnectedSlots_AscendingAndIsACopy()
        {
            TeamRoster roster = new TeamRoster(4);
            roster.SetConnected(1, false);
            int[] slots = roster.ConnectedSlots();
            CollectionAssert.AreEqual(new[] { 0, 2, 3 }, slots);
            slots[0] = 99;
            CollectionAssert.AreEqual(new[] { 0, 2, 3 }, roster.ConnectedSlots());
        }
    }
}
