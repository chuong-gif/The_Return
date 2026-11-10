/*
 * Mục đích: Fixture NUnit dùng chung của bộ hợp đồng mạng (T-039, M1). Nhiệm vụ nào cũng chỉ cần MỘT lớp con rỗng:
 *     [TestFixture] public sealed class XxxContractTests : MissionContractTests<XxxCmd, XxxPub, XxxPriv>
 *     { protected override MissionContractAdapter<XxxCmd, XxxPub, XxxPriv> CreateAdapter() { return new XxxAdapter(); } }
 * Mười test C1..C9 + N1 (mạng giả có lỗi), mỗi test chạy mọi cỡ đội x mọi seed bên trong; thông điệp lỗi có Nhiệm vụ + ca + n + seed để quy về người giữ.
 * Hàm: CreateAdapter (abstract); SeedCount; TeamSizes; SetUpRunner; C1..C9; AssertNoFailures.
 */
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace TheReturn.Network.Contract
{
    /// <summary>Bộ hợp đồng C1-C9 cho một nhiệm vụ (lớp con khai adapter).</summary>
    public abstract class MissionContractTests<TCmd, TPub, TPriv>
    {
        ContractRunner<TCmd, TPub, TPriv> runner;

        /// <summary>Dựng adapter của nhiệm vụ.</summary>
        protected abstract MissionContractAdapter<TCmd, TPub, TPriv> CreateAdapter();

        /// <summary>Số seed cho mỗi cỡ đội (mặc định 50).</summary>
        protected virtual int SeedCount { get { return 50; } }

        /// <summary>Các cỡ đội kiểm (mặc định 2, 3, 4).</summary>
        protected virtual int[] TeamSizes { get { return new[] { 2, 3, 4 }; } }

        /// <summary>Dựng engine một lần cho cả fixture.</summary>
        [OneTimeSetUp]
        public void SetUpRunner()
        {
            runner = new ContractRunner<TCmd, TPub, TPriv>(CreateAdapter(), SeedCount, TeamSizes);
        }

        [Test] public void C1_MaHoaGiaiMaRaDungNhuCu() { AssertNoFailures("C1"); }
        [Test] public void C2_DuLieuRacKhongNemLoi() { AssertNoFailures("C2"); }
        [Test] public void C3_ChuoiKhongQuaDai() { AssertNoFailures("C3"); }
        [Test] public void C4_LenhSaiBiTuChoiKhongDoiVersion() { AssertNoFailures("C4"); }
        [Test] public void C5_BiMatKhongLot() { AssertNoFailures("C5"); }
        [Test] public void C6_BotTuGiaiToiThang() { AssertNoFailures("C6"); }
        [Test] public void C7_VanThuaHetGioVanNhatQuan() { AssertNoFailures("C7"); }
        [Test] public void C8_VersionDonDieuVaoMuonKhop() { AssertNoFailures("C8"); }
        [Test] public void C9_CungSeedCungVan() { AssertNoFailures("C9"); }
        [Test] public void N1_MangGiaXauVanHoiTu() { AssertNoFailures("N1"); }

        /// <summary>Chạy một ca; đỏ thì in tối đa 3 lỗi đầu (đủ nhiệm vụ/ca/n/seed) + số lỗi còn lại; C7 không có thua thì bỏ qua.</summary>
        void AssertNoFailures(string caseCode)
        {
            List<ContractFailure> failures = runner.Run(caseCode);
            if (runner.Skipped) Assert.Ignore("Adapter không khai ca " + caseCode + " (ForceLoss trả false).");
            if (failures.Count == 0) return;
            var sb = new StringBuilder();
            sb.AppendLine(failures.Count + " lỗi hợp đồng " + caseCode + ":");
            for (int i = 0; i < failures.Count && i < 3; i++) sb.AppendLine("  " + failures[i]);
            if (failures.Count > 3) sb.AppendLine("  (+" + (failures.Count - 3) + " lỗi khác)");
            Assert.Fail(sb.ToString());
        }
    }
}
