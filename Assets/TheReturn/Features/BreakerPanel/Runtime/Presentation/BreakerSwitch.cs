/*
 * Mục đích: Công tắc nhìn thấy của Bảng cầu dao (T-006a2); chỉ hiển thị, không quyết định luật.
 * T-056: công tắc TẮT mang màu riêng của vai chủ (xanh dương, cam, tím, hồng), BẬT thì vàng sáng; nhãn «số [BẬT] / VAI x».
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
        static readonly Color[] OwnerColors =
        {
            new Color(.25f, .50f, .95f), new Color(.95f, .55f, .20f), new Color(.65f, .40f, .90f), new Color(.92f, .38f, .58f)
        };
        MaterialPropertyBlock properties;
        int previous = -99;

        /// <summary>Nhận trạng thái bật và chủ (ID người); chỉ cập nhật khi đổi, không tạo material mới.</summary>
        public void Show(bool on, int owner)
        {
            int key = (on ? 100 : 0) + owner;
            if (key == previous) return;
            previous = key;
            if (properties == null) properties = new MaterialPropertyBlock();
            Color own = OwnerColors[Mathf.Clamp(owner, 0, OwnerColors.Length - 1)];
            properties.SetColor("_BaseColor", on ? new Color(1f, .85f, .15f) : own * .8f);
            if (indicator != null) indicator.SetPropertyBlock(properties);
            if (label != null) label.text = (index + 1) + (on ? " BẬT" : "") + "\nVAI " + (owner + 1);
        }
    }
}
