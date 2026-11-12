# LOG — nhật ký vận hành gần đây

LOG không phải nguồn trạng thái. Xem `CURRENT.md`, `TASKS.md`, `QUEUE.md` và plan đang làm.

## Quy tắc chống phình
- Giữ tối đa 8 mục gần nhất và tối đa 8 KB; mỗi mục 1–3 dòng, ghi kết quả và mức xác minh.
- Chi tiết bền vững chuyển vào plan/decision/system card; trạng thái task chỉ lưu ở TASKS/QUEUE.
- Khi vượt giới hạn, gộp hoặc bỏ START/PROGRESS/DONE đã được ghi đầy đủ ở nơi chuẩn; không tạo bản sao LOG_FULL cho task/hệ thống ngoài Core4.
- Không chép nguyên lỗi test, danh sách file dài hoặc kết quả lặp. Ghi rõ chưa kiểm; M3 test, M4 runtime/build, M5 chủ dự án xác nhận.

## Mục gần nhất

### 2026-11-12 | T-042 S1–S6 | DONE (M3/M4 một máy)
- Đã nối PlankAndKey, BreakerPanel, Choir, Attendance vào MapSchoolF/MapFlowOnline; host Play một mình đi qua từng nhiệm vụ và reset được. Gauntlet: 981 test/25 assembly, 976 đạt, 0 đỏ, 5 bỏ qua.
- Chưa xác nhận thao tác thực qua mạng, người vào muộn hoặc smoke 2 máy. Chi tiết: `PLANS/T-042-net-wiring-mapschoolf.md`.

### 2026-11-12 | T-042 S7 | PROGRESS (M2)
- P-013 cho phép đổi scene đích và Build Settings; scene offline đích là MapSchoolF. Đường online phòng→map chưa nối, chờ P-014; Q-024 chờ bước trước.

### 2026-11-12 | T-043 | START
- Rà soát và thu gọn `.ai` theo phạm vi 4 nhiệm vụ, map mới và online; giữ nguyên thay đổi Assets/scene hiện có. Kế hoạch: `PLANS/T-043-ai-docs-core4-prune.md`.

### 2026-11-13 | T-043 | DONE (M2)
- Còn 49 file `.ai`/~95 KB; 7 plan; LOG 4 mục/25 dòng/<2 KB. Đã kiểm tham chiếu tài liệu bị xóa và `git diff --check` sạch.
- Không chạy Unity test; snippet index chưa chạy trong Editor. Không sửa Assets/code/scene/Build Settings; T-042/P-014 giữ nguyên.
