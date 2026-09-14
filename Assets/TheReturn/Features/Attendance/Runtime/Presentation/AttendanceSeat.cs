/*
 * Mục đích: Điểm tương tác độc lập gắn trên prefab bàn ghế; giữ ID và các mốc ngồi/đứng.
 * Danh sách hàm:
 * - Configure: gán ID 0–23 và nhãn địa chỉ.
 * - Show: hiển thị người đang ngồi và trạng thái bằng MaterialPropertyBlock.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class AttendanceSeat : MonoBehaviour
    {
        public int index;
        public Transform sitPoint;
        public Transform standPoint;
        public TextMesh label;
        public Renderer indicator;
        MaterialPropertyBlock colors;

        /// <summary>Nhận ID ghế; cập nhật index và nhãn hàng/cột, không thay đổi vị trí prefab.</summary>
        public void Configure(int seatIndex)
        {
            index = seatIndex;
            if (label != null) label.text = AttendanceRound.SeatCode(index);
        }

        /// <summary>Nhận tên người hoặc null và trạng thái; đổi chữ/màu mà không nhân bản material.</summary>
        public void Show(string occupant, bool solved, bool error)
        {
            if (colors == null) colors = new MaterialPropertyBlock();
            Color color = error ? new Color(.8f, .22f, .16f) :
                solved ? new Color(.2f, .65f, .43f) :
                occupant != null ? new Color(.8f, .59f, .20f) : new Color(.23f, .32f, .35f);
            colors.SetColor("_BaseColor", color);
            indicator.SetPropertyBlock(colors);
            label.text = AttendanceRound.SeatCode(index) + (occupant == null ? "" : "\n" + occupant);
        }
    }
}
