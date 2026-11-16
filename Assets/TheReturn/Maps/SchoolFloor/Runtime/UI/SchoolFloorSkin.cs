/*
 * Mục đích: Phủ giao diện hiện đại (ModernUI) lên Canvas MapSchoolF (T-053) mà KHÔNG đổi cấu trúc object hay cách nối nút của SchoolFloorCanvasView/Presenter:
 * thẻ bo góc mờ cho HUD, thông báo, mạnh mối, menu tạm dừng, menu test và trình đọc tài liệu; nút Primary/Secondary/Ghost; chấm ngắm tròn.
 * Tắt CanvasThemeBinding lúc chạy để theme cũ không ghi đè màu mỗi lần panel bật. Panel nhiệm vụ cũ giữ nguyên (Presenter còn tham chiếu), vốn tắt và không controller nào bật trong MapSchoolF.
 * Hàm: Start; Card; Txt; Btn; AccentBar; StyleHud; StyleClues; StylePause; StyleTests; StyleReader; Find.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Tô giao diện Canvas MapSchoolF theo ModernUI lúc chạy; đặt cùng object với SchoolFloorCanvasView.</summary>
    [RequireComponent(typeof(SchoolFloorCanvasView))]
    public sealed class SchoolFloorSkin : MonoBehaviour
    {
        Transform safe;

        /// <summary>Không nhận tham số; tắt theme cũ rồi tô từng nhóm giao diện (chạy sau Awake của Presenter).</summary>
        void Start()
        {
            safe = transform.Find("SafeFrame_1280x720");
            if (safe == null) return;
            foreach (CanvasThemeBinding binding in GetComponentsInChildren<CanvasThemeBinding>(true)) binding.enabled = false;
            StyleHud();
            StyleClues();
            StylePause();
            StyleTests();
            StyleReader();
        }

        /// <summary>Nhận đường dẫn con của SafeFrame; trả Transform hoặc null.</summary>
        Transform Find(string path) { return safe.Find(path); }

        /// <summary>Nhận đường dẫn, độ mờ, bo góc và cờ nền đậm (dark); tô Image thành thẻ nền ModernUI.</summary>
        void Card(string path, float alpha, int radius, bool dark = false)
        {
            Transform t = Find(path);
            Image img = t != null ? t.GetComponent<Image>() : null;
            if (img == null) return;
            Color c = dark ? ModernUI.Bg : ModernUI.Surface; c.a = alpha;
            ModernUI.StylePanel(img, c, radius);
        }

        /// <summary>Nhận đường dẫn, cỡ chữ, màu, kiểu; tô Text theo ModernUI.</summary>
        void Txt(string path, int size, Color color, FontStyle style)
        {
            Transform t = Find(path);
            Text x = t != null ? t.GetComponent<Text>() : null;
            if (x != null) ModernUI.StyleLabel(x, size, color, style);
        }

        /// <summary>Nhận đường dẫn nút, kiểu nút, cỡ chữ; tô nút và chữ đậm (giữ màu chữ do kiểu nút đặt).</summary>
        void Btn(string path, ModernButtonStyle style, int labelSize)
        {
            Transform t = Find(path);
            Button b = t != null ? t.GetComponent<Button>() : null;
            if (b == null) return;
            ModernUI.StyleButton(b, style);
            if (style == ModernButtonStyle.Secondary)
            {
                ColorBlock cb = b.colors; // nâng sáng để nổi trên nền menu đậm
                cb.normalColor = ModernUI.SurfaceHover; cb.selectedColor = ModernUI.SurfaceHover;
                cb.highlightedColor = new Color32(58, 71, 92, 255);
                b.colors = cb;
            }
            Text l = b.GetComponentInChildren<Text>(true);
            if (l != null) { l.fontSize = labelSize; l.fontStyle = FontStyle.Bold; }
        }

        /// <summary>Nhận thẻ cha; thêm vạch nhấn mảnh bên trái (một lần).</summary>
        void AccentBar(Transform card)
        {
            if (card == null || card.Find("AccentBar") != null) return;
            Image bar = ModernUI.Box(card, "AccentBar", ModernUI.Accent, 2);
            bar.raycastTarget = false;
            RectTransform r = bar.rectTransform;
            r.anchorMin = new Vector2(0f, 0f); r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, .5f);
            r.anchoredPosition = new Vector2(8f, 0f); r.sizeDelta = new Vector2(4f, -26f);
        }

        /// <summary>Không nhận tham số; tô tiêu đề nhiệm vụ, thông báo, gợi ý tương tác và chấm ngắm.</summary>
        void StyleHud()
        {
            Card("HUD/MissionHeader", .82f, 18);
            AccentBar(Find("HUD/MissionHeader"));
            Txt("HUD/MissionHeader/MissionTitle", 26, ModernUI.TextPrimary, FontStyle.Bold);
            Txt("HUD/MissionHeader/Objective", 16, ModernUI.TextDim, FontStyle.Normal);
            Txt("HUD/MissionHeader/PartyRole", 24, ModernUI.Accent, FontStyle.Bold);
            Card("HUD/FeedbackFooter", .82f, 18);
            Txt("HUD/FeedbackFooter/Feedback", 18, ModernUI.TextPrimary, FontStyle.Normal);
            Txt("HUD/FeedbackFooter/Controls", 14, ModernUI.TextDim, FontStyle.Normal);
            Card("HUD/InteractionPrompt", .88f, 16);
            Txt("HUD/InteractionPrompt/Prompt", 18, ModernUI.TextPrimary, FontStyle.Bold);
            Transform cross = Find("HUD/Crosshair");
            Image ci = cross != null ? cross.GetComponent<Image>() : null;
            if (ci != null)
            {
                ci.sprite = ModernUI.Circle();
                ci.color = new Color(1f, 1f, 1f, .85f);
                ci.raycastTarget = false;
                RectTransform r = ci.rectTransform;
                if (r.anchorMin == r.anchorMax) r.sizeDelta = new Vector2(8f, 8f);
            }
        }

        /// <summary>Không nhận tham số; tô khung mạnh mối riêng của vai.</summary>
        void StyleClues()
        {
            Card("PrivateClues", .74f, 18);
            AccentBar(Find("PrivateClues"));
            Txt("PrivateClues/Heading", 20, ModernUI.Accent, FontStyle.Bold);
            Txt("PrivateClues/ClueScroll/Viewport/Content", 18, ModernUI.TextPrimary, FontStyle.Normal);
        }

        /// <summary>Không nhận tham số; tô menu tạm dừng / thiết lập đội.</summary>
        void StylePause()
        {
            Card("PauseMenu", .97f, 26, true);
            AccentBar(Find("PauseMenu"));
            Txt("PauseMenu/Title", 28, ModernUI.TextPrimary, FontStyle.Bold);
            Btn("PauseMenu/Resume", ModernButtonStyle.Primary, 20);
            Btn("PauseMenu/Retry", ModernButtonStyle.Secondary, 18);
            Btn("PauseMenu/NewCase", ModernButtonStyle.Secondary, 18);
            Btn("PauseMenu/RestartMap", ModernButtonStyle.Secondary, 18);
            Btn("PauseMenu/OpenTests", ModernButtonStyle.Ghost, 18);
            Btn("PauseMenu/Team_2", ModernButtonStyle.Secondary, 18);
            Btn("PauseMenu/Team_3", ModernButtonStyle.Secondary, 18);
            Btn("PauseMenu/Team_4", ModernButtonStyle.Secondary, 18);
            Txt("PauseMenu/Note", 14, ModernUI.TextDim, FontStyle.Normal);
        }

        /// <summary>Không nhận tham số; tô menu vào thẳng nhiệm vụ (kể cả nút nhân bản lúc Awake).</summary>
        void StyleTests()
        {
            Card("MissionTestMenu", .97f, 26, true);
            AccentBar(Find("MissionTestMenu"));
            Txt("MissionTestMenu/Title", 28, ModernUI.TextPrimary, FontStyle.Bold);
            Txt("MissionTestMenu/TeamStatus", 16, ModernUI.TextDim, FontStyle.Normal);
            Btn("MissionTestMenu/TestTeam_2", ModernButtonStyle.Secondary, 18);
            Btn("MissionTestMenu/TestTeam_3", ModernButtonStyle.Secondary, 18);
            Btn("MissionTestMenu/TestTeam_4", ModernButtonStyle.Secondary, 18);
            Btn("MissionTestMenu/Back", ModernButtonStyle.Ghost, 18);
            Transform menu = Find("MissionTestMenu");
            if (menu == null) return;
            for (int i = 0; i < menu.childCount; i++)
            {
                Transform c = menu.GetChild(i);
                if (c.name.StartsWith("Mission_")) Btn("MissionTestMenu/" + c.name, ModernButtonStyle.Secondary, 17);
            }
        }

        /// <summary>Không nhận tham số; tô trình đọc tài liệu.</summary>
        void StyleReader()
        {
            Card("DocumentReader", .98f, 26, true);
            AccentBar(Find("DocumentReader"));
            Txt("DocumentReader/Title", 28, ModernUI.TextPrimary, FontStyle.Bold);
            Txt("DocumentReader/DocumentScroll/Viewport/Content", 20, ModernUI.TextPrimary, FontStyle.Normal);
            Btn("DocumentReader/Close", ModernButtonStyle.Secondary, 18);
        }
    }
}
