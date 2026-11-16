/*
 * Mục đích: Dựng (hoặc dựng lại) prefab giao diện Cài đặt Resources/PF_SettingsMenu: bánh răng, khung cửa sổ, 4 tab và TỪNG DÒNG cài đặt đều là
 * đối tượng Canvas thật trong prefab, mở Prefab Mode là chỉnh được. Chỉ chạy khi gọi menu The Return > UI > Dựng prefab Cài đặt (CẢNH BÁO: ghi đè prefab,
 * mất các chỉnh sửa tay). Runtime không dùng file này; SettingsMenu chỉ nối logic vào prefab đã dựng.
 * Hàm: Build (menu); MakeGearPng; BuildRoot; BuildGear; BuildWindow; BuildPage...; BuildAudioPage; BuildGraphicsPage; BuildControlsPage; BuildSystemPage;
 * Row*, Header, Note (dòng/đề mục); Img, Txt, Btn, Slider, Switch, Selector (thành phần nhỏ); Assign (gán trường serialize).
 */
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn.EditorTools
{
    /// <summary>Công cụ Editor dựng prefab Cài đặt.</summary>
    public static class SettingsMenuPrefabBuilder
    {
        const string PrefabPath = "Assets/TheReturn/Features/GameSettings/Resources/PF_SettingsMenu.prefab";
        const string GearPngPath = "Assets/TheReturn/Shared/Resources/ModernUI/Gear.png";
        const int WindowW = 1120, WindowH = 720, HeaderH = 84, FooterH = 76, SideW = 260;
        static readonly string[] TabNames = { "Âm thanh", "Đồ họa", "Điều khiển", "Hệ thống" };

        static Font font;

        /// <summary>Không nhận tham số; dựng lại prefab Cài đặt và lưu vào Resources.</summary>
        [MenuItem("The Return/UI/Dựng prefab Cài đặt")]
        public static void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            MakeGearPng();
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            GameObject root = BuildRoot();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Đã dựng prefab Cài đặt: " + PrefabPath);
        }

        /// <summary>Không nhận tham số; vẽ biểu tượng bánh răng 8 răng thành file PNG trắng nền trong suốt (nếu chưa có) và nhập như Sprite.</summary>
        static void MakeGearPng()
        {
            if (File.Exists(GearPngPath)) return;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color32[n * n];
            float c = (n - 1) * .5f, radius = n * .5f - 2f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = x - c, dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dy, dx);
                    float s = Mathf.Clamp01((Mathf.Cos(ang * 8f) + .15f) / .4f);
                    s = s * s * (3f - 2f * s);
                    float outer = radius * (.74f + .26f * s);
                    float inner = radius * .34f;
                    float a = Mathf.Clamp01(outer - d + .5f) * Mathf.Clamp01(d - inner + .5f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(GearPngPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(GearPngPath, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(GearPngPath);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }

        // ─────────────── Thành phần nhỏ ───────────────

        /// <summary>Nhận tên sprite trong Resources/ModernUI (hoặc null); trả sprite hoặc null.</summary>
        static Sprite Spr(string name) { return string.IsNullOrEmpty(name) ? null : Resources.Load<Sprite>("ModernUI/" + name); }

        /// <summary>Nhận tên và cha; tạo object UI rỗng có RectTransform, layer UI.</summary>
        static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Nhận RectTransform và lề trái/dưới/phải/trên; kéo giãn kín cha.</summary>
        static void Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
        }

        /// <summary>Nhận RectTransform, neo min/max, pivot, vị trí và kích thước; đặt tất cả một lần.</summary>
        static void Place(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        /// <summary>Nhận RectTransform, rộng, cao; đặt giữa cha với kích thước đó.</summary>
        static void Center(RectTransform rt, float w, float h)
        {
            Place(rt, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(w, h));
        }

        /// <summary>Nhận cha, tên, màu, tên sprite bo góc (hoặc null = hình chữ nhật) và cờ chặn chuột; tạo Image.</summary>
        static Image Img(Transform parent, string name, Color color, string sprite = null, bool raycast = true)
        {
            RectTransform rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            Sprite s = Spr(sprite);
            if (s != null) { img.sprite = s; img.type = sprite == "Circle" ? Image.Type.Simple : Image.Type.Sliced; }
            img.raycastTarget = raycast;
            return img;
        }

        /// <summary>Nhận cha, tên, chữ, cỡ, màu, căn lề và kiểu chữ; tạo Text không chặn chuột.</summary>
        static Text Txt(Transform parent, string name, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            RectTransform rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor; t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false;
            return t;
        }

        /// <summary>Nhận object, rộng ưu tiên, cao ưu tiên, giãn ngang; thêm LayoutElement (giá trị âm = bỏ qua).</summary>
        static LayoutElement Le(GameObject go, float w = -1, float h = -1, float flexW = -1)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            return le;
        }

        /// <summary>Nhận nút và 3 màu; đặt nền trắng và tô trạng thái bằng ColorBlock (kiểu ModernUI), dùng cho nút KHÔNG bị code đổi màu.</summary>
        static void StyleFixed(Button b, Color normal, Color hover, Color pressed)
        {
            ((Image)b.targetGraphic).color = Color.white;
            b.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = b.colors;
            cb.normalColor = normal; cb.highlightedColor = hover; cb.selectedColor = normal; cb.pressedColor = pressed;
            cb.disabledColor = new Color(normal.r, normal.g, normal.b, normal.a * .35f);
            cb.colorMultiplier = 1f; cb.fadeDuration = .08f;
            b.colors = cb;
        }

        /// <summary>Nhận nút; đặt ColorBlock xám nhẹ để màu nền (do code đổi theo trạng thái) vẫn nhấp nháy khi rê chuột/bấm.</summary>
        static void StyleRecolor(Button b)
        {
            b.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = b.colors;
            cb.normalColor = new Color(.94f, .94f, .94f, 1f); cb.highlightedColor = Color.white; cb.selectedColor = new Color(.94f, .94f, .94f, 1f);
            cb.pressedColor = new Color(.78f, .78f, .78f, 1f); cb.colorMultiplier = 1f; cb.fadeDuration = .08f;
            b.colors = cb;
        }

        /// <summary>Nhận cha, tên, chữ, tên sprite, rộng, cao và cờ giữ nền; tạo nút bo góc có chữ giữa (nền trắng, tô bằng StyleFixed sau).</summary>
        static Button Btn(Transform parent, string name, string label, string sprite, float w, float h, out Text text, int fontSize = 20)
        {
            Image img = Img(parent, name, Color.white, sprite);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            if (w >= 0 || h >= 0) Le(img.gameObject, w, h);
            text = Txt(img.transform, "Label", label, fontSize, ModernUI.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform, 12, 0, 12, 0);
            return b;
        }

        /// <summary>Nhận đối tượng, tên trường serialize và giá trị; gán trường bằng SerializedObject (object, mảng object hoặc chuỗi).</summary>
        static void Assign(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Nhận đối tượng, tên trường mảng và các object; gán mảng tham chiếu.</summary>
        static void AssignArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Nhận canvas gốc; thêm Canvas phủ màn hình, co giãn theo 1920x1080.</summary>
        static Canvas MakeCanvas(string name, Transform parent, int order)
        {
            RectTransform rt = Rect(name, parent);
            var canvas = rt.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = rt.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            rt.gameObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ─────────────── Dựng prefab ───────────────

        /// <summary>Không nhận tham số; dựng toàn bộ cây object của prefab và gán các trường của SettingsMenu; trả object gốc (chưa lưu).</summary>
        static GameObject BuildRoot()
        {
            var root = new GameObject("PF_SettingsMenu");
            var menu = root.AddComponent<SettingsMenu>();
            root.AddComponent<UiFontApplier>();

            BuildGear(root.transform, menu);
            BuildWindow(root.transform, menu);
            return root;
        }

        /// <summary>Nhận cha và SettingsMenu; dựng nút bánh răng tròn góc phải trên kèm chữ F10 rồi gán trường.</summary>
        static void BuildGear(Transform parent, SettingsMenu menu)
        {
            Canvas gc = MakeCanvas("GearCanvas", parent, 4000);
            Image ring = Img(gc.transform, "GearRing", ModernUI.Stroke, "Circle", false);
            Place(ring.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-26, -26), new Vector2(58, 58));
            Text unused;
            Image bg = Img(ring.transform, "Btn_Gear", Color.white, "Circle");
            Stretch(bg.rectTransform, 2, 2, 2, 2);
            var button = bg.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            StyleFixed(button, new Color(ModernUI.SurfaceHigh.r, ModernUI.SurfaceHigh.g, ModernUI.SurfaceHigh.b, .92f), ModernUI.SurfaceHover, ModernUI.Surface);
            Image icon = Img(bg.transform, "Icon", ModernUI.TextPrimary, null, false);
            Sprite gear = AssetDatabase.LoadAssetAtPath<Sprite>(GearPngPath);
            icon.sprite = gear;
            Stretch(icon.rectTransform, 12, 12, 12, 12);
            Text hint = Txt(gc.transform, "KeyHint", "F10", 13, ModernUI.TextDim, TextAnchor.UpperCenter, FontStyle.Bold);
            Place(hint.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-26, -88), new Vector2(58, 20));
            Assign(menu, "gearRoot", gc.gameObject);
            Assign(menu, "gearButton", button);
        }

        /// <summary>Nhận cha và SettingsMenu; dựng cửa sổ: nền mờ, khung, đầu trang, tab, 4 trang, chân trang, hộp xác nhận rồi gán trường.</summary>
        static void BuildWindow(Transform parent, SettingsMenu menu)
        {
            Canvas mc = MakeCanvas("MenuCanvas", parent, 5000);
            Assign(menu, "menuRoot", mc.gameObject);
            Image dim = Img(mc.transform, "Dim", new Color(0f, 0f, 0f, .72f));
            Stretch(dim.rectTransform);
            Image border = Img(mc.transform, "WindowBorder", ModernUI.Stroke, "Round18");
            Center(border.rectTransform, WindowW + 4, WindowH + 4);
            Image window = Img(border.transform, "Window", new Color32(15, 19, 26, 255), "Round16");
            Stretch(window.rectTransform, 2, 2, 2, 2);

            // Đầu trang
            RectTransform header = Rect("Header", window.transform);
            Place(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1), Vector2.zero, new Vector2(0, HeaderH));
            Image bar = Img(header, "AccentBar", ModernUI.Accent, "Round2", false);
            Place(bar.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(24, 0), new Vector2(4, 46));
            Text title = Txt(header, "Title", "CÀI ĐẶT", 30, ModernUI.TextPrimary, TextAnchor.LowerLeft, FontStyle.Bold);
            Stretch(title.rectTransform, 44, 38, 100, 8);
            Text subtitle = Txt(header, "Subtitle", "Tùy chỉnh trải nghiệm chơi của bạn", 16, ModernUI.TextDim, TextAnchor.UpperLeft);
            Stretch(subtitle.rectTransform, 44, 10, 100, 50);
            Text closeText;
            Button close = Btn(header, "Btn_Close", "×", "Round8", 48, 48, out closeText, 32);
            StyleFixed(close, new Color(1, 1, 1, 0), new Color(1, 1, 1, .08f), new Color(1, 1, 1, .14f));
            Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -18), new Vector2(48, 48));
            Image headLine = Img(header, "Divider", ModernUI.Stroke, null, false);
            Place(headLine.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, 0), Vector2.zero, new Vector2(0, 1));

            // Cột tab
            RectTransform side = Rect("Sidebar", window.transform);
            side.anchorMin = new Vector2(0, 0); side.anchorMax = new Vector2(0, 1); side.pivot = new Vector2(0, .5f);
            side.offsetMin = new Vector2(0, FooterH); side.offsetMax = new Vector2(SideW, -HeaderH);
            var sl = side.gameObject.AddComponent<VerticalLayoutGroup>();
            sl.padding = new RectOffset(18, 18, 24, 0); sl.spacing = 8;
            sl.childControlWidth = true; sl.childControlHeight = true; sl.childForceExpandWidth = true; sl.childForceExpandHeight = false;
            var tabButtons = new Object[4]; var tabIndicators = new Object[4]; var tabLabels = new Object[4]; var pages = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                Text label;
                Button b = Btn(side, "Tab_" + i, TabNames[i], "Round8", -1, 54, out label, 20);
                StyleRecolor(b);
                label.alignment = TextAnchor.MiddleLeft;
                Stretch(label.rectTransform, 26, 0, 12, 0);
                Image ind = Img(b.transform, "Indicator", ModernUI.Accent, "Round2", false);
                ind.rectTransform.anchorMin = new Vector2(0, 0); ind.rectTransform.anchorMax = new Vector2(0, 1); ind.rectTransform.pivot = new Vector2(0, .5f);
                ind.rectTransform.offsetMin = new Vector2(6, 14); ind.rectTransform.offsetMax = new Vector2(10, -14);
                tabButtons[i] = b; tabIndicators[i] = ind; tabLabels[i] = label;
            }
            Image sideLine = Img(window.transform, "SideDivider", ModernUI.Stroke, null, false);
            sideLine.rectTransform.anchorMin = new Vector2(0, 0); sideLine.rectTransform.anchorMax = new Vector2(0, 1);
            sideLine.rectTransform.offsetMin = new Vector2(SideW, FooterH); sideLine.rectTransform.offsetMax = new Vector2(SideW + 1, -HeaderH);

            // Nội dung: 4 trang cuộn
            RectTransform content = Rect("Content", window.transform);
            Stretch(content, SideW + 28, FooterH + 8, 28, HeaderH + 8);
            Text versionText;
            pages[0] = BuildPage(content, "Page_0", c => BuildAudioPage(c)).gameObject;
            pages[1] = BuildPage(content, "Page_1", c => BuildGraphicsPage(c)).gameObject;
            pages[2] = BuildPage(content, "Page_2", c => BuildControlsPage(c)).gameObject;
            Text[] vt = new Text[1];
            pages[3] = BuildPage(content, "Page_3", c => BuildSystemPage(c, vt)).gameObject;
            versionText = vt[0];
            for (int i = 1; i < 4; i++) ((GameObject)pages[i]).SetActive(false);

            // Chân trang
            RectTransform footer = Rect("Footer", window.transform);
            Place(footer, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, 0), Vector2.zero, new Vector2(0, FooterH));
            Image footLine = Img(footer, "Divider", ModernUI.Stroke, null, false);
            Place(footLine.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1), Vector2.zero, new Vector2(0, 1));
            Text toast = Txt(footer, "Toast", "", 17, ModernUI.Accent, TextAnchor.MiddleLeft);
            Stretch(toast.rectTransform, 36, 0, 470, 0);
            RectTransform buttons = Rect("Buttons", footer);
            Place(buttons, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-28, 0), new Vector2(430, 48));
            var hl = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 14; hl.childAlignment = TextAnchor.MiddleRight;
            hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            Text resetLabel, doneLabel;
            Button reset = Btn(buttons, "Btn_Reset", "Khôi phục âm thanh", "Round8", 250, 48, out resetLabel);
            StyleFixed(reset, ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255));
            Button done = Btn(buttons, "Btn_Done", "Xong", "Round8", 150, 48, out doneLabel);
            StyleFixed(done, ModernUI.Accent, ModernUI.AccentHover, ModernUI.AccentPressed);
            doneLabel.color = ModernUI.AccentText;

            // Hộp xác nhận
            Image veil = Img(window.transform, "ConfirmVeil", new Color(0f, 0f, 0f, .6f), "Round16");
            Stretch(veil.rectTransform);
            Image card = Img(veil.transform, "Card", ModernUI.SurfaceHigh, "Round14");
            Center(card.rectTransform, 560, 270);
            Text cTitle = Txt(card.transform, "Title", "", 26, ModernUI.TextPrimary, TextAnchor.UpperLeft, FontStyle.Bold);
            Stretch(cTitle.rectTransform, 32, 150, 32, 28);
            Text cBody = Txt(card.transform, "Body", "", 18, ModernUI.TextDim, TextAnchor.UpperLeft);
            Stretch(cBody.rectTransform, 32, 90, 32, 78);
            RectTransform row = Rect("Buttons", card.transform);
            row.anchorMin = new Vector2(0, 0); row.anchorMax = new Vector2(1, 0); row.pivot = new Vector2(.5f, 0);
            row.offsetMin = new Vector2(32, 26); row.offsetMax = new Vector2(-32, 80);
            var rh = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rh.spacing = 14; rh.childAlignment = TextAnchor.MiddleRight;
            rh.childControlWidth = true; rh.childControlHeight = true; rh.childForceExpandWidth = false; rh.childForceExpandHeight = false;
            Text noLabel, yesLabel;
            Button no = Btn(row, "Btn_No", "Hủy", "Round8", 140, 48, out noLabel);
            StyleFixed(no, ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255));
            noLabel.transform.parent.GetComponent<Image>().color = Color.white;
            Button yes = Btn(row, "Btn_Yes", "Xác nhận", "Round8", 190, 48, out yesLabel);
            StyleRecolor(yes);
            ((Image)yes.targetGraphic).color = ModernUI.Danger;
            yesLabel.color = Color.white;
            veil.gameObject.SetActive(false);

            // Gán trường SettingsMenu
            Assign(menu, "closeButton", close);
            Assign(menu, "doneButton", done);
            Assign(menu, "toast", toast);
            Assign(menu, "versionText", versionText);
            AssignArray(menu, "tabButtons", tabButtons);
            AssignArray(menu, "tabIndicators", tabIndicators);
            AssignArray(menu, "tabLabels", tabLabels);
            AssignArray(menu, "pages", pages);
            Assign(menu, "resetButton", reset);
            Assign(menu, "resetLabel", resetLabel);
            Assign(menu, "confirmRoot", veil.gameObject);
            Assign(menu, "confirmTitle", cTitle);
            Assign(menu, "confirmBody", cBody);
            Assign(menu, "confirmYes", yes);
            Assign(menu, "confirmYesLabel", yesLabel);
            Assign(menu, "confirmNo", no);
        }

        /// <summary>Nhận vùng nội dung, tên trang và hàm điền dòng; tạo trang cuộn dọc (viewport + content xếp dòng tự động) rồi gọi hàm điền; trả RectTransform của trang.</summary>
        static RectTransform BuildPage(RectTransform parent, string name, System.Action<RectTransform> fill)
        {
            RectTransform page = Rect(name, parent);
            Stretch(page);
            var catcher = page.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            RectTransform viewport = Rect("Viewport", page);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1);
            content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
            var v = content.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(0, 18, 0, 24); v.spacing = 0;
            v.childControlWidth = true; v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = page.gameObject.AddComponent<ScrollRect>();
            scroll.content = content; scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;
            fill(content);
            return page;
        }

        // ─────────────── Dòng ───────────────

        /// <summary>Nhận trang và chữ; thêm đề mục nhóm nhỏ màu nhấn.</summary>
        static void Header(RectTransform c, string text)
        {
            Text t = Txt(c, "Header_" + text, text.ToUpperInvariant(), 14, ModernUI.Accent, TextAnchor.LowerLeft, FontStyle.Bold);
            Le(t.gameObject, -1, 48);
        }

        /// <summary>Nhận trang và chữ; thêm dòng ghi chú mờ tự xuống dòng.</summary>
        static void Note(RectTransform c, string text)
        {
            Text t = Txt(c, "Note", text, 15, ModernUI.TextDim, TextAnchor.UpperLeft);
            Le(t.gameObject, -1, -1).minHeight = 40;
        }

        /// <summary>Nhận trang, nhãn, bề rộng vùng điều khiển và chiều cao; tạo một dòng (nhãn trái, vùng điều khiển phải, đường kẻ dưới), trả vùng điều khiển.</summary>
        static RectTransform Row(RectTransform c, string label, float controlWidth, float height = 60f)
        {
            RectTransform row = Rect("Row_" + label.Split('\n')[0], c);
            Le(row.gameObject, -1, height);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft; h.spacing = 16;
            h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            h.padding = new RectOffset(4, 4, 0, 0);
            Text t = Txt(row, "Label", label, 20, ModernUI.TextPrimary);
            Le(t.gameObject, -1, -1, 1);
            RectTransform control = Rect("Control", row);
            Le(control.gameObject, controlWidth, -1);
            var ch = control.gameObject.AddComponent<HorizontalLayoutGroup>();
            ch.childAlignment = TextAnchor.MiddleRight; ch.spacing = 12;
            ch.childControlWidth = true; ch.childControlHeight = true; ch.childForceExpandWidth = false; ch.childForceExpandHeight = false;
            Image line = Img(row, "Divider", new Color(1f, 1f, 1f, .06f), null, false);
            Place(line.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, 0), Vector2.zero, new Vector2(0, 1));
            line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return control;
        }

        /// <summary>Nhận trang, nhãn, mã dòng và định dạng; thêm dòng thanh trượt kèm số hiển thị.</summary>
        static void SliderRow(RectTransform c, string label, string id, SliderFormat format)
        {
            RectTransform control = Row(c, label, 400);
            RectTransform rowRt = (RectTransform)control.parent;
            RectTransform root = Rect("Slider", control);
            Le(root.gameObject, 300, 28);
            Image track = Img(root, "Track", ModernUI.SurfaceHigh, "Round3", false);
            Place(track.rectTransform, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            track.rectTransform.offsetMin = new Vector2(0, -3); track.rectTransform.offsetMax = new Vector2(0, 3);
            RectTransform fillArea = Rect("Fill Area", root);
            fillArea.anchorMin = new Vector2(0, .5f); fillArea.anchorMax = new Vector2(1, .5f);
            fillArea.offsetMin = new Vector2(0, -3); fillArea.offsetMax = new Vector2(0, 3);
            Image fill = Img(fillArea, "Fill", ModernUI.Accent, "Round3", false);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(.5f, 1);
            fill.rectTransform.offsetMin = Vector2.zero; fill.rectTransform.offsetMax = Vector2.zero;
            RectTransform handleArea = Rect("Handle Slide Area", root);
            Stretch(handleArea, 10, 0, 10, 0);
            Image handle = Img(handleArea, "Handle", Color.white, "Circle");
            Place(handle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(20, 20));
            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.value = .5f;
            ColorBlock cb = slider.colors;
            cb.normalColor = Color.white; cb.highlightedColor = ModernUI.AccentHover; cb.pressedColor = ModernUI.Accent; cb.selectedColor = Color.white;
            slider.colors = cb;
            Text value = Txt(control, "Value", "50%", 19, ModernUI.TextPrimary, TextAnchor.MiddleRight, FontStyle.Bold);
            Le(value.gameObject, 76, -1);
            var row = rowRt.gameObject.AddComponent<SettingsSliderRow>();
            row.id = id; row.slider = slider; row.valueText = value; row.format = format;
        }

        /// <summary>Nhận trang, nhãn và mã dòng; thêm dòng công tắc dạng viên thuốc.</summary>
        static void ToggleRow(RectTransform c, string label, string id)
        {
            RectTransform control = Row(c, label, 400);
            RectTransform rowRt = (RectTransform)control.parent;
            RectTransform root = Rect("Switch", control);
            Le(root.gameObject, 54, 30);
            Image bg = Img(root, "Background", ModernUI.SurfaceHigh, "Round14");
            Stretch(bg.rectTransform);
            Image knob = Img(bg.transform, "Knob", ModernUI.TextDim, "Circle", false);
            Place(knob.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(4, 0), new Vector2(22, 22));
            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = bg; toggle.transition = Selectable.Transition.None; toggle.isOn = false;
            var visual = root.gameObject.AddComponent<ModernSwitchVisual>();
            var so = new SerializedObject(visual);
            so.FindProperty("toggle").objectReferenceValue = toggle;
            so.FindProperty("background").objectReferenceValue = bg;
            so.FindProperty("knob").objectReferenceValue = knob;
            so.ApplyModifiedPropertiesWithoutUndo();
            var row = rowRt.gameObject.AddComponent<SettingsToggleRow>();
            row.id = id; row.toggle = toggle; row.visual = visual;
        }

        /// <summary>Nhận trang, nhãn, mã dòng và giá trị mẫu; thêm dòng bộ chọn «‹ giá trị ›» (danh sách lựa chọn do code điền lúc chạy).</summary>
        static void SelectorRow(RectTransform c, string label, string id, string sample)
        {
            RectTransform control = Row(c, label, 400);
            RectTransform rowRt = (RectTransform)control.parent;
            RectTransform root = Rect("Selector", control);
            Le(root.gameObject, 320, 40);
            var h = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter; h.spacing = 6;
            h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = true;
            Text pl, nl;
            Button prev = Btn(root, "Btn_Prev", "‹", "Round8", 40, 40, out pl);
            StyleFixed(prev, ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255));
            Image box = Img(root, "Value", ModernUI.SurfaceHigh, "Round8", false);
            Le(box.gameObject, -1, -1, 1);
            Text value = Txt(box.transform, "Text", sample, 19, ModernUI.TextPrimary, TextAnchor.MiddleCenter);
            Stretch(value.rectTransform, 8, 0, 8, 0);
            Button next = Btn(root, "Btn_Next", "›", "Round8", 40, 40, out nl);
            StyleFixed(next, ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255));
            var row = rowRt.gameObject.AddComponent<SettingsSelectorRow>();
            row.id = id; row.previous = prev; row.next = next; row.valueText = value;
        }

        /// <summary>Nhận trang và hành động; thêm dòng đổi phím: nhãn tiếng Việt từ GameKeys, nút hiện tên phím mặc định.</summary>
        static void KeyRow(RectTransform c, GameAction action)
        {
            RectTransform control = Row(c, GameKeys.Label(action), 400);
            RectTransform rowRt = (RectTransform)control.parent;
            Text label;
            Button b = Btn(control, "Btn_Key", "?", "Round8", 190, 42, out label);
            StyleRecolor(b);
            ((Image)b.targetGraphic).color = ModernUI.SurfaceHigh;
            var row = rowRt.gameObject.AddComponent<SettingsKeyRow>();
            row.action = action; row.button = b; row.label = label;
        }

        /// <summary>Nhận trang, tiêu đề, mô tả, chữ nút, mã dòng và kiểu nút (0 thường, 1 nguy hiểm); thêm dòng hành động cao 84 có nút bên phải.</summary>
        static void ActionRow(RectTransform c, string title, string description, string buttonText, string id, bool danger)
        {
            RectTransform control = Row(c, title + "\n<size=15><color=#8E9AAB>" + description + "</color></size>", 240, 84);
            RectTransform rowRt = (RectTransform)control.parent;
            Text label;
            Button b = Btn(control, "Btn_Action", buttonText, "Round8", 220, 44, out label);
            if (danger) { StyleFixed(b, ModernUI.Danger, ModernUI.DangerHover, new Color32(180, 52, 56, 255)); label.color = Color.white; }
            else StyleFixed(b, ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255));
            var row = rowRt.gameObject.AddComponent<SettingsActionRow>();
            row.id = id; row.button = b;
        }

        /// <summary>Nhận trang; thêm âm lượng tổng và các kênh nhạc, hiệu ứng, môi trường, giọng nói.</summary>
        static void BuildAudioPage(RectTransform c)
        {
            Header(c, "Âm lượng");
            SliderRow(c, "Âm lượng tổng", SettingsRowIds.MasterVolume, SliderFormat.Percent);
            SliderRow(c, "Nhạc nền", SettingsRowIds.MusicVolume, SliderFormat.Percent);
            SliderRow(c, "Hiệu ứng âm thanh", SettingsRowIds.SfxVolume, SliderFormat.Percent);
            SliderRow(c, "Âm thanh môi trường", SettingsRowIds.AmbienceVolume, SliderFormat.Percent);
            SliderRow(c, "Giọng nói đồng đội", SettingsRowIds.VoiceVolume, SliderFormat.Percent);
            Note(c, "Âm lượng tổng áp dụng cho toàn bộ game, các kênh còn lại được nhân thêm lên trên đó.");
        }

        /// <summary>Nhận trang; thêm độ phân giải, toàn màn hình, VSync, giới hạn FPS và chất lượng đồ họa.</summary>
        static void BuildGraphicsPage(RectTransform c)
        {
            Header(c, "Hiển thị");
            SelectorRow(c, "Độ phân giải", SettingsRowIds.Resolution, "1920 × 1080");
            ToggleRow(c, "Toàn màn hình", SettingsRowIds.Fullscreen);
            ToggleRow(c, "Đồng bộ dọc (VSync)", SettingsRowIds.VSync);
            SelectorRow(c, "Giới hạn khung hình", SettingsRowIds.FpsCap, "60 FPS");
            Note(c, "Khi bật VSync, tốc độ khung hình bám theo tần số quét của màn hình và bỏ qua giới hạn FPS.");
            Header(c, "Chất lượng");
            SelectorRow(c, "Chất lượng đồ họa", SettingsRowIds.Quality, "Cao");
            Note(c, "Mức chất lượng cao đẹp hơn nhưng cần máy mạnh hơn. Độ phân giải và toàn màn hình chỉ có tác dụng đầy đủ ở bản build.");
        }

        /// <summary>Nhận trang; thêm độ nhạy chuột, đảo trục nhìn và 13 dòng phím tắt chia nhóm theo GameKeys.Group.</summary>
        static void BuildControlsPage(RectTransform c)
        {
            Header(c, "Chuột");
            SliderRow(c, "Độ nhạy chuột", SettingsRowIds.Sensitivity, SliderFormat.Multiplier);
            ToggleRow(c, "Đảo trục nhìn lên/xuống", SettingsRowIds.InvertY);
            string last = null;
            foreach (GameAction action in GameKeys.All)
            {
                string group = GameKeys.Group(action);
                if (group != last) { Header(c, "Phím tắt • " + group); last = group; }
                KeyRow(c, action);
            }
            Note(c, "Bấm vào ô phím rồi nhấn phím mới. Nếu phím đó đang dùng cho hành động khác, hai hành động sẽ đổi phím cho nhau. Esc, phím 1–4 (đổi vai) và F1–F12 được giữ riêng.");
        }

        /// <summary>Nhận trang và mảng nhận ô chữ phiên bản; thêm về menu chính, khôi phục mọi cài đặt, thoát game và thông tin.</summary>
        static void BuildSystemPage(RectTransform c, Text[] versionOut)
        {
            Header(c, "Phiên chơi");
            ActionRow(c, "Về menu chính", "Rời ván đang chơi và quay lại màn hình chính.", "Về menu chính", SettingsRowIds.BackToMenu, false);
            ActionRow(c, "Khôi phục mọi cài đặt", "Âm thanh, đồ họa, độ nhạy và phím tắt về mặc định.", "Khôi phục tất cả", SettingsRowIds.ResetAll, false);
            Header(c, "Thoát");
            ActionRow(c, "Thoát game", "Đóng The Return và quay về máy tính.", "Thoát game", SettingsRowIds.QuitGame, true);
            Header(c, "Thông tin");
            RectTransform v = Row(c, "Phiên bản", 300);
            Text version = Txt(v, "Value", "Bản thử", 19, ModernUI.TextDim, TextAnchor.MiddleRight);
            Le(version.gameObject, -1, -1, 1);
            versionOut[0] = version;
            RectTransform s = Row(c, "Lưu cài đặt", 300);
            Text saved = Txt(s, "Value", "Tự động trên máy này", 19, ModernUI.TextDim, TextAnchor.MiddleRight);
            Le(saved.gameObject, -1, -1, 1);
        }
    }
}
