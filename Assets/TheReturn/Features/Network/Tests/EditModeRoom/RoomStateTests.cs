/*
 * Mục đích: Test luật phòng online (RoomState, T-026): ô cố định, đội 2/3/4 người, phòng đầy, chỉ chủ phòng bắt đầu khi đủ người,
 * làm sạch + chống trùng tên, rời phòng trả ô. Không cần Unity/mạng.
 */
using System;
using NUnit.Framework;
using TheReturn.Network.Room;

namespace TheReturn.Network.Room.Tests
{
    public class RoomStateTests
    {
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Join_FillsSlotsInOrder_AndRefusesWhenFull(int size)
        {
            RoomState room = new RoomState(size);
            for (int i = 0; i < size; i++) Assert.AreEqual(i, room.Join((ulong)i, "A" + i));
            Assert.AreEqual(size, room.Count);
            Assert.AreEqual(-1, room.Join(99, "Thừa"));
            Assert.AreEqual(size, room.Count);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(0)]
        public void Constructor_RejectsTeamSizeOutside2To4(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomState(size));
        }

        [Test]
        public void Join_SameClientAgain_KeepsSlotAndUpdatesName()
        {
            RoomState room = new RoomState(3);
            Assert.AreEqual(0, room.Join(0, "Lan"));
            Assert.AreEqual(1, room.Join(5, "Minh"));
            Assert.AreEqual(1, room.Join(5, "Minh2"));
            Assert.AreEqual(2, room.Count);
            Assert.AreEqual("Minh2", room.Rows()[1].Name);
        }

        [Test]
        public void Leave_FreesOnlyThatSlot_AndNextJoinerTakesSmallestFree()
        {
            RoomState room = new RoomState(4);
            room.Join(0, "A"); room.Join(1, "B"); room.Join(2, "C");
            Assert.IsTrue(room.Leave(1));
            Assert.AreEqual(2, room.Count);
            Assert.AreEqual(1, room.Join(7, "D"));
            Assert.AreEqual(2, room.Rows()[2].Slot);
            Assert.IsFalse(room.Leave(42));
        }

        [Test]
        public void CanStart_OnlyWhenFull_AndOnlyHostMayStart()
        {
            RoomState room = new RoomState(2);
            room.Join(0, "Chủ");
            Assert.IsFalse(room.CanStart);
            Assert.IsFalse(room.TryStart(0));
            room.Join(1, "Khách");
            Assert.IsTrue(room.CanStart);
            Assert.IsFalse(room.TryStart(1));
            Assert.IsFalse(room.Started);
            Assert.IsTrue(room.TryStart(0));
            Assert.IsTrue(room.Started);
            Assert.IsFalse(room.TryStart(0));
        }

        [Test]
        public void AfterStart_NoNewPlayersAndNoAdmit()
        {
            RoomState room = new RoomState(2);
            room.Join(0, "A"); room.Join(1, "B");
            room.TryStart(0);
            room.Leave(1);
            Assert.AreEqual(-1, room.Join(9, "Vào muộn"));
            Assert.IsFalse(room.CanAdmit(1));
        }

        [Test]
        public void CanAdmit_ChecksConnectedCountAgainstTeamSize()
        {
            RoomState room = new RoomState(3);
            Assert.IsTrue(room.CanAdmit(0));
            Assert.IsTrue(room.CanAdmit(2));
            Assert.IsFalse(room.CanAdmit(3));
        }

        [Test]
        public void Rows_MarkHost_AndSortBySlot()
        {
            RoomState room = new RoomState(3, 10);
            room.Join(10, "Chủ"); room.Join(11, "B"); room.Join(12, "C");
            var rows = room.Rows();
            Assert.AreEqual(3, rows.Count);
            Assert.IsTrue(rows[0].IsHost);
            Assert.IsFalse(rows[1].IsHost);
            Assert.AreEqual("C", rows[2].Name);
        }

        [Test]
        public void SanitizeName_HandlesEmptyLongAndControlChars()
        {
            Assert.AreEqual("Người chơi 1", RoomState.SanitizeName(null, 0));
            Assert.AreEqual("Người chơi 3", RoomState.SanitizeName("   ", 2));
            Assert.AreEqual("AB", RoomState.SanitizeName("A\nB", 0));
            Assert.AreEqual(20, RoomState.SanitizeName(new string('x', 50), 0).Length);
            Assert.AreEqual("Lan", RoomState.SanitizeName("  Lan  ", 0));
        }

        [Test]
        public void DuplicateNames_AreMadeUnique_IgnoringCase()
        {
            RoomState room = new RoomState(3);
            room.Join(0, "Bạn"); room.Join(1, "bạn"); room.Join(2, "Bạn");
            var rows = room.Rows();
            Assert.AreEqual("Bạn", rows[0].Name);
            Assert.AreEqual("bạn (2)", rows[1].Name);
            Assert.AreEqual("Bạn (3)", rows[2].Name);
        }
    }
}
