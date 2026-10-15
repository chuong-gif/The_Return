# LOG — nhật ký sự kiện (CHỈ THÊM, mới nhất ở CUỐI)

Mỗi mục 3-6 dòng, giờ UTC. Mẫu:
`### 2026-10-15 03:10 | <AI/model hoặc OWNER> | <LOẠI> | <Task ID>`
`- Làm gì / kết quả (ngắn).`
`- Files: ...  | Xác minh: mức 1-5 (nêu bằng chứng)`
`- Dở/Tiếp theo: ...  | Cần chủ dự án: ...`
LOẠI: START, PROGRESS, DONE, PAUSED, BLOCKED, TAKEOVER, ALREADY-DONE, UNLOGGED-CHANGE, OWNER-CHANGE-PROCESSED, TEST-RUN, DECISION, NOTE.
Quy tắc: không sửa/xóa mục cũ; sai thì thêm mục mới đính chính. Khi file quá dài (>~300 dòng), AI cuối cùng gom mục cũ thành bản tóm tắt ở `.ai/SESSIONS/` rồi giữ lại 30 mục gần nhất.

---

### 2026-10-14 ~17:30 | Claude (chat + Unity MCP) | DONE | T-000
- Khảo sát dự án (Unity 6000.3.7f1, 21 asmdef, 75 file .cs) và thiết lập bộ memory: AGENTS.md, CLAUDE.md, .ai/*, index script/symbol (.ai/MAPS/index-scripts.md) + script tạo lại.
- Files: AGENTS.md, CLAUDE.md, .ai/* (mới) | Xác minh: mức 1-2 (file tạo; Console 0 lỗi CS sau đó). Chưa đổi scene/gameplay.
- Chủ dự án tự chạy `npx skills add`: 5 skill vào .claude/skills và .agents/skills (xem TOOLING.md).

### 2026-10-15 ~01:10 | Claude (chat + Unity MCP) | TEST-RUN | T-000
- Chạy thử QuietCorridorChecks.Run() trong Play Mode: THẤT BẠI ở 'Scene 2: same scene transition'; chủ dự án xác nhận là test lỗi thời sau Layout V2 (BUGS/quiet-corridor-check-stale-after-layout-v2.md). Thoát Play không lưu; Design/Verification/QuietCorridor.json không bị ghi.
- Xác minh: mức 3 (test chạy, kết quả fail có nguyên nhân). Bốn bộ Checks còn lại chưa chạy.

### 2026-10-15 ~01:51 | Claude (chat + Unity MCP) | DECISION | T-001
- Chốt yêu cầu luồng tự chọn phòng (DECISIONS/2026-10-14-nonlinear-mission-selection.md) và viết kế hoạch (PLANS/nonlinear-mission-flow.md). Chưa viết code gameplay.
- Cần chủ dự án: duyệt kế hoạch trước S1.

### 2026-10-15 01:59 | (không rõ — nghi là chủ dự án) | UNLOGGED-CHANGE | -
- `Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity` có mtime 01:59 UTC, sau mọi lần ghi của Claude (cuối 01:51); khi Claude kiểm tra trước đó scene không bị đánh dấu dirty. `.gitignore` đổi lúc 2026-10-14 17:24 (không phải Claude). Chưa đọc nội dung thay đổi.
- Chủ dự án nói đã đặt tên phòng/hành lang trong object BlockoutCampus_Generated — có thể liên quan. AI nào làm T-001 phải đọc scene hiện tại, không dùng giả định cũ.

### 2026-10-15 02:15 | Claude (chat + Unity MCP) | START | T-001
- Trích bảng kề phòng/hành lang + gate từ scene đang mở (chỉ đọc). Sẽ tạo .ai/MAPS/area-adjacency.md; không sửa scene/prefab/code.
- Files định đụng: .ai/MAPS/area-adjacency.md (mới), .ai/TASKS.md, .ai/LOG.md, .ai/CURRENT.md

### 2026-10-15 06:06 | Claude (chat + Unity MCP) | DONE | T-001
- Đọc scene (chỉ đọc) và tạo .ai/MAPS/area-adjacency.md: vòng tròn Phong/Hanh_Lang, bảng kề, hướng 3 cổng mỗi phòng, ánh xạ 6 nhiệm vụ -> khu thực. Cập nhật MISSIONS_TODO.md sang tên object.
- Files: .ai/MAPS/area-adjacency.md (mới), .ai/MISSIONS_TODO.md, TASKS.md, CURRENT.md | Xác minh: mức 4 cho vị trí/kề đo từ scene đang mở; hướng cổng 'Cua 3 = sân' chỉ suy luận. Scene/prefab/code không đổi (scene không dirty).
- Phát hiện: nhãn cổng 'Hanh lang 2/4' khác tên object Hanh_Lang_1/3; root prefab nhiệm vụ không khớp khu của cổng.
- Cần chủ dự án: xác nhận đánh số chuẩn (tên object?) và duyệt kế hoạch để bắt đầu T-002.

### 2026-10-15 06:22 | Claude (chat + Unity MCP) | DONE | T-000 (bổ sung quy trình)
- Xử lý OWNER_UPDATES (đổi tên hành lang): area-adjacency mục 4 đã chốt. T-001 -> DONE.
- Thêm quy trình lập kế hoạch cho task (PROTOCOL 12), xử lý lỗi (PROTOCOL 13), mẫu PLANS/_TEMPLATE.md, SYSTEM_CARDS (9 thẻ + INDEX; chỉ SchoolFloor và QuietCorridor có bảng triệu chứng thật).
- Xác minh: mức 1-2 (file tạo, không đổi code/scene). Quy trình CHƯA được thử bởi AI mới.

### 2026-10-15 06:31 | Claude (chat + Unity MCP) | START | T-014
- Khảo sát hiện trạng 6 nhiệm vụ (Attendance, GradeRepair, NavigationExam, TeacherTruth, QuietCorridor, DontLookBack) từ README, code, prefab, scene để ghi vào SYSTEM_CARDS (phần B/C). Chỉ đọc code/scene; chỉ ghi .ai/.
- Files định đụng: .ai/SYSTEM_CARDS/*.md, .ai/TASKS.md, .ai/LOG.md

### 2026-10-15 06:33 | Claude (chat + Unity MCP) | DONE | T-014
- Đọc README 6 nhiệm vụ + PROJECT_STATE, toàn bộ file Domain của 6 nhiệm vụ, reflection field/method của 6 prototype trong scene, Design/Verification/*.json (kèm ngày). Ghi mục B/C vào SYSTEM_CARDS 6 nhiệm vụ.
- Mức xác minh: 2-3 (đọc code/dữ liệu; KHÔNG chạy lại test). Presentation chỉ xem qua tên field/method và README, chưa đọc từng dòng. Mục B suy từ code, chưa có lỗi thật để xác nhận (trừ QuietCorridor hàng 1).
- Phát hiện: tài liệu cũ dùng số hành lang cũ (đã ghi bảng đổi số trong AGENTS.md); bằng chứng test nhiệm vụ đều có trước Layout V2 (09-14 đến 09-26).
- Cần chủ dự án: cho phép sửa số hành lang trong README QuietCorridor/DontLookBack, PROJECT_STATE.md và Design/ cho khớp tên mới (tôi chưa sửa).

### 2026-10-15 06:37 | Claude (chat + Unity MCP) | START | T-015
- Chủ dự án cho phép sửa số hành lang CŨ -> tên mới trong PROJECT_STATE.md, README (Features/*, Assets/TheReturn) và Design/*.md. Quy đổi cũ->mới: 1->6, 2->1, 3->2, 4->3, 5->4, 6->5. Sẽ kiểm từng chỗ theo ngữ cảnh, chỉ sửa chữ số, không sửa nội dung khác.
- Files định đụng: PROJECT_STATE.md, Assets/TheReturn/**/README.md, Design/*.md (chỉ phần có số hành lang)

### 2026-10-15 06:38 | Claude (chat + Unity MCP) | DONE | T-015
- Sửa 14 chỗ số hành lang cũ -> mới trong 7 file tài liệu của chủ dự án (có ghi chú cuối file cho 3 file Design/PROJECT_STATE). Chỉ đổi số, giữ BOM/kiểu xuống dòng. Cập nhật 6 thẻ + AGENTS.md.
- Không sửa: nhãn cổng trong scene (areaName), `Corridor_02` ở Ke-hoach-tai-bo-tri-v2.md L79 (không rõ có áp dụng không), câu «Phong_1–Phong_6 lần lượt chứa [6 nhiệm vụ]» ở Bo-cuc-theo-blockout.md L7 (nội dung cũ không khớp thực tế: 2 nhiệm vụ nằm ở hành lang) — báo chủ dự án.
- Xác minh: mức 2 (quét lại bằng regex: không còn số hành lang cũ trong các chỗ đã sửa). Git sẽ thấy 7 file đã sửa; chưa commit.

### 2026-10-15 07:29 | Claude Sonnet 5.5 (chat + Unity MCP) | START | T-012
- Nhan T-012: viet tai lieu cach dung cho tung prefab tuong tac (moc tuong tac, collider, ID, child thay model). Chi doc prefab/scene (read-only), chi ghi tai lieu; KHONG sua scene/prefab/code.
- Files dinh dung: .ai/PLANS/T-012-prefab-usage-docs.md (moi), .ai/TASKS.md, .ai/LOG.md, README feature lien quan (chi THEM muc 'Prefab' o cuoi file), .ai/CURRENT.md

### 2026-10-15 07:40 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #2) | START | T-013
- Nhan T-013: ra soat skill unity-cli (chi doc SKILL.md, SECURITY.md, references; doi chieu hash trong skills-lock.json; so .claude vs .agents). Chi ghi ket qua vao .ai/TOOLING.md. KHONG chay lenh CLI, KHONG cai package/CLI, KHONG sua scene/prefab/code.
- PREFLIGHT: Console 0 loi/canh bao; khong Play Mode; scene khong dirty. Luu y: T-012 dang IN-PROGRESS do 'Claude Sonnet 5.5' (phien khac) giu tu 07:29, cap nhat cuoi 07:34 (PREFAB_GUIDE.md + PLANS/T-012 da ghi XONG nhung README/TASKS/LOG/CURRENT chua duoc cap nhat) -> toi KHONG dung T-012 de tranh lam trung.
- Files dinh dung: .ai/TOOLING.md, .ai/TASKS.md, .ai/LOG.md

### 2026-10-15 07:46 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #3) | START | T-002
- Chủ dự án DUYỆT kế hoạch PLANS/nonlinear-mission-flow.md trong chat (2026-10-15) -> mở T-002 (S1 của kế hoạch): `MapProgress` (C# thuần) + NUnit EditMode test, CHƯA nối scene.
- PREFLIGHT: Console 0 lỗi/cảnh báo; không Play Mode; scene SchoolFloorPrototype không dirty; Scene lock trống; T-002 chưa ai giữ. T-012/T-013 do phiên khác giữ - không đụng.
- Files định đụng: Assets/TheReturn/Maps/SchoolFloor/Runtime/Domain/ (asmdef + MapProgress.cs, MỚI), Assets/TheReturn/Maps/SchoolFloor/Tests/EditMode/ (asmdef + test, MỚI), .ai/PLANS/T-002-map-progress.md (MỚI), .ai/TASKS.md, .ai/LOG.md, .ai/CURRENT.md, SYSTEM_CARDS/SchoolFloor.md, MAPS/asmdef-graph.md. KHÔNG sửa scene/prefab/code hiện có.

### 2026-10-15 07:52 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #3) | DONE | T-002
- Tạo `MapProgress` (C# thuần): vòng Phong/Hanh_Lang qua `CreateRing`, `CheckBegin`/`TryBegin` (một khu đang làm; hành lang chỉ vào từ 2 phòng kề; phòng vào từ sân hoặc hành lang kề; đội phải đủ), `Complete`, `Abandon`, `Reset`, `Won`, `MissingMissionAreas`, snapshot Serializable. Tạo 21 test NUnit EditMode.
- Files MỚI: Assets/TheReturn/Maps/SchoolFloor/Runtime/Domain/{TheReturn.SchoolFloor.Domain.asmdef, MapProgress.cs}; Assets/TheReturn/Maps/SchoolFloor/Tests/EditMode/{TheReturn.SchoolFloor.Domain.Tests.asmdef, MapProgressTests.cs}; .ai/PLANS/T-002-map-progress.md. Sửa .ai: TASKS, CURRENT, SYSTEM_CARDS/SchoolFloor.md, MAPS/asmdef-graph.md, TESTING.md. KHÔNG đụng scene/prefab/code cũ.
- Xác minh: MỨC 3. Console 0 lỗi/cảnh báo sau compile; run_tests EditMode 21/21 passed (0,63 s). Scene SchoolFloorPrototype không dirty. CHƯA nối vào scene/gameplay nên chưa có xác nhận runtime (mức 4). `index-scripts.md` (tự sinh) chưa dựng lại cho 2 script mới.
- Giả định cần chủ dự án xác nhận khi nối scene: A1 phòng vào được từ sân hoặc hành lang kề; A2 `Abandon` giữ nguyên tiến độ khu khác (câu hỏi mở: rời khu giữa chừng giữ hay reset); A3 khu chưa có nhiệm vụ không bắt đầu được và tự vào điều kiện thắng khi đăng ký nhiệm vụ (theo DECISIONS mục 5).
- Bước kế tiếp: T-003 (adapter 6 nhiệm vụ + registry, chưa đổi hành vi; cần PLANS/T-003-* riêng). T-004 cần Scene lock + snapshot scene.

### 2026-10-15 07:59 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #3) | START | T-003
- OWNER-CHANGE-PROCESSED: chủ dự án xác nhận A1-A3 của T-002 (ghi OWNER_UPDATES + DECISIONS mục 8-10) và cho làm tiếp T-003. Chủ dự án yêu cầu: ghi tiến độ vào .ai trước khi hết hạn mức token 5h để AI khác tiếp quản -> mỗi subtask xong ghi LOG `PROGRESS` + cập nhật CURRENT.
- PREFLIGHT: Console 0 lỗi; không Play Mode; LOG không có mục lạ sau T-002; Scene lock trống; T-003 chưa ai giữ (T-012/T-013 do phiên khác).
- Files định đụng (chỉ TẠO MỚI): Runtime/Domain/{IMissionAdapter.cs, MissionRegistry.cs}, Tests/EditMode/MissionRegistryTests.cs, Maps/SchoolFloor/Runtime/Missions/*.cs (6 adapter + factory), .ai/PLANS/T-003-mission-adapters.md; sửa .ai: TASKS, LOG, CURRENT, SYSTEM_CARDS/SchoolFloor.md. KHÔNG sửa scene/prefab/SchoolFloorFlow/6 nhiệm vụ.

### 2026-10-15 08:02 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #3) | PROGRESS | T-003
- S1+S2 xong: tạo Runtime/Domain/IMissionAdapter.cs, MissionRegistry.cs, Tests/EditMode/MissionRegistryTests.cs. Mức 3: run_tests EditMode 34/34 passed, Console 0 lỗi. Chưa đụng scene/code cũ.
- Còn lại: S3 (6 adapter thật + MissionRegistryFactory trong Maps/SchoolFloor/Runtime/Missions, mức 2 compile) và S4 (bàn giao). Nếu bị ngắt: đọc PLANS/T-003-mission-adapters.md mục 4, kiểm thư mục Runtime/Missions xem đã có file nào rồi làm tiếp.

### 2026-10-15 08:05 | Claude Sonnet 5.5 (chat + Unity MCP, phiên #3) | DONE | T-003
- Tạo: Runtime/Domain/{IMissionAdapter.cs, MissionRegistry.cs}; Tests/EditMode/MissionRegistryTests.cs (13 test); Maps/SchoolFloor/Runtime/Missions/{MissionAdapters.cs (6 adapter), MissionRegistryFactory.cs}; .ai/PLANS/T-003-mission-adapters.md, T-004-wire-registry.md (nháp). SỬA 1 file có sẵn: Maps/SchoolFloor/Runtime/TheReturn.SchoolFloor.asmdef (thêm tham chiếu TheReturn.SchoolFloor.Domain, 1 dòng).
- Xác minh: MỨC 3 cho MissionRegistry (34/34 test xanh, Console 0 lỗi); MỨC 2 cho 6 adapter + factory (compile, chưa chạy trong scene). Scene không dirty, không Play Mode, không đụng SchoolFloorFlow/6 nhiệm vụ/prefab.
- Ghi chú cho T-004: Attendance chạy từ đầu scene (Begun do UI/khởi động; flow bàn giao bằng ExternalControl + tắt attendanceHud). GradeRepair/NavigationExam chỉ bàn giao khi !PanelOpen. Bàn giao hiện tại còn điều kiện VỊ TRÍ (cả đội ở transitionGatherZones, Attendance còn cần staging zone + chỗ ngồi): đó là Presentation, thuộc T-004, registry chỉ nhận cờ teamReady.
- index-scripts.md (tự sinh) chưa dựng lại cho 5 script mới. Bước kế: T-004 cần chủ dự án duyệt nháp kế hoạch (đổi gameplay/scene) trước khi code.
