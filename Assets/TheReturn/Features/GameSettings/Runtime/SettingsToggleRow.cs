using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Dòng công tắc bật/tắt: mã dòng, Toggle và hình công tắc (nếu có).</summary>
    public sealed class SettingsToggleRow : MonoBehaviour
    {
        public string id;
        public Toggle toggle;
        public ModernSwitchVisual visual;

        /// <summary>Nhận giá trị; đặt công tắc mà không bắn sự kiện và vẽ lại hình công tắc.</summary>
        public void Set(bool on)
        {
            if (toggle == null) return;
            toggle.SetIsOnWithoutNotify(on);
            if (visual != null) visual.Refresh(on);
        }

        /// <summary>Nhận hàm xử lý; gọi mỗi khi người chơi bật/tắt.</summary>
        public void Listen(Action<bool> onChanged)
        {
            if (toggle != null) toggle.onValueChanged.AddListener(v => onChanged(v));
        }
    }
}
