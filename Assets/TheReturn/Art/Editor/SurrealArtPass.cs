/*
 * Mục đích: Trang trí sảnh và tầng học bằng bộ kiến trúc thiên văn tái sử dụng.
 * Hàm: Apply trang trí scene hiện tại; Lobby dựng sảnh; School dựng thiên văn đài;
 * Clock dựng mặt đồng hồ; Ring tạo vòng mesh; Box tạo chi tiết; Prop đặt asset;
 * Label tạo chữ; LightAt tạo đèn; Finish cài hậu kỳ; Material tạo vật liệu;
 * ClearSightlines mở hướng nhìn; Beam tạo dầm nối hai điểm;
 * MeshRing tạo mesh vòng dùng chung; Folder bảo đảm thư mục lưu asset tồn tại.
 * Mọi chi tiết mới không có collider; không thay đổi dữ liệu câu đố/checkpoint.
 */
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheReturn.Art.Editor
{
    public static class SurrealArtPass
    {
        const string Path = "Assets/TheReturn/Art/Environment/";
        static Transform root;
        static Material stone, dark, brass, ice, gold, paper;

        /// <summary>Không có đầu vào; thay lớp trang trí cũ, dựng theo scene hiện tại và lưu scene/assets.</summary>
        [MenuItem("The Return/Art/Apply Observatory Decoration")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Dừng Play Mode trước khi trang trí.");
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene != "Sanh_Cho" && scene != "SchoolFloorPrototype") throw new InvalidOperationException("Mở sảnh hoặc tầng học.");
            Folder(Path.TrimEnd('/'));
            var old = GameObject.Find("Observatory_Decoration");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            root = new GameObject("Observatory_Decoration").transform;
            stone = Material("Limestone", new Color(.32f,.35f,.37f), Color.black, .08f);
            dark = Material("Obsidian", new Color(.025f,.045f,.055f), Color.black, .35f);
            brass = Material("AgedBrass", new Color(.48f,.28f,.09f), Color.black, .65f);
            ice = Material("AstralCyan", new Color(.06f,.5f,.65f), new Color(.12f,2.4f,3.4f), .1f);
            gold = Material("AstralGold", new Color(.7f,.35f,.08f), new Color(2.8f,1.1f,.22f), .15f);
            paper = Material("Paper", new Color(.66f,.58f,.39f), Color.black, 0);
            // Các lớp phủ cũ có mặt đồng phẳng và cột giữa phòng che manh mối.
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name.StartsWith("SurfaceSkin_") || t.name.StartsWith("ReadyPedestalSkin_") ||
                    t.name.StartsWith("MissionBeacon_") || t.name.StartsWith("IslandRail_") ||
                    t.name == "ClockCore" || t.name == "LobbyPortalFog") t.gameObject.SetActive(false);
            ClearSightlines(scene);
            if (scene == "Sanh_Cho") Lobby(); else School();
            Finish(scene);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Nhận tên scene; ẩn chi tiết cũ chắn tiêu điểm, chỉ tác động root cảnh quan.</summary>
        static void ClearSightlines(string scene)
        {
            var art=GameObject.Find(scene=="Sanh_Cho"?"Lobby_Architecture_Replaceable":"MonumentalSchool_ArtOnly");
            if(art==null)return;
            foreach(Transform t in art.transform)
            {
                bool hide=scene=="Sanh_Cho"?(t.name.StartsWith("CeilingRib")||t.name.StartsWith("FloatingSlab")||t.name.StartsWith("PortalHalo_")):
                    (t.name.StartsWith("FloatingDebris")||(t.name.StartsWith("CathedralPillar_")&&Mathf.Abs(t.position.x)<7&&t.position.z<23));
                if(hide)t.gameObject.SetActive(false);
            }
        }

        /// <summary>Nhận hai đầu mút và độ dày; tạo dầm trang trí định hướng giữa hai điểm.</summary>
        static void Beam(string name,Vector3 a,Vector3 b,float width,Material material)
        {
            var go=Box(name,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),material);
            go.transform.rotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }

        /// <summary>Không có đầu vào; dựng sàn ô, khung tường, portal thiên văn và các góc trường học.</summary>
        static void Lobby()
        {
            for (int x=-7;x<=7;x++) for (int z=-6;z<=6;z++)
                Box("InlaidFloor",new Vector3(x*2.1f,.009f,z*2.1f),new Vector3(2.06f,.015f,2.06f),(x+z)%2==0?stone:dark);
            for (int side=-1;side<=1;side+=2) for (int i=0;i<5;i++)
            {
                float z=-10+i*5.2f;
                Box("ColumnPlinth",new Vector3(side*12.5f,.28f,z),new Vector3(2.2f,.56f,2.2f),dark);
                Box("ColumnCapital",new Vector3(side*12.5f,8.9f,z),new Vector3(2.2f,.32f,2.2f),brass);
                Box("WallRecess",new Vector3(side*15.85f,4.7f,z),new Vector3(.08f,6.7f,3.8f),stone);
                Box("WallLight",new Vector3(side*15.73f,4.7f,z),new Vector3(.1f,4.6f,.08f),gold);
                Box("WallCornice",new Vector3(side*15.6f,8.3f,z),new Vector3(.35f,.2f,4.7f),brass);
                if (i%2==0) LightAt("AmberSconce",new Vector3(side*13.5f,4,z),new Color(1,.55f,.2f),2.4f,7);
            }
            Ring("PortalOrbit",new Vector3(0,5.2f,12.15f),Quaternion.identity,5.35f,.12f,gold);
            Ring("PortalInnerOrbit",new Vector3(0,5.2f,12.05f),Quaternion.identity,4.95f,.055f,ice);
            Clock(new Vector3(0,5.2f,11.9f),4.6f,false);
            Label("SchoolTitle","THE RETURN",new Vector3(0,10.9f,12),.16f,new Color(1,.76f,.39f));
            Label("Subtitle","A C A D E M Y   /   0 0 : 0 0",new Vector3(0,10.2f,12),.045f,Color.white);
            for(int i=0;i<4;i++)
            {
                Vector3 p=new Vector3(-7.5f+i*5,.79f,3.5f);
                Ring("ReadyHalo",p,Quaternion.Euler(90,0,0),.87f,.035f,gold);
                Label("SlotNumeral",new[]{"I","II","III","IV"}[i],p+new Vector3(0,.55f,0),.1f,new Color(1,.75f,.3f));
            }
            for (int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<5;i++) Prop("Props_Locker",new Vector3(side*14.6f,0,6.5f-i*1.1f),side<0?90:-90);
                Prop("Props_Table2",new Vector3(side*10.7f,0,-8.5f),0);
                Prop("Props_Chair",new Vector3(side*10.7f,0,-9.5f),180);
                Prop("Props_TrashCan",new Vector3(side*14.2f,0,-8),0);
                for(int i=0;i<8;i++) Box("LostExamPaper",new Vector3(side*(9.6f+i*.17f),1.08f+i*.014f,-8.4f),new Vector3(.25f,.012f,.34f),paper,new Vector3(0,i*17,0));
            }
            Ring("CeilingOculus",new Vector3(0,12.7f,0),Quaternion.Euler(90,0,0),6.8f,.18f,brass);
            Ring("CeilingLight",new Vector3(0,12.6f,0),Quaternion.Euler(90,0,0),6.45f,.04f,ice);
            for(int i=0;i<5;i++)
            {
                float z=-10+i*5.2f;
                Beam("VaultRib",new Vector3(-12.5f,9,z),new Vector3(0,15,z),.3f,stone);
                Beam("VaultRib",new Vector3(12.5f,9,z),new Vector3(0,15,z),.3f,stone);
                Beam("VaultGoldEdge",new Vector3(-12.4f,8.8f,z),new Vector3(0,14.8f,z),.035f,gold);
                Beam("VaultGoldEdge",new Vector3(12.4f,8.8f,z),new Vector3(0,14.8f,z),.035f,gold);
            }
        }

        /// <summary>Không có đầu vào; dựng đồng hồ lớn, các vòng quỹ đạo, cột đá phân tầng quanh vực.</summary>
        static void School()
        {
            Vector3 center=new Vector3(0,14,31);
            Clock(center,8,true);
            Ring("OrbitalArmillaryA",center,Quaternion.Euler(65,25,0),10.7f,.13f,brass);
            Ring("OrbitalArmillaryB",center,Quaternion.Euler(-35,-40,20),11.8f,.075f,ice);
            Ring("OrbitalArmillaryC",center,Quaternion.Euler(15,60,-20),12.9f,.09f,gold);
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI/8;
                Vector3 p=new Vector3(Mathf.Sin(a)*17,0,31+Mathf.Cos(a)*17);
                if(Mathf.Abs(p.x)<7&&p.z<23)continue;
                for(int level=0;level<4;level++)
                {
                    Box("PierCollar",p+Vector3.up*(1+level*5.3f),new Vector3(2.15f,.38f,2.15f),brass,new Vector3(0,i*22.5f,0));
                    Box("PierStone",p+Vector3.up*(3+level*5.3f),new Vector3(1.5f,3.6f,1.5f),stone,new Vector3(0,i*22.5f,0));
                }
                Box("HangingPendulum",p*.82f+new Vector3(0,25,5.58f),new Vector3(.035f,13,.035f),gold);
            }
            for(int level=0;level<3;level++)
                Ring("FoundationOrbit",new Vector3(0,-1+level*.4f,31),Quaternion.Euler(90,0,0),8+level*2,.09f,level==1?gold:brass);
            LightAt("ClockWarmFill",new Vector3(0,14,24),new Color(1,.6f,.22f),5,22);
            LightAt("ClockColdRim",new Vector3(0,18,36),new Color(.15f,.6f,1),4,20);
            Label("CentralInscription","TEMPUS  /  FRACTUM",new Vector3(0,25.5f,30),.14f,new Color(1,.76f,.4f));
        }

        /// <summary>Nhận tâm, bán kính, cờ kim; tạo mặt số và các vòng đồng tâm, không trả dữ liệu.</summary>
        static void Clock(Vector3 center,float radius,bool hands)
        {
            Ring("ClockStoneRim",center,Quaternion.identity,radius,.24f,brass);
            Ring("ClockLightRim",center-new Vector3(0,0,.08f),Quaternion.identity,radius-.35f,.055f,gold);
            for(int i=0;i<60;i++)
            {
                float a=i*Mathf.PI/30;bool major=i%5==0;
                Box("ClockTick",center+new Vector3(Mathf.Sin(a)*(radius-.7f),Mathf.Cos(a)*(radius-.7f),-.14f),new Vector3(major?.11f:.04f,major?.55f:.2f,.08f),major?gold:brass,new Vector3(0,0,-i*6));
                if(major) Label("ClockNumber",new[]{"XII","I","II","III","IV","V","VI","VII","VIII","IX","X","XI"}[i/5],center+new Vector3(Mathf.Sin(a)*(radius-1.45f),Mathf.Cos(a)*(radius-1.45f),-.2f),radius*.018f,new Color(1,.76f,.4f));
            }
            if(hands)
            {
                Box("MinuteHand",center+new Vector3(0,radius*.32f,-.3f),new Vector3(.12f,radius*.64f,.12f),ice);
                Box("HourHand",center+new Vector3(-radius*.18f,radius*.18f,-.35f),new Vector3(.2f,radius*.5f,.14f),gold,new Vector3(0,0,45));
                Ring("ClockHub",center-new Vector3(0,0,.4f),Quaternion.identity,.5f,.14f,gold);
            }
        }

        /// <summary>Nhận tên, tâm, hướng, bán kính và vật liệu; tạo vòng mesh dùng lại, không collider.</summary>
        static void Ring(string name,Vector3 position,Quaternion rotation,float radius,float width,Material material)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);
            go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=Vector3.one*radius;
            go.GetComponent<MeshFilter>().sharedMesh=MeshRing(width/radius);
            go.GetComponent<MeshRenderer>().sharedMaterial=material;
        }

        /// <summary>Nhận tỷ lệ độ dày; trả mesh vành khăn hai mặt được lưu ổn định trong Assets.</summary>
        static Mesh MeshRing(float thickness)
        {
            string path=Path+"Ring_"+Mathf.RoundToInt(thickness*10000)+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null)return existing;
            const int n=128;var v=new Vector3[n*2];var uv=new Vector2[n*2];var triangles=new int[n*12];
            for(int i=0;i<n;i++)
            {
                float a=i*2*Mathf.PI/n;Vector3 radial=new Vector3(Mathf.Sin(a),Mathf.Cos(a),0);
                v[i*2]=radial*(1-thickness);v[i*2+1]=radial;
                uv[i*2]=new Vector2(i/(float)n,0);uv[i*2+1]=new Vector2(i/(float)n,1);
                int a0=i*2,b0=i*2+1,c0=((i+1)%n)*2,d0=c0+1,k=i*12;
                int[] face={a0,b0,c0,b0,d0,c0,c0,b0,a0,c0,d0,b0};Array.Copy(face,0,triangles,k,12);
            }
            var normals=new Vector3[v.Length];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.back;
            var mesh=new Mesh{name="ObservatoryRing",vertices=v,uv=uv,triangles=triangles,normals=normals};mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }

        /// <summary>Nhận hình học và vật liệu; trả cube trang trí không collider.</summary>
        static GameObject Box(string name,Vector3 position,Vector3 scale,Material material,Vector3 euler=default)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.SetPositionAndRotation(position,Quaternion.Euler(euler));go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }

        /// <summary>Nhận tên prefab đã nhập, vị trí và yaw; đặt bản trang trí, loại collider ở instance.</summary>
        static void Prop(string name,Vector3 position,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/BackroomsLikeAsset/prefab/Props/"+name+".prefab");if(prefab==null)return;
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            foreach(var c in go.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
        }

        /// <summary>Nhận nội dung và pose; tạo chữ 3D dùng font material gốc để giữ đúng nét chữ.</summary>
        static void Label(string name,string value,Vector3 position,float size,Color color)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=position;
            var text=go.AddComponent<TextMesh>();text.text=value;text.fontSize=96;text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.color=color;
        }

        /// <summary>Nhận tên, vị trí, màu, cường độ, bán kính; tạo đèn không bóng để hạn chế chi phí GPU.</summary>
        static void LightAt(string name,Vector3 position,Color color,float intensity,float range)
        {
            var light=new GameObject(name,typeof(Light)).GetComponent<Light>();light.transform.SetParent(root);light.transform.position=position;light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;light.shadows=LightShadows.None;
        }

        /// <summary>Nhận tên scene; tạo profile hậu kỳ riêng và bật nó trên camera của người chơi.</summary>
        static void Finish(string scene)
        {
            string path=Path+scene+"_Look.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            Bloom bloom;if(!profile.TryGet(out bloom)){bloom=profile.Add<Bloom>(true);AssetDatabase.AddObjectToAsset(bloom,profile);}
            bloom.intensity.Override(.35f);bloom.threshold.Override(1.1f);bloom.scatter.Override(.65f);
            Tonemapping tone;if(!profile.TryGet(out tone)){tone=profile.Add<Tonemapping>(true);AssetDatabase.AddObjectToAsset(tone,profile);}tone.mode.Override(TonemappingMode.ACES);
            ColorAdjustments color;if(!profile.TryGet(out color)){color=profile.Add<ColorAdjustments>(true);AssetDatabase.AddObjectToAsset(color,profile);}color.postExposure.Override(.35f);color.contrast.Override(12);color.saturation.Override(-8);
            Vignette vignette;if(!profile.TryGet(out vignette)){vignette=profile.Add<Vignette>(true);AssetDatabase.AddObjectToAsset(vignette,profile);}vignette.intensity.Override(.18f);vignette.smoothness.Override(.6f);
            var volume=new GameObject("ObservatoryColorGrade",typeof(Volume)).GetComponent<Volume>();volume.transform.SetParent(root);volume.isGlobal=true;volume.priority=20;volume.sharedProfile=profile;
            foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)){camera.allowHDR=true;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
            EditorUtility.SetDirty(profile);
        }

        /// <summary>Nhận tên, màu, emission, độ kim loại; trả material URP dùng chung, lưu thành asset.</summary>
        static Material Material(string name,Color color,Color emission,float metallic)
        {
            string path=Path+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.SetColor("_BaseColor",color);material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",.38f);
            if(name=="Limestone")
            {
                var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/ThirdParty/YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_SciFi.mat");
                if(source!=null){material.SetTexture("_BaseMap",source.GetTexture("_BaseMap"));material.SetTexture("_BumpMap",source.GetTexture("_BumpMap"));material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.45f);}
            }
            material.enableInstancing=true;material.SetColor("_EmissionColor",emission);if(emission.maxColorComponent>0)material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);return material;
        }

        /// <summary>Nhận đường dẫn Assets; tạo các thư mục thiếu, không trả dữ liệu.</summary>
        static void Folder(string path)
        {
            string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}
        }
    }
}
