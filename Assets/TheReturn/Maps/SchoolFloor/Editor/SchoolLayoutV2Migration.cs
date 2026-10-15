/*
 * Mục đích: hoàn tất ánh xạ 4 phòng/2 hành lang theo blockout, tạo cửa chuẩn bị có logic.
 * Build: lưu snapshot, chuyển nhiệm vụ, bỏ vỏ cũ, tạo 22 cửa/vùng chuẩn bị và lưu scene.
 * MoveRoom/RetireShell: đổi vị trí root và giữ vỏ cũ trong nhóm ẩn để khôi phục.
 * PrepareAttendance: bỏ vỏ nhỏ, phân bố 24 bàn và giữ liên kết ghế/đồ trang trí.
 * GatePrefab/CreateGate: tạo prefab logic và instance đúng kích thước cửa blockout.
 * LocalPoint/SetPose: đổi hệ tọa độ; StripDecor: chỉ bỏ kiến trúc cũ, giữ đồ nội thất.
 * UnpackStatic: gỡ liên kết prefab khối tĩnh đang dùng; không xóa asset hay GUID.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
    public static class SchoolLayoutV2Migration
    {
        static Transform architecture,root;
        const float Floor=11.14f;
        const string Folder="Assets/TheReturn/Maps/SchoolFloor/Prefabs/";
        static readonly List<SchoolPreparationGate> gates=new List<SchoolPreparationGate>();

        /// <summary>Không có đầu vào; chạy migration một lần trên scene chính, giữ snapshot gồm sửa tay hiện tại.</summary>
        [MenuItem("The Return/School Floor/Apply Layout V2 and Preparation Gates")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying || scene.name!="SchoolFloorPrototype")throw new InvalidOperationException("Open SchoolFloorPrototype in Edit Mode.");
            if(GameObject.Find("SchoolLayoutV2"))throw new InvalidOperationException("V2 already exists; edit it instead of regenerating.");
            var f=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            architecture=GameObject.Find("BlockoutCampus_Generated/Architecture_From_User_Blockout").transform;
            for(int i=1;i<=6;i++)if(!architecture.Find("Phong_"+i))throw new InvalidOperationException("Missing user room "+i);
            if(!EditorSceneManager.SaveScene(scene,"Assets/TheReturn/Maps/SchoolFloor/Scenes/BeforeLayoutV2_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new InvalidOperationException("Snapshot failed.");
            root=new GameObject("SchoolLayoutV2").transform;gates.Clear();
            MoveRoom(f.navigationExam.transform,3);
            MoveRoom(f.teacherTruth.transform,4);
            RetireShell(f.dontLookBack.transform);
            var hall=architecture.Find("Hanh_Lang_4");
            SetPose(f.dontLookBack.transform,new Vector3(5.8f,0,92),new Vector3(hall.position.x,10.85f,hall.position.z),90);
            f.dontLookBack.exitDoor.gameObject.SetActive(false);
            PrepareAttendance(f);
            f.gradeRepair.exitDoor.gameObject.SetActive(false);f.gradeEntrance.gameObject.SetActive(false);
            f.navigationExam.exitDoor.gameObject.SetActive(false);f.teacherTruth.exitDoor.gameObject.SetActive(false);
            // Old text and classroom patches describe the rejected mapping.
            foreach(Transform t in architecture.parent)
                if(t.name=="CampusSign" || t.name=="ClassroomRearWall") t.gameObject.SetActive(false);
            var prefab=GatePrefab();
            int[] missions={0,2,3,5,-1,-1};
            for(int r=1;r<=6;r++)
            {
                var shell=architecture.Find("Phong_"+r);
                var roomGroup=new GameObject("Phong_"+r+"_Preparation").transform;roomGroup.SetParent(root,false);
                // Three real openings: positive Z entry, negative Z exit, positive X to central hall.
                for(int side=0;side<3;side++)
                {
                    Vector3 portal=side==0?new Vector3(.295f,0,22.65f):side==1?new Vector3(.245f,0,-22.65f):new Vector3(22.65f,0,-.11f);
                    float yaw=shell.eulerAngles.y+(side==0?180:side==1?0:270);
                    var gate=CreateGate(prefab,roomGroup,"Phong "+r+" / Cua "+(side+1),LocalPoint(shell,portal),yaw,side==2?7.3f:9.15f,7.75f,missions[r-1],side==0);
                    if(side==0 && missions[r-1]>0)f.transitionGatherZones[missions[r-1]-1]=gate.preparationZone;
                }
            }
            foreach(int mission in new[]{1,4})
            {
                int hallNumber=mission==1?2:4;
                var shell=architecture.Find("Hanh_Lang_"+hallNumber);
                var group=new GameObject("Hanh_Lang_"+hallNumber+"_Preparation").transform;group.SetParent(root,false);
                var center=new Vector3(shell.position.x,10.85f,shell.position.z);var rotation=Quaternion.Euler(0,mission==1?210:90,0);
                for(int side=0;side<2;side++)
                {
                    Vector3 portal=center+rotation*new Vector3(0,0,side==0?-26:26);
                    var gate=CreateGate(prefab,group,"Hanh lang "+hallNumber+" / "+(side==0?"Vao":"Ra"),portal,rotation.eulerAngles.y+(side==0?0:180),12.4f,7.8f,mission,side==0);
                    if(side==0)f.transitionGatherZones[mission-1]=gate.preparationZone;
                }
                // User corridors have open sides: guards prevent bypassing the mission via the gallery.
                if(mission==4)foreach(float side in new[]{-1f,1f})
                {
                    var rail=GameObject.CreatePrimitive(PrimitiveType.Cube);rail.name="SafetyRail";rail.transform.SetParent(group,false);
                    rail.transform.SetPositionAndRotation(center+rotation*new Vector3(side*6.1f,.7f,0),rotation);rail.transform.localScale=new Vector3(.18f,1.4f,54);
                    rail.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/DarkTimber.mat");
                }
            }
            f.stagingZone=f.transitionGatherZones[0];
            var access=root.gameObject.AddComponent<SchoolFloorAccessController>();access.flow=f;access.gates=gates.ToArray();
            // Keep Edit Mode uncluttered; runtime Awake closes the correct gates before play.
            foreach(var gate in gates)gate.Apply(true,false,f.attendance.party);
            UnpackStatic();Physics.SyncTransforms();EditorUtility.SetDirty(f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=root.gameObject;
        }

        /// <summary>Nhận root nhiệm vụ và số phòng; chuyển đồ logic/nội thất, không đưa vỏ cũ sang phòng mới.</summary>
        static void MoveRoom(Transform mission,int number)
        {
            var floor=mission.Find("Floor");var center=floor.localPosition;center.y=0;
            RetireShell(mission);var shell=architecture.Find("Phong_"+number);
            SetPose(mission,center,new Vector3(shell.position.x,Floor,shell.position.z),shell.eulerAngles.y+180);
        }

        /// <summary>Nhận root nhiệm vụ; ẩn tường/sàn cũ, giữ object và reference để snapshot/rollback rõ ràng.</summary>
        static void RetireShell(Transform mission)
        {
            var retired=new GameObject("RetiredShell_Disabled").transform;retired.SetParent(mission,false);
            var list=new List<Transform>();
            foreach(Transform t in mission)
                if(t.name=="Floor"||t.name.StartsWith("Wall_")||t.name.StartsWith("Front_")||t.name.StartsWith("Back_")||t.name.StartsWith("Entrance_")||t.name.StartsWith("Exit_Left")||t.name.StartsWith("Exit_Right")||t.name.StartsWith("Exit_Header")||t.name.StartsWith("NextArea_")||t.name=="Interior_Decoration")list.Add(t);
            foreach(var t in list)t.SetParent(retired,true);
            retired.gameObject.SetActive(false);
            var decor=mission.Find("SchoolRoom_Environment");if(decor)StripDecor(decor);
        }

        /// <summary>Nhận nhóm trang trí; giữ furniture/props, bỏ trần và các mảng kiến trúc của phòng nhỏ cũ.</summary>
        static void StripDecor(Transform decor)
        {
            foreach(Transform t in decor)
                if(t.name.Contains("Ceiling")||t.name.Contains("Dado")||t.name.Contains("Rail")||t.name.Contains("Skirting")||t.name.Contains("Cornice")||t.name.Contains("Window")||t.name.Contains("Lintel")||t.name=="RoomSign"||t.name=="FrostedPane"||t.name=="LampHousing"||t.name=="LampDiffuser"||t.name=="NoticeFrame"||t.name=="PinnedPaper")t.gameObject.SetActive(false);
        }

        /// <summary>Nhận flow; bỏ vỏ Attendance, giãn vị trí 24 ghế và đồ trang trí tương ứng, giữ kích thước model.</summary>
        static void PrepareAttendance(SchoolFloorFlow f)
        {
            var env=GameObject.Find("Environment_LogicOnly");if(env)env.SetActive(false);
            var art=GameObject.Find("Classroom_Environment");
            if(art)
            {
                StripDecor(art.transform);
                foreach(Transform t in art.transform)
                {
                    if(t.name.StartsWith("Light")||t.name=="Diffuser"||t.name.StartsWith("Board")||t.name=="ChalkTray"||t.name=="NoticeBoardModule")t.gameObject.SetActive(false);
                    if(t.name=="SchoolDesk"||t.name=="SchoolChair"||t.name=="ExerciseBook")
                    {
                        var p=t.localPosition;p.x*=1.6f;p.z=(p.z+2)*1.35f-2;t.localPosition=p;
                        if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                    }
                }
            }
            foreach(var seat in f.attendance.seats)
            {
                var p=seat.transform.localPosition;p.x*=1.6f;p.z=(p.z+2)*1.35f-2;seat.transform.localPosition=p;
                PrefabUtility.RecordPrefabInstancePropertyModifications(seat.transform);
            }
            f.attendance.exitDoor.gameObject.SetActive(false);
            var exitLabel=f.attendance.transform.Find("ExitLabel");if(exitLabel)exitLabel.gameObject.SetActive(false);
        }

        /// <summary>Nhận root, tâm local, tâm world và yaw; thay pose, không scale object nhiệm vụ.</summary>
        static void SetPose(Transform target,Vector3 localCenter,Vector3 center,float yaw)
        { target.rotation=Quaternion.Euler(0,yaw,0);target.position=center-target.rotation*localCenter; }

        /// <summary>Nhận shell và tọa độ trên mặt bằng; trả vị trí world với cốt sàn chơi.</summary>
        static Vector3 LocalPoint(Transform shell,Vector3 point)
        { return new Vector3(shell.position.x,Floor,shell.position.z)+Quaternion.Euler(0,shell.eulerAngles.y,0)*point; }

        /// <summary>Không có đầu vào; tạo prefab cửa chức năng gồm collider, visual, nhãn và vùng chuẩn bị.</summary>
        static GameObject GatePrefab()
        {
            System.IO.Directory.CreateDirectory(Folder);
            var go=new GameObject("PF_PreparationGate");var gate=go.AddComponent<SchoolPreparationGate>();
            var collision=new GameObject("Interaction_Blocker",typeof(BoxCollider)).transform;collision.SetParent(go.transform,false);gate.blocker=collision.GetComponent<BoxCollider>();
            var visual=GameObject.CreatePrimitive(PrimitiveType.Cube);visual.name="VisualRoot";visual.transform.SetParent(go.transform,false);UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/SchoolSage.mat");gate.visualRoot=visual;
            var zone=new GameObject("PreparationZone",typeof(BoxCollider)).transform;zone.SetParent(go.transform,false);gate.preparationZone=zone.GetComponent<BoxCollider>();gate.preparationZone.isTrigger=true;
            var label=new GameObject("Status",typeof(TextMesh)).GetComponent<TextMesh>();label.transform.SetParent(go.transform,false);label.transform.localPosition=new Vector3(0,3.3f,-.3f);label.fontSize=64;label.characterSize=.05f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
            label.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/WorldFont.mat");gate.statusLabel=label;
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Folder+"PF_PreparationGate.prefab");UnityEngine.Object.DestroyImmediate(go);return prefab;
        }

        /// <summary>Nhận hình học cổng và nhiệm vụ; trả cửa mới, vùng tập hợp nằm 4 m phía trong để không kẹp đội.</summary>
        static SchoolPreparationGate CreateGate(GameObject prefab,Transform parent,string name,Vector3 position,float yaw,float width,float height,int mission,bool entrance)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name=name.Replace(" / ","_");go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            var g=go.GetComponent<SchoolPreparationGate>();g.mission=mission;g.designatedEntrance=entrance;g.areaName=name;
            g.blocker.center=new Vector3(0,height/2,0);g.blocker.size=new Vector3(width,height,.4f);
            g.visualRoot.transform.localPosition=g.blocker.center;g.visualRoot.transform.localScale=g.blocker.size;
            g.preparationZone.transform.localPosition=new Vector3(0,1.2f,4.5f);g.preparationZone.size=new Vector3(width-.5f,3.6f,4);
            foreach(var obj in new UnityEngine.Object[]{g,go.transform,g.blocker,g.visualRoot.transform,g.preparationZone,g.preparationZone.transform})PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            gates.Add(g);return g;
        }

        /// <summary>Không có đầu vào; gỡ prefab khối tĩnh đang hoạt động, không đụng prefab có MonoBehaviour hay asset nguồn.</summary>
        static void UnpackStatic()
        {
            var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach(var t in all)
            {
                if(!t || !PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject))continue;
                string path=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
                if(path.EndsWith("PF_GB_Block.prefab",StringComparison.Ordinal) && t.GetComponentsInChildren<MonoBehaviour>(true).Length==0)
                    PrefabUtility.UnpackPrefabInstance(t.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            }
        }
    }
}
