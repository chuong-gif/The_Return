# T-008 — Màn hình bắt đầu, lập nhóm, Settings, Sảnh chờ (khung online-ready)

Trạng thái: IN-PROGRESS (phiên #4, cửa sổ chat thứ ba, bắt đầu 2026-10-21 10:57 UTC). Chủ dự án chốt (2026-10-21): (1) làm cả KHUNG cho online (lớp trung gian IPartyService), CHƯA nối mạng thật; (2) màn hình bắt đầu = scene MainMenu mới, đặt ĐẦU Build Settings; (3) Settings gồm âm lượng, độ nhạy chuột, toàn màn hình.

## Luồng
MainMenu -> Chơi -> (Tạo phòng / Vào phòng bằng mã) -> phòng chờ nhóm (đủ 2-4 người, ai cũng Sẵn sàng, chủ phòng bấm Bắt đầu) -> Sanh_Cho (LobbyReadyController có sẵn: sẵn sàng + đếm ngược) -> SchoolFloorPrototype. Số người của nhóm đi theo PartySession.

## Kiến trúc (tách luật khỏi hiển thị để sau này host quyết định qua mạng)
- `Features/Party/Runtime/Domain` (asmdef TheReturn.Party.Domain, thuần C#): PartyState (luật: tối đa 4, tối thiểu 2, chủ phòng, sẵn sàng, bắt đầu), PartySnapshot (ảnh chụp bất biến), PartyResult, PartyCode, IPartyService (CreateParty/JoinParty/SetReady/Leave/StartGame + sự kiện Changed), LocalPartyService (một máy, có người giả lập để thử).
- `Features/GameSettings/Runtime/Domain` (TheReturn.GameSettings.Domain): GameSettingsData (âm lượng 0..1, độ nhạy 0,2..3,0 nhân với mặc định, toàn màn hình), ISettingsStore.
- Runtime Unity: PartySession (tĩnh, giữ dịch vụ nhóm + số người), GameSettingsRuntime (PlayerPrefs, áp dụng AudioListener.volume, Screen.fullScreen, PrototypePartyController.SensitivityScale; tự áp dụng khi chạy mọi scene).
- `Maps/MainMenu`: scene MainMenu + MainMenuController/View (uGUI Text/Button/Slider/Toggle như Lobby) + builder Editor dựng scene.
- Nối: LobbyReadyController (số người từ PartySession), SchoolFloorFlow (SetParticipantCount từ PartySession), PrototypePartyController (+SensitivityScale), Build Settings (MainMenu thành scene 0).

## Giả định tôi tự chọn (chủ dự án chưa nói)
1. Tên hiển thị mặc định 'Học sinh n'; mã phòng 4 ký tự (bỏ I, O, 0, 1 cho khỏi nhầm).
2. Chủ phòng = người tạo; chủ phòng rời thì người vào sớm nhất lên chủ phòng; phòng rỗng thì đóng.
3. 'Vào phòng bằng mã' ở chế độ một máy chỉ vào được phòng đã tạo trong cùng phiên chạy (online sau này thay LocalPartyService).
4. Chủ phòng chỉ Bắt đầu được khi từ 2 người và TẤT CẢ (kể cả chủ phòng) đã Sẵn sàng. Bắt đầu rồi thì không ai vào thêm.
5. Để thử một máy: nút '+ Người giả lập' thêm người (tự Sẵn sàng) tối đa 4.
6. Độ nhạy chuột là hệ số nhân (mặc định 1,0 = lookSensitivity hiện tại 0,10).
7. Cài đặt lưu bằng PlayerPrefs, áp dụng ngay khi kéo; không cần nút Lưu riêng.

## Việc đụng file chung
PrototypePartyController (Core, +1 biến tĩnh), LobbyReadyController, SchoolFloorFlow, asmdef TheReturn.Lobby và TheReturn.SchoolFloor (+TheReturn.Party), EditorBuildSettings. KHÔNG sửa SchoolFloorPrototype.unity và Sanh_Cho.unity.

## Tiến độ
- [ ] S1 Domain Party + test
- [ ] S2 Domain Settings + test
- [ ] S3 Runtime (PartySession, GameSettingsRuntime) + nối Core/Lobby/SchoolFloor
- [ ] S4 MainMenu UI + scene + Build Settings
- [ ] S5 Play Mode mô phỏng cả luồng, tài liệu
