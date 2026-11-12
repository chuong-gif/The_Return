# CARD: Choir — Phong_6

Chỉ mục tra cứu, không phải đặc tả đầy đủ. Domain/README/test hiện hành là nguồn sự thật.

- Thành phần chính: `Assets/TheReturn/Features/Choir/Runtime/{Domain,Network,Online,Presentation}`; assembly test `TheReturn.Choir.Network.Tests`.
- Luật bền vững: nhịp online theo số người: 3 giây (2 người), 2.5 giây (3), 2 giây (4). Thẻ ký hiệu/bục là thông tin riêng của người sở hữu.
- Scene map đích: `MapSchoolF`; online adapter đã nối bởi T-042. Room→map và smoke xem `QUEUE.md`.
- Nơi bắt đầu lỗi: `ChoirState`/`ChoirRules`, HostRules/Codec; dùng `MapFlowOnline`/adapter để tra tích hợp.
- Không nới cửa sổ nhịp hoặc thay luật vì lo độ trễ khi chưa có quyết định; kiểm 2–4 người/Relay theo T-042.
- Kiểm tra: `TheReturn.Choir.Network.Tests`; Gauntlet 2026-11-12: 65/66 đạt, 1 bỏ qua; không phải kiểm nhiều máy.
