/*
 * Mục đích: Cửa vào duy nhất để bật/tắt phiên ONLINE (T-054). Phiên (NetworkManager + dịch vụ phòng + bàn giao map) là prefab Resources/OnlineBootstrap/PF_OnlineSession,
 * chỉ được sinh khi người chơi chọn Chơi online ở MainMenu và sống xuyên scene MainMenu -> Sanh_Cho -> MapSchoolF. Chơi offline không có phiên này.
 * Hàm: Ensure; Shutdown; thuộc tính Active, Service.
 */
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Room
{
    /// <summary>Bật/tắt phiên online dùng chung cho mọi scene.</summary>
    public static class OnlineSession
    {
        private const string PrefabPath = "OnlineBootstrap/PF_OnlineSession";

        /// <summary>True khi đã có NetworkManager (phiên online đang tồn tại).</summary>
        public static bool Active { get { return NetworkManager.Singleton != null; } }

        /// <summary>Dịch vụ phòng của phiên hiện tại (null nếu chưa có phiên).</summary>
        public static RoomService Service { get { return RoomService.Instance; } }

        /// <summary>Không nhận tham số; trả dịch vụ phòng, sinh phiên từ prefab nếu chưa có. Dịch vụ chỉ dùng được khi Service.Ready = true (sau một khung hình). Trả null nếu thiếu prefab.</summary>
        public static RoomService Ensure()
        {
            if (Active) return RoomService.Instance;
            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[OnlineSession] Không thấy prefab Resources/" + PrefabPath);
                return null;
            }
            GameObject go = Object.Instantiate(prefab);
            go.name = "OnlineSession";
            Object.DontDestroyOnLoad(go);
            return go.GetComponent<RoomService>();
        }

        /// <summary>Không nhận tham số; rời phòng và hủy toàn bộ phiên online (về chơi offline hoặc về menu).</summary>
        public static void Shutdown()
        {
            RoomService service = RoomService.Instance;
            if (service != null) service.Leave();
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null) return;
            if (nm.IsListening) nm.Shutdown();
            Object.Destroy(nm.gameObject);
        }
    }
}
