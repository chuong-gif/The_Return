# CONVENTIONS (tóm tắt; bản đầy đủ: Design/Cau-truc-du-an.md)

- Prefab tiền tố `PF_`, material `M_`. Prefab tương tác có child thay model được (VisualRoot / Visual_Replaceable); không xóa mốc tương tác, collider, ID khi thay model.
- Domain: C# thuần. Presentation: MonoBehaviour. Data: ScriptableObject. Editor/Tests: asmdef riêng, không vào build.
- Mỗi file C# mới: comment đầu file (mục đích + danh sách hàm), summary tiếng Việt trước mỗi hàm (làm gì / nhận gì / trả gì). Tên hàm tiếng Anh nhất quán.
- Không tạo thư mục Managers chứa mọi hệ thống. Chỉ đưa vào Core/Shared khi có lý do dùng chung cụ thể.
- Di chuyển asset bằng Unity để giữ GUID/.meta.
- Scene thử tách với scene map phát hành.
