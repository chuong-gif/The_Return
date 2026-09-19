/*
 * Mục đích: Asset giao diện dùng chung: font, màu và sprite thay được trong Inspector.
 * Hàm: không có; đây là cấu hình mỹ thuật, không chứa luật hoặc bố cục.
 */
using UnityEngine;
namespace TheReturn
{
    [CreateAssetMenu(menuName="The Return/UI Theme")]
    public sealed class SchoolUITheme : ScriptableObject
    {
        public Font font;
        public Sprite panelSprite, buttonSprite, crosshairSprite;
        public Color panelColor=new Color(.035f,.06f,.085f,.97f);
        public Color buttonColor=new Color(.12f,.27f,.32f,1);
        public Color textColor=new Color(.93f,.96f,.97f,1);
        public Color accentColor=new Color(.25f,.84f,.7f,1);
    }
}
