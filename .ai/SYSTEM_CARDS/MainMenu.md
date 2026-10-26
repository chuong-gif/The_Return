# CARD: MainMenu (menu bắt đầu, nhóm chơi, Settings)

Khu / vai trò: scene MainMenu (đầu Build Settings) + hai hệ thống dùng chung cho cả game. Cập nhật: 2026-10-23 03:56 UTC. Trạng thái: chạy được ở chế độ MỘT MÁY; online chưa nối.

## A. Bản đồ
- Luật nhóm (thuần C#): Features/Party/Runtime/Domain: PartyState (2-4 người, chủ phòng, sẵn sàng, bắt đầu), PartyCode, PartyTypes (PartyResult, PartyMemberInfo, PartySnapshot), IPartyService, LocalPartyService + LocalPartyRegistry (máy chủ giả một máy, người giả lập). Test: Features/Party/Tests/EditMode/PartyTests.cs (28).
- Phiên nhóm xuyên scene: Features/Party/Runtime/PartySession.cs (static; HasStartedParty, PlayerCount, LocalSlot, Service, Reset).
- Cài đặt (thuần C#): Features/GameSettings/Runtime/Domain: GameSettingsData (âm lượng 0..1, độ nhạy 0,2..3,0 là hệ số nhân, toàn màn hình), GameSettingsModel, ISettingsStore, InMemorySettingsStore. Test: GameSettingsTests.cs (13).
- Áp dụng vào Unity: Features/GameSettings/Runtime/GameSettingsRuntime.cs (tự chạy trước scene đầu qua RuntimeInitializeOnLoadMethod: AudioListener.volume, Screen.fullScreenMode, PrototypePartyController.SensitivityScale), PlayerPrefsSettingsStore.cs.
- Giao diện: Maps/MainMenu/Runtime/MainMenuController.cs (4 màn: Main, PartyEntry, PartyRoom, Settings; nút nối bằng code), Maps/MainMenu/Editor/MainMenuSceneBuilder.cs (Build(overwrite) dựng scene; KHÔNG có MenuItem; từ chối ghi đè), scene Maps/MainMenu/Scenes/MainMenu.unity.
- Nối vào game: LobbyReadyController.Start (participantCount từ PartySession), SchoolFloorFlow.Start (attendance.SetParticipantCount từ PartySession), PrototypePartyController.SensitivityScale (Core, nhân vào độ nhạy chuột).
- Luồng: MainMenu -> Chơi -> Tạo/Vào phòng -> phòng chờ (Sẵn sàng, chủ phòng Bắt đầu) -> Sanh_Cho -> SchoolFloorPrototype. Thiết kế + 7 giả định: PLANS/T-008-menu-party-settings.md.

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Bắt đầu rồi mà số người trong Sanh_Cho/SchoolFloor vẫn là 4 | PartySession.HasStartedParty, LobbyReadyController.Start, SchoolFloorFlow.Start | Đọc PartySession.PlayerCount và lobby.participantCount |
| Nút Bắt đầu không bấm được | PartyState.CanStart (>=2 người, mọi người Sẵn sàng, chủ phòng) | Xem MainMenuController.RenderRoom, roomMessage |
| Âm lượng/độ nhạy không đổi trong game | GameSettingsRuntime.Apply, PrototypePartyController.SensitivityScale | Đọc AudioListener.volume, SensitivityScale |
| Cài đặt không nhớ lần sau | PlayerPrefsSettingsStore (khóa TheReturn.Settings.*) | PlayerPrefs.GetFloat |
| Mất giao diện hoặc vỡ chữ | Dựng lại bằng MainMenuSceneBuilder.Build(true) (ghi đè scene MainMenu) | Chạy từ execute_code khi được phép |

## C. Ghi chú đã xác minh
- 2026-10-23 03:56 UTC: Play Mode mô phỏng cả luồng MainMenu -> Sanh_Cho (3 người) -> SchoolFloor (PartySize 3), Settings đổi và lưu được. Chưa bấm chuột thật, chưa nhìn panel Settings.

## D. Task liên quan
- T-008 REVIEW. Chưa làm: kết nối mạng thật (T-009, chờ chọn công nghệ), trở về MainMenu và Settings trong lúc chơi.
