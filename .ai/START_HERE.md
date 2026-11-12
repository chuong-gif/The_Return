# START_HERE — hướng dẫn khởi động

Dự án Unity The Return, co-op 2–4 người. Chủ dự án giao tiếp bằng tiếng Việt.

## Mỗi phiên
1. Đọc file này, `.ai/CURRENT.md` và tối đa 8 mục cuối `.ai/LOG.md`.
2. Mở đúng dòng việc trong `.ai/TASKS.md`; nếu làm bước map/online, mở đúng thẻ ở `.ai/QUEUE.md`.
3. Chỉ đọc plan, quyết định, system card, owner update hoặc mã nguồn liên quan trực tiếp. Tìm đúng đoạn trước; không quét cả thư mục hay lịch sử.
4. Trước khi sửa, kiểm tra trạng thái thực và thay đổi hiện có. Ghi START/TASKS; khi dừng hoặc xong, ghi LOG/TASKS/CURRENT theo `.ai/PROTOCOL.md`.

## Phạm vi hiện hành
Chỉ phát triển PlankAndKey, Attendance, BreakerPanel, Choir, map mới và online dùng chung. Không mở rộng tám nhiệm vụ đã bỏ. Việc xóa code của chúng vẫn bị chặn theo Q-013 tới khi chủ dự án xác nhận map mới chạy và đã commit/tag. Trạng thái chuẩn nằm trong CURRENT, TASKS và QUEUE.

## An toàn
- Không commit, push, đổi nhánh, reset hay xóa nếu chưa được yêu cầu rõ; không ghi bí mật.
- Không chạy builder/migration cũ trong menu `The Return/`.
- Chỉ sửa scene/prefab khi đã có phép phù hợp; chụp snapshot trước sửa và không ghi đè scene đang dirty.
- Báo rõ mức xác minh: tạo/sửa file, compile, test, runtime hay chủ dự án xác nhận. Compile không đồng nghĩa runtime đã đúng.

## Mở tài liệu có chọn lọc
- `CURRENT.md`: trạng thái và cảnh báo hiện hành.
- `TASKS.md` / `QUEUE.md`: chủ việc và bước tiếp theo; QUEUE là nguồn cho thẻ online.
- `PROTOCOL.md`: quy trình khi cần thao tác task, sửa lỗi, tiếp quản hoặc kết thúc.
- `PLANS/`, `SYSTEM_CARDS/`, `MAPS/`, `DECISIONS/`, `BUGS/`: chỉ mở tài liệu được task trỏ tới.
- `LOG.md`: tối đa 8 mục gần nhất; chi tiết cũ đã loại nếu không còn tác dụng cho Core4.
- `NETWORK.md`, `TESTING.md`, `TOOLING.md`, `MAPS/`, `SYSTEM_CARDS/`: chỉ mở phần liên quan tới task hiện tại.
- `PROJECT_STATE.md`, `Design/`: tài liệu lịch sử/cold; không đọc trừ khi task cần.

Không lưu danh sách việc rảnh ở đây; kiểm tra TASKS/QUEUE trước khi nhận việc.
