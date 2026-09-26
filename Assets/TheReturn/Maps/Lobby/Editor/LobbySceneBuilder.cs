/*
 * Mục đích: Dựng scene sảnh 2–4 người với Canvas sẵn sàng, portal và luồng sang tầng học.
 * Hàm: Build dựng/lưu scene; CreateArchitecture tạo kiến trúc; CreateParty tạo đội FPS;
 * CreateCanvas tạo UI; Block/Cylinder/Material tạo module; ConfigureBuildSettings đăng ký scene.
 */
using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheReturn.Editor
{
    public static class LobbySceneBuilder
    {
        const string LobbyPath="Assets/TheReturn/Scenes/Maps/Lobby/Sanh_Cho.unity";
        const string SchoolPath="Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity";
        const string MatFolder="Assets/TheReturn/Maps/Lobby/Art/Materials/";
        static Material stone,dark,glow;
        static Transform architecture;

        /// <summary>Không nhận đầu vào; dựng và lưu sảnh rồi đưa hai scene vào Build Settings.</summary>
        [MenuItem("The Return/Lobby/Rebuild Monumental Lobby")]
        public static void Build()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Hãy dừng Play Mode trước khi dựng sảnh.");
            Scene scene=EditorSceneManager.OpenScene(LobbyPath,OpenSceneMode.Single);
            foreach(GameObject root in scene.GetRootGameObjects())UnityEngine.Object.DestroyImmediate(root);
            EnsureFolder("Assets/TheReturn/Maps/Lobby/Art/Materials");
            stone=MakeMaterial("M_Lobby_Stone",new Color(.075f,.105f,.14f),Color.black,.18f);
            dark=MakeMaterial("M_Lobby_Dark",new Color(.018f,.025f,.04f),Color.black,.55f);
            glow=MakeMaterial("M_Lobby_Glow",new Color(.03f,.22f,.27f),new Color(0f,2.8f,3.4f),.3f);
            CreateArchitecture(); ApplyThirdPartyArt();
            PrototypePartyController party=CreateParty();
            LobbyCanvasView view=CreateCanvas();
            var system=new GameObject("LobbyReadySystem").AddComponent<LobbyReadyController>();
            system.party=party; system.view=view;
            system.portalRenderer=GameObject.Find("Portal_Core").GetComponent<Renderer>();
            system.schoolSceneName="SchoolFloorPrototype";
            new GameObject("SceneTransitionService").AddComponent<SceneTransitionService>();
            new GameObject("UI_EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            ConfigureLighting(); ConfigureBuildSettings();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene,LobbyPath);
            AssetDatabase.SaveAssets(); Selection.activeObject=system;
        }

        /// <summary>Không nhận đầu vào; tạo sàn, cột, vòm, bốn bục và cổng trung tâm.</summary>
        static void CreateArchitecture()
        {
            architecture=new GameObject("Lobby_Architecture_Replaceable").transform;
            Block("Floor",new Vector3(0,-.3f,0),new Vector3(34,.6f,30),stone);
            Block("BackWall",new Vector3(0,7,14.5f),new Vector3(34,14,1),dark);
            Block("LeftWall",new Vector3(-16.5f,7,0),new Vector3(1,14,30),dark);
            Block("RightWall",new Vector3(16.5f,7,0),new Vector3(1,14,30),dark);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
                Cylinder("MonumentColumn",new Vector3(side*12.5f,4.5f,-10+i*5.2f),new Vector3(1.5f,4.5f,1.5f),stone);
            for(int i=0;i<7;i++)
            {
                GameObject rib=Block("CeilingRib",new Vector3(0,10.5f+i*.3f,-12+i*4f),new Vector3(30-i*1.5f,.35f,.55f),glow);
                rib.transform.rotation=Quaternion.Euler(0,0,i%2==0?4:-4);
            }
            Block("Portal_Frame_Left",new Vector3(-3.8f,4.5f,12.8f),new Vector3(1.2f,9,1.2f),stone);
            Block("Portal_Frame_Right",new Vector3(3.8f,4.5f,12.8f),new Vector3(1.2f,9,1.2f),stone);
            Block("Portal_Frame_Top",new Vector3(0,8.5f,12.8f),new Vector3(8.8f,1.2f,1.2f),stone);
            Block("Portal_Core",new Vector3(0,4.4f,13.2f),new Vector3(6.4f,7.2f,.25f),glow);
            for(int i=0;i<4;i++)
            {
                float x=-7.5f+i*5f;
                Cylinder("ReadyPedestal_"+(i+1),new Vector3(x,.2f,3.5f),new Vector3(1.8f,.5f,1.8f),stone);
                Cylinder("ReadyRune_"+(i+1),new Vector3(x,.72f,3.5f),new Vector3(1.35f,.04f,1.35f),glow);
            }
            for(int i=0;i<12;i++)
            {
                float a=i*30f*Mathf.Deg2Rad;
                GameObject slab=Block("FloatingSlab",new Vector3(Mathf.Cos(a)*14f,9+(i%3)*1.8f,Mathf.Sin(a)*11f),new Vector3(3.2f,.35f,1.2f),stone);
                slab.transform.rotation=Quaternion.Euler(i*7f,-i*30f,i%2==0?12:-12);
            }
        }

        /// <summary>Không nhận đầu vào; thêm đạo cụ, fog và skybox ThirdParty; thiếu gói vẫn giữ nguyên sảnh nền.</summary>
        static void ApplyThirdPartyArt()
        {
            const string third="Assets/ThirdParty/";
            GameObject locker=AssetDatabase.LoadAssetAtPath<GameObject>(third+"BackroomsLikeAsset/prefab/Props/Props_Locker.prefab");
            GameObject table=AssetDatabase.LoadAssetAtPath<GameObject>(third+"BackroomsLikeAsset/prefab/Props/Props_Table1.prefab");
            GameObject chair=AssetDatabase.LoadAssetAtPath<GameObject>(third+"BackroomsLikeAsset/prefab/Props/Props_Chair.prefab");
            GameObject fog=AssetDatabase.LoadAssetAtPath<GameObject>(third+"Fog Particles/Prefabs/Bluish Fog.prefab");
            Material concrete=AssetDatabase.LoadAssetAtPath<Material>(third+"YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_SciFi.mat");
            Material sky=AssetDatabase.LoadAssetAtPath<Material>(third+"Fantasy Skybox FREE/Panoramics/FS017/FS017_Night.mat");
            if(sky!=null)RenderSettings.skybox=sky;
            if(locker==null||table==null||chair==null||fog==null||concrete==null)
            {
                Debug.LogWarning("ThirdParty chưa đủ; sảnh vẫn dùng lớp graybox an toàn.");
                return;
            }
            for(int side=-1;side<=1;side+=2)
            {
                PlacePrefab(locker,"LobbyLocker",new Vector3(side*13.8f,0,9),Quaternion.Euler(0,side<0?90:-90,0),Vector3.one);
                PlacePrefab(table,"LobbyTable",new Vector3(side*10.5f,0,-5),Quaternion.Euler(0,side<0?28:-28,0),Vector3.one);
                PlacePrefab(chair,"LobbyChair",new Vector3(side*9.2f,0,-4),Quaternion.Euler(0,side<0?210:150,0),Vector3.one);
            }
            for(int i=0;i<4;i++)
            {
                var skin=GameObject.CreatePrimitive(PrimitiveType.Cylinder);skin.name="ReadyPedestalSkin_"+(i+1);skin.transform.SetParent(architecture);
                skin.transform.position=new Vector3(-7.5f+i*5f,.735f,3.5f);skin.transform.localScale=new Vector3(1.38f,.025f,1.38f);
                UnityEngine.Object.DestroyImmediate(skin.GetComponent<Collider>());skin.GetComponent<Renderer>().sharedMaterial=concrete;
            }
            PlacePrefab(fog,"LobbyPortalFog",new Vector3(0,1.2f,11.5f),Quaternion.identity,new Vector3(3,2,2));
        }

        /// <summary>Nhận prefab, pose và scale; trả instance lồng dưới kiến trúc sảnh.</summary>
        static GameObject PlacePrefab(GameObject prefab,string name,Vector3 position,Quaternion rotation,Vector3 scale)
        {var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.name=name;go.transform.SetParent(architecture);go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=scale;return go;}

        /// <summary>Không nhận đầu vào; tạo bốn avatar, camera và trả controller đã nối mảng.</summary>
        static PrototypePartyController CreateParty()
        {
            var root=new GameObject("LobbyParty"); var controller=root.AddComponent<PrototypePartyController>();
            controller.players=new Transform[4]; controller.bodies=new Renderer[4];
            Color[] colors={new Color(.2f,.75f,1f),new Color(1f,.45f,.3f),new Color(.55f,1f,.45f),new Color(.85f,.45f,1f)};
            for(int i=0;i<4;i++)
            {
                var player=new GameObject("Player_"+(i+1),typeof(CharacterController)); player.transform.SetParent(root.transform);
                player.transform.position=new Vector3(-7.5f+i*5f,.05f,1.7f);
                var body=GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name="Visual_Replaceable";
                body.transform.SetParent(player.transform,false); body.transform.localPosition=Vector3.up; body.transform.localScale=new Vector3(.55f,.9f,.55f);
                UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
                body.GetComponent<Renderer>().sharedMaterial=MakeMaterial("M_Lobby_Player_"+(i+1),colors[i]*.25f,colors[i]*1.8f,.25f);
                controller.players[i]=player.transform; controller.bodies[i]=body.GetComponent<Renderer>();
            }
            var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); cameraObject.tag="MainCamera";
            controller.viewCamera=cameraObject.GetComponent<Camera>(); controller.viewCamera.fieldOfView=75f;
            return controller;
        }

        /// <summary>Không nhận đầu vào; tạo Canvas, nút quy mô đội và bốn nút ready rồi trả view.</summary>
        static LobbyCanvasView CreateCanvas()
        {
            var root=new GameObject("PF_LobbyCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(LobbyCanvasView));
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
            var view=root.GetComponent<LobbyCanvasView>();
            var panel=new GameObject("LobbyPanel",typeof(RectTransform),typeof(Image)); panel.transform.SetParent(root.transform,false);
            SetRect(panel.GetComponent<RectTransform>(),new Vector2(.04f,.08f),new Vector2(.39f,.94f));
            panel.GetComponent<Image>().color=new Color(.015f,.025f,.045f,.88f);
            view.title=MakeText("Title",panel.transform,new Vector2(.07f,.76f),new Vector2(.93f,.96f),34,TextAnchor.MiddleCenter,new Color(.55f,1f,.95f));
            view.slotLabels=new Text[4]; view.slotButtons=new Button[4];
            for(int i=0;i<4;i++)
            {
                float top=.72f-i*.135f; Button button=MakeButton("Slot_"+(i+1),panel.transform,new Vector2(.08f,top-.1f),new Vector2(.92f,top),out Text label);
                UnityEventTools.AddIntPersistentListener(button.onClick,view.ToggleSlot,i); view.slotButtons[i]=button; view.slotLabels[i]=label;
            }
            for(int count=2;count<=4;count++)
            {
                float left=.08f+(count-2)*.285f; Button button=MakeButton(count+"Players",panel.transform,new Vector2(left,.12f),new Vector2(left+.25f,.2f),out Text label);
                label.text=count+" NGƯỜI"; UnityEventTools.AddIntPersistentListener(button.onClick,view.SetPartySize,count);
            }
            view.status=MakeText("Status",panel.transform,new Vector2(.07f,.01f),new Vector2(.93f,.1f),17,TextAnchor.MiddleCenter,Color.white);
            return view;
        }

        /// <summary>Không nhận đầu vào; thiết lập sương, môi trường và đèn cho sảnh.</summary>
        static void ConfigureLighting()
        {
            RenderSettings.fog=true; RenderSettings.fogColor=new Color(.015f,.035f,.06f); RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.012f;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight; RenderSettings.ambientSkyColor=new Color(.08f,.16f,.22f);
            RenderSettings.ambientEquatorColor=new Color(.03f,.055f,.08f); RenderSettings.ambientGroundColor=new Color(.008f,.012f,.02f);
            var sun=new GameObject("Moonlight",typeof(Light)).GetComponent<Light>(); sun.type=LightType.Directional; sun.color=new Color(.48f,.68f,1f); sun.intensity=1.15f; sun.transform.rotation=Quaternion.Euler(42,-28,0);
            for(int i=0;i<4;i++){var light=new GameObject("PortalLight_"+i,typeof(Light)).GetComponent<Light>(); light.type=LightType.Point; light.color=new Color(.1f,.9f,1f); light.range=14; light.intensity=4; light.transform.position=new Vector3((i-1.5f)*2.2f,2.5f+(i%2)*3f,10.5f);}
        }

        /// <summary>Nhận hình học; trả cube có collider trong root kiến trúc.</summary>
        static GameObject Block(string name,Vector3 position,Vector3 scale,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(architecture);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}

        /// <summary>Nhận hình học; trả cylinder có collider trong root kiến trúc.</summary>
        static GameObject Cylinder(string name,Vector3 position,Vector3 scale,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(architecture);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}

        /// <summary>Nhận màu; trả material URP được lưu để tái sử dụng.</summary>
        static Material MakeMaterial(string name,Color baseColor,Color emission,float metallic)
        {
            string path=MatFolder+name+".mat"; var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");material=new Material(shader){name=name};AssetDatabase.CreateAsset(material,path);}
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",baseColor); if(material.HasProperty("_Color"))material.SetColor("_Color",baseColor);
            material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",.55f);
            if(emission.maxColorComponent>.01f){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",emission);} EditorUtility.SetDirty(material);return material;
        }

        /// <summary>Nhận bố cục; trả Text UI đã neo theo cha.</summary>
        static Text MakeText(string name,Transform parent,Vector2 min,Vector2 max,int size,TextAnchor anchor,Color color)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);SetRect(go.GetComponent<RectTransform>(),min,max);var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=anchor;text.color=color;text.resizeTextForBestFit=true;text.resizeTextMinSize=12;text.resizeTextMaxSize=size;return text;}

        /// <summary>Nhận bố cục; trả Button và Text con để caller nối callback.</summary>
        static Button MakeButton(string name,Transform parent,Vector2 min,Vector2 max,out Text label)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);SetRect(go.GetComponent<RectTransform>(),min,max);go.GetComponent<Image>().color=new Color(.07f,.2f,.25f,.95f);label=MakeText("Label",go.transform,Vector2.zero,Vector2.one,20,TextAnchor.MiddleCenter,Color.white);return go.GetComponent<Button>();}

        /// <summary>Nhận RectTransform và anchors; kéo giãn nội dung đúng vùng.</summary>
        static void SetRect(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}

        /// <summary>Nhận đường dẫn; tạo từng thư mục Assets còn thiếu.</summary>
        static void EnsureFolder(string path){string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}}

        /// <summary>Không nhận đầu vào; đặt Lobby đầu và SchoolFloor thứ hai trong Build Settings.</summary>
        static void ConfigureBuildSettings(){EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(LobbyPath,true),new EditorBuildSettingsScene(SchoolPath,true)};}
    }
}
