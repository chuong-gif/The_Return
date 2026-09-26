/*
 * Mục đích: Đồng bộ Canvas với sáu nhiệm vụ, giữ bố cục/ảnh/font trong prefab thay vì vẽ OnGUI.
 * Hàm: Awake gắn nút; Bind đăng ký callback; LateUpdate đồng bộ panel và HUD;
 * Resume / Retry / NextCase gọi controller hiện tại; OpenTests / Launch chọn bài test;
 * RefreshHud cập nhật dữ liệu chung; RefreshGrade / RefreshExam cập nhật giao diện riêng (file partial).
 */
using UnityEngine;
using UnityEngine.UI;
namespace TheReturn
{
    public sealed partial class SchoolFloorCanvasPresenter : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        public MissionTestLauncher launcher;
        public SchoolFloorCanvasView view;
        int count=4, voucher, source, target;
        bool testingMenu;
        int Phase => flow.teacherTruth!=null && flow.teacherTruth.Active ? 5 :
            flow.dontLookBack!=null && flow.dontLookBack.Active ? 4 : flow.navigationExam!=null && flow.navigationExam.Active ? 3 :
            flow.gradeRepair!=null && flow.gradeRepair.Active ? 2 : flow.corridor.Active ? 1 : 0;

        /// <summary>Nhận nút và callback; đăng ký một lần khi prefab được tạo, không sửa persistent listener của designer.</summary>
        static void Bind(Button button,UnityEngine.Events.UnityAction action) { button.onClick.AddListener(action); }

        /// <summary>Không nhận tham số; nối thao tác UI tới controller, giữ luật ngoài View.</summary>
        void Awake()
        {
            BindBack();
            Bind(view.resume,Resume);
            Bind(view.retry,Retry);
            Bind(view.nextCase,NextCase);
            Bind(view.restartMap,()=>{testingMenu=false;launcher.EndTestSession();flow.RestartMap();});
            Bind(view.openTests,OpenTests);
            Bind(view.closeTests,()=>{testingMenu=false;});
            for(int i=0;i<view.teamButtons.Length;i++)
            {
                int size=i%3+2;
                Bind(view.teamButtons[i],()=>{count=size;if(!testingMenu && Phase==0)flow.attendance.SetParticipantCount(size);});
            }
            for(int i=0;i<view.missionButtons.Length;i++) { int mission=i; Bind(view.missionButtons[i],()=>Launch(mission)); }
            for(int i=0;i<3;i++) { int selected=i;Bind(view.gradeVouchers[i],()=>voucher=selected); }
            for(int i=0;i<4;i++)
            {
                int selected=i;
                Bind(view.gradeTargets[i],()=>target=selected);
                Bind(view.gradeSources[i],()=>source=selected);
            }
            Bind(view.gradeApply,()=>flow.gradeRepair.Apply(voucher,voucher==2?source:-1,target));
            Bind(view.gradeUndo,()=>flow.gradeRepair.Undo());
            Bind(view.gradeSubmit,()=>flow.gradeRepair.Submit());
            Bind(view.gradeClose,()=>flow.gradeRepair.ClosePanel());
            Bind(view.documentClose,()=>flow.gradeRepair.ClosePanel());
            Bind(view.examClose,()=>flow.navigationExam.ClosePanel());
            int[] dx={0,1,0,-1},dy={-1,0,1,0};
            for(int i=0;i<4;i++) { int direction=i;Bind(view.directions[i],()=>{
                var e=flow.navigationExam;e.Step(dx[direction],dy[direction],e.State.Steps);}); }
        }

        /// <summary>Không nhận tham số; mở lựa chọn nhiệm vụ test và thả chuột; chỉ có trong bản phát triển.</summary>
        public void OpenTests()
        {
            if(!MissionTestLauncher.Available()) return;
            if(Phase==2 && flow.gradeRepair.PanelOpen)flow.gradeRepair.ClosePanel();
            if(Phase==3 && flow.navigationExam.PanelOpen)flow.navigationExam.ClosePanel();
            if(Phase==4 && flow.dontLookBack.PanelOpen)flow.dontLookBack.ClosePanel();
            count=Mathf.Clamp(flow.attendance.PartySize,2,4);
            testingMenu=true;
            PrototypePartyController.SetCursor(false);
        }

        /// <summary>Nhận nhiệm vụ; bắt đầu qua launcher, đóng menu khi thành công.</summary>
        void Launch(int mission)
        {
            if(launcher.Launch(mission,count))testingMenu=false;
        }

        /// <summary>Không nhận tham số; tiếp tục đúng controller đang sở hữu nhân vật.</summary>
        void Resume()
        {
            testingMenu=false;
            if(Phase==0) flow.attendance.BeginSession();
            else PrototypePartyController.SetCursor(true);
        }

        /// <summary>Không nhận tham số; reset riêng nhiệm vụ hiện tại, giữ tiến trình trước.</summary>
        void Retry()
        {
            switch(Phase)
            {
                case 0: flow.attendance.RetryRound();flow.attendance.BeginSession();break;
                case 1: flow.corridor.Retry();PrototypePartyController.SetCursor(true);break;
                case 2: flow.gradeRepair.Retry();break;
                case 3: flow.navigationExam.ResetRound();break;
                case 4: flow.dontLookBack.Retry();break;
                case 5: flow.teacherTruth.Retry();break;
            }
        }

        /// <summary>Không nhận tham số; tạo đề/biến thể mới cho nhiệm vụ đang hỗ trợ.</summary>
        void NextCase()
        {
            if(Phase==0){flow.attendance.StartNewRound();flow.attendance.BeginSession();}
            if(Phase==2)flow.gradeRepair.NewCase();
            if(Phase==3)flow.navigationExam.ResetRound(true);
            if(Phase==4)flow.dontLookBack.Retry(true);
            if(Phase==5)flow.teacherTruth.Retry(true);
        }

        /// <summary>Không nhận tham số; cập nhật panel sau gameplay, không tạo GameObject hoặc asset trong mỗi frame.</summary>
        void LateUpdate()
        {
            if(flow.attendance.State==null)return;
            var keyboard=UnityEngine.InputSystem.Keyboard.current;
            if(keyboard!=null && keyboard.f8Key.wasPressedThisFrame)OpenTests();
            bool grade=Phase==2 && flow.gradeRepair.PanelOpen;
            bool exam=Phase==3 && flow.navigationExam.PanelOpen;
            bool back=Phase==4 && flow.dontLookBack.PanelOpen;
            bool paused=!grade&&!exam&&!back&&Cursor.lockState!=CursorLockMode.Locked;
            if(testingMenu)PrototypePartyController.SetCursor(false);
            SchoolFloorCanvasView.SetVisible(view.pause,paused&&!testingMenu);
            SchoolFloorCanvasView.SetVisible(view.tests,testingMenu);
            SchoolFloorCanvasView.SetVisible(view.gradeBoard,grade&&flow.gradeRepair.OpenDocument<0&&!testingMenu);
            SchoolFloorCanvasView.SetVisible(view.document,grade&&flow.gradeRepair.OpenDocument>=0&&!testingMenu);
            SchoolFloorCanvasView.SetVisible(view.examBoard,exam&&!testingMenu);
            SchoolFloorCanvasView.SetVisible(view.cluePanel,(Phase==0||Phase==3||Phase==4||Phase==5)&&!paused&&!testingMenu);
            SchoolFloorCanvasView.SetVisible(view.noisePanel,Phase==1&&!paused&&!testingMenu);
            view.crosshair.enabled=!paused&&!testingMenu&&!grade&&!exam&&!back;
            view.nextCase.gameObject.SetActive(Phase!=1);
            view.openTests.gameObject.SetActive(MissionTestLauncher.Available());
            if(!testingMenu && Phase==0)count=flow.attendance.PartySize;
            for(int i=0;i<view.teamButtons.Length;i++)
            {
                view.teamButtons[i].gameObject.SetActive(testingMenu||Phase==0);
                Caption(view.teamButtons[i],(i%3+2==count?"● ":"")+(i%3+2)+" người");
            }
            SchoolFloorCanvasView.SetText(view.testLabel,"ĐỘI "+count+" NGƯỜI • Chọn nhiệm vụ để tạo phiên test mới");
            RefreshHud();
            RefreshBack();
            if(grade)RefreshGrade();
            if(exam)RefreshExam();
        }

        /// <summary>Không nhận tham số; chỉ cập nhật text/slider từ controller và chỉ dẫn tương tác hiện tại.</summary>
        void RefreshHud()
        {
            var a=flow.attendance;
            string[] titles={a.textCatalog.title,flow.corridor.settings.title,flow.gradeRepair.catalog.title,flow.navigationExam.catalog.title,
                flow.dontLookBack!=null?flow.dontLookBack.catalog.title:"",flow.teacherTruth!=null?flow.teacherTruth.catalog.title:""};
            SchoolFloorCanvasView.SetText(view.title,titles[Phase]);
            SchoolFloorCanvasView.SetText(view.role,(launcher.TestSession?"TEST • ":"")+"VAI "+(a.party.ActivePlayer+1)+" / ĐỘI "+a.PartySize);
            string info="",message="",hint="",help="";
            if(Phase==0)
            {
                info=a.State.Solved?"Đã điểm danh • Tập hợp ngoài lớp: "+flow.Gathered+"/"+a.PartySize :
                    "Đã ngồi "+a.State.OccupiedCount+"/"+a.PartySize+" • 24 ghế";
                message=a.State.Solved?"Đứng dậy và đưa cả đội ra vùng TẬP HỢP ngoài cửa lớp.":a.Status;
                hint=a.State.SeatOf(a.ActivePlayer)>=0?"[E] Đứng dậy":a.TargetSeat>=0?"[E] Ngồi "+AttendanceRound.SeatCode(a.TargetSeat):"Nhìn vào bàn/ghế gần để tương tác";
                help="WASD đi • E ngồi/đứng • H gợi ý • 1–4 đổi vai • Tab menu";
                SchoolFloorCanvasView.SetText(view.clues,a.CluePackets[a.ActivePlayer]);
            }
            if(Phase==1)
            {
                var q=flow.corridor;
                info="Tiếng ồn "+Mathf.CeilToInt(q.State.Noise)+"/100 • "+(q.State.SpeakerOn?"Loa đang rè":q.State.Held?"Loa đã tắt":"Ân hạn "+q.State.Grace.ToString("0.0")+"s");
                message=q.Status;
                hint=q.IsHolding(a.ActivePlayer)?"[E] Nhả nút":q.TargetStation>=0?"[E] Giữ nút "+(q.TargetStation==0?"A":"B"):"A: "+(q.State.Owner(0)+1)+" • B: "+(q.State.Owner(1)+1)+" • 0 = chưa giữ";
                help=q.settings.localControls;
                view.noise.value=q.State.Noise/100f;
            }
            if(Phase==2)
            {
                var g=flow.gradeRepair;
                info="Tổng cần đạt 32 • Mỗi hồ sơ 6–10";
                message=g.Status;
                hint=g.State.Solved?g.catalog.success:g.Target==null?"Tới bàn chứng từ hoặc bảng sửa":
                    g.Target.document<0?"[E] Mở bảng sửa":"[E] Hồ sơ của vai "+(g.State.Round.OwnerOf(g.Target.document)+1);
                help=g.catalog.controls;
            }
            if(Phase==3)
            {
                var e=flow.navigationExam;
                info=NavigationExamState.Code(e.State.Start)+" → "+NavigationExamState.Code(e.State.Goal)+" • Dấu "+e.State.Stamps+"/2 • Lượt vai "+(e.State.Turn+1);
                message=e.Status;
                hint=e.State.Solved?e.catalog.success:e.Targeted?"[E] Mở sa bàn":"Tới gần sa bàn để tương tác";
                help=e.catalog.controls;
                SchoolFloorCanvasView.SetText(view.clues,e.catalog.Packet(e.State,a.ActivePlayer));
            }
            if(Phase==4)
            {
                var b=flow.dontLookBack;
                info="Ký hiệu đã được đọc "+b.State.WitnessedCount+"/"+a.PartySize+" • Checkpoint "+(b.Checkpoint+1)+" • Lần quay đầu "+b.State.Penalties;
                message=b.Warning?b.catalog.warning:b.Status;
                hint=b.State.Solved?b.catalog.success:b.ReadTarget>=0?"[E] Đọc thẻ "+(b.ReadTarget+1):
                    b.LockTarget?"[E] Mở bảng khóa":"Hướng an toàn +Z • Lùi để nhìn lưng đồng đội";
                help=b.catalog.controls;
                SchoolFloorCanvasView.SetText(view.clues,b.catalog.Notebook(b.State,a.ActivePlayer));
            }
            if(Phase==5)
            {
                var teacher=flow.teacherTruth;
                info=teacher.ResultText();
                message=teacher.Status;
                hint=teacher.State.Solved?teacher.catalog.success:
                    teacher.TargetTeacher!=null?"[E] Nghe kênh "+(teacher.TargetTeacher.channel+1):
                    teacher.TargetStation!=null?"[E] Xác nhận thủ công "+(teacher.TargetStation.channel+1):
                    "Tìm bốn giáo viên, đèn MẤT ĐỒNG BỘ và bốn nút xác nhận";
                help=teacher.catalog.controls;
                SchoolFloorCanvasView.SetText(view.clues,teacher.catalog.Notebook(teacher.State,a.ActivePlayer));
            }
            SchoolFloorCanvasView.SetText(view.objective,info);
            SchoolFloorCanvasView.SetText(view.status,message);
            SchoolFloorCanvasView.SetText(view.prompt,hint);
            SchoolFloorCanvasView.SetText(view.controls,help+(MissionTestLauncher.Available()?" • F8 chọn bài test":""));
        }
    }
}
