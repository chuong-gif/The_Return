/*
 * Mục đích: Giao diện CHAT THOẠI tạm (T-029), IMGUI ở góc phải màn hình: trạng thái, nút Mic bật/tắt, danh sách người trong kênh có chấm báo
 * ai đang nói, nút tắt tiếng riêng từng người. Chỉ hiện khi đang trong phòng.
 * Hàm: OnGUI.
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn.Network.Voice
{
    public sealed class VoiceHud : MonoBehaviour
    {
        [SerializeField] private VoiceChatService voice;
        private readonly List<VoiceParticipantInfo> _list = new List<VoiceParticipantInfo>();

        private void OnGUI()
        {
            if (voice == null) voice = FindFirstObjectByType<VoiceChatService>();
            if (voice == null) return;
            NetworkSessionService s = FindFirstObjectByType<NetworkSessionService>();
            if (s == null || (s.State != NetworkSessionState.Hosting && s.State != NetworkSessionState.Joined)) return;
            float w = 320f;
            GUILayout.BeginArea(new Rect(Screen.width - w - 10f, 10f, w, 360f), GUI.skin.box);
            GUILayout.Label("CHAT THOẠI (T-029)");
            GUILayout.Label(voice.StatusText);
            if (voice.State == VoiceState.Connected)
            {
                string mic = voice.MicDeviceName;
                GUILayout.Label(string.IsNullOrEmpty(mic) ? "Micro: (không có)" : "Micro: " + mic);
                if (GUILayout.Button(voice.MicMuted ? "Mic đang TẮT (bấm để bật)" : "Mic đang BẬT (bấm để tắt)")) voice.ToggleMic();
                voice.GetParticipants(_list);
                GUILayout.Label("Trong kênh: " + _list.Count + " người");
                foreach (VoiceParticipantInfo p in _list)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((p.Speaking ? "(o) " : "    ") + p.DisplayName + (p.IsSelf ? " (bạn)" : "") + (p.Muted ? " [tắt tiếng]" : ""));
                    if (!p.IsSelf && GUILayout.Button(p.Muted ? "Bật tiếng" : "Tắt tiếng", GUILayout.Width(80))) voice.ToggleParticipantMute(p.PlayerId);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
        }
    }
}
