/*
 * Mục đích: Giao diện CHAT THOẠI tạm (T-029, gọn lại ở T-047), IMGUI (kiểu ModernGui, T-053) ở góc TRÁI màn hình (thu gọn mặc định): trạng thái, mic bật/tắt (phím V), nút «Thử lại» khi lỗi,
 * danh sách người trong kênh có chấm báo ai đang nói, nút tắt tiếng riêng từng người. Chỉ hiện khi đang trong phòng. Là singleton cùng VoiceRoot (bản trùng tự bỏ).
 * Hàm: Awake; OnDestroy; Update; OnGUI.
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn.Network.Voice
{
    public sealed class VoiceHud : MonoBehaviour
    {
        private static VoiceHud _instance;

        /// <summary>Đặt true khi một giao diện khác (sảnh online, T-054) đã có điều khiển thoại riêng; HUD IMGUI này nằm im.</summary>
        public static bool Suppressed;

        [SerializeField] private VoiceChatService voice;
        private NetworkSessionService _session;
        private float _nextFind;
        private bool _inRoom;
        private bool _expanded; // mặc định thu gọn thành một nút nhỏ để không che tầm nhìn
        private readonly List<VoiceParticipantInfo> _list = new List<VoiceParticipantInfo>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);   // chỉ bỏ component trùng
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Mỗi frame: chốt dữ liệu cho OnGUI (để Layout và Repaint cùng một danh sách). Nhận/Trả: không.</summary>
        private void Update()
        {
            if (voice == null) voice = VoiceChatService.Instance;
            if (_session == null && Time.unscaledTime >= _nextFind)
            {
                _nextFind = Time.unscaledTime + 0.5f;
                _session = FindFirstObjectByType<NetworkSessionService>();
            }
            _inRoom = voice != null && _session != null && (_session.State == NetworkSessionState.Hosting || _session.State == NetworkSessionState.Joined);
            if (_inRoom && voice.State == VoiceState.Connected) voice.GetParticipants(_list);
            else _list.Clear();
        }

        private void OnGUI()
        {
            if (Suppressed || !_inRoom) return;
            bool connected = voice.State == VoiceState.Connected;
            bool error = voice.State == VoiceState.Error;
            float vw, vh;
            ModernGui.Begin(out vw, out vh);
            // Góc TRÁI, dưới thẻ nhiệm vụ: không chồng bản đồ nhỏ/nút cài đặt (phải) và không che tầm nhìn giữa màn hình.
            const float x = 24f, y = 160f, w = 300f;
            string speaker = null;
            for (int i = 0; i < _list.Count; i++) if (_list[i].Speaking && !_list[i].IsSelf) { speaker = _list[i].DisplayName; break; }
            string head = connected
                ? (voice.MicMuted ? "Mic TẮT" : "Mic BẬT") + "  •  " + _list.Count + " người" + (speaker != null ? "  •  " + speaker : "")
                : voice.StatusText;
            if (!_expanded && !error)
            {
                GUILayout.BeginArea(new Rect(x, y, w, 46f), GUI.skin.box);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(head + "  ▾", GUILayout.Height(34f))) _expanded = true;
                if (connected && GUILayout.Button(voice.MicMuted ? "Bật" : "Tắt", voice.MicMuted ? GUI.skin.button : ModernGui.Primary, GUILayout.Width(64f), GUILayout.Height(34f))) voice.ToggleMic();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                ModernGui.End();
                return;
            }
            float hgt = 116f + (voice.StatusText.Length > 30 ? 24f : 0f) + (error ? 54f : 0f) + (connected ? 54f + 36f * _list.Count : 0f);
            GUILayout.BeginArea(new Rect(x, y, w, hgt), GUI.skin.box);
            if (GUILayout.Button("Chat thoại  ▴  (thu gọn)", ModernGui.Title)) _expanded = false;
            GUILayout.Label(voice.StatusText + "  •  phím " + VoiceChatService.MicKeyName + " bật/tắt mic", ModernGui.Dim);
            if (error && GUILayout.Button("Thử lại", ModernGui.Primary)) voice.Retry();
            if (connected)
            {
                if (GUILayout.Button(voice.MicMuted ? "Mic đang TẮT — bấm để bật" : "Mic đang BẬT — bấm để tắt", voice.MicMuted ? GUI.skin.button : ModernGui.Primary)) voice.ToggleMic();
                foreach (VoiceParticipantInfo p in _list)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((p.Speaking ? "●  " : "○  ") + p.DisplayName + (p.IsSelf ? " (bạn)" : "") + (p.Muted ? "  [tắt tiếng]" : ""));
                    if (!p.IsSelf && GUILayout.Button(p.Muted ? "Bật tiếng" : "Tắt tiếng", GUILayout.Width(96f), GUILayout.Height(32f))) voice.ToggleParticipantMute(p.PlayerId);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
            ModernGui.End();
        }
    }
}
