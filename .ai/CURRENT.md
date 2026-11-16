# CURRENT — trạng thái hiện hành (2026-11-13 UTC)

- Phạm vi: map mới và 4 nhiệm vụ: PlankAndKey, Attendance, BreakerPanel, Choir. Nhiệm vụ khác ngoài phạm vi; không sửa code của chúng.
- MapSchoolF do chủ dự án tạo hiện là scene đích; scene blockout SchoolFloorCore4 ban đầu đã bị thay thế. T-040/Q-012 được đánh dấu SUPERSEDED; Q-014 đang làm phần dùng chung, Q-015 xong. Q-013 chưa được làm nếu chủ dự án chưa xác nhận scene mới chạy và đã commit/tag.
- T-042: Q-016–Q-022 xong; Q-023 phần scene offline/Build Settings đã làm theo P-013. Phần online room->map chưa làm, chờ P-014; Q-024 chờ bước trước.
- Kết quả gần nhất được ghi: Gauntlet 2026-11-12 10:42, 981 test/25 assembly, 0 lỗi (5 bỏ qua). Không xem đây là xác nhận smoke 2 máy.
- T-043 đã rút gọn tài liệu AI; trạng thái REVIEW chờ chủ dự án xem. Không thay đổi công việc Assets/scene hiện có.
- Đọc `.ai/TASKS.md`, `.ai/QUEUE.md`, P-014 và plan của task trước khi tiếp tục; Unity/code là nguồn sự thật.
- 2026-11-13: chủ dự án muốn build thử online với người thật (có mic), ưu tiên tốc độ. Plan `PLANS/T-046-051-online-playtest.md` (T-046–T-051, Q-025) CHỜ DUYỆT + P-014; T-047 (thoại) đã xong, REVIEW; T-050 làm ngay được. Chủ dự án: AI chạy nối tiếp, không song song.
- 2026-11-13: plan T-046–T-051 + P-014 ĐÃ DUYỆT. T-046 dựng xong (scene OnlineRoom, nút «Chơi online», Build Settings +OnlineRoom), PAUSED chờ kiểm Play vì MapSchoolF đang dirty trong Editor (không tự lưu/bỏ). Kế tiếp theo plan: T-048, T-049, T-051 (nối tiếp); T-050 làm được ngay.
- 2026-11-13: T-046 REVIEW (đã kiểm một máy, test xanh). Kế tiếp: T-048, T-049, T-051; T-050 làm được ngay.
- 2026-11-14: T-052 REVIEW — sảnh chờ Sanh_Cho đã dùng mô hình người chơi như MapSchoolF + tiếng bước/nhảy (scene đã lưu).
- 2026-11-14: T-053 REVIEW — UI chuyên nghiệp 3 scene + menu Cài đặt dùng chung (MapSchoolF dùng SchoolFloorSkin). Panel nhiệm vụ cũ ẩn, chưa xóa (chờ Q-013). Chờ chủ dự án nhìn bằng mắt.
- 2026-11-14: giao diện MainMenu/Sanh_Cho/MapSchoolF đã bake vào object scene (chỉnh Inspector được). Việc kế của T-053: dựng menu Cài đặt thành prefab Canvas (hiện 100% code).
- 2026-11-14 (chat #4): T-054 phòng chờ online trong Sanh_Cho đang làm; plan `PLANS/T-054-online-lobby-sanh-cho.md`. S1 xong; kế: S2 test RoomState, S3 PF_OnlineSession.
- 2026-11-15: T-054 phần lớn xong 1 máy (tạo phòng Relay → Sanh_Cho online, Build Settings 3 scene, DevOnly). Còn: thử 2 máy (vào bằng mã, sẵn sàng, Bắt đầu→MapSchoolF), Vivox thật, quyết nút «CHƠI» offline ở bản thật.
- 2026-11-16: T-057 (chọn mic trong Cài đặt) REVIEW. EditMode còn 1 test đỏ của T-056 (BreakerPanelTests) — chờ chủ task T-056 chỉnh. T-054 vẫn PAUSED.
- 2026-11-16: T-055 REVIEW — tên người chơi online dùng trong nhiệm vụ Điểm danh + biển tên; T-054 đang tạm dừng (còn S7 UI dev EditorOnly, S8 Build Settings + smoke).
