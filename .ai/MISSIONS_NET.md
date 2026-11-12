# MISSIONS_NET — registry online Core4

Phạm vi registry là bốn nhiệm vụ được giữ. Gauntlet mới phải kiểm đúng các assembly này và hạ tầng chung; không dùng các nhiệm vụ khác làm tiêu chí phát triển.

| Nhiệm vụ | Khu | Assembly test | Tình trạng tích hợp | Lần Gauntlet có số liệu |
|---|---|---|---|---|
| PlankAndKey | Hanh_Lang_6 | `TheReturn.PlankAndKey.Network.Tests` | Online; đã nối MapSchoolF trong T-042. | 55/55 đạt |
| Attendance | Phong_1 | `TheReturn.Attendance.Network.Tests` | Online; đã nối MapSchoolF trong T-042. | 53/54 đạt, 1 bỏ qua |
| BreakerPanel | Phong_5 | `TheReturn.BreakerPanel.Network.Tests` | Online; đã nối MapSchoolF trong T-042. | 70/71 đạt, 1 bỏ qua |
| Choir | Phong_6 | `TheReturn.Choir.Network.Tests` | Online; đã nối MapSchoolF trong T-042. | 65/66 đạt, 1 bỏ qua |

Số liệu là từ Gauntlet 2026-11-12, không xác nhận thao tác mạng nhiều máy. Tình trạng online phòng→map và smoke xem `QUEUE.md`; mọi số liệu phải được cập nhật bằng lần chạy mới, không suy ra từ đây.

Assembly dùng chung trong phép lọc cũ: `TheReturn.BreakerPanel.Domain.Tests`, `TheReturn.Choir.Domain.Tests`, `TheReturn.Core.Tests`, `TheReturn.Core4.Domain.Tests`, `TheReturn.GameAudio.Domain.Tests`, `TheReturn.GameSettings.Domain.Tests`, `TheReturn.Network.Contract.Tests`, `TheReturn.Network.Framework.Tests`, `TheReturn.Network.Online.Tests`, `TheReturn.Network.Room.Tests`, `TheReturn.Party.Domain.Tests`, `TheReturn.PlankAndKey.Domain.Tests`, `TheReturn.SchoolFloor.Domain.Tests`, `TheReturn.SchoolFloor.Online.Tests`. Danh sách này phản ánh phép lọc GAUNTLET 2026-11-12, không khẳng định mọi assembly còn đúng tên ở source hiện hành.
