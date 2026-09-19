/*
 * Mục đích: Nối Canvas nhiệm vụ 5 với controller; bố cục và ảnh vẫn nằm trong prefab.
 * Hàm: BindBack nối nút một lần; RefreshBack cập nhật mã đang nhập và màn tối khi về checkpoint.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed partial class SchoolFloorCanvasPresenter
    {
        /// <summary>Không nhận tham số; nối nút chọn ký hiệu, nộp và đóng của nhiệm vụ 5 nếu prefab đã nâng cấp.</summary>
        void BindBack()
        {
            if(view.backSlots==null)return;
            for(int i=0;i<view.backSlots.Length;i++)
            {
                int slot=i;
                Bind(view.backSlots[i],()=>{
                    var game=flow.dontLookBack;
                    game.Select(slot,(game.State.Entry(slot)+1)%DontLookBackState.SymbolCount);
                });
            }
            if(view.backSubmit!=null)Bind(view.backSubmit,()=>flow.dontLookBack.Submit());
            if(view.backClose!=null)Bind(view.backClose,()=>flow.dontLookBack.ClosePanel());
        }
        /// <summary>Không nhận tham số; hiện số ô theo đội, mã đã chọn và lớp tối, không hiển thị đáp án.</summary>
        void RefreshBack()
        {
            var game=flow.dontLookBack;
            bool active=Phase==4;
            SchoolFloorCanvasView.SetVisible(view.backBoard,active&&game.PanelOpen&&!testingMenu);
            if(view.blackout!=null)
            {
                view.blackout.gameObject.SetActive(active&&game.Blackout>0);
                view.blackout.color=new Color(0,0,0,active?Mathf.Clamp01(game.Blackout/.15f):0);
            }
            if(!active)return;
            for(int i=0;i<view.backSlots.Length;i++)
            {
                view.backSlots[i].gameObject.SetActive(i<game.State.PlayerCount);
                if(i<game.State.PlayerCount)
                {
                    Caption(view.backSlots[i],"Thẻ "+(i+1)+"\n"+game.catalog.Symbol(game.State.Entry(i))+"  ›");
                    view.backSlots[i].interactable=!game.State.Solved;
                }
            }
            view.backSubmit.interactable=!game.State.Solved;
            SchoolFloorCanvasView.SetText(view.backSummary,"Đã có người đọc: "+game.State.WitnessedCount+"/"+game.State.PlayerCount+
                " • Nhấn từng ô để đổi ký hiệu. Nhập theo SỐ THẺ.");
        }
    }
}
