/*
 * Mục đích: Lõi điều phối THỜI GIAN THỰC của «Cầu ván và thẻ từ» (T-006b2): bọc PlankState, biến mỗi lượt qua cầu thành một đoạn
 * thời gian (chi phí đơn vị x giây mỗi đơn vị), chặn lượt mới khi đang qua cầu, báo khi nhóm đến nơi và tính đồng hồ HUD.
 * Đồng hồ chỉ chạy khi có lượt qua cầu (quyết định chủ dự án 2026-10-16). Thuần C#, không phụ thuộc Unity; Presentation gọi Tick mỗi khung.
 * Hàm: constructor; TryStartCrossing (2 dạng); Tick; Reset; CrossingGroupCopy; LastCrossingGroupCopy.
 */
using System;

namespace TheReturn
{
    /// <summary>Pha hiện tại của phiên cầu ván.</summary>
    public enum PlankPhase { Idle, Crossing }

    /// <summary>Kết quả yêu cầu bắt đầu một lượt qua cầu.</summary>
    public enum PlankStartResult
    {
        /// <summary>Lượt đã bắt đầu; nhóm đang qua cầu.</summary>
        Started,
        /// <summary>Đang có lượt qua cầu khác chưa đến nơi.</summary>
        Busy,
        /// <summary>Lượt này làm vượt hạn: thất bại, lượt không bắt đầu.</summary>
        TimedOut,
        /// <summary>Chỉ số người sai hoặc trùng.</summary>
        InvalidPlayer,
        /// <summary>Hai người ở hai bờ khác nhau.</summary>
        WrongBank,
        /// <summary>Thẻ không ở bờ của nhóm.</summary>
        CardNotWithGroup,
        /// <summary>Nhiệm vụ đã thắng hoặc thất bại; cần Reset.</summary>
        Finished
    }

    /// <summary>Phiên chơi cầu ván có thời gian thực trên một đề.</summary>
    public sealed class PlankSession
    {
        readonly PlankState state;
        readonly float secondsPerUnit;
        int[] crossingGroup = new int[0];
        int[] lastGroup = new int[0];

        /// <summary>Trạng thái luật bên dưới (chỉ đọc; đổi qua phiên này).</summary>
        public PlankState State { get { return state; } }
        /// <summary>Số giây thật cho mỗi đơn vị thời gian của đề.</summary>
        public float SecondsPerUnit { get { return secondsPerUnit; } }
        /// <summary>Đang rảnh hay đang qua cầu.</summary>
        public PlankPhase Phase { get; private set; }
        /// <summary>Tổng giây của lượt đang qua cầu (0 khi rảnh).</summary>
        public float CrossingSecondsTotal { get; private set; }
        /// <summary>Số giây còn lại của lượt đang qua cầu (0 khi rảnh).</summary>
        public float CrossingSecondsLeft { get; private set; }
        /// <summary>Bờ mà nhóm đang qua cầu tới (chỉ có nghĩa khi Phase là Crossing).</summary>
        public PlankBank CrossingTo { get; private set; }
        /// <summary>Hạn tổng tính bằng giây.</summary>
        public float DeadlineSeconds { get { return state.Round.DeadlineUnits * secondsPerUnit; } }
        /// <summary>Số giây qua cầu đã dùng, tăng liên tục trong lúc qua cầu (đồng hồ HUD).</summary>
        public float DisplayUsedSeconds
        {
            get
            {
                float used = state.UsedUnits * secondsPerUnit - CrossingSecondsLeft;
                if (used < 0f) used = 0f;
                float max = DeadlineSeconds;
                return used > max ? max : used;
            }
        }
        /// <summary>Số giây còn lại đến hạn theo đồng hồ HUD.</summary>
        public float DisplayRemainingSeconds { get { return DeadlineSeconds - DisplayUsedSeconds; } }
        /// <summary>True khi cả đội đã qua VÀ lượt cuối đã đến nơi.</summary>
        public bool Solved { get { return state.Status == PlankStatus.Won && Phase == PlankPhase.Idle; } }
        /// <summary>True khi nhiệm vụ đã thất bại (hết hạn); cần Reset để thử lại.</summary>
        public bool Failed { get { return state.Status == PlankStatus.Failed; } }
        /// <summary>True khi giải xong và an toàn bàn giao (không còn nhóm đang qua cầu).</summary>
        public bool CanHandOff { get { return Solved; } }

        /// <summary>
        /// Nhận đề và số giây thật mỗi đơn vị (> 0, hữu hạn); tạo phiên rảnh ở trạng thái đầu.
        /// Ném ArgumentNullException nếu đề null, ArgumentOutOfRangeException nếu số giây sai.
        /// </summary>
        public PlankSession(PlankRound round, float secondsPerUnit)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            if (!(secondsPerUnit > 0f) || float.IsInfinity(secondsPerUnit)) throw new ArgumentOutOfRangeException(nameof(secondsPerUnit));
            state = new PlankState(round);
            this.secondsPerUnit = secondsPerUnit;
            Phase = PlankPhase.Idle;
        }

        /// <summary>Nhận một người; thử bắt đầu lượt qua cầu một mình (mang thẻ).</summary>
        public PlankStartResult TryStartCrossing(int player)
        {
            return TryStartCrossing(player, PlankState.NoPlayer);
        }

        /// <summary>
        /// Nhận người thứ nhất và thứ hai (PlankState.NoPlayer nếu một mình); kiểm luật qua PlankState rồi bắt đầu đếm thời gian
        /// của lượt (chi phí đơn vị x giây mỗi đơn vị). Trả kết quả; chỉ Started mới đổi pha sang Crossing.
        /// </summary>
        public PlankStartResult TryStartCrossing(int playerA, int playerB)
        {
            if (Phase == PlankPhase.Crossing) return PlankStartResult.Busy;
            int before = state.UsedUnits;
            PlankResult r = state.TryCross(playerA, playerB);
            switch (r)
            {
                case PlankResult.Crossed:
                case PlankResult.Won:
                    break;
                case PlankResult.TimedOut: return PlankStartResult.TimedOut;
                case PlankResult.InvalidPlayer: return PlankStartResult.InvalidPlayer;
                case PlankResult.WrongBank: return PlankStartResult.WrongBank;
                case PlankResult.CardNotWithGroup: return PlankStartResult.CardNotWithGroup;
                default: return PlankStartResult.Finished;
            }
            int cost = state.UsedUnits - before;
            crossingGroup = playerB == PlankState.NoPlayer ? new[] { playerA } : new[] { playerA, playerB };
            CrossingTo = state.CardBank;
            CrossingSecondsTotal = cost * secondsPerUnit;
            CrossingSecondsLeft = CrossingSecondsTotal;
            Phase = PlankPhase.Crossing;
            return PlankStartResult.Started;
        }

        /// <summary>
        /// Nhận số giây trôi qua (>= 0); giảm thời gian lượt đang qua cầu. Trả true ĐÚNG MỘT LẦN khi nhóm vừa đến nơi
        /// (nhóm đó đọc bằng LastCrossingGroupCopy); các trường hợp khác trả false. Ném ArgumentOutOfRangeException nếu số giây âm.
        /// </summary>
        public bool Tick(float seconds)
        {
            if (seconds < 0f) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Phase != PlankPhase.Crossing) return false;
            CrossingSecondsLeft -= seconds;
            if (CrossingSecondsLeft > 0f) return false;
            CrossingSecondsLeft = 0f;
            Phase = PlankPhase.Idle;
            lastGroup = crossingGroup;
            crossingGroup = new int[0];
            return true;
        }

        /// <summary>Làm lại cùng đề: hủy lượt đang qua cầu, đưa luật và đồng hồ về đầu.</summary>
        public void Reset()
        {
            state.Reset();
            Phase = PlankPhase.Idle;
            CrossingSecondsTotal = 0f;
            CrossingSecondsLeft = 0f;
            crossingGroup = new int[0];
            lastGroup = new int[0];
        }

        /// <summary>Trả bản sao danh sách người đang qua cầu (rỗng khi rảnh).</summary>
        public int[] CrossingGroupCopy()
        {
            return (int[])crossingGroup.Clone();
        }

        /// <summary>Trả bản sao danh sách người của lượt vừa đến nơi gần nhất (rỗng nếu chưa có).</summary>
        public int[] LastCrossingGroupCopy()
        {
            return (int[])lastGroup.Clone();
        }
    }
}
