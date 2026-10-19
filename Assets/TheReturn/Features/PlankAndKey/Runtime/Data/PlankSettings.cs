/*
 * Mục đích: Quản lý thông số và lời hướng dẫn nhiệm vụ «Cầu ván và thẻ từ» (Hanh_Lang_6) bằng asset Inspector (T-006b2).
 * Hàm: không có; đây là dữ liệu cấu hình, không xử lý gameplay.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Thông số chỉnh được: giây thật mỗi đơn vị thời gian, phần dư của hạn và các câu chữ tiếng Việt.</summary>
    [CreateAssetMenu(menuName = "The Return/Plank And Key Settings")]
    public sealed class PlankSettings : ScriptableObject
    {
        /// <summary>Số giây thật cho mỗi đơn vị thời gian của đề (giả định 3, cần playtest).</summary>
        [Min(0.1f)] public float secondsPerUnit = 3f;
        /// <summary>Phần dư (%) cộng vào thời gian tối ưu để ra hạn (thiết kế: 20-25%).</summary>
        [Range(0, 200)] public int slackPercent = 25;
        public string title = "CẦU VÁN VÀ THẺ TỪ";
        [TextArea(3, 6)] public string instructions = "Cả đội phải qua khe sang bờ bên kia. Ván chỉ chở tối đa 2 người và chỉ hạ xuống khi có thẻ từ trên ván, nên phải có người mang thẻ quay về đón những người còn lại. Mỗi lượt tốn thời gian của người chậm hơn. Hết thời gian thì cả đội về đầu khe.";
        public string failure = "Hết thời gian! Cả đội về đầu khe. Hãy thử lại.";
        public string success = "Cả đội đã qua khe! Đã hoàn thành Cầu ván và thẻ từ.";
        public string waiting = "Cả đội đứng đủ ở vùng chuẩn bị để bắt đầu.";
    }
}
