# The Return — hướng dẫn thư mục

Đọc file này trước khi xóa hoặc di chuyển tài sản. Các đường dẫn dưới đây tính từ `Assets/TheReturn`.

```text
TheReturn/
├── Art/                         Vật liệu, hình ảnh và công cụ dựng môi trường của dự án.
│   ├── Environment/             Tài sản môi trường, blockout và các lần dựng trước.
│   ├── Interiors/               Vật liệu nội thất, bản chuyển URP dùng với ThirdParty.
│   └── Editor/                  Các công cụ dựng cảnh CŨ; không tự chạy lại để refresh.
├── Assets3D/                    Model nguồn do người dùng nhập.
│   └── Map1SanhCho/             Có Untitled.fbx; giữ file và .meta để không mất liên kết.
├── Core/Runtime/                Thành phần nền dùng giữa các map.
│   ├── Players/                PrototypePartyController: di chuyển, đổi vai, teleport.
│   └── SceneFlow/              SceneTransitionService: chuyển scene.
├── Features/                    Sáu nhiệm vụ; mỗi thư mục có README riêng.
│   ├── Attendance/             Điểm danh — Phong_1, 24 bàn ghế.
│   ├── QuietCorridor/          Giữ trật tự — Hanh_Lang_2.
│   ├── GradeRepair/            Sửa bảng điểm — Phong_2.
│   ├── NavigationExam/         Bài kiểm tra — Phong_3.
│   ├── DontLookBack/           Không được quay đầu — Hanh_Lang_4.
│   └── TeacherTruth/           Giáo viên nói thật/nói dối — Phong_4.
├── Maps/                        Scene chơi và phần nối nhiệm vụ theo từng map.
│   ├── Lobby/                  Sảnh chờ: trạng thái sẵn sàng, Canvas, công cụ dựng.
│   └── SchoolFloor/
│       ├── Scenes/             SchoolFloorPrototype.unity là scene tầng học đang sửa.
│       ├── Runtime/            SchoolFloorFlow nối nhiệm vụ; AccessController khóa các lối.
│       │   ├── Testing/        MissionTestLauncher: vào thẳng nhiệm vụ để test.
│       │   └── UI/             Presenter/View cho Canvas nhiệm vụ.
│       ├── Prefabs/            PF_PreparationGate: cửa chặn + vùng chuẩn bị có logic.
│       ├── UI/                 Canvas và tài sản UI của tầng học.
│       ├── Editor/             Công cụ quản lý, migration và các builder cũ.
│       └── Tests/Editor/       Kiểm tra luật/scene; một số bài cũ dùng tọa độ bố cục cũ.
├── Scenes/Maps/Lobby/           Scene Sanh_Cho riêng đã có; không phải thư mục trùng vô dụng.
└── Shared/                      Tài sản và code dùng chung cho nhiều nhiệm vụ/map.
    ├── Runtime/Interaction/    PuzzleDoor: đóng/mở panel cửa.
    ├── Runtime/UI/             Theme và liên kết theme Canvas.
    ├── Art/                    Shader, vật liệu dùng chung.
    ├── Prefabs/Graybox/        Prefab cửa, bàn ghế mẫu, khối mẫu cũ — xem lưu ý bên dưới.
    └── UI/Prefabs/             Nút/panel giao diện có chức năng, không phải đồ trang trí cảnh.
```

## Mở và sửa đúng chỗ
- Scene tầng học: `Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`.
- Menu **The Return > School Floor > Objects - Find and Edit**: tìm đồ tương tác, chọn instance, mở prefab nguồn.
- Chỉnh nội dung nhiệm vụ trong `Features/<nhiệm vụ>/Data/*.asset`, không cần sửa script để đổi text có sẵn.
- Trong scene, `SchoolLayoutV2` quản lý các cửa/vùng chuẩn bị. `Archives_SpatialPass01` là nội thất phòng Sửa bảng điểm.
- `BlockoutCampus_Generated/Architecture_From_User_Blockout` là kiến trúc đang chơi; bản nguồn `Map The Reture ` được giữ riêng. Không đặt lại cả phòng nhỏ vào blockout.
- `RetiredShell_Disabled`, các root môi trường cũ đang ẩn, `BeforeSpatialPass_*` và `BeforeLayoutV2_*` là phần phục hồi/lịch sử, không phải cảnh đang chơi. Chưa tự xóa chúng.

## Quy tắc prefab đã rà soát
**Prefab gameplay dành cho đồ có logic/tương tác:** ghế điểm danh, nút, bảng, thẻ, kênh giáo viên, cửa. Cửa chuẩn bị mới cũng thuộc nhóm này.

Khối tường/sàn tĩnh dùng `PF_GB_Block` ở scene đang chơi đã được gỡ liên kết khi là instance độc lập không có logic. Asset mẫu cũ vẫn giữ để không làm mất reference ở scene dự phòng/builder; không xem đó là chuẩn cho kiến trúc mới. `PF_GB_DeskChair` là hình mẫu bàn ghế, có thể nằm trong ghế điểm danh; không xóa trước khi kiểm tra dependency. Thư viện ThirdParty vẫn giữ nguyên prefab nguồn của nhà phát hành; không có nghĩa dự án cần tạo thêm prefab riêng cho từng bức tường.

UI button/panel và Canvas là prefab có chức năng giao diện, được giữ. Các model FBX cũng do Unity nhập dưới dạng model prefab; giữ nguồn nhập, không đổi/xóa vì tên gọi prefab.

## Thay model mà giữ logic
- Với `PF_GradeTerminal` và `PF_GB_SlidingDoor`: thay mesh con trong `VisualRoot`, giữ parent chứa collider, Label và panel chuyển động. Kiểm tra kích thước vì parent hiện có scale.
- Với `PF_PreparationGate`: collider nằm ở `Interaction_Blocker`, hình ảnh ở `VisualRoot`, trigger ở `PreparationZone`; ba phần tách rời. Đổi model không tự đổi kích thước collider.
- Các prefab nhiệm vụ còn lại chưa được chuẩn hóa toàn bộ sang VisualRoot; xem README từng nhiệm vụ để giữ các reference bắt buộc.
- Di chuyển asset bằng cửa sổ Project trong Unity để giữ `.meta`/GUID. Không xóa `.meta`, `.asmdef`, ScriptableObject hoặc prefab có scene tham chiếu.

## Cửa và tiến trình
Mỗi phòng có 3 cửa và vùng chuẩn bị. Phòng nhiệm vụ chưa đến lượt khóa các lối; lối vào chỉ mở sau khi nhiệm vụ trước hoàn thành. Cả đội đi vào vùng tập hợp phía trong thì nhiệm vụ bắt đầu và khóa cả 3 lối. Hoàn thành mở lại cả 3. Hai phòng chưa có nhiệm vụ luôn mở. Hai hành lang nhiệm vụ có cửa đầu/cuối riêng; cổng âm thanh bên trong Giữ trật tự vẫn giữ luật của nó.

Các builder cũ có thể ghi đè cảnh. `GradeRepairSpatialPass` và `SchoolLayoutV2Migration` đã chạy; **không chạy lại để cập nhật cảnh**. Các bước tiếp theo chỉnh cảnh hiện tại hoặc tạo migration mới có snapshot. Bản ghi làm việc ngắn nằm ở `PROJECT_STATE.md` tại gốc project, ngoài Assets.
