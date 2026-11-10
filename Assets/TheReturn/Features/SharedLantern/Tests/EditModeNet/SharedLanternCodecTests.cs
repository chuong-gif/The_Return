/*
 * Mục đích: Kiểm thử bộ chuyển chuỗi (SharedLanternCodec) của «Đèn lồng chung» online (T-037): đi-về nguyên vẹn cho lệnh, trạng thái chung (suốt một ván
 * kịch bản: đọc, trao đèn, nhập, nhập sai, bị đưa về, thắng) và sổ riêng ở đội 2/3/4, và MỌI chuỗi sai từ mạng (rác, thiếu/thừa trường, ngoài khoảng,
 * mặt nạ sai, thắng mà bảng chưa đúng, ký hiệu trùng trong sổ) đều bị từ chối bằng false, không ném lỗi.
 * Hàm: các test trong SharedLanternCodecTests.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.LanternNet.Tests
{
    [TestFixture]
    public sealed class SharedLanternCodecTests
    {
        /// <summary>Nhận chuỗi trạng thái chung, chỉ số trường và giá trị mới; trả chuỗi đã thay trường đó.</summary>
        static string Mutate(string text, int field, string value)
        {
            string[] parts = text.Split('|');
            parts[field] = value;
            return string.Join("|", parts);
        }

        /// <summary>Nhận luật, ô và lệnh; bắt buộc Validate chấp nhận rồi Apply.</summary>
        static void Do(SharedLanternHostRules rules, int slot, SharedLanternCommand command)
        {
            Assert.IsTrue(rules.Validate(slot, command).IsAccepted, "Lệnh " + command.Kind + " bị từ chối.");
            rules.Apply(slot, command);
        }

        /// <summary>Nhận số người và seed; trả các chuỗi trạng thái chung chụp dọc một ván kịch bản đủ mọi loại thông báo.</summary>
        static List<string> Script(int n, int seed)
        {
            List<string> states = new List<string>();
            FakeLanternWorld world = new FakeLanternWorld();
            SharedLanternHostRules rules = new SharedLanternHostRules(n, seed, world);
            int[] solution = rules.Round.Solution();
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            Do(rules, 0, SharedLanternCommand.Read(0));
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            Do(rules, 0, SharedLanternCommand.Pass(1));
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            world.OutsideSlots.Add(0);
            rules.Tick(1f);
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            rules.Tick(6f);
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            world.OutsideSlots.Clear();
            rules.Tick(0.1f);
            Do(rules, 1, SharedLanternCommand.SetEntry(0, solution[0]));
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            Do(rules, 1, SharedLanternCommand.ClearEntries());
            for (int i = 0; i < 4; i++) Do(rules, 1, SharedLanternCommand.SetEntry(i, (solution[i] + 1) % 6));
            Do(rules, 1, SharedLanternCommand.Submit());
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            rules.Tick(11f);
            for (int i = 0; i < 4; i++) Do(rules, 1, SharedLanternCommand.SetEntry(i, solution[i]));
            world.AllGate = false;
            Do(rules, 1, SharedLanternCommand.Submit());
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            world.AllGate = true;
            rules.Tick(0.1f);
            states.Add(SharedLanternCodec.EncodePublic(rules.BuildPublicState()));
            return states;
        }

        [Test]
        public void Command_RoundTrips()
        {
            List<SharedLanternCommand> all = new List<SharedLanternCommand>();
            for (int i = 0; i < 4; i++) { all.Add(SharedLanternCommand.Pass(i)); all.Add(SharedLanternCommand.Read(i)); }
            for (int slot = 0; slot < 4; slot++)
                for (int symbol = 0; symbol < 6; symbol++) all.Add(SharedLanternCommand.SetEntry(slot, symbol));
            all.Add(SharedLanternCommand.ClearEntries());
            all.Add(SharedLanternCommand.Submit());
            all.Add(SharedLanternCommand.Retry());
            foreach (SharedLanternCommand c in all)
            {
                SharedLanternCommand back;
                Assert.IsTrue(SharedLanternCodec.TryDecodeCommand(SharedLanternCodec.EncodeCommand(c), out back), SharedLanternCodec.EncodeCommand(c));
                Assert.AreEqual(c.Kind, back.Kind);
                Assert.AreEqual(c.Index, back.Index);
                Assert.AreEqual(c.Symbol, back.Symbol);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("0")]
        [TestCase("0|1")]
        [TestCase("0|4|0")]
        [TestCase("0|-1|0")]
        [TestCase("0|0|1")]
        [TestCase("1|4|0")]
        [TestCase("1|0|1")]
        [TestCase("2|4|0")]
        [TestCase("2|-1|0")]
        [TestCase("2|0|6")]
        [TestCase("2|0|-1")]
        [TestCase("3|1|0")]
        [TestCase("3|0|1")]
        [TestCase("4|0|1")]
        [TestCase("4|1|0")]
        [TestCase("5|1|0")]
        [TestCase("6|0|0")]
        [TestCase("-1|0|0")]
        [TestCase("0|0|0|0")]
        [TestCase("0|x|0")]
        [TestCase("0|0|y")]
        [TestCase("00000000000000000|0|0")]
        public void Command_RejectsGarbage(string text)
        {
            SharedLanternCommand c;
            Assert.IsFalse(SharedLanternCodec.TryDecodeCommand(text, out c), "Phải bác: " + text);
            Assert.IsNull(c);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Public_RoundTrips_AlongAWholeGame(int n)
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (string text in Script(n, seed))
                {
                    SharedLanternPublicState after;
                    Assert.IsTrue(SharedLanternCodec.TryDecodePublic(text, out after), "n=" + n + " seed=" + seed + ": " + text);
                    Assert.AreEqual(text, SharedLanternCodec.EncodePublic(after));
                }
            }
        }

        [Test]
        public void Public_ScriptCoversEveryMessageKind()
        {
            HashSet<int> kinds = new HashSet<int>();
            foreach (string text in Script(3, 5))
            {
                SharedLanternPublicState s;
                Assert.IsTrue(SharedLanternCodec.TryDecodePublic(text, out s));
                kinds.Add((int)s.LastMessage);
            }
            foreach (SharedLanternMessage m in new[] { SharedLanternMessage.None, SharedLanternMessage.Passed, SharedLanternMessage.Read, SharedLanternMessage.EntrySet,
                SharedLanternMessage.WrongCode, SharedLanternMessage.CodeAccepted, SharedLanternMessage.SentBack, SharedLanternMessage.Solved })
                Assert.IsTrue(kinds.Contains((int)m), "Kịch bản chưa chạm thông báo " + m);
        }

        [Test]
        public void Public_RejectsMalformedAndInconsistent()
        {
            SharedLanternHostRules rules = new SharedLanternHostRules(3, 4, new FakeLanternWorld());
            Do(rules, 0, SharedLanternCommand.SetEntry(0, 2));
            string good = SharedLanternCodec.EncodePublic(rules.BuildPublicState());
            SharedLanternPublicState s;
            Assert.IsTrue(SharedLanternCodec.TryDecodePublic(good, out s), good);
            string solvedMsg = ((int)SharedLanternMessage.Solved).ToString();
            string[] bad =
            {
                null,
                "",
                "rác",
                good + "|0",
                good.Substring(0, good.LastIndexOf('|')),
                Mutate(good, 0, "5"),
                Mutate(good, 0, "1"),
                Mutate(good, 1, "3"),
                Mutate(good, 1, "-1"),
                Mutate(good, 2, "99"),
                Mutate(good, 2, "-1"),
                Mutate(good, 2, "1"),
                Mutate(good, 3, "-1"),
                Mutate(good, 4, "99"),
                Mutate(good, 4, "1"),
                Mutate(good, 5, ""),
                Mutate(good, 5, "..."),
                Mutate(good, 5, "....."),
                Mutate(good, 5, "7..."),
                Mutate(good, 5, "x..."),
                Mutate(good, 6, "2"),
                Mutate(good, 6, "1"),
                Mutate(good, 7, "2"),
                Mutate(good, 7, "1"),
                Mutate(good, 8, "-1"),
                Mutate(good, 9, "0"),
                Mutate(good, 9, "-5"),
                Mutate(good, 9, "999999999"),
                Mutate(good, 10, "-1"),
                Mutate(good, 10, "999999999"),
                Mutate(good, 11, "-1"),
                Mutate(good, 12, "99"),
                Mutate(good, 12, solvedMsg),
                Mutate(good, 13, "9"),
                Mutate(good, 13, "-2"),
                Mutate(good, 14, "9"),
                Mutate(good, 14, "-2"),
                new string('1', SharedLanternCodec.MaxChars + 1)
            };
            foreach (string text in bad)
            {
                SharedLanternPublicState decoded;
                Assert.IsFalse(SharedLanternCodec.TryDecodePublic(text, out decoded), "Phải bác: " + text);
                Assert.IsNull(decoded);
            }
        }

        [Test]
        public void Public_RejectsSolvedMismatches()
        {
            string[] states = Script(2, 8).ToArray();
            string solved = states[states.Length - 1];
            SharedLanternPublicState s;
            Assert.IsTrue(SharedLanternCodec.TryDecodePublic(solved, out s));
            Assert.IsTrue(s.Solved);
            Assert.IsFalse(SharedLanternCodec.TryDecodePublic(Mutate(solved, 7, "0"), out s), "Thông báo Solved mà cờ thắng tắt.");
            Assert.IsFalse(SharedLanternCodec.TryDecodePublic(Mutate(solved, 6, "0"), out s), "Thắng mà bảng chưa đúng.");
            Assert.IsFalse(SharedLanternCodec.TryDecodePublic(Mutate(solved, 12, ((int)SharedLanternMessage.Retry).ToString()), out s), "Thắng mà thông báo không phải Solved.");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Private_RoundTrips_ForEverySlot(int n)
        {
            SharedLanternHostRules rules = new SharedLanternHostRules(n, 13, new FakeLanternWorld());
            for (int sign = 0; sign < 4; sign++) Do(rules, sign % n, SharedLanternCommand.Read(sign));
            for (int slot = 0; slot < n; slot++)
            {
                SharedLanternPrivateState before = rules.BuildPrivateState(slot);
                string text = SharedLanternCodec.EncodePrivate(before);
                SharedLanternPrivateState after;
                Assert.IsTrue(SharedLanternCodec.TryDecodePrivate(text, out after), text);
                CollectionAssert.AreEqual(before.SignsCopy(), after.SignsCopy());
                CollectionAssert.AreEqual(before.SymbolsCopy(), after.SymbolsCopy());
                Assert.AreEqual(text, SharedLanternCodec.EncodePrivate(after));
            }
        }

        [Test]
        public void Private_EmptyIsValid()
        {
            SharedLanternPrivateState s;
            Assert.IsTrue(SharedLanternCodec.TryDecodePrivate("o:", out s));
            Assert.AreEqual(0, s.ReadCount);
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
        [TestCase("o:0=6")]
        [TestCase("o:-1=0")]
        [TestCase("o:0=0,1=1,2=2,3=3,3=4")]
        [TestCase("o:0=a")]
        [TestCase("o:0=1=2")]
        public void Private_RejectsGarbage(string text)
        {
            SharedLanternPrivateState s;
            Assert.IsFalse(SharedLanternCodec.TryDecodePrivate(text, out s), "Phải bác: " + text);
            Assert.AreEqual(0, s.ReadCount);
        }

        [Test]
        public void States_RejectInvalidConstruction()
        {
            int[] empty = { -1, -1, -1, -1 };
            int[] full = { 0, 1, 2, 3 };
            SharedLanternMessage none = SharedLanternMessage.None;
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(1, 0, 0, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 2, 0, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 1, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, -1, -1), "Người cầm đèn nằm trong mặt nạ ngoài sáng.");
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 4, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 0, 0, 1, empty, false, false, 0, 5.0, 10.0, 0, none, -1, -1), "Chưa ai bị đưa về mà mặt nạ khác rỗng.");
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, new[] { -1, -1, -1 }, false, false, 0, 5.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, new[] { 6, -1, -1, -1 }, false, false, 0, 5.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, true, false, 0, 5.0, 10.0, 0, none, -1, -1), "Đúng mà ô còn trống.");
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, full, false, true, 0, 5.0, 10.0, 0, SharedLanternMessage.Solved, -1, -1), "Thắng mà chưa đúng.");
            Assert.Throws<ArgumentException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, full, true, false, 0, 5.0, 10.0, 0, SharedLanternMessage.Solved, -1, -1), "Thông báo Solved mà chưa thắng.");
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, false, false, 0, 0.0, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, false, false, 0, double.NaN, 10.0, 0, none, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, false, false, 0, 5.0, -1.0, 0, none, -1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, 2, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SharedLanternPublicState(2, 0, 0, 0, 0, empty, false, false, 0, 5.0, 10.0, 0, none, -1, 6));
            Assert.Throws<ArgumentException>(() => new SharedLanternPrivateState(new[] { 0, 1 }, new[] { 0 }));
        }
    }
}
