# CARD: OnlineAvatars — đội và slot

- Vị trí: `Assets/TheReturn/Features/Network/Runtime/Online`; controller local ở `Assets/TheReturn/Core`.
- Đội 2–4 người; mỗi client có một slot/avatar cố định; slot không đổi khi một người rời; không host migration.
- `OnlinePartySlots` ánh xạ client/slot; `OnlineAvatar` sở hữu transform; `OnlinePartyLocalDriver` điều khiển avatar của client hiện tại.
- Luật nhiệm vụ lấy slot từ host/network connection, không từ command client. Từng nhiệm vụ nhận vị trí từ world của host.
- MapSchoolF và bốn adapters đã nối trong T-042; smoke nhiều người còn xem `QUEUE.md`.
- Test dùng chung: `TheReturn.Network.Online.Tests` và `TheReturn.Network.Room.Tests`; kiểm đúng assembly, phân biệt M3/M4/M5.
