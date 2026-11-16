/*
 * Mục đích: Test luật phòng chờ linh hoạt của RoomState (T-054): chủ phòng luôn sẵn sàng, khách bấm Sẵn sàng, mời ra, Bắt đầu khi từ 2 người
 * và mọi khách sẵn sàng, dồn ô thành đội đúng số người đang có. Không cần Unity/mạng.
 */
using NUnit.Framework;
using TheReturn.Network.Room;

namespace TheReturn.Network.Room.Tests
{
    public class RoomStateFlexibleTests
    {
        private static RoomState Room(int teamSize, int players)
        {
            RoomState room = new RoomState(teamSize, 0);
            for (int i = 0; i < players; i++) room.Join((ulong)i, "P" + i);
            return room;
        }

        [Test]
        public void Host_IsAlwaysReady_GuestsStartNotReady()
        {
            RoomState room = Room(4, 3);
            Assert.IsTrue(room.Rows()[0].Ready);
            Assert.IsFalse(room.Rows()[1].Ready);
            Assert.IsFalse(room.Rows()[2].Ready);
        }

        [Test]
        public void SetReady_RejectsHost_Stranger_AndAfterStart()
        {
            RoomState room = Room(4, 2);
            Assert.IsFalse(room.SetReady(0, true));
            Assert.IsFalse(room.SetReady(99, true));
            Assert.IsTrue(room.SetReady(1, true));
            Assert.IsTrue(room.TryStartFlexible(0));
            Assert.IsFalse(room.SetReady(1, false));
        }

        [Test]
        public void CanStartFlexible_NeedsTwoPlayers()
        {
            RoomState room = Room(4, 1);
            Assert.IsFalse(room.CanStartFlexible);
            room.Join(1, "Khách");
            room.SetReady(1, true);
            Assert.IsTrue(room.CanStartFlexible);
        }

        [Test]
        public void CanStartFlexible_FalseWhileAnyGuestNotReady_TrueWhenAllReady()
        {
            RoomState room = Room(4, 3);
            room.SetReady(1, true);
            Assert.IsFalse(room.CanStartFlexible);
            room.SetReady(2, true);
            Assert.IsTrue(room.CanStartFlexible);
            room.SetReady(2, false);
            Assert.IsFalse(room.CanStartFlexible);
        }

        [Test]
        public void TryStartFlexible_OnlyHostMayStart()
        {
            RoomState room = Room(4, 2);
            room.SetReady(1, true);
            Assert.IsFalse(room.TryStartFlexible(1));
            Assert.IsFalse(room.Started);
            Assert.IsTrue(room.TryStartFlexible(0));
            Assert.IsTrue(room.Started);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void TryStartFlexible_TeamSizeEqualsPlayersPresent(int players)
        {
            RoomState room = Room(4, players);
            for (int i = 1; i < players; i++) room.SetReady((ulong)i, true);
            Assert.IsTrue(room.TryStartFlexible(0));
            Assert.AreEqual(players, room.TeamSize);
            Assert.AreEqual(players, room.Count);
        }

        [Test]
        public void TryStartFlexible_CompactsSlotsAfterSomeoneLeft()
        {
            RoomState room = Room(4, 4);
            room.Leave(1);
            room.SetReady(2, true); room.SetReady(3, true);
            Assert.IsTrue(room.TryStartFlexible(0));
            var rows = room.Rows();
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(0, rows[0].Slot); Assert.AreEqual(1, rows[1].Slot); Assert.AreEqual(2, rows[2].Slot);
            Assert.AreEqual(0UL, rows[0].ClientId); Assert.AreEqual(2UL, rows[1].ClientId); Assert.AreEqual(3UL, rows[2].ClientId);
        }

        [Test]
        public void AfterStart_NoNewPlayerMayJoin()
        {
            RoomState room = Room(4, 2);
            room.SetReady(1, true);
            room.TryStartFlexible(0);
            Assert.AreEqual(-1, room.Join(9, "Muộn"));
            Assert.IsFalse(room.CanAdmit(2));
        }

        [Test]
        public void Kick_OnlyHost_NotSelf_NotAfterStart()
        {
            RoomState room = Room(4, 3);
            Assert.IsFalse(room.Kick(1, 2));
            Assert.IsFalse(room.Kick(0, 0));
            Assert.IsTrue(room.Kick(0, 2));
            Assert.AreEqual(2, room.Count);
            room.SetReady(1, true);
            room.TryStartFlexible(0);
            Assert.IsFalse(room.Kick(0, 1));
        }

        [Test]
        public void Kick_FreesSlot_AndKickedPlayerLosesReadyFlag()
        {
            RoomState room = Room(4, 3);
            room.SetReady(2, true);
            Assert.IsTrue(room.Kick(0, 2));
            Assert.IsFalse(room.IsReady(2));
            Assert.AreEqual(2, room.Join(7, "Mới"));
            Assert.IsFalse(room.Rows()[2].Ready);
        }

        [Test]
        public void GuestWhoLeavesAndRejoins_IsNotReadyAnymore()
        {
            RoomState room = Room(4, 2);
            room.SetReady(1, true);
            room.Leave(1);
            room.Join(1, "P1");
            Assert.IsFalse(room.Rows()[1].Ready);
            Assert.IsFalse(room.CanStartFlexible);
        }
    }
}
