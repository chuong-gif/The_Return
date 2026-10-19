/*
 * Mục đích: Đèn nhìn thấy của Bảng cầu dao (T-006a2) kèm chấm đèn đích; chỉ hiển thị.
 * Hàm: Show đặt màu đèn hiện tại và màu chấm đích.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class BreakerLamp : MonoBehaviour
    {
        public int index;
        public Renderer lamp;
        public Renderer targetMarker;
        MaterialPropertyBlock lampProps, targetProps;
        int previous = -99;

        /// <summary>Nhận đèn đang sáng và đèn đích sáng; chỉ cập nhật khi đổi.</summary>
        public void Show(bool lit, bool targetLit)
        {
            int key = (lit ? 1 : 0) + (targetLit ? 2 : 0);
            if (key == previous) return;
            previous = key;
            if (lampProps == null) { lampProps = new MaterialPropertyBlock(); targetProps = new MaterialPropertyBlock(); }
            lampProps.SetColor("_BaseColor", lit ? new Color(1f, .9f, .3f) : new Color(.15f, .15f, .18f));
            if (lamp != null) lamp.SetPropertyBlock(lampProps);
            targetProps.SetColor("_BaseColor", targetLit ? new Color(.2f, .9f, .4f) : new Color(.25f, .25f, .3f));
            if (targetMarker != null) targetMarker.SetPropertyBlock(targetProps);
        }
    }
}
