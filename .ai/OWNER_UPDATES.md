# OWNER UPDATES — chủ dự án báo thay đổi (CHỈ THÊM, mới nhất ở CUỐI)

Chủ dự án chỉ cần viết 3 dòng. Hoặc nói với bất kỳ AI nào «ghi vào OWNER_UPDATES: ...» — AI phải ghi vào đây.
Mẫu:
`### 2026-10-15 | OWNER`
`- Tôi đã đổi: ... (file/scene/khu vực nếu nhớ)`
`- AI cần làm gì/kiểm tra gì: ... (có thể để trống)`
AI khi xử lý xong mục nào thì ghi LOG `OWNER-CHANGE-PROCESSED` (không sửa mục ở đây).

---

### 2026-10-15 | OWNER (qua chat với Claude, AI ghi hộ)
- Đã commit các thay đổi chưa commit trước đó (git status 2026-10-14 có 17 file đã sửa + nhiều thư mục mới). Đã cài 5 skill Unity vào project.
- Trong object BlockoutCampus_Generated (scene SchoolFloorPrototype) đã đặt tên cho từng phòng và hành lang — dùng làm nguồn cho bảng kề (T-001).
- Điều kiện thắng game: làm xong hết nhiệm vụ là thắng, AI tự quyết cách làm.

### 2026-10-15 | OWNER (qua chat với Claude, AI ghi hộ)
- Đã đổi tên hành lang: hành lang 1 cũ thành hành lang 6, hành lang 2 cũ thành hành lang 1 (đúng thứ tự mong muốn). Tên object Hanh_Lang_1..6 trong scene là CHUẨN; nhãn cũ trên cổng («Hanh lang 2», «Hanh lang 4») đã lỗi thời.
- Muốn: mỗi AI nhận task phải đọc phần liên quan, lập kế hoạch chi tiết rồi làm từng task nhỏ, báo cáo ngắn; AI gặp lỗi phải tìm ra task/hệ thống của lỗi, đọc kế hoạch để biết chỗ lỗi và sửa với ít token nhất.

### 2026-10-15 | OWNER (qua chat với Claude, AI ghi hộ)
- Xác nhận giả định A1-A3 của PLANS/T-002-map-progress.md: (A1) phòng bắt đầu được từ hành lang kề VÀ từ sân; (A2) rời khu giữa chừng thì GIỮ tiến độ, không reset; (A3) khu chưa có nhiệm vụ không bắt đầu được và tự vào điều kiện thắng khi đăng ký nhiệm vụ.
- Yêu cầu: AI phải ghi trạng thái/tiến độ vào .ai (LOG, TASKS, CURRENT) trước khi hết hạn mức token 5 giờ để AI khác tiếp quản nhiệm vụ. Cho phép làm tiếp T-003.

### 2026-10-15 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- Xác nhận lại: Attendance (Phong_1) KHÔNG tự chạy, chờ người chơi chọn như mọi khu; yêu cầu phiên #3 tiếp nối T-004 (task đang do phiên #4 giữ từ 08:25, log cuối 08:50).

### 2026-10-16 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- DUYỆT bản đề xuất PLANS/T-006-mission-designs.md (6 nhiệm vụ: BreakerPanel, Choir, GlassBridge, SharedLantern, TeacherSays, PlankAndKey).
- Yêu cầu riêng cho Phong_6 (Choir): giá trị khoảng thời gian giữa hai nốt (đề xuất 2 giây) PHẢI chỉnh được, vì hiện đang phát triển một máy và đổi nhân vật bằng phím 1-4 nên sợ không kịp phản ứng.
- Chưa trả lời các câu hỏi 2-5 của đề xuất (tổng thời lượng, Hanh_Lang_6 cho 2 người, cho AI thêm cổng vào scene, tên chủ đề phòng): AI coi là «chấp nhận mặc định» cho 2, 3, 5; KHÔNG coi là đã cho phép sửa scene (câu 4) — hỏi lại khi tới bước scene.
