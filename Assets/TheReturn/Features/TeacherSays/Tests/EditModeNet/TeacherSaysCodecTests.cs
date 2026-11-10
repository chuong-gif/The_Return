/*
 * Mục đích: Kiểm thử Codec «Cô nói» (T-037): mã hóa rồi giải mã phải ra đúng như cũ; chuỗi sai/độc hại từ mạng phải bị từ chối bằng false
 * và KHÔNG BAO GIỜ ném lỗi (máy khách xấu không được làm sập host). Có fuzz ngẫu nhiên cố định seed.
 * Hàm: các test trong TeacherSaysCodecTests.
 */
using System;
using System.Text;
using NUnit.Framework;

namespace TheReturn.TeacherSaysNet.Tests
{
    [TestFixture]
    public sealed class TeacherSaysCodecTests
    {
        static string Mutate(string text, int index, string value)
        {
            string[] f = text.Split('|');
            f[index] = value;
            return string.Join("|", f);
        }

        static void AssertPublicEqual(TeacherSaysPublicState a, TeacherSaysPublicState b)
        {
            Assert.AreEqual(a.PlayerCount, b.PlayerCount);
            Assert.AreEqual(a.Started, b.Started);
            Assert.AreEqual(a.Solved, b.Solved);
            Assert.AreEqual(a.Segment, b.Segment);
            Assert.AreEqual(a.Command, b.Command);
            Assert.AreEqual(a.Listener, b.Listener);
            Assert.AreEqual(a.Warnings, b.Warnings);
            Assert.AreEqual(a.MaxWarnings, b.MaxWarnings);
            Assert.AreEqual(a.SentBacks, b.SentBacks);
            Assert.AreEqual(a.TotalWarnings, b.TotalWarnings);
            Assert.AreEqual(a.RetryCount, b.RetryCount);
            Assert.AreEqual(a.LastEvent, b.LastEvent);
        }

        static void AssertRoundTrip(TeacherSaysPublicState s)
        {
            string text = TeacherSaysCodec.EncodePublic(s);
            TeacherSaysPublicState back;
            Assert.IsTrue(TeacherSaysCodec.TryDecodePublic(text, out back), "Không giải mã lại được: " + text);
            AssertPublicEqual(s, back);
            Assert.AreEqual(text, TeacherSaysCodec.EncodePublic(back));
        }

        [Test]
        public void Command_RoundTrip()
        {
            TeacherSaysCommand c;
            Assert.IsTrue(TeacherSaysCodec.TryDecodeCommand(TeacherSaysCodec.EncodeCommand(TeacherSaysCommand.Retry()), out c));
            Assert.AreEqual(TeacherSaysCommandKind.Retry, c.Kind);
            Assert.AreEqual("0", TeacherSaysCodec.EncodeCommand(TeacherSaysCommand.Retry()));
        }

        [Test]
        public void Command_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad = { null, "", "1", "-1", "00", " 0", "0 ", "+0", "0|0", "x", "0.0", "9", "99999999999", new string('0', 5000) };
            foreach (string s in bad)
            {
                bool ok = true;
                TeacherSaysCommand decoded = null;
                Assert.DoesNotThrow(() => { ok = TeacherSaysCodec.TryDecodeCommand(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s == null ? "null" : (s.Length > 20 ? s.Substring(0, 20) + "..." : s)));
                Assert.IsNull(decoded);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_RoundTrip_AcrossStates(int n)
        {
            FakeTeacherSaysWorld world = new FakeTeacherSaysWorld();
            TeacherSaysHostRules r = new TeacherSaysHostRules(TeacherSaysHostRulesTests.MakeRound(n), world);
            AssertRoundTrip(r.BuildPublicState());

            r.Begin();
            AssertRoundTrip(r.BuildPublicState());

            world.OutOfZone.Add(0);
            Assert.IsTrue(r.Tick(8.1f));
            AssertRoundTrip(r.BuildPublicState());
            Assert.IsTrue(r.Tick(8.1f));
            Assert.IsTrue(r.Tick(8.1f));
            Assert.AreEqual(1, r.SentBacks);
            AssertRoundTrip(r.BuildPublicState());

            world.OutOfZone.Clear();
            Assert.IsTrue(r.Tick(0.2f));
            AssertRoundTrip(r.BuildPublicState());
            Assert.IsTrue(r.Tick(0.2f));
            Assert.IsTrue(r.Tick(5.1f));
            AssertRoundTrip(r.BuildPublicState());

            Assert.IsTrue(r.Validate(0, TeacherSaysCommand.Retry()).IsAccepted);
            r.Apply(0, TeacherSaysCommand.Retry());
            AssertRoundTrip(r.BuildPublicState());

            for (int i = 0; i < TeacherSaysRound.SegmentCount; i++)
            {
                if (r.Round.RequiredAction(r.State.Segment) == TeacherCommand.Step) r.Tick(0.2f);
                else { r.Tick(5.1f); r.Tick(0.1f); }
            }
            Assert.IsTrue(r.Solved);
            AssertRoundTrip(r.BuildPublicState());
        }

        [Test]
        public void Public_Malformed_ReturnsFalse_NeverThrows()
        {
            TeacherSaysHostRules r = new TeacherSaysHostRules(TeacherSaysHostRulesTests.MakeRound(3), new FakeTeacherSaysWorld());
            r.Begin();
            string good = TeacherSaysCodec.EncodePublic(r.BuildPublicState());
            TeacherSaysPublicState tmp;
            Assert.IsTrue(TeacherSaysCodec.TryDecodePublic(good, out tmp));
            Assert.AreEqual("3|1|0|0|0|0|0|3|0|0|0|1", good, "Mẫu chuẩn để so các biến thể sai.");

            string[] bad =
            {
                null, "", "x", "|||||||||||", good + "|0", good.Substring(0, good.LastIndexOf('|')),
                Mutate(good, 0, "1"), Mutate(good, 0, "5"), Mutate(good, 0, "x"), Mutate(good, 0, ""),
                Mutate(good, 1, "2"), Mutate(good, 1, "true"), Mutate(good, 1, "-1"),
                Mutate(good, 2, "2"), Mutate(good, 2, "1"),
                Mutate(good, 3, "6"), Mutate(good, 3, "-1"), Mutate(good, 3, "x"), Mutate(good, 3, "5"),
                Mutate(good, 4, "2"), Mutate(good, 4, "-1"), Mutate(good, 4, "x"),
                Mutate(good, 5, "-1"), Mutate(good, 5, "3"), Mutate(good, 5, "x"), Mutate(good, 5, "+0"),
                Mutate(good, 6, "3"), Mutate(good, 6, "-1"), Mutate(good, 6, "x"),
                Mutate(good, 7, "0"), Mutate(good, 7, "101"), Mutate(good, 7, "x"),
                Mutate(good, 8, "-1"), Mutate(good, 8, "1000001"), Mutate(good, 8, "x"),
                Mutate(good, 9, "-1"), Mutate(good, 9, "1000001"),
                Mutate(good, 10, "-1"), Mutate(good, 10, "1000001"),
                Mutate(good, 11, "8"), Mutate(good, 11, "-1"), Mutate(good, 11, "x"),
                Mutate(Mutate(good, 6, "2"), 9, "1"),
                Mutate(Mutate(good, 1, "0"), 3, "2"),
                Mutate(Mutate(good, 1, "0"), 5, "-1") + "|",
                Mutate(Mutate(good, 2, "1"), 3, "5"),
                new string('1', TeacherSaysCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                TeacherSaysPublicState decoded = null;
                Assert.DoesNotThrow(() => { ok = TeacherSaysCodec.TryDecodePublic(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s == null ? "null" : (s.Length > 40 ? s.Substring(0, 40) + "..." : s)));
                Assert.IsNull(decoded);
            }
        }

        [Test]
        public void Public_NotStarted_WithSegmentOrListener_Rejected()
        {
            TeacherSaysPublicState s;
            Assert.IsTrue(TeacherSaysCodec.TryDecodePublic("3|0|0|0|0|-1|0|3|0|0|0|0", out s));
            Assert.IsFalse(s.Active);
            Assert.IsFalse(TeacherSaysCodec.TryDecodePublic("3|0|0|0|0|1|0|3|0|0|0|0", out s), "Chưa bắt đầu thì không có người nghe.");
            Assert.IsFalse(TeacherSaysCodec.TryDecodePublic("3|0|0|1|0|-1|0|3|0|0|0|0", out s), "Chưa bắt đầu thì đoạn phải là 0.");
            Assert.IsFalse(TeacherSaysCodec.TryDecodePublic("3|0|1|5|0|-1|0|3|0|0|0|4", out s), "Thắng mà chưa bắt đầu.");
            Assert.IsTrue(TeacherSaysCodec.TryDecodePublic("3|1|1|5|0|-1|0|3|0|0|0|4", out s));
            Assert.IsTrue(s.Solved);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RoundTrip_EverySlot_EverySegment(int n)
        {
            TeacherSaysHostRules r = new TeacherSaysHostRules(TeacherSaysHostRulesTests.MakeRound(n), new FakeTeacherSaysWorld());
            r.Begin();
            for (int seg = 0; seg < TeacherSaysRound.SegmentCount; seg++)
            {
                for (int slot = 0; slot < n; slot++)
                {
                    TeacherSaysPrivateState p = r.BuildPrivateState(slot);
                    string text = TeacherSaysCodec.EncodePrivate(p);
                    TeacherSaysPrivateState back;
                    Assert.IsTrue(TeacherSaysCodec.TryDecodePrivate(text, out back), text);
                    Assert.AreEqual(p.IsListener, back.IsListener);
                    Assert.AreEqual(p.TeacherSaid, back.TeacherSaid);
                    Assert.AreEqual(p.RemainingMs, back.RemainingMs);
                    Assert.AreEqual(p.StandSatisfied, back.StandSatisfied);
                    Assert.AreEqual(text, TeacherSaysCodec.EncodePrivate(back));
                    if (!p.IsListener) Assert.AreEqual("o:", text, "Người không nghe chỉ nhận chuỗi rỗng, không lộ gì.");
                }
                if (r.Round.RequiredAction(r.State.Segment) == TeacherCommand.Step) r.Tick(0.2f);
                else { r.Tick(5.1f); r.Tick(0.1f); }
            }
        }

        [Test]
        public void Private_Empty_IsValid()
        {
            TeacherSaysPrivateState p;
            Assert.IsTrue(TeacherSaysCodec.TryDecodePrivate("o:", out p));
            Assert.IsFalse(p.IsListener);
            Assert.AreEqual("o:", TeacherSaysCodec.EncodePrivate(TeacherSaysPrivateState.None));
        }

        [Test]
        public void Private_Listener_NoLimit_RoundTrips()
        {
            TeacherSaysPrivateState p = new TeacherSaysPrivateState(true, true, -1, false);
            TeacherSaysPrivateState back;
            Assert.IsTrue(TeacherSaysCodec.TryDecodePrivate(TeacherSaysCodec.EncodePrivate(p), out back));
            Assert.AreEqual(-1, back.RemainingMs);
            Assert.AreEqual(-1f, back.RemainingSeconds, 0.0001f);
        }

        [Test]
        public void Private_Constructor_RejectsInfoForNonListener()
        {
            Assert.Throws<ArgumentException>(() => new TeacherSaysPrivateState(false, true, -1, false));
            Assert.Throws<ArgumentException>(() => new TeacherSaysPrivateState(false, false, 100, false));
            Assert.Throws<ArgumentException>(() => new TeacherSaysPrivateState(false, false, -1, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TeacherSaysPrivateState(true, true, -2, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TeacherSaysPrivateState(true, true, TeacherSaysPrivateState.MaxRemainingMs + 1, false));
        }

        [Test]
        public void Private_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad =
            {
                null, "", "x", "o", "o:1", "o:1|1|5", "o:1|1|5|0|0", "o:0|0|-1|0", "o:2|0|0|0", "o:1|2|0|0", "o:1|1|-2|0", "o:1|1|600001|0",
                "o:1|1|x|0", "o:1|1|5|2", "o:1|1||0", "o:|||", "o:1|1|+5|0", "o:1|1| 5|0", "O:1|1|5|0", " o:1|1|5|0", "o:1|1|5|0|",
                new string('o', TeacherSaysCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                TeacherSaysPrivateState decoded = TeacherSaysPrivateState.None;
                Assert.DoesNotThrow(() => { ok = TeacherSaysCodec.TryDecodePrivate(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s == null ? "null" : (s.Length > 40 ? s.Substring(0, 40) + "..." : s)));
                Assert.IsFalse(decoded.IsListener, "Giải mã hỏng phải trả trạng thái rỗng.");
            }
        }

        [Test]
        public void Fuzz_RandomStrings_NeverThrow_AndAcceptedOnesRoundTrip()
        {
            Random rng = new Random(20261105);
            const string alphabet = "0123456789|:=,-+ oxe";
            for (int i = 0; i < 4000; i++)
            {
                int len = rng.Next(0, 60);
                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < len; k++) sb.Append(alphabet[rng.Next(alphabet.Length)]);
                string s = sb.ToString();
                if (rng.Next(4) == 0) s = "o:" + s;
                Assert.DoesNotThrow(() =>
                {
                    TeacherSaysCommand c;
                    TeacherSaysCodec.TryDecodeCommand(s, out c);
                    TeacherSaysPublicState pub;
                    if (TeacherSaysCodec.TryDecodePublic(s, out pub)) AssertRoundTrip(pub);
                    TeacherSaysPrivateState priv;
                    if (TeacherSaysCodec.TryDecodePrivate(s, out priv))
                    {
                        TeacherSaysPrivateState again;
                        Assert.IsTrue(TeacherSaysCodec.TryDecodePrivate(TeacherSaysCodec.EncodePrivate(priv), out again));
                    }
                }, "Chuỗi fuzz làm Codec ném lỗi: " + s);
            }
        }
    }
}
