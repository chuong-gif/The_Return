# T-037: NHÓM B online: đồng hồ thời gian thực + đứng đúng vùng (Choir, TeacherSays, SharedLantern)
Trạng thái: TODO, chưa ai nhận | Tạo: 2026-11-04 UTC (phiên #7, tách từ T-031) | Mức hiện tại: M1 (chỉ tài liệu) | Chủ dự án chưa duyệt cách nhóm
Đọc trước: NETWORK.md (toàn bộ), PLANS/T-027-plank-network-pattern.md (PlankAndKey có đồng hồ qua cầu: mẫu gần nhất cho Tick), PLANS/T-022-design-2to4.md, DECISIONS/2026-11-04-t031-split.md

## 1. Mục tiêu
Mỗi nhiệm vụ chơi trọn qua mạng với đội 2/3/4 người, trong đó HOST giữ đồng hồ và quyết định «ai đang đứng ở đâu lúc nào» để chấm luật. Client chỉ hiển thị. Giữ nguyên chế độ 1 máy và Domain.

## 2. Vì sao gộp nhóm này
Cả ba chấm luật theo vị trí và thời gian: Choir (đứng trên bục trong cửa sổ giữa hai nốt), TeacherSays (đứng/đi trong vùng, hạn 8 hoặc 5 giây), SharedLantern (nằm trong vùng sáng, quá 5 giây ngoài vùng thì bị đưa về). Dùng chung: đồng hồ Tick trên host, việc suy ra «người ở trong vùng nào» từ vị trí nhân vật (IWorld.ZoneOf/IsInside), và cách phát trạng thái đúng nhịp.

## 3. Phạm vi
Trong: lớp mạng + test + lớp Online + scene thử + build/kịch bản + thẻ hệ thống cho Choir, TeacherSays, SharedLantern; đổi ChoirRules.Online theo số người (xem mục 5) và sửa test cho khớp.
Ngoài: đổi gameplay/độ khó ngoài quyết định T-022 mục 2; nối cửa ra / luồng map (T-028); chat thoại; sửa scene map; sửa Build Settings.

## 4. Quy trình cho MỖI nhiệm vụ
Giống T-036 mục 4 (8 bước: đọc, test Domain, lớp mạng thuần, test EditModeNet, lớp Online, scene thử bằng Additive, kiểm thử nhiều tiến trình, ghi tài liệu). Khác biệt của nhóm này:
- Domain của cả ba ĐÃ CÓ test NUnit (Choir.Domain.Tests, TeacherSays.Domain.Tests, SharedLantern.Domain.Tests) và asmdef Domain đã noEngine: không cần bước viết test Domain mới, chạy bộ test hiện có trước và sau khi thêm lớp mạng.
- Mọi đồng hồ chạy TRÊN HOST bằng Tick(deltaTime) (mẫu PlankHostRules/AttendanceHostRules.Tick); khi Tick làm đổi trạng thái mà không có lệnh thì Online host gọi HostAuthority.MarkChanged().
- IWorld phải trả lời bằng vị trí host thấy (OnlineAvatar.transform.position), KHÔNG tin client khai vị trí. Thử trường hợp client ở xa thật.
- Cẩn thận phát lại: client nhận trạng thái theo Version; đồng hồ hiển thị ở client chỉ nội suy từ số giây còn lại host gửi, không tự quyết.

## 5. Đặc thù từng nhiệm vụ (nguồn: PLANS/T-022-design-2to4.md; Domain/test: kiểm 2026-11-04)
Choir (Phong_6):
- Luật: 4 bục xếp một hàng (cách nhau 3 / 4 / 3 m, hai đầu cách 10 m); dãy 6 nốt; thẻ nhạc chia theo người (N=2: 2+2; N=3: 2+1+1; N=4: 1 mỗi người); người đứng trên bục theo thẻ.
- VIỆC BẮT BUỘC (quyết định chủ dự án/AI 2026-10-31, T-022 mục 2 câu 3): cửa sổ giữa hai nốt khi ONLINE đổi theo N: N=2 -> 3 giây, N=3 -> 2,5 giây, N=4 -> 2 giây. Hiện ChoirRules.Online cố định 2,0 giây và test ChoirTests đang khẳng định 2,0: phải đổi code VÀ test có chủ đích, ghi rõ trong LOG. Giá trị là điểm khởi đầu, chỉnh theo chơi thật.
- Mạng: RIÊNG = thẻ nhạc của mình; CHUNG = nốt hiện tại, bục nào có ai, còn bao lâu. Host suy ra «ai đứng trên bục nào» từ vị trí lúc hết cửa sổ.
- Domain: noEngine CÓ; 4 file .cs; có test (Choir.Domain.Tests). Chưa có lớp mạng.
TeacherSays (Hanh_Lang_5):
- Luật: 5 đoạn; mỗi đoạn một người nghe «Cô nói», người nghe KHÁC đoạn liền trước và mọi người nghe >= 1 lần (TeacherSaysGenerator.IsValid, nên N=2 luân phiên); 2-3 lệnh thật; 3 cảnh cáo thì về đầu; hạn 8 giây (bước tới) / 5 giây (đứng yên) cho cả đội, không đổi theo N; mỗi đoạn là vùng 10,5 x 5 m.
- Mạng: RIÊNG = tiền tố «Cô nói» (chỉ 1 người thấy mỗi đoạn); CHUNG = lệnh hiện tại, hạn còn lại, số cảnh cáo. Host phải phân biệt «đang bước» và «đứng yên» của từng người: suy ra từ vị trí đo theo thời gian (tốc độ đi bộ/chạy lấy từ PrototypePartyController) hoặc dùng trạng thái avatar nếu T-038/C0 đã có.
- Domain: noEngine CÓ; 4 file .cs; có test. [CẦN KIỂM] teachers[4] dựng cho 4 người: N<4 ẩn phần thừa.
SharedLantern (Hanh_Lang_4):
- Luật: 4 biển (6 ký hiệu); 1 đèn lồng, bán kính sáng R (đèn Point range 6 m trong scene); ai ở ngoài vùng sáng quá 5 giây thì bị đưa về điểm đầu; mỗi người tự đọc ~2 biển (N=2) tới ~1 (N=4); sổ riêng từng người, không tự chia sẻ; đèn lồng trao tay bằng E trong 2 m; đèn ban đầu ở người số 0.
- Mạng: RIÊNG = sổ của mình (những biển mình đã đọc); CHUNG = ai cầm đèn, ai đang ngoài vùng sáng và còn mấy giây. Host kiểm mỗi Tick vị trí từng người so với vị trí/bán kính đèn; trao đèn là lệnh có kiểm khoảng cách 2 m; đọc biển cần đứng đủ gần biển (host kiểm).
- Domain: noEngine CÓ; 3 file .cs; có test. [CẦN KIỂM] đội 4 người khó cùng nằm trong R: theo dõi khi chơi thật (T-022: nếu hay bị đưa về thì tăng bán kính).

## 6. Nghiệm thu nhóm
- Mỗi nhiệm vụ: test EditModeNet xanh (đội 2/3/4) + test Domain cũ vẫn xanh (riêng Choir: test cửa sổ theo N được sửa có chủ đích); 3 tiến trình LAN ca thắng + ca thua + ca hết giờ/cảnh báo/bị đưa về có log (M4); compile 0 lỗi/0 cảnh báo; scene thử lưu; Build Settings sạch; thẻ hệ thống có mục Mạng.
- Cần ít nhất 1 ca kiểm «client đứng xa nên bị từ chối/không tính» để chứng minh host không tin vị trí do client khai.

## 7. Bảng nhận việc
| Nhiệm vụ | Trạng thái | AI giữ | Cập nhật | Ghi chú / bằng chứng |
|---|---|---|---|---|
| Choir | PAUSED (GĐ1 xong) | Claude Sonnet 5.5 (phiên #14) | 2026-11-07 | GĐ1 XONG (M3): Runtime/Network + Tests/EditModeNet 65/66 đạt (C7 bỏ qua), Gauntlet XANH 763/766. CÒN GĐ2: Runtime/Online + NetworkChoirTest + LAN (M4). P-003/P-004 PROVISIONAL |
| TeacherSays | PAUSED (GĐ1 xong) | Claude Sonnet 5.5 (phiên #11) | 2026-11-05 | GĐ1 XONG (M3): Runtime/Network + Tests/EditModeNet 70 test xanh, EditMode toàn bộ 645/645. CÒN GĐ2: Runtime/Online + scene NetworkTeacherSaysTest + LAN (M4). Chờ chủ dự án xác nhận quyết định đồng hồ riêng (mục 9) |
| SharedLantern | PAUSED (GĐ1 xong) | Claude Sonnet 5.5 (phiên #14) | 2026-11-07 | GĐ1 XONG (M3): Runtime/Network + Tests/EditModeNet 83/84 đạt (C7 bỏ qua), Domain 37/37. CÒN GĐ2: Runtime/Online + NetworkSharedLanternTest + LAN (M4). P-005 PROVISIONAL |

## 8. Đề xuất / rủi ro
- Thứ tự gợi ý: Choir (đã có việc đổi cửa sổ theo N, vùng đứng đơn giản nhất) -> SharedLantern -> TeacherSays. Ước lượng 1,2-1,5 đơn vị Attendance mỗi nhiệm vụ.
- Rủi ro: độ trễ mạng làm cửa sổ 2-3 giây dễ chấm sai khi chơi thật qua Internet (vị trí host thấy trễ hơn vị trí người chơi thấy). Test LAN không lộ ra. Ghi chú rõ ở thẻ hệ thống và đề xuất cho chủ dự án thử qua Relay với bạn bè (M5). Không tự nới luật thay chủ dự án.

## 9. TeacherSays — thiết kế mạng và kết quả giai đoạn 1 (phiên #11, 2026-11-05)
- Đã làm (file MỚI): Features/TeacherSays/Runtime/Network/{asmdef, TeacherSaysCommand, ITeacherSaysWorld, TeacherSaysPublicState, TeacherSaysHostRules, TeacherSaysCodec}.cs và Tests/EditModeNet/{asmdef, TeacherSaysHostRulesTests, TeacherSaysCodecTests}.cs. Không đụng Domain, scene, prefab, Build Settings.
- Kết quả (M3): compile sạch, Console 0 lỗi/0 cảnh báo; 70/70 test mới; EditMode toàn bộ 645/645 (575 cũ + 70). Chưa có M4 (LAN) và M5.
- QUYẾT ĐỊNH CẦN CHỦ DỰ ÁN XÁC NHẬN: brief ghi trạng thái CHUNG gồm «hạn còn lại». Tôi KHÔNG làm vậy: đoạn Bước có hạn 8 giây, đoạn Đứng đếm 5 giây, nên một đồng hồ chung cho phép người không nghe suy ra đáp án (đếm từ 8 là Bước, từ 5 là Đứng) và làm mất ý nghĩa «chỉ một người biết». Nên đồng hồ và cờ «Cô nói» chỉ gửi RIÊNG cho người nghe. Hệ quả: đồng đội không có đồng hồ, chỉ thấy lệnh loa, ai là người nghe, cảnh cáo và sự kiện sau khi xảy ra. Muốn đảo lại thì thêm một trường vào TeacherSaysPublicState (nhưng nên đổi luật để hai đồng hồ giống nhau).
- Lưu ý bản 1 máy: TeacherSaysPrototype.Notebook hiện đồng hồ Bước/Đứng cho MỌI vai, tức tự lộ đáp án. Không phải lỗi khi một người điều khiển cả 4 vai; chỉ cần sửa nếu sau này dùng chung giao diện. Tôi không sửa.
- GĐ2 cần làm (chưa ai nhận): (1) Runtime/Online (TheReturn.TeacherSays.Online) theo mẫu Attendance/Breaker: TeacherSaysOnlineHost cài ITeacherSaysWorld từ OnlineAvatar.transform.position (ngưỡng đi lấy từ TeacherSaysSettings.movingSpeed, hộp vùng từ 5 BoxCollider SegmentZone theo toạ độ cục bộ), gọi Begin khi đội vào khu, Tick mỗi frame và MarkChanged khi Tick trả true; dịch chuyển đội về StartPoint và đặt lại mốc đo khi SentBacks hoặc RetryCount tăng. (2) Client: HUD lệnh loa + ai nghe + cảnh cáo; người nghe thấy thêm tiền tố và đồng hồ; phím R gửi Retry. (3) Scene thử NetworkTeacherSaysTest (Additive, không thay scene đang mở của chủ dự án). (4) LAN 3 tiến trình: ca thắng, ca bị đưa về, ca client ở xa bị host bác (M4).
- Rủi ro đã biết: độ trễ mạng làm vị trí host thấy trễ hơn vị trí người chơi thấy; cửa sổ 1 giây phản ứng (ReactionSeconds) và ngưỡng đi 0,4 m/s có thể chấm sai khi chơi qua Internet. Test LAN không lộ ra; cần thử qua Relay (M5). Không tự nới luật.
