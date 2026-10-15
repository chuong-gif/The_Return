# CURRENT (HOT — ≤ 30 dòng, ghi đè)

Cập nhật: 2026-10-15 08:05 UTC bởi Claude Sonnet 5.5 (chat + Unity MCP, phiên #3)
Scene lock: không ai giữ
Trọng tâm: luồng nhiệm vụ tự chọn phòng. DONE: T-001 (bảng kề), T-002 (MapProgress), T-003 (IMissionAdapter + MissionRegistry + 6 adapter + factory). Test NUnit 34/34 xanh; adapter thật mới compile. Gameplay/scene CHƯA đổi, chưa có gì gọi registry trong scene.
Cảnh báo:
- `SchoolFloorPrototype.unity` có thay đổi lúc 01:59 UTC chưa ai ghi/xác nhận (LOG). Đọc lại scene thực tế trước khi dùng giả định.
- Các bộ `*Checks` cũ lỗi thời (BUGS/quiet-corridor-...); đừng chạy lại để «xác nhận» gameplay mới. Test mới: NUnit EditMode (TESTING.md).
- Mọi MenuItem builder/migration: không chạy (AGENTS.md).
- T-012, T-013 do phiên Claude khác giữ (07:29, 07:40 UTC): không làm trùng.
Việc tiếp theo: T-004 (nối registry vào SchoolFloorFlow/AccessController) — dòng TASKS BLOCKED; cần chủ dự án duyệt nháp PLANS/T-004-wire-registry.md và quyết định: Phong_1/Attendance tự chạy từ đầu hay chờ người chơi chọn. Cần Scene lock + snapshot scene khi làm.
Việc khác chưa ai giữ, cần brief của chủ dự án: T-006 (nhiệm vụ cho Phong_5/6 + Hanh_Lang_2/4/5/6), T-008, T-010, T-011. T-007 (màn thắng) làm được sau T-004 (dùng MissionRegistry.Won).
Chủ dự án báo thay đổi ở: OWNER_UPDATES.md. Lịch sử: LOG.md. Git status chỉ lấy được bằng terminal ngoài.
