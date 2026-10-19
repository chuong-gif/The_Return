/*
 * Mục đích: Biển ký hiệu trên tường hành lang của «Đèn lồng chung» (T-006f). Chỉ hiện ký hiệu khi nằm trong vùng sáng; không
 * quyết định luật. Prototype gọi Show mỗi frame.
 * Hàm: Show đặt chữ và màu theo trạng thái sáng.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class LanternSign : MonoBehaviour
    {
        [Tooltip("Số thứ tự biển 0-3 (biển số 1 là 0). Phải khớp thứ tự trên tường.")]
        public int index;
        public TextMesh label;

        /// <summary>Nhận chữ ký hiệu và biển có đang trong vùng sáng không; trong sáng hiện chữ vàng, ngoài sáng hiện dấu hỏi xám tối.</summary>
        public void Show(string symbol, bool lit)
        {
            if (label == null) return;
            label.text = lit ? symbol : "?";
            label.color = lit ? new Color(1f, 0.9f, 0.4f) : new Color(0.25f, 0.25f, 0.28f);
        }
    }
}
