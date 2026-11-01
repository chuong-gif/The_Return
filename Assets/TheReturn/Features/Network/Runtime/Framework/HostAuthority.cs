/*
 * Mục đích: Lõi khung mạng HOST-AUTHORITATIVE (T-024): máy khách gửi LỆNH, host kiểm rồi chạy luật, rồi gửi TRẠNG THÁI.
 * Host kiểm theo thứ tự: ô người gửi hợp lệ, còn kết nối, lệnh không rỗng, số thứ tự lớn hơn lệnh trước của người đó
 * (chặn lệnh gửi lại), rồi mới hỏi luật nhiệm vụ. Lệnh bị từ chối luôn được ghi nhật ký và không làm đổi trạng thái.
 * Mỗi lệnh được chấp nhận làm Version tăng 1. Mỗi người nhận một gói riêng: trạng thái chung + thông tin riêng của đúng người đó.
 * Thuần C#, không phụ thuộc Unity: lớp NGO bên ngoài chỉ chuyển lệnh vào Submit và chuyển gói BuildBroadcast ra mạng.
 * Hàm: constructor; Submit; MarkChanged; ConnectSlot; DisconnectSlot; BuildBroadcast; BuildStateFor.
 */
using System;

namespace TheReturn.Network.Framework
{
    /// <summary>Host của một ván: nhận lệnh từ các ô, chạy luật nhiệm vụ, phát trạng thái.</summary>
    public sealed class HostAuthority<TCommand, TPublic, TPrivate>
    {
        readonly TeamRoster roster;
        readonly IHostRules<TCommand, TPublic, TPrivate> rules;
        readonly HostLog log;
        readonly int[] lastSequence;
        int version;

        /// <summary>Đội đang chơi (2-4 người, cố định).</summary>
        public TeamRoster Roster { get { return roster; } }

        /// <summary>Nhật ký của host.</summary>
        public HostLog Log { get { return log; } }

        /// <summary>Phiên bản trạng thái hiện tại; bắt đầu từ 0.</summary>
        public int Version { get { return version; } }

        /// <summary>Có ghi cả lệnh được chấp nhận vào nhật ký không (mặc định không, tránh tràn nhật ký).</summary>
        public bool LogAccepted { get; set; }

        /// <summary>Nhận đội, luật nhiệm vụ và nhật ký (null thì tự tạo). Ném lỗi nếu đội hoặc luật là null.</summary>
        public HostAuthority(TeamRoster roster, IHostRules<TCommand, TPublic, TPrivate> rules, HostLog log)
        {
            if (roster == null) throw new ArgumentNullException("roster");
            if (rules == null) throw new ArgumentNullException("rules");
            this.roster = roster;
            this.rules = rules;
            this.log = log ?? new HostLog();
            lastSequence = new int[roster.Size];
        }

        /// <summary>
        /// Xử lý một lệnh. Nhận ô người gửi (host lấy từ kết nối, không tin nội dung lệnh), số thứ tự lệnh của người đó
        /// (tăng dần, bắt đầu từ 1) và lệnh. Trả kết quả; lệnh bị từ chối thì trạng thái luật không đổi. Lỗi ném ra từ
        /// luật nhiệm vụ được bắt lại để một lệnh xấu không làm sập host.
        /// </summary>
        public CommandResult Submit(int senderSlot, int sequence, TCommand command)
        {
            if (!roster.IsValidSlot(senderSlot))
                return Reject(RejectReason.InvalidSlot, senderSlot, sequence, "Ô người gửi không hợp lệ.");
            if (!roster.IsConnected(senderSlot))
                return Reject(RejectReason.NotConnected, senderSlot, sequence, "Người gửi không còn kết nối.");
            if (command == null)
                return Reject(RejectReason.NullCommand, senderSlot, sequence, "Lệnh rỗng.");
            if (sequence <= lastSequence[senderSlot])
                return Reject(RejectReason.StaleSequence, senderSlot, sequence,
                    string.Format("Số thứ tự {0} không mới hơn lệnh trước ({1}).", sequence, lastSequence[senderSlot]));

            // Số thứ tự được dùng hết ngay cả khi luật từ chối: gửi lại đúng lệnh đó vẫn là lệnh trùng.
            lastSequence[senderSlot] = sequence;

            RuleVerdict verdict;
            try
            {
                verdict = rules.Validate(senderSlot, command);
            }
            catch (Exception ex)
            {
                return Reject(RejectReason.RuleError, senderSlot, sequence, "Luật ném lỗi khi kiểm lệnh: " + ex.Message);
            }
            if (!verdict.IsAccepted)
            {
                string why = string.IsNullOrEmpty(verdict.Reason) ? "Luật nhiệm vụ từ chối lệnh." : verdict.Reason;
                return Reject(RejectReason.RejectedByRules, senderSlot, sequence, why);
            }

            try
            {
                rules.Apply(senderSlot, command);
            }
            catch (Exception ex)
            {
                // Apply có thể đã đổi trạng thái một phần trước khi lỗi: tăng Version để máy khách nhận trạng thái thật.
                version++;
                return Reject(RejectReason.RuleError, senderSlot, sequence, "Luật ném lỗi khi chạy lệnh: " + ex.Message);
            }

            version++;
            if (LogAccepted)
                log.Info(string.Format("Chấp nhận lệnh của ô {0} (số thứ tự {1}), phiên bản {2}.", senderSlot, sequence, version));
            return CommandResult.Accepted(version);
        }

        /// <summary>
        /// Báo trạng thái đã đổi không do lệnh nào (vd luật có đồng hồ tự chạy), để lần phát sau mang phiên bản mới.
        /// </summary>
        public void MarkChanged()
        {
            version++;
        }

        /// <summary>
        /// Một người kết nối hoặc vào lại. Nhận ô người đó. Số thứ tự lệnh của ô được đặt lại về 0 vì máy khách mới
        /// đếm lại từ 1. Ném lỗi nếu ô không hợp lệ.
        /// </summary>
        public void ConnectSlot(int slot)
        {
            roster.SetConnected(slot, true);
            lastSequence[slot] = 0;
            log.Info(string.Format("Ô {0} đã kết nối.", slot));
        }

        /// <summary>Một người mất kết nối: từ giờ lệnh của ô này bị từ chối và ô này không nhận gói. Ném lỗi nếu ô không hợp lệ.</summary>
        public void DisconnectSlot(int slot)
        {
            roster.SetConnected(slot, false);
            log.Info(string.Format("Ô {0} đã mất kết nối.", slot));
        }

        /// <summary>
        /// Tạo gói trạng thái cho MỌI người đang kết nối, theo thứ tự ô tăng dần. Trạng thái chung chụp một lần;
        /// thông tin riêng chụp riêng cho từng ô và chỉ nằm trong gói của ô đó. Trả mảng rỗng nếu không ai kết nối.
        /// </summary>
        public StateMessage<TPublic, TPrivate>[] BuildBroadcast()
        {
            int[] slots = roster.ConnectedSlots();
            StateMessage<TPublic, TPrivate>[] messages = new StateMessage<TPublic, TPrivate>[slots.Length];
            if (slots.Length == 0) return messages;

            TPublic publicState = rules.BuildPublicState();
            for (int i = 0; i < slots.Length; i++)
            {
                messages[i] = new StateMessage<TPublic, TPrivate>(slots[i], version, publicState, rules.BuildPrivateState(slots[i]));
            }
            return messages;
        }

        /// <summary>
        /// Tạo gói ĐẦY ĐỦ cho một người (người vào muộn hoặc vào lại). Nhận ô người đó. Trả null nếu ô không hợp lệ
        /// hoặc người đó không đang kết nối.
        /// </summary>
        public StateMessage<TPublic, TPrivate> BuildStateFor(int slot)
        {
            if (!roster.IsConnected(slot)) return null;
            return new StateMessage<TPublic, TPrivate>(slot, version, rules.BuildPublicState(), rules.BuildPrivateState(slot));
        }

        CommandResult Reject(RejectReason reason, int senderSlot, int sequence, string detail)
        {
            log.Warning(string.Format("Từ chối lệnh của ô {0} (số thứ tự {1}) [{2}]: {3}", senderSlot, sequence, reason, detail));
            return CommandResult.Rejected(reason, detail, version);
        }
    }
}
