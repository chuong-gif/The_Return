/*
 * Mục đích: Dữ liệu một đề của nhiệm vụ Phong_5 «Bảng cầu dao» (T-006a): bao nhiêu công tắc/đèn, mỗi công tắc đảo
 * những đèn nào, ai là chủ công tắc, trạng thái đèn ban đầu và đích. Thuần C#, không phụ thuộc Unity.
 * Đèn/công tắc được đánh số từ 0; tập đèn và tập công tắc biểu diễn bằng mặt nạ bit (bit i = đèn/công tắc i).
 * Hàm: constructor; SwitchMask; OwnerOf; Combine; CountBits.
 */
using System;

namespace TheReturn
{
    /// <summary>Một đề cầu dao: S công tắc, S đèn, mỗi công tắc đảo một tập đèn cố định.</summary>
    public sealed class BreakerRound
    {
        readonly int[] switchMasks;

        public int Seed { get; }
        public int PlayerCount { get; }
        /// <summary>Số công tắc, bằng số đèn.</summary>
        public int Size { get; }
        /// <summary>Số công tắc tối đa được BẬT cùng lúc; vượt quá thì cầu dao nhảy.</summary>
        public int OverloadLimit { get; }
        public int StartLamps { get; }
        public int TargetLamps { get; }
        /// <summary>Tập công tắc cần bật để từ trạng thái đầu ra đích (chỉ host/test được dùng, không hiện cho người chơi).</summary>
        public int SolutionSwitches { get; }

        /// <summary>
        /// Nhận seed, số người (2-4), mặt nạ đèn của từng công tắc, trạng thái đèn đầu, đích và tập nghiệm; kiểm tính nhất quán.
        /// Ném ArgumentException nếu kích thước sai hoặc nghiệm không đưa trạng thái đầu về đích.
        /// </summary>
        public BreakerRound(int seed, int playerCount, int[] switchMasks, int startLamps, int targetLamps, int solutionSwitches)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (switchMasks == null || switchMasks.Length != playerCount + 2)
                throw new ArgumentException("Số công tắc phải bằng số người + 2", nameof(switchMasks));
            Seed = seed;
            PlayerCount = playerCount;
            Size = switchMasks.Length;
            OverloadLimit = (Size + 1) / 2;
            this.switchMasks = (int[])switchMasks.Clone();
            StartLamps = startLamps;
            TargetLamps = targetLamps;
            SolutionSwitches = solutionSwitches;
            if ((startLamps ^ Combine(solutionSwitches)) != targetLamps)
                throw new ArgumentException("Tập nghiệm không đưa trạng thái đầu về đích", nameof(solutionSwitches));
        }

        /// <summary>Nhận chỉ số công tắc; trả mặt nạ đèn mà công tắc đó đảo (ngoài khoảng trả 0).</summary>
        public int SwitchMask(int sw) { return sw >= 0 && sw < Size ? switchMasks[sw] : 0; }

        /// <summary>Nhận chỉ số công tắc; trả chỉ số người chơi là chủ công tắc (công tắc i thuộc người i % số người).</summary>
        public int OwnerOf(int sw) { return sw % PlayerCount; }

        /// <summary>Nhận tập công tắc; trả mặt nạ đèn bị đảo (XOR các công tắc trong tập).</summary>
        public int Combine(int switchSet)
        {
            int result = 0;
            for (int i = 0; i < Size; i++)
                if ((switchSet >> i & 1) == 1) result ^= switchMasks[i];
            return result;
        }

        /// <summary>Nhận số nguyên; trả số bit 1.</summary>
        public static int CountBits(int value)
        {
            int c = 0;
            while (value != 0) { c += value & 1; value >>= 1; }
            return c;
        }
    }
}
