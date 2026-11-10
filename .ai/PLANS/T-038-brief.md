# PHIẾU T-038: NHÓM C online (GlassBridge, QuietCorridor, DontLookBack)
- Mục tiêu (1 câu): 3 nhiệm vụ cần vị trí/chạy/hướng nhìn của từng nhân vật chơi trọn qua mạng với đội 2/3/4, host tự tính từ trạng thái avatar.
- Việc đầu tiên (C0): host đọc được cờ đang chạy + góc nhìn của từng avatar (ưu tiên suy ra từ vị trí; hướng nhìn thì gửi yaw). Báo qua LOG trước khi sửa OnlineAvatar/PF_OnlineAvatar (của T-025/#9 và T-010/#6).
- Được đụng: file MỚI trong Features/<Tên>/Runtime/{Network,Online}, Tests/EditModeNet, scene thử mới Network<Tên>Test.unity, thẻ hệ thống; QuietCorridor và DontLookBack cần viết test Domain đặc tả trước (Domain chưa có test).
- KHÔNG được đụng: Domain (trừ khi test chứng minh lỗi: báo chủ dự án), scene map, prefab nhiệm vụ gốc, Build Settings (chỉ gỡ scene thử mình để lại), manifest. Đọc T-033 trước khi đụng QuietCorridor.
- Cách kiểm xong: test xanh + 3 tiến trình LAN ca thắng/thua có log (M4); người thật chơi qua Relay là M5, bắt buộc vì sai số vị trí/hướng nhìn.
- Đọc trước (tối đa 3): NETWORK.md; PLANS/T-038-movement-observation-missions.md; SYSTEM_CARDS/OnlineAvatars.md.
- Chờ: không bị chặn, nhưng nên làm sau T-036/T-037.
