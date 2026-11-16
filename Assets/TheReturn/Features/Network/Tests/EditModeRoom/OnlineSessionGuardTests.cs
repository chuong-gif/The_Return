/*
 * Mục đích: Kiểm cách chọn câu thông báo khi mất kết nối (T-048, OnlineSessionGuard.ChooseNotice).
 */
using NUnit.Framework;
using TheReturn.Network.Room;

namespace TheReturn.Network.Room.Tests
{
    public sealed class OnlineSessionGuardTests
    {
        [Test]
        public void EmptyReason_UsesRoleDefault()
        {
            Assert.AreEqual(OnlineSessionGuard.ClientLostNotice, OnlineSessionGuard.ChooseNotice("", false));
            Assert.AreEqual(OnlineSessionGuard.HostLostNotice, OnlineSessionGuard.ChooseNotice(null, true));
        }

        [Test]
        public void TechnicalReason_IsHidden()
        {
            Assert.AreEqual(OnlineSessionGuard.ClientLostNotice, OnlineSessionGuard.ChooseNotice("[Transport] timeout", false));
        }

        [Test]
        public void HostMessage_IsShownAsIs()
        {
            Assert.AreEqual(OnlineSessionGuard.PeerLeftNotice, OnlineSessionGuard.ChooseNotice(OnlineSessionGuard.PeerLeftNotice, false));
        }
    }
}
