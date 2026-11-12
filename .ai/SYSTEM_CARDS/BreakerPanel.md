# CARD: BreakerPanel — Phong_5

Chỉ mục tra cứu, không phải đặc tả đầy đủ. Domain/README/test hiện hành là nguồn sự thật.

- Thành phần chính: `Assets/TheReturn/Features/BreakerPanel/Runtime/{Domain,Network,Online,Presentation}`; assembly test `TheReturn.BreakerPanel.Network.Tests`.
- Luật bền vững: 2–4 người; thao tác/công tắc được ràng buộc theo quyền sở hữu; kiểm quá tải và thời gian giữ khớp theo Domain.
- Scene map đích: `MapSchoolF`; online adapter đã nối bởi T-042. Room→map và smoke xem `QUEUE.md`.
- Nơi bắt đầu lỗi: `BreakerState`/`BreakerRound` cho luật; Breaker HostRules cho xác thực; `MapFlowOnline`/adapter cho tích hợp.
- Lỗi đã sửa cần tránh: snapshot public không được đổi trong cùng Version do đồng hồ; xem F-001/`FAILURES.md`.
- Kiểm tra: `TheReturn.BreakerPanel.Network.Tests`; Gauntlet 2026-11-12: 70/71 đạt, 1 bỏ qua; không phải kiểm nhiều máy.
