/*
 * Mục đích: Kiểm thử bộ chuyển chuỗi (ChoirCodec) của «Hợp xướng» online (T-037): đi-về nguyên vẹn cho lệnh, trạng thái chung và thông tin riêng
 * ở đội 2/3/4, và MỌI chuỗi sai từ mạng (rác, thiếu/thừa trường, ngoài khoảng, không khớp tiến độ, bục trùng) đều bị từ chối bằng false, không ném lỗi.
 * Hàm: các test trong ChoirCodecTests.
 */
using System;
using NUnit.Framework;

namespace TheReturn.ChoirNet.Tests
{
    [TestFixture]
    public sealed class ChoirCodecTests
    {
        /// <summary>Nhận chuỗi trạng thái chung, chỉ số trường và giá trị mới; trả chuỗi đã thay trường đó.</summary>
        static string Mutate(string text, int field, string value)
        {
            string[] parts = text.Split('|');
            parts[field] = value;
            return string.Join("|", parts);
        }

        /// <summary>Nhận số người, seed và số nốt đã bấm đúng; trả luật host ở trạng thái đó.</summary>
        static ChoirHostRules Play(int n, int seed, int correctNotes)
        {
            ChoirHostRules rules = new ChoirHostRules(n, seed, new FakeChoirWorld());
            for (int i = 0; i < correctNotes; i++)
            {
                int pad = rules.Round.SolutionPad(rules.State.Progress);
                Assert.IsTrue(rules.Validate(i % n, ChoirCommand.Press(pad)).IsAccepted);
                rules.Apply(i % n, ChoirCommand.Press(pad));
            }
            return rules;
        }

        [Test]
        public void Command_RoundTrips()
        {
            for (int pad = 0; pad < 4; pad++)
            {
                ChoirCommand back;
                Assert.IsTrue(ChoirCodec.TryDecodeCommand(ChoirCodec.EncodeCommand(ChoirCommand.Press(pad)), out back));
                Assert.AreEqual(ChoirCommandKind.Press, back.Kind);
                Assert.AreEqual(pad, back.Pad);
            }
            ChoirCommand retry;
            Assert.IsTrue(ChoirCodec.TryDecodeCommand(ChoirCodec.EncodeCommand(ChoirCommand.Retry()), out retry));
            Assert.AreEqual(ChoirCommandKind.Retry, retry.Kind);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("0")]
        [TestCase("0|4")]
        [TestCase("0|-1")]
        [TestCase("2|0")]
        [TestCase("1|1")]
        [TestCase("0|0|0")]
        [TestCase("0|x")]
        [TestCase("00000000000000000|0")]
        public void Command_RejectsGarbage(string text)
        {
            ChoirCommand c;
            Assert.IsFalse(ChoirCodec.TryDecodeCommand(text, out c));
            Assert.IsNull(c);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_RoundTrips_AtEveryProgress(int n)
        {
            for (int seed = 1; seed <= 20; seed++)
            {
                int notes = ChoirGenerator.NoteCount;
                for (int k = 0; k <= notes; k++)
                {
                    ChoirPublicState before = Play(n, seed, k).BuildPublicState();
                    string text = ChoirCodec.EncodePublic(before);
                    ChoirPublicState after;
                    Assert.IsTrue(ChoirCodec.TryDecodePublic(text, out after), "n=" + n + " seed=" + seed + " k=" + k + ": " + text);
                    Assert.AreEqual(text, ChoirCodec.EncodePublic(after));
                    Assert.AreEqual(before.PlayerCount, after.PlayerCount);
                    Assert.AreEqual(before.Progress, after.Progress);
                    Assert.AreEqual(before.Solved, after.Solved);
                    Assert.AreEqual(before.WindowSeconds, after.WindowSeconds, 1e-9);
                    CollectionAssert.AreEqual(before.SymbolsCopy(), after.SymbolsCopy());
                }
            }
        }

        [Test]
        public void Public_RejectsMalformedAndInconsistent()
        {
            string good = ChoirCodec.EncodePublic(Play(3, 4, 2).BuildPublicState());
            ChoirPublicState s;
            Assert.IsTrue(ChoirCodec.TryDecodePublic(good, out s));
            string[] bad =
            {
                null,
                "",
                "rác",
                good + "|0",
                good.Substring(0, good.LastIndexOf('|')),
                Mutate(good, 0, "5"),
                Mutate(good, 0, "1"),
                Mutate(good, 1, "0"),
                Mutate(good, 1, "01234"),
                Mutate(good, 1, "01x301"),
                Mutate(good, 1, "0123012301230"),
                Mutate(good, 2, "99"),
                Mutate(good, 2, "-1"),
                Mutate(good, 3, "-1"),
                Mutate(good, 4, "-1"),
                Mutate(good, 5, "2"),
                Mutate(good, 5, "1"),
                Mutate(good, 6, "-5"),
                Mutate(good, 6, "999999999"),
                Mutate(good, 7, "-1"),
                Mutate(good, 8, "99"),
                Mutate(good, 9, "9"),
                Mutate(good, 10, "9"),
                Mutate(good, 10, "-2"),
                new string('1', ChoirCodec.MaxChars + 1)
            };
            foreach (string text in bad)
            {
                ChoirPublicState decoded;
                Assert.IsFalse(ChoirCodec.TryDecodePublic(text, out decoded), "Phải bác: " + text);
                Assert.IsNull(decoded);
            }
        }

        [Test]
        public void Public_RejectsSolvedWithWrongMessage_AndFullProgressNotSolved()
        {
            string solved = ChoirCodec.EncodePublic(Play(2, 6, ChoirGenerator.NoteCount).BuildPublicState());
            ChoirPublicState s;
            Assert.IsTrue(ChoirCodec.TryDecodePublic(solved, out s));
            Assert.IsTrue(s.Solved);
            Assert.IsFalse(ChoirCodec.TryDecodePublic(Mutate(solved, 8, ((int)ChoirMessage.Correct).ToString()), out s), "Thắng mà thông báo không phải Completed.");
            Assert.IsFalse(ChoirCodec.TryDecodePublic(Mutate(solved, 5, "0"), out s), "Hết dãy mà chưa thắng.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RoundTrips_ForEverySlot(int n)
        {
            ChoirHostRules rules = new ChoirHostRules(n, 13, new FakeChoirWorld());
            for (int slot = 0; slot < n; slot++)
            {
                ChoirPrivateState before = rules.BuildPrivateState(slot);
                string text = ChoirCodec.EncodePrivate(before);
                ChoirPrivateState after;
                Assert.IsTrue(ChoirCodec.TryDecodePrivate(text, out after), text);
                CollectionAssert.AreEqual(before.SymbolsCopy(), after.SymbolsCopy());
                CollectionAssert.AreEqual(before.PadsCopy(), after.PadsCopy());
                Assert.AreEqual(text, ChoirCodec.EncodePrivate(after));
            }
        }

        [Test]
        public void Private_EmptyIsValid()
        {
            ChoirPrivateState s;
            Assert.IsTrue(ChoirCodec.TryDecodePrivate("o:", out s));
            Assert.AreEqual(0, s.OwnedCount);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("x")]
        [TestCase("0=1")]
        [TestCase("o:0")]
        [TestCase("o:0=0,0=1")]
        [TestCase("o:1=0,0=1")]
        [TestCase("o:0=0,1=0")]
        [TestCase("o:4=0")]
        [TestCase("o:0=4")]
        [TestCase("o:-1=0")]
        [TestCase("o:0=0,1=1,2=2,3=3,3=0")]
        [TestCase("o:0=a")]
        public void Private_RejectsGarbage(string text)
        {
            ChoirPrivateState s;
            Assert.IsFalse(ChoirCodec.TryDecodePrivate(text, out s), "Phải bác: " + text);
            Assert.AreEqual(0, s.OwnedCount);
        }

        [Test]
        public void States_RejectInvalidConstruction()
        {
            int[] song = { 0, 1, 2, 3, 0, 1 };
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirPublicState(1, song, 0, 0, 0, false, 2.0, 0, ChoirMessage.None, -1, -1));
            Assert.Throws<ArgumentException>(() => new ChoirPublicState(2, new[] { 0 }, 0, 0, 0, false, 2.0, 0, ChoirMessage.None, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirPublicState(2, new[] { 0, 4 }, 0, 0, 0, false, 2.0, 0, ChoirMessage.None, -1, -1));
            Assert.Throws<ArgumentException>(() => new ChoirPublicState(2, song, 6, 0, 0, false, 2.0, 0, ChoirMessage.Completed, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChoirPublicState(2, song, 0, 0, 0, false, double.NaN, 0, ChoirMessage.None, -1, -1));
            Assert.Throws<ArgumentException>(() => new ChoirPrivateState(new[] { 0, 1 }, new[] { 0 }));
        }
    }
}
