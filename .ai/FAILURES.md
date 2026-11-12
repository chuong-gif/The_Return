# FAILURES — lỗi test còn cần theo dõi

Chỉ giữ lỗi tái hiện được có liên quan Core4 hoặc online chung. Lỗi lịch sử đã sửa và test giả lỗi đã bỏ; không coi chúng là việc đang mở.

| ID | Phạm vi | Triệu chứng/nguyên nhân đã biết | Trạng thái |
|---|---|---|---|
| F-001 | BreakerPanel | Public snapshot phải bất biến trong cùng Version; đồng hồ không được làm payload đổi ngầm. Regression: `PublicState_SameVersion_IsImmutable`. | FIXED; Gauntlet 2026-11-12 xanh |
| F-006 | PlankAndKey | Đồng hồ chạy từng làm public state đổi trong cùng Version; snapshot chỉ phát tại điểm bắt đầu lượt. | FIXED; 55/55 test trong Gauntlet 2026-11-12 |

Lỗi mới: ghi triệu chứng, assembly/test, lần đầu thấy, nguyên nhân và trạng thái. Khi đã tái hiện, tạo một thẻ FIX trong `QUEUE.md`; chỉ đóng sau test liên quan + Gauntlet xanh. Không lưu lỗi giả cố ý hoặc lịch sử không còn ảnh hưởng Core4.
