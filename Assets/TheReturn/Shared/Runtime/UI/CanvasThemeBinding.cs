/*
 * Mục đích: Áp style dùng chung cho một Image hoặc Text, giữ RectTransform của designer.
 * Hàm: OnEnable áp theme khi bật; Apply có menu Inspector để xem thay đổi ngay.
 */
using UnityEngine;
using UnityEngine.UI;
namespace TheReturn
{
    public sealed class CanvasThemeBinding : MonoBehaviour
    {
        public SchoolUITheme theme;
        public bool button, accent;
        /// <summary>Không nhận tham số; áp theme khi đối tượng bật, không sinh tài sản mới.</summary>
        void OnEnable() { Apply(); }
        /// <summary>Không nhận tham số; gán font/màu/sprite có trong theme, giữ ảnh riêng nếu theme để trống.</summary>
        [ContextMenu("Apply Theme")]
        public void Apply()
        {
            if(theme==null)return;
            var text=GetComponent<Text>();
            if(text!=null)
            {
                if(theme.font!=null)text.font=theme.font;
                text.color=accent?theme.accentColor:theme.textColor;
            }
            var image=GetComponent<Image>();
            if(image!=null)
            {
                var sprite=button?theme.buttonSprite:theme.panelSprite;
                if(sprite!=null){image.sprite=sprite;image.type=Image.Type.Sliced;}
                image.color=accent?theme.accentColor:button?theme.buttonColor:theme.panelColor;
            }
        }
    }
}
