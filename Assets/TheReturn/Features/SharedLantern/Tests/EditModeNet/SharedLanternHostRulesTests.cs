/*
 * Mục đích: Kiểm thử LUẬT HOST của «Đèn lồng chung» online (T-037) ở EditMode, không cần Unity chạy: trao đèn (đúng người cầm, trong tầm), đọc biển
 * (đứng gần, biển trong vùng sáng, sổ riêng), bảng nhập (đứng gần, đủ ô, nhập sai bị khóa), đồng hồ ngoài vùng sáng (đưa về, người cầm đèn miễn),
 * thắng khi bảng đúng và cả đội ở cổng, làm lại, trạng thái chung không lộ đề và không đổi khi đồng hồ chạy, và chơi trọn một ván cho đội 2/3/4.
 * Hàm: FakeLanternWorld; các test trong SharedLanternHostRulesTests.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.LanternNet.Tests
{
    /// <summary>Thế giới giả: mặc định mọi người đứng gần mọi thứ, trong vùng sáng, cách nhau 1 m, cả đội ở cổng.</summary>
    public sealed class FakeLanternWorld : ISharedLanternWorld
    {
        public readonly HashSet<int> FarSlots = new HashSet<int>();
        public readonly HashSet<int> OutsideSlots = new HashSet<int>();
        public readonly HashSet<int> DarkSigns = new HashSet<int>();
        public bool AllGate = true;
        public double Distance = 1.0;

        public bool IsNearSign(int slot, int sign) { return !FarSlots.Contains(slot); }
        public bool IsSignInLight(int sign) { return !DarkSigns.Contains(sign); }
        public bool IsInLight(int slot) { return !OutsideSlots.Contains(slot); }
        public double DistanceBetween(int slotA, int slotB) { return FarSlots.Contains(slotA) || FarSlots.Contains(slotB) ? 100.0 : Distance; }
        public bool IsNearPanel(int slot) { return !FarSlots.Contains(slot); }
        public bool AllAtGate() { return AllGate; }
    }

    [TestFixture]
    public sealed class SharedLanternHostRulesTests
    {
        FakeLanternWorld world;
        SharedLanternHostRules rules;
        HostAuthority<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState> host;
        ClientReplica<SharedLanternPublicState, SharedLanternPrivateState>[] replicas;

        void Setup(int n, int seed = 7, double outsideLimit = 5.0, double passRange = 2.0, double lockSeconds = 10.0)
        {
            world = new FakeLanternWorld();
            rules = new SharedLanternHostRules(n, seed, world, outsideLimit, passRange, lockSeconds);
            host = new HostAuthority<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState>(new TeamRoster(n), rules, new HostLog(64));
            replicas = new ClientReplica<SharedLanternPublicState, SharedLanternPrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<SharedLanternPublicState, SharedLanternPrivateState>(i);
        }

        CommandResult Send(int slot, SharedLanternCommand command)
        {
            return host.Submit(slot, replicas[slot].NextSequence(), command);
        }

        SharedLanternPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        /// <summary>Gửi lệnh và bắt buộc được nhận.</summary>
        void Ok(int slot, SharedLanternCommand command)
        {
            CommandResult r = Send(slot, command);
            Assert.IsTrue(r.IsAccepted, "Lệnh " + command.Kind + " bị từ chối: " + r.Detail);
        }

        /// <summary>Nhập đúng đáp án vào 4 ô bằng ô cho trước.</summary>
        void FillCorrect(int slot)
        {
            int[] solution = rules.Round.Solution();
            for (int i = 0; i < solution.Length; i++) Ok(slot, SharedLanternCommand.SetEntry(i, solution[i]));
        }

        /// <summary>Nhập 4 ô SAI (mỗi ô lệch một ký hiệu so với đáp án).</summary>
        void FillWrong(int slot)
        {
            int[] solution = rules.Round.Solution();
            for (int i = 0; i < solution.Length; i++) Ok(slot, SharedLanternCommand.SetEntry(i, (solution[i] + 1) % SharedLanternPublicState.SymbolCount));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void FullFlow_TeamReadsSharesAndWins(int n)
        {
            Setup(n);
            for (int sign = 0; sign < 4; sign++) Ok(sign % n, SharedLanternCommand.Read(sign));
            int[] solution = rules.Round.Solution();
            for (int sign = 0; sign < 4; sign++)
            {
                int symbol;
                Assert.IsTrue(rules.BuildPrivateState(sign % n).TryGetSymbol(sign, out symbol));
                Assert.AreEqual(solution[sign], symbol, "Sổ của người đọc phải khớp ký hiệu thật của biển.");
            }
            for (int i = 0; i < 4; i++) Ok(i % n, SharedLanternCommand.SetEntry(i, solution[i]));
            Ok(0, SharedLanternCommand.Submit());
            SharedLanternPublicState p = Pub();
            Assert.IsTrue(p.CodeAccepted);
            Assert.IsTrue(p.Solved);
            Assert.AreEqual(SharedLanternMessage.Solved, p.LastMessage);
            Assert.IsTrue(rules.Solved);
        }

        [Test]
        public void Pass_OnlyHolder_InRange_ToAnotherPlayer()
        {
            Setup(3);
            Assert.AreEqual(0, Pub().Holder);
            Assert.IsFalse(Send(1, SharedLanternCommand.Pass(2)).IsAccepted, "Người không cầm đèn không được trao.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(0)).IsAccepted, "Không tự trao cho mình.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(3)).IsAccepted, "Người nhận ngoài đội.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(-1)).IsAccepted);
            world.Distance = 3.0;
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(1)).IsAccepted, "Quá tầm 2 m.");
            Assert.AreEqual(0, Pub().Holder);
            world.Distance = 2.0;
            Ok(0, SharedLanternCommand.Pass(1));
            SharedLanternPublicState p = Pub();
            Assert.AreEqual(1, p.Holder);
            Assert.AreEqual(SharedLanternMessage.Passed, p.LastMessage);
            Assert.AreEqual(0, p.LastActor);
            Assert.AreEqual(1, p.LastIndex);
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(2)).IsAccepted, "Người cũ hết quyền trao.");
        }

        [Test]
        public void Pass_ToFarPlayer_IsRejectedByHostMeasurement()
        {
            Setup(2);
            world.FarSlots.Add(1);
            string before = SharedLanternCodec.EncodePublic(Pub());
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(1)).IsAccepted);
            Assert.AreEqual(before, SharedLanternCodec.EncodePublic(Pub()));
        }

        [Test]
        public void Read_Rejections()
        {
            Setup(3);
            Assert.IsFalse(Send(0, SharedLanternCommand.Read(4)).IsAccepted);
            Assert.IsFalse(Send(0, SharedLanternCommand.Read(-1)).IsAccepted);
            world.FarSlots.Add(1);
            Assert.IsFalse(Send(1, SharedLanternCommand.Read(0)).IsAccepted, "Đứng xa biển.");
            world.DarkSigns.Add(2);
            Assert.IsFalse(Send(0, SharedLanternCommand.Read(2)).IsAccepted, "Biển ngoài vùng sáng.");
            Ok(0, SharedLanternCommand.Read(0));
            Assert.IsFalse(Send(0, SharedLanternCommand.Read(0)).IsAccepted, "Đã đọc rồi.");
            Ok(2, SharedLanternCommand.Read(0));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Read_IsPrivate_OnlyReaderSeesSymbol(int n)
        {
            Setup(n, 21);
            Ok(0, SharedLanternCommand.Read(1));
            int symbol;
            Assert.IsTrue(rules.BuildPrivateState(0).TryGetSymbol(1, out symbol));
            Assert.AreEqual(rules.Round.SymbolOfSign(1), symbol);
            Assert.IsFalse(rules.BuildPrivateState(0).TryGetSymbol(0, out symbol), "Chưa đọc biển 0.");
            Assert.AreEqual(-1, symbol);
            for (int other = 1; other < n; other++)
                Assert.AreEqual(0, rules.BuildPrivateState(other).ReadCount, "Sổ của ô 0 lộ sang ô " + other);
            Assert.AreEqual(0, rules.BuildPrivateState(n).ReadCount, "Ô ngoài đội không có sổ.");
            Ok(1, SharedLanternCommand.Read(1));
            Assert.AreEqual(1, rules.BuildPrivateState(1).ReadCount, "Mỗi người tự đọc, sổ không chia sẻ.");
        }

        [Test]
        public void PublicState_NeverLeaksPuzzle()
        {
            string first = null;
            for (int seed = 1; seed <= 20; seed++)
            {
                SharedLanternHostRules r = new SharedLanternHostRules(3, seed, new FakeLanternWorld());
                string text = SharedLanternCodec.EncodePublic(r.BuildPublicState());
                Assert.IsFalse(text.Contains("="), "Cặp «biển=ký hiệu» chỉ được gửi riêng.");
                if (first == null) first = text;
                Assert.AreEqual(first, text, "Trạng thái chung ban đầu không được phụ thuộc seed.");
            }
            Setup(3, 9);
            for (int sign = 0; sign < 4; sign++) Ok(0, SharedLanternCommand.Read(sign));
            string afterReads = SharedLanternCodec.EncodePublic(Pub());
            Assert.IsFalse(afterReads.Contains("="));
            Assert.AreEqual(4, rules.BuildPrivateState(0).ReadCount);
        }

        [Test]
        public void SetEntry_Rejections()
        {
            Setup(3);
            Assert.IsFalse(Send(0, SharedLanternCommand.SetEntry(4, 0)).IsAccepted);
            Assert.IsFalse(Send(0, SharedLanternCommand.SetEntry(-1, 0)).IsAccepted);
            Assert.IsFalse(Send(0, SharedLanternCommand.SetEntry(0, 6)).IsAccepted);
            Assert.IsFalse(Send(0, SharedLanternCommand.SetEntry(0, -1)).IsAccepted);
            world.FarSlots.Add(2);
            Assert.IsFalse(Send(2, SharedLanternCommand.SetEntry(0, 0)).IsAccepted, "Đứng xa bảng.");
            Assert.IsFalse(Send(2, SharedLanternCommand.ClearEntries()).IsAccepted, "Đứng xa bảng.");
            Assert.AreEqual(-1, Pub().Entry(0));
            Ok(1, SharedLanternCommand.SetEntry(0, 3));
            Assert.AreEqual(3, Pub().Entry(0));
            Ok(1, SharedLanternCommand.ClearEntries());
            Assert.AreEqual(-1, Pub().Entry(0));
            Assert.AreEqual(SharedLanternMessage.EntriesCleared, Pub().LastMessage);
        }

        [Test]
        public void Submit_Incomplete_IsRejected_AndFarSlotIsRejected()
        {
            Setup(2);
            Ok(0, SharedLanternCommand.SetEntry(0, 1));
            Assert.IsFalse(Send(0, SharedLanternCommand.Submit()).IsAccepted, "Chưa đủ 4 ô.");
            FillCorrect(0);
            world.FarSlots.Add(1);
            Assert.IsFalse(Send(1, SharedLanternCommand.Submit()).IsAccepted, "Đứng xa bảng.");
            Assert.IsFalse(Pub().CodeAccepted);
        }

        [Test]
        public void Submit_Wrong_ClearsEntries_AndLocksPanelUntilTime()
        {
            Setup(3);
            FillWrong(0);
            Ok(0, SharedLanternCommand.Submit());
            SharedLanternPublicState p = Pub();
            Assert.AreEqual(SharedLanternMessage.WrongCode, p.LastMessage);
            Assert.AreEqual(1, p.WrongSubmits);
            Assert.IsFalse(p.CodeAccepted);
            Assert.IsFalse(p.EntriesComplete, "Nhập sai thì xóa cả 4 ô.");
            FillCorrect(1);
            Assert.IsFalse(Send(1, SharedLanternCommand.Submit()).IsAccepted, "Bảng đang bị khóa 10 giây.");
            Assert.IsFalse(rules.Tick(9f));
            Assert.IsFalse(Send(1, SharedLanternCommand.Submit()).IsAccepted, "Mới 9 giây.");
            Assert.IsFalse(rules.Tick(1.5f));
            Ok(1, SharedLanternCommand.Submit());
            Assert.IsTrue(Pub().Solved);
        }

        [Test]
        public void Submit_Correct_WaitsForWholeTeamAtGate()
        {
            Setup(3);
            world.AllGate = false;
            FillCorrect(0);
            Ok(0, SharedLanternCommand.Submit());
            SharedLanternPublicState p = Pub();
            Assert.IsTrue(p.CodeAccepted);
            Assert.IsFalse(p.Solved);
            Assert.AreEqual(SharedLanternMessage.CodeAccepted, p.LastMessage);
            Assert.IsFalse(Send(0, SharedLanternCommand.SetEntry(0, 0)).IsAccepted, "Bảng đã đúng thì khóa sửa.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Submit()).IsAccepted);
            Assert.IsFalse(rules.Tick(1f), "Chưa đủ người ở cổng.");
            world.AllGate = true;
            Assert.IsTrue(rules.Tick(0.1f), "Cả đội tới cổng thì thắng, Tick báo đổi để MarkChanged.");
            Assert.IsTrue(Pub().Solved);
            Assert.AreEqual(SharedLanternMessage.Solved, Pub().LastMessage);
            Assert.IsFalse(rules.Tick(5f), "Đã thắng thì dừng.");
        }

        [Test]
        public void Tick_OutsideTooLong_SendsBackOnce_AndReportsToTake()
        {
            Setup(3);
            world.OutsideSlots.Add(1);
            Assert.IsTrue(rules.Tick(1f), "Mặt nạ ngoài sáng đổi 0 -> bit 1.");
            Assert.IsTrue(Pub().IsOutside(1));
            Assert.AreEqual(0, Pub().SentBackCount);
            Assert.IsFalse(rules.Tick(4.5f), "Mới 4,5 giây ngoài sáng: chưa quá 5 giây, không đổi gì.");
            Assert.IsTrue(rules.Tick(0.6f), "Quá 5 giây thì bị đưa về.");
            SharedLanternPublicState p = Pub();
            Assert.AreEqual(1, p.SentBackCount);
            Assert.AreEqual(2, p.LastSentBackMask);
            Assert.AreEqual(SharedLanternMessage.SentBack, p.LastMessage);
            Assert.AreEqual(2, rules.TakeSentBack());
            Assert.AreEqual(0, rules.TakeSentBack(), "TakeSentBack xóa sau khi lấy.");
            world.OutsideSlots.Clear();
            Assert.IsTrue(rules.Tick(0.1f), "Về trong vùng sáng thì mặt nạ ngoài sáng xóa.");
            Assert.IsFalse(Pub().IsOutside(1));
        }

        [Test]
        public void Tick_HolderIsNeverSentBack()
        {
            Setup(2);
            world.OutsideSlots.Add(0);
            for (int i = 0; i < 40; i++) Assert.IsFalse(rules.Tick(1f));
            SharedLanternPublicState p = Pub();
            Assert.AreEqual(0, p.SentBackCount);
            Assert.AreEqual(0, p.OutsideMask, "Người cầm đèn luôn được coi là trong vùng sáng.");
        }

        [Test]
        public void Pass_RecomputesOutsideMask_AndNewHolderIsSafe()
        {
            Setup(3);
            world.OutsideSlots.Add(1);
            Assert.IsTrue(rules.Tick(1f));
            Assert.IsTrue(Pub().IsOutside(1));
            Ok(0, SharedLanternCommand.Pass(1));
            Assert.IsFalse(Pub().IsOutside(1), "Người vừa nhận đèn thì ở trong vùng sáng.");
            world.OutsideSlots.Add(0);
            Assert.IsTrue(rules.Tick(0.5f), "Người cũ giờ đứng ngoài sáng -> mặt nạ đổi.");
            Assert.IsTrue(Pub().IsOutside(0));
        }

        [Test]
        public void CustomOutsideLimit_IsHonored()
        {
            Setup(2, 3, 1.0);
            Assert.AreEqual(1.0, Pub().OutsideLimitSeconds, 1e-9);
            world.OutsideSlots.Add(1);
            Assert.IsTrue(rules.Tick(0.1f));
            Assert.IsTrue(rules.Tick(1.2f), "Quá giới hạn 1 giây.");
            Assert.AreEqual(1, Pub().SentBackCount);
        }

        [Test]
        public void PublicState_IsStableWhileNothingChanges()
        {
            Setup(4);
            Ok(0, SharedLanternCommand.Read(0));
            string a = SharedLanternCodec.EncodePublic(Pub());
            Assert.IsFalse(rules.Tick(0.5f));
            Assert.IsFalse(rules.Tick(0.5f));
            Assert.AreEqual(a, SharedLanternCodec.EncodePublic(Pub()), "Đồng hồ đang chạy không được lọt vào gói: cùng Version cùng chuỗi.");
        }

        [Test]
        public void Tick_IgnoresBadDelta()
        {
            Setup(2);
            world.OutsideSlots.Add(1);
            Assert.IsFalse(rules.Tick(float.NaN));
            Assert.IsFalse(rules.Tick(-1f));
            Assert.IsFalse(rules.Tick(float.PositiveInfinity));
            Assert.AreEqual(0, Pub().OutsideMask);
        }

        [Test]
        public void Retry_ResetsEverything_AndIsRejectedOnceSolved()
        {
            Setup(3);
            Ok(0, SharedLanternCommand.Read(0));
            Ok(0, SharedLanternCommand.Pass(1));
            world.OutsideSlots.Add(2);
            rules.Tick(1f);
            rules.Tick(6f);
            Assert.AreEqual(1, Pub().SentBackCount);
            Ok(2, SharedLanternCommand.Retry());
            SharedLanternPublicState p = Pub();
            Assert.AreEqual(0, p.Holder);
            Assert.AreEqual(0, p.SentBackCount);
            Assert.AreEqual(0, p.LastSentBackMask);
            Assert.AreEqual(1, p.ResetCount);
            Assert.AreEqual(SharedLanternMessage.Retry, p.LastMessage);
            Assert.AreEqual(2, p.LastActor);
            Assert.AreEqual(0, rules.BuildPrivateState(0).ReadCount, "Làm lại thì xóa sổ.");
            Assert.AreEqual(0, rules.TakeSentBack());
            FillCorrect(0);
            Ok(0, SharedLanternCommand.Submit());
            Assert.IsTrue(rules.Solved);
            Assert.IsFalse(Send(0, SharedLanternCommand.Retry()).IsAccepted, "Đã xong thì không làm lại.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Read(0)).IsAccepted, "Đã xong thì không thao tác thêm.");
            Assert.IsFalse(Send(0, SharedLanternCommand.Pass(1)).IsAccepted);
        }

        [Test]
        public void SameSeed_GivesSameSecrets_AndDifferentSeedsDiffer()
        {
            SharedLanternHostRules a = new SharedLanternHostRules(3, 99, new FakeLanternWorld());
            SharedLanternHostRules b = new SharedLanternHostRules(3, 99, new FakeLanternWorld());
            CollectionAssert.AreEqual(a.Round.Solution(), b.Round.Solution());
            bool anyDiffer = false;
            for (int seed = 100; seed < 120; seed++)
                if (!SameArray(new SharedLanternHostRules(3, seed, new FakeLanternWorld()).Round.Solution(), a.Round.Solution())) anyDiffer = true;
            Assert.IsTrue(anyDiffer, "20 seed khác nhau mà đáp án y hệt: bộ sinh có vấn đề.");
        }

        static bool SameArray(int[] x, int[] y)
        {
            if (x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; i++) if (x[i] != y[i]) return false;
            return true;
        }

        [Test]
        public void Constructor_RejectsBadArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new SharedLanternHostRules(2, 1, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternHostRules(1, 1, new FakeLanternWorld()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternHostRules(5, 1, new FakeLanternWorld()));
            Assert.Throws<ArgumentNullException>(() => new SharedLanternHostRules((LanternRound)null, new FakeLanternWorld()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternHostRules(2, 1, new FakeLanternWorld(), 0.0));
        }
    }
}
