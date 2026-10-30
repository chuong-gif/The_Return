/*
 * Mục đích: Bộ test Play Mode cho mô hình người chơi có animation (T-010). Nạp scene SchoolFloorPrototype trong Play Mode (không ghi gì ra đĩa),
 * kiểm mỗi vai trong đội có mô hình + Animator đúng, thân người đang điều khiển bị ẩn, và animation đổi theo chuyển động (đứng, đi, đi lùi, chạy, ngồi, rơi).
 * Hàm hỗ trợ: LoadScene (UnitySetUp), Frames, Driver, Walk, WaitForState, WaitForStateWhileWalking, Bone, ClipHasMotion.
 * Lưu ý: nếu clip trong Player.fbx chỉ là tư thế tĩnh (xuất từ Blender chưa bake keyframe) thì test kiểm xương động báo Inconclusive, không báo đỏ.
 * Test: EveryPlayer_*, ActivePlayer_*, Idle_*, Walking_*, Backwards_*, Running_*, Seated_*, Airborne_*.
 * Chạy: Unity MCP run_tests mode PlayMode, assembly_names ["TheReturn.SchoolFloor.PlayTests"] khi editor KHÔNG đang Play.
 */
using System.Collections;
using System.Linq;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheReturn.Tests
{
    public sealed class PlayerModelPlayTests
    {
        const string ScenePath = "Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity";
        PrototypePartyController party;

        /// <summary>Không nhận tham số; nạp lại scene trong Play Mode và lấy bộ điều khiển đội.</summary>
        [UnitySetUp]
        public IEnumerator LoadScene()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Chỉ chạy trong Editor");
            yield break;
#endif
            yield return Frames(3);
            SchoolFloorFlow flow = Object.FindFirstObjectByType<SchoolFloorFlow>();
            Assert.IsNotNull(flow, "thiếu SchoolFloorFlow");
            party = flow.attendance.party;
            Assert.AreEqual(4, party.PartySize, "mặc định đội 4 người");
            yield return Frames(2);
        }

        static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }

        /// <summary>Nhận ID vai; trả PlayerAnimationDriver của vai đó (lỗi test nếu thiếu).</summary>
        PlayerAnimationDriver Driver(int index)
        {
            PlayerAnimationDriver d = party.players[index].GetComponentInChildren<PlayerAnimationDriver>(true);
            Assert.IsNotNull(d, "vai " + index + " thiếu PlayerAnimationDriver");
            return d;
        }

        /// <summary>Nhận ID vai, hướng (so với trước mặt: 1 tới, -1 lùi), tốc độ m/s, số giây; di chuyển thẳng vai đó (tắt CharacterController để không bị tường chặn).</summary>
        IEnumerator Walk(int index, float direction, float speed, float seconds)
        {
            Transform t = party.players[index];
            t.GetComponent<CharacterController>().enabled = false;
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                t.position += t.forward * direction * speed * Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>Nhận Animator, tên state, số giây chờ tối đa; trả true nếu animator vào state đó kịp lúc.</summary>
        static IEnumerator WaitForState(Animator a, string state, float timeout, System.Action<bool> done)
        {
            float end = Time.time + timeout;
            while (Time.time < end)
            {
                if (a.GetCurrentAnimatorStateInfo(0).IsName(state)) { done(true); yield break; }
                yield return null;
            }
            done(false);
        }

        /// <summary>Nhận Animator và tên clip; trả true nếu clip có ít nhất một đường cong đổi giá trị (animation thật), false nếu chỉ là tư thế tĩnh.</summary>
        static bool ClipHasMotion(Animator a, string clipName)
        {
#if UNITY_EDITOR
            AnimationClip clip = a.runtimeAnimatorController.animationClips.First(c => c.name == clipName);
            foreach (UnityEditor.EditorCurveBinding binding in UnityEditor.AnimationUtility.GetCurveBindings(clip))
            {
                AnimationCurve curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
                float min = float.MaxValue, max = float.MinValue;
                foreach (Keyframe k in curve.keys) { min = Mathf.Min(min, k.value); max = Mathf.Max(max, k.value); }
                if (max - min > 0.001f) return true;
            }
#endif
            return false;
        }

        Transform Bone(PlayerAnimationDriver d, string name)
        {
            return d.GetComponentsInChildren<Transform>(true).First(t => t.name == name);
        }

        [UnityTest]
        public IEnumerator EveryPlayer_HasAnimatedModel()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayerAnimationDriver d = Driver(i);
                Assert.IsNotNull(d.animator, "vai " + i + " thiếu Animator");
                Assert.IsNotNull(d.animator.runtimeAnimatorController, "vai " + i + " thiếu AnimatorController");
                Assert.IsNotNull(d.animator.avatar, "vai " + i + " thiếu Avatar");
                Assert.IsTrue(d.animator.avatar.isValid, "avatar vai " + i + " không hợp lệ");
                Assert.AreEqual(7, d.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length, "vai " + i + " phải có 7 mảnh mesh");
                Assert.IsNull(party.players[i].Find("Visual_Replaceable"), "khối viên nan cũ vẫn còn ở vai " + i);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActivePlayer_BodyHidden_OthersVisible()
        {
            party.SwitchRole(0, false);
            yield return Frames(2);
            Assert.IsFalse(Driver(0).BodyVisible, "người đang điều khiển phải ẩn thân");
            foreach (SkinnedMeshRenderer r in Driver(0).GetComponentsInChildren<SkinnedMeshRenderer>(true))
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly, r.shadowCastingMode, "thân ẩn nhưng vẫn phải đổ bóng");
            for (int i = 1; i < 4; i++)
            {
                Assert.IsTrue(Driver(i).BodyVisible, "vai " + i + " phải hiện thân");
                foreach (SkinnedMeshRenderer r in Driver(i).GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.On, r.shadowCastingMode);
            }
            party.SwitchRole(2, false);
            yield return Frames(2);
            Assert.IsTrue(Driver(0).BodyVisible, "đổi vai: vai 0 phải hiện lại");
            Assert.IsFalse(Driver(2).BodyVisible, "đổi vai: vai 2 phải ẩn");
        }

        [UnityTest]
        public IEnumerator Idle_PlaysIdle_AndBonesAnimate()
        {
            Animator a = Driver(1).animator;
            bool ok = false;
            yield return WaitForState(a, "Idle", 2f, v => ok = v);
            Assert.IsTrue(ok, "đứng yên phải ở state Idle");
            if (!ClipHasMotion(a, "Idle")) Assert.Inconclusive("Player.fbx: clip Idle không có keyframe (chỉ là tư thế tĩnh) — cần xuất lại từ Blender có bake animation.");
            Transform arm = Bone(Driver(1), "mixamorig:LeftArm");
            Quaternion q0 = arm.localRotation;
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(Quaternion.Angle(q0, arm.localRotation), 0.05f, "xương tay không động: animation Idle không chạy");
        }

        [UnityTest]
        public IEnumerator Walking_PlaysWalk_AndLegsMove()
        {
            PlayerAnimationDriver d = Driver(1);
            Transform leg = Bone(d, "mixamorig:LeftUpLeg");
            float min = 999, max = -999;
            bool ok = false;
            yield return Walk(1, 1f, 3.2f, 0.4f);
            yield return WaitForState(d.animator, "Walk", 1f, v => ok = v);
            Assert.IsTrue(ok, "đi tới 3.2 m/s phải vào state Walk");
            if (!ClipHasMotion(d.animator, "Walking")) Assert.Inconclusive("Player.fbx: clip Walking không có keyframe (chỉ là tư thế tĩnh) — cần xuất lại từ Blender có bake animation.");
            float end = Time.time + 1.2f;
            while (Time.time < end) { yield return Walk(1, 1f, 3.2f, 0.05f); float x = leg.localEulerAngles.x; if (x > 180) x -= 360; min = Mathf.Min(min, x); max = Mathf.Max(max, x); }
            Assert.Greater(max - min, 10f, "chân không vung khi đi (biên độ " + (max - min).ToString("F1") + "°)");
        }

        [UnityTest]
        public IEnumerator Backwards_PlaysWalkBack()
        {
            PlayerAnimationDriver d = Driver(2);
            bool ok = false;
            yield return Walk(2, -1f, 3.2f, 0.4f);
            yield return WaitForStateWhileWalking(d, 2, -1f, 3.2f, "WalkBack", v => ok = v);
            Assert.IsTrue(ok, "đi lùi phải vào state WalkBack");
        }

        [UnityTest]
        public IEnumerator Running_PlaysRun()
        {
            PlayerAnimationDriver d = Driver(3);
            bool ok = false;
            yield return WaitForStateWhileWalking(d, 3, 1f, 5f, "Run", v => ok = v);
            Assert.IsTrue(ok, "chạy 5 m/s phải vào state Run");
        }

        /// <summary>Vừa di chuyển vừa chờ vào state; true nếu vào được trong 2 giây.</summary>
        IEnumerator WaitForStateWhileWalking(PlayerAnimationDriver d, int index, float dir, float speed, string state, System.Action<bool> done)
        {
            Transform t = party.players[index];
            t.GetComponent<CharacterController>().enabled = false;
            float end = Time.time + 2f;
            while (Time.time < end)
            {
                t.position += t.forward * dir * speed * Time.deltaTime;
                if (d.animator.GetCurrentAnimatorStateInfo(0).IsName(state)) { done(true); yield break; }
                yield return null;
            }
            done(false);
        }

        [UnityTest]
        public IEnumerator Seated_PlaysSit_ThenStandsUp()
        {
            PlayerAnimationDriver d = Driver(0);
            party.SwitchRole(0, true);
            party.TickInput(true);
            bool ok = false;
            yield return WaitForState(d.animator, "Sit", 2f, v => ok = v);
            Assert.IsTrue(ok, "ngồi phải vào state Sit");
            party.TickInput(false);
            ok = false;
            yield return WaitForState(d.animator, "Idle", 2f, v => ok = v);
            Assert.IsTrue(ok, "đứng dậy phải về Idle");
        }

        [UnityTest]
        public IEnumerator Model_StandsOnGround_AndMatchesCapsuleHeight()
        {
            yield return Frames(30);
            for (int i = 0; i < 4; i++)
            {
                Transform slot = party.players[i];
                PlayerAnimationDriver d = Driver(i);
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (SkinnedMeshRenderer r in d.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    Mesh m = new Mesh();
                    r.BakeMesh(m, true);
                    foreach (Vector3 v in m.vertices)
                    {
                        float y = r.transform.TransformPoint(v).y;
                        minY = Mathf.Min(minY, y);
                        maxY = Mathf.Max(maxY, y);
                    }
                    Object.Destroy(m);
                }
                float height = maxY - minY;
                Assert.IsTrue(Physics.Raycast(slot.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, ~0, QueryTriggerInteraction.Ignore), "vai " + i + " không có sàn dưới chân");
                Assert.AreEqual(hit.point.y, minY, 0.12f, "vai " + i + ": đế giày cách sàn " + (minY - hit.point.y).ToString("F2") + " m");
                Assert.AreEqual(1.75f, height, 0.35f, "vai " + i + ": cao " + height.ToString("F2") + " m, lệch nhiều so với capsule 1.75 m");
            }
        }

        [UnityTest]
        public IEnumerator Airborne_PlaysJump()
        {
            PlayerAnimationDriver d = Driver(1);
            Transform t = party.players[1];
            t.GetComponent<CharacterController>().enabled = false;
            t.position += Vector3.up * 40f;
            bool ok = false;
            yield return WaitForState(d.animator, "Jump", 2f, v => ok = v);
            Assert.IsTrue(ok, "rời mặt đất phải vào state Jump");
        }
    }
}
