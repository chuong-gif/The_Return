# KẾ HOẠCH T-007: Màn hình thắng game   (file: .ai/PLANS/T-007-win-screen.md)

Trạng thái: XONG (chờ chủ dự án chơi thật xác nhận) | AI giữ: Claude Sonnet 5.5 (phiên #5) | Cập nhật: 2026-10-16 01:45 UTC

## 1. Mục tiêu và nghiệm thu
- Khi cả đội xong mọi nhiệm vụ đã đăng ký (MissionRegistry.Won) và không còn nhiệm vụ nào chạy, hiện màn hình HOÀN THÀNH: danh sách khu đã xong, ghi chú số khu chưa có nhiệm vụ (bật/tắt được), nút Chơi lại / Về sảnh (ẩn khi chưa nối Sảnh chờ) / Thoát.
- Nghiệm thu: test NUnit WinSummary xanh (mức 3); Play Mode ép thắng -> màn hình hiện đúng nội dung, Chơi lại reset map (mức 4).

## 2. Phạm vi
- Được đụng: file MỚI Runtime/Domain/WinSummary.cs, Tests/EditMode/WinSummaryTests.cs, Runtime/UI/WinScreenPresenter.cs; thêm 1 object vào scene (S3).
- KHÔNG đụng: MissionFlowController, SchoolFloorFlow, Canvas hiện có, bố cục của chủ dự án.
- Scene lock + snapshot + chủ dự án đồng ý trước khi LƯU scene (CURRENT dặn không lưu scene khi chưa hỏi).

## 3. Ngữ cảnh đã đọc
| File | Kết luận |
|---|---|
| Domain/MapProgress.cs | `Won` = có ít nhất 1 khu đã đăng ký nhiệm vụ và mọi khu đã đăng ký đều xong (khu chưa có nhiệm vụ không chặn); `MissingMissionAreas`; `Complete` chỉ nhận khu đang làm |
| Missions/MissionFlowController.cs | `Registry`, `AnyActive`; `Registry.Won`; `SchoolFloorFlow.RestartMap()` gọi ResetFlow |
| Runtime/UI/*Canvas*.cs, Editor/SchoolFloorCanvasBuilder.cs | UI dùng uGUI `Text` + font `LegacyRuntime.ttf`; Canvas dựng trong Editor; màn thắng tự dựng bằng code để khỏi sửa scene |

## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | WinSummary (Domain thuần) + 9 test | WinSummary.cs, WinSummaryTests.cs | run_tests EditMode | DONE | 43/43 xanh (34 cũ + 9), Console 0 lỗi (mức 3) |
| S2 | WinScreenPresenter (dựng Canvas bằng code) | WinScreenPresenter.cs | compile | DONE | Console 0 lỗi (mức 2) |
| S3 | Kiểm Play Mode bằng object tạm (không lưu scene) | - | ép thắng, đọc UI, bấm Chơi lại | DONE | Mức 4 (thắng ép bằng registry, không chơi thật 6 nhiệm vụ): màn hiện đủ chữ; raycast UI thật trúng nút Chơi lại; Chơi lại reset (Won false, done 0) và khóa lại chuột. Lỗi tìm thấy và SỬA: Show gọi SetCursor(true) = KHÓA chuột nên không bấm được nút -> đổi SetCursor(false); OnReplay khóa lại |
| S4 | Gắn `WinScreenPresenter` vào scene (object WinScreen, gán missionFlow + flow) và lưu | scene SchoolFloorPrototype | chơi thử | DONE | Chủ dự án đồng ý 2026-10-16. Snapshot Scenes/BeforeWinScreen_20261016_015040.unity; thêm đúng 1 object `WinScreen` (gốc scene, gán missionFlow + flow), lưu; kiểm lại trên đĩa (1 tham chiếu GUID script) và Play Mode với object thật: canvas hiện, Chơi lại reset + khóa chuột, thoát Play scene không dirty (mức 4, thắng ép bằng registry) |

## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Xong hết nhiệm vụ mà không hiện màn thắng | WinScreenPresenter.Update (cần `missionFlow` gán, `Registry.Won`, `AnyActive` false); component chưa gắn scene | Đọc `Registry.Progress.Won`, `AnyActive`; có object WinScreen trong scene không |
| Hiện màn thắng quá sớm | MapProgress.Won chỉ tính khu đã đăng ký nhiệm vụ: khi đăng ký thêm nhiệm vụ mới (T-006) màn thắng tự dời | MissionRegistryFactory.CreateDefault |
| Nút bấm không ăn | EventSystem (EnsureEventSystem tạo nếu thiếu), Canvas sortingOrder 600 | Kiểm EventSystem và module Input System |
| Chữ tiếng Việt lỗi | Font LegacyRuntime.ttf trong WinScreenPresenter.BuildUi | |
| Bấm nút không được dù màn thắng hiện | Chuột đang KHÓA: SetCursor(true) = khóa, SetCursor(false) = thả (PrototypePartyController.SetCursor(bool locked)) | Đọc Cursor.lockState khi màn thắng hiện (phải None) |
| Chơi lại không reset | SchoolFloorFlow.RestartMap > missionFlow.ResetFlow | Đọc Registry.Progress.DoneCount sau khi bấm |

## 6. Rủi ro và câu hỏi cho chủ dự án
- Khi T-006 đăng ký thêm nhiệm vụ cho Phong_5/6 và hành lang, điều kiện thắng tự cần làm hết các nhiệm vụ đó (đúng A3).
- Cần đồng ý lưu scene để gắn object WinScreen (S4). Nút Về sảnh chờ T-008.
- Chưa có thông tin thời gian chơi/điểm trên màn thắng (chưa có yêu cầu).

## 7. Tiến độ
- 2026-10-16 01:45: S1, S2 xong (43/43 test xanh sau khi chạy lại). Chuẩn bị S3.
- 2026-10-16 01:47: S3 xong. Phát hiện qua kiểm thật: PrototypePartyController.SetCursor(true) là KHÓA chuột (SetCursor(bool locked)); sửa trong WinScreenPresenter rồi kiểm lại. Scene không bị lưu/sửa (sau Play: không dirty). Bài học đã ghi vào bản đồ lỗi bên dưới.
