# CARD: GradeRepair

Khu / vai trò: Phong_2 (mission cũ 2)
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/GradeRepair | 5 file .cs, 457 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.GradeRepair -> [TheReturn.GradeRepair.Domain,TheReturn.Core,TheReturn.Shared,Unity.InputSystem]; TheReturn.GradeRepair.Domain -> []
- Kiểu (14 đầu): GradeRepairCatalog, GradeRepairRound, GradeResult, GradeRepairState, GradeRepairPrototype, GradeRepairTerminal
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/GradeRepair/Runtime/Presentation/GradeRepairPrototype.cs (190); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/GradeRepair/Runtime/Domain/GradeRepairState.cs (128); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/GradeRepair/Runtime/Data/GradeRepairCatalog.cs (71); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/GradeRepair/Runtime/Domain/GradeRepairRound.cs (51)
- README: Assets/TheReturn/Features/GradeRepair/README.md
- Tests: không có trong thư mục này
- Prefab: PF_GradeTerminal
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem (suy từ code đọc 2026-10-15; chưa tái hiện lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| Nộp bảng bị từ chối | Domain/GradeRepairState.cs > Submit (MissingEvidence: chưa đọc đủ 4 tài liệu; MissingVouchers: chưa dùng 3 phiếu; WrongRecord; Requirements: mỗi điểm 6-10 và tổng 32) | Đọc `GradeResult` trả về, `State.Read(i)`, `State.Used(i)`, `Score(i)` | Design/Sua-bang-diem.md |
| Không đọc được tài liệu | Presentation/GradeRepairPrototype.cs > CanReach, TryOpen; Domain/GradeRepairRound.cs > OwnerOf (doc % số người) | Chỉ đúng vai phụ trách mới đọc được; kiểm vai đang chơi và khoảng cách | README |
| Áp phiếu bị từ chối | GradeRepairState.Apply (AlreadyUsed/OutOfRange/Invalid) | Kiểm giới hạn điểm của hồ sơ nguồn/đích | |
| Hoàn tác sai điểm | GradeRepairState.Undo/Reset (Reset KHÔNG xóa trạng thái đã đọc) | | |
| Bảng/tài liệu không bấm được | GradeRepairTerminal (document=-1 là bảng, 0-3 là tài liệu), PF_GradeTerminal.prefab (VisualRoot, collider ở parent) | Mảng `documents[4]` đúng thứ tự | README (prefab) |

## C. Hiện trạng đã xác minh (2026-10-15)
- Scene: `GradeRepair_LogicOnly` (GradeRepairPrototype), Phong_2; `board`, `documents[4]`, `exitDoor` cũ ẩn, `checkpoints[4]`, `initialSeed`=0. Nội thất nhiệm vụ: cụm tra cứu tại root `Archives_SpatialPass01`.
- Luật: 4 hồ sơ có mã `HS-<tiền tố><i>`, điểm gốc {4,7,8,9} xáo theo seed; 4 tài liệu (sổ mã, hai phiếu bổ sung +2, biên bản chuyển 1 điểm), mỗi tài liệu chỉ vai phụ trách đọc được; 3 phiếu áp lên hồ sơ trong giới hạn điểm; Undo hoàn trả cả điểm và quyền phiếu; Submit khóa kết quả khi đủ chứng từ, đúng hồ sơ, điểm 6-10, tổng 32.
- Bằng chứng: GradeRepair.json 2026-09-14 (4257 assertion, 90 seed, đội 2/3/4: đọc riêng, cửa, retry), GradeRepairInput.json 2026-09-14, CanvasUI.json 2026-09-17 (giải qua Canvas). `PF_GradeTerminal.prefab` đã sửa gần đây (git status 2026-10-14).
- Chưa có: random vị trí tài liệu/sổ tay (README ghi rõ chưa triển khai); chưa kiểm sau Layout V2. Spec: `Design/Sua-bang-diem.md`.
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
