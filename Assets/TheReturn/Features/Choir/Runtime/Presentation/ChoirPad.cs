/*
 * Mục đích: Bục bấm của Hợp xướng, chỉ hiện SỐ 1-4 và đèn trạng thái; không quyết định luật.
 * Hàm: Flash đặt màu phản hồi đúng/sai/nghỉ.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class ChoirPad : MonoBehaviour
    {
        public int index;
        public Renderer indicator;
        public TextMesh label;
        MaterialPropertyBlock properties;
        Color current = new Color(.8f, .46f, .1f);
        float flashUntil;
        Color flashColor;

        /// <summary>Nhận màu và thời lượng giây; bục sáng màu đó rồi quay về màu nghỉ.</summary>
        public void Flash(Color color, float seconds)
        {
            flashColor = color; flashUntil = Time.time + seconds; Apply(color);
        }

        /// <summary>Không nhận tham số; trả về màu nghỉ khi hết thời gian nháy.</summary>
        void Update()
        {
            if (flashUntil > 0 && Time.time >= flashUntil) { flashUntil = 0; Apply(new Color(.8f, .46f, .1f)); }
        }

        /// <summary>Nhận màu; ghi vào renderer bằng MaterialPropertyBlock, không tạo material mới.</summary>
        void Apply(Color color)
        {
            if (indicator == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            current = color;
            properties.SetColor("_BaseColor", color);
            indicator.SetPropertyBlock(properties);
        }

        /// <summary>Không nhận tham số; đặt chữ số cho bục lúc bật.</summary>
        void OnEnable()
        {
            if (label != null) label.text = (index + 1).ToString();
            Apply(new Color(.8f, .46f, .1f));
        }
    }
}
