/*
 * Mục đích: Nâng prefab Canvas hiện có với nút test nhiệm vụ 6, không thay các panel đã chỉnh.
 * Hàm: UpgradeTeacherTruth mở/lưu prefab an toàn; AddTeacherTest thêm và xếp sáu nút nhiệm vụ.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TheReturn.Editor
{
    public static partial class SchoolFloorCanvasBuilder
    {
        /// <summary>Không nhận tham số; thêm mục nhiệm vụ 6 nếu thiếu rồi lưu prefab Canvas.</summary>
        public static void UpgradeTeacherTruth()
        {
            string path=Root+"Prefabs/PF_SchoolFloorCanvas.prefab";
            var contents=PrefabUtility.LoadPrefabContents(path);
            try
            {
                view=contents.GetComponent<SchoolFloorCanvasView>();
                theme=AssetDatabase.LoadAssetAtPath<SchoolUITheme>(Shared+"Themes/SchoolUITheme.asset");
                CreateAtoms();
                if(view.missionButtons==null||view.missionButtons.Length<6)AddTeacherTest();
                PrefabUtility.SaveAsPrefabAsset(contents,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        /// <summary>Không nhận tham số; thêm nút giáo viên và thu gọn sáu mục để không đè nút quay lại.</summary>
        static void AddTeacherTest()
        {
            int oldCount=view.missionButtons==null?0:view.missionButtons.Length;
            var list=new Button[6];
            for(int i=0;i<Mathf.Min(oldCount,5);i++)list[i]=view.missionButtons[i];
            if(list[5]==null)list[5]=Button(view.tests.transform,"Mission_5","6 • Giáo viên thật / dối",24,328,752,33);
            for(int i=0;i<6;i++)
            {
                if(list[i]==null)continue;
                Rect(list[i].GetComponent<RectTransform>(),view.tests.transform,24,137+i*37,752,33);
            }
            Rect(view.closeTests.GetComponent<RectTransform>(),view.tests.transform,24,366,752,35);
            view.missionButtons=list;
        }
    }
}
