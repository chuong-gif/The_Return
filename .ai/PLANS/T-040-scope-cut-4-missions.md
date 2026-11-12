# T-040 — chỉ giữ Core4, hoàn tất map mới

Trạng thái: IN-PROGRESS. Scene đích hiện hành: `Assets/TheReturn/Scenes/Maps/MapSchoolF.unity` do chủ dự án dựng. Tình trạng online chi tiết: T-042/`QUEUE.md`.

## Phạm vi
- Phát triển: PlankAndKey (Hanh_Lang_6), Attendance (Phong_1), BreakerPanel (Phong_5), Choir (Phong_6), map và online dùng chung.
- Hanh_Lang_5 là lối đi, không có nhiệm vụ.
- Đã cắt: TeacherSays, GlassBridge, SharedLantern, QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth. Không tiếp tục phát triển.
- Code/scene bị cắt giữ nguyên cho tới điều kiện Q-013. Danh sách phụ thuộc an toàn: `T-040-dependencies.md`.

## Tình trạng
- Layout SchoolFloorCore4 và sơ đồ T-040 ban đầu đã bị MapSchoolF thay thế; không dùng layout cũ làm căn cứ sửa scene.
- T-042 đã nối 4 online adapter vào MapSchoolF; host một mình Play/Gauntlet đã kiểm. Online nhiều máy chưa xác nhận.
- Q-014 tích hợp dùng chung đang tiếp tục; Q-015 xong. T-042 Q-023 đã hoàn tất đường offline/Build Settings theo P-013, nhưng room→map online chờ P-014. Q-024 chờ bước trước.
- Scene phải được snapshot trước khi sửa; không xóa/dời object của chủ dự án nếu chưa được giao rõ.

## Cổng hoàn tất và xóa code
1. Hoàn tất map/flow + online theo Q-014, Q-023/024; test, Gauntlet và smoke thực tế được ghi rõ theo mức xác minh.
2. Chủ dự án xác nhận map mới chạy và đã commit/tag.
3. Chỉ khi đủ điều kiện 2 mới mở Q-013 để xóa tám nhiệm vụ theo dependency list, theo từng đợt compile sạch + Gauntlet. Không tự commit/tag.

P-014 trong `DECISIONS/PENDING.md` là câu hỏi cần chủ dự án duyệt; không tự nối NetworkRoomTest/RoomMapHandoff.
