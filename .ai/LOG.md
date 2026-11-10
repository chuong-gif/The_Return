# LOG — Nhật ký dự án (bản tối ưu cho AI)

> **Vai trò:** chỉ lưu quyết định, mốc hoàn thành, kết quả kiểm thử và rủi ro còn mở. Không dùng LOG làm trạng thái hiện hành.
> **Nguồn sự thật khi tiếp quản:** `CURRENT.md` → `TASKS.md`/`TASKS_DONE.md` → tài liệu task trong `PLANS/`, `BUGS/`, `DECISIONS/`, `SYSTEM_CARDS/`.
> Chỉ tra mục LOG theo mã task khi cần lịch sử. Nhật ký chi tiết cũ: `SESSIONS/LOG_FULL_20261029.md` và các log phiên liên quan.
>
> **Mức xác minh:** M1 tạo/sửa file · M2 kiểm nội dung · M3 compile/test tự động · M4 chạy thực tế/Play Mode/build · M5 chủ dự án xác nhận.
> Không suy diễn: “chưa kiểm” không đồng nghĩa “lỗi”; kết quả chỉ đúng trong phạm vi ghi rõ.

## Lịch sử rút gọn

### 2026-10-22 đến 2026-10-29 — nền tảng dự án
- **T-008/T-011/T-012/T-018/T-019:** MainMenu, party cục bộ, Settings, HUD/minimap, tài liệu nhiệm vụ và âm thanh đã được triển khai; các lần kiểm thử ghi nhận 250–256 test xanh. Chi tiết/trạng thái duyệt: `TASKS.md`, `PLANS/T-008-menu-party-settings.md`.
- **T-005:** lỗi cổng Hành lang 1 được sửa; Play Mode 11/11, EditMode 256/256. Còn chủ dự án kiểm tra trực quan. `BUGS/gate-hanh-lang-1-vung-chuan-bi-mac-dinh.md`.
- **T-020/T-021:** 11 root nhiệm vụ chuyển thành prefab; EditMode 256/256, Play Mode 11/11 sau khi T-005 sửa lỗi cổng. Còn kiểm tra trực quan. `PLANS/T-020-mission-prefabs.md`, `PLANS/T-021-old-mission-prefabs.md`.
- **T-009:** khởi tạo multiplayer NGO. Ban đầu LAN có kết quả một phần, Relay chưa ổn định; tiếp tục kiểm ở các mục dưới.
- **T-010:** thay capsule bằng `Player.fbx`; xử lý import, material, animation. Phát hiện FBX tĩnh do NLA solo Idle; xuất lại animation. PlayMode 20/20 ở lần kiểm sau. Đã sửa tham chiếu prefab/controller để không phụ thuộc `Player_Animated.fbx`; thêm nhảy (cao 0,8 m). Còn chủ dự án kiểm tra trực quan/chơi tay. `BUGS/player-fbx-clip-khong-co-keyframe.md`.

### 2026-10-29 đến 2026-10-31 — framework và multiplayer
- **Quyết định đội chơi:** 2–4 người; mỗi người giữ một nhân vật cố định, không đổi vai; số nhân vật bằng số người. Thông số co giãn nhiệm vụ chốt tại `DECISIONS/2026-10-31-t022-team-size.md`. T-022 hoàn tất.
- **T-024 — framework host-authoritative:** framework C# thuần + `HostAuthority`, `ClientReplica`, `IHostRules`, `CommandResult`, `StateMessage`, `HostLog`; 67/67 EditMode xanh (M3). Chưa nối NGO tại thời điểm hoàn thành. `SYSTEM_CARDS/NetworkFramework.md`.
- **T-009 S1 — LAN:** xác minh 2 tiến trình thật, đồng bộ di chuyển hai chiều host↔client; client rời đột ngột được despawn, client còn lại tiếp tục. M4; chưa kiểm 3–4 người.
- **T-023 — Relay:** sửa cấu hình WebSockets; WSS và DTLS đều kết nối, đồng bộ hai chiều. Chủ dự án xác nhận một người bạn ở tỉnh khác kết nối qua mạng Wi-Fi khác; ma trận tạo/vào phòng WSS/UDP/DTLS đều thành công. M5 trong phạm vi 2 người/2 mạng tại Việt Nam. Chưa kiểm 4G, nước ngoài, 3–4 người, host mất mạng hay độ trễ định lượng. Mặc định WSS tại lần ghi nhận. T-023 hoàn tất; `PLANS/T-009-online-coop.md`.
- **T-025 — avatar online:** có `SlotAllocator`, chế độ online trong `PrototypePartyController`, `PF_OnlineAvatar`, scene test riêng và build. EditMode 361/361; host một mình kiểm được (M3 + M4 một phần). Chủ dự án từng báo client T-pose; build sau đó hết lỗi, log xác nhận Animator chạy trên cả avatar local/remote. Nguyên nhân chính xác chưa xác định. Chưa có xác nhận trực quan đầy đủ 2 máy/3–4 người.
- **T-026 — phòng 2–4 người:** `RoomState/RoomRoster/RoomService/RoomHud`, prefab và scene test riêng. EditMode 14/14; LAN 3 tiến trình và Relay 4 tiến trình xác minh: danh sách phòng, bắt đầu khi đủ người, từ chối người thứ 4 khi phòng đầy, cập nhật khi người rời. M4 một phần. Chưa kiểm đội 4, giao diện bằng mắt và host thoát giữa chừng. T-026 ở REVIEW.
- **T-029 — voice chat Vivox:** 2 tiến trình qua Relay vào cùng voice channel; nhận diện thành viên, rời kênh khi host thoát; compile/build sạch. M4 cho kết nối kênh, **chưa kiểm âm thanh nói/nghe thật**, mic mute, chất lượng, 3–4 người. T-029 ở REVIEW.
- **T-030 — host migration:** HỦY theo quyết định chủ dự án; khi host thoát, cả nhóm rời phòng. `DECISIONS/2026-10-31-t030-no-host-migration.md`.

### 2026-10-31 đến 2026-11-01 — nhiệm vụ và sửa lỗi
- **T-033 — lỗi gameplay:** sửa điều kiện cả đội vào khu, vùng đích Hành lang 1, tương tác giữ nút B/E và dữ liệu Prompt/Controls của 6 HUD nhiệm vụ mới. Mở rộng `Team_Goal` của `PF_QuietCorridor` để bao vùng đích. EditMode 279/279; PlayMode các bộ liên quan 11/11 và 2/2; Console 0 lỗi/cảnh báo (M4 mô phỏng). Còn chủ dự án chơi thật xác nhận Hành lang 1 và gợi ý “bấm E” ở Phòng 6.
- **T-034 — phòng mới/collider:** HỦY vì chủ dự án tự sửa. Lưu ý 5 mesh collider `Assets/My_Asset/Map/Phong/Collision/*_2Mat.asset` phải có trong source/commit để tường, cột, cửa có collider.
- **T-027 — PlankAndKey online:** host rules, codec, `MissionNetBridge`, host/client online, scene test và build. EditMode toàn dự án 430/430, network tests 45/45; host một mình chạy trọn vòng nhặt thẻ → đi ván → đến đích → từ chối lệnh sai → Retry. Sau đó sửa animation đi tại chỗ khi người chơi được ván chở: `SuppressLocomotion`; 7 test bổ sung, EditMode 437/437. PlayMode host xác nhận Speed=0 khi đang ngồi trên ván; build đã cập nhật. **Còn chủ dự án thử 2 máy để xác nhận trực quan**, 3–4 người và tích hợp map. `PLANS/T-027-plank-network-pattern.md`.
- **T-031 — Attendance online:** host rules + codec + online host/client + scene test. EditMode network 44/44; LAN 3 tiến trình xác minh đội 3 thắng, tranh ghế bị từ chối đúng, người rời được giải phóng ghế; kiểm bổ sung đội 2 ngồi sai → báo sai → đứng dậy xóa trạng thái sai. M4 một phần. Test R1 đội 2 thắng bị log cắt do script kill tiến trình quá sớm; không kết luận được từ log đó. Còn chơi tay 2 máy, đội 4 và tích hợp luồng map/cửa ra. Attendance ở REVIEW. `PLANS/T-031-remaining-missions.md`.
- **T-035 — đóng gói source:** ZIP 76,1 MB/1.591 tệp; kiểm tra integrity. Giữ Assets (trừ ThirdParty có license), Packages, ProjectSettings, tài liệu AI; bỏ cache, Builds, Git, backup và dữ liệu editor. Người nhận cần cài ThirdParty bằng license riêng.

### 2026-11-03 — trạng thái mới nhất được ghi trong file nguồn
- **T-028 — map flow online:** thêm `MapFlowState`, `RemoteMissionAdapter`, chế độ online trong `MissionFlowController`, `IOnlineMission/MapFlowOnline`, tích hợp thử PlankOnlineMission. EditMode toàn dự án 517/517 (M3). Chưa chạy lại PlayMode SchoolFloor 18/18; chưa tích hợp scene thật.
- **T-028 đang DỪNG chờ quyết định kiến trúc:** (1) đặt lớp online trong `SchoolFloorPrototype` hay scene bản sao; (2) luồng “Bắt đầu ở phòng → mọi người vào map” chưa có `NetworkManager` trong scene game — quyết định làm trong T-028 hay tách task. Xem `PLANS/T-028-map-flow-online.md`. Không tự chọn thay chủ dự án.
- **T-031 BreakerPanel:** phiên #10 nhận task lúc 2026-11-03 14:07 UTC; mới ghi START. Phạm vi: chỉ tạo file mới trong `Features/BreakerPanel/Runtime/Network` và `Tests/EditModeNet`, sau đó mới đến Online/scene test riêng. Không sửa Domain, scene map, prefab hoặc Build Settings. **Chưa có kết quả hoàn thành trong LOG này.**

## Quy tắc tiếp tục công việc
1. Đọc `CURRENT.md` và `TASKS.md` trước; trạng thái trong hai file đó mới nhất, LOG có thể cũ.
2. Đọc đúng brief/plan/system card của task; không đọc lại toàn bộ LOG.
3. Trước khi sửa scene/prefab hoặc dùng Play Mode/cổng LAN 7777: kiểm tra Editor, scene dirty, phiên khác và quyền sửa; phối hợp lock/test window.
4. Tách rõ M3 (compile/test tự động), M4 (chạy thật) và M5 (chủ dự án xác nhận). Không gọi test nhiều tiến trình cùng máy là kiểm chứng nhiều máy/mạng.
5. Chỉ ghi LOG khi có thay đổi đáng kể hoặc kết quả kiểm chứng; mỗi mục 1–3 gạch đầu dòng, nêu rõ kết quả, giới hạn và đường dẫn chi tiết.

### 2026-11-04 19:13 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | START (tiếp, giai đoạn nối scene) | T-028
- Chủ dự án «làm theo lời khuyên»: đặt lớp online TRONG SchoolFloorPrototype (đã được cho phép sửa scene; có bản Before... trước). Quyết định (2) tôi hiểu theo hướng thu hẹp: KHÔNG làm đường «phòng -> map» trong T-028 (tách task riêng); thay vào đó dùng khởi động bằng tham số dòng lệnh -netauto host|client (như các bản build thử) với NetworkRoot tạo lúc chạy từ prefab trong Resources, nên scene thật KHÔNG chứa NetworkManager và chế độ một máy không đổi.
- Sẽ sửa: OnlinePartyLocalDriver (tắt 4 PlayerSlot offline khi có người online), OnlinePartySlots (-teamsize N), thêm OnlineMapBootstrap + prefab Features/Network/Online/Resources/OnlineBootstrap/PF_NetworkRoot.prefab, rồi thêm gốc OnlineLayer vào SchoolFloorPrototype (bản Before... trước). Scene đang không dirty.

### 2026-11-05 07:38 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | PROGRESS | T-031 BreakerPanel — giai đoạn 1 (lớp mạng thuần C#)
- Tạo MỚI (không sửa Domain/scene/prefab/Build Settings): Features/BreakerPanel/Runtime/Network/{TheReturn.BreakerPanel.Network.asmdef, BreakerCommand, IBreakerWorld, BreakerPublicState (+BreakerPrivateState), BreakerHostRules, BreakerCodec}.cs và Tests/EditModeNet/{TheReturn.BreakerPanel.Network.Tests.asmdef, BreakerHostRulesTests, BreakerCodecTests}.cs.
- Thiết kế: seed/đáp án/mặt nạ công tắc người khác KHÔNG bao giờ vào trạng thái chung; riêng mỗi ô chỉ nhận công tắc của mình + đèn nó đảo. Đồng hồ giữ khớp ở host (Tick -> MarkChanged khi thắng); máy khách ngoại suy HoldElapsed. Công tắc của người rớt mạng không bấm được (theo T-030 cả phòng rời nên bỏ qua).
- KẾT QUẢ: biên dịch sạch (Console 0 lỗi/0 cảnh báo); run_tests EditMode assembly TheReturn.BreakerPanel.Network.Tests = 58/58 pass (0.11s). Toàn bộ suite EditMode sau khi thêm = 575/575 pass, 0 fail (không phá test cũ).
- CÒN: giai đoạn 2 (Runtime/Online + scene thử NetworkBreakerTest + chạy LAN thật), chơi tay 2 máy.

### 2026-11-05 07:49 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | DONE (chờ REVIEW) | T-031 BreakerPanel
- Giai đoạn 2: tạo MỚI Features/BreakerPanel/Runtime/Online/{TheReturn.BreakerPanel.Online.asmdef, BreakerOnlineHost, BreakerOnlineClient, BreakerOnlineAutoRun}.cs theo mẫu Attendance (host dựng BreakerHostRules + HostAuthority, IBreakerWorld theo khoảng cách nhân vật-công tắc 3,5 m; client gửi E/R, vẽ công tắc/đèn từ bản sao, HUD hiện công tắc RIÊNG của mình).
- Scene thử MỚI Features/Network/Scenes/NetworkBreakerTest.unity: nhân bản NetworkAttendanceTest, xóa AttendanceRoom/AttendanceBoard, đổi AttendanceOnline -> BreakerOnline (giữ NetworkObject + MissionNetBridge), thêm PF_BreakerPanel cách đội ~8 m (BreakerPanelPrototype 1 máy bị tắt). Mọi tham chiếu đã nối (0 null, 0 script thiếu).
- KẾT QUẢ: biên dịch sạch; EditMode toàn bộ 575/575 pass. CHƯA chạy Play-mode/LAN thật vì mở scene thử sẽ thay scene đang mở của chủ dự án; Build Settings KHÔNG đụng nên muốn chạy build player phải truyền scenes riêng.
- Lưu ý: scene SchoolFloorPrototype tự hiện dirty vì PF_SchoolFloorCanvas (RectTransform/Canvas) tính lại layout khi nạp/đóng scene thử; không phải thay đổi có chủ ý, không cần lưu.
- CÒN: chơi thử scene NetworkBreakerTest (2 máy), kiểm khoảng cách 3,5 m và thứ tự công tắc trái/phải (đang hiển thị 1 ở bên phải khi nhìn từ đội), rồi nối IOnlineMission cho luồng map (T-028).

### 2026-11-05 08:10 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #11) | START | T-037 TeacherSays (Hanh_Lang_5)
- Chọn TeacherSays trong nhóm B. Phạm vi: chỉ file MỚI Features/TeacherSays/Runtime/Network + Tests/EditModeNet (giai đoạn 1, lớp mạng thuần C#). KHÔNG đụng Domain, scene map, prefab gốc, Build Settings, ChoirRules.
- Quyết định thiết kế: đồng hồ Bước(8s)/Đứng(5s) và cờ «Cô nói» CHỈ gửi RIÊNG cho người nghe; trạng thái chung KHÔNG chứa đồng hồ, vì đồng hồ khác nhau giữa Bước và Đứng sẽ làm lộ đáp án cho người không nghe (bản 1 máy đang lộ vì sổ tay vai nào cũng thấy đồng hồ). Cần chủ dự án xác nhận.
- File dự định đụng: .ai/LOG.md, .ai/TASKS.md, .ai/PLANS/T-037-timing-zone-missions.md (bảng nhận việc + mục thiết kế).

### 2026-11-05 08:35 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #11) | PAUSED (giai đoạn 1 xong) | T-037 TeacherSays
- Tạo MỚI Features/TeacherSays/Runtime/Network/{asmdef, TeacherSaysCommand, ITeacherSaysWorld, TeacherSaysPublicState, TeacherSaysHostRules, TeacherSaysCodec}.cs và Tests/EditModeNet/{asmdef, TeacherSaysHostRulesTests, TeacherSaysCodecTests}.cs. Không đụng Domain, scene, prefab, Build Settings, ChoirRules.
- KẾT QUẢ (M3): compile sạch, Console 0 lỗi/0 cảnh báo; Network.Tests 70/70; EditMode toàn bộ 645/645 (575 cũ + 70), gồm 18 test Domain TeacherSays. CHƯA M4 (LAN) và M5.
- QUYẾT ĐỊNH cần chủ dự án xác nhận: đồng hồ + cờ «Cô nói» chỉ gửi RIÊNG người nghe (brief ghi «hạn còn lại» là CHUNG nhưng sẽ lộ đáp án). Chi tiết: PLANS/T-037-timing-zone-missions.md mục 9. CÒN GĐ2: Runtime/Online + scene NetworkTeacherSaysTest + LAN.

### 2026-11-05 17:12 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #12) | START | T-037 Choir (Phong_6)
- Nhận Choir (TeacherSays do #11 giữ, GĐ1 xong; SharedLantern còn trống). Phạm vi GĐ1: đổi ChoirRules theo số người (thêm OnlineFor(n): N=2 -> 3s, N=3 -> 2,5s, N=4 -> 2s; giữ Online = giá trị đội 4) + sửa/thêm test ChoirTests có chủ đích; file MỚI Features/Choir/Runtime/Network + Tests/EditModeNet. Không đụng scene map, prefab gốc, Build Settings.
- File dự định đụng: Domain/ChoirRules.cs, Tests/EditMode/ChoirTests.cs, .ai/LOG.md, .ai/TASKS.md, .ai/PLANS/T-037-timing-zone-missions.md, SYSTEM_CARDS/Choir.md.

### 2026-11-06 05:02 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | TEST | T-031 BreakerPanel — Play-mode 1 tiến trình
- Mở NetworkBreakerTest (Single), Play, OnlinePartySlots.SetTeamSize(2) + NetworkSessionService.StartLanHost(). Host dựng đề 2 người; khách (ô 0) nhận trạng thái chung + riêng (công tắc 0,2) qua MissionNetBridge thật.
- Kết quả: công tắc của ô khác -> host từ chối «không phải của bạn»; công tắc của mình khi đứng cách ~8 m -> «đứng quá xa»; sau PlaceNearSwitch(0) bật được: version 0->1, đèn 14 -> 0 (= 14 XOR mặt nạ 14, đúng luật); tắt + Retry: version 3, bật=0, đèn về 14, resets=1, msg=Retry. Công tắc riêng chỉ {0,2}. Console 0 lỗi/0 cảnh báo.
- CHƯA kiểm: thắng ván (cần ô 1 là tiến trình thứ hai), quá tải (đội 2 mỗi người chỉ có 2 công tắc < giới hạn+1 = 3; đã có unit test), giữ khớp 3 giây qua mạng (có unit test).
- Lưu ý: giữa các lượt làm việc, phiên #11/#12 đã dùng Editor; phiên Play đầu của tôi bị mất, đã chạy lại từ đầu. Đã trả scene về SchoolFloorPrototype (không dirty).

### 2026-11-06 05:26 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | PLAN | T-039 Đẩy nhanh online (quy trình chịu ngắt + test hợp đồng)
- Chủ dự án yêu cầu: AI luân phiên, có thể hết limit giữa chừng (hồi sau ~5 giờ), muốn xây hết logic rồi test MỘT lượt, lỗi quy về đúng AI/nhiệm vụ để người đó sửa, làm nhanh hơn. Ghi tại DECISIONS/2026-11-06-t039-owner-requirements.md.
- Tạo MỚI: PLANS/T-039-online-fast-track.md (trạng thái CHỜ DUYỆT, có khối RESUME), DECISIONS/2026-11-06-t039-owner-requirements.md; thêm dòng T-039 (TODO, chưa ai giữ) vào TASKS.md. Chưa tạo code/công cụ, chưa sửa PROTOCOL.
- ĐO: BreakerPanel vs Attendance chỉ 37% số dòng trùng nguyên văn (host Online 81%, test 15-29%); EditMode ~0,9 giây nên điểm nghẽn là số bước tay + chờ người + khảo sát lại khi đổi AI. Bỏ đề xuất dotnet test (AI không có terminal).
- Cần chủ dự án trả lời 4 câu ở mục 9 của kế hoạch. Không ảnh hưởng T-036/T-037/T-038 đang làm.

### 2026-11-06 06:26 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | START | T-039 Đẩy nhanh online
- Chủ dự án DUYỆT thiết kế + cho phép M5 + đồng ý PROVISIONAL (DECISIONS/2026-11-06-t039-owner-approval.md). Editor lúc nhận: play=False compiling=False scene=SchoolFloorPrototype dirty=False.
- Làm: S0.2, S0.3 (file .ai/ MỚI + PROTOCOL mục 14, START_HERE), rồi M1-M4. File định đụng: .ai/{QUEUE,MISSIONS_NET,FAILURES,GAUNTLET}.md, DECISIONS/PENDING.md, PROTOCOL.md, START_HERE.md, TASKS.md, PLANS/T-039*.md, Features/Network/Tests/{Contract,Sim} (MỚI).

### 2026-11-06 06:38 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | PROGRESS | T-039 M1 DONE (M3)
- S0.1-S0.3 xong (QUEUE/MISSIONS_NET/FAILURES/GAUNTLET/PENDING + PROTOCOL mục 14 + START_HERE). M1: Features/Network/Tests/Contract/{asmdef, ContractTypes, ContractRunner, MissionContractTests, FakeMission, ContractMetaTests}.cs. Hợp đồng C1-C9 x 2/3/4 x 50 seed.
- KẾT QUẢ: compile sạch; TheReturn.Network.Contract.Tests 19/19 (0,29 s), 7 lỗi cố ý bị đúng ca bắt; khung + Attendance + BreakerPanel + PlankAndKey + TeacherSays + Contract gộp 314/314 (0,37 s) trong MỘT run_tests nhiều assembly. Lưu ý: run_tests KHÔNG kèm assembly_names không khởi động được. Đang làm M2.

### 2026-11-06 15:26 UTC | Claude Sonnet 5.5 (phiên #13) | PROGRESS | T-039 M2 DONE
- SimNet.cs (uplink FIFO trễ/trùng; downlink trễ/đảo/trùng/mất; Settle), ContractRunnerSim.cs (ca N1: mạng êm + mạng xấu, gói bất biến, hội hụ), SimNetTests.cs; FakeFlaw.AliasedState. Contract.Tests 25/25 (0,42 s). Đang làm M3.

### 2026-11-07 05:34 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | START | T-036 GradeRepair (pha L)
- Chủ dự án: «tìm một task trong dự án để làm». QUEUE: thẻ READY đầu là Q-006 nhưng cần Q-005 đang IN-PROGRESS của #13 (log 15:26, chưa 6 giờ) -> KHÔNG đụng T-039. Chọn T-036 (TODO, chưa ai, không trùng file với #13), nhiệm vụ GradeRepair đầu tiên.
- Pha L chỉ file MỚI trong Features/GradeRepair/{Runtime/Network,Tests/EditModeNet}: test Domain đặc tả (không sửa Domain), lớp mạng thuần C#, test, adapter hợp đồng. KHÔNG scene/Play/build/Build Settings. Editor lúc nhận: play=False compiling=False scene=SchoolFloorPrototype dirty=False.

### 2026-11-07 05:34 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | CANCEL + RESUME | T-036 GradeRepair hủy nhận; tiếp T-039 Q-005 (M3)
- Hủy START T-036 ngay trên (chưa làm file nào): đồng hồ nhảy 14 giờ nên Q-005 của #13 (log cuối 2026-11-06 15:26) đã >= 6 giờ = INTERRUPTED; QUEUE yêu cầu nhận thẻ INTERRUPTED trước. T-036 + GradeRepair đã trả về TODO.
- RESUME Q-005 (T-039 M3): trên đĩa chưa có file *Adapter* nào; M1/M2 (Contract + SimNet) xong, Console sạch. Làm adapter Attendance + BreakerPanel, đo số dòng + thời gian chạy, điền PLANS/T-039 mục 8.

### 2026-11-07 05:42 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | DONE | T-039 Q-005 (M3) adapter Attendance + BreakerPanel
- MỚI: Features/{Attendance,BreakerPanel}/Tests/EditModeNet/*ContractAdapter.cs (+ reference TheReturn.Network.Contract.Tests trong 2 asmdef test). Đo: Attendance 104 dòng (78 dòng code), BreakerPanel 97 dòng (73 dòng code).
- Hợp đồng BẮT LỖI THẬT: BreakerPanel C8+N1 (HoldElapsed là đồng hồ trong trạng thái chung -> cùng Version khác nội dung). Ghi F-001, sửa BreakerHostRules.BuildPublicState + test hồi quy, FIXED. Attendance qua hết, không lỗi.
- KẾT QUẢ: Console 0 lỗi; 2 assembly 125 test (123 pass, 2 bỏ qua C7); toàn bộ 22 assembly *.Tests = 700 test, 698 pass, 0 fail, 5,4 s. Thẻ kế tiếp: Q-006 (M4). Editor: scene gốc, không Play.

### 2026-11-07 05:55 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | CANCEL + RESUME | T-039 phần trùng Q-005; nhận Q-006 (M4)
- Sau quãng nghỉ ~14 giờ mình (#13) tiếp tục mà KHÔNG kiểm lại QUEUE/LOG nên tạo adapter Attendance+BreakerPanel trong Tests/EditModeNetContract (asmdef riêng) TRÙNG với bản của #10 (Q-005 đã DONE 05:42, kèm sửa F-001). Đã XÓA toàn bộ 2 thư mục EditModeNetContract của #13 (bản #10 là bản chính thức). Không đụng file nào của #10. Bài học: sau khoảng nghỉ phải đọc lại QUEUE+LOG trước khi tiếp (PROTOCOL 14.4/14.5).
- RESUME: nhận Q-006 (M4 Gauntlet) từ trạng thái READY. Ghi chú: Contract.Tests 25/25 xanh trước đó; cả 2 adapter (bản #10 và bản trùng của #13) đều qua C1-C9/N1 trên BreakerHostRules đã sửa.

### 2026-11-07 06:07 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | DONE | T-039 Q-006 (M4) Gauntlet
- MỚI: Features/Network/Editor/Gauntlet/{TheReturn.Network.Gauntlet.Editor.asmdef, Gauntlet.cs} (Editor script dùng TestRunnerApi, thay cho snippet .ai/tools như kế hoạch vì chạy một lệnh và tự ghi báo cáo). Ghi tự động: GAUNTLET.md, FAILURES.md (không ghi trùng; XANH-LẠI khi test xanh lại), cột Gauntlet của MISSIONS_NET.md, thẻ FIX trong QUEUE.md.
- KẾT QUẢ: 700 test / 20 assembly có test, 698 đạt, 0 đỏ, 2 bỏ qua có chủ đích, ~5 s. PHÉP THỬ: cố ý phá Attendance.IsSolvedPublic -> 2 đỏ (C6, N1) quy đúng Attendance / T-031 / #7, F-002+F-003 + 2 thẻ FIX; khôi phục từ bản sao (Temp/) -> XANH-LẠI. Đã đóng 2 dòng cố ý thành FIXED. Editor: scene gốc, không Play, không dirty.
- SỰ CỐ: lệnh execute_code có Thread.Sleep(1500) ngay sau khi chạy Gauntlet làm Unity treo (chặn luồng chính) -> chủ dự án phải mở lại Unity; đã ghi vào PROTOCOL 14.12. Thẻ kế tiếp READY: Q-007 (M5).

### 2026-11-07 06:11 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | RESUME | T-037 Choir (Phong_6)
- Chủ dự án bảo chọn 1 nhiệm vụ T-037 và làm. Chọn Choir: thẻ #12 START 2026-11-05 17:12 UTC, không LOG sau đó (> 36 giờ theo đồng hồ máy) = INTERRUPTED (PROTOCOL 14.5), nên RESUME không TAKEOVER. Trên đĩa: ChoirRules.cs + ChoirTests.cs sửa 11-06 00:14 (OnlineFor theo N, P-003 PROVISIONAL), CHƯA có Runtime/Network.
- Phạm vi GĐ1 (file MỚI, không đụng Domain/scene/prefab/Build Settings): Features/Choir/Runtime/Network/{TheReturn.Choir.Network.asmdef, ChoirCommand, IChoirWorld, ChoirPublicState(+Private), ChoirHostRules, ChoirCodec}.cs và Tests/EditModeNet/{asmdef, ChoirHostRulesTests, ChoirCodecTests}.cs. Online/scene/LAN để GĐ2 sau Q-007.

### 2026-11-07 06:20 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | PAUSED (GĐ1 xong) | T-037 Choir
- MỚI (không đụng Domain/scene/prefab/Build Settings): Features/Choir/Runtime/Network/{asmdef, ChoirCommand, IChoirWorld, ChoirPublicState(+Private), ChoirHostRules, ChoirCodec}.cs và Tests/EditModeNet/{asmdef, ChoirHostRulesTests, ChoirCodecTests, ChoirContractAdapter}.cs. ChoirRules.OnlineFor(n) có sẵn từ #12 (Domain 31/31 xanh). Thiết kế: thẻ nhạc RIÊNG, bản nhạc CHUNG, đồng hồ KHÔNG vào trạng thái chung (bài học F-001), bấm E gần bục do host đo = P-004 PROVISIONAL.
- KẾT QUẢ (M3): compile sạch, Console 0 lỗi/0 cảnh báo; Choir.Network.Tests 65/66 đạt (C7 bỏ qua có chủ đích: Choir không có «thua»); Gauntlet 06:20 UTC XANH 766 test, 763 đạt, 0 đỏ. Chưa M4/M5. Thẻ hệ thống Choir có mục E; MISSIONS_NET có dòng Choir.
- CÒN GĐ2 (chưa ai nhận, sau Q-007): Runtime/Online + scene NetworkChoirTest + LAN 3 tiến trình (thắng/quá giờ/client ở xa bị bác). Editor: scene gốc, không Play.

### 2026-11-07 06:26 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | DONE một phần (mức 3 + build thử; CHƯA thử 2 máy; CHƯA xử lý mất kết nối) | T-028
- Làm: thêm OnlineLayer vào SchoolFloorPrototype (bản Before...: SchoolFloorBeforeOnlineLayer_20261104_1914), OnlineMapBootstrap + prefab PF_NetworkRoot (Features/Network/Online/Resources/OnlineBootstrap), OnlinePartyLocalDriver tắt PlayerSlot một máy, OnlinePartySlots -teamsize, build Builds/NetworkMap (+ chay_host.bat, chay_client.bat). Scene không còn dirty sau khi lưu.
- Kiểm: EditMode 517/517; PlayMode SchoolFloor 28/32. 4 lỗi cố định ở cổng «Phong 2_Cua 1» (chạy lại riêng vẫn y như vậy), chưa chẩn đoán, không chứng minh được là do tôi hay có sẵn (chưa A/B). Luồng logic EveryGate_OfEveryMissionArea_StartsItsArea qua.
- Sự cố công cụ: sau khi chủ dự án dừng Play Mode giữa chừng, trạng thái test của công cụ MCP bị kẹt «tests_running» và chặn mọi lần chạy test/refresh; xử lý bằng EditorUtility.RequestScriptReload (không mất dữ liệu). Chủ dự án nên tránh bấm Stop khi bộ test PlayMode đang chạy.
- KHÔNG sửa: Domain MapProgress, NetworkSessionService/Room/Voice, PF_PlayerModel, Build Settings (SchoolFloorPrototype đã sẵn trong danh sách nên build không thêm scene mới).

### 2026-11-07 06:28 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #9) | GHI CHÚ | T-028
- Chủ dự án xem Play Mode lúc 4 test đi bộ chạy (đã chạy hết): bằng mắt KHÔNG thấy bất thường. Điều này KHÔNG xóa kết quả test (4 lỗi khẳng định ở cổng «Phong 2_Cua 1»: cổng armed là «Hanh lang 2_Ra», outside=0 inside=0, cửa đóng); chỉ gợi ý đây có thể là đường đi của test hoặc vùng chuẩn bị của hai cổng chứ chưa chắc game hỏng. Chưa kết luận; cần kiểm bằng tay cổng này (một máy, offline) hoặc người sở hữu test (T-005) xem lại.

### 2026-11-07 20:34 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | START | T-039 Q-007 (M5) lớp cơ sở Online
- Đã đọc lại QUEUE/LOG (Q-007 READY, không ai giữ). Chủ dự án cho phép M5 (DECISIONS/2026-11-06-t039-owner-approval.md). Phạm vi: lớp cơ sở HOST (trùng 81%) + CLIENT (trùng 53%) CHỈ THÊM vào Features/Network/Online; chuyển BreakerOnlineHost/Client (của #10) sang dùng. AutoRun giữ riêng từng nhiệm vụ (kịch bản khác nhau). KHÔNG sửa Attendance/PlankAndKey.
- File định đụng: MỚI Features/Network/Online/MissionOnlineHostBase.cs, MissionOnlineClientBase.cs; SỬA Features/BreakerPanel/Runtime/Online/BreakerOnlineHost.cs, BreakerOnlineClient.cs. Scene NetworkBreakerTest chỉ MỞ để thử, không lưu. Phiên #9 (T-028) và #14 (T-037) đang làm việc khác.

### 2026-11-07 20:47 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | START | T-036 GradeRepair (pha L)
- Chủ dự án: «tìm việc làm đi». QUEUE không có thẻ READY (Q-006 DONE, Q-007 IN-PROGRESS của #13 từ 20:34, Q-008/Q-009 BLOCKED) -> không đụng T-039. Nhận T-036 (TODO, chưa ai), GradeRepair đầu tiên; không trùng file với #13.
- Pha L chỉ file MỚI trong Features/GradeRepair/{Runtime/Network,Tests/EditModeNet}: lớp mạng thuần C#, test, adapter hợp đồng. KHÔNG scene/Play/build/Build Settings. Editor lúc nhận: play=False compiling=False scene=SchoolFloorPrototype dirty=False.

### 2026-11-07 20:56 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | START | T-037 SharedLantern (Hanh_Lang_4) pha L
- Chủ dự án bảo «tìm task để làm». QUEUE: Q-007 do #13 giữ (START 20:34), T-036 GradeRepair do #10 giữ (START 20:47), cả hai còn mới nên KHÔNG đụng. Nhận SharedLantern (TODO, chưa ai). Editor lúc nhận: play=False compiling=False scene=SchoolFloorPrototype dirty=True (do phiên khác; mình KHÔNG mở/lưu scene).
- Chỉ file MỚI: Features/SharedLantern/Runtime/Network/{asmdef, SharedLanternCommand, ISharedLanternWorld, SharedLanternPublicState(+Private), SharedLanternHostRules, SharedLanternCodec}.cs và Tests/EditModeNet/{asmdef, HostRulesTests, CodecTests, ContractAdapter}.cs. Không đụng Domain/scene/prefab/Build Settings/Runtime/Online.

### 2026-11-07 21:03 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | PAUSED (GĐ1 xong) | T-037 SharedLantern
- MỚI (không đụng Domain/scene/prefab/Build Settings/Runtime/Online): Features/SharedLantern/Runtime/Network/{asmdef, SharedLanternCommand, ISharedLanternWorld, SharedLanternPublicState(+Private), SharedLanternHostRules, SharedLanternCodec}.cs và Tests/EditModeNet/{asmdef, SharedLanternHostRulesTests, SharedLanternCodecTests, SharedLanternContractAdapter}.cs. Thiết kế: sổ đọc biển RIÊNG; chung = ai cầm đèn/ai ngoài sáng/bảng nhập/số lần bị đưa về; KHÔNG đưa đồng hồ vào trạng thái chung (bài học F-001); host đo vị trí (trao đèn 2 m, đứng gần biển/bảng, biển trong vùng sáng). P-005 PROVISIONAL.
- KẾT QUẢ (M3): compile sạch, Console 0 lỗi/0 cảnh báo; SharedLantern.Network.Tests 83/84 đạt (C7 bỏ qua có chủ đích), Domain.Tests 37/37; Gauntlet 21:03 UTC: 921 test, 916 đạt, 1 ĐỎ ở GradeRepair.Network.Tests (việc dở của #10, T-036, KHÔNG phải của mình, không sửa). Chưa M4/M5. Thẻ hệ thống SharedLantern có mục E; MISSIONS_NET có dòng SharedLantern.
- CÒN GĐ2 (chưa ai nhận, sau Q-007 của #13): Runtime/Online + scene NetworkSharedLanternTest + LAN 3 tiến trình (thắng/bị đưa về/client ở xa bị bác). Editor lúc kết thúc: play=False compiling=False; scene SchoolFloorPrototype dirty do phiên khác, mình không mở/lưu scene.

### 2026-11-08 13:39 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | DONE | T-039 Q-007 (M5) lớp cơ sở Online + Breaker làm thử
- MỚI: Features/Network/Online/MissionOnlineHostBase.cs, MissionOnlineClientBase.cs (generic, lớp con đóng kiểu). SỬA: BreakerOnlineHost.cs, BreakerOnlineClient.cs sang kế thừa (field serialize giữ nguyên tên; bridge chuyển lên lớp cơ sở). KHÔNG đụng Attendance/PlankAndKey/BreakerOnlineAutoRun. Số đo: host Breaker 167->82 dòng, client 236->184 dòng; lớp cơ sở 193 + 138 dòng dùng chung.
- KIỂM: compile sạch; Gauntlet 766 test 0 đỏ (sau khi sửa); scene NetworkBreakerTest mở không lưu: bridge/switches/lamps/settings/party/switchReach đủ, 0 null; Play host một mình (SetTeamSize(2)+StartLanHost): ô 0 kết nối, đề 2 người; Toggle(1) -> «không phải của bạn», Toggle(0) khi xa -> «đứng quá xa», PlaceNearSwitch(0)+Toggle(0) bật được (Toggled), tắt+Retry: host.Version=3, bật=0, đèn 14, ResetCount=1, LastMessage=Retry; Console 0 lỗi/0 cảnh báo. Khớp mốc #10 2026-11-06 05:02. Đã dừng Play, trả scene SchoolFloorPrototype, không dirty.
- LƯU Ý: bản sao lưu gốc để ở Temp/ (Unity dọn mất); bản gốc 2 file Breaker lấy lại bằng git nếu cần. CHƯA kiểm: thắng ván/giữ khớp qua 2 tiến trình thật (đã có unit test; M7 smoke chung). Báo cáo Gauntlet 21:03 có F-004 (GradeRepair, nhiệm vụ mới của phiên khác, thiếu dòng MISSIONS_NET) KHÔNG phải của #13, giữ nguyên. Thẻ kế: Q-008 (M6) READY.

### 2026-11-08 13:52 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | START | T-039 Q-008 (M6) khung nhiệm vụ mới
- Đã đọc lại QUEUE/LOG (Q-008 READY, không ai giữ; #10 giữ T-036, #14 giữ T-037 nên brief/bảng T-036/37/38 chỉ sửa tối thiểu sau khi kiểm). File định đụng: MỚI Features/MissionTemplate/**, Features/Network/Editor/Gauntlet/MissionScaffold.cs, .ai/tools/*; SỬA NETWORK.md mục 4-5, MISSIONS_NET.md (chỉ thêm dòng khi thử, rồi gỡ), TASKS.md/brief T-036/37/38 (nếu còn nói quy trình cũ).

### 2026-11-08 14:06 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | DONE | T-039 FIX-F-004 (GradeRepair.Network.Tests)
- Nhận thẻ FIX (READY, đứng đầu QUEUE). #10 (T-036 GradeRepair) không có LOG sau 20:47 UTC hôm qua nên chỉ sửa đúng lỗi, KHÔNG nhận T-036. Nguyên nhân là lỗi MÃ chứ không phải test: Uri.UnescapeDataString không ném lỗi với «%» trần hay «%zz» (trả lại nguyên văn) nên GradeRepairCodec.TryDecodePrivate nhận chuỗi sai định dạng và đi-về không chính tắc («g:0=%» -> «g:0=%25»).
- SỬA: 1 dòng trong Runtime/Network/GradeRepairCodec.cs (thoát lại chuỗi vừa giải mã phải ra đúng chuỗi nhận được); MỚI Tests/EditModeNet/GradeRepairCodecCanonicalTests.cs (9 ca, file riêng để không đè file của #10). Nếu #10 tiếp tục và GHI ĐÈ GradeRepairCodec.cs thì phải giữ dòng này, test hồi quy sẽ đỏ lại nếu mất.
- KẾT QUẢ (M3): Console 0 lỗi; GradeRepair.Network.Tests 55/55; Gauntlet 14:06 UTC XANH 929 test, 925 đạt, 0 đỏ, 4 bỏ qua. Ghi nhận: Q-007 do #13 đã DONE, #13 đang giữ Q-008 (START 13:52 UTC) nên mình không đụng NETWORK.md/briefs/scaffolder. Editor: không Play.

### 2026-11-08 14:30 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | START | T-037 Choir GĐ2 (lớp Online)
- Đã đọc lại LOG/QUEUE: Q-007 DONE (#13), #13 đang giữ Q-008; không ai giữ Choir GĐ2. File định đụng: MỚI Features/Choir/Runtime/Online/** (asmdef TheReturn.Choir.Online, ChoirOnlineHost/Client/AutoRun). Không đụng scene, Domain, Network.Online, file của #13/#10. Dừng hỏi chủ trước khi dựng/mở scene NetworkChoirTest hoặc Play.

### 2026-11-08 14:45 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | PAUSED | T-037 Choir GĐ2 (code Online xong, chờ chủ cho dựng scene)
- MỚI Features/Choir/Runtime/Online/: TheReturn.Choir.Online.asmdef, ChoirOnlineHost.cs (kế thừa MissionOnlineHostBase; IChoirWorld đo từ vị trí nhân vật, padReach 3,5 m), ChoirOnlineClient.cs (E bấm bục, R làm lại, HUD thẻ riêng), ChoirOnlineAutoRun.cs (-choirauto win|far|late). Compile sạch, Console 0 lỗi, assembly nạp đủ 3 kiểu.
- CÒN (cần chủ cho phép vì mở scene/Play): scene Network/Scenes/NetworkChoirTest.unity (gắn pads/settings/party/bridge cho Host+Client+AutoRun) rồi LAN 3 tiến trình: thắng / quá giờ / client xa bị bác. Chưa chạy Gauntlet sau bước này (không đổi test). Editor: không Play, không mở scene.

### 2026-11-08 14:39 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #15) | DONE phần việc | T-028 sửa lỗi cổng armed nhảy sang cổng khác (4 test «Phong 2_Cua 1»)
- NGUYÊN NHÂN (đã tái hiện bằng test thật + thử tay Play): vùng chuẩn bị của «Hanh lang 2_Ra» sâu 4,00 m (snapshot BeforeNewRooms_20261031) nay 12,13 m (hộp 16,38×12,13 m, xoay 30°), nên vùng ngoài của nó chồng lên chỗ đứng chờ trước «Phong 2_Cua 1». UpdateArming duyệt bindings và gán ArmedGate cho cổng khớp kế tiếp ngay khi cổng hiện tại không còn đủ đội: người đầu bước vào làm Cửa 1 mất «đủ đội» mà đội vẫn nằm trong vùng ngoài rộng của Hanh lang 2_Ra → armed nhảy sang đó, Cửa 1 đóng sập, nhân vật kẹt z≈47,8 (blocker ≈47,2), 3 giây sau vùng chuẩn bị bị nhả. Chỉ 1/30 cổng bị chồng. Không liên quan lớp OnlineLayer (bản SchoolFloorBeforeOnlineLayer_20261104 có hình học y hệt).
- SỬA (cách B, chủ dự án chọn): Maps/SchoolFloor/Runtime/Missions/MissionFlowController.cs, UpdateArming: nếu ArmedGate còn người ở vùng ngoài/đã vào thì GIỮ NGUYÊN và return (disarmTimer=0), chỉ tìm cổng mới khi cổng đang armed đã trống. Không đụng scene, không đụng test. Cách A (thu vùng chuẩn bị Hanh lang 2_Ra) chưa làm vì chưa rõ chủ cố ý mở rộng vùng đó.
- KẾT QUẢ: Compile sạch, Console 0 lỗi/cảnh báo. PlayMode TheReturn.SchoolFloor.PlayTests 32/32 xanh (trước 28/32). EditMode 934 pass / 0 fail / 5 skip (C7, vốn bỏ qua).
- LƯU Ý: tái hiện bằng AssetDatabase/scene additive chỉ để đo, đã đóng, scene SchoolFloorPrototype không dirty. Chạy Play thủ công 2 lần rồi Stop (không có test đang chạy). Build Builds/NetworkMap (2026-11-06 04:57 UTC) chưa build lại: không chứa sửa này, nhưng online chỉ có Cầu ván nên không ảnh hưởng thử 2 máy. chay_client.bat luôn nối 127.0.0.1 (chỉ cùng máy); thử 2 máy thật dùng -netauto relayhost / relayclient -netcode (mã phòng chỉ có trong host.log, map build không có HUD).

### 2026-11-09 16:50 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | PAUSED | T-037 Choir GĐ2 (scene + thử host một máy xong)
- Chủ cho phép («làm đi»). MỚI Features/Network/Scenes/NetworkChoirTest.unity (sao từ NetworkBreakerTest, bỏ Breaker, thêm PF_Choir tại (0,0.25,-3) + đối tượng ChoirOnline: NetworkObject, MissionNetBridge, Host, Client, AutoRun; pads/settings/bridge/party đủ, 0 null). Scene cũ NetworkBreakerTest KHÔNG đụng; chưa thêm scene mới vào Build Settings.
- THỬ Play host một mình (SetTeamSize(2)+StartLanHost): đề 2 người 6 nốt, cửa sổ 3 s, thẻ riêng ô 0 = ký hiệu 0->bục 0, 2->bục 2. Bấm khi đứng xa: «Bạn đứng quá xa bục này.» (progress 0). Đứng giữa bục 0 và 2, bấm 0 rồi 2: tiến 2/6, msg Correct. Chờ >3 s: TimedOut, tiến về 0, Timeouts=1. Console 0 lỗi/0 cảnh báo. Đã Stop, trả SchoolFloorPrototype, không dirty.
- CÒN: LAN 3 tiến trình thật (-choirauto win|far|late) cần bản build có scene này (chưa có); thắng đủ 6 nốt qua 2 máy chưa kiểm (đã có unit test). Chưa chạy Gauntlet sau bước này (chỉ thêm file Runtime/scene, không đổi test).

### 2026-11-10 07:51 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #10) | DONE (pha L) | T-036 GradeRepair
- MỚI (không sửa Domain/scene/prefab/Build Settings): Features/GradeRepair/Runtime/Network/{asmdef, GradeRepairCommand, GradeRepairPublicState, GradeRepairHostRules, GradeRepairCodec}.cs; Tests/EditMode/{asmdef, GradeRepairDomainTests}; Tests/EditModeNet/{asmdef, GradeRepairHostRulesTests, GradeRepairCodecTests, GradeRepairContractAdapter}. Đăng ký MISSIONS_NET.
- Hợp đồng bắt lỗi thật F-005 (C5: bí mật bị thoát ký tự) -> đổi cú pháp sự kiện sang '.' và '_' (gói ngắn hơn ~3 lần). Lỗi F-004 (codec nhận '%' trần) do #14 sửa trong lúc tôi nghỉ; tôi gỡ dòng kiểm tra trùng của mình, giữ bản của #14.
- KẾT QUẢ: Gauntlet 2026-11-08 14:17 XANH: 939 test, 934 đạt, 0 đỏ, 5 bỏ qua (7,1 s); GradeRepair.Network.Tests 65 (64 đạt), Domain.Tests 24. Editor: scene gốc, không Play.
- CÒN: pha O (Online + scene thử), S, H. Dòng GradeRepair trong plan T-036 để PAUSED. NavigationExam, TeacherTruth vẫn chưa ai.

### 2026-11-10 08:16 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | CANCEL + PLAN | T-039 Q-008 (M6) hủy; lập T-040 cắt còn 4 nhiệm vụ
- Chủ dự án quyết (chat 2026-11-10): cắt 12 -> 4 nhiệm vụ (PlankAndKey, Attendance, BreakerPanel, Choir), bỏ 8 nhiệm vụ còn lại, yêu cầu scene mới + thiết kế lại map. Ghi DECISIONS/2026-11-10-scope-cut-4-missions.md.
- Q-008 (M6 scaffolder) CANCELLED: không còn nhiệm vụ mới để sinh khung. Q-009 (M7) gộp vào T-040 P5. Phần mình đã tạo dở cho M6 (Features/MissionTemplate: 3 asmdef + MissionTemplateTypes.cs + MissionTemplateRules.cs, file Online đã xóa) sẽ XÓA (chưa ai dùng, chưa vào registry/Gauntlet).
- MỚI: PLANS/T-040-scope-cut-4-missions.md (hiện trạng, phạm vi, ảnh hưởng task, 3 phương án map, bước P0-P6, rủi ro, câu hỏi mục 9), TASKS T-040, QUEUE Q-010 (READY) + Q-011 (BLOCKED), PENDING P-004..P-008, biển báo START_HERE, CURRENT. T-038 -> BLOCKED (hủy, chờ xác nhận); MISSIONS_NET đánh dấu CẮT GradeRepair/TeacherSays/SharedLantern. KHÔNG sửa dòng T-036/T-037 của #10/#14 (ghi đề xuất trong plan mục 4). Chưa đụng scene/code/Build Settings.
- ĐÍNH CHÍNH: hôm trước mình nói game chưa chơi qua Internet là SAI: Relay (T-023) đã nghiệm thu Internet theo NETWORK.md; và Choir đã có Online + scene thử + adapter (T-037, #14), không phải việc mới.

### 2026-11-10 08:21 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #13) | PROGRESS | T-040 chủ dự án trả lời hướng map
- Chủ dự án chọn: (1) DỰNG MỚI HOÀN TOÀN, (2) XÓA HẲN code 8 nhiệm vụ sau khi scene mới chạy được, (3) giữ hành lang 5 làm hành lang đi lại. Ghi DECISIONS + PENDING (P-004..P-006 ANSWERED; thêm P-009 scene cũ hỏng khi xóa code, P-010 sơ đồ chờ duyệt) + plan mục 10; thêm bước P3a thiết kế layout, P3b dựng, P7 xóa; QUEUE Q-010 (khảo sát, thêm footprint + danh sách xóa) READY, Q-011..Q-013 BLOCKED theo thứ tự. Chưa đụng scene/code.

### 2026-11-10 08:21 UTC | Claude Sonnet 5.5 (phiên #13) | START | T-040 Q-010 khảo sát (CHỈ ĐỌC)
- Đọc code/scene/prefab; KHÔNG sửa Assets. Ghi kết quả vào .ai/PLANS/T-040-dependencies.md (file mới).

### 2026-11-10 08:33 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | START | T-040 adapter hợp đồng PlankAndKey (P4, mục 8 plan)
- Chủ dự án đổi kế hoạch sang T-040. Đã đọc plan + LOG/QUEUE: Q-010 do #13 giữ (START 08:21), Q-011/012/013 BLOCKED nên KHÔNG làm trùng. SharedLantern bị cắt: HỦY kế hoạch GĐ2 SharedLantern (chưa ghi file nào; GĐ1 giữ nguyên, đóng băng). Choir giữ (GĐ2 PAUSED, còn LAN 3 tiến trình thuộc P5).
- Việc nhận: thêm adapter hợp đồng cho PlankAndKey. File định đụng: MỚI Features/PlankAndKey/Tests/EditModeNet/PlankContractAdapter.cs (+ sửa asmdef test để tham chiếu TheReturn.Network.Contract.Tests nếu thiếu), MISSIONS_NET.md (cột adapter của PlankAndKey). KHÔNG đụng Domain/Runtime/scene, không đụng file của #13.

### 2026-11-10 08:36 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | DONE | T-040 adapter hợp đồng PlankAndKey (P4, mục 8 plan)
- MỚI Features/PlankAndKey/Tests/EditModeNet/PlankContractAdapter.cs (bot theo PlankSolver: nhặt thẻ -> thả lên ván -> đặt nhóm -> gạt cần; lượt trung gian bot tự cho luật chạy hết, lượt cuối để hợp đồng tick; ép thua bằng gạt qua lại một người tới quá hạn, nên C7 chạy thật). SỬA asmdef test: thêm TheReturn.Network.Contract.Tests.
- PHÁT HIỆN + SỬA (F-006, MISSION, cùng loại F-001): PlankHostRules.BuildPublicState phát CrossingSecondsLeft/DisplayUsedSeconds đang chạy theo giờ => C8 (150 lỗi) + N1 (98 lỗi) đỏ. Sửa 1 khối trong BuildPublicState: khi đang qua cầu phát giá trị LÚC BẮT ĐẦU lượt (còn lại = tổng; đã dùng trừ phần trôi, làm tròn 3 số lẻ). Chơi thật không đổi hành vi (host chỉ phát lúc gạt cần khi Left == Total; client PlankOnlineClient đã tự đếm từ leftAtSnapshot); chỉ người vào muộn / xin lại trạng thái giữa lượt thấy đồng hồ đếm lại từ đầu lượt. Không đụng Domain, Online, scene. Nếu #9/#10 ghi đè PlankHostRules.cs phải giữ khối F-001, test C8/N1 sẽ đỏ lại nếu mất.
- KẾT QUẢ: Console 0 lỗi; PlankAndKey.Network.Tests 55/55 (10 ca hợp đồng đạt, 0 bỏ qua); Gauntlet 08:33 UTC XANH 949 test, 944 đạt, 0 đỏ, 5 bỏ qua. MISSIONS_NET: adapter PlankAndKey = có. FAILURES: F-006 FIXED. CHƯA kiểm lại bằng 2 máy thật sau sửa này (nên gộp vào smoke P5). Không mở scene, không Play. SharedLantern GĐ2 HỦY theo T-040 (chưa ghi file nào). Q-010 vẫn của #13.

### 2026-11-10 09:10 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | START | T-040 hệ thống dùng chung cho scene mới (Q-014)
- Chủ dự án: #13 lo THIẾT KẾ MAP; #14 hỗ trợ phần hệ thống để đẩy nhanh: (1) luồng/registry cho 4 nhiệm vụ, (2) âm thanh, (3) chuyển người chơi phòng->map. KHÔNG làm khảo sát Q-010 (của #13), KHÔNG vẽ sơ đồ/dựng scene.
- KHẢO SÁT NHANH (chỉ đọc, #13 dùng được cho Q-010): lõi luồng cửa (MissionFlowController, SchoolPreparationGate, SchoolFloorAccessController, MapProgress, MapFlowState, MapFlowOnline, WinScreen) KHÔNG nhắc tên nhiệm vụ nào bị cắt; dính 12 khu chỉ ở: MissionRegistryFactory (hằng ID + CreateDefault = MapProgress.CreateRing(6)), MissionAdapters.cs, SchoolFloorFlow (12 tham chiếu), MissionAudioDirector (switch theo ID khu), MissionTestLauncher, SchoolFloorCanvasPresenter (Phase 0-5 + 12 khu), AccessController (case 0-5 cũ), *Hud riêng của khu bị cắt. Assembly SchoolFloor tham chiếu cứng 8 assembly nhiệm vụ (xóa ở P7 phải gỡ cùng lúc). MapProgress nhận topology tùy ý (new MapProgress(IEnumerable<AreaInfo>), MapFlowState tối đa 32 khu). Luật vào khu: PHÒNG vào được từ Sân hoặc hành lang kề; HÀNH LANG chỉ vào từ hai phòng kề (không từ Sân).
- KHUYẾN NGHỊ cho thiết kế map (để âm thanh/registry/UI cũ dùng lại không sửa): giữ ID khu của 4 nhiệm vụ = Phong_1 (Attendance), Phong_5 (BreakerPanel), Phong_6 (Choir), Hanh_Lang_6 (PlankAndKey); Sân = San. Hanh_Lang_6 phải kề ít nhất một phòng (Phong_6/Phong_1) vì hành lang chỉ vào từ phòng kề.
- File định đụng: MỚI Maps/Core4/Runtime/Domain/** (asmdef TheReturn.Core4.Domain: Core4Layout, kiểm tra layout) + Tests/EditMode (TheReturn.Core4.Domain.Tests); sau đó cầu nối NHỎ cộng thêm (mặc định không đổi hành vi) ở MissionFlowController/MissionRegistryFactory; rồi lớp chuyển người phòng->map online (file mới ở Features/Network). KHÔNG đụng scene, Domain nhiệm vụ, file #13 đang giữ (NETWORK.md mục 4-5, scaffolder đã hủy).

### 2026-11-10 19:19 UTC | Claude Sonnet 5.5 (phiên #13) | DONE | T-040 Q-010 khảo sát
- Ghi PLANS/T-040-dependencies.md. Kết quả: 41 file code ngoài thư mục riêng nhắc 8 nhiệm vụ (36 Maps/SchoolFloor, 4 Art/Editor, 1 GameAudio); Features của 4 nhiệm vụ giữ và Features/Network KHÔNG dính; lõi MissionFlowController/MapProgress/AccessController không nhắc tên; 9 scene dính (SchoolFloorPrototype + 7 bản lưu Before* + _Recovery/0), 9 scene khác an toàn; 8 thư mục cắt = 67 .cs, 9447 dòng; footprint: Attendance ~26x23, Plank ~18,5x16, Choir ~11,6x7, Breaker tường ~14. Chỉ đọc, không sửa Assets (scene gốc, không dirty). Bắt đầu Q-011 (thiết kế layout).

### 2026-11-10 19:21 UTC | Claude Sonnet 5.5 (phiên #13) | DONE | T-040 Q-011 thiết kế layout (đề xuất v1)
- Ghi PLANS/T-040-map-design.md: sân dài 128x26 m phía nam + một hàng phía bắc (Cầu dao 30x30, hành lang đi lại 12x30, Choir 28x26, PlankAndKey 26x22, Điểm danh 32x28), 4 cổng, luật tiến trình giữ nguyên, kích thước tối thiểu theo footprint nêu rõ. Sơ đồ vẽ trong chat. CHƯA dựng gì trong Unity; chờ chủ dự án duyệt (P-010) rồi mới mở Q-012.

### 2026-11-10 19:23 UTC | Claude Sonnet 5.5 (phiên #13) | PROGRESS | T-040 layout v2
- Chủ dự án bác v1; chọn: bố cục vòng quanh sân/chữ L/U + kích thước thu sát. Viết lại PLANS/T-040-map-design.md thành v2 chữ U quanh sân giữa 36x30 (Cầu dao 24x20, hành lang đi lại 32x10, Choir 20x16, PlankAndKey 30x20, Điểm danh 30x26), tổng ~90x50 m, 4 cổng. Vẫn chưa dựng gì; P-010 CHỜ DUYỆT v2.

### 2026-11-10 19:24 UTC | Claude Sonnet 5.5 (phiên #13) | PROGRESS | T-040 layout v3
- Chủ dự án bác v2 (chữ U), muốn vòng khép kín. Viết PLANS/T-040-map-design.md v3: sân giữa 36x30 bao kín bốn phía, thêm hành lang khép vòng phía nam (~1.260 m2), tổng ~90x60 m, 4 cổng; nêu rõ khép kín = bao quanh sân, không phải vòng đi xuyên các phòng; phương án rẻ hơn: thay hành lang bằng dải tường. Chưa dựng gì; P-010 CHỜ DUYỆT v3.

### 2026-11-10 19:35 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | PROGRESS | T-040 Q-014 (1)+(2) xong, tiếp (3)+(4)
- Chủ dự án (11-11 02:28 giờ VN): #13 giữ thiết kế map; #14 hỗ trợ chuyển người chơi, âm thanh, hệ thống xử lý cho scene mới. Kiểm lại: (1) Maps/Core4/Runtime/Domain/Core4Layout.cs + Tests (TheReturn.Core4.Domain.Tests 13/13 xanh) và (2) cầu nối MapTopologyProvider (SchoolFloor/Runtime/Missions, MissionFlowController.topology, để trống = hành vi cũ) + Core4TopologyProvider đã có; compile sạch, Console 0 lỗi. Chưa đụng scene. Tiếp: (3) chuyển người phòng->map online, (4) âm thanh 4 khu.

### 2026-11-10 19:50 UTC | Claude Sonnet 5.5 (chat + Unity MCP, phiên #14) | START | T-040 Q-014 (3) đường phòng -> map online (P-001)
- Khoảng trống đo được: không ai nạp scene khi RoomRoster.Started; RoomService tắt CreatePlayerObject; NetworkManager scene phòng KHÔNG có PlayerPrefab và NetworkPrefabsList_Room chỉ có PF_RoomRoster (EnableSceneManagement=1, ForceSamePrefabs=1, NGO 2.13.3). SlotAllocator chia ô nhỏ nhất còn trống theo thứ tự spawn => spawn theo thứ tự ô phòng thì ô avatar = ô phòng.
- File định đụng (TẤT CẢ MỚI): Features/Network/Runtime/Framework/RoomHandoffPlan.cs (thuần) + Tests/EditMode/RoomHandoffPlanTests.cs; Features/Network/Handoff/{TheReturn.Network.Handoff.asmdef, RoomMapHandoff.cs}. KHÔNG sửa file cũ (Room/Online/scene). Đặt vào scene phòng + thử Play cần chủ dự án cho phép (mở scene).

### 2026-11-11 | Git maintenance | START | Tách các thay đổi đang chờ thành commit theo nhóm
- Rà soát diff tracked/untracked, giữ nguyên nội dung hiện có; chỉ stage các nhóm đã kiểm tra.
- Không push; không đổi nhánh. Chưa có dòng task riêng trong TASKS.md cho thao tác quản lý Git.

### 2026-11-11 | Git maintenance | DONE | Chia thay đổi thành commit chức năng
- Tạo 3 commit chức năng: online mission framework, Core4/map handoff, Unity render settings/folder metadata; commit docs riêng.
- Để lại scene blockout, snapshot/recovery, chỉnh vật liệu, và cài đặt editor cá nhân ngoài commit do phạm vi map còn chờ duyệt.
