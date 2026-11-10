/*
 * Mục đích: Lệnh người chơi gửi lên HOST trong «Đèn lồng chung» khi chơi online (T-037). Lệnh KHÔNG mang ô người gửi: host lấy ô từ kết nối
 * (HostAuthority.Submit) nên không ai giả mạo được người khác. Bất biến; thuần C#, không phụ thuộc Unity.
 * Hàm: Pass; Read; SetEntry; ClearEntries; Submit; Retry (các hàm tạo lệnh).
 */
namespace TheReturn.LanternNet
{
    /// <summary>Loại lệnh của nhiệm vụ Đèn lồng chung.</summary>
    public enum SharedLanternCommandKind
    {
        /// <summary>Trao đèn lồng cho một người (chỉ người đang cầm, người nhận phải đứng trong tầm).</summary>
        Pass,
        /// <summary>Đọc một biển (phải đứng gần biển và biển nằm trong vùng sáng); ký hiệu vào sổ riêng của người đọc.</summary>
        Read,
        /// <summary>Đặt một ký hiệu vào một ô của bảng nhập cuối hành lang (phải đứng gần bảng).</summary>
        SetEntry,
        /// <summary>Xóa cả 4 ô bảng nhập (phải đứng gần bảng).</summary>
        ClearEntries,
        /// <summary>Nộp bảng nhập (đủ 4 ô, không đang bị khóa); sai thì bị phạt khóa bảng.</summary>
        Submit,
        /// <summary>Làm lại cùng đề.</summary>
        Retry
    }

    /// <summary>Một lệnh của người chơi.</summary>
    public sealed class SharedLanternCommand
    {
        /// <summary>Loại lệnh.</summary>
        public SharedLanternCommandKind Kind { get; }

        /// <summary>Pass: ô người nhận đèn; Read: chỉ số biển 0-3; SetEntry: chỉ số ô nhập 0-3; các lệnh khác: 0.</summary>
        public int Index { get; }

        /// <summary>Chỉ có nghĩa với SetEntry: ký hiệu 0-5; các lệnh khác: 0.</summary>
        public int Symbol { get; }

        SharedLanternCommand(SharedLanternCommandKind kind, int index, int symbol)
        {
            Kind = kind;
            Index = index;
            Symbol = symbol;
        }

        /// <summary>Nhận ô người nhận; trả lệnh trao đèn (host kiểm người gửi đang cầm đèn và người nhận đứng trong tầm).</summary>
        public static SharedLanternCommand Pass(int to)
        {
            return new SharedLanternCommand(SharedLanternCommandKind.Pass, to, 0);
        }

        /// <summary>Nhận chỉ số biển; trả lệnh đọc biển (host kiểm đứng gần biển và biển trong vùng sáng).</summary>
        public static SharedLanternCommand Read(int sign)
        {
            return new SharedLanternCommand(SharedLanternCommandKind.Read, sign, 0);
        }

        /// <summary>Nhận ô nhập và ký hiệu; trả lệnh đặt ký hiệu vào ô (host kiểm đứng gần bảng và giá trị trong khoảng).</summary>
        public static SharedLanternCommand SetEntry(int slot, int symbol)
        {
            return new SharedLanternCommand(SharedLanternCommandKind.SetEntry, slot, symbol);
        }

        /// <summary>Trả lệnh xóa bảng nhập.</summary>
        public static SharedLanternCommand ClearEntries()
        {
            return new SharedLanternCommand(SharedLanternCommandKind.ClearEntries, 0, 0);
        }

        /// <summary>Trả lệnh nộp bảng nhập.</summary>
        public static SharedLanternCommand Submit()
        {
            return new SharedLanternCommand(SharedLanternCommandKind.Submit, 0, 0);
        }

        /// <summary>Trả lệnh làm lại cùng đề.</summary>
        public static SharedLanternCommand Retry()
        {
            return new SharedLanternCommand(SharedLanternCommandKind.Retry, 0, 0);
        }
    }
}
