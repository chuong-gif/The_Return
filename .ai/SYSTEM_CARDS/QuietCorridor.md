# CARD: QuietCorridor

Khu / vai trò: Hanh_Lang_1 (mission cũ 1)
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/QuietCorridor | 5 file .cs, 348 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.QuietCorridor -> [TheReturn.Core,TheReturn.Shared,Unity.InputSystem,TheReturn.QuietCorridor.Domain]; TheReturn.QuietCorridor.Domain -> []
- Kiểu (14 đầu): QuietCorridorSettings, QuietCorridorState, QuietCorridorPrototype, QuietHoldStation, QuietSpeakerFeedback
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/QuietCorridor/Runtime/Presentation/QuietCorridorPrototype.cs (167); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/QuietCorridor/Runtime/Domain/QuietCorridorState.cs (85); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/QuietCorridor/Runtime/Presentation/QuietSpeakerFeedback.cs (46); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/QuietCorridor/Runtime/Presentation/QuietHoldStation.cs (27)
- README: Assets/TheReturn/Features/QuietCorridor/README.md
- Tests: không có trong thư mục này
- Prefab: PF_QuietHoldStation, PF_QuietSpeaker
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem (hàng 1 từ lỗi thật; còn lại suy từ code, chưa tái hiện)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| QuietCorridorChecks.Run() fail ở 'Scene 2: same scene transition' | Maps/SchoolFloor/Tests/Editor/QuietCorridorChecks.cs > Integrated; điều kiện chuyển ở SchoolFloorFlow.Update | Test đặt người ở `corridor.checkpoints` (ngoài PreparationZone) | BUGS/quiet-corridor-check-stale-after-layout-v2.md |
| Ồn tăng/giảm sai | Domain/QuietCorridorState.cs > Tick (ân hạn 3s, chạy +20/s mỗi người, loa +15/s, im lặng hồi 15/s, thua ở 100) | Đọc `Noise`, `Grace`, `SpeakerOn`; thông số ở Data/QuietCorridorSettings + QuietCorridor_VI.asset | |
| Cổng âm thanh không mở/kẹp người | Presentation/QuietCorridorPrototype.cs > GateOccupied, `gate` (AcousticGate), `safetyZone` (Gate_AntiCrush); State.GateRequested | PuzzleDoor + PF_GB_SlidingDoor | |
| Cả đội qua rồi mà không hoàn thành | State.TryComplete(allAcross): cần HandedOver (hai người khác nhau giữ hai nút cùng lúc) và AllAcross trên `goalZone` (hộp local) | Kiểm `HandedOver`, vị trí đội so với goalZone | |
| Loa/phản hồi âm thanh | QuietSpeakerFeedback + PF_QuietSpeaker (gán `session` khi tạo instance mới) | | README |

## C. Hiện trạng đã xác minh (2026-10-15)
- Scene: `QuietCorridor_LogicOnly` (QuietCorridorPrototype), Hanh_Lang_1 (cổng gắn nhãn cũ «Hanh lang 2»); `stations[2]`, `gate`=AcousticGate, `safetyZone`=Gate_AntiCrush, `goalZone`=Team_Goal, `checkpoints[4]`, `speakerIndicator`. PreparationZone của nó = `transitionGatherZones[0]`, tâm ≈ (-100.2, 12.1, 100.4).
- Luật: hai nút giữ; loa bật khi không ai giữ và hết ân hạn; một người giữ nút A để tắt loa và mở cổng, người qua trước giữ nút B khi A còn giữ (bàn giao), rồi người ở A nhả và đi qua; cả đội tới vùng đích để xong; ồn đạt 100 thì cả đội về đầu hành lang (Điểm danh vẫn giữ).
- Bằng chứng: QuietCorridor.json 2026-09-14 (103 assertion), QuietCorridorInput.json 2026-09-14. Test tích hợp hiện FAIL do lỗi thời (xem BUGS). Spec: `Design/Giu-trat-tu.md`.
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
