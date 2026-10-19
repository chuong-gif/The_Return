/*
 * Mục đích: Một ô kính của «Cầu kính» (T-006e2). Chỉ là hình + va chạm: không quyết định luật.
 * Khi vỡ: ô mất mặt đi được (walkable tắt), bật rào cao (barrier) để không ai bước vào chỗ vỡ, đổi sang màu đỏ.
 * Hàm: SetBroken đặt/bỏ trạng thái vỡ; Flash nháy màu phản hồi; Update trả về màu nghỉ; OnEnable áp màu nghỉ.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class GlassTile : MonoBehaviour
    {
        public int row;
        public int side;
        public Renderer glass;
        [Tooltip("Mặt đi được; tắt khi ô vỡ.")]
        public BoxCollider walkable;
        [Tooltip("Rào cao phủ chỗ ô; chỉ bật khi ô vỡ.")]
        public BoxCollider barrier;
        public bool IsBroken { get; private set; }

        static readonly Color Rest = new Color(.55f, .85f, 1f);
        static readonly Color BrokenColor = new Color(.7f, .1f, .1f);
        MaterialPropertyBlock properties;
        float flashUntil;

        /// <summary>Nhận trạng thái vỡ; bật/tắt mặt đi, rào và đổi màu.</summary>
        public void SetBroken(bool broken)
        {
            IsBroken = broken;
            flashUntil = 0;
            if (walkable != null) walkable.enabled = !broken;
            if (barrier != null) barrier.enabled = broken;
            Apply(broken ? BrokenColor : Rest);
        }

        /// <summary>Nhận màu và thời lượng giây; ô sáng màu đó rồi trở về màu nghỉ (hoặc màu vỡ).</summary>
        public void Flash(Color color, float seconds)
        {
            flashUntil = Time.time + seconds;
            Apply(color);
        }

        /// <summary>Không nhận tham số; hết thời gian nháy thì trả về màu nghỉ.</summary>
        void Update()
        {
            if (flashUntil > 0 && Time.time >= flashUntil)
            {
                flashUntil = 0;
                Apply(IsBroken ? BrokenColor : Rest);
            }
        }

        /// <summary>Không nhận tham số; đặt trạng thái ban đầu khi bật.</summary>
        void OnEnable() { SetBroken(IsBroken); }

        /// <summary>Nhận màu; ghi vào renderer bằng MaterialPropertyBlock, không tạo material mới.</summary>
        void Apply(Color color)
        {
            if (glass == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", color);
            glass.SetPropertyBlock(properties);
        }
    }
}
