/*
 * Mục đích: Phía MÁY KHÁCH của khung mạng host-authoritative (T-024): đếm số thứ tự lệnh gửi đi và giữ bản sao trạng thái
 * host gửi về. Máy khách không tự chạy luật, chỉ hiển thị bản sao này. Gói của ô khác, gói cũ hoặc trùng bị bỏ qua nên gói
 * đến trễ hay sai thứ tự không làm trạng thái lùi lại.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; NextSequence; Receive; Reset.
 */
using System;

namespace TheReturn.Network.Framework
{
    /// <summary>Bản sao trạng thái của một người chơi ở máy khách.</summary>
    public sealed class ClientReplica<TPublic, TPrivate>
    {
        int nextSequence = 1;

        /// <summary>Ô của người chơi này.</summary>
        public int Slot { get; private set; }

        /// <summary>True khi đã nhận ít nhất một gói trạng thái.</summary>
        public bool HasState { get; private set; }

        /// <summary>Phiên bản của gói đã nhận gần nhất (0 khi chưa có).</summary>
        public int Version { get; private set; }

        /// <summary>Trạng thái chung mới nhất.</summary>
        public TPublic Public { get; private set; }

        /// <summary>Thông tin riêng mới nhất của người chơi này.</summary>
        public TPrivate Private { get; private set; }

        /// <summary>Nhận ô của người chơi (không âm). Ném lỗi nếu ô âm.</summary>
        public ClientReplica(int slot)
        {
            if (slot < 0) throw new ArgumentOutOfRangeException("slot", slot, "Ô người chơi không được âm.");
            Slot = slot;
        }

        /// <summary>Trả số thứ tự cho lệnh kế tiếp (1, 2, 3...). Mỗi lệnh gửi đi phải dùng một số mới.</summary>
        public int NextSequence()
        {
            return nextSequence++;
        }

        /// <summary>
        /// Nhận một gói trạng thái từ host. Trả true nếu gói được áp dụng; false nếu gói rỗng, gửi cho ô khác,
        /// hoặc có phiên bản không mới hơn gói đã có.
        /// </summary>
        public bool Receive(StateMessage<TPublic, TPrivate> message)
        {
            if (message == null) return false;
            if (message.Slot != Slot) return false;
            if (HasState && message.Version <= Version) return false;
            HasState = true;
            Version = message.Version;
            Public = message.Public;
            Private = message.Private;
            return true;
        }

        /// <summary>Xóa bản sao và đếm lại số thứ tự từ 1; gọi khi kết nối lại (host cũng đặt lại số thứ tự của ô này).</summary>
        public void Reset()
        {
            nextSequence = 1;
            HasState = false;
            Version = 0;
            Public = default(TPublic);
            Private = default(TPrivate);
        }
    }
}
