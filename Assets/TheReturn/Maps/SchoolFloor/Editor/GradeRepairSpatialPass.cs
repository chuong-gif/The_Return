/*
 * Mục đích: bố trí Sửa bảng điểm trực tiếp trong Phong_2 và chuyển Giữ trật tự ra Hanh_Lang_2.
 * Build: kiểm tra, lưu bản sao hiện trạng rồi bố trí hai khu vực; không dựng lại blockout.
 * Strip: lưu ẩn phần vỏ cũ; Pose: đặt hệ tọa độ nhiệm vụ; Place: đặt vật theo phòng.
 * Zone: tạo cụm chức năng; Prop: đặt tài sản ThirdParty với vật liệu đã chuyển URP.
 * Label: biển chỉ dẫn; Terminal: bố trí điểm tương tác; Validate: kiểm tra liên kết và checkpoint.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn.Editor
{
    public static class GradeRepairSpatialPass
    {
        static Transform room;
        const string Art = "Assets/TheReturn/Art/Interiors/";

        /// <summary>Không có đầu vào; lưu snapshot rồi bố trí hai khu vực. Chỉ chạy một lần trên scene chính.</summary>
        [MenuItem("The Return/School Floor/Spatial Pass 01 - Archives and Quiet Corridor")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            var flow = UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            var architecture = GameObject.Find("BlockoutCampus_Generated/Architecture_From_User_Blockout");
            if (Application.isPlaying || scene.name != "SchoolFloorPrototype" || !flow || !architecture)
                throw new InvalidOperationException("Open SchoolFloorPrototype in Edit Mode with the user blockout.");
            if (GameObject.Find("Archives_SpatialPass01")) throw new InvalidOperationException("Pass already applied; edit the existing zones instead.");
            // Snapshot includes unsaved user edits; it does not replace an older backup.
            string backup = "Assets/TheReturn/Maps/SchoolFloor/Scenes/BeforeSpatialPass_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
            if (!EditorSceneManager.SaveScene(scene, backup, true)) throw new InvalidOperationException("Snapshot failed.");
            Undo.RegisterFullObjectHierarchyUndo(flow.gradeRepair.gameObject, "Spatial pass archives");
            Undo.RegisterFullObjectHierarchyUndo(flow.corridor.gameObject, "Spatial pass corridor");

            var quiet = flow.corridor;
            Strip(quiet.transform, new[]{"Floor","Wall_","Connector_","GradeEntrance_","SchoolRoom_Environment","Interior_Decoration"});
            var hall = architecture.transform.Find("Hanh_Lang_2");
            Pose(quiet.transform, new Vector3(5.8f,0,23.7f), new Vector3(hall.position.x,10.85f,hall.position.z), 210);
            // Keep the original 20 m station spacing so the timing rule does not silently change.
            flow.transitionGatherZones[0].transform.SetPositionAndRotation(quiet.transform.TransformPoint(new Vector3(5.8f,1.2f,10.5f)), quiet.transform.rotation);
            flow.stagingZone = flow.transitionGatherZones[0];
            flow.stagingZone.size = new Vector3(4.2f,3.8f,4);

            var grade = flow.gradeRepair;
            Strip(grade.transform, new[]{"Floor","Wall_","Front_","Back_","Entrance_","Exit_Header","NextArea_","ExamEntrance_","EvidenceTable_","TableSupport_","Room_Title","SchoolRoom_Environment","Interior_Decoration"});
            var shell = architecture.transform.Find("Phong_2");
            var center = new Vector3(shell.position.x,11.14f,shell.position.z);
            Pose(grade.transform,new Vector3(5.8f,0,47),center,180);
            room = new GameObject("Archives_SpatialPass01").transform;
            room.SetPositionAndRotation(center, Quaternion.Euler(0,180,0));

            string[] names={"01_TiepNhan","02_DoiChieuMa","03_BaiThiBoSung","04_BienBan","05_NopBangDiem"};
            Vector3[] points={new Vector3(-8,0,-12),new Vector3(-12,0,0),new Vector3(10,0,-3),new Vector3(9,0,10),new Vector3(-4,0,9)};
            string[] titles={"TIEP NHAN / HUONG DAN","02 / SO DOI CHIEU MA","03 / BAI THI BO SUNG","04 / BIEN BAN NHAP NHAM","05 / DOI CHIEU VA NOP"};
            for(int i=0;i<5;i++) Zone(names[i],points[i],titles[i]);
            Terminal(grade.documents[0],points[1]+new Vector3(0,1.3f,0));
            Terminal(grade.documents[1],points[2]+new Vector3(0,1.3f,0));
            Terminal(grade.documents[2],points[2]+new Vector3(2.8f,1.3f,0));
            Terminal(grade.documents[3],points[3]+new Vector3(0,1.3f,0));
            Terminal(grade.board,points[4]+new Vector3(0,1.6f,0));
            Label(room,"Tra so ma tai khu 02\nLay hai phieu tai khu 03\nDoi chieu bien ban tai khu 04\nCung nop bang tai khu 05",points[0]+new Vector3(0,1.5f,-.15f),.035f);
            for(int i=0;i<4;i++) Place(grade.checkpoints[i],new Vector3((i%2)*2-1,0.06f,-18+i/2*1.7f));
            Place(flow.gradeEntrance.transform,new Vector3(0,0,-23.8f));
            Place(grade.exitDoor.transform,new Vector3(0,0,23.8f));
            var gather=flow.transitionGatherZones[1];
            gather.transform.SetPositionAndRotation(room.TransformPoint(new Vector3(0,1.2f,-26)),room.rotation);
            gather.size=new Vector3(5,3.8f,4);
            // GradeRepair now occupies room 2; later tasks keep their current location in this first pass.
            Physics.SyncTransforms();
            EditorUtility.SetDirty(flow);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject=room.gameObject;
            Debug.Log(Validate());
        }

        /// <summary>Nhận root và tiền tố vỏ cũ; chuyển chúng vào nhóm ẩn để phục hồi, không xóa logic.</summary>
        static void Strip(Transform root,string[] prefixes)
        {
            var retired=new GameObject("RetiredShell_Disabled").transform;
            retired.SetParent(root,false);
            var list=new List<Transform>();
            foreach(Transform child in root)
                foreach(string prefix in prefixes) if(child.name.StartsWith(prefix,StringComparison.Ordinal)){list.Add(child);break;}
            foreach(var child in list) child.SetParent(retired,true);
            retired.gameObject.SetActive(false);
        }

        /// <summary>Nhận tâm local, tâm world và góc; đặt root không thay đổi tỷ lệ vật bên trong.</summary>
        static void Pose(Transform root,Vector3 localCenter,Vector3 worldCenter,float yaw)
        { root.rotation=Quaternion.Euler(0,yaw,0); root.position=worldCenter-root.rotation*localCenter; }

        /// <summary>Nhận transform và vị trí theo phòng; cập nhật pose và lưu override prefab nếu có.</summary>
        static void Place(Transform target,Vector3 position)
        {
            target.SetPositionAndRotation(room.TransformPoint(position),room.rotation);
            if(PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        /// <summary>Nhận terminal và vị trí; giữ ID, collider, label và liên kết runtime.</summary>
        static void Terminal(GradeRepairTerminal terminal,Vector3 position)
        { Place(terminal.transform,position); }

        /// <summary>Nhận tên cụm, tâm và biển; dựng bàn, tủ, ghế thật, chừa lối trung tâm và giữa các cụm.</summary>
        static void Zone(string name,Vector3 center,string title)
        {
            var zone=new GameObject(name).transform;zone.SetParent(room,false);zone.localPosition=center;
            Prop(zone,"Tables/Table01.prefab","BanTraCuu",Vector3.zero,.8f,0);
            Prop(zone,"Tables/Table01.prefab","BanPhu",new Vector3(2.8f,0,0),.8f,0);
            for(int i=0;i<3;i++) Prop(zone,"Closets/Closet12.prefab","TuHoSo_"+(i+1),new Vector3((i-1)*2.5f,0,3),2.1f,180);
            Prop(zone,"Chairs/Chair01.prefab","GheNhanVien",new Vector3(0,0,1.1f),.95f,180);
            Label(zone,title,new Vector3(0,2.65f,2.6f),.05f);
            var light=new GameObject("DenKhuTraCuu",typeof(Light)).GetComponent<Light>();
            light.transform.SetParent(zone,false);light.transform.localPosition=new Vector3(0,4,0);
            light.type=LightType.Point;light.range=11;light.intensity=2;light.color=new Color(1,.87f,.68f);
        }

        /// <summary>Nhận prefab, pose và chiều cao; tạo model đúng tỷ lệ cùng collider, dùng bản vật liệu URP đã có.</summary>
        static void Prop(Transform parent,string path,string name,Vector3 position,float height,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Furniture Mega Pack/Prefabs/"+path);
            if(!prefab) throw new InvalidOperationException(path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name=name;go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.identity;
            var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            go.transform.localScale*=height/bounds.size.y;
            bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var wrapper=new GameObject(name+"_Placement").transform;wrapper.SetParent(parent,false);
            wrapper.localPosition=position;wrapper.localRotation=Quaternion.Euler(0,yaw,0);
            go.transform.SetParent(wrapper,false);go.transform.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var c in go.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(c);
            var collider=wrapper.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,bounds.size.y/2,0);collider.size=bounds.size;
            foreach(var r in renderers)
            {
                var materials=r.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var converted=AssetDatabase.LoadAssetAtPath<Material>(Art+"MissionRooms/"+materials[i].name+".mat");
                    if(converted) materials[i]=converted;
                }
                r.sharedMaterials=materials;
            }
        }

        /// <summary>Nhận nội dung và pose local; trả biển có kiểm tra chiều sâu, không xuyên tường.</summary>
        static void Label(Transform parent,string text,Vector3 position,float size)
        {
            var label=new GameObject("ChiDan",typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.SetParent(parent,false);label.transform.localPosition=position;
            label.text=text;label.characterSize=size;label.fontSize=64;label.anchor=TextAnchor.MiddleCenter;
            label.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"Classroom/WorldFont.mat");
        }

        /// <summary>Không có đầu vào; hoàn thiện phòng mẫu và rào cổng hành lang; chạy một lần sau Build.</summary>
        public static void FinishSample()
        {
            room=GameObject.Find("Archives_SpatialPass01").transform;
            if(room.Find("ReadingCommons")) throw new InvalidOperationException("Sample already finished.");
            var commons=new GameObject("ReadingCommons").transform;commons.SetParent(room,false);
            for(int row=0;row<3;row++) foreach(float x in new[]{-3.5f,3.5f})
            {
                var p=new Vector3(x,0,-7+row*3.3f);
                Prop(commons,"Tables/Table01.prefab","BanDoiChieu",p,.78f,0);
                Prop(commons,"Chairs/Chair01.prefab","GheDoc",p+Vector3.forward,.95f,180);
                Prop(commons,"Chairs/Chair01.prefab","GheDoc",p-Vector3.forward,.95f,0);
            }
            var grade=UnityEngine.Object.FindFirstObjectByType<GradeRepairPrototype>();
            Place(grade.documents[1].transform,new Vector3(10,1.3f,-3));
            Place(grade.documents[2].transform,new Vector3(12.8f,1.3f,-3));
            var arch=GameObject.Find("BlockoutCampus_Generated/Architecture_From_User_Blockout").transform;
            arch.Find("San_Phong_2").GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"Classroom/Terrazzo.mat");
            arch.Find("Phong_2").GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"Classroom/IvoryPlaster.mat");
            // Small pieces are scene objects; no architectural prefab dependency.
            var quiet=UnityEngine.Object.FindFirstObjectByType<QuietCorridorPrototype>();
            var barrier=new GameObject("AcousticGate_SideGuards").transform;barrier.SetParent(quiet.transform,false);
            foreach(float side in new[]{-1f,1f})
            {
                Box(barrier,"GateSidePanel",new Vector3(5.8f+side*4.4f,1.5f,29),new Vector3(3.7f,3,.25f));
                Box(barrier,"SafetyRail",new Vector3(5.8f+side*6.1f,.65f,23.7f),new Vector3(.16f,1.3f,56));
            }
            quiet.goalZone.size=new Vector3(11.8f,3.5f,5);
            // Mark the five zones on the floor without creating new room shells.
            foreach(Transform zone in room)
            {
                if(zone.name=="ReadingCommons") continue;
                Box(zone,"ZoneBorder",new Vector3(1.3f,.015f,-1.6f),new Vector3(6,.02f,.09f),false);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>Nhận parent, pose, kích thước và cờ va chạm; tạo chi tiết cảnh thường, không tạo prefab tường.</summary>
        static void Box(Transform parent,string name,Vector3 p,Vector3 size,bool solid=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=p;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"Classroom/DarkTimber.mat");
            if(!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        /// <summary>Không có đầu vào; trả số liên kết và số checkpoint thiếu sàn hoặc bị chặn.</summary>
        public static string Validate()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<GradeRepairPrototype>();int blocked=0,missing=0;
            foreach(var p in game.checkpoints)
            {
                if(!Physics.Raycast(p.position+Vector3.up,Vector3.down,2,~0,QueryTriggerInteraction.Ignore))missing++;
                if(Physics.CheckCapsule(p.position+Vector3.up*.4f,p.position+Vector3.up*1.5f,.3f,~0,QueryTriggerInteraction.Ignore))blocked++;
            }
            return "SpatialPass01: documents="+game.documents.Length+", missing floor="+missing+", blocked checkpoints="+blocked;
        }
    }
}
