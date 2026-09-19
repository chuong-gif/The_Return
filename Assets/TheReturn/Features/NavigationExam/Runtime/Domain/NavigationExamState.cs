/*
 * Mục đích: Luật sa bàn 3×3, lượt 2–4 người, ô cấm, cạnh một chiều và hai con dấu.
 * Hàm: constructor tạo đề; Cell / Code đổi tọa độ theo biến thể; OwnerOf chia manh mối;
 * Blocked đọc ô cấm; Move xử lý bước có xác thực lượt; Reset đặt quân và dấu về đầu.
 */
using System;
namespace TheReturn
{
    public enum ExamResult { Moved, Solved, WrongTurn, Outside, Blocked, OneWay, StampOrder, MissingStamps, Invalid }
    public sealed class NavigationExamState
    {
        public int PlayerCount { get; }
        public int Seed { get; }
        public int Position { get; private set; }
        public int Turn { get; private set; }
        public int Stamps { get; private set; }
        public int Steps { get; private set; }
        public bool Solved { get; private set; }
        public int Start => Cell(0);
        public int Goal => Cell(8);
        public int FirstStamp => Cell(4);
        public int SecondStamp => Cell(5);
        public int Variant => ((Seed % 4) + 4) % 4;

        /// <summary>Nhận đội 2–4 và seed; tạo biến thể đối xứng có cùng độ khó và lời giải.</summary>
        public NavigationExamState(int count, int seed = 0)
        {
            if (count < 2 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));
            PlayerCount = count;
            Seed = seed;
            Reset();
        }

        /// <summary>Nhận ô mẫu 0–8; trả ô sau đối xứng ngang/dọc theo seed, -1 nếu sai ID.</summary>
        public int Cell(int template)
        {
            if (template < 0 || template > 8) return -1;
            int x = template % 3, y = template / 3;
            if ((Variant & 1) != 0) x = 2 - x;
            if ((Variant & 2) != 0) y = 2 - y;
            return y * 3 + x;
        }

        /// <summary>Nhận ô 0–8; trả tọa độ A1–C3, hoặc dấu hỏi nếu không hợp lệ.</summary>
        public static string Code(int cell) => cell >= 0 && cell < 9 ? ((char)('A' + cell % 3)).ToString() + (cell / 3 + 1) : "?";

        /// <summary>Nhận manh mối 0–3; trả vai phụ trách theo vòng số người hoặc -1 nếu sai ID.</summary>
        public int OwnerOf(int clue) => clue >= 0 && clue < 4 ? clue % PlayerCount : -1;

        /// <summary>Nhận ô; trả true nếu ô thuộc một trong ba vị trí bị khóa của đề.</summary>
        public bool Blocked(int cell) => cell == Cell(3) || cell == Cell(7) || cell == Cell(2);

        /// <summary>Nhận người và vector bước nguyên; chỉ cho đi một ô ngang/dọc đúng lượt; sai không đổi lượt hoặc dấu.</summary>
        public ExamResult Move(int actor, int dx, int dy)
        {
            if (actor < 0 || actor >= PlayerCount) return ExamResult.Invalid;
            if (Solved) return ExamResult.Solved;
            if (actor != Turn) return ExamResult.WrongTurn;
            if (!((dx == 0 && (dy == -1 || dy == 1)) || (dy == 0 && (dx == -1 || dx == 1)))) return ExamResult.Invalid;
            int x = Position % 3 + dx, y = Position / 3 + dy;
            if (x < 0 || x > 2 || y < 0 || y > 2) return ExamResult.Outside;
            int next = y * 3 + x;
            if (Blocked(next)) return ExamResult.Blocked;
            if (Position == SecondStamp && next == FirstStamp) return ExamResult.OneWay;
            if (next == SecondStamp && Stamps == 0) return ExamResult.StampOrder;
            if (next == Goal && Stamps < 2) return ExamResult.MissingStamps;
            Position = next;
            if (next == FirstStamp && Stamps == 0) Stamps = 1;
            if (next == SecondStamp && Stamps == 1) Stamps = 2;
            Steps++;
            Solved = next == Goal;
            if (!Solved) Turn = (Turn + 1) % PlayerCount;
            return Solved ? ExamResult.Solved : ExamResult.Moved;
        }

        /// <summary>Không nhận tham số; trả quân, lượt, dấu và kết quả về trạng thái ban đầu của cùng đề.</summary>
        public void Reset()
        {
            Position = Start;
            Turn = 0;
            Stamps = Steps = 0;
            Solved = false;
        }
    }
}
