# Kế hoạch bố trí lại theo blockout — phiên bản 2

Trạng thái: đề xuất ngày 2026-09-29, chưa triển khai. Căn cứ: phản hồi mới của người dùng và ảnh phòng cũ nằm trong phòng lớn. Thay thế phương án ghép nguyên phòng ở tài liệu Bo-cuc-theo-blockout.

## 1. Thiết kế đích

Blockout của người dùng là kiến trúc chính. Không đặt thêm hộp phòng có sàn, tường, trần riêng bên trong. Tháo lớp vỏ cũ khỏi bản chơi, giữ component nhiệm vụ, trạng thái, tương tác và nội thất có thể sử dụng lại. Bố trí từng vật trong không gian mới thay vì di chuyển cả cụm cũ.

Phân khu bằng hướng bàn ghế, tủ hồ sơ, ánh sáng, biển khu và thay đổi vật liệu. Chỉ thêm vách thấp/kệ khi giúp câu đố hoặc dẫn đường; không xây thêm một phòng kín để tránh phải thiết kế lại. Không phóng to đồng loạt nhân vật, bàn ghế và khoảng tương tác để lấp phòng rộng.

## 2. Phân bổ 12 khu

Đề xuất giữ thứ tự sáu nhiệm vụ đang có, dựa trên kết nối hành lang đã ghi nhận. Đối chiếu lại các cửa thật trước khi triển khai nếu người dùng đã sửa blockout.

| Khu | Công năng đề xuất | Trạng thái nhiệm vụ |
|---|---|---|
| Phong_1 | Điểm danh | Chuyển 24 bàn ghế ra toàn không gian phòng |
| Hanh_Lang_2, nối phòng 1–2 | Giữ trật tự | Đặt trạm giữ nút, loa, cửa vào chính hành lang |
| Phong_2 | Sửa bảng điểm | Phòng học vụ chia vùng tìm và đối chiếu hồ sơ |
| Hanh_Lang_3 | Chuyển tiếp | Chưa có nhiệm vụ |
| Phong_3 | Bài kiểm tra | Phòng khảo thí với vùng hướng dẫn, giải mã, thao tác |
| Hanh_Lang_4, nối phòng 3–4 | Không được quay đầu | Dùng hành lang thật, bố trí làn và checkpoint theo hướng đi |
| Phong_4 | Giáo viên nói thật/nói dối | Các trạm nghe và khu hội ý/xác nhận |
| Hanh_Lang_5 | Chuyển tiếp | Chưa có nhiệm vụ |
| Phong_5 | Dự trữ | Chưa có nhiệm vụ |
| Hanh_Lang_6 | Chuyển tiếp | Chưa có nhiệm vụ |
| Phong_6 | Dự trữ | Chưa có nhiệm vụ |
| Hanh_Lang_1, nối phòng 6–1 | Khép vòng | Chưa có nhiệm vụ |

Khu chuẩn bị phục vụ đọc hướng dẫn, tập hợp, checkpoint và nhận diện nhiệm vụ phía trước; không chiếm bằng phòng nhiệm vụ nhỏ. Hai phòng trống và bốn hành lang trống không có puzzle giả, không tính là nhiệm vụ hoàn tất. Mốc kết thúc sáu nhiệm vụ tạm thời đặt sau giáo viên; vẫn cho phép khảo sát khu dự trữ theo chế độ test.

Sảnh và cầu thang tạo nhiều đường tiếp cận. Phải kiểm tra đường vòng để không vô tình bỏ qua cửa nhiệm vụ hoặc kích hoạt nhiệm vụ sau từ phía sau. Điều kiện bắt đầu dựa trên nhiệm vụ trước đã hoàn thành + cả đội ở đúng khu chuẩn bị; cửa/chốt cụ thể phục vụ luật từng nhiệm vụ. Không khóa mọi lối một cách máy móc chỉ vì có đường vòng.

## 3. Sửa bảng điểm: biến phòng lớn thành không gian điều tra

Giữ lõi “tìm bằng chứng, suy luận và sửa dữ liệu”, không chỉ rải bốn bàn ra bốn góc. Tổ chức một vòng tìm kiếm có đường quay lại ngắn:

1. **Tiếp nhận gần cửa:** cho biết danh sách hồ sơ bị sai, mục tiêu và cách đọc mã tủ. Người chơi hiểu mình cần tìm gì trước khi tản ra.
2. **Kho hồ sơ học sinh:** tủ theo lớp/niên khóa, cung cấp danh tính và mã hồ sơ. Mã dẫn tới khu khác, không bắt mở toàn bộ ngăn ngẫu nhiên.
3. **Khu bài thi và phúc khảo:** bằng chứng điểm gốc, điểm sửa, thời điểm thay đổi. Nhóm phải phân biệt chứng cứ đúng niên khóa/lần thi.
4. **Khu sổ trực và biên bản:** bằng chứng độc lập về người ký, ngày hoặc điều kiện hợp lệ, dùng đối chiếu tài liệu nghi vấn.
5. **Bàn hội ý và bảng điểm:** nơi tổng hợp bằng chứng, chọn sửa và nộp kết quả. Đặt ở vị trí dễ nhận diện từ các khu, không chặn cửa.

Ví dụ chuỗi chơi đề xuất: mã hồ sơ ở kho → tìm đúng biên bản phúc khảo → đối chiếu dấu thời gian với sổ trực → xác định ô điểm cần sửa. Đây là ý tưởng phân bố bằng chứng; cần đối chiếu catalog hiện có để giữ hoặc điều chỉnh luật một cách có chủ đích, không tự thêm đáp án chỉ từ nội thất.

Vị trí bằng chứng biến thể trong các vị trí được đánh dấu sẵn; mã khu và logic định hướng giữ ổn định. Có dấu hiệu thị giác để phân biệt vật nhiệm vụ với đồ trang trí. Không tạo hàng chục tủ có nút mở nhưng không mang thông tin.

Đội 2 người có thể luân chuyển làm đủ các bước; không bắt buộc bốn trạm đồng thời. Đội 3–4 có thể chia nhau tìm và báo kết quả. Không tăng độ khó bằng việc tăng quãng chạy. Ghi nhận bằng chứng đã xem để tránh phải đi lại chỉ vì quên một dòng chữ. Text/manh mối/seed được quản lý ở catalog dữ liệu, không rải literal trong scene builder.

## 4. Các nhiệm vụ còn lại

- **Điểm danh:** 24 bàn ghế chia các cụm có hàng/cột và lối đi rõ; bảng hướng dẫn ở đầu lớp, vùng tìm manh mối ở các mép phòng. Biến thể manh mối gắn ID ghế ổn định, không phụ thuộc tên mesh.
- **Giữ trật tự:** chia hành lang thành vùng chuẩn bị, vùng phối hợp qua loa/cửa, vùng tập hợp an toàn. Khoảng cách hai trạm phải phù hợp việc phối hợp 2 người. Đo lại thời gian đi thực tế trước khi đổi graceSeconds hoặc ngưỡng tiếng ồn.
- **Bài kiểm tra:** bố trí hướng dẫn, các điểm tra cứu/gợi ý và sa bàn trung tâm; đường đi quanh sa bàn đủ cho bốn người. Không lấp chỗ trống bằng bàn thi vô nghĩa; các vùng phục vụ đọc đề, suy luận, thao tác, đối chiếu.
- **Không quay đầu:** hướng cấm dựa theo trục tuyến hành lang, điểm đầu/điểm cuối và checkpoint; kiểm tra góc cua, cửa và chướng ngại không buộc người chơi quay đầu để vượt qua. Dùng biển trên tường và dấu làn thay chữ khổng lồ treo giữa không gian.
- **Giáo viên:** các điểm nghe phân bố thành cụm quanh vùng hội ý; đọc được danh tính kênh và đường tới khu xác nhận. Giữ khoảng cách hợp lý để giao tiếp nhóm, không biến thành chạy qua lại giữa bốn góc xa.

## 5. Prefab giúp thay model dễ dàng

Ưu tiên các prefab hiện có thay vì tạo bộ thứ hai cùng chức năng. Chuẩn hóa cửa, ghế tương tác, trạm giữ nút, hồ sơ, bảng điểm, sa bàn, thẻ lưng, terminal và trạm giáo viên. Kiến trúc độc bản giữ dưới nhóm Architecture; chỉ tạo prefab kiến trúc khi thật sự cần lặp lại và sửa đồng bộ, không coi việc mọi bức tường phải là prefab là mục tiêu.

Cấu trúc mục tiêu cho mỗi vật tương tác:

```text
PF_GradeEvidenceStation
  Logic              # component, ID, tham chiếu dữ liệu
  Interaction        # vùng tương tác, hitbox được đặt tên
  Anchors            # điểm đọc, điểm nhìn, âm thanh, vị trí đồ vật
  VisualRoot         # model có thể thay; không chứa luật nhiệm vụ
  Feedback           # đèn trạng thái, nhãn, hiệu ứng được tham chiếu rõ
```

Component có thể nằm ở root nếu hệ thống hiện tại yêu cầu; mục tiêu là tách phụ thuộc, không chuyển component tùy tiện. Cửa có MovingVisualRoot riêng để animation và collider chuyển động khớp nhau. Ghế giữ SitPoint/StandPoint và SeatId khi thay mesh. Collider phục vụ tương tác khác collider va chạm vật lý; thay model phải kiểm tra cả hai.

Không tìm mesh theo tên như Head/Table để điều khiển luật. Dùng trường tham chiếu serialize hoặc binding hình ảnh; giữ lại renderer phản hồi trạng thái khi thay model. Tài sản ThirdParty để nguyên nguồn; prefab dự án bọc chúng hoặc dùng variant phù hợp. Có bảng liệt kê đường dẫn prefab, nhiệm vụ sử dụng, VisualRoot và anchor cần giữ.

Tổ chức mục tiêu: `Features/<Mission>/Prefabs/Interactables/`, `Shared/Prefabs/Interactables/` cho cửa/vật thực sự dùng chung, `Maps/SchoolFloor/Environment/` cho môi trường. Khi chuyển asset phải giữ GUID qua Unity AssetDatabase, không xóa rồi tạo lại. Bổ sung cửa sổ editor “School Floor Objects” lọc theo nhiệm vụ, nút chọn instance trong scene và mở prefab; thao tác thay visual giữ logic và nhắc kiểm tra bounds/collider.

Hierarchy scene theo khu: `Campus/Architecture`, `Campus/Areas/Room_01/...` và `Campus/Areas/Corridor_02/...`; mỗi khu có Gameplay, Dressing, Lighting, SpawnAndGather. Các hệ quản lý flow/UI/party có nhóm Systems riêng. Tên vật gồm loại + ID rõ ràng, không dùng Cube (123).

## 6. Thứ tự triển khai và điểm nghiệm thu

1. **Bảo toàn hiện trạng:** đọc PROJECT_STATE, kiểm tra thay đổi mới trong scene; tạo snapshot mới. Lập ánh xạ vật nhiệm vụ và dữ liệu tham chiếu. Không chạy builder cũ.
2. **Chuẩn hóa một cửa và một trạm hồ sơ:** thay thử model qua VisualRoot, xác nhận interaction/ID/feedback không hỏng. Lập danh mục prefab để người dùng tìm được ngay.
3. **Làm mẫu phòng học vụ trong Phong_2:** gỡ vỏ phòng cũ khỏi bản chơi; bố trí theo năm vùng ở trên. Chụp góc nhìn người chơi và mặt bằng. Đây là mốc đánh giá trước khi nhân rộng art.
4. **Chuyển hai nhiệm vụ ra hành lang:** chuyển các mốc, trigger, checkpoint và cửa theo tuyến thật; xử lý đường tiếp cận từ sảnh. Kiểm thử đội 2 người trước rồi 3–4 người.
5. **Chuyển ba nhiệm vụ dùng phòng còn lại:** áp dụng kiến trúc prefab đã kiểm chứng; bỏ toàn bộ hộp phòng lồng bên trong. Đánh dấu các khu dự trữ chưa có nhiệm vụ.
6. **Cập nhật flow và kiểm thử:** thứ tự nhiệm vụ độc lập với số phòng; không coi mỗi khu kiến trúc là một nhiệm vụ. Kiểm tra hoàn thành, retry, test nhanh, quay lại và kết thúc map. Sau mỗi phần cập nhật PROJECT_STATE và bằng chứng ngắn.

Nghiệm thu: không còn vỏ phòng cũ lồng trong blockout; đúng 4 nhiệm vụ phòng + 2 nhiệm vụ hành lang; hai phòng/bốn hành lang dự trữ không làm kẹt progression; thay model không mất component/ID; đủ 24 bàn ghế điểm danh; manh mối tìm được có định hướng; lối cửa và checkpoint không bị che; ảnh góc nhìn người chơi thể hiện rõ chức năng mỗi vùng. Compile sạch và raycast đi được là điều kiện cần, không thay cho đánh giá thiết kế hoặc playtest 2–4 người. Không báo multiplayer đã đạt nếu chỉ test đổi vai trên một máy.
