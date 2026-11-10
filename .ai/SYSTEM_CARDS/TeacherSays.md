# TeacherSays — Hanh_Lang_5 «Cô nói»

Trạng thái: REVIEW (2026-10-18 15:56 UTC).

## A. Luật
5 đoạn hành lang. Loa đọc lệnh Bước/Đứng; có lệnh thật thì chỉ người nghe được tiền tố «Cô nói» thấy rõ; không có tiền tố thì làm NGƯỢC. Bước: cả đội vào vùng sáng trong 20s (+3s mỗi lần đổi vai). Đứng: cả đội đứng yên 5s (di chuyển sau 1s ân hạn = cảnh cáo). 3 cảnh cáo -> về điểm xuất phát. Chỉnh trong Data/TeacherSays_VI.asset (live tuning). Online preset 8s/0 có sẵn.

## B. Code
Features/TeacherSays/Runtime (Domain: Rules/Round/Generator/State; Data: Settings; Presentation: TeacherSaysPrototype). Adapter: Maps/SchoolFloor/Runtime/Missions/TeacherSaysMissionAdapter.cs, area id Hanh_Lang_5. Test: TeacherSaysTests (18).

## C. Scene
TeacherSays_LogicOnly (SegmentZone_1..5 y=11.5 size (10.5,4,5), StartPoint_1..4, SpeakerBoard). Cổng: nhóm Hanh_Lang_6_Preparation ('Hanh lang 6_Vao/Ra' do nhãn cũ). Snapshot BeforeTeacherSays_20261018_155142.unity.

## D. Còn thiếu
Người chơi thật; thử nhánh cảnh cáo/SentBack/RestartMap bằng chạy thật; thiết kế mô hình 3D thay hộp.

## E. Mạng (T-037, giai đoạn 1 xong 2026-11-05 — mức 3: compile + EditMode, CHƯA chạy LAN thật)
- Lớp thuần C# (asmdef TheReturn.TeacherSays.Network, namespace TheReturn.TeacherSaysNet, Features/TeacherSays/Runtime/Network): TeacherSaysCommand (chỉ Retry), ITeacherSaysWorld (host cung cấp: Sample, IsMoving, IsInZone), TeacherSaysPublicState (+ TeacherSaysPrivateState, TeacherSaysNetEvent), TeacherSaysHostRules (bọc Domain; Begin, Tick, Validate/Apply), TeacherSaysCodec. Domain KHÔNG đổi.
- Chung (mọi người thấy): đoạn, lệnh loa, AI là người nghe, cảnh cáo, lần bị đưa về, lần làm lại, sự kiện gần nhất. RIÊNG (chỉ người nghe): cờ «Cô nói», mili giây còn lại, cờ đã đứng đủ giờ. Đồng hồ KHÔNG nằm trong trạng thái chung vì Bước 8 giây và Đứng 5 giây khác nhau nên sẽ làm lộ đáp án.
- Host quyết ai đứng đâu và ai đang đi từ vị trí host thấy (ITeacherSaysWorld); máy khách không khai gì. Lớp Unity phải dịch chuyển đội khi SentBacks hoặc RetryCount tăng, và đặt lại mốc đo chuyển động sau khi dịch chuyển.
- Test: Features/TeacherSays/Tests/EditModeNet (assembly TheReturn.TeacherSays.Network.Tests, 70 test; đội 2/3/4; 40 seed × 3 cỡ đội chơi tới thắng).

| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Người không nghe vẫn biết đáp án | TeacherSaysHostRules.BuildPublicState (không được thêm cờ hoặc đồng hồ) | Test Public_DoesNotDependOnTeacherSaid |
| Máy khách ở xa vẫn được tính qua đoạn | ITeacherSaysWorld.IsInZone (do host đo) | Test Step_OnePlayerFarAway_*, Stand_Done_ButOnePlayerFarAway_* |
| Chuỗi mạng hỏng làm host lỗi | TeacherSaysCodec.TryDecode* | Test *_Malformed_*, Fuzz_RandomStrings_* |
| Cảnh cáo, đưa về, qua đoạn lệch | TeacherSaysState.Tick (Domain) | TeacherSaysTests (18) + Step_*/Stand_* |
- CÒN: giai đoạn 2 (Runtime/Online + scene thử NetworkTeacherSaysTest + LAN 3 tiến trình), chơi thật 2 máy; tinh chỉnh ngưỡng đi/đứng yên (TeacherSaysSettings.movingSpeed 0,4 m/s) khi có độ trễ mạng.
