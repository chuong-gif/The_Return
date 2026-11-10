/*
 * Mục đích: Bộ test ĐẶC TẢ cho Domain «Sửa bảng điểm» (T-036): Domain chưa có test NUnit nào (bằng chứng cũ chỉ là file JSON 2026-09), nên ghi lại
 * luật đang chạy để lớp mạng bọc bên trên không làm hỏng luật. KHÔNG sửa Domain. Kiểm: đề sinh theo seed, phân vai tài liệu 2/3/4 người, chỉ đúng
 * vai mới đọc được, áp phiếu và các giới hạn, hoàn tác, đặt lại giữ tài liệu đã đọc, thứ tự lỗi khi nộp, và đáp án chuẩn thắng với mọi cỡ đội x 200 seed.
 * Hàm: các test trong GradeRepairDomainTests; trợ giúp ReadAll, ApplySolution.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Tests
{
    [TestFixture]
    public sealed class GradeRepairDomainTests
    {
        static void ReadAll(GradeRepairState s)
        {
            for (int d = 0; d < 4; d++) Assert.IsTrue(s.MarkRead(d, s.Round.OwnerOf(d)), "đọc tài liệu " + d);
        }

        /// <summary>Đáp án chuẩn: chuyển điểm (phiếu 2) TRƯỚC, vì hồ sơ điểm 9 chỉ nhận +2 sau khi đã bớt 1.</summary>
        static void ApplySolution(GradeRepairState s)
        {
            GradeRepairRound r = s.Round;
            Assert.AreEqual(GradeResult.Ok, s.Apply(2, r.CorrectSource(2), r.CorrectTarget(2)));
            Assert.AreEqual(GradeResult.Ok, s.Apply(0, -1, r.CorrectTarget(0)));
            Assert.AreEqual(GradeResult.Ok, s.Apply(1, -1, r.CorrectTarget(1)));
        }

        [Test]
        public void Round_SameSeed_SameCase_DifferentSeeds_VaryTheCodes()
        {
            GradeRepairRound a = new GradeRepairRound(3, 42);
            GradeRepairRound b = new GradeRepairRound(3, 42);
            CollectionAssert.AreEqual(a.Codes, b.Codes);
            CollectionAssert.AreEqual(a.InitialScores, b.InitialScores);
            bool anyDifferent = false;
            for (int seed = 1; seed <= 20 && !anyDifferent; seed++)
                if (new GradeRepairRound(3, seed).Codes[0] != a.Codes[0]) anyDifferent = true;
            Assert.IsTrue(anyDifferent, "Seed khác nhau phải cho mã hồ sơ khác nhau.");
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(0)]
        public void Round_InvalidTeamSize_Throws(int n)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GradeRepairRound(n, 1));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Round_OwnerOf_IsRoundRobin_AndMinusOneOutOfRange(int n)
        {
            GradeRepairRound r = new GradeRepairRound(n, 7);
            for (int d = 0; d < 4; d++) Assert.AreEqual(d % n, r.OwnerOf(d));
            Assert.AreEqual(-1, r.OwnerOf(-1));
            Assert.AreEqual(-1, r.OwnerOf(4));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Round_InitialScores_AreFourSevenEightNine_AndCodesAreDistinct(int n)
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                GradeRepairRound r = new GradeRepairRound(n, seed);
                int[] sorted = (int[])r.InitialScores.Clone();
                Array.Sort(sorted);
                CollectionAssert.AreEqual(new[] { 4, 7, 8, 9 }, sorted, "seed " + seed);
                for (int i = 0; i < 4; i++)
                {
                    StringAssert.StartsWith("HS-", r.Codes[i]);
                    for (int j = i + 1; j < 4; j++) Assert.AreNotEqual(r.Codes[i], r.Codes[j], "seed " + seed);
                }
            }
        }

        [Test]
        public void Round_Answer_FollowsDesign()
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                GradeRepairRound r = new GradeRepairRound(2, seed);
                Assert.AreEqual(-1, r.CorrectSource(0));
                Assert.AreEqual(-1, r.CorrectSource(1));
                Assert.AreEqual(r.CorrectTarget(1), r.CorrectSource(2), "biên bản chuyển điểm lấy từ hồ sơ nhận phiếu 1");
                Assert.AreNotEqual(r.CorrectTarget(0), r.CorrectTarget(1));
                Assert.AreEqual(-1, r.CorrectTarget(3));
                Assert.AreEqual(-1, r.CorrectSource(3));
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void MarkRead_OnlyTheOwnerCanRead(int n)
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(n, 5));
            for (int d = 0; d < 4; d++)
            {
                for (int p = 0; p < n; p++)
                {
                    if (p == s.Round.OwnerOf(d)) continue;
                    Assert.IsFalse(s.MarkRead(d, p), "vai " + p + " không được đọc tài liệu " + d);
                    Assert.IsFalse(s.Read(d));
                }
                Assert.IsTrue(s.MarkRead(d, s.Round.OwnerOf(d)));
                Assert.IsTrue(s.Read(d));
            }
            Assert.IsFalse(s.MarkRead(-1, 0));
            Assert.IsFalse(s.MarkRead(4, 0));
            Assert.IsFalse(s.MarkRead(0, n));
        }

        [Test]
        public void Apply_Supplement_AddsTwo_AndRejectsBadArguments()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(3, 9));
            int t = s.Round.CorrectTarget(0);
            int before = s.Score(t);
            Assert.AreEqual(GradeResult.Invalid, s.Apply(0, 1, t), "phiếu bổ sung không có nguồn");
            Assert.AreEqual(GradeResult.Invalid, s.Apply(5, -1, t));
            Assert.AreEqual(GradeResult.Invalid, s.Apply(0, -1, 4));
            Assert.AreEqual(GradeResult.Invalid, s.Apply(0, -1, -1));
            Assert.AreEqual(before, s.Score(t));
            Assert.AreEqual(0, s.Revision);
            Assert.AreEqual(GradeResult.Ok, s.Apply(0, -1, t));
            Assert.AreEqual(before + 2, s.Score(t));
            Assert.IsTrue(s.Used(0));
            Assert.AreEqual(1, s.Revision);
            Assert.AreEqual(GradeResult.AlreadyUsed, s.Apply(0, -1, t));
        }

        [Test]
        public void Apply_Transfer_MovesOnePoint_AndRejectsSameRow()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(2, 11));
            int src = s.Round.CorrectSource(2), dst = s.Round.CorrectTarget(2);
            int a = s.Score(src), b = s.Score(dst);
            Assert.AreEqual(GradeResult.Invalid, s.Apply(2, src, src));
            Assert.AreEqual(GradeResult.Invalid, s.Apply(2, -1, dst));
            Assert.AreEqual(GradeResult.Ok, s.Apply(2, src, dst));
            Assert.AreEqual(a - 1, s.Score(src));
            Assert.AreEqual(b + 1, s.Score(dst));
        }

        [Test]
        public void Apply_SupplementBeforeTransfer_IsOutOfRange_AndChangesNothing()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(2, 13));
            int t1 = s.Round.CorrectTarget(1);
            Assert.AreEqual(9, s.Score(t1), "hồ sơ nhận phiếu 1 bắt đầu ở 9 điểm");
            Assert.AreEqual(GradeResult.OutOfRange, s.Apply(1, -1, t1));
            Assert.IsFalse(s.Used(1));
            Assert.AreEqual(0, s.Revision);
            Assert.AreEqual(9, s.Score(t1));
        }

        [Test]
        public void Undo_RestoresScoresAndVoucher_AndReportsNothingToUndo()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(4, 17));
            int[] start = (int[])s.Round.InitialScores.Clone();
            Assert.AreEqual(GradeResult.NothingToUndo, s.Undo());
            ApplySolution(s);
            Assert.AreEqual(3, s.UndoCount);
            for (int i = 0; i < 3; i++) Assert.AreEqual(GradeResult.Ok, s.Undo());
            for (int i = 0; i < 4; i++) Assert.AreEqual(start[i], s.Score(i));
            for (int v = 0; v < 3; v++) Assert.IsFalse(s.Used(v));
            Assert.AreEqual(GradeResult.NothingToUndo, s.Undo());
        }

        [Test]
        public void Reset_ClearsScoresAndVouchers_ButKeepsDocumentsRead()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(3, 19));
            ReadAll(s);
            ApplySolution(s);
            s.Reset();
            for (int i = 0; i < 4; i++) Assert.AreEqual(s.Round.InitialScores[i], s.Score(i));
            for (int v = 0; v < 3; v++) Assert.IsFalse(s.Used(v));
            for (int d = 0; d < 4; d++) Assert.IsTrue(s.Read(d), "đặt lại không xóa tài liệu đã đọc");
            Assert.IsFalse(s.Solved);
        }

        [Test]
        public void Submit_ReportsMissingEvidence_ThenMissingVouchers_ThenWrongRecord()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(3, 23));
            Assert.AreEqual(GradeResult.MissingEvidence, s.Submit());
            ReadAll(s);
            Assert.AreEqual(GradeResult.MissingVouchers, s.Submit());
            GradeRepairRound r = s.Round;
            Assert.AreEqual(GradeResult.Ok, s.Apply(2, r.CorrectSource(2), r.CorrectTarget(2)));
            Assert.AreEqual(GradeResult.Ok, s.Apply(0, -1, r.CorrectTarget(0)));
            int wrong = -1;
            for (int row = 0; row < 4; row++)
                if (row != r.CorrectTarget(0) && row != r.CorrectTarget(1) && row != r.CorrectTarget(2)) wrong = row;
            Assert.GreaterOrEqual(wrong, 0);
            Assert.AreEqual(GradeResult.Ok, s.Apply(1, -1, wrong), "áp phiếu 1 sai hồ sơ vẫn hợp lệ về giới hạn điểm");
            Assert.AreEqual(GradeResult.WrongRecord, s.Submit());
            Assert.IsFalse(s.Solved);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void StandardSolution_Solves_AcrossManySeeds(int n)
        {
            for (int seed = 1; seed <= 200; seed++)
            {
                GradeRepairState s = new GradeRepairState(new GradeRepairRound(n, seed));
                ReadAll(s);
                ApplySolution(s);
                Assert.AreEqual(GradeResult.Solved, s.Submit(), "n=" + n + " seed=" + seed);
                Assert.IsTrue(s.Solved);
                int total = 0;
                for (int i = 0; i < 4; i++)
                {
                    Assert.GreaterOrEqual(s.Score(i), 6, "seed " + seed);
                    Assert.LessOrEqual(s.Score(i), 10, "seed " + seed);
                    total += s.Score(i);
                }
                Assert.AreEqual(32, total);
            }
        }

        [Test]
        public void AfterSolved_EveryCommandReportsSolved_AndRevisionStops()
        {
            GradeRepairState s = new GradeRepairState(new GradeRepairRound(2, 29));
            ReadAll(s);
            ApplySolution(s);
            Assert.AreEqual(GradeResult.Solved, s.Submit());
            int revision = s.Revision;
            Assert.AreEqual(GradeResult.Solved, s.Apply(0, -1, 0));
            Assert.AreEqual(GradeResult.Solved, s.Undo());
            Assert.AreEqual(GradeResult.Solved, s.Submit());
            Assert.AreEqual(revision, s.Revision);
        }
    }
}
