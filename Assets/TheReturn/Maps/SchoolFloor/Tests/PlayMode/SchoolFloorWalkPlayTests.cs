/*
 * Mục đích: Test Play Mode mô phỏng NGƯỜI CHƠI THẬT ĐI BỘ qua cửa (T-033). Khác SchoolFloorFlowPlayTests (dịch chuyển tức thời vào tâm vùng),
 * test này đặt đội ở khu chuẩn bị NGOÀI cửa rồi cho từng nhân vật đi bằng CharacterController.Move xuyên cửa vào vùng trong, đúng như khi chơi,
 * rồi kiểm nhiệm vụ của khu có bắt đầu không; chạy với đội 2/3/4 người và cả sau khi đã xong Phong_1 (đúng thứ tự chơi thật).
 * Hàm: LoadScene (UnitySetUp), OutsideCenter, OnFloor, Walk, Report, WalkGate, SolveAttendance; test EveryGate_*, AfterPhong1_*.
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
    public sealed class SchoolFloorWalkPlayTests
    {
        const string ScenePath = "Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity";

        MissionFlowController ctrl;
        SchoolFloorFlow schoolFlow;
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
            for (int i = 0; i < 5; i++) yield return null;
            ctrl = Object.FindFirstObjectByType<MissionFlowController>();
            schoolFlow = Object.FindFirstObjectByType<SchoolFloorFlow>();
            Assert.IsNotNull(ctrl, "thiếu MissionFlowController");
            Assert.IsNotNull(schoolFlow, "thiếu SchoolFloorFlow");
            party = schoolFlow.attendance.party;
        }

        /// <summary>Nhận cổng; trả tâm khu chuẩn bị NGOÀI cửa (phản chiếu vùng chuẩn bị trong qua mặt phẳng cửa).</summary>
        static Vector3 OutsideCenter(SchoolPreparationGate gate)
        {
            BoxCollider zone = gate.preparationZone;
            Vector3 world = zone.transform.TransformPoint(zone.center);
            Vector3 local = gate.transform.InverseTransformPoint(world);
            return gate.transform.TransformPoint(new Vector3(local.x, local.y, -local.z));
        }

        /// <summary>Nhận điểm; trả điểm trên sàn ngay dưới điểm đó (nhân vật đứng trên sàn như khi chơi), hoặc chính điểm đó nếu không thấy sàn.</summary>
        static Vector3 OnFloor(Vector3 p)
        {
            RaycastHit hit;
            if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out hit, 6f, ~0, QueryTriggerInteraction.Ignore)) return hit.point + Vector3.up * 0.02f;
            return p;
        }

        /// <summary>Nhận chỉ số nhân vật, đích và thời gian tối đa; đi thẳng tới đích bằng CharacterController.Move như TickInput.</summary>
        IEnumerator Walk(int index, Vector3 target, float seconds)
        {
            Transform t = party.players[index];
            CharacterController cc = t.GetComponent<CharacterController>();
            float until = Time.time + seconds;
            while (Time.time < until)
            {
                Vector3 d = target - t.position;
                d.y = 0;
                if (d.magnitude < 0.2f) break;
                cc.Move((d.normalized * party.moveSpeed + Vector3.down * 2f) * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>Nhận cổng; trả một dòng mô tả trạng thái hiện tại để báo lỗi dễ đọc.</summary>
        string Report(SchoolPreparationGate gate)
        {
            string pos = string.Join(" ", Enumerable.Range(0, party.PartySize).Select(i => party.players[i].position.ToString("F1")));
            return "armed=" + (ctrl.ArmedGate ? ctrl.ArmedGate.name : "null") + " active=" + (ctrl.Registry.Progress.ActiveAreaId ?? "null") +
                   " outside=" + gate.CountOutside(party) + " inside=" + gate.CountInside(party) + "/" + party.PartySize +
                   " doorOpen=" + gate.IsOpen + " blocker=" + (gate.blocker != null && gate.blocker.enabled) + " pos=[" + pos + "]";
        }

        /// <summary>Nhận dòng cổng, danh sách lỗi, độ sâu thêm (m) và khoảng cách ngang giữa các người; đặt đội ngoài cửa, cho từng người đi bộ vào trong, ghi lỗi nếu khu không bắt đầu. Trả đã kiểm hay chưa qua failures.</summary>
        IEnumerator WalkGate(GateBinding b, List<string> failures, float deep = 0f, float spread = 0.9f)
        {
            SchoolPreparationGate gate = b.gate;
            int n = party.PartySize;
            Vector3 right = gate.transform.right;
            Bounds inner = gate.preparationZone.bounds;
            Vector3 outCenter = OutsideCenter(gate);
            for (int i = 0; i < n; i++)
                party.Teleport(i, OnFloor(outCenter + right * ((i - (n - 1) * 0.5f) * 0.9f)));
            for (int f = 0; f < 6; f++) yield return null;
            if (ctrl.ArmedGate != gate) { failures.Add(gate.name + " [" + b.areaId + "] đội " + n + ": KHÔNG armed khi đội đứng đủ ngoài cửa: " + Report(gate)); yield break; }
            for (int i = 0; i < n; i++)
            {
                Vector3 target = OnFloor(new Vector3(inner.center.x, inner.center.y, inner.center.z) + gate.transform.forward * deep + right * ((i - (n - 1) * 0.5f) * spread));
                yield return Walk(i, target, 12f);
            }
            for (int f = 0; f < 10; f++) yield return null;
            if (ctrl.Registry.Progress.ActiveAreaId != b.areaId) failures.Add(gate.name + " [" + b.areaId + "] đội " + n + ": nhiệm vụ KHÔNG bắt đầu sau khi cả đội đi bộ vào: " + Report(gate));
        }

        /// <summary>Không nhận tham số; giải câu điểm danh của Phong_1 như người chơi (ngồi đúng chỗ rồi đứng dậy) và chờ registry ghi nhận khu đã xong.</summary>
        IEnumerator SolveAttendance()
        {
            AttendancePrototype attendance = schoolFlow.attendance;
            int[] answer = attendance.State.Round.CopySolution();
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.TrySit(i, answer[i]);
            attendance.State.Evaluate();
            for (int i = 0; i < attendance.PartySize; i++) attendance.State.Stand(i);
            for (int f = 0; f < 5; f++) yield return null;
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator EveryGate_WalkingTeam_StartsItsArea([Values(2, 3, 4)] int size)
        {
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            Time.timeScale = 2f;
            var failures = new List<string>();
            int tested = 0;
            foreach (GateBinding b in ctrl.bindings.ToArray())
            {
                AreaInfo area = ctrl.Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission) continue;
                yield return WalkGate(b, failures);
                tested++;
                schoolFlow.RestartMap();
                for (int f = 0; f < 4; f++) yield return null;
            }
            Time.timeScale = 1f;
            Assert.GreaterOrEqual(tested, 20, "ít cổng được kiểm hơn dự kiến");
            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator AfterPhong1_WalkingTeam_StartsNextAreas([Values(2, 4)] int size)
        {
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            Time.timeScale = 2f;
            var failures = new List<string>();
            foreach (string next in new[] { "Hanh lang 1_Ra", "Hanh lang 2_Vao", "Phong 1_Cua 1", "Phong 1_Cua 2" })
            {
                GateBinding first = ctrl.bindings.First(x => x.gate.name == "Phong 1_Cua 3");
                yield return WalkGate(first, failures);
                if (ctrl.Registry.Progress.ActiveAreaId != "Phong_1") { schoolFlow.RestartMap(); yield return null; continue; }
                yield return SolveAttendance();
                if (!ctrl.Registry.Progress.IsDone("Phong_1")) { failures.Add("đội " + size + ": Phong_1 không được ghi nhận xong sau khi giải điểm danh"); schoolFlow.RestartMap(); yield return null; continue; }
                GateBinding target = ctrl.bindings.First(x => x.gate.name == next);
                if (target.areaId == "Phong_1") { schoolFlow.RestartMap(); yield return null; continue; }
                yield return WalkGate(target, failures);
                schoolFlow.RestartMap();
                for (int f = 0; f < 4; f++) yield return null;
            }
            Time.timeScale = 1f;
            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator Corridor_HandoverThenTeamAtGoal_CompletesAndOpensDoors([Values(2, 4)] int size)
        {
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            QuietCorridorPrototype q = Object.FindFirstObjectByType<QuietCorridorPrototype>();
            Assert.IsNotNull(q, "thiếu QuietCorridorPrototype");
            var failures = new List<string>();
            GateBinding first = ctrl.bindings.First(x => x.gate.name == "Phong 1_Cua 3");
            yield return WalkGate(first, failures);
            yield return SolveAttendance();
            Assert.IsTrue(ctrl.Registry.Progress.IsDone("Phong_1"), "Phong_1 chưa xong");
            GateBinding hl = ctrl.bindings.First(x => x.areaId == "Hanh_Lang_1" && x.entryFromId == "Phong_1");
            yield return WalkGate(hl, failures);
            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
            Assert.AreEqual("Hanh_Lang_1", ctrl.Registry.Progress.ActiveAreaId);
            Assert.IsTrue(q.Active, "QuietCorridor phải Active");
            // bàn giao thật: người 0 giữ nút A, người 1 giữ nút B khi A còn giữ, rồi người 0 nhả
            Assert.IsTrue(q.State.Hold(0, 0));
            Assert.IsTrue(q.State.Hold(1, 1));
            Assert.IsTrue(q.State.HandedOver, "phải ghi nhận bàn giao");
            q.State.Release(0);
            // cả đội tới vùng đích (đặt trên sàn, trong hộp goalZone)
            Bounds goal = q.goalZone.bounds;
            Vector3 right = q.goalZone.transform.right;
            for (int i = 0; i < size; i++)
                party.Teleport(i, OnFloor(new Vector3(goal.center.x, goal.center.y, goal.center.z) + right * ((i - (size - 1) * 0.5f) * 0.9f)));
            for (int f = 0; f < 15; f++) yield return null;
            string diag = "AllAcross=" + q.AllAcross() + " solved=" + (q.State != null && q.State.Solved) + " active=" + ctrl.Registry.Progress.ActiveAreaId + " done=" + ctrl.Registry.Progress.IsDone("Hanh_Lang_1") + " failed=" + (q.State != null && q.State.Failed) + " cursor=" + Cursor.lockState;
            Assert.IsTrue(ctrl.Registry.Progress.IsDone("Hanh_Lang_1"), "cả đội ở vùng đích sau bàn giao nhưng khu không hoàn thành: " + diag);
            var open = ctrl.bindings.Where(b => b.areaId == "Hanh_Lang_1").Select(b => b.gate.IsOpen).ToArray();
            Assert.IsTrue(open.All(o => o), "cả hai cổng Hanh_Lang_1 phải mở sau khi xong: " + string.Join(",", open) + " " + diag);
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator Corridor_TeamStandsInExitGatePreparationZone_Completes([Values(2, 4)] int size)
        {
            // T-033 (bug 2 lần 2): chủ dự án đứng trong PreparationZone của cổng RA (nơi có biển «VÙNG ĐÍCH») mà khu không hoàn thành vì goalZone cũ chỉ sâu 5 m.
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            QuietCorridorPrototype q = Object.FindFirstObjectByType<QuietCorridorPrototype>();
            var failures = new List<string>();
            yield return WalkGate(ctrl.bindings.First(x => x.gate.name == "Phong 1_Cua 3"), failures);
            yield return SolveAttendance();
            yield return WalkGate(ctrl.bindings.First(x => x.areaId == "Hanh_Lang_1" && x.entryFromId == "Phong_1"), failures);
            Assert.AreEqual(0, failures.Count, string.Join("; ", failures));
            Assert.IsTrue(q.Active, "QuietCorridor phải Active");
            Assert.IsTrue(q.State.Hold(0, 0));
            Assert.IsTrue(q.State.Hold(1, 1));
            q.State.Release(0);
            q.State.Release(1); // không ai giữ nút: mọi người phải nằm trong vùng đích thật sự
            GateBinding exit = ctrl.bindings.Where(x => x.areaId == "Hanh_Lang_1")
                .OrderByDescending(x => Vector3.Distance(x.gate.transform.position, q.stations[0].transform.position)).First();
            BoxCollider zone = exit.gate.preparationZone;
            Vector3 center = zone.transform.TransformPoint(zone.center);
            Vector3 right = zone.transform.right;
            for (int i = 0; i < size; i++)
                party.Teleport(i, OnFloor(center + right * ((i - (size - 1) * 0.5f) * 0.9f)));
            for (int f = 0; f < 15; f++) yield return null;
            string diag = "AllAcross=" + q.AllAcross() + " solved=" + (q.State != null && q.State.Solved) + " cổng ra=" + exit.gate.name + " pos=[" + string.Join(" ", Enumerable.Range(0, size).Select(i => party.players[i].position.ToString("F1"))) + "]";
            Assert.IsTrue(ctrl.Registry.Progress.IsDone("Hanh_Lang_1"), "cả đội đứng trong PreparationZone cổng ra mà khu không hoàn thành: " + diag);
        }

        /// <summary>Nhận trạm, điểm xuất phát và khoảng cách; trả điểm trên sàn cách trạm d mét về phía điểm xuất phát (chỗ người chơi thật đứng để giữ nút).</summary>
        static Vector3 NearStation(Transform station, Vector3 from, float d)
        {
            Vector3 dir = from - station.position;
            dir.y = 0;
            return OnFloor(station.position + dir.normalized * d);
        }

        /// <summary>Nhận hành lang, đội, trạm và người; trả mô tả khoảng cách mắt-trạm và vật chắn tia, để báo vì sao không giữ được nút.</summary>
        static string HoldDiag(QuietCorridorPrototype q, PrototypePartyController pty, int station, int player)
        {
            Vector3 eye = pty.players[player].position + Vector3.up * 1.4f;
            Vector3 target = q.stations[station].transform.position;
            string hits = string.Join(",", Physics.RaycastAll(eye, (target - eye).normalized, Vector3.Distance(eye, target), ~0, QueryTriggerInteraction.Ignore)
                .Where(h => !h.transform.IsChildOf(pty.players[player]) && !h.transform.IsChildOf(q.stations[station].transform)).Select(h => h.collider.name));
            return "khoảng cách mắt-trạm=" + Vector3.Distance(eye, target).ToString("F2") + " (tối đa 2,8) vật chắn tia=[" + hits + "] nút A=P" + (q.State.Owner(0) + 1) + " B=P" + (q.State.Owner(1) + 1) + " bànGiao=" + q.State.HandedOver;
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator Corridor_RealisticHandover_WalkAndHold_CompletesAndOpensDoors([Values(2, 4)] int size)
        {
            // Đúng cách người chơi làm: đi bộ tới nút A và giữ (TryToggleStation = phím E), người kế đi qua cửa tới nút B và giữ khi A còn giữ,
            // người ở A bỏ đi (tự nhả khi xa hơn 3 m), cả đội đi bộ tới vùng đích. Không gọi State.Hold trực tiếp.
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            QuietCorridorPrototype q = Object.FindFirstObjectByType<QuietCorridorPrototype>();
            var failures = new List<string>();
            yield return WalkGate(ctrl.bindings.First(x => x.gate.name == "Phong 1_Cua 3"), failures);
            yield return SolveAttendance();
            yield return WalkGate(ctrl.bindings.First(x => x.areaId == "Hanh_Lang_1" && x.entryFromId == "Phong_1"), failures);
            Assert.AreEqual(0, failures.Count, string.Join("; ", failures));
            Assert.IsTrue(q.Active, "QuietCorridor phải Active");
            Time.timeScale = 2f;
            Vector3 gatePos = OnFloor(q.gate.transform.position);
            yield return Walk(0, NearStation(q.stations[0].transform, q.checkpoints[0].position, 1.3f), 25f);
            Assert.IsTrue(q.TryToggleStation(0, 0), "P1 không giữ được nút A: " + HoldDiag(q, party, 0, 0));
            for (int f = 0; f < 10; f++) yield return null;
            yield return Walk(1, gatePos, 30f);
            yield return Walk(1, NearStation(q.stations[1].transform, gatePos, 1.3f), 20f);
            Assert.IsTrue(q.TryToggleStation(1, 1), "P2 không giữ được nút B khi A còn giữ: " + HoldDiag(q, party, 1, 1) + " pos=" + party.players[1].position.ToString("F1"));
            Assert.IsTrue(q.State.HandedOver, "phải ghi nhận bàn giao");
            Vector3 goal = q.goalZone.bounds.center;
            Vector3 right = q.goalZone.transform.right;
            for (int i = 2; i < size; i++) { yield return Walk(i, gatePos, 30f); yield return Walk(i, OnFloor(goal + right * ((i - 2) * 1.2f - 1.0f)), 20f); }
            yield return Walk(0, gatePos, 40f);
            yield return Walk(0, OnFloor(goal + right * 2.0f), 20f);
            for (int f = 0; f < 30; f++) yield return null;
            Time.timeScale = 1f;
            string diag = "AllAcross=" + q.AllAcross() + " solved=" + (q.State != null && q.State.Solved) + " bànGiao=" + q.State.HandedOver + " failed=" + q.State.Failed + " ồn=" + q.State.Noise.ToString("F0") + " active=" + ctrl.Registry.Progress.ActiveAreaId + " cursor=" + Cursor.lockState + " pos=[" + string.Join(" ", Enumerable.Range(0, size).Select(i => party.players[i].position.ToString("F1"))) + "]";
            Assert.IsTrue(ctrl.Registry.Progress.IsDone("Hanh_Lang_1"), "đi bộ + giữ nút đúng cách mà khu không hoàn thành: " + diag);
            Assert.IsTrue(ctrl.bindings.Where(b => b.areaId == "Hanh_Lang_1").All(b => b.gate.IsOpen), "cả hai cổng Hanh_Lang_1 phải mở sau khi xong: " + diag);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator EveryGate_TeamWalksDeeperIntoArea_StartsItsArea([Values(4)] int size)
        {
            // Người chơi thật không dừng gọn trong mảng 4 m sát cửa: họ đi sâu vào phòng/hành lang và tản ra. Nhiệm vụ vẫn phải bắt đầu.
            schoolFlow.attendance.SetParticipantCount(size);
            for (int f = 0; f < 3; f++) yield return null;
            Time.timeScale = 2f;
            var failures = new List<string>();
            int tested = 0;
            foreach (GateBinding b in ctrl.bindings.ToArray())
            {
                AreaInfo area = ctrl.Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission) continue;
                yield return WalkGate(b, failures, 9f, 3f);
                tested++;
                schoolFlow.RestartMap();
                for (int f = 0; f < 4; f++) yield return null;
            }
            Time.timeScale = 1f;
            Assert.GreaterOrEqual(tested, 20, "ít cổng được kiểm hơn dự kiến");
            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }
    }
}
