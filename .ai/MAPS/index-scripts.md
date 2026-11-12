# Chỉ mục nguồn — Core4/map/online

Chỉ đọc đúng khu vực đang làm. Không chứa symbol snapshot để tránh bảng lớn và stale; tìm symbol bằng IDE/`rg` trong các thư mục dưới đây. Source và Unity là nguồn sự thật.

| Phạm vi | Thư mục |
|---|---|
| Bốn nhiệm vụ | `Assets/TheReturn/Features/{Attendance,BreakerPanel,Choir,PlankAndKey}` |
| Online chung | `Assets/TheReturn/Features/Network` |
| Party/settings dùng chung | `Assets/TheReturn/Features/{Party,GameSettings}` |
| Luồng map | `Assets/TheReturn/Maps/SchoolFloor/Runtime` |
| Lobby/menu | `Assets/TheReturn/Maps/{Lobby,MainMenu}/Runtime` |
| Avatar/controller/interactions | `Assets/TheReturn/Core`, `Assets/TheReturn/Shared` |
| Scene đích | `Assets/TheReturn/Scenes/Maps/MapSchoolF.unity` |

`tools/rebuild_index.snippet.cs` có thể sinh symbol index giới hạn theo các thư mục Core4 ở trên khi cần; không chạy index toàn dự án.
