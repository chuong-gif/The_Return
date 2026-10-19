/*
 * Mục đích: Công tắc nhìn thấy của Bảng cầu dao (T-006a2); chỉ hiển thị, không quyết định luật.
 * Hàm: Show đặt màu và nhãn theo trạng thái bật/tắt và chủ.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class BreakerSwitch : MonoBehaviour
    {
        public int index;
        public Renderer indicator;
        public TextMesh label;
        MaterialPropertyBlock properties;
        int previous = -99;

        /// <summary>Nhận trạng thái bật và chủ (ID người); chỉ cập nhật khi đổi, không tạo material mới.</summary>
        public void Show(bool on, int owner)
        {
            int key = (on ? 100 : 0) + owner;
            if (key == previous) return;
            previous = key;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", on ? new Color(.95f, .75f, .1f) : new Color(.35f, .35f, .4f));
            if (indicator != null) indicator.SetPropertyBlock(properties);
            if (label != null) label.text = (index + 1) + " / VAI " + (owner + 1) + (on ? " BẬT" : "");
        }
    }
}
