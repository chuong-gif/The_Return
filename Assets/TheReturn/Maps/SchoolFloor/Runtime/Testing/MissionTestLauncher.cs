/*
 * Mục đích: Vào thẳng nhiệm vụ trong Editor/Development Build, chuẩn bị tiến trình trước bằng luật thật.
 * Hàm: Start áp lựa chọn Inspector; Launch đặt lại phiên và mở nhiệm vụ;
 * PrepareAttendance / PrepareCorridor / PrepareGrades / PrepareExam / PrepareBack hoàn tất điều kiện trước;
 * PlaceParty đưa đội tới checkpoint; Available giới hạn công cụ test; EndTestSession xóa nhãn test.
 */
using UnityEngine;
namespace TheReturn
{
    public enum MissionTestEntry
    {
        NormalPlay = -1, Attendance = 0, QuietCorridor = 1, GradeRepair = 2, NavigationExam = 3, DontLookBack = 4, TeacherTruth = 5,
        Choir = 6, BreakerPanel = 7, GlassBridge = 8, SharedLantern = 9, PlankAndKey = 10, TeacherSays = 11
    }
    public sealed class MissionTestLauncher : MonoBehaviour
    {
        /// <summary>Khu theo chỉ số bài test (0-5 khớp thứ tự cũ; 6+ là nhiệm vụ mới). Thêm nhiệm vụ mới: thêm cuối hai bảng này.</summary>
        public static readonly string[] TestAreas =
            { "Phong_1", "Hanh_Lang_1", "Phong_2", "Phong_3", "Hanh_Lang_3", "Phong_4", "Phong_6", "Phong_5", "Hanh_Lang_2", "Hanh_Lang_4", "Hanh_Lang_6", "Hanh_Lang_5" };
        /// <summary>Tên hiển thị tương ứng TestAreas.</summary>
        public static readonly string[] TestLabels =
            { "Phòng 1 • Điểm danh", "Hành lang 1 • Giữ trật tự", "Phòng 2 • Sửa bảng điểm", "Phòng 3 • Bài kiểm tra", "Hành lang 3 • Không quay đầu",
              "Phòng 4 • Giáo viên thật/dối", "Phòng 6 • Hợp xướng", "Phòng 5 • Bảng cầu dao", "Hành lang 2 • Cầu kính", "Hành lang 4 • Đèn lồng chung",
              "Hành lang 6 • Cầu ván và thẻ từ", "Hành lang 5 • Cô nói" };
        /// <summary>Trả số bài test hiện có.</summary>
        public static int AreaCount { get { return TestAreas.Length; } }
        /// <summary>Nhận chỉ số bài test; trả ID khu hoặc null nếu ngoài khoảng.</summary>
        public static string AreaId(int index) { return index>=0 && index<TestAreas.Length ? TestAreas[index] : null; }
        /// <summary>Nhận chỉ số bài test; trả tên hiển thị hoặc chuỗi rỗng nếu ngoài khoảng.</summary>
        public static string AreaLabel(int index) { return index>=0 && index<TestLabels.Length ? TestLabels[index] : ""; }

        public SchoolFloorFlow flow;
        [Tooltip("-1 chơi bình thường (tự chọn phòng); 0-10 vào thẳng khu theo bảng TestAreas (0 điểm danh, 1 giữ trật tự, 2 bảng điểm, 3 bài kiểm tra, 4 không quay đầu, 5 giáo viên, 6 hợp xướng, 7 cầu dao, 8 cầu kính, 9 đèn lồng, 10 cầu ván, 11 cô nói).")]
        public MissionTestEntry startMission = MissionTestEntry.NormalPlay;
        [Range(2,4)] public int testPartySize = 4;
        public bool TestSession { get; private set; }
        public int SelectedMission { get; private set; }

        /// <summary>Không nhận tham số; xóa cờ test khi người dùng chọn chơi lại toàn bộ map.</summary>
        public void EndTestSession() { TestSession=false; }

        /// <summary>Không nhận tham số; trả true chỉ trong Editor hoặc Development Build.</summary>
        public static bool Available()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }

        /// <summary>Không nhận tham số; chạy lựa chọn Inspector sau khi các controller đã Awake.</summary>
        void Start()
        {
            if (Available() && (int)startMission >= 0) Launch((int)startMission,testPartySize);
        }

        /// <summary>Nhận nhiệm vụ 0–5 và đội 2–4; tạo phiên test sạch, trả false nếu sai hoặc đang ngoài Play.</summary>
        public bool Launch(int mission,int count)
        {
            if (!Available() || !Application.isPlaying || count<2 || count>4) return false;
            bool freeFlow = flow.missionFlow != null && flow.missionFlow.isActiveAndEnabled && flow.missionFlow.Registry != null;
            if (freeFlow) return LaunchArea(mission,count);
            return mission>=0 && mission<=5 && LaunchLegacy(mission,count);
        }

        /// <summary>Nhận bài test và đội; đặt lại bản đồ, đặt đội ở lối vào rồi bắt đầu khu bằng luật thật của registry. Trả false nếu khu chưa có nhiệm vụ.</summary>
        bool LaunchArea(int mission,int count)
        {
            string area=AreaId(mission);
            if (area==null) return false;
            flow.RestartMap();
            flow.attendance.SetParticipantCount(count);
            GateBinding entry=FindEntry(area);
            string entryFrom=entry!=null?entry.entryFromId:"San";
            BeginResult result=flow.missionFlow.Registry.TryBegin(area,entryFrom,true);
            if (result!=BeginResult.Started)
            {
                Debug.LogWarning("MissionTestLauncher: không bắt đầu được "+area+" ("+result+"). Khu có nhiệm vụ trong scene chưa?",this);
                return false;
            }
            SelectedMission=mission;
            TestSession=true;
            if (entry!=null) PlaceInGate(entry);
            PrototypePartyController.SetCursor(true);
            return true;
        }

        /// <summary>Nhận ID khu; trả cổng vào ưu tiên từ sân (phòng), nếu không có thì cổng đầu tiên của khu (hành lang), null nếu không có.</summary>
        GateBinding FindEntry(string area)
        {
            GateBinding first=null;
            if (flow.missionFlow.bindings==null) return null;
            foreach (GateBinding b in flow.missionFlow.bindings)
            {
                if (b==null || b.gate==null || b.areaId!=area) continue;
                if (b.entryFromId=="San") return b;
                if (first==null) first=b;
            }
            return first;
        }

        /// <summary>Nhận cổng vào; đặt từng thành viên lên sàn tại khu chuẩn bị của cổng (rải trong hộp), chọn vai 1.</summary>
        void PlaceInGate(GateBinding entry)
        {
            var party=flow.attendance.party;
            BoxCollider zone=entry.gate.preparationZone;
            if (zone==null) return;
            Vector3 center=zone.transform.TransformPoint(zone.center);
            Vector3 half=Vector3.Scale(zone.size,zone.transform.lossyScale)*0.5f;
            for (int i=0;i<party.PartySize;i++)
            {
                float ox=(i%2==0?-1f:1f)*half.x*0.4f, oz=(i<2?-1f:1f)*half.z*0.4f;
                Vector3 p=center+zone.transform.right*ox+zone.transform.forward*oz;
                RaycastHit hit;
                float y=center.y;
                if (Physics.Raycast(p+Vector3.up*3f,Vector3.down,out hit,10f,~0,QueryTriggerInteraction.Ignore)) y=hit.point.y+0.1f;
                party.Teleport(i,new Vector3(p.x,y,p.z));
            }
            party.SwitchRole(0,false);
        }

        /// <summary>Nhận nhiệm vụ 0-5 và đội 2-4; luồng tuyến tính cũ (không có missionFlow): chuẩn bị tiến trình bằng luật thật rồi mở nhiệm vụ.</summary>
        bool LaunchLegacy(int mission,int count)
        {
            flow.RestartMap();
            flow.attendance.ExternalControl=false; flow.attendanceHud.enabled=true; // giữ hành vi cũ của bản thử (T-004: RestartMap mới trả Attendance về chờ)
            flow.attendance.SetParticipantCount(count);
            flow.attendance.BeginSession();
            SelectedMission=mission;
            TestSession=true;
            if(mission==0) return true;
            PrepareAttendance();
            flow.corridor.Begin();
            if(mission==1) { PlaceParty(flow.corridor.checkpoints); return true; }
            PrepareCorridor();
            flow.gradeRepair.Begin();
            if(mission==2) { PlaceParty(flow.gradeRepair.checkpoints); return true; }
            PrepareGrades();
            flow.navigationExam.Begin();
            if(mission==3) { PlaceParty(flow.navigationExam.checkpoints); return true; }
            PrepareExam();
            flow.dontLookBack.Begin();
            flow.dontLookBack.Retry();
            if(mission==4) return true;
            PrepareBack();
            flow.teacherTruth.Begin();
            return true;
        }

        /// <summary>Không nhận tham số; giải điểm danh theo đáp án đề, nhả ghế và mở cửa để phiên test nhất quán.</summary>
        void PrepareAttendance()
        {
            var a=flow.attendance;
            int[] answer=a.State.Round.CopySolution();
            for(int i=0;i<a.PartySize;i++) a.State.TrySit(i,answer[i]);
            if(!a.State.Evaluate()) throw new System.InvalidOperationException("Cannot prepare attendance.");
            for(int i=0;i<a.PartySize;i++) a.State.Stand(i);
            a.exitDoor.SetOpen(true,true);
            a.ExternalControl=true;
            flow.attendanceHud.enabled=false;
        }

        /// <summary>Không nhận tham số; ghi bàn giao hợp lệ, giữ kết quả và cửa hành lang đã mở.</summary>
        void PrepareCorridor()
        {
            var q=flow.corridor;
            q.State.Hold(0,0);
            q.State.Hold(1,1);
            if(!q.State.TryComplete(true)) throw new System.InvalidOperationException("Cannot prepare corridor.");
            q.gate.SetOpen(true,true);
            q.StopSession(false);
            flow.gradeEntrance.SetOpen(true,true);
        }

        /// <summary>Không nhận tham số; dùng chứng từ đúng để hoàn tất bảng điểm và mở cửa trước nhiệm vụ 4.</summary>
        void PrepareGrades()
        {
            var g=flow.gradeRepair;
            for(int i=0;i<4;i++) g.State.MarkRead(i,g.State.Round.OwnerOf(i));
            foreach(int voucher in new[]{2,0,1}) g.State.Apply(voucher,g.State.Round.CorrectSource(voucher),g.State.Round.CorrectTarget(voucher));
            if(g.State.Submit()!=GradeResult.Solved) throw new System.InvalidOperationException("Cannot prepare grades.");
            g.exitDoor.SetOpen(true,true);
            g.StopSession(false);
        }

        /// <summary>Không nhận tham số; đi đường hợp lệ qua hai dấu, giữ sa bàn đã giải và mở cửa nhiệm vụ 5.</summary>
        void PrepareExam()
        {
            var e=flow.navigationExam;
            foreach(int cell in new[]{1,4,5,8})
            {
                int next=e.State.Cell(cell);
                e.State.Move(e.State.Turn,next%3-e.State.Position%3,next/3-e.State.Position/3);
            }
            if(!e.State.Solved)throw new System.InvalidOperationException("Cannot prepare exam.");
            e.exitDoor.SetOpen(true,true);
            e.StopSession(false);
        }

        /// <summary>Không nhận tham số; ghi nhận mọi thẻ, nhập đúng mã và giữ cửa nhiệm vụ 5 mở trước khi vào phòng giáo viên.</summary>
        void PrepareBack()
        {
            var game=flow.dontLookBack;
            for(int subject=0;subject<game.State.PlayerCount;subject++)
            {
                int observer=(subject+1)%game.State.PlayerCount;
                game.State.Observe(observer,subject);
                game.State.Select(subject,game.State.SymbolOf(subject));
            }
            if(game.State.Submit(true)!=BackLockResult.Solved)
                throw new System.InvalidOperationException("Cannot prepare dont look back.");
            game.exitDoor.SetOpen(true,true);
            game.StopSession(false);
        }

        /// <summary>Nhận mốc nhiệm vụ; đặt đủ đội vào chỗ trống, chọn vai 1 và khóa chuột để chơi.</summary>
        void PlaceParty(Transform[] points)
        {
            var party=flow.attendance.party;
            for(int i=0;i<party.PartySize;i++) party.Teleport(i,points[i].position);
            party.SwitchRole(0,false);
            PrototypePartyController.SetCursor(true);
        }
    }
}
