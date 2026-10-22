# TeacherSays — Cô nói (Hanh_Lang_5)

**Vị trí:** Hanh_Lang_5 (nhóm cổng `Hanh_Lang_6_Preparation`, nhãn cổng theo số cũ «Hanh lang 6»). **Trạng thái:** Domain + Presentation + scene đã có; kiểm Play Mode mô phỏng đoạn Step/Stand/hoàn thành (`.ai/SYSTEM_CARDS/TeacherSays.md`); chưa người chơi thật.

## Luật (theo thiết kế đã duyệt)
- Hành lang chia 5 đoạn; mỗi đoạn loa ra lệnh bằng chữ BƯỚC TỚI hoặc ĐỨNG YÊN. Lệnh có tiền tố «Cô nói» là lệnh thật (làm đúng); lệnh không có là lệnh giả (làm NGƯỢC). Tiền tố «Cô nói» chỉ hiện trên HUD của MỘT người, người đó đổi sau mỗi đoạn và phải báo cho đội.
- Lệnh thật BƯỚC TỚI: cả đội vào đoạn kế trong 8 giây; lệnh thật ĐỨNG YÊN: đứng yên 5 giây; lệnh giả thì ngược lại. Vi phạm +1 cảnh cáo cho đội; đủ 3 cảnh cáo thì về đầu hành lang. Thắng khi qua đủ 5 đoạn. Kiểm bằng sự kiện chuyển động/đứng yên, không dùng micro. Thông số (thời gian, số cảnh cáo) ở asset `TeacherSays_VI`.

## Cấu trúc trong scene (đã đo 2026-10-21)
Gốc `TeacherSays_LogicOnly` (script `TeacherSaysPrototype`: party, settings, `segmentZones[5]`, `zoneMarkers[5]`, `startPoints[4]`, speakerBoard, speakerText=`Command`, progressText=`Progress`, drawOnGui; yaw 330°).
| Object | Thành phần | Vai trò |
|---|---|---|
| `SegmentZone_1..5` | BoxCollider (trigger), cỡ (10.5, 4, 5) | 5 đoạn hành lang |
| `ZonePad_1..5` | MeshRenderer | tấm đánh dấu từng đoạn trên sàn (đổi màu theo trạng thái) |
| `StartPoint_1..4` | Transform | điểm đầu cho 4 người (về đây khi bị phạt đủ cảnh cáo) |
| `SpeakerBoard` | 2 con TextMesh: `Command`, `Progress` | bảng loa hiện lệnh và tiến độ |

## Thay asset 3D
- Giữ: trigger `SegmentZone_n` (cỡ và vị trí quyết định đoạn nào), thứ tự `ZonePad_n`, `StartPoint_n`, hai TextMesh `Command` và `Progress`.
- Thay được: mesh/material của `ZonePad_n` và khung loa `SpeakerBoard` (giữ hai TextMesh con).

**Quy tắc chung khi đổi asset 3D (đối tượng dựng trong scene, CHƯA là prefab):** giữ nguyên tên object, collider, script cùng các tham chiếu đã gán trên prototype, và tên con mà script dùng (ghi bên dưới). Chỉ thay phần hình: đổi mesh/material của MeshRenderer hoặc thêm model làm con rồi tắt MeshRenderer cũ. Đừng xóa object nào có script hay collider. Chi tiết chung và bảng prefab cũ: `Assets/TheReturn/PREFAB_GUIDE.md`.

## Kiểm
Test NUnit Domain: `Features/TeacherSays/Tests/EditMode` (số test: xem LOG T-006c). Chưa chơi thật.
