/*
 * Mục đích: Kiểm tra OnlineNames (T-055): tên người chơi theo ô thay cho Minh/An/Bình/Chi, thiếu thì dùng tên mặc định, trùng thì tự đánh số, và nguồn tên cho PrototypePartyController.
 * Hàm kiểm: Resolve_UsesPlayerNames; Resolve_FallsBackWhenBlankOrMissing; Resolve_RenamesDuplicates; Resolve_TrimsAndCutsToTeamSize; SetSynced_FeedsPartyController; Clear_RemovesProvider.
 */
using NUnit.Framework;
using TheReturn.Network.Online;

namespace TheReturn.Tests
{
    public sealed class OnlineNamesTests
    {
        private static readonly string[] Defaults = { "Minh", "An", "Bình", "Chi" };

        [TearDown]
        public void Cleanup() { OnlineNames.Clear(); }

        [Test]
        public void Resolve_UsesPlayerNames()
        {
            string[] names = OnlineNames.Resolve(new[] { "Hùng", "Lan", null, null }, Defaults, 2);
            CollectionAssert.AreEqual(new[] { "Hùng", "Lan" }, names);
        }

        [Test]
        public void Resolve_FallsBackWhenBlankOrMissing()
        {
            string[] names = OnlineNames.Resolve(new[] { "Hùng", "  ", null }, Defaults, 4);
            CollectionAssert.AreEqual(new[] { "Hùng", "An", "Bình", "Chi" }, names);
            CollectionAssert.AreEqual(Defaults, OnlineNames.Resolve(null, Defaults, 4));
            Assert.AreEqual("Người chơi 3", OnlineNames.Resolve(null, null, 3)[2]);
        }

        [Test]
        public void Resolve_RenamesDuplicates()
        {
            string[] names = OnlineNames.Resolve(new[] { "Lan", "lan", "LAN", "Minh" }, Defaults, 4);
            CollectionAssert.AreEqual(new[] { "Lan", "lan (2)", "LAN (3)", "Minh" }, names);
        }

        [Test]
        public void Resolve_TrimsAndCutsToTeamSize()
        {
            string[] names = OnlineNames.Resolve(new[] { "  Hùng  ", "Lan", "Mai", "Tú" }, Defaults, 3);
            CollectionAssert.AreEqual(new[] { "Hùng", "Lan", "Mai" }, names);
        }

        [Test]
        public void SetSynced_FeedsPartyController()
        {
            OnlineNames.SetSynced(new[] { "Hùng", null, "Mai" });
            Assert.AreEqual("Hùng", PrototypePartyController.SlotNameProvider(0));
            Assert.IsNull(PrototypePartyController.SlotNameProvider(1));
            Assert.AreEqual("Mai", OnlineNames.OfSlot(2));
            Assert.IsNull(OnlineNames.OfSlot(9));
        }

        [Test]
        public void Clear_RemovesProvider()
        {
            OnlineNames.SetSynced(new[] { "Hùng" });
            OnlineNames.Clear();
            Assert.IsNull(PrototypePartyController.SlotNameProvider);
            Assert.IsNull(OnlineNames.OfSlot(0));
        }
    }
}
