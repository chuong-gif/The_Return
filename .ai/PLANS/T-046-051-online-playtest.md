# KẾ HOẠCH T-046…T-051: Bản build thử online cho người chơi thật   (file: .ai/PLANS/T-046-051-online-playtest.md)

Trạng thái: ĐÃ DUYỆT 2026-11-13 (chủ dự án, chat; P-014 = ĐỒNG Ý) | AI giữ: — | Cập nhật: 2026-11-13 UTC | Soạn bởi: Claude (chat, Unity MCP; chỉ đọc Unity, chỉ ghi `.ai`)

## 1. Mục tiêu và nghiệm thu
- Yêu cầu chủ dự án (2026-11-13): làm một loạt tính năng, rồi build MỘT bản để chủ dự án cùng người thật/máy thật thử, có mic. ƯU TIÊN TỐC ĐỘ SẢN XUẤT: làm bản tối thiểu dùng được, không đánh bóng, không làm ngoài danh sách.
- Nghiệm thu: build Windows chạy được luồng Menu → tạo/vào phòng bằng mã (Relay) → phòng → host Bắt đầu → MapSchoolF cả đội → 4 nhiệm vụ → thắng → về Menu; có tiếng qua mic (Vivox); chế độ offline không đổi.
- Phần AI chứng minh được: M3 (compile/test/Gauntlet) + M4 (build chạy 1–2 tiến trình trên máy chủ dự án). Tiếng thật và chơi nhiều máy = M5 do chủ dự án + người thử xác nhận.

## 2. Hiện trạng đo được (2026-11-13, chỉ đọc)
| Điểm | Kết quả |
|---|---|
| Gói | NGO 2.13.3, Multiplayer Services 2.4.1 (Relay), Vivox 16.12.1, Authentication 3.8.1; Unity 6000.3.7f1; project đã liên kết Unity Cloud |
| Test | 35 test EditMode (các assembly Network khớp tên) đạt; Console 0 lỗi/cảnh báo. KHÔNG thay Gauntlet |
| Build Settings | 8 scene: MainMenu, Sanh_Cho, MapSchoolF, SchoolFloorPrototype, NetworkPartyTest/PlankTest/BreakerTest/ChoirTest. Scene phòng `NetworkRoomTest` KHÔNG có trong Build Settings |
| MainMenu | Không có thành phần mạng nào |
| Sanh_Cho | Chỉ có `LobbyReadyController` (sảnh OFFLINE: đếm ngược cục bộ rồi nạp MapSchoolF), không có mạng |
| Phòng online | `RoomService/RoomHud/RoomMapHandoff` (theo header code) thuộc scene phòng thử `NetworkRoomTest`; `OnlineMapBootstrap` chỉ chạy khi có tham số `-netauto` |
| MapSchoolF | Có `MissionNetBridge` + 4 `*OnlineMission`; không có NetworkManager riêng (đúng thiết kế); T-045 ghi scene này CHƯA LƯU |
| Thoại | `VoiceChatService` + `VoiceHud` chỉ gắn trong `NetworkRoomTest`; KHÔNG có trong `PF_NetworkRoot`, MapSchoolF, Sanh_Cho, MainMenu |
| Player Settings | `runInBackground = false` (alt-tab sẽ làm host/thoại ngừng) |
| Chưa kiểm | Play/online nhiều máy, Vivox đã bật trên Unity Cloud dashboard hay chưa, tiếng thật |

## 3. Phạm vi chung
- Được đụng: đúng file/object ở từng task dưới đây. Sửa scene/prefab: SNAPSHOT trước (theo mẫu Before*), chỉ THÊM/NỐI, không dời/xóa object của người khác.
- KHÔNG đụng: gameplay/thiết kế/bố cục MapSchoolF của chủ dự án; code 8 nhiệm vụ đã cắt (Q-013 vẫn BLOCKED); không commit/push/đổi branch/reset/xóa; không thêm package nếu chưa hỏi; không thay chế độ offline.
- Không ghi «đã chạy» nếu chưa chạy; ghi rõ mức M1–M5.

## 4. Thứ tự và song song (ưu tiên tốc độ)
- Sau khi chủ dự án DUYỆT plan + trả lời P-014: Đợt A chạy song song được vì phạm vi file không giao nhau: T-046, T-047, T-048, T-049, T-050 (T-050 chỉ viết tài liệu, có thể làm ngay không cần chờ).
- Đợt B: T-051 (cổng kiểm) sau khi T-046…T-049 xong. Rồi chủ dự án build/gửi người thử.
- Chủ dự án xác nhận 2026-11-13: AI chạy NỐI TIẾP (hết limit mới đổi AI), không song song; phạm vi file chỉ để tránh sửa chồng khi đổi AI giữa chừng (PROTOCOL §11). Chỗ chung duy nhất: Build Settings (T-046 chỉ THÊM scene phòng; T-049 chốt danh sách cuối) và `NetworkSessionService` (T-048 sở hữu; AI khác muốn sửa phải hỏi).

## 5. Phiếu giao việc (mỗi task AI nhận KHÔNG cần khảo sát thêm)

### T-046 Đường vào online: Menu → phòng → MapSchoolF   [cần P-014 + duyệt]
- Mục tiêu: người chơi online đi được Menu → phòng → map; offline giữ nguyên.
- Được đụng: scene MainMenu (THÊM nút «Chơi online»), scene phòng online MỚI (bản sao của `NetworkRoomTest`, KHÔNG sửa bản thử), `RoomMapHandoff` (mapSceneName=MapSchoolF, avatar prefab PF_OnlineAvatar), Build Settings (chỉ thêm scene phòng).
- Không đụng: `Sanh_Cho` (giữ cho offline), MapSchoolF, logic nhiệm vụ.
- PROVISIONAL: online dùng scene phòng riêng thay sảnh; sảnh Sanh_Cho chỉ cho offline.
- Thoại đã tự chạy nhờ VoiceRoot (T-047): scene phòng mới KHÔNG cần thêm Voice; bản trong NetworkRoomTest tự bỏ khi chạy.
- S1 xác nhận scene phòng thử có đủ RoomService/RoomHud/RoomMapHandoff/NetworkManager. S2 tạo scene phòng thật + nối nút MainMenu. S3 thêm Build Settings. S4 Play Editor: tạo phòng → Bắt đầu → vào MapSchoolF, nhân vật spawn đúng ô (dùng BeginForTest nếu 1 máy không đủ 2 người).
- Xong khi: M3 compile + test Room/Online xanh; M4 host một máy qua được Menu→phòng→map; offline vẫn vào MapSchoolF qua Sanh_Cho.
- Đọc trước: `NETWORK.md`; `RoomMapHandoff.cs` (header + BeginHandoff); `SYSTEM_CARDS/MainMenu.md`, `Lobby.md`.

### T-047 Mic / chat thoại trong game   [REVIEW 2026-11-13: làm bằng singleton VoiceRoot, KHÔNG sửa PF_NetworkRoot/scene — xem LOG]
- Mục tiêu: có tiếng qua mic ở phòng và trong map.
- Được đụng: `Voice/VoiceChatService.cs`, `Voice/VoiceHud.cs`, prefab `Resources/OnlineBootstrap/PF_NetworkRoot.prefab` (thêm Voice; snapshot trước).
- Không đụng: gameplay, scene map, package.
- PROVISIONAL: mic mặc định BẬT khi vào kênh; có nút/phím tắt-bật mic (chọn phím chưa dùng trong Input Actions, ghi vào PLAYTEST.md); HUD hiện «ai đang nói» + trạng thái lỗi (không thấy mic / Vivox lỗi).
- S1 thêm Voice vào PF_NetworkRoot và kiểm NetworkRoot sống qua đổi scene phòng→map (nếu không, kênh bị ngắt khi nạp map). S2 phím tắt mic + HUD. S3 lỗi Vivox không được làm hỏng game (thoại là phụ). S4 kiểm.
- Xong khi: M3 compile; M4 Editor tạo phòng Relay → log `[Voice] CONNECTED` (cần O-2); đổi scene phòng→map không rớt kênh. Nghe thật = M5.
- Đọc trước: `VoiceChatService.cs`; `.ai/NETWORK.md`; `PF_NetworkRoot` (xem component).

### T-048 Rời phòng / mất kết nối / thoát sạch   [REVIEW 2026-11-13: làm bằng SessionExit (Core) + OnlineSessionGuard (Room), KHÔNG sửa scene/prefab; xem LOG]
- Mục tiêu: không treo, không kẹt khi có người rời; vào lại phòng lần 2 không lỗi.
- Được đụng: `NetworkSessionService.cs`, `RoomService.OnClientDisconnect`, `MissionNetBridge`/`MapFlowOnline` (phần rời ván), `WinScreenPresenter` (nút Menu chính: rời phòng + Shutdown rồi mới nạp MainMenu — T-045 ghi «online chưa Shutdown»).
- Không đụng: MainMenu UI, logic nhiệm vụ.
- PROVISIONAL: host thoát/mất kết nối → cả đội về Menu kèm thông báo (đúng quyết định không host migration); client rời giữa ván → kết thúc ván cho cả đội, về Menu kèm thông báo (đúng luật «mọi nhiệm vụ cần cả đội»). Chủ dự án có thể đổi sau.
- S1 host rời trong MAP (RoomService hiện mới xử lý ở phòng). S2 client rời giữa ván. S3 Menu chính/thoát = rời phòng + Shutdown + dọn singleton. S4 xác nhận từ chối vào muộn sau Bắt đầu bằng test hiện có.
- Xong khi: test Room/Online mới xanh + Gauntlet 0 đỏ; M4 host một máy: vào map → Menu chính → vào lại phòng được.
- Đọc trước: `RoomService.cs`; `NetworkSessionService.cs`; `WinScreenPresenter.cs`.

### T-049 Đường ống build thử + chẩn đoán trong bản build
- Mục tiêu: một lệnh ra bản build Windows để gửi người thử, và lấy được log khi lỗi.
- Được đụng: file Editor MỚI (menu `TheReturn/Build/Bản build thử Windows`), Build Settings (chốt danh sách scene), overlay chẩn đoán (có thể dùng lại `NetworkTestHud`).
- Không đụng: ProjectSettings lâu dài — `runInBackground=true` chỉ đặt trong lúc build rồi trả giá trị cũ; gameplay.
- S1 script build: đích `Builds/PlaytestWin64/` (kiểm .gitignore đã bỏ qua `Builds/`), Mono (nhanh), Development Build bật. S2 chốt scene: MainMenu, phòng online (T-046), Sanh_Cho, MapSchoolF; bỏ SchoolFloorPrototype và Network*Test khỏi bản thử [cần P-014]. S3 phím F1 hiện: trạng thái session, mã phòng, số người, RTT, trạng thái thoại, version build; log `[Net]`/`[Voice]` vào Player.log. S4 build thật 1 lần, ghi đường dẫn + dung lượng; chạy exe tới Menu không lỗi.
- Xong khi: M3 build thành công; M4 exe mở tới Menu, F1 hiện đúng.
- Đọc trước: `NetworkTestHud.cs`; `.ai/TOOLING.md`.

### T-050 Bộ tài liệu thử cho người chơi (`.ai/PLAYTEST.md`)   [làm ngay được, chỉ tài liệu]
- Mục tiêu: người thử không cần hỏi mới chơi được; lỗi báo về đủ thông tin.
- Nội dung: yêu cầu máy (Windows 10/11, mic, bật «cho phép ứng dụng desktop dùng micro», Firewall hỏi thì Cho phép; Relay nên không cần mở port); các bước (A tạo phòng + gửi mã, B/C vào bằng mã, Sẵn sàng, chơi 4 nhiệm vụ); phím mic = V (T-047); việc cần quan sát (thấy nhau, nhiệm vụ đồng bộ, tiếng, rời giữa chừng); lấy `Player.log` (`%USERPROFILE%\AppData\LocalLow\<companyName>\The Return\Player.log`, AI điền đúng companyName từ PlayerSettings); mẫu báo lỗi 1 dòng (bước | mong đợi | thực tế | giờ | ai là host).
- Xong khi: M2 (file tồn tại, đủ mục). Cập nhật phím mic sau T-047.

### T-051 Cổng kiểm trước khi gửi chủ dự án   [sau T-046…T-049]
- Mục tiêu: chắc chắn bản build đủ tốt trước khi tốn thời gian của người thử.
- Không sửa code (lỗi tìm ra → thẻ FIX theo PROTOCOL §14.11).
- S1 compile sạch + Gauntlet (`TheReturn/Gauntlet/Chạy Gauntlet mạng`) 0 đỏ; cập nhật số liệu `TESTING.md`/`MISSIONS_NET.md`. S2 smoke 2 tiến trình trên máy chủ dự án (Editor host + 1 exe client, hoặc 2 exe; tham số `-netauto` đã có): tạo/vào phòng Relay → Bắt đầu → vào MapSchoolF → mỗi nhiệm vụ có ≥1 thao tác của client được host chấp nhận → về Menu → vào lại. Thoại kết nối ở cả hai (nghe thật = M5 vì cùng một mic). S3 ghi kết quả, cập nhật `QUEUE.md` (Q-023/Q-024), `NETWORK.md`; báo chủ dự án «sẵn sàng thử người thật» hoặc danh sách lỗi.
- Xong khi: Gauntlet 0 đỏ + smoke 2 tiến trình qua đủ 4 nhiệm vụ (M4).

## 6. Việc của CHỦ DỰ ÁN (AI không làm thay)
- O-1: trả lời P-014 (khuyến nghị ĐỒNG Ý) và duyệt plan này.
- O-2: bật Relay + Vivox cho project trên Unity Cloud dashboard (AI không kiểm được từ Unity; cách xác nhận: tạo phòng Relay trong Editor thấy log `[Voice] CONNECTED`). ĐÃ XÁC NHẬN 2026-11-13: Editor tạo phòng Relay và Vivox vào kênh thành công (Relay + Vivox đã bật).
- O-3: lưu scene MapSchoolF (T-045 đang REVIEW, scene chưa lưu — build dùng bản đã lưu) và commit/tag trước khi build.
- O-4: người thử: Windows 10/11, mic, Firewall; thử 2–4 người để đủ đội.
- O-5: chạy hoặc cho phép AI chạy exe cho smoke T-051 (công cụ AI có thể không mở được tiến trình).

## 7. Rủi ro và giả định PROVISIONAL
- Rời giữa ván = kết thúc cho cả đội (T-048). Mic mặc định bật, phím chọn sau (T-047). Online dùng scene phòng riêng (T-046).
- Relay mặc định WSS (xem NETWORK.md): giữ nguyên; chỉ thử UDP nếu độ trễ tệ.
- Vivox chỉ có ở phòng Relay có mã; phòng LAN không có tiếng.
- Smoke trên một máy dùng chung một mic: chỉ chứng minh kết nối, không chứng minh nghe rõ.
- Nếu `PF_NetworkRoot` bị hủy khi đổi scene thì thoại rớt: xử lý ở T-047 S1, không né bằng cách sửa scene map.

## 8. Bản đồ lỗi (AI sửa lỗi đọc TRƯỚC)
| Triệu chứng có thể gặp | File > hàm / object | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Vào phòng được nhưng không thấy nhau | `NetworkSessionService` (Relay, mã phòng, WSS/UDP) | Player.log `[Net]`; `UseWebSockets` khớp giao thức | T-046, T-049 |
| Bấm Bắt đầu không vào map | `RoomMapHandoff.Phase/Status` | mapSceneName có trong Build Settings; scene map không có NetworkManager | T-046 |
| Vào map nhưng nhân vật không spawn/lệch ô | `OnlinePartySlots`, `OnlineAvatar` | avatarPrefab đã đăng ký; số người đã đặt | T-046 |
| Không có tiếng | `VoiceChatService.StatusText` | lỗi Vivox = dashboard chưa bật; «không thấy micro» = quyền Windows; phòng LAN = không thoại | T-047 |
| Thoại rớt khi vào map | `PF_NetworkRoot` bị hủy khi đổi scene | log `[Voice] LEFT` ngay sau nạp map | T-047 |
| Alt-tab thì treo/mất tiếng | PlayerSettings.runInBackground | bản build có đặt true lúc build chưa | T-049 |
| Vào lại phòng lần 2 lỗi | NetworkManager chưa Shutdown/singleton cũ | `RoomService.Instance`, `NetworkManager.Singleton` sau Menu chính | T-048 |
| Một nhiệm vụ lệch trạng thái trên client | `MissionOnlineClientBase` của nhiệm vụ | `MISSIONS_NET.md` + thẻ FIX | T-051 |

## 9. Tiến độ
- 2026-11-13: plan soạn xong, CHỜ DUYỆT (cần O-1). T-050 làm ngay được.
- 2026-11-13: T-047 xong (REVIEW) bằng singleton VoiceRoot; O-2 (Relay + Vivox) xác nhận chạy được từ Editor. Plan vẫn CHỜ DUYỆT cho T-046/T-048/T-049/T-051 + P-014.
- 2026-11-13: chủ dự án DUYỆT plan + P-014 = ĐỒNG Ý. T-046 dựng xong (OnlineRoom + RoomMapHandoff + nút MainMenu + Build Settings), PAUSED chờ kiểm M4 vì MapSchoolF đang dirty. Ghi chú: `NetworkRoomTest` vốn KHÔNG có `RoomMapHandoff`; đã thêm vào bản sao.
- 2026-11-13: T-046 → REVIEW. Đã kiểm một máy: Menu→OnlineRoom→phòng LAN→MapSchoolF (Phase=Done, avatar spawn), offline qua Sanh_Cho vẫn chạy, EditMode 976/0 fail. Tiếp: T-048 (nút quay Menu + Shutdown), T-049, T-051.
- 2026-11-13: T-048 xong (REVIEW): `Core/Runtime/SceneFlow/SessionExit.cs`, `Network/Runtime/Room/OnlineSessionGuard.cs` (singleton tự tạo), `NetworkSessionService.LeaveSessionAsync`, Room asmdef +TheReturn.Core, `WinScreenPresenter` Menu chính qua SessionExit. Gauntlet XANH; Play LAN một máy qua Menu chính + vào lại phòng + mất mạng giữa ván.
