# PHIẾU T-024: Khung mạng host-authoritative
- Mục tiêu (1 câu): khung dùng chung 'client gửi LỆNH -> host kiểm + chạy luật -> gửi TRẠNG THÁI', có đường gửi thông tin RIÊNG cho từng người (vd tốc độ cặp sách ở cầu ván).
- Được đụng: tạo mới Features/Network/Runtime/Framework/* (phần luật thuần C#, tách khỏi Unity) + test EditMode. KHÔNG sửa Domain của nhiệm vụ.
- KHÔNG được đụng: scene, prefab, NetworkSessionService.cs.
- Phép chủ dự án: không cần (code mới).
- Cách kiểm xong: test EditMode xanh: lệnh hợp lệ chạy, lệnh sai bị từ chối + log, thông tin riêng không lọt sang người khác, chạy đúng với 2/3/4 người; compile sạch.
- Đọc trước (tối đa 3): PLANS/T-009-online-coop.md mục 2 và 8; Features/PlankAndKey/Runtime/Domain (mẫu luật thuần); Features/Network/Runtime/NetworkSessionService.cs
- Việc kế tiếp sau khi xong: T-027 dùng khung này.
