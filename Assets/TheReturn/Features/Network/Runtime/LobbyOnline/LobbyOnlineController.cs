/*
 * Mục đích: Điều khiển SẢNH CHỜ ONLINE trong Sanh_Cho (T-054). Khi có phiên online (OnlineSession.Active): ẩn Canvas sảnh offline, báo LobbyReadyController chuyển sang onlineMode,
 * rồi mỗi 0.2 giây đọc RoomRoster.Rows đổ vào bảng người chơi: tên, chủ phòng/khách, SẴN SÀNG/ĐANG CHỜ, mic và âm lượng từng người (qua VoiceChatService), nút MỜI RA (chỉ chủ phòng).
 * Khách bấm SẴN SÀNG; chủ phòng bấm BẮT ĐẦU khi RoomRoster.CanStartNow (>= 2 người, mọi khách sẵn sàng) -> RoomService.StartGame -> RoomMapHandoff nạp MapSchoolF.
 * Không có phiên online thì tự tắt và để sảnh offline (LobbyReadyController + LobbySkin) chạy như cũ. Phím L ẩn/hiện bảng.
 * Hàm: Awake; Start; OnDestroy; Update; Refresh; RefreshRow; ShowEmptyRow; SyncAvatars; FindVoice; OnReady; OnStart; OnLeave; OnKick; OnMic; OnVolume; OnCopyCode; SetCollapsed.
 */
using System.Collections.Generic;
using TheReturn.Network.Room;
using TheReturn.Network.Voice;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheReturn.Network.LobbyOnline
{
    /// <summary>Đổ dữ liệu phòng online vào Canvas sảnh và nhận thao tác của người chơi.</summary>
    public sealed class LobbyOnlineController : MonoBehaviour
    {
        public LobbyOnlineView view;
        [Tooltip("Canvas sảnh offline (PF_LobbyCanvas) sẽ bị ẩn khi chơi online.")]
        public GameObject offlineCanvas;
        public LobbyReadyController lobbyReady;
        public PrototypePartyController party;

        private const float RefreshInterval = 0.2f;
        private float nextRefresh;
        private bool online;
        private bool collapsed;
        private int boundSlot = -1;
        private readonly List<VoiceParticipantInfo> voice = new List<VoiceParticipantInfo>();
        private bool wasVoiceConnected;

        /// <summary>Không nhận tham số; không có phiên online thì tắt bảng và dừng; có thì ẩn sảnh offline và bật onlineMode trước khi LobbyReadyController chạy Start.</summary>
        private void Awake()
        {
            online = OnlineSession.Active;
            if (!online)
            {
                if (view != null && view.gameObject != null) view.gameObject.SetActive(false);
                enabled = false;
                return;
            }
            if (offlineCanvas != null) offlineCanvas.SetActive(false);
            if (lobbyReady != null) lobbyReady.onlineMode = true;
            VoiceHud.Suppressed = true;
        }

        /// <summary>Không nhận tham số; nối các nút của bảng với hành động tương ứng.</summary>
        private void Start()
        {
            if (!online || view == null) return;
            Bind(view.readyButton, OnReady);
            Bind(view.startButton, OnStart);
            Bind(view.leaveButton, OnLeave);
            Bind(view.copyCodeButton, OnCopyCode);
            Bind(view.collapseButton, delegate { SetCollapsed(true); });
            Bind(view.expandButton, delegate { SetCollapsed(false); });
            for (int i = 0; i < view.rows.Length; i++)
            {
                int index = i;
                LobbyOnlinePlayerRow row = view.rows[i];
                if (row == null) continue;
                Bind(row.micButton, delegate { OnMic(index); });
                Bind(row.kickButton, delegate { OnKick(index); });
                if (row.volumeSlider != null) row.volumeSlider.onValueChanged.AddListener(delegate (float v) { OnVolume(index, v); });
            }
            if (view.hintText != null) view.hintText.text = "Tab: chuột • L: thu gọn";
            SetCollapsed(false);
            Refresh();
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void OnDestroy()
        {
            if (online) VoiceHud.Suppressed = false;
        }

        /// <summary>Mỗi khung hình: phím L ẩn/hiện bảng; cứ 0.2 giây làm mới dữ liệu từ phòng.</summary>
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.lKey.wasPressedThisFrame && GUIUtility.keyboardControl == 0) SetCollapsed(!collapsed);
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        /// <summary>Không nhận tham số; đọc RoomRoster + thoại rồi cập nhật toàn bộ bảng, nút và mô hình đứng trong sảnh.</summary>
        private void Refresh()
        {
            if (view == null) return;
            RoomRoster roster = RoomRoster.Current;
            NetworkManager nm = NetworkManager.Singleton;
            RoomService service = RoomService.Instance;
            if (view.codeText != null)
            {
                string code = service != null && service.Session != null ? service.Session.JoinCode : "";
                view.codeText.text = string.IsNullOrEmpty(code) ? "—" : code;
            }
            if (roster == null || nm == null || !nm.IsListening)
            {
                for (int i = 0; i < view.rows.Length; i++) ShowEmptyRow(view.rows[i]);
                if (view.summaryText != null) view.summaryText.text = "Đang kết nối phòng...";
                SetButton(view.readyButton, false, false);
                SetButton(view.startButton, false, false);
                return;
            }

            bool isHost = nm.IsServer;
            ulong me = nm.LocalClientId;
            VoiceChatService vc = VoiceChatService.Instance;
            bool voiceOn = vc != null && vc.State == VoiceState.Connected;
            voice.Clear();
            if (voiceOn) vc.GetParticipants(voice);
            if (voiceOn != wasVoiceConnected) { wasVoiceConnected = voiceOn; if (voiceOn) vc.SetMicVolume(vc.MicVolume); }

            int count = roster.Rows.Count;
            int guests = 0, ready = 0;
            bool myReady = false;
            int mySlot = -1;
            bool[] slotShown = new bool[4];
            for (int i = 0; i < view.rows.Length; i++)
            {
                if (i < count)
                {
                    RoomNetRow r = roster.Rows[i];
                    RefreshRow(view.rows[i], r, r.ClientId == me, isHost, vc, voiceOn);
                    if (!r.IsHost) { guests++; if (r.Ready) ready++; }
                    if (r.ClientId == me) { myReady = r.IsHost || r.Ready; mySlot = r.Slot; }
                    if (r.Slot >= 0 && r.Slot < slotShown.Length) slotShown[r.Slot] = true;
                }
                else ShowEmptyRow(view.rows[i]);
            }

            bool started = roster.Started.Value;
            if (view.banner != null) view.banner.SetActive(started);
            if (view.bannerText != null && started) view.bannerText.text = "ĐANG VÀO MAP...";
            bool canStart = isHost && !started && roster.CanStartNow;
            SetButton(view.startButton, isHost, canStart);
            SetButton(view.readyButton, !isHost, !started);
            if (view.readyLabel != null) view.readyLabel.text = myReady ? "HỦY SẴN SÀNG" : "SẴN SÀNG";
            if (view.startLabel != null) view.startLabel.text = canStart ? "BẮT ĐẦU" : "CHỜ KHÁCH SẴN SÀNG";
            if (view.summaryText != null)
            {
                if (started) view.summaryText.text = "Chủ phòng đã bắt đầu.";
                else if (isHost) view.summaryText.text = count < RoomState.MinPlayers ? "Cần ít nhất 2 người để bắt đầu. Gửi mã phòng cho bạn bè." : "Khách sẵn sàng: " + ready + "/" + guests + (canStart ? "  —  có thể bắt đầu" : "");
                else SetGuestSummary(myReady);
            }
            SyncAvatars(slotShown, mySlot);
        }

        /// <summary>Nhận cờ 'mình đã sẵn sàng'; đặt câu hướng dẫn cho khách.</summary>
        private void SetGuestSummary(bool myReady)
        {
            if (view.summaryText != null) view.summaryText.text = myReady ? "Đã sẵn sàng. Chờ chủ phòng bắt đầu." : "Bấm SẴN SÀNG khi bạn đã chuẩn bị xong.";
        }

        private static void SetButton(Button button, bool visible, bool interactable)
        {
            if (button == null) return;
            if (button.gameObject.activeSelf != visible) button.gameObject.SetActive(visible);
            if (button.interactable != interactable) button.interactable = interactable;
        }

        /// <summary>Nhận dòng giao diện, dòng phòng, cờ 'là mình', cờ 'mình là chủ phòng', dịch vụ thoại; điền tên, trạng thái, mic, âm lượng, nút mời ra.</summary>
        private void RefreshRow(LobbyOnlinePlayerRow row, RoomNetRow r, bool mine, bool isHost, VoiceChatService vc, bool voiceOn)
        {
            if (row == null) return;
            row.ClientId = r.ClientId;
            row.Occupied = true;
            string name = r.Name.ToString();
            if (row.slotText != null) row.slotText.text = (r.Slot + 1).ToString();
            if (row.nameText != null) row.nameText.text = name + (mine ? "  (bạn)" : "");
            if (row.roleText != null) row.roleText.text = r.IsHost ? "Chủ phòng" : "Khách";
            bool good = r.IsHost || r.Ready;
            if (row.statusText != null) { row.statusText.text = r.IsHost ? "CHỦ PHÒNG" : (r.Ready ? "SẴN SÀNG" : "ĐANG CHỜ"); row.statusText.color = good ? ModernUI.AccentText : ModernUI.TextDim; }
            if (row.statusChip != null) row.statusChip.color = good ? ModernUI.Accent : ModernUI.SurfaceHover;
            if (row.background != null) row.background.color = mine ? ModernUI.SurfaceHover : ModernUI.SurfaceHigh;
            SetActive(row.kickButton, isHost && !r.IsHost);

            VoiceParticipantInfo info = default(VoiceParticipantInfo);
            bool found = voiceOn && FindVoice(name, mine, out info);
            if (row.micButton != null)
            {
                row.micButton.interactable = found;
                if (row.micLabel != null)
                {
                    if (!found) row.micLabel.text = "CHƯA CÓ THOẠI";
                    else if (mine) row.micLabel.text = vc.MicMuted ? "MIC TẮT" : "MIC BẬT";
                    else row.micLabel.text = info.Muted ? "ĐÃ TẮT TIẾNG" : "ĐANG NGHE";
                }
            }
            if (row.volumeSlider != null)
            {
                row.volumeSlider.interactable = found;
                if (found) row.volumeSlider.SetValueWithoutNotify(info.Volume);
            }
            if (row.volumeText != null) row.volumeText.text = found ? Mathf.RoundToInt((mine ? vc.MicVolume : info.Volume) * 100f) + "%" : "—";
        }

        /// <summary>Nhận dòng giao diện; đặt dòng về trạng thái trống («VỊ TRÍ TRỐNG»).</summary>
        private static void ShowEmptyRow(LobbyOnlinePlayerRow row)
        {
            if (row == null) return;
            row.ClientId = 0; row.Occupied = false;
            if (row.nameText != null) row.nameText.text = "Vị trí trống";
            if (row.slotText != null) row.slotText.text = "•";
            if (row.roleText != null) row.roleText.text = "Đang chờ người vào";
            if (row.statusText != null) { row.statusText.text = "TRỐNG"; row.statusText.color = ModernUI.TextDim; }
            if (row.statusChip != null) row.statusChip.color = ModernUI.Surface;
            if (row.background != null) row.background.color = ModernUI.Surface;
            SetActive(row.kickButton, false);
            if (row.micButton != null) row.micButton.interactable = false;
            if (row.micLabel != null) row.micLabel.text = "—";
            if (row.volumeSlider != null) row.volumeSlider.interactable = false;
            if (row.volumeText != null) row.volumeText.text = "—";
        }

        private static void SetActive(Button b, bool on)
        {
            if (b != null && b.gameObject.activeSelf != on) b.gameObject.SetActive(on);
        }

        /// <summary>Nhận ô nào có người và ô của mình; chỉ hiện mô hình người ở ô đã có người, và đặt camera vào nhân vật của mình (một lần mỗi khi ô đổi).</summary>
        private void SyncAvatars(bool[] slotShown, int mySlot)
        {
            if (party == null || party.players == null) return;
            for (int i = 0; i < party.players.Length && i < slotShown.Length; i++)
            {
                Transform t = party.players[i];
                if (t != null && t.gameObject.activeSelf != slotShown[i]) t.gameObject.SetActive(slotShown[i]);
            }
            if (mySlot >= 0 && mySlot != boundSlot && mySlot < party.players.Length)
            {
                // Chỉ ghi nhớ khi gắn camera thành công: ở khung hình đầu LobbyReadyController chưa Start (PartySize = 0) nên SwitchRole trả false và phải thử lại.
                if (party.SwitchRole(mySlot, false)) boundSlot = mySlot;
            }
        }

        /// <summary>Nhận tên hiển thị và cờ 'là mình'; tìm người tương ứng trong kênh thoại (mình = IsSelf, người khác = trùng tên). Trả true nếu thấy.</summary>
        private bool FindVoice(string name, bool mine, out VoiceParticipantInfo info)
        {
            for (int i = 0; i < voice.Count; i++)
            {
                VoiceParticipantInfo p = voice[i];
                if (mine ? p.IsSelf : (!p.IsSelf && p.DisplayName == name)) { info = p; return true; }
            }
            info = default(VoiceParticipantInfo);
            return false;
        }

        private void OnReady()
        {
            RoomRoster roster = RoomRoster.Current;
            NetworkManager nm = NetworkManager.Singleton;
            if (roster == null || nm == null || nm.IsServer) return;
            bool mineReady = false;
            for (int i = 0; i < roster.Rows.Count; i++) if (roster.Rows[i].ClientId == nm.LocalClientId) mineReady = roster.Rows[i].Ready;
            roster.SetReady(!mineReady);
        }

        private void OnStart()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return; // chỉ chủ phòng mới được bắt đầu
            RoomService service = RoomService.Instance;
            if (service != null) service.StartGame();
        }

        private void OnLeave()
        {
            SessionExit.ReturnToMenu(SessionExit.DefaultMenuScene);
        }

        private void OnKick(int index)
        {
            RoomRoster roster = RoomRoster.Current;
            if (roster == null || index < 0 || index >= view.rows.Length || !view.rows[index].Occupied) return;
            roster.Kick(view.rows[index].ClientId);
        }

        private void OnMic(int index)
        {
            VoiceChatService vc = VoiceChatService.Instance;
            LobbyOnlinePlayerRow row = view.rows[index];
            NetworkManager nm = NetworkManager.Singleton;
            if (vc == null || nm == null || !row.Occupied) return;
            if (row.ClientId == nm.LocalClientId) { vc.ToggleMic(); return; }
            VoiceParticipantInfo info;
            if (FindVoice(RowName(row), false, out info)) vc.ToggleParticipantMute(info.PlayerId);
        }

        private void OnVolume(int index, float value)
        {
            VoiceChatService vc = VoiceChatService.Instance;
            LobbyOnlinePlayerRow row = view.rows[index];
            NetworkManager nm = NetworkManager.Singleton;
            if (vc == null || nm == null || !row.Occupied) return;
            if (row.ClientId == nm.LocalClientId) { vc.SetMicVolume(value); return; }
            VoiceParticipantInfo info;
            if (FindVoice(RowName(row), false, out info)) vc.SetParticipantVolume(info.PlayerId, value);
        }

        private static string RowName(LobbyOnlinePlayerRow row)
        {
            string t = row.nameText != null ? row.nameText.text : "";
            const string suffix = "  (bạn)";
            return t.EndsWith(suffix) ? t.Substring(0, t.Length - suffix.Length) : t;
        }

        private void OnCopyCode()
        {
            RoomService service = RoomService.Instance;
            if (service != null && service.Session != null && !string.IsNullOrEmpty(service.Session.JoinCode)) GUIUtility.systemCopyBuffer = service.Session.JoinCode;
        }

        /// <summary>Nhận true = thu gọn bảng thành thanh nhỏ, false = mở lại bảng đầy đủ.</summary>
        private void SetCollapsed(bool value)
        {
            collapsed = value;
            if (view == null) return;
            if (view.panel != null) view.panel.SetActive(!value);
            if (view.collapsedBar != null) view.collapsedBar.SetActive(value);
        }
    }
}
