/*
 * Mục đích: Trạng thái «Cầu ván và thẻ từ» MỌI người đều thấy (T-027) và phần thông tin RIÊNG từng người. Public: pha, ván ở bờ nào, ai
 * đang trên ván, thẻ ở ai, đồng hồ, bờ của từng người. Private: tốc độ cặp sách của CHÍNH người đó (người khác phải hỏi). Bất biến (host
 * tạo bản mới mỗi lần chụp). Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor PlankPublicState; constructor PlankPrivateState; bản sao mảng (RidersCopy, PlayerBanksCopy).
 */
using System;

namespace TheReturn.PlankNet
{
    /// <summary>Sự kiện gần nhất, để máy khách hiện thông báo/âm thanh đúng lúc.</summary>
    public enum PlankMessage
    {
        None,
        CardPicked,
        CardDropped,
        CardOnDeck,
        CrossingStarted,
        Arrived,
        Solved,
        TimedOut,
        Reset
    }

    /// <summary>Thẻ từ đang ở đâu: chỗ ban đầu, trên tay một người, trên sàn, hoặc trên ván.</summary>
    public enum PlankCardMode
    {
        AtHome,
        Held,
        OnFloor,
        OnDeck
    }

    /// <summary>Thông tin riêng của một người: thời gian qua cầu (đơn vị) của cặp sách của họ.</summary>
    public readonly struct PlankPrivateState
    {
        /// <summary>Thời gian qua cầu của người này; 0 nếu ô không tham gia.</summary>
        public int CrossUnits { get; }

        /// <summary>Nhận thời gian qua cầu của người này.</summary>
        public PlankPrivateState(int crossUnits)
        {
            CrossUnits = crossUnits;
        }
    }

    /// <summary>Ảnh chụp trạng thái chung của nhiệm vụ.</summary>
    public sealed class PlankPublicState
    {
        readonly int[] riders;
        readonly PlankBank[] playerBanks;

        public int PlayerCount { get; }
        public PlankPhase Phase { get; }
        public PlankStatus Status { get; }
        /// <summary>True khi ván đang đậu ở bờ đầu khe (bờ nơi đội bắt đầu).</summary>
        public bool DeckAtNear { get; }
        public float CrossingSecondsTotal { get; }
        public float CrossingSecondsLeft { get; }
        /// <summary>Ô đang giữ thẻ từ; -1 nếu không ai giữ.</summary>
        public int CardHolder { get; }
        /// <summary>Thẻ đang ở đâu.</summary>
        public PlankCardMode CardMode { get; }
        /// <summary>Toạ độ X của thẻ trong hệ làn (OnFloor: tuyệt đối; OnDeck: lệch so với tâm ván; còn lại 0).</summary>
        public float CardLocalX { get; }
        /// <summary>Toạ độ Z của thẻ trong hệ làn, cùng quy ước như CardLocalX.</summary>
        public float CardLocalZ { get; }
        /// <summary>True khi thẻ không ai cầm và nằm trên ván.</summary>
        public bool CardOnDeck { get { return CardMode == PlankCardMode.OnDeck; } }
        public float DisplayUsedSeconds { get; }
        public float DeadlineSeconds { get; }
        public int Attempts { get; }
        public int ResetCount { get; }
        public bool Solved { get; }
        public PlankMessage LastMessage { get; }

        public PlankPublicState(int playerCount, PlankPhase phase, PlankStatus status, bool deckAtNear, float crossingSecondsTotal,
            float crossingSecondsLeft, int[] riders, PlankBank[] playerBanks, int cardHolder, PlankCardMode cardMode, float cardLocalX, float cardLocalZ, float displayUsedSeconds,
            float deadlineSeconds, int attempts, int resetCount, bool solved, PlankMessage lastMessage)
        {
            if (riders == null) throw new ArgumentNullException("riders");
            if (playerBanks == null) throw new ArgumentNullException("playerBanks");
            PlayerCount = playerCount;
            Phase = phase;
            Status = status;
            DeckAtNear = deckAtNear;
            CrossingSecondsTotal = crossingSecondsTotal;
            CrossingSecondsLeft = crossingSecondsLeft;
            this.riders = (int[])riders.Clone();
            this.playerBanks = (PlankBank[])playerBanks.Clone();
            CardHolder = cardHolder;
            CardMode = cardMode;
            CardLocalX = cardLocalX;
            CardLocalZ = cardLocalZ;
            DisplayUsedSeconds = displayUsedSeconds;
            DeadlineSeconds = deadlineSeconds;
            Attempts = attempts;
            ResetCount = resetCount;
            Solved = solved;
            LastMessage = lastMessage;
        }

        /// <summary>Trả bản sao danh sách ô đang ngồi trên ván chạy (rỗng khi rảnh).</summary>
        public int[] RidersCopy()
        {
            return (int[])riders.Clone();
        }

        /// <summary>Trả bản sao bờ của từng người (chỉ số = ô).</summary>
        public PlankBank[] PlayerBanksCopy()
        {
            return (PlankBank[])playerBanks.Clone();
        }
    }
}
