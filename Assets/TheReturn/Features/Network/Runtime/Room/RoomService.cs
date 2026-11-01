/*
 * Mục đích: Điều phối PHÒNG online (T-026) trên NetworkSessionService: tạo phòng LAN/Relay với số người cố định 2-4, vào phòng bằng mã,
 * rời phòng, bắt đầu. Duyệt kết nối (đủ người/đã bắt đầu thì từ chối), sinh RoomRoster khi host chạy, báo khi mất kết nối với chủ phòng.
 * Hàm: Awake; Start; OnDestroy; CreateRelay; CreateLan; JoinRelay; JoinLan; Leave; StartGame; Approve; SpawnRoster; OnClientDisconnect.
 */
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Room
{
    public sealed class RoomService : MonoBehaviour
    {
        [SerializeField] private NetworkSessionService session;
        [SerializeField] private RoomRoster rosterPrefab;

        /// <summary>Tên người chơi và số người đội do giao diện đặt trước khi tạo/vào phòng (RoomRoster đọc).</summary>
        public static string PendingName = "";
        public static int PendingTeamSize = 2;

        /// <summary>Dịch vụ phòng đang hoạt động trong scene.</summary>
        public static RoomService Instance { get; private set; }

        /// <summary>Thông báo gần nhất cho người chơi (vd bị từ chối, mất kết nối chủ phòng).</summary>
        public string Notice { get; private set; } = "";

        public NetworkSessionService Session { get { return session; } }

        private void Awake()
        {
            Instance = this;
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();
        }

        private void Start()
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null) return;
            nm.NetworkConfig.ConnectionApproval = true;
            nm.ConnectionApprovalCallback = Approve;
            nm.OnServerStarted += SpawnRoster;
            nm.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null) return;
            nm.OnServerStarted -= SpawnRoster;
            nm.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        /// <summary>Tạo phòng Relay. Nhận số người (2-4) và tên. Trả: không (xem session.State/JoinCode).</summary>
        public void CreateRelay(int teamSize, string playerName)
        {
            Prepare(teamSize, playerName);
            session.MaxPlayers = PendingTeamSize;
            session.StartRelayHost();
        }

        /// <summary>Tạo phòng LAN/localhost. Nhận số người (2-4) và tên.</summary>
        public void CreateLan(int teamSize, string playerName)
        {
            Prepare(teamSize, playerName);
            session.StartLanHost();
        }

        /// <summary>Vào phòng Relay bằng mã. Nhận mã và tên.</summary>
        public void JoinRelay(string code, string playerName)
        {
            Prepare(PendingTeamSize, playerName);
            session.JoinRelayByCode(code);
        }

        /// <summary>Vào phòng LAN (địa chỉ theo session.LanAddress). Nhận tên.</summary>
        public void JoinLan(string playerName)
        {
            Prepare(PendingTeamSize, playerName);
            session.StartLanClient();
        }

        /// <summary>Rời phòng / đóng phòng (chủ phòng rời thì phòng đóng).</summary>
        public void Leave()
        {
            Notice = "";
            session.LeaveSession();
        }

        /// <summary>Chủ phòng bấm Bắt đầu. Host tự kiểm đủ người; người khác bấm sẽ bị từ chối.</summary>
        public void StartGame()
        {
            if (RoomRoster.Current != null) RoomRoster.Current.RequestStart();
        }

        private void Prepare(int teamSize, string playerName)
        {
            Notice = "";
            PendingTeamSize = Mathf.Clamp(teamSize, 2, 4);
            PendingName = playerName ?? "";
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            NetworkManager nm = NetworkManager.Singleton;
            bool ok = RoomRoster.Current != null ? RoomRoster.Current.CanAdmit(nm.ConnectedClientsIds.Count) : nm.ConnectedClientsIds.Count < PendingTeamSize;
            response.Approved = ok;
            response.CreatePlayerObject = false;
            if (!ok) response.Reason = "Phòng đã đủ người hoặc ván đã bắt đầu";
        }

        private void SpawnRoster()
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer || rosterPrefab == null || RoomRoster.Current != null) return;
            RoomRoster roster = Instantiate(rosterPrefab);
            roster.GetComponent<NetworkObject>().Spawn();
        }

        private void OnClientDisconnect(ulong clientId)
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || nm.IsServer || clientId != nm.LocalClientId) return;
            if (session.State == NetworkSessionState.Idle) return;
            string reason = nm.DisconnectReason;
            // Chuỗi bắt đầu bằng '[' là thông báo kỹ thuật của transport; chỉ hiện lý do do mình đặt (vd phòng đầy).
            Notice = (string.IsNullOrEmpty(reason) || reason.StartsWith("[")) ? "Mất kết nối với chủ phòng" : reason;
            session.LeaveSession();
        }
    }
}
