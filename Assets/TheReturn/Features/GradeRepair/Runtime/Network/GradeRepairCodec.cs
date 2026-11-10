/*
 * Mục đích: Chuyển lệnh và trạng thái của «Sửa bảng điểm» thành CHUỖI ngắn và ngược lại (T-036), để cầu nối mạng chỉ việc gửi chuỗi.
 * Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng, thừa/thiếu trường, số hoặc enum ngoài khoảng, đã thắng mà chưa đọc đủ/dùng đủ phiếu,
 * sự kiện tài liệu không phải ASCII in được... đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host. Sự kiện tài liệu được thoát
 * bằng Uri.EscapeDataString. Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.GradeRepairNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ sửa bảng điểm.</summary>
    public static class GradeRepairCodec
    {
        /// <summary>Giới hạn ký tự mỗi chuỗi của cầu nối mạng (MissionNetBridge).</summary>
        public const int MaxChars = 2000;

        const char Sep = '|';
        const char ListSep = ',';
        const char PairSep = '=';
        const string PrivatePrefix = "g:";
        const int PublicFields = 9;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|chỉ số|nguồn|đích».</summary>
        public static string EncodeCommand(GradeRepairCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + command.Index.ToString(Inv) + Sep + command.Source.ToString(Inv) + Sep + command.Target.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ về khoảng giá trị, ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out GradeRepairCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text) || text.Length > 32) return false;
            string[] f = text.Split(Sep);
            if (f.Length != 4) return false;
            int kind, index, source, target;
            if (!Int(f[0], out kind) || !Int(f[1], out index) || !Int(f[2], out source) || !Int(f[3], out target)) return false;
            if (!Enum.IsDefined(typeof(GradeRepairCommandKind), kind)) return false;
            switch ((GradeRepairCommandKind)kind)
            {
                case GradeRepairCommandKind.Read:
                    if (index < 0 || index >= GradeRepairPublicState.Documents || source != 0 || target != 0) return false;
                    command = GradeRepairCommand.Read(index);
                    return true;
                case GradeRepairCommandKind.Apply:
                    if (index < 0 || index >= GradeRepairPublicState.Vouchers) return false;
                    if (source < -1 || source >= GradeRepairPublicState.Rows) return false;
                    if (target < 0 || target >= GradeRepairPublicState.Rows) return false;
                    command = GradeRepairCommand.Apply(index, source, target);
                    return true;
                case GradeRepairCommandKind.Undo:
                    if (index != 0 || source != 0 || target != 0) return false;
                    command = GradeRepairCommand.Undo();
                    return true;
                case GradeRepairCommandKind.Submit:
                    if (index != 0 || source != 0 || target != 0) return false;
                    command = GradeRepairCommand.Submit();
                    return true;
                case GradeRepairCommandKind.Reset:
                    if (index != 0 || source != 0 || target != 0) return false;
                    command = GradeRepairCommand.Reset();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi 9 trường cách nhau bởi |.</summary>
        public static string EncodePublic(GradeRepairPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            for (int i = 0; i < GradeRepairPublicState.Rows; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(s.Score(i).ToString(Inv));
            }
            sb.Append(Sep).Append(s.UsedMask.ToString(Inv));
            sb.Append(Sep).Append(s.ReadMask.ToString(Inv));
            sb.Append(Sep).Append(s.Solved ? "1" : "0");
            sb.Append(Sep).Append(s.ResetCount.ToString(Inv));
            sb.Append(Sep).Append(((int)s.LastEvent).ToString(Inv));
            sb.Append(Sep).Append(((int)s.LastResult).ToString(Inv));
            sb.Append(Sep).Append(s.LastActor.ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out GradeRepairPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text) || text.Length > MaxChars) return false;
            string[] f = text.Split(Sep);
            if (f.Length != PublicFields) return false;
            int playerCount, used, read, resets, evt, result, actor;
            bool solved;
            if (!Int(f[0], out playerCount) || playerCount < 2 || playerCount > 4) return false;
            string[] sc = f[1].Split(ListSep);
            if (sc.Length != GradeRepairPublicState.Rows) return false;
            int[] scores = new int[GradeRepairPublicState.Rows];
            for (int i = 0; i < scores.Length; i++)
                if (!Int(sc[i], out scores[i]) || scores[i] < 0 || scores[i] > GradeRepairPublicState.MaxScore) return false;
            if (!Int(f[2], out used) || used < 0 || used >= (1 << GradeRepairPublicState.Vouchers)) return false;
            if (!Int(f[3], out read) || read < 0 || read >= (1 << GradeRepairPublicState.Documents)) return false;
            if (!TryBool(f[4], out solved)) return false;
            if (solved && (used != (1 << GradeRepairPublicState.Vouchers) - 1 || read != (1 << GradeRepairPublicState.Documents) - 1)) return false;
            if (!Int(f[5], out resets) || resets < 0 || resets > 1000000) return false;
            if (!Int(f[6], out evt) || !Enum.IsDefined(typeof(GradeRepairEvent), evt)) return false;
            if (!Int(f[7], out result) || !Enum.IsDefined(typeof(GradeResult), result)) return false;
            if (!Int(f[8], out actor) || actor < -1 || actor >= playerCount) return false;
            try
            {
                state = new GradeRepairPublicState(playerCount, scores, used, read, solved, resets, (GradeRepairEvent)evt, (GradeResult)result, actor);
                return true;
            }
            catch (ArgumentException)
            {
                state = null;
                return false;
            }
        }

        /// <summary>Nhận thông tin riêng; trả chuỗi «g:» + các cặp «tài liệu=sự kiện đã thoát» cách nhau bởi dấu phẩy.</summary>
        public static string EncodePrivate(GradeRepairPrivateState state)
        {
            int[] docs = state.DocumentsCopy();
            string[] facts = state.FactsCopy();
            StringBuilder sb = new StringBuilder(PrivatePrefix);
            for (int i = 0; i < docs.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(docs[i].ToString(Inv)).Append(PairSep).Append(Uri.EscapeDataString(facts[i]));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ (tài liệu 0-3 tăng dần, sự kiện ASCII in được dài tối đa 100),
        /// ngược lại false (state = mặc định rỗng).
        /// </summary>
        public static bool TryDecodePrivate(string text, out GradeRepairPrivateState state)
        {
            state = new GradeRepairPrivateState(null, null);
            if (text == null || text.Length > MaxChars || !text.StartsWith(PrivatePrefix, StringComparison.Ordinal)) return false;
            string body = text.Substring(PrivatePrefix.Length);
            if (body.Length == 0) return true;
            string[] pairs = body.Split(ListSep);
            if (pairs.Length > GradeRepairPublicState.Documents) return false;
            int[] docs = new int[pairs.Length];
            string[] facts = new string[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] kv = pairs[i].Split(PairSep);
                if (kv.Length != 2) return false;
                int doc;
                if (!Int(kv[0], out doc) || doc < 0 || doc >= GradeRepairPublicState.Documents) return false;
                if (i > 0 && doc <= docs[i - 1]) return false;
                string fact;
                try
                {
                    fact = Uri.UnescapeDataString(kv[1]);
                }
                catch (UriFormatException)
                {
                    return false;
                }
                if (!GradeRepairPrivateState.IsValidFact(fact)) return false;
                // Dạng thoát phải CHÍNH TẮC: Uri.UnescapeDataString không ném lỗi với «%» trần hay «%zz» (trả lại nguyên văn), trong khi bộ mã hóa luôn thoát «%» thành «%25».
                // Thoát lại chuỗi vừa giải mã mà không ra đúng chuỗi nhận được = sai định dạng (FIX-F-004).
                if (!string.Equals(Uri.EscapeDataString(fact), kv[1], StringComparison.Ordinal)) return false;
                docs[i] = doc;
                facts[i] = fact;
            }
            state = new GradeRepairPrivateState(docs, facts);
            return true;
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
