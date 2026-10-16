# ĐỀ XUẤT T-006: thiết kế nhiệm vụ cho 2 phòng + 4 hành lang còn trống   (file: .ai/PLANS/T-006-mission-designs.md)

Trạng thái: ĐÃ DUYỆT 2026-10-16 (chủ dự án, chat; xem DECISIONS/2026-10-16-t006-mission-designs-approved.md) | Tác giả: Claude Sonnet 5.5 (phiên #3) | Viết: 2026-10-16 00:06 UTC
Phạm vi: CHỈ thiết kế (tài liệu). Chưa có code, chưa đụng scene. Chủ dự án yêu cầu đề xuất rồi duyệt (chat 2026-10-16).
Lưu ý quy ước: MISSIONS_TODO.md cấm AI tự bịa thiết kế; đây là ĐỀ XUẤT để duyệt, không phải thiết kế đã chốt.

## 0. Tóm tắt (mỗi khu 1 đề xuất chính, có phương án dự phòng ở mục 4)
| Khu | Tên đề xuất | Cơ chế lõi (khác 6 nhiệm vụ cũ ở đâu) | Thông tin bị chia | Thời lượng | Chi phí |
|---|---|---|---|---|---|
| Phong_5 | Bảng cầu dao (BreakerPanel) | Đảo công tắc để đèn khớp đích; mỗi công tắc ảnh hưởng nhiều đèn (logic tuyến tính) | Mỗi người chỉ biết công tắc của mình đảo đèn nào | 3-4 phút | M |
| Phong_6 | Hợp xướng (Choir) | Bấm bục theo thứ tự trong cửa sổ thời gian (chuỗi + nhịp) | Mỗi người giữ 1 thẻ nhạc dịch ký hiệu -> số bục | 2-3 phút | S-M |
| Hanh_Lang_2 | Cầu kính (GlassBridge) | Người dẫn đường - người đi: ô an toàn chỉ hiện ở đài quan sát | Dãy ô an toàn chỉ thấy từ đài | 1-2 phút | M |
| Hanh_Lang_4 | Đèn lồng chung (SharedLantern) | Giữ cả đội trong một vùng sáng; biển chỉ đọc được trong ánh sáng | Sổ ghi riêng từng người | 1,5-2 phút | M |
| Hanh_Lang_5 | Cô nói (TeacherSays) | Nội quy ngược: lệnh thật làm đúng, lệnh giả làm NGƯỢC; chỉ 1 người nghe được «Cô nói» | Ai nghe được tiền tố đổi mỗi đoạn | 1-1,5 phút | S-M |
| Hanh_Lang_6 | Cầu ván và thẻ từ (PlankAndKey) | Sức chứa 2 + thẻ phải mang qua, tốc độ mỗi người khác nhau (bài toán thời gian tối ưu) | Tốc độ cặp sách của từng người | 1-1,5 phút | S-M |
Chi phí: S = nhỏ (như QuietCorridor), M = vừa (như GradeRepair). Cả 6 theo khuôn Features/<Tên>: Domain thuần + Data (catalog tiếng Việt) + Presentation + NUnit test + README.

## 1. Nguyên tắc (rút từ Design/Thiet-ke-tang-hoc.md và quyết định đã chốt)
- Co-op 2-4 người; MỖI nhiệm vụ cần cả đội; thao tác đơn giản: xem manh mối, bấm, giữ, đi qua vùng, ngồi/đứng; không dùng microphone; luôn có hình/chữ thay cho âm thanh.
- Mỗi quy luật có dấu hiệu để suy ra (nội quy dán ở cửa); không đoán ý tác giả.
- Thất bại chỉ reset phạm vi nhiệm vụ đó, không xóa tiến trình các khu khác (khớp A2 đã chốt).
- Đề sinh theo seed; có Begin / StopSession(reset) / Retry / NewCase như 6 nhiệm vụ cũ để gắn IMissionAdapter (T-003) không sửa framework.
- Host quyết định: mọi luật nằm trong Domain thuần C#, Presentation chỉ hiển thị (để sau đồng bộ mạng).
- TỰ CHỨA: vì người chơi vào khu theo thứ tự tùy ý (DECISIONS 2026-10-14), KHÔNG nhiệm vụ nào được phụ thuộc manh mối nằm ở khu khác.
- Cơ chế đã dùng (tránh trùng): Attendance = suy luận chỗ ngồi; QuietCorridor = giữ nút + ồn do chạy; GradeRepair = sổ điểm + chứng từ chia vai; NavigationExam = tìm đường theo lượt + bản đồ chia thông tin; DontLookBack = hướng nhìn + thẻ lưng + khóa; TeacherTruth = mâu thuẫn + xác nhận đồng thời.

## 2. Hai phòng

### Phong_5 - Bảng cầu dao (BreakerPanel), phòng kỹ thuật điện của trường
- Cảnh: một bảng đèn báo trên tường (S đèn) và S công tắc cầu dao chia cho các người chơi. S = số người + 2 (2 người: 4; 3 người: 5; 4 người: 6). Mỗi người đứng trước 1-2 công tắc riêng. Nội quy dán cửa: «Đèn nào cũng phải đúng chỗ, trường mới mở cửa.»
- Luật: mỗi công tắc ĐẢO trạng thái của 2-3 đèn cố định. Chỉ CHỦ công tắc được bấm nó; khi chủ ngắm vào công tắc, các đèn nó đảo nhấp nháy (chỉ chủ thấy). Mọi người thấy chung trạng thái đèn. Một tấm bảng nội quy cho thấy trạng thái ĐÍCH (hình + vị trí, không chỉ màu).
- Quá tải: bật cùng lúc nhiều hơn ceil(S/2) công tắc thì cầu dao nhảy: mọi công tắc về tắt, đèn về trạng thái đầu. Không phạt gì thêm.
- Thắng: đèn khớp đích và giữ 3 giây.
- Chia thông tin: muốn biết công tắc của người khác đảo đèn nào phải hỏi họ; không ai tự thử hết được vì không bấm hộ nhau.
- Co giãn: số công tắc/đèn theo số người; đề sinh bằng seed với ma trận bit khả nghịch nên LUÔN có đúng 1 cách bật.
- Vật tương tác (prefab): PF_BreakerSwitch, PF_BreakerLamp (+ bảng đích là dữ liệu hiển thị).
- Rủi ro: một người nắm lý thuyết có thể giải nhanh; chấp nhận được vì vẫn phải thu thập thông tin từ mọi người.

### Phong_6 - Hợp xướng (Choir), phòng âm nhạc
- Cảnh: 4 bục đàn đặt hình cung cách nhau 3-4 m, mỗi bục có SỐ 1-4 và 1 ký hiệu hình dạng riêng; trên tường có bản nhạc là DÃY 6 ký hiệu. Nội quy: «Hát đúng nhịp, đúng thứ tự.»
- Luật: mỗi người giữ 1 «thẻ nhạc» (4 người: 1 thẻ/người; 3 người: 2+1+1; 2 người: 2+2) cho biết ký hiệu nào ứng với bục số mấy cho thẻ của mình. Cả đội phải ghép các thẻ để dịch bản nhạc thành dãy SỐ BỤC rồi bấm bục theo thứ tự; mỗi lần bấm cách lần trước không quá `StepWindowSeconds` giây (nốt đầu không giới hạn). Thông số này CHỈNH ĐƯỢC (chủ dự án yêu cầu 2026-10-16: đang đổi nhân vật bằng phím 1-4 nên 2 giây quá ngắn): 0 = không giới hạn; mỗi lần đổi vai khi đang dở dãy được cộng `SwitchExtraSeconds`. Mặc định giai đoạn phát triển 5 giây + 1,5 giây; hạ gần 2 giây khi chơi online.
- Sai bục hoặc quá giờ: dãy quay về nốt đầu; thẻ nhạc và đề giữ nguyên (không phạt thêm).
- Thắng: bấm đủ 6 nốt đúng.
- Co giãn: số thẻ chia theo số người; trình sinh phải đảm bảo hai nốt liền nhau không buộc một người chạy quá xa trong 2 giây (kiểm bằng khoảng cách bục thật trong scene).
- Vật tương tác: PF_ChoirPad (bục); bản nhạc và thẻ là dữ liệu HUD.
- Nhịp thể hiện bằng đèn + phụ đề; âm thanh chỉ là phụ.

## 3. Bốn hành lang (ngắn, thay nhịp, dạy phối hợp khi di chuyển)

### Hanh_Lang_2 - Cầu kính (GlassBridge)
- Cảnh: sàn kính 6 hàng x 2 ô; mỗi hàng chỉ 1 ô chịu được. Có HAI đài quan sát (đầu và cuối cầu); đứng ở đài thì bảng sơ đồ của đài hiện dãy ô an toàn của cầu. Người đi trên cầu không thấy.
- Luật: người dẫn ở đài báo ô đúng; người đi bước. Bước ô sai: ô vỡ, CHỈ người đó về đầu cầu; các hàng đã đi đúng giữ nguyên. Người dẫn đầu tiên cuối cùng cũng phải qua: lúc đó đồng đội ở đài cuối dẫn lại.
- Thắng: cả đội qua cầu.
- Co giãn: 2 người: một đi, một dẫn, rồi đổi chiều. Đề sinh bằng seed.
- Vật tương tác: PF_GlassTilePair, PF_BridgeObservationPanel.
- Cần có (scene): chưa có cổng/khu chuẩn bị cho khu này.

### Hanh_Lang_4 - Đèn lồng chung (SharedLantern)
- Cảnh: hành lang mất điện; chỉ có MỘT đèn lồng, ánh sáng bán kính R quanh người cầm. Ngoài vùng sáng tầm nhìn bị che tối.
- Luật: đèn lồng trao tay được (nhấn E khi đứng gần người khác trong 2 m). Dọc hành lang có 4 biển đánh số 1-4, mỗi biển có 1 ký hiệu; CHỈ đọc được khi biển nằm trong vùng sáng. Mỗi người có sổ riêng ghi những biển CHÍNH MÌNH đã đọc (không tự chia sẻ cho người khác). Ở cuối có bảng nhập: 4 ký hiệu theo thứ tự số 1-4.
- Ai ở ngoài vùng sáng quá 5 giây thì về điểm đầu.
- Thắng: nhập đúng bảng cuối và cả đội đến cổng.
- Co giãn: số biển cố định 4; ít người thì mỗi người đọc nhiều biển hơn.
- Vật tương tác: PF_LanternHolder, PF_LanternSign.
- Cần có (scene): chưa có cổng/khu chuẩn bị.

### Hanh_Lang_5 - Cô nói (TeacherSays)
- Cảnh: hành lang chia 5 đoạn; ở mỗi đoạn loa ra một lệnh bằng chữ: BƯỚC TỚI hoặc ĐỨNG YÊN. Nội quy ở cửa: «Lệnh có Cô nói thì làm đúng. Lệnh không có Cô nói là lệnh giả - làm NGƯỢC lại.»
- Chia thông tin: tiền tố «Cô nói» chỉ hiện trên HUD của MỘT người, người đó đổi sau mỗi đoạn; người đó phải báo cho đội lệnh thật hay giả.
- Luật: lệnh thật BƯỚC TỚI -> cả đội đi vào đoạn kế trong 8 giây; lệnh thật ĐỨNG YÊN -> đứng yên 5 giây. Lệnh giả thì làm ngược lại. Ai vi phạm: cảnh cáo +1 cho đội; đủ 3 cảnh cáo thì về đầu hành lang.
- Thắng: qua đủ 5 đoạn.
- Kiểm bằng sự kiện game: chuyển động/đứng yên (giống cách QuietCorridor đo chạy), không dùng microphone.
- Vật tương tác: loa/biển hiển thị (không cần prefab logic riêng).
- Cần có (scene): chưa có cổng/khu chuẩn bị.

### Hanh_Lang_6 - Cầu ván và thẻ từ (PlankAndKey)
- Cảnh: hành lang bị đứt bởi một khe; có một tấm ván hẹp chịu tối đa 2 người và MỘT thẻ từ; cổng ở bờ bên kia chỉ mở khi cả đội đã qua và thẻ ở bờ đó. Ván chỉ hạ xuống khi thẻ nằm trên ván.
- Luật: mỗi người mang cặp sách nặng khác nhau nên mất thời gian qua khác nhau; thời gian qua của cả cặp = người chậm hơn. Có đồng hồ tổng.
- Thắng: cả đội qua kịp hạn. Hết giờ: về đầu khe.
- Số liệu đã KIỂM BẰNG TÍNH TOÁN (xem mục 5): tốc độ [1,2,5] min 8; [1,2,5,10] min 17; [2,3,4,6] min 17; [1,2,3,4] min 11. Hạn đề xuất = min + 20-25%.
- Hạn chế: với 2 người nhiệm vụ gần như không có thử thách (cả hai qua cùng lúc). Cần quyết định: giữ ngắn hay thay bằng phương án khác cho đội 2 người.
- Vật tương tác: PF_PlankLift, PF_KeyCard.
- Cần có (scene): chưa có cổng/khu chuẩn bị.

## 4. Ý tưởng dự phòng (nếu bạn không thích đề xuất chính)
- Thay Phong_5: Phòng chiếu - chồng phim: mỗi người giữ 1 tấm phim, xoay tấm của mình (0/90/180/270 độ) để hình chiếu chung khớp mẫu; 4^4 = 256 trạng thái.
- Thay Phong_6: Phòng y tế - đơn thuốc: triệu chứng -> thuốc theo nội quy kỳ quặc, bảng tra bị chia cho từng người, có liều lượng tính đúng.
- Thay Hanh_Lang_2/4/5/6: Hành lang tủ đồ (mã 3 số, mỗi người đọc 1 mảnh, 1 mảnh giả); Hành lang xe đẩy (cần 2 người đẩy hai tay cầm, người khác bật công tắc dọn đường); Hành lang camera giám sát (một người xem sơ đồ quét chu kỳ để báo cho đội đi/đứng).

## 5. Kiểm chứng đã làm (2026-10-16, bằng tính toán trong Unity Editor, chưa dựng gì)
- Bảng cầu dao: tìm ma trận công tắc-đèn khả nghịch trên GF(2) cho S = 4, 5, 6; mỗi đề có ĐÚNG 1 nghiệm, nghiệm dùng 2/4, 3/5, 3/6 công tắc, không vượt giới hạn quá tải ceil(S/2). Trình sinh chỉ cần lặp chọn ma trận khả nghịch.
- Cầu ván và thẻ từ: Dijkstra trên trạng thái (người ở bờ nào, thẻ ở bờ nào, sức chứa 2) cho các bộ tốc độ ở mục 3.
- CHƯA kiểm: độ khó/độ vui khi chơi thật; mọi thứ dưới đây là thiết kế, cần playtest.

## 6. Tổng thời lượng và các điểm cần chủ dự án quyết định
- Ước 6 nhiệm vụ mới ~ 10-14 phút. 6 nhiệm vụ cũ theo Design: hành lang 1-2 phút, bảng điểm 4-5, bài kiểm tra 3-4, quay đầu 3-4, giáo viên 3-4, điểm danh (không ghi). Tổng cả map có thể ~ 28-39 phút, VƯỢT mục tiêu 22-30 phút trong Design/Thiet-ke-tang-hoc.md.
- Câu hỏi:
  1. Duyệt/loại/thay từng khu (6 dòng ở mục 0).
  2. Chấp nhận tổng thời lượng dài hơn mục tiêu, hay muốn rút (ví dụ bớt một hành lang)?
  3. Hanh_Lang_6 cho đội 2 người: giữ nhẹ hay đổi phương án?
  4. Cho phép AI thêm cổng + khu chuẩn bị cho Hanh_Lang_2/4/5/6 vào scene (cần Scene lock + snapshot), hay chủ dự án tự dựng?
  5. Tên chủ đề phòng (phòng kỹ thuật điện, phòng âm nhạc) có hợp với bố cục/đồ họa bạn đang làm không?

## 7. Nếu được duyệt: cách triển khai
- Mỗi nhiệm vụ = 1 task con (T-006a..f), thứ tự đề xuất theo chi phí/giá trị: Phong_5 (BreakerPanel), Hanh_Lang_6, Hanh_Lang_5, Phong_6, Hanh_Lang_2, Hanh_Lang_4.
- Mỗi task con: (1) kế hoạch riêng theo PROTOCOL mục 12; (2) Domain thuần + NUnit test (trước, mức 3); (3) Data catalog tiếng Việt + Presentation + prefab + README; (4) đăng ký vào MissionRegistryFactory (T-003) bằng adapter mới và khu đúng tên object; (5) kiểm Play Mode (mức 4).
- Scene: Phong_5/6 đã có 3 cổng, luôn mở; Hanh_Lang_2/4/5/6 chưa có cổng/khu chuẩn bị. Không tự dời bố cục; chủ dự án đang tự bố trí nội thất.
- UI: HUD riêng từng khu thuộc T-011.

## 8. Phát hiện phụ (cần kiểm, CHƯA xác minh trong code)
- Design/Thiet-ke-tang-hoc.md mục 6 nói nhiệm vụ Giáo viên thật/dối dựa vào tờ nội quy ở «phòng trước». Với luồng tự chọn, người chơi có thể vào Phong_4 trước. Cần kiểm xem tờ này có nằm trong chính Phong_4 hay không; nếu không thì phải chuyển vào Phong_4 (thuộc T-005/T-011, không phải T-006).

## 9. Tiến độ
- 2026-10-16 00:06 UTC: bản đề xuất viết xong, chờ chủ dự án duyệt. Chưa có code.
- 2026-10-16 00:16 UTC: chủ dự án DUYỆT. Phong_6: thời gian giữa hai nốt chỉnh được (đã sửa mục 2). Tách T-006a (BreakerPanel) .. T-006f (SharedLantern) trong TASKS; phiên #3 làm Domain của T-006a và T-006d.
