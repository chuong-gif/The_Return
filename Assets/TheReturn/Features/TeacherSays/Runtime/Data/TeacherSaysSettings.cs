/*
 * Mục đích: Asset chỉnh được trong Inspector cho nhiệm vụ Hanh_Lang_5 «Cô nói» (T-006c): thời gian Bước, bù giờ khi đổi vai, thời gian Đứng, số cảnh cáo.
 * Giai đoạn phát triển một máy đổi vai bằng phím 1-4 nên cửa sổ Bước dài; TeacherSaysPrototype đọc lại asset mỗi frame nên chỉnh được khi đang chơi thử.
 * Thành viên: stepWindowSeconds, switchExtraSeconds, standSeconds, maxWarnings, fixedSeed, chữ hiển thị; ToRules; ApplyDevPreset; ApplyOnlinePreset.
 */
using UnityEngine;

namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Teacher Says Settings", fileName = "TeacherSaysSettings")]
    public sealed class TeacherSaysSettings : ScriptableObject
    {
        [Header("Thời gian")]
        [Tooltip("Số giây cả đội có để vào vùng sáng khi phải BƯỚC TỚI. 0 = không giới hạn (chế độ thử). Online gợi ý 8 giây.")]
        [Min(0f)] public float stepWindowSeconds = 20f;
        [Tooltip("Số giây cộng thêm mỗi lần đổi vai (phím 1-4) khi đang trong đoạn BƯỚC; tổng cộng thêm mỗi đoạn không quá stepWindowSeconds.")]
        [Min(0f)] public float switchExtraSeconds = 3f;
        [Tooltip("Số giây cả đội phải đứng yên khi phải ĐỨNG YÊN.")]
        [Min(0.5f)] public float standSeconds = 5f;
        [Tooltip("Số cảnh cáo khiến cả đội về đầu hành lang.")]
        [Min(1)] public int maxWarnings = 3;
        [Tooltip("Vận tốc ngang (m/giây) trên mức này tính là «đang di chuyển».")]
        [Min(0.05f)] public float movingSpeed = 0.4f;

        [Header("Đề")]
        [Tooltip("0 = đề ngẫu nhiên mỗi lần bắt đầu; khác 0 = luôn cùng một đề (để test).")]
        public int fixedSeed;

        [Header("Chữ hiển thị")]
        [TextArea] public string instructions = "Loa ra lệnh BƯỚC TỚI hoặc ĐỨNG YÊN. Chỉ MỘT người nghe được lệnh có «Cô nói» hay không: có thì làm ĐÚNG lệnh, không thì làm NGƯỢC lại. Làm sai bị cảnh cáo.";
        public string advanced = "Đúng! Sang đoạn tiếp theo.";
        public string standOk = "Được rồi, đi tiếp tới vùng sáng.";
        public string warned = "Sai! Cảnh cáo - lệnh được lặp lại.";
        public string sentBack = "Đủ cảnh cáo - cả đội về đầu hành lang.";
        public string success = "Hoàn thành hành lang «Cô nói»!";

        /// <summary>Không nhận tham số; trả quy tắc thời gian cho Domain từ giá trị Inspector.</summary>
        public TeacherSaysRules ToRules() { return new TeacherSaysRules(stepWindowSeconds, switchExtraSeconds, standSeconds, maxWarnings); }

        /// <summary>Không nhận tham số; đặt giá trị phát triển một máy (Bước 20s, +3s mỗi lần đổi vai, Đứng 5s, 3 cảnh cáo).</summary>
        [ContextMenu("Preset: phát triển một máy (20s, +3s)")]
        public void ApplyDevPreset() { stepWindowSeconds = 20f; switchExtraSeconds = 3f; standSeconds = 5f; maxWarnings = 3; }

        /// <summary>Không nhận tham số; đặt giá trị online (Bước 8s, không cộng thêm, Đứng 5s, 3 cảnh cáo).</summary>
        [ContextMenu("Preset: online (8s, +0)")]
        public void ApplyOnlinePreset() { stepWindowSeconds = 8f; switchExtraSeconds = 0f; standSeconds = 5f; maxWarnings = 3; }
    }
}
