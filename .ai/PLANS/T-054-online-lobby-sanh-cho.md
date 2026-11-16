# T-054: Phòng chờ online ngay trong Sanh_Cho (MainMenu → Sanh_Cho → MapSchoolF)

Trạng thái: ĐANG LÀM (chat #4 tiếp quản AI trước hết lượt) | Chủ dự án yêu cầu 2026-11-14

## 1. Yêu cầu của chủ dự án (nguyên ý)
- Game chỉ còn 3 scene: MainMenu → Sanh_Cho → MapSchoolF. `OnlineRoom` bị thay bằng Sanh_Cho.
- Chủ phòng tạo phòng ở MainMenu, bấm Chơi Online → vào Sanh_Cho chờ; bạn bè nhập mã → vào cùng Sanh_Cho; khách bấm Sẵn sàng.
- Chủ phòng bật/tắt menu xem ai sẵn sàng; mời bất kỳ ai ra; bật/tắt mic và chỉnh âm lượng mic từng người (nghe ở máy chủ phòng).
- Menu của người chơi khác cũng có bật/tắc mic và chỉnh âm lượng mic của nhau.
- Mọi khách sẵn sàng → nút Bắt đầu sáng; bấm thì vào MapSchoolF, đội 2/3/4 theo số người đang có.
- UI là đối tượng Canvas trong scene để chỉnh. Có UI dev (thêm người giả lập, phím 1–4 đổi nhân vật) tách riêng, KHÔNG vào bản build.

## 2. Hiện trạng đã có (AI trước làm, chưa ghi sổ; chat #4 xác nhận bằng code)
- `RoomState`: SetReady, AllGuestsReady, CanStartFlexible (>=2 người, mọi khách sẵn sàng), TryStartFlexible (dồn ô 0..n-1, đội = số người), Kick. KHÔNG có test.
- `RoomRoster` (mạng): cờ Ready trong RoomNetRow, SetReady/Kick/CanStartNow; RoomIdentity.LocalName.
- Menu Cài đặt: prefab `Features/GameSettings/Resources/PF_SettingsMenu.prefab` (301 object, 0 script thiếu; các dòng đã tách file SettingsSliderRow/ToggleRow/SelectorRow/KeyRow/ActionRow). Prefab nằm ở Resources, tự sinh lúc chạy, KHÔNG có trong 3 scene.
- Còn thiếu: NetworkManager đang nằm trong scene OnlineRoom; sảnh offline (LobbyReadyController) và phòng online (RoomRoster) là hai hệ riêng; thoại chỉ có ToggleParticipantMute, chưa có âm lượng từng người.

## 3. Thiết kế
- Prefab mới `Resources/OnlineBootstrap/PF_OnlineSession`: NetworkManager + UnityTransport + NetworkSessionService + RoomService + RoomMapHandoff (cấu hình MapSchoolF, PF_OnlineAvatar), lấy từ NetworkRoot của OnlineRoom, bỏ RoomHud IMGUI. API tĩnh `OnlineSession.Ensure()` sinh khi bấm «Chơi online» (sống xuyên scene).
- MainMenu: panel Online (Canvas) gồm Tạo phòng (tên, tối đa 4) và Vào phòng (mã). Host tạo xong thì NetworkManager.SceneManager.LoadScene("Sanh_Cho"); khách vào xong được NGO tự đồng bộ sang Sanh_Cho.
- Sanh_Cho: `LobbyOnlineController` đọc RoomRoster.Rows đổ vào Canvas người chơi; nút Sẵn sàng (khách), Bắt đầu (chủ, sáng khi CanStartNow), Mời ra, mic/âm lượng từng người. Offline giữ LobbyReadyController.
- UI dev (thêm người giả lập, phím 1–4) là Canvas riêng gắn tag EditorOnly để Unity tự bỏ khỏi build.
- Bắt đầu → RoomMapHandoff nạp MapSchoolF; đội = số người (đã có TryStartFlexible).

## 4. Subtask
| # | Việc | Bằng chứng | Trạng thái |
|---|---|---|---|
| S1 | Prefab Cài đặt hết lỗi script thiếu; xác nhận không nằm trong 3 scene | quét prefab: 0 thiếu | XONG (AI trước; chat #4 kiểm) |
| S2 | Test cho luật RoomState mới (Ready/Kick/StartFlexible) | EditMode 1001: 996 đạt/0 đỏ/5 bỏ qua | XONG (+13 test, file RoomStateFlexibleTests.cs) |
| S3 | PF_OnlineSession + OnlineSession.Ensure; bỏ NetworkManager khỏi OnlineRoom | Play: Menu→Ensure→CreateLan(4)→NGO nạp Sanh_Cho; roster còn, rows=1, teamSize=4, Console 0 lỗi | XONG phần lõi (chưa có nút UI ở MainMenu; OnlineRoom chưa gỡ khỏi Build Settings) |
| S4 | Thoại: SetParticipantVolume + mute từng người cho chủ phòng | code có + compile sạch; UI có nút mic + thanh âm lượng từng hàng; CHƯA thử với 2 người thật qua Vivox | MỘT PHẦN |
| S5 | LobbyOnlineController + Canvas người chơi trong Sanh_Cho | Play 1 máy qua Relay + ảnh: mã phòng, 4 hàng, mic/âm lượng, nút Chờ khách sẵn sàng, Rời phòng, thu gọn (L) | XONG 1 máy; chưa thử khách thật |
| S6 | Panel Online ở MainMenu (tạo/vào bằng mã) | Play: Btn_Online→Panel_Online (ảnh)→TẠO PHÒNG Relay→Hosting mã Q9PKMK→tự vào Sanh_Cho | XONG tạo phòng; VÀO bằng mã chưa thử (cần máy 2) |
| S7 | Canvas UI dev EditorOnly (giả lập người, phím 1–4) | component `DevOnly` (Shared/Runtime/DevOnly.cs) gắn lên Btn_AddSim, Btn_RemoveSim (MainMenu) và PF_LobbyCanvas (Sanh_Cho); giữ trong Editor/Development Build, tự gỡ ở bản build thật; Play Editor: đường dev vẫn chạy | XONG (chưa Build thật để xác nhận; xem rủi ro) |
| S8 | Bắt đầu → MapSchoolF đội đúng số người; gỡ OnlineRoom khỏi Build Settings; Gauntlet + smoke 2 tiến trình | Build Settings còn đúng 3 scene (MainMenu, Sanh_Cho, MapSchoolF) | MỘT PHẦN: Build Settings xong; Bắt đầu→MapSchoolF 2–4 người, Gauntlet, smoke 2 tiến trình CHƯA |

## 5. Rủi ro / hỏi chủ dự án
- Đồng bộ scene NGO: khách phải chuyển Menu→Sanh_Cho cùng host; RoomRoster/handoff phải sống qua đổi scene (đã sống qua Sanh_Cho→MapSchoolF ở T-046).
- Build Settings còn các scene test (NetworkPartyTest...) và OnlineRoom; chủ dự án muốn build chỉ 3 scene → chốt ở S8.
- 2026-11-14 (chat #4): S2 + lõi S3 xong. Prefab `Features/Network/Online/Resources/OnlineBootstrap/PF_OnlineSession.prefab` (NetworkManager, UnityTransport, NetworkSessionService, RoomService, RoomMapHandoff; 0 script thiếu), `Network/Runtime/Room/OnlineSession.cs` (Ensure/Shutdown), `RoomService.Ready`. Lưu ý: trong Sanh_Cho, LobbyReadyController (offline) vẫn chạy song song (participantCount=4 từ PartySession cũ) → S5 phải làm nó biết chế độ online.
- 2026-11-14 18:15 UTC (chat #5 tiếp quản): phát hiện AI trước đã viết xong CODE S4–S6 chưa ghi sổ: `VoiceChatService` (SetParticipantVolume, MicVolume/SetMicVolume), `VoiceHud.Suppressed`, `LobbyReadyController.onlineMode`, thư mục `Network/Runtime/LobbyOnline/` (asmdef + `LobbyOnlineController`, `LobbyOnlineView`/`LobbyOnlinePlayerRow`, `OnlineMenuPanel`), `MainMenuOnlineEntry` sửa 18:10. Compile sạch. CHƯA có object scene nào: Sanh_Cho không có Lobby_OnlineCanvas; MainMenu chưa có panel Online (Btn_Online còn nạp OnlineRoom). Việc kế: dựng Canvas Sanh_Cho (S5), panel MainMenu (S6), UI dev (S7), Build Settings + smoke (S8).
- 2026-11-15 (chat #6 tiếp quản): sửa lỗi compile asmdef Voice (thiếu Unity.Netcode.Runtime); sửa Panel_Online bật sẵn lúc vào MainMenu (chồng lên menu chính); Build Settings chỉ còn 3 scene theo yêu cầu chủ dự án (OnlineRoom + 5 scene test không còn trong build, file scene vẫn còn trong Assets); thêm `DevOnly`. Rủi ro: nút «CHƠI» (offline) ở MainMenu vẫn còn trong bản thật nhưng sảnh dev bị gỡ → vào Sanh_Cho offline sẽ không có UI; chờ chủ dự án quyết ẩn/gỡ nút này ở bản thật.
- 2026-11-16 10:59 UTC (chat #5): XONG S5 (scene Sanh_Cho: `Lobby_OnlineCanvas` + `Lobby_OnlineController`, đã LƯU) và S6 (scene MainMenu: `Panel_Online` + `OnlineMenuPanel`, Btn_Online mở panel, đã LƯU). Sửa lỗi: tách `LobbyOnlinePlayerRow` ra file riêng (Unity mất script nếu hai MonoBehaviour chung một file). Kiểm Play thật: Menu -> TẠO PHÒNG qua Relay (WSS) -> host mã `78W7MR` -> NGO tự nạp Sanh_Cho, bảng hiện đúng 1 dòng chủ phòng, nút Bắt đầu mờ 'Chờ khách sẵn sàng', 1 avatar. CHƯA làm: S7 (UI dev EditorOnly), S8 (Build Settings bỏ OnlineRoom/test, smoke Start->MapSchoolF), vào phòng bằng mã (cần 2 tiến trình), phần slider âm lượng/mic của khách. Backups: `Backups/Sanh_Cho_BeforeT054Canvas.unity.txt`, `MainMenu_BeforeT054Panel.unity.txt`. Chủ dự án ngắt sang việc khác (nhiệm vụ Cầu dao).
