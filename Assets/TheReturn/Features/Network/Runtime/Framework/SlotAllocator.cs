/*
 * Mục đích: Chia Ô (nhân vật) CỐ ĐỊNH cho người chơi khi vào phòng online (T-025). Quy tắc chủ dự án đã chốt: mỗi người giữ đúng MỘT
 * nhân vật cả ván, không đổi vai; số nhân vật = số người trong đội (2-4). Người vào trước lấy ô nhỏ nhất còn trống; người rời chỉ
 * trả ô của mình, các ô khác KHÔNG bị dồn lại. Chạy trên host; thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; Assign; Release; SlotOf; TryGetClient; IsFull; AssignedCount; Roster.
 */
using System;

namespace TheReturn.Network.Framework
{
    /// <summary>Bảng ô ↔ người chơi (theo mã client) của một ván.</summary>
    public sealed class SlotAllocator
    {
        readonly TeamRoster roster;
        readonly ulong[] owners;
        readonly bool[] taken;

        /// <summary>Đội đang chơi; ô nào đã được chia thì ở trạng thái đang kết nối.</summary>
        public TeamRoster Roster { get { return roster; } }

        /// <summary>Số ô đã có người.</summary>
        public int AssignedCount { get { return roster.ConnectedCount; } }

        /// <summary>True khi mọi ô đều đã có người.</summary>
        public bool IsFull { get { return roster.ConnectedCount == roster.Size; } }

        /// <summary>Nhận số người của đội (2-4). Ném lỗi nếu ngoài khoảng đó. Ban đầu chưa ô nào có người.</summary>
        public SlotAllocator(int teamSize)
        {
            roster = new TeamRoster(teamSize, false);
            owners = new ulong[teamSize];
            taken = new bool[teamSize];
        }

        /// <summary>
        /// Chia ô cho một người. Nhận mã client. Trả ô được chia (người đã có ô thì trả lại đúng ô cũ), hoặc -1 nếu đội đã đủ người.
        /// </summary>
        public int Assign(ulong clientId)
        {
            int existing = SlotOf(clientId);
            if (existing >= 0) return existing;
            for (int slot = 0; slot < taken.Length; slot++)
            {
                if (taken[slot]) continue;
                taken[slot] = true;
                owners[slot] = clientId;
                roster.SetConnected(slot, true);
                return slot;
            }
            return -1;
        }

        /// <summary>Trả ô của một người khi họ rời. Nhận mã client. Trả true nếu người đó đang giữ ô; các ô khác không đổi.</summary>
        public bool Release(ulong clientId)
        {
            int slot = SlotOf(clientId);
            if (slot < 0) return false;
            taken[slot] = false;
            owners[slot] = 0;
            roster.SetConnected(slot, false);
            return true;
        }

        /// <summary>Nhận mã client; trả ô người đó đang giữ, hoặc -1 nếu chưa có.</summary>
        public int SlotOf(ulong clientId)
        {
            for (int slot = 0; slot < taken.Length; slot++)
            {
                if (taken[slot] && owners[slot] == clientId) return slot;
            }
            return -1;
        }

        /// <summary>Nhận ô; trả true và mã client nếu ô đang có người, ngược lại false.</summary>
        public bool TryGetClient(int slot, out ulong clientId)
        {
            clientId = 0;
            if (slot < 0 || slot >= taken.Length || !taken[slot]) return false;
            clientId = owners[slot];
            return true;
        }
    }
}
