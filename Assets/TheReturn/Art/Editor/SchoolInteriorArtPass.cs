/*
 * Mục đích: Hoàn thiện nội thất sáu phòng bằng module trang trí độc lập với câu đố.
 * Hàm: Apply dựng toàn bộ; Dress bố trí một phòng theo sàn hiện có; Box tạo module;
 * Board tạo bảng trang trí; Text tạo chữ; Prop đặt prefab cũ; PlacePrefab đặt và chuẩn hoá asset mới;
 * DressImportedProps phân phối cửa/tủ/VFX; DressDesks thay hình ảnh 24 bàn ghế bằng prefab URP;
 * ClearIntrusions ẩn trang trí đại sảnh xuyên phòng; Folder tạo thư mục.
 */
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheReturn.Art.Editor
{
    public static class SchoolInteriorArtPass
    {
        const string Output="Assets/TheReturn/Art/Interiors/";
        static Material plaster, flooring, wood, metal, chalk, paper, lamp, energy;
        static Transform root;

        /// <summary>Không có đầu vào; dựng nội thất sáu khu đang mở rồi lưu scene và material.</summary>
        [MenuItem("The Return/Art/Dress All Mission Interiors")]
        public static void Apply()
        {
            if(Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="SchoolFloorPrototype")
                throw new InvalidOperationException("Mở SchoolFloorPrototype trong Edit Mode.");
            Folder(Output.TrimEnd('/'));
            plaster=Make("WarmPlaster",new Color(.57f,.55f,.47f));
            flooring=Make("Terrazzo",new Color(.24f,.29f,.28f));
            wood=Make("Walnut",new Color(.19f,.105f,.05f));
            metal=Make("PaintedSteel",new Color(.055f,.075f,.075f));
            chalk=Make("Chalkboard",new Color(.025f,.085f,.065f));
            paper=Make("ArchivePaper",new Color(.7f,.64f,.44f));
            lamp=Make("Fluorescent",new Color(.82f,.94f,1));lamp.EnableKeyword("_EMISSION");lamp.SetColor("_EmissionColor",new Color(2,2.2f,2.3f));
            energy=AssetDatabase.LoadAssetAtPath<Material>("Assets/ThirdParty/G-spot_Lab/Magic Energy Seamless Textures Free Pack/Materials/MagicEnergy_Blue_02.mat");
            string[] names={"Environment_LogicOnly","QuietCorridor_LogicOnly","GradeRepair_LogicOnly","NavigationExam_LogicOnly","DontLookBack_LogicOnly","TeacherTruth_LogicOnly"};
            string[] labels={"01 / HOMEROOM","02 / SILENT WING","03 / RECORDS OFFICE","04 / EXAMINATION","05 / RESTRICTED WING","06 / FACULTY COUNCIL"};
            Color[] colors={new Color(.12f,.24f,.2f),new Color(.14f,.21f,.26f),new Color(.3f,.21f,.12f),new Color(.14f,.23f,.3f),new Color(.27f,.12f,.13f),new Color(.23f,.17f,.27f)};
            for(int i=0;i<names.Length;i++)
            {
                var room=GameObject.Find(names[i]);if(room==null)throw new InvalidOperationException("Thiếu phòng "+names[i]);
                Dress(room.transform,i,labels[i],colors[i]);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>Nhận root phòng, chỉ số, tên và màu; thêm nội thất theo tọa độ local, không sửa collider/checkpoint.</summary>
        static void Dress(Transform room,int index,string label,Color color)
        {
            Transform floor=room.Find("Floor");if(floor==null)throw new InvalidOperationException("Thiếu sàn "+room.name);
            var old=room.Find("Interior_Decoration");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            root=new GameObject("Interior_Decoration").transform;root.SetParent(room,false);
            Vector3 c=floor.localPosition;float w=floor.localScale.x,d=floor.localScale.z,y=c.y+floor.localScale.y*.5f;
            ClearIntrusions(room,new Bounds(new Vector3(c.x,y+1.8f,c.z),new Vector3(w-.5f,3.2f,d-.5f)));
            Material accent=Make("Wainscot_"+index,color);
            var floorRenderer=floor.GetComponent<Renderer>();if(floorRenderer!=null){floorRenderer.sharedMaterial=flooring;PrefabUtility.RecordPrefabInstancePropertyModifications(floorRenderer);}
            foreach(Transform t in room)
            {
                if(t.name.StartsWith("Wall_")||t.name.EndsWith("_Header")||t.name.StartsWith("Front_")||t.name.StartsWith("Back_"))
                {var r=t.GetComponent<Renderer>();if(r!=null){r.sharedMaterial=plaster;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}}
            }
            // Phân lớp kiến trúc: nền tối, tường sáng và viền gỗ; luôn nằm sát mép phòng.
            for(int side=-1;side<=1;side+=2)
            {
                float x=c.x+side*(w*.5f-.13f);
                Box("WallPanelling",new Vector3(x,y+.65f,c.z),new Vector3(.055f,1.3f,d-.25f),accent);
                Box("ChairRail",new Vector3(x-side*.04f,y+1.34f,c.z),new Vector3(.1f,.08f,d-.25f),wood);
                Box("Skirting",new Vector3(x-side*.04f,y+.09f,c.z),new Vector3(.1f,.18f,d-.25f),wood);
                Box("Cornice",new Vector3(x-side*.06f,y+3.45f,c.z),new Vector3(.18f,.16f,d-.25f),wood);
                for(float z=c.z-d*.5f+1.7f;z<c.z+d*.5f-1;z+=3.8f)
                {
                    Box("WallPilaster",new Vector3(x-side*.035f,y+2.37f,z),new Vector3(.09f,2.05f,.12f),wood);
                    if(side==1)
                    {
                        // Cửa sổ giả nằm hoàn toàn sát tường, không chặn đường đi hay bàn nhiệm vụ.
                        Box("WindowFrame",new Vector3(x-side*.09f,y+2.35f,z+1.05f),new Vector3(.12f,1.65f,1.45f),metal);
                        var glass=Make("FrostedGlass",new Color(.19f,.34f,.39f));
                        Box("WindowGlass",new Vector3(x-side*.16f,y+2.35f,z+1.05f),new Vector3(.025f,1.43f,1.24f),glass);
                        for(int slat=0;slat<4;slat++)Box("WindowBlind",new Vector3(x-side*.19f,y+2.02f+slat*.23f,z+1.05f),new Vector3(.025f,.035f,1.28f),wood);
                    }
                }
            }
            // Trần kỹ thuật hở để vẫn nhìn được kiến trúc siêu nhiên phía trên.
            int count=Mathf.Max(2,Mathf.FloorToInt(d/5));
            for(int i=0;i<count;i++)
            {
                float z=c.z-d*.5f+2.5f+i*(d-5)/(count-1);
                Box("CeilingBeam",new Vector3(c.x,y+3.7f,z),new Vector3(w,.22f,.18f),metal);
                Box("LampHousing",new Vector3(c.x,y+3.42f,z),new Vector3(Mathf.Min(w*.55f,4),.14f,.42f),metal);
                Box("LampDiffuser",new Vector3(c.x,y+3.335f,z),new Vector3(Mathf.Min(w*.55f,4)-.12f,.025f,.32f),lamp);
                var light=new GameObject("InteriorFill",typeof(Light)).GetComponent<Light>();light.transform.SetParent(root,false);light.transform.localPosition=new Vector3(c.x,y+2.95f,z);
                light.type=LightType.Point;light.range=9;light.intensity=index==4?1.1f:5f;light.color=index==4?new Color(1,.47f,.3f):new Color(1,.89f,.72f);light.shadows=LightShadows.None;
                light.renderMode=LightRenderMode.ForcePixel;
            }
            Board(new Vector3(c.x-w*.5f+.23f,y+2.35f,c.z-d*.25f),Quaternion.Euler(0,-90,0),"CAMPUS ARCHIVE\n________________\nTERM 09  /  1986\nACCESS: STAFF ONLY",2.4f);
            Text(label,new Vector3(c.x,y+3.06f,c.z+d*.5f-.35f),Quaternion.identity,.043f);
            if(w>6)
            {
                // Tủ đặt sát tường sau; phần giữa giữ trống cho cơ chế nhiệm vụ.
                for(int i=0;i<3;i++)Prop("Props_Locker",new Vector3(c.x-w*.5f+.6f,y,c.z+d*.5f-1.1f-i*1.05f),90);
                Prop("Props_TrashCan",new Vector3(c.x+w*.5f-.75f,y,c.z-d*.5f+1),0);
                Board(new Vector3(c.x+w*.5f-.24f,y+2.35f,c.z+d*.2f),Quaternion.Euler(0,90,0),index==2?"STUDENT RECORDS\n________________\nCONFIDENTIAL":index==5?"FACULTY COUNCIL\n________________\nSESSION IN PROGRESS":"SCHOOL NOTICE\n________________\nRETURN ALL BOOKS",2.6f);
            }
            else
            {
                for(int i=0;i<3;i++)Board(new Vector3(c.x-w*.5f+.23f,y+2.3f,c.z-4+i*4),Quaternion.Euler(0,-90,0),"SILENCE\nPLEASE",1.4f);
            }
            DressImportedProps(index,c,w,d,y);
            if(index==0)DressDesks();
        }

        /// <summary>Nhận phòng và kích thước; đặt cửa PBR, đồ lưu trữ, đạo cụ và điểm nhấn siêu nhiên sát tường.</summary>
        static void DressImportedProps(int index,Vector3 c,float w,float d,float y)
        {
            const string furniture="Assets/ThirdParty/Furniture Mega Pack/Prefabs/";
            PlacePrefab("Assets/ThirdParty/Free Wood Door Pack/Prefab/Wood/Door_9/Door_9_Venge.prefab","PBR_Door",
                new Vector3(c.x,y+1.35f,c.z+d*.5f-.2f),Quaternion.Euler(0,180,0),new Vector3(1.35f,2.7f,.28f));

            string storage=index==2||index==5?"Closets/Closet12.prefab":"Drawers/Drawer03.prefab";
            PlacePrefab(furniture+storage,"ArchiveStorage",new Vector3(c.x+w*.5f-.48f,y+.85f,c.z-d*.5f+1.45f),
                Quaternion.Euler(0,-90,0),new Vector3(index==2||index==5?.85f:1.05f,1.7f,.55f));

            if(index==2)
            {
                PlacePrefab(furniture+"Closets/Closet17.prefab","RecordsCabinet",new Vector3(c.x+w*.5f-.48f,y+1.05f,c.z),
                    Quaternion.Euler(0,-90,0),new Vector3(.9f,2.1f,1.55f));
                PlacePrefab(furniture+"Tables/Table07.prefab","RecordsDesk",new Vector3(c.x+w*.5f-1.2f,y+.42f,c.z+d*.18f),
                    Quaternion.Euler(0,-90,0),new Vector3(1.45f,.84f,.72f));
            }
            if(index==5)
            {
                PlacePrefab("Assets/ThirdParty/HotelRoom-Mavi3D/Prefabs/URP/FlowerPot.prefab","FacultyPlant",
                    new Vector3(c.x-w*.5f+.55f,y+.62f,c.z+d*.5f-1.1f),Quaternion.identity,new Vector3(.65f,1.25f,.65f));
            }
            if(index==4)
            {
                PlacePrefab("Assets/ThirdParty/RainSnowCloudEffect/Prefabs/PF_Fog_Ground.prefab","RestrictedFog",
                    new Vector3(c.x,y+.05f,c.z+d*.12f),Quaternion.identity,new Vector3(Mathf.Min(w*.75f,7),1.1f,Mathf.Min(d*.35f,6)));
                if(energy!=null)
                {
                    var anomaly=Box("EnergyAnomaly",new Vector3(c.x-w*.5f+.18f,y+2.1f,c.z),new Vector3(.035f,2.5f,2.5f),energy);
                    anomaly.transform.localRotation=Quaternion.Euler(0,0,18);
                }
            }
        }

        /// <summary>Nhận root và thể tích phòng local; ẩn riêng vật trang trí đại sảnh xuyên vào vùng chơi.</summary>
        static void ClearIntrusions(Transform room,Bounds interior)
        {
            foreach(string name in new[]{"MonumentalSchool_ArtOnly","Observatory_Decoration"})
            {
                var art=GameObject.Find(name);if(art==null)continue;
                foreach(Transform child in art.transform)
                {
                    if(!child.gameObject.activeSelf)continue;
                    foreach(var renderer in child.GetComponentsInChildren<MeshRenderer>())
                    {
                        Bounds b=renderer.bounds;Bounds local=new Bounds(room.InverseTransformPoint(b.center),Vector3.zero);
                        for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                            local.Encapsulate(room.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));
                        if(local.Intersects(interior)){child.gameObject.SetActive(false);break;}
                    }
                }
            }
        }

        /// <summary>Không có đầu vào; ẩn mesh bàn ghế tạm và đặt 24 bộ prefab URP đúng vị trí, giữ collider và nhãn ghế.</summary>
        static void DressDesks()
        {
            const string tablePath="Assets/ThirdParty/Furniture Mega Pack/Prefabs/Tables/Table01.prefab";
            const string chairPath="Assets/ThirdParty/Furniture Mega Pack/Prefabs/Chairs/Chair01.prefab";
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(r.transform.parent==null||r.transform.parent.name!="Visual_Replaceable")continue;
                bool desk=r.name=="Desk",chair=r.name=="ChairSeat";
                if(desk||chair||r.name=="ChairBack")
                {
                    r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);
                    if(!desk&&!chair)continue;
                    Vector3 p=root.InverseTransformPoint(r.transform.position);
                    if(desk)
                    {
                        PlacePrefab(tablePath,"ImportedSchoolDesk",new Vector3(p.x,.43f,p.z),Quaternion.identity,new Vector3(1.45f,.78f,.72f));
                        Box("ExerciseBook",new Vector3(p.x-.34f,.84f,p.z+.08f),new Vector3(.3f,.025f,.38f),paper);
                    }
                    else PlacePrefab(chairPath,"ImportedSchoolChair",new Vector3(p.x,.45f,p.z),Quaternion.identity,new Vector3(.58f,.9f,.58f));
                }
                if(r.name=="DeskSupport"||r.name=="ChairSupport")
                {r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            }
        }

        /// <summary>Nhận đường dẫn, tên, tâm, góc và kích thước đích; trả prefab đã co giãn theo bounds và bỏ collider.</summary>
        static GameObject PlacePrefab(string path,string name,Vector3 center,Quaternion rotation,Vector3 desiredSize)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null){Debug.LogWarning("Thiếu prefab: "+path);return null;}
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.name=name;go.transform.localPosition=center;go.transform.localRotation=rotation;go.transform.localScale=Vector3.one;
            foreach(var c in go.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
            Bounds source=LocalBounds(go);if(source.size.sqrMagnitude>.0001f)
            {
                go.transform.localScale=new Vector3(desiredSize.x/Mathf.Max(source.size.x,.001f),desiredSize.y/Mathf.Max(source.size.y,.001f),desiredSize.z/Mathf.Max(source.size.z,.001f));
                Bounds fitted=LocalBounds(go);go.transform.localPosition+=center-fitted.center;
            }
            return go;
        }

        /// <summary>Nhận prefab trong root hiện tại; trả bounds của toàn bộ renderer theo hệ tọa độ local của root.</summary>
        static Bounds LocalBounds(GameObject go)
        {
            bool found=false;Bounds result=new Bounds();
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                Bounds b=renderer.bounds;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                {
                    Vector3 p=root.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z)));
                    if(!found){result=new Bounds(p,Vector3.zero);found=true;}else result.Encapsulate(p);
                }
            }
            return result;
        }

        /// <summary>Nhận tên/pose/material; trả module trang trí không collider.</summary>
        static GameObject Box(string name,Vector3 p,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }

        /// <summary>Nhận tâm, góc, nội dung và chiều rộng; tạo bảng thông báo đóng khung gỗ.</summary>
        static void Board(Vector3 p,Quaternion q,string text,float width)
        {
            var frame=Box("NoticeFrame",p,new Vector3(width,1.45f,.1f),wood);frame.transform.localRotation=q;
            var panel=Box("NoticeBoard",p-q*Vector3.forward*.065f,new Vector3(width-.14f,1.31f,.025f),chalk);panel.transform.localRotation=q;
            Text(text,p-q*Vector3.forward*.09f,q,.026f);
        }

        /// <summary>Nhận nội dung, pose local và kích thước; tạo TextMesh không liên quan manh mối câu đố.</summary>
        static void Text(string text,Vector3 p,Quaternion rotation,float size)
        {
            var go=new GameObject("EnvironmentalSign");go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=rotation;
            var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=new Color(.85f,.82f,.63f);
        }

        /// <summary>Nhận prefab đã nhập và pose local; đặt instance trang trí và bỏ collider instance.</summary>
        static void Prop(string name,Vector3 p,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/BackroomsLikeAsset/prefab/Props/"+name+".prefab");if(prefab==null)return;
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var c in go.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
        }

        /// <summary>Nhận tên và màu; trả material URP được lưu, dùng lại giữa các phòng.</summary>
        static Material Make(string name,Color color)
        {
            string path=Output+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",name=="Terrazzo"?.35f:.16f);m.enableInstancing=true;
            if(name=="Terrazzo"||name=="WarmPlaster")
            {
                var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/ThirdParty/YughuesFreeConcreteMaterials/YughuesFreeConcreteMaterials/Materials/M_YFCM_"+(name=="Terrazzo"?"Polished":"Plastered")+".mat");
                if(source!=null){m.SetTexture("_BaseMap",source.GetTexture("_BaseMap"));m.SetTexture("_BumpMap",source.GetTexture("_BumpMap"));m.SetTextureScale("_BaseMap",new Vector2(4,4));m.SetTextureScale("_BumpMap",new Vector2(4,4));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.3f);}
            }
            EditorUtility.SetDirty(m);return m;
        }

        /// <summary>Nhận đường dẫn Assets; tạo từng thư mục còn thiếu.</summary>
        static void Folder(string path)
        {string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}}
    }
}
