/*
 * Mục đích: SẢNH THỬ MỘT MÁY cho NHÀ PHÁT TRIỂN trong Sanh_Cho (T-054): dùng CHÍNH bảng phòng mới (Lobby_OnlineCanvas) nhưng dữ liệu lấy từ PartySession (LocalPartyService),
 * không cần mạng. Có thêm nút «+ Người giả lập» / «− Người giả lập», phím 1–4 đổi nhân vật đang điều khiển, nút SẴN SÀNG / phím F1–F4 bấm sẵn sàng thay cho nhân vật đó,
 * BẮT ĐẦU khi mọi người sẵn sàng -> nạp MapSchoolF với đúng số người. Đặt lên đối tượng có thành phần DevOnly nên bản build thật tự gỡ cả đối tượng này.
 * Chỉ chạy khi KHÔNG có phiên online (OnlineSession.Active = false); có phiên online thì LobbyOnlineController làm việc.
 * Hàm: Awake; Start; Update; Refresh; RefreshRow; OnAddSim; OnRemoveSim; OnReady; OnStart; OnLeave; ToggleReadyAt.
 */
using TheReturn.Network.Room;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheReturn.Network.LobbyOnline
{
    /// <summary>Điều khiển bảng phòng ở chế độ thử một máy (chỉ Editor / Development Build).</summary>
    public sealed class LobbyLocalDevController : MonoBehaviour
    {
        public LobbyOnlineView view;
        [Tooltip("Canvas sảnh cũ (PF_LobbyCanvas); bị ẩn vì bảng phòng mới thay thế.")]
        public GameObject offlineCanvas;
        public LobbyReadyController lobbyReady;
        public PrototypePartyController party;
        [Header("Nút chỉ dành cho nhà phát triển")]
        public Button addSimButton;
        public Button removeSimButton;
        public Text devHintText;

        private LocalPartyService service;
        private float nextRefresh;
        private int lastCount = -1;
        private bool active;

        /// <summary>Không nhận tham số; có phiên online thì tự tắt, không thì báo LobbyReadyController chuyển sang devSimMode TRƯỚC khi nó chạy Start.</summary>
        private void Awake()
        {
            if (OnlineSession.Active) { enabled = false; return; }
            active = true;
            if (lobbyReady != null) lobbyReady.devSimMode = true;
        }

        /// <summary>Không nhận tham số; bật bảng phòng, ẩn sảnh cũ, tạo nhóm một máy (chủ phòng tự sẵn sàng) và nối các nút.</summary>
        private void Start()
        {
            if (!active || view == null) return;
            if (view.gameObject != null) view.gameObject.SetActive(true);
            if (offlineCanvas != null) offlineCanvas.SetActive(false);
            PartySession.Reset();
            service = PartySession.Service as LocalPartyService;
            if (service == null) { enabled = false; return; }
            service.CreateParty("Chủ phòng (dev)");
            service.SetReady(true);
            if (view.readyButton != null)
            {
                RectTransform rr = (RectTransform)view.readyButton.transform; // Sẵn sàng nằm cạnh Bắt đầu để dev thấy cả hai
                rr.anchoredPosition = new Vector2(656f, rr.anchoredPosition.y);
                rr.sizeDelta = new Vector2(172f, rr.sizeDelta.y);
            }
            Bind(view.readyButton, OnReady);
            Bind(view.startButton, OnStart);
            Bind(view.leaveButton, OnLeave);
            Bind(addSimButton, OnAddSim);
            Bind(removeSimButton, OnRemoveSim);
            if (view.hintText != null) view.hintText.text = "Tab / Esc: bật-tắt chuột  •  WASD: đi  •  L: thu gọn bảng";
            if (devHintText != null) devHintText.text = "DEV: 1–4 đổi nhân vật  •  F2–F4 sẵn sàng thay người đó  •  + / − người giả lập";
            if (view.collapsedBar != null) view.collapsedBar.SetActive(false);
            if (view.panel != null) view.panel.SetActive(true);
            nextRefresh = 0f; // lần làm mới đầu để Update lo (sau khi LobbyReadyController.Start đã Initialize)
        }

        private static void Bind(Button b, UnityEngine.Events.UnityAction a)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(a);
        }

        /// <summary>Mỗi khung hình: phím F1–F4 đổi sẵn sàng của ô tương ứng; mỗi 0.2 giây làm mới bảng.</summary>
        private void Update()
        {
            if (service == null) return;
            Keyboard kb = Keyboard.current;
            if (kb != null && GUIUtility.keyboardControl == 0)
            {
                if (kb.f1Key.wasPressedThisFrame) ToggleReadyAt(0);
                if (kb.f2Key.wasPressedThisFrame) ToggleReadyAt(1);
                if (kb.f3Key.wasPressedThisFrame) ToggleReadyAt(2);
                if (kb.f4Key.wasPressedThisFrame) ToggleReadyAt(3);
                if (kb.lKey.wasPressedThisFrame && view.panel != null)
                {
                    bool show = !view.panel.activeSelf;
                    view.panel.SetActive(show);
                    if (view.collapsedBar != null) view.collapsedBar.SetActive(!show);
                }
            }
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            Refresh();
        }

        /// <summary>Không nhận tham số; đọc nhóm một máy rồi đổ vào bảng (tên, vai, sẵn sàng), nút Bắt đầu/Sẵn sàng và số mô hình người trong sảnh.</summary>
        private void Refresh()
        {
            PartySnapshot s = service.Current;
            if (s == null || view == null) return;
            if (s.Count != lastCount)
            {
                lastCount = s.Count;
                party.SetPartySize(s.Count);
                if (party.ActivePlayer >= s.Count) party.SwitchRole(0, false);
                else party.SwitchRole(party.ActivePlayer, false);
            }
            int active = party.ActivePlayer;
            int guests = 0, ready = 0;
            for (int i = 0; i < view.rows.Length; i++)
            {
                PartyMemberInfo m = s.MemberAtSlot(i);
                RefreshRow(view.rows[i], m, i == active);
                if (m != null && !m.IsHost) { guests++; if (m.Ready) ready++; }
            }
            if (view.codeText != null) view.codeText.text = s.Code;
            bool canStart = s.Count >= PartyState.MinPlayers && s.CanStart;
            if (view.startButton != null) { view.startButton.gameObject.SetActive(true); view.startButton.interactable = canStart; }
            if (view.startLabel != null) view.startLabel.text = canStart ? "BẮT ĐẦU" : "CHỜ KHÁCH SẴN SÀNG";
            PartyMemberInfo cur = s.MemberAtSlot(active);
            bool curIsHost = cur == null || cur.IsHost;
            if (view.readyButton != null) { view.readyButton.gameObject.SetActive(!curIsHost); view.readyButton.interactable = true; }
            if (view.readyLabel != null) view.readyLabel.text = cur != null && cur.Ready ? "HỦY" : "SẴN SÀNG";
            if (view.summaryText != null)
                view.summaryText.text = s.Count < PartyState.MinPlayers
                    ? "Bấm «+ Người giả lập» để thêm khách thử. Phím 1–4 đổi nhân vật."
                    : "Đang điều khiển ô " + (active + 1) + (curIsHost ? " (chủ phòng)" : "") + "  •  khách sẵn sàng " + ready + "/" + guests;
            if (addSimButton != null) addSimButton.interactable = s.Count < PartyState.MaxPlayers;
            if (removeSimButton != null) removeSimButton.interactable = s.Count > 1;
        }

        /// <summary>Nhận dòng giao diện, thành viên (có thể null) và cờ 'đang điều khiển'; điền chữ, màu và tắt mic/âm lượng/mời ra (không có thoại ở chế độ một máy).</summary>
        private static void RefreshRow(LobbyOnlinePlayerRow row, PartyMemberInfo m, bool controlled)
        {
            if (row == null) return;
            row.Occupied = false;
            if (m == null)
            {
                if (row.nameText != null) row.nameText.text = "Vị trí trống";
                if (row.slotText != null) row.slotText.text = "•";
                if (row.roleText != null) row.roleText.text = "Bấm «+ Người giả lập»";
                if (row.statusText != null) { row.statusText.text = "TRỐNG"; row.statusText.color = ModernUI.TextDim; }
                if (row.statusChip != null) row.statusChip.color = ModernUI.Surface;
                if (row.background != null) row.background.color = ModernUI.Surface;
            }
            else
            {
                bool good = m.IsHost || m.Ready;
                if (row.nameText != null) row.nameText.text = m.Name + (controlled ? "  ◄ đang điều khiển" : "");
                if (row.slotText != null) row.slotText.text = (m.Slot + 1).ToString();
                if (row.roleText != null) row.roleText.text = m.IsHost ? "Chủ phòng" : "Khách (giả lập)";
                if (row.statusText != null) { row.statusText.text = m.IsHost ? "CHỦ PHÒNG" : (m.Ready ? "SẴN SÀNG" : "ĐANG CHỜ"); row.statusText.color = good ? ModernUI.AccentText : ModernUI.TextDim; }
                if (row.statusChip != null) row.statusChip.color = good ? ModernUI.Accent : ModernUI.SurfaceHover;
                if (row.background != null) row.background.color = controlled ? ModernUI.SurfaceHover : ModernUI.SurfaceHigh;
            }
            if (row.kickButton != null && row.kickButton.gameObject.activeSelf) row.kickButton.gameObject.SetActive(false);
            if (row.micButton != null) row.micButton.interactable = false;
            if (row.micLabel != null) row.micLabel.text = "—";
            if (row.volumeSlider != null) row.volumeSlider.interactable = false;
            if (row.volumeText != null) row.volumeText.text = "—";
        }

        /// <summary>Không nhận tham số; thêm một người giả lập (chưa sẵn sàng để nhà phát triển tự bấm).</summary>
        private void OnAddSim() { service.AddSimulatedPlayer(false); Refresh(); }

        /// <summary>Không nhận tham số; bớt người giả lập có ô lớn nhất.</summary>
        private void OnRemoveSim() { service.RemoveSimulatedPlayer(); Refresh(); }

        /// <summary>Không nhận tham số; nút SẴN SÀNG đổi trạng thái của nhân vật đang điều khiển.</summary>
        private void OnReady() { ToggleReadyAt(party.ActivePlayer); }

        /// <summary>Nhận ô 0-3; đảo Sẵn sàng của khách ở ô đó (chủ phòng luôn sẵn sàng).</summary>
        private void ToggleReadyAt(int slot)
        {
            PartySnapshot s = service.Current;
            if (s == null) return;
            PartyMemberInfo m = s.MemberAtSlot(slot);
            if (m == null || m.IsHost) return;
            service.SetReadyAtSlot(slot, !m.Ready);
            Refresh();
        }

        /// <summary>Không nhận tham số; chủ phòng bắt đầu: đánh dấu nhóm đã bắt đầu (MapSchoolF đọc số người từ PartySession) rồi nạp MapSchoolF.</summary>
        private void OnStart()
        {
            if (service.StartGame() != PartyResult.Ok) return;
            if (view.banner != null) view.banner.SetActive(true);
            if (view.bannerText != null) view.bannerText.text = "ĐANG VÀO MAP...";
            if (SceneTransitionService.Instance == null) new GameObject("SceneTransitionService").AddComponent<SceneTransitionService>();
            SceneTransitionService.Instance.LoadScene(lobbyReady != null ? lobbyReady.schoolSceneName : "MapSchoolF");
        }

        /// <summary>Không nhận tham số; rời sảnh thử về menu chính.</summary>
        private void OnLeave()
        {
            PartySession.Reset();
            SessionExit.ReturnToMenu(SessionExit.DefaultMenuScene);
        }
    }
}
