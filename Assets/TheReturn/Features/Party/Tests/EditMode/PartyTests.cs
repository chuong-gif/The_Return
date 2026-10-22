/*
 * Mục đích: Test NUnit EditMode cho hệ thống nhóm chơi (T-008): mã phòng, luật PartyState (vào/rời/sẵn sàng/bắt đầu/chủ phòng) và
 * LocalPartyService (tạo, vào bằng mã, người giả lập, sự kiện Changed).
 * Hàm: các test PartyCode_*, State_*, Snapshot_*, Local_*.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class PartyTests
    {
        static PartyState NewState() { return new PartyState("ABCD"); }

        static PartyState Filled(int count)
        {
            PartyState s = NewState();
            for (int i = 0; i < count; i++) Assert.AreEqual(PartyResult.Ok, s.Join("p" + i, "Người " + i));
            return s;
        }

        static LocalPartyService NewLocal(string id = "me", int seed = 7) { return new LocalPartyService(id, new LocalPartyRegistry(), seed); }

        // ---------- PartyCode ----------

        [Test]
        public void PartyCode_GenerateIsValidAndDeterministicPerSeed()
        {
            for (int seed = 1; seed <= 200; seed++)
            {
                string a = PartyCode.Generate(new Random(seed));
                Assert.IsTrue(PartyCode.IsValid(a), a);
                Assert.AreEqual(a, PartyCode.Generate(new Random(seed)));
            }
        }

        [Test]
        public void PartyCode_AlphabetHasNoConfusingCharacters()
        {
            foreach (char c in "IO01") Assert.AreEqual(-1, PartyCode.Alphabet.IndexOf(c), c.ToString());
        }

        [Test]
        public void PartyCode_NormalizeAcceptsLowerCaseAndSpacesAndRejectsBadInput()
        {
            Assert.AreEqual("AB2C", PartyCode.Normalize(" ab2c "));
            Assert.AreEqual("AB2C", PartyCode.Normalize("a b 2 c"));
            Assert.IsNull(PartyCode.Normalize(null));
            Assert.IsNull(PartyCode.Normalize(""));
            Assert.IsNull(PartyCode.Normalize("ABC"));
            Assert.IsNull(PartyCode.Normalize("ABCDE"));
            Assert.IsNull(PartyCode.Normalize("AB0C"));
            Assert.IsNull(PartyCode.Normalize("AB!C"));
        }

        [Test]
        public void PartyCode_GenerateRejectsNullRandom()
        {
            Assert.Throws<ArgumentNullException>(() => PartyCode.Generate(null));
        }

        // ---------- PartyState ----------

        [Test]
        public void State_RejectsInvalidCode()
        {
            Assert.Throws<ArgumentException>(() => new PartyState("ab"));
            Assert.Throws<ArgumentException>(() => new PartyState("abcd"));
            Assert.Throws<ArgumentException>(() => new PartyState(null));
        }

        [Test]
        public void State_FirstJoinerIsHostWithSlotZero()
        {
            PartyState s = NewState();
            Assert.AreEqual(PartyResult.Ok, s.Join("a", "An"));
            Assert.AreEqual("a", s.HostId);
            Assert.AreEqual(0, s.Snapshot("a").MemberAtSlot(0).Slot);
            Assert.IsTrue(s.Snapshot("a").MemberAtSlot(0).IsHost);
        }

        [Test]
        public void State_SlotsAreFilledLowestFirstAndReused()
        {
            PartyState s = Filled(3);
            s.Leave("p1");
            Assert.AreEqual(PartyResult.Ok, s.Join("new", "Mới"));
            PartySnapshot snap = s.Snapshot(null);
            Assert.AreEqual("new", snap.MemberAtSlot(1).Id);
            Assert.AreEqual(3, snap.Count);
        }

        [Test]
        public void State_RejectsFifthPlayerAndDuplicatesAndBlankNames()
        {
            PartyState s = Filled(4);
            Assert.AreEqual(PartyResult.Full, s.Join("p5", "Năm"));
            Assert.AreEqual(PartyResult.AlreadyInParty, s.Join("p0", "Lại"));
            PartyState t = NewState();
            Assert.AreEqual(PartyResult.InvalidName, t.Join("x", "   "));
            Assert.AreEqual(PartyResult.InvalidName, t.Join("x", null));
            Assert.Throws<ArgumentException>(() => t.Join("", "Tên"));
        }

        [Test]
        public void State_LongNamesAreTrimmedTo16()
        {
            PartyState s = NewState();
            s.Join("a", "  " + new string('x', 40) + "  ");
            Assert.AreEqual(PartyState.MaxNameLength, s.Snapshot("a").MemberAtSlot(0).Name.Length);
        }

        [Test]
        public void State_HostLeavingPassesHostToEarliestJoiner()
        {
            PartyState s = Filled(3);
            s.Leave("p0");
            Assert.AreEqual("p1", s.HostId);
            s.Join("late", "Muộn");
            s.Leave("p1");
            Assert.AreEqual("p2", s.HostId);
        }

        [Test]
        public void State_ClosesWhenEmptyAndLeaveByStrangerIsNoParty()
        {
            PartyState s = Filled(1);
            Assert.IsFalse(s.Closed);
            Assert.AreEqual(PartyResult.NoParty, s.Leave("stranger"));
            Assert.AreEqual(PartyResult.Ok, s.Leave("p0"));
            Assert.IsTrue(s.Closed);
            Assert.IsNull(s.HostId);
        }

        [Test]
        public void State_CannotStartWithOnePlayerOrWhenNotAllReady()
        {
            PartyState s = Filled(1);
            s.SetReady("p0", true);
            Assert.IsFalse(s.CanStart);
            Assert.AreEqual(PartyResult.NotEnoughPlayers, s.TryStart("p0"));
            s.Join("p1", "Hai");
            Assert.IsFalse(s.CanStart);
            Assert.AreEqual(PartyResult.NotAllReady, s.TryStart("p0"));
        }

        [Test]
        public void State_OnlyHostStartsAndOnlyWhenEveryoneReady()
        {
            PartyState s = Filled(3);
            for (int i = 0; i < 3; i++) s.SetReady("p" + i, true);
            Assert.IsTrue(s.CanStart);
            Assert.AreEqual(PartyResult.NotHost, s.TryStart("p1"));
            Assert.AreEqual(PartyResult.NoParty, s.TryStart("stranger"));
            Assert.IsFalse(s.Started);
            Assert.AreEqual(PartyResult.Ok, s.TryStart("p0"));
            Assert.IsTrue(s.Started);
        }

        [Test]
        public void State_NewJoinerOrUnreadyBlocksStart()
        {
            PartyState s = Filled(2);
            s.SetReady("p0", true);
            s.SetReady("p1", true);
            Assert.IsTrue(s.CanStart);
            s.Join("p2", "Ba");
            Assert.IsFalse(s.CanStart);
            s.SetReady("p2", true);
            Assert.IsTrue(s.CanStart);
            s.SetReady("p1", false);
            Assert.IsFalse(s.CanStart);
        }

        [Test]
        public void State_AfterStartNoJoinNoReadyChangeNoSecondStart()
        {
            PartyState s = Filled(2);
            s.SetReady("p0", true);
            s.SetReady("p1", true);
            s.TryStart("p0");
            Assert.AreEqual(PartyResult.Started, s.Join("late", "Muộn"));
            Assert.AreEqual(PartyResult.Started, s.SetReady("p1", false));
            Assert.AreEqual(PartyResult.Started, s.TryStart("p0"));
            Assert.IsFalse(s.CanStart);
        }

        [Test]
        public void State_SetReadyByStrangerIsNoParty()
        {
            PartyState s = Filled(2);
            Assert.AreEqual(PartyResult.NoParty, s.SetReady("stranger", true));
        }

        [Test]
        public void Snapshot_IsSortedBySlotAndMarksLocalAndHost()
        {
            PartyState s = Filled(4);
            s.Leave("p0");
            s.Join("back", "Về");
            PartySnapshot snap = s.Snapshot("p2");
            for (int i = 0; i < snap.Count; i++) Assert.AreEqual(i, snap.Members[i].Slot);
            Assert.AreEqual("back", snap.MemberAtSlot(0).Id);
            Assert.AreEqual(2, snap.LocalSlot == 2 ? 2 : -1);
            Assert.IsFalse(snap.IsLocalHost);
            Assert.IsTrue(s.Snapshot("p1").IsLocalHost);
            Assert.AreEqual(-1, s.Snapshot(null).LocalSlot);
            Assert.IsNull(snap.MemberAtSlot(7));
        }

        [Test]
        public void Snapshot_IsImmutableCopy()
        {
            PartyState s = Filled(2);
            PartySnapshot before = s.Snapshot("p0");
            s.Join("p2", "Ba");
            Assert.AreEqual(2, before.Count);
            Assert.AreEqual(3, s.Snapshot("p0").Count);
        }

        // ---------- LocalPartyService ----------

        [Test]
        public void Local_CreateMakesLocalPlayerHostAndNotifies()
        {
            LocalPartyService svc = NewLocal();
            var seen = new List<PartySnapshot>();
            svc.Changed += snap => seen.Add(snap);
            Assert.IsNull(svc.Current);
            Assert.AreEqual(PartyResult.Ok, svc.CreateParty("Minh"));
            Assert.IsTrue(PartyCode.IsValid(svc.Current.Code));
            Assert.IsTrue(svc.Current.IsLocalHost);
            Assert.AreEqual(0, svc.Current.LocalSlot);
            Assert.AreEqual(1, seen.Count);
            Assert.AreEqual(PartyResult.AlreadyInParty, svc.CreateParty("Lại"));
        }

        [Test]
        public void Local_CreateRejectsBlankName()
        {
            Assert.AreEqual(PartyResult.InvalidName, NewLocal().CreateParty(" "));
        }

        [Test]
        public void Local_JoinByCodeWorksForSecondServiceAndRejectsBadCodes()
        {
            var reg = new LocalPartyRegistry();
            var host = new LocalPartyService("host", reg, 3);
            var guest = new LocalPartyService("guest", reg, 4);
            host.CreateParty("Chủ");
            string code = host.Current.Code;
            Assert.AreEqual(PartyResult.InvalidCode, guest.JoinParty("xx", "Khách"));
            Assert.AreEqual(PartyResult.NotFound, guest.JoinParty(code == "AAAA" ? "BBBB" : "AAAA", "Khách"));
            Assert.AreEqual(PartyResult.Ok, guest.JoinParty(code.ToLowerInvariant(), "Khách"));
            Assert.AreEqual(1, guest.Current.LocalSlot);
            Assert.IsFalse(guest.Current.IsLocalHost);
            Assert.AreEqual(2, host.Current.Count);
            Assert.AreEqual(PartyResult.AlreadyInParty, guest.JoinParty(code, "Lại"));
        }

        [Test]
        public void Local_CommandsWithoutPartyReturnNoParty()
        {
            LocalPartyService svc = NewLocal();
            Assert.AreEqual(PartyResult.NoParty, svc.SetReady(true));
            Assert.AreEqual(PartyResult.NoParty, svc.Leave());
            Assert.AreEqual(PartyResult.NoParty, svc.StartGame());
            Assert.AreEqual(PartyResult.NoParty, svc.AddSimulatedPlayer());
            Assert.AreEqual(PartyResult.NoParty, svc.RemoveSimulatedPlayer());
        }

        [Test]
        public void Local_SimulatedPlayersFillUpToFourAndStartFlowWorks()
        {
            LocalPartyService svc = NewLocal();
            svc.CreateParty("Minh");
            Assert.AreEqual(PartyResult.NotEnoughPlayers, svc.StartGame());
            Assert.AreEqual(PartyResult.Ok, svc.AddSimulatedPlayer());
            Assert.AreEqual(PartyResult.NotAllReady, svc.StartGame(), "người chơi thật chưa sẵn sàng");
            svc.SetReady(true);
            Assert.IsTrue(svc.Current.CanStart);
            Assert.AreEqual(PartyResult.Ok, svc.AddSimulatedPlayer());
            Assert.AreEqual(PartyResult.Ok, svc.AddSimulatedPlayer());
            Assert.AreEqual(PartyResult.Full, svc.AddSimulatedPlayer());
            Assert.AreEqual(4, svc.Current.Count);
            Assert.AreEqual(PartyResult.Ok, svc.StartGame());
            Assert.IsTrue(svc.Current.Started);
        }

        [Test]
        public void Local_RemoveSimulatedPlayerRemovesHighestSlotOnly()
        {
            LocalPartyService svc = NewLocal();
            svc.CreateParty("Minh");
            svc.AddSimulatedPlayer();
            svc.AddSimulatedPlayer();
            Assert.AreEqual(PartyResult.Ok, svc.RemoveSimulatedPlayer());
            Assert.AreEqual(2, svc.Current.Count);
            Assert.AreEqual(PartyResult.Ok, svc.RemoveSimulatedPlayer());
            Assert.AreEqual(PartyResult.NoParty, svc.RemoveSimulatedPlayer(), "không còn người giả lập");
            Assert.AreEqual(1, svc.Current.Count);
            Assert.IsTrue(svc.Current.Members[0].IsLocal);
        }

        [Test]
        public void Local_LeaveClosesRoomWhenOnlySimulatedPlayersRemain()
        {
            var reg = new LocalPartyRegistry();
            var svc = new LocalPartyService("me", reg, 5);
            svc.CreateParty("Minh");
            string code = svc.Current.Code;
            svc.AddSimulatedPlayer();
            Assert.AreEqual(1, reg.Count);
            var seen = new List<PartySnapshot>();
            svc.Changed += snap => seen.Add(snap);
            Assert.AreEqual(PartyResult.Ok, svc.Leave());
            Assert.IsNull(svc.Current);
            Assert.AreEqual(0, reg.Count);
            Assert.IsNull(reg.Find(code));
            Assert.AreEqual(1, seen.Count);
            Assert.IsNull(seen[0], "Changed(null) báo đã rời nhóm");
            Assert.AreEqual(PartyResult.Ok, svc.CreateParty("Minh"), "có thể tạo phòng mới sau khi rời");
        }

        [Test]
        public void Local_LeaveKeepsRoomWhileAnotherRealPlayerIsInside()
        {
            var reg = new LocalPartyRegistry();
            var host = new LocalPartyService("host", reg, 3);
            var guest = new LocalPartyService("guest", reg, 4);
            host.CreateParty("Chủ");
            guest.JoinParty(host.Current.Code, "Khách");
            host.Leave();
            Assert.AreEqual(1, reg.Count);
            Assert.IsTrue(guest.Current.IsLocalHost, "khách lên làm chủ phòng");
        }

        [Test]
        public void Local_ChangedFiresOnReadyAndStart()
        {
            LocalPartyService svc = NewLocal();
            svc.CreateParty("Minh");
            svc.AddSimulatedPlayer();
            int count = 0;
            svc.Changed += snap => count++;
            svc.SetReady(true);
            svc.StartGame();
            Assert.AreEqual(2, count);
            Assert.AreEqual(PartyResult.Started, svc.SetReady(false));
            Assert.AreEqual(2, count, "lệnh bị từ chối không bắn sự kiện");
        }

        [Test]
        public void Local_ConstructorRejectsEmptyPlayerId()
        {
            Assert.Throws<ArgumentException>(() => new LocalPartyService("", new LocalPartyRegistry(), 1));
        }
    }
}
