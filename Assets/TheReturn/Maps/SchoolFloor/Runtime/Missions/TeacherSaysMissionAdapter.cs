/*
 * Mục đích: Adapter bọc TeacherSaysPrototype (Hanh_Lang_5 «Cô nói») thành IMissionAdapter (T-006c).
 * Hàm: constructor, AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;

namespace TheReturn
{
    /// <summary>Adapter TeacherSays: Begin/StopSession và State.Solved.</summary>
    public sealed class TeacherSaysMissionAdapter : IMissionAdapter
    {
        readonly TeacherSaysPrototype mission;

        public string AreaId { get; }
        public bool Active => mission.Active;
        public bool Solved => mission.State != null && mission.State.Solved;
        public bool CanHandOff => Solved;

        /// <summary>Nhận ID khu và prototype (không null); tạo adapter.</summary>
        public TeacherSaysMissionAdapter(string areaId, TeacherSaysPrototype mission)
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
