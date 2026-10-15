# AGENTS.md — The Return (quy tắc chung cho mọi AI agent)

Unity 6000.3.7f1, URP 17.3, Input System 1.18, Windows. Gốc dự án: `E:\kho model 3D\Project Unity\The Return`.

## Đọc theo thứ tự (dừng khi đã đủ cho task)
1. `.ai/CURRENT.md` — trạng thái và task hiện tại (HOT).
2. `PROJECT_STATE.md` — trạng thái triển khai + quy tắc bảo toàn do chủ dự án duy trì.
3. `.ai/ARCHITECTURE.md`, `.ai/SYSTEMS.md` — chỉ phần liên quan (WARM).
4. README của feature trong `Assets/TheReturn/Features/<Tên>/` và `Assets/TheReturn/README.md`.
Không quét lại toàn dự án. Source code và Unity Editor là nguồn sự thật; file `.ai/` chỉ là bản đồ.

## Quy tắc an toàn (bắt buộc)
- Không commit, push, đổi nhánh, reset, xóa nếu chưa được yêu cầu rõ.
- Không sửa gameplay, thiết kế, blockout của người dùng khi chưa kiểm tra hiện trạng.
- KHÔNG chạy lại các builder cũ (`GradeRepairSpatialPass.Build/FinishSample`, `SchoolLayoutV2Migration.Build`, `UserBlockoutLayoutBuilder`, `MissionRoomEnvironmentPass`, `ClassroomEnvironmentPass`, các builder trong `Art/Editor`): có thể tái tạo bố cục đã bị bác bỏ.
- Trước khi sửa scene: tạo snapshot scene trong cùng thư mục Scenes (theo mẫu `BeforeXxx_yyyyMMdd_HHmmss.unity`).
- Di chuyển asset bằng Unity/AssetDatabase để giữ GUID/.meta. Không chép thư mục Backups vào Assets.
- Không tự sáng tác nhiệm vụ cho phòng 5/6 và hành lang 1/3/5/6.
- Không ghi API key, token, mật khẩu vào bất kỳ file nào.

## Quy ước code
- Domain assembly là C# thuần, không tham chiếu Unity. Core/Shared không tham chiếu ngược Feature.
- Đầu mỗi file C# mới: mô tả mục đích + danh sách hàm. Trước mỗi hàm: summary tiếng Việt (làm gì, nhận gì, trả gì). Cập nhật danh sách đầu file khi thêm/sửa/xóa hàm.
- Chi tiết: `.ai/CONVENTIONS.md`.

## Nghiệm thu (phân biệt rõ từng mức)
File đã tạo / code đã compile / test đã chạy / runtime đã xác nhận / chưa xác nhận được. Không nói đã sửa xong chỉ vì không có lỗi compile. Xem `.ai/TESTING.md`.

## Quản lý ngữ cảnh
HOT = `.ai/CURRENT.md` + file đang sửa. WARM = kiến trúc/README của hệ thống liên quan. COLD = toàn source, Git history, `Design/`, `Backups/`, báo cáo cũ — chỉ truy xuất khi cần.

## Sau mỗi task
Cập nhật `.ai/CURRENT.md`; thêm vào `.ai/TASKS.md`; chỉ ghi `DECISIONS/` hoặc `BUGS/` khi có tri thức lâu dài. Xem `.ai/AI_HANDOFF.md`.

## MenuItem GHI/TÁI TẠO scene — không chạy nếu chưa được yêu cầu rõ (kiểm kê 2026-10-14)
Mọi mục dưới menu `The Return/` NGOẠI TRỪ `School Floor/Objects - Find and Edit` (cửa sổ tìm vật) và `Attendance/Validate Generated Puzzles` (ghi đè một file báo cáo) đều là builder/migration: Art/* (Classroom And Gallery, Mission Rooms, Dress All Interiors, Observatory), School Floor/* (Build Connected Walkways, Apply User Blockout Layout, Add Dont Look Back/Grade Repair/Navigation Exam/Quiet Corridor/Teacher Truth, Spatial Pass 01, Monumental Atrium, Apply Layout V2), Attendance/Rebuild Logic Classroom, Lobby/Rebuild Monumental Lobby, UI/Install School Floor Canvas. Chạy một mục ngoài ý định có thể ghi đè bố cục đã được chủ dự án chấp nhận.

## Skill (chi tiết: `.ai/TOOLING.md`)
Skill nằm ở `.claude/skills/` và `.agents/skills/`. Khi task khớp, chỉ đọc SKILL.md của skill đó: UI/Canvas -> `ui` rồi `ui-ugui`; trigger/collision lỗi -> `physics-3d-collision`; tìm asset trong Editor -> `generate-editor-search-query`; CLI ngoài Editor -> `unity-cli` (chỉ đọc phần cần; không cài CLI/package khi chưa được đồng ý). Không nạp tất cả cùng lúc.
