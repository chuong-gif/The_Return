/*
 * Mục đích: Sinh hồ sơ sửa điểm theo seed; vị trí hồ sơ đổi nhưng luôn có lời giải hợp lệ.
 * Hàm: constructor sinh mã/điểm/đích chứng từ; OwnerOf phân tài liệu cho đội;
 * CorrectTarget / CorrectSource đọc đáp án để bộ kiểm tra đối chiếu.
 */
using System;
namespace TheReturn
{
    public sealed class GradeRepairRound
    {
        public int Seed { get; }
        public int PlayerCount { get; }
        public readonly string[] Codes;
        public readonly int[] InitialScores;
        readonly int[] roles;

        /// <summary>Nhận đội 2–4 và seed; tạo bốn hồ sơ, hai phiếu +2 và một biên bản chuyển 1 điểm.</summary>
        public GradeRepairRound(int count, int seed)
        {
            if (count < 2 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));
            PlayerCount = count;
            Seed = seed;
            var random = new Random(seed);
            roles = new[] { 0, 1, 2, 3 };
            for (int i = roles.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int value = roles[i]; roles[i] = roles[j]; roles[j] = value;
            }
            Codes = new string[4];
            InitialScores = new int[4];
            int[] scores = { 4, 7, 8, 9 };
            int prefix = random.Next(10, 90);
            for (int i = 0; i < 4; i++)
            {
                InitialScores[roles[i]] = scores[i];
                Codes[roles[i]] = "HS-" + prefix + (i + 1);
            }
        }

        /// <summary>Nhận tài liệu 0–3; trả vai phụ trách theo vòng 2–4 người, hoặc -1 nếu sai ID.</summary>
        public int OwnerOf(int document) => document >= 0 && document < 4 ? document % PlayerCount : -1;

        /// <summary>Nhận phiếu 0/1/2; trả hồ sơ đích, hoặc -1 nếu không có phiếu.</summary>
        public int CorrectTarget(int voucher) => voucher == 0 ? roles[0] : voucher == 1 ? roles[3] : voucher == 2 ? roles[1] : -1;

        /// <summary>Nhận phiếu; trả hồ sơ nguồn cho biên bản chuyển điểm, -1 với phiếu bổ sung.</summary>
        public int CorrectSource(int voucher) => voucher == 2 ? roles[3] : -1;
    }
}
