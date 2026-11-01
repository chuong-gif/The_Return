/*
 * Mục đích: LUẬT HOST của «Cầu ván và thẻ từ» khi chơi online (T-027): bọc PlankSession (luật + thời gian thực đã có, KHÔNG đổi Domain) theo
 * giao diện IHostRules của khung T-024. Host nhận lệnh nhặt/thả thẻ, gạt cần, làm lại; kiểm điều kiện (ai đứng gần, ai trên ván, thẻ ở đâu,
 * ván đang ở bờ nào) bằng dữ liệu vật lý do IPlankWorld cung cấp, rồi chạy luật. Trạng thái chung gửi mọi người; tốc độ cặp sách chỉ gửi
 * riêng cho chính người đó. Đồng hồ chạy bằng Tick trên host. Đây là MẪU CHUẨN cho các nhiệm vụ khác (xem PLANS/T-027-*.md).
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; Validate; Apply; BuildPublicState; BuildPrivateState; Tick; Session; PlayerCount; ResetCount; IsResetPending.
 */
using System;
using System.Collections.Generic;
using TheReturn.Network.Framework;

namespace TheReturn.PlankNet
{
    /// <summary>Luật host của nhiệm vụ cầu ván (lệnh PlankCommand, trạng thái chung PlankPublicState, riêng PlankPrivateState).</summary>
    public sealed class PlankHostRules : IHostRules<PlankCommand, PlankPublicState, PlankPrivateState>
    {
        readonly PlankSession session;
        readonly IPlankWorld world;
        readonly float failResetSeconds;
        bool deckAtNear = true;
        int cardHolder = -1;
        PlankCardMode cardMode = PlankCardMode.AtHome;
        float cardX;
        float cardZ;
        int[] riders = new int[0];
        int[] validatedDeck = new int[0];
        float failTimer;
        int resetCount;
        PlankMessage lastMessage;

        bool CardOnDeckZone { get { return cardMode == PlankCardMode.OnDeck; } }

        /// <summary>Phiên luật + thời gian thực bên dưới (chỉ đọc; đổi qua lệnh và Tick).</summary>
        public PlankSession Session { get { return session; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return session.State.Round.PlayerCount; } }

        /// <summary>Số lần đã đặt lại; lớp Unity trên host so sánh để đưa cả đội về đầu khe và đặt lại ván/thẻ.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True khi vừa hết giờ và đang chờ tự đặt lại (lệnh khác bị từ chối).</summary>
        public bool IsResetPending { get { return failTimer > 0f; } }

        /// <summary>
        /// Nhận đề, số giây thật mỗi đơn vị (> 0), nguồn vật lý của host và số giây chờ trước khi tự đặt lại sau khi hết giờ (>= 0).
        /// Ném lỗi nếu đề/nguồn null hoặc số sai.
        /// </summary>
        public PlankHostRules(PlankRound round, float secondsPerUnit, IPlankWorld world, float failResetSeconds)
        {
            if (world == null) throw new ArgumentNullException("world");
            if (failResetSeconds < 0f) throw new ArgumentOutOfRangeException("failResetSeconds");
            session = new PlankSession(round, secondsPerUnit);
            this.world = world;
            this.failResetSeconds = failResetSeconds;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, PlankCommand command)
        {
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            if (command.Kind == PlankCommandKind.Retry) return RuleVerdict.Accept();
            if (failTimer > 0f) return RuleVerdict.Reject("Đang đặt lại sau khi hết giờ.");
            switch (command.Kind)
            {
                case PlankCommandKind.PickUpCard: return ValidatePickUp(senderSlot);
                case PlankCommandKind.DropCard: return ValidateDrop(senderSlot);
                case PlankCommandKind.UseLever: return ValidateLever(senderSlot, command.SideA);
                default: return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, PlankCommand command)
        {
            switch (command.Kind)
            {
                case PlankCommandKind.Retry:
                    ResetAll();
                    break;
                case PlankCommandKind.PickUpCard:
                    cardHolder = senderSlot;
                    cardMode = PlankCardMode.Held;
                    cardX = 0f;
                    cardZ = 0f;
                    lastMessage = PlankMessage.CardPicked;
                    break;
                case PlankCommandKind.DropCard:
                    {
                        PlankDropSpot spot = world.DropSpot(senderSlot);
                        cardHolder = -1;
                        cardMode = spot.OnDeck ? PlankCardMode.OnDeck : PlankCardMode.OnFloor;
                        cardX = spot.LocalX;
                        cardZ = spot.LocalZ;
                        lastMessage = spot.OnDeck ? PlankMessage.CardOnDeck : PlankMessage.CardDropped;
                    }
                    break;
                case PlankCommandKind.UseLever:
                    StartCrossing();
                    break;
            }
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến).</summary>
        public PlankPublicState BuildPublicState()
        {
            int count = PlayerCount;
            PlankBank[] banks = new PlankBank[count];
            for (int i = 0; i < count; i++) banks[i] = session.State.BankOf(i);
            return new PlankPublicState(count, session.Phase, session.State.Status, deckAtNear, session.CrossingSecondsTotal,
                session.CrossingSecondsLeft, riders, banks, cardHolder, cardMode, cardX, cardZ, session.DisplayUsedSeconds,
                session.DeadlineSeconds, session.State.Attempts, resetCount, session.Solved, lastMessage);
        }

        /// <summary>Nhận ô; trả thông tin RIÊNG của ô đó: thời gian qua cầu của cặp sách của chính họ (0 nếu ô không tham gia).</summary>
        public PlankPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new PlankPrivateState(0);
            return new PlankPrivateState(session.State.Round.CrossUnits(slot));
        }

        /// <summary>
        /// Nhận số giây trôi qua (>= 0) trên host: chạy ván, đến nơi, hoặc tự đặt lại sau khi hết giờ. Trả true nếu có thay đổi mà
        /// lệnh nào cũng không gây ra (ván đến nơi, đặt lại): host cần gọi HostAuthority.MarkChanged để phát trạng thái mới.
        /// </summary>
        public bool Tick(float seconds)
        {
            if (seconds < 0f) throw new ArgumentOutOfRangeException("seconds");
            if (failTimer > 0f)
            {
                failTimer -= seconds;
                if (failTimer > 0f) return false;
                ResetAll();
                return true;
            }
            if (session.Phase != PlankPhase.Crossing) return false;
            if (!session.Tick(seconds)) return false;
            deckAtNear = !deckAtNear;
            riders = new int[0];
            lastMessage = session.Solved ? PlankMessage.Solved : PlankMessage.Arrived;
            return true;
        }

        RuleVerdict ValidatePickUp(int slot)
        {
            if (cardHolder >= 0) return RuleVerdict.Reject("Vai " + (cardHolder + 1) + " đang cầm thẻ từ.");
            if (session.Phase == PlankPhase.Crossing && CardOnDeckZone) return RuleVerdict.Reject("Thẻ đang trên ván, không nhặt được lúc ván chạy.");
            if (!world.IsNearCard(slot)) return RuleVerdict.Reject("Đứng gần thẻ hơn.");
            return RuleVerdict.Accept();
        }

        RuleVerdict ValidateDrop(int slot)
        {
            if (cardHolder != slot) return RuleVerdict.Reject("Bạn không cầm thẻ từ.");
            if (session.Phase == PlankPhase.Crossing) return RuleVerdict.Reject("Không thả thẻ khi ván đang chạy.");
            return RuleVerdict.Accept();
        }

        RuleVerdict ValidateLever(int slot, bool sideA)
        {
            // Bờ A là bờ đầu khe (Near); cần ở bờ nào thì chỉ dùng được khi ván đang đậu ở bờ đó.
            if (sideA != deckAtNear) return RuleVerdict.Reject("Ván đang ở bờ bên kia. Gạt cần ở bờ nơi ván đang đậu.");
            if (!world.IsNearLever(slot, sideA)) return RuleVerdict.Reject("Đứng gần cần gạt hơn.");
            if (session.Phase == PlankPhase.Crossing) return RuleVerdict.Reject("Ván đang chạy.");
            if (session.State.Status != PlankStatus.Playing) return RuleVerdict.Reject("Nhiệm vụ đã xong hoặc đã thất bại; bấm R để làm lại.");

            List<int> on = new List<int>();
            int[] raw = world.PlayersOnDeck();
            if (raw != null)
            {
                for (int i = 0; i < raw.Length; i++)
                {
                    if (raw[i] >= 0 && raw[i] < PlayerCount && !on.Contains(raw[i])) on.Add(raw[i]);
                }
            }
            on.Sort();
            if (on.Count == 0) return RuleVerdict.Reject("Cần có người đứng trên ván.");
            if (on.Count > 2) return RuleVerdict.Reject("Ván chỉ chở tối đa 2 người (đang có " + on.Count + " người).");

            PlankBank deckBank = deckAtNear ? PlankBank.Near : PlankBank.Far;
            for (int i = 0; i < on.Count; i++)
            {
                if (session.State.BankOf(on[i]) != deckBank) return RuleVerdict.Reject("Người trên ván không cùng bờ với ván.");
            }
            bool cardAboard = cardHolder >= 0 ? on.Contains(cardHolder) : CardOnDeckZone;
            if (!cardAboard) return RuleVerdict.Reject("Ván chỉ hạ xuống khi thẻ từ nằm trên ván: người trên ván cầm thẻ hoặc đặt thẻ lên ván.");

            validatedDeck = on.ToArray();
            return RuleVerdict.Accept();
        }

        void StartCrossing()
        {
            int[] on = validatedDeck;
            PlankStartResult result = on.Length == 1 ? session.TryStartCrossing(on[0]) : session.TryStartCrossing(on[0], on[1]);
            if (result == PlankStartResult.Started)
            {
                riders = on;
                lastMessage = PlankMessage.CrossingStarted;
            }
            else if (result == PlankStartResult.TimedOut)
            {
                lastMessage = PlankMessage.TimedOut;
                failTimer = failResetSeconds > 0f ? failResetSeconds : 0.0001f;
            }
        }

        void ResetAll()
        {
            session.Reset();
            deckAtNear = true;
            cardHolder = -1;
            cardMode = PlankCardMode.AtHome;
            cardX = 0f;
            cardZ = 0f;
            riders = new int[0];
            validatedDeck = new int[0];
            failTimer = 0f;
            resetCount++;
            lastMessage = PlankMessage.Reset;
        }
    }
}
