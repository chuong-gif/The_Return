# GlassBridge — Cầu kính (Hanh_Lang_2)

**Vị trí:** Hanh_Lang_2 (nhóm cổng `Hanh_Lang_3_Preparation`, nhãn cổng theo số cũ «Hanh lang 3»). **Trạng thái:** Domain + Presentation + scene đã có; đã kiểm Play Mode mô phỏng, chưa người chơi thật (`.ai/SYSTEM_CARDS/GlassBridge.md`).

## Luật (theo thiết kế đã duyệt)
- Sàn kính 6 hàng x 2 ô; mỗi hàng chỉ 1 ô chịu được. Hai đài (đầu và cuối cầu) có bảng sơ đồ hiện dãy ô an toàn; người đi trên cầu không thấy sơ đồ. Bước ô sai: ô vỡ (có rào chặn), CHỈ người đó về đài đầu; hàng đã đi đúng giữ nguyên. Thắng khi cả đội qua cầu; người dẫn cuối cùng cũng phải qua (đồng đội ở đài cuối dẫn lại).

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `GlassBridge_LogicOnly` (script `GlassBridgePrototype`: laneOrigin, startPlatform, endPlatform, `tiles[12]`, `boards[2]`, laneHalfWidth 2.8, firstRowZ -11.25, rowSpacing 4.5, rowHalfDepth 1.6, fixedSeed).
| Object | Thành phần | Vai trò |
|---|---|---|
| `LaneFloor` | BoxCollider, MeshRenderer | sàn lối giữa |
| `Rail_Left`, `Rail_Right` | BoxCollider, MeshRenderer | rào hai bên (chặn đi vòng/xuống rãnh) |
| `StartPlatform`, `EndPlatform` | BoxCollider (trigger) | đài đầu và cuối; đội ở đài thấy sơ đồ |
| `Tiles/Tile_R<h>_<L hoặc R>` (12) | BoxCollider, `GlassTile` + con `Glass` (MeshRenderer), `Barrier` (BoxCollider) | từng ô kính; `Barrier` chặn khi ô vỡ |
| `Board_Start`, `Board_End` | MeshRenderer, TextMesh | bảng sơ đồ ở hai đài |

## Thay asset 3D
- Giữ: tên và thứ tự `Tile_R<h>_<L hoặc R>`, BoxCollider của ô và `Barrier`, hai trigger đài, hai `Board_*` (TextMesh), và các số hình học trên `GlassBridgePrototype` (laneHalfWidth, firstRowZ, rowSpacing, rowHalfDepth). Đổi vị trí ô thì phải sửa các số này cho khớp.
- Thay được: mesh/material của `Glass`, `LaneFloor`, `Rail_*` (giữ collider cùng kích thước).
- Lưu ý đã biết: hành lang có 2 rãnh bên thấp hơn lối giữa 0,4 m; ai bước xuống rãnh ở khu đài sẽ không leo lại được.

**Quy tắc chung khi đổi asset 3D (đối tượng là prefab instance của `PF_GlassBridge` trong scene từ T-020; file nguồn `Features/GlassBridge/Prefabs/PF_GlassBridge.prefab`, xem thêm `Assets/TheReturn/PREFAB_GUIDE.md`):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain: `Features/GlassBridge/Tests/EditMode`. Play Mode: đi thật qua 6 hàng, ô sai vỡ và về đài đầu, đủ 4 người qua thì Solved (phiên mô phỏng 2026-10-17).
