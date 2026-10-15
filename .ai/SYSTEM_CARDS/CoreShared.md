# CARD: CoreShared

Khu / vai trò: Người chơi dùng chung, tương tác, cửa trượt
Quy ước thẻ: phần A tự sinh 2026-10-15 (có thể cũ — nguồn sự thật là code; file/kiểu đầy đủ: MAPS/index-scripts.md). Phần B-D do AI bổ sung: MỖI LẦN sửa một lỗi hoặc làm xong một task phải thêm/sửa dòng ở B hoặc C.

## A. Bản đồ (tự sinh)
- Thư mục: Assets/TheReturn/Core, Assets/TheReturn/Shared | 5 file .cs, 350 dòng, 1 asset ScriptableObject
- Assembly: TheReturn.Core -> [Unity.InputSystem]; TheReturn.Shared -> [Unity.ugui]
- Kiểu (14 đầu): PrototypePartyController, SceneTransitionService, PuzzleDoor, CanvasThemeBinding, SchoolUITheme
- File lớn nhất: E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Core/Runtime/Players/PrototypePartyController.cs (159); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Core/Runtime/SceneFlow/SceneTransitionService.cs (82); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Shared/Runtime/Interaction/PuzzleDoor.cs (54); E:/kho model 3D/Project Unity/The Return/Assets/TheReturn/Shared/Runtime/UI/CanvasThemeBinding.cs (36)
- README: chưa có
- Tests: không có trong thư mục này
- Prefab: PF_GB_Block, PF_GB_DeskChair, PF_GB_SlidingDoor, PF_UI_Button, PF_UI_Panel
- MenuItem builder/migration (ĐỪNG chạy khi chưa được yêu cầu): không có

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem (file > hàm) | Cách kiểm | Liên quan |
|---|---|---|---|
| (chưa có — thêm dòng khi gặp và sửa lỗi đầu tiên) | | | |

## C. Ghi chú đã xác minh
- (chưa có; hãy ghi: điểm vào, luồng dữ liệu, bất biến, phụ thuộc, cách kiểm khi bạn đọc code hệ thống này)

## D. Task/kế hoạch liên quan
- Xem .ai/TASKS.md và .ai/PLANS/ (luồng tự chọn phòng: PLANS/nonlinear-mission-flow.md)
