# T-038: NHÓM C online: chuyển động và hướng nhìn của từng nhân vật (GlassBridge, QuietCorridor, DontLookBack)
Trạng thái: TODO, chưa ai nhận | Tạo: 2026-11-04 UTC (phiên #7, tách từ T-031) | Mức hiện tại: M1 (chỉ tài liệu) | Chủ dự án chưa duyệt cách nhóm | Độ khó: cao nhất trong 3 nhóm
Đọc trước: NETWORK.md (toàn bộ), PLANS/T-027-plank-network-pattern.md, PLANS/T-022-design-2to4.md, SYSTEM_CARDS/OnlineAvatars.md, DECISIONS/2026-11-04-t031-split.md

## 1. Mục tiêu
Mỗi nhiệm vụ chơi trọn qua mạng với đội 2/3/4 người, trong đó host đọc được trạng thái chi tiết của từng nhân vật (ô sàn đang đứng, đang chạy hay đi bộ, hướng nhìn) để chấm luật. Giữ nguyên chế độ 1 máy và Domain.

## 2. Vì sao gộp nhóm này
Cả ba cần thông tin mà hạ tầng avatar online (T-025) có thể CHƯA cung cấp cho host: GlassBridge cần biết người đang bước vào ô nào; QuietCorridor cần biết ai đang CHẠY (tiếng ồn tăng theo người chạy) và ai đang giữ nút; DontLookBack cần biết ai nhìn thấy lưng ai (vị trí + hướng nhìn). Làm chung một lần phần hạ tầng này (việc C0) rồi mới làm từng nhiệm vụ thì rẻ hơn nhiều so với mỗi nhiệm vụ tự đo một kiểu.

## 3. Phạm vi
Trong: C0 (hạ tầng trạng thái avatar cho host) + lớp mạng, test, lớp Online, scene thử, build/kịch bản, thẻ hệ thống cho GlassBridge, QuietCorridor, DontLookBack.
Ngoài: đổi gameplay/độ khó (đã chốt T-022); nối cửa ra / luồng map (T-028); chat thoại; sửa scene map; sửa Build Settings.

## 4. Việc C0: trạng thái avatar cho host đọc (làm TRƯỚC)
- Đọc Features/Network/Online (OnlineAvatar, OnlinePartySlots, OnlinePartyLocalDriver) và SYSTEM_CARDS/OnlineAvatars.md để biết host hiện đọc được gì (vị trí chắc chắn có; tốc độ/đang chạy/hướng nhìn: CHƯA kiểm).
- Nếu thiếu: thêm CÁCH NHỎ NHẤT để owner (người điều khiển) gửi lên host cờ 'đang chạy' và góc nhìn (yaw), hoặc host suy ra từ vị trí theo thời gian (tốc độ > ngưỡng đi bộ = chạy). Ưu tiên suy ra từ vị trí nếu đủ chính xác vì không đụng prefab avatar và không tốn băng thông. Hướng nhìn thì không suy ra được từ vị trí: cần gửi yaw.
- Phối hợp: OnlineAvatar/PF_OnlineAvatar là của T-025 (phiên #9). Báo qua LOG trước khi sửa file có sẵn; ưu tiên thêm file mới. Prefab nhân vật còn là của T-010 (phiên #6).
- Có thể làm C0 như một task con riêng (một AI) rồi cả nhóm dùng; ghi ở bảng mục 7.
- Test: EditMode cho phần suy luận thuần (tốc độ -> chạy/đi bộ, yaw -> thấy lưng); Online bằng 2 tiến trình.

## 5. Quy trình cho MỖI nhiệm vụ
Giống T-036 mục 4 (8 bước). Khác biệt:
- GlassBridge Domain ĐÃ CÓ test (GlassBridge.Domain.Tests, noEngine CÓ). QuietCorridor (Domain KHÔNG noEngine, chưa có test) và DontLookBack (KHÔNG noEngine, chưa có test) phải viết test Domain đặc tả trước, không sửa Domain.
- Mọi quyết định dựa vào vị trí/chạy/hướng nhìn do HOST tính từ trạng thái avatar host thấy (C0), không tin client khai.

## 6. Đặc thù từng nhiệm vụ (nguồn: PLANS/T-022-design-2to4.md; Domain/test: kiểm 2026-11-04)
GlassBridge (Hanh_Lang_2):
- Luật: 6 hàng x 2 ô, 2 đài quan sát. Một người dẫn ở đài, một người đi; xong đổi chiều (người đi tới đài cuối thì dẫn lại). Cả N người phải qua; ô sai chỉ đưa người đó về đài đầu, hàng đã đi đúng giữ nguyên. Dãy ô an toàn chỉ hiện ở bảng đài quan sát, không thấy khi đang trên cầu. Số hàng không đổi theo N.
- Mạng: host suy ra ô người đang bước vào từ vị trí. QUYẾT ĐỊNH thiết kế cần làm: dãy ô an toàn chỉ gửi cho những người mà host thấy đang đứng ở đài (nhất quán với host-authoritative) hay gửi chung và để client tự ẩn. Ghi chọn lựa vào thẻ hệ thống; nếu thấy là quyết định gameplay thì hỏi chủ dự án.
- Domain: noEngine CÓ; 3 file .cs; có test. [CẦN KIỂM] lượt dẫn/đi khi có người mất kết nối giữa cầu.
QuietCorridor (Hanh_Lang_1):
- Luật: tiếng ồn tăng 20/giây cho mỗi người đang CHẠY (tối đa +40/+60/+80 mỗi giây với N=2/3/4), thua ở 100, tự hồi 15/giây khi im lặng, loa +15/giây; bàn giao cần HAI người khác nhau cùng giữ 2 nút (đủ cho N=2). T-022 quyết định GIỮ nguyên, không co giãn tốc độ ồn theo N.
- Mạng: cần biết ai đang chạy (C0) và ai đang GIỮ nút: giữ nút là lệnh có trạng thái (bắt đầu giữ / thả), host phải xử lý người giữ nút rồi mất kết nối (thả nút). Đồng hồ ồn chạy trên host (Tick), client chỉ hiển thị thanh ồn.
- LƯU Ý T-033 (REVIEW): đã sửa cơ chế giữ nút B/E và mở rộng vùng đích Team_Goal của PF_QuietCorridor; đọc LOG/PLANS của T-033 trước khi làm để khỏi hỏng bản sửa.
- Domain: asmdef KHÔNG noEngine; 1 file .cs; chưa có test NUnit.
DontLookBack (Hanh_Lang_3):
- Luật: mỗi người một ký hiệu lưng (N ký hiệu trong 6, không trùng); mỗi người nhìn được lưng người khác nhưng không thấy lưng mình; ghi chép riêng; mã nhập N ô; nộp mã khi cả đội ở lối ra VÀ mọi người đều đã được ai đó thấy lưng.
- Mạng: RIÊNG = ký hiệu lưng của người khác mà mình đã thấy + sổ ghi chép; CHUNG = ai đã được thấy lưng, mã đã nhập. Host tính «A thấy lưng B» từ vị trí + hướng nhìn của A và hướng của B (C0): đây là phần khó nhất của cả T-038; cần định nghĩa chính xác góc/khoảng cách đang dùng ở bản 1 máy (đọc DontLookBack...Prototype) và tái dùng đúng công thức.
- Domain: asmdef KHÔNG noEngine; 1 file .cs; chưa có test NUnit. [CẦN KIỂM] BackCard_1..4 và StartPoint_1..4 trong scene dựng cố định cho 4 người: N<4 ẩn phần thừa.

## 7. Bảng nhận việc
| Việc | Trạng thái | AI giữ | Cập nhật | Ghi chú / bằng chứng |
|---|---|---|---|---|
| C0 hạ tầng trạng thái avatar | TODO | | | Làm trước |
| GlassBridge | TODO | | | |
| QuietCorridor | TODO | | | Cần C0 |
| DontLookBack | TODO | | | Cần C0; khó nhất |

## 8. Nghiệm thu nhóm
- C0: test thuần xanh + 2 tiến trình đo được cờ chạy/góc nhìn đúng trên host (log).
- Mỗi nhiệm vụ: test EditModeNet + Domain xanh; 3 tiến trình LAN ca thắng + ca thua có log (M4); compile 0 lỗi/0 cảnh báo; scene thử lưu; Build Settings sạch; thẻ hệ thống có mục Mạng. Người thật chơi là M5.

## 9. Đề xuất / rủi ro
- Thứ tự gợi ý: C0 -> GlassBridge -> QuietCorridor -> DontLookBack. Ước lượng 1,5-2 đơn vị Attendance mỗi nhiệm vụ, C0 ~0,5-1.
- Rủi ro lớn: sai số vị trí/hướng nhìn giữa host và client gây chấm sai khi mạng trễ (nhất là DontLookBack và QuietCorridor). Ghi giới hạn đo được; M5 qua Relay với bạn bè bắt buộc trước khi coi là xong thật.
