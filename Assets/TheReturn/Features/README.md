# Các nhiệm vụ

| Thư mục | Nhiệm vụ | Khu vực |
|---|---|---|
| Attendance | Điểm danh | Phòng 1 |
| QuietCorridor | Giữ trật tự | Hành lang 1 |
| GradeRepair | Sửa bảng điểm | Phòng 2 |
| NavigationExam | Bài kiểm tra | Phòng 3 |
| DontLookBack | Không được quay đầu | Hành lang 3 |
| TeacherTruth | Giáo viên nói thật/nói dối | Phòng 4 |

Trong mỗi thư mục: `Data` chứa nội dung chỉnh qua Inspector; `Runtime/Data` định nghĩa kiểu dữ liệu; `Runtime/Domain` chứa luật không phụ thuộc đồ họa; `Runtime/Presentation` nối luật với nhân vật, input và đồ tương tác; `Prefabs` chứa vật nhiệm vụ. `.asmdef` chia cụm biên dịch, không phải file thừa.

Đọc README trong từng nhiệm vụ trước khi thay model. Code ghép map, vùng chuẩn bị, khóa cửa và Canvas nằm ở `Maps/SchoolFloor`, không bị thiếu khỏi Features. Hiện đổi vai 1–4 là mô phỏng một máy, không phải bằng chứng đã triển khai co-op mạng.
