# The Return Thiết kế lại tầng học và kế hoạch phát triển

Ngày 13 tháng 9 năm 2026

## Phạm vi và hướng thiết kế

Game giải đố góc nhìn thứ nhất cho 2–4 người trong một ngôi trường siêu nhiên, có không khí kỳ lạ và những tình huống gây cười do phối hợp. Giai đoạn đầu gồm sảnh chờ và tầng học. Trong tài liệu này dùng cách gọi của bạn: Map 1 là sảnh chờ, Map 2 là tầng học. Tài liệu Word gốc đánh số hai khu này là 0 và 1.

Bản này đề xuất thiết kế lại cả 7 nhiệm vụ của tầng học. Đây là thiết kế để triển khai và chơi thử; độ khó, thời lượng và mức gây cười phải được kiểm chứng với người chơi. Bản Unity hiện triển khai ba nhiệm vụ đầu trong cùng map trên một máy, hỗ trợ đội 2–4 vai. Các nhiệm vụ còn lại trong tài liệu này cần được rà soát theo quy mô đội mới trước khi triển khai. Chưa có kết nối mạng.

Giữ nét riêng của bản gốc: chia sẻ thông tin, tuân thủ nội quy kỳ quặc, sự bất thường tăng dần. Dùng các thao tác đơn giản như xem manh mối, ngồi/đứng, chọn thẻ, bấm nút và đi qua vùng kiểm tra. Mỗi quy luật siêu nhiên phải có dấu hiệu để người chơi suy ra được.

Không bắt cả 7 nhiệm vụ đều là phòng giải đố dài. Nhiệm vụ 2 là thử thách phối hợp ngắn; nhiệm vụ 7 là sự kiện chuyển tiếp có kiểm soát. Năm nhiệm vụ còn lại phát triển khả năng suy luận.

## Đánh giá bản gốc

| Nhiệm vụ | Điểm cần sửa | Hướng mới |
| --- | --- | --- |
| Điểm danh sai người | Chưa xác định người chơi mang tên nào, ghế đánh số ra sao, bộ manh mối có duy nhất một nghiệm không | Cấp sẵn thẻ tên; 4 ghế có số; 4 manh mối đều cần thiết |
| Giữ trật tự | Đi chậm đơn thuần dễ nhàm, một người có thể phá liên tục; chưa rõ hệ thống đo tiếng ồn bằng gì | Tắt loa theo cặp, trao vị trí giữ công tắc; đo sự kiện gameplay, không nghe microphone |
| Sửa bảng điểm | Chuyển và đổi điểm không làm tăng tổng điểm; yêu cầu trung bình 8 có thể bất khả thi | Phân biệt bổ sung điểm được xác nhận và chuyển điểm nhập nhầm; có ví dụ giải được |
| Bài kiểm tra | Gần giống bài điểm danh: mỗi người đọc số rồi đứng vào ô | Dẫn một quân cờ qua sơ đồ trường bằng bản đồ bị chia thông tin |
| Không được quay đầu | Người cuối không ai nhìn được lưng; hướng ban đầu có thể khác nhau; kéo dài vô hạn khó kiểm soát | Hai làn có cửa quan sát, hướng chuẩn cố định, đủ dữ kiện và checkpoint |
| Giáo viên nói thật/nói dối | Nhảy từ mâu thuẫn sang bấm 4 nút là đoán ý tác giả | Cung cấp nội quy chế độ thủ công, nhãn rõ trên nút và cảnh báo kênh loa sai |
| Chuông đổi tiết | Ngẫu nhiên ở mọi lúc có thể xung đột nhiệm vụ giữ yên lặng hoặc nhốt người đang ngồi | Chỉ kích hoạt tại các đoạn chuyển tiếp và chỉ chọn phòng an toàn đã xác minh đi tới được |

## 1 Điểm danh thích ứng 2 đến 4 người

Phần này đã được thay thế bằng [thiết kế v2](Diem-danh-v2.md): 24 ghế, roster 2–4 người, đề sinh theo seed, 10 loại điều kiện và text quản lý bằng asset. Xem tài liệu v2 cho quy tắc, lời hướng dẫn và kết quả kiểm chứng.

## 2 Giữ trật tự ở hành lang

Bản thử đã triển khai trong cùng scene lớp học; xem [Giữ trật tự](Giu-trat-tu.md) cho luật hiện tại, chỉnh sửa và cách chơi.

**Vai trò:** thay nhịp, dạy phối hợp khi di chuyển. Mục tiêu 1–2 phút.

Hành lang có một loa bị rè khiến cửa cách âm không mở. Nội quy nêu rõ: “Giữ nút tắt loa để mở cửa.” Bàn điều khiển ở hai đầu cùng điều khiển chế độ im lặng. Giữ một nút bất kỳ thì loa tắt và cửa mở; thả cả hai thì có 3 giây cảnh báo trước khi cửa đóng.

Một người giữ ở đầu gần, ba người qua. Một người bên kia tiếp quản nút, báo đồng đội, người cuối cùng đi qua. Hai người còn lại kiểm tra đường và phòng kế tiếp. Luân phiên người giữ khi chơi các khu sau, tránh khóa một người vào vai đứng chờ lâu.

Đo tiếng ồn bằng sự kiện game đã định nghĩa: chạy làm tăng thanh cảnh báo; đi bộ không tăng; tương tác làm rơi đồ có thể bổ sung sau. Bản đầu không thêm nhặt ném vật lý chỉ để phục vụ nhiệm vụ này. Không lấy âm thanh voice chat hoặc microphone làm dữ liệu.

Ngưỡng thử ban đầu: thanh 0–100, chạy +20 mỗi giây, đứng/đi bộ giảm 15 mỗi giây khi không có nguồn gây ồn. Loa bật +15 mỗi giây sau 3 giây cảnh báo. Khi đủ 100, cả đội trở lại checkpoint đầu hành lang và thanh về 0. Không xóa nhiệm vụ 1 đã hoàn thành.

Cửa phải có cảm biến không đóng xuyên người; nếu có người trong cửa, giữ mở rồi tiếp tục khi vùng cửa trống. Hiện thanh và lý do tăng để đội biết lỗi đến từ đâu. Sự kiện chuông đổi tiết không được chạy trong đoạn này.

**Xây dựng:** hai nút giữ, vùng hành lang, biến mức cảnh báo, vùng chống kẹp cửa, checkpoint. Host tính thời gian và độ ồn; client chỉ hiển thị. Khi người giữ mất kết nối phải hủy thao tác giữ và phát cảnh báo.

## 3 Sửa bảng điểm trong phòng giáo viên

Đã có bản thử nối sau hành lang trong cùng map, hỗ trợ đội 2–4; xem [Sửa bảng điểm](Sua-bang-diem.md) để chơi và chỉnh sửa.

**Vai trò:** câu logic có thao tác, yêu cầu đối chiếu chứng cứ. Mục tiêu 4–5 phút.

Thay yêu cầu sửa điểm tùy ý bằng “Khôi phục bảng điểm từ hồ sơ gốc”. Bảng chung có bốn hồ sơ A, B, C, D với điểm ban đầu 4, 7, 8, 9. Có hai giấy xác nhận bổ sung, mỗi giấy +2 điểm, dùng một lần; hồ sơ cho biết chúng thuộc A và D. Một biên bản ghi 1 điểm của B bị nhập nhầm vào D, nên cần chuyển 1 điểm từ D sang B.

Phân vai: một cặp đọc giấy bổ sung và đối chiếu mã học sinh; một cặp đọc biên bản và vận hành bảng sửa. Ai cũng có thể đổi vai. Mã và chứng cứ nằm trên hai bàn cách nhau để đội phải trao đổi.

Các thao tác có giới hạn:
- Áp giấy xác nhận +2 đúng mã hồ sơ, không được dùng lại.
- Chuyển 1 điểm theo biên bản D sang B, dùng một lần.
- Xác nhận toàn bộ bảng.
- Hoàn tác một bước hoặc đặt lại bài khi cần.

Điểm không được vượt 10 hay âm sau một thao tác; yêu cầu “không ai dưới 6” chỉ kiểm tra lúc nộp vì A ban đầu là 4.

Ví dụ lời giải: chuyển D sang B thành 4,8,8,8; thêm +2 cho A thành 6,8,8,8; thêm +2 cho D thành 6,8,8,10. Tổng 32, trung bình 8, không ai dưới 6 và không ai trên 10. Có thể thêm điểm A trước khi chuyển; chấp nhận mọi thứ tự hợp lệ, không ép một chuỗi duy nhất.

Nộp sai chỉ báo loại lỗi: thiếu chứng từ, sai hồ sơ, chưa đạt điều kiện. Chứng từ sai không bị tiêu hủy. Nút hoàn tác phải đảo đúng cả điểm lẫn cờ dùng chứng từ. Hai lệnh cùng lúc được host xử lý lần lượt và gửi bảng mới cho cả đội.

**Xây dựng:** bảng số nhỏ, thẻ chứng từ có ID, ô chọn hồ sơ, ba loại thao tác hữu hạn, lịch sử hoàn tác. Không nhận chữ viết tay hoặc câu trả lời tự do. Đây là logic rời rạc, không cần AI diễn giải.

## 4 Bài kiểm tra dẫn đường

**Vai trò:** đổi từ suy luận chỗ ngồi sang phối hợp theo không gian. Mục tiêu 3–4 phút.

Trên bàn giáo viên có sa bàn lưới 3×3, hàng đánh số 1–3 từ trên xuống, cột A–C từ trái sang. Quân học sinh bắt đầu A1, cần tới C3. Chỉ được đi lên, xuống, trái, phải từng ô. Mỗi người có một trang bản đồ chỉ tiết lộ một điều:

- Vai 1 thấy ô A2 bị khóa.
- Vai 2 thấy ô C1 bị khóa.
- Vai 3 thấy ô B3 bị khóa.
- Vai 4 thấy mũi tên chỉ được đi từ B2 sang C2, chiều ngược lại bị khóa.

Cả đội tổng hợp được tuyến A1 → B1 → B2 → C2 → C3. Hướng mũi tên là thông tin xác nhận cửa một chiều; không coi đây là manh mối bắt buộc để tìm đường vì tuyến đơn giản đã có thể được suy ra từ ba ô khóa.

Để vai 4 vẫn có đóng góp thực sự, chỉ trang của vai này có thứ tự hai con dấu cần lấy: “sổ lớp tại B2 trước chìa khóa tại C2”. Chưa lấy đủ theo thứ tự thì vào C3 không hoàn thành. Những vòng sau có thể tăng lưới nhưng không tăng trong bản đầu.

Bộ điều khiển chuyển người cầm sau mỗi bước hợp lệ theo vòng 1→2→3→4. Người chưa đến lượt vẫn nhìn vị trí quân chung và chỉ đường. Sau một lần bị chặn, lượt chưa đổi; màn hình hiện lý do và đội thử hướng khác. Không reset cả đường sau một lỗi.

**Xây dựng:** lưới trên UI hoặc một sa bàn phẳng, quân cờ di chuyển giữa các điểm cố định, danh sách ô khóa/cạnh một chiều, cờ con dấu, lượt thao tác. Không tạo mê cung 3D hoặc nhận dạng nét vẽ.

**Kiểm chứng trước dựng:** duyệt đường để chứng minh đích đi tới được; kiểm tra con dấu theo thứ tự và quyền người đang đến lượt. Nếu thử chơi thấy việc luân phiên quá gò bó, chuyển thành điều khiển tự do với một người thao tác tại một thời điểm; ghi lại quyết định sau playtest.

## 5 Hành lang không được quay đầu

**Vai trò:** đưa hiện tượng siêu nhiên thành quy luật rõ ràng. Mục tiêu 3–4 phút.

Dựng hai làn song song chạy cùng hướng, ngăn bằng vách kính có cửa quan sát. Mỗi làn có hai người. Vạch sàn chỉ hướng tiến chung; không lấy hướng camera ngẫu nhiên lúc bước vào làm chuẩn.

Mỗi người có một ký hiệu trên lưng và một số thứ tự 1–4 cố định trên thẻ phía trước. Hai làn có các hốc đứng lệch nhau ở điểm quan sát. Người ở làn trái đứng lùi để nhìn lưng người bên phải; sau đó hai bên đổi vị trí tiến/lùi nhưng vẫn nhìn theo trục hành lang. Phải cho phép lùi bằng phím S; luật cấm quay nhìn phía sau, không cấm bước lùi.

Điểm quan sát phải đủ rộng và chênh lệch chiều dọc để cả bốn ký hiệu đều có người khác đọc được. Đây là điều phải kiểm chứng bằng bản dựng trước khi hoàn thiện mỹ thuật.

Khóa cuối yêu cầu nhập ký hiệu theo số thẻ 1–4, không theo thứ tự đến cửa. Ví dụ bộ mẫu: 1 tam giác, 2 tròn, 3 vuông, 4 chữ X. Màu chỉ trang trí, luôn có hình.

Cảnh báo khi camera quay lệch quá 80 độ so với hướng chuẩn. Quay quá 110 độ liên tục 0,6 giây mới phạt, để tránh lỗi do rê chuột nhẹ. Khi phạt, màn hình tối ngắn và cả đội quay lại điểm quan sát gần nhất. Giữ các thông tin người chơi đã đọc; không xóa ký ức nhân tạo.

Hiệu ứng “hành lang dài ra” chỉ dùng một đoạn module lặp có giới hạn hoặc đổi trang trí sau lần phạt. Không cần không gian phi Euclid, portal thật hay sinh vô hạn.

**Xây dựng:** hai làn, camera angle check theo hướng cố định, dấu hiệu cảnh báo, ký hiệu lưng, bảng nhập 4 ô, checkpoint. Host quyết định vi phạm từ dữ liệu hướng đã nhận, có dung sai độ trễ. Không bật chuông đổi tiết ở đây.

## 6 Giáo viên và chế độ thủ công

**Vai trò:** câu cuối tầng, dạy người chơi nghi ngờ đúng chỗ dựa vào chứng cứ. Mục tiêu 3–4 phút.

Bốn tượng giáo viên phát bốn phiên bản lời chỉ dẫn mâu thuẫn. Giữ sự bất thường của bản gốc, nhưng thêm đường suy luận có thể chứng minh:

1. Ở phòng trước đã có tờ “Khi các kênh thông báo mâu thuẫn, chuyển sang xác nhận thủ công”.
2. Trong phòng cuối có sơ đồ bốn kênh loa và đèn “MẤT ĐỒNG BỘ”.
3. Bốn nút được ghi rõ “XÁC NHẬN THỦ CÔNG 1–4”, đặt cạnh bốn tượng.
4. Nội quy bên cửa ghi “Cần bốn xác nhận trong cùng khoảng kiểm tra”.

Cả đội trao đổi để nhận ra kênh loa mâu thuẫn, tìm thông báo chế độ thủ công, phân người đến bốn nút và cùng giữ.

Dùng cửa sổ phối hợp 3 giây: nút đầu mở đợt xác nhận; đủ bốn người khác nhau giữ nút trong cửa sổ đó thì mở cửa. Màn hình hiển thị 1/4, 2/4… và đếm ngược. Không yêu cầu cùng một frame. Thất bại chỉ reset bốn nút, không phạt cả tầng.

Cho phép đội nhận ra chế độ thủ công ngay và giải sớm. Không bắt nghe hết đoạn thoại chỉ để kéo dài thời lượng. Luôn có phụ đề riêng theo người nhận; không bắt dùng âm thanh mới giải được.

**Xây dựng:** bốn nguồn thoại/phụ đề có ID, trạng thái kênh, bốn nút giữ, cửa sổ thời gian theo host và cổng thoát tầng. Không cần NPC AI đối thoại, suy luận ngôn ngữ hoặc lip sync.

## 7 Chuông trường đổi tiết

**Vai trò:** sự kiện tạo nhịp gấp giữa các câu đố, không phải phòng thứ bảy.

Bản đầu đặt đúng hai điểm kích hoạt đã biết: sau bảng điểm và sau bài kiểm tra dẫn đường. Chỉ kích hoạt khi cả đội đã rời phòng cũ, không ai đang ngồi hoặc trong hội thoại, và không có thử thách hành lang đặc biệt đang chạy.

Biển đèn xanh đánh dấu phòng an toàn; chuông và phụ đề cùng báo tên phòng. Chọn trong các phòng có đường đi thực tế từ vị trí cả đội. Khởi đầu 20 giây, điều chỉnh theo playtest và chiều dài đường; không mặc định 15 giây cho mọi tình huống.

Tất cả vào vùng an toàn thì sự kiện kết thúc sớm. Hết giờ thiếu người: đưa cả đội về checkpoint chuyển tiếp, giữ các câu đố đã giải, mở lại đường cần thiết. Bản đầu dùng ánh đèn và tiếng bước chân giám thị; robot đuổi thật là phần mở rộng sau khi nhịp chạy đã vui.

Không kích hoạt chuông trong nhiệm vụ 2, nhiệm vụ 5 hoặc khi đang mở cửa cuối. Nếu không có phòng hợp lệ, bỏ sự kiện thay vì tạo tình huống không thể thắng. Bản đầu không random vị trí; chỉ random sau khi kiểm tra tự động tính đi tới được.

**Xây dựng:** trigger chuyển tiếp, một bộ điều phối sự kiện, vùng an toàn, timer, checkpoint. Host giữ thời gian; client trình bày âm thanh và phụ đề.

## Bố cục và nhịp chơi

Sảnh chờ → Lớp 101 điểm danh → Hành lang yên lặng → Phòng giáo viên → Chuông lần 1 → Phòng kiểm tra dẫn đường → Chuông lần 2 → Hành lang hai làn → Lớp cuối → Cầu thang khóa sang khu tiếp theo.

Mỗi phòng có cửa vào, vùng chờ không cản đồng đội, biển định hướng và checkpoint hợp lý. Sau khi giải, cửa liên quan giữ trạng thái mở. Câu đố thất bại không xóa tiến trình trước đó.

Mục tiêu tầng học khoảng 22–30 phút sau khi đã biết điều khiển; đây là mục tiêu thiết kế, không phải thời lượng đã đo. Giảm bằng chứng từ và vòng lặp nếu người mới vượt quá 30 phút. Sảnh chờ chỉ cần đủ chức năng, không ép người chơi ở đó 5–10 phút.

## Kế hoạch triển khai

| Mốc | Kết quả có thể chơi | Điều kiện qua mốc |
| --- | --- | --- |
| A | Câu điểm danh thử một máy, 4 vai | Đọc manh mối, di chuyển, ngồi/đứng, sai/thử lại, đúng/mở cửa; bộ đề có một nghiệm |
| B | Nền co-op 4 người và sảnh chờ | Tạo/vào phòng, đủ 4 vai, ready, chuyển scene cùng nhau, manh mối riêng |
| C | Điểm danh chạy qua mạng | Hai người tranh ghế không trùng; disconnect nhả ghế; hoàn thành nhất quán trên cả 4 máy |
| D | Dựng thô toàn tầng | Đường đi, các cổng và checkpoint chơi được trước khi trang trí |
| E | Bảng điểm và bài dẫn đường | Dữ kiện đủ, không thể mất vật phẩm hoặc mất lượt vĩnh viễn |
| F | Hai hành lang, lớp cuối, chuông | Không xung đột luật; cửa không kẹp; sự kiện không tạo đường bất khả thi |
| G | Mỹ thuật và âm thanh | Tài nguyên cùng phong cách, tương tác nổi bật, phụ đề rõ, hiệu năng có số đo |
| H | Build để nhóm ngoài chơi thử | 4 người mới hoàn thành mà không cần tác giả giải thích ngoài game |

Không ước lượng lịch cứng khi chưa biết thời gian bạn dành cho dự án, nhân lực, ngân sách tài nguyên và cách phát hành. Sau mốc C mới có cơ sở ước lượng phần còn lại từ tốc độ thực tế.

### Nền mạng cần quyết định ở mốc B

Dự án hiện có Multiplayer Center nhưng chưa có thư viện gameplay networking. Cần xác định LAN hay online qua mã phòng, có dùng Steam không, và cách xử lý khi host thoát. Không cài nhiều thư viện mạng cùng lúc.

Thiết kế mặc định để triển khai: một host giữ trạng thái câu đố; client gửi ý định tương tác. Danh tính lấy từ kết nối, không nhận danh tính tự khai trong gói lệnh. Host kiểm tra khoảng cách, quyền tương tác, điều kiện nhiệm vụ và số phiên bản. Chỉ gửi manh mối riêng cho đúng người nhận; điều này phục vụ trải nghiệm, không phải cam kết chống gian lận.

Bản đầu nếu host thoát thì kết thúc phiên có thông báo; host migration là tính năng riêng. Cần chốt chính sách người chơi thiếu: dừng chờ hoặc quay về lobby, tránh giả vờ hỗ trợ 1–4 người khi câu đố yêu cầu đủ 4.

### Cấu trúc dùng lại

Tách dữ liệu đề bài, trạng thái nhiệm vụ, lệnh tương tác và phần trình bày. Các cửa chỉ nhận sự kiện hoàn thành, không tự đoán trạng thái từ vật thể đang hiển thị. Checkpoint lưu các ID câu đố đã xong. Sự kiện chuông phải hỏi bộ điều phối trước khi chạy.

Tạo bộ prefab trường học: đoạn tường, cửa, cửa sổ, bàn ghế, bảng, bảng thông báo, đèn, tủ và công tắc. Chỉ mua/nhập tài nguyên sau khi kích thước lối đi và phòng giải đố đã chơi được.

## Những gì bản thử hiện có và còn thiếu

Bản thử điểm danh tập trung vào khả năng hiểu manh mối và thao tác ngồi/đứng. Cho phép đổi vai trên một máy để tác giả tự diễn tập. Cảnh được tạo từ hình khối và vật liệu trong dự án; không dùng tài nguyên tải ngoài.

Chưa có: co-op qua mạng, lobby hoàn chỉnh, voice chat, lưu tiến trình, build phát hành và các nhiệm vụ 4–7. Không đánh đồng bài kiểm tra logic tự động với playtest bốn người thật.

## Cách đánh giá với người chơi

Ghi thời gian đọc đề, lần thử sai, ai đang đóng góp, đoạn phải nhờ giải thích và lỗi thao tác. Hỏi người chơi vì sao họ nghĩ lời giải đúng. Nếu đáp án đến từ thử hết thay vì hiểu quy luật, cải thiện manh mối trước khi tăng hình phạt.

Với câu điểm danh, thành công là người chơi hiểu tên của mình, hướng đánh số ghế và biết cách sửa chỗ sai. Với bản online, phải quan sát cả bốn người trao đổi; một người điều khiển cả bốn vai chỉ xác minh được cơ chế.
