/*
 * Mục đích: Test hồi quy FIX-F-004 cho GradeRepairCodec (T-036/T-039): sổ riêng phải ở dạng thoát CHÍNH TẮC. Uri.UnescapeDataString không ném lỗi với «%» trần
 * hay «%zz» nên codec từng nhận chuỗi sai định dạng; nay chuỗi nào mà thoát lại không ra đúng chính nó đều bị từ chối, và mọi sổ do bộ mã hóa sinh ra
 * vẫn đi-về nguyên vẹn.
 * Hàm: các test trong GradeRepairCodecCanonicalTests.
 */
using NUnit.Framework;

namespace TheReturn.GradeRepairNet.Tests
{
    [TestFixture]
    public sealed class GradeRepairCodecCanonicalTests
    {
        [TestCase("g:0=%")]
        [TestCase("g:0=%zz")]
        [TestCase("g:0=a%")]
        [TestCase("g:0=%4")]
        [TestCase("g:0=%41")]
        [TestCase("g:0=%c3%a9")]
        [TestCase("g:0=a%b")]
        public void Private_NonCanonicalEscape_IsRejected(string text)
        {
            GradeRepairPrivateState p;
            Assert.IsFalse(GradeRepairCodec.TryDecodePrivate(text, out p), "Phải bác: " + text);
            Assert.AreEqual(0, p.OwnedCount);
        }

        [Test]
        public void Private_FactWithPercentAndSpaces_RoundTrips()
        {
            GradeRepairPrivateState before = new GradeRepairPrivateState(new[] { 0, 2 }, new[] { "Dat 100% mon Toan, lop 9A", "C:HS-111 = 8.5" });
            string text = GradeRepairCodec.EncodePrivate(before);
            GradeRepairPrivateState after;
            Assert.IsTrue(GradeRepairCodec.TryDecodePrivate(text, out after), text);
            Assert.AreEqual(text, GradeRepairCodec.EncodePrivate(after));
            Assert.AreEqual(2, after.OwnedCount);
        }
    }
}
