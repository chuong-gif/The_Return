# CARD: PlankAndKey — Hanh_Lang_6

Chỉ mục tra cứu, không phải đặc tả đầy đủ. Domain/README/test hiện hành là nguồn sự thật.

- Thành phần chính: `Assets/TheReturn/Features/PlankAndKey/Runtime/{Domain,Network,Online,Presentation}`; assembly test `TheReturn.PlankAndKey.Network.Tests`.
- Luật bền vững: người chơi đi theo đội qua cầu; thẻ phải cùng bờ với đội; thời gian đi theo lượt của người chậm nhất; host quyết định trạng thái.
- Đơn vị thời gian mặc định 3 giây; đồng hồ tổng chỉ tính lượt qua cầu (quyết định chủ dự án, T-006b2); kiểm `PlankSettings` nếu hành vi khác.
- Online đã được người dùng thử 2 máy trước đây; T-042 nối vào MapSchoolF. Room→map/kiểm smoke hiện tại xem `QUEUE.md`.
- Nơi bắt đầu lỗi: `PlankState`/`PlankSession` cho luật và timer; `PlankOnlineHost`/`Client` cho tương tác; `MapFlowOnline` cho handoff.
- Lỗi đã sửa cần tránh: timer không được làm public snapshot đổi trong cùng Version; xem F-006/`FAILURES.md`.
- Kiểm tra: `TheReturn.PlankAndKey.Network.Tests`; Gauntlet 2026-11-12: 55/55 đạt.
