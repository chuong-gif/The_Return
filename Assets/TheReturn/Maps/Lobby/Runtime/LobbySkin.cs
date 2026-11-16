/*
 * Mục đích: Phủ giao diện hiện đại (ModernUI) lên Canvas sảnh chờ (T-053) mà KHÔNG đổi cấu trúc object hay cách nối nút của LobbyCanvasView/LobbyReadyController:
 * thẻ bo góc mờ, tiêu đề hai dòng, bốn ô học sinh đổi kiểu theo trạng thái (trống / đang chờ / sẵn sàng), nút chọn 2–4 người có ô đang chọn nổi bật,
 * số đã sẵn sàng, và biểu ngữ đếm ngược lớn giữa màn hình. Đặt cùng object với LobbyCanvasView (PF_LobbyCanvas); tự nhận kết quả vẽ qua sự kiện Rendered.
 * Hàm: Awake; OnDestroy; BuildStatic; OnRendered; StyleSlots; StyleSizeButtons; FormatSlot; Hex; Child.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Tô giao diện sảnh chờ theo ModernUI lúc chạy.</summary>
    [RequireComponent(typeof(LobbyCanvasView))]
    public sealed class LobbySkin : MonoBehaviour
    {
        [Tooltip("Bật = dựng/tô phần tĩnh lúc chạy (như cũ). Tắt = đã bake vào scene, chỉnh trực tiếp trong Inspector; phần đổi theo trạng thái (ô sẵn sàng, đếm ngược) vẫn chạy bằng code.")]
        public bool applyStaticAtRuntime = true;

        LobbyCanvasView view;
        Text readyChip;
        GameObject banner;
        Text bannerText;
        Button[] sizeButtons;
        int lastCount = -1;
        int lastMask = -1;
        int lastCountdown = -2;

        /// <summary>Không nhận tham số; tô phần tĩnh rồi đăng ký nhận kết quả Render (Awake chạy trước lần Render đầu).</summary>
        void Awake()
        {
            view = GetComponent<LobbyCanvasView>();
            view.titleText = "<size=26><color=#" + Hex(ModernUI.TextDim) + ">THE RETURN</color></size>\n<size=46><b>SẢNH TIẾP NHẬN</b></size>";
            view.slotFormatter = FormatSlot;
            BuildStatic();
            view.Rendered += OnRendered;
        }

        void OnDestroy()
        {
            if (view != null) view.Rendered -= OnRendered;
        }

        /// <summary>Không nhận tham số; tô khung, tiêu đề, dòng hướng dẫn và dựng chip sẵn sàng + biểu ngữ đếm ngược.</summary>
        void BuildStatic()
        {
            if (!applyStaticAtRuntime) { BindBaked(); return; }
            Transform panel = transform.Find("LobbyPanel");
            if (panel != null)
            {
                ModernUI.StylePanel(panel.GetComponent<Image>(), new Color(ModernUI.Surface.r, ModernUI.Surface.g, ModernUI.Surface.b, .88f), 28);
                Image bar = ModernUI.Box(panel, "AccentBar", ModernUI.Accent, 3);
                bar.raycastTarget = false;
                RectTransform br = bar.rectTransform;
                br.anchorMin = new Vector2(.045f, .79f); br.anchorMax = new Vector2(.045f, .93f);
                br.pivot = new Vector2(0f, .5f); br.anchoredPosition = Vector2.zero; br.sizeDelta = new Vector2(6f, 0f);
                readyChip = ModernUI.Label(panel, "", 24, ModernUI.Accent, TextAnchor.MiddleRight, FontStyle.Bold);
                readyChip.name = "ReadyChip";
                RectTransform cr = readyChip.rectTransform;
                cr.anchorMin = new Vector2(.45f, .755f); cr.anchorMax = new Vector2(.93f, .80f);
                cr.offsetMin = cr.offsetMax = Vector2.zero;
            }
            if (view.title != null)
            {
                ModernUI.StyleLabel(view.title, 46, ModernUI.TextPrimary);
                view.title.alignment = TextAnchor.MiddleLeft;
                view.title.supportRichText = true;
                view.title.lineSpacing = .95f;
                RectTransform tr = view.title.rectTransform;
                tr.anchorMin = new Vector2(.09f, .76f); tr.anchorMax = new Vector2(.93f, .96f);
                tr.offsetMin = tr.offsetMax = Vector2.zero;
            }
            if (view.status != null)
            {
                ModernUI.StyleLabel(view.status, 22, ModernUI.TextDim);
                view.status.alignment = TextAnchor.MiddleCenter;
            }
            Image box = ModernUI.Box(transform, "CountdownBanner", new Color(ModernUI.Surface.r, ModernUI.Surface.g, ModernUI.Surface.b, .92f), 24);
            box.raycastTarget = false;
            RectTransform rt = box.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, 1f); rt.pivot = new Vector2(.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -64f); rt.sizeDelta = new Vector2(860f, 130f);
            bannerText = ModernUI.Label(box.transform, "", 54, ModernUI.Accent, TextAnchor.MiddleCenter, FontStyle.Bold);
            ModernUI.Stretch(bannerText.rectTransform);
            bannerText.supportRichText = true;
            bannerText.raycastTarget = false;
            banner = box.gameObject;
            banner.SetActive(false);
            Transform p2 = transform.Find("LobbyPanel");
            sizeButtons = new Button[] { Child<Button>(p2, "2Players"), Child<Button>(p2, "3Players"), Child<Button>(p2, "4Players") };
            foreach (Button b in sizeButtons)
            {
                if (b == null) continue;
                Text l = b.GetComponentInChildren<Text>(true);
                if (l != null) ModernUI.StyleLabel(l, 26, ModernUI.TextPrimary, FontStyle.Bold);
            }
        }

        /// <summary>Nhận số người, mảng ready và giây đếm ngược từ LobbyCanvasView; chỉ tô lại khi trạng thái đổi.</summary>
        void OnRendered(int count, bool[] ready, int countdown)
        {
            int mask = 0;
            for (int i = 0; i < ready.Length && i < 4; i++) if (ready[i]) mask |= 1 << i;
            if (count != lastCount || mask != lastMask)
            {
                lastCount = count; lastMask = mask;
                StyleSlots(count, ready);
                StyleSizeButtons(count);
                int done = 0;
                for (int i = 0; i < count && i < ready.Length; i++) if (ready[i]) done++;
                if (readyChip != null) readyChip.text = "SẴN SÀNG  " + done + "/" + count;
            }
            if (countdown != lastCountdown)
            {
                lastCountdown = countdown;
                if (banner != null) banner.SetActive(countdown >= 0);
                if (countdown >= 0 && bannerText != null) bannerText.text = "CỔNG MỞ SAU  <size=84>" + countdown + "</size>";
            }
        }

        /// <summary>Nhận số người và mảng ready; đổi kiểu nút và màu chữ từng ô: trống = mờ, đang chờ = thường, sẵn sàng = nhấn mạnh.</summary>
        void StyleSlots(int count, bool[] ready)
        {
            for (int i = 0; i < view.slotButtons.Length && i < view.slotLabels.Length; i++)
            {
                bool joined = i < count;
                bool isReady = joined && ready[i];
                ModernButtonStyle style = !joined ? ModernButtonStyle.Ghost : isReady ? ModernButtonStyle.Primary : ModernButtonStyle.Secondary;
                ModernUI.StyleButton(view.slotButtons[i], style);
                Text l = view.slotLabels[i];
                ModernUI.StyleLabel(l, 30, isReady ? ModernUI.AccentText : joined ? ModernUI.TextPrimary : ModernUI.TextDim);
                l.alignment = TextAnchor.MiddleLeft;
                l.supportRichText = true;
                l.rectTransform.offsetMin = new Vector2(34f, 0f);
                l.rectTransform.offsetMax = new Vector2(-16f, 0f);
            }
        }

        /// <summary>Nhận số người hiện tại; ô đang chọn (2/3/4 người) dùng kiểu nhấn mạnh, các ô còn lại kiểu thường.</summary>
        void StyleSizeButtons(int count)
        {
            for (int i = 0; i < sizeButtons.Length; i++)
            {
                if (sizeButtons[i] == null) continue;
                bool selected = count == i + 2;
                ModernUI.StyleButton(sizeButtons[i], selected ? ModernButtonStyle.Primary : ModernButtonStyle.Secondary);
                Text l = sizeButtons[i].GetComponentInChildren<Text>(true);
                if (l != null) l.color = selected ? ModernUI.AccentText : ModernUI.TextPrimary;
            }
        }

        /// <summary>Không nhận tham số; giao diện đã bake nên chỉ tìm lại chip, biểu ngữ và nút số người theo tên.</summary>
        void BindBaked()
        {
            Transform panel = transform.Find("LobbyPanel");
            Transform chip = panel != null ? panel.Find("ReadyChip") : null;
            readyChip = chip != null ? chip.GetComponent<Text>() : null;
            Transform b = transform.Find("CountdownBanner");
            if (b != null) { banner = b.gameObject; bannerText = b.GetComponentInChildren<Text>(true); banner.SetActive(false); }
            sizeButtons = new Button[] { Child<Button>(panel, "2Players"), Child<Button>(panel, "3Players"), Child<Button>(panel, "4Players") };
        }

        /// <summary>Không nhận tham số; chạy ở Edit Mode (chuột phải vào component): dựng phần tĩnh thành object thật trong scene, đổi font hệ điều hành sang font dự phòng lưu được, rồi tắt applyStaticAtRuntime.</summary>
        [ContextMenu("Bake giao diện vào scene (Edit Mode)")]
        public void BakeStatic()
        {
            view = GetComponent<LobbyCanvasView>();
            Transform panel = transform.Find("LobbyPanel");
            if (panel != null && panel.Find("AccentBar") != null) { Debug.LogWarning("LobbySkin: đã bake rồi (có AccentBar), bỏ qua."); return; }
            applyStaticAtRuntime = true;
            BuildStatic();
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (Text x in GetComponentsInChildren<Text>(true)) x.font = fallback;
            applyStaticAtRuntime = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>Nhận chỉ số ô, đã vào đội, đã sẵn sàng; trả chữ hai dòng (tên đậm + trạng thái nhỏ) cho ô đó.</summary>
        string FormatSlot(int i, bool joined, bool ready)
        {
            if (!joined) return "VỊ TRÍ TRỐNG";
            string name = "<b>HỌC SINH " + (i + 1) + "</b>";
            if (ready) return name + "\n<size=22>ĐÃ SẴN SÀNG  ·  F" + (i + 1) + "</size>";
            return name + "\n<size=22><color=#" + Hex(ModernUI.TextDim) + ">ĐANG CHỜ  ·  F" + (i + 1) + "</color></size>";
        }

        /// <summary>Nhận màu; trả mã hex RGB (không có #) dùng trong thẻ màu của chữ.</summary>
        static string Hex(Color c) { return ColorUtility.ToHtmlStringRGB(c); }

        /// <summary>Nhận cha và tên con; trả component loại T của con đó (null nếu không có).</summary>
        static T Child<T>(Transform parent, string name) where T : Component
        {
            if (parent == null) return null;
            Transform t = parent.Find(name);
            return t != null ? t.GetComponent<T>() : null;
        }
    }
}
