/*
 * Mục đích: Sinh và kiểm tra mã phòng 4 ký tự (T-008). Bảng ký tự bỏ I, O, 0, 1 để người chơi không nhầm khi đọc mã cho nhau.
 * Hàm: Generate (sinh mã), Normalize (chuẩn hóa chữ người dùng nhập), IsValid (kiểm tra mã đã chuẩn hóa).
 */
using System;
using System.Text;

namespace TheReturn
{
    /// <summary>Tiện ích mã phòng.</summary>
    public static class PartyCode
    {
        public const int Length = 4;
        public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>Nhận bộ sinh số ngẫu nhiên (không null); trả mã mới gồm 4 ký tự trong bảng.</summary>
        public static string Generate(Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            var sb = new StringBuilder(Length);
            for (int i = 0; i < Length; i++) sb.Append(Alphabet[rng.Next(Alphabet.Length)]);
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi người dùng nhập; trả mã viết hoa, bỏ khoảng trắng, hoặc null nếu không hợp lệ.</summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string code = input.Replace(" ", "").Trim().ToUpperInvariant();
            return IsValid(code) ? code : null;
        }

        /// <summary>Nhận mã; trả true nếu đủ 4 ký tự và mọi ký tự nằm trong bảng.</summary>
        public static bool IsValid(string code)
        {
            if (code == null || code.Length != Length) return false;
            foreach (char c in code) if (Alphabet.IndexOf(c) < 0) return false;
            return true;
        }
    }
}
