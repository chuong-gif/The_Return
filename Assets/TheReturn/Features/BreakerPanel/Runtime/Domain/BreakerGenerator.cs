/*
 * Mục đích: Sinh đề «Bảng cầu dao» theo seed, bảo đảm LUÔN có đúng một cách bật công tắc đưa đèn về đích, cách đó không
 * vượt giới hạn quá tải và cần công tắc của ít nhất hai người khác nhau (T-006a). Thuần C#.
 * Hàm: Generate; IsInvertible.
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề cầu dao: ma trận công tắc-đèn khả nghịch trên GF(2) nên nghiệm duy nhất.</summary>
    public static class BreakerGenerator
    {
        /// <summary>
        /// Nhận số người (2-4) và seed; trả đề mới (cùng seed cho cùng đề). Số công tắc = số đèn = số người + 2;
        /// mỗi công tắc đảo 2-3 đèn; nghiệm dùng 2..OverloadLimit công tắc thuộc ít nhất hai chủ khác nhau.
        /// Ném InvalidOperationException nếu không sinh được trong số lần thử tối đa (không xảy ra với tham số hợp lệ).
        /// </summary>
        public static BreakerRound Generate(int playerCount, int seed)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            int size = playerCount + 2;
            int limit = (size + 1) / 2;
            var rng = new Random(seed);
            for (int attempt = 0; attempt < 20000; attempt++)
            {
                var masks = new int[size];
                for (int i = 0; i < size; i++)
                {
                    int want = 2 + rng.Next(2);
                    int bits = 0;
                    while (BreakerRound.CountBits(bits) < want) bits |= 1 << rng.Next(size);
                    masks[i] = bits;
                }
                if (!IsInvertible(masks)) continue;

                int k = 2 + rng.Next(limit - 1);
                int solution = 0;
                while (BreakerRound.CountBits(solution) < k) solution |= 1 << rng.Next(size);
                if (OwnerCount(solution, size, playerCount) < 2) continue;

                int start = rng.Next(1 << size);
                int target = start;
                for (int i = 0; i < size; i++)
                    if ((solution >> i & 1) == 1) target ^= masks[i];
                if (target == start) continue;
                return new BreakerRound(seed, playerCount, masks, start, target, solution);
            }
            throw new InvalidOperationException("Không sinh được đề cầu dao hợp lệ");
        }

        /// <summary>Nhận các mặt nạ; trả true nếu mọi tổ hợp công tắc khác rỗng đều đảo ít nhất một đèn (nên nghiệm duy nhất).</summary>
        public static bool IsInvertible(int[] masks)
        {
            int n = masks.Length;
            for (int s = 1; s < (1 << n); s++)
            {
                int r = 0;
                for (int i = 0; i < n; i++)
                    if ((s >> i & 1) == 1) r ^= masks[i];
                if (r == 0) return false;
            }
            return true;
        }

        /// <summary>Nhận tập công tắc, số công tắc và số người; trả số chủ khác nhau của các công tắc trong tập.</summary>
        static int OwnerCount(int set, int size, int players)
        {
            int owners = 0;
            for (int i = 0; i < size; i++)
                if ((set >> i & 1) == 1) owners |= 1 << (i % players);
            return BreakerRound.CountBits(owners);
        }
    }
}
