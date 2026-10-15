/*
 * Mục đích: NUnit EditMode test cho MissionRegistry (T-003) bằng adapter giả: đăng ký, bắt đầu khu, tự hoàn thành,
 * bỏ dở giữ tiến độ, luồng cả map theo thứ tự tùy ý. Không cần scene hay Play Mode.
 * Hàm: FakeAdapter (adapter giả đếm lời gọi); Make / AdapterFor (dựng registry vòng 6 phòng với 6 nhiệm vụ);
 * các test Register_*, TryBegin_*, Tick_*, Abandon_*, Reset_*, WholeMap_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class MissionRegistryTests
    {
        sealed class FakeAdapter : IMissionAdapter
        {
            public string AreaId { get; }
            public bool Active { get; private set; }
            public bool Solved;
            public bool CanHandOffFlag = true;
            public int BeginCalls;
            public int StopCalls;
            public bool LastStopReset;

            public bool CanHandOff => CanHandOffFlag;

            /// <summary>Nhận ID khu; tạo adapter giả chưa giải.</summary>
            public FakeAdapter(string areaId) { AreaId = areaId; }

            public void Begin() { BeginCalls++; Active = true; }

            public void Stop(bool reset) { StopCalls++; LastStopReset = reset; Active = false; }

            bool IMissionAdapter.Solved => Solved;
        }

        static readonly string[] MissionAreas =
        {
            "Phong_1", "Phong_2", "Phong_3", "Phong_4", "Hanh_Lang_1", "Hanh_Lang_3"
        };

        /// <summary>Không nhận gì; trả registry vòng 6 phòng (chưa khu nào có nhiệm vụ), 6 adapter giả đã đăng ký qua out danh sách.</summary>
        static MissionRegistry Make(out Dictionary<string, FakeAdapter> fakes)
        {
            var registry = new MissionRegistry(MapProgress.CreateRing(6, null));
            fakes = new Dictionary<string, FakeAdapter>();
            foreach (string id in MissionAreas)
            {
                var f = new FakeAdapter(id);
                fakes[id] = f;
                registry.Register(f);
            }
            return registry;
        }

        /// <summary>Nhận khu; trả ID khu chuẩn bị hợp lệ để bắt đầu (sân cho phòng, phòng kề đầu tiên cho hành lang).</summary>
        static string EntryFor(MapProgress p, string areaId)
        {
            AreaInfo a = p.Find(areaId);
            return a.Kind == AreaKind.Room ? MapProgress.Yard : a.Neighbors[0];
        }

        // ---------- Đăng ký ----------

        [Test]
        public void Register_MarksAreaAsHavingMission()
        {
            var registry = new MissionRegistry(MapProgress.CreateRing(6, null));
            Assert.IsFalse(registry.Progress.Find("Phong_1").HasMission);
            registry.Register(new FakeAdapter("Phong_1"));
            Assert.IsTrue(registry.Progress.Find("Phong_1").HasMission);
            Assert.IsNotNull(registry.Find("Phong_1"));
            Assert.IsNull(registry.Find("Phong_2"));
        }

        [Test]
        public void Register_RejectsNullUnknownAndDuplicate()
        {
            var registry = new MissionRegistry(MapProgress.CreateRing(6, null));
            Assert.Throws<ArgumentNullException>(() => registry.Register(null));
            Assert.Throws<ArgumentException>(() => registry.Register(new FakeAdapter("Phong_99")));
            registry.Register(new FakeAdapter("Phong_1"));
            Assert.Throws<InvalidOperationException>(() => registry.Register(new FakeAdapter("Phong_1")));
        }

        [Test]
        public void Register_RejectedWhileAreaIsActive()
        {
            var registry = new MissionRegistry(MapProgress.CreateRing(6, null));
            registry.Register(new FakeAdapter("Phong_1"));
            Assert.AreEqual(BeginResult.Started, registry.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.Throws<InvalidOperationException>(() => registry.Register(new FakeAdapter("Phong_1")));
        }

        [Test]
        public void Constructor_RejectsNullProgress()
        {
            Assert.Throws<ArgumentNullException>(() => new MissionRegistry(null));
        }

        // ---------- Bắt đầu ----------

        [Test]
        public void TryBegin_StartsAdapterAndMarksActive()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            Assert.AreEqual(BeginResult.Started, r.TryBegin("Phong_3", MapProgress.Yard, true));
            Assert.AreEqual(1, fakes["Phong_3"].BeginCalls);
            Assert.AreSame(fakes["Phong_3"], r.ActiveAdapter);
            Assert.AreEqual("Phong_3", r.Progress.ActiveAreaId);
        }

        [Test]
        public void TryBegin_RuleFailure_DoesNotTouchAdapter()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            Assert.AreEqual(BeginResult.TeamNotReady, r.TryBegin("Phong_1", MapProgress.Yard, false));
            Assert.AreEqual(BeginResult.InvalidEntry, r.TryBegin("Hanh_Lang_1", MapProgress.Yard, true));
            r.TryBegin("Phong_1", MapProgress.Yard, true);
            Assert.AreEqual(BeginResult.AnotherActive, r.TryBegin("Phong_2", MapProgress.Yard, true));
            Assert.AreEqual(0, fakes["Phong_2"].BeginCalls);
            Assert.AreEqual(0, fakes["Hanh_Lang_1"].BeginCalls);
        }

        [Test]
        public void TryBegin_AreaFlaggedButWithoutAdapter_ReturnsNoMission()
        {
            var registry = new MissionRegistry(MapProgress.CreateRing(6, new[] { "Phong_1" }));
            Assert.AreEqual(BeginResult.NoMission, registry.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.IsNull(registry.Progress.ActiveAreaId);
        }

        // ---------- Tự hoàn thành ----------

        [Test]
        public void Tick_CompletesOnlyWhenSolvedAndHandOffReady()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            r.TryBegin("Phong_2", MapProgress.Yard, true);
            Assert.IsFalse(r.Tick(), "Chưa giải");
            fakes["Phong_2"].Solved = true;
            fakes["Phong_2"].CanHandOffFlag = false;
            Assert.IsFalse(r.Tick(), "Giải xong nhưng còn bảng mở");
            Assert.AreEqual("Phong_2", r.Progress.ActiveAreaId);
            fakes["Phong_2"].CanHandOffFlag = true;
            Assert.IsTrue(r.Tick());
            Assert.IsTrue(r.Progress.IsDone("Phong_2"));
            Assert.IsNull(r.Progress.ActiveAreaId);
            Assert.AreEqual(1, fakes["Phong_2"].StopCalls);
            Assert.IsFalse(fakes["Phong_2"].LastStopReset, "Hoàn thành thì giữ trạng thái đã giải");
            Assert.IsFalse(r.Tick(), "Không hoàn thành lần hai");
        }

        [Test]
        public void Tick_DoesNothingWithoutActiveArea()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            fakes["Phong_1"].Solved = true;
            Assert.IsFalse(r.Tick());
            Assert.AreEqual(0, r.Progress.DoneCount);
        }

        // ---------- Bỏ dở / đặt lại ----------

        [Test]
        public void Abandon_StopsWithReset_AndKeepsOtherProgress()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            r.TryBegin("Phong_1", MapProgress.Yard, true);
            fakes["Phong_1"].Solved = true;
            r.Tick();
            r.TryBegin("Phong_2", MapProgress.Yard, true);
            Assert.IsTrue(r.Abandon());
            Assert.IsTrue(fakes["Phong_2"].LastStopReset);
            Assert.IsNull(r.Progress.ActiveAreaId);
            Assert.IsTrue(r.Progress.IsDone("Phong_1"));
            Assert.IsFalse(r.Progress.IsDone("Phong_2"));
            Assert.IsFalse(r.Abandon(), "Không còn khu nào đang làm");
            Assert.AreEqual(BeginResult.Started, r.TryBegin("Phong_2", MapProgress.Yard, true), "Làm lại được");
        }

        [Test]
        public void Reset_StopsActiveAndClearsProgress_KeepsAdapters()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            r.TryBegin("Phong_1", MapProgress.Yard, true);
            fakes["Phong_1"].Solved = true;
            r.Tick();
            r.TryBegin("Phong_2", MapProgress.Yard, true);
            r.Reset();
            Assert.AreEqual(0, r.Progress.DoneCount);
            Assert.IsNull(r.Progress.ActiveAreaId);
            Assert.IsTrue(fakes["Phong_2"].LastStopReset);
            Assert.IsNotNull(r.Find("Phong_1"));
            Assert.IsTrue(r.Progress.Find("Phong_1").HasMission);
        }

        // ---------- Cả map ----------

        [Test]
        public void WholeMap_AnyOrder_WinsOnlyAfterAllRegisteredMissions()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            string[] order = { "Hanh_Lang_3", "Phong_4", "Phong_1", "Hanh_Lang_1", "Phong_3", "Phong_2" };
            for (int i = 0; i < order.Length; i++)
            {
                Assert.IsFalse(r.Won, "Chưa thắng trước khi xong " + order[i]);
                Assert.AreEqual(BeginResult.Started, r.TryBegin(order[i], EntryFor(r.Progress, order[i]), true), order[i]);
                fakes[order[i]].Solved = true;
                Assert.IsTrue(r.Tick(), order[i]);
            }
            Assert.IsTrue(r.Won);
            CollectionAssert.AreEquivalent(
                new[] { "Phong_5", "Phong_6", "Hanh_Lang_2", "Hanh_Lang_4", "Hanh_Lang_5", "Hanh_Lang_6" },
                r.Progress.MissingMissionAreas);
        }

        [Test]
        public void WholeMap_RegisteringNewMissionAfterwards_ReopensWinCondition()
        {
            Dictionary<string, FakeAdapter> fakes;
            MissionRegistry r = Make(out fakes);
            foreach (string id in MissionAreas)
            {
                r.TryBegin(id, EntryFor(r.Progress, id), true);
                fakes[id].Solved = true;
                r.Tick();
            }
            Assert.IsTrue(r.Won);
            r.Register(new FakeAdapter("Phong_5"));
            Assert.IsFalse(r.Won);
            Assert.AreEqual(BeginResult.Started, r.TryBegin("Phong_5", MapProgress.Yard, true));
        }
    }
}
