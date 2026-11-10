# PHIẾU T-037: NHÓM B online (Choir, TeacherSays, SharedLantern)
- Mục tiêu (1 câu): 3 nhiệm vụ chấm theo vị trí và thời gian chơi trọn qua mạng, đồng hồ và vị trí do HOST quyết, đội 2/3/4.
- Được đụng: file MỚI trong Features/<Tên>/Runtime/{Network,Online} và Tests/EditModeNet; scene thử mới Network<Tên>Test.unity; thẻ hệ thống; RIÊNG Choir: ChoirRules.Online đổi theo số người (3 / 2,5 / 2 giây) + sửa ChoirTests tương ứng (có chủ đích, quyết định T-022).
- KHÔNG được đụng: Domain khác ngoài việc trên, scene map, prefab nhiệm vụ gốc, Build Settings (chỉ gỡ scene thử mình để lại), manifest.
- Phép chủ dự án: scene map chỉ khi OWNER_UPDATES cho phép.
- Cách kiểm xong: test xanh + 3 tiến trình LAN ca thắng/thua/hết giờ có log (M4) + 1 ca client ở xa bị host bác; người thật chơi là M5.
- Đọc trước (tối đa 3): NETWORK.md; PLANS/T-037-timing-zone-missions.md; Features/PlankAndKey/Runtime/Network (Tick).
- Chờ: không bị chặn. Cân nhắc làm sau T-036 vì mẫu Attendance rõ hơn.
