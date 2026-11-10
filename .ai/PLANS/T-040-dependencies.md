# T-040 Khảo sát phụ thuộc (Q-010, CHỈ ĐỌC) — lập 2026-11-10 19:18 UTC bởi phiên #13
Không sửa Assets khi khảo sát. Số đo lấy từ scene SchoolFloorPrototype đang mở và prefab (bounds renderer/collider gồm vật con, làm tròn 0,1 m).

## 0. KẾT LUẬN (đọc phần này là đủ)
1. Chỗ dính 8 nhiệm vụ bị cắt GỌN: 41 file code ngoài thư mục riêng của chúng, hầu hết nằm trong Maps/SchoolFloor, vài builder ở Art/Editor và 1 file âm thanh. KHÔNG file nào trong Features của 4 nhiệm vụ giữ (hay Features/Network) nhắc tên nhiệm vụ bị cắt.
2. Lõi điều phối KHÔNG nhắc tên nhiệm vụ nào: MissionFlowController, MapProgress, SchoolFloorAccessController (không có trong danh sách (a)). Đây là phần có thể tái dùng cho scene mới (cần xác nhận khi dựng).
3. Phần dính cứng cần viết lại/cắt nhánh cho 4 nhiệm vụ: SchoolFloorFlow, MissionRegistryFactory, MissionTestLauncher, SchoolFloorCanvasPresenter (và các partial), âm thanh theo khu.
4. Scene dính 8 nhiệm vụ: SchoolFloorPrototype + 7 bản lưu Before* + Assets/_Recovery/0.unity (9 file). Các scene khác (7 scene Network*, MainMenu, Sanh_Cho) KHÔNG dính nên an toàn khi xóa.
5. Kích cỡ nhiệm vụ nhỏ hơn phòng hiện tại rất nhiều (mục 2): dựng mới gọn được nhiều.
6. Rủi ro cho P7 (xóa hẳn): scene cũ + 7 bản lưu sẽ hỏng khi xóa code (chủ dự án cần chốt P-009); phải commit/tag git trước (AI không chạy git).

## 1. (a) File code NGOÀI thư mục riêng có nhắc tên 8 nhiệm vụ (41 file)
| File (trong Assets/TheReturn/) | Nhắc tới | Xử lý đề xuất khi xóa (P7) |
|---|---|---|
| Art/Editor/MissionRoomEnvironmentPass.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Art/Editor/SchoolCirculationBuilder.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Art/Editor/SchoolInteriorArtPass.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Art/Editor/UserBlockoutLayoutBuilder.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Features/GameAudio/Runtime/Domain/AudioCues.cs | TeacherSays | nhiều khả năng file riêng của TeacherSays: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Editor/DontLookBackSceneBuilder.cs | DontLookBack | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/GradeRepairSceneBuilder.cs | GradeRepair | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/GradeRepairSpatialPass.cs | GradeRepair, QuietCorridor | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/NavigationExamSceneBuilder.cs | NavigationExam | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/QuietCorridorSceneBuilder.cs | QuietCorridor | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.DontLookBack.cs | DontLookBack | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.Panels.cs | GradeRepair, NavigationExam | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.TeacherTruth.cs | TeacherTruth | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/SchoolFloorObjectsWindow.cs | GradeRepair | nhiều khả năng file riêng của GradeRepair: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Editor/TeacherTruthSceneBuilder.cs | TeacherTruth | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Editor/TheReturn.SchoolFloor.Editor.asmdef | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | bỏ tham chiếu assembly bị xóa |
| Maps/SchoolFloor/Runtime/Audio/MissionAudioDirector.cs | TeacherSays | nhiều khả năng file riêng của TeacherSays: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/GradeRepairHud.cs | GradeRepair | nhiều khả năng file riêng của GradeRepair: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/Missions/GlassBridgeMissionAdapter.cs | GlassBridge | nhiều khả năng file riêng của GlassBridge: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/Missions/MissionAdapters.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Maps/SchoolFloor/Runtime/Missions/MissionRegistryFactory.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack, GlassBridge, SharedLantern, TeacherSays | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Maps/SchoolFloor/Runtime/Missions/SharedLanternMissionAdapter.cs | SharedLantern | nhiều khả năng file riêng của SharedLantern: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/Missions/TeacherSaysMissionAdapter.cs | TeacherSays | nhiều khả năng file riêng của TeacherSays: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/NavigationExamHud.cs | NavigationExam | nhiều khả năng file riêng của NavigationExam: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/QuietCorridorHud.cs | QuietCorridor | nhiều khả năng file riêng của QuietCorridor: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/SchoolFloorFlow.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack, GlassBridge, SharedLantern, TeacherSays | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Maps/SchoolFloor/Runtime/Testing/MissionTestLauncher.cs | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack, GlassBridge, SharedLantern, TeacherSays | kiểm và cắt nhánh các nhiệm vụ bị cắt |
| Maps/SchoolFloor/Runtime/TheReturn.SchoolFloor.asmdef | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack, GlassBridge, SharedLantern, TeacherSays | bỏ tham chiếu assembly bị xóa |
| Maps/SchoolFloor/Runtime/UI/AreaHudProviders.cs | TeacherSays | nhiều khả năng file riêng của TeacherSays: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/UI/LegacyAreaHud.cs | NavigationExam | nhiều khả năng file riêng của NavigationExam: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.cs | NavigationExam | nhiều khả năng file riêng của NavigationExam: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.DontLookBack.cs | DontLookBack | nhiều khả năng file riêng của DontLookBack: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.Puzzles.cs | NavigationExam | nhiều khả năng file riêng của NavigationExam: XÓA hoặc cắt nhánh |
| Maps/SchoolFloor/Tests/Editor/DontLookBackChecks.cs | DontLookBack | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/Editor/GradeRepairChecks.cs | GradeRepair | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/Editor/NavigationExamChecks.cs | GradeRepair, NavigationExam | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/Editor/QuietCorridorChecks.cs | QuietCorridor | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/Editor/TeacherTruthChecks.cs | TeacherTruth | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/Editor/TheReturn.SchoolFloor.Tests.asmdef | GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack | bỏ tham chiếu assembly bị xóa |
| Maps/SchoolFloor/Tests/PlayMode/SchoolFloorWalkPlayTests.cs | QuietCorridor | builder/kiểm riêng của nhiệm vụ cắt: nhiều khả năng XÓA cả file (xem nội dung) |
| Maps/SchoolFloor/Tests/PlayMode/TheReturn.SchoolFloor.PlayTests.asmdef | QuietCorridor | bỏ tham chiếu assembly bị xóa |

## 2. (b) Footprint 4 nhiệm vụ giữ + kích thước phòng hiện tại (m)
| Thành phần | Kích thước đo được | Ghi chú |
|---|---|---|
| Attendance (Puzzle_Attendance) | khoảng 26,0 x 23,3 | 49 đối tượng tên có 'seat'; 13 con trực tiếp; ghế 1 người/ô; đội 2-4 |
| PlankAndKey | khoảng 18,5 x 16,1 | đang nằm trong hành lang Hanh_Lang_6 rộng 59,3 x 18,7: bề sâu hành lang chỉ vừa 18,7 nên sân chơi cần >= khoảng 20 x 20 |
| Choir | khoảng 11,6 x 7,0 | nằm giữa Phong_6 (64,9 x 64,9); nhỏ nhất trong 4 nhiệm vụ |
| BreakerPanel | tấm tường dài khoảng 13,9, dày 0,9, cao 3,9 | công tắc nằm trên một mặt tường; Phong_5 là 47,5 x 47,5 |
| Phong_1 / Phong_6 | 64,9 x 64,9 | rất lớn so với nhiệm vụ |
| Phong_5 | 47,5 x 47,5 | |
| Hanh_Lang_6 | 59,3 x 18,7 | |
| Hanh_Lang_5 | 45,9 x 60,7 (hành lang xoay 90 độ trong vòng) | hết TeacherSays, giữ làm hành lang đi lại (P-006) |
- Đáng chú ý: trong scene cũ Choir, BreakerPanel, PlankAndKey đang là đối tượng LogicOnly (logic + vật tối thiểu), chưa phải khu nghệ thuật hoàn chỉnh; thiết kế mới nên chừa chỗ cho nghệ thuật sau này.
- Gợi ý cho P3a (chưa phải thiết kế): mỗi khu chỉ cần khoảng 25-30 m mỗi chiều (Attendance ~26 x 23 là lớn nhất), hành lang rộng >= 20 m ở chỗ đặt PlankAndKey. So với scene cũ có thể thu bản đồ nhỏ lại nhiều.

## 3. (c) DANH SÁCH XÓA cho P7 (8 nhiệm vụ bị cắt)
| Nhiệm vụ | Thư mục Features | .cs | Dòng | asmdef | prefab |
|---|---|---|---|---|---|
| GradeRepair | Features/GradeRepair | 14 | 2074 | 5 | 2 |
| NavigationExam | Features/NavigationExam | 3 | 292 | 2 | 2 |
| TeacherTruth | Features/TeacherTruth | 5 | 430 | 2 | 3 |
| QuietCorridor | Features/QuietCorridor | 5 | 343 | 2 | 3 |
| DontLookBack | Features/DontLookBack | 3 | 329 | 2 | 3 |
| GlassBridge | Features/GlassBridge | 6 | 948 | 3 | 1 |
| SharedLantern | Features/SharedLantern | 17 | 2772 | 5 | 1 |
| TeacherSays | Features/TeacherSays | 14 | 2259 | 5 | 1 |
| Tổng | 8 thư mục | 67 | 9447 | 26 | 16 |
- Scene tham chiếu script/asmdef của 8 nhiệm vụ (sẽ HỎNG khi xóa): Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity, BeforeNewRooms_20261031_201851, BeforeRegistry_20261015_082336, BeforeSpatialPass_20260929_143309, BeforeTeacherSays_20261018_155142, BeforeWinScreen_20261016_015040, SchoolFloorBeforeOnlineLayer_20261104_1914, SchoolFloorBeforeUserBlockout, và Assets/_Recovery/0.unity.
- Scene KHÔNG dính (giữ): Features/Network/Scenes/{NetworkAttendanceTest, NetworkBreakerTest, NetworkChoirTest, NetworkPartyTest, NetworkPlankTest, NetworkRoomTest, NetworkS1Test}, Maps/MainMenu, Scenes/Maps/Lobby/Sanh_Cho.
- Ngoài ra: file code ở mục (a), prefab/asset/Material nằm trong thư mục riêng (bị xóa theo), tài liệu .ai/SYSTEM_CARDS và .ai/PLANS của 8 nhiệm vụ (giữ trong git nhưng nên đánh dấu CẮT).

## 4. Thứ tự xóa an toàn đề xuất (P7)
0. Chủ dự án xác nhận scene mới chạy được và đã commit/tag git (AI không chạy git).
1. Gỡ scene cũ + 7 bản lưu + _Recovery/0.unity khỏi dự án (hoặc di chuyển ra ngoài thư mục Assets) và khỏi Build Settings.
2. Xóa file builder/migration/kiểm RIÊNG của nhiệm vụ cắt (mục (a), loại 'XÓA').
3. Cắt nhánh nhiệm vụ cắt trong file dùng chung còn lại (Flow, Factory, Launcher, Canvas, âm thanh) + bỏ tham chiếu asmdef.
4. Xóa 8 thư mục Features.
5. Sau MỖI bước: compile sạch + Gauntlet xanh + Console 0 lỗi; ghi LOG. Dừng ngay nếu đỏ.

## 5. Chưa biết / cần làm tiếp
- Chưa đọc từng file ở mục (a) để phân loại chính xác 'xóa cả file' hay 'cắt nhánh': làm ở đầu P7, trước khi xóa.
- Chưa kiểm MissionFlowController/MapProgress/AccessController có phụ thuộc gián tiếp vào kiểu của 8 nhiệm vụ không (chỉ biết không nhắc TÊN): cần thử biên dịch scene mới ở P3b.
- Ghế Attendance chưa xác nhận số đếm bằng tên; thiết kế khu Attendance nên lấy số ghế từ AttendanceRound (đội 2-4).
