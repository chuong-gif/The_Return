/*
 * Mục đích: NUnit EditMode test cho WinSummary (T-007): tên hiển thị, danh sách khu đã xong theo thứ tự khai báo,
 * khu chưa có nhiệm vụ, điều kiện thắng. Không cần scene hay Play Mode.
 * Hàm: Make; Finish; các test DisplayName_*, Build_*.
 */
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class WinSummaryTests
    {
        static readonly string[] MissionAreas = { "Phong_1", "Phong_2", "Phong_3", "Phong_4", "Hanh_Lang_1", "Hanh_Lang_3" };

        /// <summary>Không nhận gì; trả vòng 6 phòng/6 hành lang với 6 khu đã có nhiệm vụ.</summary>
        static MapProgress Make() { return MapProgress.CreateRing(6, MissionAreas); }

        /// <summary>Nhận tiến độ, ID khu, nơi đội vào; bắt đầu rồi hoàn thành khu đó (assert cả hai bước thành công).</summary>
        static void Finish(MapProgress p, string id, string from)
        {
            Assert.AreEqual(BeginResult.Started, p.TryBegin(id, from, true));
            Assert.IsTrue(p.Complete(id));
        }

        [Test]
        public void DisplayName_Room() { Assert.AreEqual("Phòng 1", WinSummary.DisplayName("Phong_1")); }

        [Test]
        public void DisplayName_Corridor() { Assert.AreEqual("Hành lang 6", WinSummary.DisplayName("Hanh_Lang_6")); }

        [Test]
        public void DisplayName_Yard() { Assert.AreEqual("Sân", WinSummary.DisplayName(MapProgress.Yard)); }

        [Test]
        public void DisplayName_UnknownAndNull()
        {
            Assert.AreEqual("Khu_Lạ", WinSummary.DisplayName("Khu_Lạ"));
            Assert.AreEqual(string.Empty, WinSummary.DisplayName(null));
        }

        [Test]
        public void Build_Null_IsNotWonAndEmpty()
        {
            WinSummary s = WinSummary.Build(null);
            Assert.IsFalse(s.Won);
            Assert.AreEqual(0, s.DoneCount);
            Assert.AreEqual(0, s.DoneAreaNames.Count);
            Assert.AreEqual(0, s.MissingAreaNames.Count);
        }

        [Test]
        public void Build_NothingDone_NotWon()
        {
            WinSummary s = WinSummary.Build(Make());
            Assert.IsFalse(s.Won);
            Assert.AreEqual(0, s.DoneCount);
            Assert.AreEqual(6, s.MissingAreaNames.Count);
        }

        [Test]
        public void Build_Partial_ListsDoneInDeclaredOrder()
        {
            MapProgress p = Make();
            Finish(p, "Phong_2", MapProgress.Yard);
            Finish(p, "Phong_1", MapProgress.Yard);
            WinSummary s = WinSummary.Build(p);
            Assert.IsFalse(s.Won);
            Assert.AreEqual(2, s.DoneCount);
            CollectionAssert.AreEqual(new[] { "Phòng 1", "Phòng 2" }, s.DoneAreaNames);
        }

        [Test]
        public void Build_AllRegisteredDone_IsWonAndMissingAreNamed()
        {
            MapProgress p = Make();
            Finish(p, "Phong_1", MapProgress.Yard);
            Finish(p, "Phong_2", MapProgress.Yard);
            Finish(p, "Phong_3", MapProgress.Yard);
            Finish(p, "Phong_4", MapProgress.Yard);
            Finish(p, "Hanh_Lang_1", "Phong_1");
            Finish(p, "Hanh_Lang_3", "Phong_3");
            WinSummary s = WinSummary.Build(p);
            Assert.IsTrue(s.Won);
            Assert.AreEqual(6, s.DoneCount);
            CollectionAssert.AreEquivalent(
                new[] { "Phòng 5", "Phòng 6", "Hành lang 2", "Hành lang 4", "Hành lang 5", "Hành lang 6" }, s.MissingAreaNames);
        }

        [Test]
        public void Build_RegisteringNewMission_RemovesWon()
        {
            MapProgress p = Make();
            Finish(p, "Phong_1", MapProgress.Yard);
            Finish(p, "Phong_2", MapProgress.Yard);
            Finish(p, "Phong_3", MapProgress.Yard);
            Finish(p, "Phong_4", MapProgress.Yard);
            Finish(p, "Hanh_Lang_1", "Phong_1");
            Finish(p, "Hanh_Lang_3", "Phong_3");
            Assert.IsTrue(WinSummary.Build(p).Won);
            Assert.IsTrue(p.SetHasMission("Phong_5", true));
            WinSummary s = WinSummary.Build(p);
            Assert.IsFalse(s.Won);
            Assert.AreEqual(5, s.MissingAreaNames.Count);
        }
    }
}
