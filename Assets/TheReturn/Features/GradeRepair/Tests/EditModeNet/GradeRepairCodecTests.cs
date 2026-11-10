/*
 * Mục đích: Kiểm thử Codec «Sửa bảng điểm» (T-036): mã hóa rồi giải mã ra đúng như cũ; chuỗi sai từ mạng bị từ chối bằng false và KHÔNG BAO GIỜ ném lỗi.
 * Các kiểm tra chung (fuzz, độ dài, round-trip theo mọi trạng thái trong ván) đã có ở bộ hợp đồng; file này chỉ ghim các quy tắc RIÊNG của nhiệm vụ này
 * (khoảng giá trị lệnh, bất biến «đã thắng thì phải đọc đủ và dùng đủ phiếu», sự kiện tài liệu phải là ASCII).
 * Hàm: các test trong GradeRepairCodecTests.
 */
using System;
using NUnit.Framework;

namespace TheReturn.GradeRepairNet.Tests
{
    [TestFixture]
    public sealed class GradeRepairCodecTests
    {
        static GradeRepairHostRules NewRules(int n, int seed = 11)
        {
            return new GradeRepairHostRules(n, seed, new FakeGradeRepairWorld());
        }

        static string Mutate(string text, int index, string value)
        {
            string[] f = text.Split('|');
            f[index] = value;
            return string.Join("|", f);
        }

        static void Play(GradeRepairHostRules r, int slot, GradeRepairCommand command)
        {
            Assert.IsTrue(r.Validate(slot, command).IsAccepted);
            r.Apply(slot, command);
        }

        static void AssertPublicEqual(GradeRepairPublicState a, GradeRepairPublicState b)
        {
            Assert.AreEqual(a.PlayerCount, b.PlayerCount);
            CollectionAssert.AreEqual(a.ScoresCopy(), b.ScoresCopy());
            Assert.AreEqual(a.UsedMask, b.UsedMask);
            Assert.AreEqual(a.ReadMask, b.ReadMask);
            Assert.AreEqual(a.Solved, b.Solved);
            Assert.AreEqual(a.ResetCount, b.ResetCount);
            Assert.AreEqual(a.LastEvent, b.LastEvent);
            Assert.AreEqual(a.LastResult, b.LastResult);
            Assert.AreEqual(a.LastActor, b.LastActor);
        }

        [Test]
        public void Command_RoundTrip_AllKinds()
        {
            GradeRepairCommand[] all =
            {
                GradeRepairCommand.Read(0), GradeRepairCommand.Read(3), GradeRepairCommand.Apply(0, -1, 2), GradeRepairCommand.Apply(2, 3, 0),
                GradeRepairCommand.Undo(), GradeRepairCommand.Submit(), GradeRepairCommand.Reset()
            };
            foreach (GradeRepairCommand c in all)
            {
                GradeRepairCommand back;
                Assert.IsTrue(GradeRepairCodec.TryDecodeCommand(GradeRepairCodec.EncodeCommand(c), out back));
                Assert.AreEqual(c.Kind, back.Kind);
                Assert.AreEqual(c.Index, back.Index);
                Assert.AreEqual(c.Source, back.Source);
                Assert.AreEqual(c.Target, back.Target);
            }
        }

        [Test]
        public void Command_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad =
            {
                null, "", "x", "0", "0|0|0", "0|0|0|0|0", "9|0|0|0", "-1|0|0|0",
                "0|4|0|0", "0|-1|0|0", "0|0|1|0", "0|0|0|1",
                "1|3|-1|0", "1|0|-2|0", "1|0|4|0", "1|0|0|4", "1|0|0|-1",
                "2|1|0|0", "3|0|1|0", "4|0|0|1", "0|a|0|0", "0|1.5|0|0", "0|99999999999|0|0",
                new string('1', 40)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                Assert.DoesNotThrow(() => { GradeRepairCommand tmp; ok = GradeRepairCodec.TryDecodeCommand(s, out tmp); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_RoundTrip_AcrossStates(int n)
        {
            GradeRepairHostRules r = NewRules(n);
            AssertRoundTrip(r.BuildPublicState());
            for (int d = 0; d < 4; d++) Play(r, r.Round.OwnerOf(d), GradeRepairCommand.Read(d));
            AssertRoundTrip(r.BuildPublicState());
            Play(r, 0, GradeRepairCommand.Apply(1, -1, r.Round.CorrectTarget(1)));
            AssertRoundTrip(r.BuildPublicState());
            Play(r, 0, GradeRepairCommand.Reset());
            int[] order = { 2, 0, 1 };
            foreach (int v in order) Play(r, 0, GradeRepairCommand.Apply(v, r.Round.CorrectSource(v), r.Round.CorrectTarget(v)));
            AssertRoundTrip(r.BuildPublicState());
            Play(r, 0, GradeRepairCommand.Submit());
            Assert.IsTrue(r.Solved);
            AssertRoundTrip(r.BuildPublicState());
        }

        static void AssertRoundTrip(GradeRepairPublicState s)
        {
            string text = GradeRepairCodec.EncodePublic(s);
            GradeRepairPublicState back;
            Assert.IsTrue(GradeRepairCodec.TryDecodePublic(text, out back), "Không giải mã lại được: " + text);
            AssertPublicEqual(s, back);
            Assert.AreEqual(text, GradeRepairCodec.EncodePublic(back));
        }

        [Test]
        public void Public_Malformed_ReturnsFalse_NeverThrows()
        {
            string good = GradeRepairCodec.EncodePublic(NewRules(3).BuildPublicState());
            string[] bad =
            {
                null, "", "x", "||||||||", good + "|0", good.Substring(0, good.LastIndexOf('|')),
                Mutate(good, 0, "1"), Mutate(good, 0, "5"), Mutate(good, 0, "x"),
                Mutate(good, 1, "1,2,3"), Mutate(good, 1, "1,2,3,4,5"), Mutate(good, 1, "1,2,3,11"), Mutate(good, 1, "1,2,3,-1"), Mutate(good, 1, "a,b,c,d"),
                Mutate(good, 2, "8"), Mutate(good, 2, "-1"), Mutate(good, 3, "16"), Mutate(good, 3, "-1"),
                Mutate(good, 4, "2"), Mutate(good, 4, "true"), Mutate(good, 4, "1"),
                Mutate(good, 5, "-1"), Mutate(good, 5, "99999999"),
                Mutate(good, 6, "99"), Mutate(good, 6, "-1"), Mutate(good, 7, "99"), Mutate(good, 7, "x"),
                Mutate(good, 8, "3"), Mutate(good, 8, "-2"),
                new string('1', GradeRepairCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                GradeRepairPublicState decoded = null;
                Assert.DoesNotThrow(() => { ok = GradeRepairCodec.TryDecodePublic(s, out decoded); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
                Assert.IsNull(decoded);
            }
        }

        [Test]
        public void Public_Solved_WithoutEvidenceOrVouchers_IsRejected()
        {
            string good = GradeRepairCodec.EncodePublic(NewRules(2).BuildPublicState());
            GradeRepairPublicState s;
            Assert.IsFalse(GradeRepairCodec.TryDecodePublic(Mutate(good, 4, "1"), out s), "thắng mà chưa đọc/dùng gì");
            Assert.IsFalse(GradeRepairCodec.TryDecodePublic(Mutate(Mutate(good, 4, "1"), 3, "15"), out s), "thắng mà chưa dùng phiếu");
            Assert.IsFalse(GradeRepairCodec.TryDecodePublic(Mutate(Mutate(good, 4, "1"), 2, "7"), out s), "thắng mà chưa đọc đủ");
            Assert.IsTrue(GradeRepairCodec.TryDecodePublic(Mutate(Mutate(Mutate(good, 4, "1"), 2, "7"), 3, "15"), out s), "đủ cả hai thì hợp lệ");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RoundTrip_EverySlot(int n)
        {
            GradeRepairHostRules r = NewRules(n);
            for (int d = 0; d < 4; d++) Play(r, r.Round.OwnerOf(d), GradeRepairCommand.Read(d));
            for (int slot = 0; slot < n; slot++)
            {
                GradeRepairPrivateState p = r.BuildPrivateState(slot);
                string text = GradeRepairCodec.EncodePrivate(p);
                GradeRepairPrivateState back;
                Assert.IsTrue(GradeRepairCodec.TryDecodePrivate(text, out back), text);
                CollectionAssert.AreEqual(p.DocumentsCopy(), back.DocumentsCopy());
                CollectionAssert.AreEqual(p.FactsCopy(), back.FactsCopy());
                Assert.AreEqual(text, GradeRepairCodec.EncodePrivate(back));
            }
        }

        [Test]
        public void Private_Empty_IsValid()
        {
            GradeRepairPrivateState p;
            Assert.IsTrue(GradeRepairCodec.TryDecodePrivate("g:", out p));
            Assert.AreEqual(0, p.OwnedCount);
            Assert.AreEqual("g:", GradeRepairCodec.EncodePrivate(new GradeRepairPrivateState(null, null)));
        }

        [Test]
        public void Private_Malformed_ReturnsFalse_NeverThrows()
        {
            string[] bad =
            {
                null, "", "x", "g", "0=C%3AHS-111", "g:0", "g:0=", "g:=a", "g:4=a", "g:-1=a", "g:a=b",
                "g:1=a,0=b", "g:0=a,0=b", "g:0=a,", ",g:0=a", "g:0=a=b", "g:0=a,1=b,2=c,3=d,4=e",
                "g:0=%C3%A9", "g:0=%01", "g:0=" + new string('a', 101), "g:0=%", "g:0=%zz",
                new string('g', GradeRepairCodec.MaxChars + 1)
            };
            foreach (string s in bad)
            {
                bool ok = true;
                Assert.DoesNotThrow(() => { GradeRepairPrivateState tmp; ok = GradeRepairCodec.TryDecodePrivate(s, out tmp); });
                Assert.IsFalse(ok, "Chuỗi lẽ ra bị từ chối: " + (s ?? "null"));
            }
        }

        [Test]
        public void Fuzz_RandomGarbage_NeverThrows()
        {
            Random rnd = new Random(20261107);
            const string alphabet = "0123456789|,=g:-.%> xXCVT";
            for (int i = 0; i < 4000; i++)
            {
                int len = rnd.Next(0, 70);
                char[] chars = new char[len];
                for (int k = 0; k < len; k++) chars[k] = alphabet[rnd.Next(alphabet.Length)];
                string s = new string(chars);
                Assert.DoesNotThrow(() =>
                {
                    GradeRepairCommand c;
                    GradeRepairPublicState pub;
                    GradeRepairPrivateState priv;
                    GradeRepairCodec.TryDecodeCommand(s, out c);
                    GradeRepairCodec.TryDecodePublic(s, out pub);
                    GradeRepairCodec.TryDecodePrivate(s, out priv);
                }, "Chuỗi gây lỗi: " + s);
            }
        }
    }
}
