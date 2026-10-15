# PROJECT — The Return

Chú giải trạng thái: [V] đã xác minh 2026-10-14 bằng Unity MCP | [S] suy luận từ cấu trúc/tên | [?] chưa biết

- [V] Unity 6000.3.7f1, URP (UniversalRenderPipelineAsset), Linear color space, Input System 1.18.0, Test Framework 1.6.0.
- [V] Gói MCP: com.coplaydev.unity-mcp (git, nhánh main — không ghim phiên bản).
- [V] Build scenes: `Assets/TheReturn/Scenes/Maps/Lobby/Sanh_Cho.unity`, `Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`.
- [V] Scene đang mở: SchoolFloorPrototype.
- [V] Nội dung: theo `PROJECT_STATE.md` và `Design/` — tầng trường học với các nhiệm vụ theo đội (2–4 người): Attendance, GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack. Phòng 5/6 và hành lang 1/3/5/6 chưa có nhiệm vụ (do chủ dự án quyết định, không tự bịa).
- [?] Mạng nhiều máy: không thấy Mirror/Netcode/Photon trong code; test hiện dùng teleport trên một máy [V].
- [?] Save/load: không thấy PlayerPrefs; JsonUtility xuất hiện ở 3 file — chưa xác định có phải save system.
- Nguồn tài liệu thiết kế của chủ dự án: thư mục `Design/` (tiếng Việt), `PROJECT_STATE.md`, README trong `Assets/TheReturn`.

## Ý đồ thiết kế (chủ dự án nêu 2026-10-14; chi tiết: DECISIONS/2026-10-14-nonlinear-mission-selection.md)
- Bố cục: 6 phòng + 6 hành lang thành vòng tròn; mỗi cửa vào được từ sân; mọi phòng/hành lang nhiệm vụ có khu chuẩn bị phía trước.
- Hiện: 4 phòng + 2 hành lang có nhiệm vụ, luồng tuyến tính trong code. Hướng mới: người chơi tự chọn phòng. Chưa triển khai.

## Sản phẩm và việc chưa làm (chủ dự án nêu 2026-10-14)
- Thể loại: game co-op ONLINE 2-4 người chơi.
- Chưa có: màn hình bắt đầu game; tìm và lập nhóm (matchmaking/party); giao diện Settings; map Sảnh chờ chưa xong (mục đích: sau khi lập nhóm, người chơi xuất hiện ở đây, đủ người thì chủ phòng bắt đầu để vào map); giao diện thắng game; tích hợp online/mạng (hiện chỉ nhấn phím 1,2,3,4 để đổi qua lại người chơi khi phát triển); nhân vật; hệ thống animation nhân vật.
- Chủ dự án tự nâng cấp đồ họa map. Bố cục phòng cơ bản giống object `Archives_SpatialPass01` trong map nhiệm vụ; bên trong phòng chủ dự án sẽ bố trí khác đôi chút. Không tự dời bố cục.
- Prefab: phải linh hoạt để thay asset 3D, có mô tả cách dùng; chỉ vật có chức năng tương tác/phục vụ logic nhiệm vụ mới làm prefab.
- UI: cần tổ chức lại để nhiều không gian nhìn và linh hoạt giữa các khu nhiệm vụ.
- Hệ quả cho thiết kế: trạng thái map/nhiệm vụ nên tách khỏi hiển thị để sau này đồng bộ mạng (host quyết định) — chưa triển khai mạng.
