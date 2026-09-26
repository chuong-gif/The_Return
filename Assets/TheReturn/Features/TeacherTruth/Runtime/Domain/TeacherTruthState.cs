/*
 * Mục đích: Luật thuần C# cho bốn kênh giáo viên mâu thuẫn và cửa sổ xác nhận thủ công 2–4 người.
 * Hàm: constructor tạo phiên; AnnouncementVariant chọn bộ thoại; Hear / HasHeard ghi phụ đề riêng;
 * Confirm xác nhận một kênh và kiểm đủ đội; Tick đếm cửa sổ; ResetConfirmations / Reset đặt lại trạng thái.
 */
using System;

namespace TheReturn
{
    public enum ManualConfirmResult
    {
        Confirmed,
        AlreadyConfirmed,
        NeedEveryPlayer,
        Solved,
        Invalid
    }

    public sealed class TeacherTruthState
    {
        public const int ChannelCount = 4;
        public int PlayerCount { get; }
        public int Seed { get; }
        public int AnnouncementVariant => ((Seed % 3) + 3) % 3;
        public bool Solved { get; private set; }
        public bool WindowActive { get; private set; }
        public float Remaining { get; private set; }
        public int ConfirmedCount { get; private set; }
        public int ParticipantCount
        {
            get
            {
                int count = 0;
                for (int actor = 0; actor < PlayerCount; actor++)
                    if (Participated(actor)) count++;
                return count;
            }
        }

        readonly bool[,] heard;
        readonly int[] confirmedBy;
        readonly float windowSeconds;

        /// <summary>Nhận đội 2–4, seed và cửa sổ dương; tạo phiên chưa nghe/chưa xác nhận.</summary>
        public TeacherTruthState(int playerCount, int seed, float confirmationWindow = 3f)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException(nameof(playerCount));
            if (confirmationWindow <= 0 || float.IsNaN(confirmationWindow) || float.IsInfinity(confirmationWindow))
                throw new ArgumentOutOfRangeException(nameof(confirmationWindow));
            PlayerCount = playerCount;
            Seed = seed;
            windowSeconds = confirmationWindow;
            heard = new bool[playerCount, ChannelCount];
            confirmedBy = new int[ChannelCount];
            Reset();
        }

        /// <summary>Nhận người nghe và kênh; ghi phụ đề riêng, trả false nếu ID sai hoặc đã giải.</summary>
        public bool Hear(int actor, int channel)
        {
            if (Solved || actor < 0 || actor >= PlayerCount || channel < 0 || channel >= ChannelCount) return false;
            heard[actor, channel] = true;
            return true;
        }

        /// <summary>Nhận người nghe và kênh; trả true nếu người đó đã nghe thông báo tương ứng.</summary>
        public bool HasHeard(int actor, int channel)
        {
            return actor >= 0 && actor < PlayerCount && channel >= 0 && channel < ChannelCount && heard[actor, channel];
        }

        /// <summary>Nhận kênh; trả ID người xác nhận, hoặc -1 nếu kênh chưa được xác nhận.</summary>
        public int ConfirmedBy(int channel)
        {
            return channel >= 0 && channel < ChannelCount ? confirmedBy[channel] : -1;
        }

        /// <summary>Nhận người; trả true nếu người đó đã xác nhận ít nhất một kênh trong cửa sổ hiện tại.</summary>
        public bool Participated(int actor)
        {
            if (actor < 0 || actor >= PlayerCount) return false;
            for (int channel = 0; channel < ChannelCount; channel++)
                if (confirmedBy[channel] == actor) return true;
            return false;
        }

        /// <summary>Nhận người và kênh; chốt kênh trong cửa sổ, yêu cầu đủ bốn kênh và mọi người đều tham gia.</summary>
        public ManualConfirmResult Confirm(int actor, int channel)
        {
            if (Solved) return ManualConfirmResult.Solved;
            if (actor < 0 || actor >= PlayerCount || channel < 0 || channel >= ChannelCount)
                return ManualConfirmResult.Invalid;
            if (confirmedBy[channel] >= 0) return ManualConfirmResult.AlreadyConfirmed;
            if (!WindowActive)
            {
                WindowActive = true;
                Remaining = windowSeconds;
            }
            confirmedBy[channel] = actor;
            ConfirmedCount++;
            if (ConfirmedCount < ChannelCount) return ManualConfirmResult.Confirmed;
            if (ParticipantCount < PlayerCount)
            {
                ResetConfirmations();
                return ManualConfirmResult.NeedEveryPlayer;
            }
            Solved = true;
            WindowActive = false;
            Remaining = 0;
            return ManualConfirmResult.Solved;
        }

        /// <summary>Nhận deltaTime; giảm thời gian khi đang xác nhận, trả true đúng frame cửa sổ hết hạn và reset nút.</summary>
        public bool Tick(float deltaTime)
        {
            if (!WindowActive || Solved || deltaTime <= 0 || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                return false;
            Remaining = Math.Max(0, Remaining - deltaTime);
            if (Remaining > 0) return false;
            ResetConfirmations();
            return true;
        }

        /// <summary>Không nhận tham số; bỏ các xác nhận và đồng hồ, giữ phụ đề đã nghe.</summary>
        public void ResetConfirmations()
        {
            for (int i = 0; i < confirmedBy.Length; i++) confirmedBy[i] = -1;
            ConfirmedCount = 0;
            Remaining = 0;
            WindowActive = false;
        }

        /// <summary>Không nhận tham số; đặt lại kết quả, phụ đề và xác nhận của cùng biến thể.</summary>
        public void Reset()
        {
            Solved = false;
            Array.Clear(heard, 0, heard.Length);
            ResetConfirmations();
        }
    }
}
