# TESTING — kiểm tra có liên quan Core4

## Quy trình
- Dùng Unity Test Runner/CLI theo tool khả dụng; chạy đúng assembly liên quan. Đọc output/Console thực tế.
- Test Unity có thể ghi file hoặc thay đổi scene; xác định tác dụng phụ trước khi chạy.
- Mức xác minh: M1 file; M2 nội dung/diff; M3 compile hoặc test chạy; M4 runtime/build/nhiều tiến trình; M5 chủ dự án/người thật xác nhận.
- Gauntlet: `TheReturn/Gauntlet/Chạy Gauntlet mạng`; đọc `.ai/GAUNTLET.md` khi không còn trạng thái «ĐANG CHẠY». Lỗi tái hiện ghi vào `FAILURES.md`; test xanh lại rồi mới đóng lỗi.

## Kết quả gần đây
- Gauntlet 2026-11-12: 981 test, 976 đạt, 0 đỏ, 5 bỏ qua, 25 assembly. Kết quả cũ gồm nhiệm vụ đã bị cắt; không coi đây là baseline sạch Core4 hoặc lần chạy mới.
- Bộ lọc cũ từ báo cáo đó cho Core4 + hạ tầng: 656 test, 653 đạt, 0 đỏ, 3 bỏ qua/18 assembly. Đây là phép lọc tài liệu, không phải lần chạy thực.
- T-042: host Play một mình đã đi qua 4 nhiệm vụ; chưa xác nhận online nhiều máy/Relay. Xem `QUEUE.md`.

## Phạm vi
Chỉ chạy test liên quan bốn nhiệm vụ, map mới, online dùng chung. Không chạy test legacy có teleport/tọa độ cũ nếu chưa đối chiếu tác dụng phụ và scene hiện hành. Không ghi lại kết quả cũ như xác minh mới.
