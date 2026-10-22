/*
 * Mục đích: Nhà cung cấp HUD cho sáu nhiệm vụ mới (T-011): mỗi khu tự điền AreaHudModel (Progress = dòng tiến độ,
 * Objective = dòng trạng thái, Notebook = sổ tay của vai). Presenter chỉ gọi theo ID khu, không còn switch theo từng nhiệm vụ.
 * Hàm: AreaHudProviders.Find(flow, areaId) trả nhà cung cấp hoặc null; mỗi lớp lồng nhau thực hiện FillHud.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Tra cứu nhà cung cấp HUD theo ID khu cho các nhiệm vụ mới.</summary>
    public static class AreaHudProviders
    {
        /// <summary>Nhận flow và ID khu; trả nhà cung cấp HUD của khu đó, hoặc null nếu khu dùng đường HUD cũ/chưa có nhiệm vụ.</summary>
        public static IAreaHudProvider Find(SchoolFloorFlow flow, string areaId)
        {
            if (flow == null || areaId == null) return null;
            switch (areaId)
            {
                case "Phong_5": return flow.breakerPanel != null ? new Breaker(flow) : null;
                case "Phong_6": return flow.choir != null ? new ChoirHud(flow) : null;
                case "Hanh_Lang_2": return flow.glassBridge != null ? new Glass(flow) : null;
                case "Hanh_Lang_4": return flow.sharedLantern != null ? new Lantern(flow) : null;
                case "Hanh_Lang_5": return flow.teacherSays != null ? new Teacher(flow) : null;
                case "Hanh_Lang_6": return flow.plankAndKey != null ? new Plank(flow) : null;
                default: return LegacyAreaHud.Find(flow, areaId);
            }
        }

        sealed class Breaker : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Breaker(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var p = f.breakerPanel;
                m.Objective = p.Status;
                if (p.State != null) m.Progress = "Công tắc bật " + p.State.OnCount + "/" + p.State.Round.OverloadLimit;
                m.Notebook = p.Notebook(player);
            }
        }

        sealed class ChoirHud : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public ChoirHud(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var c = f.choir;
                m.Objective = c.Status;
                if (c.State != null) m.Progress = "Nốt " + c.State.Progress + "/" + c.State.Round.Length;
                m.Notebook = c.Notebook(player);
            }
        }

        sealed class Glass : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Glass(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var g = f.glassBridge;
                m.Objective = g.Status;
                if (g.State != null) m.Progress = "Đã qua cầu " + g.State.AcrossCount + "/" + f.attendance.PartySize + " • Lần rơi " + g.State.Falls;
                m.Notebook = g.Notebook(player);
            }
        }

        sealed class Lantern : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Lantern(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var l = f.sharedLantern;
                m.Objective = l.Status;
                if (l.State != null)
                    m.Progress = "Đèn: vai " + (l.State.Holder + 1) + " • Bị đưa về " + l.State.SentBackCount + " • Nhập sai " + l.State.WrongSubmits;
                m.Notebook = l.Notebook(player, now);
            }
        }

        sealed class Teacher : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Teacher(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                var t = f.teacherSays;
                m.Objective = t.Status;
                if (t.State != null)
                    m.Progress = "Đoạn " + Mathf.Min(t.State.Segment + 1, TeacherSaysRound.SegmentCount) + "/" + TeacherSaysRound.SegmentCount
                        + " • Cảnh cáo " + t.State.Warnings + "/" + t.State.Rules.MaxWarnings;
                m.Notebook = t.Notebook(player);
            }
        }

        sealed class Plank : IAreaHudProvider
        {
            readonly SchoolFloorFlow f; public Plank(SchoolFloorFlow flow) { f = flow; }
            public void FillHud(AreaHudModel m, int player, double now)
            {
                m.Objective = f.plankAndKey.Status;
                m.Notebook = f.plankAndKey.Notebook(player);
                if (f.plankAndKey.Session != null) m.Progress = "Lần " + f.plankAndKey.Session.State.Attempts + " • Còn " + f.plankAndKey.Session.DisplayRemainingSeconds.ToString("0") + " giây";
            }
        }
    }
}
