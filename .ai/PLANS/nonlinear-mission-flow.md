# KẾ HOẠCH: luồng nhiệm vụ tự chọn (viết 2026-10-14, CHƯA duyệt, CHƯA triển khai)

Yêu cầu: `.ai/DECISIONS/2026-10-14-nonlinear-mission-selection.md`. Không đổi nội dung từng nhiệm vụ (Domain/Presentation của 6 nhiệm vụ giữ nguyên); chỉ đổi cách điều phối và mở cửa.

## Điểm ràng buộc tuyến tính trong code [V]
1. `SchoolFloorFlow.Update`: chuỗi if theo thứ tự cố định + `transitionGatherZones[0..4]`.
2. `SchoolFloorAccessController.Refresh`: mở cửa theo `m-1` đã xong; `gate.mission` là chỉ số tuyến tính 0..5.
3. Attendance mặc định chạy ngay từ đầu, `ExternalControl` là cờ nhường quyền cho nhiệm vụ sau.
4. 5 bộ `*Checks` giả định luồng tuyến tính + tọa độ cũ (sẽ viết lại, xem bước S5).

## Tiến độ
- S0 (T-001): DONE 2026-10-15. S1+ (T-002...): chờ chủ dự án duyệt kế hoạch. Khi bắt đầu T-002, tách thành kế hoạch riêng theo `_TEMPLATE.md` (mục 12 PROTOCOL).

## Thiết kế đề xuất
- `MapProgress` (C# thuần, không Unity, tách Domain mới ví dụ `TheReturn.SchoolFloor.Domain`): danh sách khu (id, loại Room/Corridor, có nhiệm vụ hay chưa, 2 phòng kề nếu là hành lang), tập khu đã xong, khu đang làm (tối đa 1 vì mọi nhiệm vụ cần cả đội). Quy tắc: bắt đầu khu X khi không có khu nào đang làm, X chưa xong, và cả đội đang ở khu chuẩn bị của X; hành lang chỉ bắt đầu từ khu chuẩn bị gắn với 1 trong 2 phòng kề; thắng khi mọi khu đã xong. Serializable để sau này host đồng bộ qua mạng.
- Bộ chuyển (adapter) mỏng cho từng nhiệm vụ: `Active`, `Solved`, `Begin()`, `Stop(bool)` — bọc 6 prototype hiện có, không sửa logic của chúng.
- `SchoolFloorFlow.Update` thay chuỗi if bằng vòng lặp trên registry; `SchoolFloorAccessController` mở cửa theo `MapProgress` (khu đã xong luôn mở để đi qua; khu chưa xong chỉ mở lối vào khi không có khu nào đang làm; cửa khu chưa có nhiệm vụ giữ hành vi mở sẵn và hiển thị chưa có nhiệm vụ như hiện nay).
- Khu chưa có nhiệm vụ: đăng ký là placeholder (`hasMission=false`). GIẢ ĐỊNH cần chủ dự án xác nhận khi triển khai: điều kiện thắng bị chặn bởi các placeholder (đúng yêu cầu 'phải làm hết') hay tạm cho thắng khi xong các khu có nhiệm vụ; mặc định chờ, không tự chọn.

## Việc phải xác minh TRƯỚC khi code (bước S0, chỉ đọc)
- (Chủ dự án 2026-10-15: tên phòng/hành lang đã đặt trong object `BlockoutCampus_Generated`; điều kiện thắng: xong hết nhiệm vụ đã đăng ký, xem DECISIONS.) Bảng kề thật: hành lang nào nối 2 phòng nào, gate nào thuộc khu nào (đọc `SchoolPreparationGate.areaName/mission/designatedEntrance` của 22 gate trong SchoolFloorPrototype). Chủ dự án nói phòng 1 có cửa từ sân, tới hành lang 2 và từ hành lang 6; cần đối chiếu với scene, không suy từ số thứ tự.
- Cổng nào là cổng sân vào phòng, cổng nào là cổng phòng<->hành lang (mỗi khu có tối đa 3 lối).
- Attendance (khu đầu): cách nó bắt đầu hiện tại để không phá khi chuyển sang kiểu chọn khu.

## Bước làm (mỗi bước kiểm chứng riêng; không bước nào bỏ qua snapshot scene theo AGENTS.md)
- S0: trích bảng kề + danh sách gate từ scene (chỉ đọc) -> `.ai/MAPS/area-adjacency.md`, chủ dự án xác nhận.
- S1: tạo `MapProgress` + NUnit EditMode test (assembly mới, dùng Test Framework đã cài; chạy được qua Test Runner/CLI). Chưa nối vào scene. Mức nghiệm thu: test chạy và xanh.
- S2: adapter 6 nhiệm vụ + registry (chưa đổi hành vi, chưa gắn vào scene).
- S3: snapshot scene; nối registry vào `SchoolFloorFlow` và `SchoolFloorAccessController`; compile.
- S4: kiểm Play Mode: chọn từng phòng theo thứ tự bất kỳ; hành lang chỉ vào từ 2 phòng kề; mở cửa khu đã xong; đội thiếu người không bắt đầu được. Tọa độ lấy từ `preparationZone` của scene, không hardcode.
- S5: thay 5 bộ `*Checks` cũ bằng bộ kiểm theo luồng mới; chuyển cái nào chạy được sang NUnit.
- S6: HUD/canvas theo khu (liên quan yêu cầu tổ chức lại UI).

## Rủi ro
- Sửa scene (gắn component, gán field) — cần snapshot; scene hai file đang có thay đổi gần đây của chủ dự án.
- Attendance mặc định chạy từ đầu: đổi sang kiểu chọn có thể ảnh hưởng nhiệm vụ này.
- `FindFirstObjectByType` ở nhiều file: registry nên nhận tham chiếu qua field, không tìm động.
- Chủ dự án đang tự chỉnh đồ họa/nội thất: không dời gate/zone ngoài ý muốn.

## Câu hỏi còn mở (không chặn S0-S2)
Khi cả đội đã vào một khu đang làm dở rồi rời đi giữa chừng thì tiến độ giữ hay reset? Retry/thất bại trong khu có buộc quay ra khu chuẩn bị không? (Hiện từng nhiệm vụ tự xử lý retry.)
