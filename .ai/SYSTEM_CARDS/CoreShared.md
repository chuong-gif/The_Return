# CARD: Core / Shared

- Vị trí: `Assets/TheReturn/Core` và `Assets/TheReturn/Shared`; dùng chung bởi map/nhiệm vụ.
- `PrototypePartyController`: điều khiển một máy; chế độ online riêng, không phá luồng local.
- `SceneTransitionService`: chuyển scene; online handoff chỉ thay đổi theo quyết định P-014.
- `PuzzleDoor`, interaction và theme binding là thành phần dùng chung; thay đổi cần kiểm scene prefab/map đang tham chiếu.
- Tra code hiện hành qua `.ai/MAPS/index-scripts.md`; kiểm test/integration theo task.
