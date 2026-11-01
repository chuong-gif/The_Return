/*
 * Mục đích: NUnit test cho CHẾ ĐỘ ONLINE của PrototypePartyController (T-025): mặc định chế độ 1 máy giữ nguyên (đổi vai tự do),
 * bật online thì chỉ điều khiển đúng một ô cố định, ô chưa có nhân vật không làm lỗi, gỡ nhân vật thì camera được thả ra.
 * Dựng nhân vật giả bằng code (CharacterController + camera), không cần scene.
 * Hàm: Build/TearDown (trợ giúp); các test Party_*.
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PartyControllerOnlineModeTests
    {
        readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            created.Clear();
        }

        /// <summary>Nhận số ô, và số ô thật sự có nhân vật (các ô sau đó để null). Trả bộ điều khiển đã Initialize.</summary>
        PrototypePartyController Build(int slots, int bound)
        {
            GameObject host = new GameObject("PartyControllerUnderTest");
            created.Add(host);
            PrototypePartyController controller = host.AddComponent<PrototypePartyController>();
            GameObject cameraObject = new GameObject("Cam");
            created.Add(cameraObject);
            controller.viewCamera = cameraObject.AddComponent<Camera>();
            controller.players = new Transform[slots];
            controller.bodies = new Renderer[0];
            for (int i = 0; i < bound; i++) controller.players[i] = NewPlayer(i).transform;
            controller.Initialize();
            return controller;
        }

        GameObject NewPlayer(int index)
        {
            GameObject player = new GameObject("Player_" + index);
            created.Add(player);
            player.transform.position = new Vector3(index * 2f, 0f, 0f);
            player.AddComponent<CharacterController>();
            return player;
        }

        [Test]
        public void Party_DefaultIsOfflineAndRolesSwitchFreely()
        {
            PrototypePartyController controller = Build(4, 4);
            controller.SetPartySize(3);
            Assert.IsFalse(controller.IsOnline);
            Assert.AreEqual(-1, controller.OnlineLocalSlot);
            Assert.IsTrue(controller.SwitchRole(2, false));
            Assert.IsTrue(controller.SwitchRole(0, false));
            Assert.IsFalse(controller.SwitchRole(3, false));
        }

        [TestCase(2, 0)]
        [TestCase(3, 2)]
        [TestCase(4, 3)]
        public void Party_EnterOnlineMode_ControlsOnlyTheLocalSlot(int teamSize, int localSlot)
        {
            PrototypePartyController controller = Build(4, teamSize);
            Assert.IsTrue(controller.EnterOnlineMode(localSlot, teamSize));
            Assert.IsTrue(controller.IsOnline);
            Assert.AreEqual(localSlot, controller.OnlineLocalSlot);
            Assert.AreEqual(localSlot, controller.ActivePlayer);
            Assert.AreEqual(teamSize, controller.PartySize);
            Assert.AreEqual(controller.players[localSlot], controller.viewCamera.transform.parent);
            for (int other = 0; other < teamSize; other++)
            {
                if (other == localSlot) continue;
                Assert.IsFalse(controller.SwitchRole(other, false), "không được đổi sang ô " + other);
            }
            Assert.AreEqual(localSlot, controller.ActivePlayer);
            Assert.IsTrue(controller.SwitchRole(localSlot, false));
        }

        [Test]
        public void Party_ExitOnlineMode_RestoresFreeRoleSwitching()
        {
            PrototypePartyController controller = Build(4, 3);
            controller.EnterOnlineMode(1, 3);
            controller.ExitOnlineMode();
            Assert.IsFalse(controller.IsOnline);
            Assert.IsTrue(controller.SwitchRole(0, false));
            Assert.IsTrue(controller.SwitchRole(2, false));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Party_EnterOnlineMode_InvalidSlot_StaysOffline(int slot)
        {
            PrototypePartyController controller = Build(4, 4);
            Assert.IsFalse(controller.EnterOnlineMode(slot, 4));
            Assert.IsFalse(controller.IsOnline);
        }

        [Test]
        public void Party_EnterOnlineMode_SlotWithoutCharacter_FailsAndKeepsPreviousMode()
        {
            PrototypePartyController controller = Build(4, 2);
            Assert.IsFalse(controller.EnterOnlineMode(3, 4));
            Assert.IsFalse(controller.IsOnline);
            Assert.IsTrue(controller.EnterOnlineMode(1, 2));
            Assert.IsFalse(controller.EnterOnlineMode(3, 4));
            Assert.AreEqual(1, controller.OnlineLocalSlot);
            Assert.AreEqual(2, controller.PartySize);
        }

        [Test]
        public void Party_BindSlotLater_AllowsEnteringOnlineMode()
        {
            PrototypePartyController controller = Build(4, 0);
            Assert.IsFalse(controller.EnterOnlineMode(2, 3));
            Assert.IsTrue(controller.BindSlot(2, NewPlayer(2).transform));
            Assert.IsTrue(controller.EnterOnlineMode(2, 3));
            Assert.AreEqual(2, controller.ActivePlayer);
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Party_BindSlot_InvalidIndex_ReturnsFalse(int index)
        {
            PrototypePartyController controller = Build(4, 0);
            Assert.IsFalse(controller.BindSlot(index, null));
        }

        [Test]
        public void Party_UnbindLocalSlot_ReleasesCameraAndTickInputIsSafe()
        {
            PrototypePartyController controller = Build(4, 3);
            controller.EnterOnlineMode(1, 3);
            Assert.AreEqual(controller.players[1], controller.viewCamera.transform.parent);
            Assert.IsTrue(controller.BindSlot(1, null));
            Assert.IsNull(controller.viewCamera.transform.parent);
            Assert.DoesNotThrow(() => controller.TickInput(false));
        }

        [Test]
        public void Party_EmptySlotsDoNotBreakInitializeOrReset()
        {
            PrototypePartyController controller = Build(4, 2);
            Vector3 moved = new Vector3(9f, 0f, 9f);
            controller.Teleport(1, moved);
            Assert.DoesNotThrow(() => controller.ResetPositions());
            Assert.DoesNotThrow(() => controller.SetPartySize(4));
            Assert.AreEqual(new Vector3(2f, 0f, 0f), controller.players[1].position);
        }

        [Test]
        public void Party_TickInputWithNoCharactersDoesNotThrow()
        {
            PrototypePartyController controller = Build(4, 0);
            controller.SetPartySize(4);
            Assert.DoesNotThrow(() => controller.TickInput(false));
        }
    }
}
