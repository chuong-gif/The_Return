# TESTING

## Hiện trạng [V, đọc 2026-10-14]
- Test Framework 1.6.0 đã cài, nhưng dự án KHÔNG dùng NUnit/[UnityTest] (grep = 0). Các bộ kiểm tra là class tĩnh tùy biến trong assembly Tests (Editor). [CẬP NHẬT 2026-10-15 07:52: từ T-002 đã có NUnit EditMode, xem mục cuối file]
- Bộ kiểm tra và điểm vào:
  - `AttendanceChecks.Run()` (void) — MenuItem `The Return/Attendance/Validate Generated Puzzles`. Thuần Domain (90 đề, 24 ghế). GHI ĐÈ `Design/Verification/AttendanceV2.json` mỗi lần chạy.
  - 5 bộ SchoolFloor (đọc code 2026-10-14), namespace `TheReturn.Editor`, đặt tại `Assets/TheReturn/Maps/SchoolFloor/Tests/Editor/`: `DontLookBackChecks`, `GradeRepairChecks`, `NavigationExamChecks`, `QuietCorridorChecks`, `TeacherTruthChecks`; điểm vào `public static string Run()`, không có MenuItem.
    - Tiền điều kiện: PHẢI đang ở Play Mode (không thì ném InvalidOperationException) và scene có `SchoolFloorFlow` (tìm bằng FindFirstObjectByType) => chạy trên SchoolFloorPrototype.
    - Hành vi: kiểm Domain rồi kiểm tích hợp với đội 2/3/4 người; dịch chuyển (teleport) người chơi trong scene; khôi phục `settleSeconds` và con trỏ ở cuối (trừ DontLookBack/TeacherTruth không thấy khôi phục settleSeconds, chỉ trả con trỏ).
    - Tác dụng phụ: GHI ĐÈ `Design/Verification/<Tên>.json` (DontLookBack, GradeRepair, NavigationExam, QuietCorridor, TeacherTruth). Trạng thái Play Mode mất khi thoát Play, miễn không lưu scene.
    - Chưa xác minh: các tọa độ teleport có còn khớp bố cục V2 hay không (PROJECT_STATE cảnh báo bài hardcode tọa độ cũ).
- PROJECT_STATE ghi một số bài hardcode tọa độ cũ: không chạy lại để 'xác nhận' khi chưa đối chiếu.
- Kết quả lịch sử: `Design/Verification/`, `Design/Bai-kiem-tra.md`.

## Đã chạy thử (2026-10-14)
- `QuietCorridorChecks.Run()` qua execute_code trong Play Mode: THẤT BẠI ở 'Scene 2: same scene transition' — test lỗi thời so với Layout V2, xem `.ai/BUGS/quiet-corridor-check-stale-after-layout-v2.md`. Harness chạy được (vào/thoát Play Mode, gọi Run, đọc kết quả); nội dung bài kiểm tra cần cập nhật. Bốn bộ SchoolFloor còn lại chưa chạy.
- Lưu ý: Editor không focus vẫn chạy game nhưng chậm (time 5s vs 26s thực); `Run()` đồng bộ nên không ảnh hưởng.

## Chưa chạy
Các bộ còn lại: mọi bộ đều ghi đè một file báo cáo trong `Design/Verification/`; 5 bộ SchoolFloor còn cần Play Mode trên scene thật. Test Runner của MCP không có test NUnit để chạy.

## Cách chạy lại an toàn (đề xuất, chưa thử)
1. Commit hoặc backup `Design/Verification/` và hai scene chính (git hiện có thay đổi chưa commit).
2. Mở SchoolFloorPrototype, vào Play Mode, xác nhận game đang chạy (frame tăng) rồi gọi `TheReturn.Editor.<Tên>Checks.Run()` bằng execute_code.
3. Thoát Play Mode KHÔNG lưu scene; đọc `git diff Design/Verification` để xem kết quả thay đổi.
4. Báo cáo theo mức nghiệm thu, ghi rõ bài nào có thể sai do tọa độ cũ.

## Mức nghiệm thu (dùng khi báo cáo)
1 tạo file -> 2 compile (Console không lỗi CS) -> 3 test đã chạy (ghi tên + kết quả) -> 4 hành vi runtime xác nhận -> 5 chưa xác nhận.

## Cần làm tiếp
- Đọc từng `*Checks.cs` để ghi tác dụng phụ; quyết định có chạy qua execute_code hay bọc bằng wrapper.
- Nếu thêm test mới: ưu tiên NUnit EditMode cho Domain (assembly Domain không phụ thuộc Unity) để Test Runner/CLI chạy được.

## NUnit EditMode (T-002, 2026-10-15 07:52 UTC)
- Assembly `TheReturn.SchoolFloor.Domain.Tests` (Tests/EditMode/MapProgressTests.cs): 21 test cho MapProgress, chạy 0,63 s, không cần scene/Play Mode, không ghi đè file nào.
- Chạy qua Unity MCP: `run_tests` (mode EditMode, assembly_names ["TheReturn.SchoolFloor.Domain.Tests"]) rồi `get_test_job`; hoặc Window > General > Test Runner > EditMode. Kết quả 2026-10-15: 21/21 passed.
- Khuôn mẫu cho test Domain mới: asmdef Editor-only + overrideReferences + nunit.framework.dll + defineConstraints UNITY_INCLUDE_TESTS.

## Cập nhật 2026-10-16 10:26 UTC (T-006d)
- Thêm assembly NUnit TheReturn.Choir.Domain.Tests (24 test). Hiện có 3 assembly NUnit: TheReturn.SchoolFloor.Domain.Tests, TheReturn.BreakerPanel.Domain.Tests, TheReturn.Choir.Domain.Tests (chạy chung: 83/83 xanh ngày 2026-10-16).
- Khi dùng Unity MCP run_tests: LUÔN truyền assembly_names; gọi không kèm tên bị timeout phía Unity (gặp ngày 2026-10-16).
