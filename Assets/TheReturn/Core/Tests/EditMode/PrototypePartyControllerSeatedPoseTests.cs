/*
 * Mục đích: NUnit EditMode test cho cờ pose ngồi của PrototypePartyController.TickInput (T-033): ghế thật vẫn phát pose ngồi, còn đứng giữ nút
 * (khóa đi lại nhưng không phải ghế) thì KHÔNG phát pose ngồi. Không cần scene hay Play Mode.
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PrototypePartyControllerSeatedPoseTests
    {
        readonly List<GameObject> created = new List<GameObject>();
        PrototypePartyController controller;
        PlayerAnimationDriver driver;

        [SetUp]
        public void Build()
        {
            GameObject host = new GameObject("controller");
            created.Add(host);
            controller = host.AddComponent<PrototypePartyController>();
            GameObject avatar = new GameObject("avatar");
            created.Add(avatar);
            avatar.AddComponent<CharacterController>();
            GameObject model = new GameObject("model");
            model.transform.SetParent(avatar.transform, false);
            driver = model.AddComponent<PlayerAnimationDriver>();
            controller.players = new Transform[] { avatar.transform };
            controller.bodies = new Renderer[0];
            controller.Initialize();
            controller.SetPartySize(1);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject go in created) if (go != null) Object.DestroyImmediate(go);
            created.Clear();
        }

        [Test]
        public void TickInput_RealSeat_PlaysSeatedPose()
        {
            controller.TickInput(true);
            Assert.IsTrue(driver.Seated);
        }

        [Test]
        public void TickInput_HoldingButtonWithoutSeatPose_DoesNotPlaySeatedPose()
        {
            controller.TickInput(true, false);
            Assert.IsFalse(driver.Seated, "đứng giữ nút không phải ghế nên không được phát pose ngồi");
        }

        [Test]
        public void TickInput_StandingUp_ClearsSeatedPose()
        {
            controller.TickInput(true);
            controller.TickInput(false);
            Assert.IsFalse(driver.Seated);
        }
    }
}
