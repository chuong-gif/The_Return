/*
 * Mục đích: Sáu adapter bọc các prototype nhiệm vụ hiện có thành IMissionAdapter cho MissionRegistry (T-003).
 * Chỉ ĐỌC trạng thái và gọi API công khai sẵn có theo đúng quy ước bàn giao của SchoolFloorFlow; không sửa logic nhiệm vụ,
 * chưa được ai dùng trong scene (T-004 mới nối).
 * Kiểu: AttendanceMissionAdapter, QuietCorridorMissionAdapter, GradeRepairMissionAdapter,
 * NavigationExamMissionAdapter, DontLookBackMissionAdapter, TeacherTruthMissionAdapter.
 * Hàm mỗi adapter: constructor (ID khu + nhiệm vụ), AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Adapter Attendance: chạy theo Begun/ExternalControl, bàn giao bằng ExternalControl và tắt HUD như SchoolFloorFlow.</summary>
    public sealed class AttendanceMissionAdapter : IMissionAdapter
    {
        readonly AttendancePrototype mission;
        readonly Behaviour hud;

        public string AreaId { get; }
        public bool Active => mission.Begun && !mission.ExternalControl;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu, prototype Attendance (không null) và HUD tùy chọn (null được); tạo adapter.</summary>
        public AttendanceMissionAdapter(string areaId, AttendancePrototype mission, Behaviour hud)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
            this.hud = hud;
        }

        /// <summary>Không nhận tham số; trả quyền điều khiển cho Attendance, bật HUD và bắt đầu phiên nếu chưa bắt đầu.</summary>
        public void Begin()
        {
            mission.ExternalControl = false;
            if (hud != null) hud.enabled = true;
            if (!mission.Begun) mission.BeginSession();
        }

        /// <summary>Nhận cờ reset; reset=true đặt lại chỗ ngồi (giữ đề); luôn nhường quyền (ExternalControl) và tắt HUD.</summary>
        public void Stop(bool reset)
        {
            if (reset) mission.RetryRound();
            mission.ExternalControl = true;
            if (hud != null) hud.enabled = false;
        }
    }

    /// <summary>Adapter QuietCorridor (hành lang): Begin/StopSession và State.Solved.</summary>
    public sealed class QuietCorridorMissionAdapter : IMissionAdapter
    {
        readonly QuietCorridorPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public QuietCorridorMissionAdapter(string areaId, QuietCorridorPrototype mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
        }

        /// <summary>Không nhận tham số; bắt đầu nhiệm vụ.</summary>
        public void Begin() { mission.Begin(); }

        /// <summary>Nhận cờ reset; dừng phiên (reset=false giữ trạng thái đã giải).</summary>
        public void Stop(bool reset) { mission.StopSession(reset); }
    }

    /// <summary>Adapter GradeRepair (phòng): mở cửa vào trước khi bắt đầu; chỉ bàn giao khi bảng điều khiển đã đóng.</summary>
    public sealed class GradeRepairMissionAdapter : IMissionAdapter
    {
        readonly GradeRepairPrototype mission;
        readonly PuzzleDoor entrance;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved && !mission.PanelOpen;

        /// <summary>Nhận ID khu, prototype (không null) và cửa vào tùy chọn (null được); tạo adapter.</summary>
        public GradeRepairMissionAdapter(string areaId, GradeRepairPrototype mission, PuzzleDoor entrance)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
            this.entrance = entrance;
        }

        /// <summary>Không nhận tham số; mở cửa vào (nếu có) rồi bắt đầu nhiệm vụ.</summary>
        public void Begin()
        {
            if (entrance != null) entrance.SetOpen(true);
            mission.Begin();
        }

        /// <summary>Nhận cờ reset; dừng phiên; reset=true đóng cửa vào ngay để làm lại sạch.</summary>
        public void Stop(bool reset)
        {
            mission.StopSession(reset);
            if (reset && entrance != null) entrance.SetOpen(false, true);
        }
    }

    /// <summary>Adapter NavigationExam (phòng): chỉ bàn giao khi bảng điều khiển đã đóng.</summary>
    public sealed class NavigationExamMissionAdapter : IMissionAdapter
    {
        readonly NavigationExamPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved && !mission.PanelOpen;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public NavigationExamMissionAdapter(string areaId, NavigationExamPrototype mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
        }

        /// <summary>Không nhận tham số; bắt đầu nhiệm vụ.</summary>
        public void Begin() { mission.Begin(); }

        /// <summary>Nhận cờ reset; dừng phiên (reset=false giữ trạng thái đã giải).</summary>
        public void Stop(bool reset) { mission.StopSession(reset); }
    }

    /// <summary>Adapter DontLookBack (hành lang): Begin/StopSession và State.Solved.</summary>
    public sealed class DontLookBackMissionAdapter : IMissionAdapter
    {
        readonly DontLookBackPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public DontLookBackMissionAdapter(string areaId, DontLookBackPrototype mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
        }

        /// <summary>Không nhận tham số; bắt đầu nhiệm vụ.</summary>
        public void Begin() { mission.Begin(); }

        /// <summary>Nhận cờ reset; dừng phiên (reset=false giữ trạng thái đã giải).</summary>
        public void Stop(bool reset) { mission.StopSession(reset); }
    }

    /// <summary>Adapter TeacherTruth (phòng): Begin/StopSession và State.Solved.</summary>
    public sealed class TeacherTruthMissionAdapter : IMissionAdapter
    {
        readonly TeacherTruthPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public TeacherTruthMissionAdapter(string areaId, TeacherTruthPrototype mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            AreaId = areaId;
            this.mission = mission;
        }

        /// <summary>Không nhận tham số; bắt đầu nhiệm vụ.</summary>
        public void Begin() { mission.Begin(); }

        /// <summary>Nhận cờ reset; dừng phiên (reset=false giữ trạng thái đã giải).</summary>
        public void Stop(bool reset) { mission.StopSession(reset); }
    }
}
