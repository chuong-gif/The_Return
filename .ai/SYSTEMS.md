# SYSTEMS — chỉ liệt kê hệ thống thực sự tồn tại

| Hệ thống | Vị trí | Số .cs | Ghi chú |
|---|---|---|---|
| Attendance (Phòng 1, 24 ghế) | Features/Attendance | 10 | Domain + Data (ScriptableObject) + Presentation + Editor + Tests |
| GradeRepair (Phòng 2) | Features/GradeRepair | 5 | |
| NavigationExam (Phòng 3) | Features/NavigationExam | 3 | |
| TeacherTruth (Phòng 4) | Features/TeacherTruth | 5 | |
| QuietCorridor (Hành lang 2) | Features/QuietCorridor | 5 | Prefab PF_QuietSpeaker |
| DontLookBack (Hành lang 4) | Features/DontLookBack | 3 | |
| SchoolFloor (tầng học, cửa, access) | Maps/SchoolFloor | 29 | SchoolFloorAccessController, SchoolPreparationGate (22 cổng), canvas presenter, MissionTestLauncher |
| Lobby (Sanh_Cho) | Maps/Lobby | 3 | |
| Core (người chơi dùng chung) | Core/Runtime | 2 | Bộ điều khiển thử một máy |
| Shared (cửa, tương tác) | Shared/Runtime | 3 | Interaction, SlidingDoor |

Nguồn mô tả hành vi: `PROJECT_STATE.md`, `Design/*.md`, README từng feature. Chưa xác minh: UI chi tiết, animation, save/load, audio — [?].
Danh mục class/method chi tiết: chưa có index (xem `.ai/TOOLING.md`).
