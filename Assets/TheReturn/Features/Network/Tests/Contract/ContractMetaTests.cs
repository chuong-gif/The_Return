/*
 * Mục đích: Chứng minh bộ hợp đồng (T-039, M1) vừa ĐẠT với adapter đúng vừa BẮT được adapter hỏng:
 *  - FakeMission_ContractTests: nhiệm vụ giả sạch chạy đủ C1-C9 x đội 2/3/4 x 50 seed (cũng là mẫu để copy khi viết adapter thật).
 *  - Contract_CatchesBrokenAdaptersTests: mỗi lỗi cố ý (FakeFlaw) phải bị ĐÚNG ca bắt; thông điệp lỗi mang nhiệm vụ + ca + n + seed.
 * Hàm: FakeMission_ContractTests.CreateAdapter; Contract_CatchesBrokenAdaptersTests.CleanAdapter_HasNoFailures;
 * Flaw_IsCaughtByExpectedCase; FailureText_NamesMissionCaseTeamAndSeed; Runner_RejectsUnknownCaseCode.
 */
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace TheReturn.Network.Contract.Tests
{
    /// <summary>Nhiệm vụ giả SẠCH: mọi ca C1-C9 phải xanh (C7 chạy thật vì nhiệm vụ giả có hết giờ).</summary>
    [TestFixture]
    public sealed class FakeMission_ContractTests : MissionContractTests<FakeCommand, FakePublic, FakePrivate>
    {
        protected override MissionContractAdapter<FakeCommand, FakePublic, FakePrivate> CreateAdapter()
        {
            return new FakeMissionAdapter(FakeFlaw.None);
        }
    }

    /// <summary>Hợp đồng phải bắt đúng lỗi cố ý.</summary>
    [TestFixture]
    public sealed class Contract_CatchesBrokenAdaptersTests
    {
        static List<ContractFailure> RunAll(FakeFlaw flaw, int seeds)
        {
            var runner = new ContractRunner<FakeCommand, FakePublic, FakePrivate>(new FakeMissionAdapter(flaw), seeds, new[] { 2, 3, 4 });
            var all = new List<ContractFailure>();
            foreach (string c in ContractRunner<FakeCommand, FakePublic, FakePrivate>.AllCases) all.AddRange(runner.Run(c));
            return all;
        }

        static string Describe(List<ContractFailure> all)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < all.Count && i < 6; i++) sb.AppendLine(all[i].ToString());
            return sb.ToString();
        }

        /// <summary>Adapter sạch: không lỗi nào ở bất kỳ ca nào.</summary>
        [Test]
        public void CleanAdapter_HasNoFailures()
        {
            var all = RunAll(FakeFlaw.None, 5);
            Assert.AreEqual(0, all.Count, Describe(all));
        }

        /// <summary>Mỗi lỗi cố ý phải làm đúng ca tương ứng đỏ.</summary>
        [TestCase(FakeFlaw.LeakSecret, "C5")]
        [TestCase(FakeFlaw.DecodeThrows, "C2")]
        [TestCase(FakeFlaw.TooLong, "C3")]
        [TestCase(FakeFlaw.CodecLossy, "C1")]
        [TestCase(FakeFlaw.RejectChangesState, "C4")]
        [TestCase(FakeFlaw.NonDeterministic, "C9")]
        [TestCase(FakeFlaw.BotFails, "C6")]
        [TestCase(FakeFlaw.AliasedState, "N1")]
        public void Flaw_IsCaughtByExpectedCase(FakeFlaw flaw, string expectedCase)
        {
            var all = RunAll(flaw, 5);
            Assert.IsTrue(all.Exists(f => f.Case == expectedCase), "Lỗi " + flaw + " không bị ca " + expectedCase + " bắt. Lỗi thấy được:\n" + Describe(all));
        }

        /// <summary>Thông điệp lỗi phải có tên nhiệm vụ, mã ca, số người và seed (để quy về người giữ nhiệm vụ).</summary>
        [Test]
        public void FailureText_NamesMissionCaseTeamAndSeed()
        {
            var all = RunAll(FakeFlaw.LeakSecret, 3);
            var hit = all.Find(f => f.Case == "C5");
            Assert.IsNotNull(hit, "Không có lỗi C5 để kiểm định dạng.");
            string text = hit.ToString();
            StringAssert.Contains("FakeMission(LeakSecret)", text);
            StringAssert.Contains("C5", text);
            StringAssert.Contains("n=", text);
            StringAssert.Contains("seed=", text);
        }

        /// <summary>Mã ca lạ phải bị từ chối rõ ràng.</summary>
        [Test]
        public void Runner_RejectsUnknownCaseCode()
        {
            var runner = new ContractRunner<FakeCommand, FakePublic, FakePrivate>(new FakeMissionAdapter(FakeFlaw.None), 1, new[] { 2 });
            Assert.Throws<ArgumentException>(() => runner.Run("C99"));
        }
    }
}
