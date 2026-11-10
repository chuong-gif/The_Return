/*
 * Mục đích: Chuyển lệnh và trạng thái của «Cô nói» thành CHUỖI ngắn và ngược lại (T-037), để cầu nối mạng chỉ việc gửi chuỗi.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, dấu +, khoảng trắng, đoạn không khớp cờ thắng...
 * đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host. Thuần C#, không phụ thuộc Unity.
 * Định dạng: lệnh «0» (làm lại); trạng thái chung 12 trường cách nhau bởi |; thông tin riêng «o:» (rỗng) hoặc «o:1|cờ-cô-nói|ms-còn-lại|đã-đứng-đủ».
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.TeacherSaysNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ Cô nói.</summary>
    public static class TeacherSaysCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const string PrivatePrefix = "o:";
        const int PublicFields = 12;
        const int PrivateFields = 4;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi «0» cho lệnh làm lại.</summary>
        public static string EncodeCommand(TeacherSaysCommand command)
        {
            return ((int)command.Kind).ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu đúng «0», ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out TeacherSaysCommand command)
        {
            command = null;
            if (text != "0") return false;
            command = TeacherSaysCommand.Retry();
            return true;
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 12 trường cách nhau bởi |.</summary>
        public static string EncodePublic(TeacherSaysPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            sb.Append(s.Started ? "1" : "0").Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(s.Segment.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.Command).ToString(Inv)).Append(Sep);
            sb.Append(s.Listener.ToString(Inv)).Append(Sep);
            sb.Append(s.Warnings.ToString(Inv)).Append(Sep);
            sb.Append(s.MaxWarnings.ToString(Inv)).Append(Sep);
            sb.Append(s.SentBacks.ToString(Inv)).Append(Sep);
            sb.Append(s.TotalWarnings.ToString(Inv)).Append(Sep);
            sb.Append(s.RetryCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.LastEvent).ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out TeacherSaysPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != PublicFields) return false;
            int playerCount, segment, command, listener, warnings, maxWarnings, sentBacks, total, retry, lastEvent;
            bool started, solved;
            if (!Int(f[0], out playerCount)) return false;
            if (!TryBool(f[1], out started)) return false;
            if (!TryBool(f[2], out solved)) return false;
            if (!Int(f[3], out segment)) return false;
            if (!Int(f[4], out command)) return false;
            if (!Int(f[5], out listener)) return false;
            if (!Int(f[6], out warnings)) return false;
            if (!Int(f[7], out maxWarnings)) return false;
            if (!Int(f[8], out sentBacks)) return false;
            if (!Int(f[9], out total)) return false;
            if (!Int(f[10], out retry)) return false;
            if (!Int(f[11], out lastEvent)) return false;
            if (!Enum.IsDefined(typeof(TheReturn.TeacherCommand), command)) return false;
            if (!Enum.IsDefined(typeof(TeacherSaysNetEvent), lastEvent)) return false;
            try
            {
                state = new TeacherSaysPublicState(playerCount, started, solved, segment, (TheReturn.TeacherCommand)command, listener,
                    warnings, maxWarnings, sentBacks, total, retry, (TeacherSaysNetEvent)lastEvent);
                return true;
            }
            catch (ArgumentException)
            {
                state = null;
                return false;
            }
        }

        /// <summary>Nhận thông tin riêng; trả «o:» nếu rỗng (không phải người nghe), ngược lại «o:1|cờ|ms|cờ».</summary>
        public static string EncodePrivate(TeacherSaysPrivateState state)
        {
            if (!state.IsListener) return PrivatePrefix;
            StringBuilder sb = new StringBuilder(PrivatePrefix);
            sb.Append("1").Append(Sep);
            sb.Append(state.TeacherSaid ? "1" : "0").Append(Sep);
            sb.Append(state.RemainingMs.ToString(Inv)).Append(Sep);
            sb.Append(state.StandSatisfied ? "1" : "0");
            return sb.ToString();
        }

        /// <summary>
        /// Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ (đúng «o:» hoặc «o:» + 4 trường với trường đầu là 1), ngược lại false
        /// (state = rỗng).
        /// </summary>
        public static bool TryDecodePrivate(string text, out TeacherSaysPrivateState state)
        {
            state = TeacherSaysPrivateState.None;
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            string body = text.Substring(PrivatePrefix.Length);
            if (body.Length == 0) return true;
            string[] f = body.Split(Sep);
            if (f.Length != PrivateFields) return false;
            if (f[0] != "1") return false;
            bool said, stand;
            int remaining;
            if (!TryBool(f[1], out said)) return false;
            if (!Int(f[2], out remaining)) return false;
            if (!TryBool(f[3], out stand)) return false;
            try
            {
                state = new TeacherSaysPrivateState(true, said, remaining, stand);
                return true;
            }
            catch (ArgumentException)
            {
                state = TeacherSaysPrivateState.None;
                return false;
            }
        }

        /// <summary>Nhận chuỗi; trả true và số nguyên nếu là số thập phân thường (cho phép dấu trừ, cấm dấu +, khoảng trắng, chuỗi rỗng, tràn số).</summary>
        static bool Int(string value, out int result)
        {
            result = 0;
            if (string.IsNullOrEmpty(value) || value[0] == '+') return false;
            return int.TryParse(value, NumberStyles.AllowLeadingSign, Inv, out result);
        }

        /// <summary>Nhận chuỗi; trả true nếu đúng «0» hoặc «1» và gán cờ tương ứng.</summary>
        static bool TryBool(string value, out bool result)
        {
            result = false;
            if (value == "0") return true;
            if (value == "1") { result = true; return true; }
            return false;
        }
    }
}
