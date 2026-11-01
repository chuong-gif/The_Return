// Mục đích: dịch vụ phiên chơi online co-op (T-009 S1, T-023). Hỗ trợ 2 chế độ:
//   - LAN/localhost: StartHost/StartClient trực tiếp qua UnityTransport (test 1 máy, không cần cloud).
//   - Relay: tạo/vào phòng bằng mã qua Unity Multiplayer Services (Sessions API + Relay), tối đa maxPlayers.
//     T-023: chọn được giao thức Relay (DTLS/UDP hoặc WSS qua cổng 443 để vượt mạng chặn UDP) và vùng máy chủ;
//     tự báo lỗi + dọn phòng khi Relay mất kết nối (Transport failure).
//     WSS bắt buộc bật UnityTransport.UseWebSockets (tự chỉnh trong ApplyRelayTransportMode; LAN luôn tắt).
// Hàm: StartLanHost, StartLanClient, StartRelayHost, JoinRelayByCode, LeaveSession, ResetError; thuộc tính RelayProtocolChoice, RelayRegion.
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace TheReturn.Network
{
    public enum NetworkSessionState { Idle, Working, Hosting, Joined, Error }

    public sealed class NetworkSessionService : MonoBehaviour
    {
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private string lanAddress = "127.0.0.1";
        [SerializeField] private ushort lanPort = 7777;
        [SerializeField] private RelayProtocol relayProtocol = RelayProtocol.WSS; // T-023: WSS (cổng 443) mặc định vì vượt tường lửa/mạng chặn UDP tốt hơn DTLS; HUD đổi được
        [SerializeField] private string relayRegion = ""; // rỗng = tự chọn theo QoS

        private ISession _session;

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Idle;
        public string StatusText { get; private set; } = "Sẵn sàng";
        public string JoinCode { get; private set; } = "";
        public string LanAddress { get { return lanAddress; } set { lanAddress = value; } }

        /// <summary>Số người tối đa của phòng Relay (2-4), đặt TRƯỚC khi tạo phòng (T-026: số người chốt lúc lập đội).</summary>
        public int MaxPlayers { get { return maxPlayers; } set { maxPlayers = Mathf.Clamp(value, 2, 4); } }

        /// <summary>Giao thức Relay cho lần tạo/vào phòng kế tiếp (DTLS mặc định; WSS qua cổng 443). Đổi được lúc chạy.</summary>
        public RelayProtocol RelayProtocolChoice { get { return relayProtocol; } set { relayProtocol = value; } }

        /// <summary>Vùng Relay ép dùng khi tạo phòng (vd asia-southeast1); rỗng = tự chọn.</summary>
        public string RelayRegion { get { return relayRegion; } set { relayRegion = value ?? ""; } }

        /// <summary>Host LAN/localhost. Nhận: không. Trả: không (cập nhật State/StatusText).</summary>
        public void StartLanHost()
        {
            if (State != NetworkSessionState.Idle) return;
            var transport = GetTransport();
            if (transport == null) return;
            transport.UseWebSockets = false;
            transport.SetConnectionData("127.0.0.1", lanPort, "0.0.0.0");
            if (NetworkManager.Singleton.StartHost()) SetState(NetworkSessionState.Hosting, "Đang host LAN, cổng " + lanPort);
            else SetState(NetworkSessionState.Error, "Không start được host LAN");
        }

        /// <summary>Vào host LAN theo lanAddress. Nhận: không. Trả: không.</summary>
        public void StartLanClient()
        {
            if (State != NetworkSessionState.Idle) return;
            var transport = GetTransport();
            if (transport == null) return;
            transport.UseWebSockets = false;
            transport.SetConnectionData(lanAddress, lanPort);
            if (NetworkManager.Singleton.StartClient()) SetState(NetworkSessionState.Joined, "Đang kết nối tới " + lanAddress);
            else SetState(NetworkSessionState.Error, "Không start được client LAN");
        }

        /// <summary>Tạo phòng Relay, sinh mã phòng. Nhận: không. Trả: không (JoinCode được gán khi xong).</summary>
        public async void StartRelayHost()
        {
            if (State != NetworkSessionState.Idle) return;
            SetState(NetworkSessionState.Working, "Đang tạo phòng (" + relayProtocol + ")...");
            try
            {
                if (!ApplyRelayTransportMode()) return;
                await EnsureSignedInAsync();
                string region = string.IsNullOrWhiteSpace(relayRegion) ? null : relayRegion.Trim();
                var options = new SessionOptions { MaxPlayers = maxPlayers }
                    .WithRelayNetwork(region)
                    .WithNetworkOptions(new NetworkOptions { RelayProtocol = relayProtocol });
                _session = await MultiplayerService.Instance.CreateSessionAsync(options);
                JoinCode = _session.Code;
                WatchTransport();
                SetState(NetworkSessionState.Hosting, "Đang host (" + relayProtocol + "). Mã phòng: " + JoinCode);
            }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>Vào phòng Relay bằng mã. Nhận: mã phòng. Trả: không.</summary>
        public async void JoinRelayByCode(string code)
        {
            if (State != NetworkSessionState.Idle) return;
            if (string.IsNullOrWhiteSpace(code)) { SetState(NetworkSessionState.Error, "Chưa nhập mã phòng"); return; }
            SetState(NetworkSessionState.Working, "Đang vào phòng (" + relayProtocol + ")...");
            try
            {
                if (!ApplyRelayTransportMode()) return;
                await EnsureSignedInAsync();
                var joinOptions = new JoinSessionOptions().WithNetworkOptions(new NetworkOptions { RelayProtocol = relayProtocol });
                _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant(), joinOptions);
                JoinCode = _session.Code;
                WatchTransport();
                SetState(NetworkSessionState.Joined, "Đã vào phòng " + JoinCode + " (" + relayProtocol + ")");
            }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>Rời phòng/tắt mạng, về trạng thái Idle. Nhận/Trả: không.</summary>
        public async void LeaveSession()
        {
            UnwatchTransport();
            await LeaveCloudSessionAsync(State == NetworkSessionState.Hosting);
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            JoinCode = "";
            SetState(NetworkSessionState.Idle, "Sẵn sàng");
        }

        private void OnDestroy()
        {
            UnwatchTransport();
        }

        /// <summary>Rời/xóa phiên trên cloud (host thì xóa). Nhận: có phải host. Trả: Task hoàn tất.</summary>
        private async Task LeaveCloudSessionAsync(bool asHost)
        {
            var s = _session;
            _session = null;
            if (s == null) return;
            try
            {
                var host = s as IHostSession;
                if (host != null && asHost) await host.DeleteAsync();
                else await s.LeaveAsync();
            }
            catch (Exception e) { Debug.LogWarning("[Network] Rời phòng lỗi: " + e.Message); }
        }

        /// <summary>Đăng ký bắt lỗi transport (Relay mất kết nối). Nhận/Trả: không.</summary>
        private void WatchTransport()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;
            nm.OnTransportFailure -= OnTransportFailure;
            nm.OnTransportFailure += OnTransportFailure;
        }

        private void UnwatchTransport()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null) nm.OnTransportFailure -= OnTransportFailure;
        }

        /// <summary>Relay/transport hỏng: báo lỗi rõ cho người dùng và dọn phòng cloud. Nhận/Trả: không.</summary>
        private void OnTransportFailure()
        {
            Debug.LogWarning("[Network] Transport failure, giao thức " + relayProtocol);
            bool wasHost = State == NetworkSessionState.Hosting;
            JoinCode = "";
            SetState(NetworkSessionState.Error, "Mất kết nối Relay (" + relayProtocol + "). Thử giao thức WSS hoặc đổi mạng");
            UnwatchTransport();
            var ignored = LeaveCloudSessionAsync(wasHost);
        }

        /// <summary>WSS cần bật 'Use WebSockets' trên UnityTransport, DTLS/UDP phải tắt (nếu lệch, NGO báo Mismatched Relay configuration). Nhận: không. Trả: true nếu chỉnh được.</summary>
        private bool ApplyRelayTransportMode()
        {
            var transport = GetTransport();
            if (transport == null) return false;
            transport.UseWebSockets = relayProtocol == RelayProtocol.WSS;
            return true;
        }

        private UnityTransport GetTransport()
        {
            if (NetworkManager.Singleton == null) { SetState(NetworkSessionState.Error, "Không có NetworkManager trong scene"); return null; }
            var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (t == null) SetState(NetworkSessionState.Error, "NetworkManager thiếu UnityTransport");
            return t;
        }

        private static async Task EnsureSignedInAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                // Profile ngẫu nhiên để nhiều Editor/bản build trên 1 máy có PlayerId khác nhau khi test.
                var init = new InitializationOptions().SetProfile("p" + UnityEngine.Random.Range(0, 1000000));
                await UnityServices.InitializeAsync(init);
            }
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Fail(Exception e)
        {
            Debug.LogException(e);
            _session = null;
            string message = e.Message ?? "";
            // Phòng Relay đầy: SDK trả 'session is full' (tiếng Anh); đổi sang câu dễ hiểu (T-026).
            if (message.IndexOf("session is full", StringComparison.OrdinalIgnoreCase) >= 0) message = "Phòng đã đủ người";
            SetState(NetworkSessionState.Error, "Lỗi (" + relayProtocol + "): " + message);
        }

        private void SetState(NetworkSessionState state, string text)
        {
            State = state;
            StatusText = text;
            Debug.Log("[Network] " + state + " - " + text);
        }

        /// <summary>Cho HUD đưa Error về Idle để thử lại. Nhận/Trả: không.</summary>
        public void ResetError()
        {
            if (State == NetworkSessionState.Error) SetState(NetworkSessionState.Idle, "Sẵn sàng");
        }
    }
}
