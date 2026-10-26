# Rà soát chất lượng 12 khu (2026-10-22 08:17 UTC, phiên #3)

## Đã kiểm (bằng chứng)
- EditMode: 250/250 test xanh (mức 3).
- Play Mode (nạp SchoolFloorPrototype bằng LoadSceneInPlayMode, không đổi scene đang mở của editor): bắt đầu cả 12 khu với đội 2, 3, 4 người, đổi khu liên tiếp -> 0 ngoại lệ, 0 lỗi Console (mức 4 mô phỏng).
- RestartMap sau từng khu: không adapter nào còn Active, progress sạch (12/12).
- Hoàn thành đủ 12 khu qua MapProgress -> Won=true, màn thắng hiện, liệt kê đủ 12 khu.
- Tham chiếu null trong scene: chỉ QuietSpeakerFeedback.replacementClip (âm thanh, chưa gán) và outsideZone của cổng (tùy chọn theo thiết kế).

## Lỗi tìm thấy
- WinScreenPresenter: danh sách 12 khu cao 450px > khung 440px, verticalOverflow=Truncate -> dòng cuối có thể bị cắt. ĐÃ SỬA (Overflow), compile sạch; chưa nhìn bằng mắt.

## Chưa kiểm được (cần người chơi thật)
- Cảm giác chơi, cân bằng thời gian/phạt, đổi vai bằng phím giữa chừng, va chạm CharacterController thật, âm thanh, hình tạm (hộp/khe tối HL6).
- Nhánh cảnh cáo/hết giờ/bị đưa về của Hanh_Lang_5 và các hành lang có phạt (chỉ có test Domain).
- Hoàn thành từng khu bằng cách chơi thật (mới ép qua registry, trừ HL5 đã mô phỏng).
