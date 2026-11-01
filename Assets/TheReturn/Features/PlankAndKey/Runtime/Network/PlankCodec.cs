/*
 * Mục đích: Chuyển lệnh và trạng thái của nhiệm vụ cầu ván thành CHUỖI ngắn và ngược lại (T-027), để cầu nối mạng chỉ việc gửi chuỗi.
 * Dùng InvariantCulture nên máy dùng dấu phẩy thập phân (vi-VN) vẫn đọc đúng. Phần giải mã KHÔNG tin dữ liệu từ mạng: sai định dạng,
 * thừa/thiếu trường, số không hữu hạn, enum hoặc chỉ số ngoài khoảng đều trả false chứ không ném lỗi, để máy khách xấu không làm sập host.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: EncodeCommand; TryDecodeCommand; EncodePublic; TryDecodePublic; EncodePrivate; TryDecodePrivate.
 */
using System;
using System.Globalization;
using System.Text;

namespace TheReturn.PlankNet
{
    /// <summary>Mã hóa/giải mã chuỗi cho lệnh, trạng thái chung và thông tin riêng của nhiệm vụ cầu ván.</summary>
    public static class PlankCodec
    {
        const char Sep = '|';
        const char ListSep = ',';
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Nhận lệnh; trả chuỗi dạng «loại|bờA».</summary>
        public static string EncodeCommand(PlankCommand command)
        {
            return ((int)command.Kind).ToString(Inv) + Sep + (command.SideA ? "1" : "0");
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và lệnh nếu hợp lệ, ngược lại false (command = null).</summary>
        public static bool TryDecodeCommand(string text, out PlankCommand command)
        {
            command = null;
            if (string.IsNullOrEmpty(text)) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 2) return false;
            int kind;
            if (!int.TryParse(parts[0], NumberStyles.Integer, Inv, out kind)) return false;
            if (!Enum.IsDefined(typeof(PlankCommandKind), kind)) return false;
            if (parts[1] != "0" && parts[1] != "1") return false;
            switch ((PlankCommandKind)kind)
            {
                case PlankCommandKind.PickUpCard: command = PlankCommand.PickUpCard(); break;
                case PlankCommandKind.DropCard: command = PlankCommand.DropCard(); break;
                case PlankCommandKind.UseLever: command = PlankCommand.UseLever(parts[1] == "1"); break;
                case PlankCommandKind.Retry: command = PlankCommand.Retry(); break;
                default: return false;
            }
            return true;
        }

        /// <summary>Nhận trạng thái chung; trả chuỗi gồm 18 trường cách nhau bởi |.</summary>
        public static string EncodePublic(PlankPublicState s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.PlayerCount.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.Phase).ToString(Inv)).Append(Sep);
            sb.Append(((int)s.Status).ToString(Inv)).Append(Sep);
            sb.Append(s.DeckAtNear ? "1" : "0").Append(Sep);
            sb.Append(Float(s.CrossingSecondsTotal)).Append(Sep);
            sb.Append(Float(s.CrossingSecondsLeft)).Append(Sep);
            int[] riders = s.RidersCopy();
            for (int i = 0; i < riders.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(riders[i].ToString(Inv));
            }
            sb.Append(Sep);
            PlankBank[] banks = s.PlayerBanksCopy();
            for (int i = 0; i < banks.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(((int)banks[i]).ToString(Inv));
            }
            sb.Append(Sep);
            sb.Append(s.CardHolder.ToString(Inv)).Append(Sep);
            sb.Append(((int)s.CardMode).ToString(Inv)).Append(Sep);
            sb.Append(Float(s.CardLocalX)).Append(Sep);
            sb.Append(Float(s.CardLocalZ)).Append(Sep);
            sb.Append(Float(s.DisplayUsedSeconds)).Append(Sep);
            sb.Append(Float(s.DeadlineSeconds)).Append(Sep);
            sb.Append(s.Attempts.ToString(Inv)).Append(Sep);
            sb.Append(s.ResetCount.ToString(Inv)).Append(Sep);
            sb.Append(s.Solved ? "1" : "0").Append(Sep);
            sb.Append(((int)s.LastMessage).ToString(Inv));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecodePublic(string text, out PlankPublicState state)
        {
            state = null;
            if (string.IsNullOrEmpty(text)) return false;
            string[] p = text.Split(Sep);
            if (p.Length != 18) return false;

            int playerCount, phase, status, cardHolder, cardMode, attempts, resetCount, message;
            bool deckAtNear, solved;
            float total, left, cardX, cardZ, used, deadline;
            if (!Int(p[0], out playerCount) || playerCount < PlankRound.MinPlayers || playerCount > PlankRound.MaxPlayers) return false;
            if (!Int(p[1], out phase) || !Enum.IsDefined(typeof(PlankPhase), phase)) return false;
            if (!Int(p[2], out status) || !Enum.IsDefined(typeof(PlankStatus), status)) return false;
            if (!Bool(p[3], out deckAtNear)) return false;
            if (!Float(p[4], out total) || total < 0f) return false;
            if (!Float(p[5], out left) || left < 0f) return false;

            int[] riders;
            if (!IntList(p[6], out riders) || riders.Length > 2) return false;
            for (int i = 0; i < riders.Length; i++)
            {
                if (riders[i] < 0 || riders[i] >= playerCount) return false;
            }

            int[] bankInts;
            if (!IntList(p[7], out bankInts) || bankInts.Length != playerCount) return false;
            PlankBank[] banks = new PlankBank[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                if (!Enum.IsDefined(typeof(PlankBank), bankInts[i])) return false;
                banks[i] = (PlankBank)bankInts[i];
            }

            if (!Int(p[8], out cardHolder) || cardHolder < -1 || cardHolder >= playerCount) return false;
            if (!Int(p[9], out cardMode) || !Enum.IsDefined(typeof(PlankCardMode), cardMode)) return false;
            if (!Float(p[10], out cardX) || !Float(p[11], out cardZ)) return false;
            if (!Float(p[12], out used) || used < 0f) return false;
            if (!Float(p[13], out deadline) || deadline < 0f) return false;
            if (!Int(p[14], out attempts) || attempts < 0) return false;
            if (!Int(p[15], out resetCount) || resetCount < 0) return false;
            if (!Bool(p[16], out solved)) return false;
            if (!Int(p[17], out message) || !Enum.IsDefined(typeof(PlankMessage), message)) return false;

            state = new PlankPublicState(playerCount, (PlankPhase)phase, (PlankStatus)status, deckAtNear, total, left, riders, banks,
                cardHolder, (PlankCardMode)cardMode, cardX, cardZ, used, deadline, attempts, resetCount, solved, (PlankMessage)message);
            return true;
        }

        /// <summary>Nhận thông tin riêng; trả chuỗi.</summary>
        public static string EncodePrivate(PlankPrivateState state)
        {
            return state.CrossUnits.ToString(Inv);
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và thông tin riêng nếu hợp lệ (số nguyên không âm).</summary>
        public static bool TryDecodePrivate(string text, out PlankPrivateState state)
        {
            state = default(PlankPrivateState);
            int units;
            if (!Int(text, out units) || units < 0) return false;
            state = new PlankPrivateState(units);
            return true;
        }

        static string Float(float value)
        {
            return value.ToString("R", Inv);
        }

        static bool Float(string text, out float value)
        {
            if (!float.TryParse(text, NumberStyles.Float, Inv, out value)) return false;
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        static bool Int(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, Inv, out value);
        }

        static bool Bool(string text, out bool value)
        {
            value = text == "1";
            return text == "0" || text == "1";
        }

        static bool IntList(string text, out int[] values)
        {
            if (text == null) { values = null; return false; }
            if (text.Length == 0) { values = new int[0]; return true; }
            string[] parts = text.Split(ListSep);
            values = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!Int(parts[i], out values[i])) return false;
            }
            return true;
        }
    }
}
