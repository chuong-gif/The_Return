# Lớp trang trí thiên văn

## Xem kết quả

- Mở `Sanh_Cho` rồi Play để xem portal mặt số, vòm nhọn, sàn chia ô, các bục I–IV và dãy tủ học sinh.
- Tầng học có đồng hồ treo giữa vực, ba vòng quỹ đạo nghiêng và cột phân tầng. Quan sát từ phía cầu đến đại sảnh.
- Ảnh chụp Play Mode nằm trong `ArtPreviews/Lobby_Observatory_Final.png` và `ArtPreviews/School_Observatory_Final-1.png`.

## Chỉnh sửa và tái sử dụng

- `Observatory_Decoration` chứa toàn bộ chi tiết của lượt trang trí này; mỗi chi tiết mới không có collider.
- Material, mesh vòng và profile màu lưu trong `Assets/TheReturn/Art/Environment`.
- Mở một trong hai scene, dùng `The Return/Art/Apply Observatory Decoration` để dựng lại. Lệnh thay lớp trang trí cùng tên, không nhân bản lớp cũ.
- Nếu dùng bộ dựng sảnh/đại sảnh cũ, chạy lại lệnh trang trí trên sau khi dựng.
- Profile có ACES, bloom nhẹ, cân màu và vignette; chỉnh trực tiếp profile nếu muốn thay cảm giác hình ảnh. Chạy lại lệnh sẽ phục hồi các giá trị mặc định của bộ dựng.
- Đạo cụ sử dụng prefab Backrooms trong ThirdParty. Vật liệu Limestone tham chiếu texture SciFi của Yughues; không sửa asset gốc của nhà phát hành.

## Phạm vi kiểm tra

- Hai scene đã được tải và quan sát trong Play Mode; kiểm tra scene không phát hiện missing script hoặc broken prefab.
- Không chạy lại bộ câu đố đầy đủ cho lượt trang trí này; chưa đo FPS trên máy cấu hình mục tiêu.
- Console ghi cảnh báo nội bộ PlayerLoop đệ quy khi sử dụng công cụ kiểm tra Unity. Chưa xác định nguyên nhân.
- Chưa tải được asset mới: công cụ trình duyệt không khởi động được do lỗi helper sandbox. Lượt này dùng các gói đã nhập, không có tải mới từ Asset Store.
- Không tạo commit hoặc push GitHub.
