/*
 * Mục đích: Phủ giao diện hiện đại (ModernUI) lên MainMenu hiện có (T-053) mà KHÔNG đổi cấu trúc object hay cách nối nút của MainMenuController:
 * nền chuyển màu có quầng sáng, cột menu căn trái với tiêu đề lớn, nút kiểu Primary/Secondary/Ghost, các khung Nhóm chơi và Phòng chờ thành thẻ bo góc,
 * nút CÀI ĐẶT mở menu Cài đặt dùng chung (SettingsMenu) thay cho bảng cài đặt cũ. Chạy ở Start (sau Awake của MainMenuController).
 * Hàm: Start; BuildBackground; StyleMain; StyleEntry; StyleRoom; StyleAllText; Place; StyleMenuButton; StyleInput; MakeBackdropSprite; FadeIn.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Tô giao diện MainMenu theo ModernUI lúc chạy; đặt cùng object với MainMenuController.</summary>
    public sealed class MainMenuSkin : MonoBehaviour
    {
        [Tooltip("Lề trái của cột menu chính (px, theo khung 1920x1080).")]
        public float leftMargin = 180f;

        [Tooltip("Bật = tô giao diện lúc chạy (như cũ). Tắt = giao diện đã bake vào scene, chỉnh trực tiếp trong Inspector.")]
        public bool applyStyleAtRuntime = true;

        MainMenuController controller;

        /// <summary>Không nhận tham số; tìm controller và các khung theo tên rồi tô lại toàn bộ.</summary>
        void Start()
        {
            controller = GetComponent<MainMenuController>();
            if (applyStyleAtRuntime) { BuildBackground(); StyleMain(); StyleEntry(); StyleRoom(); }
            if (controller != null && controller.settingsPanel != null) controller.settingsPanel.SetActive(false);
            if (controller != null && controller.settingsButton != null)
            {
                controller.settingsButton.onClick.RemoveAllListeners();
                controller.settingsButton.onClick.AddListener(SettingsMenu.Open);
            }
            if (applyStyleAtRuntime) StyleAllText();
            Transform main = transform.Find("Panel_Main");
            if (main != null) StartCoroutine(FadeIn(main.gameObject));
        }

        /// <summary>Không nhận tham số; chạy ở Edit Mode (chuột phải vào component): tô giao diện thẳng vào các object trong scene để chỉnh bằng Inspector, đổi font hệ điều hành sang font dự phòng lưu được. Sau đó tắt applyStyleAtRuntime.</summary>
        [ContextMenu("Bake giao diện vào scene (Edit Mode)")]
        public void BakeStyleNow()
        {
            controller = GetComponent<MainMenuController>();
            BuildBackground(); StyleMain(); StyleEntry(); StyleRoom(); StyleAllText();
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (Text x in GetComponentsInChildren<Text>(true)) x.font = fallback;
            applyStyleAtRuntime = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>Không nhận tham số; đặt nền chuyển màu + quầng sáng + tối góc, và gỡ nền cũ.</summary>
        void BuildBackground()
        {
            Transform bg = transform.Find("Background");
            if (bg == null) return;
            Image img = bg.GetComponent<Image>();
            if (img == null) return;
            img.sprite = MakeBackdropSprite();
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.raycastTarget = false;
        }

        /// <summary>Không nhận tham số; tô khung menu chính thành cột trái: tiêu đề, gạch nhấn, bốn nút, chân trang.</summary>
        void StyleMain()
        {
            Transform main = transform.Find("Panel_Main");
            if (main == null) return;
            Image panelImage = main.GetComponent<Image>();
            if (panelImage != null) { panelImage.sprite = null; panelImage.color = new Color(0f, 0f, 0f, 0f); panelImage.raycastTarget = false; }
            RectTransform pr = (RectTransform)main;
            pr.anchorMin = pr.anchorMax = new Vector2(0f, .5f);
            pr.pivot = new Vector2(0f, .5f);
            pr.anchoredPosition = new Vector2(leftMargin, 0f);
            pr.sizeDelta = new Vector2(760f, 900f);

            Text title = Child<Text>(main, "Title");
            if (title != null)
            {
                ModernUI.StyleLabel(title, 124, ModernUI.TextPrimary, FontStyle.Bold);
                title.alignment = TextAnchor.MiddleLeft;
                Place((RectTransform)title.transform, 0f, 300f, 760f, 150f);
            }
            Text sub = Child<Text>(main, "Subtitle");
            if (sub != null)
            {
                ModernUI.StyleLabel(sub, 34, ModernUI.TextDim);
                sub.alignment = TextAnchor.MiddleLeft;
                Place((RectTransform)sub.transform, 4f, 212f, 760f, 52f);
            }
            Image bar = ModernUI.Box(main, "AccentBar", ModernUI.Accent, 3);
            Place(bar.rectTransform, 4f, 168f, 110f, 6f);
            bar.raycastTarget = false;

            StyleMenuButton(Child<Button>(main, "Btn_Online"), "CHƠI ONLINE", ModernButtonStyle.Primary, 60f);
            StyleMenuButton(Child<Button>(main, "Btn_Settings"), "CÀI ĐẶT", ModernButtonStyle.Secondary, -44f);
            StyleMenuButton(Child<Button>(main, "Btn_Quit"), "THOÁT", ModernButtonStyle.Ghost, -148f);

            Transform hint = transform.Find("Hint");
            if (hint != null)
            {
                Text t = hint.GetComponent<Text>();
                RectTransform r = (RectTransform)hint;
                r.anchorMin = r.anchorMax = new Vector2(0f, 0f);
                r.pivot = new Vector2(0f, 0f);
                r.anchoredPosition = new Vector2(leftMargin, 44f);
                r.sizeDelta = new Vector2(900f, 36f);
                ModernUI.StyleLabel(t, 24, ModernUI.TextDim);
                t.alignment = TextAnchor.MiddleLeft;
                t.text = "Esc: quay lại   •   F10: Cài đặt";
            }
            Text version = ModernUI.Label(transform, "v" + Application.version, 22, ModernUI.TextDim, TextAnchor.MiddleRight);
            version.name = "Version";
            RectTransform vr = version.rectTransform;
            vr.anchorMin = vr.anchorMax = new Vector2(1f, 0f);
            vr.pivot = new Vector2(1f, 0f);
            vr.anchoredPosition = new Vector2(-48f, 44f);
            vr.sizeDelta = new Vector2(300f, 36f);
        }

        /// <summary>Không nhận tham số; tô khung Nhóm chơi (nhập tên, tạo/vào phòng) thành thẻ bo góc.</summary>
        void StyleEntry()
        {
            Transform p = transform.Find("Panel_PartyEntry");
            if (p == null) return;
            ModernUI.StylePanel(p.GetComponent<Image>(), ModernUI.Surface, 24);
            ModernUI.StyleLabel(Child<Text>(p, "Title"), 56, ModernUI.TextPrimary, FontStyle.Bold);
            ModernUI.StyleLabel(Child<Text>(p, "NameLabel"), 28, ModernUI.TextDim);
            ModernUI.StyleLabel(Child<Text>(p, "OrLabel"), 26, ModernUI.TextDim);
            ModernUI.StyleLabel(Child<Text>(p, "Message"), 26, ModernUI.TextDim);
            StyleInput(Child<InputField>(p, "Input_Name"));
            StyleInput(Child<InputField>(p, "Input_Code"));
            StyleActionButton(Child<Button>(p, "Btn_Create"), ModernButtonStyle.Primary);
            StyleActionButton(Child<Button>(p, "Btn_Join"), ModernButtonStyle.Secondary);
            StyleActionButton(Child<Button>(p, "Btn_Back"), ModernButtonStyle.Ghost);
        }

        /// <summary>Không nhận tham số; tô khung Phòng chờ: mã phòng, bốn dòng thành viên, các nút.</summary>
        void StyleRoom()
        {
            Transform p = transform.Find("Panel_PartyRoom");
            if (p == null) return;
            ModernUI.StylePanel(p.GetComponent<Image>(), ModernUI.Surface, 24);
            ModernUI.StyleLabel(Child<Text>(p, "CodeLabel"), 64, ModernUI.Accent, FontStyle.Bold);
            ModernUI.StyleLabel(Child<Text>(p, "Message"), 26, ModernUI.TextDim);
            for (int i = 1; i <= 4; i++)
            {
                Transform row = p.Find("MemberRow_" + i);
                if (row == null) continue;
                ModernUI.StylePanel(row.GetComponent<Image>(), ModernUI.SurfaceHigh, 14);
                Text t = row.GetComponentInChildren<Text>(true);
                if (t != null)
                {
                    ModernUI.StyleLabel(t, 30, ModernUI.TextPrimary);
                    t.alignment = TextAnchor.MiddleLeft;
                    RectTransform tr = t.rectTransform;
                    tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                    tr.offsetMin = new Vector2(28f, 0f);
                    tr.offsetMax = new Vector2(-20f, 0f);
                }
            }
            StyleActionButton(Child<Button>(p, "Btn_Ready"), ModernButtonStyle.Primary);
            StyleActionButton(Child<Button>(p, "Btn_Leave"), ModernButtonStyle.Secondary);
            StyleActionButton(Child<Button>(p, "Btn_Start"), ModernButtonStyle.Primary);
            StyleActionButton(Child<Button>(p, "Btn_AddSim"), ModernButtonStyle.Ghost);
            StyleActionButton(Child<Button>(p, "Btn_RemoveSim"), ModernButtonStyle.Ghost);
        }

        /// <summary>Không nhận tham số; áp font dùng chung cho mọi Text còn lại trong canvas.</summary>
        void StyleAllText()
        {
            foreach (Text t in GetComponentsInChildren<Text>(true)) t.font = ModernUI.Font;
        }

        /// <summary>Nhận RectTransform, vị trí và kích thước; đặt neo trái-giữa để căn lề trái trong cột menu.</summary>
        static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, .5f);
            rt.pivot = new Vector2(0f, .5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        /// <summary>Nhận nút, chữ mới, kiểu và độ cao y; tô nút cột menu: rộng 520, căn trái chữ, chữ 30 đậm.</summary>
        static void StyleMenuButton(Button b, string text, ModernButtonStyle style, float y)
        {
            if (b == null) return;
            ModernUI.StyleButton(b, style);
            Place((RectTransform)b.transform, 0f, y, 520f, 88f);
            Text label = b.GetComponentInChildren<Text>(true);
            if (label == null) return;
            label.text = text;
            ModernUI.StyleLabel(label, 30, style == ModernButtonStyle.Primary ? ModernUI.AccentText : ModernUI.TextPrimary, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(36f, 0f);
            label.rectTransform.offsetMax = new Vector2(-16f, 0f);
        }

        /// <summary>Nhận nút và kiểu; tô nút hành động trong thẻ (chữ giữa, 30 đậm), giữ nguyên vị trí cũ.</summary>
        static void StyleActionButton(Button b, ModernButtonStyle style)
        {
            if (b == null) return;
            ModernUI.StyleButton(b, style);
            Text label = b.GetComponentInChildren<Text>(true);
            if (label != null) ModernUI.StyleLabel(label, 30, style == ModernButtonStyle.Primary ? ModernUI.AccentText : ModernUI.TextPrimary, FontStyle.Bold);
        }

        /// <summary>Nhận ô nhập; tô nền bo góc, chữ và chữ gợi ý.</summary>
        static void StyleInput(InputField input)
        {
            if (input == null) return;
            ModernUI.StylePanel(input.GetComponent<Image>(), ModernUI.SurfaceHigh, 12);
            Text text = input.textComponent;
            if (text != null) ModernUI.StyleLabel(text, 32, ModernUI.TextPrimary);
            Text ph = input.placeholder as Text;
            if (ph != null) ModernUI.StyleLabel(ph, 30, ModernUI.TextDim, FontStyle.Italic);
            input.caretColor = ModernUI.Accent;
            input.selectionColor = new Color(ModernUI.Accent.r, ModernUI.Accent.g, ModernUI.Accent.b, .35f);
        }

        /// <summary>Nhận cha và tên con; trả component loại T của con đó (null nếu không có).</summary>
        static T Child<T>(Transform parent, string name) where T : Component
        {
            Transform t = parent.Find(name);
            return t != null ? t.GetComponent<T>() : null;
        }

        /// <summary>Không nhận tham số; tạo sprite nền: chuyển màu tối, quầng sáng xanh lục bên trái, tối dần ra bốn góc.</summary>
        static Sprite MakeBackdropSprite()
        {
            Sprite baked = Resources.Load<Sprite>("ModernUI/MenuBackdrop");
            if (baked != null) return baked;
            const int w = 256, h = 144;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear; tex.hideFlags = HideFlags.DontSave;
            Color baseTop = new Color32(12, 18, 26, 255), baseBottom = new Color32(7, 9, 13, 255), glow = new Color32(20, 74, 70, 255);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = x / (w - 1f), v = y / (h - 1f);
                    Color c = Color.Lerp(baseBottom, baseTop, v);
                    float dx = (u - .22f) * 1.6f, dy = (v - .30f) * 1.9f;
                    float g = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    c = Color.Lerp(c, glow, g * g * .75f);
                    float vx = (u - .5f) * 2f, vy = (v - .5f) * 2f;
                    float vig = Mathf.Clamp01(1f - (vx * vx * .45f + vy * vy * .55f) * .55f);
                    c *= Mathf.Lerp(.55f, 1f, vig);
                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 100f);
            s.hideFlags = HideFlags.DontSave;
            return s;
        }

        /// <summary>Nhận object; hiện dần và trượt nhẹ từ trái trong nửa giây.</summary>
        static IEnumerator FadeIn(GameObject go)
        {
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            RectTransform rt = (RectTransform)go.transform;
            Vector2 end = rt.anchoredPosition;
            for (float t = 0f; t < .5f; t += Time.unscaledDeltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / .5f, 3f);
                group.alpha = k;
                rt.anchoredPosition = end + new Vector2(-40f * (1f - k), 0f);
                yield return null;
            }
            group.alpha = 1f;
            rt.anchoredPosition = end;
        }
    }
}
