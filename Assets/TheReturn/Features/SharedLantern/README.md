# SharedLantern — Đèn lồng chung (Hanh_Lang_4)

**Vị trí:** Hanh_Lang_4 (nhóm cổng `Hanh_Lang_5_Preparation`, nhãn cổng theo số cũ «Hanh lang 5»). **Trạng thái:** Domain + Presentation + scene xong; đã kiểm Play Mode mô phỏng, chưa người chơi thật (`.ai/SYSTEM_CARDS/SharedLantern.md`, `.ai/PLANS/T-006f-shared-lantern.md`).

## Luật
- Hành lang tối, MỘT đèn lồng, vùng sáng bán kính 4 m quanh người cầm (chỉnh ở `SharedLantern_VI`). 4 biển đánh số BIỂN 1-4, mỗi biển 1 ký hiệu (chữ A-F), chỉ đọc được khi nằm trong vùng sáng; bấm E để ghi vào SỔ RIÊNG (không chia sẻ). Trao đèn bằng F cho người gần nhất trong 2 m.
- Ngoài vùng sáng quá 5 giây thì về điểm đầu (giữ sổ). Cuối hành lang nhập 4 ký hiệu theo thứ tự biển 1-4: sai thì xóa 4 ô và khóa bảng 10 giây; đúng và cả đội ở vùng cổng cuối thì hoàn thành. Vào từ cổng Vao hay Ra đều được (bảng ở cả hai đầu).

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `SharedLantern_LogicOnly` (script `SharedLanternPrototype`; yaw 30°, z+ từ Phong_4 sang Phong_5).
| Object | Thành phần | Vai trò |
|---|---|---|
| `Signs/Sign_1..4` | BoxCollider, MeshRenderer, `LanternSign` (index 0-3, label) + con `Label` (ký hiệu), `Number` (BIỂN n) — TextMesh | biển trên tường, ở mép lối giữa |
| `Panel`, `Panel_Start` | mỗi cái 14 con | bảng nhập ở hai đầu: `Backing`, `StatusBoard`, `Slot_1..4`, `Key_A..F`, `Key_Clear`, `Key_Submit` |
| `Panel*/Key_*` | BoxCollider, MeshRenderer, `LanternPanelKey` (kind, symbol, label) + con `Label` | phím; bấm E |
| `GoalZone`, `GoalZoneFromEnd` | BoxCollider (trigger) | vùng cổng cuối cho mỗi chiều vào |
| `Checkpoints`, `CheckpointsFromEnd` | 4 Transform mỗi nhóm | điểm đầu (về đây khi bị phạt) |
| `Lantern` (+ con `Light`) | MeshRenderer; Light điểm | đèn lồng, bám theo người cầm |

## Thay asset 3D
- Giữ: `LanternSign.index` (thứ tự = số trên biển), `LanternPanelKey.kind/symbol`, thứ tự `Slot_1..4`, BoxCollider của biển và phím (E dùng raycast), hai trigger `GoalZone*`, hai nhóm checkpoint, các con `Label`/`Number` (TextMesh), `Lantern` và `Light`.
- Thay được: mesh/material của biển, `Backing`, phím, `Lantern`; ký hiệu A-F là chữ, có thể đổi sang hình (giữ TextMesh hoặc sửa `LanternSign.Show`).
- Tối hành lang và ánh sáng: việc đồ họa của chủ dự án; có sẵn `Light` của đèn lồng và bán kính trong asset.

**Quy tắc chung khi đổi asset 3D (đối tượng dựng trong scene, CHƯA là prefab):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain: `Features/SharedLantern/Tests/EditMode` (37 test). Play Mode mô phỏng: đọc biển, sổ riêng, trao đèn, phạt ngoài sáng, phạt nhập sai, hoàn thành, vào từ hai đầu.
