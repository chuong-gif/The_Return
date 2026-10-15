/*
 * Mục đích: hoàn thiện năm phòng nhiệm vụ còn lại theo bộ kiến trúc trường học.
 * Build dựng và lưu năm phòng; Dress tạo vỏ kiến trúc, ánh sáng và nội thất theo chủ đề;
 * Prop đặt prefab theo kích thước local và giữ bản nguồn; BoundsLocal đo kích thước;
 * MaterialFor sao chép texture sang URP; Box tạo module; Window tạo cửa sổ giả áp tường;
 * Notice tạo bảng thông báo; Label tạo chữ có depth test; Lamp tạo đèn;
 * Validate kiểm tra checkpoint bị nội thất mới che; Mat đọc vật liệu khu lớp mẫu.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheReturn.Art.Editor
{
    public static class MissionRoomEnvironmentPass
    {
        const string Output="Assets/TheReturn/Art/Interiors/MissionRooms/";
        const string Furniture="Furniture Mega Pack/Prefabs/";
        static readonly string[] Rooms={"QuietCorridor_LogicOnly","GradeRepair_LogicOnly","NavigationExam_LogicOnly","DontLookBack_LogicOnly","TeacherTruth_LogicOnly"};
        static readonly string[] Titles={"02 / GIU TRAT TU","03 / PHONG HOC VU","04 / PHONG KHAO THI","05 / KHU HAN CHE","06 / PHONG GIAO VIEN"};
        static Transform root;
        static Material plaster,green,wood,floor,glass,lamp,paper;
        static readonly Dictionary<Material,Material> converted=new Dictionary<Material,Material>();

        /// <summary>Không nhận đầu vào; dựng năm phòng trong scene hiện tại, lưu scene và material riêng.</summary>
        [MenuItem("The Return/Art/Upgrade Remaining Mission Rooms")]
        public static void Build()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="SchoolFloorPrototype")throw new InvalidOperationException("Open SchoolFloorPrototype in Edit Mode.");
            System.IO.Directory.CreateDirectory(Output);AssetDatabase.Refresh();converted.Clear();
            plaster=Mat("IvoryPlaster");green=Mat("SchoolSage");wood=Mat("DarkTimber");floor=Mat("Terrazzo");glass=Mat("WindowDaylight");lamp=Mat("Fluorescent");paper=Mat("NoticePaper");
            for(int i=0;i<Rooms.Length;i++)Dress(GameObject.Find(Rooms[i]).transform,i);
            Physics.SyncTransforms();Debug.Log(Validate());AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>Nhận root phòng và chủ đề; thay lớp trang trí, giữ vị trí và component nhiệm vụ.</summary>
        static void Dress(Transform room,int index)
        {
            var old=room.Find("Interior_Decoration");if(old)old.gameObject.SetActive(false);
            old=room.Find("SchoolRoom_Environment");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            root=new GameObject("SchoolRoom_Environment").transform;root.SetParent(room,false);
            var f=room.Find("Floor");var c=f.localPosition;float w=f.localScale.x,d=f.localScale.z,y=c.y+f.localScale.y/2;
            float left=c.x-w/2,right=c.x+w/2,near=c.z-d/2,far=c.z+d/2;
            string accentPath=Output+"WallAccent_"+index+".mat";
            var accent=AssetDatabase.LoadAssetAtPath<Material>(accentPath);
            if(!accent){accent=new Material(green);AssetDatabase.CreateAsset(accent,accentPath);}
            Color[] colors={new Color(.16f,.25f,.22f),new Color(.26f,.22f,.15f),new Color(.16f,.23f,.29f),new Color(.18f,.12f,.12f),new Color(.22f,.18f,.16f)};
            accent.SetColor("_BaseColor",colors[index]);EditorUtility.SetDirty(accent);
            foreach(Transform t in room)
            {
                var r=t.GetComponent<Renderer>();if(!r)continue;
                if(t.name.Contains("Floor"))r.sharedMaterial=floor;
                else if(t.name.Contains("Wall")||t.name.Contains("Header")||t.name.StartsWith("Front_")||t.name.StartsWith("Back_"))r.sharedMaterial=plaster;
            }
            Box("Ceiling",new Vector3(c.x,y+3.78f,c.z),new Vector3(w+.12f,.24f,d),plaster);
            foreach(float x in new[]{left+.15f,right-.15f})
            {
                Box("Dado",new Vector3(x,y+.6f,c.z),new Vector3(.075f,1.2f,d-.2f),accent);
                Box("ChairRail",new Vector3(x,y+1.23f,c.z),new Vector3(.13f,.06f,d-.2f),wood);
                Box("Skirting",new Vector3(x,y+.08f,c.z),new Vector3(.14f,.16f,d-.2f),wood);
                Box("Cornice",new Vector3(x,y+3.5f,c.z),new Vector3(.25f,.15f,d-.2f),plaster);
            }
            for(float z=near+2.2f;z<far-1;z+=4.5f)
            {
                Box("CeilingRib",new Vector3(c.x,y+3.58f,z),new Vector3(w,.12f,.18f),plaster);
                if(w>10){Lamp(new Vector3(c.x-3,y+3.45f,z),index);Lamp(new Vector3(c.x+3,y+3.45f,z),index);}
                else Lamp(new Vector3(c.x,y+3.45f,z),index);
                if(index!=3)Window(new Vector3(right-.24f,y+2.3f,z),2.9f);
                else
                {
                    // Bảng cảnh báo nằm cao, không che đường đi và terminal.
                    Notice(new Vector3(right-.25f,y+2.65f,z),"KHU VUC HAN CHE",1.6f);
                    Box("WallServicePipe",new Vector3(left+.25f,y+3.2f,z),new Vector3(.1f,.1f,4.5f),wood);
                }
            }
            Label(Titles[index],new Vector3(c.x,y+3.22f,far-.3f),Quaternion.identity,.032f);
            // Các hốc cửa giữa phòng giữ nguyên bề rộng; chỉ tạo phào trên cao.
            Box("ExitLintel",new Vector3(c.x,y+3.54f,far-.2f),new Vector3(Mathf.Min(w,4),.16f,.2f),wood);
            foreach(Transform next in room)if(next.name=="NextArea_Floor")Box("VestibuleCeiling",next.localPosition+Vector3.up*3.98f,new Vector3(next.localScale.x,.24f,next.localScale.z),plaster);
            if(index==0)
            {
                foreach(float z in new[]{17f,23f,35.5f})
                {
                    Prop("BackroomsLikeAsset/prefab/Props/Props_Locker.prefab","StudentLocker",new Vector3(right-.48f,y,z),1.75f,-90,.65f);
                    Notice(new Vector3(left+.22f,y+2.3f,z),"GIU YEN LANG",1.65f,true);
                }
            }
            if(index==1)
            {
                foreach(float z in new[]{41f,45f,49f,53f})Prop(Furniture+"Closets/Closet12.prefab","ArchiveCabinet",new Vector3(left+.62f,y,z),2.05f,-90,.85f);
                foreach(Transform t in room)if(t.name.StartsWith("EvidenceTable_"))
                {
                    t.GetComponent<Renderer>().enabled=false;
                    Prop(Furniture+"Tables/Table01.prefab","EvidenceDesk",new Vector3(t.localPosition.x,y,t.localPosition.z),.86f,0,2.1f);
                    Box("ClosedFolder",new Vector3(t.localPosition.x-.65f,y+.89f,t.localPosition.z),new Vector3(.33f,.035f,.4f),paper);
                }
                foreach(Transform t in room)if(t.name.StartsWith("TableSupport_"))t.GetComponent<Renderer>().enabled=false;
                Notice(new Vector3(left+.23f,y+2.8f,c.z),"HO SO / NIEN KHOA 1986",3,true);
            }
            if(index==2)
            {
                // Hai dãy bàn sát biên; vùng bảng điều hướng ở giữa không thêm đồ vật.
                foreach(float x in new[]{left+1.3f,right-1.3f})foreach(float z in new[]{near+5f,near+8.5f,near+12f})
                {
                    Prop(Furniture+"Tables/Table01.prefab","ExamDesk",new Vector3(x,y,z),.78f,0,1.35f);
                    Prop(Furniture+"Chairs/Chair01.prefab","ExamChair",new Vector3(x,y,z-.85f),.9f,0,.6f);
                    Box("ExamPaper",new Vector3(x,y+.8f,z),new Vector3(.28f,.01f,.39f),paper);
                }
                Notice(new Vector3(left+.23f,y+2.5f,c.z),"PHONG THI / GIU TRAT TU",2.6f,true);
                var nav=room.Find("PF_NavigationBoard");foreach(var r in nav.GetComponentsInChildren<MeshRenderer>())if(r.name=="Table"||r.name=="Support")r.sharedMaterial=wood;
            }
            if(index==3)
            {
                foreach(float z in new[]{88f,94f})Prop("BackroomsLikeAsset/prefab/Props/Props_Locker.prefab","SealedLocker",new Vector3(left+.45f,y,z),1.8f,90,.65f);
            }
            if(index==4)
            {
                foreach(float z in new[]{near+8f,near+12f,near+16f})
                {
                    Prop(Furniture+"Closets/Closet12.prefab","FacultyCabinet",new Vector3(left+.65f,y,z),2f,-90,.95f);
                    Prop(Furniture+"Tables/Table01.prefab","FacultyDesk",new Vector3(right-1.25f,y,z),.8f,-90,1.5f);
                    Prop(Furniture+"Chairs/Chair01.prefab","FacultyChair",new Vector3(right-2.25f,y,z),.95f,90,.65f);
                    Box("FacultyFolder",new Vector3(right-1.25f,y+.82f,z),new Vector3(.34f,.04f,.45f),paper);
                }
                Notice(new Vector3(left+.23f,y+2.8f,c.z),"HOI DONG GIAO VIEN",3,true);
                foreach(Transform station in room)if(station.name.StartsWith("TeacherChannel_"))
                {
                    // Giữ hình nhân và các trạng thái tương tác, đồng bộ vật liệu với nội thất gỗ.
                    var pedestal=station.Find("Pedestal");if(pedestal)pedestal.GetComponent<Renderer>().sharedMaterial=wood;
                    var head=station.Find("Head");if(head)head.GetComponent<Renderer>().sharedMaterial=plaster;
                }
            }
            if(w>10)
            {
                var spot=new GameObject("RoomShadowLight",typeof(Light)).GetComponent<Light>();spot.transform.SetParent(root,false);
                spot.transform.localPosition=new Vector3(c.x,y+3.35f,c.z);spot.transform.localRotation=Quaternion.Euler(90,0,0);spot.type=LightType.Spot;spot.range=10;spot.spotAngle=120;spot.intensity=6;spot.color=new Color(.8f,.87f,1);spot.shadows=LightShadows.Soft;spot.shadowBias=.025f;
            }
        }

        /// <summary>Nhận đường dẫn, chân, cao, hướng và độ rộng tối đa; trả prefab chuẩn tỷ lệ trong hệ tọa độ phòng.</summary>
        static GameObject Prop(string path,string name,Vector3 p,float height,float yaw,float maxWidth)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/"+path);if(!prefab)throw new InvalidOperationException(path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.name=name;go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;
            var b=BoundsLocal(go);float scale=Mathf.Min(height/b.size.y,maxWidth/b.size.x);go.transform.localScale*=scale;
            go.transform.localRotation=Quaternion.Euler(0,yaw,0);b=BoundsLocal(go);go.transform.localPosition+=p-new Vector3(b.center.x,b.min.y,b.center.z);
            foreach(var collider in go.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(collider);
            foreach(var r in go.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=MaterialFor(mats[i]);r.sharedMaterials=mats;}
            // Collider gọn theo bounds mới, có thể kiểm tra vật cản mà không sửa asset gốc.
            b=BoundsLocal(go);var proxy=new GameObject(name+"_Collision");proxy.transform.SetParent(root,false);proxy.transform.localPosition=b.center;
            proxy.AddComponent<BoxCollider>().size=b.size;
            return go;
        }

        /// <summary>Nhận instance; trả bounds local từ đỉnh bounds mesh, tránh sai kích thước khi phòng xoay.</summary>
        static Bounds BoundsLocal(GameObject go)
        {
            bool first=true;var result=new Bounds();
            foreach(var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var b=filter.sharedMesh.bounds;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                {
                    var p=root.InverseTransformPoint(filter.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));
                    if(first){result=new Bounds(p,Vector3.zero);first=false;}else result.Encapsulate(p);
                }
            }
            return result;
        }

        /// <summary>Nhận material nguồn; trả bản URP riêng giữ màu, albedo và normal của asset.</summary>
        static Material MaterialFor(Material source)
        {
            if(!source)return wood;if(converted.TryGetValue(source,out var m))return m;
            string path=Output+source.name+".mat";m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):Color.white);
            foreach(var name in new[]{"_BaseMap","_MainTex","_ColorTexture"})if(source.HasProperty(name)&&source.GetTexture(name)){m.SetTexture("_BaseMap",source.GetTexture(name));break;}
            foreach(var name in new[]{"_BumpMap","_Normal"})if(source.HasProperty(name)&&source.GetTexture(name)){m.SetTexture("_BumpMap",source.GetTexture(name));m.EnableKeyword("_NORMALMAP");break;}
            m.SetFloat("_Smoothness",.2f);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);converted[source]=m;return m;
        }

        /// <summary>Nhận tên; trả vật liệu dùng chung từ lớp mẫu.</summary>
        static Material Mat(string name){return AssetDatabase.LoadAssetAtPath<Material>("Assets/TheReturn/Art/Interiors/Classroom/"+name+".mat");}

        /// <summary>Nhận tên, pose local và material; trả module trang trí không có collider.</summary>
        static GameObject Box(string name,Vector3 p,Vector3 size,Material m)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }

        /// <summary>Nhận tâm trên tường phải và bề rộng; dựng kính áp tường cùng khung và bậu.</summary>
        static void Window(Vector3 p,float width)
        {
            Box("WindowFrame",p,new Vector3(.1f,1.8f,width),wood);
            Box("FrostedPane",p+Vector3.left*.065f,new Vector3(.025f,1.64f,width-.15f),glass);
            Box("WindowMullion",p+Vector3.left*.085f,new Vector3(.04f,1.7f,.055f),wood);
            Box("WindowTransom",p+new Vector3(-.085f,.25f,0),new Vector3(.04f,.055f,width),wood);
            Box("WindowSill",p+Vector3.down*.94f,new Vector3(.32f,.1f,width+.15f),plaster);
        }

        /// <summary>Nhận tâm, nội dung và hướng tường; dựng bảng tin không liên quan manh mối.</summary>
        static void Notice(Vector3 p,string title,float width,bool left=false)
        {
            float inward=left?1:-1;
            Box("NoticeFrame",p,new Vector3(.08f,.9f,width),wood);
            for(int i=0;i<3;i++)Box("PinnedPaper",p+new Vector3(inward*.06f,0,(i-1)*width*.3f),new Vector3(.015f,.64f,width*.23f),paper);
            Label(title,p+new Vector3(inward*.08f,.61f,0),Quaternion.Euler(0,left?-90:90,0),.022f);
        }

        /// <summary>Nhận nội dung, pose local và cỡ chữ; tạo TextMesh dùng shader depth test có sẵn.</summary>
        static void Label(string text,Vector3 p,Quaternion q,float size)
        {
            var tm=new GameObject("RoomSign",typeof(TextMesh)).GetComponent<TextMesh>();tm.transform.SetParent(root,false);tm.transform.localPosition=p;tm.transform.localRotation=q;tm.text=text;tm.fontSize=64;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(.82f,.8f,.67f);
            tm.GetComponent<Renderer>().sharedMaterial=Mat("WorldFont");
        }

        /// <summary>Nhận vị trí và chủ đề; tạo đèn trần, phòng hạn chế dùng ánh sáng yếu hơn.</summary>
        static void Lamp(Vector3 p,int index)
        {
            Box("LampHousing",p,new Vector3(1.5f,.12f,.35f),wood);Box("LampDiffuser",p+Vector3.down*.075f,new Vector3(1.38f,.03f,.27f),lamp);
            var l=new GameObject("RoomLight",typeof(Light)).GetComponent<Light>();l.transform.SetParent(root,false);l.transform.localPosition=p+Vector3.down*.65f;l.type=LightType.Point;l.range=7;l.intensity=index==3?1.1f:1.9f;l.color=index==3?new Color(.65f,.76f,.85f):new Color(1,.88f,.7f);
        }

        /// <summary>Không nhận đầu vào; trả số checkpoint chồng collider mới, không tác động runtime.</summary>
        public static string Validate()
        {
            int points=0,blocked=0;
            foreach(string name in Rooms){var room=GameObject.Find(name).transform;foreach(Transform t in room)if(t.name.StartsWith("Checkpoint_"))
            {points++;foreach(var c in Physics.OverlapCapsule(t.position+Vector3.up*.5f,t.position+Vector3.up*1.5f,.3f,~0,QueryTriggerInteraction.Ignore))if(c.transform.parent!=null&&c.transform.parent.name=="SchoolRoom_Environment")blocked++;}}
            return "Mission interiors: "+points+" checkpoints, new obstruction overlaps="+blocked;
        }
    }
}
