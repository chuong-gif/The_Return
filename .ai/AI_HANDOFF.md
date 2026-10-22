# AI HANDOFF — The Return

- Gốc dự án: `E:\kho model 3D\Project Unity\The Return`. Unity 6000.3.7f1, URP 17.3. Nhánh Git hiện tại: `main` (đọc từ .git/HEAD; status chưa xác minh).
- QUY TRÌNH ĐẦY ĐỦ (nhận task, ghi LOG, tiếp quản việc dở, chủ dự án báo thay đổi): `.ai/PROTOCOL.md`. Nhật ký: `.ai/LOG.md`. Thay đổi do chủ dự án làm: `.ai/OWNER_UPDATES.md`.
- Đọc đầu tiên: `.ai/START_HERE.md` -> `.ai/CURRENT.md` (KHÔNG đọc PROJECT_STATE.md trừ khi task cần). Sau đó chỉ đọc phần liên quan: `.ai/ARCHITECTURE.md`, `.ai/SYSTEMS.md`, README của feature.
- Task hiện tại: `.ai/CURRENT.md`; backlog: `.ai/TASKS.md`.
- Tra kiến trúc: `.ai/ARCHITECTURE.md`, `.ai/MAPS/`. Code graph/index: CHƯA có (xem TOOLING.md). Tạm thời tìm tên file/symbol trong `Assets/TheReturn` rồi mở file liên quan.
- Công cụ: xem `.ai/TOOLING.md`. Unity phải đang mở và MCP đã Start Session.
- Compile/test: Console không lỗi CS là mức 2; cách chạy test: `.ai/TESTING.md` (chưa hoàn tất).
- An toàn: xem AGENTS.md (không chạy lại builder cũ, không commit/push, snapshot scene trước khi sửa).
- Sau task: cập nhật CURRENT.md và TASKS.md; ghi DECISIONS/BUGS chỉ khi có tri thức lâu dài.
- Giới hạn đã biết: Git và CLI chưa chạy được qua MCP; skill chưa cài; index chưa dựng; save/load, UI chi tiết, animation chưa khảo sát.
- Nếu memory có thể lỗi thời: so ngày trong CURRENT.md/PROJECT_STATE.md với `git log` và mtime file; khi mâu thuẫn, tin source code và trạng thái Unity thực tế rồi sửa file memory.
- Không ghi API key/token/mật khẩu vào bất kỳ file nào.
