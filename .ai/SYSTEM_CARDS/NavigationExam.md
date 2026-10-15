# CARD: NavigationExam

Khu / vai trò: Phong_3 (mission cũ 3)
Cập nhật B/C: 2026-10-15 06:33 UTC (T-014). Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Features/NavigationExam | 3 file .cs, 295 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.NavigationExam -> [TheReturn.NavigationExam.Domain,TheReturn.Core,TheReturn.Shared,Unity.InputSystem]; TheReturn.NavigationExam.Domain -> []
- Kiểu (14 đầu): NavigationExamCatalog, ExamResult, NavigationExamState, NavigationExamPrototype
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/NavigationExam/Runtime/Presentation/NavigationExamPrototype.cs (151); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/NavigationExam/Runtime/Domain/NavigationExamState.cs (86); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Features/NavigationExam/Runtime/Data/NavigationExamCatalog.cs (58)
- README: Assets/TheReturn/Features/NavigationExam/README.md
- Tests: không có trong thư mục này
- Prefab: PF_NavigationBoard
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem (suy từ code đọc 2026-10-15; chưa tái hiện lỗi thật)
| Triệu chứng | Nơi xem | Cách kiểm | Liên quan |
|---|---|---|---|
| Bước đi bị từ chối | Domain/NavigationExamState.cs > Move (WrongTurn/Blocked/OneWay/StampOrder/MissingStamps/Outside) | Đọc `ExamResult`, `Position`, `Turn`, `Stamps` | Design/Bai-kiem-tra.md |
| Sa bàn lệch/ô sai so với đề | NavigationExamState.Cell (biến thể = seed%4, lật ngang/dọc) và `cells[9]` trong prototype | Đối chiếu thứ tự 9 ô với mẫu | |
| Không mở được bảng/bước | NavigationExamPrototype > CanReach, TryOpen, Step | Vị trí vai, `PanelOpen`, `Targeted` | README: giữ board/pawn/cells/boardLabel |
| Thay model làm hỏng raycast | PF_NavigationBoard.prefab (chưa chuẩn hóa VisualRoot; collider phải chạm đúng vùng) | | README (prefab) |

## C. Hiện trạng đã xác minh (2026-10-15)
- Scene: `NavigationExam_LogicOnly` (NavigationExamPrototype), Phong_3; `board`, `pawn`, `cells[9]`, `checkpoints[4]`, `boardLabel`, `exitDoor` cũ ẩn.
- Luật: sa bàn 3x3 (A1-C3), 4 biến thể đối xứng theo seed; xuất phát ô mẫu 0, đích ô mẫu 8, hai con dấu ở ô mẫu 4 rồi 5 (phải lấy theo thứ tự, không được đi ngược từ dấu 2 về dấu 1, chưa đủ dấu thì không vào đích); ô cấm ở ô mẫu 2, 3, 7; các vai thay phiên nhau đi từng ô; 4 manh mối chia theo vai.
- Bằng chứng: NavigationExam.json 2026-09-26 (561 assertion, 12 cấu hình, mọi trạng thái đạt được đều hoàn thành được), NavigationExamInput.json 2026-09-17, CanvasUI.json 2026-09-17.
- Chưa kiểm sau Layout V2. Spec chủ dự án (suy theo tên README «Bài kiểm tra»): `Design/Bai-kiem-tra.md` [?].
- Điều khiển thử MỘT MÁY (chưa có mạng): E tương tác, 1-4 đổi vai, WASD đi, Shift chạy, R thử lại, Tab menu (`PrototypePartyController`, `MissionTestLauncher`/Canvas chọn nhiệm vụ).
- Giao tiếp với `SchoolFloorFlow`: `Begin()`, `Active`, `State.Solved`, `StopSession(bool)`; thua/thử lại = `Retry()` đưa đội về `checkpoints`.
- Chưa chạy lại bất kỳ bộ kiểm tra nào sau Layout V2 (trừ kiểm tra cửa/chuyển tiếp 2026-09-29 theo PROJECT_STATE). Bằng chứng nhiệm vụ bên dưới có TRƯỚC Layout V2.
- Số hành lang trong README/PROJECT_STATE/Design đã sửa theo tên object mới (2026-10-15); chỉ nhãn trên cổng trong scene (areaName «Hanh lang 2/4») còn số cũ.

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
