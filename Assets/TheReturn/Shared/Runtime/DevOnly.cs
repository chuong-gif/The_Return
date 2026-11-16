/*
 * Mục đích: Đánh dấu đối tượng CHỈ DÀNH CHO NHÀ PHÁT TRIỂN (nút thêm người giả lập, bảng sảnh dev...). Giữ nguyên khi chạy trong Editor và Development Build;
 * bản build thật (Release) tự gỡ đối tượng ngay khi nạp, nên người chơi không bao giờ thấy.
 * Hàm: Awake.
 */
using UnityEngine;

namespace TheReturn
{
    /// <summary>Gắn lên đối tượng chỉ nhà phát triển dùng; bản build thật tự xóa đối tượng này.</summary>
    [DisallowMultipleComponent]
    public sealed class DevOnly : MonoBehaviour
    {
        /// <summary>Không nhận tham số; ở bản build thật (không Editor, không Development Build) thì xóa cả đối tượng.</summary>
        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            Destroy(gameObject);
#endif
        }
    }
}
