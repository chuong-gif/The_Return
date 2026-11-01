/*
 * Mục đích: Chuyển lệnh và trạng thái của «Điểm danh» thành CHUỖI ngắn và ngược lại (T-031), để cầu nối mạng chỉ việc gửi chuỗi.
 * Chuỗi tự do (tên vai, manh mối có xuống dòng và tiếng Việt) được thoát bằng Uri.EscapeDataString nên không phá dấu phân cách.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, hai ghế cùng một người đều trả false
 * chứ không ném lỗi, để máy khách xấu không làm sập host. Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.AttendanceNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ điểm danh.</summary>
    public static class AttendanceCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const char ListSep = ',';
        const string PrivatePrefix = "c:";
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|ghế».</summary>
        public static string EncodeCommand(AttendanceCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + command.Seat.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ, ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out AttendanceCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text) || text.Length > 32) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 2) return false;
            int kind, seat;
            if (!int.TryParse(parts[0], NumberStyles.Integer, Inv, out kind)) return false;
            if (!int.TryParse(parts[1], NumberStyles.Integer, Inv, out seat)) return false;
            if (!Enum.IsDefined(typeof(AttendanceCommandKind), kind)) return false;
            switch ((AttendanceCommandKind)kind)
            {
                case AttendanceCommandKind.Sit:
                    if (seat < 0 || seat >= AttendancePublicState.SeatCount) return false;
                    command = AttendanceCommand.Sit(seat);
                    return true;
                case AttendanceCommandKind.Stand:
                    if (seat != 0) return false;
                    command = AttendanceCommand.Stand();
                    return true;
                case AttendanceCommandKind.Retry:
                    if (seat != 0) return false;
                    command = AttendanceCommand.Retry();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 8 trường cách nhau bởi |.</summary>
        public static string EncodePublic(AttendancePublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            string[] names = s.NamesCopy();
            for (int i = 0; i < names.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(Uri.EscapeDataString(names[i] ?? string.Empty));
            }
            sb.Append(Sep);
            int[] seats = s.SeatsCopy();
            for (int i = 0; i < seats.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(seats[i].ToString(Inv));
            }
            sb.Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(s.Wrong ? "1" : "0").Append(Sep);
            sb.Append(s.Settling ? "1" : "0").Append(Sep);
            sb.Append(s.ResetCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.LastMessage).ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out AttendancePublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != 8) return false;
            int playerCount;
            if (!int.TryParse(f[0], NumberStyles.Integer, Inv, out playerCount)) return false;
            if (playerCount < 2 || playerCount > 4) return false;
            string[] rawNames = f[1].Split(ListSep);
            if (rawNames.Length != playerCount) return false;
            string[] names = new string[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                string name = Uri.UnescapeDataString(rawNames[i]);
                if (name.Length == 0 || name.Length > 40) return false;
                names[i] = name;
            }
            string[] rawSeats = f[2].Split(ListSep);
            if (rawSeats.Length != AttendancePublicState.SeatCount) return false;
            int[] seats = new int[rawSeats.Length];
            bool[] seen = new bool[playerCount];
            for (int i = 0; i < rawSeats.Length; i++)
            {
                int who;
                if (!int.TryParse(rawSeats[i], NumberStyles.Integer, Inv, out who)) return false;
                if (who < -1 || who >= playerCount) return false;
                if (who >= 0)
                {
                    if (seen[who]) return false;
                    seen[who] = true;
                }
                seats[i] = who;
            }
            bool solved, wrong, settling;
            if (!TryBool(f[3], out solved) || !TryBool(f[4], out wrong) || !TryBool(f[5], out settling)) return false;
            int resetCount, message;
            if (!int.TryParse(f[6], NumberStyles.Integer, Inv, out resetCount) || resetCount < 0 || resetCount > 1000000) return false;
            if (!int.TryParse(f[7], NumberStyles.Integer, Inv, out message) || !Enum.IsDefined(typeof(AttendanceMessage), message)) return false;
            state = new AttendancePublicState(playerCount, names, seats, solved, wrong, settling, resetCount, (AttendanceMessage)message);
            return true;
        }

        /// <summary>Nhận thông tin riêng; trả chuỗi «c:» + gói manh mối đã thoát ký tự.</summary>
        public static string EncodePrivate(AttendancePrivateState state)
        {
            return PrivatePrefix + Uri.EscapeDataString(state.Clues ?? string.Empty);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ, ngược lại false (state = mặc định).</summary>
        public static bool TryDecodePrivate(string text, out AttendancePrivateState state)
        {
            state = new AttendancePrivateState(string.Empty);
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            state = new AttendancePrivateState(Uri.UnescapeDataString(text.Substring(PrivatePrefix.Length)));
            return true;
        }

        static bool TryBool(string value, out bool result)
        {
            result = false;
            if (value == "0") return true;
            if (value == "1") { result = true; return true; }
            return false;
        }
    }
}
