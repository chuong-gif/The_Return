# Sửa bảng điểm — nhiệm vụ 3

Đã nối phòng giáo viên sau hành lang Giữ trật tự trong cùng scene:
`Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorPrototype.unity`.

Lớp học vẫn có 24 bàn ghế. Khi cả đội hoàn thành nhiệm vụ 2, cửa phòng giáo viên mở và điều khiển chuyển sang nhiệm vụ 3; không tải scene khác.

## Cách chơi

1. Chọn đội 2–4 người ở đầu map, giải điểm danh và Giữ trật tự.
2. Đi vào phòng giáo viên. Bốn bàn ghi số hồ sơ và vai phụ trách.
3. Nhìn vào tài liệu, nhấn **E** bằng đúng vai. Đọc sổ đối chiếu mã, hai phiếu bổ sung và biên bản nhập nhầm.
4. Trao đổi để nối mã học sinh trên chứng từ với hồ sơ A–D trên bảng. Bảng sửa không hiển thị sẵn mã trong sổ.
5. Tới bảng lớn cuối phòng, nhấn E. Chọn phiếu, hồ sơ nhận; với biên bản, chọn cả hồ sơ nguồn.
6. Áp dụng, hoàn tác nếu cần, rồi nộp bảng. Đúng chứng từ và đủ điều kiện thì cửa ra mở.

Mỗi vai có ít nhất một tài liệu: đội 2 chia 2–2; đội 3 chia 2–1–1; đội 4 mỗi vai một tài liệu. Bốn hồ sơ học sinh vẫn giữ nguyên số lượng, không phụ thuộc số người trong đội. Ai cũng có thể vận hành bảng.

Đây là diễn tập một máy: **1–N** đổi vai. Tài liệu được chia theo vai để thử cách phối hợp; chưa có multiplayer hoặc bảo mật dữ liệu qua mạng.

## Điều khiển

| Phím/nút | Tác dụng |
| --- | --- |
| WASD / chuột | Đi và nhìn khi không mở bảng |
| E | Đọc tài liệu, mở hoặc đóng bảng |
| 1–N | Đổi vai; tự đóng bảng/tài liệu đang xem |
| Áp dụng | Sử dụng chứng từ đang chọn |
| Hoàn tác | Trả lại điểm và quyền dùng chứng từ của thao tác cuối |
| Nộp bảng | Kiểm tra chứng cứ và điều kiện hoàn thành |
| R | Đặt lại riêng bảng, giữ tài liệu đã đọc |
| F5 | Hồ sơ mới, xóa dấu đã đọc của nhiệm vụ 3 |
| Tab / Esc | Đóng bảng; khi đang đi thì mở menu |

R/F5 dùng khi đang đi. Menu cũng có các nút thử lại bảng, hồ sơ mới và chơi lại cả map.

## Luật và lời giải mẫu

Bốn điểm khởi đầu là 4, 7, 8, 9, được xáo vị trí A–D theo seed. Sổ đối chiếu và chứng từ dùng mã học sinh tương ứng.

Có hai phiếu bổ sung **+2**, mỗi phiếu dùng một lần, và một biên bản **chuyển 1 điểm** từ hồ sơ đang có 9 sang hồ sơ đang có 7. Phiếu 01 thuộc hồ sơ ban đầu có 4; phiếu 02 thuộc hồ sơ ban đầu có 9. Đây là cấu trúc bản thử hiện tại; seed đổi vị trí/mã, không sinh ra một luật hoàn toàn mới.

Ví dụ khi A/B/C/D lần lượt 4/7/8/9:
- Chuyển D → B: 4/8/8/8.
- Áp phiếu 01 cho A: 6/8/8/8.
- Áp phiếu 02 cho D: 6/8/8/10.

Có ba thứ tự thao tác hợp lệ; không bắt một chuỗi duy nhất. Bổ sung vào hồ sơ 9 trước khi chuyển điểm sẽ vượt 10 và bị từ chối, phiếu vẫn còn.

Điểm luôn nằm trong 0–10 khi thao tác. Điều kiện 6–10 và tổng 32 được kiểm tra lúc nộp. Cần đọc đủ bốn tài liệu và dùng đúng từng chứng từ; đạt tổng 32 nhưng tráo hai phiếu vẫn sai. Nộp sai không reset bảng hoặc tiêu hủy giấy.

Thử lại/hồ sơ mới đưa đội về checkpoint trong phòng, giữ điểm danh và Giữ trật tự đã hoàn thành. Chỉ nút chơi lại cả map mới xóa tiến trình trước.

## Chỉnh nội dung và tài sản

Asset lời hướng dẫn:
`Assets/TheReturn/Features/GradeRepair/Data/GradeRepair_VI.asset`.

- Title, Introduction, Document Titles: tiêu đề và mục tiêu.
- Supplement Variants: mẫu phiếu +2, giữ token `{code}`.
- Transfer Variants: mẫu chuyển điểm, giữ `{source}` và `{target}`.
- Các trường thông báo: vượt giới hạn, thiếu chứng từ, sai hồ sơ và thành công.
- Không đổi con số +2/chuyển 1 trong text mà không cập nhật luật tương ứng.

Prefab tương tác:
`Assets/TheReturn/Features/GradeRepair/Prefabs/PF_GradeTerminal.prefab`.
Thay child Visual_Replaceable để đổi model, giữ component và nhãn. Bàn/sàn/tường/cửa dùng module graybox chung; không thêm model trang trí.

Trong Hierarchy, `GradeRepair_LogicOnly` giữ board, documents, cửa và checkpoint. Initial Seed = 0 tạo seed khi bắt đầu; nhập số khác 0 để diễn tập lại đề cụ thể.

Mã chia thành Runtime/Domain, Runtime/Data và Runtime/Presentation. Domain không tham chiếu Unity. Maps/SchoolFloor/Runtime nối các nhiệm vụ, không tạo phụ thuộc giữa từng feature. Các file mới có mô tả đầu file và chú thích trước từng hàm bằng tiếng Việt.

Menu Add Grade Repair Room chỉ dùng một lần trên map chưa có phòng; không cần dựng lại để sửa text.

## Kiểm chứng và giới hạn

- 4.257 kiểm tra đạt trên 90 tổ hợp đội/seed (2, 3, 4 người × 30 seed), gồm thứ tự hợp lệ, phiếu trùng/sai, hoàn tác, điểm vượt giới hạn.
- Kiểm tra luồng cả ba nhiệm vụ cho đội 2–4, cửa đi qua được bằng CharacterController, tường chặn đọc xuyên, quyền vai và reset giữ tiến trình trước.
- Thử input E mở bảng, phím 2 đổi vai/đóng bảng, E đọc tài liệu được phân công.
- Console không có lỗi/cảnh báo trong lượt kiểm tra; scene không có missing script.

Báo cáo: `Design/Verification/GradeRepair.json`, `GradeRepairInput.json`.
Trong Play, chạy `TheReturn.Editor.GradeRepairChecks.Run()` để kiểm tra lại. Lệnh thay đổi trạng thái lượt đang thử.

Ảnh: `Captures/grade-repair-board.png`, `grade-repair-document.png`, `school-floor-three-tasks.png`.

Chưa có mạng, lưu tiến trình lâu dài hoặc chuông đổi tiết sau nhiệm vụ 3. Lối sau cửa hiện là vùng kết thúc phần thử; nhiệm vụ kế tiếp sẽ nối tiếp tại đây. Độ vui/khó cần playtest nhóm thật.
