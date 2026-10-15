# Bố cục theo Map The Reture

> LỊCH SỬ — phương án bên dưới đã bị người dùng bác bỏ ngày 2026-09-29 vì đặt phòng cũ vào trong blockout. Không dùng làm thiết kế đích hoặc tự chạy lại builder. Đọc PROJECT_STATE.md và Design/Ke-hoach-tai-bo-tri-v2.md trước khi tiếp tục. Scene hiện chưa được chuyển sang phương án mới.

Map The Reture (có khoảng trắng cuối tên trong scene) là bản thiết kế gốc của người dùng. Root này được giữ nguyên hình học, ẩn để không chồng lên bản dùng trong game. `BlockoutCampus_Generated/Architecture_From_User_Blockout` là bản có collider và xử lý ngưỡng sàn.

Phong_1–Phong_6 lần lượt chứa điểm danh, giữ trật tự, sửa bảng điểm, bài kiểm tra, không quay đầu, giáo viên nói thật/nói dối. Giữ nguyên kích thước nội thất/câu đố; mỗi vùng chơi nằm trong khoang phòng lớn của blockout. Cửa vào và ra nối qua khu chuẩn bị với hành lang vòng. Hanh_Lang_2 nối phòng 1–2, tiếp tục tới Hanh_Lang_6 nối 5–6; Hanh_Lang_1 khép vòng 6–1.

Khu chuẩn bị ở đầu vào phòng 2–6 chứa vùng tập hợp cho bước chuyển nhiệm vụ. Cả đội phải hoàn thành nhiệm vụ hiện tại và vào vùng này. Kiểm tra hộp theo tọa độ local, không dùng bounds bao trục world cho các phòng xoay. Các điểm checkpoint đi cùng root nhiệm vụ. Lớp đầu có cửa sau mới; cửa ra phòng giáo viên được chỉnh hai vách để có ô thông 2,4 m.

Sàn phòng bản dùng trong game đặt về cốt 11,14 m để khớp chân cửa (bản gốc phòng 2–6 có sàn cao hơn). Sàn ngưỡng nối liên tục theo đường có sẵn. Sáu cầu thang giữ mesh gốc, thêm mặt dốc va chạm ẩn để CharacterController đi lên/xuống được. Cầu nối tự sinh cũ đã ẩn.

Scene dự phòng trước khi bố trí: `Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorBeforeUserBlockout.unity`. Scene chơi hiện tại vẫn là SchoolFloorPrototype. Không chạy lại các công cụ Monumental Atrium, Connected Walkways hoặc Build Classroom And Gallery vì chúng dựng bố cục cũ. Dựng lại bố cục này bằng The Return > School Floor > Apply User Blockout Layout sẽ thay root generated và sắp lại vị trí phòng; các chỉnh sửa thủ công dưới root generated sẽ bị thay thế.

Kiểm tra sau khi ghép: 1.858 điểm tuyến có sàn, không gặp vật cản ngoài cửa nhiệm vụ; các điểm xuất phát có sàn; CharacterController đi cả hai chiều trên sáu dốc cầu thang (12 lượt); 30 kiểm tra vùng tập hợp xoay với đội 2–4 người đạt. Play Mode có 9 kiểm tra vị trí/hướng khi teleport, reset và góc trước/sau của nhiệm vụ không quay đầu đạt; Console không có lỗi. Đây không thay thế việc chơi lại toàn bộ câu đố và thử nhiều máy qua mạng.

Teleport theo mốc Transform giờ giữ hướng ngang của checkpoint/ghế. ResetPositions khôi phục cả vị trí lẫn hướng xuất phát. Góc cấm quay đầu đo theo trục forward của root nhiệm vụ thay vì trục Z cố định của thế giới.
