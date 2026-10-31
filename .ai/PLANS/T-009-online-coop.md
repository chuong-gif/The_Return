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

## 8. Cập nhật 2026-10-29: quyết định của chủ dự án + chia task
- Quyết định: xem DECISIONS/2026-10-29-t009-owner-answers.md (chỉ gửi build cho bạn bè; có voice; có chuyển host; đội 2-4 người cố định lúc lập đội).
- T-009 giờ là UMBRELLA + S1 (nền LAN). Các subtask S2..S6 ở mục 3 được thay bằng task riêng T-022..T-032 trong TASKS.md; mỗi task có phiếu PLANS/T-0xx-brief.md.
- Phụ thuộc: T-022 + T-024 -> T-025 -> T-027 -> T-028 -> (T-030, T-031); T-026 -> T-029; T-023 + T-028 -> T-032.
- Làm NGAY được (không chờ ai): T-022 (tài liệu), T-023 (Relay), T-024 (khung), T-026 (phòng, chạy LAN), phần tài liệu của T-030.
- Quy tắc test mạng chung: cổng LAN 7777 chỉ MỘT phiên dùng tại một thời điểm; ghi LOG 'NET-TEST START/END' quanh mỗi lần chạy; công cụ test tự động dùng tham số -s1auto/-s1join/-s1walk/-s1quit (NetworkS1AutoRun.cs) hoặc -netauto (NetworkAutoTest.cs): đang gộp về một (việc của T-009).
- File dùng chung dễ đụng nhau: NetworkSessionService.cs (T-023 giữ phần Relay, T-026 thêm lớp phòng bằng file mới), PrototypePartyController.cs (T-025), prefab nhân vật (T-010 của phiên #6).
- (2026-10-29, sau câu trả lời về cỡ đội) KHÔNG đổi vai, nhân vật = số người, nhiệm vụ co giãn theo đội 2/3/4. Phụ thuộc mới: T-025 KHÔNG còn chờ T-022 (T-025 làm ngay được); T-022 giờ chỉ chặn T-031 và thông số T-027.
- (2026-10-30) T-023 DONE: Relay chạy thật qua Internet (DTLS, UDP, WSS đều tạo/vào được chéo nhau; chủ dự án + bạn khác tỉnh). Phụ thuộc mới: T-032 chỉ còn chờ T-028. Chưa thử: 4G/CGNAT, nước ngoài, 3-4 người, đo độ trễ.
- (2026-10-31, phiên #6) T-022: bản đề xuất bảng 12 nhiệm vụ x 2/3/4 người đã viết ở PLANS/T-022-design-2to4.md, chờ chủ dự án duyệt 8 câu hỏi (mục 2 của file đó); câu quan trọng nhất cho T-027 là PlankAndKey với đội 2 người (mặc định: giữ nguyên).
- (2026-10-31) T-026 REVIEW: phòng online đội 2-4 (RoomState/RoomRoster/RoomService/RoomHud, scene NetworkRoomTest). T-029 (voice) hết bị chặn. T-028/T-027 vẫn theo T-024/T-025 REVIEW của #9. Còn mở: xem HUD bằng mắt, đội 4 người thật, nối phòng -> nạp map (T-028).
- (2026-10-31, phiên #6) T-022 DONE: chủ dự án duyệt theo mặc định (DECISIONS/2026-10-31-t022-team-size.md). T-031 chỉ còn chờ T-027. Việc phát sinh: ChoirRules.Online theo số người 3 / 2,5 / 2 giây (làm trong T-031).
- (2026-10-31, phiên #6) T-030 bước 1 viết xong: PLANS/T-030-host-migration-design.md (đề xuất tự làm trên NGO + Sessions bằng nhật ký sự kiện; thử nghiệm S0 quyết định đường đi; chờ chủ dự án duyệt).
- (2026-10-31) T-029 REVIEW: VoiceChatService/VoiceHud (Vivox) tự vào kênh room-<mã phòng> khi vào phòng Relay; đã vào kênh + thấy nhau qua Relay thật; còn nghe/nói thật bằng 2 người (gói Builds/TheReturn_PhongThoai_BanBe.zip). T-032 cần ghi vào hướng dẫn: quyền micro Windows, đeo tai nghe.
- (2026-10-31, phiên #6) T-030 HỦY: chủ dự án bỏ chuyển host và vào lại; host off hoặc ai rời giữa ván thì cả phòng về màn hình chính (DECISIONS/2026-10-31-t030-no-host-migration.md). Mục 8 phần chuyển host không còn hiệu lực.
