# QuietCorridorChecks thất bại ở 'Scene 2: same scene transition' (2026-10-14)

Trạng thái: nguyên nhân được chủ dự án XÁC NHẬN (2026-10-14): hành vi đúng là tập hợp ở khu chuẩn bị trước khi vào hành lang/phòng => đây là test lỗi thời, không phải bug gameplay. CHƯA sửa test.

## Tái hiện [V]
SchoolFloorPrototype, Play Mode (Editor không focus, game vẫn chạy chậm), gọi `TheReturn.Editor.QuietCorridorChecks.Run()` bằng execute_code (reflection, assembly `TheReturn.SchoolFloor.Tests`). Domain() qua; lỗi ném ở `QuietCorridorChecks.cs:85` (Integrated, n=2).

## Cơ chế [V, từ code]
`SchoolFloorFlow.Update` chỉ gọi `corridor.Begin()` khi attendance đã giải xong, cả đội đứng trong `stagingZone` VÀ `ReachedNextRoom(0)` (cả đội trong `transitionGatherZones[0]`). Test đặt người chơi ở `g.checkpoints[i].position`.

## Số đo [V, runtime]
- `stagingZone` = `PreparationZone`, tâm (-100.16, 12.05, 100.38), kích thước (12.31, 3.60, 9.41), cũng chính là `transitionGatherZones[0]`.
- 4 `checkpoints` nằm ở khoảng (-103..-106, 10.9, 91..94): cả 4 NẰM NGOÀI stagingZone (z 91-94 so với vùng z 95.7-105.1).
- `g.Active=False`, `a.ExternalControl=False` sau khi test đặt đủ người tại checkpoint => chuyển tiếp không xảy ra.

## Kết luận
Nhiều khả năng bài kiểm tra đã lỗi thời so với Layout V2 (vùng chuẩn bị đã dời, test vẫn dùng `corridor.checkpoints` làm điểm tập hợp). Chưa chứng minh bằng cách đặt người vào PreparationZone rồi chạy lại; chưa loại trừ hẳn hồi quy gameplay. Cần chủ dự án xác nhận ý đồ: tập hợp ở PreparationZone rồi mới vào hành lang?

## Hướng sửa (chưa làm; sẽ đổi file test, không đổi gameplay)
Trong `Integrated`, đặt người vào PreparationZone (staging) trước, gọi Update, kiểm chuyển tiếp, rồi mới đặt người ở checkpoint/trạm. Các bài `GradeRepair/NavigationExam/DontLookBack/TeacherTruth` có thể gặp lỗi tương tự (cùng tiền đề tọa độ) — chưa chạy.

## Tác dụng phụ của lần chạy
`Design/Verification/QuietCorridor.json` KHÔNG bị ghi (test ném lỗi trước dòng ghi file; mtime vẫn 2026-09-14). Thoát Play Mode không lưu scene; Console 0 lỗi/cảnh báo sau đó.

## Cập nhật sau xác nhận
- Các bước sau trong `Integrated` còn dùng tọa độ cứng cũ (ví dụ `(4.9, .05, 13)`, `(5.8, .05, 33)`) khác hẳn tọa độ thế giới hiện tại (~-100, 11, 92-105) nên sửa riêng bước chuyển tiếp là chưa đủ.
- Hướng thiết kế mới (tự chọn phòng) sẽ thay luồng tuyến tính mà các bộ Checks đang giả định => viết lại test theo luồng mới hiệu quả hơn vá test cũ. Chờ chủ dự án chọn.
