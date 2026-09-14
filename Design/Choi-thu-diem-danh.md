# Chơi thử điểm danh v2

Scene: Assets/TheReturn/Features/Attendance/Scenes/AttendancePrototype.unity.

Nhấn Play, chọn tab Game, bấm 2, 3 hoặc 4 người rồi Bắt đầu. Đây là chế độ diễn tập một máy; dùng 1–N để đổi vai và đọc từng gói manh mối.

- WASD và chuột: đi/nhìn.
- E: nhìn vào bàn/ghế trong tầm để ngồi; nhấn lại để đứng.
- H: gợi ý.
- F5: đề mới với đáp án mới.
- R: thử lại cùng đề.
- Tab hoặc Esc: mở menu và thả chuột.

24 ghế đánh địa chỉ H1-C1 đến H6-C4. Hàng 1 sát bảng, hàng 6 xa bảng. Cột tăng từ trái sang phải khi nhìn lên bảng. Trước/sau là quan hệ hàng; trái/phải là quan hệ cột, không mặc định cùng hàng.

Khi đủ số người của đội ngồi, đợi 2 giây để kiểm tra. Sai thì đứng lên đổi chỗ. Đúng thì đứng dậy và ra cửa EXIT. Đổi số người giữa lượt sẽ tạo lại đề; không cần dừng Play.

Chỉnh manh mối: chọn Features/Attendance/Data/AttendanceText_VI.asset, mở Entries > Variants. Hướng dẫn token và báo lỗi có ngay trong Inspector. Text không nằm trong code gameplay.

Xem Diem-danh-v2.md cho quy luật sinh đề và Cau-truc-du-an.md cho tổ chức tài sản.

Để kiểm tra lại: menu The Return > Attendance > Validate Generated Puzzles. Báo cáo được ghi tại Design/Verification/AttendanceV2.json.

Menu Rebuild Logic Classroom tạo lại bố cục scene từ prefab. Lưu thay đổi trước khi dùng; thao tác này thay bố cục scene sinh tự động, nhưng giữ prefab và catalog text đã có. Không cần rebuild để chỉnh text.

Chưa có multiplayer, voice chat hoặc build phát hành. Phiên này đã kiểm tra bằng Play Mode và mô phỏng roster/input.
