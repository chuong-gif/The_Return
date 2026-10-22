# TASKS_DONE (COLD — chỉ tra khi cần; không đọc lúc khởi động)

| ID | Trạng thái | Giữ | Cập nhật | Task | Đọc | Nghiệm thu |
|---|---|---|---|---|---|---|
| T-004 | DONE | Claude Sonnet 5.5 (phiên #4) | 2026-10-15 23:37 | Luồng tự chọn khu đã nối (MissionFlowController). Lỗi chủ dự án báo sau khi chơi thật (cửa mặc định đóng, khu chuẩn bị ngoài cửa, UI sân + tên vai) xử lý ở T-016, KHÔNG ở đây | PLANS/T-004-wire-registry.md | Xong phần nối; lỗi sau thử -> T-016
| T-006e | DONE | Claude Sonnet 5.5 (phiên #4, cửa sổ chat thứ ba) | 2026-10-17 02:42 | Hanh_Lang_2 GlassBridge: Domain thuần + 27 test NUnit xanh (S1). Giả định tự chọn xem PLANS/T-006e-glass-bridge.md | PLANS/T-006e-glass-bridge.md; Features/GlassBridge | Mức 3 (test xanh)
| T-016 | DONE | Claude Sonnet 5.5 (phiên #4 cửa sổ chat thứ ba, tiếp quản 2026-10-18 10:05) | 2026-10-18 10:05 | Luật cửa mới xong: mọi cửa đóng từ đầu; cả đội ở khu chuẩn bị ngoài cửa thì cửa mở; vào trong đủ đội thì bắt đầu + khóa; xong mở cả 3 lối; UI do T-017 | SYSTEM_CARDS/SchoolFloor.md mục C | Play Mode giả lập qua; 185/185 test; chưa người chơi thật

| T-000 | DONE | Claude | 2026-10-15 | Thiết lập memory/index/skill/handoff | AGENTS.md, PROTOCOL.md | .ai/ đủ để AI mới bắt đầu |
| T-001 | DONE | Claude | 2026-10-15 06:22 | S0: bảng kề -> `.ai/MAPS/area-adjacency.md`; chủ dự án xác nhận đánh số theo tên object | MAPS/area-adjacency.md | Xong |
| T-002 | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-15 07:52 | S1: `MapProgress` (C# thuần, assembly TheReturn.SchoolFloor.Domain) + NUnit EditMode test, chưa nối scene. Chủ dự án đã duyệt kế hoạch 2026-10-15 | PLANS/T-002-map-progress.md | XONG: 21/21 tes… |
| T-003 | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-15 08:05 | S2: adapter 6 nhiệm vụ + `MissionRegistry` (chưa đổi hành vi, chưa gắn scene). Chủ dự án cho làm tiếp 2026-10-15 | PLANS/T-003-mission-adapters.md | XONG: compile sạch, 34/34 test NUnit xanh (r… |
| T-006 | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-16 00:16 | Thiết kế nhiệm vụ cho Phong_5/6 + Hanh_Lang_2/4/5/6: chủ dự án ĐÃ DUYỆT 2026-10-16; tách T-006a..f bên dưới | PLANS/T-006-mission-designs.md; DECISIONS/2026-10-16-t006-mission-designs-approved.… |
| T-006a | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-18 09:35 | Phong_5 BreakerPanel Domain + 16 test XONG; Presentation/scene làm ở T-006a2 | PLANS/T-006a-breaker-panel.md; SYSTEM_CARDS/BreakerPanel.md | 16/16 test (mức 3) |
| T-006b | DONE | Claude Sonnet 5.5 (phiên MCP #2) | 2026-10-16 11:00 | Hanh_Lang_6 PlankAndKey (cầu ván + thẻ từ): S1 Domain + NUnit test XONG (chưa Presentation/scene) | PLANS/T-006b-plank-and-key.md | 25/25 test xanh (mức 3) |
| T-006d | DONE | Claude Sonnet 5.5 (phiên #3) | 2026-10-16 10:26 | Phong_6 Choir (thời gian giữa hai nốt chỉnh được): S1 Domain + NUnit test XONG (chưa Presentation/scene) | PLANS/T-006d-choir.md | 24/24 test xanh, chạy chung 83/83 (mức 3) |
| T-007 | DONE | Claude Sonnet 5.5 (phiên #5) | 2026-10-16 01:51 | Màn hình thắng: WinSummary (9 test) + WinScreenPresenter + object `WinScreen` đã gắn và lưu trong SchoolFloorPrototype (snapshot BeforeWinScreen_20261016_015040). Kiểm Play Mode mức 4 với objec… |
| T-013 | DONE | Claude Sonnet 5.5 (phiên MCP #2) | 2026-10-16 00:26 | Rà soát unity-cli (chỉ đọc, không chạy CLI) và xác minh đọc/nạp skill trong phiên Claude Desktop + Unity MCP. Phạm vi: đọc được 5 SKILL.md; 5 skill không có trong available_skills của phiên… |
| T-014 | DONE | Claude | 2026-10-15 06:33 | Hiện trạng 6 nhiệm vụ đã ghi vào SYSTEM_CARDS (luật, scene, bằng chứng + ngày, thiếu sót). Chưa chạy lại test; mục B chỉ suy từ code, chỉ hàng QuietCorridor đầu là lỗi thật | SYSTEM_CARDS/<Tên>.md | Mỗi thẻ có B/C |
| T-015 | DONE | Claude | 2026-10-15 06:38 | Đã sửa số hành lang trong PROJECT_STATE.md, Design/Bo-cuc-theo-blockout.md, Design/Ke-hoach-tai-bo-tri-v2.md, Assets/TheReturn/README.md, Features/README.md, README DontLookBack và QuietCorridor (14 chỗ) | AGENTS.md… |
