/*
 * Mục đích: Adapter bọc BreakerPanelPrototype (Phong_5 «Bảng cầu dao») thành IMissionAdapter (T-006a2).
 * Hàm: constructor, AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;

namespace TheReturn
{
    /// <summary>Adapter BreakerPanel: Begin/StopSession và State.Solved.</summary>
    public sealed class BreakerPanelMissionAdapter : IMissionAdapter
    {
        readonly BreakerPanelPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public BreakerPanelMissionAdapter(string areaId, BreakerPanelPrototype mission)
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
