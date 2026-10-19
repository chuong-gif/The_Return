/*
 * Mục đích: Adapter bọc GlassBridgePrototype (Hanh_Lang_2 «Cầu kính») thành IMissionAdapter cho MissionRegistry (T-006e2).
 * Hàm: constructor (ID khu + nhiệm vụ), AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;

namespace TheReturn
{
    /// <summary>Adapter Cầu kính: Begin/StopSession và State.Solved.</summary>
    public sealed class GlassBridgeMissionAdapter : IMissionAdapter
    {
        readonly GlassBridgePrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public GlassBridgeMissionAdapter(string areaId, GlassBridgePrototype mission)
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
