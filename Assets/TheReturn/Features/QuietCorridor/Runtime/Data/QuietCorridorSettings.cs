/*
 * Mục đích: Quản lý lời hướng dẫn và thông số nhiệm vụ 2 bằng asset Inspector.
 * Hàm: không có; đây là dữ liệu cấu hình, không xử lý gameplay.
 */
using UnityEngine;
namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Quiet Corridor Settings")]
    public sealed class QuietCorridorSettings : ScriptableObject
    {
        [Min(0)] public float graceSeconds = 3;
        [Min(0)] public float sprintNoisePerSecond = 20;
        [Min(0)] public float speakerNoisePerSecond = 15;
        [Min(0)] public float recoveryPerSecond = 15;
        public string title = "GIỮ TRẬT TỰ";
        [TextArea(3, 6)] public string instructions = "Một người giữ nút A để tắt loa và mở cửa. Người qua trước giữ nút B khi A vẫn đang được giữ. Sau đó người ở A nhả nút và đi qua. Cả đội đến vùng đích để hoàn thành.";
        [TextArea(2, 4)] public string localControls = "BẢN THỬ MỘT MÁY • E giữ/nhả nút • Đổi vai vẫn giữ nút; vai giữ đứng tại chỗ • 1–4 đổi vai • WASD đi • Shift chạy • R thử lại hành lang • Tab menu";
        public string failure = "Quá ồn! Cả đội về đầu hành lang. Điểm danh vẫn được giữ.";
        public string success = "Cả đội đã qua! Đã hoàn thành Giữ trật tự.";
        public string waiting = "Điểm danh xong: đứng dậy và đưa tất cả các vai tới vùng TẬP HỢP ngoài cửa lớp.";
    }
}
