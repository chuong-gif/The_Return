/*
 * Mục đích: Gói trạng thái host gửi cho MỘT người chơi (T-024): số phiên bản, trạng thái chung và thông tin RIÊNG của đúng người đó.
 * Gói bất biến; mỗi người nhận một gói riêng nên thông tin riêng của người này không bao giờ nằm trong gói của người kia.
 * NoPrivate dùng cho nhiệm vụ không có thông tin riêng. Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor StateMessage.
 */
namespace TheReturn.Network.Framework
{
    /// <summary>Giá trị dùng cho TPrivate khi nhiệm vụ không có thông tin riêng.</summary>
    public readonly struct NoPrivate { }

    /// <summary>Trạng thái host gửi cho đúng một người.</summary>
    public sealed class StateMessage<TPublic, TPrivate>
    {
        /// <summary>Ô của người nhận.</summary>
        public int Slot { get; }

        /// <summary>Phiên bản trạng thái; máy khách bỏ qua gói có phiên bản không mới hơn.</summary>
        public int Version { get; }

        /// <summary>Trạng thái chung mọi người cùng thấy.</summary>
        public TPublic Public { get; }

        /// <summary>Thông tin riêng của người nhận.</summary>
        public TPrivate Private { get; }

        /// <summary>Nhận ô người nhận, phiên bản, trạng thái chung và thông tin riêng.</summary>
        public StateMessage(int slot, int version, TPublic publicState, TPrivate privateState)
        {
            Slot = slot;
            Version = version;
            Public = publicState;
            Private = privateState;
        }
    }
}
