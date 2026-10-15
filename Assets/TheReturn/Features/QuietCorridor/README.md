# QuietCorridor — Giữ trật tự

**Vị trí:** Hanh_Lang_2; hai trạm giữ, cổng âm thanh, vùng chống kẹp và vùng đích.

| File code | Vai trò |
|---|---|
| Runtime/Domain/QuietCorridorState.cs | Quyền giữ trạm, mức ồn, khoảng đệm, hoàn thành và reset. |
| Runtime/Data/QuietCorridorSettings.cs | Thông số âm thanh/thời gian và nội dung hướng dẫn. |
| Runtime/Presentation/QuietCorridorPrototype.cs | Input, kiểm tra tương tác, mô phỏng ồn, cổng, vùng đích và retry. |
| Runtime/Presentation/QuietHoldStation.cs | ID nút giữ và phản hồi hình ảnh. |
| Runtime/Presentation/QuietSpeakerFeedback.cs | Loa phản hồi 3D; có thể gán AudioClip riêng, không đọc microphone. |

**Dữ liệu:** `Data/QuietCorridor_VI.asset`. Thay thời gian hoặc khoảng cách hai nút cần test lại đội 2 người.

**Prefab:** `Prefabs/PF_QuietHoldStation.prefab`. Giữ ID trạm, collider và các reference hiển thị của component. Cổng dùng `Shared/Prefabs/Graybox/PF_GB_SlidingDoor.prefab`; chỉ thay VisualRoot, giữ panel và collider. Hai cửa chuẩn bị ngoài hành lang là hệ kiểm soát tiến trình riêng, không thay cổng âm thanh.

`Prefabs/PF_QuietSpeaker.prefab` là loa có `QuietSpeakerFeedback` và AudioSource, đã tách khỏi prefab khối xám chung. Khi kéo instance mới vào scene, gán `session` tới QuietCorridorPrototype. Prefab nguồn không giữ reference tới scene; instance hiện tại đã được gán.

Giữ `safetyZone`, `goalZone`, checkpoints và `speakerIndicator` trên controller. Vùng đích dùng hộp local nên có thể xoay hành lang. Test nhanh nhiệm vụ 1. Kiểm tra cũ tại `Maps/SchoolFloor/Tests/Editor/QuietCorridorChecks.cs` có giả định tọa độ cũ; không chạy rồi coi mọi lỗi là lỗi luật mới.
