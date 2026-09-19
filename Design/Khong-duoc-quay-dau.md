# Nhiệm vụ 5 — Không được quay đầu

## Vào chơi nhanh
Mở SchoolFloorPrototype, nhấn Play → F8 → chọn 2–4 người → **5 • Không được quay đầu**.

Hoặc chọn SchoolFloor_Flow → MissionTestLauncher → Start Mission = Dont Look Back trước khi Play. Normal Play vẫn là mặc định.

Khi chơi theo tiến trình, giải sa bàn Bài kiểm tra rồi đóng bảng. Cửa phòng kiểm tra mở ra hành lang nhiệm vụ 5 trong cùng scene. Đưa đủ đội vào vùng tập hợp đầu hành lang để kích hoạt luật. Không kiểm góc quay khi mọi người còn ở phòng kiểm tra.

## Cách chơi
1. Đội chia vào hai làn, theo số thẻ: 1/3 ở trái, 2/4 ở phải. Đội 2 người có một người mỗi làn; đội 3 có hai người bên trái.
2. Hướng an toàn luôn là +Z theo mũi tên sàn. Được đi lùi bằng S. Mỗi người có một ký hiệu trên lưng; không tự đọc thẻ mình.
3. Người đọc đứng lùi hơn đồng đội, nhìn vào lưng họ rồi nhấn E. Đổi vị trí trước/sau bằng đi tiến hoặc lùi để đọc đủ thẻ, kể cả người đứng cuối.
4. Ghi chép chỉ hiện thẻ vai hiện tại đã đọc. Báo ký hiệu cho chủ thẻ qua trao đổi giữa người chơi; khi thử một máy dùng 1–4 đổi vai.
5. Đưa cả đội vào vùng cuối hành lang. Tới một trong hai bảng khóa, nhấn E. Nhấn từng ô để chuyển qua sáu ký hiệu, nhập theo **số thẻ**, rồi xác nhận.

Khóa yêu cầu mỗi thẻ đã được một đồng đội quan sát bằng E, mã đúng và đủ đội ở cuối hành lang. Không thể chỉ đoán mã mà bỏ qua quan sát. Mã sai có phản hồi và cho sửa ngay.

## Luật góc nhìn và checkpoint
- Góc ngang lệch từ 80°: HUD cảnh báo.
- Quá 110° liên tục đủ 0,6 giây: tối màn hình ngắn và cả đội trở về checkpoint.
- Nhìn lên/xuống không tính là quay đầu.
- Trở lại góc an toàn sẽ xóa thời gian vi phạm liên tục. Đổi vai không xóa bộ đếm của người vừa quay.
- Menu/bảng khóa không tích thêm thời gian.
- Checkpoint 2 được lưu khi cả đội vào khu vực từ vạch checkpoint thứ hai tới cuối hành lang.
- Khi bị phạt, giữ nguyên đề, ghi chép và mã đang nhập; đặt hướng tất cả về +Z.
- Khi giải xong, bỏ hạn chế quay đầu và mở cửa đi tiếp.

R hoặc nút Thử lại: đặt lại riêng nhiệm vụ 5, xóa ghi chép/mã, về checkpoint đầu nhưng giữ cùng đề. Nút Đề / biến thể mới: đổi đề. Chơi lại cả map: đặt lại cả năm nhiệm vụ.

## Quản lý nội dung và tài sản
- Catalog: Assets/TheReturn/Features/DontLookBack/Data/DontLookBack_VI.asset.
  Sửa tiêu đề, hướng dẫn, cảnh báo, thông báo phạt, tên và hình ký hiệu tại đây. Giữ đủ 6 phần tử tương ứng trong Symbol Names và World Symbols.
- DontLookBack_LogicOnly trong scene: controller, checkpoint, vùng tập hợp, hai bảng khóa và cửa. Initial Seed = 0 sinh đề mới mỗi phiên; đặt số khác 0 để tái hiện một đề khi debug.
- Runtime/Domain: luật C# độc lập Unity, phù hợp để nối host-authoritative sau này.
- Runtime/Data: cấu hình text; Runtime/Presentation: camera, nhân vật, kiểm tra tầm nhìn và tương tác.
- Prefabs/PF_BackCard.prefab: thẻ lưng/số thẻ trước ngực.
- Prefabs/PF_BackLockTerminal.prefab: bảng khóa; Visual_Replaceable là phần hình có thể thay.
- Hành lang dùng lại PF_GB_Block và PF_GB_SlidingDoor trong Shared. Vách thấp chặn đi xuyên nhưng chừa đường nhìn, giúp kiểm chứng gameplay trước khi thay bằng kính/cửa quan sát.
- Canvas chính có DontLookBackPanel và CheckpointBlackout, sử dụng prefab nút/panel và theme chung. Sửa bố cục/ảnh bằng Prefab Mode; không dựng UI lại mỗi frame.
- Công cụ dựng một lần: Maps/SchoolFloor/Editor/DontLookBackSceneBuilder.cs. Không chạy Add Dont Look Back lại trên scene đã cài.

## Kiểm chứng và giới hạn
Đã kiểm tra:
- 100 seed × 3 cỡ đội; mã sai/đúng, tự đọc bị chặn, dung sai góc, góc gián đoạn, người khác nhau không cộng chung thời gian.
- Tầm nhìn thực tế của mỗi thẻ cho đội 2–4, bao gồm thẻ người cuối; đọc phía trước và đọc từ xa bị chặn.
- Nhập mã bằng Canvas, yêu cầu tập hợp, reset riêng nhiệm vụ và chơi lại map.
- Đi hai làn bằng CharacterController, vách ngăn chặn đi xuyên.
- Chuyển tiếp bình thường từ Bài kiểm tra; luật chỉ bật khi đội tập hợp.
- Quay đầu trong Play: cả đội về checkpoint 2, giữ ghi chép.
- Nút chọn nhiệm vụ và nút khóa qua raycast/EventSystem; đã mở khóa thành công.
- Bộ kiểm chứng Bài kiểm tra vẫn qua sau khi thêm nhiệm vụ 5.

Kết quả: Design/Verification/DontLookBack*.json. Ảnh: Captures/dont-look-back-*.png.

Đây là bản graybox kiểm tra logic, hiện dùng đổi vai trên một máy. Chưa có đồng bộ mạng, âm thanh gọi sau lưng, hành lang kéo dài hoặc mỹ thuật hoàn chỉnh. Cửa cuối dẫn tới phần đệm cho nhiệm vụ tiếp theo, chưa triển khai nhiệm vụ giáo viên.
