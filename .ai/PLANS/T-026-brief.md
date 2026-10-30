# PHIẾU T-026: Phòng online + đội 2-4 người
- Mục tiêu (1 câu): màn phòng: tạo phòng chọn số người 2-4 (cố định từ lúc lập đội), hiện mã, nhập mã vào, danh sách người, bắt đầu, rời phòng.
- Được đụng: tạo mới Features/Network/Runtime/Room/* (UI + RoomService), scene thử riêng. MainMenu/Sanh_Cho chỉ khi OWNER_UPDATES cho phép.
- KHÔNG được đụng: phần Relay trong NetworkSessionService.cs (T-023 giữ; cần đổi thì báo qua LOG).
- Phép chủ dự án: Không sửa scene map (SchoolFloorPrototype, Sanh_Cho, MainMenu) nếu OWNER_UPDATES chưa cho phép; scene thử riêng trong Features/Network thì tự do.
- Cách kiểm xong: chạy được trên LAN trước (Relay gắn sau): A tạo phòng 2 người, B nhập mã, hai bên thấy tên nhau, bắt đầu, rời phòng không lỗi.
- Đọc trước (tối đa 3): NetworkSessionService.cs; NetworkTestHud.cs; PLANS/T-008-menu-party-settings.md
- Việc kế tiếp sau khi xong: T-029 (chat giọng nói) cần phòng này; T-028.
