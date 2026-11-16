# QUEUE — Core4 map/online work

Chỉ các bước còn cần để hoàn tất map mới và online của bốn nhiệm vụ. Trạng thái chuẩn của từng project task xem `TASKS.md`; kỹ thuật chi tiết ở plan được chỉ định. P-014 là quyết định còn chờ, không tự thực hiện phần bị chặn.

| ID | Phạm vi | Việc / điều kiện hoàn tất | Trạng thái |
|---|---|---|---|
| Q-013 | T-040 P7 | Xóa code/scene của tám nhiệm vụ đã cắt theo `PLANS/T-040-dependencies.md`; chỉ sau khi chủ dự án xác nhận map mới chạy và đã commit/tag. Mỗi đợt compile sạch + Gauntlet. | BLOCKED — chờ xác nhận chủ dự án |
| Q-014 | T-040 P3c/P4 | Hoàn tất tích hợp map mới: layout/topology/flow, 4 khu, UI/âm thanh và handoff online theo phạm vi được duyệt. | IN-PROGRESS — chi tiết ở T-040/T-042 plans |
| Q-015 | T-040 P-011 | YardEntry + Core4Layout/TopologyProvider + test. | DONE — 2026-11-11 |
| Q-016–Q-022 | T-042 S0–S6 | Khảo sát, vòng đời OnlineMissionLink, bốn adapter nhiệm vụ, MapFlowOnline và spawn. | DONE — 2026-11-12; chi tiết ở `PLANS/T-042-net-wiring-mapschoolf.md` |
| Q-023 | T-042 S7 | Scene offline đích + Build Settings hoàn tất. Phần online room→map cần P-014 trước khi làm. | IN-PROGRESS — online bị chờ quyết định |
| Q-024 | T-042 S8 | Kiểm tổng và chuẩn bị smoke 2 máy sau khi Q-023 hoàn tất. | BLOCKED — chờ Q-023 |
| Q-025 | T-046–T-051 | Bản build thử online cho người thật (đường vào, mic, rời phòng, build, tài liệu, cổng kiểm). Gộp phần online của Q-023 và Q-024. | ĐÃ DUYỆT 2026-11-13 — T-046 REVIEW (đã kiểm một máy); T-047 REVIEW; T-048/T-049/T-051 chờ giao; T-050 TODO |

## Quyết định đang chờ
- P-014 (`DECISIONS/PENDING.md`): cho phép nối `RoomMapHandoff`/`NetworkRoomTest` và Build Settings để kiểm online room→map? Không thay đổi phần này trước khi có trả lời.

## Nền đã hoàn tất
T-039 contract tests, SimNet, Gauntlet và lớp host/client dùng chung đã xong; chi tiết/lịch sử không cần duy trì tại đây. Xem `NETWORK.md`, `TASKS_DONE.md`.
