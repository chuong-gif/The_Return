# Đồ thị asmdef (TỰ SINH 2026-11-04 — đừng sửa tay)

Sinh bằng code đọc mọi *.asmdef dưới Assets/TheReturn. «T.» = tiền tố TheReturn. Cột Cờ: Editor = chỉ trong Editor; noEngine = không được dùng UnityEngine. Test = tên kết thúc .Tests/.PlayTests. Nguồn sự thật là file .asmdef; bảng này chỉ là bản đồ. Ngoài các assembly này còn gói Unity (Unity.Netcode.Runtime, Unity.Services.*, Unity.InputSystem, ...).

Số asmdef: 73

| Assembly | Đường dẫn | Cờ | Tham chiếu |
|---|---|---|---|
| TheReturn.Attendance | Features/Attendance/Runtime/TheReturn.Attendance.asmdef |  | T.Attendance.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.Attendance.Domain | Features/Attendance/Runtime/Domain/TheReturn.Attendance.Domain.asmdef | noEngine |  |
| TheReturn.Attendance.Editor | Features/Attendance/Editor/TheReturn.Attendance.Editor.asmdef | Editor | T.Attendance, T.Attendance.Domain, T.Core, T.Shared |
| TheReturn.Attendance.Network | Features/Attendance/Runtime/Network/TheReturn.Attendance.Network.asmdef | noEngine | T.Attendance.Domain, T.Network.Framework |
| TheReturn.Attendance.Network.Tests | Features/Attendance/Tests/EditModeNet/TheReturn.Attendance.Network.Tests.asmdef | Editor | T.Attendance.Network, T.Attendance.Domain, T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.Attendance.Online | Features/Attendance/Runtime/Online/TheReturn.Attendance.Online.asmdef |  | T.Attendance, T.Attendance.Domain, T.Attendance.Network, T.Network.Framework, T.Network.Online, T.Core, Unity.Netcode.Runtime, Unity.InputSystem |
| TheReturn.Attendance.Tests | Features/Attendance/Tests/Editor/TheReturn.Attendance.Tests.asmdef | Editor | T.Attendance, T.Attendance.Domain, T.Core, T.Shared |
| TheReturn.BreakerPanel | Features/BreakerPanel/Runtime/TheReturn.BreakerPanel.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.BreakerPanel.Domain |
| TheReturn.BreakerPanel.Domain | Features/BreakerPanel/Runtime/Domain/TheReturn.BreakerPanel.Domain.asmdef | noEngine |  |
| TheReturn.BreakerPanel.Domain.Tests | Features/BreakerPanel/Tests/EditMode/TheReturn.BreakerPanel.Domain.Tests.asmdef | Editor | T.BreakerPanel.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.BreakerPanel.Network | Features/BreakerPanel/Runtime/Network/TheReturn.BreakerPanel.Network.asmdef | noEngine | T.BreakerPanel.Domain, T.Network.Framework |
| TheReturn.BreakerPanel.Network.Tests | Features/BreakerPanel/Tests/EditModeNet/TheReturn.BreakerPanel.Network.Tests.asmdef | Editor | T.BreakerPanel.Network, T.BreakerPanel.Domain, T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.BreakerPanel.Online | Features/BreakerPanel/Runtime/Online/TheReturn.BreakerPanel.Online.asmdef |  | T.BreakerPanel, T.BreakerPanel.Domain, T.BreakerPanel.Network, T.Network.Framework, T.Network.Online, T.Core, Unity.Netcode.Runtime, Unity.InputSystem |
| TheReturn.Choir | Features/Choir/Runtime/TheReturn.Choir.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.Choir.Domain |
| TheReturn.Choir.Domain | Features/Choir/Runtime/Domain/TheReturn.Choir.Domain.asmdef | noEngine |  |
| TheReturn.Choir.Domain.Tests | Features/Choir/Tests/EditMode/TheReturn.Choir.Domain.Tests.asmdef | Editor | T.Choir.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.Core | Core/Runtime/TheReturn.Core.asmdef |  | Unity.InputSystem |
| TheReturn.Core.Tests | Core/Tests/EditMode/TheReturn.Core.Tests.asmdef | Editor | T.Core, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.DontLookBack | Features/DontLookBack/Runtime/TheReturn.DontLookBack.asmdef |  | T.DontLookBack.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.DontLookBack.Domain | Features/DontLookBack/Runtime/Domain/TheReturn.DontLookBack.Domain.asmdef |  |  |
| TheReturn.GameAudio | Features/GameAudio/Runtime/TheReturn.GameAudio.asmdef |  | T.GameAudio.Domain |
| TheReturn.GameAudio.Domain | Features/GameAudio/Runtime/Domain/TheReturn.GameAudio.Domain.asmdef | noEngine |  |
| TheReturn.GameAudio.Domain.Tests | Features/GameAudio/Tests/EditMode/TheReturn.GameAudio.Domain.Tests.asmdef | Editor | T.GameAudio.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.GameSettings | Features/GameSettings/Runtime/TheReturn.GameSettings.asmdef |  | T.GameSettings.Domain, T.Core |
| TheReturn.GameSettings.Domain | Features/GameSettings/Runtime/Domain/TheReturn.GameSettings.Domain.asmdef | noEngine |  |
| TheReturn.GameSettings.Domain.Tests | Features/GameSettings/Tests/EditMode/TheReturn.GameSettings.Domain.Tests.asmdef | Editor | T.GameSettings.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.GlassBridge | Features/GlassBridge/Runtime/TheReturn.GlassBridge.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.GlassBridge.Domain |
| TheReturn.GlassBridge.Domain | Features/GlassBridge/Runtime/Domain/TheReturn.GlassBridge.Domain.asmdef | noEngine |  |
| TheReturn.GlassBridge.Domain.Tests | Features/GlassBridge/Tests/EditMode/TheReturn.GlassBridge.Domain.Tests.asmdef | Editor | T.GlassBridge.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.GradeRepair | Features/GradeRepair/Runtime/TheReturn.GradeRepair.asmdef |  | T.GradeRepair.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.GradeRepair.Domain | Features/GradeRepair/Runtime/Domain/TheReturn.GradeRepair.Domain.asmdef |  |  |
| TheReturn.Lobby | Maps/Lobby/Runtime/TheReturn.Lobby.asmdef |  | T.Core, T.Party, T.Party.Domain, Unity.ugui, Unity.InputSystem |
| TheReturn.Lobby.Editor | Maps/Lobby/Editor/TheReturn.Lobby.Editor.asmdef | Editor | T.Core, T.Lobby, Unity.ugui, Unity.InputSystem |
| TheReturn.MainMenu | Maps/MainMenu/Runtime/TheReturn.MainMenu.asmdef |  | T.Core, T.Party, T.Party.Domain, T.GameSettings, T.GameSettings.Domain, Unity.ugui, Unity.InputSystem |
| TheReturn.MainMenu.Editor | Maps/MainMenu/Editor/TheReturn.MainMenu.Editor.asmdef | Editor | T.MainMenu, T.Party.Domain, T.GameSettings.Domain, Unity.ugui, Unity.InputSystem |
| TheReturn.NavigationExam | Features/NavigationExam/Runtime/TheReturn.NavigationExam.asmdef |  | T.NavigationExam.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.NavigationExam.Domain | Features/NavigationExam/Runtime/Domain/TheReturn.NavigationExam.Domain.asmdef |  |  |
| TheReturn.Network | Features/Network/Runtime/TheReturn.Network.asmdef |  | Unity.Netcode.Runtime, Unity.Networking.Transport, Unity.Collections, Unity.Services.Core, Unity.Services.Authentication, Unity.Services.Multiplayer, Unity.InputSystem |
| TheReturn.Network.Framework | Features/Network/Runtime/Framework/TheReturn.Network.Framework.asmdef | noEngine |  |
| TheReturn.Network.Framework.Tests | Features/Network/Tests/EditMode/TheReturn.Network.Framework.Tests.asmdef | Editor | T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.Network.Online | Features/Network/Online/TheReturn.Network.Online.asmdef |  | T.Core, T.Network, T.Network.Framework, Unity.Netcode.Runtime, Unity.Collections, Unity.InputSystem |
| TheReturn.Network.Online.Tests | Features/Network/Tests/EditModeOnline/TheReturn.Network.Online.Tests.asmdef | Editor | T.Core, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.Network.Room | Features/Network/Runtime/Room/TheReturn.Network.Room.asmdef |  | T.Network, T.Network.Framework, Unity.Netcode.Runtime, Unity.Collections, Unity.Services.Multiplayer |
| TheReturn.Network.Room.Tests | Features/Network/Tests/EditModeRoom/TheReturn.Network.Room.Tests.asmdef | Editor | T.Network.Room, T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.Network.Voice | Features/Network/Runtime/Voice/TheReturn.Network.Voice.asmdef |  | T.Network, T.Network.Room, Unity.Services.Core, Unity.Services.Vivox |
| TheReturn.Party | Features/Party/Runtime/TheReturn.Party.asmdef |  | T.Party.Domain |
| TheReturn.Party.Domain | Features/Party/Runtime/Domain/TheReturn.Party.Domain.asmdef | noEngine |  |
| TheReturn.Party.Domain.Tests | Features/Party/Tests/EditMode/TheReturn.Party.Domain.Tests.asmdef | Editor | T.Party.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.PlankAndKey | Features/PlankAndKey/Runtime/TheReturn.PlankAndKey.asmdef |  | T.PlankAndKey.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.PlankAndKey.Domain | Features/PlankAndKey/Runtime/Domain/TheReturn.PlankAndKey.Domain.asmdef | noEngine |  |
| TheReturn.PlankAndKey.Domain.Tests | Features/PlankAndKey/Tests/EditMode/TheReturn.PlankAndKey.Domain.Tests.asmdef | Editor | T.PlankAndKey.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.PlankAndKey.Network | Features/PlankAndKey/Runtime/Network/TheReturn.PlankAndKey.Network.asmdef | noEngine | T.PlankAndKey.Domain, T.Network.Framework |
| TheReturn.PlankAndKey.Network.Tests | Features/PlankAndKey/Tests/EditModeNet/TheReturn.PlankAndKey.Network.Tests.asmdef | Editor | T.PlankAndKey.Network, T.PlankAndKey.Domain, T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.PlankAndKey.Online | Features/PlankAndKey/Runtime/Online/TheReturn.PlankAndKey.Online.asmdef |  | T.PlankAndKey, T.PlankAndKey.Domain, T.PlankAndKey.Network, T.Network.Framework, T.Network.Online, T.SchoolFloor.Domain, T.SchoolFloor.Online, T.Core, Unity.Netcode.Runtime, Unity.InputSystem |
| TheReturn.QuietCorridor | Features/QuietCorridor/Runtime/TheReturn.QuietCorridor.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.QuietCorridor.Domain |
| TheReturn.QuietCorridor.Domain | Features/QuietCorridor/Runtime/Domain/TheReturn.QuietCorridor.Domain.asmdef |  |  |
| TheReturn.SchoolFloor | Maps/SchoolFloor/Runtime/TheReturn.SchoolFloor.asmdef |  | T.SchoolFloor.Domain, T.Attendance, T.Attendance.Domain, T.Core, T.Party, T.Party.Domain, T.Shared, T.QuietCorridor, T.QuietCorridor.Domain, T.GradeRepair, T.GradeRepair.Domain, T.NavigationExam, T.NavigationExam.Domain, Unity.ugui, Unity.InputSystem, T.DontLookBack, T.DontLookBack.Domain, T.TeacherTruth, T.TeacherTruth.Domain, T.Choir, T.BreakerPanel, T.TeacherSays, T.TeacherSays.Domain, T.BreakerPanel.Domain, T.Choir.Domain, T.GlassBridge, T.GlassBridge.Domain, T.SharedLantern, T.SharedLantern.Domain, T.PlankAndKey, T.PlankAndKey.Domain, T.GameAudio.Domain |
| TheReturn.SchoolFloor.Domain | Maps/SchoolFloor/Runtime/Domain/TheReturn.SchoolFloor.Domain.asmdef | noEngine |  |
| TheReturn.SchoolFloor.Domain.Tests | Maps/SchoolFloor/Tests/EditMode/TheReturn.SchoolFloor.Domain.Tests.asmdef | Editor | T.SchoolFloor.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.SchoolFloor.Editor | Maps/SchoolFloor/Editor/TheReturn.SchoolFloor.Editor.asmdef | Editor | T.SchoolFloor, T.Attendance, T.Attendance.Domain, T.QuietCorridor, T.Core, T.Shared, T.GradeRepair, T.GradeRepair.Domain, T.NavigationExam, T.NavigationExam.Domain, Unity.ugui, Unity.InputSystem, T.DontLookBack, T.DontLookBack.Domain, T.TeacherTruth, T.TeacherTruth.Domain |
| TheReturn.SchoolFloor.Online | Maps/SchoolFloor/Online/TheReturn.SchoolFloor.Online.asmdef |  | T.SchoolFloor, T.SchoolFloor.Domain, T.Network.Online, T.Network.Framework, T.Core, Unity.Netcode.Runtime |
| TheReturn.SchoolFloor.Online.Tests | Features/Network/Tests/EditModeFlow/TheReturn.SchoolFloor.Online.Tests.asmdef | Editor | T.SchoolFloor, T.SchoolFloor.Domain, T.SchoolFloor.Online, T.Core, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.SchoolFloor.PlayTests | Maps/SchoolFloor/Tests/PlayMode/TheReturn.SchoolFloor.PlayTests.asmdef |  | T.SchoolFloor, T.SchoolFloor.Domain, T.Core, T.Attendance, T.Attendance.Domain, T.QuietCorridor, T.QuietCorridor.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner, T.Shared |
| TheReturn.SchoolFloor.Tests | Maps/SchoolFloor/Tests/Editor/TheReturn.SchoolFloor.Tests.asmdef | Editor | T.SchoolFloor, T.Attendance, T.Attendance.Domain, T.QuietCorridor, T.QuietCorridor.Domain, T.Core, T.Shared, T.GradeRepair, T.GradeRepair.Domain, T.NavigationExam, T.NavigationExam.Domain, T.DontLookBack, T.DontLookBack.Domain, Unity.ugui, T.TeacherTruth, T.TeacherTruth.Domain |
| TheReturn.Shared | Shared/Runtime/TheReturn.Shared.asmdef |  | Unity.ugui |
| TheReturn.SharedLantern | Features/SharedLantern/Runtime/TheReturn.SharedLantern.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.SharedLantern.Domain |
| TheReturn.SharedLantern.Domain | Features/SharedLantern/Runtime/Domain/TheReturn.SharedLantern.Domain.asmdef | noEngine |  |
| TheReturn.SharedLantern.Domain.Tests | Features/SharedLantern/Tests/EditMode/TheReturn.SharedLantern.Domain.Tests.asmdef | Editor | T.SharedLantern.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.TeacherSays | Features/TeacherSays/Runtime/TheReturn.TeacherSays.asmdef |  | T.Core, T.Shared, Unity.InputSystem, T.TeacherSays.Domain |
| TheReturn.TeacherSays.Domain | Features/TeacherSays/Runtime/Domain/TheReturn.TeacherSays.Domain.asmdef | noEngine |  |
| TheReturn.TeacherSays.Domain.Tests | Features/TeacherSays/Tests/EditMode/TheReturn.TeacherSays.Domain.Tests.asmdef | Editor | T.TeacherSays.Domain, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.TeacherSays.Network | Features/TeacherSays/Runtime/Network/TheReturn.TeacherSays.Network.asmdef | noEngine | T.TeacherSays.Domain, T.Network.Framework |
| TheReturn.TeacherSays.Network.Tests | Features/TeacherSays/Tests/EditModeNet/TheReturn.TeacherSays.Network.Tests.asmdef | Editor | T.TeacherSays.Network, T.TeacherSays.Domain, T.Network.Framework, UnityEngine.TestRunner, UnityEditor.TestRunner |
| TheReturn.TeacherTruth | Features/TeacherTruth/Runtime/TheReturn.TeacherTruth.asmdef |  | T.TeacherTruth.Domain, T.Core, T.Shared, Unity.InputSystem |
| TheReturn.TeacherTruth.Domain | Features/TeacherTruth/Runtime/Domain/TheReturn.TeacherTruth.Domain.asmdef | noEngine |  |
