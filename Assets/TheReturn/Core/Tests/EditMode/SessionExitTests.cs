/*
 * Mục đích: Kiểm SessionExit (T-048): về Menu một máy thì nạp thẳng; có mạng thì rời phòng xong mới nạp; tên scene trống thì bỏ qua.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class SessionExitTests
    {
        Func<bool> oldOnline;
        Action<string, Action> oldLeave;
        Action<string> oldLoad;

        [SetUp]
        public void SetUp()
        {
            oldOnline = SessionExit.IsOnline;
            oldLeave = SessionExit.LeaveOnline;
            oldLoad = SessionExit.LoadMenu;
            SessionExit.IsOnline = null;
            SessionExit.LeaveOnline = null;
            SessionExit.GameFinished = true;
            SessionExit.MenuNotice = "";
        }

        [TearDown]
        public void TearDown()
        {
            SessionExit.IsOnline = oldOnline;
            SessionExit.LeaveOnline = oldLeave;
            SessionExit.LoadMenu = oldLoad;
            SessionExit.GameFinished = false;
            SessionExit.MenuNotice = "";
        }

        [Test]
        public void Offline_LoadsMenuImmediately_AndClearsGameFinished()
        {
            string loaded = null;
            SessionExit.LoadMenu = s => loaded = s;
            SessionExit.ReturnToMenu("MainMenu");
            Assert.AreEqual("MainMenu", loaded);
            Assert.IsFalse(SessionExit.GameFinished);
        }

        [Test]
        public void Online_LeavesFirst_ThenLoadsMenu()
        {
            string loaded = null;
            string leaveNotice = null;
            Action then = null;
            SessionExit.LoadMenu = s => loaded = s;
            SessionExit.IsOnline = () => true;
            SessionExit.LeaveOnline = (n, a) => { leaveNotice = n; then = a; };
            SessionExit.ReturnToMenu("MainMenu", "Có người rời");
            Assert.IsNull(loaded, "Chưa được nạp Menu khi chưa rời xong");
            Assert.AreEqual("Có người rời", leaveNotice);
            Assert.AreEqual("Có người rời", SessionExit.MenuNotice);
            then();
            Assert.AreEqual("MainMenu", loaded);
            Assert.IsFalse(SessionExit.GameFinished);
        }

        [Test]
        public void OnlineHookButNoNetwork_LoadsDirectly()
        {
            string loaded = null;
            bool left = false;
            SessionExit.LoadMenu = s => loaded = s;
            SessionExit.IsOnline = () => false;
            SessionExit.LeaveOnline = (n, a) => left = true;
            SessionExit.ReturnToMenu("MainMenu");
            Assert.IsFalse(left);
            Assert.AreEqual("MainMenu", loaded);
        }

        [Test]
        public void BlankSceneName_DoesNothing()
        {
            string loaded = null;
            SessionExit.LoadMenu = s => loaded = s;
            SessionExit.ReturnToMenu("   ", "x");
            Assert.IsNull(loaded);
            Assert.AreEqual("", SessionExit.MenuNotice);
        }
    }
}
