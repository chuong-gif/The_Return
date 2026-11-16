/*
 * Mục đích: TÊN NGƯỜI CHƠI theo ô khi chơi online (T-055). Tên người chơi nhập ở panel «Chơi online» đi vào danh sách phòng (RoomRoster); khi chủ phòng bắt đầu,
 * RoomMapHandoff ghi tên theo ô vào đây trên MỌI máy. Nhiệm vụ Điểm danh (host), HUD «VAI n • tên» và biển tên trên đầu đồng đội đọc tên từ đây thay cho bốn tên mặc định Minh/An/Bình/Chi.
 * Chơi offline không có tên đồng bộ thì mọi nơi vẫn dùng tên mặc định cũ.
 * Hàm: SetSynced; Clear; OfSlot; Resolve.
 */
using TheReturn;

namespace TheReturn.Network.Online
{
    /// <summary>Tên người chơi theo ô (0..3) trong ván online đang chạy.</summary>
    public static class OnlineNames
    {
        private static string[] synced = new string[0];

        /// <summary>Các tên đã đồng bộ theo ô (phần tử trống = ô chưa có người).</summary>
        public static string[] Synced { get { return synced; } }

        /// <summary>Nhận mảng tên theo ô; lưu lại và đăng ký làm nguồn tên cho PrototypePartyController.SlotName.</summary>
        public static void SetSynced(string[] names)
        {
            synced = names ?? new string[0];
            PrototypePartyController.SlotNameProvider = OfSlot;
        }

        /// <summary>Không nhận tham số; xóa tên đã đồng bộ (hết ván / rời phòng) và gỡ nguồn tên khỏi PrototypePartyController.</summary>
        public static void Clear()
        {
            synced = new string[0];
            PrototypePartyController.SlotNameProvider = null;
        }

        /// <summary>Nhận chỉ số ô; trả tên người ở ô đó, hoặc null nếu chưa có.</summary>
        public static string OfSlot(int slot)
        {
            if (slot < 0 || slot >= synced.Length || string.IsNullOrWhiteSpace(synced[slot])) return null;
            return synced[slot].Trim();
        }

        /// <summary>Nhận tên đã đồng bộ theo ô, tên mặc định theo ô và số người của đội; trả đúng teamSize tên: ưu tiên tên người chơi, thiếu thì lấy tên mặc định, trùng nhau (không phân biệt hoa thường) thì thêm « (2)», « (3)»...</summary>
        public static string[] Resolve(string[] syncedNames, string[] fallback, int teamSize)
        {
            string[] result = new string[teamSize];
            for (int i = 0; i < teamSize; i++)
            {
                string name = null;
                if (syncedNames != null && i < syncedNames.Length && !string.IsNullOrWhiteSpace(syncedNames[i])) name = syncedNames[i].Trim();
                else if (fallback != null && i < fallback.Length && !string.IsNullOrWhiteSpace(fallback[i])) name = fallback[i].Trim();
                else name = "Người chơi " + (i + 1);
                string candidate = name;
                int n = 2;
                while (Taken(result, i, candidate)) { candidate = name + " (" + n + ")"; n++; }
                result[i] = candidate;
            }
            return result;
        }

        private static bool Taken(string[] result, int upTo, string candidate)
        {
            for (int j = 0; j < upTo; j++) if (string.Equals(result[j], candidate, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
