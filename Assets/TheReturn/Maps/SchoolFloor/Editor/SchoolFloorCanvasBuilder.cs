/*
 * Mục đích: Tạo Canvas prefab và gắn vào map một lần; UI tồn tại sẵn trong Editor, không dựng mỗi frame.
 * Hàm: Build kiểm tra/lắp scene; CreateAtoms tạo prefab panel/nút; Rect đặt bố cục;
 * Panel / Text / Button tạo instance; Skin gắn theme; Scroll tạo nội dung dài có cuộn.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
    public static partial class SchoolFloorCanvasBuilder
    {
        const string Root="Assets/TheReturn/Maps/SchoolFloor/UI/";
        const string Shared="Assets/TheReturn/Shared/UI/";
        static SchoolUITheme theme;
        static GameObject panelPrefab,buttonPrefab;
        static SchoolFloorCanvasView view;

        /// <summary>Không nhận tham số; tạo prefab Canvas và gắn flow/launcher vào scene đã lưu, từ chối ghi đè UI có sẵn.</summary>
        [MenuItem("The Return/UI/Install School Floor Canvas")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            if(Application.isPlaying||scene.isDirty||flow==null)throw new InvalidOperationException("Open saved school scene in Edit mode.");
            if(UnityEngine.Object.FindFirstObjectByType<SchoolFloorCanvasPresenter>()!=null)
                throw new InvalidOperationException("Canvas exists. Edit its prefab instead of rebuilding.");
            Directory.CreateDirectory(Root+"Prefabs");
            Directory.CreateDirectory(Shared+"Prefabs");
            Directory.CreateDirectory(Shared+"Themes");
            AssetDatabase.Refresh();
            theme=AssetDatabase.LoadAssetAtPath<SchoolUITheme>(Shared+"Themes/SchoolUITheme.asset");
            if(theme==null)
            {
                theme=ScriptableObject.CreateInstance<SchoolUITheme>();
                theme.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                theme.panelSprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                theme.buttonSprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                AssetDatabase.CreateAsset(theme,Shared+"Themes/SchoolUITheme.asset");
            }
            CreateAtoms();
            var canvasObject=new GameObject("PF_SchoolFloorCanvas",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasObject.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=100;
            var scaler=canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);
            scaler.matchWidthOrHeight=.5f;
            var frame=new GameObject("SafeFrame_1280x720",typeof(RectTransform)).GetComponent<RectTransform>();
            frame.SetParent(canvasObject.transform,false);
            frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);
            frame.sizeDelta=new Vector2(1280,720);
            view=canvasObject.AddComponent<SchoolFloorCanvasView>();
            var presenter=canvasObject.AddComponent<SchoolFloorCanvasPresenter>();
            presenter.view=view;
            BuildHud(frame);
            BuildMenus(frame);
            BuildGrade(frame);
            BuildExam(frame);
            BuildBack(frame);
            AddBackTest();
            AddTeacherTest();
            view.pause.SetActive(false); view.tests.SetActive(false); view.gradeBoard.SetActive(false);
            view.document.SetActive(false); view.examBoard.SetActive(false);
            var prefab=PrefabUtility.SaveAsPrefabAsset(canvasObject,Root+"Prefabs/PF_SchoolFloorCanvas.prefab");
            UnityEngine.Object.DestroyImmediate(canvasObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var live=instance.GetComponent<SchoolFloorCanvasPresenter>();
            live.flow=flow;
            var launcher=flow.GetComponent<MissionTestLauncher>();
            if(launcher==null)launcher=flow.gameObject.AddComponent<MissionTestLauncher>();
            launcher.flow=flow;
            live.launcher=launcher;
            PrefabUtility.RecordPrefabInstancePropertyModifications(live);
            flow.useCanvas=true;
            flow.attendanceHud.useCanvas=true;
            if(UnityEngine.Object.FindFirstObjectByType<EventSystem>()==null)
            {
                var events=new GameObject("UI_EventSystem",typeof(EventSystem));
                events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject=instance;
        }

        /// <summary>Không nhận tham số; lưu các phần tử UI dùng chung nếu chưa có, không thay bản đã chỉnh.</summary>
        static void CreateAtoms()
        {
            string panelPath=Shared+"Prefabs/PF_UI_Panel.prefab";
            panelPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(panelPath);
            if(panelPrefab==null)
            {
                var go=new GameObject("PF_UI_Panel",typeof(RectTransform),typeof(Image));
                Skin(go,false,false);
                panelPrefab=PrefabUtility.SaveAsPrefabAsset(go,panelPath);
                UnityEngine.Object.DestroyImmediate(go);
            }
            string buttonPath=Shared+"Prefabs/PF_UI_Button.prefab";
            buttonPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(buttonPath);
            if(buttonPrefab==null)
            {
                var go=new GameObject("PF_UI_Button",typeof(RectTransform),typeof(Image),typeof(Button));
                Skin(go,true,false);
                go.GetComponent<Button>().targetGraphic=go.GetComponent<Image>();
                var label=Text(go.transform,"Label","Button",0,0,200,44,18);
                var r=label.rectTransform;
                r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(10,4);r.offsetMax=new Vector2(-10,-4);
                label.alignment=TextAnchor.MiddleCenter;
                buttonPrefab=PrefabUtility.SaveAsPrefabAsset(go,buttonPath);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>Nhận đối tượng/vai style; gắn theme, không thay kích thước hay nội dung.</summary>
        static void Skin(GameObject go,bool button,bool accent)
        {
            var style=go.AddComponent<CanvasThemeBinding>();
            style.theme=theme;style.button=button;style.accent=accent;style.Apply();
        }

        /// <summary>Nhận rect và tọa độ góc trái trên; thiết lập anchor/pivot/kích thước để designer sửa trực tiếp.</summary>
        static void Rect(RectTransform r,Transform parent,float x,float y,float width,float height)
        {
            r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(width,height);
        }

        /// <summary>Nhận tên, parent và khung; trả panel prefab có Image để thay sprite.</summary>
        static GameObject Panel(Transform parent,string name,float x,float y,float width,float height)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(panelPrefab);
            go.name=name;Rect(go.GetComponent<RectTransform>(),parent,x,y,width,height);
            return go;
        }

        /// <summary>Nhận nội dung/font size và khung; tạo Text Unicode có thể đổi font bằng theme.</summary>
        static Text Text(Transform parent,string name,string value,float x,float y,float width,float height,int size=18)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));
            var text=go.GetComponent<Text>();
            Rect(text.rectTransform,parent,x,y,width,height);
            text.text=value;text.fontSize=size;text.alignment=TextAnchor.UpperLeft;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
            text.raycastTarget=false;
            Skin(go,false,false);
            return text;
        }

        /// <summary>Nhận nhãn và khung; tạo instance nút chung, nội dung không chứa callback runtime.</summary>
        static Button Button(Transform parent,string name,string caption,float x,float y,float width,float height=44)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab);
            go.name=name;Rect(go.GetComponent<RectTransform>(),parent,x,y,width,height);
            go.GetComponentInChildren<Text>().text=caption;
            return go.GetComponent<Button>();
        }

        /// <summary>Nhận khung; tạo ScrollRect và Text tự giãn theo nội dung để không cắt manh mối dài.</summary>
        static Text Scroll(Transform parent,string name,float x,float y,float width,float height)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(ScrollRect));
            Rect(go.GetComponent<RectTransform>(),parent,x,y,width,height);
            var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(Image),typeof(RectMask2D));
            Rect(viewport.GetComponent<RectTransform>(),go.transform,0,0,width,height);
            viewport.GetComponent<Image>().color=Color.clear;
            var content=Text(viewport.transform,"Content","",0,0,width-12,height);
            content.rectTransform.anchorMax=new Vector2(1,1);
            content.rectTransform.sizeDelta=new Vector2(-12,height);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=go.GetComponent<ScrollRect>();
            scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=content.rectTransform;
            scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=24;
            scroll.movementType=ScrollRect.MovementType.Clamped;
            return content;
        }
    }
}
