# CARD: SharedLantern

Khu / vai trò: Hanh_Lang_4 — «Đèn lồng chung» (T-006f). Cập nhật: 2026-10-17 02:55 UTC. Kế hoạch: PLANS/T-006f-shared-lantern.md; thiết kế đã duyệt: PLANS/T-006-mission-designs.md (mục Hanh_Lang_4).

## A. Bản đồ
- Thư mục: Assets/TheReturn/Features/SharedLantern | Domain 3 file (LanternRound, LanternGenerator, LanternState) + Tests/EditMode (SharedLanternTests, 37 test) | Data: SharedLanternSettings + SharedLantern_VI.asset | Presentation: SharedLanternPrototype, LanternSign, LanternPanelKey, LanternSymbols | adapter: Maps/SchoolFloor/Runtime/Missions/SharedLanternMissionAdapter.cs | Scene: có (xem mục C).
- Assembly: TheReturn.SharedLantern.Domain (noEngineReferences) và TheReturn.SharedLantern.Domain.Tests.
- Namespace TheReturn; kết quả enum: LanternReadResult, LanternPassResult, LanternSubmitResult.

## B. Triệu chứng -> nơi xem (từ bản đồ lỗi của kế hoạch; chưa có lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Đọc biển bị từ chối | LanternState.Read | Đọc kết quả; vùng sáng do Presentation đo |
| Bị đưa về đầu sai giờ | LanternState.Tick (hơn OutsideLimit=5.0s; người cầm đèn miễn) | Dãy thời gian cố định như test Tick_* |
| Trao đèn thất bại | LanternState.TryPass (tầm 2 m) | Đơn vị khoảng cách; NaN là TooFar |
| Nhập đúng mà chưa xong | Submit -> CodeAccepted; TryComplete(allAtGate) | |

## C. Hiện trạng đã xác minh
- Luật Domain xong và test 37/37 xanh (mức 3); nhập sai bị phạt (xóa 4 ô + khóa 10s). 4 biển, 6 ký hiệu, ký hiệu khác nhau theo seed; sổ riêng từng người (`SymbolSeen` trả -1 nếu người đó chưa tự đọc); hợp sổ `TeamReadMask`; đèn ban đầu ở người 0; `Tick(now, inLight[])` trả mặt nạ bit người bị đưa về; `Submit` kiểm 4 ô theo thứ tự biển; `Reset` giữ đề.
- Không phụ thuộc Unity: thời gian, vùng sáng và khoảng cách do Presentation truyền vào.
- Scene SchoolFloorPrototype (2026-10-18): object gốc `SharedLantern_LogicOnly` tại Hanh_Lang_4 (yaw 30°, z+ từ Phong_4 sang Phong_5; gồm Signs/Sign_1-4 có chữ BIỂN n, Panel (mép phải, z=20.25) + Panel_Start (mép trái, z=-20.25) mỗi bảng có 6 phím A-F + XÓA + XÁC NHẬN + 4 ô + bảng trạng thái, Checkpoints (z=-20.5) + CheckpointsFromEnd (z=+20.5), GoalZone (z=23.2) + GoalZoneFromEnd (z=-23.2), Lantern + Light). Nhóm cổng `Hanh_Lang_5_Preparation` (`Hanh lang 5_Vao` entry Phong_4, `Hanh lang 5_Ra` entry Phong_5; tên theo số cũ). `flow.sharedLantern` đã gán; registry có Hanh_Lang_4. Vào từ cổng Vao hay Ra đều được: Begin xem đội ở nửa z+ hay z- để chọn checkpoint/vùng cổng; cả hai bảng đều nhập được.
- Chỉ số đã kiểm thật (mức 4 mô phỏng, chưa người chơi thật): xem PLANS/T-006f mục S3. Hạn chế đã biết: biển ở x=±2.62 nên phải đứng gần từng bên; bán kính sáng/giờ phạt chỉnh ở SharedLantern_VI.asset.
- Điều khiển: E đọc biển/bấm phím bảng, F trao đèn cho người gần nhất (≤ 2 m), R làm lại, 1-4 đổi vai, Tab/Esc thả chuột. Vùng sáng 4 m quanh người cầm đèn.

## D. Task/kế hoạch liên quan
- T-006f (S1 xong; S2-S4 TODO). Quyết định cần chủ dự án: PLANS/T-006f-shared-lantern.md mục 7.

- (2026-10-18 16:39 UTC) Kiểm chứng Play Mode mô phỏng (phiên #4): vào từ cả hai đầu hành lang, đọc 4 biển, nhập sai khóa, đúng + đủ đội ở cổng cuối thì hoàn thành. Biển ở x=±2.6 trong hệ SharedLantern_LogicOnly (mép lối giữa 1,75): phải đứng sát từng bên mới đọc được. Chưa kiểm nhánh 5 giây ngoài vùng sáng và người chơi thật.
