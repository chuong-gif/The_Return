# DECISION 2026-10-22: công nghệ mạng cho T-009 (online co-op 2-4 người)
Người quyết: chủ dự án (chat với phiên MCP #2, 2026-10-22 01:56 UTC). AI đề xuất 3 phương án (A: Netcode for GameObjects + Unity Relay; B: Photon Fusion; C: Mirror/Fish-Net + Steam/EOS relay); chủ dự án trả lời 3 câu:
1. Kết nối: qua Internet, mời bạn bằng MÃ PHÒNG.
2. Máy chủ: MỘT NGƯỜI CHƠI làm chủ phòng (host), không thuê máy chủ.
3. Quy mô dự kiến: chơi thử với bạn bè, DƯỚI ~20 người online cùng lúc.
AI kết luận (đề xuất, chủ dự án chưa xác nhận riêng): chọn PHƯƠNG ÁN A = Netcode for GameObjects + Unity Transport + Unity Relay (mã phòng lấy từ join code của Relay), host-player, KHÔNG dùng Lobby/dedicated server ở giai đoạn này. Lý do chi phí: Unity Relay miễn phí cho 50 CCU trung bình tháng đầu (trang giá UGS, kiểm 2026-10-21), dưới 20 người chỉ tốn 0 USD và không cần thẻ thanh toán; thư viện NGO miễn phí. Phương án dự phòng: Photon Fusion (100 CCU miễn phí nhưng chặn cứng khi vượt, phụ thuộc nhà cung cấp).
CHƯA quyết định: chat giọng nói; cho vào lại giữa chừng khi rớt mạng; nền tảng phát hành (Steam hay không); mức chi tối đa mỗi tháng. Việc CÀI GÓI vào project (đổi Packages/manifest.json) cần chủ dự án cho phép riêng.
