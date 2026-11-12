# TOOLING — quy tắc ngắn

- Source/Unity Editor là nguồn sự thật. Dùng công cụ Unity hiện có để kiểm scene, component, trạng thái Play/dirty, Console và test.
- Công cụ deferred phải được tìm bằng `tool_search` trước khi gọi; không đoán tên action/tham số.
- `execute_code` trong Unity MCP dùng C# 6; không dùng để khởi chạy process bên ngoài hoặc vượt safety check. Unity CLI/package chỉ cài khi chủ dự án đồng ý.
- Đọc skill đúng chủ đề theo `.ai/START_HERE.md`; không nạp skill/tài liệu lớn không cần thiết.
- Trước khi sửa scene: làm theo snapshot/preflight trong `AGENTS.md`. Không chạy builder/migration cũ.
- Kiểm tra tác dụng phụ của test/build (scene, Build Settings, report file); báo đúng mức xác minh theo `TESTING.md`.
