/*
 * Mục đích: Luật PHÒNG online (T-026), thuần C#, test được không cần Unity/mạng. Chạy trên HOST. Số người của đội (2-4) chốt lúc tạo phòng
 * và không đổi (quyết định chủ dự án 2026-10-29). Mỗi người vào nhận một ô cố định (dùng lại SlotAllocator của T-025); chỉ chủ phòng bấm
 * Bắt đầu được và chỉ khi phòng đã đủ người; bắt đầu rồi thì không nhận thêm người.
 * Hàm: constructor; Join; Leave; CanAdmit; CanStart; TryStart; Rows; thuộc tính TeamSize, Count, Started; SanitizeName.
 */
using System;
using System.Collections.Generic;
using System.Text;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Room
{
    /// <summary>Một dòng trong danh sách phòng: ô, người chơi, tên, có phải chủ phòng.</summary>
    public struct RoomRow
    {
        public ulong ClientId;
        public int Slot;
        public string Name;
        public bool IsHost;
    }

    /// <summary>Trạng thái phòng của một đội cố định 2-4 người.</summary>
    public sealed class RoomState
    {
        public const int MaxNameLength = 20;

        readonly SlotAllocator slots;
        readonly Dictionary<ulong, string> names = new Dictionary<ulong, string>();
        readonly ulong hostClientId;

        /// <summary>Số người của đội (2-4), cố định.</summary>
        public int TeamSize { get { return slots.Roster.Size; } }

        /// <summary>Số người đang ở trong phòng.</summary>
        public int Count { get { return slots.AssignedCount; } }

        /// <summary>True sau khi chủ phòng đã bắt đầu ván.</summary>
        public bool Started { get; private set; }

        /// <summary>Nhận số người (2-4) và mã client của chủ phòng (mặc định 0). Ném lỗi nếu số người ngoài 2-4.</summary>
        public RoomState(int teamSize, ulong hostClientId = 0)
        {
            slots = new SlotAllocator(teamSize);
            this.hostClientId = hostClientId;
        }

        /// <summary>
        /// Nhận một người vào phòng. Nhận mã client và tên (làm sạch, rỗng thì đặt tên mặc định). Trả ô được chia,
        /// hoặc -1 nếu phòng đã đủ hoặc ván đã bắt đầu. Người đã có ô thì trả lại ô cũ và cập nhật tên.
        /// </summary>
        public int Join(ulong clientId, string name)
        {
            int existing = slots.SlotOf(clientId);
            if (existing < 0 && Started) return -1;
            int slot = slots.Assign(clientId);
            if (slot < 0) return -1;
            names[clientId] = UniqueName(clientId, SanitizeName(name, slot));
            return slot;
        }

        /// <summary>Một người rời phòng. Nhận mã client. Trả true nếu người đó có trong phòng; ô của họ được trả, các ô khác không đổi.</summary>
        public bool Leave(ulong clientId)
        {
            if (!slots.Release(clientId)) return false;
            names.Remove(clientId);
            return true;
        }

        /// <summary>Nhận số kết nối hiện có (kể cả chủ phòng). Trả true nếu còn chỗ và ván chưa bắt đầu (dùng cho duyệt kết nối).</summary>
        public bool CanAdmit(int currentConnected)
        {
            return !Started && currentConnected < TeamSize;
        }

        /// <summary>True khi phòng đủ người và chưa bắt đầu.</summary>
        public bool CanStart { get { return !Started && slots.IsFull; } }

        /// <summary>Nhận mã client người bấm Bắt đầu. Trả true nếu là chủ phòng và phòng đủ người; khi đó ván được đánh dấu đã bắt đầu.</summary>
        public bool TryStart(ulong requesterClientId)
        {
            if (requesterClientId != hostClientId || !CanStart) return false;
            Started = true;
            return true;
        }

        /// <summary>Trả danh sách người trong phòng, sắp theo ô tăng dần.</summary>
        public List<RoomRow> Rows()
        {
            List<RoomRow> rows = new List<RoomRow>();
            for (int slot = 0; slot < TeamSize; slot++)
            {
                ulong id;
                if (!slots.TryGetClient(slot, out id)) continue;
                rows.Add(new RoomRow { ClientId = id, Slot = slot, Name = names[id], IsHost = id == hostClientId });
            }
            return rows;
        }

        /// <summary>Làm sạch tên: bỏ ký tự điều khiển, cắt khoảng trắng, tối đa 20 ký tự; rỗng thì đặt 'Người chơi N' theo ô.</summary>
        public static string SanitizeName(string name, int slot)
        {
            StringBuilder sb = new StringBuilder();
            if (name != null)
            {
                foreach (char ch in name)
                {
                    if (char.IsControl(ch)) continue;
                    sb.Append(ch);
                }
            }
            string clean = sb.ToString().Trim();
            if (clean.Length > MaxNameLength) clean = clean.Substring(0, MaxNameLength).Trim();
            if (clean.Length == 0) clean = "Người chơi " + (slot + 1);
            return clean;
        }

        string UniqueName(ulong clientId, string candidate)
        {
            string result = candidate;
            int suffix = 2;
            while (IsNameTaken(clientId, result))
            {
                result = candidate + " (" + suffix + ")";
                suffix++;
            }
            return result;
        }

        bool IsNameTaken(ulong exceptClientId, string name)
        {
            foreach (KeyValuePair<ulong, string> pair in names)
            {
                if (pair.Key == exceptClientId) continue;
                if (string.Equals(pair.Value, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
