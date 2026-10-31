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

## 6. CHƯA làm: cầu nối NGO (mỗi nhiệm vụ dùng lại một lớp chung)
- NetworkBehaviour trong scene: ServerRpc nhận lệnh (đã tuần tự hóa) -> tra ô của người gửi qua OnlinePartySlots/SlotAllocator (T-025) -> HostAuthority.Submit; host gọi BuildBroadcast mỗi khi Version đổi và gửi từng gói cho đúng client (ClientRpc nhắm một client); client đưa vào ClientReplica.Receive; lệnh bị từ chối gửi lại Detail cho đúng người gửi để hiện thông báo.
- Tuần tự hóa lệnh/trạng thái: JsonUtility hoặc INetworkSerializable; chọn một cách và dùng chung cho mọi nhiệm vụ.

## 7. CHƯA làm: Unity trên host và Presentation online của PlankAndKey
- IPlankWorld cài đặt từ vị trí avatar + vùng deckZone/khoảng cách thẻ/cần (lấy số đo từ PlankAndKeyPrototype hiện có: interactDistance + 0.8 cho thẻ, InBox(deckZone)).
- Chạy PlankHostRules.Tick(Time.deltaTime) trên host, MarkChanged khi trả true; khi ResetCount đổi thì đưa cả đội về đầu khe và đặt ván/thẻ (như PlaceAttempt).
- PlankAndKeyPrototype có chế độ online: không tự chạy luật; phím E/G/R gửi PlankCommand; ván/thẻ/người trên ván hiển thị từ PlankPublicState; HUD tốc độ cặp sách chỉ từ PlankPrivateState của máy này.
- Scene: thử trong scene riêng Features/Network (tự do); đưa vào SchoolFloorPrototype/map CHỈ sau khi OWNER_UPDATES cho phép sửa scene map.

## 8. Điều kiện để làm tiếp 6-7
- Chủ dự án thử 2 bản của T-025 (SYSTEM_CARDS/OnlineAvatars.md mục C) và báo kết quả.
- Phép sửa scene map nếu muốn đưa vào hành lang thật.
