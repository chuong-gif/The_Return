/*
 * Mục đích: Bố cục mặc định cho các panel Canvas; chỉ chạy khi cài UI lần đầu.
 * Hàm: BuildHud dựng HUD; BuildMenus dựng pause/test; BuildGrade dựng bảng/tài liệu;
 * BuildExam dựng lưới/hướng. Sau khi tạo, sửa bố cục bằng Prefab Mode.
 */
using UnityEngine;
using UnityEngine.UI;
namespace TheReturn.Editor
{
    public static partial class SchoolFloorCanvasBuilder
    {
        /// <summary>Nhận khung gốc; tạo thông tin nhân vật, mục tiêu, tâm ngắm, manh mối cuộn và thanh ồn.</summary>
        static void BuildHud(Transform frame)
        {
            view.hud=new GameObject("HUD",typeof(RectTransform));
            Rect(view.hud.GetComponent<RectTransform>(),frame,0,0,1280,720);
            var top=Panel(view.hud.transform,"MissionHeader",20,20,1240,90).transform;
            view.title=Text(top,"MissionTitle","THE RETURN",20,12,820,34,26);
            view.objective=Text(top,"Objective","Mục tiêu",20,51,820,26,16);
            view.role=Text(top,"PartyRole","VAI 1 / ĐỘI 4",900,20,315,38,24);
            var footer=Panel(view.hud.transform,"FeedbackFooter",20,580,1240,120).transform;
            view.status=Text(footer,"Feedback","",18,10,1204,65,18);
            view.controls=Text(footer,"Controls","",18,82,1204,29,14);
            var interaction=Panel(view.hud.transform,"InteractionPrompt",260,505,760,52).transform;
            view.prompt=Text(interaction,"Prompt","",18,12,724,35,18);
            var cross=new GameObject("Crosshair",typeof(RectTransform),typeof(Image));
            Rect(cross.GetComponent<RectTransform>(),view.hud.transform,637,357,6,6);
            view.crosshair=cross.GetComponent<Image>();
            view.crosshair.raycastTarget=false;
            view.crosshair.color=theme.accentColor;
            view.cluePanel=Panel(frame,"PrivateClues",875,132,385,427);
            Text(view.cluePanel.transform,"Heading","MANH MỐI CỦA BẠN",20,14,345,36,22);
            view.clues=Scroll(view.cluePanel.transform,"ClueScroll",20,64,345,340);
            view.noisePanel=Panel(frame,"NoiseMeter",20,130,525,70);
            Text(view.noisePanel.transform,"Label","MỨC CẢNH BÁO",18,8,490,23,16);
            var slider=new GameObject("NoiseSlider",typeof(RectTransform),typeof(Image),typeof(Slider));
            Rect(slider.GetComponent<RectTransform>(),view.noisePanel.transform,18,40,490,14);
            slider.GetComponent<Image>().color=new Color(.12f,.2f,.24f);
            var fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));
            Rect(fill.GetComponent<RectTransform>(),slider.transform,0,0,490,14);
            fill.GetComponent<Image>().color=theme.accentColor;
            view.noise=slider.GetComponent<Slider>();
            view.noise.fillRect=fill.GetComponent<RectTransform>();
            view.noise.interactable=false;
            view.noise.transition=Selectable.Transition.None;
            view.noise.minValue=0;view.noise.maxValue=1;
        }

        /// <summary>Nhận khung gốc; dựng menu thường và menu test có chọn đội/nhiệm vụ riêng.</summary>
        static void BuildMenus(Transform frame)
        {
            view.pause=Panel(frame,"PauseMenu",240,130,800,435);
            var p=view.pause.transform;
            Text(p,"Title","TẠM DỪNG / THIẾT LẬP ĐỘI",24,18,752,36,26);
            view.resume=Button(p,"Resume","Bắt đầu / Tiếp tục",24,75,752);
            view.retry=Button(p,"Retry","Thử lại nhiệm vụ hiện tại",24,135,366);
            view.nextCase=Button(p,"NewCase","Đề / biến thể mới",410,135,366);
            view.restartMap=Button(p,"RestartMap","Chơi lại cả map từ điểm danh",24,195,752);
            view.openTests=Button(p,"OpenTests","TEST NHANH — chọn nhiệm vụ [F8]",24,255,752);
            view.teamButtons=new Button[6];
            for(int i=0;i<3;i++)view.teamButtons[i]=Button(p,"Team_"+(i+2),(i+2)+" người",24+i*255,330,242);
            Text(p,"Note","Thử một máy • 1–4 đổi vai • Chỉnh prefab Canvas để thay bố cục/ảnh/font",24,386,752,29,14);
            view.tests=Panel(frame,"MissionTestMenu",240,130,800,435);
            p=view.tests.transform;
            Text(p,"Title","VÀO THẲNG NHIỆM VỤ",24,15,752,36,26);
            view.testLabel=Text(p,"TeamStatus","",24,57,752,28,16);
            for(int i=0;i<3;i++)view.teamButtons[i+3]=Button(p,"TestTeam_"+(i+2),(i+2)+" người",24+i*255,97,242,40);
            string[] names={"1 • Điểm danh","2 • Giữ trật tự","3 • Sửa bảng điểm","4 • Bài kiểm tra"};
            view.missionButtons=new Button[4];
            for(int i=0;i<4;i++)view.missionButtons[i]=Button(p,"Mission_"+i,names[i],24,155+i*48,752,40);
            view.closeTests=Button(p,"Back","Quay lại menu",24,365,752,40);
        }

        /// <summary>Nhận khung gốc; dựng bảng sửa và tài liệu cuộn, mọi nút có thể thay sprite trong prefab.</summary>
        static void BuildGrade(Transform frame)
        {
            view.gradeBoard=Panel(frame,"GradeRepairPanel",20,130,1240,430);
            var p=view.gradeBoard.transform;
            Text(p,"Title","BẢNG SỬA ĐIỂM",24,12,1192,35,26);
            view.gradeRows=new Text[4];
            for(int i=0;i<4;i++)view.gradeRows[i]=Text(p,"Row_"+i,"",24+i*298,61,285,32,20);
            view.gradeVouchers=new Button[3];
            for(int i=0;i<3;i++)view.gradeVouchers[i]=Button(p,"Voucher_"+i,"",24+i*400,108,384);
            view.gradeTargets=new Button[4];view.gradeSources=new Button[4];
            for(int i=0;i<4;i++)
            {
                view.gradeTargets[i]=Button(p,"Target_"+i,"",24+i*298,168,282,40);
                view.gradeSources[i]=Button(p,"Source_"+i,"",24+i*298,220,282,40);
            }
            view.gradeApply=Button(p,"Apply","Áp dụng chứng từ",24,282,384);
            view.gradeUndo=Button(p,"Undo","Hoàn tác",424,282,384);
            view.gradeSubmit=Button(p,"Submit","Nộp bảng",824,282,384);
            view.gradeSummary=Text(p,"Summary","",24,338,1192,30,17);
            view.gradeClose=Button(p,"Close","Rời bảng [E]",24,378,1192,36);
            view.document=Panel(frame,"DocumentReader",220,135,840,425);
            p=view.document.transform;
            view.documentTitle=Text(p,"Title","",24,16,792,42,26);
            view.documentBody=Scroll(p,"DocumentScroll",24,77,792,272);
            view.documentClose=Button(p,"Close","Đóng tài liệu [E]",24,366,792,40);
        }

        /// <summary>Nhận khung gốc; dựng lưới sa bàn không lộ ô khóa và bốn nút hướng.</summary>
        static void BuildExam(Transform frame)
        {
            view.examBoard=Panel(frame,"NavigationExamPanel",20,130,835,430);
            var p=view.examBoard.transform;
            Text(p,"Title","SA BÀN DẪN ĐƯỜNG",24,12,787,36,26);
            view.examCells=new Text[9];
            for(int i=0;i<9;i++)
            {
                var cell=Panel(p,"Cell_"+i,24+i%3*130,70+i/3*86,118,76);
                view.examCells[i]=Text(cell.transform,"Address","",8,7,102,62,22);
                view.examCells[i].alignment=TextAnchor.MiddleCenter;
            }
            Text(p,"Hint","Hướng theo hàng và cột trên sa bàn, không theo camera.",430,70,380,62,16);
            view.directions=new Button[4];
            view.directions[0]=Button(p,"Up","Lên",550,143,130,46);
            view.directions[1]=Button(p,"Right","Phải",681,205,130,46);
            view.directions[2]=Button(p,"Down","Xuống",550,268,130,46);
            view.directions[3]=Button(p,"Left","Trái",420,205,130,46);
            view.examSummary=Text(p,"Stamps","",24,337,787,30,18);
            view.examClose=Button(p,"Close","Rời sa bàn [E]",24,380,787,34);
        }
    }
}
