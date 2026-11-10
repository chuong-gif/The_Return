/*
 * Mục đích: Chạy kiểm thử «Hợp xướng» online TỰ ĐỘNG qua tham số dòng lệnh (T-037 GĐ2), chỉ kích hoạt khi có -choirauto. Mỗi tiến trình (host hay khách)
 * điều khiển người chơi của chính nó: với mỗi nốt của bản nhạc, nếu thẻ của MÌNH chứa ký hiệu đó thì đưa nhân vật tới bục tương ứng và gửi lệnh bấm
 * qua mạng thật; ghi log '[ChoirAuto]' mỗi giây để đối chiếu giữa các tiến trình.
 * Tham số: -choirauto win|far|late  -choirteam N (host: số người đội)  -choirdelay giây  -choirquit giây (mặc định 45)
 *   win: bấm đúng thứ tự (ai giữ thẻ nốt nào thì bấm nốt đó)   far: KHÔNG đưa nhân vật tới bục, thử bấm bục đầu để host từ chối vì đứng xa
 *   late: chờ quá cửa sổ giữa hai nốt rồi mới bấm tiếp, để thấy host reset dãy.
 * Hàm: Start (coroutine); Snapshot; Arg.
 */
using System;
using System.Collections;
using System.Text;
using TheReturn.Network.Online;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.ChoirNet.Online
{
    public sealed class ChoirOnlineAutoRun : MonoBehaviour
    {
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            string mode = (Arg(args, "-choirauto") ?? "").ToLowerInvariant();
            if (mode.Length == 0) yield break;
            int team = 0; int.TryParse(Arg(args, "-choirteam") ?? "0", out team);
            float delay = 3f; float.TryParse(Arg(args, "-choirdelay") ?? "3", out delay);
            float quit = 45f; float.TryParse(Arg(args, "-choirquit") ?? "45", out quit); if (quit <= 0f) quit = 45f;
            Debug.Log("[ChoirAuto] START mode=" + mode + " team=" + team + " delay=" + delay + " quit=" + quit);

            ChoirOnlineClient client = FindFirstObjectByType<ChoirOnlineClient>();
            if (client == null) { Debug.LogError("[ChoirAuto] Không thấy ChoirOnlineClient"); yield break; }

            float end = Time.realtimeSinceStartup + quit;
            if (team >= 2) OnlinePartySlots.Instance.SetTeamSize(team);
            float readyAt = -1f;
            int done = 0;
            while (Time.realtimeSinceStartup < end)
            {
                ChoirPublicState s = client.PublicState;
                if (s != null)
                {
                    if (readyAt < 0f) readyAt = Time.realtimeSinceStartup;
                    if (Time.realtimeSinceStartup - readyAt >= delay && !s.Solved && done < 64)
                    {
                        int next = s.Progress;
                        int symbol = next < s.NoteCount ? s.SymbolAt(next) : -1;
                        int pad;
                        if (mode == "far")
                        {
                            if (done == 0) { done++; Debug.Log("[ChoirAuto] Ô " + client.MySlot + " gửi lệnh bấm bục 0 (đứng xa)"); client.RequestPress(0); }
                        }
                        else if (symbol >= 0 && client.Private.TryGetPad(symbol, out pad))
                        {
                            done++;
                            client.PlaceNearPad(pad);
                            yield return new WaitForSecondsRealtime(0.5f);
                            if (mode == "late") yield return new WaitForSecondsRealtime((float)s.WindowSeconds + 1f);
                            Debug.Log("[ChoirAuto] Ô " + client.MySlot + " bấm bục " + pad + " cho nốt " + next + " (ký hiệu " + symbol + ")");
                            client.RequestPress(pad);
                        }
                    }
                }
                Snapshot(client);
                yield return new WaitForSecondsRealtime(1f);
            }
            Debug.Log("[ChoirAuto] DONE");
            Application.Quit();
        }

        private static void Snapshot(ChoirOnlineClient client)
        {
            ChoirPublicState s = client.PublicState;
            StringBuilder sb = new StringBuilder("[ChoirAuto] slot=").Append(client.MySlot);
            if (s == null) { Debug.Log(sb.Append(" (chưa có trạng thái)").ToString()); return; }
            sb.Append(" team=").Append(s.PlayerCount).Append(" tiến=").Append(s.Progress).Append('/').Append(s.NoteCount)
              .Append(" sai=").Append(s.Mistakes).Append(" quáGiờ=").Append(s.Timeouts).Append(" solved=").Append(s.Solved)
              .Append(" resets=").Append(s.ResetCount).Append(" msg=").Append(s.LastMessage).Append(" cửaSổ=").Append(s.WindowSeconds.ToString("0.0"));
            int[] own = client.Private.SymbolsCopy();
            sb.Append(" thẻ:");
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
