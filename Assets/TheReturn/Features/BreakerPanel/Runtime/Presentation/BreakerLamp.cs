/*
 * Mục đích: Đèn nhìn thấy của Bảng cầu dao (T-006a2) kèm chấm đèn đích; chỉ hiển thị.
 * T-056: đèn có số riêng «ĐÈN n» (nhãn tạo lúc chạy từ nhãn công tắc mẫu, không cần sửa scene).
 * Hàm: Show đặt màu đèn hiện tại và màu chấm đích; EnsureNumberLabel tạo nhãn số.
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
        TextMesh numberLabel;

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

        /// <summary>Nhận nhãn mẫu (nhãn của một công tắc); nếu đèn chưa có nhãn số thì tạo «ĐÈN n» phía dưới đèn, cùng font/vật liệu và hướng đọc với nhãn công tắc.</summary>
        public void EnsureNumberLabel(TextMesh template)
        {
            if (numberLabel != null || template == null) return;
            var go = new GameObject("NumberLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -.95f, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            numberLabel = go.AddComponent<TextMesh>();
            numberLabel.font = template.font;
            numberLabel.fontSize = 60;
            numberLabel.characterSize = .07f;
            numberLabel.anchor = TextAnchor.MiddleCenter;
            numberLabel.alignment = TextAlignment.Center;
            numberLabel.color = new Color(1f, .95f, .6f);
            numberLabel.text = "ĐÈN " + (index + 1);
            var templateRenderer = template.GetComponent<MeshRenderer>();
            var renderer = go.GetComponent<MeshRenderer>();
            if (templateRenderer != null && renderer != null) renderer.sharedMaterial = templateRenderer.sharedMaterial;
        }
    }
}
