/*
 * Mục đích: Vào thẳng nhiệm vụ trong Editor/Development Build, chuẩn bị tiến trình trước bằng luật thật.
 * Hàm: Start áp lựa chọn Inspector; Launch đặt lại phiên và mở nhiệm vụ;
 * PrepareAttendance / PrepareCorridor / PrepareGrades / PrepareExam / PrepareBack hoàn tất điều kiện trước;
 * PlaceParty đưa đội tới checkpoint; Available giới hạn công cụ test; EndTestSession xóa nhãn test.
 */
using UnityEngine;
namespace TheReturn
{
    public enum MissionTestEntry { NormalPlay = -1, Attendance = 0, QuietCorridor = 1, GradeRepair = 2, NavigationExam = 3, DontLookBack = 4, TeacherTruth = 5 }
    public sealed class MissionTestLauncher : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        [Tooltip("-1 chơi bình thường; 0 điểm danh; 1 giữ trật tự; 2 bảng điểm; 3 bài kiểm tra; 4 không quay đầu; 5 giáo viên.")]
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
            if (!Available() || !Application.isPlaying || mission<0 || mission>5 || count<2 || count>4) return false;
            flow.RestartMap();
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
