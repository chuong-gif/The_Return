// Mục đích: HUD thử nhỏ (IMGUI, góc trái trên) cho T-009 S1: Host/Join LAN, Host/Join Relay bằng mã, Rời phòng. F1 ẩn/hiện.
// Hàm: Update, OnGUI.
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.Network
{
    public sealed class NetworkTestHud : MonoBehaviour
    {
        [SerializeField] private NetworkSessionService session;

        private string _code = "";
        private bool _visible = true;

        /// <summary>Mỗi frame: F1 bật/tắt HUD. Nhận/Trả: không.</summary>
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame) _visible = !_visible;
        }

        /// <summary>Vẽ HUD theo trạng thái phiên. Nhận/Trả: không.</summary>
        private void OnGUI()
        {
            if (!_visible || session == null) return;
            GUILayout.BeginArea(new Rect(10, 10, 260, 300), GUI.skin.box);
            GUILayout.Label("T-009 S1 - Online test (F1 ẩn)");
            GUILayout.Label(session.StatusText);
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                GUILayout.Label("Người chơi: " + NetworkManager.Singleton.ConnectedClientsIds.Count + " | Tôi: " + NetworkManager.Singleton.LocalClientId);

            switch (session.State)
            {
                case NetworkSessionState.Idle:
                    if (GUILayout.Button("Host LAN (localhost)")) session.StartLanHost();
                    if (GUILayout.Button("Join LAN (127.0.0.1)")) session.StartLanClient();
                    GUILayout.Space(6);
                    if (GUILayout.Button("Tạo phòng Relay")) session.StartRelayHost();
                    GUILayout.BeginHorizontal();
                    _code = GUILayout.TextField(_code, 8, GUILayout.Width(110));
                    if (GUILayout.Button("Vào bằng mã")) session.JoinRelayByCode(_code);
                    GUILayout.EndHorizontal();
                    break;
                case NetworkSessionState.Working:
                    GUILayout.Label("Vui lòng chờ...");
                    break;
                case NetworkSessionState.Error:
                    if (GUILayout.Button("Thử lại")) session.ResetError();
                    break;
                default:
                    if (!string.IsNullOrEmpty(session.JoinCode)) GUILayout.Label("Mã phòng: " + session.JoinCode);
                    if (GUILayout.Button("Rời phòng")) session.LeaveSession();
                    break;
            }
            GUILayout.EndArea();
        }
    }
}
