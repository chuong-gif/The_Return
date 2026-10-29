# Cổng "Hanh lang 1_Vao/_Ra" (khu Hanh_Lang_6) có vùng chuẩn bị mặc định 1x1x1

- Phát hiện: 2026-10-28, phiên #6, qua SchoolFloorFlowPlayTests.EveryGate_OfEveryMissionArea_StartsItsArea (đỏ ổn định, chạy 2 lần).
- Triệu chứng: đội 4 người đứng ngoài cổng nhưng ArmedGate=null, nên Hanh_Lang_6 (PlankAndKey, T-006b2) không bắt đầu được qua cổng.
- Bằng chứng (edit mode, scene SchoolFloorPrototype): binding `Hanh lang 1_Vao` -> Hanh_Lang_6 (từ Phong_6), `Hanh lang 1_Ra` -> Hanh_Lang_6 (từ Phong_1); cả hai preparationZone.size=(1,1,1) mặc định, gate.mission=-1. Cổng chuẩn như `Hanh lang 2_*` (-> Hanh_Lang_1) có size (11.9, 3.6, 4.0). Đặt 4 người theo hàng ngang ngoài cửa: 0/4 nằm trong InOutside().
- Nguyên nhân gần: tên object cổng còn theo số cũ (Hanh lang 1 = hành lang 6 sau khi đổi số), vùng chuẩn bị chưa được đặt kích thước/vị trí.
- Cách sửa (chưa làm, cần Scene lock + snapshot + chủ dự án đồng ý vì là hình học scene): đặt preparationZone cho 2 cổng này giống cổng cùng loại (hoặc gán outsideZone), rồi chạy lại test.
- Test KHÔNG bị nới lỏng; nó đúng khi báo lỗi này.

## ĐÃ SỬA 2026-10-29 (phiên #6)
- Hai cổng là instance của PF_PreparationGate chỉ có override vị trí/tên, nên giữ giá trị mặc định 1x1x1 của prefab. Đã đặt cùng thông số như các cổng hành lang chuẩn (Hanh lang 2/4/5): PreparationZone local (0,1.2,4.5) size (11.9,3.6,4); Interaction_Blocker size (12.4,7.8,0.4) center (0,3.9,0); VisualRoot scale (12.4,7.8,0.4), y 3.9. Không đổi trạng thái chạy (blocker/VisualRoot bật-tắt do controller), không đổi mission/areaName.
- Snapshot trước khi sửa: Scenes/BeforeHL6GateZones_20261029_055540.unity. Kiểm: PlayMode 11/11, EditMode 256/256.
- Việc nhỏ còn lại (chưa làm): nhãn cổng vẫn ghi "Hanh lang 1 / Vao|Ra" (số cũ) dù cổng thuộc Hanh_Lang_6; chủ dự án quyết có đổi nhãn không.
