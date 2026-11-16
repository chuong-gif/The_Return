# KẾ HOẠCH T-056: Làm nhiệm vụ Cầu dao (Phong_5, MapSchoolF) dễ hiểu và dễ giải   (file: .ai/PLANS/T-056-breaker-easier.md)

Trạng thái: ĐANG LÀM | AI giữ: Claude (chat, Unity MCP) | 2026-11-16 11:02 UTC

## 1. Vấn đề (chủ dự án)
Nhiệm vụ Cầu dao khó hiểu, không biết giải thế nào.

## 2. Nguyên nhân
Mỗi công tắc đảo 2–3 đèn bí mật (bài XOR/GF(2)), chỉ chủ công tắc xem được; có thêm giới hạn quá tải và phải giữ khớp 3 giây; chữ hướng dẫn một câu dài; đèn không có số; nhãn công tắc «1 / VAI 2».

## 3. Thiết kế mới (giữ nguyên định dạng mạng/Domain, chỉ đổi đề và chữ)
- Mỗi công tắc bật ĐÚNG MỘT đèn cùng số (công tắc N ↔ đèn N; ngoài cảnh đèn nằm ngay trên công tắc). Đèn ban đầu tắt hết. Chấm XANH = đèn phải SÁNG. Chọn 2..giới hạn đèn đích, thuộc ít nhất 2 vai khác nhau nên cần phối hợp.
- Giới hạn quá tải giữ nguyên nhưng đề không bao giờ cần vượt (HUD ghi rõ).
- HUD/sổ tay: Cách chơi 3 dòng, công tắc của bạn, đèn cần sáng/đang sáng, «Việc cần làm» chỉ rõ BẬT/TẮT công tắc nào của vai nào. Công tắc tô màu theo vai; đèn có số.
- Giữ bản đề khó cũ thành `BreakerGenerator.GenerateHard` (test cũ vẫn dùng được).

## 4. Phạm vi
Được đụng: BreakerGenerator.cs, BreakerGuide.cs (mới, Domain), BreakerSwitch/Lamp/PanelPrototype, BreakerSettings + asset BreakerPanel_VI, BreakerOnlineClient (+asmdef Shared), AreaHudProviders.Breaker, test Breaker. KHÔNG đụng: BreakerPublicState/Codec/HostRules (định dạng mạng), scene (nhãn đèn tạo lúc chạy). Bản gốc `Backups/*_BeforeT056.*.txt`.

## 5. Nghiệm thu
Compile sạch; test Breaker + EditMode toàn bộ xanh; Play MapSchoolF phòng 5: thấy số đèn, màu công tắc theo vai, HUD «Việc cần làm» đúng, bật đúng các đèn xanh + giữ 3 giây thì thắng.

## 6. Tiến độ
- 2026-11-16: khảo sát xong (đọc Domain/Presentation/Network), bắt đầu sửa.
