/*
 * Mục đích: Điểm tương tác giữ nút và đèn trạng thái có thể thay visual.
 * Hàm: Show nhận người giữ và cập nhật màu/nhãn, không quyết định luật.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class QuietHoldStation : MonoBehaviour
    {
        public int index;
        public Renderer indicator;
        public TextMesh label;
        MaterialPropertyBlock properties;
        int previous = -99;
        /// <summary>Nhận ID người giữ hoặc -1; cập nhật visual khi đổi trạng thái, không tạo material mới.</summary>
        public void Show(int owner)
        {
            if (owner == previous) return;
            previous = owner;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", owner < 0 ? new Color(.8f,.46f,.1f) : new Color(.1f,.8f,.35f));
            indicator.SetPropertyBlock(properties);
            label.text = (index == 0 ? "A" : "B") + (owner < 0 ? " / E: GIU" : " / VAI " + (owner + 1));
        }
    }
}
