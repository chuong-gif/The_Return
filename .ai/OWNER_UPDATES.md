# OWNER UPDATES — chỉ các chỉ dẫn còn áp dụng

Các cập nhật cũ đã được áp dụng vào `AGENTS.md`, `DECISIONS/` hoặc plan liên quan và không giữ nhật ký lặp tại đây. Khi chủ dự án có thay đổi mới, ghi ngày + thay đổi + tác động cần kiểm tra.

### 2026-11-01 | Phép sửa map/network
- Đã cho phép sửa scene map cho công việc nối nhiệm vụ/luồng map mạng; snapshot trước khi sửa vẫn bắt buộc.
- Quyền đó không bao gồm tự ý thêm/bớt/đổi thứ tự Build Settings. P-013 sau đó chỉ cho phép scene đích offline MapSchoolF; online room→map đang chờ P-014.

### 2026-11-12 | Map hiện hành
- MapSchoolF do chủ dự án dựng là scene đích; thay thế scene blockout SchoolFloorCore4 ban đầu. Chỉ sửa trong phạm vi T-040/T-042 và theo snapshot/preflight.

### 2026-11-13 | Mục tiêu build thử online
- Chủ dự án muốn xây một loạt tính năng rồi build MỘT bản để thử với người thật/máy thật (có mic); ưu tiên tốc độ sản xuất; chia phần chưa sẵn sàng thành task cho AI khác nhận. AI chạy NỐI TIẾP (hết limit mới đổi AI), không chạy song song; giao T-047 trong chat.
- Tác động: `PLANS/T-046-051-online-playtest.md`, TASKS T-046–T-051, QUEUE Q-025. Đây CHƯA phải trả lời P-014 và chưa cấp quyền sửa scene/prefab/Build Settings: chờ chủ dự án duyệt plan.

### 2026-11-13 | Duyệt plan T-046–T-051 + trả lời P-014
- Chủ dự án xác nhận trong chat: ĐỒNG Ý duyệt plan `PLANS/T-046-051-online-playtest.md` và P-014 = ĐỒNG Ý (nối room→MapSchoolF, thêm scene phòng online vào Build Settings). Yêu cầu: ưu tiên làm một lượt rồi kiểm sau.
- Tác động: T-046 mở khóa (được sửa MainMenu THÊM nút, tạo scene phòng online mới từ NetworkRoomTest, `RoomMapHandoff`, Build Settings chỉ THÊM scene phòng). Vẫn snapshot trước khi sửa scene; không đụng Sanh_Cho/MapSchoolF/logic nhiệm vụ. T-048/T-049/T-051 theo plan, làm khi được giao.

Khi có OWNER UPDATE mới: kiểm tra hiện trạng scene/code, cập nhật `.ai` liên quan và ghi kết quả ngắn vào `LOG.md`; không suy diễn thêm quyền sửa.
