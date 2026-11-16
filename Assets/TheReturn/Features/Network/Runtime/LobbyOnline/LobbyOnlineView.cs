/*
 * Mục đích: Chỗ GIỮ THAM CHIẾU tới các đối tượng Canvas của sảnh online trong scene Sanh_Cho (T-054). Không chứa logic: LobbyOnlineController đọc phòng rồi đổ dữ liệu vào đây.
 * Muốn đổi bố cục, màu, cỡ chữ: chỉnh thẳng các đối tượng trong Hierarchy (Lobby_OnlineCanvas); chỉ cần giữ các ô tham chiếu ở đây còn nối.
 * Gồm: LobbyOnlineView (cả bảng); dòng người chơi nằm ở LobbyOnlinePlayerRow.cs.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn.Network.LobbyOnline
{
    /// <summary>Toàn bộ bảng phòng online: tiêu đề, mã phòng, 4 dòng người chơi, nút Sẵn sàng / Bắt đầu / Rời phòng, thanh thu gọn, biểu ngữ.</summary>
    public sealed class LobbyOnlineView : MonoBehaviour
    {
        public GameObject panel;
        public GameObject collapsedBar;
        public Text codeText;
        public Text summaryText;
        public Text hintText;
        public Button copyCodeButton;
        public Button collapseButton;
        public Button expandButton;
        public Button readyButton;
        public Text readyLabel;
        public Button startButton;
        public Text startLabel;
        public Button leaveButton;
        public GameObject banner;
        public Text bannerText;
        public LobbyOnlinePlayerRow[] rows = new LobbyOnlinePlayerRow[4];
    }
}
