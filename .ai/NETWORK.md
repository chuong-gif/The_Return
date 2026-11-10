# NETWORK.md — Tài liệu gốc về online co-op (cập nhật 2026-11-04, phiên #7)

Đọc file này TRƯỚC khi làm bất cứ việc nào liên quan mạng (T-009, T-023..T-038). Các phiếu/kế hoạch task chỉ nói phần riêng của task; phần chung nằm ở đây. Mức xác minh dùng ký hiệu của LOG.md: M1 tạo/sửa file, M2 kiểm nội dung, M3 compile/test tự động, M4 chạy thật (Play Mode/build/nhiều tiến trình), M5 chủ dự án xác nhận.

## 1. Quyết định đã chốt (chủ dự án) — đừng hỏi lại
- Phát hành: CHỈ gửi bản build cho bạn bè (không Steam). Mạng: Netcode for GameObjects + Unity Relay (mã phòng) + Vivox. Host là MỘT NGƯỜI CHƠI (không có máy chủ riêng). DECISIONS/2026-10-22-t009-network-choice.md, DECISIONS/2026-10-29-t009-owner-answers.md.
- Đội 2-4 người, CỐ ĐỊNH từ lúc lập đội. KHÔNG đổi vai: mỗi người đúng 1 nhân vật cả ván; số nhân vật = số người. Nhiệm vụ co giãn theo đội: PLANS/T-022-design-2to4.md + DECISIONS/2026-10-31-t022-team-size.md.
- Có chat giọng nói (Vivox). KHÔNG chuyển host: host thoát thì cả nhóm rời phòng (T-030 đã HỦY, DECISIONS/2026-10-31-t030-no-host-migration.md).
- Chế độ chơi 1 máy (phím 1-4 đổi vai) vẫn giữ nguyên, đừng phá.

## 2. Kiến trúc (một đoạn)
HOST-AUTHORITATIVE: client chỉ gửi LỆNH (ô người gửi do host lấy từ kết nối, không tin trong lệnh); host kiểm, chạy luật thuần C# rồi gửi TRẠNG THÁI: phần CHUNG (mọi người thấy) và phần RIÊNG (chỉ một người, vd manh mối). Client chỉ vẽ lại từ bản sao (ClientReplica). Thông tin riêng KHÔNG nằm trong trạng thái chung. Host không tin vị trí client tự khai: lớp IWorld của host đọc vị trí avatar mà host thấy. Luật nằm trong assembly thuần C# (noEngineReferences) để test EditMode không cần Unity/mạng.

## 3. Bản đồ code (thư mục -> vai trò). Đồ thị đầy đủ: MAPS/asmdef-graph.md
Features/Network/Runtime (asmdef TheReturn.Network):
- NetworkSessionService: phiên LAN (127.0.0.1:7777) và Relay (Unity Multiplayer Services Sessions). Chọn giao thức Relay DTLS/UDP/WSS (mặc định WSS, cổng 443, vượt tường lửa tốt hơn; WSS BẮT BUỘC bật UnityTransport.UseWebSockets, code tự bật/tắt), vùng Relay, MaxPlayers 2-4. Mất kết nối Relay -> trạng thái Error + dọn phòng cloud; phòng đầy -> 'Phòng đã đủ người'.
- NetworkTestHud, NetworkTestPlayer, OwnerNetworkTransform (owner điều khiển transform của mình), NetworkAutoTest (kiểm thử tự động, xem mục 5).
Features/Network/Runtime/Framework (TheReturn.Network.Framework, noEngine, thuần C#): TeamRoster, SlotAllocator, HostAuthority, ClientReplica, IHostRules, CommandResult, StateMessage, HostLog. Thẻ: SYSTEM_CARDS/NetworkFramework.md.
Features/Network/Runtime/Room (TheReturn.Network.Room): RoomState (luật phòng, thuần), RoomRoster (NetworkList đồng bộ danh sách), RoomService, RoomHud, RoomAutoRun; prefab PF_RoomRoster; chỉ host Bắt đầu, chỉ khi đủ số người đã chốt; người vào sau khi đủ/đã bắt đầu bị từ chối.
Features/Network/Runtime/Voice (TheReturn.Network.Voice): VoiceChatService, VoiceHud. Tự vào kênh Vivox 'room-<mã phòng>' khi vào phòng Relay, rời khi rời phòng. Phòng LAN KHÔNG có thoại (không có mã phòng). Mic mặc định BẬT. Có tắt mic của mình và tắt tiếng từng người (chỉ trên máy mình).
Features/Network/Online (TheReturn.Network.Online, T-025): OnlinePartySlots (ô <-> client), OnlineAvatar (nhân vật online), OnlinePartyLocalDriver, MissionNetBridge (cầu gửi lệnh/trạng thái dạng chuỗi cho mọi nhiệm vụ; chuỗi tối đa 2000 ký tự), IMissionHostEndpoint, IMissionClientEndpoint. Thẻ: SYSTEM_CARDS/OnlineAvatars.md.
Mỗi nhiệm vụ nối mạng có hai assembly: Features/<Tên>/Runtime/Network (TheReturn.<Tên>.Network, noEngine: Command, IWorld, PublicState/PrivateState, HostRules, Codec) và Features/<Tên>/Runtime/Online (TheReturn.<Tên>.Online: host, client, AutoRun). Đã có: PlankAndKey, Attendance; BreakerPanel (phiên #10 đang làm). Luồng map: Maps/SchoolFloor/Online (T-028, code xong, đang dừng chờ chủ dự án).
Scene thử mạng (Features/Network/Scenes): NetworkS1Test (nền LAN/Relay), NetworkPartyTest (avatar), NetworkRoomTest (phòng + thoại), NetworkPlankTest, NetworkAttendanceTest. Mỗi nhiệm vụ mới có scene thử riêng; KHÔNG sửa scene map nếu chủ dự án chưa cho phép.
Test: Features/Network/Tests/{EditMode (khung), EditModeOnline, EditModeRoom, EditModeFlow}, Features/<Tên>/Tests/EditModeNet. Số liệu: TESTING.md.

## 4. Mẫu nối MỘT nhiệm vụ (8 bước; bản đầy đủ ở PLANS/T-027-plank-network-pattern.md; mẫu mới nhất: Features/Attendance/Runtime/{Network,Online})
1. Đọc README + thẻ hệ thống + Domain + Presentation của nhiệm vụ và hàng của nó trong PLANS/T-022-design-2to4.md.
2. Domain chưa có test NUnit thì viết test đặc tả TRƯỚC (không sửa Domain).
3. Lớp mạng thuần C#: Command (không mang ô người gửi), I<Tên>World, PublicState/PrivateState (bất biến, sao mảng khi vào/ra), HostRules : IHostRules (Validate không đổi trạng thái; Apply chỉ chạy lệnh vừa Validate; Tick(deltaTime) cho đồng hồ; ReleaseSeat-tương-đương khi người mất kết nối), Codec.
4. Codec: chuỗi tự do (tên, manh mối có xuống dòng/tiếng Việt) thoát bằng Uri.EscapeDataString; giải mã dữ liệu xấu TRẢ false, không ném lỗi; kiểm độ dài <= 2000 ký tự ca xấu nhất.
5. Test EditModeNet: đội 2/3/4; từ chối đúng lý do; lệnh bị từ chối KHÔNG đổi Version; thông tin riêng không lọt; thắng + thua; Tick/hết giờ; mất kết nối; ván thắng qua HostAuthority + ClientReplica.
6. Lớp Online: host (dựng HostRules khi OnlinePartySlots/bridge sẵn sàng, lấy ô từ slots, phát trạng thái đúng từng người khi Version đổi, Tick -> MarkChanged), client (phím/HUD gửi lệnh, vẽ từ bản sao, đưa nhân vật CỦA MÌNH tới điểm ngồi/đứng), AutoRun.
7. Scene thử riêng (mục 6) + build + kịch bản nhiều tiến trình (mục 5).
8. Thẻ hệ thống thêm mục Mạng; LOG 1-3 gạch đầu dòng; cập nhật bảng task.
Bài học từ Attendance: gói manh mối <=128 ký tự nhưng có xuống dòng và tiếng Việt (nên cần escape); ghế/điểm trong scene dựng cố định theo 4 người thì N<4 phải bỏ phần thừa; luật đúng/sai chấm sau thời gian chờ (Tick) trên host.

## 5. Kiểm thử mạng nhiều tiến trình
Mức: EditMode (M3) -> bản build chạy nhiều tiến trình cùng máy (M4; KHÔNG phải kiểm chứng nhiều máy/mạng) -> người thật 2 máy (M5, chủ dự án).
Công cụ tự động (chỉ bật khi có tham số, không ảnh hưởng game thường); mỗi tiến trình ghi log '[...]' mỗi giây:
- NetworkAutoTest (-netauto host|client|relayhost|relayclient, -netmove, -netquit giây, -netproto dtls|wss|udp, -netregion, -netcode MÃ).
- RoomAutoRun (-roomauto lanhost|lanclient|relayhost|relayclient, -roomsize N, -roomname, -roomcode, -roomproto, -roomstart, -roomquit, -roomcodefile ĐƯỜNG_DẪN: host ghi mã phòng, client đọc).
- AttendanceOnlineAutoRun (-attauto sit|solve, -attseat, -attteam, -attsolfile, -attdelay, -attquit, -attstand). Mỗi nhiệm vụ mới tự làm AutoRun theo mẫu này; solve = host ghi đáp án ra file, khách đọc rồi ngồi đúng.
Cách chạy: build bằng manage_build (development, CHỈ scene thử) ra Builds/<Tên>/; công cụ execute_code KHÔNG cho gọi Process.Start nên AI tạo file .bat và NHỜ NGƯỜI DÙNG bấm đúp ĐÚNG MỘT LẦN. Dùng -batchmode -nographics cho kiểm luật (không thấy giao diện IMGUI). Mẫu .bat an toàn:
  @echo off / cd /d %~dp0 / nếu có file run.lock thì báo và thoát / tạo run.lock / set SOL=%~dp0sol_%RANDOM%%RANDOM%.txt / start '' Game.exe -batchmode -nographics -netauto host ... -logFile '%~dp0host.log' / ping -n 4 127.0.0.1 >nul / start ... client ... / ping -n (quit+25) 127.0.0.1 >nul / xóa run.lock / pause.
Luật của .bat kiểm thử: (a) KHÔNG taskkill giữa chừng: Unity chỉ xả nốt log khi thoát bình thường, kill sẽ cắt mất bằng chứng; (b) chờ đủ lâu cho tiến trình tự thoát; (c) khóa chống chạy trùng (bấm hai lần làm chồng tiến trình và tranh cổng 7777); (d) mỗi tình huống một tên log riêng; (e) đọc log bằng FileShare.ReadWrite; (f) host một mình (Editor/Play Mode) kiểm được lắp ghép nhưng KHÔNG chứng minh đồng bộ nhiều máy.
Mạng thật: lỗi Relay 'Failed to establish connection' của 2026-10-29 chỉ xuất hiện một lần và không tái hiện; kết quả M5: chủ dự án + bạn khác tỉnh (Wi-Fi) tạo/vào phòng đủ ma trận WSS/UDP/DTLS.

## 6. Dựng scene thử mà không đụng scene đang mở (đã dùng nhiều lần)
Editor thường đang mở SchoolFloorPrototype (nhiều khi có thay đổi chưa lưu của phiên khác). Cách an toàn: AssetDatabase.CopyAsset(scene mẫu -> scene mới); EditorSceneManager.OpenScene(..., Additive); LƯU Ý new GameObject() tạo vào scene HOẠT ĐỘNG nên phải EditorSceneManager.SetActiveScene(sceneMoi) trước khi tạo object rồi SetActiveScene(sceneCu) sau (dùng try/finally); PrefabUtility.InstantiatePrefab(prefab, scene) đặt thẳng vào scene; SaveScene(sceneMoi); CloseScene(sceneMoi, true). Kiểm scene đang mở có isDirty trước/sau giống nhau. Nếu cần OpenScene Single thì chỉ làm khi scene hiện tại isDirty = false.

## 7. Dùng chung Editor và máy (một Editor, một lúc một phiên)
- Một Editor Unity chỉ phục vụ MỘT phiên thật sự tại một thời điểm: Play Mode, đổi scene, compile, build và chạy test đều là toàn cục. Compile khi phiên khác đang Play Mode làm mất lượt thử vì EditorPrefs ScriptCompilationDuringPlay đang = 0 (tiếp tục chơi và nạp lại domain). Trước khi dùng Editor: xem CURRENT.md mục 'Đang dùng Editor / cổng 7777', xem scene có isDirty không, rồi GHI vào CURRENT.md việc mình sắp làm và giờ kết thúc dự kiến; xong thì xóa dòng đó.
- Cổng LAN 7777 chỉ một phiên dùng một lúc. Relay đi qua Internet nên không tranh cổng.
- manage_build TỰ THÊM scene thử vào Build Settings của chủ dự án. Chủ dự án (OWNER_UPDATES 2026-11-01) cho phép sửa scene map cho việc nối mạng nhưng Build Settings KHÔNG nằm trong phép đó: sau mỗi build chỉ GỠ đúng scene thử mà mình để lại (không thêm/bớt/đổi thứ tự cái khác) và ghi vào LOG. Hiện Build Settings có NetworkPartyTest, NetworkPlankTest của phiên #9 chưa gỡ (việc của #9 hoặc chủ dự án).
- Builds/ (ngoài Assets) chứa các bản build thử, hơn 1,5 GB, KHÔNG nằm trong .gitignore: đừng commit; xóa tay thư mục cũ khi không cần.

## 8. Bẫy đã gặp (đừng lặp lại)
1. WSS cần UnityTransport.UseWebSockets = true, các giao thức khác phải false; lệch thì NGO báo 'Mismatched Relay configuration' kèm NullReference.
2. Phòng Relay đầy: SDK ném 'session is full' (tiếng Anh); đã đổi sang câu tiếng Việt ở NetworkSessionService.Fail.
3. Chuỗi RPC mạng tối đa 2000 ký tự (MissionNetBridge); chuỗi tự do phải escape; tiếng Việt có dấu tốn tới 9 ký tự mỗi chữ sau escape.
4. Bấm .bat kiểm thử hai lần hoặc tiến trình thừa làm tranh cổng 7777: log hai tình huống lẫn nhau. Dùng khóa run.lock.
5. taskkill /f giữa các tình huống cắt cụt log. Đừng kill.
6. Log nhiều tiến trình ghi chung một file bị lẫn ký tự: mỗi tiến trình một file log.
7. Dòng 'stack trace' dài trong log dev build là Debug.Log bình thường của Vivox/Unity, không phải lỗi; tìm 'Exception' và 'Error'.
8. Id client đổi (1 -> 2) trong một lần chạy nghĩa là có client đã vào rồi rời (thường do bấm .bat hai lần).
9. Thread.Sleep trong execute_code chặn luồng chính nên tác vụ async không chạy tiếp: poll bằng lệnh riêng.
10. Unity reload sau compile: công cụ MCP tạm báo 'No Unity Editor instances found' hoặc 'reloading'; chờ và thử lại, đừng coi là lỗi.
11. Đội ít hơn 4 người: object scene dựng cố định cho 4 người (checkpoints[4], StartPoint_1..4, BackCard_1..4, stations[4], teachers[4], 6 phần tử Breaker) phải ẩn/bỏ qua phần thừa.
12. Nhân vật 'ngồi' trên máy chủ nhân: avatar do chủ máy điều khiển; host chỉ thấy vị trí qua OwnerNetworkTransform. Đặt nhân vật ở điểm ngồi/đứng là việc của client chủ nhân khi trạng thái host báo.

## 9. Trạng thái xác minh (xem TASKS.md/TASKS_DONE.md cho trạng thái task)
- LAN + đồng bộ vị trí hai chiều (T-009 S1): M4 (2 tiến trình).
- Relay (T-023): M5 cho 2 người ở 2 mạng khác nhau trong nước, đủ ma trận giao thức; CHƯA: 4G/CGNAT, nước ngoài, 3-4 người, đo độ trễ.
- Phòng (T-026): M4 LAN 3 tiến trình + Relay 4 tiến trình, 14 test; CHƯA: nhìn giao diện bằng mắt, đội 4 thật.
- Chat thoại (T-029): M4 vào kênh, thấy nhau, rời kênh; CHƯA: nói/nghe thật, mic mute, chất lượng, 3-4 người (cần 2 người thật).
- Khung (T-024) M3; avatar online (T-025) M3 + M4 một phần; PlankAndKey online (T-027) M4 host một mình + test; Attendance (T-031) M4 một phần LAN 3 tiến trình (đội 2 thắng qua mạng chưa kết luận); luồng map (T-028) M3, dừng chờ quyết định kiến trúc của chủ dự án.
- Chưa có nhiệm vụ nào được người thật chơi qua mạng (M5) ngoài kiểm phòng/Relay.

## 10. Việc tiếp theo
Xem TASKS.md: T-031 (tiên phong Attendance + BreakerPanel), T-036/T-037/T-038 (9 nhiệm vụ còn lại), T-028 (luồng map, chờ chủ dự án), T-032 (bản build cho bạn bè, chờ T-028).
