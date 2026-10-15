# NavigationExam — Bài kiểm tra

**Vị trí:** Phong_3. Sa bàn điều hướng là trung tâm tương tác.

| File code | Vai trò |
|---|---|
| Runtime/Domain/NavigationExamState.cs | Luật bàn điều hướng, lượt, ô, dấu mốc và hoàn thành. |
| Runtime/Data/NavigationExamCatalog.cs | Nội dung hướng dẫn và phản hồi. |
| Runtime/Presentation/NavigationExamPrototype.cs | Input, mở bảng, điều khiển quân/ô, retry và cửa. |

**Dữ liệu:** `Data/NavigationExam_VI.asset`.

**Prefab:** `Prefabs/PF_NavigationBoard.prefab` là cả bộ sa bàn có ý nghĩa gameplay. Giữ các object được controller tham chiếu: `board`, `pawn`, `cells`, `boardLabel`; không thay toàn bộ root bằng một model rỗng. Collider bàn phải cho raycast chạm đúng vùng tương tác. Prefab này chưa được chuẩn hóa toàn bộ sang VisualRoot; thay từng mesh sau khi kiểm tra reference.

Giữ checkpoints trong phòng, không đặt trong mặt bàn. Cửa nhỏ cũ được giữ ẩn cho tương thích controller; ba lối phòng dùng AccessController. Test nhanh nhiệm vụ 3. Builder và bài kiểm tra lịch sử nằm ở `Maps/SchoolFloor`, không tự dựng lại scene cũ.
