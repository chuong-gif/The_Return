# PlankAndKey — Cầu ván và thẻ từ (Hanh_Lang_6)

**Vị trí:** Hanh_Lang_6 (nhóm cổng theo nhãn cũ). **Trạng thái:** Domain + Session + Presentation + scene đã có; Play Mode thật đạt 1 vòng thắng (T-006b2, 2026-10-21), chưa người chơi thật (`.ai/SYSTEM_CARDS/PlankAndKey.md`).

## Luật (theo thiết kế đã duyệt)
- Hành lang bị đứt bởi một khe; một tấm ván chịu tối đa 2 người và MỘT thẻ từ; ván chỉ hạ xuống khi thẻ nằm trên ván; cổng ở bờ bên kia mở khi cả đội đã qua và thẻ ở bờ đó. Mỗi người có tốc độ qua khác nhau; thời gian của cả cặp = người chậm hơn; có đồng hồ tổng, hết giờ thì về đầu khe. Số liệu tối ưu đã kiểm bằng tính toán (ví dụ tốc độ [1,2,5] tối thiểu 8). Thông số ở asset `PlankAndKey_VI`. Đơn vị thời gian hiện là «đơn vị», chưa phải giây.

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `PlankAndKey_LogicOnly` (script `PlankAndKeyPrototype`: party, laneOrigin, deck, deckZone, deckRestA/B, anchorA/B, card, cardHomeA/B, `labels[3]`, settings, fixedSeed, interactDistance 3.2, failResetSeconds 2.5, hintSeconds 14).
| Object | Thành phần | Vai trò |
|---|---|---|
| `GapBlocker` | BoxCollider | chặn khe khi ván chưa hạ |
| `GapVoid` | MeshRenderer | hình hố/khe |
| `DeckRestA/B`, `AnchorA/B`, `CardHomeA/B` | (chỉ Transform) | điểm đậu của ván, điểm đứng, chỗ cất thẻ |
| `PedestalA/B` | CapsuleCollider, MeshRenderer | bệ để thẻ ở hai bờ |
| `Deck` (+ con `DeckSurface`) | BoxCollider (trigger); mặt ván là MeshRenderer | tấm ván |
| `LeverA`, `LeverB` | BoxCollider, MeshRenderer, `PlankInteractable` (kind) + con `Handle`, `Label` (TextMesh) | cần gạt hạ/nâng ván ở mỗi bờ |
| `KeyCard` | BoxCollider, MeshRenderer, `PlankInteractable` (kind=Card) + 1 con | thẻ từ |

## Thay asset 3D
- Giữ: tên và vị trí các điểm `DeckRest*`, `Anchor*`, `CardHome*` (mốc logic), BoxCollider của `GapBlocker`, trigger `Deck`, `PlankInteractable.kind` trên từng cần gạt và thẻ, con `Label` (TextMesh).
- Thay được: mesh/material của `GapVoid`, `DeckSurface`, `PedestalA/B`, `Handle`, hình thẻ (giữ collider).

**Quy tắc chung khi đổi asset 3D (đối tượng dựng trong scene, CHƯA là prefab):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain + PlankSession: `Features/PlankAndKey/Tests/EditMode`. Hạn chế đã biết: đội 2 người gần như không có thử thách (xem `.ai/PLANS/T-006-mission-designs.md`, mục Hanh_Lang_6).
