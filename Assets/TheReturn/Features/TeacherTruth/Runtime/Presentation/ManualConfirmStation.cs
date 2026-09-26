/*
 * Mục đích: Đánh dấu nút xác nhận thủ công và hiển thị trạng thái kênh trong thế giới.
 * Hàm: SetVisual đổi nhãn theo người xác nhận; không tự sửa luật.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class ManualConfirmStation : MonoBehaviour
    {
        [Range(0, 3)] public int channel;
        public TextMesh label;

        /// <summary>Nhận ID người xác nhận hoặc -1; cập nhật nhãn thế giới, không trả dữ liệu.</summary>
        public void SetVisual(int actor)
        {
            if (label == null) return;
            label.text = actor < 0
                ? "XÁC NHẬN THỦ CÔNG " + (channel + 1) + "\n[E]"
                : "KÊNH " + (channel + 1) + " ĐÃ CHỐT\nVAI " + (actor + 1);
        }
    }
}
