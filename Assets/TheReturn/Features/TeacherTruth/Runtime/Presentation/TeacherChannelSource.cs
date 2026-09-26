/*
 * Mục đích: Đánh dấu tượng giáo viên là nguồn phụ đề của một kênh; không chứa luật nhiệm vụ.
 * Hàm: không có; channel được controller đọc khi raycast trúng tượng.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class TeacherChannelSource : MonoBehaviour
    {
        [Range(0, 3)] public int channel;
    }
}
