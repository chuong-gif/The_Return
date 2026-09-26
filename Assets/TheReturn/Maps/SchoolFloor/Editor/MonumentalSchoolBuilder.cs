/*
 * Mục đích: Đổi tầng học tuyến tính thành sáu đảo nhiệm vụ xoắn quanh đại sảnh phi thực, giữ nguyên logic bên trong từng phòng.
 * Danh sách hàm:
 * - Build: kiểm tra scene, đặt lại các root nhiệm vụ, dựng art và lưu scene.
 * - ArrangeMissionIslands: xoay/đặt từng phòng theo vòng đứng và giữ vùng tập hợp nhiệm vụ 1 ở cửa lớp.
 * - BuildAtrium: tạo vực trung tâm, cột, vòng cầu, đồng hồ và các khối lơ lửng.
 * - SetRootCenter: nhận root, tâm nguồn, tâm đích và góc; tính transform không làm biến dạng phòng.
 * - Block / Cylinder / Label: tạo module hình học và biển chỉ dẫn có thể thay model.
 * - MakeMaterial: tạo hoặc cập nhật material URP dùng chung cho môi trường.
 */
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn.Editor
{
    public static class MonumentalSchoolBuilder
    {
        const string ScenePath="Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity";
        const string MatFolder="Assets/TheReturn/Maps/SchoolFloor/Environment/Materials/";
        static Transform artRoot;
        static Material concrete,voidStone,cyan,amber,magenta;

        /// <summary>Không nhận đầu vào; sắp lại sáu phòng, dựng đại sảnh và lưu scene tầng học.</summary>
        [MenuItem("The Return/School Floor/Build Monumental Atrium")]
        public static void Build()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Hãy dừng Play Mode trước khi dựng map.");
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            SchoolFloorFlow flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if(flow==null||flow.teacherTruth==null)throw new InvalidOperationException("Tầng học cần đủ sáu nhiệm vụ trước khi đổi bố cục.");
            GameObject previous=GameObject.Find("MonumentalSchool_ArtOnly"); if(previous!=null)UnityEngine.Object.DestroyImmediate(previous);
            EnsureFolder("Assets/TheReturn/Maps/SchoolFloor/Environment/Materials");
            concrete=MakeMaterial("M_Atrium_Concrete",new Color(.07f,.085f,.11f),Color.black,.12f,.28f);
            voidStone=MakeMaterial("M_Atrium_Void",new Color(.009f,.013f,.025f),Color.black,.65f,.2f);
            cyan=MakeMaterial("M_Atrium_Cyan",new Color(.02f,.24f,.28f),new Color(0f,3.5f,4.2f),.3f,.62f);
            amber=MakeMaterial("M_Atrium_Amber",new Color(.3f,.17f,.035f),new Color(4f,1.25f,.08f),.25f,.5f);
            magenta=MakeMaterial("M_Atrium_Magenta",new Color(.24f,.025f,.19f),new Color(3.2f,.05f,2.2f),.22f,.48f);
            ArrangeMissionIslands(flow); BuildAtrium(); ApplyThirdPartyArt(); ConfigureAtmosphere();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets();
            Selection.activeGameObject=artRoot.gameObject;
        }

        /// <summary>Nhận flow; đặt sáu phòng theo vòng đứng, checkpoint đi cùng root và vùng tập hợp ở lại cửa lớp.</summary>
        static void ArrangeMissionIslands(SchoolFloorFlow flow)
        {
            Transform attendance=flow.attendance.transform;
            Transform environment=GameObject.Find("Environment_LogicOnly").transform;
            Transform party=flow.attendance.party.transform;
            attendance.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            environment.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            party.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            if(flow.stagingZone!=null)
            {
                flow.stagingZone.transform.SetParent(attendance,true);
                flow.stagingZone.transform.position=new Vector3(5.8f,1.5f,11.1f);
                flow.stagingZone.size=new Vector3(5.2f,3.5f,3.2f);
            }
            SetRootCenter(flow.corridor.transform,new Vector3(.55f,1.6f,23.7f),new Vector3(39,3,5),55);
            SetRootCenter(flow.gradeRepair.transform,new Vector3(5.8f,1.6f,49.45f),new Vector3(39,7,42),125);
            SetRootCenter(flow.navigationExam.transform,new Vector3(5.8f,1.6f,71.5f),new Vector3(0,11,66),180);
            SetRootCenter(flow.dontLookBack.transform,new Vector3(5.8f,1.6f,94),new Vector3(-42,15,40),235);
            SetRootCenter(flow.teacherTruth.transform,new Vector3(5.8f,1.6f,115.5f),new Vector3(-39,19,2),305);
        }

        /// <summary>Không nhận đầu vào; tạo đại sảnh cao, các vòng cầu, cột, đồng hồ và đường nhìn nối đảo nhiệm vụ.</summary>
        static void BuildAtrium()
        {
            artRoot=new GameObject("MonumentalSchool_ArtOnly").transform;
            Block("ArrivalBridge",new Vector3(0,-.15f,13),new Vector3(7,.3f,12),concrete,Vector3.zero);
            Cylinder("AbyssRim",new Vector3(0,-1.2f,31),new Vector3(18,.8f,18),voidStone);
            Cylinder("CentralDais",new Vector3(0,.2f,31),new Vector3(7,.35f,7),concrete);
            Cylinder("ClockCore",new Vector3(0,13,31),new Vector3(3.4f,.55f,3.4f),cyan,Quaternion.Euler(90,0,0));
            for(int i=0;i<16;i++)
            {
                float angle=i*22.5f; float radians=angle*Mathf.Deg2Rad;
                Vector3 radial=new Vector3(Mathf.Sin(radians),0,Mathf.Cos(radians));
                Vector3 position=new Vector3(0,7.5f,31)+radial*17f;
                Cylinder("CathedralPillar_"+(i+1),position,new Vector3(1.25f,10.5f,1.25f),i%4==0?cyan:concrete);
                Block("FlyingButtress_"+(i+1),position+radial*3.5f+Vector3.up*7f,new Vector3(1.1f,.45f,8f),concrete,new Vector3(0,angle,25));
            }
            for(int level=0;level<5;level++)
            {
                float y=2+level*4f; float radius=18+level*2.5f;
                for(int i=0;i<12;i++)
                {
                    float angle=i*30f+level*11f; float radians=angle*Mathf.Deg2Rad;
                    Vector3 p=new Vector3(Mathf.Sin(radians)*radius,y,31+Mathf.Cos(radians)*radius);
                    Block("BrokenRing_L"+level,p,new Vector3(7,.28f,2.2f),level%2==0?concrete:voidStone,new Vector3(level*3,angle,level%2==0?5:-7));
                }
            }
            Vector3[] centers={new Vector3(0,0,-.5f),new Vector3(39,3,5),new Vector3(39,7,42),new Vector3(0,11,66),new Vector3(-42,15,40),new Vector3(-39,19,2)};
            string[] names={"I  ĐIỂM DANH","II  GIỮ TRẬT TỰ","III  HỒ SƠ ĐIỂM","IV  BÀI KIỂM TRA","V  KHÔNG QUAY ĐẦU","VI  HỘI ĐỒNG GIÁO VIÊN"};
            Material[] colors={amber,cyan,amber,cyan,magenta,magenta};
            for(int i=0;i<centers.Length;i++)
            {
                Vector3 toward=(new Vector3(0,centers[i].y,31)-centers[i]).normalized;
                Block("MissionBeacon_"+(i+1),centers[i]+Vector3.up*5f,new Vector3(.7f,10f,.7f),colors[i],Vector3.zero);
                Label("MissionLabel_"+(i+1),names[i],centers[i]+Vector3.up*7f+toward*4f,colors[i],Quaternion.LookRotation(-toward,Vector3.up));
                if(i>0)BuildSuspendedBridge("Bridge_"+i,centers[i-1],centers[i],colors[i]);
            }
            BuildSuspendedBridge("Bridge_Return",centers[5],new Vector3(0,20,31),magenta);
            for(int i=0;i<28;i++)
            {
                float angle=i*47f*Mathf.Deg2Rad; float radius=9+(i%7)*5f;
                Vector3 p=new Vector3(Mathf.Cos(angle)*radius,5+(i%6)*4.2f,31+Mathf.Sin(angle)*radius);
                Block("FloatingDebris",p,new Vector3(1.2f+(i%3),.35f,2.5f+(i%4)),i%5==0?cyan:concrete,new Vector3(i*13,i*23,i*7));
            }
        }

        /// <summary>Không nhận đầu vào; thêm lớp trang trí tùy chọn từ ThirdParty, thiếu gói thì giữ nguyên graybox.</summary>
        static void ApplyThirdPartyArt()
        {
            const string third="Assets/ThirdParty/";
            Material sciFi=AssetDatabase.LoadAssetAtPath<Material>(third+"YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_SciFi.mat");
            Material spalling=AssetDatabase.LoadAssetAtPath<Material>(third+"YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_Spalling.mat");
            GameObject locker=AssetDatabase.LoadAssetAtPath<GameObject>(third+"BackroomsLikeAsset/prefab/Props/Props_Locker.prefab");
            GameObject table=AssetDatabase.LoadAssetAtPath<GameObject>(third+"BackroomsLikeAsset/prefab/Props/Props_Table1.prefab");
            GameObject fog=AssetDatabase.LoadAssetAtPath<GameObject>(third+"Fog Particles/Prefabs/Bluish Fog.prefab");
            Material sky=AssetDatabase.LoadAssetAtPath<Material>(third+"Fantasy Skybox FREE/Panoramics/FS017/FS017_Night.mat");
            if(sky!=null)RenderSettings.skybox=sky;
            if(sciFi==null||spalling==null||locker==null||table==null||fog==null)
            {
                Debug.LogWarning("ThirdParty chưa đủ; đại sảnh vẫn dùng lớp graybox an toàn.");
                return;
            }
            Vector3[] islands={new Vector3(0,.02f,-.5f),new Vector3(39,3.02f,5),new Vector3(39,7.02f,42),new Vector3(0,11.02f,66),new Vector3(-42,15.02f,40),new Vector3(-39,19.02f,2)};
            for(int i=0;i<islands.Length;i++)
            {
                DecorativePanel("SurfaceSkin_"+(i+1),islands[i]+new Vector3(0,.02f,0),new Vector3(12,.05f,12),i%2==0?sciFi:spalling);
                PlacePrefab(locker,"LockerCluster_"+(i+1),islands[i]+new Vector3(-4,0,3),Quaternion.Euler(0,i*60,0),Vector3.one);
                if(i==2||i==5)PlacePrefab(table,"EvidenceTable_"+(i+1),islands[i]+new Vector3(3,0,1),Quaternion.Euler(0,180+i*20,0),Vector3.one);
            }
            PlacePrefab(fog,"VoidFog_A",new Vector3(0,-.4f,31),Quaternion.identity,new Vector3(8,2,8));
            PlacePrefab(fog,"VoidFog_B",new Vector3(0,8,31),Quaternion.Euler(0,90,0),new Vector3(6,2,6));
        }

        /// <summary>Nhận prefab, pose và scale; trả instance lồng dưới art root để có thể xóa cùng cảnh quan.</summary>
        static GameObject PlacePrefab(GameObject prefab,string name,Vector3 position,Quaternion rotation,Vector3 scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name=name;go.transform.SetParent(artRoot);go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=scale;return go;
        }

        /// <summary>Nhận pose và material; tạo lớp bề mặt mỏng không collider phủ trên graybox nền.</summary>
        static GameObject DecorativePanel(string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(artRoot);go.transform.position=position;go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }

        /// <summary>Nhận hai điểm và vật liệu; dựng chuỗi phiến cầu lơ lửng mang tính định hướng thị giác.</summary>
        static void BuildSuspendedBridge(string name,Vector3 start,Vector3 end,Material material)
        {
            Vector3 delta=end-start; int count=Mathf.Max(3,Mathf.CeilToInt(delta.magnitude/5f));
            for(int i=1;i<count;i++)
            {
                float t=i/(float)count; Vector3 p=Vector3.Lerp(start,end,t)+Vector3.down*(Mathf.Sin(t*Mathf.PI)*2.5f);
                Block(name+"_"+i,p,new Vector3(4.2f,.28f,2.2f),i%4==0?material:concrete,new Vector3(0,Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,i%2==0?3:-3));
            }
        }

        /// <summary>Nhận root, tâm local nguồn, tâm world đích và góc Y; cập nhật pose sao cho tâm phòng đúng vị trí.</summary>
        static void SetRootCenter(Transform root,Vector3 sourceCenter,Vector3 desiredCenter,float yaw)
        {Quaternion rotation=Quaternion.Euler(0,yaw,0);root.SetPositionAndRotation(desiredCenter-rotation*sourceCenter,rotation);}

        /// <summary>Không nhận đầu vào; đặt sương, ambient và các nguồn sáng nhấn tầng.</summary>
        static void ConfigureAtmosphere()
        {
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.012f,.02f,.038f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.008f;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.07f,.13f,.2f);RenderSettings.ambientEquatorColor=new Color(.025f,.04f,.07f);RenderSettings.ambientGroundColor=new Color(.006f,.008f,.015f);
            for(int i=0;i<6;i++)
            {float angle=i*60f*Mathf.Deg2Rad;var light=new GameObject("AtriumLight_"+(i+1),typeof(Light)).GetComponent<Light>();light.transform.SetParent(artRoot);light.transform.position=new Vector3(Mathf.Sin(angle)*16,5+i*3.2f,31+Mathf.Cos(angle)*16);light.type=LightType.Point;light.range=25;light.intensity=4;light.color=i<4?new Color(.1f,.75f,1f):new Color(.9f,.15f,.65f);}
        }

        /// <summary>Nhận tên, pose và material; trả cube có collider dưới art root.</summary>
        static GameObject Block(string name,Vector3 position,Vector3 scale,Material material,Vector3 euler)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(artRoot);go.transform.position=position;go.transform.rotation=Quaternion.Euler(euler);go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}

        /// <summary>Nhận tên, pose và material; trả cylinder có collider dưới art root.</summary>
        static GameObject Cylinder(string name,Vector3 position,Vector3 scale,Material material,Quaternion? rotation=null)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(artRoot);go.transform.position=position;go.transform.rotation=rotation??Quaternion.identity;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}

        /// <summary>Nhận nội dung, pose và material; trả TextMesh biển nhiệm vụ hướng vào đại sảnh.</summary>
        static TextMesh Label(string name,string value,Vector3 position,Material material,Quaternion rotation)
        {var go=new GameObject(name);go.transform.SetParent(artRoot);go.transform.position=position;go.transform.rotation=rotation;var text=go.AddComponent<TextMesh>();text.text=value;text.fontSize=72;text.characterSize=.055f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=Color.white;go.GetComponent<MeshRenderer>().sharedMaterial=material;return text;}

        /// <summary>Nhận màu và thông số bề mặt; trả material URP được lưu để tái sử dụng.</summary>
        static Material MakeMaterial(string name,Color baseColor,Color emission,float metallic,float smoothness)
        {string path=MatFolder+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");material=new Material(shader){name=name};AssetDatabase.CreateAsset(material,path);}if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",baseColor);if(material.HasProperty("_Color"))material.SetColor("_Color",baseColor);material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",smoothness);if(emission.maxColorComponent>.01f){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",emission);}EditorUtility.SetDirty(material);return material;}

        /// <summary>Nhận đường dẫn; tạo từng thư mục Assets còn thiếu.</summary>
        static void EnsureFolder(string path){string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}}
    }
}
