/*
 * Mục đích: Kế hoạch BÀN GIAO đội từ phòng online sang scene map (T-040 Q-014, đường «phòng -> map» của P-001). Nhận danh sách (mã client, ô)
 * lấy từ phòng, kiểm tính hợp lệ (đủ người, ô duy nhất trong 0..số người-1, mã client duy nhất) và trả danh sách XẾP THEO Ô. Host spawn nhân vật
 * đúng thứ tự này thì SlotAllocator (người vào trước lấy ô nhỏ nhất còn trống) chia ra ô nhân vật = ô trong phòng. Thuần C#, không phụ thuộc Unity.
 * Hàm: HandoffMember (constructor); RoomHandoffPlan.TryBuild; Contains; TryGetSlot; thuộc tính TeamSize, Members.
 */
using System;
using System.Collections.Generic;

namespace TheReturn.Network.Framework
{
    /// <summary>Một người trong đội cần bàn giao: mã client mạng và ô cố định trong phòng.</summary>
    public readonly struct HandoffMember
    {
        /// <summary>Mã client mạng.</summary>
        public ulong ClientId { get; }

        /// <summary>Ô cố định (0..số người-1).</summary>
        public int Slot { get; }

        /// <summary>Nhận mã client và ô.</summary>
        public HandoffMember(ulong clientId, int slot)
        {
            ClientId = clientId;
            Slot = slot;
        }
    }

    /// <summary>Kế hoạch bàn giao đã kiểm: danh sách người xếp theo ô tăng dần.</summary>
    public sealed class RoomHandoffPlan
    {
        readonly List<HandoffMember> members;

        RoomHandoffPlan(int teamSize, List<HandoffMember> ordered)
        {
            TeamSize = teamSize;
            members = ordered;
        }

        /// <summary>Số người của đội (2-4).</summary>
        public int TeamSize { get; }

        /// <summary>Những người cần bàn giao, theo ô tăng dần (chỉ đọc).</summary>
        public IReadOnlyList<HandoffMember> Members { get { return members; } }

        /// <summary>
        /// Nhận các dòng phòng, số người của đội (2-4) và cờ cho phép THIẾU người (chỉ dùng khi kiểm thử một máy; chơi thật phải đủ).
        /// Trả true và kế hoạch nếu hợp lệ; ngược lại false, plan = null, error là lý do tiếng Việt. Không bao giờ ném lỗi.
        /// </summary>
        public static bool TryBuild(IEnumerable<HandoffMember> rows, int teamSize, bool allowPartial, out RoomHandoffPlan plan, out string error)
        {
            plan = null;
            error = null;
            if (teamSize < 2 || teamSize > 4) { error = "Số người của đội phải từ 2 đến 4 (nhận " + teamSize + ")."; return false; }
            if (rows == null) { error = "Phòng chưa có danh sách người chơi."; return false; }
            List<HandoffMember> list = new List<HandoffMember>();
            HashSet<int> slots = new HashSet<int>();
            HashSet<ulong> clients = new HashSet<ulong>();
            foreach (HandoffMember m in rows)
            {
                if (m.Slot < 0 || m.Slot >= teamSize) { error = "Ô " + m.Slot + " nằm ngoài đội " + teamSize + " người."; return false; }
                if (!slots.Add(m.Slot)) { error = "Hai người cùng giữ ô " + m.Slot + "."; return false; }
                if (!clients.Add(m.ClientId)) { error = "Mã client " + m.ClientId + " xuất hiện hai lần."; return false; }
                list.Add(m);
            }
            if (list.Count == 0) { error = "Phòng không có ai để bàn giao."; return false; }
            if (!allowPartial && list.Count != teamSize) { error = "Phòng mới có " + list.Count + "/" + teamSize + " người, chưa đủ để vào map."; return false; }
            list.Sort(delegate (HandoffMember a, HandoffMember b) { return a.Slot.CompareTo(b.Slot); });
            plan = new RoomHandoffPlan(teamSize, list);
            return true;
        }

        /// <summary>Nhận mã client; trả true nếu người đó nằm trong kế hoạch.</summary>
        public bool Contains(ulong clientId)
        {
            int slot;
            return TryGetSlot(clientId, out slot);
        }

        /// <summary>Nhận mã client; trả true và ô nếu có trong kế hoạch, ngược lại false (slot = -1).</summary>
        public bool TryGetSlot(ulong clientId, out int slot)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].ClientId == clientId) { slot = members[i].Slot; return true; }
            }
            slot = -1;
            return false;
        }
    }
}
