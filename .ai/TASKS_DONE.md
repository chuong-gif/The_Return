# TASKS_DONE — các mốc nền còn dùng cho Core4

Chỉ giữ quyết định kỹ thuật/bằng chứng còn ảnh hưởng tới 4 nhiệm vụ hoặc online chung. Task không có ở đây nằm ngoài phạm vi hiện hành hoặc đã được thay thế.

| Mốc | Kết quả còn dùng |
|---|---|
| T-002/T-003/T-004 | `MapProgress`, `MissionRegistry` và luồng chọn/bắt đầu/kết thúc khu là nền cho map mới. Kiểm mã hiện hành trước khi dùng lại quy tắc cũ. |
| T-006a/b/d | Domain + Presentation nền của BreakerPanel, PlankAndKey, Choir; thẻ hệ thống từng nhiệm vụ là bản đồ tra cứu, code/README là nguồn sự thật. |
| T-007 | `WinSummary`/`WinScreenPresenter`; điều kiện thắng phải tính theo đúng 4 nhiệm vụ ở scene mới. |
| T-022 | Quy mô đội 2–4 cố định, mỗi người giữ một avatar; Choir có cửa sổ nhịp 3/2.5/2 giây theo đội 2/3/4. |
| T-023/T-024 | NGO + Unity Relay (mặc định WSS) và framework host-authoritative C# thuần; không chuyển host khi host thoát. |
| T-025/T-026/T-029 | Avatar, phòng/roster và voice đã có; trải nghiệm 2–4 người/voice còn cần kiểm tra thực tế theo task. |
| T-027/T-028 | PlankAndKey là mẫu online; `MapFlowOnline` điều phối adapter nhiệm vụ và trạng thái map trên host. T-042 nối 4 nhiệm vụ vào MapSchoolF. |
| T-039 | Contract tests, SimNet, Gauntlet và `MissionOnlineHostBase`/`MissionOnlineClientBase` dùng chung. |
