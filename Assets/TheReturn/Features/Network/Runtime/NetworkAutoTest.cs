// Mục đích: công cụ KIỂM THỬ cho T-009 S1 (không dùng khi chơi thật). Cho bản build/Editor tự host hoặc client LAN qua tham số dòng lệnh,
//   tự di chuyển người chơi do mình sở hữu, và ghi log vị trí mọi người chơi đã spawn để kiểm tra đồng bộ OwnerNetworkTransform.
// Tham số dòng lệnh: -netauto host|client   -netmove   -netquit <giây>
// Hàm: Awake, Update, StartMoveIfReady, LogSnapshot.
using System;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network
{
    public sealed class NetworkAutoTest : MonoBehaviour
    {
        [SerializeField] private NetworkSessionService session;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveSeconds = 4f;
        [SerializeField] private float logInterval = 1f;

        /// <summary>Bật để người chơi của máy này tự đi sang phải (+X) trong moveSeconds giây. Có thể bật từ Editor.</summary>
        public bool AutoMove;

        private string _mode = "";
        private float _quitAfter = -1f;
        private float _t0;
        private float _nextLog;
        private float _moveElapsed = -1f;
        private bool _started;
        private bool _moveDone;

        /// <summary>Đọc tham số dòng lệnh khi khởi động. Nhận/Trả: không.</summary>
        private void Awake()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-netauto" && i + 1 < args.Length) _mode = args[i + 1].ToLowerInvariant();
                else if (args[i] == "-netmove") AutoMove = true;
                else if (args[i] == "-netquit" && i + 1 < args.Length)
                    float.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out _quitAfter);
            }
            if (session == null) session = FindFirstObjectByType<NetworkSessionService>();
            _t0 = Time.realtimeSinceStartup;
            if (_mode != "") Debug.Log("[NetTest] Khởi động mode=" + _mode + " move=" + AutoMove + " quit=" + _quitAfter);
        }

        /// <summary>Mỗi frame: tự start host/client theo tham số, tự di chuyển, ghi log, tự thoát. Nhận/Trả: không.</summary>
        private void Update()
        {
            if (!_started && _mode != "" && session != null)
            {
                _started = true;
                if (_mode == "host") session.StartLanHost();
                else if (_mode == "client") session.StartLanClient();
            }
            StartMoveIfReady();
            if (Time.realtimeSinceStartup >= _nextLog)
            {
                _nextLog = Time.realtimeSinceStartup + logInterval;
                LogSnapshot();
            }
            if (_quitAfter > 0f && Time.realtimeSinceStartup - _t0 > _quitAfter)
            {
                Debug.Log("[NetTest] Hết giờ, thoát.");
                _quitAfter = -1f;
                Application.Quit();
            }
        }

        /// <summary>Nếu AutoMove bật và đã có PlayerObject của mình, đẩy nó sang +X trong moveSeconds giây. Nhận/Trả: không.</summary>
        private void StartMoveIfReady()
        {
            if (!AutoMove || _moveDone) return;
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.LocalClient == null) return;
            var player = nm.LocalClient.PlayerObject;
            if (player == null || !player.IsSpawned || !player.IsOwner) return;
            if (_moveElapsed < 0f)
            {
                _moveElapsed = 0f;
                Debug.Log("[NetTest] MOVE_START localId=" + nm.LocalClientId + " x=" + player.transform.position.x.ToString("F2", CultureInfo.InvariantCulture));
            }
            if (_moveElapsed < moveSeconds)
            {
                _moveElapsed += Time.deltaTime;
                player.transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            }
            else
            {
                _moveDone = true;
                Debug.Log("[NetTest] MOVE_END localId=" + nm.LocalClientId + " x=" + player.transform.position.x.ToString("F2", CultureInfo.InvariantCulture));
            }
        }

        /// <summary>Ghi log: trạng thái mạng + id, owner, vị trí của mọi object đã spawn. Nhận/Trả: không.</summary>
        private void LogSnapshot()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening) return;
            var sb = new System.Text.StringBuilder();
            sb.Append("[NetTest] SNAP host=").Append(nm.IsHost).Append(" client=").Append(nm.IsClient)
              .Append(" connected=").Append(nm.IsConnectedClient || nm.IsHost).Append(" local=").Append(nm.LocalClientId)
              .Append(" clients=").Append(nm.IsServer ? nm.ConnectedClientsIds.Count : -1)
              .Append(" spawned=").Append(nm.SpawnManager != null ? nm.SpawnManager.SpawnedObjects.Count : 0);
            if (nm.SpawnManager != null)
            {
                foreach (var kv in nm.SpawnManager.SpawnedObjects)
                {
                    var p = kv.Value.transform.position;
                    sb.Append(" | id=").Append(kv.Key).Append(" owner=").Append(kv.Value.OwnerClientId)
                      .Append(" pos=(").Append(p.x.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
                      .Append(p.y.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
                      .Append(p.z.ToString("F2", CultureInfo.InvariantCulture)).Append(")");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
