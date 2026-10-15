/*
 * Mục đích: hoàn thiện lớp điểm danh và hành lang mẫu bằng asset ThirdParty.
 * Build dựng khu mẫu và lưu scene; Shell dựng vỏ phòng; Corridor dựng hành lang;
 * Furniture thay hình ảnh 24 bàn ghế, giữ nguyên logic; Prop đặt prefab theo bounds;
 * Box dựng module kiến trúc; Mat tạo vật liệu riêng; Imported sửa vật liệu trên bản sao;
 * Label tạo chữ; Lamp tạo đèn; BoundsOf đo renderer; Window dựng cửa sổ; Notice dựng bảng tin.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheReturn.Art.Editor
{
    public static class ClassroomEnvironmentPass
    {
        const string Output = "Assets/TheReturn/Art/Interiors/Classroom/";
        static Transform root;
        static Material plaster, green, trim, floor, glass, light, board, paper;
        static readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();

        /// <summary>Không có đầu vào; dựng lớp học mẫu trong scene nhiệm vụ, lưu thay đổi và asset.</summary>
        [MenuItem("The Return/Art/Build Classroom And Gallery")]
        public static void Build()
        {
            if (Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SchoolFloorPrototype")
                throw new InvalidOperationException("Open SchoolFloorPrototype in Edit Mode.");
            System.IO.Directory.CreateDirectory(Output);
            AssetDatabase.Refresh();
            materials.Clear();
            var old = GameObject.Find("Classroom_Environment");
            if (old) UnityEngine.Object.DestroyImmediate(old);
            root = new GameObject("Classroom_Environment").transform;
            plaster = Mat("IvoryPlaster", new Color(.72f,.69f,.59f));
            green = Mat("SchoolSage", new Color(.16f,.25f,.22f));
            trim = Mat("DarkTimber", new Color(.14f,.09f,.05f));
            floor = Mat("Terrazzo", new Color(.49f,.49f,.43f));
            var source = AssetDatabase.LoadAssetAtPath<Material>("Assets/ThirdParty/YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_Polished.mat");
            if(source){var tex=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.mainTexture;floor.SetTexture("_BaseMap",tex);floor.SetTextureScale("_BaseMap",new Vector2(7,11));}
            floor.SetFloat("_Smoothness",.23f);
            glass = Mat("WindowDaylight", new Color(.34f,.46f,.48f));
            glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.19f,.29f,.32f));
            light = Mat("Fluorescent",new Color(.94f,.89f,.72f));
            light.EnableKeyword("_EMISSION");light.SetColor("_EmissionColor",new Color(2.5f,2.2f,1.6f));
            board = Mat("Blackboard",new Color(.035f,.085f,.067f));
            paper = Mat("NoticePaper",new Color(.8f,.76f,.61f));
            var room=GameObject.Find("Environment_LogicOnly").transform;
            var previous=room.Find("Interior_Decoration");if(previous)previous.gameObject.SetActive(false);
            foreach(var name in new[]{"MonumentalSchool_ArtOnly","Observatory_Decoration"}){var go=GameObject.Find(name);if(go)go.SetActive(false);}
            room.Find("Wall_Left").gameObject.SetActive(false);
            foreach(var r in room.GetComponentsInChildren<Renderer>())r.sharedMaterial=r.name=="Floor"?floor:plaster;
            Shell();Furniture();Corridor();
            // TextMesh mặc định xuyên hình trong URP; dùng shader chữ world đã có của dự án.
            foreach(var text in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {
                var renderer=text.GetComponent<Renderer>();
                if(renderer.sharedMaterial.shader.name!="GUI/Text Shader")continue;
                var original=renderer.sharedMaterial;var path=Output+"WorldFont.mat";
                var replacement=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!replacement){replacement=new Material(Shader.Find("TheReturn/WorldText"));replacement.mainTexture=original.mainTexture;AssetDatabase.CreateAsset(replacement,path);}
                renderer.sharedMaterial=replacement;
            }
            Physics.SyncTransforms();
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
            Debug.Log("Classroom environment saved: 24 imported desk/chair pairs and covered gallery.");
        }

        /// <summary>Không có đầu vào; tạo trần kín, cửa sổ, chân tường và khu bảng trước lớp.</summary>
        static void Shell()
        {
            Box("Ceiling",new Vector3(0,3.8f,-2.6f),new Vector3(14.3f,.25f,21.4f),plaster);
            Box("WindowLowerWall",new Vector3(-7,.57f,-2.7f),new Vector3(.24f,1.14f,21.6f),green,true);
            Box("WindowUpperWall",new Vector3(-7,3.45f,-2.7f),new Vector3(.24f,.7f,21.6f),plaster,true);
            for(int i=0;i<6;i++)Box("WindowPier",new Vector3(-7,2.1f,-12.7f+i*4),new Vector3(.3f,2.1f,.75f),plaster,true);
            glass.shader=Shader.Find("Universal Render Pipeline/Unlit");glass.SetColor("_BaseColor",new Color(.38f,.53f,.58f));
            for(int i=0;i<5;i++)Window(new Vector3(-6.96f,2.12f,-10.7f+i*4),Quaternion.Euler(0,-90,0),3.2f);
            foreach(int side in new[]{-1,1})
            {
                Box("Dado",new Vector3(side*6.85f,.63f,-2.65f),new Vector3(.07f,1.26f,21.4f),green);
                Box("DadoCap",new Vector3(side*6.8f,1.28f,-2.65f),new Vector3(.1f,.065f,21.4f),trim);
                Box("Skirting",new Vector3(side*6.8f,.085f,-2.65f),new Vector3(.12f,.17f,21.4f),trim);
                Box("Cornice",new Vector3(side*6.76f,3.52f,-2.65f),new Vector3(.22f,.17f,21.4f),plaster);
            }
            for(float z=-11;z<8;z+=4.3f)
            {
                Box("CeilingJoint",new Vector3(0,3.65f,z),new Vector3(13.5f,.035f,.04f),trim);
                foreach(float x in new[]{-3.2f,3.2f})Lamp(new Vector3(x,3.48f,z),new Color(1,.9f,.72f),2.3f);
            }
            Box("BoardFrame",new Vector3(-.5f,2.1f,7.94f),new Vector3(7.5f,2.12f,.10f),trim);
            Box("BoardSurface",new Vector3(-.5f,2.1f,7.86f),new Vector3(7.28f,1.9f,.025f),board);
            Box("ChalkTray",new Vector3(-.5f,1.12f,7.48f),new Vector3(7.5f,.065f,.22f),trim);
            Label("THE RETURN  /  LOP 01",new Vector3(-.5f,3.35f,7.55f),Quaternion.identity,.038f);
            Notice(new Vector3(6.73f,2.25f,-9.5f),Quaternion.Euler(0,90,0));
            foreach(float z in new[]{-5f,0f,5f})Notice(new Vector3(6.73f,2.35f,z),Quaternion.Euler(0,90,0));
            foreach(float z in new[]{-6f,2f})
            {
                var l=new GameObject("ClassroomShadowLight",typeof(Light)).GetComponent<Light>();l.transform.SetParent(root,false);
                l.transform.position=new Vector3(0,3.3f,z);l.transform.rotation=Quaternion.Euler(90,0,0);l.type=LightType.Spot;l.spotAngle=115;l.innerSpotAngle=75;l.range=9;l.intensity=8;l.color=new Color(.72f,.83f,1);l.shadows=LightShadows.Soft;l.shadowBias=.025f;
            }
            Prop("HotelRoom-Mavi3D/Prefabs/URP/FlowerPot.prefab","ClassPlant",new Vector3(-6,0,7),1.45f,0);
            Prop("BackroomsLikeAsset/prefab/Props/Props_TrashCan.prefab","WasteBin",new Vector3(6,0,-12.5f),.65f,0);
        }

        /// <summary>Không có đầu vào; thay mesh mẫu bằng 48 prefab, không đổi ghế tương tác hoặc collider.</summary>
        static void Furniture()
        {
            var puzzle=GameObject.Find("Puzzle_Attendance");
            foreach(var t in puzzle.GetComponentsInChildren<Transform>(true))
            {
                if(t.name!="Visual_Replaceable")continue;
                var desk=t.Find("Desk");var chair=t.Find("ChairSeat");if(!desk||!chair)continue;
                foreach(var r in t.GetComponentsInChildren<Renderer>())r.enabled=false;
                var d=Prop("Furniture Mega Pack/Prefabs/Tables/Table01.prefab","SchoolDesk",new Vector3(desk.position.x,0,desk.position.z),.8f,0);
                Prop("Furniture Mega Pack/Prefabs/Chairs/Chair01.prefab","SchoolChair",new Vector3(chair.position.x,0,chair.position.z),.95f,0);
                Box("ExerciseBook",new Vector3(desk.position.x-.22f,.813f,desk.position.z),new Vector3(.28f,.025f,.36f),paper);
            }
            foreach(var seat in puzzle.GetComponentsInChildren<TheReturn.AttendanceSeat>())
            {
                seat.indicator.transform.localScale=new Vector3(.62f,.26f,.035f);
                var p=seat.indicator.transform.position;p.y=.98f;seat.indicator.transform.position=p;
                p=seat.label.transform.position;p.y=.99f;seat.label.transform.position=p;seat.label.characterSize=.014f;
            }
            for(int i=0;i<5;i++)Prop("BackroomsLikeAsset/prefab/Props/Props_Locker.prefab","StudentLocker",new Vector3(-1.8f+i*.73f,0,-12.95f),1.85f,0);
        }

        /// <summary>Không có đầu vào; dựng hành lang có mái nối cửa lớp vào tuyến hiện hữu, giữ lối ra phía đông.</summary>
        static void Corridor()
        {
            var network=GameObject.Find("Circulation_Network");
            if(network)foreach(var t in network.GetComponentsInChildren<Transform>())
                if((t.name.Contains("Rail")||t.name.Contains("Guide")||t.name.Contains("Pier")||t.name.Contains("Lintel")||t.name=="Wayfinding")&&t.position.x>-7&&t.position.x<21&&t.position.z>8&&t.position.z<16.5f)t.gameObject.SetActive(false);
            Box("GalleryFloor",new Vector3(7,-.08f,12.25f),new Vector3(28,.28f,8.1f),floor,true);
            Box("GalleryRoof",new Vector3(7,4,12.2f),new Vector3(28.5f,.3f,8.4f),plaster);
            Box("GalleryEndWall",new Vector3(-7,1.9f,12.2f),new Vector3(.3f,3.8f,8),plaster,true);
            // Nối mặt tiền, giữ nguyên ô cửa nhiệm vụ hiện có.
            Box("FacadeExtension",new Vector3(14,1.9f,8),new Vector3(14,3.8f,.3f),plaster,true);
            Box("GalleryParapet",new Vector3(7,.5f,16.25f),new Vector3(28,1,.3f),green,true);
            Box("GalleryWindowHeader",new Vector3(7,3.55f,16.25f),new Vector3(28,.8f,.3f),plaster);
            for(float x=-6.6f;x<=21;x+=4.5f)
            {
                Box("GalleryColumn",new Vector3(x,1.95f,16.25f),new Vector3(.45f,3.9f,.45f),plaster,true);
                Box("ColumnBase",new Vector3(x,.16f,16.25f),new Vector3(.65f,.32f,.65f),trim);
                Box("ColumnCapital",new Vector3(x,3.65f,16.25f),new Vector3(.75f,.28f,.65f),plaster);
                Box("GalleryBeam",new Vector3(x,3.72f,12.2f),new Vector3(.25f,.2f,8),plaster);
                Lamp(new Vector3(x+1.8f,3.62f,12),new Color(1,.85f,.62f),2.2f);
            }
            for(int i=0;i<4;i++)Prop("BackroomsLikeAsset/prefab/Props/Props_Locker.prefab","HallLocker",new Vector3(9+i*.74f,0,8.65f),1.9f,180);
            for(int i=0;i<6;i++)Window(new Vector3(-4.35f+i*4.5f,2.15f,16.25f),Quaternion.identity,4.02f);
            for(int i=0;i<3;i++)Prop("Furniture Mega Pack/Prefabs/Chairs/Chair01.prefab","HallWaitingChair",new Vector3(-3+i*.85f,.06f,8.9f),.95f,0);
            Prop("HotelRoom-Mavi3D/Prefabs/URP/FlowerPot.prefab","GalleryPlant",new Vector3(-5.8f,0,15.2f),1.6f,0);
            Prop("HotelRoom-Mavi3D/Prefabs/URP/FlowerPot.prefab","GalleryPlant",new Vector3(19.7f,0,15.2f),1.6f,0);
            Notice(new Vector3(-2,2.1f,8.3f),Quaternion.Euler(0,180,0));
            Label("01  /  PHONG HOC",new Vector3(5.8f,3.3f,8.3f),Quaternion.Euler(0,180,0),.035f);
            Label("KHU LOP HOC   /   A",new Vector3(-6.8f,2.6f,12.2f),Quaternion.Euler(0,-90,0),.045f);
        }

        /// <summary>Nhận tâm, hướng và bề rộng; dựng kính, khung, đố và bậu cửa theo module.</summary>
        static void Window(Vector3 center,Quaternion rotation,float width)
        {
            var group=new GameObject("WindowModule").transform;group.SetParent(root,false);group.position=center;group.rotation=rotation;
            var previous=root;root=group;
            Box("Glass",Vector3.zero,new Vector3(width,1.95f,.04f),glass,true);
            foreach(float x in new[]{-width/2,0,width/2})Box("Mullion",new Vector3(x,0,-.065f),new Vector3(.07f,2.08f,.13f),trim);
            foreach(float y in new[]{-1f,.38f,1f})Box("Transom",new Vector3(0,y,-.065f),new Vector3(width+.1f,.065f,.13f),trim);
            Box("Sill",new Vector3(0,-1.06f,0),new Vector3(width+.28f,.12f,.42f),plaster);
            root=previous;
        }

        /// <summary>Nhận pose; tạo bảng tin với giấy thông báo trang trí, không mang manh mối nhiệm vụ.</summary>
        static void Notice(Vector3 p,Quaternion q)
        {
            var g=new GameObject("NoticeBoardModule").transform;g.SetParent(root,false);g.position=p;g.rotation=q;
            var previous=root;root=g;
            Box("Frame",Vector3.zero,new Vector3(2.8f,1.5f,.12f),trim);
            Box("Cork",new Vector3(0,0,-.075f),new Vector3(2.65f,1.35f,.025f),green);
            for(int i=0;i<6;i++)Box("PinnedNotice",new Vector3(-.95f+i%3*.91f,.33f-i/3*.66f,-.1f),new Vector3(.64f,.51f,.015f),paper);
            Label("THONG BAO / LICH HOC",new Vector3(0,.94f,-.1f),Quaternion.identity,.027f);
            root=previous;
        }

        /// <summary>Nhận đường dẫn, vị trí chân, chiều cao và hướng; trả instance chuẩn tỷ lệ với vật liệu URP riêng.</summary>
        static GameObject Prop(string path,string name,Vector3 position,float height,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/"+path);
            if(!prefab)throw new InvalidOperationException("Missing asset "+path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.name=name;
            go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var bounds=BoundsOf(go);go.transform.localScale*=height/Mathf.Max(bounds.size.y,.001f);
            bounds=BoundsOf(go);go.transform.position+=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var c in go.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                var mm=r.sharedMaterials;for(int i=0;i<mm.Length;i++)mm[i]=Imported(mm[i]);r.sharedMaterials=mm;r.enabled=true;
            }
            return go;
        }

        /// <summary>Nhận material nguồn; trả bản URP opaque riêng, giữ texture màu và normal, không sửa ThirdParty.</summary>
        static Material Imported(Material source)
        {
            if(!source)return plaster;if(materials.TryGetValue(source,out var cached))return cached;
            var m=Mat("Imported_"+source.name,source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):Color.white);
            foreach(var property in new[]{"_BaseMap","_MainTex","_Albedo","_BaseColorMap","_ColorTexture"})if(source.HasProperty(property)&&source.GetTexture(property)){m.SetTexture("_BaseMap",source.GetTexture(property));break;}
            if(source.HasProperty("_BumpMap")&&source.GetTexture("_BumpMap")){m.SetTexture("_BumpMap",source.GetTexture("_BumpMap"));m.EnableKeyword("_NORMALMAP");}
            m.SetFloat("_Cull",0);materials[source]=m;return m;
        }

        /// <summary>Nhận tên và màu; trả material URP lưu trong thư mục của khu mẫu.</summary>
        static Material Mat(string name,Color color)
        {
            string path=Output+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.15f);EditorUtility.SetDirty(m);return m;
        }

        /// <summary>Nhận instance; trả bounds world của renderer.</summary>
        static Bounds BoundsOf(GameObject go)
        {
            var rr=go.GetComponentsInChildren<Renderer>();var b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);return b;
        }

        /// <summary>Nhận tên, pose, material và yêu cầu va chạm; trả module khối.</summary>
        static GameObject Box(string name,Vector3 p,Vector3 size,Material m,bool solid=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=m;
            if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }

        /// <summary>Nhận vị trí, màu và cường độ; tạo đèn trần huỳnh quang và nguồn sáng.</summary>
        static void Lamp(Vector3 p,Color color,float intensity)
        {
            Box("LightHousing",p,new Vector3(1.65f,.12f,.4f),trim);Box("Diffuser",p+Vector3.down*.075f,new Vector3(1.5f,.035f,.3f),light);
            var l=new GameObject("CeilingLight",typeof(Light)).GetComponent<Light>();l.transform.SetParent(root,false);l.transform.position=p+Vector3.down*.7f;l.type=LightType.Point;l.range=7;l.intensity=intensity*.6f;l.color=color;l.shadows=LightShadows.None;
        }

        /// <summary>Nhận nội dung, pose và cỡ chữ; tạo nhãn trang trí.</summary>
        static void Label(string text,Vector3 p,Quaternion q,float size)
        {
            var t=new GameObject("Sign",typeof(TextMesh)).GetComponent<TextMesh>();t.transform.SetParent(root,false);t.transform.localPosition=p;t.transform.localRotation=q;t.text=text;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.85f,.81f,.66f);
        }
    }
}
