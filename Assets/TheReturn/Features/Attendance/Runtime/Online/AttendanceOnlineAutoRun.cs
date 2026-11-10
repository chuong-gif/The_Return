/*
 * Mục đích: Chạy kiểm thử «Điểm danh» online TỰ ĐỘNG qua tham số dòng lệnh (T-031), chỉ kích hoạt khi có -attauto. Mỗi tiến trình (host hay khách)
 * điều khiển người chơi của chính nó: đi tới một ghế rồi gửi lệnh ngồi qua mạng thật, và ghi log '[AttAuto]' mỗi giây (ai ngồi ghế nào, đúng/sai,
 * độ dài manh mối riêng) để đối chiếu giữa các tiến trình.
 * Tham số: -attauto sit|solve  -attseat N (chế độ sit: ghế muốn ngồi)  -attteam N (host: số người đội)  -attsolfile ĐƯỜNG_DẪN (host ghi đáp án, khách đọc)
 *   -attdelay giây (chờ trước khi ngồi)  -attquit giây (tự thoát, mặc định 45)  -attstand giây (sau từng ấy giây kể từ khi ngồi thì đứng dậy, 0 = không)
 * Hàm: Start (coroutine); Snapshot; Arg.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.AttendanceNet.Online
{
    public sealed class AttendanceOnlineAutoRun : MonoBehaviour
    {
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            string mode = (Arg(args, "-attauto") ?? "").ToLowerInvariant();
            if (mode.Length == 0) yield break;
            int seatArg = -1; int.TryParse(Arg(args, "-attseat") ?? "-1", out seatArg);
            int team = 0; int.TryParse(Arg(args, "-attteam") ?? "0", out team);
            string solFile = Arg(args, "-attsolfile") ?? "";
            float delay = 3f; float.TryParse(Arg(args, "-attdelay") ?? "3", out delay);
            float quit = 45f; float.TryParse(Arg(args, "-attquit") ?? "45", out quit); if (quit <= 0f) quit = 45f;
            float standAfter = 0f; float.TryParse(Arg(args, "-attstand") ?? "0", out standAfter);
            Debug.Log("[AttAuto] START mode=" + mode + " seat=" + seatArg + " team=" + team + " solFile=" + solFile + " delay=" + delay + " quit=" + quit);

            AttendanceOnlineHost host = FindFirstObjectByType<AttendanceOnlineHost>();
            AttendanceOnlineClient client = FindFirstObjectByType<AttendanceOnlineClient>();
            if (client == null) { Debug.LogError("[AttAuto] Không thấy AttendanceOnlineClient"); yield break; }

            float end = Time.realtimeSinceStartup + quit;
            if (team >= 2) TheReturn.Network.Online.OnlinePartySlots.Instance.SetTeamSize(team);
            bool solutionWritten = false, sat = false, stood = false;
            float readyAt = -1f, satAt = -1f;
            while (Time.realtimeSinceStartup < end)
            {
                NetworkManager nm = NetworkManager.Singleton;
                bool hosting = nm != null && nm.IsListening && nm.IsServer;
                if (hosting && host != null && solFile.Length > 0 && !solutionWritten && host.SolutionSeatsForTest != null)
                {
                    solutionWritten = true;
                    File.WriteAllText(solFile, string.Join(",", Array.ConvertAll(host.SolutionSeatsForTest, x => x.ToString())));
                    Debug.Log("[AttAuto] Host đã ghi đáp án ra file: " + string.Join(",", Array.ConvertAll(host.SolutionSeatsForTest, x => x.ToString())));
                }
                if (client.HasState && !sat)
                {
                    if (readyAt < 0f) readyAt = Time.realtimeSinceStartup;
                    if (Time.realtimeSinceStartup - readyAt >= delay)
                    {
                        int seat = seatArg;
                        if (mode == "solve")
                        {
                            seat = -1;
                            try
                            {
                                if (solFile.Length > 0 && File.Exists(solFile))
                                {
                                    string[] parts = File.ReadAllText(solFile).Trim().Split(',');
                                    int v;
                                    if (client.MySlot >= 0 && client.MySlot < parts.Length && int.TryParse(parts[client.MySlot], out v)) seat = v;
                                }
                            }
                            catch (IOException) { }
                        }
                        if (seat >= 0)
                        {
                            sat = true;
                            client.PlaceNearSeat(seat);
                            yield return new WaitForSecondsRealtime(1f);
                            Debug.Log("[AttAuto] Ô " + client.MySlot + " gửi lệnh ngồi ghế " + seat);
                            client.RequestSit(seat);
                            satAt = Time.realtimeSinceStartup;
                        }
                    }
                }
                if (sat && !stood && standAfter > 0f && Time.realtimeSinceStartup - satAt >= standAfter)
                {
                    stood = true;
                    Debug.Log("[AttAuto] Ô " + client.MySlot + " gửi lệnh đứng dậy");
                    client.RequestStand();
                }
                Snapshot(client);
                yield return new WaitForSecondsRealtime(1f);
            }
            Debug.Log("[AttAuto] DONE");
            Application.Quit();
        }

        private static void Snapshot(AttendanceOnlineClient client)
        {
            AttendancePublicState s = client.PublicState;
            StringBuilder sb = new StringBuilder("[AttAuto] slot=").Append(client.MySlot).Append(" mySeat=").Append(client.MySeat);
            if (s == null) { Debug.Log(sb.Append(" (chưa có trạng thái)").ToString()); return; }
            sb.Append(" team=").Append(s.PlayerCount).Append(" ngồi=").Append(s.OccupiedCount()).Append(" solved=").Append(s.Solved).Append(" wrong=").Append(s.Wrong)
              .Append(" settling=").Append(s.Settling).Append(" msg=").Append(s.LastMessage).Append(" resets=").Append(s.ResetCount).Append(" clueLen=").Append(client.Clues.Length);
            int[] seats = s.SeatsCopy();
            sb.Append(" ghế:");
            for (int i = 0; i < seats.Length; i++) if (seats[i] >= 0) sb.Append(" ").Append(i).Append("=ô").Append(seats[i]);
            Debug.Log(sb.ToString());
        }

        private static string Arg(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
            return null;
        }
    }
}
