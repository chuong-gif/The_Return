/*
 * Mục đích: Chạy kiểm thử «Bảng cầu dao» online TỰ ĐỘNG qua tham số dòng lệnh (T-031), chỉ kích hoạt khi có -brkauto. Mỗi tiến trình (host hay khách)
 * điều khiển người chơi của chính nó: bật các công tắc CỦA MÌNH nằm trong đáp án (đưa nhân vật tới công tắc rồi gửi lệnh qua mạng thật) và ghi log
 * '[BrkAuto]' mỗi giây (đèn, công tắc bật, thắng, công tắc riêng) để đối chiếu giữa các tiến trình.
 * Tham số: -brkauto solve|far  -brkteam N (host: số người đội)  -brksolfile ĐƯỜNG_DẪN (host ghi mặt nạ đáp án, khách đọc)  -brkdelay giây  -brkquit giây (mặc định 45)
 *   Chế độ far: KHÔNG đưa nhân vật tới công tắc, thử bấm công tắc đầu tiên của mình để xem host có từ chối vì đứng xa không.
 * Hàm: Start (coroutine); Snapshot; Arg.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.BreakerNet.Online
{
    public sealed class BreakerOnlineAutoRun : MonoBehaviour
    {
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            string mode = (Arg(args, "-brkauto") ?? "").ToLowerInvariant();
            if (mode.Length == 0) yield break;
            int team = 0; int.TryParse(Arg(args, "-brkteam") ?? "0", out team);
            string solFile = Arg(args, "-brksolfile") ?? "";
            float delay = 3f; float.TryParse(Arg(args, "-brkdelay") ?? "3", out delay);
            float quit = 45f; float.TryParse(Arg(args, "-brkquit") ?? "45", out quit); if (quit <= 0f) quit = 45f;
            Debug.Log("[BrkAuto] START mode=" + mode + " team=" + team + " solFile=" + solFile + " delay=" + delay + " quit=" + quit);

            BreakerOnlineHost host = FindFirstObjectByType<BreakerOnlineHost>();
            BreakerOnlineClient client = FindFirstObjectByType<BreakerOnlineClient>();
            if (client == null) { Debug.LogError("[BrkAuto] Không thấy BreakerOnlineClient"); yield break; }

            float end = Time.realtimeSinceStartup + quit;
            if (team >= 2) TheReturn.Network.Online.OnlinePartySlots.Instance.SetTeamSize(team);
            bool solutionWritten = false, acted = false;
            float readyAt = -1f;
            while (Time.realtimeSinceStartup < end)
            {
                NetworkManager nm = NetworkManager.Singleton;
                bool hosting = nm != null && nm.IsListening && nm.IsServer;
                if (hosting && host != null && solFile.Length > 0 && !solutionWritten && host.SolutionMaskForTest >= 0)
                {
                    solutionWritten = true;
                    File.WriteAllText(solFile, host.SolutionMaskForTest.ToString());
                    Debug.Log("[BrkAuto] Host đã ghi mặt nạ đáp án ra file: " + host.SolutionMaskForTest);
                }
                if (client.HasState && !acted)
                {
                    if (readyAt < 0f) readyAt = Time.realtimeSinceStartup;
                    if (Time.realtimeSinceStartup - readyAt >= delay)
                    {
                        int mask = -1;
                        try
                        {
                            if (mode == "far") mask = 0x3F;
                            else if (solFile.Length > 0 && File.Exists(solFile)) int.TryParse(File.ReadAllText(solFile).Trim(), out mask);
                        }
                        catch (IOException) { }
                        if (mask >= 0)
                        {
                            acted = true;
                            int[] owned = client.Private.SwitchesCopy();
                            int count = 0;
                            for (int i = 0; i < owned.Length; i++)
                            {
                                int sw = owned[i];
                                if (((mask >> sw) & 1) == 0) continue;
                                if (mode != "far") client.PlaceNearSwitch(sw);
                                yield return new WaitForSecondsRealtime(1f);
                                Debug.Log("[BrkAuto] Ô " + client.MySlot + " gửi lệnh bật công tắc " + sw + (mode == "far" ? " (đứng xa)" : ""));
                                client.RequestToggle(sw);
                                count++;
                                if (mode == "far") break;
                            }
                            Debug.Log("[BrkAuto] Ô " + client.MySlot + " đã gửi " + count + " lệnh");
                        }
                    }
                }
                Snapshot(client);
                yield return new WaitForSecondsRealtime(1f);
            }
            Debug.Log("[BrkAuto] DONE");
            Application.Quit();
        }

        private static void Snapshot(BreakerOnlineClient client)
        {
            BreakerPublicState s = client.PublicState;
            StringBuilder sb = new StringBuilder("[BrkAuto] slot=").Append(client.MySlot);
            if (s == null) { Debug.Log(sb.Append(" (chưa có trạng thái)").ToString()); return; }
            sb.Append(" team=").Append(s.PlayerCount).Append(" bật=").Append(s.OnSwitches).Append(" đèn=").Append(s.Lamps).Append(" đích=").Append(s.TargetLamps)
              .Append(" solved=").Append(s.Solved).Append(" khớp=").Append(s.Matching).Append(" trips=").Append(s.Trips).Append(" resets=").Append(s.ResetCount)
              .Append(" msg=").Append(s.LastMessage).Append(" giữ=").Append(s.HoldElapsed.ToString("0.0")).Append("/").Append(s.HoldSeconds.ToString("0.0"));
            int[] own = client.Private.SwitchesCopy();
            sb.Append(" riêng:");
            for (int i = 0; i < own.Length; i++) sb.Append(' ').Append(own[i]);
            Debug.Log(sb.ToString());
        }

        private static string Arg(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
            return null;
        }
    }
}
