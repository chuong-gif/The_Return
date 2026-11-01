/*
 * Mục đích: NUnit EditMode test cho luật host «Điểm danh» online (T-031): kiểm điều kiện từng lệnh (ngồi/đứng/làm lại), chấm sau thời gian chờ,
 * manh mối riêng chỉ gửi đúng người, và một ván THẮNG đầy đủ qua HostAuthority + ClientReplica với đội 2/3/4. Thế giới vật lý là FakeAttendanceWorld.
 * Hàm: FakeAttendanceWorld; Setup/Send/Packets (trợ giúp); các test Attendance_*. Không cần scene hay Play Mode.
 */
using System.Collections.Generic;
using NUnit.Framework;
using TheReturn.Network.Framework;

namespace TheReturn.AttendanceNet.Tests
{
    public sealed class FakeAttendanceWorld : IAttendanceWorld
    {
        public readonly HashSet<int> FarSlots = new HashSet<int>();
        public bool IsNearSeat(int slot, int seat) { return !FarSlots.Contains(slot); }
    }

    public sealed class AttendanceHostRulesTests
    {
        static readonly string[] AllNames = { "Minh", "An", "Bình", "Chi" };
        FakeAttendanceWorld world;
        AttendanceHostRules rules;
        HostAuthority<AttendanceCommand, AttendancePublicState, AttendancePrivateState> host;
        readonly int[] sequence = new int[4];

        static string[] Packets(AttendanceRound round)
        {
            string[] p = new string[round.PlayerCount];
            for (int i = 0; i < p.Length; i++) p[i] = "[P" + i + "]";
            for (int k = 0; k < round.Rules.Count; k++) p[round.ClueOwners[k]] += " r" + k + ":" + round.Rules[k].Kind;
            return p;
        }

        void Setup(int n, int seed = 3, float settle = 2f)
        {
            string[] names = new string[n];
            for (int i = 0; i < n; i++) names[i] = AllNames[i];
            world = new FakeAttendanceWorld();
            rules = new AttendanceHostRules(names, seed, Packets, world, settle);
            host = new HostAuthority<AttendanceCommand, AttendancePublicState, AttendancePrivateState>(new TeamRoster(n), rules, null);
            for (int i = 0; i < sequence.Length; i++) sequence[i] = 0;
        }

        CommandResult Send(int slot, AttendanceCommand command)
        {
            sequence[slot]++;
            return host.Submit(slot, sequence[slot], command);
        }

        void Advance(float seconds)
        {
            if (rules.Tick(seconds)) host.MarkChanged();
        }

        void SitAll(int[] seats)
        {
            for (int i = 0; i < seats.Length; i++) Assert.IsTrue(Send(i, AttendanceCommand.Sit(seats[i])).IsAccepted, "ngồi ô " + i);
        }

        int[] WrongSeats(int n)
        {
            HashSet<int> solution = new HashSet<int>(rules.Round.CopySolution());
            List<int> wrong = new List<int>();
            for (int seat = 0; seat < AttendanceRound.SeatCount && wrong.Count < n; seat++) if (!solution.Contains(seat)) wrong.Add(seat);
            return wrong.ToArray();
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Attendance_InitialStateIsEmptyForTeamOfN(int n)
        {
            Setup(n);
            AttendancePublicState s = rules.BuildPublicState();
            Assert.AreEqual(n, s.PlayerCount);
            Assert.AreEqual(0, s.OccupiedCount());
            Assert.IsFalse(s.Solved);
            Assert.IsFalse(s.Wrong);
            Assert.IsFalse(s.Settling);
            Assert.AreEqual(0, s.ResetCount);
            Assert.AreEqual(AllNames[n - 1], s.NamesCopy()[n - 1]);
        }

        [Test]
        public void Attendance_Sit_IsAccepted_AndShownToEveryone()
        {
            Setup(3);
            CommandResult r = Send(1, AttendanceCommand.Sit(5));
            Assert.IsTrue(r.IsAccepted);
            Assert.AreEqual(1, rules.BuildPublicState().OccupantOf(5));
            Assert.AreEqual(AttendanceMessage.Seated, rules.BuildPublicState().LastMessage);
        }

        [Test]
        public void Attendance_Sit_IsRejected_WhenSeatTakenAlreadySeatedFarOrInvalid()
        {
            Setup(3);
            Assert.IsTrue(Send(0, AttendanceCommand.Sit(5)).IsAccepted);
            int version = host.Version;
            Assert.IsFalse(Send(1, AttendanceCommand.Sit(5)).IsAccepted, "ghế có người");
            Assert.IsFalse(Send(0, AttendanceCommand.Sit(6)).IsAccepted, "đang ngồi rồi");
            Assert.IsFalse(Send(1, AttendanceCommand.Sit(-1)).IsAccepted, "ghế -1");
            Assert.IsFalse(Send(1, AttendanceCommand.Sit(24)).IsAccepted, "ghế 24");
            world.FarSlots.Add(2);
            Assert.IsFalse(Send(2, AttendanceCommand.Sit(7)).IsAccepted, "đứng xa");
            Assert.AreEqual(version, host.Version, "lệnh bị từ chối không đổi phiên bản");
            Assert.AreEqual(1, rules.BuildPublicState().OccupiedCount());
        }

        [Test]
        public void Attendance_Stand_ReleasesSeat_AndIsRejectedWhenNotSeated()
        {
            Setup(2);
            Assert.IsFalse(Send(0, AttendanceCommand.Stand()).IsAccepted);
            Send(0, AttendanceCommand.Sit(3));
            Assert.IsTrue(Send(0, AttendanceCommand.Stand()).IsAccepted);
            Assert.AreEqual(-1, rules.BuildPublicState().OccupantOf(3));
            Assert.AreEqual(AttendanceMessage.Stood, rules.BuildPublicState().LastMessage);
        }

        [Test]
        public void Attendance_ShotFromOutsideTeam_IsRejected()
        {
            Setup(2);
            Assert.IsFalse(rules.Validate(3, AttendanceCommand.Sit(1)).IsAccepted);
            Assert.IsFalse(rules.Validate(-1, AttendanceCommand.Stand()).IsAccepted);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Attendance_AllSeatedWrong_IsJudgedOnlyAfterSettleTime(int n)
        {
            Setup(n);
            SitAll(WrongSeats(n));
            Assert.IsTrue(rules.BuildPublicState().Settling);
            Advance(1.9f);
            Assert.IsFalse(rules.BuildPublicState().Wrong, "chưa hết thời gian chờ");
            int version = host.Version;
            Advance(0.2f);
            AttendancePublicState s = rules.BuildPublicState();
            Assert.IsTrue(s.Wrong);
            Assert.IsFalse(s.Solved);
            Assert.IsFalse(s.Settling);
            Assert.AreEqual(AttendanceMessage.Wrong, s.LastMessage);
            Assert.Greater(host.Version, version, "chấm xong phải đổi phiên bản");
        }

        [Test]
        public void Attendance_TickReturnsTrueExactlyOnce_WhenJudging()
        {
            Setup(2);
            Assert.IsFalse(rules.Tick(1f), "chưa ai ngồi");
            SitAll(WrongSeats(2));
            Assert.IsFalse(rules.Tick(1f));
            Assert.IsTrue(rules.Tick(1.5f));
            Assert.IsFalse(rules.Tick(5f), "đã chấm rồi, không chấm lại");
        }

        [Test]
        public void Attendance_StandingAfterWrong_ClearsWrongFlag_AndCancelsSettle()
        {
            Setup(2);
            SitAll(WrongSeats(2));
            Advance(3f);
            Assert.IsTrue(rules.BuildPublicState().Wrong);
            Send(0, AttendanceCommand.Stand());
            Assert.IsFalse(rules.BuildPublicState().Wrong);
            Assert.IsFalse(rules.BuildPublicState().Settling);
            Send(0, AttendanceCommand.Sit(WrongSeats(2)[0]));
            Assert.IsTrue(rules.BuildPublicState().Settling, "ngồi đủ lại thì chờ chấm lại");
        }

        [Test]
        public void Attendance_Retry_StandsEveryone_AndCountsResets()
        {
            Setup(3);
            SitAll(WrongSeats(3));
            Advance(3f);
            Assert.IsTrue(Send(2, AttendanceCommand.Retry()).IsAccepted);
            AttendancePublicState s = rules.BuildPublicState();
            Assert.AreEqual(0, s.OccupiedCount());
            Assert.AreEqual(1, s.ResetCount);
            Assert.IsFalse(s.Wrong);
            Assert.AreEqual(AttendanceMessage.Retry, s.LastMessage);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Attendance_SolvedByCorrectSeating_ThroughHostAndReplicas(int n)
        {
            Setup(n, 11);
            ClientReplica<AttendancePublicState, AttendancePrivateState>[] replicas = new ClientReplica<AttendancePublicState, AttendancePrivateState>[n];
            for (int i = 0; i < n; i++) replicas[i] = new ClientReplica<AttendancePublicState, AttendancePrivateState>(i);
            SitAll(rules.Round.CopySolution());
            Advance(2.1f);
            foreach (StateMessage<AttendancePublicState, AttendancePrivateState> m in host.BuildBroadcast()) Assert.IsTrue(replicas[m.Slot].Receive(m));
            string[] packets = Packets(rules.Round);
            for (int i = 0; i < n; i++)
            {
                Assert.IsTrue(replicas[i].Public.Solved, "mọi máy thấy đã xong, máy " + i);
                Assert.AreEqual(packets[i], replicas[i].Private.Clues, "mỗi máy chỉ thấy manh mối của mình");
                for (int j = 0; j < n; j++) if (j != i) Assert.IsFalse(replicas[i].Private.Clues.Contains("[P" + j + "]"), "máy " + i + " không được thấy manh mối của " + j);
            }
            Assert.AreEqual(AttendanceMessage.Solved, rules.BuildPublicState().LastMessage);
        }

        [Test]
        public void Attendance_AfterSolved_StandIsAllowed_ButSolvedStays_AndRetryAndSitAreRejected()
        {
            Setup(2, 5);
            SitAll(rules.Round.CopySolution());
            Advance(2.1f);
            Assert.IsTrue(rules.Solved);
            Assert.IsFalse(Send(0, AttendanceCommand.Retry()).IsAccepted);
            Assert.IsTrue(Send(0, AttendanceCommand.Stand()).IsAccepted, "đứng dậy để đi ra cửa");
            Assert.IsTrue(rules.BuildPublicState().Solved, "thành công được giữ");
            Assert.IsFalse(Send(0, AttendanceCommand.Sit(0)).IsAccepted);
        }

        [Test]
        public void Attendance_ReleaseSeat_StandsADisconnectedPlayer_AndCancelsJudging()
        {
            Setup(2);
            SitAll(WrongSeats(2));
            Assert.IsTrue(rules.BuildPublicState().Settling);
            Assert.IsTrue(rules.ReleaseSeat(1));
            Assert.IsFalse(rules.BuildPublicState().Settling);
            Assert.AreEqual(1, rules.BuildPublicState().OccupiedCount());
            Assert.IsFalse(rules.ReleaseSeat(1), "đã đứng rồi");
        }

        [Test]
        public void Attendance_PrivateState_IsOnlyForOwnSlot_AndEmptyOutsideTeam()
        {
            Setup(3);
            string[] packets = Packets(rules.Round);
            for (int i = 0; i < 3; i++) Assert.AreEqual(packets[i], rules.BuildPrivateState(i).Clues);
            Assert.AreEqual(string.Empty, rules.BuildPrivateState(3).Clues);
            Assert.AreEqual(string.Empty, rules.BuildPrivateState(-1).Clues);
        }

        [Test]
        public void Attendance_PublicState_NeverContainsClueText()
        {
            Setup(4);
            string text = AttendanceCodec.EncodePublic(rules.BuildPublicState());
            string[] packets = Packets(rules.Round);
            for (int i = 0; i < 4; i++) Assert.IsFalse(text.Contains("P" + i), "trạng thái chung không chứa manh mối");
            Assert.IsNotNull(packets);
        }

        [Test]
        public void Attendance_SameSeed_GivesSameRound_AndDifferentSeedsDiffer()
        {
            Setup(3, 21);
            int[] a = rules.Round.CopySolution();
            Setup(3, 21);
            CollectionAssert.AreEqual(a, rules.Round.CopySolution());
            bool anyDifferent = false;
            for (int seed = 22; seed < 40 && !anyDifferent; seed++)
            {
                Setup(3, seed);
                if (!System.Linq.Enumerable.SequenceEqual(a, rules.Round.CopySolution())) anyDifferent = true;
            }
            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void Attendance_Constructor_RejectsBadArguments()
        {
            FakeAttendanceWorld w = new FakeAttendanceWorld();
            Assert.Throws<System.ArgumentNullException>(() => new AttendanceHostRules(new[] { "A", "B" }, 1, null, w, 2f));
            Assert.Throws<System.ArgumentNullException>(() => new AttendanceHostRules(new[] { "A", "B" }, 1, Packets, null, 2f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new AttendanceHostRules(new[] { "A", "B" }, 1, Packets, w, -1f));
            Assert.Throws<System.ArgumentException>(() => new AttendanceHostRules(new[] { "A" }, 1, Packets, w, 2f));
            Assert.Throws<System.ArgumentException>(() => new AttendanceHostRules(new[] { "A", "B" }, 1, r => new[] { "chỉ một" }, w, 2f));
        }
    }
}
