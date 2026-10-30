# DECISION 2026-10-29: chủ dự án trả lời 4 câu mở của T-009 (online co-op)
Người quyết: chủ dự án (chat với phiên #7, 2026-10-29). Ghi lại để AI khác không phải hỏi lại.
1. Phát hành: CHỈ gửi bản build cho bạn bè. Không Steam, không cần Steam SDK. Giữ NGO + Unity Relay + mã phòng.
2. Chat giọng nói: CÓ (gói Vivox đã có trong manifest). Việc: T-029.
3. Host thoát/mất mạng: CÓ chuyển host cho người khác. Việc: T-030. LƯU Ý rủi ro (AI đề xuất, chủ dự án chưa duyệt chi tiết): NGO không có sẵn chuyển host; cần ảnh chụp trạng thái định kỳ + bầu host + tạo phòng Relay mới. Làm SAU khi luồng map qua mạng xong; bước 1 chỉ là tài liệu thiết kế.
4. Số người: mỗi đội 2-4 người, số người CỐ ĐỊNH ngay lúc lập đội (không vào/ra giữa chừng để đổi cỡ đội). Mọi nhiệm vụ phải chơi được với 2, 3 và 4 người. Việc: T-022.
5. Cách làm: chia thành nhiều task nhỏ độc lập để nhiều AI cùng nhận (xem TASKS.md T-022..T-032).
ĐÃ QUYẾT (xem mục bổ sung bên dưới): câu hỏi 4 vai khi dưới 4 người.

## TRẢ LỜI BỔ SUNG (chủ dự án, 2026-10-29): cỡ đội và vai
- Nhiệm vụ game CO GIÃN THEO ĐỘI: có bản chơi cho đội 2 người, 3 người và 4 người.
- KHÔNG ĐỔI VAI: mỗi người giữ đúng MỘT nhân vật cố định cả ván. Số nhân vật trong ván = số người trong đội.
- Hệ quả (AI đề xuất, theo lời chủ dự án): không cần cơ chế chia 4 vai cho ít người hơn hay đổi vai; phím 1-4 đổi vai chỉ còn ở chế độ 1 máy (giữ nguyên). Việc thật của T-022 là mô tả nhiệm vụ nào đổi gì (số vật, thông tin, điều kiện thắng...) theo N=2/3/4.
- Còn mở: nếu một nhiệm vụ hiện đòi một vai cụ thể mà đội nhỏ không có, nhiệm vụ đó được thiết kế lại theo N (T-022 ghi từng nhiệm vụ, chỗ mơ hồ thì hỏi chủ dự án).
