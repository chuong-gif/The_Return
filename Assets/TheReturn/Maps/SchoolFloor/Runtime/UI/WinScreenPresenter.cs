/*
 * Mục đích: Màn hình thắng game (T-007). Khi MissionRegistry.Won và không còn nhiệm vụ nào đang chạy thì hiện lớp phủ
 * HOÀN THÀNH liệt kê các khu đã xong, kèm nút Chơi lại, Về sảnh (chỉ hiện khi nối onReturnToLobby) và Thoát.
 * Canvas uGUI được dựng bằng code lần đầu hiện, nên không cần sửa scene để dựng giao diện; chỉ cần gắn component này vào
 * scene và gán missionFlow, flow (xem PLANS/T-007-win-screen.md bước S3, cần Scene lock).
 * Hàm: Update; Show; Hide; BuildUi; MakePanel; MakeText; MakeButton; EnsureEventSystem; OnReplay; OnLobby; OnQuit.
 */
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TheReturn
{
    public sealed class WinScreenPresenter : MonoBehaviour
    {
        public MissionFlowController missionFlow;
        public SchoolFloorFlow flow;

        /// <summary>Nối vào Sảnh chờ khi có (T-008). Không có listener thì nút Về sảnh bị ẩn.</summary>
        public UnityEvent onReturnToLobby;

        /// <summary>Hiện dòng ghi chú số khu chưa có nhiệm vụ (hữu ích khi còn phát triển; tắt khi phát hành).</summary>
        public bool showMissingNote = true;

        GameObject root;
        Text bodyText;
        Button lobbyButton;
        bool shown;
        static Font font;

        /// <summary>Không nhận tham số; mỗi frame hiện màn thắng khi đã thắng và hết nhiệm vụ đang chạy, ẩn khi tiến độ bị reset.</summary>
        void Update()
        {
            if (missionFlow == null || missionFlow.Registry == null) return;
            bool won = missionFlow.Registry.Won && !missionFlow.AnyActive;
            if (won && !shown) Show();
            else if (!won && shown) Hide();
        }

        /// <summary>Không nhận tham số; dựng giao diện nếu chưa có, điền danh sách khu đã xong, hiện lớp phủ và THẢ con trỏ chuột để bấm nút.</summary>
        void Show()
        {
            BuildUi();
            WinSummary summary = WinSummary.Build(missionFlow.Registry.Progress);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Cả đội đã hoàn thành mọi nhiệm vụ:");
            foreach (string name in summary.DoneAreaNames) sb.AppendLine("   •  " + name);
            if (showMissingNote && summary.MissingAreaNames.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Còn " + summary.MissingAreaNames.Count + " khu chưa có nhiệm vụ (sẽ được thêm sau): " + string.Join(", ", summary.MissingAreaNames));
            }
            bodyText.text = sb.ToString();
            lobbyButton.gameObject.SetActive(onReturnToLobby != null && onReturnToLobby.GetPersistentEventCount() > 0);
            root.SetActive(true);
            shown = true;
            PrototypePartyController.SetCursor(false);
        }

        /// <summary>Không nhận tham số; ẩn lớp phủ.</summary>
        void Hide()
        {
            if (root != null) root.SetActive(false);
            shown = false;
        }

        /// <summary>Không nhận tham số; dựng Canvas, nền mờ, bảng giữa màn hình, tiêu đề, nội dung và ba nút. Chỉ chạy lần đầu.</summary>
        void BuildUi()
        {
            if (root != null) return;
            EnsureEventSystem();
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root = new GameObject("WinScreen_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image dim = MakePanel(root.transform, "Dim", new Color(0f, 0f, 0f, 0.65f));
            dim.rectTransform.anchorMin = Vector2.zero;
            dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.offsetMin = Vector2.zero;
            dim.rectTransform.offsetMax = Vector2.zero;

            Image panel = MakePanel(root.transform, "Panel", new Color(0.07f, 0.09f, 0.12f, 0.96f));
            panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panel.rectTransform.sizeDelta = new Vector2(960f, 720f);

            Text title = MakeText(panel.transform, "Title", 56, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 0.85f, 0.35f));
            title.text = "HOÀN THÀNH";
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, 110f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -24f);

            bodyText = MakeText(panel.transform, "Body", 28, TextAnchor.UpperLeft, FontStyle.Normal, Color.white);
            bodyText.rectTransform.anchorMin = Vector2.zero;
            bodyText.rectTransform.anchorMax = Vector2.one;
            bodyText.rectTransform.offsetMin = new Vector2(60f, 130f);
            bodyText.rectTransform.offsetMax = new Vector2(-60f, -150f);

            MakeButton(panel.transform, "Chơi lại", -290f, OnReplay);
            lobbyButton = MakeButton(panel.transform, "Về sảnh", 0f, OnLobby);
            MakeButton(panel.transform, "Thoát", 290f, OnQuit);
            root.SetActive(false);
        }

        /// <summary>Nhận cha, tên, màu; tạo đối tượng Image màu đặc (giữ bắt raycast để nền mờ chặn click xuống game). Trả Image.</summary>
        static Image MakePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>Nhận cha, tên, cỡ chữ, căn lề, kiểu chữ, màu; tạo Text uGUI dùng font dựng sẵn, không bắt raycast. Trả Text.</summary>
        static Text MakeText(Transform parent, string name, int size, TextAnchor anchor, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.fontStyle = style;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Nhận cha, nhãn, vị trí ngang (đơn vị canvas) và hàm khi bấm; tạo nút 260x72 ở đáy bảng. Trả Button.</summary>
        static Button MakeButton(Transform parent, string label, float x, UnityAction onClick)
        {
            Image image = MakePanel(parent, "Button_" + label, new Color(0.16f, 0.38f, 0.55f, 1f));
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            image.rectTransform.pivot = new Vector2(0.5f, 0f);
            image.rectTransform.sizeDelta = new Vector2(260f, 72f);
            image.rectTransform.anchoredPosition = new Vector2(x, 48f);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            Text text = MakeText(image.transform, "Label", 30, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
            text.text = label;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        /// <summary>Không nhận tham số; nếu scene chưa có EventSystem thì tạo một cái dùng Input System để nút bấm được.</summary>
        static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        /// <summary>Không nhận tham số; Chơi lại: reset toàn bộ map qua SchoolFloorFlow.RestartMap, ẩn màn thắng và khóa lại chuột để chơi tiếp.</summary>
        void OnReplay()
        {
            if (flow == null) return;
            Hide();
            flow.RestartMap();
            PrototypePartyController.SetCursor(true);
        }

        /// <summary>Không nhận tham số; Về sảnh: gọi các listener đã nối (Sảnh chờ, T-008).</summary>
        void OnLobby()
        {
            if (onReturnToLobby != null) onReturnToLobby.Invoke();
        }

        /// <summary>Không nhận tham số; Thoát: dừng Play Mode trong Editor, đóng game trong bản build.</summary>
        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
