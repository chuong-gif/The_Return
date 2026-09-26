# UI Canvas và test nhanh

## Chạy thử nhiệm vụ
Mở scene Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity rồi nhấn Play.

- Nhấn F8 để mở bộ chọn nhiệm vụ.
- Chọn đội 2, 3 hoặc 4 người và chọn Điểm danh, Giữ trật tự, Sửa bảng điểm Bài kiểm tra hoặc Không được quay đầu.
- Lựa chọn tạo phiên test mới, chuẩn bị kết quả và cửa của nhiệm vụ trước, đưa đội tới khu vực cần thử. Tiến trình phiên hiện tại sẽ được đặt lại.
- Nhãn TEST trên HUD giúp phân biệt phiên thử. Đây vẫn là điều khiển đổi vai trên một máy, chưa phải chơi mạng.

Để tự vào nhiệm vụ mỗi lần nhấn Play: chọn SchoolFloor_Flow trong Hierarchy, tìm MissionTestLauncher, đặt Start Mission và Test Party Size. Chọn Normal Play để chơi từ đầu. Mặc định đã trả về Normal Play, đội 4 người.

Công cụ chọn nhiệm vụ chỉ hoạt động trong Unity Editor hoặc Development Build.

## Chỉnh giao diện trong Unity
Prefab chính: Assets/TheReturn/Maps/SchoolFloor/UI/Prefabs/PF_SchoolFloorCanvas.prefab.

Mở prefab để sửa giao diện lâu dài. Trong SafeFrame có các nhóm HUD, PrivateClues, NoiseMeter, PauseMenu, MissionTestMenu, GradeRepairPanel, DocumentReader và NavigationExamPanel, DontLookBackPanel và CheckpointBlackout. Mỗi nhóm dùng RectTransform, Image, Text, Button hoặc ScrollRect thật của Unity; có thể thay ảnh, font, kích thước và vị trí trong Inspector. Nội dung dài có vùng cuộn.

Canvas dùng Screen Space Overlay, CanvasScaler tham chiếu 1280 × 720. Bố cục đã kiểm tra ở tỷ lệ 16:9; khi thiết kế cho tỷ lệ khác cần xem lại trong Game view.

Các tài sản dùng chung:
- Assets/TheReturn/Shared/UI/Prefabs/PF_UI_Panel.prefab
- Assets/TheReturn/Shared/UI/Prefabs/PF_UI_Button.prefab
- Assets/TheReturn/Shared/UI/Themes/SchoolUITheme.asset

Theme quản lý font, màu, ảnh nền panel và ảnh nút. CanvasThemeBinding áp theme khi đối tượng bật; menu Apply Theme trên component áp lại ngay. Nếu muốn một thành phần có phong cách riêng, bỏ tham chiếu Theme của binding đó trước khi chỉnh. Theme để trống sprite sẽ giữ ảnh riêng của Image.

Với ảnh 2D của bạn, nhập vào Unity dạng Sprite (2D and UI), rồi gán vào panelSprite/buttonSprite của theme hoặc Source Image của Image riêng. Ảnh nền kéo giãn nên có border phù hợp. Tâm ngắm chỉnh trực tiếp ở Image Crosshair; trường crosshairSprite trong theme hiện chưa được nối.

Không chạy lại công cụ dựng Canvas để sửa bố cục đã có; hãy sửa prefab. Các tham chiếu tới flow và bộ test được gán trên instance trong scene.

## Tổ chức code
- Shared/Runtime/UI: cấu hình theme và thành phần áp style dùng lại.
- Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasView: các tham chiếu thành phần UI.
- SchoolFloorCanvasPresenter và phần Puzzles: cập nhật nội dung, nối nút với controller gameplay; không dựng lại bố cục mỗi frame.
- Maps/SchoolFloor/Runtime/Testing/MissionTestLauncher: tạo trạng thái test và chuẩn bị các nhiệm vụ trước.
- Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder: công cụ dựng ban đầu.
- Nội dung câu đố vẫn được quản lý trong catalog của từng feature.

Các HUD OnGUI cũ được giữ làm phương án tương thích; scene tích hợp đã bật useCanvas nên không vẽ giao diện cũ chồng lên Canvas.

## Kiểm chứng
Đã kiểm tra 12 tổ hợp nhiệm vụ/số người (4 × 3), 24 điều kiện khởi tạo, 20 thao tác nút qua raycast/EventSystem, giải bảng điểm và bài kiểm tra bằng Canvas, cùng khởi động tự động bài kiểm tra với 3 người. Kết quả lưu trong Design/Verification/CanvasUI.json và CanvasAutoStart.json.

Đây là kiểm tra chức năng UI và phiên thử cục bộ, chưa xác nhận đồng bộ mạng. Ảnh kiểm tra nằm trong Captures/canvas-mission-selector.png và Captures/canvas-exam-panel.png.

Nhiệm vụ 5 và 6 đã có trong bộ chọn F8; xem Khong-duoc-quay-dau.md, Giao-vien-that-doi.md và các báo cáo tương ứng trong Design/Verification.
