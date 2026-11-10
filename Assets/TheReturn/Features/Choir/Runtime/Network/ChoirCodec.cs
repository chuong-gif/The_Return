/*
 * Mục đích: Chuyển lệnh và trạng thái của «Hợp xướng» thành CHUỖI ngắn và ngược lại (T-037), để cầu nối mạng chỉ việc gửi chuỗi.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, tiến độ/thắng không khớp, bục trùng trong thẻ
 * đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host. Cửa sổ giây gửi dưới dạng mili giây nguyên (InvariantCulture).
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.ChoirNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ Hợp xướng.</summary>
    public static class ChoirCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const char ListSep = ',';
        const char PairSep = '=';
        const string PrivatePrefix = "o:";
        const int PublicFields = 11;
        const int MaxWindowMs = 600000;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|bục».</summary>
        public static string EncodeCommand(ChoirCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + command.Pad.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ, ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out ChoirCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text) || text.Length > 16) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 2) return false;
            int kind, pad;
            if (!Int(parts[0], out kind) || !Int(parts[1], out pad)) return false;
            if (!Enum.IsDefined(typeof(ChoirCommandKind), kind)) return false;
            switch ((ChoirCommandKind)kind)
            {
                case ChoirCommandKind.Press:
                    if (pad < 0 || pad >= ChoirPublicState.PadCount) return false;
                    command = ChoirCommand.Press(pad);
                    return true;
                case ChoirCommandKind.Retry:
                    if (pad != 0) return false;
                    command = ChoirCommand.Retry();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 11 trường cách nhau bởi | (trường 2 là dãy chữ số ký hiệu liền nhau).</summary>
        public static string EncodePublic(ChoirPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            for (int i = 0; i < s.NoteCount; i++) sb.Append(s.SymbolAt(i).ToString(Inv));
            sb.Append(Sep);
            sb.Append(s.Progress.ToString(Inv)).Append(Sep);
            sb.Append(s.Mistakes.ToString(Inv)).Append(Sep);
            sb.Append(s.Timeouts.ToString(Inv)).Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(ToMs(s.WindowSeconds).ToString(Inv)).Append(Sep);
            sb.Append(s.ResetCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.LastMessage).ToString(Inv)).Append(Sep);
            sb.Append(s.LastActor.ToString(Inv)).Append(Sep);
            sb.Append(s.LastPad.ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out ChoirPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != PublicFields) return false;
            int playerCount, progress, mistakes, timeouts, windowMs, resetCount, message, actor, pad;
            bool solved;
            if (!Int(f[0], out playerCount) || playerCount < 2 || playerCount > 4) return false;
            string seq = f[1];
            if (seq.Length < 2 || seq.Length > ChoirPublicState.MaxNotes) return false;
            int[] symbols = new int[seq.Length];
            for (int i = 0; i < seq.Length; i++)
            {
                if (seq[i] < '0' || seq[i] >= '0' + ChoirPublicState.PadCount) return false;
                symbols[i] = seq[i] - '0';
            }
            if (!Int(f[2], out progress) || progress < 0 || progress > symbols.Length) return false;
            if (!Int(f[3], out mistakes) || mistakes < 0 || mistakes > 1000000) return false;
            if (!Int(f[4], out timeouts) || timeouts < 0 || timeouts > 1000000) return false;
            if (!TryBool(f[5], out solved)) return false;
            if (solved != (progress == symbols.Length)) return false;
            if (!Int(f[6], out windowMs) || windowMs < 0 || windowMs > MaxWindowMs) return false;
            if (!Int(f[7], out resetCount) || resetCount < 0 || resetCount > 1000000) return false;
            if (!Int(f[8], out message) || !Enum.IsDefined(typeof(ChoirMessage), message)) return false;
            if (solved && (ChoirMessage)message != ChoirMessage.Completed) return false;
            if (!Int(f[9], out actor) || actor < -1 || actor >= playerCount) return false;
            if (!Int(f[10], out pad) || pad < -1 || pad >= ChoirPublicState.PadCount) return false;
            try
            {
                state = new ChoirPublicState(playerCount, symbols, progress, mistakes, timeouts, solved, windowMs / 1000.0,
                    resetCount, (ChoirMessage)message, actor, pad);
                return true;
            }
            catch (ArgumentException)
            {
                state = null;
                return false;
            }
        }

        /// <summary>Nhận thông tin riêng; trả chuỗi «o:» + các cặp «ký hiệu=bục» cách nhau bởi dấu phẩy.</summary>
        public static string EncodePrivate(ChoirPrivateState state)
        {
            int[] symbols = state.SymbolsCopy();
            int[] pads = state.PadsCopy();
            StringBuilder sb = new StringBuilder(PrivatePrefix);
            for (int i = 0; i < symbols.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(symbols[i].ToString(Inv)).Append(PairSep).Append(pads[i].ToString(Inv));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ (ký hiệu trong 0..3 và tăng dần, bục trong 0..3 và không trùng nhau),
        /// ngược lại false (state = mặc định rỗng).
        /// </summary>
        public static bool TryDecodePrivate(string text, out ChoirPrivateState state)
        {
            state = new ChoirPrivateState(null, null);
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            string body = text.Substring(PrivatePrefix.Length);
            if (body.Length == 0) return true;
            string[] pairs = body.Split(ListSep);
            if (pairs.Length > ChoirPublicState.PadCount) return false;
            int[] symbols = new int[pairs.Length];
            int[] pads = new int[pairs.Length];
            bool[] padUsed = new bool[ChoirPublicState.PadCount];
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] kv = pairs[i].Split(PairSep);
                if (kv.Length != 2) return false;
                int symbol, pad;
                if (!Int(kv[0], out symbol) || symbol < 0 || symbol >= ChoirPublicState.PadCount) return false;
                if (!Int(kv[1], out pad) || pad < 0 || pad >= ChoirPublicState.PadCount) return false;
                if (i > 0 && symbol <= symbols[i - 1]) return false;
                if (padUsed[pad]) return false;
                padUsed[pad] = true;
                symbols[i] = symbol;
                pads[i] = pad;
            }
            state = new ChoirPrivateState(symbols, pads);
            return true;
        }

        /// <summary>Nhận giây; trả mili giây nguyên đã kẹp vào 0..MaxWindowMs.</summary>
        static int ToMs(double seconds)
        {
            double ms = Math.Round(seconds * 1000.0);
            if (ms < 0) return 0;
            if (ms > MaxWindowMs) return MaxWindowMs;
            return (int)ms;
        }

        /// <summary>Nhận chuỗi; trả true và số nguyên nếu đọc được (InvariantCulture).</summary>
        static bool Int(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, Inv, out result);
        }

        /// <summary>Nhận chuỗi; chỉ «0» hoặc «1» hợp lệ.</summary>
        static bool TryBool(string value, out bool result)
        {
            result = false;
            if (value == "0") return true;
            if (value == "1") { result = true; return true; }
            return false;
        }
    }
}
