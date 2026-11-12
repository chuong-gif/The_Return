# NETWORK — nền online của Core4

## Quyết định và bất biến
- NGO + Unity Relay + Vivox; đội cố định 2–4 người, một avatar/slot mỗi người; không host migration. Host thoát thì cả nhóm rời.
- Host-authoritative: client gửi lệnh, host xác thực/chạy luật và gửi snapshot; client không được quyết định luật hoặc khai vị trí. Trạng thái riêng chỉ gửi đúng người.
- Giữ nguyên chế độ offline. Thay đổi scene/network phải tương thích điều phối map và 4 adapter.

## Thành phần chung
- `Features/Network/Runtime/Framework`: luật/authority/replica thuần C#.
- `Features/Network/Online`: avatar, slots, room, voice, MissionNetBridge.
- `Maps/SchoolFloor/Online`: MapFlowOnline và adapter giữa map với nhiệm vụ.
- Relay mặc định WSS; phải bật/tắt `UnityTransport.UseWebSockets` đúng theo giao thức.
- Hợp đồng/code chi tiết: `SYSTEM_CARDS/NetworkFramework.md`, `SYSTEM_CARDS/OnlineAvatars.md`.

## Bốn nhiệm vụ
PlankAndKey, Attendance, BreakerPanel và Choir đều đã được nối vào MapSchoolF trong T-042. Tình trạng xác minh hiện tại:
- Host Play một mình và Gauntlet đã chạy; đây không phải xác nhận mạng nhiều máy.
- Luồng online phòng→map đang chờ P-014; không tự đổi đích scene/build khi chưa có quyết định.
- Smoke 2 máy/Relay và nghiệm thu người thật còn theo T-040/T-042; chi tiết bước mới nhất ở `QUEUE.md` và `PLANS/T-042-net-wiring-mapschoolf.md`.

## Quy tắc kiểm thử
- Phân biệt M3 (compile/test), M4 (runtime/nhiều tiến trình) và M5 (chủ dự án xác nhận).
- Khi sửa adapter hoặc framework, chạy Gauntlet theo `TESTING.md`; ghi lỗi mới vào `FAILURES.md` chỉ khi còn tái hiện.
- Không chạy builder cũ hoặc tự tạo scene/build test nếu chưa được yêu cầu.
