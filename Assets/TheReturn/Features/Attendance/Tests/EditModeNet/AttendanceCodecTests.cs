/*
 * Mục đích: NUnit EditMode test cho bộ mã hóa chuỗi «Điểm danh» online (T-031): đi-về giữ nguyên dữ liệu (kể cả tiếng Việt, xuống dòng và ký tự
 * phân cách trong manh mối), từ chối mọi dữ liệu xấu mà không ném lỗi, và độ dài chuỗi nằm trong giới hạn cầu nối.
 * Hàm: các test Codec_*. Không cần scene hay Play Mode.
 */
using NUnit.Framework;

namespace TheReturn.AttendanceNet.Tests
{
    public sealed class AttendanceCodecTests
    {
        static int[] Seats(params int[] occupied)
        {
            int[] s = new int[AttendancePublicState.SeatCount];
            for (int i = 0; i < s.Length; i++) s[i] = -1;
            for (int who = 0; who < occupied.Length; who++) if (occupied[who] >= 0) s[occupied[who]] = who;
            return s;
        }

        static AttendancePublicState Sample(int n, bool solved = false, bool wrong = true, bool settling = false)
        {
            string[] all = { "Minh", "An", "Bình", "Chi" };
            string[] names = new string[n];
            for (int i = 0; i < n; i++) names[i] = all[i];
            int[] where = new int[n];
            for (int i = 0; i < n; i++) where[i] = i * 5 + 1;
            return new AttendancePublicState(n, names, Seats(where), solved, wrong, settling, 3, AttendanceMessage.Wrong);
        }

        [Test]
        public void Codec_Command_RoundTrips()
        {
            AttendanceCommand[] all = { AttendanceCommand.Sit(0), AttendanceCommand.Sit(23), AttendanceCommand.Stand(), AttendanceCommand.Retry() };
            foreach (AttendanceCommand c in all)
            {
                AttendanceCommand back;
                Assert.IsTrue(AttendanceCodec.TryDecodeCommand(AttendanceCodec.EncodeCommand(c), out back));
                Assert.AreEqual(c.Kind, back.Kind);
                Assert.AreEqual(c.Seat, back.Seat);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("x")]
        [TestCase("0")]
        [TestCase("0|24")]
        [TestCase("0|-1")]
        [TestCase("9|0")]
        [TestCase("1|5")]
        [TestCase("2|3")]
        [TestCase("0|1|2")]
        [TestCase("a|b")]
        [TestCase("0|99999999999999999999")]
        public void Codec_Command_RejectsBadInput(string text)
        {
            AttendanceCommand c;
            Assert.IsFalse(AttendanceCodec.TryDecodeCommand(text, out c));
            Assert.IsNull(c);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Codec_Public_RoundTripsForTeamOfN(int n)
        {
            AttendancePublicState s = Sample(n, solved: n == 3, wrong: n != 3, settling: n == 4);
            AttendancePublicState back;
            Assert.IsTrue(AttendanceCodec.TryDecodePublic(AttendanceCodec.EncodePublic(s), out back));
            Assert.AreEqual(s.PlayerCount, back.PlayerCount);
            CollectionAssert.AreEqual(s.NamesCopy(), back.NamesCopy());
            CollectionAssert.AreEqual(s.SeatsCopy(), back.SeatsCopy());
            Assert.AreEqual(s.Solved, back.Solved);
            Assert.AreEqual(s.Wrong, back.Wrong);
            Assert.AreEqual(s.Settling, back.Settling);
            Assert.AreEqual(s.ResetCount, back.ResetCount);
            Assert.AreEqual(s.LastMessage, back.LastMessage);
        }

        [Test]
        public void Codec_Public_RejectsBadInput()
        {
            string good = AttendanceCodec.EncodePublic(Sample(3));
            string[] f = good.Split('|');
            string[] bad =
            {
                null, "", "rác", good + "|1", string.Join("|", new[] { f[0], f[1], f[2], f[3], f[4], f[5], f[6] }),
                string.Join("|", new[] { "5", f[1], f[2], f[3], f[4], f[5], f[6], f[7] }),
                string.Join("|", new[] { "3", "Minh,An", f[2], f[3], f[4], f[5], f[6], f[7] }),
                string.Join("|", new[] { f[0], f[1], "-1,-1,-1", f[3], f[4], f[5], f[6], f[7] }),
                string.Join("|", new[] { f[0], f[1], f[2].Replace("0", "7"), f[3], f[4], f[5], f[6], f[7] }),
                string.Join("|", new[] { f[0], f[1], f[2], "2", f[4], f[5], f[6], f[7] }),
                string.Join("|", new[] { f[0], f[1], f[2], f[3], f[4], f[5], "-1", f[7] }),
                string.Join("|", new[] { f[0], f[1], f[2], f[3], f[4], f[5], f[6], "99" }),
                string.Join("|", new[] { f[0], "" + "," + ",", f[2], f[3], f[4], f[5], f[6], f[7] }),
            };
            foreach (string text in bad)
            {
                AttendancePublicState s;
                Assert.IsFalse(AttendanceCodec.TryDecodePublic(text, out s), "phải từ chối: " + text);
                Assert.IsNull(s);
            }
        }

        [Test]
        public void Codec_Public_RejectsSamePlayerInTwoSeats()
        {
            int[] seats = Seats(1, 2);
            string text = AttendanceCodec.EncodePublic(Sample(2));
            string[] f = text.Split('|');
            int[] twice = new int[AttendancePublicState.SeatCount];
            for (int i = 0; i < twice.Length; i++) twice[i] = -1;
            twice[2] = 0; twice[9] = 0;
            f[2] = string.Join(",", System.Array.ConvertAll(twice, x => x.ToString()));
            AttendancePublicState s;
            Assert.IsFalse(AttendanceCodec.TryDecodePublic(string.Join("|", f), out s));
            Assert.IsNotNull(seats);
        }

        [Test]
        public void Codec_Private_RoundTrips_VietnameseNewlinesAndSeparators()
        {
            string clues = "• Minh ở sau An đúng 4 hàng.\n\n• Kiểm tra Bình | trong các hàng, có số lẻ; c:%41";
            AttendancePrivateState back;
            Assert.IsTrue(AttendanceCodec.TryDecodePrivate(AttendanceCodec.EncodePrivate(new AttendancePrivateState(clues)), out back));
            Assert.AreEqual(clues, back.Clues);
        }

        [Test]
        public void Codec_Private_EmptyRoundTrips_AndRejectsBadInput()
        {
            AttendancePrivateState back;
            Assert.IsTrue(AttendanceCodec.TryDecodePrivate(AttendanceCodec.EncodePrivate(new AttendancePrivateState(string.Empty)), out back));
            Assert.AreEqual(string.Empty, back.Clues);
            Assert.IsFalse(AttendanceCodec.TryDecodePrivate(null, out back));
            Assert.IsFalse(AttendanceCodec.TryDecodePrivate("không có tiền tố", out back));
            Assert.IsFalse(AttendanceCodec.TryDecodePrivate("c:" + new string('a', AttendanceCodec.MaxChars), out back));
        }

        [Test]
        public void Codec_WorstCaseStrings_StayUnderBridgeLimit()
        {
            string heavyName = new string('ậ', 20);
            AttendancePublicState s = new AttendancePublicState(4, new[] { heavyName, heavyName + "1", heavyName + "2", heavyName + "3" }, Seats(0, 1, 2, 3), true, true, true, 999999, AttendanceMessage.Solved);
            Assert.Less(AttendanceCodec.EncodePublic(s).Length, AttendanceCodec.MaxChars);
            string worstClues = new string('ậ', 130);
            Assert.Less(AttendanceCodec.EncodePrivate(new AttendancePrivateState(worstClues)).Length, AttendanceCodec.MaxChars);
        }

        [Test]
        public void Codec_PublicState_RejectsInvalidConstruction()
        {
            string[] names = { "A", "B" };
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new AttendancePublicState(1, new[] { "A" }, Seats(), false, false, false, 0, AttendanceMessage.None));
            Assert.Throws<System.ArgumentException>(() => new AttendancePublicState(2, new[] { "A" }, Seats(), false, false, false, 0, AttendanceMessage.None));
            Assert.Throws<System.ArgumentException>(() => new AttendancePublicState(2, names, new int[5], false, false, false, 0, AttendanceMessage.None));
            int[] dup = Seats(); dup[0] = 0; dup[1] = 0;
            Assert.Throws<System.ArgumentException>(() => new AttendancePublicState(2, names, dup, false, false, false, 0, AttendanceMessage.None));
        }
    }
}
