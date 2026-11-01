/*
 * Mục đích: Chat GIỌNG NÓI theo phòng online bằng Unity Vivox (T-029). Tự theo dõi NetworkSessionService: khi vào phòng Relay (có mã phòng)
 * thì vào kênh thoại 'room-<mã>', khi rời/đóng phòng thì rời kênh. Cho bật/tắt mic của mình và tắt tiếng riêng từng người khác.
 * Phòng LAN không có mã phòng nên KHÔNG có thoại (ghi chú rõ cho người chơi). Mọi trạng thái được ghi log '[Voice]' để kiểm thử.
 * Cần: Unity Services đã khởi tạo + đăng nhập (NetworkSessionService làm khi tạo/vào phòng Relay) và Vivox bật trong Unity Cloud dashboard.
 * Hàm: Awake; Update; ConnectAsync; DisconnectAsync; ToggleMic; SetMicMuted; ToggleParticipantMute; GetParticipants; thuộc tính State, StatusText, MicMuted, ChannelName.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace TheReturn.Network.Voice
{
    public enum VoiceState { Off, Connecting, Connected, Error }

    /// <summary>Thông tin một người trong kênh thoại để hiển thị.</summary>
    public struct VoiceParticipantInfo
    {
        public string PlayerId;
        public string DisplayName;
        public bool IsSelf;
        public bool Speaking;
        public bool Muted;
    }

    public sealed class VoiceChatService : MonoBehaviour
    {
        [SerializeField] private NetworkSessionService session;
        [SerializeField] private string channelPrefix = "room-";

        private const float PollInterval = 0.5f;
        private float _nextPoll;
        private int _generation;       // tăng mỗi lần đổi ý (kết nối/ngắt) để bỏ kết quả async cũ
        private bool _initialized;
        private string _desiredChannel = "";
        private string _lastSignature = "";

        public VoiceState State { get; private set; } = VoiceState.Off;
        public string StatusText { get; private set; } = "Chat thoại: chưa vào phòng Relay";
        public string ChannelName { get; private set; } = "";

        /// <summary>True nếu mic của mình đang tắt (đọc từ Vivox).</summary>
        public bool MicMuted { get { return State == VoiceState.Connected && VivoxService.Instance.IsInputDeviceMuted; } }

        /// <summary>Tên thiết bị micro đang dùng, rỗng nếu không có.</summary>
        public string MicDeviceName
        {
            get
            {
                if (State != VoiceState.Connected) return "";
                var d = VivoxService.Instance.ActiveInputDevice;
                return d != null ? d.DeviceName : "";
            }
        }

        private void Awake()
        {
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();
        }

        private void Update()
        {
            if (session == null || Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + PollInterval;

            bool inRoom = (session.State == NetworkSessionState.Hosting || session.State == NetworkSessionState.Joined) && !string.IsNullOrEmpty(session.JoinCode);
            string wanted = inRoom ? channelPrefix + session.JoinCode : "";
            if (wanted != _desiredChannel)
            {
                _desiredChannel = wanted;
                _generation++;
                if (wanted.Length > 0) { var ignoredConnect = ConnectAsync(wanted, _generation); }
                else { var ignoredLeave = DisconnectAsync(_generation); }
            }
            else if (!inRoom && session.State == NetworkSessionState.Hosting)
            {
                StatusText = "Chat thoại: chỉ có ở phòng Relay (phòng LAN không có mã phòng)";
            }
            LogIfChanged();
        }

        private void OnDestroy()
        {
            if (_initialized && VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn)
            {
                var ignored = VivoxService.Instance.LogoutAsync();
            }
        }

        /// <summary>Tắt/bật mic của mình. Chỉ có tác dụng khi đã vào kênh.</summary>
        public void ToggleMic()
        {
            SetMicMuted(!MicMuted);
        }

        /// <summary>Đặt mic của mình. Nhận true = tắt mic.</summary>
        public void SetMicMuted(bool muted)
        {
            if (State != VoiceState.Connected) return;
            if (muted) VivoxService.Instance.MuteInputDevice();
            else VivoxService.Instance.UnmuteInputDevice();
        }

        /// <summary>Tắt/bật tiếng riêng một người khác (chỉ trên máy mình). Nhận PlayerId của người đó.</summary>
        public void ToggleParticipantMute(string playerId)
        {
            VivoxParticipant p = FindParticipant(playerId);
            if (p == null || p.IsSelf) return;
            if (p.IsMuted) p.UnmutePlayerLocally();
            else p.MutePlayerLocally();
        }

        /// <summary>Điền danh sách người đang ở trong kênh thoại. Nhận list đích (được xóa trước). Trả: không.</summary>
        public void GetParticipants(List<VoiceParticipantInfo> into)
        {
            into.Clear();
            if (State != VoiceState.Connected || ChannelName.Length == 0) return;
            System.Collections.ObjectModel.ReadOnlyCollection<VivoxParticipant> list;
            if (!VivoxService.Instance.ActiveChannels.TryGetValue(ChannelName, out list)) return;
            foreach (VivoxParticipant p in list)
            {
                into.Add(new VoiceParticipantInfo { PlayerId = p.PlayerId, DisplayName = p.DisplayName, IsSelf = p.IsSelf, Speaking = p.SpeechDetected, Muted = p.IsMuted });
            }
        }

        private VivoxParticipant FindParticipant(string playerId)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<VivoxParticipant> list;
            if (State != VoiceState.Connected || !VivoxService.Instance.ActiveChannels.TryGetValue(ChannelName, out list)) return null;
            foreach (VivoxParticipant p in list) if (p.PlayerId == playerId) return p;
            return null;
        }

        private async Task ConnectAsync(string channel, int generation)
        {
            State = VoiceState.Connecting;
            StatusText = "Chat thoại: đang kết nối...";
            try
            {
                if (!_initialized)
                {
                    await VivoxService.Instance.InitializeAsync();
                    _initialized = true;
                }
                if (generation != _generation) return;
                if (!VivoxService.Instance.IsLoggedIn)
                {
                    string display = string.IsNullOrWhiteSpace(Room.RoomService.PendingName) ? "Nguoi choi" : Room.RoomService.PendingName.Trim();
                    var options = new LoginOptions { DisplayName = display, ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond };
                    await VivoxService.Instance.LoginAsync(options);
                }
                if (generation != _generation) return;
                await VivoxService.Instance.JoinGroupChannelAsync(channel, ChatCapability.AudioOnly);
                if (generation != _generation)
                {
                    await VivoxService.Instance.LeaveChannelAsync(channel);
                    return;
                }
                ChannelName = channel;
                State = VoiceState.Connected;
                int devices = VivoxService.Instance.AvailableInputDevices != null ? VivoxService.Instance.AvailableInputDevices.Count : 0;
                StatusText = devices == 0 ? "Chat thoại: đã vào kênh nhưng KHÔNG thấy micro (kiểm tra quyền micro của Windows)" : "Chat thoại: đã vào kênh";
                Debug.Log("[Voice] CONNECTED channel=" + channel + " loggedIn=" + VivoxService.Instance.IsLoggedIn + " micDevices=" + devices + " activeMic=" + MicDeviceName);
            }
            catch (Exception e)
            {
                if (generation != _generation) return;
                ChannelName = "";
                State = VoiceState.Error;
                StatusText = "Chat thoại lỗi: " + e.Message;
                Debug.LogWarning("[Voice] ERROR " + e.GetType().Name + ": " + e.Message);
            }
        }

        private async Task DisconnectAsync(int generation)
        {
            string old = ChannelName;
            ChannelName = "";
            if (State == VoiceState.Off) return;
            State = VoiceState.Off;
            StatusText = "Chat thoại: chưa vào phòng Relay";
            try
            {
                if (_initialized && VivoxService.Instance.IsLoggedIn) await VivoxService.Instance.LeaveAllChannelsAsync();
                Debug.Log("[Voice] LEFT channel=" + old);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Voice] Rời kênh lỗi: " + e.Message);
            }
        }

        private void LogIfChanged()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(State).Append("|").Append(ChannelName).Append("|mic=").Append(MicMuted);
            List<VoiceParticipantInfo> list = new List<VoiceParticipantInfo>();
            GetParticipants(list);
            foreach (VoiceParticipantInfo p in list) sb.Append("|").Append(p.DisplayName).Append(p.IsSelf ? "(tôi)" : "").Append(p.Muted ? "(tắt)" : "");
            string sig = sb.ToString();
            if (sig == _lastSignature) return;
            _lastSignature = sig;
            Debug.Log("[Voice] " + sig + " | n=" + list.Count);
        }
    }
}
