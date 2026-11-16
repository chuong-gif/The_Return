/*
 * Mục đích: Một DÒNG người chơi trong bảng sảnh online (T-054): giữ tham chiếu tới số ô, tên, vai, trạng thái, mic, âm lượng, nút mời ra. Không chứa logic.
 * Tách riêng file vì Unity chỉ lưu được component khi tên file trùng tên lớp (trước đây nằm chung LobbyOnlineView.cs nên scene mất script).
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn.Network.LobbyOnline
{
    /// <summary>Một dòng người chơi trong bảng phòng: số ô, tên, vai, trạng thái, mic, âm lượng, nút mời ra.</summary>
    public sealed class LobbyOnlinePlayerRow : MonoBehaviour
    {
        public Image background;
        public Text slotText;
        public Text nameText;
        public Text roleText;
        public Image statusChip;
        public Text statusText;
        public Button micButton;
        public Text micLabel;
        public Slider volumeSlider;
        public Text volumeText;
        public Button kickButton;

        /// <summary>Mã client của người đang ở dòng này (do controller đặt mỗi lần làm mới; 0 khi trống).</summary>
        [System.NonSerialized] public ulong ClientId;
        [System.NonSerialized] public bool Occupied;
    }
}
