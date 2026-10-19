/*
 * Mục đích: Adapter bọc PlankAndKeyPrototype (Hanh_Lang_6 «Cầu ván và thẻ từ») thành IMissionAdapter cho MissionRegistry (T-006b2).
 * Hàm: constructor (ID khu + nhiệm vụ), AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;
namespace TheReturn
{
    /// <summary>Adapter Cầu ván: Begin/StopSession và PlankSession.Solved (chỉ sau khi nhóm cuối đã đến nơi).</summary>
    public sealed class PlankAndKeyMissionAdapter : IMissionAdapter
    {
        readonly PlankAndKeyPrototype mission;
        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.Session != null && mission.Session.Solved;
        public bool CanHandOff => mission.Session != null && mission.Session.CanHandOff;
        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public PlankAndKeyMissionAdapter(string areaId, PlankAndKeyPrototype mission)
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
