/*
 * Mục đích: Asset chỉnh được trong Inspector cho nhiệm vụ Phong_5 «Bảng cầu dao» (T-006a2).
 * Thành viên: holdSeconds (giây phải giữ khớp đích), fixedSeed, chữ hiển thị.
 */
using UnityEngine;

namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Breaker Settings", fileName = "BreakerSettings")]
    public sealed class BreakerSettings : ScriptableObject
    {
        [Tooltip("Số giây phải giữ đèn khớp đích liên tục để thắng.")]
        [Min(0f)] public float holdSeconds = 3f;
        [Tooltip("0 = đề ngẫu nhiên mỗi lần chơi; khác 0 = luôn cùng một đề (để test).")]
        public int fixedSeed;

        [Header("Chữ hiển thị")]
        [TextArea] public string instructions = "Mỗi người chỉ bấm được công tắc của mình và chỉ xem được công tắc đó đảo những đèn nào. Khớp hàng đèn với hàng đích (không bật quá giới hạn công tắc).";
        public string tripped = "Cầu dao nhảy! Bật quá nhiều công tắc, tất cả về tắt.";
        public string notOwner = "Công tắc này không phải của vai bạn đang điều khiển.";
        public string success = "Bảng cầu dao ổn định!";
    }
}
