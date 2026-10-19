/*
 * Mục đích: Phím trên bảng nhập cuối hành lang của «Đèn lồng chung» (T-006f): phím ký hiệu A-F, XÓA và XÁC NHẬN. Chỉ để
 * raycast nhận ra; luật nằm ở LanternState.
 * Hàm: OnEnable đặt chữ trên phím.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class LanternPanelKey : MonoBehaviour
    {
        public enum Kind { Symbol, Clear, Submit }

        public Kind kind;
        [Tooltip("Chỉ số ký hiệu 0-5 khi kind = Symbol.")]
        public int symbol;
        public TextMesh label;

        /// <summary>Không nhận tham số; đặt chữ phím: tên ký hiệu, XÓA hoặc XÁC NHẬN.</summary>
        void OnEnable()
        {
            if (label == null) return;
            label.text = kind == Kind.Symbol ? LanternSymbols.Name(symbol) : (kind == Kind.Clear ? "XÓA" : "XÁC NHẬN");
        }
    }
}
