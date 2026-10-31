# DECISION 2026-10-31: BỎ chuyển host (T-030 hủy)
Người quyết: chủ dự án (chat với phiên #6, 2026-10-31): «bỏ cơ chế chuyển host; máy host off thì cả đám rời phòng; không làm lằng nhằng, muốn game hoàn thành nhanh».
- GHI ĐÈ DECISIONS/2026-10-29-t009-owner-answers.md câu 3 (trước đó chọn CÓ chuyển host).
- Host rớt mạng hoặc thoát: mọi người rời phòng, về màn hình chính kèm thông báo. Đây đúng là hành vi hiện có (RoomService.OnClientDisconnect: «Mất kết nối với chủ phòng» rồi LeaveSession), không cần làm thêm.
- Không làm: chuyển host, nhật ký sự kiện, vào lại phòng, giữ ô theo PlayerId, thử nghiệm S0. Tài liệu PLANS/T-030-host-migration-design.md giữ lại để tham khảo, KHÔNG thực hiện.
- Phần chưa được chủ dự án nói rõ, AI mặc định cho đơn giản (chủ dự án đổi ý thì báo): nếu MỘT NGƯỜI THƯỜNG rớt hoặc thoát giữa ván thì ván kết thúc, mọi người về màn hình chính kèm thông báo «<tên> đã rời phòng», không có vào lại. Lý do: nhiệm vụ cần đủ cả đội nên đội thiếu người không chơi tiếp được; làm gộp vào T-028/T-031 khi xử lý ngắt kết nối, không thành task riêng.
