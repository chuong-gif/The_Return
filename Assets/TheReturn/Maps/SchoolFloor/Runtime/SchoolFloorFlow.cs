/*
 * Mục đích: Điều phối lớp điểm danh và hành lang trong cùng một scene.
 * Hàm: Start gán chế độ nối map; Update chờ cả đội tập hợp rồi chuyển điều khiển;
 * RestartMap đặt lại cả hai nhiệm vụ; OnGUI hiện hướng dẫn tập hợp.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class SchoolFloorFlow : MonoBehaviour
    {
        public AttendancePrototype attendance;
        public AttendanceHud attendanceHud;
        public QuietCorridorPrototype corridor;
        public BoxCollider stagingZone;
        public int Gathered { get; private set; }

        /// <summary>Không nhận tham số; bật chế độ nối tiếp để cửa lớp không kết thúc bản thử.</summary>
        void Start() { attendance.continueIntoMap = true; }

        /// <summary>Không nhận tham số; đếm đội đã đứng trong vùng tập hợp, chỉ chuyển nhiệm vụ khi đủ đội đã điểm danh.</summary>
        void Update()
        {
            if (corridor.Active || !attendance.Begun || attendance.State == null || !attendance.State.Solved) return;
            Gathered = 0;
            for (int i = 0; i < attendance.PartySize; i++)
                if (attendance.State.SeatOf(i) < 0 && stagingZone.bounds.Contains(attendance.party.players[i].position)) Gathered++;
            if (Gathered != attendance.PartySize) return;
            attendance.ExternalControl = true;
            attendanceHud.enabled = false;
            corridor.Begin();
        }

        /// <summary>Không nhận tham số; người dùng chọn chơi lại cả map thì bật lớp và sinh đề mới.</summary>
        public void RestartMap()
        {
            corridor.StopSession();
            attendance.ExternalControl = false;
            attendanceHud.enabled = true;
            attendance.StartNewRound();
            PrototypePartyController.SetCursor(false);
            Gathered = 0;
        }

        /// <summary>Không nhận tham số; hiện thông báo chuyển tiếp trên lớp, không sửa trạng thái nhiệm vụ.</summary>
        void OnGUI()
        {
            if (corridor.Active || attendance.State == null || !attendance.State.Solved) return;
            var style = new GUIStyle(GUI.skin.box) { fontSize = 18, wordWrap = true };
            GUI.Box(new Rect(20, Screen.height * .25f, Mathf.Min(520, Screen.width - 40), 115),
                corridor.settings.waiting + "\nĐã tập hợp: " + Gathered + "/" + attendance.PartySize, style);
        }
    }
}
