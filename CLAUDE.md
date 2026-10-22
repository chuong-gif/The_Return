# CLAUDE.md — hướng dẫn riêng cho Claude

Đọc `.ai/START_HERE.md` trước (ngắn, có ngân sách đọc và danh sách việc rảnh), rồi `AGENTS.md` khi cần; file này chỉ bổ sung phần riêng cho Claude.

## Công cụ thực tế (đã quan sát 2026-10-14)
- Unity MCP (`unityMCP`) hoạt động khi Unity đang mở và đã Start Session trong cửa sổ MCP for Unity.
- `execute_code` dùng compiler `codedom` (C# 6). Có thể đọc/ghi file trong project. Cơ chế `safety_checks` chặn việc khởi chạy tiến trình ngoài và xóa file, nên Git và Unity CLI KHÔNG chạy được qua MCP — cần terminal bên ngoài hoặc server Filesystem/Git/Terminal khác. Không tắt safety_checks để lách.
- Console của Editor có lỗi nhiễu `MCP-FOR-UNITY SkillSync failed: 403 rate limit` (GitHub API) — không thuộc code dự án.

## Cách làm việc
- Gom nhiều thao tác đọc độc lập vào một lần `execute_code`; không gọi từng tool nhỏ.
- Tìm đường dẫn/symbol trước, chỉ mở file liên quan. Không đọc lại file chưa đổi.
- Dùng Unity MCP cho trạng thái scene/GameObject/Console; sửa C# bằng file tools; việc hàng loạt bằng script xác định.
- Skill: xem `.ai/TOOLING.md` để biết skill nào thực sự được nạp.
