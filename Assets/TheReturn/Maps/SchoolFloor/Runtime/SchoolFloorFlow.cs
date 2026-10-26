/*
 * Mục đích: Điều phối sáu nhiệm vụ tầng học trong cùng một scene.
 * Hàm: Start gán chế độ nối map; Update chờ cả đội tập hợp rồi chuyển điều khiển;
 * RestartMap đặt lại cả sáu nhiệm vụ; OnGUI hiện hướng dẫn tập hợp.
 * T-004: khi có missionFlow đang bật, Update/OnGUI nhường quyền cho MissionFlowController; RestartMap reset registry và trả Attendance về trạng thái chờ.
 */
// ReachedNextRoom: nhận chỉ số tuyến, kiểm tra đủ thành viên tại vùng tập hợp và trả về bool.
// ContainsPlayer: kiểm tra vị trí trong hộp theo trục local để vùng tập hợp xoay vẫn chính xác.
using UnityEngine;
namespace TheReturn
{
    public sealed class SchoolFloorFlow : MonoBehaviour
    {
        public bool useCanvas;
        public AttendancePrototype attendance;
        public AttendanceHud attendanceHud;
        public QuietCorridorPrototype corridor;
        public BoxCollider stagingZone;
        public GradeRepairPrototype gradeRepair;
        public PuzzleDoor gradeEntrance;
        public NavigationExamPrototype navigationExam;
        public DontLookBackPrototype dontLookBack;
        public TeacherTruthPrototype teacherTruth;
        public ChoirPrototype choir;
        public BreakerPanelPrototype breakerPanel;
        public TeacherSaysPrototype teacherSays;
        [Tooltip("Hanh_Lang_2 «Cầu kính» (T-006e2).")]
        public GlassBridgePrototype glassBridge;
        public SharedLanternPrototype sharedLantern;
        public PlankAndKeyPrototype plankAndKey;
        public int Gathered { get; private set; }
        public BoxCollider[] transitionGatherZones;
        [Tooltip("Nếu có và đang bật: luồng tự chọn khu (T-004) thay chuỗi if tuyến tính trong Update.")]
        public MissionFlowController missionFlow;

        /// <summary>Nhận chỉ số tuyến; trả true khi cả đội đứng trong vùng đích, hoặc map cũ chưa có tuyến.</summary>
        bool ReachedNextRoom(int index)
        {
            if (transitionGatherZones == null || transitionGatherZones.Length <= index || transitionGatherZones[index] == null) return true;
            var zone = transitionGatherZones[index];
            Gathered = 0;
            for (int i = 0; i < attendance.party.PartySize; i++)
                if (ContainsPlayer(zone, attendance.party.players[i].position)) Gathered++;
            return attendance.party.PartySize >= 2 && Gathered == attendance.party.PartySize;
        }

        /// <summary>Nhận collider và vị trí world; trả true khi điểm nằm trong hộp thật, kể cả hộp đã xoay.</summary>
        static bool ContainsPlayer(BoxCollider zone, Vector3 position)
        {
            return zone != null && new Bounds(zone.center, zone.size).Contains(zone.transform.InverseTransformPoint(position));
        }

        /// <summary>Không nhận tham số; bật chế độ nối tiếp để cửa lớp không kết thúc bản thử.</summary>
        void Start()
        {
            if (GetComponent<SchoolFloorAudioDirector>() == null) gameObject.AddComponent<SchoolFloorAudioDirector>().flow = this;
            if (GetComponent<MissionAudioDirector>() == null) gameObject.AddComponent<MissionAudioDirector>().flow = this;
            attendance.continueIntoMap = true;
            if (PartySession.HasStartedParty) attendance.SetParticipantCount(PartySession.PlayerCount);
        }

        /// <summary>Không nhận tham số; kiểm điều kiện hoàn thành và chuyển quyền điều khiển tuần tự qua sáu nhiệm vụ.</summary>
        void Update()
        {
            if (missionFlow != null && missionFlow.isActiveAndEnabled) return;
            if (teacherTruth != null && teacherTruth.Active) return;
            if (dontLookBack != null && dontLookBack.Active)
            {
                if (teacherTruth != null && dontLookBack.State.Solved && ReachedNextRoom(4))
                {
                    dontLookBack.StopSession(false);
                    teacherTruth.Begin();
                }
                return;
            }
            if (navigationExam != null && navigationExam.Active)
            {
                if (dontLookBack != null && navigationExam.State.Solved && !navigationExam.PanelOpen && ReachedNextRoom(3))
                {
                    navigationExam.StopSession(false);
                    dontLookBack.Begin();
                }
                return;
            }
            if (gradeRepair != null && gradeRepair.Active)
            {
                if (navigationExam != null && gradeRepair.State != null && gradeRepair.State.Solved && !gradeRepair.PanelOpen && ReachedNextRoom(2))
                {
                    gradeRepair.StopSession(false);
                    navigationExam.Begin();
                }
                return;
            }
            if (corridor.Active)
            {
                if (gradeRepair != null && corridor.State != null && corridor.State.Solved && ReachedNextRoom(1))
                {
                    corridor.StopSession(false);
                    gradeEntrance.SetOpen(true);
                    gradeRepair.Begin();
                }
                return;
            }
            if (!attendance.Begun || attendance.State == null || !attendance.State.Solved) return;
            Gathered = 0;
            for (int i = 0; i < attendance.PartySize; i++)
                if (attendance.State.SeatOf(i) < 0 && ContainsPlayer(stagingZone, attendance.party.players[i].position)) Gathered++;
            if (Gathered != attendance.PartySize) return;
            if (!ReachedNextRoom(0)) return;
            attendance.ExternalControl = true;
            attendanceHud.enabled = false;
            corridor.Begin();
        }

        /// <summary>Không nhận tham số; người dùng chọn chơi lại cả map thì bật lớp và sinh đề mới.</summary>
        public void RestartMap()
        {
            if (missionFlow != null) missionFlow.ResetFlow();
            if (teacherTruth != null) teacherTruth.StopSession();
            if (dontLookBack != null) dontLookBack.StopSession();
            if (glassBridge != null) glassBridge.StopSession();
            if (plankAndKey != null) plankAndKey.StopSession();
            if (choir != null) choir.StopSession();
            if (breakerPanel != null) breakerPanel.StopSession();
            if (teacherSays != null) teacherSays.StopSession();
            if (navigationExam != null) navigationExam.StopSession();
            if (gradeRepair != null) gradeRepair.StopSession();
            if (gradeEntrance != null) gradeEntrance.SetOpen(false, true);
            corridor.StopSession();
            bool freeRoam = missionFlow != null && missionFlow.isActiveAndEnabled;
            attendance.ExternalControl = freeRoam;
            attendanceHud.enabled = !freeRoam;
            attendance.StartNewRound();
            PrototypePartyController.SetCursor(false);
            Gathered = 0;
        }

        /// <summary>Không nhận tham số; hiện thông báo chuyển tiếp trên lớp, không sửa trạng thái nhiệm vụ.</summary>
        void OnGUI()
        {
            if (missionFlow != null && missionFlow.isActiveAndEnabled) return;
            if ((teacherTruth != null && teacherTruth.Active) || (dontLookBack != null && dontLookBack.Active) || (navigationExam != null && navigationExam.Active) || (gradeRepair != null && gradeRepair.Active) || corridor.Active || attendance.State == null || !attendance.State.Solved) return;
            if (useCanvas) return;
            var style = new GUIStyle(GUI.skin.box) { fontSize = 18, wordWrap = true };
            GUI.Box(new Rect(20, Screen.height * .25f, Mathf.Min(520, Screen.width - 40), 115),
                corridor.settings.waiting + "\nĐã tập hợp: " + Gathered + "/" + attendance.PartySize, style);
        }
    }
}
