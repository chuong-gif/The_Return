# TASKS (bảng task; sửa đúng dòng của mình; lịch sử chi tiết ở LOG.md)

Cột Đọc: thẻ hệ thống + kế hoạch (nếu có) + file cần mở; quy trình lập kế hoạch: PROTOCOL mục 12. Trạng thái: TODO | CLAIMED | IN-PROGRESS | PAUSED | BLOCKED | STALE | REVIEW | DONE. Giờ UTC. Chi tiết quy trình: PROTOCOL.md.

Task DONE chuyển sang `.ai/TASKS_DONE.md` (2026-10-21). Mô tả đầy đủ của từng task ở LOG/PLANS; cột Task ≤ 140 ký tự (việc CÒN lại ghi rõ). Bản đầy đủ trước 2026-10-22: SESSIONS/TASKS_FULL_20261022.md.

| ID | Trạng thái | Giữ | Cập nhật | Task | Đọc | Nghiệm thu |
|---|---|---|---|---|---|---|
| T-005 | IN-PROGRESS | Claude Sonnet 5.5 (phiên #4 cửa sổ chat thứ ba) | 2026-10-19 09:21 | Test Play Mode NUnit cho luồng cổng/chọn khu/RestartMap. Chỉ thêm assembly mới, không sửa scene. | Tests/PlayMode (SchoolFloor); TESTING.md | Test chạy xanh |
| T-006a2 | REVIEW | Claude Sonnet 5.5 (phiên #3) | 2026-10-17 12:52 | Phong_5 Cầu dao: code + scene xong. CÒN: chủ dự án chơi thật. | PLANS/T-006a-breaker-panel.md | Compile + 83/83 test + registry dựng được (mức 3) |
| T-006b2 | REVIEW | Claude Sonnet 5.5 (phiên MCP #2) | 2026-10-21 16:12 | Hanh_Lang_6 PlankAndKey: Domain + Session + Presentation + adapter + đăng ký + s…  | PLANS/T-006b2-plank-presentation.md; SYSTEM_CARDS/PlankAndKey.md | Compile + test (mức 3) + Play Mode (mức 4); cần Scene lock + snapshot |
| T-006c | REVIEW | Claude Sonnet 5.5 (phiên #3, tiếp quản từ #4) | 2026-10-18 15:56 | Hanh_Lang_5 Cô nói: code + scene xong, Play Mode giả lập qua. CÒN: chơi thật, nhánh cảnh cáo/bị đưa về, cổng thật. | PLANS/T-006c-teacher-says.md; SYSTEM_CARDS/TeacherSays.md | Compile + 18/18 test (mức 3); Play Mode mô phỏng (mức 4 một phần) |
| T-006d2 | REVIEW | Claude Sonnet 5.5 (phiên #3) | 2026-10-17 02:37 | Phong_6 Choir: code + scene xong. CÒN: chủ dự án chơi thật. | PLANS/T-006d2-choir-presentation.md; SYSTEM_CARDS/Choir.md | Compile + test 67/67 xanh (mức 3); Play Mode chưa |
| T-006e2 | REVIEW | Claude Sonnet 5.5 (phiên #4, cửa sổ chat thứ ba) | 2026-10-17 12:33 | Hanh_Lang_2 GlassBridge: code + scene + 2 cổng xong. CÒN: chủ dự án chơi thật. | PLANS/T-006e-glass-bridge.md; SYSTEM_CARDS/GlassBridge.md | Mức 4 một phần |
| T-006f | REVIEW | Claude Sonnet 5.5 (phiên #5; phiên #4 kiểm tiếp 2026-10-18 16:39) | 2026-10-19 00:15 | Hanh_Lang_4 SharedLantern: code + scene xong, Play Mode giả lập qua. CÒN: chơi thật, test tích hợp. | PLANS/T-006f-shared-lantern.md; SYSTEM_CARDS/SharedLantern.md | Chủ dự án chơi thật xác nhận |
| T-008 | IN-PROGRESS | Claude Sonnet 5.5 (phiên #4, cửa sổ chat thứ ba) | 2026-10-21 10:57 | MainMenu + lập nhóm (IPartyService, một máy) + Settings (âm lượng, độ nhạy, toàn…  | PLANS/T-008-menu-party-settings.md; Features/Party, Features/GameSettings, Maps/MainMenu | Mức 4 Play Mode + test |
| T-009 | BLOCKED | - | 2026-10-15 | Online co-op 2-4 người (hiện đổi vai bằng phím 1-4). BLOCKED: chưa chọn công nghệ mạng. | PROJECT.md | Cần quyết định của chủ dự án |
| T-010 | TODO | - | 2026-10-15 | Nhân vật + animation. Cần chủ dự án cho asset/hướng. | PROJECT.md | Cần brief |
| T-011 | REVIEW | Claude Sonnet 5.5 (phiên #3) | 2026-10-21 10:45 | UI theo khu XONG S1-S5 (HUD chung, bản đồ nhỏ sân, phím N sổ tay, 12 khu dùng provider). CÒN: chơi thật. | PLANS/T-011-area-ui.md | Compile + 49/49 test; Play Mode mô phỏng cả 6 khu cũ + HL5 |
| T-012 | REVIEW | Claude Sonnet 5.5 (phiên #5) | 2026-10-22 01:40 | Tài liệu prefab/vật tương tác: PREFAB_GUIDE (14 prefab cũ, phiên trước) + 6 README mới cho BreakerPanel, Choir, GlassBridge, PlankAndKey, SharedLantern, TeacherSays + mục «Nhiệm vụ mới» (phiên này). Chỉ tài liệu. Chưa chuyển vật nào thành prefab (việc riêng, cần Scene lock) | Assets/TheReturn/PREFAB_GUIDE.md; Features/*/README.md | Chủ dự án xem tài liệu |
| T-017 | REVIEW | Claude Sonnet 5.5 (phiên #3) | 2026-10-18 09:47 | Không mặc định vào Phong_1: UI tự chọn phòng + F8 test 12 khu + nhãn cổng. CÒN: chơi thật. | PLANS/T-004-wire-registry.md; LOG T-017 | Compile + 83/83 + Play Mode mô phỏng 9/11 khu; chờ người chơi thật |
