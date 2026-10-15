# TeacherTruth — Giáo viên nói thật/nói dối

**Vị trí:** Phong_4. Bốn kênh giáo viên và bốn trạm xác nhận.

| File code | Vai trò |
|---|---|
| Runtime/Domain/TeacherTruthState.cs | Sinh/đánh giá kênh thông tin, lựa chọn, cửa sổ xác nhận và kết quả. |
| Runtime/Data/TeacherTruthCatalog.cs | Lời giáo viên, chứng cứ, hướng dẫn và phản hồi. |
| Runtime/Presentation/TeacherTruthPrototype.cs | Tương tác nghe/xác nhận, thời gian, retry, checkpoint và cửa. |
| Runtime/Presentation/TeacherChannelSource.cs | Định danh kênh giáo viên và cập nhật biểu diễn kênh. |
| Runtime/Presentation/ManualConfirmStation.cs | ID kênh của nút xác nhận và nhãn người đã xác nhận. |

**Dữ liệu:** `Data/TeacherTruth_VI.asset`. Không sửa lời thoại theo cách làm mâu thuẫn với ý nghĩa chứng cứ/luật trong catalog.

**Prefab:** `PF_TeacherChannel.prefab`, `PF_ManualConfirmStation.prefab`. Giữ ID channel 0–3, collider và reference nhãn/renderer; hai mảng `teachers` và `stations` trên controller phải khớp kênh. Các prefab này chưa được chuẩn hóa toàn bộ sang VisualRoot. Thay model không được xóa nguồn kênh hoặc biến bốn trạm thành cùng một ID.

`confirmationWindow` trên controller ảnh hưởng độ khó; thay khoảng cách các trạm phải kiểm tra đội 2 người. `Begin` hiện đưa đội tới checkpoints. Cửa nhỏ cũ giữ ẩn, ba cửa phòng được hệ AccessController mở khi hoàn thành. Test nhanh nhiệm vụ 5.
