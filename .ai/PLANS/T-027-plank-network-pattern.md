# T-027: Mẫu nối nhiệm vụ qua mạng (lấy PlankAndKey làm gốc)

Cập nhật: 2026-10-31 05:04 UTC (phiên #9). Phần 1-5 ĐÃ làm và có test; phần 6-7 CHƯA làm (xem mục 8).

## 1. Nguyên tắc
- Host quyết định, máy khách chỉ gửi LỆNH và hiển thị TRẠNG THÁI. Luật chạy đúng một chỗ: host.
- Domain/Session thuần C# của nhiệm vụ KHÔNG ĐỔI. Mỗi nhiệm vụ thêm một asmdef lớp mạng riêng (thuần C#, noEngineReferences) đứng trên Domain và khung T-024 (TheReturn.Network.Framework).
- Số nhân vật = số người (2-4), ô cố định (T-025): ô = chỉ số người chơi trong Domain. Không đổi vai.

## 2. Năm thứ cần viết cho mỗi nhiệm vụ (xem Features/PlankAndKey/Runtime/Network làm mẫu)
1. XxxCommand (bất biến): các hành động người chơi, KHÔNG chứa ô người gửi (host lấy ô từ kết nối: HostAuthority.Submit(ô, số thứ tự, lệnh)). Cần thêm tham số thì chỉ là tham số gạt cần, ô đích... không phải danh tính.
2. IXxxWorld: những điều host quan sát từ thế giới vật lý (ai đứng gần vật nào, ai đứng trong vùng nào). Lớp Unity trên host cài đặt từ vị trí nhân vật; test dùng FakeWorld. KHÔNG nhận vị trí từ máy khách.
3. XxxPublicState (bất biến, chụp mới mỗi lần) và XxxPrivateState (struct). Public = mọi người thấy; Private = chỉ riêng một ô (vd tốc độ cặp sách, sổ riêng, tiền tố Cô nói). Khi nghi ngờ một thông tin có nên riêng không: hỏi thiết kế (PLANS/T-006-mission-designs.md), mặc định là riêng nếu thiết kế bảo người chơi phải hỏi đồng đội.
4. XxxHostRules : IHostRules<Command, Public, Private>: 
   - Validate: KHÔNG đổi trạng thái; kiểm ô thuộc đề, điều kiện lệnh, hỏi IXxxWorld; trả RuleVerdict.Reject(«lý do tiếng Việt») hoặc Accept. Nếu Apply cần dữ liệu đã tính trong Validate (vd danh sách người trên ván) thì lưu vào trường, vì HostAuthority gọi Validate rồi Apply liền nhau.
   - Apply: gọi Session của Domain; kết quả Domain không phải lỗi người dùng (vd hết giờ) vẫn là lệnh được chấp nhận, ghi vào trạng thái bằng enum Message.
   - BuildPublicState / BuildPrivateState(slot): trả bản mới/bản sao; Private chỉ chứa dữ liệu của đúng ô đó.
   - Tick(dt): đồng hồ trên host; trả true khi có thay đổi mà không lệnh nào gây ra (đến nơi, tự đặt lại) để lớp Unity gọi HostAuthority.MarkChanged().
   - Đặt lại: tăng ResetCount; lớp Unity trên host thấy ResetCount đổi thì dịch chuyển người/vật về đầu (rules không biết Transform).
5. Test EditMode (xem PlankHostRulesTests): FakeWorld; mỗi lệnh chấp nhận + từ chối; thất bại/hết giờ; Tick một lần; Private đúng người; MỘT ván thắng đầy đủ qua HostAuthority + ClientReplica; chạy cho 2/3/4 người ([TestCase]).

## 3. Quy tắc dễ sai
- Không đọc ô từ nội dung lệnh. Không để máy khách báo vị trí/trạng thái của mình.
- Validate không đổi gì; nếu đổi thì lệnh bị từ chối vẫn làm đổi trạng thái.
- Snapshot Public phải bất biến (sao mảng khi tạo), vì HostAuthority chia cùng một bản cho nhiều người.
- Đồng hồ chạy ở host. Máy khách tự ngoại suy đếm ngược giữa hai gói (CrossingSecondsLeft + đồng hồ local) chỉ để hiển thị.
- Người vào lại: host gọi HostAuthority.ConnectSlot (đặt lại số thứ tự) + BuildStateFor(ô); máy khách gọi ClientReplica.Reset rồi Receive. Giữ đúng ô cũ là việc của T-028.
- Tên tiếng Việt ở trạng thái/thông báo; mã enum ở dữ liệu.

## 4. Đặt tên và nơi để file (cho nhiệm vụ Yyy)
- Features/Yyy/Runtime/Network/: TheReturn.Yyy.Network.asmdef (references: Domain của Yyy + TheReturn.Network.Framework; noEngineReferences), YyyCommand.cs, IYyyWorld.cs, YyyPublicState.cs, YyyHostRules.cs.
- Features/Yyy/Tests/EditModeNet/: asmdef test + YyyHostRulesTests.cs.

## 5. Đã xác minh cho PlankAndKey (mức 3)
- 21 test EditMode xanh (TheReturn.PlankAndKey.Network.Tests): đội 2/3/4, lệnh nhặt/thả/gạt/đặt lại, từ chối đúng lý do, ván chạy + đến nơi một lần, hết giờ tự đặt lại, ván thắng 3 người qua HostAuthority + ClientReplica (mọi máy thấy thắng, mỗi máy chỉ thấy tốc độ cặp sách của mình), 2 người cùng qua và thắng.

## 6. Cầu nối NGO dùng chung (ĐÃ VIẾT + chạy Play Mode host một mình, 2026-10-31 15:55 UTC; chưa chạy Play Mode vì Editor đang kẹt lỗi biên dịch của phiên khác)
- Features/Network/Online/MissionNetBridge.cs: NetworkBehaviour đặt trên NetworkObject nằm sẵn trong scene; chỉ chuyển CHUỖI. Máy khách: SendCommand(số thứ tự, chuỗi), RequestState(). Host: SendState(clientId, phiên bản, chuỗi chung, chuỗi riêng). Giao diện IMissionHostEndpoint (OnCommand trả null nếu chấp nhận hoặc lý do từ chối; OnStateRequested) và IMissionClientEndpoint (OnState; OnRejected). Giới hạn 2048 ký tự mỗi chuỗi.
- Mỗi nhiệm vụ chỉ cần: một Codec thuần C# (PlankCodec là mẫu: InvariantCulture, giải mã KHÔNG ném lỗi, trả false khi sai định dạng/ngoài khoảng) + Host + Client như mục 7.
- OnlinePartySlots (T-025) thêm Roster/SlotOf/TryGetClient: host dùng để đổi mã client <-> ô; OnlinePartyLocalDriver thêm Seated (nhiệm vụ đặt true khi người chơi ngồi trên phương tiện).

## 7. Host + Client của PlankAndKey (ĐÃ VIẾT + chạy Play Mode host một mình, chưa chạy; asmdef TheReturn.PlankAndKey.Online)
- PlankLane: hình học dùng chung (vị trí ván theo tiến độ, điểm đứng đầu khe, vị trí thẻ theo PlankCardMode, vùng ván). Online coi bờ A là bờ đầu khe.
- PlankOnlineHost (chỉ chạy khi là host): dựng PlankHostRules theo OnlinePartySlots.TeamSize, IPlankWorld lấy từ vị trí nhân vật + PlankInteractable + deckZone, Tick đồng hồ, phát trạng thái mỗi khi Version đổi, trả trạng thái khi client hỏi.
- PlankOnlineClient (mọi máy): E/G/R -> lệnh; vẽ ván/thẻ/nhãn/HUD từ ClientReplica; người ngồi trên ván được đưa đi theo ván TRÊN MÁY CỦA CHÍNH HỌ (nhân vật do chủ máy điều khiển, host không đẩy vị trí); về đầu khe khi ResetCount đổi; xin lại trạng thái mỗi giây cho tới khi nhận được gói đầu.
- PlankAndKeyPrototype cũ KHÔNG sửa: để Active = false (không gọi Begin), chỉ giữ điểm mốc.
- Thẻ từ: luật host giữ PlankCardMode (AtHome/Held/OnFloor/OnDeck) + toạ độ làn để mọi máy vẽ giống nhau.

## 8. Đã xác minh và còn lại (2026-10-31 21:19 UTC)
- ĐÃ xác minh trong Play Mode (Editor làm host một mình, scene Features/Network/Scenes/NetworkPlankTest.unity): bridge spawn đúng; host dựng luật theo đội 2 người; client nhận gói đầu qua RPC; nhặt thẻ (thẻ theo tay người cầm); đứng lên ván và gạt cần: ván chạy 6 giây, người ngồi trên ván đi theo ván, thẻ theo người; đến nơi: phiên bản host = bản sao; lệnh sai bờ và lệnh sai định dạng bị host từ chối, lý do hiện lên HUD, phiên bản không đổi; Retry: cả đội về đầu khe, thẻ về chỗ cũ, ván về bờ đầu. Console 0 lỗi/cảnh báo. EditMode toàn dự án 430/430.
- CHƯA xác minh: 2 MÁY THẬT (đường client -> RPC -> host -> RPC -> client chỉ chạy với host tự gửi cho chính mình), thắng/thua đủ đội 2-4 người trên máy thật, người vào lại.
- Cách chủ dự án thử: Builds/NetworkPlank (không nằm trong git): bấm đúp chay_host.bat, đợi vài giây, rồi chay_client.bat. Mỗi cửa sổ: Tab khóa chuột, WASD, chuột; E nhặt thẻ / gạt cần, G thả thẻ, R làm lại. Đội mặc định 2 người (PartyRoot > OnlinePartySlots > Team Size trong scene).
- Đưa vào SchoolFloorPrototype / Hanh_Lang_6: chủ dự án ĐÃ CHO PHÉP sửa scene (2026-10-31) nhưng phiên #6 đang sửa scene này (T-034), nên chưa làm. Việc của T-028 (luồng map qua mạng: cổng, khu chuẩn bị, MapProgress, màn thắng chạy trên host).
- Lưu ý chung cho nhiệm vụ khác: công cụ build (manage_build) TỰ THÊM các scene được build vào Build Settings; build scene thử xong phải gỡ khỏi Build Settings.
