# KẾ HOẠCH T-039: Đẩy nhanh online — quy trình chịu ngắt limit + bộ test hợp đồng chạy MỘT lượt   (file: .ai/PLANS/T-039-online-fast-track.md)

Trạng thái: ĐÃ DUYỆT 2026-11-06 | AI giữ: phiên #13 (lập bởi Claude Sonnet 5.5, phiên #10) | Cập nhật: 2026-11-06 05:26 UTC

## RESUME (AI vào sau: đọc khối này TRƯỚC, 5 dòng là đủ)
- Trạng thái thật: ĐÃ DUYỆT (cả M5 và PROVISIONAL). S0.1-S0.3 DONE. Chưa có code test nào.
- NEXT (2026-11-10 08:16, phiên #13): PHẠM VI ĐỔI (T-040, chủ dự án cắt còn 4 nhiệm vụ). M6 HỦY (Q-008 CANCELLED). M7 gộp vào T-040 P5. Còn lại của T-039: dọn sổ (CURRENT/BOARD), adapter PlankAndKey (nằm trong T-040 P4). Chuyển Attendance/PlankAndKey sang lớp cơ sở Online: chỉ khi chủ dự án cho phép riêng. Dọn dở: thư mục Features/MissionTemplate (mình tạo, chưa dùng) phải XÓA.
- Nếu chủ dự án đã duyệt mà S0.1 trong bảng mục 6 vẫn TODO: đánh dấu DONE rồi bắt đầu từ S0.2.
- Kiểm xong một bước: Console 0 lỗi + test của bước (ghi tên + kết quả vào bảng mục 6).
- Yêu cầu gốc của chủ dự án: DECISIONS/2026-11-06-t039-owner-requirements.md.

## 1. Mục tiêu và nghiệm thu
Mục tiêu: (a) AI nhận việc rồi làm nhanh, ít đọc, ít chờ; (b) quy trình chịu được việc AI hết limit giữa chừng và AI khác (hoặc chính nó sau ~5 giờ) vào tiếp; (c) xây hết LOGIC các nhiệm vụ online trước, chạy test MỘT lượt cho tất cả; (d) test đỏ tự chỉ ra nhiệm vụ nào, task nào, phiên AI nào viết, và người giữ nhiệm vụ đó sửa.
Nghiệm thu (đo được):
- A. MỘT lệnh «Gauntlet» chạy mọi nhiệm vụ online đã nối × đội 2/3/4 × nhiều seed, ghi bảng `.ai/GAUNTLET.md`; mục tiêu < 60 giây (ĐO ở M4, chưa cam kết).
- B. Mỗi test đỏ có sẵn: nhiệm vụ, task, người viết (phiên/ngày), trong `.ai/FAILURES.md` — không phải suy luận tay.
- C. Thử ngắt: một AI mới chỉ đọc QUEUE + khối RESUME tiếp được thẻ dở, không hỏi chủ dự án, không làm lại phần đã xong.
- D. Thêm một nhiệm vụ vào Gauntlet chỉ cần một adapter nhỏ (GIẢ THUYẾT ≤ ~80 dòng; đo ở M3 bằng Attendance + BreakerPanel).

## 2. Phạm vi
- Được đụng (file MỚI): Features/Network/Tests/Contract (assembly test dùng chung), Features/Network/Tests/Sim (mạng giả trong bộ nhớ), adapter trong Features/<Tên>/Tests/EditModeNet, `.ai/QUEUE.md`, `.ai/MISSIONS_NET.md`, `.ai/FAILURES.md`, `.ai/GAUNTLET.md`, `.ai/tools/*.snippet.cs`.
- Sửa tài liệu SAU KHI chủ dự án duyệt: PROTOCOL.md (thêm mục 14), START_HERE.md, NETWORK.md (mục 4-5), TESTING.md, bảng + brief của T-036/T-037/T-038.
- Thêm lớp cơ sở (CHỈ THÊM) vào Features/Network/Online (host/client/autorun dùng chung): cần chủ dự án đồng ý riêng (mục 9, câu 2).
- KHÔNG đụng: Domain gameplay, scene map, prefab nhiệm vụ gốc, Build Settings, mã runtime của Attendance/PlankAndKey (chỉ THÊM adapter test), các quyết định đã chốt ở NETWORK.md mục 1.

## 3. Ngữ cảnh đã đọc (AI sau không cần đọc lại)
| File (đọc 2026-11-06) | Kết luận 1 dòng |
|---|---|
| NETWORK.md mục 4-5, 7, 8 | Mẫu nối mạng 8 bước làm tay cho TỪNG nhiệm vụ; M4 = build + 3 tiến trình LAN + .bat do NGƯỜI bấm; một Editor một lúc một phiên; execute_code không khởi chạy được tiến trình ngoài |
| PROTOCOL.md mục 4-6, 12-13 | Ngưỡng STALE 24 giờ + thủ tục TAKEOVER là cho AI chạy lâu; chưa tính tới limit 5 giờ và AI luân phiên |
| TESTING.md + LOG 2026-11-05 | EditMode toàn bộ chạy ~0,9 giây (575 test, đo 2026-11-05); chạy test KHÔNG phải điểm nghẽn |
| LOG phiên #11 (TeacherSays) | Giai đoạn 1 (lớp mạng thuần C# + test) xong trong 25 phút (08:10 -> 08:35 UTC 2026-11-05) |
| NETWORK.md mục 9 | Attendance: «đội 2 thắng qua mạng chưa kết luận» sau LAN 3 tiến trình; log lẫn do chạy chồng (mục 8 bẫy 4, 6) |
| TASKS.md | T-028 BLOCKED chờ 2 quyết định của chủ dự án; T-032 BLOCKED vì T-028; T-037 chờ xác nhận đồng hồ «Cô nói» |
| Đo 2026-11-06 (phiên #10) | BreakerPanel vs Attendance, cùng mẫu: chỉ 37% số dòng trùng nguyên văn sau khi chuẩn hóa tên (host Online 81%, client 53%, AutoRun 44%, HostRules 38%, Codec 37%, test 15-29%). Tức là ~1/3 là khung chép lại; phần còn lại là cùng Ý (cùng kịch bản kiểm) nhưng viết lại từng nhiệm vụ |

## 4. Chẩn đoán: vì sao chậm (có nguồn) và cách xử lý
| # | Nguyên nhân | Bằng chứng | Xử lý trong task này |
|---|---|---|---|
| 1 | Mỗi nhiệm vụ làm đủ chuỗi 8 bước tay, gồm scene thử + build + LAN do NGƯỜI bấm .bat; giai đoạn logic chỉ ~25 phút nhưng giai đoạn Online/M4 kéo dài | LOG #11; NETWORK.md mục 4-5 | Tách pha: logic trước cho TẤT CẢ, kiểm bằng mạng giả trong bộ nhớ (M1-M4); chỉ còn MỘT lần build+LAN cho tất cả (pha S) |
| 2 | Cùng một bộ test viết lại cho từng nhiệm vụ (15-29% trùng dòng nhưng cùng ý) | Đo mục 3 | Bộ test HỢP ĐỒNG dùng chung, nhiệm vụ chỉ viết adapter (M1, M3) |
| 3 | Lớp Online chép lại (host trùng 81%) | Đo mục 3 | Lớp cơ sở dùng chung (M5), CHỈ THÊM |
| 4 | Dừng chờ chủ dự án quyết định (T-028, T-032, T-037) | TASKS.md | Mặc định-và-đánh-dấu + gom câu hỏi một chỗ (mục 5.4) |
| 5 | AI hết limit giữa chừng, AI sau phải khảo sát lại + làm giấy TAKEOVER | PROTOCOL mục 4, 6 | Hàng đợi thẻ việc + khối RESUME + tiếp tục không giấy tờ (mục 5.2-5.3) |
| 6 | Lỗi đỏ không biết của ai, ai sửa | Chưa có cơ chế | Registry + FAILURES + thẻ FIX (mục 5.6) |
Điều SỬA so với đề xuất ban đầu của phiên #10: bỏ `dotnet test` ngoài Unity (AI không có terminal; EditMode đã nhanh). Điểm nghẽn là số bước tay, chờ người, và khảo sát lại khi đổi AI, không phải tốc độ chạy test.

## 5. Thiết kế quy trình v2
### 5.1 Các pha (một nhiệm vụ đi lần lượt; nhiều nhiệm vụ đi theo lô)
| Pha | Làm gì | Chỉ đụng | Xong khi |
|---|---|---|---|
| L Logic | Lớp mạng thuần C# (Command, World, State, HostRules, Codec) + ADAPTER vào bộ hợp đồng | File MỚI trong Runtime/Network + Tests; KHÔNG scene, KHÔNG Play Mode, KHÔNG build | Compile sạch + adapter đăng ký trong MISSIONS_NET.md |
| G Gauntlet | Chạy MỘT lượt mọi nhiệm vụ đã qua L (hợp đồng + mạng giả + bot tự giải) × 2/3/4 × seed | Chỉ chạy, không sửa | `.ai/GAUNTLET.md` có bảng; lỗi ghi vào FAILURES |
| F Fix | Người giữ nhiệm vụ sửa đúng lỗi của nhiệm vụ mình (thẻ FIX) | File của nhiệm vụ đó | Chạy lại đúng test đỏ xanh + Gauntlet lại nhiệm vụ đó |
| O Online | Lớp Online (dùng lớp cơ sở) + scene thử | Runtime/Online + scene thử mới | Compile + 1 phiên Play host một mình |
| S Smoke | MỘT bản build + MỘT .bat cho TẤT CẢ nhiệm vụ đã qua O; chủ dự án bấm MỘT lần | Builds/ | Log mỗi tiến trình; mọi nhiệm vụ thắng + thua |
| H Người thật | M5 của chủ dự án | - | Xác nhận |
Quy tắc: pha L của mọi nhiệm vụ làm hết trước (AI nối tiếp nhau nhận thẻ), G chạy sau mỗi lô; nhiệm vụ chỉ sang O khi G xanh cho nó. Mức xác minh: thêm M3a = hợp đồng + mạng giả xanh (thay phần lớn M4 từng nhiệm vụ); M4 chỉ còn là pha S.

### 5.2 Hàng đợi thẻ việc `.ai/QUEUE.md` (một nơi, một prompt cố định)
Mỗi thẻ: ID | task | việc (1 câu) | file tạo/sửa | XONG KHI (lệnh + kết quả phải có) | cần trước | trạng thái (READY, IN-PROGRESS, INTERRUPTED, DONE, BLOCKED) | cập nhật. Thẻ FIX luôn nằm TRÊN CÙNG. Mỗi thẻ nhỏ: một AI làm xong trong MỘT lượt limit.
Prompt chuẩn chủ dự án dán cho AI bất kỳ:
> Bạn vào dự án Unity The Return tại [đường dẫn]. Đọc AGENTS.md rồi .ai/QUEUE.md. Nhận thẻ đầu tiên có trạng thái READY hoặc INTERRUPTED (thẻ FIX trước), làm đúng thẻ đó theo .ai/PROTOCOL.md mục 14. Không khảo sát thêm. Mỗi bước để dự án biên dịch sạch và cập nhật khối RESUME; hết limit giữa chừng là chuyện bình thường.

### 5.3 Quy tắc chịu ngắt (sẽ thành PROTOCOL mục 14)
1. Bước nguyên tử: mọi file của một bước được tạo liền nhau, rồi MỘT lần refresh + compile, MỘT lần chạy test của bước.
2. Luôn để trạng thái xanh: không kết thúc bước, không bắt đầu bước mới khi Console còn lỗi biên dịch do mình gây ra. Tạo file theo thứ tự phụ thuộc: asmdef -> kiểu nền -> kiểu dùng nền -> test.
3. Ghi ý định TRƯỚC, kết quả SAU: dòng NEXT trong khối RESUME (1 dòng: việc + danh sách file định tạo) cập nhật trước khi bắt đầu bước, và cập nhật lại khi bước xong.
4. Vào lại sau khi bị ngắt: đọc NEXT; kiểm đủ file + compile + test của bước; thiếu/hỏng thì làm lại bước từ đầu (chỉ file MỚI nên ghi đè an toàn); đủ và xanh thì đánh dấu xong. Không cần thủ tục TAKEOVER: chỉ một dòng LOG `RESUME`.
5. Thẻ IN-PROGRESS không có LOG >= 6 giờ = INTERRUPTED (không phải bị bỏ rơi); AI nào cũng tiếp được ngay. Ngưỡng STALE 24 giờ cũ chỉ còn cho task không có thẻ.
6. Thao tác ghi file phải idempotent: tạo-mới hoặc ghi-đè-toàn-bộ (không append, không vá). Riêng LOG chỉ thêm: kiểm cuối file trước khi thêm để không trùng.
7. Cập nhật bảng task + LOG + QUEUE gộp trong MỘT lần gọi ở cuối bước để không lệch nhau.
8. Trả Editor về như lúc nhận (scene gốc, không Play, không dirty) trước khi kết thúc thẻ.
9. Sắp hết limit (hoặc nghi ngờ): viết RESUME ngay, ngắn, rồi mới làm tiếp.

### 5.4 Quyết định chờ: mặc định-và-đánh-dấu
Khi câu hỏi thiết kế chưa được chủ dự án trả lời, AI KHÔNG dừng/BLOCKED: chọn phương án an toàn nhất, ghi DECISIONS/ dạng PROVISIONAL (lý do + cách đổi), làm tiếp. Chủ dự án đổi ý sau thì người giữ nhiệm vụ sửa (thẻ FIX). Mọi câu hỏi đang chờ gom vào MỘT file `DECISIONS/PENDING.md` để chủ dự án trả lời một lượt (T-028 x2, đồng hồ «Cô nói» của T-037, ...).

### 5.5 Bộ test hợp đồng + mạng giả (kỹ thuật chuyên nghiệp, MỘT lượt)
- Hợp đồng chung cho mọi nhiệm vụ, chạy với đội 2/3/4 và K seed (mặc định 50): C1 mã hóa-giải mã ra đúng như cũ trên MỌI trạng thái gặp trong ván; C2 dữ liệu rác (fuzz seed cố định) không bao giờ ném lỗi; C3 độ dài chuỗi xấu nhất <= 2000 ký tự; C4 ô không tham gia/đã rớt bị từ chối và lệnh bị từ chối KHÔNG đổi Version; C5 bí mật không lọt (adapter khai danh sách chuỗi bí mật + ô được biết; hợp đồng kiểm không xuất hiện trong trạng thái chung và trong riêng của ô khác); C6 BOT TỰ GIẢI: adapter khai cách thắng từ đáp án Domain, hợp đồng chạy qua HostAuthority + ClientReplica tới thắng; C7 bot thua/hết giờ nếu nhiệm vụ có; C8 vào muộn/chụp lại trạng thái trùng bản phát; Version tăng đơn điệu; C9 cùng seed ra cùng ván.
- Mạng giả trong bộ nhớ (SimNet): host + N bản sao, tiêm lỗi có seed (trễ, trùng, đảo thứ tự, mất rồi gửi lại); kiểm các bản sao hội tụ về host và host không sập.
- Adapter (mỗi nhiệm vụ): dựng HostRules(n, seed, world giả), cách thắng, cách thua, danh sách bí mật. Đây là phần DUY NHẤT nhiệm vụ phải viết.
- Chạy qua Unity `run_tests` (luôn truyền assembly_names). Gauntlet gom tất cả assembly lấy từ MISSIONS_NET.md.

### 5.6 Trách nhiệm lỗi (ai sửa)
- `.ai/MISSIONS_NET.md` (registry): nhiệm vụ | task giữ | assembly test | VIẾT BỞI (phiên/ngày) | pha | Gauntlet gần nhất.
- Tên assembly/namespace test chứa tên nhiệm vụ nên test đỏ -> nhiệm vụ -> task -> người viết tự động.
- `.ai/FAILURES.md`: F-xxx | nhiệm vụ | test | thông điệp (<= 3 dòng) | lần đầu thấy | người viết | trạng thái OPEN/FIXING/FIXED | thẻ FIX.
- Người giữ thẻ FIX (AI nào đang cầm) sửa đúng file của nhiệm vụ đó, đọc mục bản đồ lỗi của plan nhiệm vụ, tối đa 3 vòng (PROTOCOL 13) rồi BLOCKED + báo.
- Lỗi chéo (khung Network/Framework/Contract/lớp cơ sở, hoặc nhiều nhiệm vụ đỏ cùng nguyên nhân) -> thẻ FIX thuộc T-039, KHÔNG sửa trong file nhiệm vụ.
- Nghi lỗi ở Domain có sẵn -> không sửa Domain (quy tắc brief T-036); ghi F-xxx loại DOMAIN cho chủ dự án quyết.
- Đóng lỗi CHỈ khi chạy lại đúng test đỏ xanh + Gauntlet lại nhiệm vụ đó xanh. Cuối GAUNTLET.md có thống kê lỗi theo nhiệm vụ và người viết.

## 6. Các subtask (mỗi cái kiểm được; không hứa thời gian)
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S0.1 | Chủ dự án duyệt mục 9 | - | Câu trả lời ghi vào DECISIONS | DONE | DECISIONS/2026-11-06-t039-owner-approval.md |
| S0.2 | Tạo QUEUE.md, MISSIONS_NET.md, FAILURES.md (+ GAUNTLET.md rỗng) và điền registry cho Attendance, BreakerPanel, PlankAndKey đã nối | .ai/*.md MỚI | Mở file, thẻ đầu tiên tự đủ để một AI làm được | DONE | 2026-11-06 06:26 QUEUE/MISSIONS_NET/FAILURES/GAUNTLET đã tạo |
| S0.3 | Thêm PROTOCOL mục 14 (mục 5.3) + sửa mục 4/6 (INTERRUPTED 6 giờ); START_HERE dùng prompt mục 5.2; tạo DECISIONS/PENDING.md | PROTOCOL.md, START_HERE.md | Đọc lại; thử ngắt: AI mới tiếp được | DONE | 2026-11-06 06:26 PROTOCOL 14, START_HERE, PENDING.md |
| M1 | Bộ hợp đồng C1-C9 + interface adapter, chạy thử với adapter giả | Features/Network/Tests/Contract (asmdef + file MỚI) | run_tests assembly TheReturn.Network.Contract.Tests xanh | DONE | 2026-11-06 06:38 UTC: 19/19 xanh (0,29 s); 7 lỗi cố ý bị đúng ca bắt; 6 assembly mạng gộp 314/314 (0,37 s) |
| M2 | SimNet + tiêm lỗi seed | Features/Network/Tests/Sim | Test hội tụ xanh; ca cố tình hỏng bị bắt | DONE | 2026-11-06 15:26 UTC: Contract.Tests 25/25 (0,42 s); ca N1 + SimNet; AliasedState bị N1 bắt |
| M3 | Adapter THỬ cho Attendance + BreakerPanel: đo số dòng adapter, thời gian chạy | Features/Attendance/Tests/EditModeNet, Features/BreakerPanel/Tests/EditModeNet | Xanh; ghi số liệu vào mục 8 | DONE | 2026-11-07 05:42 phiên #10: 2 adapter, 125 test xanh; LỖI THẬT bắt được: F-001 (Breaker C8/N1, đã sửa) |
| M4 | Gauntlet: chạy tất cả assembly trong registry, ghi GAUNTLET.md + FAILURES.md. Thử TestRunnerApi qua execute_code; nếu không chạy được thì phương án B: AI gọi run_tests với danh sách assembly + script định dạng bảng | .ai/tools/gauntlet.snippet.cs + .ai/*.md | Một lệnh -> bảng; cố tình làm hỏng 1 adapter -> FAILURES có dòng đúng nhiệm vụ | DONE | 2026-11-07 06:07 UTC: Gauntlet 700 test/20 assembly ~5 s một lệnh; phá cố ý Attendance.IsSolvedPublic -> F-002/F-003 đúng Attendance/T-031/#7 + 2 thẻ FIX; khôi phục -> XANH-LẠI |
| M5 | Lớp cơ sở Online dùng chung (host/client/autorun) trong Features/Network/Online, chỉ THÊM; chuyển BreakerPanel (của #10) sang làm thử | Features/Network/Online (MỚI), Features/BreakerPanel/Runtime/Online | Compile; test cũ xanh; Play host một mình lại đúng như 2026-11-05 | TODO (cần mục 9 câu 2) | |
| M6 | Scaffolder tạo khung nhiệm vụ mới (asmdef, adapter stub, test stub, Online dùng lớp cơ sở, scene thử bằng CopyAsset) + sửa NETWORK.md mục 4-5 + chuyển bảng/brief T-036/T-037/T-038 sang pha L-trước | .ai/tools/new_mission_net.snippet.cs; NETWORK.md; TASKS.md; briefs | Tạo thử nhiệm vụ giả -> compile xanh; đọc brief mới | TODO | |
| M7 | Pha S dùng chung: MỘT bản build + MỘT .bat cho mọi nhiệm vụ đã qua O (thiết kế chi tiết sau khi có M5) | Builds/ (ngoài Assets) | Log mọi tiến trình, thắng + thua | TODO (chưa thiết kế) | |
Thứ tự: S0.1 -> S0.2/S0.3 -> M1 -> M2 -> M3 -> M4 (từ đây có «test một lượt») -> M5 -> M6 -> M7.
Chạy song song với T-036/T-037/T-038: không chặn. Họ đã/đang làm pha L theo mẫu cũ (có Tests/EditModeNet); M3 sẽ THÊM adapter cho họ về sau, không phá việc đang làm.

## 7. Bản đồ lỗi (AI sửa lỗi của T-039 đọc phần này TRƯỚC)
| Triệu chứng | Nơi xem | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Hợp đồng đỏ cho MỌI nhiệm vụ cùng lúc | Features/Network/Tests/Contract (fixture dùng chung) hoặc Framework | Chạy hợp đồng với adapter giả trước | Lỗi khung, không phải nhiệm vụ |
| Hợp đồng đỏ cho MỘT nhiệm vụ | Adapter + lớp mạng của nhiệm vụ đó | Đọc tên test: nhiệm vụ + ca (C1..C9) + n + seed; tái hiện đúng seed đó | Thẻ FIX của nhiệm vụ |
| Bot không thắng | Adapter.Solve (đáp án sai thứ tự/ô) hoặc HostRules.Validate chặn nhầm | In lý do từ chối đầu tiên của bot | Domain có đúng không? So với test Domain |
| SimNet không hội tụ | Fixture SimNet / ClientReplica.Receive | Chạy 1 seed không tiêm lỗi, rồi thêm từng loại lỗi | Khung T-024 |
| Gauntlet không chạy được qua execute_code | Gauntlet (TestRunnerApi) | Phương án B (run_tests + script bảng) | Công cụ MCP |
| Registry lệch thực tế | MISSIONS_NET.md | Duyệt assembly *Network.Tests trong dự án so với bảng | Bước đăng ký trong M6 |

## 8. Số liệu đo (điền khi làm; số đã có ở mục 3)
- Baseline phiên #10: Attendance 1038 dòng, BreakerPanel 1181 dòng (10 file cặp đối chiếu); trùng 37%. Số dòng adapter sau M3: Attendance 104 dòng (78 dòng code), BreakerPanel 97 dòng (73 dòng code) (so với giả thuyết ≤ ~80 dòng: vượt, vì mỗi adapter gồm ván + bot + codec + fixture; phần Domain/bot là phần riêng thật); thời gian chạy (đo 2026-11-07 05:42): 2 assembly Breaker+Attendance 125 test = 4,4 giây; toàn bộ 22 assembly 700 test = 5,4 giây (Contract C1-C9+N1 x 2/3/4 x 50 seed cho 2 nhiệm vụ nằm trong đó)

## 9. Rủi ro và câu hỏi cho chủ dự án
1. Duyệt quy trình v2 (pha L -> G -> F -> O -> S -> H, hàng đợi QUEUE, prompt cố định, ngưỡng INTERRUPTED 6 giờ)?
2. Cho phép THÊM lớp cơ sở vào Features/Network/Online (không sửa Attendance/PlankAndKey) và chuyển BreakerPanel sang làm thử?
3. Chính sách mặc định-và-đánh-dấu cho câu hỏi thiết kế chưa trả lời (mục 5.4): đồng ý không?
4. Gom câu hỏi đang chờ vào DECISIONS/PENDING.md để trả lời một lượt (T-028 x2, đồng hồ «Cô nói» T-037, ...): đồng ý không?
Rủi ro: (a) bot chỉ chứng minh luật + mạng đúng, KHÔNG chứng minh cảm giác chơi hay giao diện (vẫn cần M5 của chủ dự án); (b) nhiệm vụ nhóm C (chuyển động/hướng nhìn) cần world giả có vị trí/hướng nên adapter dài hơn; (c) quy trình mới thành THÊM giấy tờ nếu QUEUE không được giữ gọn: S0.2 kiểm xem thẻ có đủ ngắn không.

## 10. Tiến độ (1-3 dòng mỗi lần; chi tiết ở LOG.md)
- 2026-11-06 05:26 UTC phiên #10: lập kế hoạch + ghi yêu cầu chủ dự án; chưa có code.
