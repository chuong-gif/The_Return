# Nhiệm vụ cần thiết kế (để AI/người khác làm)

Quy tắc: mỗi phòng và mỗi hành lang phải có nhiệm vụ cần cả đội; map chỉ thắng khi mọi nhiệm vụ xong. Chủ dự án CHƯA thiết kế các mục dưới đây. AI không được tự bịa thiết kế; cần hỏi chủ dự án hoặc nhận brief từ họ rồi mới làm.

| Khu (tên object) | Trạng thái | Ghi chú |
|---|---|---|
| Phong_1 | Có: Attendance | |
| Phong_2 | Có: GradeRepair | |
| Phong_3 | Có: NavigationExam | |
| Phong_4 | Có: TeacherTruth | |
| Phong_5 | CHƯA CÓ nhiệm vụ | cần thiết kế; đã có 3 cổng, luôn mở |
| Phong_6 | CHƯA CÓ nhiệm vụ | cần thiết kế; đã có 3 cổng, luôn mở |
| Hanh_Lang_1 | Có: QuietCorridor | cổng gắn nhãn «Hanh lang 2» |
| Hanh_Lang_2 | CHƯA CÓ nhiệm vụ | chưa có cổng/khu chuẩn bị |
| Hanh_Lang_3 | Có: DontLookBack | cổng gắn nhãn «Hanh lang 4» |
| Hanh_Lang_4 | CHƯA CÓ nhiệm vụ | chưa có cổng/khu chuẩn bị |
| Hanh_Lang_5 | CHƯA CÓ nhiệm vụ | chưa có cổng/khu chuẩn bị |
| Hanh_Lang_6 | CHƯA CÓ nhiệm vụ | chưa có cổng/khu chuẩn bị |

Dùng TÊN OBJECT (đề xuất, chờ chủ dự án xác nhận — xem MAPS/area-adjacency.md mục 4). Bảng cũ dùng nhãn thứ tự nhiệm vụ nên lệch tên hành lang.

Mỗi nhiệm vụ mới cần: cả đội 2-4 người, khu chuẩn bị phía trước, tuân theo cấu trúc Features/<Tên> (Domain thuần + Presentation + Data + Tests), prefab chỉ cho vật có tương tác/logic.
Nguồn bảng: PROJECT_STATE.md (cập nhật 2026-09-29) + chủ dự án 2026-10-14; kiểm lại trước khi dùng.
