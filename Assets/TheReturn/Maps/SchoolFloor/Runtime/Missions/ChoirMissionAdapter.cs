/*
 * Mục đích: Adapter bọc ChoirPrototype (Phong_6 «Hợp xướng») thành IMissionAdapter cho MissionRegistry (T-006d2).
 * Hàm: constructor (ID khu + nhiệm vụ), AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;

namespace TheReturn
{
    /// <summary>Adapter Choir: Begin/StopSession và State.Solved.</summary>
    public sealed class ChoirMissionAdapter : IMissionAdapter
    {
        readonly ChoirPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public ChoirMissionAdapter(string areaId, ChoirPrototype mission)
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
