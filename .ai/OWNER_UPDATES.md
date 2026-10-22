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

### 2026-10-16 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- TRẢ LỜI câu 4 của đề xuất T-006: CHO PHÉP AI thêm đối tượng vào scene cho nhiệm vụ Phong_6 (Hợp xướng): bục, bản nhạc, HUD, controller. Bắt buộc Scene lock + snapshot scene trước khi sửa. Phạm vi được phép lần này: CHỈ khu Phong_6; chưa cho phép thêm cổng/khu chuẩn bị cho Hanh_Lang_2/4/5/6 (hỏi lại khi tới T-006e/f/c/b).

### 2026-10-16 | OWNER (qua chat với Claude phiên #4, AI ghi hộ)
- Bảo AI nhận T-006c (Hanh_Lang_5 «Cô nói») và «dựng cảnh đi»: CHO PHÉP AI thêm cổng + khu chuẩn bị + đối tượng nhiệm vụ cho Hanh_Lang_5 vào scene (bắt buộc Scene lock + snapshot). Chưa mở rộng cho Hanh_Lang_2/4/6.

### 2026-10-16 | OWNER (qua chat với Claude phiên MCP #2, AI ghi hộ)
- Trả lời báo cáo T-006b: «ok, tôi xác nhận, làm đi». AI hiểu là đồng ý 3 điểm: (1) CHO PHÉP AI sửa scene cho Hanh_Lang_6 (cổng + khu chuẩn bị + đối tượng nhiệm vụ), bắt buộc Scene lock + snapshot; chỉ Hanh_Lang_6; (2) hệ số đổi đơn vị ra giây: chủ dự án không nêu số, AI đặt mặc định 3 giây/đơn vị, chỉnh được; (3) đồng hồ chỉ tính thời gian qua cầu. Chi tiết: DECISIONS/2026-10-16-t006b2-plank-owner-confirm.md.

### 2026-10-17 | OWNER (qua chat với Claude phiên #5, AI ghi hộ)
- T-006f SharedLantern: (1) về điểm đầu thì GIỮ sổ riêng; (2) nhập sai bảng cuối PHẢI có phạt; (3) AI tự tạo cổng/khu chuẩn bị cho Hanh_Lang_4 (được phép sửa scene cho khu này, vẫn cần Scene lock).

### 2026-10-17 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- CHO PHÉP AI sửa scene cho Phong_5 (Bảng cầu dao): thêm đối tượng nhiệm vụ trong phòng; bắt buộc Scene lock + snapshot. Chỉ Phong_5.

### 2026-10-18 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- Khi bắt đầu chơi KHÔNG muốn mặc định là nhiệm vụ Phong_1: người chơi tự chọn phòng/hành lang rồi chơi. Yêu cầu: cập nhật UI và chỗ test nhanh (F8) cho đủ nhiệm vụ mới. Cho phép sửa scene nếu cần.

### 2026-10-18 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- Bảo AI làm Hanh_Lang_5 (Cô nói); CHO PHÉP dùng/sửa scene; nói các AI khác đang tạm nghỉ. => Phiên #3 tiếp quản T-006c từ phiên #4 (im từ 2026-10-16) và nhận Scene lock từ phiên #5 khi tới bước scene.

### 2026-10-19 | OWNER (qua chat với Claude phiên MCP #2, AI ghi hộ)
- Nói: «nếu cần sửa scene thì cứ làm, tôi cho phép, không phải đợi». AI hiểu: tiếp tục phép sửa scene cho Hanh_Lang_6 (T-006b2) đã có từ 2026-10-16, KHÔNG cần chờ phiên khác khi khóa scene đã trống; vẫn nhận Scene lock + snapshot theo giao thức, chỉ phạm vi Hanh_Lang_6. Nếu hiểu sai, chủ dự án nói lại.

### 2026-10-19 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- Duyệt kế hoạch T-011: CHO PHÉP sửa scene (Canvas); sổ tay vai chỉ hiện khi bấm phím; sân dùng bản đồ nhỏ.

### 2026-10-20 | OWNER (qua chat với Claude phiên #3, AI ghi hộ)
- «Mở khóa scene làm đi, tôi cho phép»: phiên #3 được lấy Scene lock từ MCP #2 cho T-011.
