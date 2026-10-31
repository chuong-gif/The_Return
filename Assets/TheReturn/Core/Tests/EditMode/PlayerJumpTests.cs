/*
 * Mục đích: Test EditMode cho nhảy của người chơi (T-010): vận tốc đứng do PrototypePartyController.NextFallSpeed tính.
 * Test: Grounded_NoJump_StaysDown, Grounded_Jump_ReachesConfiguredHeight, Airborne_JumpPressed_NoDoubleJump, Falling_AcceleratesDownward.
 */
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PlayerJumpTests
    {
        [Test]
        public void Grounded_NoJump_StaysDown()
        {
            Assert.AreEqual(-2f, PrototypePartyController.NextFallSpeed(5f, true, false, 0.8f, 0.016f), 1e-4f);
        }

        [Test]
        public void Grounded_Jump_ReachesConfiguredHeight()
        {
            float v = PrototypePartyController.NextFallSpeed(-2f, true, true, 0.8f, 1f / 60f);
            Assert.Greater(v, 0f, "nhảy phải đi lên");
            float y = 0f, max = 0f;
            for (int i = 0; i < 600 && (i == 0 || y > 0f); i++)
            {
                y += v * (1f / 60f);
                max = Mathf.Max(max, y);
                v = PrototypePartyController.NextFallSpeed(v, false, false, 0.8f, 1f / 60f);
            }
            Assert.AreEqual(0.8f, max, 0.06f, "độ cao tối đa lệch cấu hình");
            Assert.LessOrEqual(y, 0f, "phải rơi về đất");
        }

        [Test]
        public void Airborne_JumpPressed_NoDoubleJump()
        {
            float v = PrototypePartyController.NextFallSpeed(1f, false, true, 0.8f, 0.1f);
            Assert.Less(v, 1f, "đang ở trên không không được nhảy tiếp");
        }

        [Test]
        public void Falling_AcceleratesDownward()
        {
            float v = PrototypePartyController.NextFallSpeed(-2f, false, false, 0.8f, 0.5f);
            Assert.AreEqual(-2f - PrototypePartyController.Gravity * 0.5f, v, 1e-4f);
        }
    }
}
