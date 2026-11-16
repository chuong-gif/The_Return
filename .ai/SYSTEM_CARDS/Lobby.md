# CARD: Lobby / Sanh_Cho

- Sảnh chờ dùng chung cho luồng party; online KHÔNG đi qua sảnh này: dùng scene `OnlineRoom` riêng (T-046, P-014 đồng ý); Sanh_Cho chỉ cho offline.
- Code chính: `Assets/TheReturn/Maps/Lobby/Runtime/LobbyReadyController.cs`, `LobbyCanvasView.cs`; kiểm tham chiếu scene trước khi sửa.
- Không chạy `The Return/Lobby/Rebuild Monumental Lobby` khi chưa được yêu cầu.
- Trạng thái phòng và quyết định: `NETWORK.md`, `QUEUE.md`, `DECISIONS/PENDING.md`; không dùng thông tin build/scene đích cũ trong thẻ lịch sử.
- Người chơi trong sảnh dùng cùng mô hình với MapSchoolF (`Core/Prefabs/PF_PlayerModel`, T-052), không còn khối bầu dục. Tiếng bước/nhảy: `LobbyLocomotionAudio` + `Audio_Slots` của scene (cùng clip với MapSchoolF; đổi clip thì sửa cả hai scene).
- `LobbySceneBuilder` (menu Rebuild Monumental Lobby) vẫn tạo khối bầu dục và không có Audio_Slots: chạy nó sẽ mất thay đổi T-052.
