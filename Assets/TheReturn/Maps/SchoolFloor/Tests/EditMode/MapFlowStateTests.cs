/*
 * Mục đích: NUnit EditMode test cho MapFlowState (T-028): khứ hồi mã hóa, khôi phục MapProgress ở máy khách đúng như host, từ chối dữ liệu xấu.
 * Hàm: các test FlowState_*. Không cần scene hay Play Mode.
 */
using System.Collections.Generic;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class MapFlowStateTests
    {
        static MapProgress NewProgress(params string[] withMission)
        {
            return MapProgress.CreateRing(6, withMission);
        }

        [Test]
        public void FlowState_FreshProgressEncodesAsEmptyFields()
        {
            MapFlowState state = MapFlowState.From(NewProgress("Hanh_Lang_6"), -1);
            Assert.AreEqual("||", state.Encode());
            Assert.AreEqual(string.Empty, state.ActiveAreaId);
            Assert.AreEqual(0, state.DoneAreaIds.Count);
            Assert.AreEqual(-1, state.ArmedGateIndex);
        }

        [Test]
        public void FlowState_RoundTripsActiveDoneAndArmed()
        {
            MapProgress progress = NewProgress("Phong_1", "Hanh_Lang_6", "Hanh_Lang_2");
            Assert.AreEqual(BeginResult.Started, progress.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.IsTrue(progress.Complete("Phong_1"));
            Assert.AreEqual(BeginResult.Started, progress.TryBegin("Hanh_Lang_6", "Phong_6", true));

            MapFlowState source = MapFlowState.From(progress, 7);
            MapFlowState back;
            Assert.IsTrue(MapFlowState.TryDecode(source.Encode(), out back));
            Assert.AreEqual("Hanh_Lang_6", back.ActiveAreaId);
            CollectionAssert.AreEqual(new[] { "Phong_1" }, back.DoneAreaIds);
            Assert.AreEqual(7, back.ArmedGateIndex);
            StringAssert.EndsWith("|7", source.Encode());
            Assert.AreEqual(source.Encode(), back.Encode());
        }

        [Test]
        public void FlowState_ClientRestoresTheSameProgressAsHost()
        {
            MapProgress host = NewProgress("Phong_1", "Hanh_Lang_6");
            host.TryBegin("Phong_1", MapProgress.Yard, true);
            host.Complete("Phong_1");
            host.TryBegin("Hanh_Lang_6", "Phong_6", true);

            MapProgress client = NewProgress("Phong_1", "Hanh_Lang_6");
            MapFlowState received;
            Assert.IsTrue(MapFlowState.TryDecode(MapFlowState.From(host, -1).Encode(), out received));
            Assert.IsTrue(client.RestoreSnapshot(received.ToSnapshot()));
            Assert.IsTrue(client.IsDone("Phong_1"));
            Assert.IsTrue(client.IsActive("Hanh_Lang_6"));
            Assert.IsFalse(client.Won);

            host.Complete("Hanh_Lang_6");
            Assert.IsTrue(MapFlowState.TryDecode(MapFlowState.From(host, -1).Encode(), out received));
            Assert.IsTrue(client.RestoreSnapshot(received.ToSnapshot()));
            Assert.IsTrue(client.Won);
            Assert.AreEqual(2, client.DoneCount);
        }

        [Test]
        public void FlowState_SnapshotUsesNullForNoActiveArea()
        {
            MapProgressSnapshot snapshot = new MapFlowState(null, new[] { "Phong_1" }, -1).ToSnapshot();
            Assert.IsNull(snapshot.ActiveAreaId);
            CollectionAssert.AreEqual(new[] { "Phong_1" }, snapshot.DoneAreaIds);
        }

        [Test]
        public void FlowState_ConstructorCopiesItsInput()
        {
            List<string> done = new List<string> { "Phong_1" };
            MapFlowState state = new MapFlowState(null, done, -1);
            done.Add("Phong_2");
            Assert.AreEqual(1, state.DoneAreaIds.Count);
        }

        [Test]
        public void FlowState_FromNullProgressThrows()
        {
            Assert.Throws<System.ArgumentNullException>(() => MapFlowState.From(null, -1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("garbage")]
        [TestCase("|")]
        [TestCase("|||")]
        [TestCase("a|b")]
        [TestCase("Phong 1||")]
        [TestCase("|Phong_1,Phong_1|")]
        [TestCase("|Phong_1,,Phong_2|")]
        [TestCase("||Hanh-Lang")]
        [TestCase("||-1")]
        [TestCase("||256")]
        [TestCase("||1.5")]
        [TestCase("||abc")]
        [TestCase("|Phong_1,|")]
        public void FlowState_BadInputIsRejectedNotThrown(string text)
        {
            MapFlowState state;
            Assert.IsFalse(MapFlowState.TryDecode(text, out state));
            Assert.IsNull(state);
        }

        [Test]
        public void FlowState_TooLongIdOrTooManyAreasRejected()
        {
            MapFlowState state;
            Assert.IsFalse(MapFlowState.TryDecode(new string('a', 25) + "||", out state));
            List<string> many = new List<string>();
            for (int i = 0; i < MapFlowState.MaxAreas + 1; i++) many.Add("A" + i);
            Assert.IsFalse(MapFlowState.TryDecode("|" + string.Join(",", many) + "|", out state));
        }

        [Test]
        public void FlowState_NegativeGateIndexMeansNone()
        {
            MapFlowState state = new MapFlowState(null, null, -5);
            Assert.AreEqual(-1, state.ArmedGateIndex);
            Assert.AreEqual("||", state.Encode());
        }

        [Test]
        public void FlowState_UnknownAreaIsDecodedButRestoreRefusesIt()
        {
            MapFlowState state;
            Assert.IsTrue(MapFlowState.TryDecode("|Phong_99|", out state));
            MapProgress client = NewProgress("Phong_1");
            Assert.IsFalse(client.RestoreSnapshot(state.ToSnapshot()));
            Assert.AreEqual(0, client.DoneCount);
        }
    }
}
