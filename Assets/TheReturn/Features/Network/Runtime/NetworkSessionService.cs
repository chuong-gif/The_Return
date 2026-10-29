// Mục đích: dịch vụ phiên chơi online co-op (T-009 S1). Hỗ trợ 2 chế độ:
//   - LAN/localhost: StartHost/StartClient trực tiếp qua UnityTransport (test 1 máy, không cần cloud).
//   - Relay: tạo/vào phòng bằng mã qua Unity Multiplayer Services (Sessions API + Relay), tối đa maxPlayers.
// Hàm: StartLanHost, StartLanClient, StartRelayHost, JoinRelayByCode, LeaveSession.
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

        private ISession _session;

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Idle;
        public string StatusText { get; private set; } = "Sẵn sàng";
        public string JoinCode { get; private set; } = "";
        public string LanAddress { get { return lanAddress; } set { lanAddress = value; } }

        /// <summary>Host LAN/localhost. Nhận: không. Trả: không (cập nhật State/StatusText).</summary>
        public void StartLanHost()
        {
            if (State != NetworkSessionState.Idle) return;
            var transport = GetTransport();
            if (transport == null) return;
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
            transport.SetConnectionData(lanAddress, lanPort);
            if (NetworkManager.Singleton.StartClient()) SetState(NetworkSessionState.Joined, "Đang kết nối tới " + lanAddress);
            else SetState(NetworkSessionState.Error, "Không start được client LAN");
        }

        /// <summary>Tạo phòng Relay, sinh mã phòng. Nhận: không. Trả: không (JoinCode được gán khi xong).</summary>
        public async void StartRelayHost()
        {
            if (State != NetworkSessionState.Idle) return;
            SetState(NetworkSessionState.Working, "Đang tạo phòng...");
            try
            {
                await EnsureSignedInAsync();
                var options = new SessionOptions { MaxPlayers = maxPlayers }.WithRelayNetwork();
                _session = await MultiplayerService.Instance.CreateSessionAsync(options);
                JoinCode = _session.Code;
                SetState(NetworkSessionState.Hosting, "Đang host. Mã phòng: " + JoinCode);
            }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>Vào phòng Relay bằng mã. Nhận: mã phòng. Trả: không.</summary>
        public async void JoinRelayByCode(string code)
        {
            if (State != NetworkSessionState.Idle) return;
            if (string.IsNullOrWhiteSpace(code)) { SetState(NetworkSessionState.Error, "Chưa nhập mã phòng"); return; }
            SetState(NetworkSessionState.Working, "Đang vào phòng...");
            try
            {
                await EnsureSignedInAsync();
                _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
                JoinCode = _session.Code;
                SetState(NetworkSessionState.Joined, "Đã vào phòng " + JoinCode);
            }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>Rời phòng/tắt mạng, về trạng thái Idle. Nhận/Trả: không.</summary>
        public async void LeaveSession()
        {
            try
            {
                if (_session != null)
                {
                    var host = _session as IHostSession;
                    if (host != null && State == NetworkSessionState.Hosting) await host.DeleteAsync();
                    else await _session.LeaveAsync();
                    _session = null;
                }
            }
            catch (Exception e) { Debug.LogWarning("[Network] Rời phòng lỗi: " + e.Message); }
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            JoinCode = "";
            SetState(NetworkSessionState.Idle, "Sẵn sàng");
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
            SetState(NetworkSessionState.Error, "Lỗi: " + e.Message);
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
