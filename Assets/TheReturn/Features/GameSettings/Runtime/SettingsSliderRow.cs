using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Dòng thanh trượt: mã dòng, thanh trượt và ô chữ hiện giá trị.</summary>
    public sealed class SettingsSliderRow : MonoBehaviour
    {
        public string id;
        public Slider slider;
        public Text valueText;
        public SliderFormat format = SliderFormat.Percent;

        /// <summary>Nhận giá trị; trả chữ hiển thị theo định dạng đã chọn (phần trăm hoặc hệ số nhân).</summary>
        public string Format(float value)
        {
            return format == SliderFormat.Percent ? Mathf.RoundToInt(value * 100f) + "%" : "x" + value.ToString("0.0");
        }

        /// <summary>Nhận khoảng và giá trị; đặt thanh trượt mà không bắn sự kiện, cập nhật chữ.</summary>
        public void Set(float min, float max, float value)
        {
            if (slider == null) return;
            slider.minValue = min; slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            if (valueText != null) valueText.text = Format(value);
        }

        /// <summary>Nhận hàm xử lý; mỗi lần kéo thì cập nhật chữ rồi gọi hàm đó.</summary>
        public void Listen(Action<float> onChanged)
        {
            if (slider == null) return;
            slider.onValueChanged.AddListener(v => { if (valueText != null) valueText.text = Format(v); onChanged(v); });
        }
    }
}
