# Bài kiểm tra dẫn đường — nhiệm vụ 4

Đã nối phòng kiểm tra sau phòng giáo viên trong cùng scene:
`Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`.

Sau khi nộp đúng bảng điểm, rời bảng bằng E rồi đi qua cửa đã mở. Điều khiển chuyển sang nhiệm vụ dẫn đường; không tải scene mới và không bật chuông đổi tiết trong bản thử này.

## Cách chơi

1. Đưa các vai tới gần sa bàn. Nhìn vào bàn và nhấn E.
2. Đọc manh mối riêng bên phải, trao đổi với các vai khác để xác định ô khóa, chiều đi và thứ tự dấu.
3. Vai đang tới lượt chọn nút Lên/Phải/Xuống/Trái. Hướng theo tọa độ trên UI, không theo góc camera.
4. Mỗi bước hợp lệ chuyển lượt 1 → 2 → … → N → 1. Sai hướng, ô khóa hoặc bước ra mép không đổi lượt và không reset quân.
5. Lấy SỔ LỚP trước CHÌA KHÓA rồi tới đích. Đủ điều kiện thì cửa ra mở.

Mặc định: A1 → B1 → B2 (Sổ lớp) → C2 (Chìa khóa) → C3. Ô A2, B3, C1 khóa; cạnh B2 → C2 không đi ngược. Có thể đi lùi trên các cạnh khác; mọi trạng thái tới được đều còn đường hoàn thành.

Bốn bước ngắn nhất cho đội 4 giúp mỗi vai thao tác một lần. Đội 2 luân phiên hai lượt mỗi người; đội 3 quay lại vai 1 ở bước cuối. Manh mối phân 2–2, 2–1–1 hoặc 1–1–1–1. Gói thứ tư có thứ tự dấu và chiều cạnh.

Bản thử dùng đổi vai trên một máy, chưa có co-op qua mạng. Bài này ưu tiên xác minh cơ chế; độ khó và thời lượng cần playtest nhóm thật, chưa cam kết đạt 3–4 phút.

## Điều khiển

- WASD/chuột: đi và nhìn khi chưa mở sa bàn.
- E: mở/đóng sa bàn.
- 1–N: đổi vai. Nếu vai mới đứng đủ gần, giữ giao diện mở; nếu ở xa, đóng để đưa vai đó tới bàn.
- Các nút hướng: chỉ sáng khi đúng lượt, chưa hoàn thành.
- R hoặc nút Thử lại: đưa đội về checkpoint phòng kiểm tra, đặt lại quân/lượt/dấu; giữ ba nhiệm vụ trước.
- F5 hoặc Biến thể tiếp theo: đổi đối xứng lưới; tọa độ trong manh mối đổi cùng.
- Tab/Esc: đóng sa bàn hoặc mở menu khi đang đi.
- Chơi lại cả map: đặt lại toàn bộ bốn nhiệm vụ.

## Chỉnh nội dung

Chọn `Assets/TheReturn/Features/NavigationExam/Data/NavigationExam_VI.asset`.

- Title: tên nhiệm vụ.
- Introduction: dùng token `{start}`, `{goal}`.
- Clues: giữ đúng 4 phần tử. Token `{blocked1}`, `{blocked2}`, `{blocked3}`, `{first}`, `{second}` tự lấy tọa độ từ đề.
- Success và Controls: thông báo thành công, điều khiển.

Có bốn biến thể đối xứng, không phải sinh mê cung bất kỳ. Initial Seed trên NavigationExam_LogicOnly chọn biến thể theo số dư chia 4; 0 là đề mẫu A1 → C3. F5 tuần tự chuyển biến thể.

Prefab `Features/NavigationExam/Prefabs/PF_NavigationBoard.prefab` chứa bàn, chín ô, quân và chữ. Giữ tên Cell_0…Cell_8, Pawn, Instruction khi dùng công cụ dựng lại. Có thể thay model và gán lại tham chiếu trên controller; không cần sửa luật.

Sàn, tường, cửa và các khối bàn dùng prefab graybox chung. Không thêm model trang trí. Runtime chia Domain/Data/Presentation; Domain không tham chiếu Unity. Map flow và HUD nằm trong Maps/SchoolFloor. Code mới có mục đích/danh sách hàm đầu file và chú thích tiếng Việt trước từng hàm.

## Kiểm chứng

561 kiểm tra đạt cho 12 cấu hình (đội 2/3/4 × 4 biến thể):
- Đúng người, đúng lượt, từ chối bước chéo/ô khóa/đi ngược.
- Bước sai giữ trạng thái; thu dấu và hoàn thành đúng.
- Duyệt mọi trạng thái có thể tới, xác nhận từng trạng thái còn đường tới đích.
- Nối từ các nhiệm vụ trước, giữ kết quả bảng điểm, cửa đi qua được bằng CharacterController.
- Tương tác từ xa và lệnh cũ bị từ chối.
- Thử lại/đổi biến thể giữ ba nhiệm vụ trước.
- Input E và đổi vai gần bàn đã được thử trong Play Mode.

Báo cáo: `Design/Verification/NavigationExam.json`, `NavigationExamInput.json`.
Để chạy lại trong Play: `TheReturn.Editor.NavigationExamChecks.Run()`. Kiểm tra làm thay đổi lượt đang chơi.
Ảnh giao diện: `Captures/navigation-exam-ui.png`.

Lối sau cửa là điểm kết thúc phần thử hiện tại. Chưa triển khai nhiệm vụ 5–7, multiplayer hoặc lưu tiến trình lâu dài.
