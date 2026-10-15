/*
 * Mục đích: chuyển sáu nhiệm vụ sang bố cục Map The Reture do người dùng thiết kế.
 * Build lưu dự phòng, tạo bản kiến trúc dùng trong game, đặt phòng và vùng chuẩn bị;
 * Find tìm cả root đang ẩn; Move đặt tâm sàn mà không đổi kích thước câu đố;
 * Carry di chuyển các root liên quan lớp điểm danh bằng cùng phép biến đổi;
 * ShellPoint chuyển tọa độ mặt bằng blockout ra world; Link nối các cửa theo hành lang;
 * Deck tạo sàn ngưỡng/ramp; Label tạo biển chỉ dẫn; Validate kiểm tra đường và spawn.
 * Bản gốc được giữ nguyên hình học và ẩn; chỉ dựng lại root BlockoutCampus_Generated.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn.Art.Editor
{
    public static class UserBlockoutLayoutBuilder
    {
        const string Backup="Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorBeforeUserBlockout.unity";
        const string Folder="Assets/TheReturn/Art/Environment/UserBlockout/";
        static readonly string[] Names={"Environment_LogicOnly","QuietCorridor_LogicOnly","GradeRepair_LogicOnly","NavigationExam_LogicOnly","DontLookBack_LogicOnly","TeacherTruth_LogicOnly"};
        static readonly string[] Titles={"DIEM DANH","GIU TRAT TU","SUA BANG DIEM","BAI KIEM TRA","KHONG QUAY DAU","GIAO VIEN"};
        static Transform generated,architecture;
        static Material floor,trim,font;
        static readonly List<Vector3> routeSamples=new List<Vector3>();
        static readonly List<Vector3> spawnSamples=new List<Vector3>();
        const float FloorY=11.14f;

        /// <summary>Không có đầu vào; ghép scene đang mở vào blockout, lưu bản gốc và scene sau khi dựng.</summary>
        [MenuItem("The Return/School Floor/Apply User Blockout Layout")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying||scene.name!="SchoolFloorPrototype")throw new InvalidOperationException("Open SchoolFloorPrototype in Edit Mode.");
            var source=Find("Map The Reture");if(!source)throw new InvalidOperationException("Missing Map The Reture.");
            for(int i=1;i<=6;i++)if(!source.Find("Phong_"+i)||!source.Find("Khu_Chuan_Bi_"+i)||!source.Find("Hanh_Lang_"+i))throw new InvalidOperationException("Missing blockout area "+i);
            if(!System.IO.File.Exists(Backup))EditorSceneManager.SaveScene(scene,Backup,true);
            System.IO.Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            var previous=Find("BlockoutCampus_Generated");if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            generated=new GameObject("BlockoutCampus_Generated").transform;
            architecture=UnityEngine.Object.Instantiate(source.gameObject,generated).transform;
            architecture.name="Architecture_From_User_Blockout";architecture.gameObject.SetActive(true);source.gameObject.SetActive(false);
            floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/Terrazzo.mat");
            trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/DarkTimber.mat");
            font=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/WorldFont.mat");
            routeSamples.Clear();spawnSamples.Clear();
            foreach(Transform t in architecture)
            {
                var mesh=t.GetComponent<MeshFilter>();if(!mesh)continue;
                bool solid=t.name.StartsWith("Phong_")||t.name.StartsWith("Khu_Chuan_Bi")||t.name.StartsWith("Hanh_Lang")||t.name.StartsWith("San")||t.name.StartsWith("Cau_Thang")||t.name=="Lang_Can";
                if(solid){var col=t.GetComponent<MeshCollider>();if(!col)col=t.gameObject.AddComponent<MeshCollider>();col.sharedMesh=mesh.sharedMesh;col.convex=false;}
                // Các sàn 2–6 trong bản nhập cao hơn chân cửa; hạ bản dùng trong game về cùng cốt chân cửa.
                if(t.name.StartsWith("San_Phong")){var p=t.position;p.y+=FloorY-t.GetComponent<Renderer>().bounds.max.y;t.position=p;}
            }
            foreach(string name in new[]{"Circulation_Network","MonumentalSchool_ArtOnly","Observatory_Decoration"}){var t=Find(name);if(t)t.gameObject.SetActive(false);}
            var flow=UnityEngine.Object.FindFirstObjectByType<TheReturn.SchoolFloorFlow>();
            var environments=new Transform[6];
            for(int i=0;i<6;i++)
            {
                var room=Find(Names[i]);environments[i]=room;
                var shell=architecture.Find("Phong_"+(i+1));
                var center=shell.position;center.y=FloorY;
                var rotation=Quaternion.Euler(0,shell.eulerAngles.y+180,0);
                var f=room.Find("Floor");var sourceCenter=f.localPosition;sourceCenter.y=0;
                Vector3 oldPosition=room.position;Quaternion oldRotation=room.rotation;
                Move(room,sourceCenter,center,rotation);
                if(i==5)
                {
                    // Hai vách cũ gặp nhau tại tâm cửa; trả lại ô cửa rộng 2,4 m theo header.
                    var left=room.Find("Exit_Left");var right=room.Find("Exit_Right");
                    var p=left.localPosition;p.x=1.7f;left.localPosition=p;
                    p=right.localPosition;p.x=9.9f;right.localPosition=p;
                }
                if(i==0)
                {
                    Carry(Find("Puzzle_Attendance"),oldPosition,oldRotation,room);
                    Carry(Find("LocalParty"),oldPosition,oldRotation,room);
                    var art=Find("Classroom_Environment");
                    if(art&&art.parent!=room)
                    {
                        foreach(Transform t in art)if(t.localPosition.z>8.05f)t.gameObject.SetActive(false);
                        Carry(art,oldPosition,oldRotation,room);
                    }
                    if(flow.attendance.party.viewCamera.transform.parent==null)Carry(flow.attendance.party.viewCamera.transform,oldPosition,oldRotation,room);
                    // Tạo cửa sau lớp để khu chuẩn bị dẫn vào phòng thay vì đụng tường kín.
                    var back=room.Find("Wall_Back");if(back)back.gameObject.SetActive(false);
                    for(int side=-1;side<=1;side+=2)
                    {
                        var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="ClassroomRearWall";wall.transform.SetParent(generated);
                        wall.transform.SetPositionAndRotation(room.TransformPoint(new Vector3(side*4.4f,1.8f,-13.5f)),room.rotation);wall.transform.localScale=new Vector3(5.2f,3.6f,.2f);wall.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/IvoryPlaster.mat");
                    }
                }
                // Cốt sàn bằng sàn blockout; giữ collider cũ, tắt mặt trùng để tránh nhấp nháy.
                f.GetComponent<Renderer>().enabled=false;
                foreach(Transform t in room)if(t.name.StartsWith("Checkpoint_"))spawnSamples.Add(t.position);
                float near=f.localPosition.z-f.localScale.z/2;
                Vector3 entry=room.TransformPoint(new Vector3(f.localPosition.x,0,near+.6f));
                Vector3 portal=ShellPoint(i,new Vector3(.27f,0,23.8f));
                Deck("RoomEntry_"+(i+1),portal,entry,3.4f);
                Vector3 prep=ShellPoint(i,new Vector3(.27f,0,27));
                Label("KHU CHUAN BI "+(i+1)+"\n"+Titles[i],prep+Vector3.up*2.8f,rotation,.065f);
                // Nhánh hướng sảnh theo tên Khu_Chuan_Bi: giữ đường vào sảnh dùng được.
                Vector3 radial=ShellPoint(i,new Vector3(27,0,0));
                Label("SANH DONG HO",radial+Vector3.up*2.8f,Quaternion.Euler(0,shell.eulerAngles.y+90,0),.06f);
                Vector3 landing=ShellPoint(i,new Vector3(39,0,0));landing.y=10.9f;
                Deck("RadialPreparation_"+i,ShellPoint(i,new Vector3(21.4f,0,0)),landing,4.6f);
                Vector3 stairTop=ShellPoint(i,new Vector3(41,0,0));stairTop.y=10.9f;
                Vector3 stairBottom=ShellPoint(i,new Vector3(62.3f,0,0));stairBottom.y=2.4f;
                Deck("StairLanding_"+i,landing,stairTop,4.6f);
                Deck("StairRamp_"+i,stairTop,stairBottom,4.6f);
                // Ramp chỉ xử lý va chạm; bậc thang do người dùng dựng vẫn là hình ảnh nhìn thấy.
                generated.Find("StairRamp_"+i).GetComponent<Renderer>().enabled=false;
            }
            flow.transitionGatherZones=new BoxCollider[5];
            for(int i=0;i<5;i++)
            {
                Vector3 prep=ShellPoint(i+1,new Vector3(.27f,0,25.5f));
                var zone=new GameObject("Preparation_"+(i+2)+"_Gather");zone.transform.SetParent(generated);zone.transform.position=prep+Vector3.up*1.2f;
                zone.transform.rotation=Quaternion.Euler(0,architecture.Find("Phong_"+(i+2)).eulerAngles.y,0);
                var collider=zone.AddComponent<BoxCollider>();collider.isTrigger=true;collider.size=new Vector3(5.8f,3.8f,5.8f);flow.transitionGatherZones[i]=collider;
                Link(i,environments[i]);
            }
            Link(5,environments[5]);
            flow.stagingZone=flow.transitionGatherZones[0];
            EditorUtility.SetDirty(flow);
            foreach(var p in flow.attendance.party.players)spawnSamples.Add(p.position);
            Physics.SyncTransforms();Debug.Log(Validate());
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=generated.gameObject;
        }

        /// <summary>Nhận tên root; trả Transform dù root đang bị ẩn, so tên bỏ khoảng trắng cuối.</summary>
        static Transform Find(string name){foreach(var g in SceneManager.GetActiveScene().GetRootGameObjects())if(g.name.Trim()==name)return g.transform;return null;}

        /// <summary>Nhận tâm nguồn, tâm đích và hướng; đặt root không scale.</summary>
        static void Move(Transform room,Vector3 source,Vector3 target,Quaternion rotation){room.SetPositionAndRotation(target-rotation*source,rotation);}

        /// <summary>Nhận root liên quan và pose cũ; áp cùng phép di chuyển với root môi trường mới.</summary>
        static void Carry(Transform t,Vector3 oldPosition,Quaternion oldRotation,Transform room)
        {if(!t)return;var delta=room.rotation*Quaternion.Inverse(oldRotation);t.SetPositionAndRotation(room.position+delta*(t.position-oldPosition),delta*t.rotation);}

        /// <summary>Nhận chỉ số phòng và offset mét theo mặt bằng gốc; trả điểm world ở cốt sàn phòng.</summary>
        static Vector3 ShellPoint(int index,Vector3 offset)
        {var t=architecture.Find("Phong_"+(index+1));var p=t.position;p.y=FloorY;return p+Quaternion.Euler(0,t.eulerAngles.y,0)*offset;}

        /// <summary>Nhận phòng hiện tại; nối cửa ra tới hành lang vòng và khu chuẩn bị của phòng sau.</summary>
        static void Link(int index,Transform room)
        {
            var f=room.Find("Floor");float far=f.localPosition.z+f.localScale.z/2;
            Vector3 exit=room.TransformPoint(new Vector3(index==0?5.8f:f.localPosition.x,0,index==0?8.2f:far+.1f));
            Vector3 turn=exit+room.forward*(index==0?3:5.3f);
            Vector3 door=ShellPoint(index,new Vector3(.27f,0,-23.8f));
            Vector3 prep=ShellPoint(index,new Vector3(-1,0,-29));prep.y=10.85f;
            int nextIndex=(index+1)%6;
            var hall=architecture.Find("Hanh_Lang_"+(nextIndex+1));var middle=hall.position;middle.y=10.85f;
            Vector3 axis=Quaternion.Euler(0,hall.eulerAngles.y,0)*Vector3.right;
            Vector3 a=middle-axis*27,b=middle+axis*27;
            if(Vector3.Distance(a,prep)>Vector3.Distance(b,prep)){var swap=a;a=b;b=swap;}
            Vector3 next=ShellPoint(nextIndex,new Vector3(-1,0,29));next.y=10.85f;
            Vector3 end=ShellPoint(nextIndex,new Vector3(.27f,0,23.8f));
            Vector3[] points={exit,turn,door,prep,a,middle,b,next,end};
            for(int i=1;i<points.Length;i++)Deck("Route_"+(index+1)+"_"+i,points[i-1],points[i],3.4f);
            Label("DEN PHONG "+(nextIndex+1)+" / "+Titles[nextIndex],middle+Vector3.up*3,Quaternion.LookRotation(b-a),.06f);
        }

        /// <summary>Nhận hai đầu và chiều rộng; dựng sàn ngưỡng liên tục, thêm mẫu để kiểm tra vật cản.</summary>
        static void Deck(string name,Vector3 a,Vector3 b,float width)
        {
            var delta=b-a;if(delta.magnitude<.02f)return;
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(generated);go.transform.position=(a+b)/2-Vector3.up*.15f;
            go.transform.rotation=Quaternion.LookRotation(delta);go.transform.localScale=new Vector3(width,.3f,delta.magnitude+.15f);go.GetComponent<Renderer>().sharedMaterial=floor;
            int count=Mathf.CeilToInt(delta.magnitude/.5f);for(int i=0;i<=count;i++)routeSamples.Add(Vector3.Lerp(a,b,(float)i/count));
        }

        /// <summary>Nhận nội dung và pose world; tạo biển chỉ hướng dùng font depth test.</summary>
        static void Label(string text,Vector3 p,Quaternion q,float size)
        {
            var t=new GameObject("CampusSign",typeof(TextMesh)).GetComponent<TextMesh>();t.transform.SetParent(generated);t.transform.SetPositionAndRotation(p,q);t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.text=text;t.GetComponent<Renderer>().sharedMaterial=font;
        }

        /// <summary>Không nhận đầu vào; trả số điểm có sàn, vướng vật cản và spawn chưa có sàn.</summary>
        public static string Validate()
        {
            int missing=0;var blocked=new HashSet<string>();
            foreach(var p in routeSamples)
            {
                if(!Physics.Raycast(p+Vector3.up*.4f,Vector3.down,1.5f,~0,QueryTriggerInteraction.Ignore))missing++;
                foreach(var c in Physics.OverlapCapsule(p+Vector3.up*.55f,p+Vector3.up*1.55f,.27f,~0,QueryTriggerInteraction.Ignore))
                    if(!(c is CharacterController)&&c.GetComponentInParent<TheReturn.PuzzleDoor>()==null)blocked.Add(c.name+" @ "+p);
            }
            int unsafeSpawns=0;foreach(var p in spawnSamples)if(!Physics.Raycast(p+Vector3.up*.4f,Vector3.down,1.5f,~0,QueryTriggerInteraction.Ignore))unsafeSpawns++;
            return "Blockout: "+routeSamples.Count+" route samples; missing="+missing+"; blocked="+blocked.Count+"; spawns without floor="+unsafeSpawns+"\n"+string.Join("\n",blocked);
        }
    }
}
