/*
 * Mục đích: NUnit EditMode test cho RoomHandoffPlan (T-040 Q-014): kế hoạch bàn giao phòng -> map xếp đúng theo ô, từ chối mọi danh sách sai
 * (thiếu người, trùng ô, ô ngoài đội, trùng mã client, đội ngoài 2-4, null) mà không ném lỗi, và dùng chung với SlotAllocator cho ra ô nhân vật = ô phòng.
 * Hàm: các test Handoff_*.
 */
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class RoomHandoffPlanTests
    {
        static HandoffMember M(ulong id, int slot) { return new HandoffMember(id, slot); }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Handoff_FullTeamIsOrderedBySlotWhateverTheInputOrder(int n)
        {
            List<HandoffMember> rows = new List<HandoffMember>();
            for (int i = n - 1; i >= 0; i--) rows.Add(M((ulong)(100 + i * 7), i));
            RoomHandoffPlan plan;
            string error;
            Assert.IsTrue(RoomHandoffPlan.TryBuild(rows, n, false, out plan, out error), error);
            Assert.IsNull(error);
            Assert.AreEqual(n, plan.TeamSize);
            Assert.AreEqual(n, plan.Members.Count);
            for (int i = 0; i < n; i++) Assert.AreEqual(i, plan.Members[i].Slot);
        }

        [Test]
        public void Handoff_LookupByClientReturnsItsSlot()
        {
            RoomHandoffPlan plan;
            string error;
            Assert.IsTrue(RoomHandoffPlan.TryBuild(new[] { M(0, 0), M(5, 1), M(9, 2) }, 3, false, out plan, out error));
            int slot;
            Assert.IsTrue(plan.TryGetSlot(9, out slot));
            Assert.AreEqual(2, slot);
            Assert.IsTrue(plan.Contains(5));
            Assert.IsFalse(plan.TryGetSlot(77, out slot));
            Assert.AreEqual(-1, slot);
            Assert.IsFalse(plan.Contains(77));
        }

        [Test]
        public void Handoff_MissingPlayersAreRejectedUnlessPartialAllowed()
        {
            RoomHandoffPlan plan;
            string error;
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(0, 0) }, 2, false, out plan, out error));
            Assert.IsNull(plan);
            StringAssert.Contains("1/2", error);
            Assert.IsTrue(RoomHandoffPlan.TryBuild(new[] { M(0, 0) }, 2, true, out plan, out error));
            Assert.AreEqual(1, plan.Members.Count);
            Assert.AreEqual(2, plan.TeamSize);
        }

        [Test]
        public void Handoff_EmptyAndNullRowsAreRejectedEvenWhenPartialAllowed()
        {
            RoomHandoffPlan plan;
            string error;
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new HandoffMember[0], 2, true, out plan, out error));
            Assert.IsNotEmpty(error);
            Assert.IsFalse(RoomHandoffPlan.TryBuild(null, 2, true, out plan, out error));
            Assert.IsNotEmpty(error);
            Assert.IsNull(plan);
        }

        [Test]
        public void Handoff_DuplicateSlotDuplicateClientAndOutOfRangeSlotAreRejected()
        {
            RoomHandoffPlan plan;
            string error;
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(1, 0), M(2, 0) }, 2, false, out plan, out error));
            StringAssert.Contains("ô 0", error);
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(1, 0), M(1, 1) }, 2, false, out plan, out error));
            StringAssert.Contains("hai lần", error);
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(1, 0), M(2, 2) }, 2, false, out plan, out error));
            StringAssert.Contains("ngoài đội", error);
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(1, 0), M(2, -1) }, 2, false, out plan, out error));
            Assert.IsNull(plan);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(-3)]
        public void Handoff_TeamSizeOutsideTwoToFourIsRejected(int teamSize)
        {
            RoomHandoffPlan plan;
            string error;
            Assert.IsFalse(RoomHandoffPlan.TryBuild(new[] { M(1, 0), M(2, 1) }, teamSize, true, out plan, out error));
            StringAssert.Contains("2 đến 4", error);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Handoff_SpawningInPlanOrderGivesAvatarSlotEqualToRoomSlot(int n)
        {
            // Mã client giảm dần theo ô để chắc chắn thứ tự theo ô (không theo mã) quyết định.
            List<HandoffMember> rows = new List<HandoffMember>();
            for (int i = 0; i < n; i++) rows.Add(M((ulong)(900 - i * 100), i));
            RoomHandoffPlan plan;
            string error;
            Assert.IsTrue(RoomHandoffPlan.TryBuild(rows, n, false, out plan, out error), error);
            SlotAllocator allocator = new SlotAllocator(plan.TeamSize);
            foreach (HandoffMember m in plan.Members) Assert.AreEqual(m.Slot, allocator.Assign(m.ClientId));
            Assert.IsTrue(allocator.IsFull);
        }
    }
}
