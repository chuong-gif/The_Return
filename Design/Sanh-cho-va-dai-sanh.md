# Sảnh chờ và đại sảnh tầng học

## Luồng chạy

1. Game mở `Sanh_Cho` ở build index 0.
2. Chọn quy mô đội 2–4 người trên Canvas.
3. F1–F4 hoặc các nút Canvas đổi trạng thái sẵn sàng của từng slot.
4. Khi toàn đội sẵn sàng, sảnh đếm ngược 5 giây rồi tải `SchoolFloorPrototype` bất đồng bộ.
5. F8 trong tầng học mở menu vào thẳng một trong sáu nhiệm vụ để kiểm thử.

## Bố cục tầng học

- Lớp điểm danh nằm ở tầng thấp và giữ đủ 24 bàn ghế.
- Năm nhiệm vụ tiếp theo là các đảo kiến trúc theo vòng xoắn tăng dần độ cao quanh vực trung tâm.
- Mỗi nhiệm vụ giữ nguyên root logic; toàn bộ checkpoint và vật tương tác đi theo transform của root.
- Vùng tập hợp sau điểm danh được chuyển về cửa lớp, vì nhiệm vụ 2 chỉ bắt đầu sau khi đủ đội.
- `MonumentalSchool_ArtOnly` chỉ chứa cảnh quan có thể thay thế. Xóa hoặc dựng lại root này không sửa dữ liệu câu đố.

## Công cụ Editor

- `The Return/Lobby/Rebuild Monumental Lobby`: dựng lại sảnh và Build Settings.
- `The Return/School Floor/Build Monumental Atrium`: dựng lại bố cục vòng xoắn và cảnh quan đại sảnh.

## Asset bên thứ ba

Khi nhập package từ Asset Store, giữ nguyên package trong `Assets/ThirdParty/<Publisher>/<Package>`.
Prefab sử dụng trong game phải được bọc hoặc tạo Prefab Variant dưới `Assets/TheReturn`, không sửa trực tiếp file của nhà phát hành.

Danh sách ưu tiên: Backrooms Like Asset Pack, Yughues Free Concrete Materials, Fog Particles và FREE Skybox Extended Shader. Mỗi package phải được thử riêng trên Unity 6 URP trước khi đưa vào scene chính.

## Ngôn ngữ hình ảnh hiện tại

- Sảnh chờ dùng một trục nghi lễ chạy từ vị trí xuất hiện tới portal. Vòm lặp, rune sàn và ánh sáng cyan giúp người chơi luôn nhận biết hướng tiến tới.
- Portal có bốn bậc, hai tháp và quầng mảnh vỡ để trở thành điểm nhấn chính ngay cả khi chưa có model hoàn thiện.
- Tầng học dùng sáu đảo nhiệm vụ xoắn quanh vực trung tâm. Mỗi đảo có cổng, rune, đèn dẫn đường và lan can nhận diện bằng màu nhiệm vụ.
- Ánh sáng môi trường xanh lạnh được giữ ở cường độ thấp; ánh sáng nhiệm vụ và portal tạo phân cấp thị giác.
- Cấu trúc mới nằm trong các root art và dùng module tái sử dụng, vì vậy có thể thay bằng model chất lượng cao mà không chạm vào gameplay.
