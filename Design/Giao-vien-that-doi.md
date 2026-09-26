# Nhiệm vụ 6 — Giáo viên nói thật / nói dối

## Ý tưởng và lời giải

Bốn tượng giáo viên là bốn kênh thông báo đang phát lời chỉ dẫn mâu thuẫn. Người chơi không phải đoán kênh nào “được tác giả chọn là đúng”. Ba chứng cứ trong môi trường tạo đường suy luận có thể kiểm tra:

1. Bản ghi ở phòng trước nói rằng khi các kênh mâu thuẫn phải chuyển sang xác nhận thủ công.
2. Phòng giáo viên có sơ đồ kênh và đèn **MẤT ĐỒNG BỘ**.
3. Nội quy cạnh cửa yêu cầu xác nhận đủ bốn kênh trong ba giây và mọi thành viên phải tham gia.

Đội có thể nghe từng giáo viên bằng **E**. Phụ đề được ghi riêng cho vai đã nghe, nên người chơi cần chia sẻ nội dung. Nếu đã nhận ra chế độ thủ công từ chứng cứ, đội được phép giải ngay mà không phải nghe hết thoại.

## Cách xác nhận cho đội 2–4 người

Bốn nút được ghi **XÁC NHẬN THỦ CÔNG 1–4**. Nút đầu tiên bắt đầu cửa sổ ba giây; đủ bốn kênh trong thời gian đó sẽ mở cổng nếu mọi người trong đội đã tham gia ít nhất một lần.

| Số người | Cách chia tối thiểu |
| --- | --- |
| 2 | Mỗi người xác nhận 2 kênh |
| 3 | Một người xác nhận 2 kênh, hai người còn lại mỗi người 1 kênh |
| 4 | Mỗi người xác nhận 1 kênh |

Một kênh đã chốt không thể bấm lại trong cùng lượt. Nếu đủ bốn kênh nhưng thiếu người tham gia, hoặc hết ba giây, bốn nút đặt lại. Phụ đề đã nghe vẫn được giữ. Không có hình phạt cho các nhiệm vụ trước.

## Chơi và test nhanh

Mở scene SchoolFloorPrototype và nhấn Play.

- Chơi theo tiến trình: hoàn thành “Không được quay đầu”; hệ thống chuyển sang phòng giáo viên.
- Test trực tiếp: nhấn **F8**, chọn đội 2–4 người, chọn **6 • Giáo viên thật / dối**.
- Khi thử trên một máy, dùng phím **1–4** để đổi vai. Bố trí bốn nút thành cụm 2×2 để đội 2 hoặc 3 người có thể di chuyển tới nút thứ hai trong cửa sổ.
- **R** đặt lại riêng nhiệm vụ cùng bộ thoại; **F5** đổi sang bộ thoại khác.

## Tài sản và nơi chỉnh sửa

- Nội dung: Assets/TheReturn/Features/TeacherTruth/Data/TeacherTruth_VI.asset.
  Có thể sửa tiêu đề, hướng dẫn, ba chứng cứ, phản hồi và 12 lời thoại — ba biến thể × bốn kênh.
- Luật độc lập Unity: Runtime/Domain/TeacherTruthState.cs.
- Điều khiển scene: Runtime/Presentation/TeacherTruthPrototype.cs.
- Tượng: Prefabs/PF_TeacherChannel.prefab. Thay model tại child **Visual_Replaceable**, giữ component và collider.
- Nút: Prefabs/PF_ManualConfirmStation.prefab. Thay model tại child **Visual_Replaceable**, giữ component và collider.
- Phòng: GameObject **TeacherTruth_LogicOnly** trong scene, gồm checkpoint, chứng cứ, tượng, nút và cổng.
- Canvas: dùng HUD, prompt và panel manh mối chung; menu F8 đã có nút nhiệm vụ thứ sáu.
- Công cụ dựng một lần: Maps/SchoolFloor/Editor/TeacherTruthSceneBuilder.cs. Không chạy lại trên scene đã cài.

Initial Seed bằng 0 sẽ chọn biến thể ngẫu nhiên mỗi phiên. Đặt seed cố định trên TeacherTruthPrototype để tái hiện một trường hợp khi sửa lỗi.

## Kiểm chứng

Đã kiểm tra:

- Luật cho đội 2, 3 và 4 người trên cả ba biến thể.
- Phụ đề riêng, ID sai, kênh trùng, thiếu thành viên, hết cửa sổ và nghiệm đúng.
- Tầm tương tác thật với tượng và nút; raycast ngang tầm mắt nhận đúng nút.
- Đồng hồ hết hạn trong Play tự đặt lại bốn nút và giữ phụ đề.
- Test nhanh chuẩn bị đúng kết quả của năm nhiệm vụ trước.
- Chuyển tiếp bình thường từ “Không được quay đầu”.
- Nút nhiệm vụ thứ sáu hoạt động qua Canvas/EventSystem.
- Kiểm thử hồi quy cho “Bài kiểm tra” và “Không được quay đầu” vẫn qua.

Kết quả nằm tại Design/Verification/TeacherTruth*.json. Ảnh kiểm tra nằm tại Captures/teacher-truth-*.png.

Đây là bản graybox logic và điều khiển đổi vai trên một máy. Chưa có mạng, giọng đọc, lip sync, animation giáo viên hoặc mỹ thuật hoàn chỉnh.
