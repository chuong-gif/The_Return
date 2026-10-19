/*
 * Mục đích: Dựng MissionRegistry cho tầng học từ các tham chiếu nhiệm vụ trong SchoolFloorFlow (T-003).
 * Ánh xạ khu <-> nhiệm vụ theo .ai/MAPS/area-adjacency.md mục 3 (tên object là chuẩn). Chưa ai gọi trong scene (T-004 mới nối).
 * Hàm: Create (đăng ký adapter vào MapProgress có sẵn); CreateDefault (vòng 6 phòng + đăng ký).
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Nhà máy dựng registry: gom 6 nhiệm vụ hiện có vào đúng khu của chúng.</summary>
    public static class MissionRegistryFactory
    {
        public const string AttendanceArea = "Phong_1";
        public const string GradeRepairArea = "Phong_2";
        public const string NavigationExamArea = "Phong_3";
        public const string TeacherTruthArea = "Phong_4";
        public const string ChoirArea = "Phong_6";
        public const string BreakerPanelArea = "Phong_5";
        public const string TeacherSaysArea = "Hanh_Lang_5";
        public const string QuietCorridorArea = "Hanh_Lang_1";
        public const string DontLookBackArea = "Hanh_Lang_3";
        public const string GlassBridgeArea = "Hanh_Lang_2";
        public const string SharedLanternArea = "Hanh_Lang_4";
        public const string PlankAndKeyArea = "Hanh_Lang_6";

        /// <summary>
        /// Nhận MapProgress (có đủ các khu trên) và SchoolFloorFlow giữ tham chiếu nhiệm vụ; đăng ký adapter cho mọi nhiệm vụ
        /// mà flow có tham chiếu (bỏ qua cái null). Trả registry (MapProgress của registry chính là tham số).
        /// </summary>
        public static MissionRegistry Create(MapProgress progress, SchoolFloorFlow flow)
        {
            var registry = new MissionRegistry(progress);
            if (flow == null) return registry;
            if (flow.attendance != null)
                registry.Register(new AttendanceMissionAdapter(AttendanceArea, flow.attendance, flow.attendanceHud));
            if (flow.gradeRepair != null)
                registry.Register(new GradeRepairMissionAdapter(GradeRepairArea, flow.gradeRepair, flow.gradeEntrance));
            if (flow.navigationExam != null)
                registry.Register(new NavigationExamMissionAdapter(NavigationExamArea, flow.navigationExam));
            if (flow.teacherTruth != null)
                registry.Register(new TeacherTruthMissionAdapter(TeacherTruthArea, flow.teacherTruth));
            if (flow.corridor != null)
                registry.Register(new QuietCorridorMissionAdapter(QuietCorridorArea, flow.corridor));
            if (flow.dontLookBack != null)
                registry.Register(new DontLookBackMissionAdapter(DontLookBackArea, flow.dontLookBack));
            if (flow.choir != null)
                registry.Register(new ChoirMissionAdapter(ChoirArea, flow.choir));
            if (flow.breakerPanel != null)
                registry.Register(new BreakerPanelMissionAdapter(BreakerPanelArea, flow.breakerPanel));
            if (flow.teacherSays != null)
                registry.Register(new TeacherSaysMissionAdapter(TeacherSaysArea, flow.teacherSays));
            if (flow.glassBridge != null)
                registry.Register(new GlassBridgeMissionAdapter(GlassBridgeArea, flow.glassBridge));
            if (flow.sharedLantern != null)
                registry.Register(new SharedLanternMissionAdapter(SharedLanternArea, flow.sharedLantern));
            if (flow.plankAndKey != null)
                registry.Register(new PlankAndKeyMissionAdapter(PlankAndKeyArea, flow.plankAndKey));
            return registry;
        }

        /// <summary>Nhận SchoolFloorFlow; tạo vòng 6 phòng/6 hành lang (chưa khu nào có nhiệm vụ) rồi đăng ký adapter qua Create.</summary>
        public static MissionRegistry CreateDefault(SchoolFloorFlow flow)
        {
            return Create(MapProgress.CreateRing(6, null), flow);
        }
    }
}
