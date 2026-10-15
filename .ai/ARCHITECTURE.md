# ARCHITECTURE (WARM)

Nguồn: asmdef + cấu trúc thư mục, đọc 2026-10-14. Quy ước thư mục của chủ dự án: `Design/Cau-truc-du-an.md`.

## Tầng assembly [V]
- Domain (`TheReturn.<Feature>.Domain`): refs rỗng. Feature: Attendance, GradeRepair, NavigationExam, QuietCorridor, DontLookBack, TeacherTruth.
- Feature runtime (`TheReturn.<Feature>`) -> Domain của chính nó + `TheReturn.Core` + `TheReturn.Shared` + Unity.InputSystem.
- `TheReturn.Core` -> Unity.InputSystem. `TheReturn.Shared` -> Unity.ugui.
- `TheReturn.SchoolFloor` (Maps/SchoolFloor/Runtime) -> TẤT CẢ feature + Domain + Core + Shared (điểm tập trung phụ thuộc).
- `TheReturn.Lobby` -> Core, ugui, InputSystem.
- Editor: Attendance.Editor, Lobby.Editor, SchoolFloor.Editor. Tests (Editor): Attendance.Tests, SchoolFloor.Tests.
- Chưa có asmdef: `Assets/Scripts/Player.cs` (vào Assembly-CSharp), `Art/Editor/*` (Assembly-CSharp-Editor).

## Cấu trúc Assets/TheReturn [V]
Art, Assets3D, Core, Features (6 feature), Maps (Lobby, SchoolFloor), Scenes, Shared. Ngoài Assets: Design, Backups, Captures, ArtPreviews. `Assets/ThirdParty` bị gitignore (nội dung Asset Store có bản quyền, xem `Design/ThirdParty-manifest.md`).

## Số liệu [V]
80 file .cs (khoảng 9.3k dòng không tính ThirdParty), 20 scene, 877 prefab, 52 .asset, 21 asmdef. File .cs lớn nhất 330 dòng (AttendancePrototype.cs) — không có file quá lớn.

## Điểm rủi ro
- [V] SchoolFloor phụ thuộc mọi feature: sửa một feature có thể ảnh hưởng SchoolFloor, SchoolFloor.Editor và SchoolFloor.Tests.
- [V] `FindFirstObjectByType` xuất hiện ở 16 file: phụ thuộc scene ngầm, dễ vỡ khi đổi cấu trúc scene.
- [V] Controller cần tham chiếu cửa nhỏ cũ đang ẩn (PROJECT_STATE): không xóa object ẩn khi chưa kiểm tra reference.
- [V] Builder cũ trong Art/Editor và SchoolFloor/Editor có thể ghi đè bố cục (xem AGENTS.md).
- [V] Một số bài kiểm tra lịch sử hardcode tọa độ cũ (PROJECT_STATE) — kết quả có thể sai nếu chạy lại.
- [S] Hai scene dự phòng, `_Recovery/0.unity`, `Assets/Scripts/Player.cs`: vai trò chưa rõ, không xóa.
