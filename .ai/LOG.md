# LOG — Nhật ký sự kiện

> Mục đích: ghi lại thay đổi quan trọng và kết quả xác minh.
> Trạng thái hiện tại: `CURRENT.md`; công việc: `TASKS.md`;
> kế hoạch chi tiết: `PLANS/`; lỗi: `BUGS/`.
> Nhật ký đầy đủ trước khi rút gọn: `SESSIONS/LOG_FULL_20261029.md`.

## Quy tắc ghi LOG

- Chỉ ghi sự kiện có giá trị lâu dài: thay đổi kiến trúc, hoàn thành task, kết quả kiểm thử, lỗi mới hoặc quyết định của chủ dự án.
- Không ghi từng bước thao tác, PREFLIGHT thông thường hoặc nội dung đã có đầy đủ trong tài liệu khác.
- Mỗi mục tối đa 3 dòng, 500 ký tự; chi tiết dài chuyển sang tài liệu chuyên biệt.
- Ghi rõ mức xác minh: 1 = tạo/sửa file; 2 = kiểm tra nội dung; 3 = compile; 4 = kiểm thử thực tế; 5 = chủ dự án xác nhận.
- Chỉ ghi kết quả đã quan sát. Phân biệt rõ điều đã xác minh, điều chưa kiểm tra và giả thuyết.
- Khi tiếp quản task, đọc `CURRENT.md` và `TASKS.md`, sau đó tra LOG theo ID task nếu cần. Không đọc toàn bộ nhật ký cũ mặc định.

---

### 2026-10-22 | T-008, T-011, T-012, T-018
- T-008: hoàn thành khung MainMenu, nhóm chơi cục bộ và Settings; T-011: HUD theo khu và minimap; T-012: tài liệu cho 6 nhiệm vụ mới. T-018 bắt đầu xây dựng hệ thống âm thanh.
- Bằng chứng và giới hạn: `PLANS/T-008-menu-party-settings.md`, `SESSIONS/REVIEW_12_AREAS_20261022.md`; 250/250 test tại thời điểm rà soát 12 khu.

### 2026-10-23 | T-018
- Thêm hệ thống GameAudio, AudioCueBank, AudioService và âm thanh theo nhiệm vụ; 256/256 test, Console 0 lỗi tại thời điểm kiểm tra. Chỉ xác minh Play Mode một phần, chưa nghe kiểm tra đủ 12 khu.
- Cảnh báo lịch sử: từng phát hiện `Features/Audio` trùng kiểu với `Features/GameAudio`; cần xác nhận trạng thái hiện tại trước khi xóa hoặc hợp nhất. Chi tiết trong nhật ký đầy đủ.

### 2026-10-24 | T-019
- Sửa HUD Phong_1: khôi phục sổ tay phím N, biển tên đồng đội và chọn ghế theo mặt phẳng ngang. Play Mode mô phỏng thành công; 256/256 test, Console 0 lỗi tại thời điểm kiểm tra.
- Chưa xác minh đầy đủ bằng thao tác chuột/phím thật và nhiều cấu hình đội: xem LOG đầy đủ nếu tiếp tục kiểm tra T-019.

### 2026-10-28 | T-005
- Bộ Play Mode `TheReturn.SchoolFloor.PlayTests` đạt 10/11; sửa lỗi race trong một test. Còn lỗi `EveryGate_OfEveryMissionArea_StartsItsArea` tại hai cổng Hành lang 1, khu Hanh_Lang_6.
- Báo cáo lỗi: `BUGS/gate-hanh-lang-1-vung-chuan-bi-mac-dinh.md`. Chờ chẩn đoán và sửa lỗi scene.

### 2026-10-29 | T-005
- Sửa vùng chuẩn bị, blocker và VisualRoot của hai cổng Hành lang 1; Play Mode đạt 11/11, EditMode 256/256.
- Chưa xác minh bằng chơi tay; chủ dự án cần kiểm tra vị trí và hình dạng cổng trong game.

### 2026-10-29 | T-008
- Kiểm tra giao diện Settings và phòng chờ qua Play Mode bằng gọi hàm; hiển thị tiếng Việt đúng, lưu thiết lập và luồng MainMenu → phòng chờ → Sanh_Cho hoạt động trong phạm vi đã thử.
- Chưa xác minh toàn bộ luồng vào SchoolFloor trong lần kiểm tra này, thao tác chuột thật, bản build và multiplayer trực tuyến. Chi tiết: `PLANS/T-008-menu-party-settings.md`.

### 2026-10-29 | T-020, T-021
- Chuyển 11 root nhiệm vụ thành prefab: 6 nhiệm vụ mới và 5 nhiệm vụ cũ; giữ tên object và các tham chiếu đã kiểm tra. Mỗi instance có 1 override theo kết quả kiểm tra.
- EditMode 256/256; Play Mode ban đầu 10/11 do lỗi cổng Hành lang 1 có trước thay đổi prefab. Sau đó T-005 sửa lỗi và đạt 11/11. Snapshot, hướng dẫn và chi tiết trong `PLANS/T-020-mission-prefabs.md`, `PLANS/T-021-old-mission-prefabs.md` và `PREFAB_GUIDE.md`.

### 2026-10-29 | T-009 — Network S1
- Tạo khung multiplayer dùng Netcode for GameObjects; host LAN chạy được, client kết nối và thấy vị trí của các client khác đồng bộ trong bài test tự động. Chưa xác minh đầy đủ chiều đồng bộ host → client và bài test sạch chỉ có 2 người.
- Relay tạo phòng được nhưng kết nối Relay server thất bại. Chưa xác định nguyên nhân; không coi multiplayer Internet là hoạt động. Kế hoạch tổng: `PLANS/T-009-online-coop.md`.

### 2026-10-29 | T-009 — Chia task
- Chủ dự án xác nhận: game phát hành qua build cho bạn bè, có voice chat, có yêu cầu chuyển host và đội 2–4 người cố định lúc lập phòng.
- Tạo T-022 đến T-032 và các brief tương ứng. Chuyển host là hạng mục rủi ro, cần thiết kế và đánh giá trước khi triển khai. Quyết định: `DECISIONS/2026-10-29-t009-owner-answers.md`.

### 2026-10-29 | T-010
- Bắt đầu thay khối người chơi bằng `Player.fbx`; trạng thái ban đầu: Generic, NoAvatar, 7 clip không loop, material nhúng chưa có texture. Dự kiến cần xử lý import, material, animation và các PlayerSlot.
- Chưa có kết quả hoàn thành hoặc xác minh trong nhật ký nguồn. Tra `TASKS.md` và `CURRENT.md` trước khi tiếp tục.

### 2026-10-29 | Trạng thái cần lưu ý
- T-005: Play Mode 11/11 sau sửa lỗi cổng; chờ chủ dự án chơi thử.
- T-020/T-021: 11 root nhiệm vụ đã thành prefab; chờ kiểm tra trực quan.
- T-009: LAN đã có kết quả một phần; Relay chưa kết nối; cần thống nhất công cụ test tự động và hoàn tất các bài kiểm thử còn thiếu.
- T-010: thay model người chơi; cần kiểm tra trạng thái thực tế trước khi tiếp tục.
- Các task T-008, T-011, T-012, T-018, T-019: tra `TASKS.md` để biết trạng thái duyệt và việc tồn đọng mới nhất.


### 2026-10-29 12:43 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | START | T-024
- Chủ dự án bảo làm T-024. Phạm vi theo phiếu: tạo mới Features/Network/Runtime/Framework/* (luật thuần C#, tách khỏi Unity) + test EditMode. KHÔNG đụng scene/prefab/NetworkSessionService.cs/Domain nhiệm vụ. Không cần Play Mode, không cần Scene lock.

### 2026-10-29 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | DOCS | T-009
- Sinh lại BOARD (START_HERE.md) sau khi thêm T-022..T-032; sửa mục FREE (thêm mục 0 ONLINE CO-OP, bỏ câu 'không còn task rảnh') và dòng 'Đang chạy' trong CURRENT.md. Phát hiện: phiên #9 đã nhận T-024 lúc 12:43 UTC.

### 2026-10-29 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | OWNER-DECISION | T-009 / T-022
- Chủ dự án trả lời câu mở của T-022: nhiệm vụ co giãn theo đội 2/3/4 người, KHÔNG đổi vai, mỗi người 1 nhân vật cố định, số nhân vật = số người. Ghi vào DECISIONS/2026-10-29-t009-owner-answers.md.
- Cập nhật: phiếu + dòng T-022 (nay chỉ là tài liệu co giãn từng nhiệm vụ), T-025 mở khóa BLOCKED -> TODO, PLANS/T-009 mục 8, START_HERE (FREE + BOARD). Không đụng code/scene.

### 2026-10-30 02:49 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | TAKEOVER-CONTINUE | T-024
- Thấy LOG START T-024 lúc 12:43 (nhãn phiên #9) + 3 file lúc 12:47 (Framework/TeamRoster.cs, 2 asmdef), không có LOG nào sau đó, file không đổi gần 5 giờ -> coi là lần làm trước bị ngắt; chủ dự án vừa bảo làm T-024 nên tiếp tục. Đã đọc TeamRoster.cs: đúng phiếu, GIỮ NGUYÊN (chưa có test).
- Thêm trong Features/Network/Runtime/Framework: CommandResult.cs, IHostRules.cs, HostLog.cs, StateMessage.cs, HostAuthority.cs, ClientReplica.cs. Sắp thêm test trong Features/Network/Tests/EditMode. KHÔNG đụng NetworkSessionService.cs, scene, prefab (T-009/T-023/T-026 của phiên khác).

### 2026-10-30 02:54 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | DONE | T-024
- Khung mạng host-authoritative thuần C# trong Features/Network/Runtime/Framework: HostAuthority, ClientReplica, IHostRules, CommandResult(+RejectReason, RuleVerdict), StateMessage(+NoPrivate), HostLog (cùng TeamRoster + 2 asmdef có từ lần trước). Test trong Features/Network/Tests/EditMode: HostAuthorityTests, ClientReplicaTests, HostLogTests, NetworkFrameworkEndToEndTests (cùng TeamRosterTests + NetworkTestRules.cs có từ trước). Thẻ mới SYSTEM_CARDS/NetworkFramework.md + dòng INDEX.
- Mức 3: compile sạch, 67/67 test EditMode xanh (assembly TheReturn.Network.Framework.Tests); kiểm với đội 2/3/4, lệnh sai bị từ chối + ghi log, luật ném lỗi không sập host, thông tin riêng không lộ, vào lại bắt kịp. CHƯA nối NGO nên chưa có mức 4. Không đụng scene/prefab/NetworkSessionService.cs, không vào Play Mode.
- Sự cố nhỏ: tôi tạo nhầm NetworkFrameworkTestKit.cs (bản trùng ClaimRules/EchoRules, một phiên cùng nhãn #9 đã tạo NetworkTestRules.cs trước đó vài phút). Công cụ chặn xóa file nên chỉ để lại 2 dòng chú thích: chủ dự án xóa tay file này. Kiểm: không còn lỗi biên dịch.
- Bước kế: T-025 (tách PrototypePartyController theo host) và T-027 (viết lớp luật PlankAndKey bọc PlankSession theo IHostRules). Cần T-022 cho thông số nhiệm vụ theo 2/3/4. Câu hỏi: không có.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #6) | DONE (mức 4 một phần: chưa chơi tay, chưa có animation thật) | T-010
- Snapshot scene Scenes/BeforePlayerModel_20261029_105938.unity (bị .gitignore). Sửa SchoolFloorPrototype: 4 PlayerSlot bỏ Visual_Replaceable (capsule) thay bằng instance Core/Prefabs/PF_PlayerModel (Model = Player.fbx, y -0.131 để đế giày chạm sàn); PrototypePartyController.bodies[] đặt null.
- Code: Core/Runtime/Players/PlayerAnimationDriver.cs mới (Speed/Forward/Airborne/Seated, SetBodyVisible, ẩn thân nhưng vẫn đổ bóng); PrototypePartyController: cache drivers, SwitchRole ẩn thân vai đang điều khiển, TickInput truyền seated, ResetPositions bỏ ngồi. Core/Animation/AC_Player.controller (Idle, Walk, WalkBack, Run, Jump, Sit).
- Shader/material: nguyên nhân sai là Player_N chưa đặt kiểu NormalMap (sRGB, DXT1). Đã sửa kiểu, tách material ra Assets/My_Asset/Materials/M_Player.mat (URP Lit). Lúc kiểm lại, M_Player đã có thêm Player_Smoothness.png gắn vào _MetallicGlossMap (ai đó thêm sau) => kênh R thành metallic ~0.7 nên áo trắng bị bóng kim loại; đã đổi sang Player_MetalSmooth.png (R=0 metallic, A=smoothness lấy từ Player_Smoothness) và _Smoothness=1. Chưa nhìn được hình thật (AI không xem được viewport), chủ dự án cần so với Blender.
- Import Player.fbx: Generic + Avatar từ model, clip Idle/Walking/Walking Backwards/Running loop. PHÁT HIỆN: 6 clip không có keyframe (BUGS/player-fbx-clip-khong-co-keyframe.md).
- Test: PlayerModelPlayTests 9 test (7 qua, 2 Inconclusive vì clip tĩnh), toàn PlayMode 18/18, EditMode 323/323.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST START | T-009 S1
- Gộp công cụ test: GIỮ NetworkAutoTest.cs (host+client, log cả 2 phía, đã gắn NetworkRoot), XÓA NetworkS1AutoRun.cs (của mình, compile sạch, 0 lỗi). Bản test: Builds/NetworkS1/NetworkS1.exe (05:36, đã có AutoTest, không cần build lại). Builds/NetworkS1Test (bản cũ có S1AutoRun) đã LỖI THỜI, không dùng.
- Đang chiếm cổng LAN 7777 (Editor = host, scene NetworkS1Test mở, Play Mode). Phiên khác ĐỪNG chạy test mạng/Play Mode đến khi có mục NET-TEST END. Chờ chủ dự án bấm đúp Builds/NetworkS1/chay_client_tu_dong.bat ĐÚNG 1 LẦN.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST END + DONE (mức 4, 2 tiến trình thật) | T-009 S1
- ĐÃ XÁC MINH LAN 2 người (host=Editor NetworkS1Test, client=Builds/NetworkS1/NetworkS1.exe, chay_client_tu_dong.bat, ghi cả Console host lẫn client_auto.log): (1) client kết nối OK, host thấy clients=2, spawned=2 (host owner=0 id=1, client owner=2 id=3), mỗi bên thấy cả hai người chơi. (2) client->host: host thấy player client đi 4.04 -> 11.00 (+8m/4s). (3) host->client: sau khi client vào 7s host AutoMove, client thấy host đi 0 -> 8.01 (khớp x cuối hai phía). OwnerNetworkTransform đồng bộ ĐÚNG cả hai hướng.
- Dọn: GIỮ NetworkAutoTest.cs (-netauto host|client, -netmove, -netquit), XÓA NetworkS1AutoRun.cs. Builds/NetworkS1Test (bản cũ) và Builds/NetworkS1/TheReturnNetS1.exe (05:31) LỖI THỜI, nên xóa tay. Đã dừng Play Mode, scene không dirty.
- CÒN TỒN TẠI / CHƯA RÕ: (a) id client đổi 1 -> 2 trong một lần chạy và NetworkObjectId 2 bị bỏ qua: có vẻ client id 1 vào rồi rời trước khi id 2 vào (host không bao giờ thấy clients=3; chưa rõ do bấm .bat 2 lần hay reconnect tự động). Lần sau chạy test chỉ bấm 1 lần và ghi log host ngay từ đầu. (b) tiến trình NetworkS1.exe vẫn sống sau khi hết -netquit 50 (Application.Quit không thoát hẳn?), cần đóng tay. (c) Relay vẫn lỗi (T-023). (d) chưa thử 3-4 người.
- play=False scene=Assets/TheReturn/Features/Network/Scenes/NetworkS1Test.unity dirty=False tiến trình NetworkS1 còn=1. Cổng 7777 đã nhả: phiên khác dùng được.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | ĐÍNH CHÍNH KẾT QUẢ | T-009 S1
- Chủ dự án cho biết: đã bấm chay_client_tu_dong.bat HAI lần, thấy 2 cửa sổ rồi TẮT TAY một cái. Giải thích: client id 1 vào rồi bị tắt (NetworkObjectId 2 bị bỏ qua, id đổi 1->2), client id 2 ở lại và là bên được đo; 2 tiến trình ghi chung client_auto.log nên log bị lẫn. Mục (a) trong NET-TEST END ĐÃ GIẢI THÍCH, không phải lỗi game.
- Điểm cộng ngoài kế hoạch: 1 client bị tắt đột ngột giữa chừng, host không báo lỗi/cảnh báo nào (Console 0 error, 0 warning), player của client đó được despawn, client còn lại vẫn đồng bộ bình thường. Kết quả đồng bộ 2 hướng vẫn hợp lệ vì host không bao giờ ghi nhận quá 2 client cùng lúc ở các snapshot đã đọc.
- Còn: tiến trình NetworkS1.exe sống sau -netquit (nay còn 1 tiến trình) - nhiều khả năng là cửa sổ bị tắt tay hoặc Application.Quit không thoát hẳn; chưa phân biệt được, test lại 1 client duy nhất khi cần.

### 2026-10-30 03:46 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | START | T-023
- Nhận T-023 (Relay qua Internet). Sẽ sửa phần Relay trong NetworkSessionService.cs (StartRelayHost/JoinRelayByCode) + scene thử NetworkS1Test; KHÔNG đụng luật game/map/Build Settings/manifest. T-026 dùng chung file: sẽ chỉ thêm tham số, không đổi chữ ký hàm công khai. Mức dự kiến: 3 (code+log) cho tới khi có mạng thứ hai để thử mức 4.

### 2026-10-30 03:56 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | START | T-025
- Nhận T-025 (nhân vật online, mỗi người 1 nhân vật cố định). Kế hoạch theo giai đoạn, mỗi giai đoạn có kiểm: (1) SlotAllocator thuần C# trong Features/Network/Runtime/Framework + test EditMode 2/3/4; (2) thêm CHẾ ĐỘ ONLINE vào Core/Runtime/Players/PrototypePartyController.cs (mặc định tắt, chế độ 1 máy giữ nguyên); (3) lớp NGO + prefab/scene thử RIÊNG trong Features/Network (không sửa scene map, không sửa PF_PlayerModel của T-010: chỉ lồng bản thể hiện của nó).
- Tránh đụng #7 (T-023): KHÔNG sửa NetworkSessionService.cs/NetworkAutoTest.cs/NetworkTestHud.cs, chỉ thêm file mới. Sẽ chỉ vào Play Mode / cổng LAN 7777 ở giai đoạn 3 và ghi NET-TEST START/END trước khi dùng.

### 2026-10-30 07:57 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | NET-TEST START | T-025
- Giai đoạn 3 đã dựng: Features/Network/Online (asmdef TheReturn.Network.Online: OnlinePartySlots, OnlineAvatar, OnlinePartyLocalDriver), prefab Features/Network/Prefabs/PF_OnlineAvatar.prefab (CharacterController giống PlayerSlot + NetworkObject + OwnerNetworkTransform + bản thể hiện PF_PlayerModel lồng bên trong, KHÔNG sửa PF_PlayerModel của T-010), NetworkPrefabsList_Party.asset, scene thử Features/Network/Scenes/NetworkPartyTest.unity (bản sao NetworkS1Test của #7, bản gốc không đụng).
- Sắp chạy Play Mode scene NetworkPartyTest, host LAN cổng 7777 (Editor là host, một mình). Phiên khác ĐỪNG vào Play Mode/cổng 7777 đến khi có NET-TEST END của tôi.

### 2026-10-30 08:01 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | NET-TEST END | T-025
- Đã dừng Play Mode, cổng 7777 đã nhả, scene mở lại là SchoolFloorPrototype (không dirty); NetworkPartyTest đã lưu, không dirty. Host một mình xác minh: avatar owner=0 slot=0 tại (-2.25,0.1,0), controller.IsOnline=true, camera gắn vào nhân vật, thân mình ẩn, SwitchRole(1) bị chặn, Console 0 lỗi/cảnh báo.
### 2026-10-30 08:01 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | DONE (mức 3 + mức 4 một phần: host đúng, CHƯA có 2 bản thật) | T-025
- Làm: (1) SlotAllocator + 11 test; (2) chế độ online trong PrototypePartyController (mặc định tắt) + 14 test; (3) Features/Network/Online (OnlinePartySlots, OnlineAvatar, OnlinePartyLocalDriver, asmdef mới), prefab PF_OnlineAvatar, NetworkPrefabsList_Party, scene NetworkPartyTest (bản sao NetworkS1Test, không sửa bản gốc), bản build Builds/NetworkParty/NetworkParty.exe (Windows, development, 158s, 0 lỗi 1 cảnh báo) + chay_client.bat. Thẻ SYSTEM_CARDS/OnlineAvatars.md + INDEX.
- Kiểm: compile sạch; EditMode 361/361; PlayMode SchoolFloor 18/18 (2 Inconclusive cũ); Play Mode host trong NetworkPartyTest như trên. KHÔNG sửa NetworkSessionService/NetworkAutoTest/NetworkTestHud của #7, scene map, PF_PlayerModel, Build Settings, manifest.
- CHƯA: 2 bản mỗi người một nhân vật (chủ dự án cần chạy client theo thẻ mục C), 3-4 người, vào lại giữ ô cũ (T-028), chạy trong SchoolFloor thật (T-027, cần phép sửa scene map). Lưu ý: NetworkAutoTest -netmove của #7 có thể không điều khiển được avatar mới (nó viết cho NetworkTestPlayer), nên thử bằng tay (Tab khóa chuột, WASD). Builds/NetworkParty không nằm trong git.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP + Blender MCP, phiên #6) | DONE (mức 4: Play Mode thật, chưa chơi tay) | T-010 animation
- Chủ dự án cho phép xuất lại FBX từ Blender đang mở. Đọc Player.blend: action có keyframe; nguyên nhân FBX tĩnh là NLA solo Idle (chi tiết BUGS/player-fbx-clip-khong-co-keyframe.md). Xuất lại ra Player_Animated.fbx rồi chép đè Player.fbx; cấu hình lại importer (Avatar, loop, tên clip). Blender: đã khôi phục use_nla/solo/action Idle/frame 42, không lưu .blend (vẫn is_dirty như trước).
- Test: PlayMode 20/20.
