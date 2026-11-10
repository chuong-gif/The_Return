/*
 * Mục đích: Khởi động mạng cho bản build thử của map thật (T-028) mà KHÔNG đặt NetworkManager vào scene. Chỉ khi chạy với tham số -netauto
 * (host/client, như các bản build thử) và scene chưa có NetworkManager thì tạo NetworkRoot từ prefab trong Resources/OnlineBootstrap; NetworkAutoTest
 * trong prefab đọc tham số và tự bắt đầu host hoặc vào phòng. Chơi bình thường (không tham số) thì không làm gì. Đường «phòng -> map» thật sẽ thay thế
 * cơ chế này (task riêng).
 * Hàm: Boot.
 */
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Online
{
    public static class OnlineMapBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool wanted = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-netauto") wanted = true;
            }
            if (!wanted || Object.FindObjectOfType<NetworkManager>() != null) return;
            GameObject prefab = Resources.Load<GameObject>("OnlineBootstrap/PF_NetworkRoot");
            if (prefab == null)
            {
                Debug.LogWarning("[OnlineMapBootstrap] Không thấy prefab Resources/OnlineBootstrap/PF_NetworkRoot.");
                return;
            }
            Object.Instantiate(prefab).name = "NetworkRoot (bootstrap)";
        }
    }
}
