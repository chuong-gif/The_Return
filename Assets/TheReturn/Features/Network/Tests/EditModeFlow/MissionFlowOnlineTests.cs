/*
 * Mục đích: NUnit EditMode test cho chế độ online của MissionFlowController (T-028): vai Host/Client/Offline, chỉ khu có bản online mới có
 * nhiệm vụ (các khu khác cửa đóng), host chạy một khu từ đầu tới xong, máy khách khôi phục đúng trạng thái host gửi (khu đang làm, khu xong,
 * cổng đang mở, thắng map) và từ chối dữ liệu không khớp. Dựng GameObject bằng code, không cần scene (Awake không chạy ở EditMode).
 * Hàm: Stub (adapter giả); các test FlowOnline_*.
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Tests
{
    public sealed class MissionFlowOnlineTests
    {
        const string Area = "Hanh_Lang_6";
        readonly List<GameObject> created = new List<GameObject>();

        sealed class Stub : IMissionAdapter
        {
            public string AreaId { get; private set; }
            public bool Active { get; set; }
            public bool Solved { get; set; }
            public bool CanHandOff { get; set; }
            public int Begins;
            public int Stops;
            public Stub(string area) { AreaId = area; }
            public void Begin() { Begins++; Active = true; }
            public void Stop(bool reset) { Stops++; Active = false; }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            created.Clear();
        }

        MissionFlowController NewFlow()
        {
            GameObject go = new GameObject("Flow");
            created.Add(go);
            return go.AddComponent<MissionFlowController>();
        }

        SchoolPreparationGate NewGate(string name)
        {
            GameObject go = new GameObject(name);
            created.Add(go);
            return go.AddComponent<SchoolPreparationGate>();
        }

        [Test]
        public void FlowOnline_DefaultRoleIsOffline()
        {
            MissionFlowController flow = NewFlow();
            Assert.AreEqual(FlowNetRole.Offline, flow.NetRole);
            Assert.IsNull(flow.BuildState());
        }

        [Test]
        public void FlowOnline_HostRegistersOnlyOnlineMissions()
        {
            MissionFlowController flow = NewFlow();
            flow.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { new Stub(Area) });
            Assert.AreEqual(FlowNetRole.Host, flow.NetRole);
            Assert.IsTrue(flow.Registry.Progress.Find(Area).HasMission);
            Assert.IsFalse(flow.Registry.Progress.Find("Phong_1").HasMission);
            Assert.AreEqual(11, flow.Registry.Progress.MissingMissionAreas.Count);
        }

        [Test]
        public void FlowOnline_HostRunsAMissionFromStartToWin()
        {
            MissionFlowController flow = NewFlow();
            Stub stub = new Stub(Area);
            flow.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { stub });

            Assert.AreEqual(BeginResult.NoMission, flow.Registry.TryBegin("Phong_1", MapProgress.Yard, true));
            Assert.AreEqual(BeginResult.TeamNotReady, flow.Registry.TryBegin(Area, "Phong_6", false));
            Assert.AreEqual(BeginResult.Started, flow.Registry.TryBegin(Area, "Phong_6", true));
            Assert.AreEqual(1, stub.Begins);
            Assert.AreEqual(Area, flow.BuildState().ActiveAreaId);

            Assert.IsFalse(flow.Registry.Tick());
            stub.Solved = true;
            stub.CanHandOff = true;
            Assert.IsTrue(flow.Registry.Tick());
            Assert.AreEqual(1, stub.Stops);
            MapFlowState state = flow.BuildState();
            Assert.AreEqual(string.Empty, state.ActiveAreaId);
            CollectionAssert.AreEqual(new[] { Area }, state.DoneAreaIds);
            Assert.IsTrue(flow.Registry.Won);
        }

        [Test]
        public void FlowOnline_ClientRestoresWhatTheHostSends()
        {
            MissionFlowController host = NewFlow();
            Stub stub = new Stub(Area);
            host.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { stub });
            MissionFlowController client = NewFlow();
            client.EnterOnline(FlowNetRole.Client, new IMissionAdapter[] { new RemoteMissionAdapter(Area) });

            host.Registry.TryBegin(Area, "Phong_6", true);
            MapFlowState sent;
            Assert.IsTrue(MapFlowState.TryDecode(host.BuildState().Encode(), out sent));
            Assert.IsTrue(client.ApplyRemoteState(sent));
            Assert.IsTrue(client.Registry.Progress.IsActive(Area));
            Assert.IsTrue(client.AnyActive);

            stub.Solved = true;
            stub.CanHandOff = true;
            host.Registry.Tick();
            Assert.IsTrue(MapFlowState.TryDecode(host.BuildState().Encode(), out sent));
            Assert.IsTrue(client.ApplyRemoteState(sent));
            Assert.IsTrue(client.Registry.Progress.IsDone(Area));
            Assert.IsTrue(client.Registry.Won);
            Assert.IsFalse(client.AnyActive);
        }

        [Test]
        public void FlowOnline_OnlyAClientAppliesRemoteState()
        {
            MissionFlowController host = NewFlow();
            host.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { new Stub(Area) });
            MissionFlowController offline = NewFlow();
            MapFlowState state = new MapFlowState(null, null, -1);
            Assert.IsFalse(host.ApplyRemoteState(state));
            Assert.IsFalse(offline.ApplyRemoteState(state));
            Assert.IsFalse(host.ApplyRemoteState(null));
        }

        [Test]
        public void FlowOnline_ClientRefusesStateThatDoesNotMatchTheMap()
        {
            MissionFlowController client = NewFlow();
            client.EnterOnline(FlowNetRole.Client, new IMissionAdapter[] { new RemoteMissionAdapter(Area) });
            Assert.IsFalse(client.ApplyRemoteState(new MapFlowState("Phong_99", null, -1)));
            Assert.IsFalse(client.ApplyRemoteState(new MapFlowState("Phong_1", null, -1)), "khu đang làm chưa có nhiệm vụ");
            Assert.IsFalse(client.AnyActive);
            Assert.AreEqual(0, client.Registry.Progress.DoneCount);
        }

        [Test]
        public void FlowOnline_ArmedGateFollowsTheIndexFromTheHost()
        {
            MissionFlowController host = NewFlow();
            MissionFlowController client = NewFlow();
            SchoolPreparationGate g0 = NewGate("G0");
            SchoolPreparationGate g1 = NewGate("G1");
            GateBinding[] table = new[]
            {
                new GateBinding { gate = g0, areaId = Area, entryFromId = "Phong_6" },
                new GateBinding { gate = g1, areaId = Area, entryFromId = "Phong_1" }
            };
            host.bindings = table;
            client.bindings = table;
            host.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { new Stub(Area) });
            client.EnterOnline(FlowNetRole.Client, new IMissionAdapter[] { new RemoteMissionAdapter(Area) });

            Assert.IsTrue(client.ApplyRemoteState(new MapFlowState(null, null, 1)));
            Assert.AreSame(g1, client.ArmedGate);
            Assert.IsTrue(client.ApplyRemoteState(new MapFlowState(null, null, -1)));
            Assert.IsNull(client.ArmedGate);
            Assert.IsTrue(client.ApplyRemoteState(new MapFlowState(null, null, 9)), "chỉ số ngoài bảng = không có cổng");
            Assert.IsNull(client.ArmedGate);
            Assert.AreEqual(-1, host.BuildState().ArmedGateIndex);
        }

        [Test]
        public void FlowOnline_ExitReturnsToOffline()
        {
            MissionFlowController flow = NewFlow();
            flow.EnterOnline(FlowNetRole.Client, new IMissionAdapter[] { new RemoteMissionAdapter(Area) });
            flow.ExitOnline();
            Assert.AreEqual(FlowNetRole.Offline, flow.NetRole);
            Assert.IsNotNull(flow.Registry);
            flow.EnterOnline(FlowNetRole.Host, new IMissionAdapter[] { new Stub(Area) });
            flow.EnterOnline(FlowNetRole.Offline, null);
            Assert.AreEqual(FlowNetRole.Offline, flow.NetRole);
        }

        [Test]
        public void FlowOnline_RemoteAdapterIsNeverActiveOrSolved()
        {
            RemoteMissionAdapter adapter = new RemoteMissionAdapter(Area);
            adapter.Begin();
            adapter.Stop(true);
            Assert.IsFalse(adapter.Active);
            Assert.IsFalse(adapter.Solved);
            Assert.IsFalse(adapter.CanHandOff);
            Assert.Throws<System.ArgumentException>(() => new RemoteMissionAdapter(""));
        }
    }
}
