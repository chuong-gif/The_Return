/*
 * Mục đích: Luật của một nhóm chơi (T-008): tối đa 4 người, tối thiểu 2, chủ phòng, sẵn sàng, bắt đầu. Đây là nguồn sự thật duy nhất;
 * giao diện chỉ đọc PartySnapshot, và khi có mạng thì host sẽ giữ PartyState này rồi gửi ảnh chụp cho các máy khác. Thuần C#.
 * Luật: người vào đầu tiên là chủ phòng; vị trí là chỗ trống nhỏ nhất 0-3; chủ phòng rời thì người vào sớm nhất lên làm chủ phòng;
 * phòng rỗng thì đóng; chỉ chủ phòng bắt đầu được và chỉ khi từ 2 người và mọi người đã sẵn sàng; bắt đầu rồi không ai vào thêm.
 * Hàm: constructor; Join; Leave; SetReady; CanStart; TryStart; Snapshot; IsMember; HostId; Count; Closed.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Trạng thái và luật của một nhóm chơi 2-4 người.</summary>
    public sealed class PartyState
    {
        public const int MaxPlayers = 4;
        public const int MinPlayers = 2;
        public const int MaxNameLength = 16;

        sealed class Member
        {
            public string Id;
            public string Name;
            public int Slot;
            public int Order;
            public bool Ready;
        }

        readonly List<Member> members = new List<Member>();
        int nextOrder;

        public string Code { get; }
        public bool Started { get; private set; }
        public string HostId { get; private set; }
        public int Count { get { return members.Count; } }
        public bool Closed { get { return members.Count == 0 && nextOrder > 0; } }

        /// <summary>Nhận mã phòng hợp lệ (4 ký tự trong bảng); ném ArgumentException nếu mã sai.</summary>
        public PartyState(string code)
        {
            if (!PartyCode.IsValid(code)) throw new ArgumentException("Mã phòng không hợp lệ", nameof(code));
            Code = code;
        }

        /// <summary>Nhận mã người (không rỗng); trả true nếu người đó đang ở trong nhóm.</summary>
        public bool IsMember(string id) { return Find(id) != null; }

        /// <summary>
        /// Nhận mã người và tên hiển thị; thêm vào vị trí trống nhỏ nhất. Trả Started nếu đã bắt đầu, AlreadyInParty nếu đã có,
        /// InvalidName nếu tên rỗng, Full nếu đủ 4 người, Ok nếu vào được (người đầu tiên thành chủ phòng).
        /// Ném ArgumentException nếu mã người rỗng.
        /// </summary>
        public PartyResult Join(string id, string name)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Thiếu mã người", nameof(id));
            if (Started) return PartyResult.Started;
            if (Find(id) != null) return PartyResult.AlreadyInParty;
            string clean = CleanName(name);
            if (clean == null) return PartyResult.InvalidName;
            if (members.Count >= MaxPlayers) return PartyResult.Full;
            members.Add(new Member { Id = id, Name = clean, Slot = LowestFreeSlot(), Order = nextOrder++ });
            if (members.Count == 1) HostId = id;
            return PartyResult.Ok;
        }

        /// <summary>Nhận mã người; gỡ khỏi nhóm. Chủ phòng rời thì người vào sớm nhất lên làm chủ phòng. NoParty nếu không ở trong nhóm.</summary>
        public PartyResult Leave(string id)
        {
            Member m = Find(id);
            if (m == null) return PartyResult.NoParty;
            members.Remove(m);
            if (members.Count == 0) { HostId = null; return PartyResult.Ok; }
            if (HostId == id)
            {
                Member next = members[0];
                foreach (Member x in members) if (x.Order < next.Order) next = x;
                HostId = next.Id;
            }
            return PartyResult.Ok;
        }

        /// <summary>Nhận mã người và trạng thái sẵn sàng; trả NoParty nếu không ở trong nhóm, Started nếu đã bắt đầu, Ok nếu đổi được.</summary>
        public PartyResult SetReady(string id, bool ready)
        {
            Member m = Find(id);
            if (m == null) return PartyResult.NoParty;
            if (Started) return PartyResult.Started;
            m.Ready = ready;
            return PartyResult.Ok;
        }

        /// <summary>True khi chưa bắt đầu, có từ 2 người và mọi người đã sẵn sàng.</summary>
        public bool CanStart
        {
            get
            {
                if (Started || members.Count < MinPlayers) return false;
                foreach (Member m in members) if (!m.Ready) return false;
                return true;
            }
        }

        /// <summary>
        /// Nhận mã người yêu cầu bắt đầu. Kiểm lần lượt: không ở trong nhóm (NoParty), đã bắt đầu (Started), không phải chủ phòng (NotHost),
        /// thiếu người (NotEnoughPlayers), chưa ai cũng sẵn sàng (NotAllReady); đủ thì đánh dấu đã bắt đầu và trả Ok.
        /// </summary>
        public PartyResult TryStart(string id)
        {
            if (Find(id) == null) return PartyResult.NoParty;
            if (Started) return PartyResult.Started;
            if (id != HostId) return PartyResult.NotHost;
            if (members.Count < MinPlayers) return PartyResult.NotEnoughPlayers;
            if (!CanStart) return PartyResult.NotAllReady;
            Started = true;
            return PartyResult.Ok;
        }

        /// <summary>Nhận mã người của máy này (có thể null); trả ảnh chụp bất biến, thành viên sắp theo vị trí 0-3.</summary>
        public PartySnapshot Snapshot(string localId)
        {
            var sorted = new List<Member>(members);
            sorted.Sort((a, b) => a.Slot.CompareTo(b.Slot));
            var infos = new PartyMemberInfo[sorted.Count];
            for (int i = 0; i < infos.Length; i++)
            {
                Member m = sorted[i];
                infos[i] = new PartyMemberInfo(m.Id, m.Name, m.Slot, m.Id == HostId, m.Ready, localId != null && m.Id == localId);
            }
            return new PartySnapshot(Code, infos, Started, CanStart);
        }

        /// <summary>Nhận mã người; trả thành viên hoặc null.</summary>
        Member Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (Member m in members) if (m.Id == id) return m;
            return null;
        }

        /// <summary>Không nhận tham số; trả vị trí 0-3 trống nhỏ nhất.</summary>
        int LowestFreeSlot()
        {
            for (int slot = 0; slot < MaxPlayers; slot++)
            {
                bool taken = false;
                foreach (Member m in members) if (m.Slot == slot) { taken = true; break; }
                if (!taken) return slot;
            }
            return MaxPlayers - 1;
        }

        /// <summary>Nhận tên thô; trả tên đã cắt khoảng trắng và giới hạn 16 ký tự, hoặc null nếu rỗng.</summary>
        static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string t = name.Trim();
            return t.Length > MaxNameLength ? t.Substring(0, MaxNameLength) : t;
        }
    }
}
