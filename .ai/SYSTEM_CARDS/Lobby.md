# CARD: Lobby / Sanh_Cho

- Sảnh chờ dùng chung cho luồng party; online room→map đang chờ P-014 (`DECISIONS/PENDING.md`).
- Code chính: `Assets/TheReturn/Maps/Lobby/Runtime/LobbyReadyController.cs`, `LobbyCanvasView.cs`; kiểm tham chiếu scene trước khi sửa.
- Không chạy `The Return/Lobby/Rebuild Monumental Lobby` khi chưa được yêu cầu.
- Trạng thái phòng và quyết định: `NETWORK.md`, `QUEUE.md`, `DECISIONS/PENDING.md`; không dùng thông tin build/scene đích cũ trong thẻ lịch sử.
