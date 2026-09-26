/*
 * Mục đích: Ghép phòng giáo viên nhiệm vụ 6 sau hành lang không quay đầu bằng module graybox tái sử dụng.
 * Hàm: Build cài phòng một lần; Block / Label tạo hình và chữ; TeacherPrefab / StationPrefab tạo prefab logic;
 * Door tạo cổng thoát tầng; AddEvidence đặt các chứng cứ có thể kiểm tra trong thế giới.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TheReturn.Editor
{
    public static class TeacherTruthSceneBuilder
    {
        const string Feature="Assets/TheReturn/Features/TeacherTruth/";
        const string Shared="Assets/TheReturn/Shared/";
        static Transform root;
        static GameObject block;
        static Material textMaterial;

        /// <summary>Không nhận tham số; thêm phòng, prefab, catalog và UI test một lần vào map đã lưu.</summary>
        [MenuItem("The Return/School Floor/Add Teacher Truth")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if(Application.isPlaying||scene.isDirty||flow==null||flow.dontLookBack==null||flow.teacherTruth!=null)
                throw new InvalidOperationException("Open saved school map in Edit mode; install Teacher Truth once.");
            var oldEnd=flow.dontLookBack.transform.Find("NextArea_End");
            if(oldEnd==null)throw new InvalidOperationException("Missing Dont Look Back connecting wall.");
            Directory.CreateDirectory(Feature+"Data");
            Directory.CreateDirectory(Feature+"Prefabs");
            AssetDatabase.Refresh();
            block=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_Block.prefab");
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/WorldText.mat");
            root=new GameObject("TeacherTruth_LogicOnly").transform;
            oldEnd.gameObject.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldEnd.gameObject);
            var oldTitle=flow.dontLookBack.transform.Find("NextArea_Title");
            if(oldTitle!=null)oldTitle.GetComponent<TextMesh>().text="PHONG GIAO VIEN / BON KENH";

            Block("Floor",new Vector3(5.8f,-.2f,116),new Vector3(14,.4f,20));
            Block("Wall_Left",new Vector3(-1.2f,1.8f,116),new Vector3(.2f,3.6f,20));
            Block("Wall_Right",new Vector3(12.8f,1.8f,116),new Vector3(.2f,3.6f,20));
            Block("Entrance_Left",new Vector3(1.7f,1.8f,106),new Vector3(5.8f,3.6f,.2f));
            Block("Entrance_Right",new Vector3(9.9f,1.8f,106),new Vector3(5.8f,3.6f,.2f));
            Block("Entrance_Header",new Vector3(5.8f,3.25f,106),new Vector3(2.4f,.7f,.2f));
            Block("Exit_Left",new Vector3(2.9f,1.8f,126),new Vector3(5.8f,3.6f,.2f));
            Block("Exit_Right",new Vector3(8.7f,1.8f,126),new Vector3(5.8f,3.6f,.2f));
            Block("Exit_Header",new Vector3(5.8f,3.25f,126),new Vector3(2.4f,.7f,.2f));

            var game=root.gameObject.AddComponent<TeacherTruthPrototype>();
            game.party=flow.attendance.party;
            string catalogPath=Feature+"Data/TeacherTruth_VI.asset";
            game.catalog=AssetDatabase.LoadAssetAtPath<TeacherTruthCatalog>(catalogPath);
            if(game.catalog==null)
            {
                game.catalog=ScriptableObject.CreateInstance<TeacherTruthCatalog>();
                AssetDatabase.CreateAsset(game.catalog,catalogPath);
            }
            game.checkpoints=new Transform[4];
            for(int i=0;i<4;i++)
            {
                var point=new GameObject("Checkpoint_Role_"+(i+1)).transform;
                point.SetParent(root);
                point.position=new Vector3(2.2f+i*2.4f,.05f,108);
                game.checkpoints[i]=point;
            }

            var teacherPrefab=TeacherPrefab();
            game.teachers=new TeacherChannelSource[4];
            float[] teacherX={1.6f,4.4f,7.2f,10f};
            for(int i=0;i<4;i++)
            {
                var teacher=(GameObject)PrefabUtility.InstantiatePrefab(teacherPrefab);
                teacher.name="TeacherChannel_"+(i+1);
                teacher.transform.SetParent(root);
                teacher.transform.position=new Vector3(teacherX[i],0,112);
                var source=teacher.GetComponent<TeacherChannelSource>();
                source.channel=i;
                teacher.transform.Find("ChannelLabel").GetComponent<TextMesh>().text="GIÁO VIÊN / KÊNH "+(i+1)+"\n[E] NGHE";
                game.teachers[i]=source;
            }

            var stationPrefab=StationPrefab();
            game.stations=new ManualConfirmStation[4];
            Vector3[] stationPositions={
                new Vector3(4.35f,1.25f,117.8f),new Vector3(7.25f,1.25f,117.8f),
                new Vector3(4.35f,1.25f,120.2f),new Vector3(7.25f,1.25f,120.2f)};
            for(int i=0;i<4;i++)
            {
                var station=(GameObject)PrefabUtility.InstantiatePrefab(stationPrefab);
                station.name="ManualConfirm_"+(i+1);
                station.transform.SetParent(root);
                station.transform.position=stationPositions[i];
                var component=station.GetComponent<ManualConfirmStation>();
                component.channel=i;
                component.SetVisual(-1);
                game.stations[i]=component;
            }

            game.syncWarning=Label("SyncWarning","MẤT ĐỒNG BỘ",new Vector3(5.8f,3.05f,114.8f),.045f);
            AddEvidence(game.catalog);
            game.exitDoor=Door(new Vector3(5.8f,0,126));
            flow.teacherTruth=game;
            SchoolFloorCanvasBuilder.UpgradeTeacherTruth();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeObject=game.catalog;
        }

        /// <summary>Nhận catalog; đặt bản ghi phòng trước, sơ đồ kênh và nội quy cửa bằng text chỉnh được trong asset.</summary>
        static void AddEvidence(TeacherTruthCatalog catalog)
        {
            Label("PriorRoomEvidence",catalog.priorEvidence,new Vector3(5.8f,2.1f,104.9f),.016f);
            Label("ChannelDiagram",catalog.roomEvidence,new Vector3(5.8f,2.35f,114.9f),.017f);
            Label("ExitRule",catalog.exitRule,new Vector3(5.8f,2.25f,125.75f),.017f);
        }

        /// <summary>Nhận tên/tâm/kích thước; trả instance khối dùng chung có collider.</summary>
        static GameObject Block(string name,Vector3 position,Vector3 scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(block);
            go.name=name;
            go.transform.SetParent(root);
            go.transform.position=position;
            go.transform.localScale=scale;
            return go;
        }

        /// <summary>Nhận tên/chữ/vị trí/cỡ; trả TextMesh dùng material chữ thế giới.</summary>
        static TextMesh Label(string name,string value,Vector3 position,float size)
        {
            var go=new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position=position;
            var label=go.AddComponent<TextMesh>();
            label.text=value;
            label.fontSize=64;
            label.characterSize=size;
            label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
            return label;
        }

        /// <summary>Không nhận tham số; tạo tượng giáo viên graybox tái sử dụng, phần Visual_Replaceable có thể thay model.</summary>
        static GameObject TeacherPrefab()
        {
            string path=Feature+"Prefabs/PF_TeacherChannel.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab!=null)return prefab;
            var previousRoot=root;
            var go=new GameObject("PF_TeacherChannel");
            root=go.transform;
            go.AddComponent<TeacherChannelSource>();
            Block("Pedestal",new Vector3(0,.25f,0),new Vector3(1.25f,.5f,1.25f));
            var visual=Block("Visual_Replaceable",new Vector3(0,1.15f,0),new Vector3(.7f,1.3f,.5f));
            visual.GetComponent<Renderer>().sharedMaterial=
                AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/M_GB_Interaction.mat");
            Block("Head",new Vector3(0,2.05f,0),new Vector3(.55f,.55f,.55f));
            Label("ChannelLabel","GIÁO VIÊN / KÊNH ?\n[E] NGHE",new Vector3(0,2.85f,0),.021f);
            prefab=PrefabUtility.SaveAsPrefabAsset(go,path);
            UnityEngine.Object.DestroyImmediate(go);
            root=previousRoot;
            return prefab;
        }

        /// <summary>Không nhận tham số; tạo nút xác nhận có collider, nhãn và component kênh dùng lại.</summary>
        static GameObject StationPrefab()
        {
            string path=Feature+"Prefabs/PF_ManualConfirmStation.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab!=null)return prefab;
            var previousRoot=root;
            var go=new GameObject("PF_ManualConfirmStation");
            root=go.transform;
            var station=go.AddComponent<ManualConfirmStation>();
            Block("Base",Vector3.zero,new Vector3(1.5f,1.2f,.35f));
            var button=Block("Visual_Replaceable",new Vector3(0,0,-.28f),new Vector3(.65f,.45f,.25f));
            button.GetComponent<Renderer>().sharedMaterial=
                AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/M_GB_Interaction.mat");
            station.label=Label("Label","XÁC NHẬN THỦ CÔNG ?\n[E]",new Vector3(0,.9f,-.2f),.019f);
            prefab=PrefabUtility.SaveAsPrefabAsset(go,path);
            UnityEngine.Object.DestroyImmediate(go);
            root=previousRoot;
            return prefab;
        }

        /// <summary>Nhận vị trí; tạo cửa trượt dùng chung và trả component.</summary>
        static PuzzleDoor Door(Vector3 position)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_SlidingDoor.prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name="SchoolFloorExit";
            go.transform.SetParent(root);
            go.transform.position=position;
            return go.GetComponent<PuzzleDoor>();
        }
    }
}
