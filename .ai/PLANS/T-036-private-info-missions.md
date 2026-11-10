# T-036: NHÓM A online: thông tin riêng + thao tác rời rạc (GradeRepair, NavigationExam, TeacherTruth)
Trạng thái: TODO, chưa ai nhận | Tạo: 2026-11-04 UTC (phiên #7, tách từ T-031) | Mức hiện tại: M1 (chỉ tài liệu) | Chủ dự án chưa duyệt cách nhóm
Đọc trước: NETWORK.md (toàn bộ), PLANS/T-027-plank-network-pattern.md, PLANS/T-022-design-2to4.md (hàng của từng nhiệm vụ), DECISIONS/2026-11-04-t031-split.md

## 1. Mục tiêu
Mỗi nhiệm vụ trong nhóm chơi trọn được qua mạng (host LAN hoặc Relay) với đội 2, 3, 4 người: mỗi người chỉ thấy thông tin RIÊNG của mình, host giữ luật và chấm, mọi người thấy cùng trạng thái chung. Giữ nguyên chế độ chơi 1 máy và giữ nguyên Domain.

## 2. Vì sao gộp nhóm này
Cả ba giống Attendance (mẫu mới nhất, REVIEW): có thông tin riêng chia theo ô người chơi, hành động rời rạc (chọn/bấm/đi một ô), host chỉ cần biết «ai làm gì, đang đứng gần đâu». Không cần theo dõi chuyển động liên tục.

## 3. Phạm vi
Trong: lớp mạng thuần C# + test, lớp Online Unity, scene thử riêng, build và kịch bản nhiều tiến trình, thẻ hệ thống, cho GradeRepair, NavigationExam, TeacherTruth.
Ngoài: đổi gameplay/độ khó (đã chốt ở T-022), nối cửa ra / luồng map (T-028), chat thoại, sửa Domain (chỉ khi test chứng minh lỗi thật: báo chủ dự án trước), sửa scene map, sửa Build Settings.

## 4. Quy trình cho MỖI nhiệm vụ (theo thứ tự)
1. Đọc: Features/<Tên>/README.md, SYSTEM_CARDS/<Tên>.md, Domain, Presentation (…Prototype), hàng của nhiệm vụ trong T-022. Đọc code mẫu Features/Attendance/Runtime/Network và Runtime/Online.
2. Domain CHƯA có test NUnit (cả ba): viết bộ test đặc tả cho Domain TRƯỚC (không sửa Domain) để chắc lớp bọc không làm hỏng luật. Bằng chứng cũ chỉ là file JSON trong Design/Verification (2026-09). Kiểm asmdef Domain: GradeRepair và NavigationExam KHÔNG đặt noEngineReferences, TeacherTruth CÓ. Nếu Domain dùng UnityEngine thì đừng sửa Domain: để lớp mạng không đặt noEngine và ghi lý do vào thẻ hệ thống.
3. Lớp mạng thuần C# (Runtime/Network, asmdef TheReturn.<Tên>.Network, namespace TheReturn.<Tên>Net): Command, I<Tên>World (điều host quan sát được, vd khoảng cách tới terminal/trạm), PublicState + PrivateState, HostRules : IHostRules, Codec. Mọi chuỗi tự do thoát bằng Uri.EscapeDataString; giải mã dữ liệu xấu trả false, không ném lỗi.
4. Test EditModeNet (asmdef TheReturn.<Tên>.Network.Tests): đội 2/3/4; từ chối đúng lý do và lệnh bị từ chối KHÔNG đổi Version; thông tin riêng không lọt sang người khác; thắng và thua; hết giờ/Tick nếu có; người mất kết nối; codec dữ liệu xấu + chuỗi <= 2000 ký tự; ván thắng đầy đủ qua HostAuthority + ClientReplica.
5. Lớp Online (Runtime/Online, asmdef TheReturn.<Tên>.Online): <Tên>OnlineHost, <Tên>OnlineClient, <Tên>OnlineAutoRun theo mẫu Attendance (tham số dòng lệnh -xxxauto ...).
6. Scene thử riêng Features/Network/Scenes/Network<Tên>Test.unity: sao chép NetworkAttendanceTest, thay phần nhiệm vụ. Dựng bằng mở ADDITIVE + SetActiveScene tạm (NETWORK.md mục 6). KHÔNG sửa scene map.
7. Kiểm thử mạng thật: manage_build chỉ scene thử, file .bat nhiều tiến trình (NETWORK.md mục 5), ca thắng + thua cho đội 2 và 3, một ca đội 4 nếu máy cho phép. Gỡ khỏi Build Settings các scene thử mà công cụ build đã tự thêm (chỉ của mình).
8. Ghi: dòng bảng ở mục 7 của file này, thẻ hệ thống (thêm mục Mạng), 1-3 gạch đầu dòng ở LOG với mức M3/M4 đúng. Người thật chơi mới tính M5.

## 5. Đặc thù từng nhiệm vụ (nguồn: PLANS/T-022-design-2to4.md; Domain asmdef/test: kiểm 2026-11-04)
GradeRepair (Phong_2):
- Luật: 4 tài liệu; tài liệu i do người i%N phụ trách (N=2 mỗi người 2; N=3 người 0 giữ tài liệu 0 và 3); chỉ người phụ trách đọc được tài liệu; 4 hồ sơ, 3 phiếu, tổng 32 điểm không đổi theo N [THEO THIẾT KẾ].
- Mạng: RIÊNG = nội dung các tài liệu mình phụ trách; CHUNG = trạng thái hồ sơ/phiếu/điểm. Lệnh theo đúng thao tác Presentation đang có (đọc GradeRepair...Prototype để biết). Host kiểm người gửi đang đứng đủ gần terminal.
- Domain: asmdef KHÔNG noEngine; 2 file .cs; chưa có test NUnit. [CẦN KIỂM] Presentation/scene chỉ dùng N phần tử đầu.
NavigationExam (Phong_3):
- Luật: 4 manh mối; manh mối i thuộc người i%N; thay phiên đi từng ô trên sa bàn 3x3, 2 con dấu, 3 ô cấm (không đổi theo N) [THEO THIẾT KẾ].
- Mạng: RIÊNG = manh mối của mình; CHUNG = sa bàn + lượt của ai + vị trí con dấu. Điểm riêng: LƯỢT. Host phải chặn người không có lượt (từ chối đúng lý do) và xử lý người giữ lượt mất kết nối (chuyển lượt hoặc dừng, ghi lại quyết định ở thẻ hệ thống).
- Domain: asmdef KHÔNG noEngine; 1 file .cs; chưa có test NUnit.
TeacherTruth (Phong_4):
- Luật: 4 kênh giáo viên cố định; cửa sổ xác nhận 3 giây; MỖI người phải bấm >= 1 kênh; mỗi người nghe và ghi phụ đề RIÊNG (heard[người, kênh]) [CÓ theo T-022]. Đội 2: mỗi người thường bấm 2 trạm trong 3 giây (trạm cách nhau 2,4-3,8 m); đội 3: ít nhất một người bấm 2.
- Mạng: RIÊNG = phụ đề của mình; CHUNG = kênh nào đã xác nhận + còn bao lâu trong cửa sổ. Host cầm đồng hồ 3 giây (Tick) và kiểm khoảng cách tới trạm lúc bấm.
- Domain: asmdef CÓ noEngine; 1 file .cs; chưa có test NUnit. [CẦN KIỂM] stations[4], teachers[4] trong scene dựng cho 4 người: với N<4 phải ẩn/bỏ qua phần thừa; đội 2 là ca khó nhất theo khoảng cách trạm.

## 6. Nghiệm thu nhóm
- Mỗi nhiệm vụ: test EditModeNet xanh (đội 2/3/4) + bộ test Domain mới xanh; 3 tiến trình LAN chạy ca thắng và ca thua có log (M4); compile 0 lỗi/0 cảnh báo; scene thử lưu; Build Settings sạch; thẻ hệ thống có mục Mạng; bảng nhận việc cập nhật.
- Chơi tay 2 máy bằng người thật = M5, do chủ dự án; ghi rõ chưa làm khi chưa có.

## 7. Bảng nhận việc (mỗi AI sửa đúng dòng của mình; ghi START ở LOG khi nhận)
| Nhiệm vụ | Trạng thái | AI giữ | Cập nhật | Ghi chú / bằng chứng |
|---|---|---|---|---|
| GradeRepair | PAUSED | Claude Sonnet 5.5 (phiên #10) | 2026-11-10 07:51 | PHA L XONG: Runtime/Network (Command, PublicState+PrivateState, HostRules, Codec) + Tests/EditMode (24 test Domain đặc tả, Domain chưa sửa) + Tests/EditModeNet (HostRules, Codec, ContractAdapter). Gauntlet 14:17 XANH. Riêng = sự kiện tài liệu ĐÃ ĐỌC dạng V1.HS-452; chung = điểm/phiếu/đã-đọc. Lỗi F-005 (C5) đã FIXED. CÒN: pha O (lớp Online + scene thử), phần Catalog dựng câu chữ từ sự kiện ở máy khách, S, H |
| NavigationExam | TODO | | | |
| TeacherTruth | TODO | | | |

## 8. Việc đề xuất làm trước / rủi ro
- Nên làm theo thứ tự GradeRepair -> NavigationExam -> TeacherTruth (từ ít tới nhiều thời gian thực). Khoảng 1 đơn vị Attendance mỗi nhiệm vụ (đơn vị = công làm đủ 8 bước như Attendance), cộng thêm bộ test Domain đặc tả.
- Rủi ro: Domain cũ chưa có test; nhiều object scene dựng cố định cho 4 người; Presentation cũ có thể giả định một máy (một ActivePlayer). Gặp vướng thật thì ghi vào thẻ hệ thống và hỏi chủ dự án, đừng tự đổi gameplay.
