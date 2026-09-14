/*
 * Mục đích: Sinh đề thay đổi theo seed và số người, rồi chứng minh đề có duy nhất một nghiệm.
 * Danh sách hàm:
 * - Generate: chọn đáp án, sinh/rút gọn luật và chia manh mối cho 2–4 người.
 * - CreateCandidates: tạo luật đúng với đáp án dự kiến.
 * - CountSolutions: đếm nghiệm, dừng sớm ở giới hạn để tránh duyệt vô ích.
 * - Search: quay lui có kiểm tra ràng buộc trên các người đã gán ghế.
 * - Shuffle: xáo trộn tại chỗ bằng nguồn ngẫu nhiên xác định.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    public static class AttendanceGenerator
    {
        /// <summary>Nhận 2–4 tên và seed; trả đề duy nhất, mọi người giữ ít nhất một luật cần thiết.</summary>
        public static AttendanceRound Generate(string[] names, int seed)
        {
            if (names == null || names.Length < 2 || names.Length > 4)
                throw new ArgumentException("Attendance supports 2–4 participants.");
            if (new HashSet<string>(names).Count != names.Length)
                throw new ArgumentException("Participant names must be unique.");
            foreach (string name in names)
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is empty.");

            var random = new Random(seed);
            for (int attempt = 0; attempt < 64; attempt++)
            {
                var available = new List<int>();
                for (int seat = 0; seat < AttendanceRound.SeatCount; seat++) available.Add(seat);
                Shuffle(available, random);
                int[] answer = available.GetRange(0, names.Length).ToArray();
                var rules = CreateCandidates(answer, random);

                // Loại luật dư nhưng giữ một nghiệm. Luật tọa độ tuyệt đối được thử bỏ trước.
                for (int i = 0; i < rules.Count;)
                {
                    AttendanceRule removed = rules[i];
                    rules.RemoveAt(i);
                    if (CountSolutions(names.Length, rules, 2) != 1)
                    {
                        rules.Insert(i, removed);
                        i++;
                    }
                }
                if (rules.Count < names.Length) continue;

                Shuffle(rules, random);
                var owners = new int[rules.Count];
                int startingOwner = random.Next(names.Length);
                for (int i = 0; i < owners.Length; i++)
                    owners[i] = (startingOwner + i) % names.Length;
                return new AttendanceRound(seed, names, answer, rules, owners);
            }
            throw new InvalidOperationException("Could not generate a cooperative puzzle for this seed.");
        }

        /// <summary>Nhận đáp án và RNG; trả các luật tọa độ, quan hệ và chẵn/lẻ đều đúng với đáp án.</summary>
        static List<AttendanceRule> CreateCandidates(int[] answer, Random random)
        {
            var absolute = new List<AttendanceRule>();
            var relative = new List<AttendanceRule>();
            for (int a = 0; a < answer.Length; a++)
            {
                int row = answer[a] / AttendanceRound.Columns;
                int column = answer[a] % AttendanceRound.Columns;
                absolute.Add(new AttendanceRule(AttendanceRuleKind.FixedRow, a, -1, row));
                absolute.Add(new AttendanceRule(AttendanceRuleKind.FixedColumn, a, -1, column));
                relative.Add(new AttendanceRule(AttendanceRuleKind.RowParity, a, -1, (row + 1) % 2));
                relative.Add(new AttendanceRule(AttendanceRuleKind.ColumnParity, a, -1, (column + 1) % 2));
                for (int b = a + 1; b < answer.Length; b++)
                {
                    int rowDelta = row - answer[b] / AttendanceRound.Columns;
                    int columnDelta = column - answer[b] % AttendanceRound.Columns;
                    var rowKind = rowDelta == 0 ? AttendanceRuleKind.SameRow :
                        rowDelta < 0 ? AttendanceRuleKind.Ahead : AttendanceRuleKind.Behind;
                    var columnKind = columnDelta == 0 ? AttendanceRuleKind.SameColumn :
                        columnDelta < 0 ? AttendanceRuleKind.Left : AttendanceRuleKind.Right;
                    relative.Add(new AttendanceRule(rowKind, a, b, Math.Abs(rowDelta)));
                    relative.Add(new AttendanceRule(columnKind, a, b, Math.Abs(columnDelta)));
                }
            }
            Shuffle(absolute, random);
            Shuffle(relative, random);
            absolute.AddRange(relative);
            return absolute;
        }

        /// <summary>Nhận số người, luật và giới hạn dương; trả số nghiệm tối đa bằng giới hạn, không sinh text.</summary>
        public static int CountSolutions(int playerCount, IReadOnlyList<AttendanceRule> rules, int limit = 2)
        {
            if (playerCount < 2 || playerCount > 4 || limit < 1) throw new ArgumentOutOfRangeException();
            int[] positions = new int[playerCount];
            for (int i = 0; i < playerCount; i++) positions[i] = -1;
            return Search(0, positions, rules, 0u, limit);
        }

        /// <summary>Nhận phần cách xếp đang xét và bitmask ghế đã dùng; trả số nghiệm còn lại, dừng khi đủ.</summary>
        static int Search(int player, int[] positions, IReadOnlyList<AttendanceRule> rules, uint used, int limit)
        {
            if (player == positions.Length) return 1;
            int found = 0;
            for (int seat = 0; seat < AttendanceRound.SeatCount; seat++)
            {
                uint bit = 1u << seat;
                if ((used & bit) != 0) continue;
                positions[player] = seat;
                bool valid = true;
                foreach (AttendanceRule rule in rules)
                {
                    if (!rule.Matches(positions)) { valid = false; break; }
                }
                if (valid) found += Search(player + 1, positions, rules, used | bit, limit - found);
                if (found >= limit) break;
            }
            positions[player] = -1;
            return found;
        }

        /// <summary>Nhận danh sách và RNG; đổi thứ tự tại chỗ, không trả dữ liệu mới.</summary>
        static void Shuffle<T>(List<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T value = list[i];
                list[i] = list[j];
                list[j] = value;
            }
        }
    }
}
