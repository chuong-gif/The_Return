# CONVENTIONS (tóm tắt; bản đầy đủ: Design/Cau-truc-du-an.md)

- Prefab tiền tố `PF_`, material `M_`. Prefab tương tác có child thay model được (VisualRoot / Visual_Replaceable); không xóa mốc tương tác, collider, ID khi thay model.
- Domain: C# thuần. Presentation: MonoBehaviour. Data: ScriptableObject. Editor/Tests: asmdef riêng, không vào build.
- Mỗi file C# mới: comment đầu file (mục đích + danh sách hàm), summary tiếng Việt trước mỗi hàm (làm gì / nhận gì / trả gì). Tên hàm tiếng Anh nhất quán.
- Không tạo thư mục Managers chứa mọi hệ thống. Chỉ đưa vào Core/Shared khi có lý do dùng chung cụ thể.
- Di chuyển asset bằng Unity để giữ GUID/.meta.
- Scene thử tách với scene map phát hành.
- Chỉ vật CÓ chức năng tương tác hoặc phục vụ logic nhiệm vụ mới làm prefab (chủ dự án). Mỗi prefab đi kèm mô tả cách dùng (README feature hoặc ghi chú cạnh prefab): mốc tương tác, collider, ID, child thay model.
- UI: ưu tiên nhiều không gian nhìn, bố cục linh hoạt theo khu nhiệm vụ; không để HUD che khu chơi.
- Không dời bố cục phòng/blockout của chủ dự án; chủ dự án tự chỉnh đồ họa map.
