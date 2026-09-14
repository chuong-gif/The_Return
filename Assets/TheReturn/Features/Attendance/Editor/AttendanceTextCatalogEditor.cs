/*
 * Mục đích: Hướng dẫn chỉnh text trong Inspector và báo lỗi placeholder ngay cho người thiết kế.
 * Danh sách hàm:
 * - OnInspectorGUI: vẽ trường dữ liệu, tài liệu token và kết quả kiểm tra catalog.
 */
using UnityEditor;
using UnityEngine;

namespace TheReturn.Editor
{
    [CustomEditor(typeof(AttendanceTextCatalog))]
    public sealed class AttendanceTextCatalogEditor : UnityEditor.Editor
    {
        /// <summary>Không nhận tham số; vẽ Inspector cho asset đang chọn, không tự sửa nội dung text.</summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Text cho điểm danh 2–4 người. Mở Entries > Variants để sửa/thêm câu cùng nghĩa.\n" +
                "{a}/{b}: tên; {row}/{column}: số hàng/cột; {steps}: khoảng cách; {parity}: chẵn/lẻ.\n" +
                "Giữ đúng ý nghĩa loại luật. Thêm biến thể câu chữ không cần sửa code.",
                MessageType.Info);
            DrawDefaultInspector();
            var catalog = (AttendanceTextCatalog)target;
            string error;
            bool valid = catalog.ValidateCatalog(out error);
            EditorGUILayout.HelpBox(valid ? "Catalog hợp lệ: đủ loại luật và placeholder." : error,
                valid ? MessageType.Info : MessageType.Error);
        }
    }
}
