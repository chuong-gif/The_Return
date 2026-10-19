/*
 * Mục đích: Đề của một lượt «Cô nói» (T-006c): 5 đoạn, mỗi đoạn có lệnh (Bước/Đứng), cờ «Cô nói» (thật/giả) và người nghe được
 * tiền tố. Bất biến sau khi tạo. Lệnh thật làm đúng, lệnh giả làm NGƯỢC.
 * Thuần C#, không phụ thuộc UnityEngine.
 * Kiểu: TeacherCommand, TeacherSaysRound.
 * Hàm TeacherSaysRound: constructor; Command; TeacherSaid; Listener; RequiredAction; RealCount; RequiredStepCount.
 */
using System;

namespace TheReturn
{
    /// <summary>Lệnh phát ra hoặc hành động đội phải làm.</summary>
    public enum TeacherCommand
    {
        Step,
        Stand
    }

    /// <summary>Đề 5 đoạn của Cô nói.</summary>
    public sealed class TeacherSaysRound
    {
        public const int SegmentCount = 5;

        readonly TeacherCommand[] commands;
        readonly bool[] teacherSaid;
        readonly int[] listeners;

        public int PlayerCount { get; }
        public int Seed { get; }

        /// <summary>Nhận số người (2-4), seed và ba mảng độ dài 5: lệnh, cờ «Cô nói», người nghe (0..playerCount-1). Sai kích thước hoặc phạm vi thì ném lỗi.</summary>
        public TeacherSaysRound(int playerCount, int seed, TeacherCommand[] commands, bool[] teacherSaid, int[] listeners)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (commands == null || commands.Length != SegmentCount) throw new ArgumentException(nameof(commands));
            if (teacherSaid == null || teacherSaid.Length != SegmentCount) throw new ArgumentException(nameof(teacherSaid));
            if (listeners == null || listeners.Length != SegmentCount) throw new ArgumentException(nameof(listeners));
            foreach (int l in listeners)
                if (l < 0 || l >= playerCount) throw new ArgumentOutOfRangeException(nameof(listeners));
            PlayerCount = playerCount;
            Seed = seed;
            this.commands = (TeacherCommand[])commands.Clone();
            this.teacherSaid = (bool[])teacherSaid.Clone();
            this.listeners = (int[])listeners.Clone();
        }

        /// <summary>Nhận chỉ số đoạn 0-4; trả lệnh loa phát ra (ai cũng thấy).</summary>
        public TeacherCommand Command(int segment) { return commands[Check(segment)]; }

        /// <summary>Nhận chỉ số đoạn; trả true nếu lệnh có «Cô nói» (lệnh thật). Chỉ người nghe của đoạn đó được biết.</summary>
        public bool TeacherSaid(int segment) { return teacherSaid[Check(segment)]; }

        /// <summary>Nhận chỉ số đoạn; trả người chơi (0-3) nghe được tiền tố ở đoạn đó.</summary>
        public int Listener(int segment) { return listeners[Check(segment)]; }

        /// <summary>Nhận chỉ số đoạn; trả hành động đội PHẢI làm: lệnh thật giữ nguyên, lệnh giả đảo ngược.</summary>
        public TeacherCommand RequiredAction(int segment)
        {
            int s = Check(segment);
            return (commands[s] == TeacherCommand.Step) == teacherSaid[s] ? TeacherCommand.Step : TeacherCommand.Stand;
        }

        /// <summary>Không nhận tham số; trả số đoạn có «Cô nói» (lệnh thật).</summary>
        public int RealCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < SegmentCount; i++) if (teacherSaid[i]) n++;
                return n;
            }
        }

        /// <summary>Không nhận tham số; trả số đoạn mà hành động đúng là BƯỚC TỚI.</summary>
        public int RequiredStepCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < SegmentCount; i++) if (RequiredAction(i) == TeacherCommand.Step) n++;
                return n;
            }
        }

        /// <summary>Nhận chỉ số đoạn; trả lại nếu hợp lệ, ngược lại ném lỗi (nội bộ).</summary>
        static int Check(int segment)
        {
            if (segment < 0 || segment >= SegmentCount) throw new ArgumentOutOfRangeException(nameof(segment));
            return segment;
        }
    }
}
