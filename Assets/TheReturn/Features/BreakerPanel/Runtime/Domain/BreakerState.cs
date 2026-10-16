/*
 * Mục đích: Trạng thái chơi của «Bảng cầu dao» (T-006a): công tắc nào đang bật, đèn hiện tại, quá tải, giữ khớp đích đủ giờ.
 * Thuần C#; thời gian do nơi gọi truyền vào (giây) để test và đồng bộ mạng được.
 * Hàm: constructor; Lamps / Matches / OnCount; LampOn / TargetOn / SwitchOn; Toggle; Inspect; Tick; HoldProgress; Reset.
 */
using System;

namespace TheReturn
{
    /// <summary>Kết quả một lần bấm công tắc.</summary>
    public enum BreakerResult
    {
        Toggled,
        NotOwner,
        InvalidSwitch,
        Tripped,
        AlreadySolved
    }

    /// <summary>Trạng thái một lượt chơi cầu dao trên một đề.</summary>
    public sealed class BreakerState
    {
        double matchSince = -1;

        public BreakerRound Round { get; }
        /// <summary>Số giây phải giữ khớp đích liên tục để thắng (mặc định 3, chỉnh được).</summary>
        public double HoldSeconds { get; }
        /// <summary>Mặt nạ các công tắc đang bật.</summary>
        public int OnSwitches { get; private set; }
        /// <summary>Số lần cầu dao đã nhảy do quá tải.</summary>
        public int Trips { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>Nhận đề và số giây giữ khớp (≥ 0); tạo trạng thái mới, mọi công tắc tắt.</summary>
        public BreakerState(BreakerRound round, double holdSeconds = 3.0)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            if (holdSeconds < 0) throw new ArgumentOutOfRangeException(nameof(holdSeconds));
            Round = round;
            HoldSeconds = holdSeconds;
        }

        /// <summary>Số công tắc đang bật.</summary>
        public int OnCount => BreakerRound.CountBits(OnSwitches);

        /// <summary>Mặt nạ đèn hiện tại = đèn đầu XOR tác dụng của các công tắc đang bật.</summary>
        public int Lamps => Round.StartLamps ^ Round.Combine(OnSwitches);

        /// <summary>True khi đèn hiện tại khớp đích.</summary>
        public bool Matches => Lamps == Round.TargetLamps;

        /// <summary>Nhận chỉ số đèn; trả true nếu đèn đang sáng.</summary>
        public bool LampOn(int lamp) { return (Lamps >> lamp & 1) == 1; }

        /// <summary>Nhận chỉ số đèn; trả true nếu đèn phải sáng ở đích.</summary>
        public bool TargetOn(int lamp) { return (Round.TargetLamps >> lamp & 1) == 1; }

        /// <summary>Nhận chỉ số công tắc; trả true nếu công tắc đang bật.</summary>
        public bool SwitchOn(int sw) { return (OnSwitches >> sw & 1) == 1; }

        /// <summary>
        /// Nhận người chơi và công tắc; chỉ chủ công tắc mới bấm được. Bật làm vượt giới hạn thì cầu dao nhảy: mọi công tắc về tắt.
        /// Mỗi lần bấm đều làm lại bộ đếm giữ khớp. Trả kết quả.
        /// </summary>
        public BreakerResult Toggle(int player, int sw)
        {
            if (Solved) return BreakerResult.AlreadySolved;
            if (sw < 0 || sw >= Round.Size || player < 0 || player >= Round.PlayerCount) return BreakerResult.InvalidSwitch;
            if (Round.OwnerOf(sw) != player) return BreakerResult.NotOwner;
            matchSince = -1;
            OnSwitches ^= 1 << sw;
            if (SwitchOn(sw) && OnCount > Round.OverloadLimit)
            {
                OnSwitches = 0;
                Trips++;
                return BreakerResult.Tripped;
            }
            return BreakerResult.Toggled;
        }

        /// <summary>Nhận người chơi và công tắc; chỉ chủ mới xem được các đèn mà công tắc đảo. Trả true và gán mặt nạ nếu được xem.</summary>
        public bool Inspect(int player, int sw, out int lampMask)
        {
            lampMask = 0;
            if (sw < 0 || sw >= Round.Size || Round.OwnerOf(sw) != player) return false;
            lampMask = Round.SwitchMask(sw);
            return true;
        }

        /// <summary>Nhận thời gian hiện tại (giây); gọi mỗi frame. Giữ khớp đích liên tục đủ HoldSeconds thì thắng. Trả true nếu đã thắng.</summary>
        public bool Tick(double now)
        {
            if (Solved) return true;
            if (!Matches) { matchSince = -1; return false; }
            if (matchSince < 0) matchSince = now;
            if (now - matchSince >= HoldSeconds) Solved = true;
            return Solved;
        }

        /// <summary>Nhận thời gian hiện tại; trả tỉ lệ 0..1 đã giữ khớp (1 nếu đã thắng) để hiện thanh tiến độ.</summary>
        public double HoldProgress(double now)
        {
            if (Solved) return 1;
            if (matchSince < 0) return 0;
            if (HoldSeconds <= 0) return 1;
            double p = (now - matchSince) / HoldSeconds;
            return p < 0 ? 0 : (p > 1 ? 1 : p);
        }

        /// <summary>Không nhận tham số; đặt lại về lúc đầu (mọi công tắc tắt, chưa thắng), GIỮ cùng đề và số lần nhảy.</summary>
        public void Reset()
        {
            OnSwitches = 0;
            Solved = false;
            matchSince = -1;
        }
    }
}
