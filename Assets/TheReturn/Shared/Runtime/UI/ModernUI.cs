/*
 * Mục đích: Bộ giao diện dùng chung phong cách tối giản hiện đại cho The Return: bảng màu, sprite bo góc, và các hàm dựng/tô kiểu
 * panel, chữ, nút, thanh trượt, công tắc, bộ chọn mũi tên. Dùng cho MainMenu, Sảnh chờ, HUD MapSchoolF và menu Cài đặt để cả game một phong cách.
 * Không chứa luật game. Sprite lấy từ Resources/ModernUI (PNG đã bake sẵn) nếu có, không thì tự vẽ lúc chạy.
 * Hàm: Round; Circle; Rect; Stretch; Box; Label; Button; StyleButton; StyleLabel; StylePanel; Slider; Switch; Selector; RowContainer; MakeRoundedTexture.
 * Kiểu: ModernButtonStyle, ModernUI, ModernSwitchVisual, ModernSelector.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Kiểu nút: Primary (nhấn mạnh), Secondary (mặc định), Ghost (trong suốt), Danger (nguy hiểm).</summary>
    public enum ModernButtonStyle { Primary, Secondary, Ghost, Danger }

    /// <summary>Bảng màu và hàm dựng UI phong cách tối giản hiện đại.</summary>
    public static class ModernUI
    {
        public static readonly Color Bg = new Color32(10, 13, 18, 255);
        public static readonly Color Surface = new Color32(17, 21, 28, 245);
        public static readonly Color SurfaceHigh = new Color32(27, 33, 43, 255);
        public static readonly Color SurfaceHover = new Color32(38, 47, 61, 255);
        public static readonly Color Stroke = new Color32(44, 54, 68, 255);
        public static readonly Color TextPrimary = new Color32(234, 240, 246, 255);
        public static readonly Color TextDim = new Color32(142, 154, 171, 255);
        public static readonly Color Accent = new Color32(64, 214, 179, 255);
        public static readonly Color AccentHover = new Color32(104, 232, 201, 255);
        public static readonly Color AccentPressed = new Color32(40, 168, 140, 255);
        public static readonly Color AccentText = new Color32(6, 28, 24, 255);
        public static readonly Color Danger = new Color32(229, 72, 77, 255);
        public static readonly Color DangerHover = new Color32(244, 106, 110, 255);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.62f);

        static Font font;
        static readonly Dictionary<int, Sprite> rounded = new Dictionary<int, Sprite>();
        static Sprite circle;

        /// <summary>Không nhận tham số; trả font chữ dùng chung (hỗ trợ tiếng Việt).</summary>
        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    // Ưu tiên font hệ thống hiện đại (đều có tiếng Việt); thiếu thì dùng font mặc định của Unity.
                    try { font = Font.CreateDynamicFontFromOSFont(new[] { "Bahnschrift", "Segoe UI", "Roboto", "Arial" }, 16); } catch (System.Exception) { font = null; }
                    if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return font;
            }
        }

        /// <summary>Nhận bán kính góc (px); trả sprite chữ nhật bo góc cắt 9 mảnh (nạp từ Resources/ModernUI nếu đã bake, không thì tự vẽ).</summary>
        public static Sprite Round(int radius)
        {
            Sprite s;
            if (rounded.TryGetValue(radius, out s) && s != null) return s;
            s = Resources.Load<Sprite>("ModernUI/Round" + radius);
            if (s == null)
            {
                int size = radius * 2 + 2;
                Texture2D tex = MakeRoundedTexture(size, radius);
                tex.hideFlags = HideFlags.DontSave;
                s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
                s.hideFlags = HideFlags.DontSave;
            }
            rounded[radius] = s;
            return s;
        }

        /// <summary>Không nhận tham số; trả sprite hình tròn (nút trượt, chấm).</summary>
        public static Sprite Circle()
        {
            if (circle != null) return circle;
            circle = Resources.Load<Sprite>("ModernUI/Circle");
            if (circle == null)
            {
                Texture2D tex = MakeRoundedTexture(64, 32);
                tex.hideFlags = HideFlags.DontSave;
                circle = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 100f);
                circle.hideFlags = HideFlags.DontSave;
            }
            return circle;
        }

        /// <summary>Nhận cạnh ảnh (px) và bán kính góc; trả texture trắng bo góc có khử răng cưa (alpha theo khoảng cách tới viền).</summary>
        public static Texture2D MakeRoundedTexture(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float half = size * .5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x + .5f - half) - (half - radius);
                    float py = Mathf.Abs(y + .5f - half) - (half - radius);
                    float outside = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude;
                    float inside = Mathf.Min(Mathf.Max(px, py), 0f);
                    float d = outside + inside - radius;
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(.5f - d) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Nhận cha và tên; tạo RectTransform con, trả về.</summary>
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Nhận RectTransform và lề trái/dưới/phải/trên; kéo giãn kín cha với lề đó.</summary>
        public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom); rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Nhận cha, tên, màu và bán kính góc (0 = vuông); tạo Image nền, trả về.</summary>
        public static Image Box(Transform parent, string name, Color color, int radius = 0)
        {
            RectTransform rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (radius > 0) { img.sprite = Round(radius); img.type = Image.Type.Sliced; }
            return img;
        }

        /// <summary>Nhận cha, chữ, cỡ chữ, màu, căn lề và kiểu chữ; tạo Text không chặn chuột, trả về.</summary>
        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            RectTransform rt = Rect(parent, "Label");
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor; t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false;
            return t;
        }

        /// <summary>Nhận cha, chữ, kiểu, rộng và cao (0 = để layout tự chia); tạo nút bo góc có hiệu ứng hover/nhấn, trả về.</summary>
        public static Button Button(Transform parent, string text, ModernButtonStyle style, float width, float height)
        {
            Image img = Box(parent, "Button_" + text, Color.white, 8);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var le = img.gameObject.AddComponent<LayoutElement>();
            if (width > 0) le.preferredWidth = width;
            if (height > 0) le.preferredHeight = height;
            Text label = Label(img.transform, text, 20, TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(label.rectTransform, 12, 0, 12, 0);
            StyleButton(button, style);
            return button;
        }

        /// <summary>Nhận nút (có sẵn hoặc mới) và kiểu; tô lại nền bo góc, màu các trạng thái và màu chữ.</summary>
        public static void StyleButton(Button button, ModernButtonStyle style)
        {
            if (button == null) return;
            var img = button.targetGraphic as Image;
            if (img == null) img = button.GetComponent<Image>();
            if (img != null) { img.sprite = Round(8); img.type = Image.Type.Sliced; img.color = Color.white; button.targetGraphic = img; }
            Color normal, hover, pressed, text;
            switch (style)
            {
                case ModernButtonStyle.Primary: normal = Accent; hover = AccentHover; pressed = AccentPressed; text = AccentText; break;
                case ModernButtonStyle.Danger: normal = Danger; hover = DangerHover; pressed = new Color32(180, 52, 56, 255); text = Color.white; break;
                case ModernButtonStyle.Ghost: normal = new Color(1f, 1f, 1f, 0f); hover = new Color(1f, 1f, 1f, .08f); pressed = new Color(1f, 1f, 1f, .14f); text = TextPrimary; break;
                default: normal = SurfaceHigh; hover = SurfaceHover; pressed = new Color32(20, 25, 33, 255); text = TextPrimary; break;
            }
            button.transition = Selectable.Transition.ColorTint;
            var cb = button.colors;
            cb.normalColor = normal; cb.highlightedColor = hover; cb.selectedColor = normal; cb.pressedColor = pressed;
            cb.disabledColor = new Color(normal.r, normal.g, normal.b, normal.a * .35f);
            cb.colorMultiplier = 1f; cb.fadeDuration = .08f;
            button.colors = cb;
            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null) { label.font = Font; label.color = text; }
        }

        /// <summary>Nhận Text có sẵn, cỡ, màu (mặc định chữ chính) và kiểu chữ; áp font dùng chung.</summary>
        public static void StyleLabel(Text text, int size, Color? color = null, FontStyle style = FontStyle.Normal)
        {
            if (text == null) return;
            text.font = Font; text.fontSize = size; text.fontStyle = style;
            text.color = color ?? TextPrimary;
        }

        /// <summary>Nhận Image có sẵn, màu và bán kính góc; đặt nền bo góc cắt 9 mảnh.</summary>
        public static void StylePanel(Image image, Color color, int radius)
        {
            if (image == null) return;
            image.sprite = radius > 0 ? Round(radius) : null;
            image.type = radius > 0 ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
        }

        /// <summary>Nhận cha, khoảng giá trị, giá trị đầu và bề rộng; tạo thanh trượt mảnh có nút tròn, trả về.</summary>
        public static Slider Slider(Transform parent, float min, float max, float value, float width)
        {
            RectTransform root = Rect(parent, "Slider");
            var le = root.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width; le.preferredHeight = 28;

            Image track = Box(root, "Track", SurfaceHigh, 3);
            track.rectTransform.anchorMin = new Vector2(0, .5f); track.rectTransform.anchorMax = new Vector2(1, .5f);
            track.rectTransform.offsetMin = new Vector2(0, -3); track.rectTransform.offsetMax = new Vector2(0, 3);

            RectTransform fillArea = Rect(root, "Fill Area");
            fillArea.anchorMin = new Vector2(0, .5f); fillArea.anchorMax = new Vector2(1, .5f);
            fillArea.offsetMin = new Vector2(0, -3); fillArea.offsetMax = new Vector2(0, 3);
            Image fill = Box(fillArea, "Fill", Accent, 3);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(.5f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero; fill.rectTransform.offsetMax = Vector2.zero;

            RectTransform handleArea = Rect(root, "Handle Slide Area");
            // Slider kéo giãn nút theo chiều cao vùng này, nên vùng chỉ cao 20 để nút tròn 20x20 (không bị méo thành bầu dục).
            handleArea.anchorMin = new Vector2(0, .5f); handleArea.anchorMax = new Vector2(1, .5f);
            handleArea.offsetMin = new Vector2(10, -10); handleArea.offsetMax = new Vector2(-10, 10);
            Image handle = Box(handleArea, "Handle", Color.white);
            handle.sprite = Circle();
            handle.rectTransform.anchorMin = new Vector2(.5f, .5f); handle.rectTransform.anchorMax = new Vector2(.5f, .5f);
            handle.rectTransform.sizeDelta = new Vector2(20, 20);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min; slider.maxValue = max; slider.value = value;
            var cb = slider.colors;
            cb.normalColor = Color.white; cb.highlightedColor = AccentHover; cb.pressedColor = Accent; cb.selectedColor = Color.white;
            slider.colors = cb;
            return slider;
        }

        /// <summary>Nhận cha và giá trị đầu; tạo công tắc dạng viên thuốc (tắt xám, bật màu nhấn), trả về.</summary>
        public static Toggle Switch(Transform parent, bool value)
        {
            RectTransform root = Rect(parent, "Switch");
            var le = root.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 54; le.preferredHeight = 30;
            Image bg = Box(root, "Background", SurfaceHigh, 15);
            Stretch(bg.rectTransform);
            Image knob = Box(bg.transform, "Knob", Color.white);
            knob.sprite = Circle();
            knob.rectTransform.anchorMin = knob.rectTransform.anchorMax = new Vector2(0, .5f);
            knob.rectTransform.sizeDelta = new Vector2(22, 22);
            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.transition = Selectable.Transition.None;
            toggle.isOn = value;
            root.gameObject.AddComponent<ModernSwitchVisual>().Bind(toggle, bg, knob);
            return toggle;
        }

        /// <summary>Nhận cha, danh sách lựa chọn, chỉ số đầu và bề rộng; tạo bộ chọn «‹ giá trị ›», trả về.</summary>
        public static ModernSelector Selector(Transform parent, string[] options, int index, float width)
        {
            return new ModernSelector(parent, options, index, width);
        }

        /// <summary>Nhận cha, nhãn và bề rộng vùng điều khiển bên phải; tạo một hàng cài đặt (nhãn trái, vùng điều khiển phải, đường kẻ dưới), trả vùng điều khiển.</summary>
        public static RectTransform RowContainer(Transform parent, string label, float controlWidth)
        {
            RectTransform row = Rect(parent, "Row_" + label);
            var rowLe = row.gameObject.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 60; rowLe.minHeight = 60;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft; h.spacing = 16;
            h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            h.padding = new RectOffset(4, 4, 0, 0);
            Text t = Label(row, label, 20, TextPrimary);
            t.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            RectTransform control = Rect(row, "Control");
            var cle = control.gameObject.AddComponent<LayoutElement>();
            cle.preferredWidth = controlWidth; cle.minWidth = controlWidth;
            var ch = control.gameObject.AddComponent<HorizontalLayoutGroup>();
            ch.childAlignment = TextAnchor.MiddleRight; ch.spacing = 12;
            ch.childControlWidth = true; ch.childControlHeight = true; ch.childForceExpandWidth = false; ch.childForceExpandHeight = false;
            Image line = Box(row, "Divider", new Color(1f, 1f, 1f, .06f));
            line.rectTransform.anchorMin = new Vector2(0, 0); line.rectTransform.anchorMax = new Vector2(1, 0);
            line.rectTransform.offsetMin = Vector2.zero; line.rectTransform.offsetMax = new Vector2(0, 1);
            line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return control;
        }
    }


    /// <summary>Bộ chọn «‹ giá trị ›» cho danh sách lựa chọn ngắn (độ phân giải, chất lượng...).</summary>
    public sealed class ModernSelector
    {
        readonly string[] options;
        readonly Text valueText;
        int index;

        /// <summary>Bắn khi người dùng đổi lựa chọn, kèm chỉ số mới.</summary>
        public event Action<int> Changed;

        /// <summary>Chỉ số lựa chọn hiện tại (đặt từ code không bắn Changed).</summary>
        public int Index
        {
            get { return index; }
            set { index = Mathf.Clamp(value, 0, options.Length - 1); Refresh(); }
        }

        /// <summary>Nhận cha, lựa chọn, chỉ số đầu và bề rộng; dựng hai nút mũi tên và ô giá trị.</summary>
        public ModernSelector(Transform parent, string[] opts, int startIndex, float width)
        {
            options = opts != null && opts.Length > 0 ? opts : new[] { "-" };
            RectTransform root = ModernUI.Rect(parent, "Selector");
            var le = root.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width; le.preferredHeight = 40;
            var h = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter; h.spacing = 6;
            h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = true;
            Button prev = ModernUI.Button(root, "‹", ModernButtonStyle.Secondary, 40, 40);
            Image box = ModernUI.Box(root, "Value", ModernUI.SurfaceHigh, 8);
            box.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            valueText = ModernUI.Label(box.transform, "", 19, ModernUI.TextPrimary, TextAnchor.MiddleCenter);
            ModernUI.Stretch(valueText.rectTransform, 8, 0, 8, 0);
            Button next = ModernUI.Button(root, "›", ModernButtonStyle.Secondary, 40, 40);
            prev.onClick.AddListener(() => Step(-1));
            next.onClick.AddListener(() => Step(1));
            index = Mathf.Clamp(startIndex, 0, options.Length - 1);
            Refresh();
        }

        /// <summary>Nhận bước ±1; đi vòng quanh danh sách, cập nhật chữ và bắn Changed.</summary>
        void Step(int delta)
        {
            index = (index + delta + options.Length) % options.Length;
            Refresh();
            if (Changed != null) Changed(index);
        }

        /// <summary>Không nhận tham số; ghi lại chữ giá trị theo chỉ số hiện tại.</summary>
        void Refresh() { valueText.text = options[index]; }
    }
}
