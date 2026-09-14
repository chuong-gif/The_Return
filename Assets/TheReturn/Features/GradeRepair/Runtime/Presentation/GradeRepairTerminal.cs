/*
 * Mục đích: Điểm tương tác cho bảng sửa hoặc tài liệu; visual thay được qua prefab.
 * Hàm: SetLabel nhận chữ và cập nhật nhãn trong thế giới.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class GradeRepairTerminal : MonoBehaviour
    {
        [Tooltip("-1 là bảng sửa; 0–3 là bốn tài liệu.")]
        public int document = -1;
        public TextMesh label;
        /// <summary>Nhận chữ; cập nhật nhãn nếu đã gán, không thay đổi luật nhiệm vụ.</summary>
        public void SetLabel(string value) { if (label != null) label.text = value; }
    }
}
