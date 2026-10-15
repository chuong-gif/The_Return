/*
 * Mục đích: Dựng năm khu chuyển tiếp có sàn liên tục giữa sáu phòng nhiệm vụ.
 * Hàm: Build tạo tuyến và vùng tập hợp; Route dựng sàn dốc, lan can và nhịp cột;
 * Landing tạo khu nghỉ; Box tạo module; Sign tạo biển; Validate kiểm tra mặt sàn/vật cản;
 * Point đổi tọa độ cửa sang world; Material lấy vật liệu dùng chung;
 * BuildPerimeter bao mép sàn; ClearOldObstacles ẩn art chắn đường; Ribbon tạo mặt đường không chồng mặt.
 * Chỉ thay lớp Circulation_Network và ẩn cầu trang trí cũ; không di chuyển phòng nhiệm vụ.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheReturn.Art.Editor
{
    public static class SchoolCirculationBuilder
    {
        static Transform root;
        static Material stone, dark, gold;
        static readonly List<Vector3> samples = new List<Vector3>();
        static readonly List<Transform> decks = new List<Transform>();
        static readonly List<Vector3> doors = new List<Vector3>();

        /// <summary>Không có đầu vào; dựng năm tuyến, cấu hình vùng tập hợp và lưu scene đang mở.</summary>
        [MenuItem("The Return/School Floor/Build Connected Walkways")]
        public static void Build()
        {
            if(Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="SchoolFloorPrototype")
                throw new InvalidOperationException("Mở SchoolFloorPrototype trong Edit Mode.");
            var previous=GameObject.Find("Circulation_Network");
            if(previous!=null)UnityEngine.Object.DestroyImmediate(previous);
            root=new GameObject("Circulation_Network").transform;
            stone=Material("Limestone");dark=Material("Obsidian");gold=Material("AstralGold");samples.Clear();decks.Clear();doors.Clear();
            var art=GameObject.Find("MonumentalSchool_ArtOnly");
            if(art!=null)foreach(Transform t in art.transform)
                if(t.name.StartsWith("Bridge_")||t.name=="ArrivalBridge")t.gameObject.SetActive(false);
            foreach(string n in new[]{"NavigationExam_LogicOnly","DontLookBack_LogicOnly"})
            { var cap=GameObject.Find(n).transform.Find("NextArea_End");if(cap!=null)cap.gameObject.SetActive(false); }
            // Cửa art trước đây không khớp cửa gameplay: bỏ lớp cửa giả để nhìn rõ lối thoát thật.
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name=="PBR_Door"&&t.parent!=null&&t.parent.name=="Interior_Decoration")t.gameObject.SetActive(false);

            Vector3[][] paths={
                new[]{new Vector3(5.8f,0,8.2f),new Vector3(5.8f,0,14),new Vector3(19,0,14),new Vector3(23,1.4f,-13),Point("QuietCorridor_LogicOnly",5.8f,7),Point("QuietCorridor_LogicOnly",5.8f,9.5f)},
                new[]{Point("QuietCorridor_LogicOnly",5.8f,37.8f),Point("QuietCorridor_LogicOnly",5.8f,42),new Vector3(64,2.4f,24),new Vector3(59,3.4f,57),new Vector3(20,5.4f,62),Point("GradeRepair_LogicOnly",5.8f,34),Point("GradeRepair_LogicOnly",5.8f,37.6f)},
                new[]{Point("GradeRepair_LogicOnly",5.8f,60.8f),Point("GradeRepair_LogicOnly",5.8f,66),new Vector3(72,6.4f,41),new Vector3(72,7.4f,86),new Vector3(0,9.4f,86),Point("NavigationExam_LogicOnly",5.8f,58),Point("NavigationExam_LogicOnly",5.8f,60.5f)},
                new[]{Point("NavigationExam_LogicOnly",5.8f,81.8f),new Vector3(0,9.4f,51),new Vector3(-17,11.4f,55),Point("DontLookBack_LogicOnly",5.8f,78),Point("DontLookBack_LogicOnly",5.8f,82.3f)},
                new[]{Point("DontLookBack_LogicOnly",5.8f,105.8f),Point("DontLookBack_LogicOnly",5.8f,110),new Vector3(-64,14.4f,19),new Vector3(-64,15.4f,-18),new Vector3(-20,17.4f,-18),Point("TeacherTruth_LogicOnly",5.8f,102),Point("TeacherTruth_LogicOnly",5.8f,105.4f)}
            };
            string[] titles={"01 / HANH LANG KY NIEM","02 / SAN HO SO","03 / CAU KHU THI","04 / BAN CONG QUAN SAT","05 / TIEN SANH GIAO VIEN"};
            var flow=UnityEngine.Object.FindFirstObjectByType<TheReturn.SchoolFloorFlow>();
            flow.transitionGatherZones=new BoxCollider[5];
            for(int i=0;i<paths.Length;i++)
            {
                Route(paths[i],i,titles[i]);
                doors.Add(paths[i][0]);doors.Add(paths[i][paths[i].Length-1]);
                var zone=new GameObject("Gather_NextMission_"+(i+2));zone.transform.SetParent(root);
                zone.transform.position=paths[i][paths[i].Length-1]+Vector3.up;
                var box=zone.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(7,3,7);flow.transitionGatherZones[i]=box;
            }
            flow.stagingZone.transform.position=flow.transitionGatherZones[0].transform.position;
            flow.stagingZone.transform.rotation=Quaternion.identity;flow.stagingZone.size=new Vector3(7,3,7);
            EditorUtility.SetDirty(flow);Physics.SyncTransforms();
            BuildPerimeter();ClearOldObstacles();Physics.SyncTransforms();
            Debug.Log(Validate());EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>Nhận root phòng và tọa độ cửa local; trả điểm trên mặt sàn world.</summary>
        static Vector3 Point(string room,float x,float z) {return GameObject.Find(room).transform.TransformPoint(new Vector3(x,0,z));}

        /// <summary>Nhận tên vật liệu; trả asset dùng chung đã được dựng bởi art pass thiên văn.</summary>
        static Material Material(string name) {return AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Environment/"+name+".mat");}

        /// <summary>Nhận đường gấp khúc, số tuyến và tiêu đề; tạo dốc liền, lan can và khung kiến trúc.</summary>
        static void Route(Vector3[] path,int index,string title)
        {
            var smooth=new List<Vector3>{path[0]};
            for(int i=1;i<path.Length-1;i++)
            {
                var incoming=path[i-1]-path[i];incoming.y=0;
                var outgoing=path[i+1]-path[i];outgoing.y=0;
                smooth.Add(path[i]+incoming.normalized*Mathf.Min(3.5f,incoming.magnitude*.35f));
                smooth.Add(path[i]);
                smooth.Add(path[i]+outgoing.normalized*Mathf.Min(3.5f,outgoing.magnitude*.35f));
            }
            smooth.Add(path[path.Length-1]);var original=path;path=smooth.ToArray();
            for(int i=0;i<path.Length-1;i++)
            {
                Vector3 a=path[i],b=path[i+1],delta=b-a;Quaternion q=Quaternion.LookRotation(delta);
                float length=delta.magnitude;Vector3 right=q*Vector3.right;
                Box("WalkableDeck_"+index,(a+b)*.5f-Vector3.up*.18f,new Vector3(4.4f,.36f,length+.12f),stone,q,true);
                int count=Mathf.Max(1,Mathf.CeilToInt(length));
                for(int j=0;j<=count;j++)samples.Add(Vector3.Lerp(a,b,j/(float)count));
                for(float t=5;t<length-4;t+=9)
                {
                    Vector3 p=Vector3.Lerp(a,b,t/length);
                    for(int side=-1;side<=1;side+=2)Box("GalleryPier",p+right*side*2.38f+Vector3.up*2.3f,new Vector3(.28f,4.6f,.4f),stone,q,false);
                    Box("GalleryLintel",p+Vector3.up*4.6f,new Vector3(5.2f,.25f,.55f),stone,q,false);
                }
            }
            Ribbon(path,index);
            for(int i=1;i<original.Length-1;i++)Landing(original[i],i==2?title:null,index);
            Sign("PHONG "+(index+2)+"  /  TAP HOP CA DOI",path[path.Length-2]+Vector3.up*3.1f,
                Quaternion.LookRotation(path[path.Length-1]-path[path.Length-2]));
        }

        /// <summary>Nhận tâm, titre tùy chọn và số tuyến; tạo khu nghỉ mở, ghế, đèn và mốc định hướng.</summary>
        static void Landing(Vector3 p,string title,int index)
        {
            Box("Landing",p-Vector3.up*.145f,new Vector3(6,.36f,6),stone,Quaternion.identity,true);
            if(title==null)return;
            // Khu nghi o phia ngoai tuyen; ban cong so 4 quay sang bac tranh duong doc.
            Vector3 offset=index==3?new Vector3(0,0,4):new Vector3(index==4?-4:4,0,0);
            Quaternion orient=Quaternion.FromToRotation(Vector3.right,offset.normalized);
            Box("RestAlcove",p+offset-Vector3.up*.13f,new Vector3(4,.36f,6),stone,orient,true);
            Box("BenchSeat",p+offset*1.25f+Vector3.up*.45f,new Vector3(.6f,.18f,2.8f),stone,orient,true);
            Box("BenchBase",p+offset*1.25f+Vector3.up*.2f,new Vector3(.4f,.4f,2.4f),dark,orient,true);
            Sign(title,p+offset*1.4f+Vector3.up*2.3f,orient*Quaternion.Euler(0,90,0));
            var l=new GameObject("RestLight_"+index,typeof(Light)).GetComponent<Light>();l.transform.SetParent(root);l.transform.position=p+Vector3.up*3;
            l.type=LightType.Point;l.color=new Color(1,.76f,.44f);l.range=12;l.intensity=4;l.shadows=LightShadows.None;
        }

        /// <summary>Nhận pose, taille, matériau và cờ collider; trả module kiến trúc dưới root tuyến.</summary>
        static GameObject Box(string name,Vector3 p,Vector3 size,Material mat,Quaternion q,bool collision)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,q);go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            if(name.StartsWith("WalkableDeck"))go.GetComponent<Renderer>().enabled=false;
            if(name.StartsWith("WalkableDeck")||name=="Landing"||name=="RestAlcove")decks.Add(go.transform);
            return go;
        }

        /// <summary>Nhận các điểm tuyến và số thứ tự; lưu mesh mặt đường nối góc liên tục, tránh nhấp nháy mặt chồng.</summary>
        static void Ribbon(Vector3[] path,int index)
        {
            var vertices=new Vector3[path.Length*2];var uv=new Vector2[vertices.Length];var triangles=new int[(path.Length-1)*6];float distance=0;
            for(int i=0;i<path.Length;i++)
            {
                Vector3 before=path[i]-path[Mathf.Max(0,i-1)],after=path[Mathf.Min(path.Length-1,i+1)]-path[i];
                before.y=after.y=0;if(before.sqrMagnitude<.001f)before=after;if(after.sqrMagnitude<.001f)after=before;
                Vector3 r1=Vector3.Cross(Vector3.up,before.normalized),r2=Vector3.Cross(Vector3.up,after.normalized);
                Vector3 m=(r1+r2).normalized;float width=Mathf.Min(3,2.2f/Mathf.Max(.5f,Vector3.Dot(m,r1)));
                if(i>0)distance+=Vector3.Distance(path[i],path[i-1]);
                vertices[i*2]=path[i]-m*width;vertices[i*2+1]=path[i]+m*width;
                uv[i*2]=new Vector2(0,distance/3);uv[i*2+1]=new Vector2(1.5f,distance/3);
                if(i<path.Length-1){int k=i*6,v=i*2;triangles[k]=v;triangles[k+1]=v+2;triangles[k+2]=v+1;triangles[k+3]=v+1;triangles[k+4]=v+2;triangles[k+5]=v+3;}
            }
            string folder="Assets/TheReturn/Art/Environment";string asset=folder+"/Walkway_"+index+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(asset);if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,asset);}else mesh.Clear();
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var go=new GameObject("ContinuousSurface_"+index,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=stone;
            AssetDatabase.SaveAssets();
        }

        /// <summary>Không có đầu vào; bao mép sàn bằng lan can, bỏ các cạnh trong và khoảng cửa vào phòng.</summary>
        static void BuildPerimeter()
        {
            foreach(var deck in decks.ToArray())for(int edge=0;edge<4;edge++)
            {
                bool alongZ=edge<2;float sign=edge%2==0?-1:1;
                float length=alongZ?deck.localScale.z:deck.localScale.x;
                int count=Mathf.CeilToInt(length/.7f);
                for(int i=0;i<count;i++)
                {
                    float t=(i+.5f)/count-.5f;
                    Vector3 local=alongZ?new Vector3(sign*.5f,.5f,t):new Vector3(t,.5f,sign*.5f);
                    Vector3 p=deck.TransformPoint(local);
                    Vector3 outward=deck.TransformDirection(alongZ?Vector3.right*sign:Vector3.forward*sign);
                    bool doorway=doors.Exists(d=>Vector3.Distance(d,p)<3.2f);
                    if(doorway||Physics.Raycast(p+outward*.32f+Vector3.up*.6f,Vector3.down,1.2f,~0,QueryTriggerInteraction.Ignore))continue;
                    Quaternion q=Quaternion.LookRotation(alongZ?deck.forward:deck.right);
                    Box("PerimeterRail",p+Vector3.up*.65f,new Vector3(.12f,1.3f,length/count+.04f),dark,q,true);
                    Box("PerimeterGuide",p+Vector3.up*1.32f,new Vector3(.05f,.035f,length/count+.04f),gold,q,false);
                }
            }
        }

        /// <summary>Không có đầu vào; ẩn riêng các khối art cũ đâm vào đường đi, giữ cửa nhiệm vụ.</summary>
        static void ClearOldObstacles()
        {
            var art=GameObject.Find("MonumentalSchool_ArtOnly");if(art==null)return;
            foreach(var p in samples)foreach(var c in Physics.OverlapCapsule(p+Vector3.up*.5f,p+Vector3.up*1.5f,.3f,~0,QueryTriggerInteraction.Ignore))
                if(c.transform.IsChildOf(art.transform))c.gameObject.SetActive(false);
        }

        /// <summary>Nhận chữ, vị trí, hướng; tạo biển hai mặt đọc được từ hai chiều.</summary>
        static void Sign(string text,Vector3 p,Quaternion q)
        {
            for(int i=0;i<2;i++){
                var go=new GameObject("Wayfinding");go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,q*Quaternion.Euler(0,i*180,0));
                var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=.035f;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(1,.85f,.6f);
            }
        }

        /// <summary>Không có đầu vào; trả thống kê mẫu đường có sàn và vật cản ngang capsule 1,8m.</summary>
        public static string Validate()
        {
            int missing=0;var blocked=new HashSet<string>();
            foreach(var p in samples){
                if(!Physics.Raycast(p+Vector3.up*.3f,Vector3.down,.7f,~0,QueryTriggerInteraction.Ignore))missing++;
                foreach(var c in Physics.OverlapCapsule(p+Vector3.up*.5f,p+Vector3.up*1.5f,.25f,~0,QueryTriggerInteraction.Ignore))
                    if(!(c is CharacterController)&&c.GetComponentInParent<TheReturn.PuzzleDoor>()==null)blocked.Add(c.name+" @ "+p);
            }
            return "Circulation: "+samples.Count+" samples; missing floor="+missing+"; blocked="+blocked.Count+"\n"+string.Join("\n",blocked);
        }
    }
}
