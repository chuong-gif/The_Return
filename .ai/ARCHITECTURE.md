# ARCHITECTURE — Core4 và online chung

Tóm tắt điều hướng; kiểm asmdef/code hiện hành trước khi thay đổi kiến trúc.

## Luồng
- Domain nhiệm vụ thuần C#; Presentation/Unity scene ở lớp feature.
- `Features/Network/Runtime/Framework`: HostAuthority, ClientReplica, roster/slot, luật host-authoritative.
- `Features/Network/Online`: avatar, room/voice, bridge lệnh/trạng thái.
- `Maps/SchoolFloor`: điều phối map/cổng/tiến trình; scene đích hiện hành là MapSchoolF.
- Mỗi nhiệm vụ có Domain/Network/Online phù hợp; host sở hữu luật và trạng thái, client hiển thị snapshot. MapFlowOnline điều phối adapter nhiệm vụ.

## Ranh giới phụ thuộc
- Domain không tham chiếu Unity; Core/Shared không tham chiếu ngược Feature.
- Chỉ bốn nhiệm vụ Core4 là phạm vi phát triển. Assembly map cũ có thể còn tham chiếu nhiệm vụ đã cắt; không xóa code trước điều kiện Q-013.
- Nguồn chi tiết: `.ai/NETWORK.md`, `.ai/MAPS/asmdef-graph.md`, `.ai/SYSTEM_CARDS/INDEX.md`.

## An toàn
- Trước khi sửa scene tạo snapshot theo `AGENTS.md`.
- Không chạy builder/migration cũ trong menu; xem danh sách cấm ở `AGENTS.md`.
- Thay đổi trạng thái/thứ tự online xem `CURRENT.md`, `QUEUE.md`, `DECISIONS/PENDING.md`.
