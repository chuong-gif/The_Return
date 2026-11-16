# CARD: MainMenu / party / settings

- MainMenu và party là hạ tầng chung; đường online: nút `Btn_Online` («Chơi online», `MainMenuOnlineEntry`) nạp scene `OnlineRoom` → `RoomMapHandoff` → MapSchoolF (T-046, P-014 đã đồng ý; chưa kiểm M4).
- Domain party/settings nằm dưới `Assets/TheReturn/Features/Party` và `Features/GameSettings`; UI ở `Assets/TheReturn/Maps/MainMenu`.
- Chỉ dùng để điều tra lỗi luồng party/settings hoặc quyết định P-014. Không đọc thêm khi task nhiệm vụ không liên quan.
- Không chạy builder scene/menu hoặc thay Build Settings ngoài phạm vi đã được chủ dự án cho phép.
