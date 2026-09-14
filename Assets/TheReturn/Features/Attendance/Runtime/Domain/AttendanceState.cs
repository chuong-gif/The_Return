/*
 * Mục đích: Quản lý chiếm ghế và kết quả điểm danh cho số người của đề hiện tại; không phụ thuộc Unity.
 * Danh sách hàm:
 * - AttendanceState: tạo trạng thái trống từ một đề.
 * - Occupant / SeatOf: tra người ở ghế hoặc ghế của người.
 * - TrySit / Stand: nhận yêu cầu ngồi hoặc đứng và trả kết quả chấp nhận.
 * - Evaluate: chốt thành công nếu đủ người và mọi luật đều đúng.
 * - Reset: xóa chỗ ngồi nhưng giữ nguyên đề để thử lại.
 */
using System;

namespace TheReturn
{
    public sealed class AttendanceState
    {
        readonly int[] seats = new int[AttendanceRound.SeatCount];
        public AttendanceRound Round { get; }
        public int PlayerCount => Round.PlayerCount;
        public int Revision { get; private set; }
        public bool Solved { get; private set; }
        public int OccupiedCount { get; private set; }

        /// <summary>Nhận đề hợp lệ; tạo 24 ghế trống, không tự đánh giá đáp án.</summary>
        public AttendanceState(AttendanceRound round)
        {
            Round = round ?? throw new ArgumentNullException(nameof(round));
            Reset();
        }

        /// <summary>Nhận chỉ số ghế; trả ID người hoặc -1 nếu ghế trống/chỉ số không hợp lệ.</summary>
        public int Occupant(int seat)
        {
            return seat >= 0 && seat < seats.Length ? seats[seat] : -1;
        }

        /// <summary>Nhận ID người trong đề; trả chỉ số ghế hoặc -1 nếu chưa ngồi/ID không hợp lệ.</summary>
        public int SeatOf(int player)
        {
            return player >= 0 && player < PlayerCount ? Array.IndexOf(seats, player) : -1;
        }

        /// <summary>Nhận ID người và ghế; trả true và tăng Revision nếu ghế trống, người chưa ngồi, đề chưa xong.</summary>
        public bool TrySit(int player, int seat)
        {
            if (Solved || player < 0 || player >= PlayerCount || seat < 0 || seat >= seats.Length)
                return false;
            if (seats[seat] >= 0 || SeatOf(player) >= 0) return false;
            seats[seat] = player;
            OccupiedCount++;
            Revision++;
            return true;
        }

        /// <summary>Nhận ID người; trả true khi đã nhả ghế. Thành công trước đó vẫn được giữ.</summary>
        public bool Stand(int player)
        {
            int seat = SeatOf(player);
            if (seat < 0) return false;
            seats[seat] = -1;
            OccupiedCount--;
            Revision++;
            return true;
        }

        /// <summary>Không nhận tham số; trả true khi mọi người trong đề ngồi đúng. Không chờ đủ 4 người cố định.</summary>
        public bool Evaluate()
        {
            if (Solved) return true;
            if (OccupiedCount != PlayerCount) return false;
            int[] positions = new int[PlayerCount];
            for (int player = 0; player < PlayerCount; player++) positions[player] = SeatOf(player);
            foreach (AttendanceRule rule in Round.Rules)
                if (!rule.Matches(positions)) return false;
            Solved = true;
            return true;
        }

        /// <summary>Không nhận tham số; làm trống 24 ghế, xóa kết quả và tăng Revision; giữ cùng đáp án.</summary>
        public void Reset()
        {
            for (int i = 0; i < seats.Length; i++) seats[i] = -1;
            Solved = false;
            OccupiedCount = 0;
            Revision++;
        }
    }
}
