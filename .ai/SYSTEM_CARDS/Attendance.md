# CARD: Attendance

Khu / vai trò: Phong_1 (mission cũ 0). Điểm danh/24 ghế. Chạy từ đầu; nhường quyền bằng ExternalControl
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/Attendance | 10 file .cs, 1372 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.Attendance.Editor -> [TheReturn.Attendance,TheReturn.Attendance.Domain,TheReturn.Core,TheReturn.Shared]; TheReturn.Attendance -> [TheReturn.Attendance.Domain,TheReturn.Core,TheReturn.Shared,Unity.InputSystem]; TheReturn.Attendance.Domain -> []; TheReturn.Attendance.Tests -> [TheReturn.Attendance,TheReturn.Attendance.Domain,TheReturn.Core,TheReturn.Shared]
- Kiểu (14 đầu): AttendanceSceneBuilder, AttendanceTextCatalogEditor, AttendanceTextEntry, AttendanceTextCatalog, AttendanceGenerator, AttendanceRuleKind, AttendanceRule, AttendanceRound, AttendanceState, AttendanceHud, AttendancePrototype, AttendanceSeat, AttendanceChecks
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Runtime/Presentation/AttendancePrototype.cs (331); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Editor/AttendanceSceneBuilder.cs (268); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Runtime/Data/AttendanceTextCatalog.cs (155); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Runtime/Domain/AttendanceGenerator.cs (134)
- README: Assets/TheReturn/Features/Attendance/README.md
- Tests: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Tests/Editor/AttendanceChecks.cs
- Prefab: PF_AttendanceSeat
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): The Return/Attendance/Rebuild Logic Classroom <- E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Editor/AttendanceSceneBuilder.cs; The Return/Attendance/Validate Generated Puzzles <- E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/Attendance/Tests/Editor/AttendanceChecks.cs

## B. Triệu chứng -> nơi xem (suy từ code đọc 2026-10-15; chưa tái hiện lỗi thật — sửa lại khi có lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| Không ngồi được vào ghế | Domain/AttendanceState.cs > TrySit (từ chối khi ghế có người, người đã ngồi, đề đã xong); Presentation/AttendancePrototype.cs > TrySitActive, UpdateTarget (raycast tới ghế) | Gọi State.TrySit trực tiếp; kiểm `seats[24]` đủ và đúng thứ tự ID | README: giữ đủ 24 phần tử đúng thứ tự |
| Đề vô nghiệm/nhiều nghiệm/manh mối sai | Domain/AttendanceGenerator.cs > Generate, CountSolutions | Chạy `AttendanceChecks.Run()` (thuần Domain, ghi đè Design/Verification/AttendanceV2.json) | TESTING.md |
| Chữ manh mối lạ/thiếu | Data/AttendanceTextCatalog.cs + Data/AttendanceText_VI.asset (30 biến thể) | Inspector của catalog (AttendanceTextCatalogEditor) | README: đừng đổi ký hiệu thay thế |
| Thử lại đổi đề / vòng mới giữ đề | AttendancePrototype > RetryRound (giữ seed), StartNewRound (đổi đáp án) | Đối chiếu AttendanceV2Integration.json | |
| Xong mà không chuyển/lối không mở | SchoolFloor: Flow.Update, AccessController.Refresh; `ExternalControl` | SYSTEM_CARDS/SchoolFloor.md | luồng tuyến tính |

## C. Hiện trạng đã xác minh (đọc code + README + Design/Verification, 2026-10-15)
- Scene: `Puzzle_Attendance` (AttendancePrototype), Phong_1; party `LocalParty`, `seats[24]`, `exitDoor` cũ ẩn, `exitZone`, `board` TextMesh; `participantCount`=4, `initialSeed`=0, `continueIntoMap`=true, `settleSeconds`=2. Thuộc tính: State, ActivePlayer, PartySize, Begun, Completed, ExternalControl.
- Luật (Domain, thuần C#): lưới 6 hàng x 4 cột = 24 ghế, mã `H<hàng>-C<cột>`. 2-4 người. `AttendanceGenerator.Generate(names, seed)` sinh đề có ĐÚNG MỘT nghiệm; luật loại FixedRow/FixedColumn/Ahead/Behind/Left/Right/SameRow/SameColumn/RowParity/ColumnParity chia thành manh mối riêng cho từng người (`ClueOwners`). `AttendanceState`: TrySit/Stand/Evaluate (xong khi mọi người ngồi và mọi luật đúng)/Reset (giữ đáp án).
- Mặc định chạy từ đầu map; Flow nhường quyền bằng `ExternalControl`. Có HUD cũ (`AttendanceHud`), map dùng Canvas riêng.
- Bằng chứng: AttendanceV2.json 2026-09-14 (90 đề, 2076 assertion, passed); AttendanceV2Integration.json 2026-09-14 (2/3/4 người giải và mở cửa, retry giữ seed, 24 điểm đứng, raycast E). Tài liệu thiết kế của chủ dự án: `Design/Diem-danh-v2.md`, `Design/Choi-thu-diem-danh.md`.
- Thiếu/rủi ro: chưa kiểm sau Layout V2; Attendance mặc định chạy từ đầu nên cần xử lý khi chuyển sang luồng tự chọn (PLANS/nonlinear-mission-flow.md).
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
