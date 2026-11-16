/*
 * Mục đích: Đánh dấu một điều khiển UGUI (hoặc cả nhóm con) KHÔNG phát âm giao diện (UiSoundDirector bỏ qua). Dùng cho nút có âm riêng hoặc nút cần im lặng.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Gắn lên nút/nhóm nút để tắt âm giao diện chung cho chúng.</summary>
    public sealed class UiSoundMute : MonoBehaviour { }
}
