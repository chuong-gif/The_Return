/*
 * Mục đích: Chuyển lệnh và trạng thái của «Bảng cầu dao» thành CHUỖI ngắn và ngược lại (T-031), để cầu nối mạng chỉ việc gửi chuỗi.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, bật quá giới hạn, thắng mà đèn chưa khớp
 * đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host. Số thực được gửi dưới dạng mili giây nguyên (InvariantCulture).
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.BreakerNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ cầu dao.</summary>
    public static class BreakerCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const char ListSep = ',';
        const char PairSep = '=';
        const string PrivatePrefix = "o:";
        const int PublicFields = 12;
        const int MaxHoldMs = 600000;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|công tắc».</summary>
        public static string EncodeCommand(BreakerCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + command.Switch.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ, ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out BreakerCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text) || text.Length > 16) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 2) return false;
            int kind, sw;
            if (!int.TryParse(parts[0], NumberStyles.Integer, Inv, out kind)) return false;
            if (!int.TryParse(parts[1], NumberStyles.Integer, Inv, out sw)) return false;
            if (!Enum.IsDefined(typeof(BreakerCommandKind), kind)) return false;
            switch ((BreakerCommandKind)kind)
            {
                case BreakerCommandKind.Toggle:
                    if (sw < 0 || sw >= BreakerPublicState.MaxSize) return false;
                    command = BreakerCommand.Toggle(sw);
                    return true;
                case BreakerCommandKind.Retry:
                    if (sw != 0) return false;
                    command = BreakerCommand.Retry();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 12 trường cách nhau bởi |.</summary>
        public static string EncodePublic(BreakerPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            sb.Append(s.OnSwitches.ToString(Inv)).Append(Sep);
            sb.Append(s.Lamps.ToString(Inv)).Append(Sep);
            sb.Append(s.TargetLamps.ToString(Inv)).Append(Sep);
            sb.Append(s.Trips.ToString(Inv)).Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(ToMs(s.HoldElapsed).ToString(Inv)).Append(Sep);
            sb.Append(ToMs(s.HoldSeconds).ToString(Inv)).Append(Sep);
            sb.Append(s.ResetCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.LastMessage).ToString(Inv)).Append(Sep);
            sb.Append(s.LastActor.ToString(Inv)).Append(Sep);
            sb.Append(s.LastSwitch.ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out BreakerPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != PublicFields) return false;
            int playerCount, on, lamps, target, trips, resetCount, message, actor, sw, holdMs, elapsedMs;
            bool solved;
            if (!Int(f[0], out playerCount) || playerCount < 2 || playerCount > 4) return false;
            int size = playerCount + 2;
            int all = (1 << size) - 1;
            if (!Int(f[1], out on) || on < 0 || on > all) return false;
            if (TheReturn.BreakerRound.CountBits(on) > (size + 1) / 2) return false;
            if (!Int(f[2], out lamps) || lamps < 0 || lamps > all) return false;
            if (!Int(f[3], out target) || target < 0 || target > all) return false;
            if (!Int(f[4], out trips) || trips < 0 || trips > 1000000) return false;
            if (!TryBool(f[5], out solved)) return false;
            if (solved && lamps != target) return false;
            if (!Int(f[6], out elapsedMs) || elapsedMs < 0 || elapsedMs > MaxHoldMs) return false;
            if (!Int(f[7], out holdMs) || holdMs < 0 || holdMs > MaxHoldMs || elapsedMs > holdMs) return false;
            if (!Int(f[8], out resetCount) || resetCount < 0 || resetCount > 1000000) return false;
            if (!Int(f[9], out message) || !Enum.IsDefined(typeof(BreakerMessage), message)) return false;
            if (!Int(f[10], out actor) || actor < -1 || actor >= playerCount) return false;
            if (!Int(f[11], out sw) || sw < -1 || sw >= size) return false;
            try
            {
                state = new BreakerPublicState(playerCount, on, lamps, target, trips, solved, elapsedMs / 1000f, holdMs / 1000f,
                    resetCount, (BreakerMessage)message, actor, sw);
                return true;
            }
            catch (ArgumentException)
            {
                state = null;
                return false;
            }
        }

        /// <summary>Nhận thông tin riêng; trả chuỗi «o:» + các cặp «công tắc=mặt nạ» cách nhau bởi dấu phẩy.</summary>
        public static string EncodePrivate(BreakerPrivateState state)
        {
            int[] sw = state.SwitchesCopy();
            int[] mask = state.MasksCopy();
            StringBuilder sb = new StringBuilder(PrivatePrefix);
            for (int i = 0; i < sw.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(sw[i].ToString(Inv)).Append(PairSep).Append(mask[i].ToString(Inv));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ (công tắc trong 0..5 và tăng dần, mặt nạ trong 6 bit), ngược lại false
        /// (state = mặc định rỗng).
        /// </summary>
        public static bool TryDecodePrivate(string text, out BreakerPrivateState state)
        {
            state = new BreakerPrivateState(null, null);
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            string body = text.Substring(PrivatePrefix.Length);
            if (body.Length == 0) return true;
            string[] pairs = body.Split(ListSep);
            if (pairs.Length > BreakerPublicState.MaxSize) return false;
            int[] switches = new int[pairs.Length];
            int[] masks = new int[pairs.Length];
            int maxMask = (1 << BreakerPublicState.MaxSize) - 1;
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] kv = pairs[i].Split(PairSep);
                if (kv.Length != 2) return false;
                int sw, mask;
                if (!Int(kv[0], out sw) || sw < 0 || sw >= BreakerPublicState.MaxSize) return false;
                if (!Int(kv[1], out mask) || mask < 0 || mask > maxMask) return false;
                if (i > 0 && sw <= switches[i - 1]) return false;
                switches[i] = sw;
                masks[i] = mask;
            }
            state = new BreakerPrivateState(switches, masks);
            return true;
        }

        static int ToMs(float seconds)
        {
            double ms = Math.Round(seconds * 1000.0);
            if (ms < 0) return 0;
            if (ms > MaxHoldMs) return MaxHoldMs;
            return (int)ms;
        }

        static bool Int(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, Inv, out result);
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
