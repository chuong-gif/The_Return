# PHIẾU T-023: Relay qua Internet
- Mục tiêu (1 câu): 2 máy ở 2 mạng khác nhau vào cùng phòng bằng mã qua Relay. Hiện tạo phòng OK (mã dạng 79WPPN) nhưng nối Relay lỗi 'Failed to establish connection with the Relay server' -> Transport failure.
- Được đụng: Features/Network/Runtime/NetworkSessionService.cs (phần Relay), scene NetworkS1Test, Builds/ (ngoài Assets). Giữ API công khai StartLanHost/StartLanClient/LeaveSession.
- KHÔNG được đụng: luật game, scene map, Build Settings, packages/manifest.json (cần phép riêng).
- Phép chủ dự án: Không sửa scene map (SchoolFloorPrototype, Sanh_Cho, MainMenu) nếu OWNER_UPDATES chưa cho phép; scene thử riêng trong Features/Network thì tự do.
- Cách kiểm xong: mức 4 cần 2 mạng khác nhau (nhờ chủ dự án/bạn, hoặc điện thoại phát 4G). Hai máy thấy nhau di chuyển; LOG ghi từng cách đã thử (WSS, vùng máy chủ gần Việt Nam, đổi mạng) + kết quả + lỗi.
- Đọc trước (tối đa 3): LOG mục PROGRESS T-009 2026-10-29; NetworkSessionService.cs; DECISIONS/2026-10-22-t009-network-choice.md
- Việc kế tiếp sau khi xong: T-032. T-026 dùng chung file session: báo nhau qua LOG trước khi sửa.
