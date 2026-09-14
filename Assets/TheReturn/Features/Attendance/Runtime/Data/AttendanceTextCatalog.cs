/*
 * Mục đích: Asset chỉnh tên, lời hướng dẫn và các biến thể text manh mối trong Inspector.
 * Danh sách hàm:
 * - InitializeDefaults: điền bộ text tiếng Việt cho asset mới.
 * - ValidateCatalog: kiểm tra đủ loại luật và đúng placeholder.
 * - RequiredTokens: cho biết token cần có theo loại luật.
 * - BuildPackets: ghép các manh mối riêng của từng người từ đề và seed.
 * - Format: điền tên/tọa độ vào một biến thể text.
 */
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheReturn
{
    [Serializable]
    public sealed class AttendanceTextEntry
    {
        public AttendanceRuleKind kind;
        [TextArea(2, 4)] public string[] variants;
    }

    [CreateAssetMenu(menuName = "The Return/Attendance/Text Catalog", fileName = "AttendanceText_VI")]
    public sealed class AttendanceTextCatalog : ScriptableObject
    {
        [Header("Tên nhân vật dùng cho tối đa 4 người")]
        public string[] roleNames = { "Minh", "An", "Bình", "Chi" };
        [Header("Nội dung người chơi đọc")]
        public string title = "Điểm danh sai người";
        [TextArea(2, 5)] public string introduction = "Ghế có địa chỉ Hàng–Cột. Hàng 1 gần bảng, cột 1 ở bên trái khi nhìn lên bảng. Đọc manh mối của từng vai rồi tìm chỗ ngồi.";
        public string waiting = "Cần ít nhất 2 người để bắt đầu.";
        public string ready = "Đề mới đã sẵn sàng. Hãy chia sẻ manh mối của từng vai.";
        public string wrong = "Chưa đúng. Đối chiếu manh mối rồi đứng dậy đổi chỗ.";
        public string success = "Điểm danh hợp lệ. Đứng dậy và đi qua cửa EXIT.";
        [TextArea(2, 4)] public string[] hints = {
            "Tìm các manh mối chỉ rõ hàng, cột hoặc chẵn/lẻ trước.",
            "Trước/sau là hướng về bảng/xa bảng. Trái/phải được tính khi nhìn lên bảng.",
            "Chuyển từng quan hệ thành khoảng cách hàng/cột; đừng đoán chỗ từ lượt trước."
        };
        [Header("Mỗi loại có nhiều câu cùng nghĩa; chỉ sửa lời văn, giữ đúng các token")]
        public AttendanceTextEntry[] entries;

        /// <summary>Không nhận tham số; điền 30 mẫu tiếng Việt cho asset mới, không gọi lên asset người dùng đã sửa.</summary>
        public void InitializeDefaults()
        {
            entries = new[] {
                new AttendanceTextEntry { kind=AttendanceRuleKind.FixedRow, variants=new[]{
                    "{a} ngồi ở hàng {row}.", "Tên {a} thuộc hàng {row}.", "Sổ điểm danh ghi {a}: hàng {row}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.FixedColumn, variants=new[]{
                    "{a} ngồi ở cột {column}.", "Cột {column} là cột của {a}.", "Hồ sơ xếp {a} vào cột {column}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.Ahead, variants=new[]{
                    "{a} ở trước {b} đúng {steps} hàng.", "Hàng của {a} gần bảng hơn hàng của {b} đúng {steps} hàng.", "Từ hàng của {b}, đi {steps} hàng về phía bảng sẽ tới hàng của {a}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.Behind, variants=new[]{
                    "{a} ở sau {b} đúng {steps} hàng.", "Hàng của {a} xa bảng hơn hàng của {b} đúng {steps} hàng.", "Từ hàng của {b}, đi xa bảng {steps} hàng sẽ tới hàng của {a}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.Left, variants=new[]{
                    "{a} ở bên trái {b} đúng {steps} cột.", "Cột của {a} có số nhỏ hơn cột của {b} đúng {steps}.", "Từ cột của {b}, dịch trái {steps} cột sẽ tới cột của {a}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.Right, variants=new[]{
                    "{a} ở bên phải {b} đúng {steps} cột.", "Cột của {a} có số lớn hơn cột của {b} đúng {steps}.", "Từ cột của {b}, dịch phải {steps} cột sẽ tới cột của {a}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.SameRow, variants=new[]{
                    "{a} và {b} cùng hàng.", "Hàng của {a} cũng là hàng của {b}.", "Sổ lớp xếp {a} cùng hàng với {b}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.SameColumn, variants=new[]{
                    "{a} và {b} cùng cột.", "Cột của {a} cũng là cột của {b}.", "Sổ lớp xếp {a} cùng cột với {b}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.RowParity, variants=new[]{
                    "{a} ngồi ở hàng mang số {parity}.", "Số hàng của {a} là số {parity}.", "Kiểm tra {a} trong các hàng có số {parity}."}},
                new AttendanceTextEntry { kind=AttendanceRuleKind.ColumnParity, variants=new[]{
                    "{a} ngồi ở cột mang số {parity}.", "Số cột của {a} là số {parity}.", "Kiểm tra {a} trong các cột có số {parity}."}}
            };
        }

        /// <summary>Không nhận dữ liệu ngoài asset; trả bool và lỗi dễ đọc nếu thiếu loại luật, câu hoặc token.</summary>
        public bool ValidateCatalog(out string error)
        {
            error = "";
            if (roleNames == null || roleNames.Length < 4) { error = "Cần ít nhất 4 tên."; return false; }
            var names = new HashSet<string>();
            foreach (string name in roleNames)
                if (string.IsNullOrWhiteSpace(name) || !names.Add(name)) { error = "Tên phải khác nhau và không trống."; return false; }
            if (entries == null) { error = "Chưa có mẫu text."; return false; }
            foreach (AttendanceRuleKind kind in Enum.GetValues(typeof(AttendanceRuleKind)))
            {
                AttendanceTextEntry found = null;
                foreach (AttendanceTextEntry entry in entries)
                {
                    if (entry == null || entry.kind != kind) continue;
                    if (found != null) { error = "Trùng loại luật: " + kind; return false; }
                    found = entry;
                }
                if (found == null || found.variants == null || found.variants.Length == 0)
                { error = "Thiếu biến thể: " + kind; return false; }
                foreach (string template in found.variants)
                {
                    if (string.IsNullOrWhiteSpace(template)) { error = "Có câu trống: " + kind; return false; }
                    string remainder = template;
                    foreach (string token in RequiredTokens(kind))
                    {
                        if (!template.Contains(token)) { error = kind + " thiếu " + token; return false; }
                        remainder = remainder.Replace(token, "");
                    }
                    if (remainder.Contains("{") || remainder.Contains("}"))
                    { error = "Token không hợp lệ trong " + kind; return false; }
                }
            }
            return true;
        }

        /// <summary>Nhận loại luật; trả danh sách placeholder bắt buộc để người viết text giữ đủ nghĩa.</summary>
        public static string[] RequiredTokens(AttendanceRuleKind kind)
        {
            switch (kind)
            {
                case AttendanceRuleKind.FixedRow: return new[] { "{a}", "{row}" };
                case AttendanceRuleKind.FixedColumn: return new[] { "{a}", "{column}" };
                case AttendanceRuleKind.RowParity:
                case AttendanceRuleKind.ColumnParity: return new[] { "{a}", "{parity}" };
                case AttendanceRuleKind.SameRow:
                case AttendanceRuleKind.SameColumn: return new[] { "{a}", "{b}" };
                default: return new[] { "{a}", "{b}", "{steps}" };
            }
        }

        /// <summary>Nhận đề; trả một gói text cho mỗi người, chọn biến thể xác định từ seed của đề.</summary>
        public string[] BuildPackets(AttendanceRound round)
        {
            string error;
            if (!ValidateCatalog(out error)) throw new InvalidOperationException(error);
            var random = new System.Random(round.Seed ^ 17321);
            var builders = new StringBuilder[round.PlayerCount];
            for (int i = 0; i < builders.Length; i++) builders[i] = new StringBuilder();
            for (int i = 0; i < round.Rules.Count; i++)
            {
                int owner = round.ClueOwners[i];
                if (builders[owner].Length > 0) builders[owner].Append("\n\n");
                builders[owner].Append("• ").Append(Format(round.Rules[i], round.Names, random));
            }
            var packets = new string[builders.Length];
            for (int i = 0; i < packets.Length; i++) packets[i] = builders[i].ToString();
            return packets;
        }

        /// <summary>Nhận luật, tên và RNG; trả câu đã điền token, không tiết lộ toàn bộ đáp án.</summary>
        string Format(AttendanceRule rule, IReadOnlyList<string> names, System.Random random)
        {
            AttendanceTextEntry entry = Array.Find(entries, item => item.kind == rule.Kind);
            string text = entry.variants[random.Next(entry.variants.Length)];
            return text.Replace("{a}", names[rule.PersonA])
                .Replace("{b}", rule.PersonB >= 0 ? names[rule.PersonB] : "")
                .Replace("{row}", (rule.Value + 1).ToString())
                .Replace("{column}", (rule.Value + 1).ToString())
                .Replace("{steps}", rule.Value.ToString())
                .Replace("{parity}", rule.Value == 0 ? "chẵn" : "lẻ");
        }
    }
}
