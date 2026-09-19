# Quy ước tổ chức dự án The Return

## Thư mục

Assets/TheReturn là gốc tài sản do dự án sở hữu. Package bên ngoài nằm trong Packages hoặc thư mục riêng của nhà cung cấp, không trộn vào code gameplay.

| Thư mục | Trách nhiệm |
| --- | --- |
| Core/Runtime/Players | Điều khiển người chơi dùng chung, hiện có bộ điều khiển thử một máy |
| Shared/Runtime/Interaction | Thành phần dùng lại như cửa; không tham chiếu luật của từng câu đố |
| Shared/Prefabs/Graybox | Hình khối tối thiểu để thử logic |
| Shared/Art/Materials/Graybox | Vật liệu đang dùng cho bản thử |
| Shared/Art/Materials/LegacyPrototype | Vật liệu cũ được giữ để không mất liên kết của tài sản khác |
| Shared/Art/Shaders | Shader dùng chung |
| Features/Attendance/Runtime/Domain | Luật, đề và trạng thái thuần C#, không tham chiếu Unity |
| Features/Attendance/Runtime/Data | ScriptableObject quản lý nội dung |
| Features/Attendance/Runtime/Presentation | Ghế, session, HUD của tính năng điểm danh |
| Features/Attendance/Data | Các asset cấu hình và text |
| Features/Attendance/Prefabs | Prefab tương tác riêng của câu điểm danh |
| Features/Attendance/Scenes | Scene thử tính năng |
| Features/Attendance/Editor | Công cụ dựng cảnh và Inspector; không vào bản build |
| Features/Attendance/Tests/Editor | Kiểm tra tự động; không vào bản build |
| Scenes/Maps/Lobby | Scene sảnh chờ Sanh_Cho của bạn |
| Design | Thiết kế, hướng dẫn và kết quả kiểm tra ngoài Assets |
| Captures | Ảnh kiểm tra ngoài Assets |
| Backups | Bản sao trước di chuyển ngoài Assets, không bị Unity nhập lại GUID |

Mỗi tính năng mới có thể dùng cấu trúc Features/<TênTínhNăng>. Chỉ chuyển code/tài sản vào Core hoặc Shared khi có lý do dùng chung cụ thể. Không tạo một thư mục Managers chứa mọi hệ thống.

Assembly Domain không được tham chiếu Unity. Assembly Attendance tham chiếu Domain, Core, Shared; Core và Shared không tham chiếu ngược Attendance. Các assembly Editor và Tests chỉ biên dịch trong Editor.

## Quy ước tài sản

Prefab có tiền tố PF_, material có M_. Tên asset bằng ký tự dễ tìm kiếm và không gắn số thứ tự người chơi. Scene thử phân biệt với scene map dùng để phát hành. Dùng prefab có component tương tác và child Visual_Replaceable để thay model mà không viết lại logic.

Khi di chuyển asset, dùng Unity Project hoặc AssetDatabase.MoveAsset để giữ .meta/GUID. Không kéo riêng file .cs, .prefab hoặc .mat bằng Explorer rồi bỏ file .meta.

Sanh_Cho đã được chuyển vào Scenes/Maps/Lobby bằng Unity; nội dung scene được đối chiếu hash với bản sao và không đổi. Bản cũ nằm tại Backups/BeforeAttendanceV2_20260914. Không chép nguyên thư mục sao lưu vào Assets vì sẽ trùng GUID.

Các thư mục sẵn có như Assets/Scripts, Assets/Settings và TutorialInfo không tự động nhập vào cấu trúc mới vì có thể thuộc tài sản hoặc cấu hình bạn đang quản lý.

## Quy ước chú thích code

Mọi file C# do tôi viết trong Assets/TheReturn có bình luận ở đầu file:
1. Mục đích của file và phạm vi trách nhiệm.
2. Danh sách các hàm cùng chức năng.

Ngay trước mỗi hàm/constructor có summary tiếng Việt:
- Hàm làm gì.
- Nhận dữ liệu đầu vào gì.
- Trả dữ liệu đầu ra gì hoặc thay đổi trạng thái nào.

Getter biểu thức đơn giản thể hiện dữ liệu đọc; không cần sao chép lại toàn bộ logic vào chú thích. Chú thích cần giải thích cả ý định khi xử lý khó, ví dụ vì sao đổi roster phải sinh đề mới hoặc vì sao ray tương tác bỏ collider của chính nhân vật.

Khi thêm/sửa/xóa hàm, cập nhật danh sách đầu file cùng lúc. Giữ tên hàm tiếng Anh nhất quán để tìm kiếm API; bình luận và hướng dẫn dành cho bạn dùng tiếng Việt.

## Điểm mở rộng

Để kết nối mạng, thêm adapter gọi ApplyParticipantRoster với các ID kết nối ổn định. Host phải sở hữu seed, đề và AttendanceState; client chỉ gửi yêu cầu tương tác. Chưa coi bộ điều khiển đổi vai trên một máy là lớp network.

Để làm câu đố mới, tái sử dụng điều khiển nhân vật, cửa và prefab graybox; thêm Domain/Data/Presentation của tính năng mới. Không thêm luật mới vào AttendancePrototype nếu luật đó không thuộc điểm danh.

## Map tầng học và nhiệm vụ 2

Maps/SchoolFloor/Scenes chứa scene tích hợp SchoolFloorPrototype (giữ GUID scene lớp cũ). Maps/SchoolFloor/Runtime điều phối các nhiệm vụ; Editor chứa công cụ gắn hành lang và Tests/Editor chứa kiểm chứng liên nhiệm vụ. Features/QuietCorridor dùng Runtime/Domain, Runtime/Data, Runtime/Presentation, Data và Prefabs. Assembly Domain không tham chiếu Unity; hai feature không tham chiếu nhau, assembly map tham chiếu cả hai.

## Sửa bảng điểm

Features/GradeRepair gồm Runtime/Domain (luật thuần C#), Runtime/Data (catalog), Runtime/Presentation (terminal và controller), Data (asset text) và Prefabs. Maps/SchoolFloor chứa flow/HUD, công cụ ghép phòng trong Editor và kiểm tra liên nhiệm vụ trong Tests/Editor. Các feature không tham chiếu nhau; flow map chuyển điều khiển và giữ kết quả nhiệm vụ trước. Xem Sua-bang-diem.md.

## Bài kiểm tra dẫn đường
Features/NavigationExam chứa Runtime/Domain, Runtime/Data, Runtime/Presentation, Data và Prefabs. Domain giữ luật lượt/ô/dấu; map flow giữ tiến trình bốn nhiệm vụ. Công cụ dựng và kiểm tra nằm trong Maps/SchoolFloor/Editor và Tests/Editor.


## UI Canvas và công cụ test
Shared/Runtime/UI chứa theme và binding dùng chung; Shared/UI/Themes và Shared/UI/Prefabs chứa tài sản giao diện tái sử dụng. Maps/SchoolFloor/UI/Prefabs chứa Canvas của map; Runtime/UI nối giao diện với gameplay, Runtime/Testing chứa bộ chọn nhiệm vụ dành cho Editor/Development Build. Công cụ dựng giao diện nằm trong Editor. Hướng dẫn sử dụng: [UI và test nhanh](UI-va-test-nhanh.md).

## Không được quay đầu
Features/DontLookBack chứa Runtime/Domain, Runtime/Data, Runtime/Presentation, Data và Prefabs. Map flow nối sau NavigationExam; Canvas có DontLookBackPanel và nút test thứ năm. Domain không tham chiếu Unity. Xem [hướng dẫn nhiệm vụ 5](Khong-duoc-quay-dau.md).
