// Mục đích: NetworkTransform do OWNER có quyền quyết định vị trí (thay vì server), để người chơi tự di chuyển mượt (T-009 S1).
// Hàm: OnIsServerAuthoritative.
using Unity.Netcode.Components;

namespace TheReturn.Network
{
    public sealed class OwnerNetworkTransform : NetworkTransform
    {
        /// <summary>Trả false để owner là bên có quyền ghi transform. Nhận: không. Trả: bool.</summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
