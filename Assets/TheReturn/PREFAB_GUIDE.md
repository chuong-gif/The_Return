# Hướng dẫn dùng prefab tương tác (T-012)

Mục đích: để chủ dự án thay model 3D mà không làm hỏng logic. Mỗi prefab có: nơi dùng, số instance, script gốc, mốc bắt buộc, collider, phần thay được và lưu ý.

Nguồn: đọc trực tiếp từng prefab và scene `SchoolFloorPrototype` ngày 2026-10-15 (chỉ đọc, không sửa gì). Cấu trúc, field và số instance: mức xác minh 4. Hành vi runtime: mức 2 (đọc code, chưa chạy Play Mode). Giá trị scale/vị trí là của prefab nguồn; instance trong scene có thể có override.

## 1. Quy tắc chung khi thay model

1. Thay hình ở object visual (`Visual_Replaceable` hoặc `VisualRoot`). Không xóa collider, mốc (`SitPoint`, `StandPoint`...), `TextMesh` nhãn hay component trên root.
2. Chỉ gán lại field khi bạn xóa object mà field đang trỏ tới (ví dụ `indicator`, `label`).
3. Collider tương tác phải nằm TRONG cây của root. Code tìm script bằng `GetComponentInParent` hoặc `IsChildOf(root)`: collider ngoài cây không bấm được và còn bị coi là vật cản.
4. Tầm với: mắt = vị trí nhân vật + 1.5 m lên (Sửa bảng điểm, Sa bàn, Giữ trật tự) hoặc vị trí camera (Không được quay đầu). Điểm đích = vị trí PIVOT của root. Tia từ mắt tới pivot dùng `Physics.RaycastAll` (bỏ qua trigger); collider nào ngoài cây root chắn giữa sẽ làm tương tác thất bại. Nên: pivot ở chỗ nhìn thấy được, không để pivot sau/trong vật khác, không thêm collider lớn bao quanh.
5. Scale thừa kế: `Visual_Replaceable` (terminal, trạm) và `Panel` (cửa) có scale khác 1; con của chúng bị méo theo. Model mới đặt dưới đó phải bù scale (1 / scale cha) hoặc đặt ở object anh em có scale 1.
6. Đèn trạng thái đổi màu bằng `MaterialPropertyBlock` thuộc tính `_BaseColor`: renderer `indicator` phải dùng shader có `_BaseColor` (URP Lit/Unlit có).
7. Nhãn chữ là `TextMesh`, không phải TextMeshPro. Đổi sang TMP phải sửa code.
8. Prefab nguồn không giữ reference tới scene. Thêm instance mới = gán vào mảng/field của controller trong scene, đúng thứ tự ID.
9. Thử lại bằng `MissionTestLauncher` (số nhiệm vụ ở README từng nhiệm vụ). Bộ `*Checks` cũ đã lỗi thời, không dùng làm bằng chứng.

## 2. Prefab theo nhiệm vụ

### `PF_AttendanceSeat` — Điểm danh (Phong_1)
- Instance: 24, `Puzzle_Attendance/Seating_6x4/H<hàng>-C<cột>` (H1-C1 ... H6-C4). Thứ tự mảng `AttendancePrototype.seats` là ID ghế 0-23; giữ nguyên thứ tự.
- Root: `AttendanceSeat` với `index`, `sitPoint`, `standPoint`, `label`, `indicator`.
- Mốc bắt buộc: `SitPoint` (local 0, 0.04, -0.25) và `StandPoint` (0, 0.04, -1.10) là Transform rỗng; `StatusPlate` là renderer `indicator` (có BoxCollider); `SeatAddress` là `TextMesh` `label` (mã ghế và tên người ngồi).
- Thay được: toàn bộ `Visual_Replaceable` (Desk, DeskSupport, ChairSeat, ChairBack, ChairSupport: 5 khối có BoxCollider).
- Tương tác: `AttendancePrototype` raycast rồi `hit.collider.GetComponentInParent<AttendanceSeat>()`. Cần ít nhất một collider dưới root.
- Lưu ý: dời ghế thì dời cả root để mốc đi theo. `PF_GB_DeskChair` là mẫu cũ không có logic, không thay cho ghế này.

### `PF_GradeTerminal` — Sửa bảng điểm (Phong_2)
- Instance: 5 trong `GradeRepair_LogicOnly`: `GradeBoard` (`board`) và `Document_1..4` (mảng `documents`).
- Root: `GradeRepairTerminal` với `document` (-1 là bảng nộp; 0-3 là tài liệu, khớp thứ tự mảng) và `label`.
- Cấu trúc: `Visual_Replaceable` (scale 1.5 × 0.85 × 0.15, BoxCollider, renderer đã TẮT) chứa `VisualRoot` (renderer BẬT, M_GB_Board); `Label` là `TextMesh` (local z -0.09).
- Thay được: mesh trong `VisualRoot`. Giữ BoxCollider ở `Visual_Replaceable` (collider tương tác). `VisualRoot` thừa kế scale cha nên phải bù scale khi nhập model.
- Tầm: ray ngắm 3 m; `CanReach` tối đa 3.2 m tới pivot root.

### `PF_NavigationBoard` — Bài kiểm tra (Phong_3)
- Instance: 1, `NavigationExam_LogicOnly/PF_NavigationBoard`.
- Root KHÔNG có script. `NavigationExamPrototype` giữ: `board` = root, `pawn` = `Pawn`, `cells[9]` = `Cell_0..Cell_8` (đúng thứ tự), `boardLabel` = `Instruction` (`TextMesh`).
- Lưới 3×3 cách 1.2 m (x, z local): Cell_0 (-1.2, +1.2), Cell_1 (0, +1.2), Cell_2 (+1.2, +1.2), Cell_3 (-1.2, 0), Cell_4 (0, 0), Cell_5 (+1.2, 0), Cell_6 (-1.2, -1.2), Cell_7 (0, -1.2), Cell_8 (+1.2, -1.2). `Address_k` là nhãn `TextMesh` trên mỗi ô. Code đặt `Pawn` tại `cells[vị trí]` cộng 0.17 m lên.
- Thay được: hình `Table`, `Support` (nhưng `Table` là collider mặt bàn). KHÔNG đổi tên, thứ tự, tọa độ `Cell_k`, `Pawn`, `Instruction`.
- Tương tác: ngắm 3.2 m và `hit.transform.IsChildOf(board)`, nên mọi collider dưới root đều tính; `CanReach` tối đa 3.2 m tới pivot root (tâm bàn).
- `Address_k`: chưa thấy field nào trỏ tới (mức 2); cứ giữ nguyên.

### `PF_QuietHoldStation` — Giữ trật tự (Hanh_Lang_1)
- Instance: 2, `Station_A_Near` và `Station_B_Far` (mảng `stations`); `index` 0 là A, 1 là B.
- Root: `QuietHoldStation` với `index`, `indicator` (chính là renderer của `Visual_Replaceable`), `label` (`Label`, local z -0.12).
- `Visual_Replaceable` (0.75 × 0.85 × 0.20) vừa là BoxCollider tương tác vừa là đèn trạng thái (cam khi rảnh, xanh khi đang giữ).
- Thay model: giữ BoxCollider; nếu thay/xóa renderer thì gán lại `indicator`.
- Tầm: ngắm 2.6 m; giữ nút cần tối đa 2.8 m từ mắt tới pivot root và không bị chắn. Khoảng cách A-B ảnh hưởng độ khó đội 2 người.

### `PF_QuietSpeaker` — Loa (Hanh_Lang_1)
- Instance: 1, `QuietCorridor_LogicOnly/Speaker` (là `speakerIndicator` của controller).
- Root mang tất cả: BoxCollider, MeshRenderer, AudioSource, `QuietSpeakerFeedback` (`session`, `replacementClip`, `volume` 0.08).
- CẢNH BÁO: transform gốc của prefab không mặc định: vị trí (5.8, 2.7, 19), scale (0.8, 0.5, 0.35). Kéo instance mới vào scene phải đặt lại vị trí; scale không đều làm méo model con.
- `session` trống trong prefab: phải gán `QuietCorridorPrototype`. `QuietSpeakerFeedback.Update` đọc `session` không kiểm null nên bỏ trống sẽ lỗi.
- AudioSource bị code cấu hình lại lúc Awake (loop, 3D 0.8, 4-35 m, tắt playOnAwake): chỉnh trong prefab sẽ bị ghi đè. Muốn đổi tiếng hãy gán `replacementClip`.
- Nếu thay renderer, gán lại `speakerIndicator` trên controller.

### `PF_TeacherChannel` — Giáo viên nói thật/nói dối (Phong_4)
- Instance: 4, `TeacherChannel_1..4` (mảng `teachers`). Root: `TeacherChannelSource` chỉ có `channel` 0-3, khớp thứ tự mảng.
- Cấu trúc: `Pedestal` (collider), `Visual_Replaceable` (0.7 × 1.3 × 0.5 tại y 1.15, collider, thân), `Head` (0.55 tại y 2.05, collider), `ChannelLabel` (`TextMesh`, y 2.85).
- Tầm: điểm ngắm là pivot root cộng 1.3 m lên, tối đa 4 m. Giữ pivot ở chân tượng và chiều cao tương tự; vẫn cần collider để raycast trúng.

### `PF_ManualConfirmStation` — Trạm xác nhận (Phong_4)
- Instance: 4, `ManualConfirm_1..4` (mảng `stations`). Root: `ManualConfirmStation` với `channel` 0-3 và `label`.
- Cấu trúc: `Base` (1.5 × 1.2 × 0.35, collider), `Visual_Replaceable` (0.65 × 0.45 × 0.25 tại z -0.28, collider, là nút bấm), `Label` (`TextMesh`, y 0.9).
- Tầm: tối đa 3.2 m tới pivot root. Không dời pivot xa model; không để bốn trạm cùng `channel`.

### `PF_BackCard` — Không được quay đầu (Hanh_Lang_3)
- Instance: 4, `BackCard_1..4` (mảng `badges`). Gắn theo nhân vật; code bật/tắt theo vai (không hiện thẻ của người đang điều khiển).
- Không script, không collider. Hai `TextMesh` con: `BackSymbol` (z -0.38, sau lưng; được `backLabels[]` tham chiếu) và `FrontNumber` (z +0.38, mặt trước).
- PHỤ THUỘC THEO TÊN: code gọi `badges[i].Find("FrontNumber")`. KHÔNG đổi tên `FrontNumber` và giữ nó là con TRỰC TIẾP của root. Đây là phụ thuộc theo tên duy nhất trong code runtime.
- `BackSymbol` phải còn Renderer vì code dùng `GetComponent<Renderer>()` để bật/tắt.
- Model thẻ ở sau lưng đúng hướng, không che camera.

### `PF_BackLockTerminal` — Bảng khóa (Hanh_Lang_3)
- Instance: 2, `LockTerminal_0` và `LockTerminal_1` (mảng `terminals`). Không script.
- Cấu trúc: `Visual_Replaceable` (1.5 × 0.8 × 0.16, collider) và `Label` (`TextMesh`, z -0.09).
- Controller dùng vị trí pivot root: tối đa 3.2 m từ camera, góc nhìn tối đa 80 độ, không bị collider ngoài cây root chắn.

## 3. Prefab dùng chung (Maps/SchoolFloor, Shared)

### `PF_PreparationGate` — Cổng chuẩn bị
- Instance: 22 trong `SchoolLayoutV2`: Phòng 1-6 mỗi phòng 3 cửa (`Phong k_Cua 1..3`, 18 cái) và `Hanh lang 2_Vao/Ra`, `Hanh lang 4_Vao/Ra` (4 cái).
- Root: `SchoolPreparationGate` với `mission` (-1 là khu chưa có nhiệm vụ, luôn mở; 0-5 theo thứ tự nhiệm vụ), `designatedEntrance` (true là cửa chính, dùng đếm tập hợp), `blocker`, `preparationZone`, `visualRoot`, `statusLabel`, `areaName`.
- Ba phần tách rời: `Interaction_Blocker` (BoxCollider, bật khi cửa đóng), `PreparationZone` (BoxCollider trigger, đếm người theo tọa độ LOCAL nên xoay được), `VisualRoot` (bị `SetActive(false)` khi cửa mở). `Status` là `TextMesh` (y 3.3).
- Thay model: chỉ đặt hình trong `VisualRoot`; đừng để collider hay logic ở đó vì cả cây bị tắt khi mở cửa. Kích thước collider/zone không tự theo model: chỉnh scale của `Interaction_Blocker` và `PreparationZone` riêng.
- `mission`: 0 Điểm danh (Phong_1), 1 Giữ trật tự, 2 Sửa bảng điểm (Phong_2), 3 Bài kiểm tra (Phong_3), 4 Không được quay đầu, 5 Giáo viên nói thật (Phong_4); Phong 5 và 6 là -1.
- Nhãn `areaName` cũ: «Hanh lang 2» (mission 1) thực ra là Hanh_Lang_1 và «Hanh lang 4» (mission 4) là Hanh_Lang_3 (xem AGENTS.md; chủ dự án tự sửa nhãn). Chữ hiển thị trên cổng không dấu.

### `PF_GB_SlidingDoor` — Cửa trượt
- Instance: 7: `Puzzle_Attendance/ExitDoor`, `QuietCorridor_LogicOnly/AcousticGate`, `GradeRepair_LogicOnly/GradeRoom_Entrance` và `GradeRoom_Exit`, `NavigationExam_LogicOnly/ExamExit`, `DontLookBack_LogicOnly/DontLookBackExit`, `TeacherTruth_LogicOnly/SchoolFloorExit`. Hiện chỉ `AcousticGate` đang bật; 6 cửa còn lại đang ẩn nhưng vẫn là reference runtime, đừng xóa.
- Root: `PuzzleDoor` với `panel`, `openOffset` (0, 3.4, 0), `speed` 1.2. Đóng là vị trí local của `Panel` lúc khởi tạo; mở là cộng `openOffset`.
- Cấu trúc: `Panel` (scale 1.55 × 2.9 × 0.18, BoxCollider, renderer TẮT) chứa `VisualRoot` (renderer BẬT, M_GB_Interaction).
- Thay model: trong `VisualRoot`, bù scale cha. Cửa cao hơn 2.9 m hoặc trượt hướng khác thì chỉnh `openOffset` trên từng instance.

## 4. Không thuộc tài liệu này
- `PF_GB_DeskChair`: mẫu cũ không script (ghế logic là `PF_AttendanceSeat`). `PF_GB_Block`: khối xám tĩnh, không phải prefab tương tác.
- `PF_SchoolFloorCanvas`, `PF_UI_Button`, `PF_UI_Panel`: UI, thuộc T-011 và skill `ui-ugui`.

## 5. Chưa xác minh
- Chưa chạy Play Mode; chưa thử thay model thật rồi tương tác.
- Tầm tương tác của ghế điểm danh chưa đọc (xem `AttendancePrototype` quanh dòng 243).
- Chưa đối chiếu override của từng instance trong scene với prefab nguồn.
- Chưa kiểm shader/material của model thật có `_BaseColor`.

## Nhiệm vụ mới (6 nhiệm vụ dựng bằng khối cơ bản trong scene, CHƯA là prefab)

Sáu nhiệm vụ làm sau (T-006) nằm trực tiếp trong scene `SchoolFloorPrototype` dưới các object gốc `*_LogicOnly`, ghép từ cube/capsule/TextMesh, nên hiện không có prefab để thay. Mỗi nhiệm vụ có README riêng ghi luật, cấu trúc object, phần PHẢI giữ và phần THAY ĐƯỢC khi đổi asset 3D:

| Nhiệm vụ | Khu | Object gốc | Tài liệu |
|---|---|---|---|
| BreakerPanel (bảng cầu dao) | Phong_5 | `Breaker_LogicOnly` | `Features/BreakerPanel/README.md` |
| Choir (hợp xướng) | Phong_6 | `Choir_LogicOnly` | `Features/Choir/README.md` |
| GlassBridge (cầu kính) | Hanh_Lang_2 | `GlassBridge_LogicOnly` | `Features/GlassBridge/README.md` |
| SharedLantern (đèn lồng chung) | Hanh_Lang_4 | `SharedLantern_LogicOnly` | `Features/SharedLantern/README.md` |
| TeacherSays (cô nói) | Hanh_Lang_5 | `TeacherSays_LogicOnly` | `Features/TeacherSays/README.md` |
| PlankAndKey (cầu ván và thẻ từ) | Hanh_Lang_6 | `PlankAndKey_LogicOnly` | `Features/PlankAndKey/README.md` |

**Cách đổi asset 3D khi chưa có prefab:** mở scene, chọn đối tượng cần đổi, thay mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. KHÔNG xóa object có script hoặc collider, KHÔNG đổi tên object, KHÔNG đổi thứ tự các mảng đã gán trên prototype (`switches`, `pads`, `tiles`, `signs`, `segmentZones`...). Các điểm chỉ có Transform (`Checkpoint*`, `StartPoint*`, `Anchor*`...) là mốc logic: chỉ dời khi biết rõ tác dụng.

**Nếu muốn chuyển thành prefab** (đúng quy tắc: vật có tương tác/logic nhiệm vụ mới làm prefab): đây là việc riêng cần Scene lock và snapshot scene; các vật nên làm prefab trước là công tắc/đèn (BreakerPanel), bục hợp xướng (Choir), ô kính (GlassBridge), biển và phím bảng (SharedLantern), cần gạt/thẻ (PlankAndKey). Chưa làm.
