/*
 * Mục đích: NUnit EditMode test cho tia tương tác của PrototypePartyController (T-033): đồng đội đứng chắn giữa camera và nút KHÔNG được
 * chặn tương tác, còn tường thật vẫn chặn. Không cần scene hay Play Mode (dựng đối tượng tạm trong test).
 * Hàm: Build/Cleanup (trợ giúp); IsPartyBody_*; TryGetInteractionHit_*.
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class PrototypePartyControllerInteractionTests
    {
        readonly List<GameObject> created = new List<GameObject>();
        PrototypePartyController controller;
        Transform[] avatars;
        Collider button;

        [SetUp]
        public void Build()
        {
            controller = Make("controller").AddComponent<PrototypePartyController>();
            GameObject cameraObject = Make("camera");
            cameraObject.transform.position = new Vector3(0f, 1.6f, 0f);
            controller.viewCamera = cameraObject.AddComponent<Camera>();
            controller.bodies = new Renderer[0];
            controller.players = new Transform[4];
            avatars = new Transform[4];
            Vector3[] spots = { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1.5f), new Vector3(5f, 0f, 0f), new Vector3(-5f, 0f, 0f) };
            for (int i = 0; i < 4; i++)
            {
                GameObject avatar = Make("avatar" + i);
                avatar.transform.position = spots[i];
                GameObject body = new GameObject("body");
                body.transform.SetParent(avatar.transform, false);
                CapsuleCollider capsule = body.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 1f, 0f);
                capsule.height = 2f;
                capsule.radius = 0.4f;
                avatars[i] = avatar.transform;
                controller.players[i] = avatar.transform;
            }
            GameObject buttonObject = Make("button");
            buttonObject.transform.position = new Vector3(0f, 1.6f, 3f);
            BoxCollider box = buttonObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 1f, 0.2f);
            button = box;
            Physics.SyncTransforms();
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject go in created) if (go != null) Object.DestroyImmediate(go);
            created.Clear();
        }

        GameObject Make(string name)
        {
            GameObject go = new GameObject(name);
            created.Add(go);
            return go;
        }

        [Test]
        public void IsPartyBody_IncludesEveryPlayerAndTheirChildren()
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(controller.IsPartyBody(avatars[i]), "avatar " + i);
                Assert.IsTrue(controller.IsPartyBody(avatars[i].GetChild(0)), "thân của avatar " + i);
            }
        }

        [Test]
        public void IsPartyBody_ExcludesOtherObjectsAndNull()
        {
            Assert.IsFalse(controller.IsPartyBody(button.transform));
            Assert.IsFalse(controller.IsPartyBody(null));
        }

        [Test]
        public void IsPartyBody_SkipsEmptySlots()
        {
            controller.players[2] = null;
            Assert.IsFalse(controller.IsPartyBody(button.transform));
            Assert.IsTrue(controller.IsPartyBody(avatars[1]));
        }

        [Test]
        public void TryGetInteractionHit_TeammateStandingInFront_StillFindsButton()
        {
            // đồng đội (avatar1) đứng ngay trước mặt người đang điều khiển (avatar0), nút ở xa hơn
            Assert.IsTrue(controller.TryGetInteractionHit(6f, out RaycastHit hit));
            Assert.AreSame(button, hit.collider, "tia phải xuyên qua đồng đội và trúng nút, trúng: " + hit.collider.name);
        }

        [Test]
        public void TryGetInteractionHit_WallInFront_StillBlocks()
        {
            GameObject wallObject = Make("wall");
            wallObject.transform.position = new Vector3(0f, 1.6f, 2.2f);
            BoxCollider wall = wallObject.AddComponent<BoxCollider>();
            wall.size = new Vector3(4f, 4f, 0.2f);
            Physics.SyncTransforms();
            Assert.IsTrue(controller.TryGetInteractionHit(6f, out RaycastHit hit));
            Assert.AreSame(wall, hit.collider, "tường thật vẫn phải chặn tia");
        }

        [Test]
        public void TryGetInteractionHit_NothingInRange_ReturnsFalse()
        {
            Assert.IsFalse(controller.TryGetInteractionHit(1f, out RaycastHit hit));
        }
    }
}
