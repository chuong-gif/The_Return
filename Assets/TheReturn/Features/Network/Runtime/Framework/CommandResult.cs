/*
 * Mục đích: Các kiểu kết quả dùng chung của khung mạng host-authoritative (T-024): lý do từ chối một lệnh (RejectReason),
 * phán quyết của luật nhiệm vụ (RuleVerdict) và kết quả host trả về cho một lệnh của máy khách (CommandResult).
 * Thuần C#, không phụ thuộc Unity; lớp mạng thật (NGO) chỉ việc chuyển các giá trị này đi và về.
 * Hàm: RuleVerdict.Accept; RuleVerdict.Reject; CommandResult.Accepted; CommandResult.Rejected.
 */
namespace TheReturn.Network.Framework
{
    /// <summary>Vì sao host từ chối một lệnh.</summary>
    public enum RejectReason
    {
        /// <summary>Không bị từ chối (chỉ là giá trị mặc định).</summary>
        None,
        /// <summary>Lệnh rỗng (null).</summary>
        NullCommand,
        /// <summary>Ô người gửi nằm ngoài đội.</summary>
        InvalidSlot,
        /// <summary>Người gửi không còn kết nối.</summary>
        NotConnected,
        /// <summary>Số thứ tự lệnh cũ hoặc trùng (lệnh bị gửi lại).</summary>
        StaleSequence,
        /// <summary>Luật nhiệm vụ không cho phép lệnh này.</summary>
        RejectedByRules,
        /// <summary>Luật nhiệm vụ ném lỗi khi kiểm hoặc chạy lệnh.</summary>
        RuleError
    }

    /// <summary>Phán quyết của luật nhiệm vụ cho một lệnh: chấp nhận, hoặc từ chối kèm lý do.</summary>
    public readonly struct RuleVerdict
    {
        /// <summary>True nếu luật cho phép lệnh.</summary>
        public bool IsAccepted { get; }

        /// <summary>Lý do từ chối (rỗng khi chấp nhận).</summary>
        public string Reason { get; }

        RuleVerdict(bool isAccepted, string reason)
        {
            IsAccepted = isAccepted;
            Reason = reason;
        }

        /// <summary>Phán quyết chấp nhận.</summary>
        public static RuleVerdict Accept()
        {
            return new RuleVerdict(true, string.Empty);
        }

        /// <summary>Nhận lý do (có thể rỗng); trả phán quyết từ chối.</summary>
        public static RuleVerdict Reject(string reason)
        {
            return new RuleVerdict(false, string.IsNullOrEmpty(reason) ? "Luật nhiệm vụ từ chối lệnh." : reason);
        }
    }

    /// <summary>Kết quả host trả về cho một lệnh của máy khách.</summary>
    public readonly struct CommandResult
    {
        /// <summary>True nếu lệnh đã được chạy.</summary>
        public bool IsAccepted { get; }

        /// <summary>Lý do bị từ chối (None khi được chấp nhận).</summary>
        public RejectReason Reason { get; }

        /// <summary>Mô tả ngắn lý do từ chối (rỗng khi được chấp nhận).</summary>
        public string Detail { get; }

        /// <summary>Phiên bản trạng thái của host sau khi xử lý lệnh.</summary>
        public int Version { get; }

        CommandResult(bool isAccepted, RejectReason reason, string detail, int version)
        {
            IsAccepted = isAccepted;
            Reason = reason;
            Detail = detail;
            Version = version;
        }

        /// <summary>Nhận phiên bản mới; trả kết quả chấp nhận.</summary>
        public static CommandResult Accepted(int version)
        {
            return new CommandResult(true, RejectReason.None, string.Empty, version);
        }

        /// <summary>Nhận lý do, mô tả và phiên bản hiện tại; trả kết quả từ chối.</summary>
        public static CommandResult Rejected(RejectReason reason, string detail, int version)
        {
            return new CommandResult(false, reason, detail ?? string.Empty, version);
        }
    }
}
