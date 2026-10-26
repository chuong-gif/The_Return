# T-008 — Màn hình bắt đầu, lập nhóm, Settings, Sảnh chờ (khung online-ready)

Trạng thái: REVIEW (code + scene + Play Mode mô phỏng xong 2026-10-23 03:56 UTC; bắt đầu 2026-10-21 10:57 UTC). Chủ dự án chốt (2026-10-21): (1) làm cả KHUNG cho online (lớp trung gian IPartyService), CHƯA nối mạng thật; (2) màn hình bắt đầu = scene MainMenu mới, đặt ĐẦU Build Settings; (3) Settings gồm âm lượng, độ nhạy chuột, toàn màn hình.

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
- [x] S1 Domain Party + 28 test
- [x] S2 Domain Settings + 13 test
- [x] S3 Runtime (PartySession, GameSettingsRuntime, PlayerPrefsSettingsStore) + nối Core/Lobby/SchoolFloor
- [x] S4 MainMenu UI + scene + Build Settings (MainMenu đứng đầu)
- [x] S5 Play Mode mô phỏng luồng + tài liệu (CHƯA: nhìn tận mắt panel Settings; bấm chuột thật)

## Kết quả (2026-10-23 03:56 UTC)
- Đã làm đúng phạm vi chủ dự án chốt: khung online-ready (IPartyService + LocalPartyService một máy), scene MainMenu mới đầu Build Settings, Settings = âm lượng, độ nhạy chuột, toàn màn hình. KHÔNG nối mạng thật. KHÔNG sửa SchoolFloorPrototype.unity và Sanh_Cho.unity.
- Luồng đã chạy thử trong Play Mode (gọi nút bằng code, không bấm chuột): MainMenu -> Chơi -> mã sai/để trống bị từ chối -> Tạo phòng (mã 4 ký tự) -> +Người giả lập (tối đa 4) -> Bắt đầu bị từ chối khi chưa đủ sẵn sàng -> Sẵn sàng -> Bắt đầu -> nạp Sanh_Cho với participantCount = 3 (đúng số người nhóm) -> sẵn sàng cả 3 -> nạp SchoolFloorPrototype với attendance PartySize = 3. Settings: kéo âm lượng/độ nhạy đổi AudioListener.volume và PrototypePartyController.SensitivityScale, lưu PlayerPrefs, nhãn cập nhật; Mặc định khôi phục. Test: EditMode 256/256 (Party 28, GameSettings 13).
- Chụp màn hình thấy menu chính và phòng chờ nhóm hiển thị đúng tiếng Việt. Panel Settings chưa chụp được (Play Mode bị phiên khác ngắt).
## Việc để lại / lưu ý
- Mặc định Toàn màn hình = BẬT: bản build lần đầu sẽ ép FullScreenWindow (Editor bỏ qua). Đổi DefaultFullscreen trong GameSettingsData nếu muốn mặc định cửa sổ.
- 'Vào phòng bằng mã' chỉ vào được phòng tạo trong cùng phiên chạy (chế độ một máy). Khi chọn công nghệ mạng: viết lớp mới cài IPartyService và gán PartySession.Service.
- Chưa có đường quay lại MainMenu từ trong game, chưa có Settings trong lúc chơi (MainMenuController đã tách nên dùng lại được).
- Esc trong phòng chờ = rời phòng (không hỏi lại).
- Tác dụng phụ: 3 ảnh chụp Assets/Screenshots/screenshot-20261022-1501*.png, -150129.png, -225413.png (công cụ chặn xóa; chủ dự án xóa tay). Trong lúc làm, SchoolFloorCanvasPresenter.cs của phiên khác đang lỗi biên dịch giữa chừng (UpdateNameTags, RoleNameSuffix) — KHÔNG phải của T-008.
