/*
 * Mục đích: Giữ cho phiên online KHÔNG treo, KHÔNG kẹt khi có người rời hoặc mất kết nối (T-048). Singleton tự tạo lúc chạy (không cần đặt vào scene),
 * sống qua đổi scene. Luật (chủ dự án có thể đổi sau): chủ phòng mất/rời giữa ván -> cả đội về Menu kèm thông báo (không chuyển chủ phòng);
 * một người rời giữa ván -> HOST kết thúc ván cho cả đội, về Menu kèm thông báo (mọi nhiệm vụ cần cả đội). Sau khi đã thắng (SessionExit.GameFinished)
 * người rời không còn kéo cả đội đi. Về Menu từ nút (SessionExit.ReturnToMenu) = host báo lý do cho khách, rời phòng, tắt NetworkManager, hủy NetworkRoot
 * (NetworkManager sống qua đổi scene) để vào lại phòng lần sau tạo bản mới. Thông báo hiện bằng khung nhỏ ở đầu màn hình cho tới khi bấm OK.
 * Hàm: Boot (tĩnh); Awake; OnDestroy; OnApplicationQuit; Update; Hook; Unhook; OnClientDisconnect; Exit; IsOnlineNow; LeaveOnline; LeaveRoutine; OnGUI; ChooseNotice (tĩnh).
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Room
{
    /// <summary>Người gác phiên online: phát hiện mất kết nối/người rời giữa ván và dọn sạch khi về Menu.</summary>
    public sealed class OnlineSessionGuard : MonoBehaviour
    {
        /// <summary>Thông báo mặc định cho máy khách khi mất chủ phòng.</summary>
        public const string ClientLostNotice = "Mất kết nối với chủ phòng";
        /// <summary>Thông báo mặc định cho host khi mạng hỏng.</summary>
        public const string HostLostNotice = "Mất kết nối mạng, ván kết thúc";
        /// <summary>Thông báo khi có người rời giữa ván (host báo cho cả đội).</summary>
        public const string PeerLeftNotice = "Có người chơi đã rời phòng nên ván kết thúc";
        /// <summary>Thông báo khi chủ phòng chủ động rời phòng.</summary>
        public const string HostLeftNotice = "Chủ phòng đã rời phòng";

        private static OnlineSessionGuard instance;

        private NetworkManager hooked;
        private bool gameStarted;
        private bool wasHost;
        private bool exiting;
        private string lastReason = "";
        private string pendingExit;

        /// <summary>Không nhận tham số; tự tạo một bản duy nhất trước khi scene đầu tiên nạp.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (instance != null) return;
            GameObject go = new GameObject("OnlineSessionGuard");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<OnlineSessionGuard>();
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            SessionExit.IsOnline = IsOnlineNow;
            SessionExit.LeaveOnline = LeaveOnline;
        }

        private void OnDestroy()
        {
            Unhook();
            if (instance != this) return;
            instance = null;
            SessionExit.IsOnline = null;
            SessionExit.LeaveOnline = null;
        }

        /// <summary>Không nhận tham số; thoát game thì rời phiên cloud tốt nhất có thể (host thoát thì phòng không còn treo trên cloud).</summary>
        private void OnApplicationQuit()
        {
            NetworkSessionService session = FindFirstObjectByType<NetworkSessionService>();
            if (session != null && session.State != NetworkSessionState.Idle) session.LeaveSession();
        }

        private void Update()
        {
            if (exiting) return;
            if (pendingExit != null)
            {
                string notice = pendingExit;
                pendingExit = null;
                Exit(notice);
                return;
            }
            NetworkManager nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
            {
                Hook(nm);
                wasHost = nm.IsServer;
                RoomRoster roster = RoomRoster.Current;
                if (roster != null && roster.Started.Value) gameStarted = true;
                return;
            }
            Unhook();
            if (!gameStarted) return;
            gameStarted = false;
            Exit(ChooseNotice(lastReason, wasHost));
        }

        /// <summary>Nhận NetworkManager đang chạy; đăng ký nghe người rời một lần cho mỗi NetworkManager.</summary>
        private void Hook(NetworkManager nm)
        {
            if (hooked == nm) return;
            Unhook();
            hooked = nm;
            lastReason = "";
            nm.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void Unhook()
        {
            if (hooked != null) hooked.OnClientDisconnectCallback -= OnClientDisconnect;
            hooked = null;
        }

        /// <summary>Nhận mã client vừa rời. Host: có người rời giữa ván (chưa thắng) thì kết thúc ván. Máy khách: ghi lại lý do host gửi để hiện ở Menu.</summary>
        private void OnClientDisconnect(ulong clientId)
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || exiting) return;
            if (nm.IsServer)
            {
                if (clientId == NetworkManager.ServerClientId || !gameStarted || SessionExit.GameFinished) return;
                pendingExit = PeerLeftNotice; // làm ở Update, không ngắt khách ngay trong lúc NGO đang xử lý sự kiện rời
                return;
            }
            if (clientId == nm.LocalClientId) lastReason = nm.DisconnectReason ?? "";
        }

        /// <summary>Nhận thông báo; kéo máy này về Menu (rời phòng + dọn mạng trước).</summary>
        private void Exit(string notice)
        {
            if (exiting) return;
            Debug.LogWarning("[Net] Kết thúc ván, về Menu: " + notice);
            PrototypePartyController.SetCursor(false);
            SessionExit.ReturnToMenu(SessionExit.DefaultMenuScene, notice);
        }

        /// <summary>Không nhận tham số; còn NetworkManager nghĩa là còn hạ tầng mạng sống qua scene cần dọn trước khi về Menu.</summary>
        private static bool IsOnlineNow()
        {
            return NetworkManager.Singleton != null;
        }

        /// <summary>Nhận thông báo cho các máy còn lại và hành động chạy sau khi dọn xong; bỏ qua nếu đang rời.</summary>
        private void LeaveOnline(string notice, Action then)
        {
            if (exiting) return;
            StartCoroutine(LeaveRoutine(notice, then));
        }

        /// <summary>Host báo lý do rồi ngắt từng khách, rời phiên + tắt NetworkManager (có hạn chờ), hủy NetworkRoot, rồi gọi hành động tiếp theo.</summary>
        private IEnumerator LeaveRoutine(string notice, Action then)
        {
            exiting = true;
            NetworkManager nm = NetworkManager.Singleton;
            if (nm != null && nm.IsServer && nm.IsListening)
            {
                string reason = string.IsNullOrEmpty(notice) ? HostLeftNotice : notice;
                List<ulong> ids = new List<ulong>(nm.ConnectedClientsIds);
                for (int i = 0; i < ids.Count; i++)
                {
                    if (ids[i] == NetworkManager.ServerClientId) continue;
                    try { nm.DisconnectClient(ids[i], reason); }
                    catch (Exception e) { Debug.LogWarning("[Net] Ngắt khách lỗi: " + e.Message); }
                }
                yield return new WaitForSecondsRealtime(0.4f);
            }

            NetworkSessionService session = nm != null ? nm.GetComponent<NetworkSessionService>() : null;
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();
            if (session != null)
            {
                Task leave = session.LeaveSessionAsync();
                float limit = Time.realtimeSinceStartup + 6f;
                while (!leave.IsCompleted && Time.realtimeSinceStartup < limit) yield return null;
            }
            else if (nm != null && nm.IsListening) nm.Shutdown();

            float stop = Time.realtimeSinceStartup + 3f;
            while (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.ShutdownInProgress) && Time.realtimeSinceStartup < stop) yield return null;

            nm = NetworkManager.Singleton;
            if (nm != null) Destroy(nm.gameObject);
            Unhook();
            gameStarted = false;
            lastReason = "";
            exiting = false;
            if (then != null) then();
        }

        private void OnGUI()
        {
            string notice = SessionExit.MenuNotice;
            if (string.IsNullOrEmpty(notice) || exiting) return;
            float vw, vh;
            ModernGui.Begin(out vw, out vh);
            const float w = 560f;
            const float h = 230f;
            GUILayout.BeginArea(new Rect((vw - w) * 0.5f, 48f, w, h), GUI.skin.box);
            GUILayout.Label("Thông báo", ModernGui.Title);
            GUILayout.Label(notice);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK", ModernGui.Primary)) SessionExit.MenuNotice = "";
            GUILayout.EndArea();
            ModernGui.End();
        }

        /// <summary>Nhận lý do NGO báo và cờ máy này từng là host; trả câu thông báo tiếng Việt (lý do kỹ thuật bắt đầu bằng '[' hoặc rỗng thì dùng câu mặc định).</summary>
        public static string ChooseNotice(string reason, bool wasHost)
        {
            if (!string.IsNullOrWhiteSpace(reason) && !reason.StartsWith("[")) return reason;
            return wasHost ? HostLostNotice : ClientLostNotice;
        }
    }
}
