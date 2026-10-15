# CARD: TeacherTruth

Khu / vai trò: Phong_4 (mission cũ 5)
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/TeacherTruth | 5 file .cs, 435 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.TeacherTruth -> [TheReturn.TeacherTruth.Domain,TheReturn.Core,TheReturn.Shared,Unity.InputSystem]; TheReturn.TeacherTruth.Domain -> []
- Kiểu (14 đầu): TeacherTruthCatalog, ManualConfirmResult, TeacherTruthState, ManualConfirmStation, TeacherChannelSource, TeacherTruthPrototype
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/TeacherTruth/Runtime/Presentation/TeacherTruthPrototype.cs (177); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/TeacherTruth/Runtime/Domain/TeacherTruthState.cs (142); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/TeacherTruth/Runtime/Data/TeacherTruthCatalog.cs (78); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/TeacherTruth/Runtime/Presentation/ManualConfirmStation.cs (24)
- README: Assets/TheReturn/Features/TeacherTruth/README.md
- Tests: không có trong thư mục này
- Prefab: PF_ManualConfirmStation, PF_TeacherChannel
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem (suy từ code đọc 2026-10-15; chưa tái hiện lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| Xác nhận bị reset/không tính | Domain/TeacherTruthState.cs > Confirm (cần đủ 4 kênh trong cửa sổ, mọi người đều tham gia, nếu không NeedEveryPlayer và reset), Tick (hết cửa sổ thì reset) | `confirmationWindow`=3s trên prototype; theo dõi `WindowActive`, `Remaining`, `ParticipantCount` | Design/Giao-vien-that-doi.md |
| Không nghe/thiếu phụ đề | TeacherTruthPrototype > Hear; TeacherChannelSource (ID kênh 0-3) | `State.HasHeard(actor, channel)`; phụ đề được giữ khi reset xác nhận | |
| Nút xác nhận không phản hồi | ManualConfirmStation (ID kênh), mảng `stations[4]` phải khớp `teachers[4]` | | README (prefab) |
| Độ khó khác khi thêm/bớt người | `confirmationWindow` + khoảng cách trạm | Kiểm lại đội 2 người (README) | |

## C. Hiện trạng đã xác minh (2026-10-15)
- Scene: `TeacherTruth_LogicOnly` (TeacherTruthPrototype), Phong_4; `teachers[4]`, `stations[4]`, `checkpoints[4]`, `syncWarning`, `exitDoor` cũ ẩn, `confirmationWindow`=3, `initialSeed`=0.
- Luật: 4 kênh giáo viên đưa thông báo mâu thuẫn (3 bộ thoại theo seed%3); mỗi người nghe và ghi phụ đề RIÊNG; sau đó cả đội phải bấm xác nhận thủ công đúng 4 kênh trong một cửa sổ ngắn (3 giây từ lần xác nhận đầu) với MỖI người tham gia ít nhất một kênh; thiếu người hoặc hết giờ thì xác nhận bị xóa, phụ đề giữ.
- Bằng chứng: TeacherTruth.json 2026-09-26 (219 assertion, 3 bộ), Frames/Transition/UI 2026-09-26 (timeout runtime, raycast ngang, chuyển tiếp thường).
- Prefab chưa chuẩn hóa VisualRoot: PF_TeacherChannel, PF_ManualConfirmStation. Chưa kiểm sau Layout V2.
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
