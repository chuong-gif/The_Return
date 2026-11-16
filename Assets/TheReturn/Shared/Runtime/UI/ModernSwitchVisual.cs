using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Cập nhật hình dạng công tắc (màu nền, vị trí núm) theo trạng thái Toggle. Dùng được cả khi dựng bằng code (Bind) lẫn khi đã gắn sẵn trong prefab (các trường serialize).</summary>
    public sealed class ModernSwitchVisual : MonoBehaviour
    {
        [SerializeField] Toggle toggle;
        [SerializeField] Image background;
        [SerializeField] Image knob;
        [SerializeField] Color onColor = new Color32(64, 214, 179, 255);
        [SerializeField] Color offColor = new Color32(27, 33, 43, 255);
        [SerializeField] Color knobOnColor = new Color32(6, 28, 24, 255);
        [SerializeField] Color knobOffColor = new Color32(142, 154, 171, 255);
        [SerializeField] float knobPadding = 4f;
        bool hooked;

        /// <summary>Nhận Toggle, nền và núm; nối sự kiện đổi giá trị và vẽ trạng thái hiện tại (dùng khi dựng bằng code).</summary>
        public void Bind(Toggle t, Image bg, Image k)
        {
            toggle = t; background = bg; knob = k;
            onColor = ModernUI.Accent; offColor = ModernUI.SurfaceHigh; knobOnColor = ModernUI.AccentText; knobOffColor = ModernUI.TextDim;
            Hook();
        }

        /// <summary>Không nhận tham số; với công tắc đã gắn sẵn trong prefab thì tự nối sự kiện khi thức dậy.</summary>
        void Awake() { Hook(); }

        /// <summary>Không nhận tham số; nối sự kiện đổi giá trị một lần và vẽ trạng thái hiện tại.</summary>
        void Hook()
        {
            if (hooked || toggle == null) return;
            hooked = true;
            toggle.onValueChanged.AddListener(Refresh);
            Refresh(toggle.isOn);
        }

        /// <summary>Nhận trạng thái bật/tắt; đổi màu nền và đẩy núm sang phải hoặc trái.</summary>
        public void Refresh(bool on)
        {
            if (background == null || knob == null) return;
            background.color = on ? onColor : offColor;
            knob.color = on ? knobOnColor : knobOffColor;
            // Neo núm vào mép trái/phải của nền: không phụ thuộc kích thước đã layout.
            RectTransform kn = knob.rectTransform;
            float side = on ? 1f : 0f;
            kn.anchorMin = kn.anchorMax = new Vector2(side, .5f);
            kn.pivot = new Vector2(side, .5f);
            kn.anchoredPosition = new Vector2(on ? -knobPadding : knobPadding, 0f);
        }

        /// <summary>Không nhận tham số; vẽ lại khi bật object.</summary>
        void OnEnable() { if (toggle != null) Refresh(toggle.isOn); }
    }
}
