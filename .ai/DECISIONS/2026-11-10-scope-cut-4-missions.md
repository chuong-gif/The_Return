# DECISION 2026-11-10: chủ dự án CẮT phạm vi từ 12 nhiệm vụ xuống 4 (nhắn trong chat với phiên #13)
Lý do chủ dự án nêu: tiến độ phát triển quá lâu, ưu tiên những nhiệm vụ có tiến độ làm nhanh nhất.
GIỮ 4: PlankAndKey (Hanh_Lang_6), Attendance (Phong_1), BreakerPanel (Phong_5), Choir (Phong_6).
BỎ 8: TeacherSays (Hanh_Lang_5), GlassBridge (Hanh_Lang_2), SharedLantern (Hanh_Lang_4), QuietCorridor (Hanh_Lang_1), GradeRepair (Phong_2), NavigationExam (Phong_3), DontLookBack (Hanh_Lang_3), TeacherTruth (Phong_4).
Yêu cầu: tạo task kế hoạch; tạo SCENE MỚI và thiết kế lại map cho vừa 4 nhiệm vụ.
Kế hoạch: .ai/PLANS/T-040-scope-cut-4-missions.md (task T-040).
CHƯA quyết (xem DECISIONS/PENDING.md P-004..P-008): mức bỏ (đóng băng hay xóa code), hướng map, hành lang 5, thứ tự chơi, số phận scene cũ. Cho tới khi chủ dự án trả lời: dùng phương án PROVISIONAL (ĐÓNG BĂNG, không xóa gì).
Hiệu lực ngay: AI KHÔNG bắt đầu/tiếp tục việc mới cho 8 nhiệm vụ bị bỏ. Việc dở của 8 nhiệm vụ đó: dừng ở trạng thái xanh, ghi LOG, không xóa file.
## CÂU TRẢ LỜI của chủ dự án (chat, 2026-11-10 08:21 UTC)
1. Hướng map: DỰNG MỚI HOÀN TOÀN cho gọn (PA2). Không nhân bản/cắt scene cũ.
2. 8 nhiệm vụ bị bỏ: XÓA HẲN code SAU KHI scene mới chạy được (mức B). Trước đó vẫn đóng băng, không xóa.
3. Hành lang 5 (giữa Phòng 5 và Phòng 6): GIỮ làm hành lang đi lại (không nhiệm vụ). Trong map dựng mới: giữ một hành lang nối khu Cầu dao và khu Choir.
HỆ QUẢ AI phải xử lý: (a) cần BẢN THIẾT KẾ sơ đồ map để chủ dự án duyệt trước khi dựng; (b) xóa code 8 nhiệm vụ làm SCENE CŨ SchoolFloorPrototype hỏng (tham chiếu nhiệm vụ bị xóa) nên scene cũ + builder + bộ *Checks liên quan phải xóa/gỡ cùng đợt; (c) code điều phối dùng chung (MissionFlowController, MapProgress, AccessController...) có thể tái dùng cho scene mới nếu tách được khỏi 8 nhiệm vụ.
## Trả lời về layout (2026-11-10 19:23 UTC)
- Bố cục v1 (sân dài + một hàng) bị bác: muốn bố cục vòng quanh sân hoặc chữ L/U.
- Kích thước các khu: THU SÁT theo mức tối thiểu cho gọn.
- (2026-11-10 19:24 UTC) Layout: v2 chữ U hở bị bác; chủ dự án muốn VÒNG KHÉP KÍN quanh sân.
