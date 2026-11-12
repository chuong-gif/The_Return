# CARD: Attendance — Phong_1

Chỉ mục tra cứu, không phải đặc tả đầy đủ. Domain/README/test hiện hành là nguồn sự thật.

- Thành phần chính: `Assets/TheReturn/Features/Attendance/Runtime/{Domain,Network,Online,Presentation}`; assembly test `TheReturn.Attendance.Network.Tests`.
- Luật bền vững: đội 2–4 người; 24 ghế (6×4); đề có nghiệm duy nhất; manh mối có thể riêng theo người. Không đưa dữ liệu riêng vào snapshot chung.
- Scene map đích: `MapSchoolF`; online adapter đã nối bởi T-042. Tình trạng room→map và smoke xem `QUEUE.md`.
- Nơi bắt đầu lỗi: `AttendanceState`/`AttendancePrototype` cho luật & trình bày; HostRules/Codec cho lệnh, snapshot, dữ liệu sai; `MapFlowOnline`/adapter cho tích hợp.
- Kiểm tra: `TheReturn.Attendance.Network.Tests`; Gauntlet registry ở `MISSIONS_NET.md`. Gauntlet 2026-11-12: 53/54 đạt, 1 bỏ qua; không phải kiểm nhiều máy.
- Trước khi chạy test Editor cũ, kiểm tác dụng phụ của `AttendanceChecks.Run()`: có thể ghi đè báo cáo `Design/Verification/AttendanceV2.json`.
