# KẾ HOẠCH T-009: Online co-op 2-4 người   (file: .ai/PLANS/T-009-online-coop.md)
Trạng thái: LẬP KẾ HOẠCH (chưa code, chưa cài gói) | AI giữ: Claude Sonnet 5.5 (phiên MCP #2) | Cập nhật: 2026-10-22 01:56 UTC | Quyết định công nghệ: DECISIONS/2026-10-22-t009-network-choice.md
## 1. Mục tiêu và nghiệm thu
- 2-4 người ở các máy khác nhau vào cùng một phòng bằng mã phòng, mỗi người điều khiển MỘT nhân vật (thay cho đổi vai bằng phím 1-4 trên một máy), cùng chơi các nhiệm vụ của SchoolFloor. Giữ được chế độ chơi một máy hiện có.
- Nghiệm thu từng bước: (S1) 2 bản chạy thấy nhau di chuyển; (S3) 1 nhiệm vụ chơi trọn vẹn qua mạng; (S5) cả 8 nhiệm vụ + thắng map. Mức 4 cần 2 bản game chạy thật (xem rủi ro).
## 2. Kiến trúc đề xuất (host-authoritative, hợp với code hiện có)
- Máy của người làm chủ phòng (host) chạy MỌI luật: các lớp Domain (PlankSession, ChoirState, MapProgress...) là C# thuần nên dùng nguyên. Máy khách chỉ gửi LỆNH (nhặt thẻ, gạt cần, bấm ghế...) và nhận TRẠNG THÁI để hiển thị.
- Thông tin riêng từng người (vd tốc độ cặp sách ở cầu ván) chỉ gửi đúng máy của người đó.
- MissionFlowController, SchoolFloorAccessController, cổng, MapProgress chạy trên host; máy khách chỉ thấy kết quả (cổng mở/đóng, nhiệm vụ nào xong).
- Mạng: Netcode for GameObjects + Unity Transport + Unity Relay (host xin mã phòng = join code, máy khách nhập mã). Chưa dùng Lobby (không cần cho vào bằng mã).
## 3. Subtask
| ID | Việc | Cách kiểm | Trạng thái |
|---|---|---|---|
| S0 | Kiểm tài liệu/phiên bản NGO + Transport + Relay có tương thích Unity 6000.3.7f1 không; liệt kê gói cần cài | Đọc tài liệu chính thức | TODO |
| S1 | (CẦN chủ dự án cho phép cài gói) Thử nghiệm rời: scene riêng, host/join bằng mã, nhân vật đơn giản đồng bộ vị trí | 2 bản chạy thấy nhau | BLOCKED chờ phép cài gói |
| S2 | Thiết kế lớp lệnh/trạng thái dùng chung; tách PrototypePartyController: mỗi người sở hữu 1 nhân vật (chế độ online) | Plan riêng + test EditMode cho phần luật | TODO |
| S3 | Nối 1 nhiệm vụ mẫu (đề xuất Hanh_Lang_6 PlankAndKey: luật đã ở host-style, có thông tin riêng) | Chơi trọn vẹn 2 máy | TODO |
| S4 | Luồng map qua mạng: cổng, khu chuẩn bị, bắt đầu/thoát nhiệm vụ, MapProgress, thắng map | Chơi 2 máy | TODO |
| S5 | Nối các nhiệm vụ còn lại (Phong_1..6, Hanh_Lang_1..5 theo mức độ dễ) | Từng nhiệm vụ | TODO |
| S6 | Rớt mạng, vào lại, thoát phòng, giao diện nhập/chia sẻ mã phòng | Thử ngắt mạng | TODO |
## 4. Chi phí (đã kiểm 2026-10-21)
- NGO miễn phí. Unity Relay: 50 CCU trung bình tháng đầu miễn phí, 0,16 USD mỗi CCU thêm; 3 GiB băng thông miễn phí mỗi CCU (tối đa 150 GiB); dưới 20 người online: 0 USD, không cần thẻ. Phòng 4 người = 4 CCU.
- Chi phí chính là CÔNG SỨC sửa 8 nhiệm vụ (đang một máy, đổi vai bằng phím), không phải tiền dịch vụ.
## 5. Rủi ro / câu hỏi cho chủ dự án
- Cài gói đổi Packages/manifest.json: cần chủ dự án cho phép trước S1.
- Kiểm thử mạng cần 2 bản chạy cùng lúc (build + Editor, hoặc Multiplayer Play Mode nếu dùng được); AI không tự kiểm được mức 4 nếu chỉ có một Editor: sẽ báo rõ.
- Nhân vật dùng CharacterController: đồng bộ vị trí/ván/thang di chuyển cần thiết kế (đề xuất host quyết định vị trí các vật chuyển động).
- Chưa quyết: chat giọng nói, vào lại giữa chừng, nền tảng phát hành (Steam hay không), mức chi tối đa mỗi tháng.
