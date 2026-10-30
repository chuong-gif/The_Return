# PHIẾU T-030: Rớt mạng, vào lại, chuyển host
- Mục tiêu (1 câu): người rớt mạng vào lại được; khi host thoát/mất, chuyển host cho người khác (chủ dự án chọn CÓ).
- BƯỚC 1 (làm trước, CHƯA code): tài liệu PLANS/T-030-host-migration-design.md: NGO không có sẵn chuyển host; hướng đề xuất = host gửi ảnh chụp trạng thái định kỳ cho mọi máy; mất host thì 1 máy được bầu tạo phòng Relay mới + nạp ảnh chụp, người còn lại tự vào bằng mã mới. Nêu công sức, rủi ro, cái sẽ mất (vd nhiệm vụ đang dở).
- Được đụng: tài liệu; code chỉ SAU khi chủ dự án duyệt thiết kế (Features/Network/*).
- KHÔNG được đụng: Domain nhiệm vụ.
- Cách kiểm xong: thử rút mạng host giữa chừng, 2 máy còn lại tiếp tục được.
- Đọc trước (tối đa 3): PLANS/T-009 mục 8; T-028 (trạng thái map); tài liệu NGO (web).
- Chờ: T-028 cho phần code; phần tài liệu làm được ngay.
