# TOOLING (khả dụng thực tế, đã kiểm 2026-10-14; bổ sung 2026-10-16)

| Loại việc | Công cụ | Trạng thái |
|---|---|---|
| Trạng thái Editor, quét file, đọc/ghi file project | Unity MCP `execute_code` (C# 6 codedom) | Hoạt động [V] |
| Console | Unity MCP `read_console` | Hoạt động [V] (2026-10-14: chỉ thấy lỗi nhiễu MCP SkillSync 403; 2026-10-16: 0 lỗi, 0 cảnh báo) |
| Scene/GameObject/Component/Prefab | Unity MCP (manage_*) | Đọc: `manage_scene` get_active và `manage_asset` search chạy được [V]. Thao tác ghi: chưa thử. Các manage_* còn lại: chưa nạp/chưa gọi |
| Chạy test | execute_code gọi `*Checks.Run()`; Test Runner MCP vô dụng vì không có NUnit | Chưa chạy, xem TESTING.md |
| Git (status/diff/log) | Cần terminal/Git server ngoài | KHÔNG chạy được qua execute_code (safety_checks chặn); chỉ đọc được .git/HEAD (nhánh main) |
| Unity CLI / compile ngoài Editor | Terminal | Chưa có quyền truy cập |
| Symbol/usage index | `.ai/MAPS/index-scripts.md` (75 file .cs, 35 script được scene/prefab/asset tham chiếu) | Hoạt động [V]; chạy lại bằng `.ai/tools/rebuild_index.snippet.cs` |

Phát hiện thay đổi chưa ghi: `.ai/tools/changed_since.snippet.cs` (chỉ đọc, qua execute_code) [V, đã chạy]; có terminal thì dùng thêm git status/log.

## Môi trường và cách làm việc (kiểm 2026-10-16, trước 00:40 UTC; chỉ phản ánh phiên kiểm tra này)
### Môi trường
- Ứng dụng: Claude Desktop (chế độ chat) + Unity MCP. KHÔNG giả định có Claude Code: giao diện Desktop có nút chế độ Code `</>` nhưng chủ dự án không dùng và chưa kiểm tra.
- Unity MCP: package `com.coplaydev.unity-mcp` (Library/PackageCache), server `mcp-for-unity` 10.2.0, transport stdio, cổng 6401 (log Console: `StdioBridgeHost started on port 6401`). Unity 6000.3.7f1. Instance đang chọn: `The Return@3c137dc4`.
- Phía server không nhận diện được ứng dụng khách: `debug_request_context` trả `client_id: null`, `meta: null`. Việc đây là Claude Desktop dựa trên giao diện chủ dự án đang dùng, không phải xác nhận từ server.
- `execute_code` tự dùng CodeDom (C# 6); không thấy Roslyn trong Editor này.

### Nạp công cụ theo nhu cầu (tool_search)
- 47 công cụ unityMCP ở dạng nạp theo yêu cầu (deferred): phải nạp bằng `tool_search` trước khi gọi. Ví dụ: `tool_search(query="read_console manage_scene execute_code", limit=8)`; kết quả trả tên tham số chính xác, không đoán tham số.
- Gọi công cụ chưa nạp sẽ lỗi "has not been loaded yet" (gặp với `read_console` ngày 2026-10-16). Phiên mới hoặc ngữ cảnh dài có thể mất công cụ đã nạp: gặp lỗi đó thì nạp lại.
- `read_console`: nên truyền `count` dạng chuỗi (theo mô tả công cụ).
- `manage_editor` KHÔNG có action đọc trạng thái (chỉ `telemetry_status`, `telemetry_ping`, `play`, `pause`, `stop`, `set_active_tool`, thêm/xóa tag và layer, `deploy_package`, `restore_package`, `undo`, `redo`): đọc trạng thái Editor bằng `execute_code`.
- `find_in_file` chỉ làm việc với script `.cs` (đuôi khác bị đổi thành .cs nên báo "Script not found").

### Trạng thái 47 công cụ unityMCP (kiểm 2026-10-16)
| Trạng thái | Công cụ |
|---|---|
| Gọi thành công | `execute_code`, `read_console` (get), `manage_scene` (get_active), `manage_asset` (search), `debug_request_context`, `unity_docs` (lookup `CharacterController` trả đúng trang ScriptReference) (`tool_search` là công cụ nạp, không tính trong 47) |
| Gọi nhưng lỗi | `manage_editor` (action `get_state` không tồn tại), `find_in_file` (đường dẫn `.md`: chỉ nhận script) |
| Đã nạp, chưa gọi | `refresh_unity`, `batch_execute`, `manage_build` |
| Chưa nạp, chưa gọi | 36 công cụ còn lại, gồm `manage_gameobject`, `manage_components`, `manage_prefabs`, `manage_camera`, `run_tests`, `unity_reflect`... |

### MCP resources
- Trong phiên kiểm tra, CHƯA truy cập được resources của Unity MCP: `list_mcp_resources(source="unityMCP")` và `read_resource_link` trả "Server not found. Available sources: 'Claude Docs', 'visualize'". Nghĩa là ứng dụng khách trong phiên này không đưa nguồn `unityMCP` cho Claude.
- KHÔNG kết luận Unity MCP không hỗ trợ resources. Đã xác minh ở mức code (2026-10-16): server Python của `mcp-for-unity` 10.2.0 (trong uv cache) định nghĩa 26 decorator `@mcp_for_unity_resource` với 37 URI `mcpforunity://...` (vd `editor/state`, `editor/selection`, `instances`, `custom-tools`, `menu-items`, `prefab/...`). Chưa gọi thử khi chạy thật: ứng dụng khách của phiên này không đưa nguồn `unityMCP` cho Claude.
- Hệ quả thực hành: bước "đọc `mcpforunity://editor/state`" trong skill chưa dùng được; thay bằng `execute_code` đọc `EditorApplication`.

### Quy trình an toàn khi làm việc với Editor
1. Trước khi làm: đọc trạng thái bằng `execute_code` (chỉ đọc): `EditorApplication.isPlaying/isPaused/isCompiling/isUpdating`, scene đang mở và `isDirty`; `read_console` (action `get`, types error/warning). Không bắt đầu sửa khi Editor đang compile/cập nhật. Đọc `.ai/CURRENT.md` (cảnh báo).
2. Sau khi tạo/sửa script: chờ compile xong (đọc lại `EditorApplication.isCompiling` cho tới khi false; mất kết nối ngắn do domain reload là bình thường theo tài liệu skill orchestrator [theo tài liệu, chưa thử trong dự án này]). Đọc code package: `ManageScript.cs` gọi `AssetDatabase.ImportAsset` rồi `CompilationPipeline.RequestScriptCompilation()` ở hai chỗ [chưa chạy thật; chưa xác nhận hai chỗ ứng với `create_script`/`script_apply_edits`]. Sau đó `read_console` kiểm lỗi trước khi gắn component hoặc chạy test.
3. Đọc Console sau mỗi thay đổi lớn (`types` error/warning, `count` dạng chuỗi). Không gọi action `clear` trừ khi cần và chủ dự án biết.
4. Xác minh bằng screenshot khi thay đổi nhìn thấy được: `manage_camera(action="screenshot")` [công cụ chưa nạp, chưa gọi]. Đọc code package (2026-10-16): `ScreenshotUtility.cs` luôn `File.WriteAllBytes` TRƯỚC khi xét `include_image`, nên `include_image=True` chỉ thêm ảnh base64, KHÔNG tránh được việc ghi file PNG vào dự án (thư mục mặc định chưa xác nhận trong code; `Assets/Screenshots` có tồn tại). Vì ghi file vào dự án, chỉ chụp khi chủ dự án cho phép và phải báo file đã tạo.
5. Scene đang dirty hoặc đang bị khóa trong CURRENT.md: không `load`/`create`/`close_scene`, không save thay chủ dự án khi chưa được phép.
6. Ghi mức xác minh trung thực theo thang PROTOCOL (1 tạo file, 2 compile, 3 test chạy, 4 runtime, 5 chưa xác nhận).

### Ghi nhận trạng thái Editor và scene (2026-10-16, trước 00:40 UTC)
- Editor: không Play Mode, không compile. Console: 0 lỗi, 0 cảnh báo, 1 dòng log (bridge cổng 6401).
- Scene `SchoolFloorPrototype` (`Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`): `isDirty=true`, 23 root object, trong đó có `MissionFlow_Controller`. Ở lần kiểm tra trước đó (đầu phiên T-013, trước 2026-10-15 12:44 UTC): `isDirty=false`, 22 root.
- KHÔNG khẳng định ai hoặc điều gì gây thay đổi. Cũng chưa chứng minh `MissionFlow_Controller` là root mới: lần trước chỉ ghi số lượng root, không ghi tên. Có thay đổi scene chưa lưu trong Editor; chủ dự án cần kiểm tra trước khi đóng Unity.

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
- Claude Code: CHƯA kiểm tra (dự án hiện dùng Claude Desktop + Unity MCP, không dùng Claude Code). Không có kết luận nào về Claude Code.
- Phiên Claude Desktop (chat + Unity MCP) lúc kiểm tra: 5 skill này không xuất hiện trong danh sách `available_skills` mà phiên quan sát được. Đây chỉ là kết quả của một phiên kiểm tra, không phải kết luận về khả năng của Claude Desktop (xem mục «Xác minh nạp skill» bên dưới). Cách dùng hiện tại: đọc `SKILL.md` của đúng skill bằng execute_code khi task khớp, không đọc tất cả.
- Cảnh báo từ trình cài: skill chạy với quyền agent đầy đủ. Đã rà soát `unity-cli` ở mức đọc file (T-013, xem mục bên dưới); chưa đọc từng dòng các file references/CHANGELOG.
- `skills-lock.json` ở gốc ghi nguồn (Unity-Technologies/skills) và hash cho 5 skill; dùng để biết skill nào được cài và phát hiện thay đổi.
- `find-skills` (vercel-labs) được cài ở mức người dùng `~\.agents\skills\find-skills` (ngoài project), symlink cho Claude Code. Không thuộc project; không cần cho workflow này.
- Chưa khảo sát repo skill ngoài Unity-Technologies; repo có 34 skill, đã bỏ qua 29 skill không liên quan (IAP, ads, UGS, 2D, localization, migrate-birp-to-urp...).

## Rà soát unity-cli (T-013, 2026-10-16 00:26 UTC; chỉ đọc file, không chạy CLI; mức xác minh 1: chỉ tạo/sửa tài liệu, không compile/test/runtime)
Phạm vi đã đọc: toàn bộ SKILL.md (51 KB), SECURITY.md; quét tự động 13 file .md của unity-cli + 3 skill còn lại theo mẫu rủi ro (curl/iex/pipe shell, base64, sudo, ~/.ssh, 'ignore previous', 'without asking'...) và danh sách host URL. KHÔNG đọc từng dòng 12 file references/CHANGELOG (~400 KB); chỉ xem ngữ cảnh các dòng khớp mẫu.
Kết quả:
- Không thấy chỉ thị ẩn/prompt injection/thu thập bí mật. Mọi dòng khớp mẫu đều là tài liệu mô tả lệnh (vd `sudo apt upgrade unity-cli`, SSH dùng ssh-agent sẵn có, 'silently' = không nuốt lỗi).
- Rủi ro CÓ THẬT (SECURITY.md tự khai, chấp nhận theo thiết kế): (1) `unity command eval` chạy C# tùy ý trong Editor; (2) lệnh cài CLI tải-và-chạy script từ unity.com / public-cdn.cloud.unity3d.com (`curl | bash`, `irm | iex`); (3) `unity skill install/refresh/enable/disable` và `unity setup claude` ghi vào cấu hình AI client; (4) `unity pipeline install` thêm package com.unity.pipeline vào Packages/manifest.json. Đã kiểm tra (2026-10-16 00:26 UTC): `Packages/manifest.json` và `Packages/packages-lock.json` không chứa `com.unity.pipeline`. Suy luận (chưa thử): khi thiếu package này, CLI có thể không điều khiển được Editor đang mở.
- Host URL trong skill: chỉ unity.com, unity3d.com, public-cdn.cloud.unity3d.com, packages.unity.com, github.com/raw.githubusercontent.com, docs.github.com, spdx.org, keepachangelog.com; còn lại là ví dụ example.com/proxy.
- Toàn vẹn: hai bản `.claude/skills` và `.agents/skills` giống hệt (24/24 file, so SHA-256). `computedHash` trong skills-lock.json KHÔNG khớp SHA-256 của SKILL.md (bản gốc hoặc chuẩn hóa LF) — thuật toán của trình cài chưa biết nên CHƯA xác minh được hash; không phải bằng chứng bị sửa.
- Trùng chức năng với Unity MCP: SKILL.md bảo 'ưu tiên điều khiển Editor đang mở' qua CLI; trong workflow này đã có Unity MCP nên không cần cài CLI/pipeline.
- Khuyến nghị: GIỮ nguyên trạng, không nạp mặc định, không chạy lệnh cài nào. Chỉ đọc references/build-run-test.md khi chủ dự án muốn build/test headless; mọi `unity install/pipeline install/setup claude/skill install` cần chủ dự án đồng ý trước.
### Xác minh nạp skill (T-013, phiên kiểm tra 2026-10-16 00:26 UTC; chỉ phản ánh phiên này)
Đã xác minh:
- Đọc được `SKILL.md` của cả 5 skill (ui, ui-ugui, physics-3d-collision, generate-editor-search-query, unity-cli) qua Unity MCP `execute_code` (đọc file trên đĩa).
- Trong phiên Claude Desktop (chat + Unity MCP) hiện tại, 5 skill này không xuất hiện trong danh sách `available_skills` mà phiên quan sát được (danh sách có 12 skill khác: docx, pdf, pptx, xlsx, product-self-knowledge, frontend-design, file-reading, pdf-reading, docs, import-memory, morning, skill-creator).
Chưa xác minh / không kết luận:
- Chưa xác minh Claude Desktop có cơ chế tự động nạp skill từ `.claude/skills/` của dự án hay không. Phiên kiểm tra không có công cụ truy vấn cơ chế nạp skill. Kết quả trên chỉ phản ánh phiên kiểm tra hiện tại và KHÔNG chứng minh Claude Desktop không thể tự nạp các skill này.
- Chưa kiểm tra Claude Code vì dự án hiện dùng Claude Desktop + Unity MCP.
- Đọc được file skill khác với việc hệ thống tự nạp skill: đọc được chỉ chứng minh file tồn tại và truy cập được qua MCP.
Cách dùng hiện tại: khi task khớp, đọc `SKILL.md` của đúng skill bằng execute_code (không đọc tất cả).

### Trạng thái 6 skill Unity (kiểm 2026-10-16): «đọc được file» khác với «hệ thống tự nạp»
| Skill | Vị trí | Đọc được SKILL.md (execute_code) | Có trong `available_skills` của phiên | Hệ thống tự nạp |
|---|---|---|---|---|
| `ui`, `ui-ugui`, `physics-3d-collision`, `generate-editor-search-query`, `unity-cli` | `.claude/skills/<tên>/` trong dự án (bản sao ở `.agents/skills/`) | Có (5/5) | Không | Chưa xác minh |
| `unity-mcp-orchestrator` | `C:\Users\<người dùng>\.claude\skills\unity-mcp-skill\` (mức người dùng, ngoài dự án; tên thư mục khác `name:` trong SKILL.md) | Có | Không | Chưa xác minh |
Chi tiết `unity-mcp-orchestrator`:
- Nguồn: package Unity MCP tự đồng bộ (`SkillSyncService.cs`; marker `.unity-mcp-skill-sync` ghi `managed-by-unity-mcp-skill-sync`) từ GitHub `CoplayDev/unity-mcp`. File sửa lần cuối 2026-10-14 11:58 UTC.
- Gồm SKILL.md (286 dòng, đã đọc), `references/tools-reference.md` (~60 KB) và `references/workflows.md` (~74 KB) (hai file này chưa đọc chi tiết). SKILL.md dẫn tới `references/resources-reference.md` nhưng file này KHÔNG có trên đĩa (chưa biết lý do).
- Cách dùng: đọc theo nhu cầu bằng `execute_code`, chỉ đoạn cần thiết. Quy tắc áp dụng được đã tóm trong «Quy trình an toàn»; phần phụ thuộc resources chưa dùng được.
Chưa xác minh: Claude Desktop có tự nạp skill từ `.claude/skills/` của dự án hoặc từ `~/.claude/skills/` hay không (phiên kiểm tra không có công cụ truy vấn cơ chế này; kết quả không chứng minh Desktop không thể tự nạp). Claude Code chưa kiểm tra.

### Kiểm chứng bổ sung (2026-10-16, chỉ đọc; không gọi công cụ ghi, không tạo file)
| Mục | Kết quả | Mức chắc chắn |
|---|---|---|
| Server Unity MCP có resources | Có: 26 `@mcp_for_unity_resource`, 37 URI trong code server | Xác minh trong code; chưa gọi khi chạy thật |
| Screenshot có ghi file | Có: `File.WriteAllBytes` chạy trước nhánh `include_image` (`ScreenshotUtility.cs` dòng 130-131 và 252-253) | Xác minh bằng đọc code; chưa gọi `manage_camera`. `CURRENT.md` ghi một PNG thừa trong `Assets/Screenshots/`; chưa chứng minh ai tạo |
| Tạo/sửa script kích compile | `ManageScript.cs` có `ImportAsset` + `RequestScriptCompilation` | Đọc code; chưa chạy |
| `unity_docs` | Hoạt động (lookup `CharacterController`) | Đã gọi thành công |
| Kho skill của Claude Desktop | Có kho riêng `...\Packages\Claude_*\LocalCache\Roaming\Claude\local-agent-mode-sessions\skills-plugin\`: 4 cây, mỗi cây 14-18 skill, đều `creatorType: anthropic`, KHÔNG có skill Unity nào (18 tên: built-in-browser, chrome-browser, computer-use, consolidate-memory, deep-research, docs, docx, explain-usage, google-workspace, import-memory, morning, pdf, pptx, schedule, setup-claude, setup-cowork, skill-creator, xlsx) | Bằng chứng gián tiếp |
| Skill từ GitHub gốc | Thảo luận PR #380 của CoplayDev/unity-mcp (nguồn phụ, không phải tài liệu chính thức) nói cài skill cho Claude Desktop bằng cách tải `.zip` lên | Nguồn phụ qua tìm kiếm web |
Diễn giải thận trọng: dữ liệu trên nghiêng về việc Claude Desktop quản lý skill bằng kho riêng/tải lên chứ không quét `.claude/skills/` của dự án hay `~/.claude/skills/`, nhưng CHƯA chứng minh (không thấy mã nguồn Desktop). Skill như `frontend-design`, `product-self-knowledge`, `file-reading`, `pdf-reading` có trong `available_skills` nhưng không nằm trong kho cục bộ này, nên còn nguồn skill khác.
Quan sát (chưa đọc nội dung): thư mục `claude-code-sessions` của Desktop có 4 mục; không biết là gì.
Vẫn chưa xác minh: resources Unity MCP khi chạy thật; vì sao `references/resources-reference.md` thiếu trên đĩa (tìm web không thấy xác nhận có/không ở repo gốc); chờ kết nối lại sau domain reload; thư mục lưu screenshot mặc định; Claude Code.

## Index mở rộng (chưa làm)
Gọi hàm, tham chiếu tạo bằng code, vector DB: chưa cần. Cân nhắc Roslyn/Tree-sitter nếu symbol-level search trở thành nút thắt.
