# BreakerPanel — Bảng cầu dao (Phong_5)

**Vị trí:** Phong_5. **Trạng thái:** Domain + Presentation + scene đã có; chưa chơi thật (xem `.ai/SYSTEM_CARDS/BreakerPanel.md`).

## Luật (theo thiết kế đã duyệt, `.ai/PLANS/T-006-mission-designs.md`)
- Bảng có S đèn và S công tắc, S = số người + 2 (2 người: 4, 3 người: 5, 4 người: 6). Mỗi công tắc ĐẢO 2-3 đèn cố định; mỗi người chỉ bấm được công tắc của mình và chỉ chủ công tắc thấy các đèn nó đảo (khi ngắm vào).
- Mọi người thấy chung trạng thái đèn và trạng thái ĐÍCH. Bật cùng lúc quá ceil(S/2) công tắc thì cầu dao nhảy: mọi công tắc về tắt. Thắng khi đèn khớp đích và giữ 3 giây. Mỗi đề (seed) có đúng 1 cách bật. Thông số chỉnh ở asset `BreakerPanel_VI`.

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `Breaker_LogicOnly` (script `BreakerPanelPrototype`: party, settings, `switches[6]`, `lamps[6]`, drawOnGui).
| Object | Thành phần | Vai trò |
|---|---|---|
| `BreakerSwitch_1..6` | BoxCollider, MeshRenderer, `BreakerSwitch` (index, indicator, label) + con `Label` | công tắc; bấm E |
| `BreakerLamp_1..6` | `BreakerLamp` (index, lamp, targetMarker) + con `Bulb` (Renderer), `Target` (Renderer) | đèn hiện tại (`Bulb`) và dấu đích (`Target`) |
Scene dựng đủ 6 phần tử; nhiệm vụ dùng S phần tử đầu theo số người.

## Thay asset 3D
- Giữ: tên `BreakerSwitch_n`/`BreakerLamp_n`, BoxCollider của công tắc, thứ tự trong mảng `switches`/`lamps`, và các con `Label`, `Bulb`, `Target`.
- Thay được: mesh/material của công tắc và của `Bulb`/`Target` (script đổi màu qua renderer).

**Quy tắc chung khi đổi asset 3D (đối tượng dựng trong scene, CHƯA là prefab):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain: `Features/BreakerPanel/Tests/EditMode`. Runtime: chưa kiểm bằng người chơi thật.
