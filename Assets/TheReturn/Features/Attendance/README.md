# Attendance — Điểm danh

**Vị trí:** Phong_1. 24 bàn ghế; đội 2–4 người. Giữ ID ghế 0–23 dù di chuyển bàn.

| File code | Vai trò |
|---|---|
| Runtime/Domain/AttendanceRound.cs | Dữ liệu lượt, mã ghế và lời giải. |
| Runtime/Domain/AttendanceGenerator.cs | Sinh đề/manh mối theo số người và seed. |
| Runtime/Domain/AttendanceState.cs | Ngồi, đứng, kiểm tra đáp án và trạng thái lượt. |
| Runtime/Data/AttendanceTextCatalog.cs | Kho mẫu text, biến thể manh mối, kiểm tra nội dung. |
| Runtime/Presentation/AttendancePrototype.cs | Khởi tạo đội, input, tương tác ghế, retry và nối luật. |
| Runtime/Presentation/AttendanceSeat.cs | ID ghế, mốc ngồi/đứng, nhãn và đèn trạng thái. |
| Runtime/Presentation/AttendanceHud.cs | HUD cũ; map có Canvas riêng điều phối việc hiển thị. |
| Editor/AttendanceTextCatalogEditor.cs | Giao diện chỉnh/kiểm tra kho text trong Inspector. |
| Editor/AttendanceSceneBuilder.cs | Builder cảnh gốc; không chạy lại trên blockout hiện tại. |
| Tests/Editor/AttendanceChecks.cs | Kiểm tra luật điểm danh. |

**Dữ liệu:** `Data/AttendanceText_VI.asset` để chỉnh manh mối. Đừng đổi/xóa ký hiệu thay thế trong mẫu khi chưa xem yêu cầu của catalog.

**Prefab:** `Prefabs/PF_AttendanceSeat.prefab` là ghế có logic, không chỉ là model. Giữ component `AttendanceSeat`, `sitPoint`, `standPoint`, `label`, `indicator`; thay hình ảnh rồi gán lại indicator nếu bỏ renderer cũ. Di chuyển cả root ghế để các mốc đi theo. Bàn/ghế trang trí từ ThirdParty đang được bố trí riêng trong scene, cần di chuyển đồng bộ nếu đổi vị trí ghế logic.

Giữ đủ 24 phần tử trong `AttendancePrototype.seats`, đúng thứ tự ID. Cửa thoát nhỏ cũ đang ẩn nhưng còn được runtime tham chiếu; khóa ba lối thật do `SchoolFloorAccessController` quản lý. Test nhanh dùng `MissionTestLauncher`, nhiệm vụ 0.
