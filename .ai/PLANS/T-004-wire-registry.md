# KẾ HOẠCH T-004 (NHÁP, CHỜ DUYỆT): nối MissionRegistry vào SchoolFloorFlow / SchoolFloorAccessController   (file: .ai/PLANS/T-004-wire-registry.md)

Trạng thái: DONE (2026-10-15 23:37 UTC; code + scene đã nối, logic qua Play Mode, chủ dự án tự đi thử từng phòng) | AI giữ: - | Viết: 2026-10-15 08:05 UTC bởi Claude Sonnet 5.5 (phiên #3) | Là bước S3 của PLANS/nonlinear-mission-flow.md

## 1. Mục tiêu
- Thay luồng tuyến tính (chuỗi if trong `SchoolFloorFlow.Update` + mở cửa theo `m-1` trong `SchoolFloorAccessController.Refresh`) bằng `MissionRegistry`: người chơi tự chọn khu theo luật đã chốt (DECISIONS mục 1-10). Nghiệm thu: compile sạch + chơi thử Play Mode (mức 4).

## 2. Đã có (không làm lại)
- `MapProgress`, `MissionRegistry`, 6 adapter, `MissionRegistryFactory.CreateDefault(flow)` — xem PLANS/T-002, T-003. Registry nhận `teamReady` và `entryFromId` từ lớp Presentation; vị trí đội phải do T-004 tính từ `SchoolPreparationGate.preparationZone` (22 cổng), KHÔNG hardcode tọa độ.

## 3. Việc phải đọc trước (CHƯA đọc, đừng giả định)
- `Maps/SchoolFloor/Runtime/SchoolFloorAccessController.cs` và `SchoolPreparationGate.cs` (field areaName, mission, designatedEntrance, preparationZone; cách mở cửa).
- Cách `SchoolFloorFlow.Start/RestartMap`, `SchoolFloorCanvasPresenter` và các `*Hud` dùng `Active/State` (HUD theo khu thuộc S6 của kế hoạch lớn).
- Nhãn cổng cũ («Hanh lang 2/4» = Hanh_Lang_1/3) và cổng nào là sân->phòng / phòng->hành lang / hành lang->phòng (area-adjacency mục 2-3, mục 2 còn suy luận [S]).

## 4. Subtask đề xuất
| ID | Việc | Cách kiểm |
|---|---|---|
| S1 | Đọc có giới hạn AccessController + Gate; viết bảng gate -> (khu, entryFromId) lấy từ scene (chỉ đọc) | Bảng khớp 22 cổng |
| S2 | Snapshot scene (BeforeRegistry_yyyyMMdd_HHmmss.unity) + Scene lock | File snapshot tồn tại |
| S3 | Thêm component điều phối mỏng (ví dụ MissionFlowController) giữ registry, gọi `Tick()` mỗi frame, tính teamReady/entryFrom từ vùng chuẩn bị; KHÔNG sửa logic 6 nhiệm vụ | Compile sạch |
| S4 | `SchoolFloorAccessController` mở cửa theo MapProgress (khu xong luôn mở; khu chưa xong chỉ mở lối vào khi không có khu nào đang làm; khu chưa có nhiệm vụ giữ hành vi mở sẵn) | Play Mode |
| S5 | Vô hiệu chuỗi if cũ trong `SchoolFloorFlow.Update` (giữ RestartMap -> `registry.Reset()` + dựng lại Attendance) | Play Mode: chơi thứ tự bất kỳ |
| S6 | Kiểm Play Mode: chọn phòng thứ tự bất kỳ; hành lang chỉ vào từ 2 phòng kề; đội thiếu người không bắt đầu; bỏ dở giữ tiến độ | Ghi mức 4 |

## 5. Quyết định cần chủ dự án
- Phong_1 (Attendance) hiện chạy ngay từ đầu scene. Khi chuyển sang tự chọn: (a) vẫn tự bắt đầu ở Phong_1 như hiện nay, hay (b) chờ người chơi chọn như mọi khu? Mặc định đề xuất (b) cho nhất quán; cần xác nhận.
- Cho phép sửa `SchoolFloorFlow`, `SchoolFloorAccessController` và scene `SchoolFloorPrototype` (có thay đổi chưa ghi của chủ dự án lúc 01:59 UTC) — cần Scene lock và snapshot.

## 6. Rủi ro
- Gắn component/gán field trong scene chủ dự án đang tự chỉnh; 5 bộ `*Checks` cũ sẽ hỏng (thay ở S5 của kế hoạch lớn).
- `FindFirstObjectByType` rải nhiều file; ưu tiên truyền tham chiếu qua field.


## Kết quả (phiên #4, 2026-10-15 23:37 UTC)
- Chủ dự án duyệt và chốt: Attendance không tự chạy; 4 người xuất hiện quanh tâm sân; giữ vùng chuẩn bị phía TRONG cửa; cửa khu chưa xong luôn mở khi không khu nào đang làm; đủ đội trong vùng thì khu bắt đầu, khóa 3 lối; xong thì mở lại.
- Làm: MissionFlowController (mới), SchoolFloorFlow/AccessController/Gate/MissionAdapters/MissionTestLauncher (sửa nhỏ), scene (object điều phối, bảng 22 cổng, dời 4 PlayerSlot). Snapshot trước khi sửa scene: Scenes/BeforeRegistry_20261015_082336.unity.
- Kiểm: EditMode 43/43; Play Mode (teleport đội vào vùng): 3/4 không bắt đầu, 4/4 bắt đầu Phong_1 và khóa 16 cổng nhiệm vụ, phải đứng dậy mới bàn giao, hoàn thành mở cửa, vào Hanh lang 2_Vao thì Hanh_Lang_1 bắt đầu, RestartMap về sân. Chủ dự án tự đi bộ thử từng phòng.
- CHƯA kiểm/để lại: vòng bắt đầu-kết thúc thật của Phong_2..4 và hành lang 3 (T-005); Canvas còn hiện bảng Điểm danh ở sân (T-011); test launcher không tự chuyển khu; index-scripts.md (tự sinh) chưa dựng lại cho script mới.
