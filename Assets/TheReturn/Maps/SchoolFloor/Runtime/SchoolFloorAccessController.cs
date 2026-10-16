/*
 * Mục đích: đồng bộ khóa cả ba lối phòng và hai đầu hành lang với tiến trình nhiệm vụ.
 * Awake/LateUpdate: áp dụng quyền cửa lúc tải và sau cập nhật nhiệm vụ.
 * Refresh: nếu có missionFlow (T-004) thì mở/khóa theo MapProgress: khu chưa có nhiệm vụ và khu đã xong luôn mở, khu chưa xong mở khi
 * không có khu nào đang làm, khu đang làm khóa; không có missionFlow thì dùng luồng tuyến tính cũ (mở khu đã xong, chỉ mở lối vào khu kế tiếp).
 * IsSolved/IsActive: đọc trạng thái nhiệm vụ bằng chỉ số 0..5 (luồng cũ); không sửa đáp án hay trạng thái luật chơi.
 */
using UnityEngine;
namespace TheReturn
{
    [DefaultExecutionOrder(200)]
    public sealed class SchoolFloorAccessController : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        public SchoolPreparationGate[] gates;
        [Tooltip("Nếu có và đang bật: cửa mở/khóa theo luồng tự chọn khu (T-004) thay vì chỉ số tuyến tính cũ.")]
        public MissionFlowController missionFlow;

        /// <summary>Không có đầu vào; đóng đúng các lối trước khung hình chơi đầu tiên.</summary>
        void Awake(){Refresh();}
        /// <summary>Không có đầu vào; đồng bộ sau controller nhiệm vụ mỗi khung hình.</summary>
        void LateUpdate(){Refresh();}
        /// <summary>Không có đầu vào; áp dụng quyền truy cập cho mọi cửa, kể cả sau retry và test nhanh.</summary>
        public void Refresh()
        {
            if(!flow || gates==null)return;
            bool useFlow=missionFlow && missionFlow.isActiveAndEnabled;
            foreach(var gate in gates)
            {
                if(!gate)continue;
                bool flowOpen,flowActive,flowDone;
                if(useFlow && missionFlow.TryGetGateState(gate,out flowOpen,out flowActive,out flowDone))
                {
                    gate.Apply(flowOpen,flowActive,flow.attendance.party,flowDone);
                    continue;
                }
                int m=gate.mission;bool active=m>=0 && IsActive(m);
                bool open=m<0 || IsSolved(m) || (!active && gate.designatedEntrance && m>0 && IsSolved(m-1));
                gate.Apply(open,active,flow.attendance.party);
            }
        }
        /// <summary>Nhận chỉ số 0..5; trả true khi nhiệm vụ hiện có trạng thái hoàn thành.</summary>
        public bool IsSolved(int mission)
        {
            switch(mission)
            {
                case 0:return flow.attendance.State!=null && flow.attendance.State.Solved;
                case 1:return flow.corridor.State!=null && flow.corridor.State.Solved;
                case 2:return flow.gradeRepair.State!=null && flow.gradeRepair.State.Solved;
                case 3:return flow.navigationExam.State!=null && flow.navigationExam.State.Solved;
                case 4:return flow.dontLookBack.State!=null && flow.dontLookBack.State.Solved;
                case 5:return flow.teacherTruth.State!=null && flow.teacherTruth.State.Solved;
                default:return false;
            }
        }
        /// <summary>Nhận chỉ số 0..5; trả true khi controller tương ứng đang giữ quyền chơi.</summary>
        public bool IsActive(int mission)
        {
            switch(mission)
            {
                case 0:return !flow.attendance.ExternalControl;
                case 1:return flow.corridor.Active;
                case 2:return flow.gradeRepair.Active;
                case 3:return flow.navigationExam.Active;
                case 4:return flow.dontLookBack.Active;
                case 5:return flow.teacherTruth.Active;
                default:return false;
            }
        }
    }
}
