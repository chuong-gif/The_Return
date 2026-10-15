# TASKS (bảng task; sửa đúng dòng của mình; lịch sử chi tiết ở LOG.md)

Cột Đọc: thẻ hệ thống + kế hoạch (nếu có) + file cần mở; quy trình lập kế hoạch: PROTOCOL mục 12. Trạng thái: TODO | CLAIMED | IN-PROGRESS | PAUSED | BLOCKED | STALE | REVIEW | DONE. Giờ UTC. Chi tiết quy trình: PROTOCOL.md.

| ID | Trạng thái | Giữ | Cập nhật | Task | Đọc | Nghiệm thu |
|---|---|---|---|---|---|---|
| T-000 | DONE | Claude | 2026-10-15 | Thiết lập memory/index/skill/handoff | AGENTS.md, PROTOCOL.md | .ai/ đủ để AI mới bắt đầu |
| T-001 | DONE | Claude | 2026-10-15 06:22 | S0: bảng kề -> `.ai/MAPS/area-adjacency.md`; chủ dự án xác nhận đánh số theo tên object | MAPS/area-adjacency.md | Xong |
| T-002 | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-15 07:52 | S1: `MapProgress` (C# thuần, assembly TheReturn.SchoolFloor.Domain) + NUnit EditMode test, chưa nối scene. Chủ dự án đã duyệt kế hoạch 2026-10-15 | PLANS/T-002-map-progress.md | XONG: 21/21 test NUnit EditMode xanh qua Test Runner, Console 0 lỗi (mức 3) |
| T-003 | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-15 08:05 | S2: adapter 6 nhiệm vụ + `MissionRegistry` (chưa đổi hành vi, chưa gắn scene). Chủ dự án cho làm tiếp 2026-10-15 | PLANS/T-003-mission-adapters.md | XONG: compile sạch, 34/34 test NUnit xanh (registry bằng adapter giả; adapter thật mới mức 2) |
| T-004 | BLOCKED | - | 2026-10-15 08:05 | S3: nối registry vào SchoolFloorFlow/AccessController (cần Scene lock + snapshot). Chờ chủ dự án duyệt nháp PLANS/T-004-wire-registry.md + quyết định Attendance (Phong_1) tự chạy hay chờ chọn | PLANS/T-004-wire-registry.md; SchoolFloor/Runtime | Compile + chơi thử Play Mode |
| T-005 | BLOCKED | - | 2026-10-15 | S4-S5: kiểm Play Mode luồng tự chọn, thay 5 bộ *Checks cũ | TESTING.md; BUGS/quiet-corridor-... | Test mới xanh, tọa độ lấy từ scene |
| T-006 | BLOCKED | - | 2026-10-15 | Thiết kế nhiệm vụ cho 2 phòng + 4 hành lang chưa có — cần brief của chủ dự án | MISSIONS_TODO.md | Brief được duyệt trước khi làm |
| T-007 | TODO | - | 2026-10-15 | Màn hình thắng game (thắng khi mọi nhiệm vụ đã đăng ký xong; AI tự chọn cách hiển thị) — làm sau T-002/T-003 | DECISIONS/2026-10-14-... | Xong hết nhiệm vụ => hiện màn thắng |
| T-008 | TODO | - | 2026-10-15 | Màn hình bắt đầu, tìm/lập nhóm, Settings, Sảnh chờ (Sanh_Cho) — cần chủ dự án chốt công nghệ mạng trước phần online | PROJECT.md (mục Sản phẩm) | Cần brief |
| T-009 | BLOCKED | - | 2026-10-15 | Tích hợp online co-op 2-4 người (hiện đổi người bằng phím 1-4) — chưa chọn công nghệ mạng | PROJECT.md | Cần quyết định của chủ dự án |
| T-010 | TODO | - | 2026-10-15 | Nhân vật + animation — cần chủ dự án cho asset/hướng | PROJECT.md | Cần brief |
| T-011 | TODO | - | 2026-10-15 | Tổ chức lại UI (nhiều không gian nhìn, linh hoạt theo khu); skill ui-ugui | CONVENTIONS.md; SchoolFloor/Runtime/UI | Cần brief |
| T-012 | IN-PROGRESS | Claude Sonnet 5.5 | 2026-10-15 07:29 | Tài liệu cách dùng cho từng prefab tương tác (để chủ dự án thay asset 3D) | Features/*/README.md, CONVENTIONS.md | Mỗi prefab có mô tả |
| T-013 | IN-PROGRESS | Claude Sonnet 5.5 (phiên MCP #2) | 2026-10-15 07:40 | Rà soát sâu skill unity-cli; thử Claude Code mở tại gốc dự án có nạp .claude/skills không | TOOLING.md | Ghi kết quả vào TOOLING.md |
| T-014 | DONE | Claude | 2026-10-15 06:33 | Hiện trạng 6 nhiệm vụ đã ghi vào SYSTEM_CARDS (luật, scene, bằng chứng + ngày, thiếu sót). Chưa chạy lại test; mục B chỉ suy từ code, chỉ hàng QuietCorridor đầu là lỗi thật | SYSTEM_CARDS/<Tên>.md | Mỗi thẻ có B/C |
| T-015 | DONE | Claude | 2026-10-15 06:38 | Đã sửa số hành lang trong PROJECT_STATE.md, Design/Bo-cuc-theo-blockout.md, Design/Ke-hoach-tai-bo-tri-v2.md, Assets/TheReturn/README.md, Features/README.md, README DontLookBack và QuietCorridor (14 chỗ) | AGENTS.md (mục ĐỔI SỐ) | Xong; còn nhãn cổng trong scene (việc của chủ dự án) |
