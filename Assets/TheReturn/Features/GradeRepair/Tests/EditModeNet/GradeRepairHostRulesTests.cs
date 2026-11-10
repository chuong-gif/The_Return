/*
 * Mục đích: Kiểm thử LUẬT HOST của «Sửa bảng điểm» online (T-036) ở EditMode: quyền đọc tài liệu (đúng vai, đứng gần, chưa đọc), quyền thao tác bảng (đứng gần),
 * kết quả Domain là PHẢN HỒI chung chứ không phải lệnh bị từ chối, đặt lại giữ tài liệu đã đọc, thông tin riêng chỉ có tài liệu mình ĐÃ ĐỌC,
 * và chơi trọn một ván thắng qua HostAuthority + ClientReplica cho đội 2/3/4. Các kiểm tra chung (mã hóa, bí mật, bot, mạng xấu) do bộ hợp đồng lo.
 * Hàm: FakeGradeRepairWorld; các test trong GradeRepairHostRulesTests.
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.GradeRepairNet.Tests
{
    /// <summary>Thế giới giả: mặc định mọi ô đứng gần mọi thứ; thêm ô vào tập tương ứng để cho đứng xa bảng hoặc xa bàn tài liệu.</summary>
    public sealed class FakeGradeRepairWorld : IGradeRepairWorld
    {
        public readonly HashSet<int> FarFromBoard = new HashSet<int>();
        public readonly HashSet<int> FarFromDocuments = new HashSet<int>();

        public bool IsNearBoard(int slot) { return !FarFromBoard.Contains(slot); }
        public bool IsNearDocument(int slot, int document) { return !FarFromDocuments.Contains(slot); }
    }

    [TestFixture]
    public sealed class GradeRepairHostRulesTests
    {
        FakeGradeRepairWorld world;
        GradeRepairHostRules rules;
        HostAuthority<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState> host;
        ClientReplica<GradeRepairPublicState, GradeRepairPrivateState>[] replicas;

        void Setup(int n, int seed = 11)
        {
            world = new FakeGradeRepairWorld();
            rules = new GradeRepairHostRules(n, seed, world);
            host = new HostAuthority<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState>(new TeamRoster(n), rules, new HostLog(64));
            replicas = new ClientReplica<GradeRepairPublicState, GradeRepairPrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<GradeRepairPublicState, GradeRepairPrivateState>(i);
        }

        CommandResult Send(int slot, GradeRepairCommand command)
        {
            return host.Submit(slot, replicas[slot].NextSequence(), command);
        }

        GradeRepairPublicState Pub()
        {
            return rules.BuildPublicState();
        }

        void ReadAllByOwners()
        {
            for (int d = 0; d < 4; d++)
            {
                CommandResult r = Send(rules.Round.OwnerOf(d), GradeRepairCommand.Read(d));
                Assert.IsTrue(r.IsAccepted, "Đọc tài liệu " + d + " bị từ chối: " + r.Detail);
            }
        }

        /// <summary>Đáp án chuẩn qua host, ô 0 đứng gần bảng: chuyển điểm TRƯỚC rồi hai phiếu bổ sung.</summary>
        void ApplySolution()
        {
            GradeRepairRound r = rules.Round;
            int[] order = { 2, 0, 1 };
            foreach (int v in order)
            {
                CommandResult res = Send(0, GradeRepairCommand.Apply(v, r.CorrectSource(v), r.CorrectTarget(v)));
                Assert.IsTrue(res.IsAccepted, res.Detail);
                Assert.AreEqual(GradeResult.Ok, Pub().LastResult, "áp phiếu " + v);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Read_Owner_Accepted_PublicShowsRead_PrivateGetsFact(int n)
        {
            Setup(n);
            int owner = rules.Round.OwnerOf(0);
            int before = host.Version;
            CommandResult r = Send(owner, GradeRepairCommand.Read(0));
            Assert.IsTrue(r.IsAccepted, r.Detail);
            Assert.Greater(host.Version, before);
            Assert.IsTrue(Pub().Read(0));
            Assert.AreEqual(GradeRepairEvent.Read, Pub().LastEvent);
            Assert.AreEqual(owner, Pub().LastActor);
            string fact;
            Assert.IsTrue(rules.BuildPrivateState(owner).TryGetFact(0, out fact));
            StringAssert.StartsWith("C.HS-", fact);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Read_NonOwner_Rejected_StateUnchanged(int n)
        {
            Setup(n);
            int other = (rules.Round.OwnerOf(0) + 1) % n;
            int before = host.Version;
            CommandResult r = Send(other, GradeRepairCommand.Read(0));
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.RejectedByRules, r.Reason);
            StringAssert.Contains("vai", r.Detail);
            Assert.AreEqual(before, host.Version);
            Assert.AreEqual(0, Pub().ReadMask);
        }

        [Test]
        public void Read_TooFarFromDocument_Rejected()
        {
            Setup(3);
            int owner = rules.Round.OwnerOf(1);
            world.FarFromDocuments.Add(owner);
            CommandResult r = Send(owner, GradeRepairCommand.Read(1));
            Assert.IsFalse(r.IsAccepted);
            StringAssert.Contains("xa", r.Detail);
            Assert.AreEqual(0, Pub().ReadMask);
        }

        [Test]
        public void Read_Twice_Rejected_VersionUnchanged()
        {
            Setup(2);
            Assert.IsTrue(Send(0, GradeRepairCommand.Read(0)).IsAccepted);
            int before = host.Version;
            CommandResult r = Send(0, GradeRepairCommand.Read(0));
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(before, host.Version);
        }

        [Test]
        public void Read_UnknownDocument_Rejected()
        {
            Setup(2);
            Assert.IsFalse(rules.Validate(0, GradeRepairCommand.Read(-1)).IsAccepted);
            Assert.IsFalse(rules.Validate(0, GradeRepairCommand.Read(4)).IsAccepted);
        }

        [Test]
        public void Board_TooFar_RejectsApplyUndoSubmit()
        {
            Setup(2);
            world.FarFromBoard.Add(0);
            GradeRepairRound r = rules.Round;
            int before = host.Version;
            Assert.IsFalse(Send(0, GradeRepairCommand.Apply(2, r.CorrectSource(2), r.CorrectTarget(2))).IsAccepted);
            Assert.IsFalse(Send(0, GradeRepairCommand.Undo()).IsAccepted);
            Assert.IsFalse(Send(0, GradeRepairCommand.Submit()).IsAccepted);
            Assert.AreEqual(before, host.Version);
            Assert.AreEqual(0, Pub().UsedMask);
        }

        [Test]
        public void Apply_DomainResult_IsSharedFeedback_NotARejection()
        {
            Setup(2);
            int t1 = rules.Round.CorrectTarget(1);
            int[] scores = Pub().ScoresCopy();
            CommandResult r = Send(0, GradeRepairCommand.Apply(1, -1, t1));
            Assert.IsTrue(r.IsAccepted, "Domain từ chối thao tác là phản hồi, lệnh vẫn được host nhận");
            GradeRepairPublicState s = Pub();
            Assert.AreEqual(GradeResult.OutOfRange, s.LastResult);
            Assert.AreEqual(GradeRepairEvent.Apply, s.LastEvent);
            CollectionAssert.AreEqual(scores, s.ScoresCopy());
            Assert.AreEqual(0, s.UsedMask);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Apply_ThenUndo_RestoresBoard(int n)
        {
            Setup(n);
            GradeRepairRound r = rules.Round;
            int[] start = Pub().ScoresCopy();
            Assert.IsTrue(Send(1 % n, GradeRepairCommand.Apply(2, r.CorrectSource(2), r.CorrectTarget(2))).IsAccepted);
            Assert.AreEqual(4, Pub().UsedMask);
            Assert.AreNotEqual(start[r.CorrectTarget(2)], Pub().Score(r.CorrectTarget(2)));
            Assert.IsTrue(Send(0, GradeRepairCommand.Undo()).IsAccepted);
            CollectionAssert.AreEqual(start, Pub().ScoresCopy());
            Assert.AreEqual(0, Pub().UsedMask);
        }

        [Test]
        public void Submit_BeforeReading_IsFeedbackMissingEvidence()
        {
            Setup(3);
            Assert.IsTrue(Send(0, GradeRepairCommand.Submit()).IsAccepted);
            Assert.AreEqual(GradeResult.MissingEvidence, Pub().LastResult);
            Assert.IsFalse(Pub().Solved);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Reset_KeepsDocumentsRead_ClearsVouchers_CountsReset(int n)
        {
            Setup(n);
            ReadAllByOwners();
            ApplySolution();
            Assert.IsTrue(Send(n - 1, GradeRepairCommand.Reset()).IsAccepted);
            GradeRepairPublicState s = Pub();
            Assert.AreEqual(15, s.ReadMask);
            Assert.AreEqual(0, s.UsedMask);
            Assert.AreEqual(1, s.ResetCount);
            Assert.AreEqual(GradeRepairEvent.Reset, s.LastEvent);
            CollectionAssert.AreEqual(rules.Round.InitialScores, s.ScoresCopy());
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void PrivateState_HasOnlyOwnReadDocuments(int n)
        {
            Setup(n);
            for (int slot = 0; slot < n; slot++) Assert.AreEqual(0, rules.BuildPrivateState(slot).OwnedCount, "chưa đọc gì thì riêng rỗng");
            ReadAllByOwners();
            int total = 0;
            for (int slot = 0; slot < n; slot++)
            {
                GradeRepairPrivateState p = rules.BuildPrivateState(slot);
                int[] docs = p.DocumentsCopy();
                string[] facts = p.FactsCopy();
                for (int i = 0; i < docs.Length; i++)
                {
                    Assert.AreEqual(slot, rules.Round.OwnerOf(docs[i]), "ô " + slot + " không được biết tài liệu " + docs[i]);
                    Assert.IsTrue(GradeRepairPrivateState.IsValidFact(facts[i]));
                }
                total += docs.Length;
            }
            Assert.AreEqual(4, total, "mỗi tài liệu có đúng một chủ");
            Assert.AreEqual(0, rules.BuildPrivateState(-1).OwnedCount);
            Assert.AreEqual(0, rules.BuildPrivateState(n).OwnedCount);
        }

        [Test]
        public void FactsOf_FollowCatalogVariantFormula_AndStayShortAscii()
        {
            GradeRepairRound r = new GradeRepairRound(3, 7);
            StringAssert.StartsWith("C.HS-", GradeRepairHostRules.FactsOf(r, 0, 2, 2));
            Assert.AreEqual("V" + (7 % 3) + "." + r.Codes[r.CorrectTarget(0)], GradeRepairHostRules.FactsOf(r, 1, 3, 2));
            Assert.AreEqual("V" + (7 % 2) + "." + r.Codes[r.CorrectTarget(1)], GradeRepairHostRules.FactsOf(r, 2, 2, 5));
            Assert.AreEqual("T" + (7 % 5) + "." + r.Codes[r.CorrectSource(2)] + "_" + r.Codes[r.CorrectTarget(2)], GradeRepairHostRules.FactsOf(r, 3, 2, 5));
            Assert.AreEqual(string.Empty, GradeRepairHostRules.FactsOf(r, 4, 2, 2));
            for (int d = 0; d < 4; d++) Assert.IsTrue(GradeRepairPrivateState.IsValidFact(GradeRepairHostRules.FactsOf(r, d, 2, 2)));
        }

        [Test]
        public void PublicState_NeverContainsCodesOrSeed()
        {
            Setup(3, 123456);
            ReadAllByOwners();
            ApplySolution();
            string text = GradeRepairCodec.EncodePublic(Pub());
            StringAssert.DoesNotContain("HS-", text);
            StringAssert.DoesNotContain("123456", text);
            Assert.AreEqual(9, text.Split('|').Length);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void FullGame_Win_ReachesEveryReplica(int n)
        {
            Setup(n);
            ReadAllByOwners();
            ApplySolution();
            Assert.IsTrue(Send(0, GradeRepairCommand.Submit()).IsAccepted);
            Assert.IsTrue(Pub().Solved);
            Assert.AreEqual(GradeResult.Solved, Pub().LastResult);
            StateMessage<GradeRepairPublicState, GradeRepairPrivateState>[] all = host.BuildBroadcast();
            Assert.AreEqual(n, all.Length);
            foreach (StateMessage<GradeRepairPublicState, GradeRepairPrivateState> msg in all)
                Assert.IsTrue(replicas[msg.Slot].Receive(msg));
            for (int slot = 0; slot < n; slot++)
            {
                ClientReplica<GradeRepairPublicState, GradeRepairPrivateState> rep = replicas[slot];
                Assert.IsTrue(rep.HasState);
                Assert.AreEqual(host.Version, rep.Version);
                Assert.IsTrue(rep.Public.Solved);
                Assert.AreEqual(15, rep.Public.ReadMask);
                Assert.AreEqual(7, rep.Public.UsedMask);
                int expected = 0;
                for (int d = 0; d < 4; d++) if (d % n == slot) expected++;
                Assert.AreEqual(expected, rep.Private.OwnedCount);
            }
        }

        [Test]
        public void AfterSolved_EveryCommandRejected()
        {
            Setup(2);
            ReadAllByOwners();
            ApplySolution();
            Assert.IsTrue(Send(0, GradeRepairCommand.Submit()).IsAccepted);
            int before = host.Version;
            Assert.IsFalse(Send(0, GradeRepairCommand.Undo()).IsAccepted);
            Assert.IsFalse(Send(1, GradeRepairCommand.Reset()).IsAccepted);
            Assert.IsFalse(Send(0, GradeRepairCommand.Apply(0, -1, 0)).IsAccepted);
            Assert.AreEqual(before, host.Version);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void LateJoin_BuildStateFor_GivesFullState(int n)
        {
            Setup(n);
            Assert.IsTrue(Send(0, GradeRepairCommand.Read(0)).IsAccepted);
            StateMessage<GradeRepairPublicState, GradeRepairPrivateState> msg = host.BuildStateFor(0);
            Assert.IsTrue(replicas[0].Receive(msg));
            Assert.IsTrue(replicas[0].Public.Read(0));
            Assert.AreEqual(1, replicas[0].Private.OwnedCount);
            Assert.AreEqual(host.Version, replicas[0].Version);
        }

        [TestCase(3)]
        [TestCase(4)]
        public void DisconnectedSlot_Rejected_AndSkippedInBroadcast(int n)
        {
            Setup(n);
            host.DisconnectSlot(1);
            CommandResult r = Send(1, GradeRepairCommand.Reset());
            Assert.IsFalse(r.IsAccepted);
            Assert.AreEqual(RejectReason.NotConnected, r.Reason);
            Assert.AreEqual(n - 1, host.BuildBroadcast().Length);
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            FakeGradeRepairWorld w = new FakeGradeRepairWorld();
            Assert.Catch<System.ArgumentException>(() => new GradeRepairHostRules(3, 1, null));
            Assert.Catch<System.ArgumentException>(() => new GradeRepairHostRules(3, 1, w, 0, 2));
            Assert.Catch<System.ArgumentException>(() => new GradeRepairHostRules(3, 1, w, 2, 0));
            Assert.Catch<System.Exception>(() => new GradeRepairHostRules(1, 1, w));
            Assert.Catch<System.Exception>(() => new GradeRepairHostRules(5, 1, w));
        }

        [Test]
        public void PrivateState_CopiesAreIndependent_AndBadShapesThrow()
        {
            GradeRepairPrivateState p = new GradeRepairPrivateState(new[] { 0, 2 }, new[] { "C:HS-111", "V0:HS-112" });
            int[] copy = p.DocumentsCopy();
            copy[0] = 3;
            Assert.AreEqual(0, p.DocumentsCopy()[0]);
            Assert.Throws<System.ArgumentException>(() => new GradeRepairPrivateState(new[] { 0 }, new[] { "a", "b" }));
            Assert.Throws<System.ArgumentException>(() => new GradeRepairPrivateState(new[] { 2, 1 }, new[] { "a", "b" }));
            Assert.Throws<System.ArgumentException>(() => new GradeRepairPrivateState(new[] { 0 }, new[] { "có dấu" }));
            Assert.AreEqual(0, default(GradeRepairPrivateState).OwnedCount);
        }
    }
}
