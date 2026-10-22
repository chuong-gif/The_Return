using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class AreaHudTests
    {
        static MapProgress Ring()
        {
            return MapProgress.CreateRing(6, new[] { "Phong_1", "Phong_2", "Hanh_Lang_1" });
        }

        [Test]
        public void MiniMap_HasTwelveCellsInRingOrder()
        {
            var cells = MiniMapModel.Build(Ring());
            Assert.AreEqual(12, cells.Count);
            Assert.AreEqual("Phong_1", cells[0].Id);
            Assert.AreEqual("Hanh_Lang_1", cells[1].Id);
            Assert.AreEqual("Phong_2", cells[2].Id);
            Assert.AreEqual(30f, cells[1].AngleDegrees, 0.001f);
            Assert.IsTrue(cells[1].IsCorridor);
            Assert.IsFalse(cells[0].IsCorridor);
        }

        [Test]
        public void MiniMap_StatesFollowProgress()
        {
            var p = Ring();
            var c = MiniMapModel.Build(p);
            Assert.AreEqual(MiniMapState.Available, c[0].State);
            Assert.AreEqual(MiniMapState.NoMission, c[4].State);
            p.TryBegin("Phong_1", MapProgress.Yard, true);
            c = MiniMapModel.Build(p);
            Assert.AreEqual(MiniMapState.Active, c[0].State);
            Assert.AreEqual(MiniMapState.Blocked, c[2].State);
            p.Complete("Phong_1");
            c = MiniMapModel.Build(p);
            Assert.AreEqual(MiniMapState.Done, c[0].State);
            Assert.AreEqual(MiniMapState.Available, c[2].State);
        }

        [Test]
        public void MiniMap_NullProgressGivesEmpty()
        {
            Assert.AreEqual(0, MiniMapModel.Build(null).Count);
        }

        [Test]
        public void ShortLabel_Works()
        {
            Assert.AreEqual("P3", MiniMapModel.ShortLabel("Phong_3"));
            Assert.AreEqual("H2", MiniMapModel.ShortLabel("Hanh_Lang_2"));
            Assert.AreEqual("San", MiniMapModel.ShortLabel("San"));
            Assert.AreEqual("", MiniMapModel.ShortLabel(null));
        }

        [Test]
        public void Alerts_ExpireAndExtend()
        {
            var m = new AreaHudModel();
            m.AddAlert("A", 10, 3);
            m.AddAlert("B", 10, 5);
            m.AddAlert("", 10, 5);
            Assert.AreEqual(2, m.ActiveAlerts(11).Count);
            Assert.AreEqual(1, m.ActiveAlerts(14).Count);
            m.AddAlert("B", 14, 5);
            m.ExpireAlerts(15);
            Assert.AreEqual(1, m.ActiveAlerts(18).Count);
            Assert.AreEqual(0, m.ActiveAlerts(19.5).Count);
        }

        [Test]
        public void Clear_ResetsEverything()
        {
            var m = new AreaHudModel { Title = "x", Notebook = "y" };
            m.AddAlert("A", 0, 10);
            m.Clear();
            Assert.AreEqual("", m.Title);
            Assert.AreEqual("", m.Notebook);
            Assert.AreEqual(0, m.ActiveAlerts(1).Count);
        }
    }
}
