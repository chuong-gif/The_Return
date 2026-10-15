/*
 * Mục đích: tìm nhanh đồ nhiệm vụ trong scene và mở đúng prefab để thay hình ảnh.
 * Open: mở cửa sổ; OnGUI: lọc, chọn instance hoặc mở prefab nguồn.
 * PrepareVisualRoots: tách hình ảnh khỏi collider cho terminal và cửa mẫu, giữ nguyên GUID.
 * SplitVisual: tạo VisualRoot và chuyển mesh/material sang đó, không thay component logic.
 */
using UnityEngine;
using UnityEditor;

namespace TheReturn.Editor
{
    public sealed class SchoolFloorObjectsWindow : EditorWindow
    {
        string filter="";
        Vector2 scroll;

        /// <summary>Không có đầu vào; mở bảng quản lý đồ tương tác của scene hiện tại.</summary>
        [MenuItem("The Return/School Floor/Objects - Find and Edit")]
        public static void Open() { GetWindow<SchoolFloorObjectsWindow>("School Floor Objects"); }

        /// <summary>Đọc bộ lọc; hiển thị đồ có logic, cho phép chọn instance và mở prefab, không sửa runtime.</summary>
        void OnGUI()
        {
            EditorGUILayout.HelpBox("Chọn vật để tìm trong scene. Mở prefab để thay model trong VisualRoot. Giữ nguyên Interaction, Label và component nhiệm vụ.",MessageType.Info);
            filter=EditorGUILayout.TextField("Tìm theo tên / nhiệm vụ",filter);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            foreach(var component in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if(!component || !component.gameObject.scene.IsValid() || !component.gameObject.activeInHierarchy) continue;
                string type=component.GetType().Name;
                if(!(component is PuzzleDoor) && !type.Contains("Terminal") && !type.Contains("Station") && !type.Contains("Seat") && !type.Contains("Channel"))continue;
                string title=component.name+" / "+type;
                if(!string.IsNullOrEmpty(filter) && title.IndexOf(filter,System.StringComparison.OrdinalIgnoreCase)<0)continue;
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(title)) {Selection.activeGameObject=component.gameObject;EditorGUIUtility.PingObject(component.gameObject);}
                string path=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(component.gameObject);
                using(new EditorGUI.DisabledScope(string.IsNullOrEmpty(path)))
                    if(GUILayout.Button("Prefab",GUILayout.Width(60))) AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>Không có đầu vào; chuẩn hóa hai prefab mẫu tại chỗ, giữ GUID và các tham chiếu scene.</summary>
        public static void PrepareVisualRoots()
        {
            string[] paths={"Assets/TheReturn/Features/GradeRepair/Prefabs/PF_GradeTerminal.prefab","Assets/TheReturn/Shared/Prefabs/Graybox/PF_GB_SlidingDoor.prefab"};
            foreach(string path in paths)
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var target=root.transform.Find(path.Contains("GradeTerminal")?"Visual_Replaceable":"Panel");
                    if(!target || target.Find("VisualRoot"))continue;
                    SplitVisual(target);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally {PrefabUtility.UnloadPrefabContents(root);}
            }
        }

        /// <summary>Nhận phần hiển thị cũ; tách model dưới VisualRoot, giữ collider và panel chuyển động bên ngoài.</summary>
        static void SplitVisual(Transform target)
        {
            var filter=target.GetComponent<MeshFilter>();var renderer=target.GetComponent<MeshRenderer>();
            if(!filter || !renderer)throw new System.InvalidOperationException("Expected mesh and renderer on "+target.name);
            var visual=new GameObject("VisualRoot",typeof(MeshFilter),typeof(MeshRenderer));
            visual.transform.SetParent(target,false);
            visual.GetComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
            visual.GetComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials;
            renderer.enabled=false;
            // Do not remove the parent or its collider: PuzzleDoor.panel and interaction raycasts depend on it.
        }
    }
}
