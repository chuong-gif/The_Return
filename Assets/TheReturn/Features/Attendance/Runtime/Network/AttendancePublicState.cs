/*
 * Mục đích: Trạng thái CHUNG (mọi người cùng thấy) và thông tin RIÊNG (chỉ một người) của «Điểm danh» online (T-031). Bất biến: mọi mảng được
 * sao khi tạo và khi đọc, vì HostAuthority chia cùng một bản cho nhiều người. Manh mối (gói text của từng người) CHỈ nằm ở phần riêng.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; NamesCopy; SeatsCopy; OccupantOf; OccupiedCount; (AttendancePrivateState) constructor.
 */
using System;

namespace TheReturn.AttendanceNet
{
    /// <summary>Thông báo gần nhất để HUD hiển thị (mã enum, chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum AttendanceMessage
    {
        None,
        Seated,
        Stood,
        Retry,
        Wrong,
        Solved
    }

    /// <summary>Thông tin riêng của một ô: gói manh mối của người đó.</summary>
    public readonly struct AttendancePrivateState
    {
        /// <summary>Các manh mối người này đọc được (không bao giờ null).</summary>
        public string Clues { get; }

        /// <summary>Nhận gói manh mối (null được coi là rỗng).</summary>
        public AttendancePrivateState(string clues)
        {
            Clues = clues ?? string.Empty;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ điểm danh.</summary>
    public sealed class AttendancePublicState
    {
        public const int SeatCount = 24;

        readonly string[] names;
        readonly int[] seats;

        /// <summary>Số người của đề (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>True khi cả đội đã ngồi đúng (giữ nguyên dù sau đó có người đứng dậy).</summary>
        public bool Solved { get; }

        /// <summary>True khi vừa kiểm tra và cách ngồi chưa đúng; xóa khi có người đứng dậy hoặc làm lại.</summary>
        public bool Wrong { get; }

        /// <summary>True khi đã ngồi đủ và host đang chờ vài giây trước khi chấm.</summary>
        public bool Settling { get; }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get; }

        /// <summary>Thông báo gần nhất.</summary>
        public AttendanceMessage LastMessage { get; }

        /// <summary>
        /// Nhận số người, tên các vai (đúng PlayerCount), 24 ô ghế (mỗi ô -1 hoặc ô người đang ngồi, không trùng người), và các cờ.
        /// Ném lỗi nếu dữ liệu sai hình dạng.
        /// </summary>
        public AttendancePublicState(int playerCount, string[] names, int[] seats, bool solved, bool wrong, bool settling, int resetCount, AttendanceMessage lastMessage)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            if (names == null || names.Length != playerCount) throw new ArgumentException("names");
            if (seats == null || seats.Length != SeatCount) throw new ArgumentException("seats");
            bool[] seen = new bool[playerCount];
            for (int i = 0; i < seats.Length; i++)
            {
                int who = seats[i];
                if (who < -1 || who >= playerCount) throw new ArgumentOutOfRangeException("seats");
                if (who >= 0)
                {
                    if (seen[who]) throw new ArgumentException("Một người không thể ngồi hai ghế.");
                    seen[who] = true;
                }
            }
            PlayerCount = playerCount;
            this.names = (string[])names.Clone();
            this.seats = (int[])seats.Clone();
            Solved = solved;
            Wrong = wrong;
            Settling = settling;
            ResetCount = resetCount;
            LastMessage = lastMessage;
        }

        /// <summary>Trả bản sao tên các vai.</summary>
        public string[] NamesCopy()
        {
            return (string[])names.Clone();
        }

        /// <summary>Trả bản sao 24 ô ghế (mỗi ô là ô người đang ngồi hoặc -1).</summary>
        public int[] SeatsCopy()
        {
            return (int[])seats.Clone();
        }

        /// <summary>Nhận chỉ số ghế; trả ô người đang ngồi, hoặc -1 nếu trống / ghế không tồn tại.</summary>
        public int OccupantOf(int seat)
        {
            return seat >= 0 && seat < seats.Length ? seats[seat] : -1;
        }

        /// <summary>Số ghế đang có người.</summary>
        public int OccupiedCount()
        {
            int count = 0;
            for (int i = 0; i < seats.Length; i++) if (seats[i] >= 0) count++;
            return count;
        }
    }
}
