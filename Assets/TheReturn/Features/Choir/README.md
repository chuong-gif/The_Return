# Choir — Hợp xướng (Phong_6)

**Vị trí:** Phong_6. **Trạng thái:** Domain + Presentation + scene đã có; chưa chơi thật (`.ai/SYSTEM_CARDS/Choir.md`).

## Luật (theo thiết kế đã duyệt)
- 4 bục có số 1-4 và ký hiệu riêng (hiện là chữ A-D bằng TextMesh); trên tường có bản nhạc là dãy 6 ký hiệu. Mỗi người giữ «thẻ nhạc» cho biết ký hiệu nào ứng với bục số mấy cho thẻ của mình; cả đội ghép thẻ để dịch bản nhạc thành dãy số bục rồi bấm bục theo thứ tự.
- Mỗi lần bấm phải cách lần trước không quá `StepWindowSeconds` giây (chỉnh được; 0 = không giới hạn; đổi vai khi đang dở dãy được cộng thêm `SwitchExtraSeconds`; mặc định giai đoạn phát triển 5 giây + 1,5 giây). Sai bục hoặc quá giờ thì dãy quay về nốt đầu, đề giữ nguyên. Thắng khi bấm đủ 6 nốt. Thông số ở asset `Choir_VI`.

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `Choir_LogicOnly` (script `ChoirPrototype`: party, settings, `pads[4]`, `scoreNotes[6]`, scoreBoard, statusBoard, drawOnGui).
| Object | Thành phần | Vai trò |
|---|---|---|
| `ChoirPad_1..4` | CapsuleCollider, MeshRenderer, `ChoirPad` (index, indicator, label) + con `Label` | bục; bấm E |
| `ScoreBoard` | 7 con: `Note_1..6`, `StatusText` | bản nhạc 6 nốt và dòng trạng thái |

## Thay asset 3D
- Giữ: tên `ChoirPad_n`, CapsuleCollider, thứ tự mảng `pads`, con `Label`, và các con `Note_1..6`, `StatusText` của `ScoreBoard`.
- Thay được: mesh/material của bục (script đổi màu theo nhịp qua renderer) và hình nền của `ScoreBoard`.

**Quy tắc chung khi đổi asset 3D (đối tượng là prefab instance của `PF_Choir` trong scene từ T-020; file nguồn `Features/Choir/Prefabs/PF_Choir.prefab`, xem thêm `Assets/TheReturn/PREFAB_GUIDE.md`):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain: `Features/Choir/Tests/EditMode`. Chưa kiểm bằng người chơi thật.
