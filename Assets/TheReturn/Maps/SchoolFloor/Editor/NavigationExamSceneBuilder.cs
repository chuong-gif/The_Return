/*
 * Mục đích: Nối phòng kiểm tra với lối ra phòng giáo viên, tái sử dụng module graybox.
 * Hàm: Build dựng phòng và gán tham chiếu; Block tạo module; Label tạo chữ;
 * BoardPrefab tạo/lấy sa bàn dùng lại; Door đặt cửa trượt.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
    public static class NavigationExamSceneBuilder
    {
        const string Feature = "Assets/TheReturn/Features/NavigationExam/";
        const string Shared = "Assets/TheReturn/Shared/";
        static Transform root;
        static GameObject block;
        static Material textMaterial;

        /// <summary>Không nhận tham số; thêm phòng một lần vào map đã lưu, giữ nguyên ba nhiệm vụ trước.</summary>
        [MenuItem("The Return/School Floor/Add Navigation Exam")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            var flow = UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if (Application.isPlaying || scene.isDirty || flow == null || flow.gradeRepair == null || flow.navigationExam != null)
                throw new InvalidOperationException("Open saved map with grade room, stop Play, and add only once.");
            var oldEnd = flow.gradeRepair.transform.Find("NextArea_End");
            if (oldEnd == null) throw new InvalidOperationException("Missing connecting end wall.");
            Directory.CreateDirectory(Feature + "Data");
            Directory.CreateDirectory(Feature + "Prefabs");
            AssetDatabase.Refresh();
            block = AssetDatabase.LoadAssetAtPath<GameObject>(Shared + "Prefabs/Graybox/PF_GB_Block.prefab");
            textMaterial = AssetDatabase.LoadAssetAtPath<Material>(Shared + "Art/Materials/Graybox/WorldText.mat");
            root = new GameObject("NavigationExam_LogicOnly").transform;
            oldEnd.name = "ExamEntrance_Left";
            oldEnd.position = new Vector3(4.225f,1.8f,61);
            oldEnd.localScale = new Vector3(1.35f,3.6f,.2f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldEnd);
            var oldLabel = flow.gradeRepair.transform.Find("NextArea_Title");
            if (oldLabel != null) oldLabel.GetComponent<TextMesh>().text = "PHONG KIEM TRA / DI TIEP";
            Block("Entrance_Right",new Vector3(7.375f,1.8f,61),new Vector3(1.35f,3.6f,.2f));
            Block("Floor",new Vector3(5.8f,-.2f,69),new Vector3(12,.4f,16));
            Block("Wall_Left",new Vector3(-.2f,1.8f,69),new Vector3(.2f,3.6f,16));
            Block("Wall_Right",new Vector3(11.8f,1.8f,69),new Vector3(.2f,3.6f,16));
            Block("Front_Left",new Vector3(1.675f,1.8f,61),new Vector3(3.75f,3.6f,.2f));
            Block("Front_Right",new Vector3(9.925f,1.8f,61),new Vector3(3.75f,3.6f,.2f));
            Block("Entrance_Header",new Vector3(5.8f,3.25f,61),new Vector3(1.8f,.7f,.2f));
            Block("Back_Left",new Vector3(2.35f,1.8f,77),new Vector3(5.1f,3.6f,.2f));
            Block("Back_Right",new Vector3(9.25f,1.8f,77),new Vector3(5.1f,3.6f,.2f));
            Block("Exit_Header",new Vector3(5.8f,3.25f,77),new Vector3(1.8f,.7f,.2f));
            Block("NextArea_Floor",new Vector3(5.8f,-.2f,79.5f),new Vector3(4.4f,.4f,5));
            Block("NextArea_Left",new Vector3(3.6f,1.8f,79.5f),new Vector3(.2f,3.6f,5));
            Block("NextArea_Right",new Vector3(8,1.8f,79.5f),new Vector3(.2f,3.6f,5));
            Block("NextArea_End",new Vector3(5.8f,1.8f,82),new Vector3(4.4f,3.6f,.2f));
            Label("NextArea_Title","HET PHAN THU HIEN TAI",new Vector3(5.8f,2,81.8f),.035f);
            var game = root.gameObject.AddComponent<NavigationExamPrototype>();
            game.party = flow.attendance.party;
            string path = Feature + "Data/NavigationExam_VI.asset";
            game.catalog = AssetDatabase.LoadAssetAtPath<NavigationExamCatalog>(path);
            if (game.catalog == null)
            {
                game.catalog = ScriptableObject.CreateInstance<NavigationExamCatalog>();
                AssetDatabase.CreateAsset(game.catalog,path);
            }
            var board = (GameObject)PrefabUtility.InstantiatePrefab(BoardPrefab());
            board.transform.SetParent(root);
            board.transform.position = new Vector3(5.8f,1,70);
            game.board = board.transform;
            game.pawn = board.transform.Find("Pawn");
            game.cells = new Transform[9];
            for (int i=0;i<9;i++) game.cells[i] = board.transform.Find("Cell_" + i);
            game.boardLabel = board.transform.Find("Instruction").GetComponent<TextMesh>();
            game.checkpoints = new Transform[4];
            for (int i=0;i<4;i++)
            {
                var point = new GameObject("Checkpoint_" + (i+1)).transform;
                point.SetParent(root);
                point.position = new Vector3(4.5f+i%2*2.6f,.05f,63+i/2*1.5f);
                game.checkpoints[i] = point;
            }
            game.exitDoor = Door(new Vector3(5.8f,0,77));
            flow.navigationExam = game;
            flow.gameObject.AddComponent<NavigationExamHud>().flow = flow;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeObject = game.catalog;
        }

        /// <summary>Nhận tên/tâm/kích thước; trả instance block có collider và liên kết prefab chung.</summary>
        static GameObject Block(string name,Vector3 position,Vector3 scale)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(block);
            go.name=name;
            go.transform.SetParent(root);
            go.transform.position=position;
            go.transform.localScale=scale;
            return go;
        }

        /// <summary>Nhận chữ và pose; trả nhãn world-space có kiểm tra chiều sâu.</summary>
        static TextMesh Label(string name,string text,Vector3 position,float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position=position;
            var label=go.AddComponent<TextMesh>();
            label.text=text;
            label.fontSize=64;
            label.characterSize=size;
            label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
            return label;
        }

        /// <summary>Không nhận tham số; tạo sa bàn 9 ô, quân và nhãn một lần, giữ prefab đã chỉnh sửa.</summary>
        static GameObject BoardPrefab()
        {
            string path=Feature+"Prefabs/PF_NavigationBoard.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab!=null) return prefab;
            var go=new GameObject("PF_NavigationBoard");
            var top=Block("Table",new Vector3(0,-.16f,0),new Vector3(3.7f,.18f,3.7f));
            top.transform.SetParent(go.transform);
            var support=Block("Support",new Vector3(0,-.58f,0),new Vector3(2,.64f,2));
            support.transform.SetParent(go.transform);
            for(int i=0;i<9;i++)
            {
                Vector3 point=new Vector3((i%3-1)*1.2f,0,(1-i/3)*1.2f);
                var cell=Block("Cell_"+i,point,new Vector3(1.1f,.08f,1.1f));
                cell.transform.SetParent(go.transform);
                var label=Label("Address_"+i,NavigationExamState.Code(i),point+Vector3.up*.055f,.035f);
                label.transform.rotation=Quaternion.Euler(90,0,0);
                label.transform.SetParent(go.transform);
            }
            var pawn=Block("Pawn",new Vector3(-1.2f,.17f,1.2f),new Vector3(.3f,.26f,.3f));
            pawn.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/M_GB_Interaction.mat");
            pawn.transform.SetParent(go.transform);
            var instruction=Label("Instruction","SA BAN / E",new Vector3(0,.5f,1.9f),.035f);
            instruction.transform.SetParent(go.transform);
            prefab=PrefabUtility.SaveAsPrefabAsset(go,path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        /// <summary>Nhận vị trí; tạo cửa trượt dùng chung và trả component.</summary>
        static PuzzleDoor Door(Vector3 position)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_SlidingDoor.prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name="ExamExit";
            go.transform.SetParent(root);
            go.transform.position=position;
            return go.GetComponent<PuzzleDoor>();
        }
    }
}
