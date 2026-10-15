# Bảng kề phòng / hành lang / cổng (T-001, đọc từ scene 2026-10-15)

Nguồn: scene `SchoolFloorPrototype`, object `BlockoutCampus_Generated/Architecture_From_User_Blockout` (tên do chủ dự án đặt: `Phong_1..6`, `Hanh_Lang_1..6`, `Khu_Chuan_Bi_1..6`, `Cau_Thang_1..6`, `San`), cùng 22 `SchoolPreparationGate` và 5 `transitionGatherZones`. Chỉ đọc, không sửa scene. Nhãn: [V] đo từ scene, [S] suy luận, [?] chưa rõ. Tên chuẩn dùng trong bảng này = TÊN OBJECT (Phong_k, Hanh_Lang_k). Số thứ tự trong tên cổng (`Hanh lang 2`, `Hanh lang 4`) là nhãn cũ, xem mục 3.

## 1. Vòng tròn [V]
Phong_1 -> Hanh_Lang_1 -> Phong_2 -> Hanh_Lang_2 -> Phong_3 -> Hanh_Lang_3 -> Phong_4 -> Hanh_Lang_4 -> Phong_5 -> Hanh_Lang_5 -> Phong_6 -> Hanh_Lang_6 -> Phong_1.
Đo bằng khoảng cách tâm: mỗi hành lang cách đúng 2 phòng kề 59 đơn vị, phòng thứ 3 gần nhất cách 162.

| Hành lang | Nối 2 phòng |
|---|---|
| Hanh_Lang_1 | Phong_1, Phong_2 |
| Hanh_Lang_2 | Phong_2, Phong_3 |
| Hanh_Lang_3 | Phong_3, Phong_4 |
| Hanh_Lang_4 | Phong_4, Phong_5 |
| Hanh_Lang_5 | Phong_5, Phong_6 |
| Hanh_Lang_6 | Phong_6, Phong_1 |
Phòng k kề Hanh_Lang_(k-1) (Phòng 1: Hanh_Lang_6) và Hanh_Lang_k.

## 2. Cổng của mỗi phòng (3 cổng/phòng, 18 cổng) [V cho Cua 1/2, S cho Cua 3]
- `Phong k_Cua 1` (designatedEntrance=true): hướng ra Hanh_Lang_(k-1) (đo: gần hành lang đó 38, hành lang kia 81).
- `Phong k_Cua 2`: hướng ra Hanh_Lang_k (38 / 81).
- `Phong k_Cua 3`: cách đều hai hành lang (58 / 58) và xa sân hơn (92) => nằm trên trục đối xứng của phòng, nhiều khả năng là cổng hướng sân [S]. Mỗi phòng có 1 cầu thang `Cau_Thang_k` + `RadialPreparation_(k-1)` + `StairLanding/StairRamp_(k-1)` (đi từ sân lên sàn phòng) — khớp với việc đây là lối vào từ sân [S]; chưa xác minh đường đi thực tế.
Khớp mô tả của chủ dự án: phòng 1 có 3 cổng chuẩn bị (sân->phòng, phòng->hành lang kề phía trước, hành lang kề phía sau->phòng).

## 3. Nhiệm vụ <-> khu thực (theo cổng, `mission` = chỉ số tuyến tính cũ)
| mission | Nhiệm vụ | Nhãn trên cổng | Khu thực trong scene |
|---|---|---|---|
| 0 | Attendance | Phong 1 | Phong_1 [V] |
| 1 | QuietCorridor | Hanh lang 2 (Vao/Ra) | Hanh_Lang_1 [V] — cổng nằm trong Hanh_Lang_1 |
| 2 | GradeRepair | Phong 2 | Phong_2 [V theo cổng] |
| 3 | NavigationExam | Phong 3 | Phong_3 [V theo cổng] |
| 4 | DontLookBack | Hanh lang 4 (Vao/Ra) | Hanh_Lang_3 [V] — cổng nằm trong Hanh_Lang_3 |
| 5 | TeacherTruth | Phong 4 | Phong_4 [V theo cổng] |
| -1 | (không có) | Phong 5, Phong 6 | Phong_5, Phong_6: 3 cổng mỗi phòng, luôn mở |
Hành lang CHƯA có nhiệm vụ và CHƯA có cổng/khu chuẩn bị nào: Hanh_Lang_2, 4, 5, 6.

## 4. ĐÁNH SỐ — ĐÃ CHỐT 2026-10-15: tên object là chuẩn (chủ dự án: đã đổi hành lang 1 cũ -> 6, 2 cũ -> 1). Nhãn cổng cũ lỗi thời: «Hanh lang 2» = Hanh_Lang_1, «Hanh lang 4» = Hanh_Lang_3 (đã đối chiếu scene). Quy tắc đổi số tổng quát cũ k -> k-1 (cũ 1 -> 6) là suy luận, chưa kiểm hết. (Mô tả xung đột bên dưới giữ để hiểu lịch sử.)
- Tên cổng dùng nhãn theo thứ tự nhiệm vụ cũ: «Hanh lang 2» thực ra là object Hanh_Lang_1, «Hanh lang 4» thực ra là Hanh_Lang_3. Tên object theo vòng tròn: Hanh_Lang_k nối Phong_k và Phong_(k+1).
- Khi chủ dự án nói «phòng 1 đi tới hành lang 2 và hành lang 6 đi vào phòng 1»: «hành lang 2» khớp nhãn cổng cũ (= Hanh_Lang_1), «hành lang 6» khớp tên object (= Hanh_Lang_6). Hai cách đánh số bị trộn.
- Đề xuất: lấy TÊN OBJECT làm chuẩn cho mọi tài liệu và code mới, đổi nhãn cổng sau (việc của chủ dự án hoặc task riêng). Chờ xác nhận.

## 5. Điểm bất thường cần kiểm ở bước sau [?]
- Vị trí gốc (root) của một số prefab nhiệm vụ không khớp khu của cổng: gradeRepair root nằm trong Hanh_Lang_1, navigationExam root trong Hanh_Lang_2, dontLookBack và teacherTruth root nằm ngoài mọi khu (gần Phong_3). Có thể root chỉ là container/vị trí cũ; vị trí chơi thật chưa xác minh. Dùng cổng và khu chuẩn bị làm nguồn cho vị trí khu, không dùng root.
- `transitionGatherZones[0..4]` đang là: [0] khu chuẩn bị của Hanh_Lang_1 (chồng lên Phong_1), [1] Phong_2, [2] Phong_3, [3] Hanh_Lang_3, [4] Phong_4 (cho 5 chuyển tiếp tuyến tính).
- 22 cổng = 18 cổng phòng + 4 cổng hành lang (2 hành lang có nhiệm vụ x Vao/Ra).
- Scene thay đổi lúc 01:59 UTC 2026-10-15 chưa rõ ai (LOG). Dữ liệu trên là scene đọc ở thời điểm sau mốc đó.
