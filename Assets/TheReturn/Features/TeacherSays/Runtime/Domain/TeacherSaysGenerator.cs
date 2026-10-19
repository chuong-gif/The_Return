/*
 * Mục đích: Sinh đề «Cô nói» (T-006c) theo seed và số người, xác định (cùng seed cùng đề).
 * Ràng buộc: 2-3 lệnh thật; có cả hành động đúng là BƯỚC và ĐỨNG; người nghe mỗi đoạn khác đoạn trước; mỗi người nghe ít nhất một lần.
 * Thuần C#. Nếu thử 1000 lần không ra đề hợp lệ (gần như không xảy ra) thì dùng đề cố định hợp lệ.
 * Hàm: Generate; IsValid; Fallback.
 */
using System;

namespace TheReturn
{
    /// <summary>Trình sinh đề cho Cô nói.</summary>
    public static class TeacherSaysGenerator
    {
        const int MaxAttempts = 1000;

        /// <summary>Nhận số người (2-4) và seed; trả đề hợp lệ, cùng đầu vào luôn ra cùng đề.</summary>
        public static TeacherSaysRound Generate(int playerCount, int seed)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            var rng = new Random(seed);
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var commands = new TeacherCommand[TeacherSaysRound.SegmentCount];
                var said = new bool[TeacherSaysRound.SegmentCount];
                var listeners = new int[TeacherSaysRound.SegmentCount];
                for (int i = 0; i < TeacherSaysRound.SegmentCount; i++)
                {
                    commands[i] = rng.Next(2) == 0 ? TeacherCommand.Step : TeacherCommand.Stand;
                    said[i] = rng.Next(2) == 0;
                    listeners[i] = rng.Next(playerCount);
                }
                var round = new TeacherSaysRound(playerCount, seed, commands, said, listeners);
                if (IsValid(round)) return round;
            }
            return Fallback(playerCount, seed);
        }

        /// <summary>Nhận đề; trả true nếu thỏa mọi ràng buộc ở đầu tệp.</summary>
        public static bool IsValid(TeacherSaysRound round)
        {
            if (round.RealCount < 2 || round.RealCount > 3) return false;
            int steps = round.RequiredStepCount;
            if (steps < 1 || steps > TeacherSaysRound.SegmentCount - 1) return false;
            var heard = new bool[round.PlayerCount];
            for (int i = 0; i < TeacherSaysRound.SegmentCount; i++)
            {
                if (i > 0 && round.Listener(i) == round.Listener(i - 1)) return false;
                heard[round.Listener(i)] = true;
            }
            foreach (bool h in heard) if (!h) return false;
            return true;
        }

        /// <summary>Nhận số người và seed; trả đề cố định hợp lệ (người nghe lần lượt theo vòng).</summary>
        static TeacherSaysRound Fallback(int playerCount, int seed)
        {
            var commands = new[] { TeacherCommand.Step, TeacherCommand.Stand, TeacherCommand.Step, TeacherCommand.Step, TeacherCommand.Stand };
            var said = new[] { true, true, false, false, true };
            var listeners = new int[TeacherSaysRound.SegmentCount];
            for (int i = 0; i < listeners.Length; i++) listeners[i] = i % playerCount;
            return new TeacherSaysRound(playerCount, seed, commands, said, listeners);
        }
    }
}
