# Quyết định: nhiệm vụ tự chọn thay cho tuyến tính (chủ dự án, 2026-10-14)

Trạng thái: YÊU CẦU ĐÃ CHỐT, CHƯA TRIỂN KHAI. Code hiện tại (`SchoolFloorFlow`, `SchoolFloorAccessController`) vẫn tuyến tính. Kế hoạch: `.ai/PLANS/nonlinear-mission-flow.md`.

## Quy tắc đã chốt (nguồn: chủ dự án, chat)
1. Bố cục: 6 phòng nối nhau bởi 6 hành lang thành vòng tròn; mỗi cửa phòng đi được từ sân ngoài. Trước mỗi phòng/hành lang có nhiệm vụ đều có khu vực chuẩn bị (ví dụ phòng 1 có 3: sân->phòng, phòng->hành lang 2, hành lang 6->phòng 1).
2. Không ràng buộc thứ tự: người chơi tự chọn phòng/hành lang làm nhiệm vụ, rồi đi sang khu khác.
3. Một hành lang chỉ đi vào được từ 2 phòng kề nó.
4. Mọi nhiệm vụ đều cần cả đội (2-4 người).
5. Thắng map = hoàn thành nhiệm vụ của tất cả phòng và tất cả hành lang. (Chủ dự án 2026-10-15: miễn xong hết nhiệm vụ là thắng, cách làm để AI tự quyết. Quyết định của AI: thắng khi mọi khu ĐÃ ĐĂNG KÝ nhiệm vụ đều xong; khu chưa có nhiệm vụ được liệt kê là THIẾU, không bị bỏ sót lặng lẽ, và tự tính vào điều kiện thắng khi đăng ký nhiệm vụ.)
6. Phòng/hành lang chưa có nhiệm vụ PHẢI có nhiệm vụ riêng; hiện chưa thiết kế -> ghi rõ để AI khác làm (xem `.ai/MISSIONS_TODO.md`). Không tự bịa nhiệm vụ.
7. Đã chọn: viết lại bộ test theo luồng mới (không vá test tuyến tính cũ).

## Hiện trạng code liên quan [V, đọc 2026-10-14]
- `SchoolFloorFlow.Update`: chuỗi if cố định Attendance -> QuietCorridor -> GradeRepair -> NavigationExam -> DontLookBack -> TeacherTruth, 5 `transitionGatherZones`.
- `SchoolFloorAccessController.Refresh`: gate mở khi `mission<0` hoặc nhiệm vụ đã xong hoặc (`designatedEntrance` và nhiệm vụ trước (m-1) đã xong). `gate.mission` là chỉ số 0..5 theo thứ tự tuyến tính.
- Mỗi nhiệm vụ có API tương tự: `Begin()`, `StopSession(bool)`, `Active`, `State.Solved` (Attendance dùng thêm `ExternalControl`).

## Bổ sung 2026-10-15 (chủ dự án, chat)
8. Phòng bắt đầu được từ hành lang kề và từ sân; hành lang chỉ từ 2 phòng kề.
9. Rời khu giữa chừng: GIỮ tiến độ các khu khác, không reset (khu bỏ dở không tính xong, làm lại được).
10. Khu chưa có nhiệm vụ không bắt đầu được; khi đăng ký nhiệm vụ thì tự vào điều kiện thắng.
