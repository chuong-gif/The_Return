# Giữ trật tự — nhiệm vụ 2

Đã ghép với lớp 101 trong **cùng một scene**: `Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`. Đây là scene lớp cũ đã đổi tên/vị trí, giữ GUID và đủ 24 bàn ghế. Không tải scene mới khi ra cửa lớp.

## Cách chơi thử

1. Mở scene trên, nhấn Play, chọn đội 2, 3 hoặc 4 người và giải điểm danh.
2. Đổi từng vai bằng phím số, nhấn E để đứng dậy rồi đi qua cửa EXIT. Đưa **tất cả các vai** vào vùng TẬP HỢP ở đầu hành lang. Nhiệm vụ 2 chỉ bắt đầu khi đủ đội.
3. Một vai tới nút A trên tường trái, nhìn vào nút và nhấn E. Vai đó đứng giữ nút; loa tắt, cửa cách âm mở.
4. Đổi vai và đưa người khác qua cửa. Cho một người tới nút B bên kia, nhấn E **khi A vẫn đang được giữ**. HUD xác nhận bàn giao.
5. Đổi về người giữ A, nhấn E để nhả rồi đi qua. Khi đủ đội trong vùng đích sau cửa, nhiệm vụ hoàn thành và cửa giữ mở.

Trong bản thử một máy, E bật/tắt ý định giữ: đổi vai không tự nhả nút và vai đang giữ không thể đi. Đây là cách diễn tập phối hợp; chưa phải multiplayer qua mạng.

| Phím | Chức năng trong hành lang |
| --- | --- |
| WASD / chuột | Đi / nhìn |
| Shift + di chuyển | Chạy, gây tiếng ồn |
| E | Giữ hoặc nhả nút đang tương tác |
| 1–N | Chuyển sang vai trong đội |
| R | Thử lại riêng hành lang, giữ điểm danh |
| Tab / Esc | Tạm dừng hành lang và mở menu |

Menu có nút thử lại hành lang và nút **Chơi lại cả map từ điểm danh**. Đổi số người ở menu lớp trước khi bắt đầu hành lang. Bản hiện tại chưa có xử lý roster online giữa thử thách.

## Luật

- Một trong hai nút đang được giữ: tắt loa và mở cửa.
- Cả hai nhả: chờ 3 giây trước khi loa bật lại và cửa đóng.
- Chạy: +20 ồn/giây/người chạy. Bản đổi vai chỉ có một người di chuyển tại một thời điểm.
- Loa đang rè: +15 ồn/giây. Đi bộ/đứng chỉ giảm 15/giây **khi không có nguồn gây ồn**; tránh việc loa tăng và hồi phục triệt tiêu nhau.
- Chạm 100: cả đội trở về các mốc đầu hành lang, xóa ồn và bàn giao, giữ nhiệm vụ điểm danh đã giải.
- Cửa kiểm tra cả kích thước capsule người chơi; có người trong vùng cửa thì giữ mở, không tính là đã bàn giao.
- Chỉ hoàn thành khi hai người khác nhau đã cùng giữ A/B để tiếp quản và toàn bộ đội đã qua. Chạy lách cửa hoặc đứng chặn cửa không thay thế điều kiện phối hợp.
- Tiếng ồn là dữ liệu game. Không dùng microphone hoặc voice chat. Loa có âm mẫu nhỏ và màu/phụ đề để không bắt buộc nghe âm thanh.
- Mở menu tạm dừng mô phỏng tiếng ồn. Không có sự kiện chuông đổi tiết trong đoạn này.

## Chỉnh sửa trong Unity

Chọn `Assets/TheReturn/Features/QuietCorridor/Data/QuietCorridor_VI.asset` để chỉnh title, hướng dẫn, lời thất bại/thành công, 3 giây ân hạn và các tốc độ tiếng ồn.

Chọn `QuietCorridor_LogicOnly` trong Hierarchy để xem tham chiếu nút A/B, cửa, vùng chống kẹp, vùng đích và bốn checkpoint. Điều chỉnh kích thước các vùng theo bố cục nếu đổi vị trí cửa.

- Nút: `Features/QuietCorridor/Prefabs/PF_QuietHoldStation.prefab`; thay child `Visual_Replaceable` để đổi model.
- Sàn/tường/cửa dùng các prefab trong `Shared/Prefabs/Graybox`.
- Loa: component `QuietSpeakerFeedback` trên Speaker; có thể gán Replacement Clip và chỉnh Volume.
- Luật thuần C# ở `Runtime/Domain`; cấu hình ở `Runtime/Data`; tương tác/visual ở `Runtime/Presentation`.
- `Maps/SchoolFloor/Runtime` nối hai tính năng và trình bày HUD. Attendance và QuietCorridor không phụ thuộc lẫn nhau.
- Các file mới có chú thích tiếng Việt đầu file và trước từng hàm.
- Menu Add Quiet Corridor chỉ dùng trên bản lớp chưa có hành lang. Không chạy lại để sửa text/model.
- Menu Rebuild Logic Classroom chỉ tạo bản thử riêng của tính năng điểm danh; không dùng để dựng lại map tích hợp này.

## Kiểm chứng

103 kiểm tra đạt cho đội 2/3/4 người: biên thời gian ân hạn, tốc độ ồn, quyền giữ nút, chuyển từ điểm danh, chờ người cuối, bàn giao, hoàn thành, thử lại, mất người giữ, cửa chống kẹp, checkpoint và đi qua cửa bằng CharacterController.

Báo cáo: `Design/Verification/QuietCorridor.json`. Trong Play Mode, công cụ kiểm tra gọi `TheReturn.Editor.QuietCorridorChecks.Run()`; nó thay đổi trạng thái lượt đang thử.

Đã thử input E, đổi vai vẫn giữ nút và Shift chạy bằng thiết bị input mô phỏng. Kết quả tại `Design/Verification/QuietCorridorInput.json`. Đây không thay thế playtest 2–4 người qua mạng.

Ảnh: `Captures/school-floor-two-puzzles-overview.png` và `Captures/quiet-corridor-hud.png`.

Công cụ chụp HUD của Unity MCP từng phát cảnh báo PlayerLoop gọi lặp, stack trace nằm trong ScreenshotUtility của package. Kiểm tra gameplay và console thông thường được thực hiện riêng với thao tác chụp ảnh.

Bản sao trước thay đổi: `Backups/BeforeQuietCorridor_20260914`, nằm ngoài Assets.

Kiểm tra cuối sau khi tách assembly và chuyển scene: 103 kiểm tra vẫn đạt, không có missing script; Console không có lỗi/cảnh báo trong lượt Play kiểm tra không chụp ảnh.

Sau khi Giữ trật tự hoàn thành, cửa phòng giáo viên mở và chuyển sang nhiệm vụ 3 trong cùng scene. Xem Sua-bang-diem.md. Khi đã chuyển, menu R thuộc nhiệm vụ 3, không reset hành lang.
