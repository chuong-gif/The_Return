/*
 * Mục đích: NUnit EditMode test cho MapProgress (T-002): luật chọn khu tự do, hành lang chỉ vào từ phòng kề,
 * một khu đang làm tại một thời điểm, điều kiện thắng, ảnh chụp tiến độ. Chạy qua Test Runner, không cần scene hay Play Mode.
 * Hàm: Make (tạo vòng 6 phòng với 6 nhiệm vụ hiện có); các test Ring_*, Constructor_*, Room_*, Corridor_*, TeamNotReady_*,
 * OnlyOneActiveArea, CheckBegin_*, Complete_*, Abandon_*, NoMission_*, Won_*, Registering_*, Snapshot_*, Reset_*.
 */
using System;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class MapProgressTests
    {
        static readonly string[] MissionAreas =
        {
            "Phong_1", "Phong_2", "Phong_3", "Phong_4", "Hanh_Lang_1", "Hanh_Lang_3"
        };

        /// <summary>Không nhận gì; trả vòng 6 phòng/6 hành lang với 6 khu đã có nhiệm vụ (theo MISSIONS_TODO), 6 khu còn lại chưa có.</summary>
        static MapProgress Make()
        {
            return MapProgress.CreateRing(6, MissionAreas);
        }

        // ---------- Bố cục ----------

        [Test]
        public void Ring_HasTwelveAreasWithCorrectAdjacency()
        {
            MapProgress p = Make();
            Assert.AreEqual(12, p.Areas.Count);
            CollectionAssert.AreEquivalent(new[] { "Hanh_Lang_6", "Hanh_Lang_1" }, p.Find("Phong_1").Neighbors);
            CollectionAssert.AreEquivalent(new[] { "Phong_6", "Phong_1" }, p.Find("Hanh_Lang_6").Neighbors);
            CollectionAssert.AreEquivalent(new[] { "Phong_3", "Phong_4" }, p.Find("Hanh_Lang_3").Neighbors);
            Assert.AreEqual(AreaKind.Room, p.Find("Phong_5").Kind);
            Assert.AreEqual(AreaKind.Corridor, p.Find("Hanh_Lang_5").Kind);
        }

        [Test]
        public void Ring_RejectsFewerThanThreeRooms()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MapProgress.CreateRing(2, null));
        }

        [Test]
        public void Constructor_RejectsDuplicateIds()
        {
            var list = new[]
            {
                new AreaInfo("A", AreaKind.Room, true, new string[0]),
                new AreaInfo("A", AreaKind.Room, true, new string[0])
            };
            Assert.Throws<ArgumentException>(() => new MapProgress(list));
        }

        [Test]
        public void Constructor_RejectsUnknownNeighbor()
        {
            var list = new[] { new AreaInfo("A", AreaKind.Room, true, new[] { "Z" }) };
            Assert.Throws<ArgumentException>(() => new MapProgress(list));
        }

        [Test]
        public void Constructor_RejectsAsymmetricNeighbors()
        {
            var list = new[]
            {
                new AreaInfo("A", AreaKind.Room, true, new[] { "B" }),
                new AreaInfo("B", AreaKind.Corridor, true, new string[0])
            };
            Assert.Throws<ArgumentException>(() => new MapProgress(list));
        }

        // ---------- Chọn khu ----------

        [Test]
        public void Room_CanStartFromYard_InAnyOrder()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_3", MapProgress.Yard, true));
            Assert.IsTrue(p.Complete("Phong_3"));
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.AreEqual("Phong_1", p.ActiveAreaId);
        }

        [Test]
        public void Room_CanStartFromAdjacentCorridorButNotFarOne()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Phong_2", "Hanh_Lang_3", true));
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Phong_2", "Hanh_Lang_1", true));
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Phong_2", "Hanh_Lang_2", true));
        }

        [Test]
        public void Corridor_StartsOnlyFromAdjacentRooms()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Hanh_Lang_1", "Phong_1", true));
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Hanh_Lang_1", "Phong_2", true));
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Hanh_Lang_1", "Phong_3", true));
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Hanh_Lang_1", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Hanh_Lang_1", "Hanh_Lang_2", true));
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Hanh_Lang_1", null, true));
        }

        [Test]
        public void Corridor_ThatWrapsAround_ConnectsLastAndFirstRoom()
        {
            MapProgress p = MapProgress.CreateRing(6, new[] { "Hanh_Lang_6" });
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Hanh_Lang_6", "Phong_6", true));
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Hanh_Lang_6", "Phong_1", true));
            Assert.AreEqual(BeginResult.InvalidEntry, p.CheckBegin("Hanh_Lang_6", "Phong_2", true));
        }

        [Test]
        public void TeamNotReady_BlocksStartAndKeepsStateClean()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.TeamNotReady, p.TryBegin("Phong_1", MapProgress.Yard, false));
            Assert.IsNull(p.ActiveAreaId);
            Assert.AreEqual(0, p.DoneCount);
        }

        [Test]
        public void OnlyOneActiveArea()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.AnotherActive, p.TryBegin("Phong_2", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.AnotherActive, p.TryBegin("Hanh_Lang_1", "Phong_1", true));
            Assert.AreEqual("Phong_1", p.ActiveAreaId);
        }

        [Test]
        public void CheckBegin_DoesNotChangeState()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.Started, p.CheckBegin("Phong_1", MapProgress.Yard, true));
            Assert.IsNull(p.ActiveAreaId);
        }

        [Test]
        public void NoMission_AndUnknownArea_AreRejected()
        {
            MapProgress p = Make();
            Assert.AreEqual(BeginResult.NoMission, p.TryBegin("Phong_5", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.NoMission, p.TryBegin("Hanh_Lang_2", "Phong_2", true));
            Assert.AreEqual(BeginResult.UnknownArea, p.TryBegin("Phong_9", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.UnknownArea, p.TryBegin(null, MapProgress.Yard, true));
            Assert.IsNull(p.ActiveAreaId);
        }

        // ---------- Hoàn thành / bỏ dở ----------

        [Test]
        public void Complete_MarksDoneReleasesAndBlocksRepeat()
        {
            MapProgress p = Make();
            p.TryBegin("Phong_4", MapProgress.Yard, true);
            Assert.IsFalse(p.Complete("Phong_1"), "Không phải khu đang làm");
            Assert.IsTrue(p.Complete("Phong_4"));
            Assert.IsTrue(p.IsDone("Phong_4"));
            Assert.IsNull(p.ActiveAreaId);
            Assert.AreEqual(BeginResult.AlreadyDone, p.TryBegin("Phong_4", MapProgress.Yard, true));
            Assert.IsFalse(p.Complete("Phong_4"), "Không hoàn thành hai lần");
        }

        [Test]
        public void Abandon_ReleasesWithoutMarkingDone_AndKeepsOtherProgress()
        {
            MapProgress p = Make();
            p.TryBegin("Phong_1", MapProgress.Yard, true);
            p.Complete("Phong_1");
            p.TryBegin("Phong_2", MapProgress.Yard, true);
            Assert.IsTrue(p.Abandon("Phong_2"));
            Assert.IsNull(p.ActiveAreaId);
            Assert.IsFalse(p.IsDone("Phong_2"));
            Assert.IsTrue(p.IsDone("Phong_1"));
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_2", MapProgress.Yard, true), "Có thể làm lại");
            Assert.IsFalse(p.Abandon("Phong_3"), "Không phải khu đang làm");
        }

        // ---------- Điều kiện thắng ----------

        [Test]
        public void Won_RequiresEveryRegisteredMission_AndListsMissingAreas()
        {
            MapProgress p = Make();
            CollectionAssert.AreEquivalent(
                new[] { "Phong_5", "Phong_6", "Hanh_Lang_2", "Hanh_Lang_4", "Hanh_Lang_5", "Hanh_Lang_6" },
                p.MissingMissionAreas);
            for (int i = 0; i < MissionAreas.Length; i++)
            {
                Assert.IsFalse(p.Won, "Chưa thắng khi còn " + (MissionAreas.Length - i) + " khu");
                AreaInfo a = p.Find(MissionAreas[i]);
                string entry = a.Kind == AreaKind.Room ? MapProgress.Yard : a.Neighbors[0];
                Assert.AreEqual(BeginResult.Started, p.TryBegin(a.Id, entry, true), a.Id);
                Assert.IsTrue(p.Complete(a.Id));
            }
            Assert.IsTrue(p.Won);
            Assert.AreEqual(6, p.DoneCount);
        }

        [Test]
        public void Won_IsFalseWhenNoMissionRegistered()
        {
            MapProgress p = MapProgress.CreateRing(6, null);
            Assert.IsFalse(p.Won);
            Assert.AreEqual(12, p.MissingMissionAreas.Count);
        }

        [Test]
        public void Registering_NewMission_EntersWinCondition()
        {
            MapProgress p = Make();
            foreach (string id in MissionAreas)
            {
                AreaInfo a = p.Find(id);
                p.TryBegin(id, a.Kind == AreaKind.Room ? MapProgress.Yard : a.Neighbors[0], true);
                p.Complete(id);
            }
            Assert.IsTrue(p.Won);
            Assert.IsTrue(p.SetHasMission("Phong_5", true));
            Assert.IsFalse(p.Won, "Khu mới đăng ký phải làm xong mới thắng");
            Assert.AreEqual(5, p.MissingMissionAreas.Count);
            Assert.IsFalse(p.SetHasMission("Phong_1", false), "Không gỡ nhiệm vụ của khu đã xong");
            Assert.IsFalse(p.SetHasMission("Khong_Co", true));
        }

        // ---------- Ảnh chụp / đặt lại ----------

        [Test]
        public void Snapshot_RoundTrip_ThroughJsonUtility()
        {
            MapProgress p = Make();
            p.TryBegin("Phong_1", MapProgress.Yard, true);
            p.Complete("Phong_1");
            p.TryBegin("Hanh_Lang_1", "Phong_1", true);

            string json = JsonUtility.ToJson(p.ToSnapshot());
            MapProgress q = Make();
            Assert.IsTrue(q.RestoreSnapshot(JsonUtility.FromJson<MapProgressSnapshot>(json)));
            Assert.AreEqual("Hanh_Lang_1", q.ActiveAreaId);
            Assert.IsTrue(q.IsDone("Phong_1"));
            Assert.AreEqual(1, q.DoneCount);
        }

        [Test]
        public void Snapshot_InvalidIsRejectedWithoutChangingState()
        {
            MapProgress p = Make();
            p.TryBegin("Phong_1", MapProgress.Yard, true);
            Assert.IsFalse(p.RestoreSnapshot(null));
            Assert.IsFalse(p.RestoreSnapshot(new MapProgressSnapshot { DoneAreaIds = new[] { "Khong_Co" } }));
            Assert.IsFalse(p.RestoreSnapshot(new MapProgressSnapshot { ActiveAreaId = "Phong_5" }), "Khu chưa có nhiệm vụ");
            Assert.IsFalse(p.RestoreSnapshot(new MapProgressSnapshot { ActiveAreaId = "Phong_2", DoneAreaIds = new[] { "Phong_2" } }), "Vừa đang làm vừa xong");
            Assert.AreEqual("Phong_1", p.ActiveAreaId);
            Assert.AreEqual(0, p.DoneCount);
        }

        [Test]
        public void Reset_ClearsProgressButKeepsMissionFlags()
        {
            MapProgress p = Make();
            p.TryBegin("Phong_1", MapProgress.Yard, true);
            p.Complete("Phong_1");
            p.TryBegin("Phong_2", MapProgress.Yard, true);
            p.Reset();
            Assert.IsNull(p.ActiveAreaId);
            Assert.AreEqual(0, p.DoneCount);
            Assert.IsTrue(p.Find("Phong_1").HasMission);
            Assert.AreEqual(6, p.MissingMissionAreas.Count);
        }
    }
}
