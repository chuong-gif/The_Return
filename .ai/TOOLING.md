# TOOLING (khả dụng thực tế, đã kiểm 2026-10-14)

| Loại việc | Công cụ | Trạng thái |
|---|---|---|
| Trạng thái Editor, quét file, đọc/ghi file project | Unity MCP `execute_code` (C# 6 codedom) | Hoạt động [V] |
| Console | Unity MCP `read_console` | Hoạt động [V] (chỉ thấy lỗi nhiễu MCP SkillSync 403) |
| Scene/GameObject/Component/Prefab | Unity MCP (manage_*) | Chưa thử thao tác ghi |
| Chạy test | execute_code gọi `*Checks.Run()`; Test Runner MCP vô dụng vì không có NUnit | Chưa chạy, xem TESTING.md |
| Git (status/diff/log) | Cần terminal/Git server ngoài | KHÔNG chạy được qua execute_code (safety_checks chặn); chỉ đọc được .git/HEAD (nhánh main) |
| Unity CLI / compile ngoài Editor | Terminal | Chưa có quyền truy cập |
| Symbol/usage index | `.ai/MAPS/index-scripts.md` (75 file .cs, 35 script được scene/prefab/asset tham chiếu) | Hoạt động [V]; chạy lại bằng `.ai/tools/rebuild_index.snippet.cs` |

## Skill (cài 2026-10-14 bằng `npx skills add Unity-Technologies/skills`; license Unity Companion License)
Vị trí thật (đã thấy trên đĩa, bản copy, không phải symlink): `.claude/skills/<tên>/` và `.agents/skills/<tên>/` (cùng nội dung). Chưa có skill nào được commit.

| Skill | Dung lượng | Dùng khi | Ghi chú |
|---|---|---|---|
| ui | 7 KB | UI mà chưa nói rõ framework | Router; dự án dùng uGUI |
| ui-ugui | 14 KB | Canvas, RectTransform, Layout Group, prefab UI (`SchoolFloorCanvasPresenter`) | |
| physics-3d-collision | 31 KB | Trigger/collision không bắn, xuyên vật, raycast trượt (cổng `SchoolPreparationGate`, `CharacterController`) | |
| generate-editor-search-query | 18 KB | Tìm asset/GameObject bằng Unity Search | |
| unity-cli | 461 KB (SKILL.md 51 KB) | Chỉ khi cần CLI ngoài Editor (build/test headless, cài Editor) | Gen đánh giá Med Risk; mô tả tự cài CLI bằng lệnh tải-và-chạy script từ unity.com và `unity pipeline install` (thêm package com.unity.pipeline). KHÔNG cài package/CLI nếu chưa được chủ dự án đồng ý. Trùng chức năng một phần với Unity MCP. Chỉ đọc `references/build-run-test.md` khi cần, không nạp cả skill mặc định |

Trạng thái nhận diện:
- Claude Code mở tại gốc dự án: kỳ vọng tự nạp từ `.claude/skills/` [chưa xác minh, chưa thử].
- Phiên Claude hiện tại (chat + Unity MCP): các skill này KHÔNG nằm trong danh sách skill của phiên => không tự nạp [V]. Dùng như tài liệu: đọc `SKILL.md` của đúng skill bằng execute_code khi task khớp, không đọc tất cả.
- Cảnh báo từ trình cài: skill chạy với quyền agent đầy đủ. Mới đọc phần đầu SKILL.md của 5 skill; chưa rà soát toàn bộ file/script, đặc biệt `unity-cli`.
- `skills-lock.json` ở gốc ghi nguồn (Unity-Technologies/skills) và hash cho 5 skill; dùng để biết skill nào được cài và phát hiện thay đổi.
- `find-skills` (vercel-labs) được cài ở mức người dùng `~\.agents\skills\find-skills` (ngoài project), symlink cho Claude Code. Không thuộc project; không cần cho workflow này.
- Chưa khảo sát repo skill ngoài Unity-Technologies; repo có 34 skill, đã bỏ qua 29 skill không liên quan (IAP, ads, UGS, 2D, localization, migrate-birp-to-urp...).

## Index mở rộng (chưa làm)
Gọi hàm, tham chiếu tạo bằng code, vector DB: chưa cần. Cân nhắc Roslyn/Tree-sitter nếu symbol-level search trở thành nút thắt.
