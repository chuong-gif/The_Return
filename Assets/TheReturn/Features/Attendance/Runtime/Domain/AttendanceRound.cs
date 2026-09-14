/*
 * Mục đích: Biểu diễn đề điểm danh theo tọa độ 6 hàng × 4 cột, độc lập Unity.
 * Danh sách hàm:
 * - AttendanceRule: tạo một điều kiện về vị trí người chơi.
 * - Matches: kiểm tra điều kiện trên một phần hoặc toàn bộ cách xếp ghế.
 * - AttendanceRound: lưu đề, tên, seed và phân phối manh mối cho từng người.
 * - CopySolution: trả bản sao đáp án để kiểm thử, không dùng trên HUD.
 * - SeatCode: chuyển chỉ số ghế thành nhãn Hx-Cy.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    public enum AttendanceRuleKind
    {
        FixedRow, FixedColumn, Ahead, Behind, Left, Right,
        SameRow, SameColumn, RowParity, ColumnParity
    }

    public sealed class AttendanceRule
    {
        public AttendanceRuleKind Kind { get; }
        public int PersonA { get; }
        public int PersonB { get; }
        public int Value { get; }

        /// <summary>Tạo điều kiện từ loại, hai ID người và giá trị; không thay đổi trạng thái khác.</summary>
        public AttendanceRule(AttendanceRuleKind kind, int personA, int personB, int value)
        {
            Kind = kind;
            PersonA = personA;
            PersonB = personB;
            Value = value;
        }

        /// <summary>Nhận mảng ghế theo người (-1 là chưa gán); trả false khi điều kiện đã bị vi phạm.</summary>
        public bool Matches(int[] positions)
        {
            int a = positions[PersonA];
            if (a < 0) return true;
            int rowA = a / AttendanceRound.Columns;
            int columnA = a % AttendanceRound.Columns;
            switch (Kind)
            {
                case AttendanceRuleKind.FixedRow: return rowA == Value;
                case AttendanceRuleKind.FixedColumn: return columnA == Value;
                case AttendanceRuleKind.RowParity: return (rowA + 1) % 2 == Value;
                case AttendanceRuleKind.ColumnParity: return (columnA + 1) % 2 == Value;
            }

            int b = positions[PersonB];
            if (b < 0) return true;
            int rowB = b / AttendanceRound.Columns;
            int columnB = b % AttendanceRound.Columns;
            switch (Kind)
            {
                case AttendanceRuleKind.Ahead: return rowB - rowA == Value;
                case AttendanceRuleKind.Behind: return rowA - rowB == Value;
                case AttendanceRuleKind.Left: return columnB - columnA == Value;
                case AttendanceRuleKind.Right: return columnA - columnB == Value;
                case AttendanceRuleKind.SameRow: return rowA == rowB;
                case AttendanceRuleKind.SameColumn: return columnA == columnB;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    public sealed class AttendanceRound
    {
        public const int Rows = 6;
        public const int Columns = 4;
        public const int SeatCount = Rows * Columns;
        readonly int[] solution;
        public int Seed { get; }
        public int PlayerCount => Names.Count;
        public IReadOnlyList<string> Names { get; }
        public IReadOnlyList<AttendanceRule> Rules { get; }
        public IReadOnlyList<int> ClueOwners { get; }

        /// <summary>Nhận dữ liệu đề đã kiểm chứng; sao chép để bên gọi không sửa được đáp án và luật.</summary>
        public AttendanceRound(int seed, string[] names, int[] answer, List<AttendanceRule> rules, int[] owners)
        {
            Seed = seed;
            Names = Array.AsReadOnly((string[])names.Clone());
            solution = (int[])answer.Clone();
            Rules = new List<AttendanceRule>(rules).AsReadOnly();
            ClueOwners = Array.AsReadOnly((int[])owners.Clone());
        }

        /// <summary>Không nhận tham số; trả bản sao chỉ số ghế của mỗi người để kiểm thử hoặc host lưu đề.</summary>
        public int[] CopySolution()
        {
            return (int[])solution.Clone();
        }

        /// <summary>Nhận chỉ số 0–23; trả địa chỉ H1-C1 đến H6-C4, hoặc dấu gạch nếu không hợp lệ.</summary>
        public static string SeatCode(int seat)
        {
            if (seat < 0 || seat >= SeatCount) return "—";
            return "H" + (seat / Columns + 1) + "-C" + (seat % Columns + 1);
        }
    }
}
