# CARD: SharedLantern

Khu / vai trò: Hanh_Lang_4 — «Đèn lồng chung» (T-006f). Cập nhật: 2026-10-17 02:55 UTC. Kế hoạch: PLANS/T-006f-shared-lantern.md; thiết kế đã duyệt: PLANS/T-006-mission-designs.md (mục Hanh_Lang_4).

## A. Bản đồ
- Thư mục: Assets/TheReturn/Features/SharedLantern | Domain 3 file (LanternRound, LanternGenerator, LanternState) + Tests/EditMode (SharedLanternTests, 37 test) | Data: SharedLanternSettings + SharedLantern_VI.asset | Presentation: SharedLanternPrototype, LanternSign, LanternPanelKey, LanternSymbols | adapter: Maps/SchoolFloor/Runtime/Missions/SharedLanternMissionAdapter.cs | CHƯA có object trong scene.
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
- Presentation + adapter + đăng ký factory (khi `SchoolFloorFlow.sharedLantern` != null) đã có và compile sạch. CHƯA có trong scene: object SharedLantern_LogicOnly, cổng + khu chuẩn bị của Hanh_Lang_4, gán flow.sharedLantern. Vì vậy MapProgress vẫn coi Hanh_Lang_4 là khu chưa có nhiệm vụ cho tới khi dựng cảnh (T-006f S3).
- Điều khiển: E đọc biển/bấm phím bảng, F trao đèn cho người gần nhất (≤ 2 m), R làm lại, 1-4 đổi vai, Tab/Esc thả chuột. Vùng sáng 4 m quanh người cầm đèn.

## D. Task/kế hoạch liên quan
- T-006f (S1 xong; S2-S4 TODO). Quyết định cần chủ dự án: PLANS/T-006f-shared-lantern.md mục 7.

- (2026-10-18 16:39 UTC) Kiểm chứng Play Mode mô phỏng (phiên #4): vào từ cả hai đầu hành lang, đọc 4 biển, nhập sai khóa, đúng + đủ đội ở cổng cuối thì hoàn thành. Biển ở x=±2.6 trong hệ SharedLantern_LogicOnly (mép lối giữa 1,75): phải đứng sát từng bên mới đọc được. Chưa kiểm nhánh 5 giây ngoài vùng sáng và người chơi thật.
