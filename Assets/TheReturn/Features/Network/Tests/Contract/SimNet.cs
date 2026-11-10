/*
 * Mục đích: MẠNG GIẢ trong bộ nhớ có TIÊM LỖI theo seed (T-039, M2). Thay đường mạng thật giữa host và các máy khách để kiểm
 * (không cần build, không cần LAN): lệnh đi lên (uplink) giữ thứ tự từng người nhưng có thể trễ/gửi trùng (như kênh tin cậy
 * có thử lại); trạng thái đi xuống (downlink) có thể trễ, đảo thứ tự, gửi trùng, bị mất. Hết lượt thì Settle(): xả hàng đợi rồi
 * host phát một bản chụp tin cậy cuối cùng. Chạy tất định theo seed (cùng seed = cùng chuỗi lỗi). Thuần C#.
 * Hàm: SimFaults (Calm, Rough, constructor); SimNet (constructor, ClientSend, HostBroadcast, Step, Settle, các bộ đếm, OnPublished, OnDelivered).
 */
using System;
using System.Collections.Generic;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract
{
    /// <summary>Hồ sơ lỗi mạng: trễ tối đa (nhịp), xác suất gửi trùng, xác suất mất gói trạng thái.</summary>
    public sealed class SimFaults
    {
        public readonly int MaxDelayTicks;
        public readonly double DuplicateChance;
        public readonly double DropChance;

        /// <summary>Nhận trễ tối đa, xác suất trùng, xác suất mất.</summary>
        public SimFaults(int maxDelayTicks, double duplicateChance, double dropChance)
        {
            MaxDelayTicks = maxDelayTicks;
            DuplicateChance = duplicateChance;
            DropChance = dropChance;
        }

        /// <summary>Mạng êm: không trễ, không trùng, không mất.</summary>
        public static SimFaults Calm { get { return new SimFaults(0, 0, 0); } }

        /// <summary>Mạng xấu: trễ tới 4 nhịp, 25% trùng, 20% mất gói trạng thái.</summary>
        public static SimFaults Rough { get { return new SimFaults(4, 0.25, 0.2); } }
    }

    /// <summary>Mạng giả nối một HostAuthority với các ClientReplica.</summary>
    public sealed class SimNet<TCmd, TPub, TPriv>
    {
        sealed class UpPacket
        {
            public int Due;
            public long Order;
            public int Slot;
            public int Seq;
            public TCmd Command;
        }

        sealed class DownPacket
        {
            public int Due;
            public long Order;
            public StateMessage<TPub, TPriv> Message;
        }

        readonly HostAuthority<TCmd, TPub, TPriv> host;
        readonly ClientReplica<TPub, TPriv>[] replicas;
        readonly SimFaults faults;
        readonly Random rnd;
        readonly List<UpPacket> up = new List<UpPacket>();
        readonly List<DownPacket> down = new List<DownPacket>();
        readonly int[] lastUpDue;
        long order;
        int clock;

        public int AcceptedCommands { get; private set; }
        public int StaleRejects { get; private set; }
        public int OtherRejects { get; private set; }
        public string FirstOtherReject { get; private set; }
        public int DroppedStates { get; private set; }
        public int DuplicatedPackets { get; private set; }
        public int StatesApplied { get; private set; }
        public int StatesIgnored { get; private set; }
        public int Regressions { get; private set; }

        /// <summary>Số lệnh còn đang bay lên host.</summary>
        public int PendingUp { get { return up.Count; } }

        /// <summary>Số gói trạng thái còn đang bay xuống.</summary>
        public int PendingDown { get { return down.Count; } }

        /// <summary>Gọi ngay lúc host tạo mỗi gói trạng thái (trước khi có lỗi mạng): dùng ghi «nội dung lúc phát».</summary>
        public Action<StateMessage<TPub, TPriv>> OnPublished;

        /// <summary>Gọi sau mỗi lần một gói tới máy khách (slot, gói, có được áp dụng không).</summary>
        public Action<int, StateMessage<TPub, TPriv>, bool> OnDelivered;

        /// <summary>Nhận host, các bản sao, hồ sơ lỗi và seed của dãy lỗi.</summary>
        public SimNet(HostAuthority<TCmd, TPub, TPriv> host, ClientReplica<TPub, TPriv>[] replicas, SimFaults faults, int seed)
        {
            if (host == null) throw new ArgumentNullException("host");
            if (replicas == null) throw new ArgumentNullException("replicas");
            this.host = host;
            this.replicas = replicas;
            this.faults = faults ?? SimFaults.Calm;
            rnd = new Random(seed);
            lastUpDue = new int[replicas.Length];
        }

        /// <summary>Máy khách slot gửi lệnh: lấy số thứ tự mới, xếp vào uplink (giữ thứ tự từng người, có thể trễ/gửi trùng).</summary>
        public void ClientSend(int slot, TCmd command)
        {
            int seq = replicas[slot].NextSequence();
            int due = Math.Max(lastUpDue[slot], clock + Delay());
            lastUpDue[slot] = due;
            up.Add(new UpPacket { Due = due, Order = order++, Slot = slot, Seq = seq, Command = command });
            if (rnd.NextDouble() < faults.DuplicateChance)
            {
                DuplicatedPackets++;
                up.Add(new UpPacket { Due = due + 1 + rnd.Next(3), Order = order++, Slot = slot, Seq = seq, Command = command });
            }
        }

        /// <summary>Host phát bản chụp cho mọi người đang kết nối; mỗi gói có thể trễ/đảo/trùng/mất theo hồ sơ lỗi.</summary>
        public void HostBroadcast()
        {
            foreach (var message in host.BuildBroadcast())
            {
                if (OnPublished != null) OnPublished(message);
                if (rnd.NextDouble() < faults.DropChance) { DroppedStates++; continue; }
                down.Add(new DownPacket { Due = clock + Delay(), Order = order++, Message = message });
                if (rnd.NextDouble() < faults.DuplicateChance)
                {
                    DuplicatedPackets++;
                    down.Add(new DownPacket { Due = clock + Delay(), Order = order++, Message = message });
                }
            }
        }

        /// <summary>Qua một nhịp: đồng hồ mạng tăng 1 rồi giao mọi gói đã tới hạn (theo hạn rồi theo thứ tự xếp).</summary>
        public void Step()
        {
            clock++;
            Deliver();
        }

        /// <summary>
        /// Hết lượt chơi: xả mọi gói còn bay (bỏ qua hạn), rồi host phát MỘT bản chụp tin cậy (không lỗi) tới mọi người.
        /// Sau hàm này các bản sao phải hội tụ về host.
        /// </summary>
        public void Settle()
        {
            int guard = 0;
            while ((up.Count > 0 || down.Count > 0) && guard++ < 10000)
            {
                clock++;
                Deliver();
            }
            foreach (var message in host.BuildBroadcast())
            {
                if (OnPublished != null) OnPublished(message);
                DeliverState(message);
            }
        }

        int Delay()
        {
            return faults.MaxDelayTicks <= 0 ? 0 : rnd.Next(faults.MaxDelayTicks + 1);
        }

        void Deliver()
        {
            var dueUp = up.FindAll(p => p.Due <= clock);
            dueUp.Sort((a, b) => a.Due != b.Due ? a.Due.CompareTo(b.Due) : a.Order.CompareTo(b.Order));
            foreach (var p in dueUp)
            {
                up.Remove(p);
                DeliverCommand(p);
            }
            var dueDown = down.FindAll(p => p.Due <= clock);
            dueDown.Sort((a, b) => a.Due != b.Due ? a.Due.CompareTo(b.Due) : a.Order.CompareTo(b.Order));
            foreach (var p in dueDown)
            {
                down.Remove(p);
                DeliverState(p.Message);
            }
        }

        void DeliverCommand(UpPacket p)
        {
            CommandResult result = host.Submit(p.Slot, p.Seq, p.Command);
            if (result.IsAccepted) { AcceptedCommands++; return; }
            if (result.Reason == RejectReason.StaleSequence) { StaleRejects++; return; }
            OtherRejects++;
            if (FirstOtherReject == null)
                FirstOtherReject = "ô " + p.Slot + " [" + result.Reason + "] " + result.Detail;
        }

        void DeliverState(StateMessage<TPub, TPriv> message)
        {
            var replica = replicas[message.Slot];
            int before = replica.HasState ? replica.Version : -1;
            bool applied = replica.Receive(message);
            if (applied) StatesApplied++; else StatesIgnored++;
            if (replica.HasState && replica.Version < before) Regressions++;
            if (OnDelivered != null) OnDelivered(message.Slot, message, applied);
        }
    }
}
