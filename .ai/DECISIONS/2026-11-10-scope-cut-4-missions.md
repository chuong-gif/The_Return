# Quyết định: giới hạn game còn 4 nhiệm vụ

Chủ dự án chốt ngày 2026-11-10 vì tiến độ phát triển quá lâu và cần ưu tiên phần có thể hoàn tất nhanh hơn.

## Phạm vi còn lại
- PlankAndKey — Hanh_Lang_6
- Attendance — Phong_1
- BreakerPanel — Phong_5
- Choir — Phong_6
- Giữ hành lang nối Phong_5 và Phong_6 làm lối đi, không có nhiệm vụ.
- Map hiện hành là MapSchoolF do chủ dự án dựng; thiết kế vòng map cũ trong T-040 chỉ là lịch sử.

## Phần bị cắt và điều kiện xóa
TeacherSays, GlassBridge, SharedLantern, QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth không được tiếp tục phát triển.

Chủ dự án chọn xóa code của chúng sau khi map mới chạy được, nhưng chưa cho phép bắt đầu xóa. Q-013 chỉ được mở sau khi chủ dự án xác nhận map mới chạy và đã commit/tag; trước đó giữ nguyên code và scene. Danh sách phụ thuộc/xóa an toàn: `PLANS/T-040-dependencies.md`.

## Luật/tác động đã chốt
- Mục tiêu online: đội 2–4 người, NGO + Unity Relay + Vivox, host-authoritative, không host migration.
- Điều kiện mở PlankAndKey cho phép vào từ sân, Choir và Attendance; cờ YardEntry mặc định tắt để giữ hành vi cũ.
- P-013 cho phép đổi scene đích offline và Build Settings sang MapSchoolF. Luồng online room→map chưa được duyệt, xem P-014.

Lịch sử sơ đồ/phiên bản cũ không cần thiết đã được lược bỏ; scene/code hiện hành là nguồn sự thật.
