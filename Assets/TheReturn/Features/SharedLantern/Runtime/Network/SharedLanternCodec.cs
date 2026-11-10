/*
 * Mục đích: Chuyển lệnh và trạng thái của «Đèn lồng chung» thành CHUỖI ngắn và ngược lại (T-037), để cầu nối mạng chỉ việc gửi chuỗi.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, mặt nạ sai, thắng mà bảng chưa đúng, sổ có ký hiệu trùng
 * đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host. Giây gửi dưới dạng mili giây nguyên (InvariantCulture); bảng nhập gửi 4 ký tự
 * («.» = ô trống, «0»-«5» = ký hiệu).
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.LanternNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và sổ riêng của nhiệm vụ Đèn lồng chung.</summary>
    public static class SharedLanternCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const char ListSep = ',';
        const char PairSep = '=';
        const string PrivatePrefix = "o:";
        const int PublicFields = 15;
        const int MaxMs = 600000;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|chỉ số|ký hiệu».</summary>
        public static string EncodeCommand(SharedLanternCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + command.Index.ToString(Inv) + Sep + command.Symbol.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ (trường không dùng phải bằng 0), ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out SharedLanternCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text) || text.Length > 16) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 3) return false;
            int kind, index, symbol;
            if (!Int(parts[0], out kind) || !Int(parts[1], out index) || !Int(parts[2], out symbol)) return false;
            if (!Enum.IsDefined(typeof(SharedLanternCommandKind), kind)) return false;
            switch ((SharedLanternCommandKind)kind)
            {
                case SharedLanternCommandKind.Pass:
                    if (index < 0 || index >= 4 || symbol != 0) return false;
                    command = SharedLanternCommand.Pass(index);
                    return true;
                case SharedLanternCommandKind.Read:
                    if (index < 0 || index >= SharedLanternPublicState.SignCount || symbol != 0) return false;
                    command = SharedLanternCommand.Read(index);
                    return true;
                case SharedLanternCommandKind.SetEntry:
                    if (index < 0 || index >= SharedLanternPublicState.SignCount) return false;
                    if (symbol < 0 || symbol >= SharedLanternPublicState.SymbolCount) return false;
                    command = SharedLanternCommand.SetEntry(index, symbol);
                    return true;
                case SharedLanternCommandKind.ClearEntries:
                    if (index != 0 || symbol != 0) return false;
                    command = SharedLanternCommand.ClearEntries();
                    return true;
                case SharedLanternCommandKind.Submit:
                    if (index != 0 || symbol != 0) return false;
                    command = SharedLanternCommand.Submit();
                    return true;
                case SharedLanternCommandKind.Retry:
                    if (index != 0 || symbol != 0) return false;
                    command = SharedLanternCommand.Retry();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 15 trường cách nhau bởi | (trường 6 là bảng nhập 4 ký tự).</summary>
        public static string EncodePublic(SharedLanternPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            sb.Append(s.Holder.ToString(Inv)).Append(Sep);
            sb.Append(s.OutsideMask.ToString(Inv)).Append(Sep);
            sb.Append(s.SentBackCount.ToString(Inv)).Append(Sep);
            sb.Append(s.LastSentBackMask.ToString(Inv)).Append(Sep);
            for (int i = 0; i < SharedLanternPublicState.SignCount; i++)
            {
                int v = s.Entry(i);
                sb.Append(v < 0 ? '.' : (char)('0' + v));
            }
            sb.Append(Sep);
            sb.Append(s.CodeAccepted ? "1" : "0").Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(s.WrongSubmits.ToString(Inv)).Append(Sep);
            sb.Append(ToMs(s.OutsideLimitSeconds).ToString(Inv)).Append(Sep);
            sb.Append(ToMs(s.SubmitLockSeconds).ToString(Inv)).Append(Sep);
            sb.Append(s.ResetCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.LastMessage).ToString(Inv)).Append(Sep);
            sb.Append(s.LastActor.ToString(Inv)).Append(Sep);
            sb.Append(s.LastIndex.ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out SharedLanternPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != PublicFields) return false;
            int playerCount, holder, outside, sentBack, lastSent, wrong, limitMs, lockMs, resetCount, message, actor, index;
            bool accepted, solved;
            if (!Int(f[0], out playerCount) || playerCount < 2 || playerCount > 4) return false;
            int all = (1 << playerCount) - 1;
            if (!Int(f[1], out holder) || holder < 0 || holder >= playerCount) return false;
            if (!Int(f[2], out outside) || outside < 0 || outside > all || (outside >> holder & 1) == 1) return false;
            if (!Int(f[3], out sentBack) || sentBack < 0 || sentBack > 1000000) return false;
            if (!Int(f[4], out lastSent) || lastSent < 0 || lastSent > all) return false;
            if (sentBack == 0 && lastSent != 0) return false;
            string entryText = f[5];
            if (entryText.Length != SharedLanternPublicState.SignCount) return false;
            int[] entries = new int[SharedLanternPublicState.SignCount];
            bool full = true;
            for (int i = 0; i < entries.Length; i++)
            {
                char c = entryText[i];
                if (c == '.') { entries[i] = -1; full = false; }
                else if (c >= '0' && c < '0' + SharedLanternPublicState.SymbolCount) entries[i] = c - '0';
                else return false;
            }
            if (!TryBool(f[6], out accepted)) return false;
            if (accepted && !full) return false;
            if (!TryBool(f[7], out solved)) return false;
            if (solved && !accepted) return false;
            if (!Int(f[8], out wrong) || wrong < 0 || wrong > 1000000) return false;
            if (!Int(f[9], out limitMs) || limitMs <= 0 || limitMs > MaxMs) return false;
            if (!Int(f[10], out lockMs) || lockMs < 0 || lockMs > MaxMs) return false;
            if (!Int(f[11], out resetCount) || resetCount < 0 || resetCount > 1000000) return false;
            if (!Int(f[12], out message) || !Enum.IsDefined(typeof(SharedLanternMessage), message)) return false;
            if (solved != ((SharedLanternMessage)message == SharedLanternMessage.Solved)) return false;
            if (!Int(f[13], out actor) || actor < -1 || actor >= playerCount) return false;
            if (!Int(f[14], out index) || index < -1 || index >= SharedLanternPublicState.SymbolCount) return false;
            try
            {
                state = new SharedLanternPublicState(playerCount, holder, outside, sentBack, lastSent, entries, accepted, solved, wrong,
                    limitMs / 1000.0, lockMs / 1000.0, resetCount, (SharedLanternMessage)message, actor, index);
                return true;
            }
            catch (ArgumentException)
            {
                state = null;
                return false;
            }
        }

        /// <summary>Nhận sổ riêng; trả chuỗi «o:» + các cặp «biển=ký hiệu» cách nhau bởi dấu phẩy.</summary>
        public static string EncodePrivate(SharedLanternPrivateState state)
        {
            int[] signs = state.SignsCopy();
            int[] symbols = state.SymbolsCopy();
            StringBuilder sb = new StringBuilder(PrivatePrefix);
            for (int i = 0; i < signs.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(signs[i].ToString(Inv)).Append(PairSep).Append(symbols[i].ToString(Inv));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Nhận chuỗi từ mạng; trả true và sổ riêng nếu hợp lệ (biển trong 0..3 và tăng dần, ký hiệu trong 0..5 và KHÔNG trùng nhau vì 4 biển mang 4 ký hiệu khác nhau),
        /// ngược lại false (state = mặc định rỗng).
        /// </summary>
        public static bool TryDecodePrivate(string text, out SharedLanternPrivateState state)
        {
            state = new SharedLanternPrivateState(null, null);
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            string body = text.Substring(PrivatePrefix.Length);
            if (body.Length == 0) return true;
            string[] pairs = body.Split(ListSep);
            if (pairs.Length > SharedLanternPublicState.SignCount) return false;
            int[] signs = new int[pairs.Length];
            int[] symbols = new int[pairs.Length];
            bool[] symbolUsed = new bool[SharedLanternPublicState.SymbolCount];
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] kv = pairs[i].Split(PairSep);
                if (kv.Length != 2) return false;
                int sign, symbol;
                if (!Int(kv[0], out sign) || sign < 0 || sign >= SharedLanternPublicState.SignCount) return false;
                if (!Int(kv[1], out symbol) || symbol < 0 || symbol >= SharedLanternPublicState.SymbolCount) return false;
                if (i > 0 && sign <= signs[i - 1]) return false;
                if (symbolUsed[symbol]) return false;
                symbolUsed[symbol] = true;
                signs[i] = sign;
                symbols[i] = symbol;
            }
            state = new SharedLanternPrivateState(signs, symbols);
            return true;
        }

        /// <summary>Nhận giây; trả mili giây nguyên đã kẹp vào 0..MaxMs.</summary>
        static int ToMs(double seconds)
        {
            double ms = Math.Round(seconds * 1000.0);
            if (ms < 0) return 0;
            if (ms > MaxMs) return MaxMs;
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
