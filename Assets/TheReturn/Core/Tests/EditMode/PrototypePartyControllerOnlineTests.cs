/*
 * Mục đích: NUnit EditMode test cho CHẾ ĐỘ ONLINE của PrototypePartyController (T-025): chế độ 1 máy giữ nguyên, online chỉ điều khiển
 * đúng một ô cố định, gắn/gỡ nhân vật spawn qua mạng, camera không mồ côi khi nhân vật bị gỡ.
 * Hàm: Build/TearDown (trợ giúp); các test Online_*. Không cần scene hay Play Mode (dựng đối tượng tạm trong test).
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PrototypePartyControllerOnlineTests
    {
        readonly List<GameObject> created = new List<GameObject>();
        PrototypePartyController controller;
        Transform[] avatars;

        [SetUp]
        public void Build()
        {
            GameObject host = Make("controller");
            controller = host.AddComponent<PrototypePartyController>();
            GameObject cameraObject = Make("camera");
            controller.viewCamera = cameraObject.AddComponent<Camera>();
            controller.players = new Transform[4];
            controller.bodies = new Renderer[0];
            avatars = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject avatar = Make("avatar" + i);
                avatar.AddComponent<CharacterController>();
                avatar.transform.position = new Vector3(i * 2f, 0f, 0f);
                avatars[i] = avatar.transform;
            }
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject go in created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            created.Clear();
        }

        GameObject Make(string name)
        {
            GameObject go = new GameObject(name);
            created.Add(go);
            return go;
        }

        [Test]
        public void Online_DefaultIsOffline()
        {
            Assert.IsFalse(controller.IsOnline);
            Assert.AreEqual(-1, controller.OnlineLocalSlot);
        }

        [Test]
        public void Online_OfflineModeStillSwitchesBetweenAllRoles()
        {
            controller.players = avatars;
            controller.Initialize();
            controller.SetPartySize(4);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(controller.SwitchRole(i, false));
                Assert.AreEqual(i, controller.ActivePlayer);
            }
        }

        [Test]
        public void Online_InitializeAndSetPartySizeTolerateEmptySlots()
        {
            controller.Initialize();
            Assert.DoesNotThrow(() => controller.SetPartySize(3));
            Assert.DoesNotThrow(() => controller.ResetPositions());
        }

        [Test]
        public void Online_BindSlot_RejectsSlotOutsideList()
        {
            Assert.IsFalse(controller.BindSlot(-1, avatars[0]));
            Assert.IsFalse(controller.BindSlot(4, avatars[0]));
            Assert.IsTrue(controller.BindSlot(0, avatars[0]));
        }

        [Test]
        public void Online_EnterOnlineMode_ControlsOnlyTheLocalSlot()
        {
            for (int i = 0; i < 4; i++) controller.BindSlot(i, avatars[i]);
            Assert.IsTrue(controller.EnterOnlineMode(2, 4));
            Assert.IsTrue(controller.IsOnline);
            Assert.AreEqual(2, controller.OnlineLocalSlot);
            Assert.AreEqual(2, controller.ActivePlayer);
            Assert.AreEqual(avatars[2], controller.viewCamera.transform.parent);

            Assert.IsFalse(controller.SwitchRole(1, false));
            Assert.IsFalse(controller.SwitchRole(3, false));
            Assert.AreEqual(2, controller.ActivePlayer);
            Assert.AreEqual(avatars[2], controller.viewCamera.transform.parent);
            Assert.IsTrue(controller.SwitchRole(2, true));
        }

        [TestCase(2, 0)]
        [TestCase(3, 2)]
        [TestCase(4, 3)]
        public void Online_EnterOnlineMode_WorksForTeamsOfTwoThreeFour(int teamSize, int localSlot)
        {
            for (int i = 0; i < teamSize; i++) controller.BindSlot(i, avatars[i]);
            Assert.IsTrue(controller.EnterOnlineMode(localSlot, teamSize));
            Assert.AreEqual(teamSize, controller.PartySize);
            Assert.AreEqual(localSlot, controller.ActivePlayer);
        }

        [Test]
        public void Online_EnterOnlineMode_FailsWithoutBoundAvatarAndKeepsOfflineMode()
        {
            controller.BindSlot(0, avatars[0]);
            Assert.IsFalse(controller.EnterOnlineMode(1, 2));
            Assert.IsFalse(controller.IsOnline);
            Assert.IsFalse(controller.EnterOnlineMode(9, 4));
            Assert.IsFalse(controller.IsOnline);
        }

        [Test]
        public void Online_EnterOnlineMode_SlotBeyondTeamSizeIsRejected()
        {
            for (int i = 0; i < 4; i++) controller.BindSlot(i, avatars[i]);
            Assert.IsFalse(controller.EnterOnlineMode(3, 2));
            Assert.IsFalse(controller.IsOnline);
        }

        [Test]
        public void Online_ExitOnlineMode_RestoresRoleSwitching()
        {
            for (int i = 0; i < 4; i++) controller.BindSlot(i, avatars[i]);
            controller.EnterOnlineMode(1, 4);
            controller.ExitOnlineMode();
            Assert.IsFalse(controller.IsOnline);
            Assert.IsTrue(controller.SwitchRole(3, false));
            Assert.AreEqual(3, controller.ActivePlayer);
        }

        [Test]
        public void Online_UnbindingLocalAvatar_ReleasesCameraSoItIsNotDestroyedWithIt()
        {
            for (int i = 0; i < 2; i++) controller.BindSlot(i, avatars[i]);
            controller.EnterOnlineMode(1, 2);
            Assert.AreEqual(avatars[1], controller.viewCamera.transform.parent);
            Assert.IsTrue(controller.BindSlot(1, null));
            Assert.IsNull(controller.viewCamera.transform.parent);
            Assert.DoesNotThrow(() => controller.TickInput(false));
        }

        [Test]
        public void Online_TickInputBeforeAnyAvatarIsBound_DoesNothing()
        {
            controller.Initialize();
            controller.SetPartySize(2);
            Assert.DoesNotThrow(() => controller.TickInput(false));
        }
    }
}
