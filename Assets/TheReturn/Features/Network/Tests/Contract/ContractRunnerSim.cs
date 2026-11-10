/*
 * Mục đích: Ca N1 của bộ hợp đồng (T-039, M2): chơi trọn một ván qua MẠNG GIẢ có tiêm lỗi (SimNet) cho đội 2/3/4 x seed,
 * hai hồ sơ «mạng êm» và «mạng xấu» (trễ, đảo, trùng, mất gói). Kiểm: bot vẫn thắng; host không từ chối lệnh hợp lệ (chỉ từ chối lệnh trùng);
 * bản sao không bao giờ lùi Version; gói tới nơi có nội dung ĐÚNG như lúc host phát (trạng thái bất biến, không bị sửa sau khi phát);
 * sau Settle mọi bản sao hội tụ về host. Là phần partial của ContractRunner.
 * Hàm: N1.
 */
using System;
using System.Collections.Generic;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract
{
    public sealed partial class ContractRunner<TCmd, TPub, TPriv>
    {
        void N1(int n, int seed, Action<string> fail)
        {
            for (int profile = 0; profile < 2; profile++)
            {
                SimFaults faults = profile == 0 ? SimFaults.Calm : SimFaults.Rough;
                string tag = profile == 0 ? "mạng êm" : "mạng xấu";
                var r = NewRig(n, seed);
                var net = new SimNet<TCmd, TPub, TPriv>(r.Host, r.Replicas, faults, seed * 31 + n);
                var expected = new Dictionary<string, string>();
                net.OnPublished = delegate(StateMessage<TPub, TPriv> m)
                {
                    expected[m.Slot + "@" + m.Version] = adapter.EncodePublic(m.Public) + " / " + adapter.EncodePrivate(m.Private);
                };
                net.OnDelivered = delegate(int slot, StateMessage<TPub, TPriv> m, bool applied)
                {
                    if (!applied) return;
                    string got = adapter.EncodePublic(r.Replicas[slot].Public) + " / " + adapter.EncodePrivate(r.Replicas[slot].Private);
                    string want;
                    if (expected.TryGetValue(slot + "@" + m.Version, out want) && want != got)
                        fail(tag + ": ô " + slot + " thấy nội dung Version " + m.Version + " khác lúc host phát (trạng thái không bất biến / bị sửa sau khi phát). Phát " + Cut(want) + " nhận " + Cut(got));
                };

                net.HostBroadcast();
                for (int i = 0; i < MaxMoves && !r.Game.IsSolved; i++)
                {
                    int slot;
                    TCmd command;
                    if (!r.Game.TryNextSolveMove(out slot, out command)) break;
                    net.ClientSend(slot, command);
                    for (int w = 0; w < 50 && net.PendingUp > 0; w++) net.Step();
                    net.HostBroadcast();
                    net.Step();
                }
                for (int t = 0; t < MaxIdleTicks && !r.Game.IsSolved; t++)
                {
                    if (r.Game.Tick(IdleTickSeconds)) r.Host.MarkChanged();
                    net.HostBroadcast();
                    net.Step();
                }
                if (!r.Game.IsSolved)
                {
                    fail(tag + ": bot không thắng được qua mạng giả. Từ chối khác: " + (net.FirstOtherReject ?? "không có"));
                    return;
                }

                net.Settle();
                if (net.OtherRejects > 0) fail(tag + ": host từ chối lệnh hợp lệ của bot (không phải trùng): " + net.FirstOtherReject);
                if (net.Regressions > 0) fail(tag + ": bản sao bị LÙI Version " + net.Regressions + " lần.");
                string hostPub = PubText(r);
                for (int i = 0; i < n; i++)
                {
                    var rep = r.Replicas[i];
                    if (!rep.HasState || rep.Version != r.Host.Version) { fail(tag + ": ô " + i + " không hội tụ (Version " + rep.Version + " so với host " + r.Host.Version + ")."); continue; }
                    if (!adapter.IsSolvedPublic(rep.Public)) fail(tag + ": ô " + i + " không thấy báo thắng sau khi mạng ổn định.");
                    else if (adapter.EncodePublic(rep.Public) != hostPub) fail(tag + ": ô " + i + " hội tụ Version nhưng nội dung chung khác host.");
                }
            }
        }
    }
}
