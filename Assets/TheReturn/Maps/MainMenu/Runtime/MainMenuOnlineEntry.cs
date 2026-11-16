/*
 * Mục đích: Nút «Chơi online» ở menu chính (T-046). Gắn lên chính GameObject của nút. T-054: nếu gán onlinePanel thì bấm sẽ mở panel Online (tạo/vào phòng bằng mã) thay cho menu chính;
 * nếu không gán thì nạp scene phòng online cũ (mặc định OnlineRoom).
 * Chế độ offline (nút Chơi -> Sanh_Cho) KHÔNG đổi; file này không sửa MainMenuController.
 * Hàm: Awake; OnClick.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Nút vào đường online: Menu -> scene phòng online.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class MainMenuOnlineEntry : MonoBehaviour
    {
        [Tooltip("Tên scene phòng online (phải có trong Build Settings).")]
        [SerializeField] private string onlineRoomSceneName = "OnlineRoom";

        [Tooltip("Panel Online (tạo phòng / nhập mã). Để trống = nạp scene OnlineRoom như cũ.")]
        [SerializeField] private GameObject onlinePanel;
        [Tooltip("Panel menu chính sẽ ẩn khi mở panel Online.")]
        [SerializeField] private GameObject mainPanelToHide;

        private bool loading;

        /// <summary>Không nhận tham số; nối sự kiện bấm của nút với OnClick.</summary>
        private void Awake()
        {
            Button button = GetComponent<Button>();
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        /// <summary>Không nhận tham số; mở khóa chuột rồi nạp scene phòng online (chỉ một lần).</summary>
        public void OnClick()
        {
            if (onlinePanel != null)
            {
                if (mainPanelToHide != null) mainPanelToHide.SetActive(false);
                onlinePanel.SetActive(true);
                return;
            }
            if (loading) return;
            loading = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (SceneTransitionService.Instance == null) new GameObject("SceneTransitionService").AddComponent<SceneTransitionService>();
            SceneTransitionService.Instance.LoadScene(onlineRoomSceneName);
        }
    }
}
