/*
 * Mục đích: Thêm panel nhiệm vụ 5 vào prefab Canvas mà không dựng lại giao diện đã chỉnh.
 * Hàm: UpgradeDontLookBack cập nhật prefab; BuildBack tạo ô khóa/màn tối;
 * AddBackTest thêm nút thứ năm, giữ các callback được presenter nối khi chạy.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace TheReturn.Editor
{
    public static partial class SchoolFloorCanvasBuilder
    {
        /// <summary>Không nhận tham số; mở prefab hiện có, chỉ thêm phần còn thiếu rồi lưu, luôn giải phóng Prefab Mode tạm.</summary>
        public static void UpgradeDontLookBack()
        {
            string path=Root+"Prefabs/PF_SchoolFloorCanvas.prefab";
            var contents=PrefabUtility.LoadPrefabContents(path);
            try
            {
                view=contents.GetComponent<SchoolFloorCanvasView>();
                theme=AssetDatabase.LoadAssetAtPath<SchoolUITheme>(Shared+"Themes/SchoolUITheme.asset");
                CreateAtoms();
                if(view.backBoard==null)BuildBack(contents.transform.Find("SafeFrame_1280x720"));
                if(view.missionButtons.Length<5)AddBackTest();
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
        /// <summary>Nhận khung; tạo panel nhập theo thẻ, nút xác nhận và Image tối phủ màn hình lúc bị phạt.</summary>
        static void BuildBack(Transform frame)
        {
            view.backBoard=Panel(frame,"DontLookBackPanel",20,130,835,430);
            var p=view.backBoard.transform;
            Text(p,"Title","KHÓA KÝ HIỆU / THEO SỐ THẺ",24,18,787,40,24);
            Text(p,"Instructions","Trao đổi ký hiệu trên lưng. Cả đội cần đứng trong vùng cuối hành lang.",24,70,787,60,18);
            view.backSlots=new Button[4];
            for(int i=0;i<4;i++)view.backSlots[i]=Button(p,"Card_"+i,"Thẻ "+(i+1),24+i*198,145,185,80);
            view.backSummary=Text(p,"Summary","",24,245,787,65,17);
            view.backSubmit=Button(p,"Submit","Xác nhận mã",24,315,787,40);
            view.backClose=Button(p,"Close","Rời bảng [E]",24,373,787,38);
            view.backBoard.SetActive(false);
            var dark=new GameObject("CheckpointBlackout",typeof(RectTransform),typeof(Image));
            dark.transform.SetParent(frame.parent,false);
            var rect=dark.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            view.blackout=dark.GetComponent<Image>();view.blackout.color=Color.black;view.blackout.raycastTarget=true;
            dark.SetActive(false);
        }
        /// <summary>Không nhận tham số; thêm nút nhiệm vụ 5 và thu gọn khoảng cách trong menu hiện có.</summary>
        static void AddBackTest()
        {
            var list=new Button[5];
            for(int i=0;i<4;i++)list[i]=view.missionButtons[i];
            list[4]=Button(view.tests.transform,"Mission_4","5 • Không được quay đầu",24,319,752,36);
            for(int i=0;i<5;i++)Rect(list[i].GetComponent<RectTransform>(),view.tests.transform,24,151+i*41,752,36);
            view.missionButtons=list;
        }
    }
}
