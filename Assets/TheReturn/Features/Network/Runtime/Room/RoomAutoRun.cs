/*
 * Mục đích: Chạy kiểm thử phòng online TỰ ĐỘNG qua tham số dòng lệnh (T-026), chỉ kích hoạt khi có -roomauto. Mỗi giây ghi log '[Room]' gồm
 * trạng thái phiên, mã phòng, danh sách ô + tên, đã bắt đầu chưa; để đối chiếu giữa các tiến trình mà không cần bấm tay.
 * Tham số: -roomauto lanhost|lanclient|relayhost|relayclient  -roomsize N  -roomname Tên  -roomcode MÃ  -roomproto dtls|wss|udp  -roomstart  -roomquit giây
 *   -roomcodefile ĐƯỜNG_DẪN: relayhost ghi mã phòng vào file; relayclient (không có -roomcode) chờ file rồi đọc mã (test Relay tự động 1 lần bấm)
 * Hàm: Bootstrap; Run; Snapshot; Arg.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace TheReturn.Network.Room
{
    public sealed class RoomAutoRun : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-roomauto") < 0) return;
            GameObject go = new GameObject("RoomAutoRun");
            DontDestroyOnLoad(go);
            go.AddComponent<RoomAutoRun>();
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            string mode = (Arg(args, "-roomauto") ?? "").ToLowerInvariant();
            int size = 2; int.TryParse(Arg(args, "-roomsize"), out size); if (size < 2) size = 2;
            string playerName = Arg(args, "-roomname") ?? "";
            string code = Arg(args, "-roomcode") ?? "";
            string proto = (Arg(args, "-roomproto") ?? "").ToLowerInvariant();
            bool autoStart = Array.IndexOf(args, "-roomstart") >= 0;
            string codeFile = Arg(args, "-roomcodefile") ?? "";
            float quit = 50f; float.TryParse(Arg(args, "-roomquit"), out quit); if (quit <= 0f) quit = 50f;

            float waited = 0f;
            RoomService rooms = null;
            while (waited < 10f && (rooms = FindFirstObjectByType<RoomService>()) == null) { waited += Time.unscaledDeltaTime; yield return null; }
            if (rooms == null) { Debug.LogError("[Room] Không thấy RoomService"); yield break; }
            yield return new WaitForSecondsRealtime(1f);

            NetworkSessionService s = rooms.Session;
            if (proto == "wss") s.RelayProtocolChoice = RelayProtocol.WSS;
            else if (proto == "udp") s.RelayProtocolChoice = RelayProtocol.UDP;
            else if (proto == "dtls") s.RelayProtocolChoice = RelayProtocol.DTLS;
            Debug.Log("[Room] START mode=" + mode + " size=" + size + " name=" + playerName + " proto=" + s.RelayProtocolChoice + " code=" + code + " autoStart=" + autoStart);
            if (mode == "relayclient" && code.Length == 0 && codeFile.Length > 0)
            {
                float give = Time.realtimeSinceStartup + 40f;
                while (Time.realtimeSinceStartup < give && code.Length == 0)
                {
                    try { if (File.Exists(codeFile)) code = File.ReadAllText(codeFile).Trim(); } catch (IOException) { }
                    if (code.Length == 0) yield return new WaitForSecondsRealtime(0.5f);
                }
                Debug.Log("[Room] Đọc mã phòng từ file: " + code);
            }
            if (mode == "lanhost") rooms.CreateLan(size, playerName);
            else if (mode == "lanclient") rooms.JoinLan(playerName);
            else if (mode == "relayhost") rooms.CreateRelay(size, playerName);
            else if (mode == "relayclient") rooms.JoinRelay(code, playerName);

            float end = Time.realtimeSinceStartup + quit;
            bool started = false;
            bool codeWritten = false;
            while (Time.realtimeSinceStartup < end)
            {
                if (mode == "relayhost" && codeFile.Length > 0 && !codeWritten && s.JoinCode.Length > 0)
                {
                    codeWritten = true;
                    File.WriteAllText(codeFile, s.JoinCode);
                    Debug.Log("[Room] Đã ghi mã phòng ra file: " + s.JoinCode);
                }
                RoomRoster r = RoomRoster.Current;
                if (autoStart && !started && r != null && s.State == NetworkSessionState.Hosting && r.Rows.Count >= r.TeamSize.Value)
                {
                    started = true;
                    Debug.Log("[Room] AUTO-START bấm Bắt đầu");
                    rooms.StartGame();
                }
                Snapshot(rooms);
                yield return new WaitForSecondsRealtime(1f);
            }
            Debug.Log("[Room] DONE");
            Application.Quit();
        }

        private static void Snapshot(RoomService rooms)
        {
            NetworkSessionService s = rooms.Session;
            StringBuilder sb = new StringBuilder("[Room] state=").Append(s.State).Append(" code=").Append(s.JoinCode);
            RoomRoster r = RoomRoster.Current;
            if (r != null)
            {
                sb.Append(" team=").Append(r.TeamSize.Value).Append(" rows=").Append(r.Rows.Count).Append(" started=").Append(r.Started.Value);
                for (int i = 0; i < r.Rows.Count; i++) sb.Append(" | slot=").Append(r.Rows[i].Slot).Append(" ").Append(r.Rows[i].Name.ToString()).Append(r.Rows[i].IsHost ? " (H)" : "");
            }
            if (!string.IsNullOrEmpty(rooms.Notice)) sb.Append(" NOTICE=").Append(rooms.Notice);
            Debug.Log(sb.ToString());
        }

        private static string Arg(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
            return null;
        }
    }
}
