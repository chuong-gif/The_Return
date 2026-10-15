# CARD: DontLookBack

Khu / vai trò: Hanh_Lang_3 (mission cũ 4; cổng gắn nhãn cũ «Hanh lang 4»)
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/DontLookBack | 3 file .cs, 332 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.DontLookBack -> [TheReturn.DontLookBack.Domain,TheReturn.Core,TheReturn.Shared,Unity.InputSystem]; TheReturn.DontLookBack.Domain -> []
- Kiểu (14 đầu): DontLookBackCatalog, BackLockResult, DontLookBackState, DontLookBackPrototype
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/DontLookBack/Runtime/Presentation/DontLookBackPrototype.cs (205); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/DontLookBack/Runtime/Domain/DontLookBackState.cs (83); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/DontLookBack/Runtime/Data/DontLookBackCatalog.cs (44)
- README: Assets/TheReturn/Features/DontLookBack/README.md
- Tests: không có trong thư mục này
- Prefab: PF_BackCard, PF_BackLockTerminal
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem (suy từ code đọc 2026-10-15; chưa tái hiện lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| Bị phạt oan / không bị phạt khi quay đầu | Domain/DontLookBackState.cs > SampleLook (góc ngang > 110 độ liên tục 0,6s); Presentation/DontLookBackPrototype.cs > Angle (hướng cấm = forward của root nhiệm vụ) | Kiểm hướng forward của root sau khi xoay hành lang | README |
| Khóa không mở | State.Submit (NeedParty: chưa đủ đội ở lối ra; NeedWitnesses: chưa ai thấy lưng của ai đó; WrongCode) | Đọc `BackLockResult`, `WitnessedCount`, `Entry(i)` | Design/Khong-duoc-quay-dau.md |
| Thẻ lưng sai/lệch người | RefreshBadges, `badges[4]`, `backLabels[4]`, State.SymbolOf | Đảm bảo thẻ nằm sau lưng đúng hướng (README) | PF_BackCard |
| Quay lại checkpoint sai | Prototype > ReturnToCheckpoint; `checkpoints[4]` và `laterCheckpoints[4]`; zone entry/checkpoint/exit | | |

## C. Hiện trạng đã xác minh (2026-10-15)
- Scene: `DontLookBack_LogicOnly` (DontLookBackPrototype), Hanh_Lang_3 (cổng gắn nhãn cũ «Hanh lang 4»); `checkpoints[4]`, `laterCheckpoints[4]`, `terminals[2]`, `badges[4]`, `backLabels[4]`, ba trigger `entryZone`/`checkpointZone`/`exitZone`, `exitDoor` cũ ẩn.
- Luật: mỗi người có ký hiệu bí mật (6 ký hiệu, xáo theo seed) hiển thị trên LƯNG; người chơi quan sát lưng người khác (không tự xem lưng mình), ghi chép riêng, rồi nhập mã ký hiệu cho từng người ở bảng khóa; nhìn quay lại quá 110 độ liên tục 0,6 giây = phạt (Penalties, đưa về checkpoint, có Blackout/Warning); Submit cần cả đội ở lối ra, mọi người đều đã được ai đó nhìn thấy lưng, và mã đúng.
- Bằng chứng: DontLookBack.json 2026-09-26 (8593 assertion, 300 ca seed/đội), Frames 09-19 (phạt runtime, giữ ghi chép), Transition 09-19, UI 09-19.
- Prefab: PF_BackCard (thẻ gắn vào nhân vật), PF_BackLockTerminal. Chưa kiểm sau Layout V2.
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
