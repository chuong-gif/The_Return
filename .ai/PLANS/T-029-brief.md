# PHIẾU T-029: Chat giọng nói (Vivox) theo phòng
- Mục tiêu (1 câu): vào/ra kênh thoại theo phòng, bật/tắt mic, tắt tiếng từng người (chủ dự án chọn CÓ).
- Được đụng: tạo mới Features/Network/Runtime/Voice/*. Gói com.unity.services.vivox đã có trong manifest; đổi manifest cần phép riêng.
- KHÔNG được đụng: Relay, luật game, scene map.
- Phép chủ dự án: Vivox có thể cần bật dịch vụ trong Unity Cloud của dự án -> hỏi chủ dự án nếu bị từ chối quyền.
- Cách kiểm xong: mức 4 cần 2 người thật + micro (AI một mình chỉ kiểm tới mức 3 + log vào/ra kênh); ghi rõ phần chưa kiểm.
- Đọc trước (tối đa 3): tài liệu Vivox for Unity (web, bản 16.x); NetworkSessionService.cs; phòng từ T-026.
- Chờ: T-026. Việc kế tiếp sau khi xong: T-032 (hướng dẫn micro cho bạn bè).
