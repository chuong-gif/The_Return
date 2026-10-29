/*
 * Mục đích: Bộ test Play Mode NUnit cho luồng TỰ CHỌN KHU của tầng học (T-005, thay hướng các bộ *Checks cũ). Mỗi test nạp lại scene
 * SchoolFloorPrototype trong Play Mode (không ghi gì ra đĩa, không sửa scene), rồi dịch chuyển đội vào các vùng lấy TỪ CỔNG TRONG SCENE
 * (không hardcode tọa độ) và kiểm luật cửa: mọi cửa đóng từ đầu; cả đội đứng đủ ở khu chuẩn bị NGOÀI cửa thì cửa đó mở; vào trong đủ đội thì
 * nhiệm vụ bắt đầu và khóa mọi cửa; xong thì cả ba lối của khu mở; RestartMap đóng lại; cửa armed tự đóng khi đội bỏ đi.
 * Hàm hỗ trợ: LoadScene (UnitySetUp), Frames, Binding, OutsideCenter, InnerCenter, PlaceTeam, OpenGateNames, GatherOutside, EnterInside.
 * Test: Start_*, PartialTeam_*, FullTeam_*, Inside_*, SingleSneak_*, Phong1_*, Corridor_*, ArmedGate_*, RestartMap_*, EveryGate_*.
 * Chạy: Unity MCP run_tests mode PlayMode, assembly_names ["TheReturn.SchoolFloor.PlayTests"] khi editor KHÔNG đang Play.
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TheReturn.Tests
{
    public sealed class SchoolFloorFlowPlayTests
    {
        const string ScenePath = "Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity";

        MissionFlowController ctrl;
        SchoolFloorFlow schoolFlow;
        SchoolFloorAccessController access;
        PrototypePartyController party;

        /// <summary>Không nhận tham số; nạp lại scene trong Play Mode trước mỗi test và lấy các đối tượng điều phối.</summary>
        [UnitySetUp]
        public IEnumerator LoadScene()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Chỉ chạy trong Editor");
            yield break;
#endif
            yield return Frames(3);
            ctrl = Object.FindFirstObjectByType<MissionFlowController>();
            schoolFlow = Object.FindFirstObjectByType<SchoolFloorFlow>();
            access = Object.FindFirstObjectByType<SchoolFloorAccessController>();
            Assert.IsNotNull(ctrl, "thiếu MissionFlowController trong scene");
            Assert.IsNotNull(schoolFlow, "thiếu SchoolFloorFlow");
            Assert.IsNotNull(access, "thiếu SchoolFloorAccessController");
            Assert.IsNotNull(ctrl.Registry, "registry chưa dựng");
            party = schoolFlow.attendance.party;
            Assert.AreEqual(4, party.PartySize, "mặc định đội 4 người");
            yield return Frames(2);
        }

        /// <summary>Nhận số khung hình; chờ đúng số đó (cho Update/LateUpdate của các controller chạy).</summary>
        static IEnumerator Frames(int count) { for (int i = 0; i < count; i++) yield return null; }

        /// <summary>Nhận tên cổng; trả dòng bảng cổng tương ứng (ném lỗi test nếu không có).</summary>
        GateBinding Binding(string gateName)
        {
            GateBinding b = ctrl.bindings.FirstOrDefault(x => x != null && x.gate != null && x.gate.name == gateName);
            Assert.IsNotNull(b, "không có cổng " + gateName);
            return b;
        }

        /// <summary>Nhận cổng; trả tâm khu chuẩn bị NGOÀI cửa (phản chiếu vùng chuẩn bị trong qua mặt phẳng cửa).</summary>
        static Vector3 OutsideCenter(SchoolPreparationGate gate)
        {
            BoxCollider zone = gate.preparationZone;
            Vector3 world = zone.transform.TransformPoint(zone.center);
            Vector3 local = gate.transform.InverseTransformPoint(world);
            return gate.transform.TransformPoint(new Vector3(local.x, local.y, -local.z));
        }

        /// <summary>Nhận cổng; trả điểm giữa vùng chuẩn bị BÊN TRONG, ngang sàn.</summary>
        static Vector3 InnerCenter(SchoolPreparationGate gate)
        {
            Bounds b = gate.preparationZone.bounds;
            return new Vector3(b.center.x, b.min.y + 0.2f, b.center.z);
        }

        /// <summary>Nhận tâm, trục ngang, số người; dịch chuyển người 0..count-1 thành hàng quanh tâm.</summary>
        void PlaceTeam(Vector3 center, Vector3 right, int count)
        {
            for (int i = 0; i < count; i++)
                party.Teleport(i, center + right * ((i - (count - 1) * 0.5f) * 1.0f) + Vector3.up * 0.1f);
        }

        /// <summary>Không nhận tham số; trả danh sách tên cổng đang mở, sắp xếp.</summary>
        List<string> OpenGateNames()
        {
            return access.gates.Where(g => g != null && g.IsOpen).Select(g => g.name).OrderBy(n => n).ToList();
        }

        /// <summary>Nhận cổng và số người; đưa số người đó vào khu chuẩn bị NGOÀI cửa rồi chờ vài khung hình.</summary>
        IEnumerator GatherOutside(SchoolPreparationGate gate, int count)
        {
            PlaceTeam(OutsideCenter(gate), gate.transform.right, count);
            yield return Frames(3);
        }

        /// <summary>Nhận cổng và số người; đưa số người đó vào vùng chuẩn bị BÊN TRONG rồi chờ vài khung hình.</summary>
        IEnumerator EnterInside(SchoolPreparationGate gate, int count)
        {
            PlaceTeam(InnerCenter(gate), gate.transform.right, count);
            yield return Frames(3);
        }

        // ---------------------------------------------------------------- Test

        [UnityTest]
        public IEnumerator Start_AllGatesClosed_NothingActive_TeamNotInAnyZone()
        {
            yield return null;
            Assert.AreEqual(0, OpenGateNames().Count, "lúc đầu mọi cửa phải đóng: " + string.Join(", ", OpenGateNames()));
            Assert.IsNull(ctrl.Registry.Progress.ActiveAreaId);
            Assert.IsNull(ctrl.ArmedGate);
            Assert.IsTrue(schoolFlow.attendance.ExternalControl, "Attendance không tự chạy");
            foreach (GateBinding b in ctrl.bindings)
            {
                Assert.AreEqual(0, b.gate.CountInside(party), "đội không được đứng trong vùng trong của " + b.gate.name);
                Assert.AreEqual(0, b.gate.CountOutside(party), "đội không được đứng trong khu chuẩn bị của " + b.gate.name);
            }
        }

        [UnityTest]
        public IEnumerator PartialTeam_OutsideGate_DoesNothing()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 3);
            Assert.AreEqual(3, gate.CountOutside(party));
            Assert.IsNull(ctrl.ArmedGate, "3/4 người chưa được mở cửa");
            Assert.AreEqual(0, OpenGateNames().Count);
            Assert.IsTrue(gate.blocker.enabled, "cửa vẫn chắn");
        }

        [UnityTest]
        public IEnumerator FullTeam_OutsideGate_OnlyThatDoorOpens()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            Assert.AreSame(gate, ctrl.ArmedGate);
            CollectionAssert.AreEqual(new[] { "Phong 1_Cua 3" }, OpenGateNames());
            Assert.IsFalse(gate.blocker.enabled, "cửa mở thì không chắn");
        }

        [UnityTest]
        public IEnumerator Inside_FullTeam_StartsAreaAndLocksAllDoors()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            yield return EnterInside(gate, 4);
            Assert.AreEqual("Phong_1", ctrl.Registry.Progress.ActiveAreaId);
            Assert.AreEqual(0, OpenGateNames().Count, "khu đang làm: mọi cửa khóa");
            Assert.IsFalse(schoolFlow.attendance.ExternalControl, "Attendance đã được trao quyền");
        }

        [UnityTest]
        public IEnumerator SingleSneak_InsideWithoutTeam_DoesNotStart()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            Assert.AreSame(gate, ctrl.ArmedGate);
            yield return EnterInside(gate, 1);
            for (int i = 1; i < 4; i++) party.Teleport(i, OutsideCenter(gate) + Vector3.up * 0.1f);
            yield return Frames(3);
            Assert.IsNull(ctrl.Registry.Progress.ActiveAreaId, "chỉ 1 người vào trong thì chưa được bắt đầu");
        }

        [UnityTest]
        public IEnumerator Phong1_Solved_OpensItsThreeDoorsOnly()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            yield return EnterInside(gate, 4);
            Assert.AreEqual("Phong_1", ctrl.Registry.Progress.ActiveAreaId);
            AttendancePrototype attendance = schoolFlow.attendance;
            int[] answer = attendance.State.Round.CopySolution();
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.TrySit(i, answer[i]);
            Assert.IsTrue(attendance.State.Evaluate());
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.Stand(i);
            yield return Frames(3);
            Assert.IsNull(ctrl.Registry.Progress.ActiveAreaId);
            Assert.IsTrue(ctrl.Registry.Progress.IsDone("Phong_1"));
            CollectionAssert.AreEqual(new[] { "Phong 1_Cua 1", "Phong 1_Cua 2", "Phong 1_Cua 3" }, OpenGateNames());
        }

        [UnityTest]
        public IEnumerator Corridor_AfterPhong1Done_StartsFromItsGate()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            yield return EnterInside(gate, 4);
            AttendancePrototype attendance = schoolFlow.attendance;
            int[] answer = attendance.State.Round.CopySolution();
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.TrySit(i, answer[i]);
            attendance.State.Evaluate();
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.Stand(i);
            yield return Frames(3);
            GateBinding vao = Binding("Hanh lang 2_Vao");
            Assert.AreEqual("Hanh_Lang_1", vao.areaId);
            Assert.IsFalse(vao.gate.IsOpen, "cổng hành lang đóng cho tới khi đội tập hợp");
            yield return GatherOutside(vao.gate, 4);
            Assert.AreSame(vao.gate, ctrl.ArmedGate);
            yield return EnterInside(vao.gate, 4);
            Assert.AreEqual("Hanh_Lang_1", ctrl.Registry.Progress.ActiveAreaId);
            CollectionAssert.AreEquivalent(new[] { "Phong 1_Cua 1", "Phong 1_Cua 2", "Phong 1_Cua 3" }, OpenGateNames(), "Phong_1 đã xong vẫn mở, các cổng khác khóa");
        }

        [UnityTest]
        public IEnumerator ArmedGate_ClosesAgain_WhenTeamLeavesBothZones()
        {
            SchoolPreparationGate gate = Binding("Phong 2_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            Assert.AreSame(gate, ctrl.ArmedGate);
            for (int i = 0; i < 4; i++) party.Teleport(i, new Vector3(-11.5f + i, 2.5f, 24.3f));
            float until = Time.realtimeSinceStartup + 30f;
            while (ctrl.ArmedGate != null && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsNull(ctrl.ArmedGate, "cửa armed phải tự đóng sau " + ctrl.disarmSeconds + "s khi đội rời cả hai vùng");
            yield return Frames(3);
            Assert.AreEqual(0, OpenGateNames().Count);
        }

        [UnityTest]
        public IEnumerator RestartMap_ClosesEverythingAndResetsProgress()
        {
            SchoolPreparationGate gate = Binding("Phong 1_Cua 3").gate;
            yield return GatherOutside(gate, 4);
            yield return EnterInside(gate, 4);
            Assert.AreEqual("Phong_1", ctrl.Registry.Progress.ActiveAreaId);
            schoolFlow.RestartMap();
            yield return Frames(3);
            Assert.IsNull(ctrl.Registry.Progress.ActiveAreaId);
            Assert.IsFalse(ctrl.Registry.Progress.IsDone("Phong_1"));
            Assert.IsNull(ctrl.ArmedGate);
            Assert.AreEqual(0, OpenGateNames().Count);
        }

        [UnityTest]
        public IEnumerator EveryGate_OutsideZone_HasWalkableFloor()
        {
            yield return null;
            var bad = new List<string>();
            foreach (GateBinding b in ctrl.bindings)
            {
                Vector3 outside = OutsideCenter(b.gate);
                RaycastHit hit;
                bool floor = Physics.Raycast(outside + Vector3.up * 3f, Vector3.down, out hit, 8f, ~0, QueryTriggerInteraction.Ignore);
                if (!floor || Mathf.Abs(hit.point.y - b.gate.transform.position.y) > 1.6f) bad.Add(b.gate.name + " (outside=" + outside.ToString("F1") + ")");
            }
            Assert.AreEqual(0, bad.Count, "khu chuẩn bị ngoài cửa không có sàn đi được: " + string.Join("; ", bad));
        }

        [UnityTest]
        public IEnumerator EveryGate_OfEveryMissionArea_StartsItsArea()
        {
            yield return null;
            int tested = 0;
            var failures = new List<string>();
            foreach (GateBinding b in ctrl.bindings.ToArray())
            {
                AreaInfo area = ctrl.Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission) continue;
                yield return GatherOutside(b.gate, 4);
                if (ctrl.ArmedGate != b.gate) { failures.Add(b.gate.name + ": không armed khi đội đứng đủ ngoài cửa (armed=" + (ctrl.ArmedGate ? ctrl.ArmedGate.name : "null") + ")"); schoolFlow.RestartMap(); yield return Frames(2); continue; }
                yield return EnterInside(b.gate, 4);
                if (ctrl.Registry.Progress.ActiveAreaId != b.areaId) failures.Add(b.gate.name + ": khu " + b.areaId + " không bắt đầu (active=" + (ctrl.Registry.Progress.ActiveAreaId ?? "null") + ")");
                tested++;
                schoolFlow.RestartMap();
                yield return Frames(3);
            }
            Assert.GreaterOrEqual(tested, 20, "ít cổng được kiểm hơn dự kiến (có cổng nào bị bỏ sót?)");
            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }
    }
}
