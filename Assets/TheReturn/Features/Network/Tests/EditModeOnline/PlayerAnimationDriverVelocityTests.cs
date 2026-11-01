/*
 * Mục đích: NUnit test cho PlayerAnimationDriver.HorizontalVelocity (T-027): người được ván chở đi (SuppressLocomotion) phải có vận tốc 0
 * để không phát animation đi tại chỗ; đi bộ bình thường vẫn ra đúng vận tốc; dịch chuyển tức thời và độ dời dọc không tính là chạy.
 * Hàm: các test Velocity_*. Không cần scene hay Play Mode.
 */
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PlayerAnimationDriverVelocityTests
    {
        [Test]
        public void Velocity_NormalWalkingIsMeasured()
        {
            Vector3 v = PlayerAnimationDriver.HorizontalVelocity(new Vector3(0.1f, 0f, 0f), 0.05f, false);
            Assert.AreEqual(2f, v.magnitude, 0.001f);
        }

        [Test]
        public void Velocity_RiderCarriedByMovingDeckIsZero()
        {
            // Ván chạy ~1,8 m/s: không có cờ thì bị đọc thành đi bộ, có cờ thì đứng yên.
            Vector3 carried = new Vector3(0f, 0f, 1.8f * 0.016f);
            Assert.Greater(PlayerAnimationDriver.HorizontalVelocity(carried, 0.016f, false).magnitude, 0.15f);
            Assert.AreEqual(Vector3.zero, PlayerAnimationDriver.HorizontalVelocity(carried, 0.016f, true));
        }

        [Test]
        public void Velocity_VerticalMotionIsIgnored()
        {
            Vector3 v = PlayerAnimationDriver.HorizontalVelocity(new Vector3(0f, 0.5f, 0f), 0.1f, false);
            Assert.AreEqual(0f, v.magnitude, 0.0001f);
        }

        [Test]
        public void Velocity_InstantTeleportIsNotRunning()
        {
            Assert.AreEqual(Vector3.zero, PlayerAnimationDriver.HorizontalVelocity(new Vector3(5f, 0f, 0f), 0.016f, false));
        }

        [TestCase(0f)]
        [TestCase(-0.01f)]
        public void Velocity_NonPositiveFrameTimeIsZero(float dt)
        {
            Assert.AreEqual(Vector3.zero, PlayerAnimationDriver.HorizontalVelocity(new Vector3(1f, 0f, 1f), dt, false));
        }

        [Test]
        public void Suppress_DefaultsToOff()
        {
            GameObject go = new GameObject("driver");
            try
            {
                Assert.IsFalse(go.AddComponent<PlayerAnimationDriver>().SuppressLocomotion);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
