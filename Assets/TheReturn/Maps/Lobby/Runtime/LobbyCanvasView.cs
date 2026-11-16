/*
 * Mục đích: Hiển thị giao diện sảnh bằng Canvas và chuyển thao tác nút thành lệnh cho LobbyReadyController.
 * Danh sách hàm:
 * - Render: nhận số người, mảng ready, countdown và controller; cập nhật toàn bộ nội dung UI rồi báo sự kiện Rendered (T-053: LobbySkin tô giao diện qua đây).
 * - SetStatus: nhận chuỗi trạng thái; thay dòng hướng dẫn chính.
 * - ToggleSlot: nhận slot; gọi controller đảo trạng thái.
 * - SetPartySize: nhận số người; gọi controller đổi quy mô đội.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    public sealed class LobbyCanvasView : MonoBehaviour
    {
        public Text title;
        public Text status;
        public Text[] slotLabels;
        public Button[] slotButtons;
        /// <summary>Nội dung dòng tiêu đề; LobbySkin có thể đổi sang chữ có định dạng.</summary>
        public string titleText = "THE RETURN\nSẢNH TIẾP NHẬN";
        /// <summary>Hàm tạo chữ cho từng ô (chỉ số ô, đã vào đội, đã sẵn sàng); null = chữ mặc định.</summary>
        public System.Func<int, bool, bool, string> slotFormatter;
        /// <summary>Báo sau mỗi lần Render: số người, mảng ready, giây đếm ngược (-1 = chưa đếm).</summary>
        public event System.Action<int, bool[], int> Rendered;
        LobbyReadyController controller;

        /// <summary>Nhận trạng thái sảnh; cập nhật text và khả năng bấm của từng slot, không trả dữ liệu.</summary>
        public void Render(int participantCount, bool[] ready, int countdown, LobbyReadyController source)
        {
            controller = source;
            if (title != null) title.text = titleText;
            for (int i = 0; i < slotLabels.Length; i++)
            {
                bool joined = i < participantCount;
                slotLabels[i].text = slotFormatter != null ? slotFormatter(i, joined, joined && ready[i]) : joined
                    ? "HỌC SINH " + (i + 1) + "\n" + (ready[i] ? "ĐÃ SẴN SÀNG" : "ĐANG CHỜ") + "  [F" + (i + 1) + "]"
                    : "VỊ TRÍ TRỐNG";
                if (i < slotButtons.Length) slotButtons[i].interactable = joined;
            }
            if (status != null) status.text = countdown >= 0
                ? "TOÀN ĐỘI ĐÃ SẴN SÀNG — CỔNG MỞ SAU " + countdown
                : "Chọn 2–4 người • F1–F4: sẵn sàng • 1–4: đổi vai • Tab: điều khiển";
            if (Rendered != null) Rendered(participantCount, ready, countdown);
        }

        /// <summary>Nhận nội dung; cập nhật dòng trạng thái chính nếu Text tồn tại.</summary>
        public void SetStatus(string value) { if (status != null) status.text = value; }

        /// <summary>Nhận slot 0–3 từ Button; chuyển lệnh sang controller.</summary>
        public void ToggleSlot(int slot) { if (controller != null) controller.ToggleReady(slot); }

        /// <summary>Nhận số người 2–4 từ Button; chuyển lệnh sang controller.</summary>
        public void SetPartySize(int count) { if (controller != null) controller.SetPartySize(count); }
    }
}
