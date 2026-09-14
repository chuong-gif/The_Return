/*
 * Mục đích: Cửa trượt dùng chung; chỉ nhận tín hiệu mở/đóng, không phụ thuộc câu điểm danh.
 * Danh sách hàm:
 * - Awake / EnsureInitialized: lưu vị trí đóng đúng một lần.
 * - SetOpen: nhận trạng thái mong muốn, tùy chọn áp dụng ngay.
 * - Update: nội suy vị trí panel đến trạng thái đích.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class PuzzleDoor : MonoBehaviour
    {
        public Transform panel;
        public Vector3 openOffset = new Vector3(0, 3.4f, 0);
        public float speed = 1.2f;
        public bool IsOpen { get; private set; }
        Vector3 closedPosition;
        float progress;
        bool initialized;

        /// <summary>Không có đầu vào; lưu localPosition của panel làm vị trí đóng.</summary>
        void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>Không nhận tham số; lưu mốc đóng một lần, kể cả khi session gọi cửa trước Awake.</summary>
        void EnsureInitialized()
        {
            if (initialized) return;
            closedPosition = panel.localPosition;
            initialized = true;
        }

        /// <summary>Nhận trạng thái cửa và cờ tức thì; cập nhật đích, hoặc đặt ngay panel nếu được yêu cầu.</summary>
        public void SetOpen(bool open, bool immediately = false)
        {
            EnsureInitialized();
            IsOpen = open;
            if (!immediately) return;
            progress = open ? 1 : 0;
            panel.localPosition = closedPosition + openOffset * progress;
        }

        /// <summary>Không có đầu vào ngoài deltaTime; di chuyển panel theo trạng thái hiện tại.</summary>
        void Update()
        {
            progress = Mathf.MoveTowards(progress, IsOpen ? 1 : 0, speed * Time.deltaTime);
            panel.localPosition = closedPosition + openOffset * progress;
        }
    }
}
