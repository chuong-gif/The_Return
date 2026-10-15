# Đồ thị assembly (đọc 2026-10-14)

Domain (refs rỗng): Attendance, GradeRepair, NavigationExam, QuietCorridor, DontLookBack, TeacherTruth
Feature runtime -> {own Domain, Core, Shared, Unity.InputSystem}
Core -> {Unity.InputSystem}; Shared -> {Unity.ugui}
SchoolFloor -> {6 Feature, 6 Domain, Core, Shared, ugui, InputSystem}
SchoolFloor.Editor -> {SchoolFloor, 6 Feature, 6 Domain, Core, Shared, ugui, InputSystem}
SchoolFloor.Tests -> {SchoolFloor, 6 Feature, 6 Domain, Core, Shared, ugui}
Attendance.Editor, Attendance.Tests -> {Attendance, Attendance.Domain, Core, Shared}
Lobby -> {Core, ugui, InputSystem}; Lobby.Editor -> {Core, Lobby, ugui, InputSystem}
Nguồn sự thật: file .asmdef. Tạo lại bằng script khi index được dựng.

T-002 (2026-10-15 07:52 UTC): SchoolFloor.Domain (noEngineReferences, refs rỗng) tại Maps/SchoolFloor/Runtime/Domain — CHƯA ai tham chiếu; SchoolFloor.Domain.Tests (Editor, NUnit) -> {SchoolFloor.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner} tại Maps/SchoolFloor/Tests/EditMode.
T-003 (2026-10-15 08:05 UTC): TheReturn.SchoolFloor nay tham chiếu thêm TheReturn.SchoolFloor.Domain (adapter ở Maps/SchoolFloor/Runtime/Missions dùng IMissionAdapter).
