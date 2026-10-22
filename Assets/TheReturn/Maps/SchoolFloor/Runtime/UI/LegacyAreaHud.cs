/*
 * Mục đích: Nhà cung cấp HUD cho sáu khu cũ (T-011 S5): Phong_1..4, Hanh_Lang_1, Hanh_Lang_3. Chữ lấy đúng như đường HUD cũ
 * trong SchoolFloorCanvasPresenter để không đổi hành vi; Prompt = dòng nhắc tương tác, Controls = dòng phím riêng của khu.
 * Hàm: LegacyAreaHud.Find(flow, areaId) trả nhà cung cấp hoặc null.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Tra cứu nhà cung cấp HUD cho sáu khu cũ.</summary>
    public static class LegacyAreaHud
    {
        /// <summary>Nhận flow và ID khu; trả nhà cung cấp HUD của khu cũ tương ứng, hoặc null nếu khu đó thiếu nhiệm vụ hay ID lạ.</summary>
        public static IAreaHudProvider Find(SchoolFloorFlow flow, string areaId)
        {
            if (flow == null || areaId == null) return null;
            switch (areaId)
            {
                case "Phong_1": return flow.attendance != null ? new Attendance(flow) : null;
                case "Hanh_Lang_1": return flow.corridor != null ? new Quiet(flow) : null;
                case "Phong_2": return flow.gradeRepair != null ? new Grade(flow) : null;
                case "Phong_3": return flow.navigationExam != null ? new Exam(flow) : null;
                case "Hanh_Lang_3": return flow.dontLookBack != null ? new Back(flow) : null;
                case "Phong_4": return flow.teacherTruth != null ? new Truth(flow) : null;
                default: return null;
            }
        }

        sealed class Attendance : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Attendance(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var a = f.attendance;
                if (a.State == null) return;
                m.Progress = a.State.Solved ? "Đã điểm danh • Tập hợp ngoài lớp: " + f.Gathered + "/" + a.PartySize
                    : "Đã ngồi " + a.State.OccupiedCount + "/" + a.PartySize + " • 24 ghế";
                m.Objective = a.State.Solved ? "Đứng dậy và đưa cả đội ra vùng TẬP HỢP ngoài cửa lớp." : a.Status;
                m.Prompt = a.State.SeatOf(a.party.ActivePlayer) >= 0 ? "[E] Đứng dậy"
                    : a.TargetSeat >= 0 ? "[E] Ngồi " + AttendanceRound.SeatCode(a.TargetSeat) : "Nhìn vào bàn/ghế gần để tương tác";
                m.Controls = "WASD đi • E ngồi/đứng • H gợi ý • 1–4 đổi vai • Tab menu";
                m.Notebook = a.CluePackets[a.ActivePlayer];
            }
        }

        sealed class Quiet : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Quiet(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var q = f.corridor;
                if (q.State == null) return;
                m.Progress = "Tiếng ồn " + Mathf.CeilToInt(q.State.Noise) + "/100 • "
                    + (q.State.SpeakerOn ? "Loa đang rè" : q.State.Held ? "Loa đã tắt" : "Ân hạn " + q.State.Grace.ToString("0.0") + "s");
                m.Objective = q.Status;
                m.Prompt = q.IsHolding(f.attendance.party.ActivePlayer) ? "[E] Nhả nút"
                    : q.TargetStation >= 0 ? "[E] Giữ nút " + (q.TargetStation == 0 ? "A" : "B")
                    : "A: " + (q.State.Owner(0) + 1) + " • B: " + (q.State.Owner(1) + 1) + " • 0 = chưa giữ";
                m.Controls = q.settings.localControls;
            }
        }

        sealed class Grade : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Grade(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var g = f.gradeRepair;
                if (g.State == null) return;
                m.Progress = "Tổng cần đạt 32 • Mỗi hồ sơ 6–10";
                m.Objective = g.Status;
                m.Prompt = g.State.Solved ? g.catalog.success : g.Target == null ? "Tới bàn chứng từ hoặc bảng sửa"
                    : g.Target.document < 0 ? "[E] Mở bảng sửa" : "[E] Hồ sơ của vai " + (g.State.Round.OwnerOf(g.Target.document) + 1);
                m.Controls = g.catalog.controls;
            }
        }

        sealed class Exam : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Exam(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var e = f.navigationExam;
                if (e.State == null) return;
                m.Progress = NavigationExamState.Code(e.State.Start) + " → " + NavigationExamState.Code(e.State.Goal)
                    + " • Dấu " + e.State.Stamps + "/2 • Lượt vai " + (e.State.Turn + 1);
                m.Objective = e.Status;
                m.Prompt = e.State.Solved ? e.catalog.success : e.Targeted ? "[E] Mở sa bàn" : "Tới gần sa bàn để tương tác";
                m.Controls = e.catalog.controls;
                m.Notebook = e.catalog.Packet(e.State, f.attendance.party.ActivePlayer);
            }
        }

        sealed class Back : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Back(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var b = f.dontLookBack;
                if (b.State == null) return;
                m.Progress = "Ký hiệu đã được đọc " + b.State.WitnessedCount + "/" + f.attendance.PartySize
                    + " • Checkpoint " + (b.Checkpoint + 1) + " • Lần quay đầu " + b.State.Penalties;
                m.Objective = b.Warning ? b.catalog.warning : b.Status;
                m.Prompt = b.State.Solved ? b.catalog.success : b.ReadTarget >= 0 ? "[E] Đọc thẻ " + (b.ReadTarget + 1)
                    : b.LockTarget ? "[E] Mở bảng khóa" : "Hướng an toàn +Z • Lùi để nhìn lưng đồng đội";
                m.Controls = b.catalog.controls;
                m.Notebook = b.catalog.Notebook(b.State, f.attendance.party.ActivePlayer);
            }
        }

        sealed class Truth : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Truth(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var t = f.teacherTruth;
                if (t.State == null) return;
                m.Progress = t.ResultText();
                m.Objective = t.Status;
                m.Prompt = t.State.Solved ? t.catalog.success
                    : t.TargetTeacher != null ? "[E] Nghe kênh " + (t.TargetTeacher.channel + 1)
                    : t.TargetStation != null ? "[E] Xác nhận thủ công " + (t.TargetStation.channel + 1)
                    : "Tìm bốn giáo viên, đèn MẤT ĐỒNG BỘ và bốn nút xác nhận";
                m.Controls = t.catalog.controls;
                m.Notebook = t.catalog.Notebook(t.State, f.attendance.party.ActivePlayer);
            }
        }
    }
}
