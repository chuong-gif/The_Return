/*
 * Mục đích: Gán font cho mọi Text dưới Canvas lúc chạy (T-053). Font hệ điều hành (Bahnschrift...) không lưu được vào scene, nên scene giữ font dự phòng
 * và script này gán font thật. Kéo một file font (TTF/OTF) vào ô Override Font để đổi font cả Canvas; để trống = dùng ModernUI.Font.
 * Hàm: Awake; Start; Apply.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Gán font thống nhất cho mọi Text con; đặt trên object Canvas.</summary>
    [DisallowMultipleComponent]
    public sealed class UiFontApplier : MonoBehaviour
    {
        [Tooltip("Để trống = dùng font hiện đại của ModernUI (Bahnschrift/Segoe UI...). Kéo file font vào đây để đổi font toàn Canvas.")]
        [SerializeField] private Font overrideFont;

        /// <summary>Không nhận tham số; gán font ngay khi Canvas thức dậy.</summary>
        private void Awake() { Apply(); }

        /// <summary>Không nhận tham số; gán lại lần nữa để phủ cả Text do code nhân bản sau Awake.</summary>
        private void Start() { Apply(); }

        /// <summary>Không nhận tham số; gán font đã chọn cho mọi Text dưới object này (kể cả đang tắt).</summary>
        public void Apply()
        {
            Font f = overrideFont != null ? overrideFont : ModernUI.Font;
            if (f == null) return;
            foreach (Text t in GetComponentsInChildren<Text>(true)) if (t.font != f) t.font = f;
        }
    }
}
