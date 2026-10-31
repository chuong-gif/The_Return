# T-022: nhiệm vụ co giãn theo đội 2 / 3 / 4 người (12 nhiệm vụ)   (file: .ai/PLANS/T-022-design-2to4.md)

Trạng thái: ĐỀ XUẤT, CHỜ CHỦ DỰ ÁN DUYỆT | Tác giả: Claude Sonnet 5.5 (phiên #6) | Viết: 2026-10-31 UTC | Phạm vi: CHỈ tài liệu, không sửa code/scene.
Nền: DECISIONS/2026-10-29-t009-owner-answers.md (đội 2-4 người CỐ ĐỊNH từ lúc lập đội; KHÔNG đổi vai; mỗi người đúng 1 nhân vật; số nhân vật = số người; mọi nhiệm vụ chơi được với 2, 3, 4 người).
Quy ước nhãn trong bảng: [CÓ] = đã có trong code/README/thẻ hệ thống (đọc 2026-10-31, nguồn ghi ở mục 3); [THEO THIẾT KẾ] = có trong thiết kế đã duyệt (PLANS/T-006) nhưng phiên này chưa đối chiếu với code; [ĐỀ XUẤT] = AI đề xuất, CHƯA duyệt; [CẦN KIỂM] = chưa xác minh được, phải kiểm khi nối mạng. Không có ô nào AI tự quyết gameplay: chỗ cần quyết nằm ở mục 2 (câu hỏi cho chủ dự án).

## 0. Kết luận ngắn
- Cả 12 Domain đều đã nhận số người 2-4 (ném lỗi ngoài khoảng) và không nhiệm vụ nào đòi một vai cố định ngoài «người số 0» (SharedLantern: đèn lồng ban đầu ở người 0; người số 0 luôn tồn tại). Phím 1-4 đổi vai chỉ còn ở chế độ 1 máy.
- 11/12 nhiệm vụ co giãn đã đủ dùng ở mức luật; việc còn lại là CÂN ĐỘ KHÓ theo N (mục 2) và kiểm Presentation/scene khi chỉ có N nhân vật.
- Một nhiệm vụ cần quyết định thiết kế thật: PlankAndKey cho N=2 (đội 2 người gần như không có thử thách, đã ghi từ T-006).
- Rủi ro chung khi nối mạng (T-031): nhiều object scene dựng cố định cho 4 người (checkpoints[4], StartPoint_1..4, BackCard_1..4, stations[4], teachers[4], 6 phần tử Breaker): với N<4 phải ẩn/bỏ qua phần thừa. [CẦN KIỂM] từng nhiệm vụ.

## 1. Bảng 12 nhiệm vụ x 3 cỡ đội
| Nhiệm vụ (khu) | N = 2 | N = 3 | N = 4 | Thông tin chia từng người |
|---|---|---|---|---|
| Attendance (Phong_1) | 24 ghế cố định; 2 người ngồi; đề sinh theo N, luôn đúng 1 nghiệm [CÓ] | như N=2, 3 người ngồi [CÓ] | 4 người ngồi [CÓ] | Manh mối chia riêng từng người (ClueOwners) [CÓ] |
| GradeRepair (Phong_2) | 4 tài liệu: mỗi người phụ trách 2 (tài liệu i -> người i%2) [CÓ] | người 0 giữ tài liệu 0 và 3, người 1 giữ 1, người 2 giữ 2 [CÓ] | mỗi người 1 tài liệu [CÓ] | Chỉ người phụ trách đọc được tài liệu [CÓ]. 4 hồ sơ, 3 phiếu, tổng 32 điểm không đổi theo N [THEO THIẾT KẾ] |
| NavigationExam (Phong_3) | 4 manh mối: mỗi người 2 (manh mối i -> người i%2); thay phiên đi từng ô [CÓ] | người 0 giữ 2 manh mối, còn lại 1 [CÓ] | mỗi người 1 manh mối [CÓ] | Manh mối chia theo vòng i%N [CÓ]. Sa bàn 3x3, 2 con dấu, 3 ô cấm không đổi theo N [THEO THIẾT KẾ] |
| DontLookBack (Hanh_Lang_3) | 2 ký hiệu lưng (trong 6, không trùng); mỗi người nhìn lưng người kia; mã nhập 2 ô [CÓ] | 3 ký hiệu, mã 3 ô [CÓ] | 4 ký hiệu, mã 4 ô [CÓ] | Mỗi người không thấy lưng mình; ghi chép riêng [CÓ]. Điều kiện nộp: cả đội ở lối ra + mọi người đều đã được ai đó thấy lưng [CÓ] |
| TeacherTruth (Phong_4) | 4 kênh giáo viên cố định; cửa sổ xác nhận 3 giây; MỖI người phải bấm >= 1 kênh; thực tế mỗi người thường bấm 2 trạm trong 3 giây [CÓ, suy từ luật]. Khó nhất theo khoảng cách trạm [CẦN KIỂM] | 4 kênh chia cho 3 người nên ít nhất một người bấm 2 [suy từ luật] | mỗi người 1 trạm [CÓ] | Mỗi người nghe và ghi phụ đề RIÊNG [CÓ] |
| BreakerPanel (Phong_5) | S = 4 đèn/công tắc; quá tải nếu bật > ceil(4/2) = 2 công tắc cùng lúc; nghiệm dùng 2 công tắc [THEO THIẾT KẾ] | S = 5; giới hạn 3; nghiệm dùng 3 công tắc [THEO THIẾT KẾ] | S = 6; giới hạn 3; nghiệm dùng 3 công tắc [THEO THIẾT KẾ] | Mỗi người chỉ bấm công tắc của mình và chỉ chủ thấy đèn nó đảo; đề luôn có >= 2 chủ trong nghiệm [CÓ] |
| Choir (Phong_6) | 4 bục, dãy 6 nốt; 2 thẻ nhạc cho mỗi người (2+2) [THEO THIẾT KẾ] | 2+1+1 thẻ [THEO THIẾT KẾ] | mỗi người 1 thẻ [THEO THIẾT KẾ] | Thẻ nhạc dịch ký hiệu -> số bục, chia theo người [THEO THIẾT KẾ]. Cửa sổ giữa hai nốt: 5 giây (phát triển), dự kiến ~2 giây khi online [CÓ, cần chốt]. |
| QuietCorridor (Hanh_Lang_1) | 2 nút giữ; bàn giao cần 2 người khác nhau giữ 2 nút cùng lúc: đủ cho N=2 [CÓ]. Ồn tăng 20/giây mỗi người đang chạy (tối đa +40/giây) [CÓ] | tối đa +60/giây [CÓ] | tối đa +80/giây; thua ở 100, ồn tự hồi 15/giây khi im lặng, loa +15/giây [CÓ] | Không chia thông tin; người giữ nút/người đi là việc chia công |
| GlassBridge (Hanh_Lang_2) | 6 hàng x 2 ô, 2 đài quan sát. Một người dẫn ở đài, một người đi; xong đổi chiều (người đi tới đài cuối dẫn lại) [CÓ theo thiết kế] | cả 3 phải qua; ô sai chỉ đưa người đó về đài đầu, hàng đã đi đúng giữ nguyên [CÓ] | cả 4 phải qua [CÓ] | Dãy ô an toàn chỉ hiện ở bảng đài quan sát, không thấy khi đang trên cầu [CÓ]. Số hàng không đổi theo N [THEO THIẾT KẾ] |
| SharedLantern (Hanh_Lang_4) | 4 biển (6 ký hiệu); 1 đèn lồng, bán kính sáng R; ai ở ngoài vùng sáng > 5 giây thì về điểm đầu; mỗi người tự đọc ~2 biển [THEO THIẾT KẾ] | mỗi người đọc 1-2 biển [THEO THIẾT KẾ] | mỗi người ~1 biển; 4 người khó cùng nằm trong R hơn [CẦN KIỂM] | Sổ riêng từng người, không tự chia sẻ [CÓ]. Đèn lồng trao tay (E trong 2 m) [CÓ] |
| TeacherSays (Hanh_Lang_5) | 5 đoạn; mỗi đoạn một người (ngẫu nhiên trong N) nghe được «Cô nói»; lệnh thật/giả; 3 cảnh cáo thì về đầu [THEO THIẾT KẾ] | như N=2, người nghe chọn ngẫu nhiên trong 3 [CÓ] | như N=2, trong 4 [CÓ] | Chỉ 1 người thấy tiền tố «Cô nói» mỗi đoạn [CÓ]. Hạn 8 giây (bước tới) / 5 giây (đứng yên) cho cả đội, không đổi theo N [THEO THIẾT KẾ] |
| PlankAndKey (Hanh_Lang_6) | Ván chở tối đa 2 người + 1 thẻ từ: cả hai qua cùng lúc nên GẦN NHƯ KHÔNG có thử thách [CÓ, đã biết] -> CẦN CHỦ DỰ ÁN QUYẾT (câu hỏi 1) | bộ tốc độ {1,2,5}: tối thiểu 8 đơn vị [CÓ] | bộ {1,2,5,10}: 17; {2,3,4,6}: 17; {1,2,3,4}: 11 [CÓ]; hạn = min + 20-25% [CÓ theo thiết kế] | Tốc độ cặp sách của từng người [CÓ] |

## 2. Câu hỏi cho chủ dự án (mỗi câu có mặc định AI sẽ dùng nếu chủ dự án không đổi ý)
1. PlankAndKey với đội 2 người. Hiện: ván chở 2 người nên cả hai qua cùng lúc, chỉ còn đồng hồ. Phương án A (mặc định): giữ nguyên, coi đây là hành lang nhẹ cho đội nhỏ. Phương án B: với N=2 chỉ có 1 chỗ trên ván cho người nhưng thẻ phải được đưa qua bằng cách ném/đặt lên ván (cần thiết kế lại, tốn công). Phương án C: với N=2 thêm hạn thời gian chặt hơn (nhưng không thêm luật). AI nghiêng về A để không làm lệch phạm vi.
2. QuietCorridor: ồn tăng 20/giây MỖI người chạy, nên đội 4 người dễ thua hơn đội 2 người. Mặc định: giữ nguyên (không co giãn tốc độ ồn). Nếu muốn cân bằng: tốc độ ồn mỗi người = 40/N hay ngưỡng thua = 50*N?
3. Choir: khoảng giữa hai nốt (StepWindowSeconds). Mặc định online: 2 giây cho cả 2/3/4 người (bục cách nhau 3-4 m, chạy ~1 giây). Hỏi: có muốn đội 2 người (mỗi người giữ 2 thẻ, phải chạy nhiều hơn) có cửa sổ dài hơn (ví dụ 3 giây)?
4. SharedLantern: bán kính sáng R và hạn 5 giây ngoài vùng sáng giữ nguyên cho mọi N hay tăng R theo N (đội 4 người dàn trải rộng hơn)? Mặc định: giữ nguyên, kiểm bằng chơi 4 người; nếu đội 4 hay bị đưa về thì tăng R.
5. TeacherTruth: với N=2 mỗi người bấm 2 trạm trong 3 giây. Mặc định: giữ 3 giây; chỉ đổi nếu đo thấy hai trạm xa nhau quá khả năng chạy (cần chơi thật hoặc đo khoảng cách trạm trong scene).
6. TeacherSays: người nghe «Cô nói» chọn NGẪU NHIÊN mỗi đoạn, nên với N=2 có thể cùng một người nghe cả 5 đoạn. Mặc định: giữ ngẫu nhiên; hoặc buộc xoay vòng/không lặp liền nhau để mọi người đều có việc.
7. Hạn 8 giây (bước tới) và 5 giây (đứng yên) của TeacherSays: cả đội 4 người cùng bước dễ vi phạm hơn đội 2. Mặc định: không đổi theo N.
8. GlassBridge: đội 3-4 người đi qua tuần tự sẽ lâu hơn đội 2 người. Mặc định: không đổi số hàng theo N (6 hàng).

## 3. Nguồn (đọc 2026-10-31)
- Thiết kế: PLANS/T-006-mission-designs.md (6 nhiệm vụ mới, mục 2-3), DECISIONS/2026-10-29-t009-owner-answers.md.
- Mã/README/thẻ: BreakerGenerator (S = N + 2, OwnerCount >= 2), Choir README + ChoirGenerator, GlassBridgeGenerator/Round, LanternGenerator/Round, TeacherSaysGenerator (listeners[i] = rng.Next(N)), PlankGenerator (danh mục bộ tốc độ theo N), AttendanceGenerator (chia manh mối 2-4), QuietCorridorState (ồn, bàn giao), GradeRepairRound.OwnerOf (document % N), NavigationExamState.OwnerOf (clue % N), DontLookBackState (ký hiệu từng người), TeacherTruthState (heard[actor, kênh], cửa sổ xác nhận).
- Bằng chứng kiểm cũ (thẻ hệ thống): Attendance 2/3/4 người giải được (AttendanceV2Integration 2026-09-14); GradeRepair 90 seed, đội 2/3/4 (2026-09-14); NavigationExam 12 cấu hình (2026-09-26); DontLookBack 300 ca seed/đội (2026-09-26); TeacherTruth 3 bộ thoại (2026-09-26); Breaker/Choir/GlassBridge/Lantern/Plank: test Domain EditMode xanh, chưa chơi thật.
- Chưa kiểm trong phiên này: không chạy lại test, không đo khoảng cách trong scene. Các ô [CẦN KIỂM] là việc của từng dòng T-031.

## 4. Cách dùng cho các task sau
- T-027 (PlankAndKey qua mạng): chờ câu hỏi 1; nếu không có ý kiến thì dùng phương án A (giữ luật hiện tại, N=2 không đổi).
- T-031: mỗi AI nhận một nhiệm vụ phải (1) kiểm Presentation/scene chỉ dùng N phần tử đầu, (2) chạy Domain test 2/3/4 người, (3) ghi vào bảng nhận việc kết quả các ô [CẦN KIỂM] của nhiệm vụ đó.
- Khi chủ dự án trả lời mục 2: ghi DECISIONS/2026-10-xx-t022-team-size.md và sửa các ô tương ứng ở mục 1.

## 5. Tiến độ
- 2026-10-31: bản đề xuất viết xong từ đọc code/tài liệu (không chạy gì); chờ chủ dự án duyệt/trả lời mục 2.
