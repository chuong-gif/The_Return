# CURRENT (HOT — ≤ 30 dòng, ghi đè)

Cập nhật: 2026-10-17 12:57 UTC bởi Claude Sonnet 5.5 (chat + Unity MCP, phiên MCP #2; các phần khác của file do phiên trước ghi)
Scene lock: TRỐNG (phiên #3 đã nhả 2026-10-18 15:56 UTC sau T-006c; scene đã lưu, snapshot BeforeTeacherSays_20261018_155142.unity). Hanh_Lang_5 TeacherSays đã có trong scene (REVIEW, chờ chơi thật). Hanh_Lang_6 vẫn chưa có object scene (T-006b2).
Trọng tâm: luồng nhiệm vụ tự chọn phòng ĐÃ NỐI VÀO GAME. DONE: T-001..T-004. Đội xuất hiện giữa sân; Attendance không tự chạy; cả đội đứng đủ trong vùng chuẩn bị của khu chưa xong thì khu bắt đầu và khóa cả 3 lối; xong thì mở lại. Code: Maps/SchoolFloor/Runtime/Missions/MissionFlowController.cs; scene có object MissionFlow_Controller (bảng 22 cổng). EditMode 43/43 xanh.
Cảnh báo:
- Chủ dự án đang tự đi thử từng phòng trong Play Mode: đừng dừng/điều khiển Play Mode, đừng lưu scene khi chưa hỏi.
- Phiên #3 đã ghi TAKEOVER T-004 (13:06) rồi chủ dự án giao lại cho #4; xem LOG CORRECTION/TAKEOVER cuối. Phiên #3 nếu còn: không sửa T-004.
- SchoolFloorPrototype.unity có lần lưu 11:42 UTC không rõ ai (UNLOGGED-CHANGE). Đọc scene thực tế trước khi dựa vào giả định cũ.
- Các bộ *Checks cũ lỗi thời (BUGS/quiet-corridor-...); đừng chạy lại. Test mới: NUnit EditMode (TESTING.md).
- Mọi MenuItem builder/migration: không chạy (AGENTS.md). T-012, T-013 do phiên khác giữ.
- File thừa: Assets/Screenshots/screenshot-20261015-201221.png (chủ dự án xóa tay).
Chưa kiểm/để lại: vòng bắt đầu-kết thúc thật của Phong_2..4 + hành lang 3 (T-005); Canvas còn hiện bảng Điểm danh ở sân (T-011); MissionTestLauncher không tự chuyển khu.
Việc tiếp theo: T-005 (TODO, kiểm Play Mode luồng mới, thay *Checks cũ); T-007 (màn thắng) ĐÃ GẮN SCENE (object WinScreen), chờ chủ dự án chơi thật xem màn thắng hiện sau 6 nhiệm vụ; T-011 UI theo khu cần brief; T-006 ĐÃ DUYỆT (2026-10-16): T-006a BreakerPanel Domain xong (16 test), T-006d Choir Domain xong (24 test, mốc thời gian chỉnh được), T-006b do phiên MCP #2 giữ; Presentation/scene của khu mới cần chủ dự án cho phép sửa scene (T-006d2 BLOCKED, DECISIONS 2026-10-16 mục 4); T-008/T-010 cần brief của chủ dự án.
Chủ dự án báo thay đổi ở: OWNER_UPDATES.md. Lịch sử: LOG.md. Git status chỉ lấy được bằng terminal ngoài.
