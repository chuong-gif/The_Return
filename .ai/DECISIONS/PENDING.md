# PENDING — câu hỏi chờ chủ dự án trả lời MỘT lượt (chính sách PROVISIONAL, duyệt 2026-11-06)

AI KHÔNG dừng vì câu hỏi ở đây: dùng phương án PROVISIONAL, ghi thêm dòng, làm tiếp. Chủ dự án trả lời xong -> đổi trạng thái; nếu khác phương án tạm thì người giữ nhiệm vụ sửa qua thẻ FIX trong QUEUE.

| P-id | Task | Câu hỏi | Phương án PROVISIONAL đang dùng | Cách đổi | Trạng thái |
|---|---|---|---|---|---|
| P-001 | T-028 | Đường «phòng -> map» (NetworkManager chưa có trong scene game): làm trong T-028 hay tách task? | Tách task riêng; T-028 dùng -netauto host|client + NetworkRoot tạo lúc chạy (phiên #9, 2026-11-04) | Tạo task mới cho luồng phòng->map | PROVISIONAL |
| P-002 | T-037 | Đồng hồ Bước/Đứng + cờ «Cô nói» chỉ gửi RIÊNG người nghe (brief ghi CHUNG nhưng lộ đáp án) | Gửi riêng (phiên #11, 2026-11-05) | Sửa TeacherSaysPublicState/HostRules | PROVISIONAL |
| P-003 | T-037 | Choir theo số người: N=2 -> 3s, N=3 -> 2,5s, N=4 -> 2s (ChoirRules.OnlineFor) | Như phiên #12 chọn (2026-11-05) | Sửa ChoirRules.OnlineFor + test | PROVISIONAL (xác nhận lại) |
| P-004 | T-040 | Mức bỏ 8 nhiệm vụ | ĐÃ TRẢ LỜI 2026-11-10: XÓA HẲN code sau khi scene mới chạy được (P7); trước đó đóng băng | - | ANSWERED |
| P-005 | T-040 | Hướng map | ĐÃ TRẢ LỜI 2026-11-10: DỰNG MỚI HOÀN TOÀN (PA2) | - | ANSWERED |
| P-009 | T-040 | Xóa code 8 nhiệm vụ làm scene cũ SchoolFloorPrototype hỏng: xóa luôn scene cũ + builder + *Checks cùng đợt? | Có: xóa cùng đợt ở P7, sau khi chủ dự án commit/tag git; chưa xóa gì trước đó | Chủ dự án muốn giữ scene cũ -> không làm P7 hoặc tách bản lưu ngoài dự án | PROVISIONAL |
| P-010 | T-040 | Sơ đồ layout map mới (P3a) | v1 (sân dài) và v2 (chữ U hở) bị BÁC. ĐÃ CÓ v3 vòng khép kín quanh sân ~90 x 60 m: PLANS/T-040-map-design.md. Chưa dựng gì cho tới khi chủ dự án duyệt | Duyệt xong mở Q-012 | CHỜ DUYỆT v3 |
