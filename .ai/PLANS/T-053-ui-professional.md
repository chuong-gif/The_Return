# KẾ HOẠCH T-053: UI chuyên nghiệp cho MainMenu, Sanh_Cho, MapSchoolF + menu Cài đặt dùng chung   (file: .ai/PLANS/T-053-ui-professional.md)

Trạng thái: REVIEW | AI giữ: Claude (chat, Unity MCP; nhận tiếp việc dở của phiên chat trước đã hết lượt) | Cập nhật: 2026-11-14 05:16 UTC

## 1. Mục tiêu và nghiệm thu (chủ dự án yêu cầu trong chat)
- UI 3 scene (MainMenu, Sanh_Cho, MapSchoolF) nhìn có phong cách, chuyên nghiệp như game bên ngoài.
- Menu Cài đặt mở được ở TẤT CẢ 3 scene: Âm thanh, Đồ họa, Phím tắt, nút thoát game...
- MapSchoolF: bỏ UI nhiệm vụ cũ không thuộc 4 nhiệm vụ của map (Điểm danh, Hợp xướng, Cầu dao, Ván và chìa khóa).
- Nghiệm thu: compile sạch, console 0 lỗi, Play từng scene có ảnh chụp, scene lưu; M4 một máy; nhìn bằng mắt do chủ dự án.

## 2. Phạm vi
- Được đụng: Shared/Runtime/UI/ModernUI.cs, Features/GameSettings/Runtime/{ExtraSettings,SettingsMenu}.cs + asmdef, Maps/MainMenu, Maps/Lobby (UI), Maps/SchoolFloor/Runtime/UI + UI các nhiệm vụ cũ, scene MainMenu / Sanh_Cho / MapSchoolF (snapshot trước khi sửa).
- KHÔNG đụng: luật gameplay, mạng, âm thanh (trừ nút), builder cũ ngoài việc ghi chú, xóa code 8 nhiệm vụ (Q-013 chờ chủ dự án).

## 3. Việc đã có từ phiên chat trước (CHƯA ghi log, phát hiện 2026-11-14 05:16 UTC)
- `Shared/Runtime/UI/ModernUI.cs` (bảng màu tối, bo góc, Button/Slider/Switch/Selector), `Features/GameSettings/Runtime/ExtraSettings.cs` (đồ họa, kênh âm lượng, đảo trục; PlayerPrefs; tự áp mọi scene), `SettingsMenu.cs` (766 dòng: nút bánh răng, F10, 4 tab Âm thanh/Đồ họa/Điều khiển/Hệ thống), asmdef GameSettings và SchoolFloor sửa tham chiếu. Compile sạch lúc phát hiện; chưa Play thử, chưa scene nào dùng ModernUI.

## 4. Subtask
| ID | Việc | Cách kiểm | Trạng thái |
|---|---|---|---|
| S1 | Kiểm menu Cài đặt đã viết dở: Play từng scene, mở bằng bánh răng/F10, các tab chạy, đóng trả chuột | Play + ảnh chụp | XONG (tab Âm thanh có ảnh; 3 tab còn lại chưa ảnh) |
| S2 | Khảo sát UI hiện có của 3 scene, chốt thiết kế theo ModernUI | ảnh chụp trước | XONG |
| S3 | Thiết kế lại MainMenu | Play + ảnh | XONG (chat #2; chat #3 kiểm ảnh) |
| S4 | Thiết kế lại Sanh_Cho | Play + ảnh | XONG (chat #2; chat #3 kiểm ảnh) |
| S5 | Thiết kế lại HUD MapSchoolF, gỡ UI nhiệm vụ cũ ngoài 4 nhiệm vụ | Play + ảnh | XONG phần tô (SchoolFloorSkin); UI cũ ẩn chứ không xóa (xem LOG) |
| S6 | Kiểm toàn bộ: compile, test EditMode, Gauntlet, console, lưu scene | số liệu | MỘT PHẦN: compile sạch, EditMode 983/0 fail, console 0, scene đã lưu; Gauntlet chưa chạy |

## 5. Tiến độ
- 2026-11-14: nhận việc; phát hiện file dở ở §3; bắt đầu S1.
- 2026-11-14 05:30 UTC: phiên giữ trước hết lượt (chủ dự án xác nhận). Phiên mới tiếp quản; Editor đang Play+Pause ở Sanh_Cho do phiên trước để lại, sẽ tắt Play rồi làm S1. Phiên trước đã sửa thêm MainMenu: MainMenuSkin.cs, MainMenu.asmdef, MainMenu.unity (05:19–05:21), snapshot `Maps/MainMenu/Scenes/BeforeUiRedesign_20261114_0520.unity`.
- 2026-11-14 05:33 UTC: chat #3 tiếp quản (chat #2 hết lượt). Chat #2 đã làm thêm LobbySkin/LobbyCanvasView/Lobby.asmdef + lưu Sanh_Cho 05:29 (chưa ghi). Cập nhật bảng subtask khi kiểm xong từng phần.
- 2026-11-14 12:41 (giờ máy): chat #3 làm S5 (SchoolFloorSkin), kiểm S1–S4, chạy EditMode 988 (0 đỏ). T-053 → REVIEW. Chưa làm: Gauntlet, minimap IMGUI «BẢN ĐỒ», nút Thử lại full-width, OnlineRoom IMGUI, ảnh 3 tab Cài đặt.
- 2026-11-14 11:47 UTC: chat #4 tiếp quản (chat #3 hết lượt, chủ dự án giao việc còn lại). Làm: chạy lại EditMode + Gauntlet sau bake, xem lỗi tiêu đề MainMenu to hơn ~10%, tô minimap «BẢN ĐỒ» (IMGUI) và OnlineRoom, sửa nút «Thử lại» full-width, ảnh tab Hệ thống. Prefab hóa SettingsMenu KHÔNG làm trong lượt này (việc riêng, lớn).
