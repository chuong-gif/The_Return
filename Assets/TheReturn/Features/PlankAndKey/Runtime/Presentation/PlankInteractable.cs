/*
 * Mục đích: Đánh dấu vật tương tác của nhiệm vụ «Cầu ván và thẻ từ» (thẻ từ, cần gạt hai bờ) để PlankAndKeyPrototype nhận ra khi người chơi bấm E (T-006b2).
 * Hàm: không có; chỉ giữ loại vật.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Loại vật tương tác của cầu ván.</summary>
    public enum PlankInteractKind { Card, LeverA, LeverB }

    /// <summary>Gắn lên thẻ từ và hai cần gạt; collider phải KHÔNG phải trigger (tia tương tác bỏ qua trigger).</summary>
    public sealed class PlankInteractable : MonoBehaviour
    {
        public PlankInteractKind kind;
    }
}
