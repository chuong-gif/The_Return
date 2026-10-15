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
