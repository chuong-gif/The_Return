# CARD: MapSchoolF — map và điều phối nhiệm vụ

- Scene hiện hành: `Assets/TheReturn/Scenes/Maps/MapSchoolF.unity`; do chủ dự án dựng. Không dùng scene SchoolFloorPrototype/22 cổng cũ để suy ra cấu trúc mới.
- Bốn khu: Attendance/Phong_1, BreakerPanel/Phong_5, Choir/Phong_6, PlankAndKey/Hanh_Lang_6. Hanh_Lang_5 là lối đi.
- `MissionFlowController`, `MapProgress`, `MissionRegistry`, `MapFlowOnline` và bốn adapter điều phối cửa, bắt đầu/kết thúc nhiệm vụ và trạng thái host.
- Khi map/cổng không hoạt động: trước tiên xác nhận scene/hierarchy/bindings đang dùng, rồi kiểm `MissionFlowController`/`MapProgress`/adapter. Không chạy builder cũ.
- Test: `TheReturn.SchoolFloor.Domain.Tests` và `TheReturn.SchoolFloor.Online.Tests`; integration/online theo `QUEUE.md`.
- Trước mọi sửa scene: snapshot và kiểm dirty theo `AGENTS.md`. Tình trạng T-040/T-042: `CURRENT.md`, `QUEUE.md`.
