/*
 * Mục đích: Test EditMode cho Core4Layout (T-040): layout hợp lệ dựng đúng MapProgress 4 nhiệm vụ, các lỗi thiết kế đều bị Problems() báo,
 * đi trọn 4 khu thì thắng (luật MapProgress), và trạng thái luồng map vẫn đi-về qua MapFlowState (phục vụ online).
 * Layout mẫu dùng vòng cũ 6 phòng chỉ để làm DỮ LIỆU THỬ, không phải thiết kế map mới.
 * Hàm: các test Layout_*; trợ giúp OldRing, DefaultOk.
 */
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Core4.Tests
{
    public sealed class Core4LayoutTests
    {
        static List<AreaInfo> OldRing()
        {
            return new List<AreaInfo>(MapProgress.CreateRing(6, null).Areas);
        }

        static Core4Layout DefaultOk()
        {
            return new Core4Layout(OldRing(), null);
        }

        [Test]
        public void Layout_DefaultOnOldRingHasNoProblems()
        {
            Assert.IsEmpty(DefaultOk().Problems());
        }

        [Test]
        public void Layout_DefaultAreaIdsKeepTheOldMapNames()
        {
            Assert.AreEqual("Phong_1", Core4Layout.DefaultAreaOf(Core4Mission.Attendance));
            Assert.AreEqual("Phong_5", Core4Layout.DefaultAreaOf(Core4Mission.BreakerPanel));
            Assert.AreEqual("Phong_6", Core4Layout.DefaultAreaOf(Core4Mission.Choir));
            Assert.AreEqual("Hanh_Lang_6", Core4Layout.DefaultAreaOf(Core4Mission.PlankAndKey));
        }

        [Test]
        public void Layout_CreateProgressMarksExactlyFourMissionAreas()
        {
            MapProgress p = DefaultOk().CreateProgress();
            int withMission = 0;
            foreach (AreaInfo a in p.Areas) if (a.HasMission) withMission++;
            Assert.AreEqual(Core4Layout.MissionCount, withMission);
            Assert.AreEqual(12 - 4, p.MissingMissionAreas.Count);
            Assert.IsFalse(p.Won);
        }

        [Test]
        public void Layout_RoomIsEnteredFromYardAndCorridorOnlyFromNeighborRooms()
        {
            Core4Layout layout = DefaultOk();
            CollectionAssert.Contains(layout.EntriesFor(Core4Mission.Attendance), MapProgress.Yard);
            List<string> plank = layout.EntriesFor(Core4Mission.PlankAndKey);
            CollectionAssert.DoesNotContain(plank, MapProgress.Yard);
            CollectionAssert.AreEquivalent(new[] { "Phong_6", "Phong_1" }, plank);
        }

        [Test]
        public void Layout_WinningNeedsAllFourAreasDone()
        {
            MapProgress p = DefaultOk().CreateProgress();
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.IsTrue(p.Complete("Phong_1"));
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_5", MapProgress.Yard, true));
            Assert.IsTrue(p.Complete("Phong_5"));
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Phong_6", MapProgress.Yard, true));
            Assert.IsTrue(p.Complete("Phong_6"));
            Assert.IsFalse(p.Won, "còn Cầu ván");
            Assert.AreEqual(BeginResult.InvalidEntry, p.TryBegin("Hanh_Lang_6", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.Started, p.TryBegin("Hanh_Lang_6", "Phong_6", true));
            Assert.IsTrue(p.Complete("Hanh_Lang_6"));
            Assert.IsTrue(p.Won);
            Assert.AreEqual(4, p.DoneCount);
        }

        [Test]
        public void Layout_AreaWithoutMissionStaysClosed()
        {
            MapProgress p = DefaultOk().CreateProgress();
            Assert.AreEqual(BeginResult.NoMission, p.TryBegin("Phong_2", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.NoMission, p.TryBegin("Hanh_Lang_1", "Phong_1", true));
        }

        [Test]
        public void Layout_ProgressSurvivesMapFlowStateRoundTrip()
        {
            MapProgress host = DefaultOk().CreateProgress();
            host.TryBegin("Phong_1", MapProgress.Yard, true);
            host.Complete("Phong_1");
            host.TryBegin("Phong_5", MapProgress.Yard, true);
            MapFlowState sent = MapFlowState.From(host, 3);
            MapFlowState state;
            Assert.IsTrue(MapFlowState.TryDecode(sent.Encode(), out state));
            MapProgress client = DefaultOk().CreateProgress();
            Assert.IsTrue(client.RestoreSnapshot(state.ToSnapshot()));
            Assert.AreEqual("Phong_5", client.ActiveAreaId);
            Assert.IsTrue(client.IsDone("Phong_1"));
            Assert.AreEqual(3, state.ArmedGateIndex);
        }

        [Test]
        public void Layout_MissingMissionIsReported()
        {
            Dictionary<Core4Mission, string> map = Core4Layout.DefaultMissionAreas();
            map.Remove(Core4Mission.Choir);
            Core4Layout layout = new Core4Layout(OldRing(), map);
            List<string> problems = layout.Problems();
            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("Choir", problems[0]);
            Assert.Throws<System.InvalidOperationException>(() => layout.CreateProgress());
        }

        [Test]
        public void Layout_UnknownAndDuplicateMissionAreasAreReported()
        {
            Dictionary<Core4Mission, string> map = Core4Layout.DefaultMissionAreas();
            map[Core4Mission.Choir] = "Phong_9";
            map[Core4Mission.BreakerPanel] = "Phong_1";
            List<string> problems = new Core4Layout(OldRing(), map).Problems();
            Assert.AreEqual(2, problems.Count);
        }

        [Test]
        public void Layout_CorridorWithoutRoomNeighborCannotHoldAMission()
        {
            List<AreaInfo> list = new List<AreaInfo>
            {
                new AreaInfo("Phong_1", AreaKind.Room, false, new string[0]),
                new AreaInfo("Phong_5", AreaKind.Room, false, new string[0]),
                new AreaInfo("Phong_6", AreaKind.Room, false, new string[0]),
                new AreaInfo("Hanh_Lang_6", AreaKind.Corridor, false, new string[0])
            };
            List<string> problems = new Core4Layout(list, null).Problems();
            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("Hanh_Lang_6", problems[0]);
        }

        [Test]
        public void Layout_AsymmetricNeighborsAndDuplicateIdsAreReported()
        {
            List<AreaInfo> list = OldRing();
            list.Add(new AreaInfo("Phong_1", AreaKind.Room, false, new string[0]));
            list.Add(new AreaInfo("Phong_7", AreaKind.Room, false, new[] { "Hanh_Lang_6" }));
            List<string> problems = new Core4Layout(list, null).Problems();
            Assert.IsTrue(problems.Exists(p => p.Contains("Trùng ID khu: Phong_1")));
            Assert.IsTrue(problems.Exists(p => p.Contains("không đối xứng")));
        }

        [Test]
        public void Layout_MoreThanMapFlowStateLimitIsReported()
        {
            List<AreaInfo> list = OldRing();
            for (int i = 0; i < 25; i++) list.Add(new AreaInfo("Kho_" + i, AreaKind.Room, false, new string[0]));
            Assert.IsTrue(new Core4Layout(list, null).Problems().Exists(p => p.Contains("Quá nhiều khu")));
        }

        [Test]
        public void Layout_EmptyLayoutIsReportedNotThrown()
        {
            Core4Layout layout = new Core4Layout(null, null);
            Assert.IsNotEmpty(layout.Problems());
            Assert.IsEmpty(layout.EntriesFor(Core4Mission.Choir));
        }
    }
}
