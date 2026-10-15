# KẾ HOẠCH T-003: adapter 6 nhiệm vụ + MissionRegistry (chưa đổi hành vi)   (file: .ai/PLANS/T-003-mission-adapters.md)

Trạng thái: XONG | AI giữ: - | Cập nhật: 2026-10-15 08:05 UTC | Duyệt: chủ dự án cho làm tiếp (chat 2026-10-15); là bước S2 của PLANS/nonlinear-mission-flow.md

## 1. Mục tiêu và nghiệm thu
- Bọc 6 prototype nhiệm vụ bằng một giao diện chung `IMissionAdapter` (AreaId, Active, Solved, CanHandOff, Begin, Stop) và có `MissionRegistry` nối `MapProgress` với các adapter: bắt đầu khu, tự hoàn thành khi nhiệm vụ xong, bỏ dở (giữ tiến độ khu khác). CHƯA đổi hành vi game, CHƯA gắn vào scene/SchoolFloorFlow (đó là T-004).
- Nghiệm thu: Console 0 lỗi CS; test NUnit của MissionRegistry (dùng adapter giả) xanh (mức 3); adapter thật chỉ đạt mức 2 (compile) vì chạy cần scene.

## 2. Phạm vi
- Được đụng (TẠO MỚI): `Runtime/Domain/IMissionAdapter.cs`, `Runtime/Domain/MissionRegistry.cs` (assembly TheReturn.SchoolFloor.Domain, thuần C#); `Tests/EditMode/MissionRegistryTests.cs`; `Maps/SchoolFloor/Runtime/Missions/` (6 adapter + `MissionRegistryFactory`, assembly TheReturn.SchoolFloor có sẵn).
- KHÔNG đụng: scene, prefab, SchoolFloorFlow, SchoolFloorAccessController, Domain/Presentation của 6 nhiệm vụ, builder/MenuItem.
- Scene lock: không cần. Snapshot scene: không cần.

## 3. Ngữ cảnh đã đọc
| File (đọc 2026-10-15) | Kết luận 1 dòng |
|---|---|
| SchoolFloorFlow.cs | Quy ước bàn giao hiện tại: xong khi `State.Solved` (GradeRepair/NavigationExam còn đòi `!PanelOpen`), rồi `StopSession(false)` và `Begin()` nhiệm vụ sau; GradeRepair cần `gradeEntrance.SetOpen(true)` trước `Begin()`; Attendance bàn giao bằng `ExternalControl = true` + tắt `attendanceHud` |
| 6 prototype (reflection) | Cả 5 nhiệm vụ trừ Attendance có `Active`, `Begin()`, `StopSession(bool reset = true)`, `State.Solved`; Attendance có `Begun`, `BeginSession()`, `ExternalControl`, `RetryRound()`, `State.Solved`, không có Active/StopSession |
| AttendancePrototype.cs 100-200 | `BeginSession` chỉ đặt Begun + khóa chuột, không đổi đề; `RetryRound` giữ đề và xóa chỗ ngồi; `StartNewRound` sinh đề mới |
| PuzzleDoor | `SetOpen(bool open, bool immediately = false)`, `IsOpen` |
| MAPS/area-adjacency.md mục 3 | Ánh xạ nhiệm vụ -> khu: Attendance=Phong_1, GradeRepair=Phong_2, NavigationExam=Phong_3, TeacherTruth=Phong_4, QuietCorridor=Hanh_Lang_1, DontLookBack=Hanh_Lang_3 |
| PLANS/T-002 + OWNER_UPDATES | A1-A3 đã xác nhận; MapProgress API: TryBegin, Complete, Abandon, SetHasMission, Won, MissingMissionAreas |

## 4. Các subtask
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | `IMissionAdapter` + `MissionRegistry` (Domain thuần): Register, TryBegin, Tick (tự Complete khi Solved && CanHandOff), Abandon, Reset, Won | Runtime/Domain/IMissionAdapter.cs, MissionRegistry.cs | Console 0 lỗi CS | DONE | Compile sạch (mức 2) |
| S2 | Test registry bằng adapter giả (NUnit) | Tests/EditMode/MissionRegistryTests.cs | Test Runner xanh | DONE | run_tests EditMode: 34/34 passed (21 MapProgress + 13 MissionRegistry), mức 3 |
| S3 | 6 adapter thật + `MissionRegistryFactory` (SchoolFloor assembly); thêm tham chiếu TheReturn.SchoolFloor.Domain vào TheReturn.SchoolFloor.asmdef | Runtime/Missions/MissionAdapters.cs, MissionRegistryFactory.cs, Runtime/TheReturn.SchoolFloor.asmdef | Console 0 lỗi CS | DONE | Compile sạch (mức 2); kiểu nằm đúng assembly (reflection); adapter chưa chạy trong scene |
| S4 | Bàn giao: TASKS/LOG/CURRENT/SYSTEM_CARDS/asmdef-graph; nháp PLANS/T-004-wire-registry.md | .ai/** | Đọc lại | DONE | index-scripts.md CHƯA dựng lại (tự sinh) |

## 5. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Khu không bao giờ tự hoàn thành | MissionRegistry > Tick (cần adapter.Solved && CanHandOff) | Test `Tick_CompletesWhenSolvedAndHandOffReady` | GradeRepair/NavigationExam còn mở panel thì CanHandOff=false |
| Bắt đầu khu nhưng nhiệm vụ không chạy | MissionRegistry > TryBegin (gọi adapter.Begin khi Started) | Test `TryBegin_StartsAdapter` | adapter thật: Runtime/Missions/<Tên>MissionAdapter.cs |
| Attendance không dừng/không chạy | AttendanceMissionAdapter > Begin/Stop (ExternalControl, attendanceHud) | Đọc `ExternalControl` và `attendanceHud.enabled` sau gọi | SchoolFloorFlow.Update dòng bàn giao Attendance |
| GradeRepair cửa chưa mở | GradeRepairMissionAdapter > Begin (entrance.SetOpen(true)) | Kiểm `gradeEntrance.IsOpen` | SchoolFloorFlow dòng gradeEntrance |

## 6. Rủi ro và câu hỏi
- Adapter thật chưa chạy trong scene (mức 2); T-004 phải kiểm Play Mode. `CanHandOff` mỗi nhiệm vụ lấy từ quy ước hiện tại của SchoolFloorFlow; nếu chủ dự án muốn khác thì sửa trong adapter.
- Attendance chạy từ đầu scene (Begun do UI/khởi động): khi nối registry (T-004) phải quyết định Phong_1 có tự bắt đầu hay chờ chọn. Ghi lại cho T-004.
- `Abandon` dùng `Stop(reset: true)` để khu bỏ dở sinh lại sạch; progress khu khác giữ (A2 đã xác nhận).

## 7. Tiến độ
- 2026-10-15 07:59 UTC: kế hoạch viết xong, bắt đầu S1.
- 2026-10-15 08:02 UTC: S1+S2 xong (34/34 test xanh). Đang làm S3 (adapter thật trong Runtime/Missions).
- 2026-10-15 08:05 UTC: S3+S4 xong; 34/34 test xanh; Console 0 lỗi. Bước kế: T-004 (nháp PLANS/T-004-wire-registry.md, CHỜ DUYỆT).
