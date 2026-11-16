/*
 * Mục đích: Bộ kiểu IMGUI (GUISkin) cùng phong cách ModernUI cho các màn hình còn dùng OnGUI (phòng online, chat thoại, thông báo mất kết nối) (T-053).
 * Dùng: bọc OnGUI bằng ModernGui.Begin(out w, out h) ... ModernGui.End(); trong đó w, h là kích thước màn hình ảo (đã co theo độ phân giải, tối thiểu 1080 cao).
 * Kiểu có sẵn: Skin.box (thẻ bo góc), Skin.label, Skin.button / Skin.toggle (nút; bật = màu nhấn), Skin.textField, và các kiểu riêng Title, Dim, Primary, Danger, Pill.
 * Hàm: Begin; End; Skin; Title; Dim; Primary; Danger; Build; Solid; MakeStyle; (tĩnh) Backdrop.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Kiểu IMGUI hiện đại dùng chung; tạo một lần, dùng lại cho mọi OnGUI.</summary>
    public static class ModernGui
    {
        static GUISkin skin;
        static GUIStyle title, dim, primary, danger, pill;
        static GUISkin savedSkin;
        static Matrix4x4 savedMatrix;
        static Texture2D backdrop;

        /// <summary>Không nhận tham số; trả bộ kiểu hiện đại (tạo lần đầu gọi).</summary>
        public static GUISkin Skin { get { if (skin == null) Build(); return skin; } }

        /// <summary>Kiểu chữ tiêu đề lớn đậm.</summary>
        public static GUIStyle Title { get { if (skin == null) Build(); return title; } }

        /// <summary>Kiểu chữ phụ màu xám nhạt, tự xuống dòng.</summary>
        public static GUIStyle Dim { get { if (skin == null) Build(); return dim; } }

        /// <summary>Kiểu nút nhấn mạnh (nền màu nhấn, chữ tối).</summary>
        public static GUIStyle Primary { get { if (skin == null) Build(); return primary; } }

        /// <summary>Kiểu thanh nhỏ bo tròn (một dòng chữ phụ), dùng với GUI.Box.</summary>
        public static GUIStyle Pill { get { if (skin == null) Build(); return pill; } }

        /// <summary>Kiểu nút nguy hiểm (nền đỏ, chữ trắng).</summary>
        public static GUIStyle Danger { get { if (skin == null) Build(); return danger; } }

        /// <summary>Trả hình nền tối chuyển màu (cùng ảnh MainMenu nếu có) để phủ kín màn hình; null nếu chưa có.</summary>
        public static Texture2D Backdrop
        {
            get
            {
                if (backdrop == null) backdrop = Resources.Load<Texture2D>("ModernUI/MenuBackdrop");
                return backdrop;
            }
        }

        /// <summary>Xuất kích thước màn hình ảo (px, cao tối thiểu 1080); đặt kiểu hiện đại và ma trận co theo độ phân giải. Gọi End() khi xong.</summary>
        public static void Begin(out float width, out float height)
        {
            float scale = Mathf.Max(1f, Screen.height / 1080f);
            savedSkin = GUI.skin;
            savedMatrix = GUI.matrix;
            GUI.skin = Skin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            width = Screen.width / scale;
            height = Screen.height / scale;
        }

        /// <summary>Trả lại kiểu và ma trận trước Begin.</summary>
        public static void End()
        {
            GUI.skin = savedSkin;
            GUI.matrix = savedMatrix;
        }

        static void Build()
        {
            skin = ScriptableObject.CreateInstance<GUISkin>();
            skin.hideFlags = HideFlags.HideAndDontSave;
            Font font = ModernUI.Font;
            skin.font = font;

            skin.label = MakeStyle(null, 18, ModernUI.TextPrimary, FontStyle.Normal, TextAnchor.MiddleLeft);
            skin.label.wordWrap = true;
            skin.label.margin = new RectOffset(4, 4, 2, 2);
            skin.label.padding = new RectOffset(2, 2, 2, 2);

            title = new GUIStyle(skin.label) { fontSize = 30, fontStyle = FontStyle.Bold };
            title.margin = new RectOffset(4, 4, 2, 10);
            dim = new GUIStyle(skin.label) { fontSize = 15 };
            dim.normal.textColor = ModernUI.TextDim;

            skin.box = MakeStyle(Solid(16, ModernUI.Surface), 18, ModernUI.TextPrimary, FontStyle.Normal, TextAnchor.UpperLeft);
            skin.box.border = new RectOffset(16, 16, 16, 16);
            skin.box.padding = new RectOffset(24, 24, 22, 22);

            skin.button = MakeButton(ModernUI.SurfaceHigh, ModernUI.SurfaceHover, new Color32(20, 25, 33, 255), ModernUI.TextPrimary, ModernUI.Accent, ModernUI.AccentText);
            skin.toggle = new GUIStyle(skin.button);
            skin.textField = MakeStyle(Solid(8, ModernUI.SurfaceHigh), 18, ModernUI.TextPrimary, FontStyle.Normal, TextAnchor.MiddleLeft);
            skin.textField.border = new RectOffset(8, 8, 8, 8);
            skin.textField.padding = new RectOffset(14, 14, 8, 8);
            skin.textField.margin = new RectOffset(4, 4, 4, 6);
            skin.textField.fixedHeight = 42f;
            skin.textField.focused.background = Solid(8, new Color32(34, 42, 54, 255));
            skin.textField.focused.textColor = ModernUI.TextPrimary;
            skin.settings.cursorColor = ModernUI.Accent;
            skin.settings.selectionColor = new Color(ModernUI.Accent.r, ModernUI.Accent.g, ModernUI.Accent.b, .35f);

            pill = MakeStyle(Solid(14, new Color32(17, 21, 28, 235)), 16, ModernUI.TextDim, FontStyle.Normal, TextAnchor.MiddleLeft);
            pill.border = new RectOffset(14, 14, 14, 14);
            pill.padding = new RectOffset(18, 18, 6, 6);

            primary = MakeButton(ModernUI.Accent, ModernUI.AccentHover, ModernUI.AccentPressed, ModernUI.AccentText, ModernUI.Accent, ModernUI.AccentText);
            danger = MakeButton(ModernUI.Danger, ModernUI.DangerHover, new Color32(180, 52, 56, 255), Color.white, ModernUI.Danger, Color.white);
        }

        static GUIStyle MakeButton(Color normal, Color hover, Color pressed, Color text, Color onColor, Color onText)
        {
            GUIStyle s = MakeStyle(Solid(8, normal), 18, text, FontStyle.Bold, TextAnchor.MiddleCenter);
            s.border = new RectOffset(8, 8, 8, 8);
            s.padding = new RectOffset(16, 16, 8, 8);
            s.margin = new RectOffset(4, 4, 4, 4);
            s.fixedHeight = 44f;
            s.hover.background = Solid(8, hover); s.hover.textColor = text;
            s.active.background = Solid(8, pressed); s.active.textColor = text;
            s.focused.background = s.normal.background; s.focused.textColor = text;
            s.onNormal.background = Solid(8, onColor); s.onNormal.textColor = onText;
            s.onHover.background = Solid(8, ModernUI.AccentHover); s.onHover.textColor = onText;
            s.onActive.background = Solid(8, ModernUI.AccentPressed); s.onActive.textColor = onText;
            s.onFocused.background = s.onNormal.background; s.onFocused.textColor = onText;
            return s;
        }

        static GUIStyle MakeStyle(Texture2D background, int size, Color text, FontStyle fontStyle, TextAnchor anchor)
        {
            var s = new GUIStyle();
            s.font = ModernUI.Font;
            s.fontSize = size;
            s.fontStyle = fontStyle;
            s.alignment = anchor;
            s.normal.textColor = text;
            if (background != null) s.normal.background = background;
            return s;
        }

        /// <summary>Nhận bán kính góc và màu; trả texture bo góc tô màu đó (dùng cho nền kiểu IMGUI, cắt 9 mảnh bằng border).</summary>
        static Texture2D Solid(int radius, Color color)
        {
            Texture2D tex = ModernUI.MakeRoundedTexture(radius * 2 + 2, radius);
            var px = tex.GetPixels32();
            Color32 c = color;
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(c.r, c.g, c.b, (byte)(px[i].a * c.a / 255f));
            tex.SetPixels32(px);
            tex.Apply(false, true);
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}
