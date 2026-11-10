/*
 * Mục đích: BÀN GIAO đội từ PHÒNG online sang scene MAP (T-040 Q-014, đường «phòng -> map» của P-001). Đặt trên cùng đối tượng NetworkManager
 * của scene phòng (sống qua đổi scene). Khi chủ phòng bấm Bắt đầu (RoomRoster.Started), HOST: kiểm danh sách phòng bằng RoomHandoffPlan, nạp scene
 * map cho CẢ ĐỘI bằng NetworkSceneManager, đợi mọi máy nạp xong, đặt số người cho OnlinePartySlots rồi spawn nhân vật (OnlineAvatar) cho từng người
 * THEO THỨ TỰ Ô (nên ô nhân vật = ô trong phòng; nhân vật tự đặt mình về điểm xuất phát). Máy khách chỉ hiện trạng thái, scene được NGO đồng bộ.
 * Scene phòng chưa đăng ký prefab nhân vật nên component tự thêm vào NetworkManager ở Start trên MỌI máy (trước khi tạo/vào phòng, để ForceSamePrefabs khớp).
 * Không sửa file cũ nào; scene map KHÔNG được có NetworkManager riêng (chỉ OnlinePartySlots + các cặp Online của nhiệm vụ).
 * Hàm: Start; Update; BeginHandoff; BeginForTest; OnLoadEventCompleted; Fail; OnDestroy; OnGUI; thuộc tính Phase, Status, Plan.
 */
using System.Collections.Generic;
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using TheReturn.Network.Room;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn.Network.Handoff
{
    /// <summary>Giai đoạn bàn giao phòng -> map.</summary>
    public enum HandoffPhase
    {
        /// <summary>Chưa bắt đầu ván.</summary>
        Waiting,
        /// <summary>Đang nạp scene map (cả đội).</summary>
        Loading,
        /// <summary>Scene đã nạp, host đang spawn nhân vật.</summary>
        Spawning,
        /// <summary>Xong: nhân vật đã spawn (trên máy khách: đã nhận lệnh vào map).</summary>
        Done,
        /// <summary>Lỗi; xem Status.</summary>
        Failed
    }

    public sealed class RoomMapHandoff : MonoBehaviour
    {
        [Tooltip("Tên scene map (phải có trong Build Settings, KHÔNG chứa NetworkManager).")]
        [SerializeField] private string mapSceneName = "";
        [Tooltip("Prefab nhân vật online (PF_OnlineAvatar).")]
        [SerializeField] private GameObject avatarPrefab;

        private RoomHandoffPlan plan;
        private bool prefabRegistered;
        private bool begun;
        private bool subscribed;

        /// <summary>Giai đoạn hiện tại.</summary>
        public HandoffPhase Phase { get; private set; }

        /// <summary>Thông báo tiếng Việt cho người chơi/nhật ký.</summary>
        public string Status { get; private set; } = string.Empty;

        /// <summary>Kế hoạch đã kiểm (chỉ có trên host sau khi bắt đầu; null nếu chưa).</summary>
        public RoomHandoffPlan Plan { get { return plan; } }

        /// <summary>Đặt tên scene và prefab bằng code (dùng khi dựng scene tự động). Nhận tên scene map và prefab nhân vật.</summary>
        public void Configure(string sceneName, GameObject avatar)
        {
            mapSceneName = sceneName;
            avatarPrefab = avatar;
        }

        private void Start()
        {
            RegisterAvatarPrefab();
        }

        private void Update()
        {
            if (!prefabRegistered) RegisterAvatarPrefab();
            if (begun) return;
            NetworkManager nm = NetworkManager.Singleton;
            RoomRoster roster = RoomRoster.Current;
            if (nm == null || !nm.IsListening || roster == null || !roster.Started.Value) return;
            if (nm.IsServer)
            {
                BeginHandoff(nm, roster, false);
            }
            else
            {
                begun = true;
                Phase = HandoffPhase.Loading;
                Status = "Chủ phòng đã bắt đầu, đang vào map...";
            }
        }

        /// <summary>Thêm prefab nhân vật vào NetworkManager (một lần, nếu chưa có). Phải xảy ra trước khi tạo/vào phòng; sau đó chỉ cảnh báo.</summary>
        private void RegisterAvatarPrefab()
        {
            if (prefabRegistered || avatarPrefab == null) return;
            NetworkManager nm = NetworkManager.Singleton != null ? NetworkManager.Singleton : FindFirstObjectByType<NetworkManager>();
            if (nm == null) return;
            if (nm.IsListening)
            {
                Debug.LogWarning("[RoomMapHandoff] NetworkManager đã chạy mà chưa đăng ký prefab nhân vật; spawn có thể lỗi trên máy khách.");
                prefabRegistered = true;
                return;
            }
            if (!nm.NetworkConfig.Prefabs.Contains(avatarPrefab)) nm.AddNetworkPrefab(avatarPrefab);
            prefabRegistered = true;
        }

        /// <summary>Chỉ HOST. Kiểm danh sách phòng rồi nạp scene map cho cả đội. Nhận NetworkManager, danh sách phòng và cờ cho phép thiếu người (chỉ kiểm thử).</summary>
        private void BeginHandoff(NetworkManager nm, RoomRoster roster, bool allowPartial)
        {
            begun = true;
            List<HandoffMember> rows = new List<HandoffMember>();
            for (int i = 0; i < roster.Rows.Count; i++) rows.Add(new HandoffMember(roster.Rows[i].ClientId, roster.Rows[i].Slot));
            string error;
            if (!RoomHandoffPlan.TryBuild(rows, roster.TeamSize.Value, allowPartial, out plan, out error)) { Fail(error); return; }
            if (string.IsNullOrEmpty(mapSceneName)) { Fail("Chưa đặt tên scene map."); return; }
            if (avatarPrefab == null) { Fail("Chưa gán prefab nhân vật online."); return; }
            if (nm.SceneManager == null) { Fail("NetworkManager đang tắt quản lý scene."); return; }
            Phase = HandoffPhase.Loading;
            Status = "Đang nạp map cho cả đội (" + plan.Members.Count + "/" + plan.TeamSize + ")...";
            nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            subscribed = true;
            SceneEventProgressStatus started = nm.SceneManager.LoadScene(mapSceneName, LoadSceneMode.Single);
            if (started != SceneEventProgressStatus.Started) Fail("Không nạp được scene '" + mapSceneName + "': " + started + ".");
        }

        /// <summary>Chỉ để KIỂM THỬ một máy: host bắt đầu bàn giao ngay cả khi phòng chưa đủ người. Không dùng khi chơi thật.</summary>
        public void BeginForTest()
        {
            NetworkManager nm = NetworkManager.Singleton;
            RoomRoster roster = RoomRoster.Current;
            if (begun || nm == null || !nm.IsServer || roster == null) return;
            BeginHandoff(nm, roster, true);
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName != mapSceneName) return;
            Unsubscribe();
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer || plan == null) return;
            foreach (HandoffMember m in plan.Members)
            {
                if (!clientsCompleted.Contains(m.ClientId)) { Fail("Máy của người chơi " + (m.Slot + 1) + " (client " + m.ClientId + ") nạp map quá lâu hoặc thất bại."); return; }
            }
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null) { Fail("Scene map thiếu OnlinePartySlots."); return; }
            slots.SetTeamSize(plan.TeamSize);
            Phase = HandoffPhase.Spawning;
            foreach (HandoffMember m in plan.Members)
            {
                GameObject body = Instantiate(avatarPrefab);
                NetworkObject net = body.GetComponent<NetworkObject>();
                if (net == null) { Destroy(body); Fail("Prefab nhân vật không có NetworkObject."); return; }
                net.SpawnAsPlayerObject(m.ClientId, true);
            }
            Phase = HandoffPhase.Done;
            Status = "Đã vào map: " + plan.Members.Count + " người.";
            Debug.Log("[RoomMapHandoff] " + Status + " scene=" + sceneName);
        }

        private void Fail(string reason)
        {
            Unsubscribe();
            Phase = HandoffPhase.Failed;
            Status = reason;
            Debug.LogError("[RoomMapHandoff] " + reason);
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            NetworkManager nm = NetworkManager.Singleton;
            if (nm != null && nm.SceneManager != null) nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnGUI()
        {
            if (Phase == HandoffPhase.Waiting || Phase == HandoffPhase.Done) return;
            GUI.Box(new Rect(Screen.width * 0.5f - 260f, 12f, 520f, 30f), Status);
        }
    }
}
