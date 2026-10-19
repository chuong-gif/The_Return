# DECISION 2026-10-16: duyệt thiết kế 6 nhiệm vụ còn trống (T-006)

Người quyết: chủ dự án (chat với phiên #3, ngày 2026-10-16). Bản đề xuất: PLANS/T-006-mission-designs.md.
1. Duyệt cả 6: Phong_5 BreakerPanel, Phong_6 Choir, Hanh_Lang_2 GlassBridge, Hanh_Lang_4 SharedLantern, Hanh_Lang_5 TeacherSays, Hanh_Lang_6 PlankAndKey.
2. Choir: khoảng thời gian tối đa giữa hai nốt PHẢI là thông số chỉnh được (không hardcode 2 giây). Lý do: giai đoạn phát triển một máy, đổi nhân vật bằng phím 1-4 nên người chơi không kịp. Triển khai: `StepWindowSeconds` (0 = không giới hạn) và `SwitchExtraSeconds` (cộng thêm mỗi lần đổi vai khi đang dở dãy). Mặc định giai đoạn phát triển: 5 giây và 1,5 giây (AI chọn, chỉnh sau bằng asset khi có Data catalog); khi có online nhiều máy nên hạ gần 2 giây.
3. Mặc định chấp nhận (chủ dự án không phản đối, chưa xác nhận rõ): tổng thời lượng map có thể dài hơn mục tiêu 22-30 phút; Hanh_Lang_6 cho đội 2 người giữ bản nhẹ; tên chủ đề phòng ổn.
4. CHƯA có quyết định về việc AI thêm cổng/khu chuẩn bị cho Hanh_Lang_2/4/5/6 vào scene. Mọi bước sửa scene phải hỏi lại chủ dự án (kèm Scene lock + snapshot).
5. Thứ tự làm đề xuất: Phong_5, Hanh_Lang_6, Hanh_Lang_5, Phong_6, Hanh_Lang_2, Hanh_Lang_4 (phiên #3 bắt đầu Domain của Phong_5 và Phong_6 vì không cần scene).

## Bổ sung 2026-10-16 10:29 UTC (chủ dự án, chat)
6. Mục 4 đã được trả lời MỘT PHẦN: cho phép sửa scene để dựng nhiệm vụ Phong_6 (Choir) với Scene lock + snapshot. Chưa cho phép cho các hành lang mới.

## Bổ sung 2026-10-16 10:48 UTC (chủ dự án, chat với phiên #4)
7. Mục 4 trả lời thêm MỘT PHẦN: cho phép sửa scene để dựng nhiệm vụ Hanh_Lang_5 (TeacherSays) gồm cổng + khu chuẩn bị. Vẫn chưa cho phép Hanh_Lang_2/4/6. Bắt buộc Scene lock + snapshot.
