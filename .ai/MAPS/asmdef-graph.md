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
