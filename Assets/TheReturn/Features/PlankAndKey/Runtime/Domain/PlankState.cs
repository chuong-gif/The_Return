/*
 * Mục đích: Trạng thái và luật «Cầu ván và thẻ từ» (T-006b): ván chứa tối đa 2 người, thẻ phải đi cùng nhóm qua cầu, mỗi lượt tốn
 * thời gian của người chậm nhất, vượt hạn thì thất bại, cả đội qua bờ xa thì thắng. Host quyết định; Presentation chỉ hiển thị. Thuần C#.
 * Hàm: constructor; BankOf; TryCross (2 dạng); Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Bờ khe: gần (đầu hành lang) hoặc xa (có cổng).</summary>
    public enum PlankBank { Near, Far }

    /// <summary>Tình trạng nhiệm vụ.</summary>
    public enum PlankStatus { Playing, Won, Failed }

    /// <summary>Kết quả một lượt TryCross.</summary>
    public enum PlankResult
    {
        /// <summary>Qua cầu thành công, nhiệm vụ chưa xong.</summary>
        Crossed,
        /// <summary>Qua cầu thành công và cả đội đã ở bờ xa: thắng.</summary>
        Won,
        /// <summary>Lượt này làm vượt hạn: thất bại, lượt KHÔNG được thực hiện.</summary>
        TimedOut,
        /// <summary>Chỉ số người sai, trùng người hoặc số người không hợp lệ.</summary>
        InvalidPlayer,
        /// <summary>Hai người đang ở hai bờ khác nhau.</summary>
        WrongBank,
        /// <summary>Thẻ không nằm ở bờ của nhóm nên ván không hạ xuống.</summary>
        CardNotWithGroup,
        /// <summary>Nhiệm vụ đã thắng hoặc thất bại; cần Reset.</summary>
        Finished
    }

    /// <summary>Trạng thái một lần chơi cầu ván trên một đề.</summary>
    public sealed class PlankState
    {
        /// <summary>Giá trị cho tham số người thứ hai khi chỉ một người qua cầu.</summary>
        public const int NoPlayer = -1;

        readonly PlankRound round;
        readonly PlankBank[] banks;
        PlankBank cardBank;

        /// <summary>Đề đang chơi.</summary>
        public PlankRound Round { get { return round; } }
        /// <summary>Đang chơi, thắng hay thất bại.</summary>
        public PlankStatus Status { get; private set; }
        /// <summary>Tổng thời gian (đơn vị) các lượt qua cầu đã thực hiện (không tính thời gian suy nghĩ).</summary>
        public int UsedUnits { get; private set; }
        /// <summary>Thời gian còn lại đến hạn (đơn vị).</summary>
        public int RemainingUnits { get { return round.DeadlineUnits - UsedUnits; } }
        /// <summary>Bờ đang giữ thẻ.</summary>
        public PlankBank CardBank { get { return cardBank; } }
        /// <summary>Số lần chơi (1 = lần đầu; tăng mỗi lần Reset).</summary>
        public int Attempts { get; private set; }

        /// <summary>Nhận đề; tạo trạng thái đầu: mọi người và thẻ ở bờ gần. Ném ArgumentNullException nếu đề null.</summary>
        public PlankState(PlankRound round)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            this.round = round;
            banks = new PlankBank[round.PlayerCount];
            Attempts = 1;
            ResetState();
        }

        /// <summary>Nhận chỉ số người; trả bờ người đó đang đứng. Ném ArgumentOutOfRangeException nếu sai chỉ số.</summary>
        public PlankBank BankOf(int player)
        {
            if (player < 0 || player >= banks.Length) throw new ArgumentOutOfRangeException(nameof(player));
            return banks[player];
        }

        /// <summary>Nhận một người; thử cho người đó qua cầu một mình (mang thẻ).</summary>
        public PlankResult TryCross(int player)
        {
            return TryCross(player, NoPlayer);
        }

        /// <summary>
        /// Nhận người thứ nhất và người thứ hai (NoPlayer nếu đi một mình); thử cho nhóm qua cầu cùng thẻ.
        /// Điều kiện: nhiệm vụ đang chơi, chỉ số hợp lệ và khác nhau, cả nhóm cùng bờ với thẻ. Tốn thời gian của người chậm hơn;
        /// vượt hạn thì thất bại và lượt không thực hiện. Trả kết quả (xem PlankResult).
        /// </summary>
        public PlankResult TryCross(int playerA, int playerB)
        {
            if (Status != PlankStatus.Playing) return PlankResult.Finished;
            int n = banks.Length;
            if (playerA < 0 || playerA >= n) return PlankResult.InvalidPlayer;
            bool pair = playerB != NoPlayer;
            if (pair && (playerB < 0 || playerB >= n || playerB == playerA)) return PlankResult.InvalidPlayer;

            PlankBank from = banks[playerA];
            if (pair && banks[playerB] != from) return PlankResult.WrongBank;
            if (cardBank != from) return PlankResult.CardNotWithGroup;

            int cost = round.CrossUnits(playerA);
            if (pair) cost = Math.Max(cost, round.CrossUnits(playerB));
            if (UsedUnits + cost > round.DeadlineUnits)
            {
                Status = PlankStatus.Failed;
                return PlankResult.TimedOut;
            }

            PlankBank to = from == PlankBank.Near ? PlankBank.Far : PlankBank.Near;
            banks[playerA] = to;
            if (pair) banks[playerB] = to;
            cardBank = to;
            UsedUnits += cost;

            if (AllFar())
            {
                Status = PlankStatus.Won;
                return PlankResult.Won;
            }
            return PlankResult.Crossed;
        }

        /// <summary>Làm lại cùng đề: mọi người và thẻ về bờ gần, thời gian về 0, tăng số lần chơi.</summary>
        public void Reset()
        {
            Attempts++;
            ResetState();
        }

        /// <summary>Đưa mọi thứ về trạng thái đầu (không đổi số lần chơi).</summary>
        void ResetState()
        {
            for (int i = 0; i < banks.Length; i++) banks[i] = PlankBank.Near;
            cardBank = PlankBank.Near;
            UsedUnits = 0;
            Status = PlankStatus.Playing;
        }

        /// <summary>Trả true nếu mọi người và thẻ đều ở bờ xa.</summary>
        bool AllFar()
        {
            if (cardBank != PlankBank.Far) return false;
            for (int i = 0; i < banks.Length; i++)
                if (banks[i] != PlankBank.Far) return false;
            return true;
        }
    }
}
