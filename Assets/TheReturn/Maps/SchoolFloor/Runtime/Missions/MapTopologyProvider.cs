/*
 * Mục đích: Cầu nối NHỎ (T-040) để MissionFlowController lấy topology map từ một nguồn khác vòng 12 khu cũ. Scene mới đặt một component kế thừa lớp này
 * (vd Core4TopologyProvider) vào MissionFlowController.topology; để trống thì MissionFlowController chạy y như cũ (MapProgress.CreateRing(6, null)).
 * Chỉ khai MỘT hàm trừu tượng nên không kéo thêm phụ thuộc vào assembly SchoolFloor.
 * Hàm: CreateProgress.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Nguồn topology (danh sách khu + khu kề + khu nào có nhiệm vụ) cho luồng nhiệm vụ.</summary>
    public abstract class MapTopologyProvider : MonoBehaviour
    {
        /// <summary>Không nhận tham số; trả MapProgress MỚI mỗi lần gọi (chưa có tiến độ), đã đặt cờ HasMission đúng các khu có nhiệm vụ.</summary>
        public abstract MapProgress CreateProgress();
    }
}
