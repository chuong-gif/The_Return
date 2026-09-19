/*
 * Mục đích: Ghép hành lang nhiệm vụ 5 vào lối ra phòng kiểm tra, dùng module graybox/prefab có sẵn.
 * Hàm: Build cài một lần; Block tạo khối; Label tạo chữ; Zone tạo vùng logic;
 * BadgePrefab lưu thẻ tái sử dụng; TerminalPrefab lưu bảng khóa dùng lại; GroundGuides đánh dấu vùng tập hợp.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
    public static class DontLookBackSceneBuilder
    {
        const string Feature="Assets/TheReturn/Features/DontLookBack/";
        const string Shared="Assets/TheReturn/Shared/";
        static Transform root;
        static GameObject block;
        static Material textMaterial;

        /// <summary>Không nhận tham số; thêm hành lang, catalog, thẻ và UI vào scene đã lưu; từ chối cài trùng.</summary>
        [MenuItem("The Return/School Floor/Add Dont Look Back")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if(Application.isPlaying||scene.isDirty||flow==null||flow.navigationExam==null||flow.dontLookBack!=null)
                throw new InvalidOperationException("Open saved school map in Edit mode; install once.");
            var oldEnd=flow.navigationExam.transform.Find("NextArea_End");
            if(oldEnd==null)throw new InvalidOperationException("Missing exam connecting wall.");
            Directory.CreateDirectory(Feature+"Data");Directory.CreateDirectory(Feature+"Prefabs");AssetDatabase.Refresh();
            block=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_Block.prefab");
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/WorldText.mat");
            root=new GameObject("DontLookBack_LogicOnly").transform;
            oldEnd.gameObject.SetActive(false);PrefabUtility.RecordPrefabInstancePropertyModifications(oldEnd.gameObject);
            flow.navigationExam.transform.Find("NextArea_Title").GetComponent<TextMesh>().text="KHONG DUOC QUAY DAU / DI TIEP";
            Block("Floor",new Vector3(5.8f,-.2f,92),new Vector3(8,.4f,20));
            Block("Wall_Left",new Vector3(1.8f,1.8f,92),new Vector3(.2f,3.6f,20));
            Block("Wall_Right",new Vector3(9.8f,1.8f,92),new Vector3(.2f,3.6f,20));
            Block("Entrance_Left",new Vector3(2.7f,1.8f,82),new Vector3(1.8f,3.6f,.2f));
            Block("Entrance_Right",new Vector3(8.9f,1.8f,82),new Vector3(1.8f,3.6f,.2f));
            // Vách thấp vẫn chặn di chuyển, để đường nhìn giữa hai làn không bị che.
            Block("ObservationDivider",new Vector3(5.8f,.35f,91.5f),new Vector3(.16f,.7f,17));
            Block("Exit_Left",new Vector3(3.35f,1.8f,102),new Vector3(3.1f,3.6f,.2f));
            Block("Exit_Right",new Vector3(8.25f,1.8f,102),new Vector3(3.1f,3.6f,.2f));
            Block("Exit_Header",new Vector3(5.8f,3.25f,102),new Vector3(1.8f,.7f,.2f));
            Block("NextArea_Floor",new Vector3(5.8f,-.2f,104),new Vector3(4.4f,.4f,4));
            Block("NextArea_Left",new Vector3(3.6f,1.8f,104),new Vector3(.2f,3.6f,4));
            Block("NextArea_Right",new Vector3(8,1.8f,104),new Vector3(.2f,3.6f,4));
            Block("NextArea_End",new Vector3(5.8f,1.8f,106),new Vector3(4.4f,3.6f,.2f));
            Label("NextArea_Title","HET PHAN THU / PHONG GIAO VIEN",new Vector3(5.8f,2,105.8f),.027f);
            var game=root.gameObject.AddComponent<DontLookBackPrototype>();game.party=flow.attendance.party;
            string catalogPath=Feature+"Data/DontLookBack_VI.asset";
            game.catalog=AssetDatabase.LoadAssetAtPath<DontLookBackCatalog>(catalogPath);
            if(game.catalog==null){game.catalog=ScriptableObject.CreateInstance<DontLookBackCatalog>();AssetDatabase.CreateAsset(game.catalog,catalogPath);}
            game.entryZone=Zone("EntryGather",new Vector3(5.8f,1,86),new Vector3(7.5f,3,7));
            game.checkpointZone=Zone("CheckpointTwo",new Vector3(5.8f,1,97),new Vector3(7.5f,3,10));
            game.exitZone=Zone("ExitGather",new Vector3(5.8f,1,99.5f),new Vector3(7.5f,3,4.5f));
            game.checkpoints=new Transform[4];game.laterCheckpoints=new Transform[4];
            game.badges=new Transform[4];game.backLabels=new TextMesh[4];
            var badgePrefab=BadgePrefab();
            for(int i=0;i<4;i++)
            {
                for(int stage=0;stage<2;stage++)
                {
                    var point=new GameObject("Checkpoint_"+stage+"_Role_"+(i+1)).transform;point.SetParent(root);
                    point.position=new Vector3(i%2==0?3.8f:7.8f,.05f,(stage==0?85:93)+i/2*2);
                    if(stage==0)game.checkpoints[i]=point;else game.laterCheckpoints[i]=point;
                }
                var badge=(GameObject)PrefabUtility.InstantiatePrefab(badgePrefab);badge.name="BackCard_"+(i+1);
                badge.transform.SetParent(game.party.players[i],false);
                game.badges[i]=badge.transform;game.backLabels[i]=badge.transform.Find("BackSymbol").GetComponent<TextMesh>();
                badge.transform.Find("FrontNumber").GetComponent<TextMesh>().text="THE "+(i+1);
                badge.SetActive(false);
            }
            game.terminals=new Transform[2];
            var terminalPrefab=TerminalPrefab();
            for(int i=0;i<2;i++)
            {
                var board=(GameObject)PrefabUtility.InstantiatePrefab(terminalPrefab);board.name="LockTerminal_"+i;board.transform.SetParent(root);
                board.transform.position=new Vector3(i==0?3.8f:7.8f,1.5f,100);game.terminals[i]=board.transform;
                for(int z=84;z<=96;z+=4)
                {
                    var arrow=Label("Forward_"+i+"_"+z,"^\n+Z",new Vector3(i==0?3.8f:7.8f,.015f,z),.07f);
                    arrow.transform.rotation=Quaternion.Euler(90,0,0);
                }
                Label("Rules_"+i,"KHONG QUAY DAU\nLUI DE DOC LUNG BAN\nE: DOC THE / KHOA",new Vector3(i==0?3.8f:7.8f,2.7f,89),.025f);
            }
            Label("CheckpointSign","CHECKPOINT 2",new Vector3(5.8f,2.9f,93),.027f);
            var door=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_SlidingDoor.prefab"));
            door.name="DontLookBackExit";door.transform.SetParent(root);door.transform.position=new Vector3(5.8f,0,102);
            game.exitDoor=door.GetComponent<PuzzleDoor>();
            flow.dontLookBack=game;
            GroundGuides();
            SchoolFloorCanvasBuilder.UpgradeDontLookBack();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeObject=game.catalog;
        }
        /// <summary>Không nhận tham số; thêm vạch sàn và nhãn vùng tập hợp một lần, không thêm vật cản.</summary>
        public static void GroundGuides()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<DontLookBackPrototype>();
            if(game==null||game.transform.Find("GatherGuides")!=null)return;
            root=new GameObject("GatherGuides").transform;root.SetParent(game.transform);
            block=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_Block.prefab");
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/WorldText.mat");
            var material=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/M_GB_Interaction.mat");
            for(int lane=0;lane<2;lane++)
            {
                float x=lane==0?3.8f:7.8f;
                foreach(float z in new[]{82.6f,89.4f,92f,97.3f,101.6f})
                {
                    var stripe=Block("GatherLine",new Vector3(x,.012f,z),new Vector3(3.5f,.02f,.06f));
                    stripe.GetComponent<Renderer>().sharedMaterial=material;
                    stripe.GetComponent<Collider>().enabled=false;
                }
                var entry=Label("EntryHint","TAP HOP\nBAT DAU",new Vector3(x,.03f,83.5f),.03f);
                entry.transform.rotation=Quaternion.Euler(90,0,0);
                var finish=Label("ExitHint","TAP HOP\nNHAP KHOA",new Vector3(x,.03f,98),.03f);
                finish.transform.rotation=Quaternion.Euler(90,0,0);
            }
            root=game.transform;
        }
        /// <summary>Nhận tên/tâm/kích thước; trả module graybox chung có collider.</summary>
        static GameObject Block(string name,Vector3 position,Vector3 scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(block);go.name=name;go.transform.SetParent(root);
            go.transform.position=position;go.transform.localScale=scale;return go;
        }
        /// <summary>Nhận chữ/vị trí/cỡ; trả TextMesh có material kiểm tra chiều sâu.</summary>
        static TextMesh Label(string name,string value,Vector3 position,float size)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=position;
            var label=go.AddComponent<TextMesh>();label.text=value;label.fontSize=64;label.characterSize=size;
            label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;return label;
        }
        /// <summary>Nhận tên và vùng; tạo collider trigger làm dữ liệu kiểm tra tập hợp.</summary>
        static BoxCollider Zone(string name,Vector3 center,Vector3 size)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=center;
            var collider=go.AddComponent<BoxCollider>();collider.isTrigger=true;collider.size=size;return collider;
        }
        /// <summary>Không nhận tham số; tạo prefab thẻ trước/sau một lần, không chứa đáp án cố định.</summary>
        static GameObject BadgePrefab()
        {
            string path=Feature+"Prefabs/PF_BackCard.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab!=null)return prefab;
            var go=new GameObject("PF_BackCard");
            var back=Label("BackSymbol","THE ?\n?",new Vector3(0,1.35f,-.38f),.025f);
            back.transform.SetParent(go.transform);
            var front=Label("FrontNumber","THE ?",new Vector3(0,1.35f,.38f),.02f);
            front.transform.rotation=Quaternion.Euler(0,180,0);front.transform.SetParent(go.transform);
            prefab=PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);return prefab;
        }
        /// <summary>Không nhận tham số; tạo bảng khóa dùng lại, phần chữ tách khỏi khối tương tác.</summary>
        static GameObject TerminalPrefab()
        {
            string path=Feature+"Prefabs/PF_BackLockTerminal.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab!=null)return prefab;
            var go=new GameObject("PF_BackLockTerminal");
            var panel=Block("Visual_Replaceable",Vector3.zero,new Vector3(1.5f,.8f,.16f));panel.transform.SetParent(go.transform);
            var label=Label("Label","KHOA / E\nTHE 1 -> THE N",new Vector3(0,0,-.09f),.023f);label.transform.SetParent(go.transform);
            prefab=PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);return prefab;
        }
    }
}
