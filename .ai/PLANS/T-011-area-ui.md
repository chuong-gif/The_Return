# T-011 — UI theo khu (KẾ HOẠCH, chờ chủ dự án duyệt)

Trạng thái: ĐỀ XUẤT (2026-10-19 09:07 UTC, Claude Sonnet 5.5 phiên #3). Chưa sửa code/scene.

## 1. Hiện trạng
- Một Canvas duy nhất (SchoolFloorCanvasView) + SchoolFloorCanvasPresenter (partial, ~366 dòng + DontLookBack/Puzzles) gồm: tiêu đề, mục tiêu, gợi ý, sổ tay, nút thử nhanh, màn thắng riêng (WinScreenPresenter).
- 6 nhiệm vụ cũ vẽ theo pha 0-5; 6 nhiệm vụ mới dồn vào PhaseOther=6 qua ExtraTitle/ExtraStatus/ExtraInfo/ExtraNotebook. Nhiều nhiệm vụ còn OnGUI riêng (đã tắt bằng drawOnGui=false khi có Canvas).
- Bảng Điểm danh vẫn hiện ở sân (ghi trong CURRENT).
- Nguyên tắc CONVENTIONS: nhiều không gian nhìn, bố cục linh hoạt theo khu, HUD không che khu chơi.

## 2. Vấn đề cần giải
1. Một khối text cho mọi khu: khu nào cũng chung vị trí, dài, che màn hình.
2. Sổ tay riêng từng vai (thông tin bất đối xứng) chen cùng HUD chung.
3. Chưa có HUD cho sân (chọn phòng): tiến độ 0/12, khu nào xong, khu nào kề.
4. Chưa có mô hình chung để thêm khu mới mà không sửa presenter.

## 3. Đề xuất kiến trúc
- Domain (thuần, có test): `AreaHudModel` {Title, Objective, Timer?, Warnings?, Notebook(player), Alerts[]}; mỗi adapter/nhiệm vụ trả model, presenter chỉ vẽ.
- Presentation: `IAreaHudProvider` (adapter tùy chọn implement) -> presenter gọi theo khu đang hoạt động; khu không implement thì dùng text hiện có (tương thích ngược).
- Layout chung 4 vùng cố định: (a) thanh trên nhỏ: tên khu + tiến độ; (b) góc trái dưới: sổ tay vai đang xem (thu/gom bằng phím); (c) giữa trên: cảnh báo ngắn tự tắt (đồng hồ, cảnh cáo); (d) panel khu chuyên biệt tùy chọn (ví dụ bảng Cô nói, bảng cầu dao) đặt trong thế giới như hiện tại.
- Chế độ Sân: bản đồ nhỏ 12 khu (xong/đang/chưa) + gợi ý lối kề; thay bảng Điểm danh.
- Cỡ chữ/độ mờ chỉnh được qua một ScriptableObject `HudSettings`.

## 4. Các bước (mỗi bước nhỏ, kiểm mức 3 trước)
S1 AreaHudModel + test Domain. S2 IAreaHudProvider + presenter vẽ 4 vùng (giữ đường cũ). S3 chuyển 6 nhiệm vụ mới sang provider. S4 HUD Sân + bỏ bảng Điểm danh ở sân. S5 chuyển 6 nhiệm vụ cũ (từng cái một). S6 Play Mode kiểm + chủ dự án xem.
Scene: S2/S4 cần sửa Canvas trong scene (Scene lock + snapshot + phép của chủ dự án).

## 5. Câu hỏi cho chủ dự án
1. Muốn bản đồ nhỏ ở sân hay chỉ danh sách khu?
2. Sổ tay vai: luôn hiện hay bấm phím mới mở?
3. Có cho sửa Canvas trong scene (S2, S4) không?
4. Giữ chữ tiếng Việt cố định hay cần chuẩn bị đa ngôn ngữ?
5. Ưu tiên khu nào chuyển trước (đề xuất: 6 khu mới)?
