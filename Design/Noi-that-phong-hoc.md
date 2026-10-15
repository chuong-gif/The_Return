# Nội thất sáu nhiệm vụ

- Mở SchoolFloorPrototype, dùng `The Return/Art/Dress All Mission Interiors` để dựng lại nội thất.
- Mỗi root nhiệm vụ chứa `Interior_Decoration`. Material dùng chung lưu ở `Assets/TheReturn/Art/Interiors`.
- Dựng theo sàn local của từng phòng để đi cùng bố cục đảo đã xoay. Không đặt đồ theo tọa độ world ước lượng.
- Tường hai tông, chân tường, viền gỗ, cửa sổ giả, dầm và đèn trần, bảng thông báo và tủ đồ.
- Lớp điểm danh giữ 24 bộ bàn ghế; mặt gỗ, khung thép và vở. Nhãn ghế, collider và bộ phận tương tác giữ nguyên. Khối chân cũ chỉ tắt renderer.
- Chỉ ẩn vật trong hai root art đại sảnh nếu bounds của chúng xuyên vào thể tích nội thất. Không xóa chúng; có thể bật lại trong Hierarchy.
- Khi dựng lại đại sảnh/thiên văn, chạy lệnh nội thất sau cùng để loại vật trang trí xuyên phòng.
- Kiểm tra Play Mode trực quan lớp điểm danh và phòng hồ sơ; Console không có lỗi ở lần kiểm tra cuối. Chưa chơi giải toàn bộ sáu nhiệm vụ hoặc đo FPS.
- Ảnh: `ArtPreviews/Classroom_Dressed.png`, `ArtPreviews/Records_Final.png`.
- Nguồn hiện dùng: BackroomsLikeAsset và Yughues đã nhập. Gói School assets 146253 đã mở trên Asset Store, nhưng Add to My Assets không trả xác nhận sau hai lần thử; chưa tải/import gói mới.
- Không commit/push theo yêu cầu.
