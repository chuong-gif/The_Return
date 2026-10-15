# The Return — trạng thái hiện tại

Cập nhật 2026-09-29. Đọc file này rồi chỉ kiểm tra khu vực sắp sửa; không quét lại toàn dự án.

## Đã triển khai
- Scene chính: Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity.
- Phong_1 Attendance (24 ghế), Phong_2 GradeRepair, Phong_3 NavigationExam, Phong_4 TeacherTruth. Hanh_Lang_1 QuietCorridor, Hanh_Lang_3 DontLookBack. Phòng 5/6 và hành lang 2/4/5/6 chưa có nhiệm vụ.
- Dùng trực tiếp blockout. Vỏ nhỏ cũ của các nhiệm vụ đã ẩn; Environment_LogicOnly của Attendance đã tắt. Giữ vật có logic và nội thất, không đưa nguyên phòng vào phòng. Một số đồ trang trí vẫn cần sắp xếp/thẩm mỹ thêm.
- SchoolLayoutV2 chứa SchoolFloorAccessController và 22 SchoolPreparationGate: 3 cửa mỗi phòng, 2 cửa mỗi hành lang nhiệm vụ. Hai lối vòng phòng rộng khoảng 8.9m, lối hướng sảnh khoảng 7.06m; cổng có dư biên nhỏ. Cửa vào mở khi nhiệm vụ trước xong; đủ đội tập hợp phía trong thì flow bắt đầu, khóa mọi cửa khu đang làm; giải xong mở lại. Khu chưa có nhiệm vụ luôn mở.
- Flow.transitionGatherZones và stagingZone đã gán trigger mới. Chế độ test nhanh vẫn dùng MissionTestLauncher. Cửa nhỏ cũ giữ ẩn do controller còn tham chiếu; quyền chặn các lối thật do AccessController quản lý.
- Archives_SpatialPass01 có năm cụm tra cứu và khu đọc chung, dùng Furniture Mega Pack. Luật 4 tài liệu hiện có giữ nguyên; chưa thêm random vị trí hoặc sổ tay.
- Menu The Return > School Floor > Objects - Find and Edit giúp chọn vật logic/mở prefab. GradeTerminal, SlidingDoor có VisualRoot, collider/panel bên ngoài; PreparationGate có Interaction_Blocker, VisualRoot và PreparationZone độc lập. Các prefab khác chưa đồng loạt chuẩn hóa VisualRoot.
- Đã gỡ liên kết PF_GB_Block cho khối tĩnh độc lập đang hoạt động. Speaker có logic chuyển thành Features/QuietCorridor/Prefabs/PF_QuietSpeaker.prefab; session là scene override cần gán khi tạo instance mới. Không xóa các asset mẫu, scene dự phòng hoặc prefab nguồn ThirdParty.
- Hướng dẫn người dùng: Assets/TheReturn/README.md, Features/README.md và README của cả sáu nhiệm vụ. Có cây thư mục, vai trò code/prefab, reference phải giữ và cảnh báo builder cũ.

## Bằng chứng kiểm tra
- Play Mode 2/3/4 người, 6 điểm vào test: 456 kiểm tra trạng thái cửa/vùng tập hợp/tương tác 24 ghế đạt.
- 15 chuyển tiếp (5 bước x 3 cỡ đội): thiếu người không bắt đầu, đủ người bắt đầu và khóa mọi lối khu tương ứng.
- CharacterController đi thử trung tâm cả 22 cửa: đóng chặn 22/22, mở đi qua 22/22. Sau đó thu nhỏ 6 cổng hướng sảnh theo kích thước đo mesh; không thay tâm lối đáng kể.
- 16 checkpoint của Grade/Exam/Back/Teacher có sàn và không bị chặn sau bố trí.
- Trước đó: Grade và Quiet 2/3/4 đọc/tương tác/giải/retry đạt. Quiet đã sửa AllAcross dùng hộp local khi xoay.
- Các bài kiểm tra dùng teleport giữa điểm; không phải kiểm thử mạng nhiều máy hoặc đi bộ toàn tuyến. Chưa nghiệm thu hình ảnh điện ảnh. Một số bài kiểm tra lịch sử hardcode tọa độ cũ, không tự chạy lại và diễn giải sai kết quả.

## Bảo toàn và quy tắc
- Snapshot trước đợt mới: BeforeLayoutV2_20260929_151752.unity; trước mẫu Grade: BeforeSpatialPass_20260929_143309.unity, cùng thư mục Scenes. Có cả sửa tay chưa lưu lúc bắt đầu.
- Không chạy lại GradeRepairSpatialPass.Build/FinishSample hoặc SchoolLayoutV2Migration.Build: đã chạy; sửa scene hiện tại hoặc tạo migration mới có snapshot.
- Không chạy lại Art/Editor/UserBlockoutLayoutBuilder, MissionRoomEnvironmentPass, ClassroomEnvironmentPass hay các builder cũ để refresh: có thể tái tạo bố cục đã bị bác bỏ.
- Không commit/push nếu chưa có yêu cầu mới. Không tự sáng tác nhiệm vụ cho 2 phòng và 4 hành lang trống.
- Code mới: đầu file mô tả mục đích + danh sách hàm; đầu hàm mô tả đầu vào/đầu ra. Giữ GUID/.meta khi di chuyển asset.
- Kiến trúc thật đang chơi: BlockoutCampus_Generated/Architecture_From_User_Blockout; Map The Reture nguồn giữ riêng. Không thay đổi blockout của người dùng mà không kiểm tra hiện trạng.

## Tiếp theo
- Kiểm tra người dùng chơi liên tục một vòng, đặc biệt nhánh sảnh và chỉ dẫn điểm tập hợp; hoàn thiện nội thất/ánh sáng theo khu thay vì lắp lại vỏ phòng.
- Khi thay model, mở đúng prefab logic từ cửa sổ Objects; không xóa mốc tương tác/collider/ID. Đọc README nhiệm vụ trước.

> Ghi chú 2026-10-15: số hành lang đã đổi theo tên object mới trong scene (cũ 1→6, 2→1, 3→2, 4→3, 5→4, 6→5). Chỉ đổi số hành lang, nội dung khác giữ nguyên.
