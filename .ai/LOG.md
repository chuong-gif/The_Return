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

### 2026-10-30 08:35 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #8) | START | T-033
- Chủ dự án báo 4 bug khi chơi thật (OWNER_UPDATES). Giai đoạn đầu CHỈ đọc code/tài liệu, chưa sửa. Giả thuyết cần kiểm: bug 1-3 cùng nguyên nhân (nhận diện người chơi trong vùng) và có thể liên quan T-010 của #6 (thay capsule bằng PF_PlayerModel, bodies[] = null).
- PREFLIGHT: play=False, compiling=False, SchoolFloorPrototype dirty=TRUE (không phải của #8: có phiên/chủ dự án khác đang sửa scene) nên #8 KHÔNG lưu scene, không vào Play Mode cho tới khi rõ. Phiên khác đang hoạt động: #7 (T-023), #9 (T-025). Không đụng dòng task của họ.

### 2026-10-30 08:50 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | PROGRESS (mức 3 + mức 4 một phần) | T-023
- SỬA (3 file Features/Network/Runtime, bản gốc ở Builds/_backup_T023/*.bak): NetworkSessionService.cs (thêm RelayProtocolChoice/RelayRegion, WithNetworkOptions cho host+client, tự bật/tắt UnityTransport.UseWebSockets theo giao thức, LAN luôn tắt, bắt OnTransportFailure -> Error + dọn phòng cloud; KHÔNG đổi chữ ký hàm công khai); NetworkTestHud.cs (nút đổi giao thức DTLS/WSS/UDP); NetworkAutoTest.cs (-netauto relayhost|relayclient, -netproto, -netregion, -netcode, log STATE). Compile sạch 0 lỗi. Build lại Builds/NetworkS1/NetworkS1.exe (0 lỗi, có code mới).
- TÌM RA: (1) WSS chạy sai vì UnityTransport.UseWebSockets phải BẬT khi chọn WSS (nếu tắt: NGO báo 'Mismatched Relay configuration' + NullReference trong NetworkManagerSession.cs) - đã sửa trong code; (2) lần thử này host Relay DTLS (mặc định) tạo phòng JFJPGD và ở trạng thái Hosting ổn định ~1 phút, KHÔNG có Transport failure -> lỗi 'Failed to establish connection with the Relay server' hôm qua có thể chập chờn/phụ thuộc thời điểm hoặc mạng, CHƯA kết luận là mạng chặn UDP.
- CHƯA LÀM: host WSS sau khi sửa (lượt thử bị ngắt: Editor chuyển sang SchoolFloorPrototype của phiên #8, dirty=True, mình không đụng); chưa có client vào phòng Relay (cần chạy Builds/NetworkS1 với -netauto relayclient -netcode <mã> -netproto dtls|wss, chủ dự án bấm .bat); chưa thử vùng Relay khác; chưa thử mạng thứ hai (4G). Mặc định giao thức vẫn là DTLS; chưa quyết đổi sang WSS.
- TRẠNG THÁI: không giữ Play Mode, không giữ cổng 7777 (Relay đi qua Internet), scene NetworkS1Test không dirty. T-023 vẫn IN-PROGRESS (#7), sẽ làm tiếp khi Editor rảnh.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST START | T-023
- Chủ dự án xác nhận Editor rảnh (#8 nghỉ). Mở NetworkS1Test, Play Mode để thử Relay (đi qua Internet, KHÔNG chiếm cổng LAN 7777). Phiên khác đừng Play/đổi scene tới khi có mục NET-TEST END.

### 2026-10-30 09:05 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | PROGRESS (mức 4 một phần: WSS qua Relay) | T-023
- ĐÃ XÁC MINH (nguồn: Builds/NetworkS1/client_relay.log, client vào qua Relay thật, mạng của chủ dự án): host Editor tạo phòng WSS mã WHRDT8 (Hosting ổn, 0 lỗi Console); client NetworkS1.exe -netauto relayclient -netproto wss vào được ('Joined - Đã vào phòng WHRDT8 (WSS)'), local id=1, spawned=2, đi 1.50 -> 9.51 và THẤY host đi 0 -> 8.00 (host->client qua Relay OK). Sửa UseWebSockets là đúng.
- CHƯA CÓ BẰNG CHỨNG: phía host thấy client đi (Console host bị xóa bởi lượt build NetworkS1_ban_be 09:00 không phải của mình, Play Mode bị dừng); client DTLS vào phòng Relay; vùng Relay khác; mạng thứ hai. Sẽ chạy lại với DTLS và ghi log host ra file Builds/NetworkS1/host_relay.log để không phụ thuộc Console.
- Lưu ý: Builds/NetworkS1_ban_be (136MB, 09:00) do luồng khác tạo cho bạn bè; chứa thư mục 'The Return_BurstDebugInformation_DoNotShip' không nên gửi.

### 2026-10-30 09:10 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST END + REVIEW (mức 4 qua Relay thật, CÙNG mạng chủ dự án) | T-023
- ĐÃ XÁC MINH 2 giao thức, mỗi lượt đúng 1 client, host ghi file log riêng (Builds/NetworkS1/host_relay_dtls.log) + client log: (1) WSS: phòng WHRDT8, client vào OK, thấy host đi 0->8.00 (nguồn client log). (2) DTLS (mặc định): phòng 8KTT7R, 'Joined - Đã vào phòng 8KTT7R (DTLS)'; HOST thấy clients=2, spawned=2, client đi 0->9.50 (client->host OK); host đi 0->8.00 và CLIENT thấy đúng 8.00 (host->client OK). Không bao giờ clients=3, Console 0 lỗi/0 cảnh báo.
- Lỗi 'Failed to establish connection with the Relay server' của 2026-10-29 KHÔNG tái hiện (3 lần host + 2 lần client thành công hôm nay). Kết luận: nghiêng về chập chờn theo thời điểm/mạng, KHÔNG phải lỗi code; chưa chứng minh được mạng chặn UDP.
- CHƯA XÁC MINH: 2 máy ở 2 MẠNG KHÁC NHAU (chỉ test 2 tiến trình cùng máy, nhưng qua máy chủ Relay trên Internet); vùng Relay khác; 3-4 người; độ trễ thật. Cần bạn bè thử bằng gói trong Builds/. Mặc định giao thức giữ DTLS; nếu bạn bè bị kẹt thì đổi sang WSS bằng nút trên HUD.
- Đóng: đã dừng Play Mode (play=False scene=Assets/TheReturn/Features/Network/Scenes/NetworkS1Test.unity dirty=False tiến trình NetworkS1=0). T-023 chuyển REVIEW chờ bạn bè thử mạng thứ hai.

### 2026-10-30 09:20 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | ĐÍNH CHÍNH + GÓI BẠN BÈ | T-023
- UNLOGGED-CHANGE đã phát hiện: NetworkSessionService.cs sửa lúc 08:59:17 (luồng #7 khác): MẶC ĐỊNH giao thức Relay đổi DTLS -> WSS; build Builds/NetworkS1_ban_be (09:00, dev build) từ bản này. Các lượt test của mình ép giao thức rõ ràng (WSS 08:55, DTLS 09:03) nên kết quả vẫn đúng; câu 'giữ DTLS mặc định' trong mục NET-TEST END là LỖI THỜI: mặc định hiện là WSS (chưa có đo độ trễ WSS so với DTLS).
- Đã tạo Builds/TheReturn_ThuOnline_BanBe.zip (243 file, 136 MB chưa nén; bỏ thư mục 'The Return_BurstDebugInformation_DoNotShip'; kèm HUONG_DAN.txt) để gửi bạn bè thử mạng thứ hai.

### 2026-10-30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | DONE (mức 4, người chơi thật ở mạng khác) | T-023
- CHỦ DỰ ÁN BÁO: đã gửi Builds/TheReturn_ThuOnline_BanBe.zip cho 1 bạn ở TỈNH KHÁC (Windows, Wi-Fi). Thử đủ ma trận 3x3: tạo phòng bằng WSS/UDP/DTLS, và vào phòng bằng cả 3 giao thức cho mỗi loại phòng: MỌI TỔ HỢP đều vào được, thấy nhau, đồng bộ di chuyển tốt. Vậy Relay chạy qua 2 mạng khác nhau thật và các giao thức tạo/vào chéo nhau được.
- Giới hạn của bằng chứng: 1 người thử từ xa, cùng quốc gia, Wi-Fi; chưa có số đo độ trễ (chỉ cảm nhận 'tốt'); CHƯA thử 4G/mạng CGNAT, nước ngoài, 3-4 người, host rớt mạng. Mặc định WSS giữ nguyên (không bắt buộc vì cả 3 giao thức đều chạy).
- Đóng: T-023 chuyển sang TASKS_DONE.md; T-009 (umbrella) nay ghi S1 LAN + Relay xong; T-032 chỉ còn chờ T-028; BOARD đã sinh lại. Công cụ test giữ lại: NetworkAutoTest (-netauto host|client|relayhost|relayclient).

### 2026-10-31 04:50 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | START | T-026
- Nhận T-026 (phòng online, đội 2-4 người cố định). TẠO MỚI: Features/Network/Runtime/Room/* (asmdef TheReturn.Network.Room, RoomRules thuần C# + test, RoomRoster NetworkBehaviour, RoomService, RoomHud IMGUI), prefab PF_RoomRoster, scene riêng NetworkRoomTest (bản sao NetworkS1Test, KHÔNG sửa bản gốc). Có thể thêm MỘT thuộc tính MaxPlayers vào NetworkSessionService.cs (T-023 đã DONE); không đổi chữ ký hàm có sẵn. Không đụng scene map/Build Settings/manifest/Framework của #9. Mức dự kiến: 3 (test) + 4 qua 2 tiến trình LAN/Relay.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST START (chỉ bản build, KHÔNG dùng Editor/Play Mode) | T-026
- Editor đang mở SchoolFloorPrototype dirty=True của phiên khác: mình KHÔNG chuyển scene, KHÔNG Play. Tạo scene NetworkRoomTest bằng mở Additive rồi đóng (scene kia không đổi). Test bằng Builds/NetworkRoom/NetworkRoom.exe (headless). Chiếm cổng LAN 7777 khoảng 1 phút khi chủ dự án bấm chay_thu_phong_LAN_tu_dong.bat; phiên khác đừng chạy host LAN 7777 trong lúc này.

### 2026-10-31 05:00 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | PROGRESS (mức 3 + mức 4 LAN qua 3 tiến trình thật) | T-026
- TẠO MỚI Features/Network/Runtime/Room/{TheReturn.Network.Room.asmdef, RoomState.cs, RoomRoster.cs, RoomService.cs, RoomHud.cs, RoomAutoRun.cs}, Tests/EditModeRoom/{asmdef, RoomStateTests.cs}, Prefabs/{PF_RoomRoster.prefab, NetworkPrefabsList_Room.asset}, Scenes/NetworkRoomTest.unity (bản sao NetworkS1Test, tạo bằng mở ADDITIVE nên SchoolFloorPrototype dirty của phiên khác không bị đụng). SỬA 1 chỗ: NetworkSessionService.cs +thuộc tính MaxPlayers (kẹp 2-4); không đổi hàm có sẵn.
- ĐÃ XÁC MINH: (1) EditMode TheReturn.Network.Room.Tests 14/14 xanh (đội 2/3/4, đầy, chỉ chủ phòng bắt đầu khi đủ người, rời trả ô, chuẩn hóa/chống trùng tên). (2) LAN, 3 tiến trình thật chạy ẩn (Builds/NetworkRoom): host tạo phòng đội 2; khách vào -> cả hai thấy rows=2 (slot0 Chu (H), slot1 Khach); host auto-bấm Bắt đầu -> khách thấy started=True; khách thứ 3 bị TỪ CHỐI với 'Phòng đã đủ người hoặc ván đã bắt đầu' rồi về Idle; host thoát -> khách về Idle với thông báo mất kết nối.
- LỖI NHỎ ĐÃ SỬA (cần build lại): thông báo khi host thoát hiện chuỗi kỹ thuật '[Disconnect Event]...'; đổi thành 'Mất kết nối với chủ phòng'.
- CHƯA: phòng qua Relay (MaxPlayers cloud), đội 3-4 người thật, xem giao diện RoomHud bằng mắt, người vào lại khi đã bắt đầu. Nhiễu Console 'Curl error 6 cdp.cloud.unity3d.com' là dịch vụ Unity Cloud nền, không liên quan phòng.

### 2026-10-31 05:04 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | PROGRESS (S1-S2 xong, S3 chờ điều kiện; mức 3) | T-027
- Chủ dự án bảo «làm T-027» dù T-025 mới ở REVIEW (chưa có 2 bản thật), nên tôi làm phần KHÔNG phụ thuộc T-025/scene: lớp luật host thuần C#. Thêm Features/PlankAndKey/Runtime/Network (asmdef TheReturn.PlankAndKey.Network: PlankCommand, IPlankWorld, PlankPublicState + PlankPrivateState, PlankHostRules bọc PlankSession), test Features/PlankAndKey/Tests/EditModeNet (asmdef TheReturn.PlankAndKey.Network.Tests, 21 test), PLANS/T-027-plank-network-pattern.md (MẪU CHUẨN), ghi chú vào SYSTEM_CARDS/PlankAndKey.md mục C.
- Kiểm: compile sạch; EditMode toàn bộ 400/400 xanh; luật cho đội 2/3/4 gồm ván thắng 3 người qua HostAuthority + ClientReplica (mọi máy thấy thắng, mỗi máy chỉ thấy tốc độ cặp sách của chính mình), hết giờ tự đặt lại, từ chối đúng lý do. KHÔNG sửa Domain PlankAndKey, PlankAndKeyPrototype, scene, prefab, Build Settings; không vào Play Mode; không giữ Scene lock.
- CHƯA làm (mẫu chuẩn mục 6-8): cầu nối NGO dùng chung, cài IPlankWorld từ vị trí avatar, Presentation online của PlankAndKey, Play Mode 2 máy thắng/thua. Đặt T-027 về BLOCKED vì thiếu: (a) chủ dự án thử 2 bản T-025, (b) phép sửa scene map nếu muốn đưa vào Hanh_Lang_6 thật (scene thử riêng trong Features/Network thì không cần phép). Ghi chú cho T-022: PlankGenerator đã có danh mục 2/3/4 người (hạn chế cũ: 2 người gần như không thử thách).

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #6) | DONE | T-010 (bổ sung)
- Chủ dự án xóa tay Player_Animated.fbx thì các vai mất model. Nguyên nhân: PF_PlayerModel (nhánh Model + avatar) và AC_Player (motion các state) đang tham chiếu Player_Animated.fbx, KHÔNG phải Player.fbx như phiên #6 tưởng; lúc đó phiên #6 nói sai file cần xóa. Không rõ ai đổi tham chiếu (prefab/controller ghi lại lúc 15:08-15:09 ngày 2026-10-30; Model y đã là -0.129 thay vì -0.131, nghĩa là có người chỉnh tay). Đã dựng lại nhánh Model từ Player.fbx, gắn lại motion + avatar của Player.fbx, quét toàn bộ Assets không còn tham chiếu GUID Player_Animated.fbx => xóa được. PlayerModelPlayTests 9/9.
- Chủ dự án báo: chưa nhảy được. Thêm phím Space (PrototypePartyController: allowJump=true, jumpHeight=0.8, hàm tĩnh NextFallSpeed, Gravity=20) + PlayerJumpTests (4 test EditMode) + PlayerAnimationDriver phát animation nhảy sớm hơn (airborneDelay 0.06, raycast 0.3). Chưa thử bằng tay (AI không bấm phím được). Clip Jump trong Player.fbx đứng tại chỗ (hông dịch tối đa 0.19 m), tư thế là cú nhảy chồm tới; nếu chủ dự án thấy nhân vật bị đẩy tới trước thì cần xem lại.
- EditMode 400/400. Nhảy có thể bỏ qua một số chướng vật thấp (0.8 m); khe hành lang vẫn quá rộng để nhảy qua (nhảy xa ~2.7 m khi chạy).

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | START | T-022
- Chủ dự án bảo tự tìm task rảnh. T-022 (chỉ tài liệu, không đụng code/scene) và T-030 đều chưa ai giữ; chọn T-022 vì mở khóa T-031 và chốt thông số cho T-027. Sẽ tạo PLANS/T-022-design-2to4.md và cập nhật PLANS/T-009 mục 8; câu hỏi gameplay mơ hồ ghi cho chủ dự án.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | DONE (chỉ tài liệu, chờ duyệt) | T-022
- Tạo PLANS/T-022-design-2to4.md: bảng 12 nhiệm vụ x N=2/3/4 (nhãn [CÓ] / [THEO THIẾT KẾ] / [CẦN KIỂM] / [ĐỀ XUẤT]), 8 câu hỏi cho chủ dự án có mặc định, cách dùng cho T-027/T-031. Chỉ đọc code/README/thẻ hệ thống (không chạy test, không đo scene). Phát hiện: cả 12 Domain đã nhận 2-4 người; không nhiệm vụ nào đòi vai cố định ngoài người số 0; PlankAndKey N=2 cần quyết định. Cập nhật PLANS/T-009 mục 8. Không sửa code/scene.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST START (Relay, chỉ bản build) | T-026
- Đã build lại Builds/NetworkRoom (0 lỗi, 0 cảnh báo; sửa thông báo mất kết nối + -roomcodefile). Chạy kịch bản Relay đội 3 người: host WSS, khách A DTLS, khách B UDP vào bằng mã, khách C (WSS) vào sau phải bị từ chối. Đi qua Internet, KHÔNG dùng cổng LAN 7777, KHÔNG dùng Editor/Play Mode.

### 2026-10-31 10:05 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST END + REVIEW (mức 4: 4 tiến trình qua Relay thật) | T-026
- ĐÃ XÁC MINH (Builds/NetworkRoom/relay_*.log, Internet thật, không dùng Editor): host tạo phòng ĐỘI 3 bằng WSS (mã HJGTPW); khách A vào bằng DTLS, khách B bằng UDP (3 giao thức khác nhau trong cùng phòng); cả 3 thấy cùng danh sách 3 ô (Chu (H), KhachA, KhachB); host auto Bắt đầu khi đủ 3 -> hai khách thấy started=True; khách C (WSS) vào sau bị từ chối đúng ('SessionException: session is full' -> trạng thái Lỗi); khi khách thoát host thấy rows 3 -> 1, không có Exception nào ngoài 'session is full'. Trước đó LAN đội 2 (3 tiến trình) cũng đạt, khách thứ 3 bị từ chối 'Phòng đã đủ người hoặc ván đã bắt đầu'.
- SỬA SAU TEST: RoomService thông báo 'Mất kết nối với chủ phòng' (thay vì chuỗi '[Disconnect Event]...'); NetworkSessionService.Fail đổi 'session is full' -> 'Phòng đã đủ người'. Compile sạch, build lại (0 lỗi, 0 cảnh báo) nhưng 2 sửa này CHƯA chạy lại trong kịch bản tự động (chỉ đã compile + build).
- CHƯA: xem RoomHud bằng mắt (Builds/NetworkRoom/chay_phong_thu_cong.bat), đội 4 người thật, test 'chủ phòng thoát giữa chừng' bằng bản build mới, nối phòng -> nạp map (T-028), voice (T-029). T-026 -> REVIEW; T-029 mở khóa. Cổng 7777 đã nhả. Editor không bị dùng.

### 2026-10-31 10:15 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | START | T-029
- Nhận T-029 (chat giọng nói Vivox theo phòng). TẠO MỚI: Features/Network/Runtime/Voice/* (asmdef TheReturn.Network.Voice, VoiceChatService, VoiceHud, VoiceAutoRun) và gắn vào scene NetworkRoomTest (mở Additive, không đụng scene đang mở). KHÔNG đổi manifest/Relay/luật phòng. Vivox cần bật trong Unity Cloud dashboard: nếu bị từ chối sẽ báo chủ dự án. Mức dự kiến: 3 + 4 một phần (vào kênh, thấy nhau); nghe thật cần 2 người + micro.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST START (Relay + thoại, chỉ bản build) | T-029
- Đã tạo Features/Network/Runtime/Voice/{asmdef, VoiceChatService.cs, VoiceHud.cs}, gắn vào NetworkRoomTest (Additive, SchoolFloorPrototype không đụng), build lại Builds/NetworkRoom (0 lỗi/0 cảnh báo). Sắp chạy kịch bản Relay 2 người có thoại. KHÔNG dùng Editor/Play Mode, KHÔNG dùng cổng LAN 7777.

### 2026-10-31 10:19 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #8) | PROGRESS | T-033
- Chủ dự án nhắc: lỗi 1 và 3 giống nhau, tìm nguyên nhân gốc chung rồi sửa cho TẤT CẢ phòng/hành lang. PlayMode hiện 20/20 xanh (không bắt được lỗi) nên đang đọc MissionRegistry/MapProgress/bảng cổng để tìm điều kiện test không phủ. Chưa sửa gì. Editor.log: không có exception liên quan.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | DONE | T-022 (duyệt)
- Chủ dự án: 8 câu cứ theo mặc định nếu AI thấy ổn nhất. AI kiểm thêm (chỉ đọc): đo scene SchoolFloorPrototype (bục Choir cách 3/4/3 m, hai đầu 10 m; 4 trạm TeacherTruth cách nhau 2,4-3,8 m; đèn lồng Point range 6 m), đọc TeacherSaysGenerator.IsValid (người nghe khác đoạn trước, đủ mọi người), ChoirRules.Online (2,0 s cố định). Kết quả: 7 mặc định giữ, 1 đổi (Choir online 3/2,5/2 s theo N). Sửa 1 chỗ SAI trong bản đề xuất đầu (câu 6, TeacherSays). Ghi DECISIONS/2026-10-31-t022-team-size.md; T-022 chuyển sang TASKS_DONE; T-031 chỉ còn chờ T-027; BOARD đã sinh lại.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | START | T-030 (bước 1: tài liệu thiết kế)
- Chủ dự án giao làm tiếp task rảnh. Chỉ viết PLANS/T-030-host-migration-design.md (theo T-030-brief), KHÔNG code, KHÔNG đụng Features/Network, Domain, scene. Có tra tài liệu NGO/Unity Multiplayer Services trên web.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | DONE (chỉ tài liệu, chờ duyệt) | T-030 bước 1
- Tạo PLANS/T-030-host-migration-design.md: sự thật đã tra (NGO không có chuyển host; Sessions chỉ có bầu host + IMigrationDataHandler mặc định cho Entities; Relay hết hiệu lực khi host mất), hiện trạng repo (đọc code), 6 kịch bản, 4 phương án (khuyến nghị C: nhật ký sự kiện + bầu ô nhỏ nhất), cái sẽ mất, 9 rủi ro, kế hoạch S0-S6 (9-16 phiên AI ước lượng), 7 câu hỏi cho chủ dự án. Không chạy thử mạng, không sửa code/scene/Domain. Việc cần chủ dự án: duyệt hướng và giúp chạy S0 với 2 máy.

### 2026-10-31 10:54 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | PROGRESS (chẩn đoán T-pose bản build) | T-025
- Chủ dự án thử 2 bản (T-025): nhân vật vào phòng đúng, host trong Editor có animation, nhưng client chạy bằng chay_client.bat chỉ hiện dáng chữ T. Log client_*.log xác nhận mạng đúng (local=1/2/3, vị trí nhân vật khác đổi theo), không có cảnh báo về Animator. Tĩnh: AC_Player/clip FBX 660 đường cong, Player.fbx Generic không optimize, controller/avatar hợp lệ, GPU skinning bật, Quality PC FourBones => chưa thấy nguyên nhân.
- Thêm chẩn đoán TẠM Features/Network/Online/OnlineAnimDiag.cs, gắn vào PF_OnlineAvatar.prefab (ghi [AnimDiag] vào log mỗi 1,5 giây, 12 lần) và build lại Builds/NetworkParty (development, 89s, 0 lỗi). Chờ chủ dự án chạy lại rồi đọc Builds/NetworkParty/client_*.log. Gỡ OnlineAnimDiag khi xong. Chủ dự án CHO PHÉP sửa scene map (ghi nhận để làm T-027 phần Hanh_Lang_6).
- Build Settings hiện: MainMenu.unity | Sanh_Cho.unity | SchoolFloorPrototype.unity | NetworkS1Test.unity | NetworkPartyTest.unity (NetworkS1Test/NetworkPartyTest có mặt, chưa xác định ai thêm, không tự sửa).

### 2026-10-31 11:01 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | PROGRESS (T-pose đã hết ở bản build mới) | T-025
- Chủ dự án chạy lại: nhân vật trong bản build có animation như Editor. Log client_9771: slot 0 (không phải owner) và slot 1 (owner) đều Idle rồi Walking (speedParam 3.16), hipsRot đổi => Animator chạy đúng trên cả nhân vật của mình lẫn nhân vật người khác.
- NGUYÊN NHÂN CHƯA XÁC ĐỊNH. Khác biệt giữa hai bản build chỉ là: prefab PF_OnlineAvatar được lưu lại (thêm OnlineAnimDiag) và biên dịch lại. Không khẳng định được cái nào sửa. Giữ OnlineAnimDiag trong prefab đến khi chủ dự án đồng ý gỡ; nếu dáng chữ T tái xuất thì đọc [AnimDiag] trong client_*.log.
- Chủ dự án đã CHO PHÉP sửa scene map: T-027 phần Hanh_Lang_6 có thể tiếp (mẫu: PLANS/T-027-plank-network-pattern.md mục 6-8). Chưa bắt đầu.

### 2026-10-31 11:06 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | START | T-027 (S3)
- Chủ dự án: «làm đi» sau khi thử 2 bản T-025 xong và CHO PHÉP sửa scene map. Làm theo thứ tự: (1) bổ sung luật host: vị trí thẻ (PlankCardMode + toạ độ làn) để mọi máy vẽ thẻ giống nhau; (2) PlankCodec thuần C# (lệnh/trạng thái <-> chuỗi) + test; (3) MissionNetBridge (NGO, dùng chung, Features/Network/Online); (4) PlankOnlineHost/PlankOnlineClient (asmdef mới TheReturn.PlankAndKey.Online); (5) scene thử RIÊNG NetworkPlankTest; (6) CHỈ SAU ĐÓ mới đụng SchoolFloorPrototype (sẽ tạo bản Before... và kiểm Scene lock trước).
- Tránh đụng phiên #8 (T-033 Voice) và #7 (T-026 Room): KHÔNG sửa NetworkSessionService.cs, Runtime/Room, Runtime/Voice. Chỉ file mới trong Features/Network/Online và Features/PlankAndKey. Sẽ ghi NET-TEST START/END trước khi vào Play Mode / cổng 7777.

### 2026-10-31 15:30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #7) | NET-TEST END + REVIEW (mức 3 + mức 4 một phần: vào kênh thoại qua Relay thật) | T-029
- TẠO MỚI: Features/Network/Runtime/Voice/{TheReturn.Network.Voice.asmdef, VoiceChatService.cs, VoiceHud.cs}; gắn vào scene NetworkRoomTest (mở Additive, scene đang mở của phiên khác không đụng). Không sửa manifest/Relay/luật phòng. Compile sạch 0 lỗi; build Builds/NetworkRoom 0 lỗi 0 cảnh báo.
- ĐÃ XÁC MINH (voice_host.log + voice_client.log, 2 tiến trình thật qua Relay WSS, Vivox đã BẬT trong dự án): phòng 7J7QJH -> mỗi bên tự vào kênh 'room-7J7QJH' (Vivox InitializeAsync/LoginAsync/JoinGroupChannelAsync OK), micDevices=5, activeMic='Default System Device'; host thấy n=2 (Chu(tôi), Khach); client thấy n=2 (Chu, Khach(tôi)); khi host thoát client thấy n=1 rồi về Off + 'LEFT channel'. 0 Exception, 0 cảnh báo.
- CHƯA XÁC MINH (cần 2 người thật + micro/loa): nghe/nói thật, chất lượng/độ trễ, chấm 'đang nói', nút Mic tắt/bật và Tắt tiếng từng người có hiệu quả đúng, quyền micro Windows ở máy bạn bè, đội 3-4 người. Phòng LAN không có thoại (không có mã phòng). Ghi chú: mic lúc vào kênh mặc định BẬT.
- Đã tạo Builds/TheReturn_PhongThoai_BanBe.zip (89 MB, bản có cửa sổ NetworkRoom.exe + HUONG_DAN.txt tiếng Việt, đã bỏ .bat/.log/DoNotShip). T-029 -> REVIEW; T-032 (build bạn bè) nên gộp nội dung hướng dẫn này.

### 2026-10-31 UTC | Claude Sonnet 5.5 (chat, phiên #6) | HỦY | T-030
- Chủ dự án bỏ chuyển host (host off thì cả đám rời phòng) để game xong nhanh. Ghi DECISIONS/2026-10-31-t030-no-host-migration.md (ghi đè câu 3 của 2026-10-29); T-030 sang TASKS_DONE trạng thái HỦY; đánh dấu HỦY đầu PLANS/T-030-host-migration-design.md; sửa brief T-028, PLANS/T-009; BOARD sinh lại. Không đụng code. Mặc định AI (chưa được xác nhận): ai rời giữa ván thì ván kết thúc, không vào lại.
