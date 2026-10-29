# AGENTS.md — The Return (quy tắc chung cho mọi AI agent)

Unity 6000.3.7f1, URP 17.3, Input System 1.18, Windows. Gốc dự án: `E:\kho model 3D\Project Unity\The Return`.

## Bắt đầu phiên — ĐỌC `.ai/START_HERE.md` TRƯỚC (cập nhật 2026-10-21 để giảm token)
Chỉ đọc: START_HERE.md + `.ai/CURRENT.md` + 8 mục cuối `.ai/LOG.md` (≲ 6.000 token). KHÔNG mở cả PLANS/SYSTEM_CARDS/TOOLING/PROTOCOL/MAPS/PROJECT_STATE nếu task không trỏ tới. «Tìm việc rảnh» chỉ cần mục FREE trong START_HERE. LOG cũ ở `.ai/SESSIONS/`. Danh sách cũ bên dưới chỉ để tham khảo.

### (cũ) Danh sách 5 file — chi tiết: `.ai/PROTOCOL.md`
1. `AGENTS.md` (file này). 2. `.ai/CURRENT.md` (trạng thái, cảnh báo). 3. `.ai/LOG.md` — 15 mục cuối. 4. `.ai/OWNER_UPDATES.md` — mục mới hơn mục LOG cuối. 5. `.ai/TASKS.md` — chỉ dòng task của bạn (cột Đọc cho biết file cần mở).
Rồi PREFLIGHT (PROTOCOL mục 3): task đã có người giữ/đã xong chưa, file nào đổi mà chưa ghi (`.ai/tools/changed_since.snippet.cs`, Git nếu có terminal), Console, scene có đang dirty không. Chỉ sau đó mới đọc ngữ cảnh liên quan.
Source code và Unity Editor là nguồn sự thật; file `.ai/` chỉ là bản đồ. `PROJECT_STATE.md` là mô tả triển khai dài của chủ dự án — đọc khi task cần.

## Quy tắc an toàn (bắt buộc)
- Không commit, push, đổi nhánh, reset, xóa nếu chưa được yêu cầu rõ.
- Không sửa gameplay, thiết kế, blockout của người dùng khi chưa kiểm tra hiện trạng.
- KHÔNG chạy lại các builder cũ (`GradeRepairSpatialPass.Build/FinishSample`, `SchoolLayoutV2Migration.Build`, `UserBlockoutLayoutBuilder`, `MissionRoomEnvironmentPass`, `ClassroomEnvironmentPass`, các builder trong `Art/Editor`): có thể tái tạo bố cục đã bị bác bỏ.
- Trước khi sửa scene: tạo snapshot scene trong cùng thư mục Scenes (theo mẫu `BeforeXxx_yyyyMMdd_HHmmss.unity`).
- Di chuyển asset bằng Unity/AssetDatabase để giữ GUID/.meta. Không chép thư mục Backups vào Assets.
- Không tự sáng tác nhiệm vụ cho phòng 5/6 và hành lang 1/3/5/6.
- Không ghi API key, token, mật khẩu vào bất kỳ file nào.

## ĐỔI SỐ HÀNH LANG (chủ dự án đã đổi tên, 2026-10-15)
Tên object trong scene là CHUẨN: Hanh_Lang_k nối Phong_k và Phong_(k+1) (Hanh_Lang_6 nối Phong_6 và Phong_1). QuietCorridor = Hanh_Lang_1, DontLookBack = Hanh_Lang_3; hành lang chưa có nhiệm vụ: Hanh_Lang_2, 4, 5, 6. Tài liệu (PROJECT_STATE, README, Design) đã sửa theo số mới. CÒN số cũ: nhãn `areaName` trên cổng trong scene («Hanh lang 2» = Hanh_Lang_1, «Hanh lang 4» = Hanh_Lang_3) và `Design/Ke-hoach-tai-bo-tri-v2.md` dòng ~79 (`Corridor_02`, kế hoạch hierarchy chưa thấy trong scene). Cụm «nhiệm vụ N» là số thứ tự nhiệm vụ, không phải số hành lang. Chi tiết: `.ai/MAPS/area-adjacency.md`.

## Quy ước code
- Domain assembly là C# thuần, không tham chiếu Unity. Core/Shared không tham chiếu ngược Feature.
- Đầu mỗi file C# mới: mô tả mục đích + danh sách hàm. Trước mỗi hàm: summary tiếng Việt (làm gì, nhận gì, trả gì). Cập nhật danh sách đầu file khi thêm/sửa/xóa hàm.
- Chi tiết: `.ai/CONVENTIONS.md`.

## Nghiệm thu (phân biệt rõ từng mức)
File đã tạo / code đã compile / test đã chạy / runtime đã xác nhận / chưa xác nhận được. Không nói đã sửa xong chỉ vì không có lỗi compile. Xem `.ai/TESTING.md`.

## Quản lý ngữ cảnh
HOT = `.ai/CURRENT.md` + file đang sửa. WARM = kiến trúc/README của hệ thống liên quan. COLD = toàn source, Git history, `Design/`, `Backups/`, báo cáo cũ — chỉ truy xuất khi cần.

## Lập kế hoạch và sửa lỗi (PROTOCOL mục 12-13)
- Nhận task mới (đổi >1 file/scene/hành vi): đọc có giới hạn rồi viết `.ai/PLANS/T-xxx-<slug>.md` theo `_TEMPLATE.md` TRƯỚC khi code; làm từng subtask, báo 1-3 dòng mỗi subtask.
- Gặp lỗi: `.ai/SYSTEM_CARDS/INDEX.md` -> 1 thẻ -> bảng «Triệu chứng -> nơi xem» -> mở đúng file/hàm; sửa xong phải ghi lại dòng mới vào thẻ.

## Trước khi sửa và sau khi làm (bắt buộc, cả khi chưa xong)
- Trước: ghi `START` vào `.ai/LOG.md` + đổi trạng thái dòng task ở `.ai/TASKS.md`, liệt kê file định đụng.
- Sau hoặc khi dừng: LOG (`DONE`/`PAUSED`/`BLOCKED`) + TASKS + CURRENT (nếu đổi trọng tâm) + `DECISIONS/`/`BUGS/` nếu có tri thức lâu dài + báo cáo chat ≤ 8 dòng (PROTOCOL mục 9).
- Việc dở của AI khác: tiếp quản theo PROTOCOL mục 6, không làm trùng, không hoàn tác khi chưa chắc.
- Chủ dự án báo thay đổi ở `.ai/OWNER_UPDATES.md`; nghe họ nói «ghi vào OWNER_UPDATES» thì phải ghi vào file đó.

## MenuItem GHI/TÁI TẠO scene — không chạy nếu chưa được yêu cầu rõ (kiểm kê 2026-10-14)
Mọi mục dưới menu `The Return/` NGOẠI TRỪ `School Floor/Objects - Find and Edit` (cửa sổ tìm vật) và `Attendance/Validate Generated Puzzles` (ghi đè một file báo cáo) đều là builder/migration: Art/* (Classroom And Gallery, Mission Rooms, Dress All Interiors, Observatory), School Floor/* (Build Connected Walkways, Apply User Blockout Layout, Add Dont Look Back/Grade Repair/Navigation Exam/Quiet Corridor/Teacher Truth, Spatial Pass 01, Monumental Atrium, Apply Layout V2), Attendance/Rebuild Logic Classroom, Lobby/Rebuild Monumental Lobby, UI/Install School Floor Canvas. Chạy một mục ngoài ý định có thể ghi đè bố cục đã được chủ dự án chấp nhận.

## Skill (chi tiết: `.ai/TOOLING.md`)
Skill nằm ở `.claude/skills/` và `.agents/skills/`. Khi task khớp, chỉ đọc SKILL.md của skill đó: UI/Canvas -> `ui` rồi `ui-ugui`; trigger/collision lỗi -> `physics-3d-collision`; tìm asset trong Editor -> `generate-editor-search-query`; CLI ngoài Editor -> `unity-cli` (chỉ đọc phần cần; không cài CLI/package khi chưa được đồng ý). Không nạp tất cả cùng lúc.
