// Mục đích: tự chạy kiểm thử LAN 2 người chơi cho T-009 S1 khi game được mở kèm tham số dòng lệnh.
//   Chỉ kích hoạt khi có tham số -s1auto. Không có tham số thì KHÔNG làm gì (không ảnh hưởng game thường).
//   Tham số: -s1auto (bật) | -s1join (tự Join LAN) | -s1walk (người chơi của bản này tự đi sang +X) | -s1quit=<giây> (tự thoát, mặc định 45)
//   Mỗi giây ghi log '[S1Auto]' gồm trạng thái kết nối và vị trí mọi NetworkObject đã spawn, để đối chiếu giữa các tiến trình.
// Hàm: Bootstrap, Start, Update, LogSnapshot, GetArgValue.
using System;
using System.Collections;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn.Network
{
    public sealed class NetworkS1AutoRun : MonoBehaviour
    {
        private const string SceneName = "NetworkS1Test";
        private bool _walk;
        private float _walkStart = -1f;
        private const float WalkDelay = 3f;     // chờ sau khi kết nối rồi mới đi
        private const float WalkDuration = 3f;  // thời gian đi
        private const float WalkSpeed = 2f;     // m/giây theo +X

        /// <summary>Tự tạo object chạy kiểm thử nếu có -s1auto. Nhận: không. Trả: không.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-s1auto") < 0) return;
            var go = new GameObject("S1AutoRun");
            DontDestroyOnLoad(go);
            go.AddComponent<NetworkS1AutoRun>();
        }

        /// <summary>Chờ scene + dịch vụ phiên, tự Join nếu cần, rồi ghi log mỗi giây tới khi hết giờ thì thoát. Nhận/Trả: không.</summary>
        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            bool join = Array.IndexOf(args, "-s1join") >= 0;
            _walk = Array.IndexOf(args, "-s1walk") >= 0;
            float quitAfter = 45f;
            float.TryParse(GetArgValue(args, "-s1quit="), out quitAfter);
            if (quitAfter <= 0f) quitAfter = 45f;
            Debug.Log("[S1Auto] START join=" + join + " walk=" + _walk + " quitAfter=" + quitAfter);

            NetworkSessionService svc = null;
            float waited = 0f;
            while (waited < 10f && (svc = FindFirstObjectByType<NetworkSessionService>()) == null)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            if (svc == null || SceneManager.GetActiveScene().name != SceneName)
            {
                Debug.LogError("[S1Auto] Không thấy NetworkSessionService/scene " + SceneName);
                yield break;
            }

            yield return new WaitForSecondsRealtime(1f);
            if (join) svc.StartLanClient();

            float end = Time.realtimeSinceStartup + quitAfter;
            while (Time.realtimeSinceStartup < end)
            {
                LogSnapshot(svc);
                yield return new WaitForSecondsRealtime(1f);
            }
            Debug.Log("[S1Auto] DONE");
            if (Application.isEditor) yield break;
            Application.Quit();
        }

        /// <summary>Mỗi frame: sau khi kết nối đủ lâu thì đẩy người chơi của bản này đi sang +X (qua transform của chủ sở hữu). Nhận/Trả: không.</summary>
        private void Update()
        {
            var nm = NetworkManager.Singleton;
            if (!_walk || nm == null || !nm.IsConnectedClient || nm.LocalClient == null || nm.LocalClient.PlayerObject == null) return;
            if (_walkStart < 0f) _walkStart = Time.realtimeSinceStartup + WalkDelay;
            float t = Time.realtimeSinceStartup;
            if (t < _walkStart || t > _walkStart + WalkDuration) return;
            nm.LocalClient.PlayerObject.transform.position += Vector3.right * WalkSpeed * Time.deltaTime;
        }

        /// <summary>Ghi 1 dòng log trạng thái mạng + vị trí các NetworkObject đã spawn. Nhận: dịch vụ phiên. Trả: không.</summary>
        private static void LogSnapshot(NetworkSessionService svc)
        {
            var nm = NetworkManager.Singleton;
            var sb = new StringBuilder("[S1Auto] state=").Append(svc.State);
            if (nm != null && nm.IsListening)
            {
                sb.Append(" host=").Append(nm.IsHost).Append(" connectedClient=").Append(nm.IsConnectedClient)
                  .Append(" me=").Append(nm.LocalClientId).Append(" spawned=").Append(nm.SpawnManager.SpawnedObjects.Count);
                foreach (var kv in nm.SpawnManager.SpawnedObjects)
                {
                    var p = kv.Value.transform.position;
                    sb.Append(" | owner=").Append(kv.Value.OwnerClientId).Append(" pos=(").Append(p.x.ToString("F2")).Append(",").Append(p.y.ToString("F2")).Append(",").Append(p.z.ToString("F2")).Append(')');
                }
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>Lấy giá trị sau tiền tố của một tham số. Nhận: mảng tham số, tiền tố. Trả: chuỗi giá trị hoặc rỗng.</summary>
        private static string GetArgValue(string[] args, string prefix)
        {
            foreach (var a in args) if (a.StartsWith(prefix, StringComparison.Ordinal)) return a.Substring(prefix.Length);
            return "";
        }
    }
}
