/*
 * Mục đích: Chat GIỌNG NÓI theo phòng online bằng Unity Vivox (T-029). Tự theo dõi NetworkSessionService: khi vào phòng Relay (có mã phòng)
 * thì vào kênh thoại 'room-<mã>', khi rời/đóng phòng thì rời kênh. Cho bật/tắt mic của mình và tắt tiếng riêng từng người khác.
 * Phòng LAN không có mã phòng nên KHÔNG có thoại (ghi chú rõ cho người chơi). Mọi trạng thái được ghi log '[Voice]' để kiểm thử.
 * T-047: là SINGLETON tự tạo (đối tượng VoiceRoot, DontDestroyOnLoad) TRƯỚC khi nạp scene đầu -> thoại sống qua đổi scene phòng -> map, không cần đặt vào scene/prefab;
 * bản đặt sẵn trong scene thử tự bỏ. Phím V bật/tắt mic. Lỗi Vivox chỉ làm thoại báo lỗi, không ảnh hưởng game; Retry() để thử lại.
 * Cần: Unity Services đã khởi tạo + đăng nhập (NetworkSessionService làm khi tạo/vào phòng Relay) và Vivox bật trong Unity Cloud dashboard.
 * T-054 S4: âm lượng từng người (chỉ trên máy mình, 0 = tắt tiếng, 1 = mặc định, tối đa 1.5) và âm lượng mic của chính mình.
 * Hàm: ResetStatics; Bootstrap; Awake; Update; HandleMicKey; ConnectAsync; DisconnectAsync; ToggleMic; SetMicMuted; ToggleParticipantMute; SetParticipantVolume; GetParticipantVolume; SetMicVolume; VolumeToVivox; Retry; GetParticipants; thuộc tính Instance, State, StatusText, MicMuted, ChannelName.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;

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
        /// <summary>Âm lượng nghe người này trên máy mình (1 = mặc định); với chính mình là âm lượng mic.</summary>
        public float Volume;
    }

    public sealed class VoiceChatService : MonoBehaviour
    {
        [SerializeField] private NetworkSessionService session;
        [SerializeField] private string channelPrefix = "room-";

        /// <summary>Phím bật/tắt mic của mình (không trùng phím đang dùng trong game).</summary>
        public static string MicKeyName { get { return GameKeys.DisplayName(GameKeys.Get(GameAction.ToggleMic)); } }

        /// <summary>Dịch vụ thoại duy nhất (sống qua đổi scene).</summary>
        public static VoiceChatService Instance { get; private set; }

        private const float PollInterval = 0.5f;
        private float _nextPoll;
        private int _generation;       // tăng mỗi lần đổi ý (kết nối/ngắt) để bỏ kết quả async cũ
        private bool _initialized;
        private string _desiredChannel = "";
        private string _lastSignature = "";
        private readonly Dictionary<string, float> _volumes = new Dictionary<string, float>();
        private float _micVolume = 1f;

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

        /// <summary>Xóa tham chiếu tĩnh khi vào Play (phòng khi tắt reload domain). Nhận/Trả: không.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        /// <summary>Tạo VoiceRoot (thoại + HUD) bền qua đổi scene, chạy trước khi nạp scene đầu. Nhận/Trả: không.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            GameObject root = new GameObject("VoiceRoot");
            DontDestroyOnLoad(root);
            root.AddComponent<VoiceChatService>();
            root.AddComponent<VoiceHud>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);   // chỉ bỏ component trùng, không đụng đối tượng chứa nó (có thể là NetworkManager)
                return;
            }
            Instance = this;
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();
            MicDeviceSettings.Changed += OnMicPreferenceChanged;
        }


        private void Update()
        {
            HandleMicKey();
            if (Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + PollInterval;
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();

            bool inRoom = session != null && (session.State == NetworkSessionState.Hosting || session.State == NetworkSessionState.Joined) && !string.IsNullOrEmpty(session.JoinCode);
            // T-054: có phòng (RoomRoster) thì đợi tên đã chốt để tên trong kênh thoại trùng tên trong danh sách phòng
            if (inRoom && Room.RoomRoster.Current != null && string.IsNullOrEmpty(Room.RoomIdentity.LocalName)) inRoom = false;
            string wanted = inRoom ? channelPrefix + session.JoinCode : "";
            if (wanted != _desiredChannel)
            {
                _desiredChannel = wanted;
                _generation++;
                if (wanted.Length > 0) { var ignoredConnect = ConnectAsync(wanted, _generation); }
                else { var ignoredLeave = DisconnectAsync(_generation); }
            }
            else if (!inRoom && session != null && session.State == NetworkSessionState.Hosting)
            {
                StatusText = "Chat thoại: chỉ có ở phòng Relay (phòng LAN không có mã phòng)";
            }
            LogIfChanged();
        }

        /// <summary>Mỗi frame: phím V bật/tắt mic khi đã vào kênh và không đang gõ vào ô chữ IMGUI. Nhận/Trả: không.</summary>
        private void HandleMicKey()
        {
            if (State != VoiceState.Connected || GUIUtility.keyboardControl != 0) return;
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.Control(GameAction.ToggleMic).wasPressedThisFrame) ToggleMic();
        }

        /// <summary>Thử kết nối thoại lại sau lỗi (HUD nút «Thử lại»). Chỉ có tác dụng khi State = Error và đang ở phòng Relay.</summary>
        public void Retry()
        {
            if (State != VoiceState.Error || _desiredChannel.Length == 0) return;
            _generation++;
            var ignored = ConnectAsync(_desiredChannel, _generation);
        }

        private void OnDestroy()
        {
            MicDeviceSettings.Changed -= OnMicPreferenceChanged;
            if (Instance == this) Instance = null;
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

        /// <summary>Đổi âm lượng nghe một người khác (chỉ máy mình). Nhận PlayerId và giá trị 0..1.5 (1 = mặc định); gần 0 thì tắt tiếng hẳn, tăng lại thì mở tiếng.</summary>
        public void SetParticipantVolume(string playerId, float value)
        {
            value = Mathf.Clamp(value, 0f, 1.5f);
            _volumes[playerId] = value;
            VivoxParticipant p = FindParticipant(playerId);
            if (p == null || p.IsSelf) return;
            if (value < 0.05f)
            {
                if (!p.IsMuted) p.MutePlayerLocally();
                return;
            }
            if (p.IsMuted) p.UnmutePlayerLocally();
            p.SetLocalVolume(VolumeToVivox(value));
        }

        /// <summary>Nhận PlayerId; trả âm lượng đã đặt cho người đó (mặc định 1).</summary>
        public float GetParticipantVolume(string playerId)
        {
            float v;
            return _volumes.TryGetValue(playerId, out v) ? v : 1f;
        }

        /// <summary>Đổi âm lượng thu mic của chính mình. Nhận 0..1.5 (1 = mặc định). Chỉ có tác dụng khi đã vào kênh.</summary>
        public void SetMicVolume(float value)
        {
            _micVolume = Mathf.Clamp(value, 0f, 1.5f);
            if (State == VoiceState.Connected) VivoxService.Instance.SetInputDeviceVolume(VolumeToVivox(_micVolume));
        }

        /// <summary>Trả âm lượng mic của mình đã đặt (mặc định 1).</summary>
        public float MicVolume { get { return _micVolume; } }

        /// <summary>Nhận hệ số 0..1.5; trả số nguyên Vivox -50..50 (1 -> 0 mặc định, 0.5 -> -50, 1.5 -> +50).</summary>
        public static int VolumeToVivox(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp((value - 1f) * 100f, -50f, 50f));
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
                into.Add(new VoiceParticipantInfo { PlayerId = p.PlayerId, DisplayName = p.DisplayName, IsSelf = p.IsSelf, Speaking = p.SpeechDetected, Muted = p.IsMuted, Volume = p.IsSelf ? _micVolume : GetParticipantVolume(p.PlayerId) });
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
                    string display = !string.IsNullOrWhiteSpace(Room.RoomIdentity.LocalName) ? Room.RoomIdentity.LocalName.Trim() : string.IsNullOrWhiteSpace(Room.RoomService.PendingName) ? "Nguoi choi" : Room.RoomService.PendingName.Trim();
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
                if (!_deviceEventHooked) { VivoxService.Instance.AvailableInputDevicesChanged += OnVivoxDevicesChanged; _deviceEventHooked = true; }
                await ApplyPreferredMicAsync();
                if (Mathf.Abs(_micVolume - 1f) > 0.001f) VivoxService.Instance.SetInputDeviceVolume(VolumeToVivox(_micVolume));
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
            MicDeviceSettings.SetExternalNames(null);
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

        private bool _deviceEventHooked;

        /// <summary>Không nhận tham số; người chơi vừa đổi micro trong Cài đặt: nếu đang trong kênh thoại thì đổi micro Vivox ngay.</summary>
        private void OnMicPreferenceChanged()
        {
            if (State == VoiceState.Connected) { var ignored = ApplyPreferredMicAsync(); }
        }

        /// <summary>Không nhận tham số; Vivox báo danh sách micro đổi (cắm/rút tai nghe): cập nhật danh sách và áp lại micro đã chọn.</summary>
        private void OnVivoxDevicesChanged()
        {
            if (State == VoiceState.Connected) { var ignored = ApplyPreferredMicAsync(); }
        }

        /// <summary>Không nhận tham số; đặt micro Vivox theo MicDeviceSettings.Preferred (rỗng hoặc không thấy = mặc định hệ thống), đồng thời công bố danh sách micro của Vivox cho menu Cài đặt.</summary>
        private async Task ApplyPreferredMicAsync()
        {
            try
            {
                var source = VivoxService.Instance.AvailableInputDevices;
                if (source == null || source.Count == 0) return;
                List<VivoxInputDevice> devices = new List<VivoxInputDevice>(source);
                List<string> names = new List<string>();
                foreach (VivoxInputDevice d in devices) names.Add(d.DeviceName);
                MicDeviceSettings.SetExternalNames(names);
                string want = MicDeviceSettings.Preferred;
                int idx = string.IsNullOrEmpty(want) ? -1 : MicDeviceSettings.FindMatch(names, want, 0);
                if (idx < 0)
                {
                    if (!string.IsNullOrEmpty(want)) Debug.LogWarning("[Voice] Không thấy micro đã chọn '" + want + "', dùng micro mặc định.");
                    for (int i = 0; i < names.Count; i++) if (names[i].StartsWith("Default", StringComparison.OrdinalIgnoreCase)) { idx = i; break; }
                }
                if (idx < 0) return;
                VivoxInputDevice active = VivoxService.Instance.ActiveInputDevice;
                if (active != null && active.DeviceID == devices[idx].DeviceID) return;
                await VivoxService.Instance.SetActiveInputDeviceAsync(devices[idx]);
                Debug.Log("[Voice] MIC -> " + devices[idx].DeviceName);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Voice] Đổi micro lỗi: " + e.Message);
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
