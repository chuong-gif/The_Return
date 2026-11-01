/*
 * Mục đích: NUnit EditMode test cho HostLog (T-024): nhật ký vòng, mức dòng, Sink, kiểm tham số.
 * Hàm: các test Log_*. Không cần scene hay Play Mode.
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Network.Framework.Tests
{
    public sealed class HostLogTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void Log_RejectsCapacityBelowOne(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HostLog(capacity));
        }

        [Test]
        public void Log_KeepsLevelAndMessage()
        {
            HostLog log = new HostLog();
            log.Info("a");
            log.Warning("b");
            Assert.AreEqual(2, log.Count);
            Assert.AreEqual(HostLogLevel.Info, log[0].Level);
            Assert.AreEqual("a", log[0].Message);
            Assert.AreEqual(HostLogLevel.Warning, log[1].Level);
            Assert.AreEqual(1, log.CountOf(HostLogLevel.Info));
            Assert.AreEqual(1, log.CountOf(HostLogLevel.Warning));
        }

        [Test]
        public void Log_DropsOldestBeyondCapacity()
        {
            HostLog log = new HostLog(3);
            for (int i = 1; i <= 5; i++) log.Info("m" + i);
            Assert.AreEqual(3, log.Count);
            Assert.AreEqual("m3", log[0].Message);
            Assert.AreEqual("m5", log[2].Message);
        }

        [Test]
        public void Log_SinkReceivesEveryEntryEvenWhenTrimmed()
        {
            HostLog log = new HostLog(1);
            List<string> seen = new List<string>();
            log.Sink = e => seen.Add(e.Message);
            log.Info("x");
            log.Warning("y");
            CollectionAssert.AreEqual(new[] { "x", "y" }, seen);
            Assert.AreEqual(1, log.Count);
        }

        [Test]
        public void Log_NullMessageBecomesEmptyAndClearEmptiesLog()
        {
            HostLog log = new HostLog();
            log.Info(null);
            Assert.AreEqual(string.Empty, log[0].Message);
            log.Clear();
            Assert.AreEqual(0, log.Count);
        }
    }
}
