# Điểm danh thích ứng 2 đến 4 người

## Kết quả của bản v2

Câu điểm danh dùng 24 chỗ ngồi trong lưới 6 hàng × 4 cột. Số người tham gia được lấy từ roster của phiên, không kiểm tra cố định bốn ghế. Bản hiện tại mô phỏng roster bằng nút 2, 3, 4 người; chưa có kết nối mạng.

Tất cả 24 ghế đều tương tác được. Người chơi đọc địa chỉ H1-C1 đến H6-C4: H1 gần bảng nhất, H6 xa bảng nhất; C1 ở trái và C4 ở phải khi nhìn lên bảng. Có thể dùng bất kỳ ghế nào, nhưng chỉ bộ vị trí đúng với manh mối mới hoàn thành.

## Sinh đề và khả năng chơi lại

Mỗi đề có seed, số người, tên các vai, bộ điều kiện và người giữ từng điều kiện. Bộ sinh chọn vị trí đích khác nhau cho mỗi người trong 24 ghế, tạo các luật đúng với đáp án rồi bỏ các luật không cần thiết.

Trước khi sử dụng, đề phải có đúng một nghiệm. Mỗi vai giữ ít nhất một manh mối thiết yếu. Trong kiểm thử, bỏ toàn bộ gói manh mối của bất kỳ vai nào phải khiến đề có nhiều hơn một lời giải.

Có 10 loại điều kiện: hàng cụ thể, cột cụ thể, trước, sau, trái, phải, cùng hàng, cùng cột, hàng chẵn/lẻ, cột chẵn/lẻ. Trước/sau chỉ quan hệ hàng; trái/phải chỉ quan hệ cột. Những câu này không ngầm yêu cầu cùng cột hoặc cùng hàng nếu câu không ghi rõ.

Ví dụ: “Minh ở trước Bình 2 hàng” nghĩa là số hàng của Minh nhỏ hơn Bình 2 đơn vị; hai người có thể khác cột. Điều này được ghi rõ trong hướng dẫn để không tạo cách hiểu mơ hồ.

Độ khó thay đổi theo số người: đội 2 người nhận luật liên hệ giữa 2 vai; đội 4 người nhận nhiều liên hệ hơn. Số ghế vẫn là 24 ở mọi chế độ. Đây là tăng giảm dữ liệu đề, không chỉ thay con số trên HUD.

- F5 tạo đề mới: đổi đáp án và chọn lại cách diễn đạt; không lặp nguyên đáp án ngay lượt trước.
- R thử lại: làm trống ghế và đưa người về vị trí đầu; giữ nguyên seed, đáp án và manh mối.
- initialSeed trong Inspector: 0 để mở Play với phiên ngẫu nhiên; số khác để tái hiện chuỗi đề khi kiểm thử.

## Khi số người thay đổi

ApplyParticipantRoster nhận danh sách ID theo slot từ một nguồn bên ngoài. Adapter mạng sau này phải gọi hàm này khi người tham gia thay đổi.

- Cùng roster: không làm mất tiến trình.
- Từ 2 sang 3, 3 sang 4, 4 xuống 2, hoặc thay một người bằng người khác: hủy lượt hiện tại, nhả ghế, đóng cửa và tạo đề mới cho đội mới.
- Còn 0 hoặc 1 người: chuyển sang chờ, không cố giải một đề không còn đủ manh mối.
- ID trùng hoặc quá 4 người: từ chối dữ liệu thay vì âm thầm cắt bớt.

Việc tạo lại đề khi đổi đội là chủ ý của bản thử: tránh giữ manh mối thuộc về người đã rời. Chưa triển khai khôi phục phiên qua mạng, đồng bộ seed hoặc quyền host. Những phần đó thuộc mốc multiplayer.

## Nơi chỉnh text

Asset: Assets/TheReturn/Features/Attendance/Data/AttendanceText_VI.asset.

Trong Unity, chọn asset, mở Entries, chọn một loại điều kiện rồi mở Variants. Có 30 mẫu mặc định, 3 mẫu cho mỗi loại. Bạn có thể thêm biến thể hoặc sửa lời văn mà không cần thay code.

Placeholder:
- {a}, {b}: tên nhân vật.
- {row}, {column}: số hàng/cột từ 1.
- {steps}: khoảng cách hàng/cột.
- {parity}: “chẵn” hoặc “lẻ”.

Giữ đủ placeholder và đúng nghĩa loại luật. Inspector báo câu trống, thiếu loại hoặc sai token. Hệ thống kiểm tra cấu trúc text; không thể tự hiểu để phát hiện trường hợp bạn đổi chữ “trái” thành “phải” trong một mẫu mang luật Left.

Role Names quản lý tên bốn vai. Title, Introduction, Ready, Wrong, Success và Hints quản lý lời giới thiệu, phản hồi và gợi ý. Nếu sửa asset trong Play Mode, Unity có thể lưu thay đổi asset; nên chỉnh ngoài Play rồi thử lại để dễ theo dõi.

Builder chỉ tạo catalog mặc định khi chưa có, không ghi đè text đã chỉnh.

## Tài sản 3D và tái sử dụng

Lớp gồm sàn, tường biên, bảng hướng dẫn, 24 bàn ghế, cửa và vùng thoát. Không thêm cửa sổ, gạch trang trí, tủ, ánh sáng tạo không khí hoặc đạo cụ không liên quan.

Các prefab:
- Shared/Prefabs/Graybox/PF_GB_Block: khối đơn vị có collider; dùng lại cho sàn/tường.
- Shared/Prefabs/Graybox/PF_GB_DeskChair: phần hình học tối thiểu của một bộ bàn ghế.
- Shared/Prefabs/Graybox/PF_GB_SlidingDoor: cửa nhận tín hiệu mở/đóng, không biết luật điểm danh.
- Features/Attendance/Prefabs/PF_AttendanceSeat: lồng prefab bàn ghế; thêm ID, mốc ngồi/đứng và nhãn.

24 chỗ là các prefab instance có liên kết, không phải 24 bản hình học được viết tay riêng. Có thể thay Visual_Replaceable trong prefab bàn ghế để cập nhật cả phòng. Giữ collider hợp lý và các mốc SitPoint/StandPoint khi đổi model.

Lớp được dựng theo khoảng cách hàng 2,7 m và cột 3 m, có lối đi giữa các dãy. Các mốc đứng đã được kiểm tra không giao collider. Mỹ thuật cuối sẽ thay phần visual sau khi gameplay ổn.

## Kiểm chứng

Báo cáo logic: Design/Verification/AttendanceV2.json.

Đã kiểm tra 90 đề: 30 seed cho mỗi cỡ đội 2, 3 và 4. Kiểm tra duy nhất một nghiệm, mọi người có thông tin cần thiết, sinh lại cùng seed, tranh ghế, đáp án sai, đáp án đúng, đứng sau khi giải và reset. Các đề thử bao phủ đủ 24 ghế và cả 10 loại điều kiện.

Play Mode đã kiểm tra đủ 2/3/4 người ngồi thì mở cửa; thử lại giữ đề; đề mới đổi đáp án; roster không đổi giữ tiến trình; thiếu người chuyển chờ; người thay thế tạo đề mới.

Đây chưa thay thế playtest với người thật. Cần đo thời gian đọc, độ dễ hiểu của quan hệ hàng/cột và số lần thử sai để chỉnh độ khó tiếp.
