/*
 * Mục đích: LUẬT HOST của «Điểm danh» khi chơi online (T-031): bọc AttendanceState + AttendanceGenerator (Domain có sẵn, KHÔNG đổi) theo giao diện
 * IHostRules của khung T-024. Host nhận lệnh ngồi / đứng / làm lại, kiểm điều kiện (đứng gần ghế bằng IAttendanceWorld, ghế trống, người chưa ngồi),
 * rồi chạy luật. Khi cả đội đã ngồi, chờ vài giây rồi chấm đúng/sai (Tick). Trạng thái chung (ai ngồi ghế nào, đúng/sai) gửi mọi người; gói manh mối
 * chỉ gửi RIÊNG cho chính người đó. Theo mẫu PLANS/T-027-plank-network-pattern.md. Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; Validate; Apply; BuildPublicState; BuildPrivateState; Tick; ReleaseSeat; thuộc tính State, Round, PlayerCount, ResetCount, Solved.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.AttendanceNet
{
    /// <summary>Luật host của nhiệm vụ điểm danh (lệnh AttendanceCommand, trạng thái chung AttendancePublicState, riêng AttendancePrivateState).</summary>
    public sealed class AttendanceHostRules : IHostRules<AttendanceCommand, AttendancePublicState, AttendancePrivateState>
    {
        readonly AttendanceState state;
        readonly IAttendanceWorld world;
        readonly string[] packets;
        readonly float settleSeconds;
        float settleLeft = -1f;
        bool wrong;
        int resetCount;
        AttendanceMessage lastMessage;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh và Tick).</summary>
        public AttendanceState State { get { return state; } }

        /// <summary>Đề đang chơi (để test và lớp Unity đọc đáp án khi cần).</summary>
        public AttendanceRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.PlayerCount; } }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True sau khi cả đội đã ngồi đúng.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>
        /// Nhận tên các vai (2-4, không trùng), seed của đề, hàm dựng gói manh mối từ đề (trả đúng một chuỗi cho mỗi người),
        /// nguồn vật lý của host và số giây chờ trước khi chấm (>= 0). Ném lỗi nếu tham số sai.
        /// </summary>
        public AttendanceHostRules(string[] names, int seed, Func<AttendanceRound, string[]> packetBuilder, IAttendanceWorld world, float settleSeconds)
        {
            if (packetBuilder == null) throw new ArgumentNullException("packetBuilder");
            if (world == null) throw new ArgumentNullException("world");
            if (settleSeconds < 0f || float.IsNaN(settleSeconds)) throw new ArgumentOutOfRangeException("settleSeconds");
            AttendanceRound round = AttendanceGenerator.Generate(names, seed);
            string[] built = packetBuilder(round);
            if (built == null || built.Length != round.PlayerCount) throw new ArgumentException("Gói manh mối phải có đúng một chuỗi cho mỗi người.");
            packets = new string[built.Length];
            for (int i = 0; i < built.Length; i++) packets[i] = built[i] ?? string.Empty;
            state = new AttendanceState(round);
            this.world = world;
            this.settleSeconds = settleSeconds;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, AttendanceCommand command)
        {
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            switch (command.Kind)
            {
                case AttendanceCommandKind.Retry:
                    return state.Solved ? RuleVerdict.Reject("Đã điểm danh xong, không cần làm lại.") : RuleVerdict.Accept();
                case AttendanceCommandKind.Stand:
                    return state.SeatOf(senderSlot) < 0 ? RuleVerdict.Reject("Bạn chưa ngồi ghế nào.") : RuleVerdict.Accept();
                case AttendanceCommandKind.Sit:
                    return ValidateSit(senderSlot, command.Seat);
                default:
                    return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        RuleVerdict ValidateSit(int slot, int seat)
        {
            if (state.Solved) return RuleVerdict.Reject("Đã điểm danh xong.");
            if (seat < 0 || seat >= AttendanceRound.SeatCount) return RuleVerdict.Reject("Ghế này không tồn tại.");
            if (state.SeatOf(slot) >= 0) return RuleVerdict.Reject("Bạn đang ngồi, hãy đứng dậy trước.");
            if (state.Occupant(seat) >= 0) return RuleVerdict.Reject("Ghế này đã có người ngồi.");
            if (!world.IsNearSeat(slot, seat)) return RuleVerdict.Reject("Bạn đứng quá xa ghế này.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, AttendanceCommand command)
        {
            switch (command.Kind)
            {
                case AttendanceCommandKind.Retry:
                    state.Reset();
                    resetCount++;
                    wrong = false;
                    settleLeft = -1f;
                    lastMessage = AttendanceMessage.Retry;
                    break;
                case AttendanceCommandKind.Stand:
                    state.Stand(senderSlot);
                    wrong = false;
                    settleLeft = -1f;
                    lastMessage = AttendanceMessage.Stood;
                    break;
                case AttendanceCommandKind.Sit:
                    state.TrySit(senderSlot, command.Seat);
                    wrong = false;
                    settleLeft = state.OccupiedCount == state.PlayerCount ? settleSeconds : -1f;
                    lastMessage = AttendanceMessage.Seated;
                    break;
            }
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến).</summary>
        public AttendancePublicState BuildPublicState()
        {
            int count = PlayerCount;
            string[] names = new string[count];
            for (int i = 0; i < count; i++) names[i] = state.Round.Names[i];
            int[] seats = new int[AttendanceRound.SeatCount];
            for (int seat = 0; seat < seats.Length; seat++) seats[seat] = state.Occupant(seat);
            bool settling = settleLeft >= 0f && !state.Solved;
            return new AttendancePublicState(count, names, seats, state.Solved, wrong, settling, resetCount, lastMessage);
        }

        /// <summary>Nhận ô; trả gói manh mối RIÊNG của ô đó (rỗng nếu ô không tham gia).</summary>
        public AttendancePrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new AttendancePrivateState(string.Empty);
            return new AttendancePrivateState(packets[slot]);
        }

        /// <summary>
        /// Đồng hồ chạy trên host. Nhận số giây đã trôi. Khi cả đội đã ngồi đủ và hết thời gian chờ thì chấm đúng/sai.
        /// Trả true khi vừa chấm xong (trạng thái đổi mà không lệnh nào gây ra), để lớp Unity gọi HostAuthority.MarkChanged().
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) || deltaSeconds < 0f) return false;
            if (settleLeft < 0f || state.Solved) return false;
            settleLeft -= deltaSeconds;
            if (settleLeft > 0f) return false;
            settleLeft = -1f;
            if (state.Evaluate())
            {
                wrong = false;
                lastMessage = AttendanceMessage.Solved;
            }
            else
            {
                wrong = true;
                lastMessage = AttendanceMessage.Wrong;
            }
            return true;
        }

        /// <summary>
        /// Cho lớp Unity gọi khi một người mất kết nối: cho họ đứng dậy khỏi ghế (nếu đang ngồi). Nhận ô.
        /// Trả true nếu có thay đổi (lúc đó gọi HostAuthority.MarkChanged()).
        /// </summary>
        public bool ReleaseSeat(int slot)
        {
            if (!state.Stand(slot)) return false;
            wrong = false;
            settleLeft = -1f;
            lastMessage = AttendanceMessage.Stood;
            return true;
        }
    }
}
