/*
 * Mục đích: Dựng phòng giáo viên nối sau hành lang trong scene hiện có bằng các prefab logic.
 * Hàm: Build kiểm tra và nối phòng; Spawn tạo instance; Block đặt module;
 * Label tạo chữ; TerminalPrefab tạo điểm tương tác; PlaceTerminal gán tài liệu;
 * Checkpoints tạo mốc thử lại; Door tạo cửa dùng chung.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
    public static class GradeRepairSceneBuilder
    {
        const string Feature = "Assets/TheReturn/Features/GradeRepair/";
        const string Shared = "Assets/TheReturn/Shared/";
        static Transform root;
        static GameObject block;
        static Material textMaterial;

        /// <summary>Không nhận tham số; nối phòng sau hành lang đã có, giữ nguyên hai nhiệm vụ trước và lưu scene.</summary>
        [MenuItem("The Return/School Floor/Add Grade Repair Room")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.isDirty) throw new InvalidOperationException("Stop Play and save scene first.");
            var flow = UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if (flow == null || flow.gradeRepair != null) throw new InvalidOperationException("Open the map with corridor and no grade room.");
            var oldEnd = flow.corridor.transform.Find("Wall_End");
            if (oldEnd == null) throw new InvalidOperationException("Corridor end wall is missing.");
            Directory.CreateDirectory(Feature + "Data");
            Directory.CreateDirectory(Feature + "Prefabs");
            AssetDatabase.Refresh();
            root = new GameObject("GradeRepair_LogicOnly").transform;
            block = AssetDatabase.LoadAssetAtPath<GameObject>(Shared + "Prefabs/Graybox/PF_GB_Block.prefab");
            textMaterial = AssetDatabase.LoadAssetAtPath<Material>(Shared + "Art/Materials/Graybox/WorldText.mat");

            // Thu ngắn tường cuối hành lang thành cạnh cửa; phần lớp học giữ nguyên.
            oldEnd.name = "GradeEntrance_LeftEdge";
            oldEnd.position = new Vector3(4.225f, 1.8f, 38);
            oldEnd.localScale = new Vector3(1.35f, 3.6f, .2f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldEnd);
            Block("Entrance_RightEdge", new Vector3(7.375f, 1.8f, 38), new Vector3(1.35f, 3.6f, .2f));
            Block("Floor", new Vector3(5.8f, -.2f, 47), new Vector3(14, .4f, 18));
            Block("Wall_Left", new Vector3(-1.2f, 1.8f, 47), new Vector3(.2f, 3.6f, 18));
            Block("Wall_Right", new Vector3(12.8f, 1.8f, 47), new Vector3(.2f, 3.6f, 18));
            Block("Front_Left", new Vector3(1.175f, 1.8f, 38), new Vector3(4.75f, 3.6f, .2f));
            Block("Front_Right", new Vector3(10.425f, 1.8f, 38), new Vector3(4.75f, 3.6f, .2f));
            Block("Entrance_Header", new Vector3(5.8f, 3.25f, 38), new Vector3(1.8f, .7f, .2f));
            Block("Back_Left", new Vector3(1.85f, 1.8f, 56), new Vector3(6.1f, 3.6f, .2f));
            Block("Back_Right", new Vector3(9.75f, 1.8f, 56), new Vector3(6.1f, 3.6f, .2f));
            Block("Exit_Header", new Vector3(5.8f, 3.25f, 56), new Vector3(1.8f, .7f, .2f));
            Block("NextArea_Floor", new Vector3(5.8f, -.2f, 58.5f), new Vector3(4.4f, .4f, 5));
            Block("NextArea_Left", new Vector3(3.6f, 1.8f, 58.5f), new Vector3(.2f, 3.6f, 5));
            Block("NextArea_Right", new Vector3(8, 1.8f, 58.5f), new Vector3(.2f, 3.6f, 5));
            Block("NextArea_End", new Vector3(5.8f, 1.8f, 61), new Vector3(4.4f, 3.6f, .2f));
            Label("Room_Title", "PHONG GIAO VIEN / SUA BANG DIEM", new Vector3(5.8f, 3.1f, 37.8f), .027f);
            Label("NextArea_Title", "HET PHAN THU HIEN TAI", new Vector3(5.8f, 2, 60.8f), .038f);

            var game = root.gameObject.AddComponent<GradeRepairPrototype>();
            game.party = flow.attendance.party;
            string path = Feature + "Data/GradeRepair_VI.asset";
            game.catalog = AssetDatabase.LoadAssetAtPath<GradeRepairCatalog>(path);
            if (game.catalog == null)
            {
                game.catalog = ScriptableObject.CreateInstance<GradeRepairCatalog>();
                AssetDatabase.CreateAsset(game.catalog, path);
            }
            var terminal = TerminalPrefab();
            game.documents = new GradeRepairTerminal[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 point = new Vector3(i % 2 == 0 ? 1.5f : 10.1f, 0, i < 2 ? 43 : 49);
                Block("EvidenceTable_" + (i + 1), point + new Vector3(0, .8f, 0), new Vector3(2.1f, .15f, 1.2f));
                Block("TableSupport_" + (i + 1), point + new Vector3(0, .37f, 0), new Vector3(1.2f, .74f, .5f));
                game.documents[i] = PlaceTerminal(terminal, "Document_" + (i + 1), point + Vector3.up * 1.35f, i);
            }
            game.board = PlaceTerminal(terminal, "GradeBoard", new Vector3(5.8f, 1.7f, 53.5f), -1);
            game.board.transform.localScale = new Vector3(2.2f, 1.5f, 1);
            game.board.SetLabel("BANG DIEM / E");
            game.checkpoints = Checkpoints();
            game.exitDoor = Door("GradeRoom_Exit", new Vector3(5.8f, 0, 56));
            flow.gradeEntrance = Door("GradeRoom_Entrance", new Vector3(5.8f, 0, 38));
            flow.gradeRepair = game;
            flow.gameObject.AddComponent<GradeRepairHud>().flow = flow;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeObject = game.catalog;
        }

        /// <summary>Nhận prefab, tên và pose; trả instance có liên kết để đổi model hàng loạt.</summary>
        static GameObject Spawn(GameObject prefab, string name, Vector3 position, Vector3 scale)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(root);
            go.transform.position = position;
            go.transform.localScale = scale;
            return go;
        }

        /// <summary>Nhận tên, tâm và kích thước; trả khối sàn/tường/bàn có collider.</summary>
        static GameObject Block(string name, Vector3 position, Vector3 scale) => Spawn(block, name, position, scale);

        /// <summary>Nhận chữ và pose; trả nhãn dùng vật liệu chung có kiểm tra chiều sâu.</summary>
        static TextMesh Label(string name, string text, Vector3 position, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sharedMaterial = textMaterial;
            return label;
        }

        /// <summary>Không nhận tham số; lấy hoặc tạo prefab tương tác, giữ các chỉnh sửa prefab đã có.</summary>
        static GameObject TerminalPrefab()
        {
            string path = Feature + "Prefabs/PF_GradeTerminal.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;
            var go = new GameObject("PF_GradeTerminal");
            var terminal = go.AddComponent<GradeRepairTerminal>();
            var visual = Block("Visual_Replaceable", Vector3.zero, new Vector3(1.5f, .85f, .15f));
            visual.transform.SetParent(go.transform);
            visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Shared + "Art/Materials/Graybox/M_GB_Board.mat");
            terminal.label = Label("Label", "HO SO / E", new Vector3(0, 0, -.09f), .026f);
            terminal.label.transform.SetParent(go.transform);
            prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        /// <summary>Nhận prefab, pose và ID tài liệu; trả terminal đã gán dữ liệu, ghi override prefab.</summary>
        static GradeRepairTerminal PlaceTerminal(GameObject prefab, string name, Vector3 position, int document)
        {
            var result = Spawn(prefab, name, position, Vector3.one).GetComponent<GradeRepairTerminal>();
            result.document = document;
            result.SetLabel(document < 0 ? "BANG DIEM / E" : "HO SO " + (document + 1) + " / E");
            PrefabUtility.RecordPrefabInstancePropertyModifications(result);
            PrefabUtility.RecordPrefabInstancePropertyModifications(result.label);
            return result;
        }

        /// <summary>Không nhận tham số; tạo bốn vị trí không chồng nhau trong phòng để thử lại.</summary>
        static Transform[] Checkpoints()
        {
            var points = new Transform[4];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new GameObject("Checkpoint_" + (i + 1)).transform;
                points[i].SetParent(root);
                points[i].position = new Vector3(i % 2 == 0 ? 4.6f : 7, .05f, 40 + i / 2 * 1.5f);
            }
            return points;
        }

        /// <summary>Nhận tên và vị trí; trả cửa trượt prefab dùng chung.</summary>
        static PuzzleDoor Door(string name, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Shared + "Prefabs/Graybox/PF_GB_SlidingDoor.prefab");
            return Spawn(prefab, name, position, Vector3.one).GetComponent<PuzzleDoor>();
        }
    }
}
