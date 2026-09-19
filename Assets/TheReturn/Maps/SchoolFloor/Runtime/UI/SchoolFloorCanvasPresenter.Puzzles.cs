/*
 * Mục đích: Presenter cập nhật hai panel tương tác; bố cục và hình ảnh nằm trong prefab Canvas.
 * Hàm: Caption đổi nhãn nút; RefreshGrade hiển thị phiếu/điểm/tài liệu;
 * RefreshExam cập nhật ô, dấu và quyền bấm hướng.
 */
using UnityEngine;
using UnityEngine.UI;
namespace TheReturn
{
    public sealed partial class SchoolFloorCanvasPresenter
    {
        /// <summary>Nhận nút và chữ; cập nhật Text con, không thay màu/ảnh thiết kế.</summary>
        static void Caption(Button button,string text)
        {
            SchoolFloorCanvasView.SetText(button.GetComponentInChildren<Text>(),text);
        }

        /// <summary>Không nhận tham số; đọc state bảng điểm và cập nhật panel được mở, chỉ bật lệnh hợp lệ.</summary>
        void RefreshGrade()
        {
            var g=flow.gradeRepair;
            if(g.OpenDocument>=0)
            {
                int doc=g.OpenDocument;
                SchoolFloorCanvasView.SetText(view.documentTitle,g.catalog.documentTitles!=null && doc<g.catalog.documentTitles.Length?g.catalog.documentTitles[doc]:"Tài liệu "+(doc+1));
                SchoolFloorCanvasView.SetText(view.documentBody,g.catalog.DocumentText(g.State.Round,doc));
                return;
            }
            int total=0,read=0;
            for(int i=0;i<4;i++)
            {
                total+=g.State.Score(i);
                if(g.State.Read(i))read++;
                SchoolFloorCanvasView.SetText(view.gradeRows[i],"Hồ sơ "+(char)('A'+i)+": "+g.State.Round.InitialScores[i]+" → "+g.State.Score(i));
                Caption(view.gradeTargets[i],(target==i?"● ":"")+"Nhận: "+(char)('A'+i));
                Caption(view.gradeSources[i],(source==i?"● ":"")+"Từ: "+(char)('A'+i));
                view.gradeTargets[i].interactable=!g.State.Solved;
                view.gradeSources[i].interactable=!g.State.Solved&&voucher==2;
            }
            for(int i=0;i<3;i++)
            {
                Caption(view.gradeVouchers[i],(voucher==i?"● ":"")+(i==2?"Chuyển 1":"Phiếu 0"+(i+1)+": +2")+(g.State.Used(i)?" (đã dùng)":""));
                view.gradeVouchers[i].interactable=!g.State.Solved;
            }
            SchoolFloorCanvasView.SetText(view.gradeSummary,"Tổng "+total+"/32 • Hồ sơ đã đọc "+read+"/4 • Hoàn tác "+g.State.UndoCount);
            view.gradeApply.interactable=!g.State.Solved;
            view.gradeUndo.interactable=!g.State.Solved&&g.State.UndoCount>0;
            view.gradeSubmit.interactable=!g.State.Solved;
        }

        /// <summary>Không nhận tham số; hiển thị quân/đích, giấu ô khóa và bật hướng chỉ cho người tới lượt.</summary>
        void RefreshExam()
        {
            var e=flow.navigationExam;
            for(int i=0;i<9;i++)SchoolFloorCanvasView.SetText(view.examCells[i],NavigationExamState.Code(i)+
                (i==e.State.Position?"\nQUÂN":i==e.State.Goal?"\nĐÍCH":i==e.State.Start?"\nBẮT ĐẦU":""));
            foreach(var button in view.directions)button.interactable=!e.State.Solved&&e.party.ActivePlayer==e.State.Turn;
            SchoolFloorCanvasView.SetText(view.examSummary,"Sổ lớp: "+(e.State.Stamps>=1?"đã lấy":"chưa lấy")+
                " • Chìa khóa: "+(e.State.Stamps>=2?"đã lấy":"chưa lấy"));
        }
    }
}
