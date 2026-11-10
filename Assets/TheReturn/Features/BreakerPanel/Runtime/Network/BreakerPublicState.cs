/*
 * Mục đích: Trạng thái CHUNG (mọi người cùng thấy) và thông tin RIÊNG (chỉ một người) của «Bảng cầu dao» online (T-031).
 * Chung: công tắc nào đang bật, đèn hiện tại, đèn đích, quá tải, tiến độ giữ khớp. KHÔNG chứa seed, đáp án hay mặt nạ công tắc của người khác.
 * Riêng: các công tắc thuộc người đó và đèn mỗi công tắc đảo (chỉ chủ mới được xem). Bất biến: mảng được sao khi tạo và khi đọc.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; SwitchOn; LampOn; TargetOn; OnCount; HoldFraction; (BreakerPrivateState) constructor, SwitchesCopy, MasksCopy, Owns, TryGetLampMask.
 */
using System;

namespace TheReturn.BreakerNet
{
    /// <summary>Thông báo gần nhất để HUD hiển thị (mã enum, chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum BreakerMessage
    {
        None,
        Toggled,
        Tripped,
        Solved,
        Retry
    }

    /// <summary>Thông tin riêng của một ô: các công tắc người đó sở hữu và mặt nạ đèn mỗi công tắc đảo.</summary>
    public readonly struct BreakerPrivateState
    {
        readonly int[] switches;
        readonly int[] masks;

        /// <summary>Nhận danh sách công tắc sở hữu và mặt nạ đèn tương ứng (cùng độ dài; null coi là rỗng). Ném lỗi nếu độ dài khác nhau.</summary>
        public BreakerPrivateState(int[] ownedSwitches, int[] lampMasks)
        {
            int a = ownedSwitches == null ? 0 : ownedSwitches.Length;
            int b = lampMasks == null ? 0 : lampMasks.Length;
            if (a != b) throw new ArgumentException("Số công tắc và số mặt nạ phải bằng nhau.");
            switches = a == 0 ? new int[0] : (int[])ownedSwitches.Clone();
            masks = a == 0 ? new int[0] : (int[])lampMasks.Clone();
        }

        /// <summary>Số công tắc người này sở hữu.</summary>
        public int OwnedCount { get { return switches == null ? 0 : switches.Length; } }

        /// <summary>Trả bản sao danh sách công tắc sở hữu.</summary>
        public int[] SwitchesCopy()
        {
            return switches == null ? new int[0] : (int[])switches.Clone();
        }

        /// <summary>Trả bản sao mặt nạ đèn, cùng thứ tự với SwitchesCopy.</summary>
        public int[] MasksCopy()
        {
            return masks == null ? new int[0] : (int[])masks.Clone();
        }

        /// <summary>Nhận chỉ số công tắc; trả true nếu người này là chủ.</summary>
        public bool Owns(int sw)
        {
            int mask;
            return TryGetLampMask(sw, out mask);
        }

        /// <summary>Nhận chỉ số công tắc; trả true và gán mặt nạ đèn công tắc đảo nếu người này là chủ, ngược lại false (mask = 0).</summary>
        public bool TryGetLampMask(int sw, out int mask)
        {
            mask = 0;
            if (switches == null) return false;
            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] != sw) continue;
                mask = masks[i];
                return true;
            }
            return false;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ cầu dao.</summary>
    public sealed class BreakerPublicState
    {
        /// <summary>Số công tắc/đèn tối đa (4 người + 2).</summary>
        public const int MaxSize = 6;

        /// <summary>Số người của đề (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>Số công tắc, bằng số đèn (= số người + 2).</summary>
        public int Size { get { return PlayerCount + 2; } }

        /// <summary>Số công tắc tối đa được bật cùng lúc; bật thêm thì cầu dao nhảy.</summary>
        public int OverloadLimit { get { return (Size + 1) / 2; } }

        /// <summary>Mặt nạ các công tắc đang bật (bit i = công tắc i).</summary>
        public int OnSwitches { get; }

        /// <summary>Mặt nạ đèn đang sáng.</summary>
        public int Lamps { get; }

        /// <summary>Mặt nạ đèn phải sáng ở đích.</summary>
        public int TargetLamps { get; }

        /// <summary>Số lần cầu dao đã nhảy.</summary>
        public int Trips { get; }

        /// <summary>True khi đã giữ khớp đủ giờ (thắng).</summary>
        public bool Solved { get; }

        /// <summary>Luôn 0 khi đang giữ khớp và bằng HoldSeconds khi đã thắng (bất biến theo Version); máy khách tự ước lượng tiến độ từ lúc nhận gói.</summary>
        public float HoldElapsed { get; }

        /// <summary>Số giây phải giữ khớp để thắng.</summary>
        public float HoldSeconds { get; }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get; }

        /// <summary>Thông báo gần nhất.</summary>
        public BreakerMessage LastMessage { get; }

        /// <summary>Ô người bấm lệnh gần nhất, -1 nếu chưa có.</summary>
        public int LastActor { get; }

        /// <summary>Công tắc bấm gần nhất, -1 nếu chưa có.</summary>
        public int LastSwitch { get; }

        /// <summary>True khi đèn hiện tại khớp đích.</summary>
        public bool Matching { get { return Lamps == TargetLamps; } }

        /// <summary>Số công tắc đang bật.</summary>
        public int OnCount { get { return TheReturn.BreakerRound.CountBits(OnSwitches); } }

        /// <summary>
        /// Nhận số người (2-4), mặt nạ công tắc bật, đèn hiện tại, đèn đích, số lần nhảy, cờ thắng, giây đã giữ, giây cần giữ, số lần làm lại,
        /// thông báo, ô và công tắc bấm gần nhất. Ném lỗi nếu dữ liệu sai hình dạng (mặt nạ ngoài khoảng, bật quá giới hạn, thắng mà đèn chưa khớp...).
        /// </summary>
        public BreakerPublicState(int playerCount, int onSwitches, int lamps, int targetLamps, int trips, bool solved,
            float holdElapsed, float holdSeconds, int resetCount, BreakerMessage lastMessage, int lastActor, int lastSwitch)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            int size = playerCount + 2;
            int all = (1 << size) - 1;
            if (onSwitches < 0 || onSwitches > all) throw new ArgumentOutOfRangeException("onSwitches");
            if (lamps < 0 || lamps > all) throw new ArgumentOutOfRangeException("lamps");
            if (targetLamps < 0 || targetLamps > all) throw new ArgumentOutOfRangeException("targetLamps");
            if (TheReturn.BreakerRound.CountBits(onSwitches) > (size + 1) / 2) throw new ArgumentException("Bật quá giới hạn quá tải.", "onSwitches");
            if (trips < 0) throw new ArgumentOutOfRangeException("trips");
            if (float.IsNaN(holdSeconds) || float.IsInfinity(holdSeconds) || holdSeconds < 0f) throw new ArgumentOutOfRangeException("holdSeconds");
            if (float.IsNaN(holdElapsed) || holdElapsed < 0f || holdElapsed > holdSeconds) throw new ArgumentOutOfRangeException("holdElapsed");
            if (resetCount < 0) throw new ArgumentOutOfRangeException("resetCount");
            if (lastActor < -1 || lastActor >= playerCount) throw new ArgumentOutOfRangeException("lastActor");
            if (lastSwitch < -1 || lastSwitch >= size) throw new ArgumentOutOfRangeException("lastSwitch");
            if (solved && lamps != targetLamps) throw new ArgumentException("Đã thắng thì đèn phải khớp đích.", "solved");
            PlayerCount = playerCount;
            OnSwitches = onSwitches;
            Lamps = lamps;
            TargetLamps = targetLamps;
            Trips = trips;
            Solved = solved;
            HoldElapsed = holdElapsed;
            HoldSeconds = holdSeconds;
            ResetCount = resetCount;
            LastMessage = lastMessage;
            LastActor = lastActor;
            LastSwitch = lastSwitch;
        }

        /// <summary>Nhận chỉ số công tắc; trả true nếu đang bật (ngoài khoảng trả false).</summary>
        public bool SwitchOn(int sw) { return sw >= 0 && sw < Size && (OnSwitches >> sw & 1) == 1; }

        /// <summary>Nhận chỉ số đèn; trả true nếu đang sáng (ngoài khoảng trả false).</summary>
        public bool LampOn(int lamp) { return lamp >= 0 && lamp < Size && (Lamps >> lamp & 1) == 1; }

        /// <summary>Nhận chỉ số đèn; trả true nếu đèn phải sáng ở đích (ngoài khoảng trả false).</summary>
        public bool TargetOn(int lamp) { return lamp >= 0 && lamp < Size && (TargetLamps >> lamp & 1) == 1; }

        /// <summary>Trả tỉ lệ 0..1 đã giữ khớp theo lần chụp của host (1 nếu đã thắng); dùng cho thanh tiến độ khi không ngoại suy.</summary>
        public float HoldFraction()
        {
            if (Solved) return 1f;
            if (HoldSeconds <= 0f) return Matching ? 1f : 0f;
            float p = HoldElapsed / HoldSeconds;
            return p < 0f ? 0f : (p > 1f ? 1f : p);
        }
    }
}
