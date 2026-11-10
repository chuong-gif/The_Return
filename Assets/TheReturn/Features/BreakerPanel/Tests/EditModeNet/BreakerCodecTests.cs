/*
 * Mục đích: Kiểm thử Codec «Bảng cầu dao» (T-031): mã hóa rồi giải mã phải ra đúng như cũ; chuỗi sai/độc hại từ mạng phải bị từ chối bằng false
 * và KHÔNG BAO GIỜ ném lỗi (máy khách xấu không được làm sập host). Có fuzz ngẫu nhiên cố định seed.
 * Hàm: các test trong BreakerCodecTests.
 */
using System;
using System.Text;
using NUnit.Framework;

namespace TheReturn.BreakerNet.Tests
{
    [TestFixture]
    public sealed class BreakerCodecTests
    {
        static BreakerHostRules NewRules(int n, int seed = 11)
        {
            return new BreakerHostRules(n, seed, new FakeBreakerWorld(), 3f);
        }

        static string Mutate(string text, int index, string value)
        {
            string[] f = text.Split('|');
            f[index] = value;
            return string.Join("|", f);
        }

        static void AssertPublicEqual(BreakerPublicState a, BreakerPublicState b)
        {
            Assert.AreEqual(a.PlayerCount, b.PlayerCount);
            Assert.AreEqual(a.OnSwitches, b.OnSwitches);
            Assert.AreEqual(a.Lamps, b.Lamps);
            Assert.AreEqual(a.TargetLamps, b.TargetLamps);
            Assert.AreEqual(a.Trips, b.Trips);
            Assert.AreEqual(a.Solved, b.Solved);
            Assert.AreEqual(a.HoldElapsed, b.HoldElapsed, 0.001f);
            Assert.AreEqual(a.HoldSeconds, b.HoldSeconds, 0.001f);
            Assert.AreEqual(a.ResetCount, b.ResetCount);
            Assert.AreEqual(a.LastMessage, b.LastMessage);
            Assert.AreEqual(a.LastActor, b.LastActor);
            Assert.AreEqual(a.LastSwitch, b.LastSwitch);
        }

        static void ToggleAs(BreakerHostRules r, int sw)
        {
            int owner = r.Round.OwnerOf(sw);
            Assert.IsTrue(r.Validate(owner, BreakerCommand.Toggle(sw)).IsAccepted);
            r.Apply(owner, BreakerCommand.Toggle(sw));
        }

        [Test]
        public void Command_RoundTrip()
        {
            for (int sw = 0; sw < BreakerPublicState.MaxSize; sw++)
            {
                BreakerCommand c;
                Assert.IsTrue(BreakerCodec.TryDecodeCommand(BreakerCodec.EncodeCommand(BreakerCommand.Toggle(sw)), out c));
                Assert.AreEqual(BreakerCommandKind.Toggle, c.Kind);
                Assert.AreEqual(sw, c.Switch);
            }
            BreakerCommand retry;
            Assert.IsTrue(BreakerCodec.TryDecodeCommand(BreakerCodec.EncodeCommand(BreakerCommand.Retry()), out retry));
            Assert.AreEqual(BreakerCommandKind.Retry, retry.Kind);
        }

        [Test]
        public void Command_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad =
            {
                null, "", "x", "0", "0|", "|0", "0|6", "0|-1", "2|0", "-1|0", "1|1", "1|-1", "0|0|0",
                "0|1.5", "0|99999999999", "00000000000000000|0", "0|a", "a|0", "0 |0x1"
            };
            foreach (string s in bad)
            {
                BreakerCommand c = BreakerCommand.Retry();
                bool ok = true;
                Assert.DoesNotThrow(() => { BreakerCommand tmp; ok = BreakerCodec.TryDecodeCommand(s, out tmp); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
                Assert.IsNotNull(c);
            }
            BreakerCommand outCmd;
            Assert.IsFalse(BreakerCodec.TryDecodeCommand("9|9", out outCmd));
            Assert.IsNull(outCmd);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_RoundTrip_AcrossStates(int n)
        {
            BreakerHostRules r = NewRules(n);
            AssertRoundTrip(r.BuildPublicState());

            ToggleAs(r, 0);
            AssertRoundTrip(r.BuildPublicState());

            for (int sw = 1; sw <= r.Round.OverloadLimit; sw++) ToggleAs(r, sw);
            AssertRoundTrip(r.BuildPublicState());

            int sol = r.Round.SolutionSwitches;
            for (int sw = 0; sw < r.Round.Size; sw++) if (((sol >> sw) & 1) == 1) ToggleAs(r, sw);
            r.Tick(1.25f);
            AssertRoundTrip(r.BuildPublicState());
            r.Tick(0.1f);
            r.Tick(5f);
            Assert.IsTrue(r.Solved);
            AssertRoundTrip(r.BuildPublicState());
        }

        static void AssertRoundTrip(BreakerPublicState s)
        {
            string text = BreakerCodec.EncodePublic(s);
            BreakerPublicState back;
            Assert.IsTrue(BreakerCodec.TryDecodePublic(text, out back), "Không giải mã lại được: " + text);
            AssertPublicEqual(s, back);
            Assert.AreEqual(text, BreakerCodec.EncodePublic(back));
        }

        [Test]
        public void Public_Encoded_HasExactlyTwelveFields_NoSecrets()
        {
            BreakerHostRules r = NewRules(3, 123);
            string text = BreakerCodec.EncodePublic(r.BuildPublicState());
            Assert.AreEqual(12, text.Split('|').Length);
            Assert.IsFalse(text.Contains("123"), "Seed không được lộ ra trong trạng thái chung.");
        }

        [Test]
        public void Public_Malformed_ReturnsFalse_NeverThrows()
        {
            BreakerHostRules r = NewRules(3);
            string good = BreakerCodec.EncodePublic(r.BuildPublicState());
            BreakerPublicState tmp;
            Assert.IsTrue(BreakerCodec.TryDecodePublic(good, out tmp));
            Assert.IsFalse(tmp.Matching, "Đề đầu ván không được khớp sẵn.");

            string[] bad =
            {
                null, "", "x", "|||||||||||", good + "|0", good.Substring(0, good.LastIndexOf('|')),
                Mutate(good, 0, "1"), Mutate(good, 0, "5"), Mutate(good, 0, "x"),
                Mutate(good, 1, "-1"), Mutate(good, 1, "32"), Mutate(good, 1, "31"), Mutate(good, 1, "x"),
                Mutate(good, 2, "32"), Mutate(good, 2, "-1"), Mutate(good, 3, "32"), Mutate(good, 3, "-1"),
                Mutate(good, 4, "-1"), Mutate(good, 4, "99999999"), Mutate(good, 4, "x"),
                Mutate(good, 5, "2"), Mutate(good, 5, "true"), Mutate(good, 5, "1"),
                Mutate(good, 6, "-1"), Mutate(good, 6, "999999"), Mutate(good, 6, "x"),
                Mutate(good, 7, "-1"), Mutate(good, 7, "999999999"),
                Mutate(good, 8, "-1"), Mutate(good, 8, "99999999"),
                Mutate(good, 9, "99"), Mutate(good, 9, "-1"), Mutate(good, 9, "x"),
                Mutate(good, 10, "3"), Mutate(good, 10, "-2"), Mutate(good, 10, "x"),
                Mutate(good, 11, "5"), Mutate(good, 11, "-2"), Mutate(good, 11, "x"),
                new string('1', BreakerCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                BreakerPublicState decoded = null;
                Assert.DoesNotThrow(() => { ok = BreakerCodec.TryDecodePublic(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
                Assert.IsNull(decoded);
            }
        }

        [Test]
        public void Public_ElapsedBeyondHold_Rejected()
        {
            BreakerHostRules r = NewRules(2);
            string good = BreakerCodec.EncodePublic(r.BuildPublicState());
            string bad = Mutate(Mutate(good, 7, "1000"), 6, "1001");
            BreakerPublicState s;
            Assert.IsFalse(BreakerCodec.TryDecodePublic(bad, out s));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RoundTrip_EverySlot(int n)
        {
            BreakerHostRules r = NewRules(n);
            for (int slot = 0; slot < n; slot++)
            {
                BreakerPrivateState p = r.BuildPrivateState(slot);
                string text = BreakerCodec.EncodePrivate(p);
                BreakerPrivateState back;
                Assert.IsTrue(BreakerCodec.TryDecodePrivate(text, out back), text);
                CollectionAssert.AreEqual(p.SwitchesCopy(), back.SwitchesCopy());
                CollectionAssert.AreEqual(p.MasksCopy(), back.MasksCopy());
                Assert.AreEqual(text, BreakerCodec.EncodePrivate(back));
            }
        }

        [Test]
        public void Private_Empty_IsValid()
        {
            BreakerPrivateState p;
            Assert.IsTrue(BreakerCodec.TryDecodePrivate("o:", out p));
            Assert.AreEqual(0, p.OwnedCount);
            Assert.AreEqual("o:", BreakerCodec.EncodePrivate(new BreakerPrivateState(null, null)));
        }

        [Test]
        public void Private_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad =
            {
                null, "", "x", "o", "0=1", "o:0", "o:0=", "o:=1", "o:0=1,0=2", "o:1=1,0=2", "o:6=1", "o:-1=1",
                "o:0=64", "o:0=-1", "o:a=b", "o:0=1,", ",o:0=1", "o:0=1=2", "o:0=1,1=1,2=1,3=1,4=1,5=1,6=1",
                new string('o', BreakerCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                BreakerPrivateState decoded;
                Assert.DoesNotThrow(() => { ok = BreakerCodec.TryDecodePrivate(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
            }
        }

        [Test]
        public void Fuzz_RandomGarbage_NeverThrows()
        {
            Random rnd = new Random(20261103);
            const string alphabet = "0123456789|,=o:-. xX";
            for (int i = 0; i < 4000; i++)
            {
                int len = rnd.Next(0, 70);
                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < len; k++) sb.Append(alphabet[rnd.Next(alphabet.Length)]);
                string s = sb.ToString();
                Assert.DoesNotThrow(() =>
                {
                    BreakerCommand c;
                    BreakerPublicState pub;
                    BreakerPrivateState priv;
                    BreakerCodec.TryDecodeCommand(s, out c);
                    BreakerCodec.TryDecodePublic(s, out pub);
                    BreakerCodec.TryDecodePrivate(s, out priv);
                }, "Chuỗi gây lỗi: " + s);
            }
        }

        [Test]
        public void PublicState_InvalidConstruction_Throws()
        {
            Assert.Catch<ArgumentException>(() => new BreakerPublicState(1, 0, 0, 0, 0, false, 0f, 3f, 0, BreakerMessage.None, -1, -1));
            Assert.Catch<ArgumentException>(() => new BreakerPublicState(3, 31, 0, 0, 0, false, 0f, 3f, 0, BreakerMessage.None, -1, -1));
            Assert.Catch<ArgumentException>(() => new BreakerPublicState(3, 0, 1, 2, 0, true, 3f, 3f, 0, BreakerMessage.Solved, -1, -1));
            Assert.Catch<ArgumentException>(() => new BreakerPublicState(3, 0, 0, 0, 0, false, 4f, 3f, 0, BreakerMessage.None, -1, -1));
            Assert.Catch<ArgumentException>(() => new BreakerPublicState(3, 0, 0, 0, 0, false, 0f, 3f, 0, BreakerMessage.None, 3, -1));
        }
    }
}
