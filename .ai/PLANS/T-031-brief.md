# PHIẾU T-031: Nối 11 nhiệm vụ còn lại qua mạng
- Mục tiêu (1 câu): mỗi nhiệm vụ chơi được qua mạng với 2, 3, 4 người theo MẪU CHUẨN của T-027. Danh sách + bảng nhận việc: PLANS/T-031-remaining-missions.md.
- Cách làm: mỗi AI nhận MỘT dòng trong bảng đó (ghi LOG START, sửa đúng dòng của mình), làm xong ghi bằng chứng rồi nhận dòng khác.
- Được đụng: Features/<tên nhiệm vụ>/* của dòng đã nhận. KHÔNG sửa Domain nếu test cũ đỏ; không động nhiệm vụ của AI khác.
- Phép chủ dự án: Không sửa scene map (SchoolFloorPrototype, Sanh_Cho, MainMenu) nếu OWNER_UPDATES chưa cho phép; scene thử riêng trong Features/Network thì tự do.
- Cách kiểm xong (mỗi dòng): test EditMode xanh + chơi 2 máy; thử đủ số người theo thiết kế T-022.
- Đọc trước (tối đa 3): mẫu chuẩn trong PLANS/T-027-*.md; thiết kế T-022; SYSTEM_CARDS của nhiệm vụ đó.
- Chờ: T-027 (mẫu), T-022 (thiết kế).
