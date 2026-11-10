# T-040 P3a Thiết kế map mới — ĐỀ XUẤT v3 (CHỜ CHỦ DỰ ÁN DUYỆT) — 2026-11-10 19:24 UTC, phiên #13
Nền: PLANS/T-040-dependencies.md (footprint). Quyết định: DECISIONS/2026-11-10-scope-cut-4-missions.md. Chưa dựng gì trong Unity.

## 0. Lịch sử
- v1 (sân dài + một hàng, 128 x 56 m, chừa dư): BÁC. Chủ dự án muốn vòng quanh sân + kích thước thu sát. KHÔNG đề xuất lại.
- v2 (chữ U quanh sân giữa, ~90 x 50 m): BÁC. Chủ dự án muốn VÒNG KHÉP KÍN quanh sân. KHÔNG đề xuất lại chữ U hở.
- v3 (file này): vòng khép kín quanh sân giữa.

## 1. Ý tưởng v3
Sân giữa 36 x 30 m được BAO KÍN bốn phía. Theo chiều kim đồng hồ từ cánh trái: Cầu dao (tây) - hành lang đi lại (góc tây-bắc) - Choir (bắc) - PlankAndKey (đông-bắc) - Điểm danh (đông) - hành lang khép vòng (nam, nối hai cánh) - về Cầu dao. Thứ tự bốn nhiệm vụ giữ đúng cung cũ P5 - HL5 - P6 - HL6 - P1. Tổng khoảng 90 x 60 m. Sơ đồ nhìn từ trên xuống đã vẽ trong chat.

## 2. Các không gian (m)
| Không gian | Loại | Rộng x sâu | Ghi chú |
|---|---|---|---|
| Sân giữa | xuất phát + bục thắng | 36 x 30 | bị bao kín bốn phía |
| Phòng Cầu dao | khu nhiệm vụ | 24 x 20 | mức tối thiểu |
| Hành lang đi lại (cũ HL5) | không nhiệm vụ | 32 x 10 | giữ theo chọn của chủ dự án; mở ra sân, không cổng |
| Phòng Choir | khu nhiệm vụ | 20 x 16 | mức tối thiểu |
| Hành lang PlankAndKey | khu nhiệm vụ | 30 x 20 | tối thiểu 24 x 20; chọn 30 để áp sát Choir và Điểm danh |
| Phòng Điểm danh | khu nhiệm vụ | 30 x 26 | mức tối thiểu |
| Hành lang khép vòng (MỚI) | không nhiệm vụ | dải nam 90 x 10 + hai đoạn nối 24 x 10 và 30 x 4 | tổng khoảng 1.260 m2; mở ra sân, không cổng |

## 3. Cổng và tiến trình (GIỮ NGUYÊN luật hiện hành)
- 4 cổng: Cầu dao (cạnh đông phòng), Choir (cạnh nam), PlankAndKey (cạnh nam, phần sát góc đông-bắc sân), Điểm danh (cạnh tây). Hành lang đi lại và hành lang khép vòng KHÔNG có cổng, mở ra sân.
- Mọi cổng đóng lúc đầu. Cả đội đứng đủ NGOÀI cổng thì cổng mở; vào đủ BÊN TRONG thì nhiệm vụ bắt đầu và khóa lối; xong thì mở lại. Xong cả 4 thì bục thắng ở sân kích hoạt. Thứ tự tự chọn.
- 4 điểm xuất hiện (PlayerSlot_0..3) ở nửa nam của sân, quay mặt về các cổng.

## 4. Điều cần hiểu đúng / chưa biết
- 'Khép kín' nghĩa là sân được bao kín bốn phía bởi các khu và hành lang, KHÔNG phải một vòng đi xuyên qua các phòng: các phòng chỉ có cổng ra sân, không có cửa thông nhau. Muốn đi từ khu này sang khu khác vẫn qua sân.
- Hành lang khép vòng thực chất là phần mở rộng của sân (có thể đặt đạo cụ/cốt truyện). Nếu chỉ cần ranh giới kín mà không cần không gian đi lại, có thể thay bằng một dải tường: tiết kiệm khoảng 1.260 m2 và công dựng. Đây là lựa chọn rẻ hơn nếu chủ dự án thấy dải hành lang dư thừa.
- Chưa thiết kế: chiều cao, trần, vật liệu, ánh sáng, camera, nghệ thuật. Choir/Cầu dao/PlankAndKey đang LogicOnly nên kích thước tối thiểu có thể phải nới.
- Chưa kiểm: tầm với/khoảng cách tới cổng và độ chật của sân khi cả 4 người đứng trước cùng một cổng (sẽ thử khi dựng blockout).

## 5. Câu hỏi duyệt (PENDING P-010)
Vòng khép kín v3 được chưa? Hành lang khép vòng giữ làm không gian đi lại hay thay bằng dải tường?
